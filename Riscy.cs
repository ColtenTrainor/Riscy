// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

using System.Globalization;

namespace Riscy;

public static class Riscy
{
	private enum RunMode { sim, rom };
	private static RunMode _runMode;
	private static Simulator _sim = null!;

	public static void Main(string[] args)
	{
		//foreach(var arg in args){Console.WriteLine(arg);}
		HandleArgs(args);
		
		switch (_runMode)
		{
			case RunMode.sim:
				_sim.Run();
				break;
			case RunMode.rom:
				var romProg = new RomProgrammer();
				romProg.ProgramRoms();
				romProg.SaveRomTables();
				break;
		}
	}

	private static void HandleArgs(string[] args)
	{
		byte[] program = null!;
		bool debugEnabled = false;
		string[] outFormat = {"n","x","b","x"};
		
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

		for(int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--h":
					PrintHelp();
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
				case "--of":
					outFormat = args[i+1].Split(',');
					for (int j = 0; j < outFormat.Length; j++)
						outFormat[j] = outFormat[j].ToLower();
					break;
				case "--d":
					debugEnabled = true;
					break;
			}
		}

		_sim = new Simulator(program, debugEnabled, outFormat);
	}

	private static void PrintHelp()
	{
		Console.WriteLine(@"
--h             Help.

sim [program]       Run a simulation of a riscy machine. [program] should be a file with hexadecimal machine code, each instruction on a new line.
--of [format]       Set format of numeric debug outputs. Enter as comma-separated list.
                        n for register name (only allowed for 1st parameter)
                        b for binary
                        x for hexadecimal
                        d for decimal
--d                 Enables debug output.
    
rom                 Output txt document for programming a control unit ROM.
--o [filename]      Name of output file. [not yet implemented]
");
	}
}