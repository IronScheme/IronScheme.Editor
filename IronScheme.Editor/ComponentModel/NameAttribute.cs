#region License
 /*	  xacc                																											*
 	*		Copyright (C) 2003-2006  Llewellyn@Pritchard.org                          *
 	*																																							*
	*		This program is free software; you can redistribute it and/or modify			*
	*		it under the terms of the GNU Lesser General Public License as            *
  *   published by the Free Software Foundation; either version 2.1, or					*
	*		(at your option) any later version.																				*
	*																																							*
	*		This program is distributed in the hope that it will be useful,						*
	*		but WITHOUT ANY WARRANTY; without even the implied warranty of						*
	*		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the							*
	*		GNU Lesser General Public License for more details.												*
	*																																							*
	*		You should have received a copy of the GNU Lesser General Public License	*
	*		along with this program; if not, write to the Free Software								*
	*		Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
#endregion

using System;
using System.Collections;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Defines name and descrition info .NET types
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple=false, Inherited=true)]
	public class NameAttribute : Attribute
	{
    static readonly Hashtable values = new Hashtable();
		readonly string desc;
		readonly string name;
    const string NODESC = "No description provided";

    /// <summary>
    /// Creates an instance of NameAttribute
    /// </summary>
    /// <param name="name">the name</param>
		public NameAttribute(string name):this(name, NODESC){}

    /// <summary>
    /// Creates an instance of NameAttribute
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="desc">the description</param>
		public NameAttribute(string name, string desc)
		{
			this.name = name;
			this.desc = desc;
		}

    /// <summary>
    /// Gets the name
    /// </summary>
		public string Name 
    { 
      get {return name;}
    }

    /// <summary>
    /// Gets the description
    /// </summary>
		public string Description 
    { 
      get {return desc;}
    }

    /// <summary>
    /// Get the NameAttribute for a specified type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>NameAttribute, if any</returns>
    public static NameAttribute Get(Type t)
    {
      if (values.ContainsKey(t))
      {
        return values[t] as NameAttribute;
      }
      foreach (NameAttribute na in t.GetCustomAttributes(typeof(NameAttribute), true))
      {
        values[t] = na;
        return na;
      }
      values[t] = null;
      return null;
    }

    /// <summary>
    /// Gets the name of a type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>the name, if any</returns>
    public static string GetName(Type t)
    {
      if (t == null)
      {
        return "<NULL>";
      }
      NameAttribute na = Get(t);
      if (na != null)
      {
        return na.Name;
      }
      return t.Name;
    }

    /// <summary>
    /// Gets the description for a type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>the description, if any</returns>
    public static string GetDescription(Type t)
    {
      NameAttribute na  = Get(t);
      if (na != null)
      {
        return na.Description;
      }
      return NODESC;
    }
	}
}
