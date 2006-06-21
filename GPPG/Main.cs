// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Collections.Generic;


namespace gpcc
{
	class GPCG
	{
		public static bool LINES = true;
		public static bool REPORT = false;

		private static int Main(string[] args)
		{
      try
      {
        string filename = ProcessOptions(args);

        if (filename == null)
          return 1;

        Parser parser = new Parser();
        Grammar grammar = parser.Parse(filename);

        LALRGenerator generator = new LALRGenerator(grammar);
        List<State> states = generator.BuildStates();
        generator.ComputeLookAhead();
        generator.BuildParseTable();

        if (REPORT)
          generator.Report();
        else
        {
          CodeGenerator code = new CodeGenerator();
          code.Generate(states, grammar);
        }
        return 0;
      }
      catch (Scanner.ParseException e)
      {
        Console.Error.WriteLine("Parse error (line {0}, column {1}): {2}", e.line, e.column, e.Message);
      }
      finally
      {
        Console.Out.Flush();
      }
      return 1;

      
            /*
			catch (System.Exception e)
			{
				Console.Error.WriteLine("Unexpected Error {0}", e.Message);
                Console.Error.WriteLine(e.StackTrace);
				Console.Error.WriteLine("Please report to w.kelly@qut.edu.au");
			}
             */
		}


		private static string ProcessOptions(string[] args)
		{
			string filename = null;

			foreach (string arg in args)
			{
				if (arg[0] == '-' || arg[0] == '/')
					switch (arg.Substring(1))
					{
						case "?":
						case "h":
						case "help":
							DisplayHelp();
							return null;
						case "v":
						case "version":
							DisplayVersion();
							return null;
						case "l":
						case "no-lines":
							LINES = false;
							break;
						case "r":
						case "report":
							REPORT = true;
							break;
					}
				else
					filename = arg;
			}

			if (filename == null)
				DisplayHelp();

			return filename;
		}


		private static void DisplayHelp()
		{
			Console.WriteLine("Usage gppg [options] filename");
			Console.WriteLine();
			Console.WriteLine("-help:       Display this help message");
			Console.WriteLine("-version:    Display version information");
			Console.WriteLine("-report:     Display LALR(1) parsing states");
			Console.WriteLine("-no-lines:   Suppress the generation of #line directives");
			Console.WriteLine();
		}


		private static void DisplayVersion()
		{
			Console.WriteLine("Gardens Point Parser Generator (gppg) beta 0.82 30/04/2006");
			Console.WriteLine("Written by Wayne Kelly");
			Console.WriteLine("w.kelly@qut.edu.au");
			Console.WriteLine("Queensland University of Technology");
			Console.WriteLine();
		}
	}
}
