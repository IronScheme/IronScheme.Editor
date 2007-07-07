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

using System.Xml.Serialization;

using SR = System.Resources;
using ST = System.Threading;

using Xacc.CodeModel;

using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

#endregion

#if OLDBUILD
namespace Xacc.Build
{
  /// <summary>
  /// Base class for all actions
  /// </summary>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace="xacc:build")]
	[Name("n/a")]
  [MultipleInput(false)]
  [Obsolete("Moving to MSBuild")]
	public abstract class Action : ToolTask
	{
    readonly string name;

    /// <summary>
    /// Creates an instance of Action
    /// </summary>
    protected Action()
    {
      name = NameAttribute.GetName(GetType());
    }

		internal int	ImageIndex
		{
			get
			{
				IImageListProviderService ilp = ServiceHost.ImageListProvider;
        int i = ilp[this];
        if (i == 0)
        {
          //ilp.Add(GetType(), Drawing.Utils.MakeTreeImage(GetType()));
        }
				return ilp[this];
			}
		}

    /// <summary>
    /// Gets the name of the Action.
    /// </summary>
		public string Name
		{
			get {	return name;}
		}

    /// <summary>
    /// The default NullAction, doing nothing.
    /// </summary>
		public static readonly Action None = new NullAction();

    protected override string GenerateFullPathToTool()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    protected override string ToolName
    {
      get { return Name; }
    }


  }


  #region Attributes

  /// <summary>
  /// Attribute to define OutputExtension of CustomAction
  /// </summary>
  [Obsolete("Moving to MSBuild")] 
  public class OutputExtensionAttribute : Attribute
  {
    static readonly Hashtable values = new Hashtable();
    string ext;

    /// <summary>
    /// Creates an instance of the OutputExtensionAttribute.
    /// </summary>
    /// <param name="ext">the file ext</param>
    public OutputExtensionAttribute (string ext)
    {
      this.ext = ext;
    }

    /// <summary>
    /// Get the output extension associated with a Type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>the extension, if any</returns>
    public static string GetExtension(Type t)
    {
      if (values.ContainsKey(t))
      {
        return values[t] as string;
      }

      foreach (OutputExtensionAttribute a in t.GetCustomAttributes(typeof(OutputExtensionAttribute), true))
      {
        values[t] = a.ext;
        return a.ext;
      }
      values[t] = null;
      return null;
    }
  }

  /// <summary>
  /// Attribute to define InputExtension of CustomAction
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
  [Obsolete("Moving to MSBuild")]
  public class InputExtensionAttribute : Attribute
  {
    static readonly Hashtable values = new Hashtable();
    string ext;

    /// <summary>
    /// Creates an instance of the InputExtensionAttribute.
    /// </summary>
    /// <param name="ext">the file ext</param>
    public InputExtensionAttribute(string ext)
    {
      this.ext = ext;
    }

    /// <summary>
    /// Get the input extension associated with a Type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>the extension, if any</returns>
    public static string[] GetExtensions(Type t)
    {
      if (values.ContainsKey(t))
      {
        return values[t] as string[];
      }

      ArrayList exts = new ArrayList();

      foreach (InputExtensionAttribute a in t.GetCustomAttributes(typeof(InputExtensionAttribute), true))
      {
        exts.Add(a.ext);
      }
      
      return exts.ToArray(typeof(string)) as string[];
    }
  }

  /// <summary>
  /// Attribute to define MultipleInput option of CustomAction
  /// </summary>
  [Obsolete("Moving to MSBuild")]  
  public class MultipleInputAttribute : Attribute
  {
    static readonly Hashtable values = new Hashtable();
    bool ext;

    /// <summary>
    /// Creates an instance of the MultipleInputAttribute
    /// </summary>
    public MultipleInputAttribute() : this (true)
    {
    }

    /// <summary>
    /// Creates an instance of the MultipleInputAttribute
    /// </summary>
    /// <param name="allow">to allow mulitple input</param>
    public MultipleInputAttribute(bool allow)
    {
      this.ext = allow;
    }

