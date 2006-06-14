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
using System.Collections;
using System.Text;
using System.Reflection;

namespace LSharp
{
	/// <summary>
	/// An environment is essentially a hashtable that maps variables to values.
	/// In addition, environments can be nested to support local variables.
	/// </summary>
	public sealed class Environment
	{
		private const int CAPACITY = 10;
		private Hashtable hashtable = new Hashtable(CAPACITY);
    readonly static ArrayList extensions = new ArrayList();
    readonly static ArrayList macroextensions = new ArrayList();
    int extmark = 0;

    class Extension
    {
      public Type type;
      public Type delegatetype;
    }

    static Environment()
    {
      Runtime.Import(typeof(Environment).Assembly);
      string fp = System.IO.Path.GetDirectoryName(typeof(Environment).Assembly.Location)
        + System.IO.Path.DirectorySeparatorChar + "lsc.exe";
      if (System.IO.File.Exists(fp))
      {
        Assembly a = AssemblyCache.LoadAssembly(fp);
        Runtime.Import(a);
      }
    }

    internal static void RegisterExtension(Type t, Type deltype)
    {
      Extension e = new Extension();
      e.delegatetype = deltype;
      e.type = t;

      extensions.Add(e);
    }

    internal static void RegisterMacroExtension(Type t)
    {
      macroextensions.Add(t);
    }

		// Maintain a reference to a previous environment to allow nesting
		// of environments, thus supporting local variables and recursion
		private Environment previousEnvironment;
    bool isresetting = false;

		public void GlobalReset() 
		{
			hashtable = new Hashtable(CAPACITY);
			InitialiseLSharpBindings();
		}

    public string[] GetSymbols(Type filter)
    {
      ArrayList s = new ArrayList();
      foreach (DictionaryEntry de in hashtable)
      {
        if (de.Value.GetType() == filter)
        {
          s.Add(((Symbol)de.Key).Name);
        }
      }
      if (previousEnvironment != null)
      {
        s.AddRange(previousEnvironment.GetSymbols(filter));
      }

      return s.ToArray(typeof(string)) as string[];
    }

    public void InitSpecialForms(Type t)
    {
      InitSymbols(t, typeof(SpecialForm));
    }

    public void InitFunctions(Type t)
    {
      InitSymbols(t, typeof(Function));
    }

    void InitSymbols(Type t, Type deltype)
    {
      const BindingFlags BF = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static;

      foreach (MethodInfo mi in t.GetMethods(BF))
      {
        foreach (SymbolAttribute sa in mi.GetCustomAttributes(typeof(SymbolAttribute), false))
        {
          if (sa.Name == null)
          {
            sa.Name = mi.Name;
          }
          Assign(sa, Delegate.CreateDelegate(deltype, mi));
        }
      }
    }

    void InitMacros(Type t)
    {
      object[] THIS = new object[]{ this };
      const BindingFlags BF = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static;

      foreach (MethodInfo mi in t.GetMethods(BF))
      {
        foreach (SymbolAttribute sa in mi.GetCustomAttributes(typeof(SymbolAttribute), false))
        {
          Assign(sa, mi.Invoke(null, THIS));
        }
      }
    }

    public void UpdateBindings()
    {
      for (int i = extmark; i < extensions.Count; i++)
      {
        Extension e = extensions[i] as Extension;
        InitSymbols(e.type, e.delegatetype);
        extmark++;
      }
    }

		public void InitialiseLSharpBindings() 
		{
      isresetting = true;

      foreach (Extension e in extensions)
      {
        InitSymbols(e.type, e.delegatetype);
        extmark++;
      }
			
      // leppie: should this not be Assign() ?
			// read table
			Assign(Symbol.FromName("*readtable*"),ReadTable.DefaultReadTable());

			// Macros
      foreach (Type t in macroextensions)
      {
        InitMacros(t);
      }

		
		 isresetting = false;
		}

		/// <summary>
		/// Creates a new, global environment
		/// </summary>
		public Environment() 
		{
			InitialiseLSharpBindings();
		}

		/// <summary>
		/// Creates a new environment which has access to a previous environment
		/// </summary>
		/// <param name="environment"></param>
		public Environment(Environment environment) 
		{
			this.previousEnvironment = environment;
		}

		public object GetValue(Symbol symbol) 
		{
			object o = hashtable[symbol];

			if ((o == null) && (previousEnvironment != null))
				o = previousEnvironment.GetValue(symbol);

			return o;
		}

		
		/// <summary>
		/// Determines whether the environment contains a definition for
		/// a variable with the given symbol
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>True or false</returns>
		public bool Contains(Symbol symbol) 
		{
			if (hashtable.Contains(symbol))
				return true;

			if (previousEnvironment != null)
				return previousEnvironment.Contains(symbol);

			return false;

		}
		
		private Environment GetEnvironment (Symbol symbol) 
		{
			if (hashtable.Contains(symbol))
				return this;

			if (previousEnvironment == null)
				return null;
			
			return previousEnvironment.GetEnvironment(symbol);

		}

		/// <summary>
		/// Sets a variable with given symbol to a given value
		/// </summary>
		/// <param name="symbol"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public object Assign(Symbol symbol, object value) 
		{
			Environment environment = GetEnvironment(symbol);

			if (environment == null)
				environment = this;

			return environment.AssignLocal(symbol, value);
		}

		/// <summary>
		/// Assigns value to a local variable symbol in this
		/// local environment (irrespective of whether symbol
		/// is defined in any parent environments).
		/// </summary>
		/// <param name="symbol"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public object AssignLocal(Symbol symbol, object value) 
		{
      object oldvalue = hashtable[symbol];
      hashtable[symbol] = value;
			
      if (oldvalue != value)
      {
        if (!isresetting)
        {
          EnvironmentEventArgs e = new EnvironmentEventArgs(symbol, oldvalue, value);
          if (oldvalue == null)
          {
            if (SymbolAssigned != null)
            {
              SymbolAssigned(this,e);
            }
          }
          else if (value == null)
          {
            if (SymbolRemoved != null)
            {
              SymbolRemoved(this, e);
            }
          }
          else
          {
            if (SymbolChanged != null)
            {
              SymbolChanged(this, e);
            }
          }
        }
      }
			return value;
		}

    public event EnvironmentEventHandler SymbolRemoved;
    public event EnvironmentEventHandler SymbolAssigned;
    public event EnvironmentEventHandler SymbolChanged;

		/// <summary>
		/// Returns the contents of the environment as a string suitable for use
		/// in a debugger or IDE.
		/// </summary>
		/// <returns></returns>
		public string Contents() 
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach(string key in hashtable.Keys) 
			{
				stringBuilder.AppendFormat("{0}:{1}\r\n",key.ToString(),hashtable[key]);
			}
			return stringBuilder.ToString();
		}

    public override string ToString()
    {
      return Contents();
    }

	}

  public sealed class EnvironmentEventArgs : EventArgs
  {
    readonly Symbol s;
    readonly object oldvalue;
    readonly object newvalue;

    internal EnvironmentEventArgs(Symbol s, object oldvalue, object newvalue)
    {
      this.s = s;
      this.oldvalue = oldvalue;
      this.newvalue = newvalue;
    }

    public Symbol Symbol
    {
      get {return s;}
    }

    public string SymbolName
    {
      get {return s.Name;}
    }

    public object OldValue
    {
      get {return oldvalue;}
    }

    public object NewValue
    {
      get {return newvalue;}
    }

  }

  public delegate void EnvironmentEventHandler(object sender, EnvironmentEventArgs e);
}
