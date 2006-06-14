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
using System.Text;

namespace LSharp
{
	/// <summary>
	/// Formats and prints LSharp objects
	/// </summary>
	public sealed class Printer
	{
		Printer(){}

    public static string ConsToString(Object x)
    {
      string v = WriteToString(x);
      return v.Substring(1, v.Length - 2);
    }

		public static string WriteToString(Object x) 
		{

			if (x == null) 
			{
				return "null";
			}

      if (x == Reader.EOFVALUE)
      {
        return "EOF";
      }

			Type type = x.GetType();

			if (x is string) 
			{
				return string.Format("\"{0}\"",(string) x);
			}

      if (x is bool)
      {
        return x.ToString().ToLower();
      }

			if (x is char) 
			{
				return string.Format("#\\{0}", x);
			}

			if (x is Symbol) 
			{
				return string.Format("{0}",x);
			}

			if (x is Cons) 
			{
        bool wasquote = true;
				Cons cons = (Cons) x;
        StringBuilder stringBuilder = new StringBuilder();
        Symbol car = cons.Car() as Symbol;

        if (car == Symbol.QUOTE)
        {
          stringBuilder.Append("'");
        }
        else if (car == Symbol.BACKQUOTE)
        {
          stringBuilder.Append("`");
        }
        else if (car == Symbol.SPLICE)
        {
          stringBuilder.Append(",@");
        }
        else if (car == Symbol.UNQUOTE)
        {
          stringBuilder.Append(",");
        }
        else
        {
          wasquote = false;
          stringBuilder.Append("(");
          stringBuilder.Append(WriteToString(cons.Car()));
          stringBuilder.Append(" ");
        }

        Object o;
				o = cons.Cdr();
				while (o != null) 
				{
					if (o is Cons) 
					{
						cons = (Cons)o;
						stringBuilder.Append(WriteToString(cons.Car()));
						
						o = cons.Cdr();

						if (o !=null)
							stringBuilder.Append(" ");
					} 
					else 
					{
						stringBuilder.Append(". ");
						stringBuilder.Append(WriteToString(o));
						o = null;
					}
				}
        string op = stringBuilder.ToString().Trim();

        if (wasquote)
        {
          return op;
        }
        else
        {
          return op + ")";
        }
				
			}

			return x.ToString().Trim();
		}

		public static void Write(Object x) 
		{
			Console.WriteLine(WriteToString(x));
		}
			
		
	}
}