    /// <summary>
    /// Get the MultipleInput option associated with a Type
    /// </summary>
    /// <param name="t">the type</param>
    /// <returns>result</returns>
    public static bool GetMultipleInput(Type t)
    {
      if (values.ContainsKey(t))
      {
        return (bool)values[t];
      }

      foreach (MultipleInputAttribute a in t.GetCustomAttributes(typeof(MultipleInputAttribute), true))
      {
        values[t] = a.ext;
        return a.ext;
      }
      values[t] = false;
      return false;
    }
  }

  #endregion

  /// <summary>
  /// Class to define Action with no invoke
  /// </summary>
  [Name("None")]
  [MultipleInput]
  [InputExtension("*")]
  [Obsolete("Moving to MSBuild")]
  public sealed class NullAction : CustomAction
  {

  }

  /// <summary>
  /// Base class for all CustomActions
  /// </summary>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace="xacc:build")]
  [Obsolete("Moving to MSBuild")]
  public abstract class CustomAction : Action
  {		
    /// <summary>
    /// No more info
    /// </summary>
    protected readonly Hashtable	options	= new Hashtable();

    /// <summary>
    /// Intended for internal use only.
    /// </summary>
    protected static readonly string[] ZEROARRAY = { };

    string[] input = ZEROARRAY;
    string output;
    bool isavailable = true;

    /// <summary>
    /// Gets or sets whether the action is available
    /// </summary>
    protected internal bool IsAvailable
    {
      get {return isavailable;}
      set {isavailable = value;}
    }

    /// <summary>
    /// Gets the output extenstion of the type
    /// </summary>
    public string OutputExtension
    {
      get {return OutputExtensionAttribute.GetExtension(GetType());}
    }

    /// <summary>
    /// Gets the input extenstion of the type
    /// </summary>
    public string[] InputExtension
    {
      get {return InputExtensionAttribute.GetExtensions(GetType());}
    }

    /// <summary>
    /// Gets wheter action accept multiple input files
    /// </summary>
    public bool MultipleInput
    {
      get { return MultipleInputAttribute.GetMultipleInput(GetType()); }
    }

    /// <summary>
    /// Create a default output filename if output extension is defined
    /// </summary>
    /// <param name="inputfile">the name of the input file</param>
    /// <returns>the suggested output file, or null if none</returns>
    /// <remarks>Override this method to handle your own</remarks>
    protected virtual string GetOutput(string inputfile)
    {
      if (OutputExtension == null)
      {
        return null;
      }
      return Path.ChangeExtension(inputfile, OutputExtension);
    }

