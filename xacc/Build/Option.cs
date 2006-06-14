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

#region Includes
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;

using SR = System.Resources;
#endregion

namespace Xacc.Build
{
  /// <summary>
  /// The type of the Option
  /// </summary>
  public enum OptionType
  {
    /// <summary>
    /// Normal
    /// </summary>
    Normal,
    /// <summary>
    /// Output option
    /// </summary>
    Output,
    /// <summary>
    /// Input option
    /// </summary>
    Input,
  }

  /// <summary>
  /// Defines options for CustomAction
  /// </summary>
	public sealed class Option
	{
		string name, 
			category, 
			description, 
			form, 
			formprefix, 
			argquote, 
			argsep,
			argprefix;

		string[] allowedvalues;
    string[] extensions;

		string argtype;
		OptionType type = 0;
    bool required = false;

    /// <summary>
    /// Creates an Option
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="form">the form</param>
    /// <param name="argtype">the argument type</param>
    /// <param name="category">the argument category</param>
    /// <param name="description">description</param>
    /// <param name="formprefix">the form prefix</param>
    /// <param name="required">if Option is required</param>
    /// <param name="argquote">the argument quote</param>
    /// <param name="argsep">the argument seperator</param>
    /// <param name="argprefix">the argument prefix</param>
    /// <param name="type">the Option type</param>
    /// <param name="extensions">associated input extensions</param>
    /// <param name="allowedvalues">list of allowed values</param>
		public Option(string name, string form, string argtype, string category, string description, string formprefix,
      bool required, string argquote, string argsep, string argprefix, OptionType type, string[] extensions, params string[] allowedvalues)
		{
			this.name = name;
			this.form = form;
			this.argtype = argtype;
      this.category = category;
      this.description = description;
      this.formprefix = formprefix;
      this.required = required;
      this.argquote = argquote;
      this.argsep = argsep;
      this.argprefix = argprefix;
      this.type = type;
      this.extensions = extensions;
      this.allowedvalues = allowedvalues;
		}

    /// <summary>
    /// Gets an array of input extensions if any
    /// </summary>
    public string[] Extensions
    {
      get {return extensions;}
    }

    /// <summary>
    /// Gets whether this is an output option
    /// </summary>
    public bool IsOut 
    {
      get {return type == OptionType.Output;}
    }

    /// <summary>
    /// Builds the Option to be passed to Action
    /// </summary>
    /// <param name="value">the value</param>
    /// <returns>the transformed string</returns>
		public string Build(string value)
		{
			if (argtype == "bool")
			{
				return string.Format("{0}{1}", FormPrefix, Form);
			}
      if (form == "custom")
      {
        return string.Format("{0}{1}", FormPrefix, value);
      }
      if (form == "dependson")
      {
        return string.Empty;
      }
      else
      {
        return string.Format("{0}{1}{2}{4}{3}{4}", FormPrefix, Form, ArgumentPrefix, value, ArgumentQuote);
      }
		}

    /// <summary>
    /// Gets the name of the Option
    /// </summary>
		public string			Name								{get {return name;}}

    /// <summary>
    /// Gets the category of the Option
    /// </summary>
		public string			Category						{get {return category;}}

    /// <summary>
    /// Gets the description of the Option
    /// </summary>
		public string			Description					{get {return description;}}

    /// <summary>
    /// Gets the form of the Option
    /// </summary>
		public string			Form								{get {return form;}}

    /// <summary>
    /// Gets the form prefix of the Option
    /// </summary>
		public string			FormPrefix					{get {return formprefix;}}

    /// <summary>
    /// Gets wheter the Option is required
    /// </summary>
		public bool			  Required						{get {return required;}}

    /// <summary>
    /// Gets the argument quote of the Option
    /// </summary>
		public string			ArgumentQuote				{get {return argquote;}}

    /// <summary>
    /// Gets the argument seperator of the Option
    /// </summary>
		public string			ArgumentSeperator		{get {return argsep;}}

    /// <summary>
    /// Gets the argument prefix of the Option
    /// </summary>
		public string			ArgumentPrefix			{get {return argprefix;}}

    /// <summary>
    /// Gets the argument type of the Option
    /// </summary>
		public string			ArgumentType				{get {return argtype;}}

    /// <summary>
    /// Gets the AllowedValues of the Option
    /// </summary>
		public string[]		AllowedValues				{get {return allowedvalues;}}

    /// <summary>
    /// Gets the type of the Option
    /// </summary>
		public OptionType Type      					{get {return type;}}
	}

}
