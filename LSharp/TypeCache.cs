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
using System.Reflection;
using System.IO;

namespace LSharp
{
	/// <summary>
	/// Provides a hashtable cache of common language types in order to
	/// avoid repetitive reflection. Provides features for finding types
	/// with unqualified names.
	/// </summary>
	public sealed class TypeCache
	{
		const int CAPACITY = 50;
		static readonly Hashtable typeTable = new Hashtable(CAPACITY);
		static readonly Hashtable usingTable = new Hashtable(CAPACITY);

		/// <summary>
		/// Clears the cache and resets its state to default values
		/// </summary>
		public static void Clear() 
		{
      typeTable.Clear();
      usingTable.Clear();
			Using("System");
			Using("LSharp");
		}

		/// <summary>
		/// Private constructor ensures singleton design pattern
		/// </summary>
    TypeCache()
    {
    }

		static TypeCache()
		{
      Using("System");
      Using("LSharp");
    }

		/// <summary>
		/// Finds a type given its fully qualified or unqualified name
		/// Takes advantage of the cache to speed up lookups
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type FindType (string type) 
		{
			object o = typeTable[type.ToLower()];

			if (o == null) 
			{
				o = SearchType(type);
				typeTable[type.ToLower()] = o;
			}

			return (Type) o;
		}

		public static void Using (string name) 
		{
			usingTable[name] = name;
		}

		static Type SearchType (string typeName) 
		{
			// I wonder whether there is a better way to do this, maybe using Assembly.GetAssembly() ?
			// needs further investigation

			// Look up the type in the current assembly
			Type type = Type.GetType(typeName, false,true);
			if (type != null) 
				return type;

			// Look up the type in all loaded assemblies
			foreach(Assembly assembly in AssemblyCache.GetAssemblies()) 
			{
				type = assembly.GetType(typeName,false, true);
				if (type != null) 
					return type;

			}

			// Try to use the using directives to guess the namespace ..
			foreach (string name in usingTable.Values) 
			{
				foreach(Assembly assembly in AssemblyCache.GetAssemblies()) 
				{
					type = assembly.GetType(string.Format("{0}.{1}",name, typeName),false, true);
					if (type != null) 
						return type;

				}

				type = Type.GetType(string.Format("{0}.{1}",name, typeName),false, true);
				if (type != null) 
					return type;
			}
			return null;
		}
	}
}
