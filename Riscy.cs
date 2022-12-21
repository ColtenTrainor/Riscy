// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

using System.Globalization;

namespace Riscy;

public static class Riscy
{
	private enum RunMode { sim, rom };
	private static RunMode _runMode;
	private static Simulator _sim = null!;
	private static RomProgrammer _romProg = null!;

	public static void Main(string[] args)
	{
		// foreach(var arg in args){Console.WriteLine(arg);}
		HandleArgs(args);
		
		switch (_runMode)
		{
			case RunMode.sim:
				_sim.Run();
				break;
			case RunMode.rom:
				_romProg.ProgramRoms();
				_romProg.SaveRomTables();
				break;
		}
	}

	private static void HandleArgs(string[] args)
	{
		byte[] program = null!;
		bool debugEnabled = false;
		string[] debugOutFormat = {"n","x","b","x"};
		string romFileName = "rom";
		bool romFileExtSpecified = false;
		string romFormat = "r";
		string romFileExt = "";

		if (args.Length < 1)
		{
			Console.WriteLine("Arguments invalid. --h for help.");
			Environment.Exit(-1);
		}

		if (args[0] == "--h")
		{
			PrintHelp();
			Environment.Exit(0);
		}

		switch (args[0])
		{
			case "--h":
				PrintHelp();
				Environment.Exit(0);
				break;
			case "sim":
				_runMode = RunMode.sim;
				if (!File.Exists(args[1]))
				{
					Console.WriteLine("Input file not found.");
					Environment.Exit(-2);
				}

				try
				{
					var programFile = File.ReadAllLines(args[1]);
					program = new byte[programFile.Length];
					for (int j = 0; j < programFile.Length; j++)
					{
						program[j] = Byte.Parse(programFile[j], NumberStyles.HexNumber);
					}
				}
				catch(Exception e)
				{
					Console.WriteLine("Error reading input file.\n");
					Console.WriteLine(e);
					Environment.Exit(-3);
				}
				break;
			case "rom":
				_runMode = RunMode.rom;
				break;
		}

		for(int i = 1; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--h":
					PrintHelp();
					break;
				case "--df":
					debugOutFormat = args[i+1].Split(',');
					for (int j = 0; j < debugOutFormat.Length; j++)
						debugOutFormat[j] = debugOutFormat[j].ToLower();
					i++;
					break;
				case "--d":
					debugEnabled = true;
					break;
				case "--o":
					var arg = args[i + 1];
					if (arg.Contains('.'))
					{
						romFileExtSpecified = true;
						var splitArg = arg.Split('.');
						romFileName = splitArg[0];
						Console.WriteLine("rom filename: " + romFileName);
						romFileExt = "." + splitArg[1];
						Console.WriteLine("rom file ext: " + romFileExt);
					}
					else
					{
						romFileName = arg;
						romFileExt = romFormat.Equals("r") ? ".bin" : ".txt";
					}
					i++;
					break;
				case "--rf":
					romFormat = args[i + 1];
					i++;
					break;
					
			}
		}

		_sim = new Simulator(program, debugEnabled, debugOutFormat);
		if (!romFileExtSpecified) romFileExt = romFormat.Equals("r") ? ".bin" : ".txt";
		_romProg = new RomProgrammer(romFileName, romFormat, romFileExt);
	}

	private static void PrintHelp()
	{
		Console.WriteLine(@"
--h                 Help.

sim [program]       Run a simulation of a riscy machine. [program] should be a file with hexadecimal machine code, each instruction on a new line.
--d                 Enables debug output.
--df [format]       Set format of numeric debug outputs. Enter as comma-separated list.
                        n for register name (only allowed for 1st parameter)
                        b for binary
                        x for hexadecimal
                        d for decimal
    
rom                 Output txt document for programming a control unit ROM.
--o [filename]      Name of output file.
--rf [format]       Set format of ROM file output.
                        r for raw binary data
                        b for binary
                        x for hexadecimal (default)
                        d for decimal
");
	}
}