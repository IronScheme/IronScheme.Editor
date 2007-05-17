// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)
//#define DEBUG

using System;
using System.IO;
using System.Collections.Generic;



namespace gpcc
{
  class GPCG
  {
    public static bool LINES =
#if DEBUG
 true
#else
      false
#endif
;
    public static bool REPORT = 
#if DEBUG
      true
#else
      true
#endif
      ;

    static TextWriter output;

    private static int Main(string[] args)
    {
      try
      {
        string outfile = null;
        string filename = ProcessOptions(args, ref outfile);



        if (filename == null)
          return 1;

        if (outfile != null)
        {
          if (File.Exists(outfile))
          {
            if (File.GetLastWriteTime(typeof(GPCG).Assembly.Location) > File.GetLastWriteTime(outfile))
            {
            }
            else if (File.Exists(outfile) && new FileInfo(outfile).Length > 0)
            {
              if (File.GetLastWriteTime(filename) < File.GetLastWriteTime(outfile))
              {
                return 0;
              }
            }
          }
          Console.SetOut(output = File.CreateText(outfile));
        }

        Console.Error.WriteLine("gppg {0}", System.IO.Path.GetFileName(filename));

        Console.WriteLine("#pragma warning disable 3001,3002,3003,3004,3005,3006,3007,3008,3009");

        Parser parser = new Parser();
        Grammar grammar = parser.Parse(filename);

        LALRGenerator generator = new LALRGenerator(grammar);
        List<State> states = generator.BuildStates();
        generator.ComputeLookAhead();
        generator.BuildParseTable();

        if (REPORT)
          generator.Report(filename);

        CodeGenerator code = new CodeGenerator();
        code.Generate(states, grammar);
        return 0;
      }
      catch (Scanner.ParseException e)
      {
        Console.Error.WriteLine("Parse error (line {0}, column {1}): {2}", e.line, e.column, e.Message);
      }
      finally
      {
        Console.Out.Flush();
        if (output != null)
        {
          output.Close();
        }
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


    private static string ProcessOptions(string[] args, ref string outfile)
    {
      string filename = null;

      bool expect = false;

      foreach (string arg in args)
      {
        if (expect)
        {
          outfile = arg;
          expect = false;
        }
        else
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
              case "o":
                expect = true;
                break;
            }
          else
            filename = arg;
        }
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
