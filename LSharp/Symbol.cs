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
using System.ComponentModel;
using System.Collections;

namespace LSharp
{
	/// <summary>
	/// Symbols are used for their object identity to name various entities 
	/// including (but not limited to) linguistic entities such as variables 
	/// and functions. 
	/// In L Sharp all symbols are interned. The symbol table is also
	/// encapsulated within this Symbol class.
	/// </summary>
  [TypeConverter("Xacc.Controls.Design.LSharpConverter, xacc")]
  [Editor("Xacc.Controls.Design.LSharpUIEditor, xacc", typeof(System.Drawing.Design.UITypeEditor))]
	public sealed class Symbol 
	{
		const int CAPACITY = 500;
		readonly static Hashtable symbolTable = new Hashtable(CAPACITY);

		readonly string name;

		/// <summary>
		/// Returns the Symbol's name
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
		}

		Symbol(string name)
		{
			this.name = string.Intern(name.ToLower());
		}

    public override bool Equals(object obj)
    {
      Symbol s = obj as Symbol;
      if (s == null)
      {
        return false;
      }
      return name == s.name;
    }

    public override int GetHashCode()
    {
      return name.GetHashCode ();
    }

#if DEBUG
    public string DebugInfo
    {
      get {return name;}
    }
#endif


		/// <summary>
		/// Returns a string representation of the Symbol
		/// </summary>
		/// <returns></returns>
		public override string ToString() 
		{
			return name;
		}

		/// <summary>
		/// Returns a symbol given its name. If necessary the symbol is
		/// created and interned.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Symbol FromName(string name) 
		{
      name = string.Intern(name.ToLower());
			Symbol symbol = (Symbol)symbolTable[name];
			if(symbol == null) 
			{
				symbol = new Symbol(name);
				symbolTable.Add(name, symbol);
			}

			return symbol;
		}

    public static explicit operator Symbol(string name)
    {
      return Symbol.FromName(name);
    }

    public static bool operator ==(Symbol a, Symbol b)
    {
      if (object.ReferenceEquals(a,b))
      {
        return true;
      }
      return Object.Equals(a,b);
    }

    public static bool operator !=(Symbol a, Symbol b)
    {
      return !Object.Equals(a,b);
    }


		// Define some commonly used symbols
		public static readonly Symbol TRUE        = (Symbol)"true";
		public static readonly Symbol FALSE       = (Symbol)"false";
		public static readonly Symbol NULL        = (Symbol)"null";
		public static readonly Symbol IT          = (Symbol)"it"; 
		public static readonly Symbol SPLICE      = (Symbol)"splice";
    public static readonly Symbol QUOTE       = (Symbol)"quote";
		public static readonly Symbol UNQUOTE     = (Symbol)"unquote";
		public static readonly Symbol BACKQUOTE   = (Symbol)"backquote";
	
	}

  [AttributeUsage(AttributeTargets.Method, Inherited=false, AllowMultiple=true)]
  public sealed class SymbolAttribute : Attribute
  {
    string name;

    public SymbolAttribute()
    {
    }

    public SymbolAttribute(string name)
    {
      this.name = name;
    }

    public string Name
    {
      get {return name;}
      set {name = value;}
    }

    public static implicit operator Symbol(SymbolAttribute sa)
    {
      if (sa.name == null)
      {
        return null;
      }
      return Symbol.FromName(sa.name);
    }
  }
}
