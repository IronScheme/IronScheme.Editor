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
using System.Reflection;
using System.IO;

namespace LSharp
{
  [AttributeUsage(AttributeTargets.Assembly)]
  public sealed class LSharpExtensionAttribute : Attribute
  {
  }

	/// <summary>
	/// Allows LSharp programs to be executed
	/// </summary>
	public sealed class Runtime
	{
    Runtime(){}

		private static IProfiler profiler = new DefaultProfiler();


		/// <summary>
		/// Maps eval to a list of expressions
		/// </summary>
		/// <param name="list"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public static Cons EvalList(object list, Environment environment) 
		{
			if (list == null)
				return null;

			object result = null;

			foreach (object item in (Cons)list ) 
			{
				result = new Cons(Eval(item,environment) ,result);
			}

			return  ((Cons)result).Reverse();

		}

		/// <summary>
		/// Evaluates an expression in a given lexical environment
		/// </summary>
		/// <param name="form"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public static Object Eval (Object expression, Environment environment) 
		{
			profiler.TraceCall (expression);

      if (expression == Reader.EOFVALUE)
      {
        return profiler.TraceReturn(expression);
      }
			
			if (expression == null)
				return profiler.TraceReturn(null);

			// The expression is either an atom or a list
      if (Primitives.IsAtom(expression))
      {
        // Number
        if (expression is double)
          return profiler.TraceReturn(expression);

        if (expression is int)
          return profiler.TraceReturn(expression);

        // Character
        if (expression is char)
          return profiler.TraceReturn(expression);

        // String
        if (expression is string)
          return profiler.TraceReturn(expression);

        Symbol sym = expression as Symbol;

        if (sym == Symbol.TRUE)
          return profiler.TraceReturn(true);

        if (sym == Symbol.FALSE)
          return profiler.TraceReturn(false);

        if (sym == Symbol.NULL)
          return profiler.TraceReturn(null);

        // If the symbol is bound to a value in this lexical environment
        if (environment.Contains(sym))
          // Then it's a variable so return it's value
          return profiler.TraceReturn(environment.GetValue(sym));
        else 
        {
                    // Otherwise symbols evaluate to themselves
          return profiler.TraceReturn(expression);
        }
      }
      else 
      {
        // The expression must be a list
        Cons cons = (Cons) expression;

        // Lists are assumed to be of the form (function arguments)

        // See if there is a binding to a function, clsoure, macro or special form
        // in this lexical environment
        object function = environment.GetValue((Symbol)cons.First());

        // If there is no binding, then use the function name directly - it's probably
        // the name of a .NET method
        if (function == null)
          function = cons.First();

        // If it's a special form
        if (function is SpecialForm) 
        {
          return profiler.TraceReturn(((SpecialForm) function) ((Cons)cons.Cdr(),environment));
        }

        // If its a macro application
        if (function is Macro) 
        {
          object expansion = ((Macro) function).Expand((Cons)cons.Cdr());
          return profiler.TraceReturn(Runtime.Eval(expansion, environment));
        }

        // It must be a function, closure or method invocation,
        // so call apply
        Object arguments = EvalList((Cons)cons.Cdr(),environment);
        return profiler.TraceReturn(Runtime.Apply(function, arguments, environment));
				
      }			
		}

		/// <summary>
		/// Makes a new instance of type type by calling the
		/// appropriate constructor, passing the given arguments
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="arguments">the arguments to the constructor</param>
		/// <returns></returns>
		public static object MakeInstance(Type type, object arguments) 
		{
			Type[] types = new Type[0];
			object[] paramters = new object[0];
			if (arguments != null) 
			{
				types = new Type[((Cons)arguments).Length()];
				paramters = new object[((Cons)arguments).Length()];
				int loop = 0;
				foreach (object argument in (Cons)arguments) 
				{
					types[loop] = argument.GetType();
					paramters[loop] = argument;
					loop++;
				}
			}
			
			ConstructorInfo constructorInfo = type.GetConstructor(types);

			if (constructorInfo == null) 
				throw new LSharpException(string.Format("No such constructor for {0}",type));

			return constructorInfo.Invoke(paramters);
		}

		/// <summary>
		/// Calls a .NET method.
		/// The first argument is the object to which the method is attached.
		/// Passes the rest of the arguments to the appropriate constructor
		/// </summary>
		/// <param name="method"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public static object Call(String method, Cons arguments) 
		{
			BindingFlags bindingFlags = BindingFlags.IgnoreCase  
				| BindingFlags.Public 
				| BindingFlags.NonPublic; 

      string methname = method;
      string typename = string.Empty;
      Type type = null;

      int i = methname.LastIndexOf(".");
      
      if (i >= 0)
      {
        methname = methname.Substring(i + 1);
        typename = method.Substring(0, i);
        type = TypeCache.FindType(typename);
      }

			// Is it a method on a static type or an object instance ?
      if (type == null)
      {
        if (arguments.First() is Symbol) 
        {
          bindingFlags = bindingFlags | BindingFlags.Static | BindingFlags.FlattenHierarchy;
          // Find the type object from its name
          type = TypeCache.FindType(arguments.First().ToString());
        }
        else 
        {
          bindingFlags = bindingFlags | BindingFlags.Instance;
          type = arguments.First().GetType();
        }
      }
      else
      {
        bindingFlags = bindingFlags | BindingFlags.Instance;
      }

      if (type == null)
      {
        throw new LSharpException(string.Format("Call: No such type '{0}'. Did you forget a 'using'?", arguments.First()));
      }

			Type[] types = new Type[arguments.Length() -1];
			object[] parameters = new object[arguments.Length() -1];
			int loop = 0;
			if (arguments.Rest() != null)
				foreach (object argument in (Cons)arguments.Rest()) 
				{
					types[loop] = argument.GetType();
					parameters[loop] = argument;
					loop++;
				}

			// Start by looking for a method call
			MethodInfo m = type.GetMethod(methname, 
						bindingFlags | BindingFlags.InvokeMethod
						,null,types,null);
			if (m != null)
				return m.Invoke(arguments.First(),parameters);

			// Now loook for a property get
			PropertyInfo p = type.GetProperty(methname,bindingFlags | BindingFlags.GetProperty,
				null,null, types,null);
      if (p != null)
      {
        return p.GetGetMethod().Invoke(arguments.First(),parameters);
      }

			// Now look for a field get
			FieldInfo f = type.GetField(methname,bindingFlags | BindingFlags.GetField);
			if (f != null)
				return f.GetValue(arguments.First());
			

			// or an event ?

			throw new LSharpException(string.Format("Call: No such method, property or field '{1}.{0}({2})'", 
        method.ToString(),type, TypeString(types, parameters)));
		}

