#region Copyright (C) 2006 Llewellyn Pritchard
//
// L Sharp .NET Command line Compiler
// Copyright (C) 2006 Llewellyn Pritchard
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Library General Public
// License as published by the Free Software Foundation; either
// version 2 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Library General Public License for more details.
// 
// You should have received a copy of the GNU Library General Public
// License along with this library; if not, write to the Free
// Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//
#endregion

using System;
using LSharp;
using Xacc.Utils;
using System.IO;

namespace LSharp.Compiler
{
  enum Target
  {
    Exe,
    Dll
  }

  [ArgOptions(AllowShortcut = true)]
  class Args : GetArgs
  {
    [ArgItem(Shortname = "E")]
    public bool processonly = false;

    [ArgItem(Shortname = "g")]
    public bool debug = false;

    [ArgItem(Shortname = "O")]
    public bool optimize = true;

    [ArgItem(Shortname = "t")]
    public Target target = Target.Exe;

    [DefaultArg]
    public string[] input = {};
  }

	class CLI
	{
		[STAThread]
		static int Main()
		{
      Args args = new Args();

      if (args.input.Length == 0)
      {
        Console.Error.WriteLine("error: no input file");
        return 2;
      }

      Environment env = new Environment();

      try
      {
        foreach (string infile in args.input)
        {
          if (Compiler.CompileExe(infile, args, env) == null)
          {
            return 1;
          }
        }
      }
#if !DEBUG
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        args.PrintHelp();
        return 1;
      }
#endif
      finally 
      {
        
      }

      return 0;
		}
	}
}
