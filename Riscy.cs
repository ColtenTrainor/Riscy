// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Local

using System.Globalization;

namespace Riscy;

public static class Riscy
{
	private static byte[] registers = new byte[4];
	private static byte PC,PCOutput,SP,instruction;
	private static byte[] memory = new byte[256];
	 
	private static byte[] program = null!;
	
	private static bool isHalted;
	private static String instName="",debugParamInfo="";
	
	private static bool debugEnabled;
	private static string[] outFormat = {"n","x","b","x"};

	public static void Main(string[] args)
	{
		//foreach(var arg in args){Console.WriteLine(arg);}
		HandleArgs(args);
	}
	
	public static void RunSim()
	{
		var regForm = outFormat[0];
	
		for(int i = 0; i < program.Length; i++) 
			memory[i] = program[i];
		
		while (!isHalted)
		{
			instruction=memory[PC];
			var opcode = (byte) (instruction >> 4);
			var rd = (byte) (instruction >> 2 & 3);
			var rs = (byte) (instruction & 3);
			var imm = (byte) ((opcode & 3) << 2 | rs);
			
			PCOutput = PC;
			
			if (opcode >> 2 == 3)
			{
				instName = "SLI";
				debugParamInfo = $"{rd.ToOutString2Bit(regForm)},{imm.ToOutString4Bit(outFormat[3])}";
				registers[rd] = (byte) (registers[rd] << 4 | imm);
			} 
			else switch (opcode)
			{
				case 0b0111:
					instName = "ADD";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},{rs.ToOutString2Bit(regForm)}";
					registers[rd] += registers[rs];
					break;
				case 0b0001:
					instName = "SUB";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},{rs.ToOutString2Bit(regForm)}";
					registers[rd] -= registers[rs];
					break;
				case 0b0010:
					instName = "LOAD";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},[{rs.ToOutString2Bit(regForm)}]";
					registers[rd] = memory[registers[rs]];
					break;
				case 0b0011:
					instName = "STORE";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},[{rs.ToOutString2Bit(regForm)}]";
					memory[registers[rs]] = registers[rd];
					break;
				case 0b0100:
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)}";
					switch (rs)
					{
						case 0b00:
							instName = "SKIPZ";
							if (registers[rd] == 0) PC++;
							break;
						case 0b01:
							instName = "SKIPNZ";
							if (registers[rd] != 0) PC++;
							break;
						case 0b10:
							instName = "SKIPL";
							if (registers[rd] >> 7 == 1) PC++;
							break;
						case 0b11:
							instName = "SKIPGE";
							if (registers[rd] >> 7 == 0) PC++;
							break;
					}
					break;
				case 0b0101:
					instName = "JALR";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},{rs.ToOutString2Bit(regForm)}";
					var oldPC = PC;
					PC = (byte) (registers[rs] - 1);
					registers[rd] = (byte) (oldPC + 1);
					break;
				case 0b0000:
					debugParamInfo = "";
					switch (instruction)
					{
						case 0:
							instName = "NOP";
							break;
						case 1:
							instName = "HALT";
							isHalted = true;
							break;
						default:
							instName = "INV";
							break;
					}
					break;
				case 0b1010:
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)}";
					switch (rs)
					{
						case 0b00:
							instName = "PUSH";
							SP--;
							memory[SP] = registers[rd];
							break;
						case 0b01:
							instName = "POP";
							registers[rd] = memory[SP];
							SP++;
							break;
						default:
							instName = "ERROR";
							break;
					}
					break;
				case 0b1000:
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)}";
					switch (rs)
					{
						case 0b00:
							instName = "INC";
							registers[rd]++;
							break;
						case 0b01:
							instName = "DEC";
							registers[rd]--;
							break;
						case 0b10:
							instName = "OUT";
							Console.Write(Convert.ToChar(registers[rd]));
							break;
						case 0b11:
							instName = "IN";
								while (true)
								{
									try
									{
										registers[rd] = Byte.Parse(Console.ReadLine()!);
									}
									#pragma warning disable CS0168
									catch (ArgumentNullException e)
									#pragma warning restore CS0168
									{
										Console.WriteLine("Failed to parse input, re-enter.");
										continue;
									}
									break;
								}
							break;
					}
					break;
				case 0b0110:
					instName = "NAND";
					debugParamInfo = $"{rd.ToOutString2Bit(regForm)},{rs.ToOutString2Bit(regForm)}";
					registers[rd] = (byte) ~(registers[rd] & registers[rs]);
					break;
				default:
					instName = "INV";
					debugParamInfo = "";
					break;
			}
			
			if (debugEnabled) DebugOutput();
			
			PC++;
		}
	}
	
	private static void DebugOutput()
	{
		string output = $"{instName} {debugParamInfo} {Spacing(instName+debugParamInfo, 11)}";
		output += $"PC=[{PCOutput.ToOutString(outFormat[1])}], inst=[{instruction.ToOutString(outFormat[2])}], ";
		output += $"op=[{instName}],{Spacing(instName, 6)} ";
		output += $"A=[{registers[0].ToOutString(outFormat[3])}], B=[{registers[1].ToOutString(outFormat[3])}], ";
		output += $"C=[{registers[2].ToOutString(outFormat[3])}], D=[{registers[3].ToOutString(outFormat[3])}]";
		
		Console.WriteLine(output);
		
		static string Spacing(string s, int i)
		{
			string spacing = "";
			for (int j = 0; j < i-s.Length; j++) spacing+=" ";
			return spacing;
		}
	}
	
	private static void HandleArgs(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine("Arguments invalid. --h for help.");
			Environment.Exit(-1);
		}
		
		if (!File.Exists(args[0]))
		{
			Console.WriteLine("Input file not found.");
			Environment.Exit(-2);
		}

		try
		{
			var programFile = File.ReadAllLines(args[0]);
			program = new byte[programFile.Length];
			for (int i = 0; i < programFile.Length; i++)
			{
				program[i] = Byte.Parse(programFile[i], NumberStyles.HexNumber);
			}
		}
		catch(Exception e)
		{
			Console.WriteLine("Error reading input file.\n");
			Console.WriteLine(e);
			Environment.Exit(-3);
		}

		for(int i = 1; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--h":
					Console.WriteLine(@"
sim [program]   Run a simulation of a riscy machine. [program] should be a file with hexadecimal machine code, each instruction on a new line.
rom             Output txt document for programming a control unit ROM.

--h             Help.
--of [format]   Set format of numeric outputs. Enter as comma-separated list.
                    n for register name (only allowed for 1st parameter)
                    b for binary
                    x for hexadecimal
                    d for decimal
--d             Enables debug output.");
					Environment.Exit(0);
					break;
				case "--of":
					var formats = args[i+1].Split(',');
					for (int j = 0; j < formats.Length; j++)
						outFormat[j] = formats[j].ToLower();
					break;
				case "--d":
					debugEnabled = true;
					break;
			}
		}
	}
}