    static string TypeString(Type[] ts, object[] parameters)
    {
      string[] tss = new string[ts.Length];

      for (int i = 0; i < tss.Length; i++)
      {
        tss[i] = ts[i].Name + "=" + Printer.WriteToString(parameters[i]);
      }

      return string.Join(", ", tss);
    }

		/// <summary>
		/// Applies a function to its arguments. The function can be a built in L Sharp function,
		/// a closure or a .net method
		/// </summary>
		/// <param name="function"></param>
		/// <param name="arguments"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public static object Apply (object function, object arguments, Environment environment) 
		{
			if (function is Function) 
			{
				return ((Function) function) ((Cons)arguments,environment);
			}

			// If function is an LSharp Closure, then invoke it
			if (function is Closure) 
			{
				if (arguments == null)
					return ((Closure)function).Invoke();
				else
					return ((Closure)function).Invoke((Cons)arguments);
			} 
			else 
			{
        // It must be a .net method call
        return Call(function.ToString(),(Cons)arguments);
			}
		}

    public static object ReadString(string expression) 
    {
      return ReadString(expression, new Environment());
    }

    public static object ReadString(string expression, Environment environment)
    {
      ReadTable readTable = (ReadTable) environment.GetValue(Symbol.FromName("*readtable*"));
      object input = Reader.Read(new StringReader(expression), readTable);
      
      return input;
    }

		public static object EvalString (string expression) 
		{
			return EvalString(expression, new Environment());
		}

		public static object EvalString (string expression, Environment environment) 
		{
			object input = ReadString(expression, environment);
			object output = Runtime.Eval(input, environment);
			return output;	
		}

    public static object Import(string assembly, Environment environment)
    {
      string fn = Path.GetFullPath(assembly);
      Assembly ass = AssemblyCache.LoadAssembly(fn);
      ass = Import(ass);
      if (ass != null)
      {
        environment.UpdateBindings();
        return Path.GetFileName(fn);
      }
      else
      {
        return null;
      }
    }

    public static Assembly Import(Assembly assembly)
    {
      if (assembly != null)
      {
        if (Attribute.IsDefined(assembly, typeof(LSharpExtensionAttribute)))
        {
          foreach (Type t in assembly.GetTypes())
          {
            if ( Attribute.IsDefined(t, typeof(FunctionAttribute)))
            {
              RegisterFunctionExtension(t);
            }
            else
              if ( Attribute.IsDefined(t, typeof(SpecialFormAttribute)))
            {
              RegisterSpecialFormExtension(t);
            }
            else
              if ( Attribute.IsDefined(t, typeof(MacroAttribute)))
            {
              RegisterMacroExtension(t);
            }
          }
        }
      }
      return assembly;
    }

    public static void RegisterSpecialFormExtension(Type t)
    {
      Environment.RegisterExtension(t, typeof(SpecialForm));
    }

    public static void RegisterFunctionExtension(Type t)
    {
      Environment.RegisterExtension(t, typeof(Function));
    }

    public static void RegisterMacroExtension(Type t)
    {
      Environment.RegisterMacroExtension(t);
    }

		public static IProfiler Profiler
		{
			get
			{
				return profiler;
			}
			set
			{
				profiler = value;
			}
		}

		public static object BackQuoteExpand(Object form, Environment environment) 
		{
			if (!(form is Cons))
				return form;

			Cons expression = (Cons) form;

			Cons result = null;
			foreach (object item in expression) 
			{
				if (item is Cons) 
				{
					Cons list = (Cons)item;
          Symbol sym = list.First() as Symbol;
					if (sym == Symbol.BACKQUOTE) 
					{
						result = new Cons(BackQuoteExpand(list.Second(),environment), result);
					}
					else if (sym == Symbol.UNQUOTE) 
					{
						result = new Cons(Runtime.Eval(BackQuoteExpand(list.Second(),environment),environment), result);
					}
					else if (sym == Symbol.SPLICE) 
					{
						Cons l = (Cons)Runtime.Eval(BackQuoteExpand(list.Second(),environment),environment);
						foreach(object o in l) 
						{
							result = new Cons(o, result);
						}
					}
					else 
					{
						result = new Cons(BackQuoteExpand(item,environment), result);
					}

				} 
				else 
				{
					result = new Cons(item, result);
				}
			}
			return result.Reverse();
		}
		
		
	}
}
