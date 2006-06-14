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
	/// Keeps a list of the assemblies that are loaded. Ensures that we don't
	/// load an assembly more than once. Uses a Singleton design pattern (there can
	/// only be one AssemblyCache).
	/// </summary>
	public sealed class AssemblyCache
	{
		const int CAPACITY = 20;
		static readonly Hashtable assemblyTable = new Hashtable(CAPACITY);

		/// <summary>
		/// Private constructor ensures singleton design pattern
		/// </summary>
    AssemblyCache()
    {
    }

		static AssemblyCache()
		{
      AppDomain ad = AppDomain.CurrentDomain;
      foreach (Assembly ass in ad.GetAssemblies())
      {
        Add(ass);
      }
      ad.AssemblyLoad +=new AssemblyLoadEventHandler(AssemblyLoad);
		}

		/// <summary>
		/// Loads an assembly, either from the GAC or from a file
		/// </summary>
		/// <param name="assembly">An assembly name or assembly file name</param>
		/// <returns>An Assembly object</returns>
		public static Assembly LoadAssembly (string assembly) 
		{
			object o = assemblyTable[assembly];
			if (o == null) 
			{
				if (Path.IsPathRooted(assembly))
					o = Assembly.LoadFrom(assembly);
				else
					o = Assembly.LoadWithPartialName(assembly);
				
				assemblyTable[assembly] = o;
			}

			return (Assembly) o;
		}

		/// <summary>
		/// Adds a new assembly to the assembly cache
		/// </summary>
		/// <param name="assembly"></param>
		public static void Add(Assembly assembly) 
		{
			assemblyTable[assembly.FullName] = assembly;
		}

		/// <summary>
		/// Removes an assembly from the assembly cache
		/// </summary>
		/// <param name="assembly"></param>
		public static void Remove(Assembly assembly) 
		{
			assemblyTable.Remove(assembly.FullName);
		}


		/// <summary>
		/// Returns an array of all loaded assemblies
		/// </summary>
		/// <returns></returns>
		public static Assembly[] GetAssemblies() 
		{
			return (Assembly[]) new ArrayList(assemblyTable.Values).ToArray(typeof(Assembly));
    }

    static void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
    {
      Add(args.LoadedAssembly);
    }
  }
}
