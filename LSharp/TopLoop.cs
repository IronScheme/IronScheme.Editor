#region Copyright (C) 2005 Rob Blackwell & Active Web Solutions.
//
// L Sharp .NET, a powerful lisp-based scripting language for .NET.
// Copyright (C) 2005 Rob Blackwell & Active Web Solutions.
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
using System.IO;

namespace LSharp
{
	/// <summary>
	/// Provides an toploop allowing interactive access to the
	/// L Sharp interpreter
	/// </summary>
	/// <remarks>
	/// leppie: This has changed for xacc.ide
	/// </remarks>
  public class TopLoop
  {
    // The global environment
    static Environment environment = new Environment();

    static string prompt = "> ";
    static bool trace = false, tracereturn = true;

    public static Environment Environment
    {
      get {return environment;}
    }
		
		public TopLoop() : this (environment)
		{
		  	
		}

		public TopLoop(Environment environment)
		{
			TopLoop.environment = environment;
		}

    public static bool TraceCall
    {
      get {return trace;}
      set {trace = value;}
    }

    public static bool TraceReturn
    {
      get {return tracereturn;}
      set {tracereturn = value;}
    }

    /// <summary>
    /// Allows the input prompt to be set
    /// </summary>
    public static string Prompt
    {
      get {return prompt;}
      set {prompt = value;}
    }

		/// <summary>
		/// Starts the toploop running using default input, output and error streams
		/// </summary>
		public void Run() 
		{
			Run(Console.In, Console.Out, Console.Error);
		}

		/// <summary>
		///  Starts the toploop running using specified input, output and error streams
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="writer"></param>
		/// <param name="error"></param>
		public void Run(TextReader reader, TextWriter writer, TextWriter error) 
		{
			Symbol LAST = Symbol.FromName("?");


 			while (true) 
			{
        try 
        {
					Object o = trace ? Runtime.EvalString("(eval (prl (read (in Console))))",environment) 
            : Runtime.EvalString("(eval (read (in Console)))",environment) ;

          if (o == Reader.EOFVALUE)
          {
            return;
          }

          if (tracereturn)
          {
            writer.Write(prompt);
            writer.WriteLine(Printer.WriteToString(o));
          }
          environment.AssignLocal(LAST,o);
				} 
				catch (LSharpException e) 
				{
					error.WriteLine(e.Message);
				}
				catch (Exception e) 
				{
					error.WriteLine(e.GetBaseException());
				}
			}
		}
	}
}