    /// <summary>
    /// Gets or sets an array of input files
    /// </summary>
    [XmlElement("input")]
    public string[] Input
    {
      get {return input;}
      set 
      {
        if (value == null)
        {
          input = ZEROARRAY;
        }
        else
        {
          input = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the output filename
    /// </summary>
    [XmlElement("output", IsNullable=true)]
    [Output]
    public string Output
    {
      get 
      {
        if (output == null && input.Length > 0)
        {
          Output = GetOutput(input[0]);
        }
        return output;
      }
      set 
      {
        if (output != value)
        {
          output = value;
          if (OutputOption != null)
          {
            options[OutputOption] = value;
          }
        }
      }
    }

    /// <summary>
    /// Create an instance of a CustomAction
    /// </summary>
    protected CustomAction()
    {
      const BindingFlags BF = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

      foreach (FieldInfo fi in GetType().GetFields(BF))
      {
        if (fi.FieldType == typeof(Option))
        {
          AddOption(fi.GetValue(this) as Option);
        }
      }
    }

    [Obsolete]
    internal Action GetAction(Type t)
    {
      return subactions[t] as Action;
    }

    [Obsolete]
    Hashtable subactions = new Hashtable();

    [Obsolete]
    internal Type[] ActionTypes
    {
      get { return new ArrayList(subactions.Keys).ToArray(typeof(Type)) as Type[]; }
    }

    /// <summary>
    /// Add an OptionAction to this Action
    /// </summary>
    /// <param name="oa">the OptionAction to add</param>
    [Obsolete]
    protected void AddOptionAction(OptionAction oa)
    {
      subactions.Add(oa.GetType(), oa);
    }

    /// <summary>
    /// Add an Option to this Action
    /// </summary>
    /// <param name="o">the Option to add</param>
    protected internal void AddOption(Option o)
    {
      options.Add(o, null);
    }

    /// <summary>
    /// Gets an array of Options in this Action
    /// </summary>
    [XmlIgnore]
    [CLSCompliant(false)]
    public Option[]		Options							
    {
      get {return new ArrayList(options.Keys).ToArray(typeof(Option)) as Option[] ;}
    }

    /// <summary>
    /// Sets an option value
    /// </summary>
    /// <param name="opt">the Option to set</param>
    /// <param name="value">the value of to set the Option to</param>
    protected internal void SetOptionValue(Option opt, object value)
    {
      if (opt.IsOut)
      {
        output = value as string;
      }
      options[opt] = value;
    }

    /// <summary>
    /// Gets an option value
    /// </summary>
    /// <param name="opt">the Option to get</param>
    /// <returns>the value of the Option</returns>
    protected internal object GetOptionValue(Option opt)
    {
      return options[opt];
    }

    /// <summary>
    /// Gets the output Option, if any
    /// </summary>
    [XmlIgnore]
    public Option	OutputOption				
    {
      get 
      {
        foreach (Option o in Options)
        {
          if (o.IsOut)
          {
            return o;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Gets an option by name
    /// </summary>
    /// <param name="name">the name of the option</param>
    /// <returns>the Option associated with the name, if any</returns>
    public Option GetOption(string name)
    {
      foreach (Option o in Options)
      {
        if (o.Name == name)
        {
          return o;
        }
      }
      return null;
    }
  }

  /// <summary>
  /// Base class for all ProcessAction's
  /// </summary>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace="xacc:build")]
  [Obsolete("Moving to MSBuild")]
 	public abstract class ProcessAction : CustomAction
	{
		readonly StringDictionary envvar = new StringDictionary();

    /// <summary>
    /// Default RegexOptions for out and error parsers
    /// </summary>
    protected const RegexOptions REOPTS = (RegexOptions) 0x29;
    
    /// <summary>
    /// The line offset, if needed
    /// </summary>
    protected int lineoffset = 0;

    void LogResult(ActionResult ar)
    {
      switch (ar.Type)
      {
        case ActionResultType.Error:
          Log.LogError(null, ar.ErrorCode, null, ar.Location.Filename, ar.Location.LineNumber, ar.Location.Column, 0, 0, ar.Message);
          break;
        case ActionResultType.Warning:
          Log.LogWarning(null, ar.ErrorCode, null, ar.Location.Filename, ar.Location.LineNumber, ar.Location.Column, 0, 0, ar.Message);
          break;
        case ActionResultType.Info:
        case ActionResultType.Ok:
          Log.LogMessage(null, ar.ErrorCode, null, ar.Location.Filename, ar.Location.LineNumber, ar.Location.Column, 0, 0, ar.Message);
          break;
      }
    }

    protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
    {
      switch (messageImportance)
      {
        case MessageImportance.High:
          if (ReadError)
          {
            LogResult(ParseResult(singleLine, ErrorParser));
          }
          else goto default;
          break;
        case MessageImportance.Low:
          if (ReadOut)
          {
            LogResult(ParseResult(singleLine, OutParser));
          }
          else goto default;
          break;
        default:
          base.LogEventsFromTextOutput(singleLine, messageImportance);
          break;
      }
    }

    protected override MessageImportance StandardErrorLoggingImportance
    {
      get { return MessageImportance.High;}
    }

    protected override MessageImportance StandardOutputLoggingImportance
    {
      get { return MessageImportance.Low; }
    }

    
    /// <summary>
    /// Gets the name of the program to run
    /// </summary>
    protected abstract string Program {get;}

    /// <summary>
    /// Gets the default arguments when invoking Program
    /// </summary>
		protected virtual string DefaultArguments		
    {
      get {return string.Empty;}
    }

    protected override string GenerateFullPathToTool()
    {
      return Program;
    }

    protected override string GenerateCommandLineCommands()
    {
      return BuildOptions(string.Join(" ", Input));
    }

    protected override StringDictionary EnvironmentOverride
    {
      get { return envvar; }
    }
    
    /// <summary>
    /// Gets the out parser, if any
    /// </summary>
		protected virtual Regex OutParser
    {
      get {return null;}
    }

    /// <summary>
    /// Gets the error parser, if any
    /// </summary>
    protected virtual Regex	ErrorParser
    {
      get {return null;}
    }

		bool ReadOut							
    {
      get {return OutParser != null;}
    }

		bool ReadError						
    {
      get {return ErrorParser != null;}
    }

    /// <summary>
    /// Creates an instance of ProcessAction
    /// </summary>
    protected ProcessAction()
    {
    }

		string BuildOptions(string filename)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("{0}", DefaultArguments);

			foreach (DictionaryEntry de in options)
			{
				if (de.Value != null)
				{
          string v = de.Value as string;

          if (v != null && v != string.Empty)
          {
            Option o = de.Key as Option;
            sb.AppendFormat(" {0}",  o.Build(de.Value as string));
          }

          string[] vals = de.Value as string[];
          if (vals != null)
          {
            foreach (string val in vals)
            {
              Option o = de.Key as Option;
              sb.AppendFormat(" {0}",  o.Build(val));
            }
          }
				}
			}

      sb.AppendFormat(" {0}", filename);

			return sb.ToString();
		}


    ActionResult ParseResult(string msg, Regex type)
    {
      ActionResult result = new ActionResult(ActionResultType.Info, 0, msg, null);

      Match m = type.Match(msg);
      if (m.Groups["filename"].Success)
      {
        string fn = m.Groups["filename"].Value;

        if (fn == string.Empty)
        {
          result.Location.filename = fn;
        }
        else
        {
          fn = Path.GetFullPath(fn);
          
          string root = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
          root = char.ToLower(root[0]) + root.Substring(1) + Path.DirectorySeparatorChar;
          result.Location.filename = fn.Replace(root, "");
        }
      }
      if (m.Groups["message"].Success)
      {
        result.msg = m.Groups["message"].Value;
      }
      if (m.Groups["code"].Success)
      {
        result.code = m.Groups["code"].Value;
      }
      if (m.Groups["line"].Success)
      {
        result.Location.LineNumber = Convert.ToInt32(m.Groups["line"].Value) + lineoffset;
      }
      if (m.Groups["column"].Success)
      {
        result.Location.Column = Convert.ToInt32(m.Groups["column"].Value) - 1;
      }
      if (m.Groups["error"].Success)
      {
        result.type = ActionResultType.Error;
        result.Location.Error = true;
      }
      if (m.Groups["warning"].Success)
      {
        result.type = ActionResultType.Warning;
        result.Location.Warning = true;
      }
      if (m.Groups["info"].Success)
      {
				result.type = ActionResultType.Info;	
      }
      return result;
    }
	}

  /// <summary>
  /// Base class for Options linked to Actions
  /// </summary>
	[Name("Option")]
  [Obsolete("Moving to MSBuild")]
	public abstract class OptionAction : Action
	{
		readonly Option option;
		readonly CustomAction action;
    readonly TreeNode optionnode = new TreeNode();

    /// <summary>
    /// The treenode associated with the Option
    /// </summary>
    public TreeNode OptionNode
    {
      get { return optionnode;}
    }

    /// <summary>
    /// Gets the values of the Option
    /// </summary>
    /// <returns>an array of string</returns>
    public string[] GetOption()
    {
      return action.GetOptionValue(option) as string[];
    }
		
    /// <summary>
    /// Sets the Option value
    /// </summary>
    /// <param name="values">the values to set it too</param>
    /// <remarks>Previous values will be removed</remarks>
		public void SetOption(params string[] values)
		{
      action.SetOptionValue(option, values);
      optionnode.Nodes.Clear();
      int i = ImageIndex;
      
      foreach (string s in values)
      {
        TreeNode sn = new TreeNode(Path.GetFileName(s), i,i);
        sn.Tag = s;
        optionnode.Nodes.Add(sn);
      }
		}

    /// <summary>
    /// Gets a list of input extenstion for the OptionAction
    /// </summary>
    public string[] Extensions
    {
      get {return option.Extensions;}
    }

    /// <summary>
    /// Creates an instance of OptionAction
    /// </summary>
    /// <param name="action">the associate Action</param>
    /// <param name="option">the associate Option</param>
		public OptionAction(CustomAction action, Option option)
		{
      optionnode.ImageIndex = optionnode.SelectedImageIndex = 1;
      optionnode.Text = NameAttribute.GetName(GetType());
			this.action = action;
			// dont break this again!
			//action.AddDependancy(this);
			this.option = option;
		}
	}
}
#endif