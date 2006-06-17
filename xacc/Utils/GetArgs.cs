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
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#if TESTGA
[assembly:AssemblyCopyright("(c)2004 leppie@wdsl.co.za")]
[assembly:AssemblyVersion("1.0.1.0")]
#endif

namespace Xacc.Utils
{
	#region Test code
#if TESTGA
	[ArgOptions(MessageBox=false,AllowShortcut = true, CaseSensitive=false, Prefix="-", Seperator=" ", ShortcutLength=1)]
	class MyArgs : xacc.Utils.GetArgs
	{
		//			argtype			argname				default value
		[ArgItem("Specifies the level of stuff", Shortname="lev")]
		public	int					level				= 5;
		
		[ArgItem("The best server in the world")]
		public	string			server			= "xacc.sf.net";
		
		public	bool				debug;
		
		[ArgItem(
@"This is long description
for usernames, because it takes
multiple arguments and could
confuse a user.")]
		public	string[]		usernames;
		
		public	DayOfWeek		day;
	}

	class Test 
	{
		// compile with: 
		// -d:TEST -doc:GetArgs.xml -nowarn:0649,1591 GetArgs.cs
		// run with:
		// -lev : 10 -d -usernames: {leppie, is , the, 1337357} -DAY : Monday
		[STAThread]
		static void Main()
		{
			MyArgs a = new MyArgs();
			Console.WriteLine("level:     {0}", a.level);
			Console.WriteLine("server:    {0}", a.server);
			Console.WriteLine("debug:     {0}", a.debug);
			Console.WriteLine("day:       {0}", a.day);

			if (a.usernames != null)
			{
				Console.WriteLine("usernames:");
				Console.WriteLine("{");
				foreach (string name in a.usernames)
				{
					Console.WriteLine("  {0}, ", name);
				}
				Console.WriteLine("}");
			}
		}
	}

#endif
	#endregion

	/// <summary>
	/// Provides customization of argument parsing.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public class ArgOptionsAttribute : Attribute
	{
		string prefix			= "-";
		string seperator	= ":";
		bool casesensistive = true;
		bool allowshortcut	= false;
		bool msgbox					= false;
		int shortcutlength  = 1;
    bool pauseonerror   = false;
    bool ignoreunknown  = true; 

		/// <summary>
		/// The prefix to use for the argument name eg '-'
		/// </summary>
		public string Prefix
		{
			get {return prefix;}
			set {prefix = value;}
		}

  	/// <summary>
		/// The seperator to use for the argument eg ':'
		/// </summary>
		public string Seperator
		{
			get {return seperator;}
			set {seperator = value;}
		}

		/// <summary>
		/// Specifies whether argument names are case sensitive.
		/// </summary>
		/// <remarks>Case sensitive by default</remarks>
		public bool CaseSensitive
		{
			get {return casesensistive;}
			set {casesensistive = value;}
		}

    /// <summary>
    /// Specifies whether unknown arguments are ignored.
    /// </summary>
    /// <remarks>true by default</remarks>
    public bool IgnoreUnknownArguments
    {
      get {return ignoreunknown;}
      set {ignoreunknown = value;}
    }


    /// <summary>
    /// Specifies whether the user must press enter before an error condition exits.
    /// </summary>
    /// <remarks>False by default</remarks>
    public bool PauseOnError
    {
      get {return pauseonerror;}
      set {pauseonerror = value;}
    }
    
    /// <summary>
		/// Whether shortcut argument names will be allowed.
		/// </summary>
		/// <remarks>
		/// The shortname will be the 1st letter of the argument name. If another argument starts
		/// with the same letter, the second shortname will not be used. See ArgItemAttribute for
		/// customization.
		/// </remarks>
		public bool AllowShortcut
		{
			get {return allowshortcut;}
			set {allowshortcut = value;}
		}

		/// <summary>
		/// Specifies the length of shortcut names, eg 1 or 3.
		/// </summary>
		public int ShortcutLength
		{
			get {return shortcutlength;}
			set {shortcutlength = value;}
		}

		/// <summary>
		/// Specifies whether a MessageBox will be shown rather than printing to the console.
		/// </summary>
		public bool MessageBox
		{
			get {return msgbox;}
			set {msgbox = value;}
		}

	}

  /// <summary>
  /// The default input argument
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=false)]
  public class DefaultArgAttribute : ArgItemAttribute
  {
  }


	/// <summary>
	/// Allows customization of specific argument items.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple=false, Inherited=false)]
	public class ArgItemAttribute : Attribute
	{
		string shortname		= null;
		string description	= null;
    string name         = null;

    /// <summary>
    /// Creates an instance of ArgItemAttribute.
    /// </summary>
		public ArgItemAttribute()
		{
		}

		/// <summary>
		/// Creates an instance of ArgItemAttribute with the description of the argument.
		/// </summary>
		/// <param name="description">the description of the argument</param>
		public ArgItemAttribute(string description)
		{
			this.description = description;
		}

		/// <summary>
		/// The description of the argument.
		/// </summary>
		public string Description
		{
			get {return description;}
			set {description = value;}
		}

    /// <summary>
    /// The name, if different, of the argument (if used).
    /// </summary>
    public string Name
    {
      get { return name; }
      set { name = value;}
    }

		/// <summary>
		/// The short form of the argument (if used).
		/// </summary>
		public string Shortname
		{
			get {return shortname;}
			set {shortname = value;}
		}
	}


	/// <summary>
	/// An ultra simplistic class for managing command line arguments parsing.
	/// </summary>
	/// <remarks>
	/// <p>Just inherit from this class making all args that need to be parsed
	/// public fields. Once you instantiate the inherited class the commandline
	/// arguements will be parsed and will be applied to the field in a
	/// type-safe fashion.</p>
	/// <list type="bullet">
	/// <item>Pass -? or -help for help regarding usage from the commandline.</item>
	/// <item>Argument values cannot contain spaces unless it is single or double quoted.</item>
	/// <item>Argument values must be convertable via a TypeConvertor.</item>
	/// <item>The class will print exceptions and/or any errors to to Console.Error.</item>
	/// <item>If an exception is generated, the application will exit and print usage.</item>
	/// <item>Only the first occurance of an argument will be accepted, additional 
	/// duplicate arguments will be ignored.</item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// [ArgOptions(MessageBox=false,AllowShortcut = true, CaseSensitive=false, Prefix="-", Seperator=":")]
	/// class MyArgs : xacc.Utils.GetArgs
	/// {
	/// 	//			argtype			argname				default value
	/// 	[ArgItem("Specifies the level of stuff", Shortname="lev")]
	/// 	public	int					level				= 5;
	/// 	
	/// 	[ArgItem("The best server in the world")]
	/// 	public	string			server			= "xacc.sf.net";
	/// 	
	/// 	public	bool				debug;
	/// 	
	/// 	[ArgItem(
	/// @"This is long description
	/// for usersames, because it takes
	/// multiple arguments and could
	/// confuse a user.")]
	/// 	public	string[]		usernames;
	/// 	
	/// 	public	DayOfWeek		day;
	/// }
	/// 
	/// class Test 
	/// {
	/// 	// compile with: 
	/// 	// -d:TEST -doc:GetArgs.xml -nowarn:0649,1591 GetArgs.cs
	/// 	// run with:
	/// 	// -level: 10 -debug -usernames: {leppie, is , the, 1337357}
	/// 	[STAThread]
	/// 	static void Main(string[] args)
	/// 	{
	/// 		MyArgs a = new MyArgs();
	/// 		Console.WriteLine("level:     {0}", a.level);
	/// 		Console.WriteLine("server:    {0}", a.server);
	/// 		Console.WriteLine("debug:     {0}", a.debug);
	/// 		Console.WriteLine("day:       {0}", a.day);
	/// 
	/// 		if (a.usernames != null)
	/// 		{
	/// 			Console.WriteLine("usernames:");
	/// 			Console.WriteLine("{");
	/// 			foreach (string name in a.usernames)
	/// 			{
	/// 				Console.WriteLine("  {0}, ", name);
	/// 			}
	/// 			Console.WriteLine("}");
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>
	[ArgOptions]
	public abstract class GetArgs
	{
    static readonly Dictionary<Type, string> aliasmap = new Dictionary<Type, string>();

		static GetArgs()
		{
			aliasmap.Add(typeof(int)			, "int");
			aliasmap.Add(typeof(string)		, "string");
			aliasmap.Add(typeof(bool)			, "bool");
			aliasmap.Add(typeof(uint)			, "uint");
			aliasmap.Add(typeof(byte)			, "byte");
			aliasmap.Add(typeof(char)			, "char");
			aliasmap.Add(typeof(short)		, "short");
			aliasmap.Add(typeof(ushort)		, "ushort");
			aliasmap.Add(typeof(long)			, "long");
			aliasmap.Add(typeof(ulong)		, "ulong");
			aliasmap.Add(typeof(decimal)	, "decimal");
			aliasmap.Add(typeof(sbyte)		, "sbyte");
		}

		static string GetShortTypeName(Type t)
		{
			if (aliasmap.ContainsKey(t))
			{
				return aliasmap[t];
			}
			return t.Name;
		}

    static XmlSchema GetSchema(Type t)
    {
      if (t.IsPublic)
      {
        XmlReflectionImporter xri = new XmlReflectionImporter();
        XmlTypeMapping xtm = xri.ImportTypeMapping(t);
        XmlSchemas schemas = new XmlSchemas();
        XmlSchemaExporter xse = new XmlSchemaExporter(schemas);
        xse.ExportTypeMapping(xtm);

        foreach (XmlSchema xs in schemas)
        {
          return xs;
        }
      }
      return null;
    }

    void GetSchema(TextWriter w)
    {
      XmlSchema xs = GetSchema(GetType());
      if (xs != null)
      {
        xs.Write(w);
      }
    }

		class ArgInfo
		{
			readonly FieldInfo fi;
			readonly GetArgs container;
			bool handled = false;
			object _value;
      internal readonly object defaultvalue;
			readonly ArgItemAttribute options;

			public ArgInfo(FieldInfo fi, GetArgs container, object defaultvalue)
			{
        this.defaultvalue = defaultvalue;
				this.fi = fi;
				this.container = container;
				object[] att = fi.GetCustomAttributes(typeof(ArgItemAttribute), false);
				if (att.Length == 0)
				{
					options = new ArgItemAttribute();
				}
				else
				{
					options = att[0] as ArgItemAttribute;
				}
			}

			public ArgItemAttribute Options
			{
				get {return options;}
			}

			public string Name
			{
				get {return options.Name ?? fi.Name;}
			}

			public Type Type
			{
				get {return fi.FieldType;}
			}

			public object Value
			{
				get 
				{
					if (_value != null)
					{
						return _value;
					}
					return defaultvalue;
				}
				set 
				{
					if (!handled &&(_value == null || !_value.Equals(value)))
					{
						fi.SetValue(container, value);
						_value = value;
						//handled = true;
					}
				}
			}
		}

		string MakeArg(string name, string shortname)
		{
			if (!allowshortcut || shortname == null)
			{
				return name;
			}
			return string.Format("({0}|{1})", shortname, name);
		}

    void PrintHelp(Dictionary<string, ArgInfo> map)
		{
			TextWriter writer = null;

			if (msgbox)
			{
				writer = new StringWriter();
			}
			else
			{
				writer = Console.Out;
			}

			Assembly ass = Assembly.GetEntryAssembly();
			AssemblyName assname = ass.GetName();

			AssemblyCopyrightAttribute acopy = null;
			object[] atts = ass.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if (atts.Length > 0)
			{
				acopy = atts[0] as AssemblyCopyrightAttribute;
			}

			string progname = string.Format("{0} {1}.{2} {3}", assname.Name, assname.Version.Major, assname.Version.Minor,
				acopy == null ? string.Empty : acopy.Copyright);

			if (!msgbox)
			{
				writer.WriteLine(progname);
			}
			writer.WriteLine("Usage: {0}", assname.Name);
			writer.WriteLine("{0}? or {0}help       prints usage", prefix);

      List<string> keys = new List<string>(map.Keys);
      bool acceptsdefault = false;

			foreach (KeyValuePair<string,ArgInfo> de in map)
			{
				ArgInfo arginfo = de.Value;
				string name = de.Key;
				string shortname = arginfo.Options.Shortname;

				if (shortname == name)
				{
					continue;
				}

        if (name == MAGIC)
        {
          acceptsdefault = true;
          continue;
        }

				writer.WriteLine();

				Type t = arginfo.Type;

        if (t.IsArray)
				{
					writer.WriteLine("{2}{0,-15}{3} {{ <{1}> , ... }}", 
						MakeArg(name, shortname), 
						GetShortTypeName(t.GetElementType()),
						prefix, seperator);
				}
				else
				{
					if (t == typeof(bool))
					{
						writer.WriteLine("{2}{0,-15}  {3,-20} default: {1}", 
							MakeArg(name, shortname), 
							(bool) arginfo.defaultvalue ? "on" : "off", prefix, "(toggles)");
					}
					else
					{
						writer.WriteLine("{3}{0,-15}{4} {1,-20} {2}", 
							MakeArg(name, shortname), 
							"<" + GetShortTypeName(t) + ">", 
							arginfo.Value != null ? "default: " + arginfo.Value : string.Empty, prefix, seperator);
					}
				}
				if (arginfo.Options.Description != null)
				{
					string[] lines = arginfo.Options.Description.Split('\n');
					foreach (string line in lines)
					{
						writer.WriteLine("    {0}", line.TrimEnd('\r'));
					}
				}
			}
			writer.WriteLine();
      if (acceptsdefault)
      {
        writer.WriteLine("Non-named arguments are used as input.");
      }
			writer.WriteLine("Note: Argument names are case-{0}sensitive.", casesensitive ? string.Empty : "in");
			if (msgbox)
			{
				MessageBox.Show(writer.ToString(), progname);
			}
		}

		//note: these are static just to improve performance a bit
		static bool casesensitive, allowshortcut, msgbox, pauserr, ignoreunknown;
		static string seperator, prefix;
		static int shortlen;
		static Regex re, arrre;
    const string MAGIC = "kksjhd&^D3";
    static GetArgs defvals = null;

    /// <summary>
    /// Creates an instance of GetArgs
    /// </summary>
		protected GetArgs()
		{
			const BindingFlags flags =	BindingFlags.DeclaredOnly |
							BindingFlags.Public |	BindingFlags.Instance;

			Type argclass = GetType();

			if (re == null)
			{
				ArgOptionsAttribute aoa = argclass.GetCustomAttributes(typeof(ArgOptionsAttribute), true)[0]
					as ArgOptionsAttribute;

				casesensitive = aoa.CaseSensitive;
				allowshortcut = aoa.AllowShortcut;
				seperator			= aoa.Seperator;
				prefix				= aoa.Prefix;
				msgbox				= aoa.MessageBox;
				shortlen			= aoa.ShortcutLength;
        pauserr       = aoa.PauseOnError;
        ignoreunknown = aoa.IgnoreUnknownArguments;

				re = new Regex(string.Format(@"
(({0}																# switch
(?<name>[_A-Za-z][_\w]*)						# name (any legal C# name)
({1}																# sep + optional space
(((""(?<value>((\\"")|[^""])*)"")|	# match a double quoted value (escape "" with \)
('(?<value>((\\')|[^'])*)'))|				# match a single quoted value (escape ' with \)
(\{{(?<arrayval>[^\}}]*)\}})|				# list value (escaped for string.Format)
(?<value>\S+))											# any single value
)?)|																# sep option + list
(((""(?<value>((\\"")|[^""])*)"")|	# match a double quoted value (escape "" with \)
('(?<value>((\\')|[^'])*)'))|				# match a single quoted value (escape ' with \)
(\{{(?<arrayval>[^\}}]*)\}})|				# list value (escaped for string.Format)
(?<value>\S+)))*										# any single value",						
					Regex.Escape(prefix), 
					seperator.Trim() == string.Empty ? @"\s+" : @"\s*" + Regex.Escape(seperator) + @"\s*"
					),
					RegexOptions.Compiled | RegexOptions.ExplicitCapture| RegexOptions.IgnorePatternWhitespace);

				arrre = new Regex(@"\s*(?<value>((\\,)|[^,])+)(\s*,\s*(?<value>((\\,)|[^,])+))*\s*", // escape , with \
					RegexOptions.Compiled | RegexOptions.ExplicitCapture);
			}

			Dictionary<string, ArgInfo> argz = new Dictionary<string, ArgInfo>();

			string[] args = Environment.GetCommandLineArgs();
			string allargs = string.Join(" ", args, 1, args.Length - 1).Trim();

			if (prefix == string.Empty)
			{
				throw new ArgumentException("prefix cannot be empty string");
			}

      if (defvals == null)
      {
        defvals = this;

        string schemafilename = Assembly.GetEntryAssembly().Location + ".args.xsd";
        string deffilename = Assembly.GetEntryAssembly().Location + ".args";

        if (!File.Exists(schemafilename))
        {
          using (TextWriter w = File.CreateText(schemafilename))
          {
            GetSchema(w);
          }
        }

        if (File.Exists(deffilename))
        {
          using (TextReader deffile = File.OpenText(deffilename))
          {
            defvals = new XmlSerializer(GetType()).Deserialize(deffile) as GetArgs;
          }
        }
        else
        {
          using (TextWriter w = File.CreateText(deffilename))
          {
            StringWriter sw = new StringWriter();
            new XmlSerializer(GetType()).Serialize(sw, this);
            string all = sw.ToString().Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", 
              string.Empty).Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", 
              string.Empty);
            w.WriteLine(all);
          }
        }
      }
      else
      {
        return; //this
      }

			foreach (FieldInfo fi in argclass.GetFields(flags))
			{
				object defval = fi.GetValue(defvals);
				string n = fi.Name;
				if (!casesensitive)
				{
					n = n.ToLower();
				}

        fi.SetValue(this, defval);
				ArgInfo ai = new ArgInfo(fi, this, defval);

	
        if (ai.Options is DefaultArgAttribute)
        {
          argz.Add(MAGIC, ai);
        }
        else
        {
          argz.Add(n, ai);
          if (allowshortcut)
          {
            string sn = ai.Options.Shortname;
            if (sn == null)
            {
              int nlen = n.Length - 1;
              if (nlen > 0 && shortlen < n.Length)
              {
                sn = n.Substring(0, nlen < shortlen ? nlen : shortlen);
                if (!argz.ContainsKey(sn))
                {
                  argz.Add(sn, ai);
                  ai.Options.Shortname = sn;
                }
              }
            }
            else
            {
              if (!argz.ContainsKey(sn))
              {
                argz.Add(sn, ai);
              }
            }
          }
				}
			}

      defvals = null;

			if (allargs.StartsWith(prefix + "?") || allargs.StartsWith(prefix + "help"))
			{
				PrintHelp(argz);
				Environment.Exit(0);
			}

			Group g = null;
			bool haserror = false;
      foreach (Match m in re.Matches(allargs))
      {
        string argname = null;
        try
        {
          if (m.Value == string.Empty)
          {
            continue;
          }
          object val = null;
          if ((g = m.Groups["name"]).Success)
          {
            argname = g.Value;
            if (!casesensitive)
            {
              argname = argname.ToLower();
            }
          }
          else
          {
            argname = MAGIC;
          }

          ArgInfo arginfo = argz[argname];

          if (arginfo == null)
          {
            if (ignoreunknown)
            {
              Console.Error.WriteLine("Warning: Ignoring argument unknown '{0}'", argname);
            }
            else
            {
              Console.Error.WriteLine("Error: Argument '{0}' not known", argname);
              haserror = true;
            }
            continue;
          }

          Type t = arginfo.Type;
          if (t == null)
          {
            continue;
          }

          if (t.IsArray && argname != MAGIC)
          {
            if ((g = m.Groups["arrayval"]).Success)
            {
              Type elet = t.GetElementType();
              TypeConverter tc = TypeDescriptor.GetConverter(elet);
								
              Match arrm = arrre.Match(g.Value);

              if (arrm.Success)
              {
                Group gg = arrm.Groups["value"];

                Array arr = Array.CreateInstance(elet, gg.Captures.Count);

                for (int i = 0; i < arr.Length; i++)
                {
                  arr.SetValue(tc.ConvertFromString(gg.Captures[i].Value.Trim()), i);
                }
                val = arr;
              }
            }
          }
          else
          {
            if ((g = m.Groups["value"]).Success)
            {
              string v = g.Value;
              if (t == typeof(bool) && (v == "on" || v == "off"))
              {
                val = v == "on";
              }
              else
              {
                if (t.IsArray)
                {
                  ArrayList vals = new ArrayList();
                  if (arginfo.Value != null)
                  {
                    vals.AddRange(arginfo.Value as ICollection);
                  }
                  TypeConverter tc = TypeDescriptor.GetConverter(t.GetElementType());
                  vals.Add(tc.ConvertFromString(v));

                  val = vals.ToArray(typeof(string)) as string[];
                }
                else
                {
                  TypeConverter tc = TypeDescriptor.GetConverter(t);
                  val = tc.ConvertFromString(v);
                }
              }
            }
            else
            {
              val = t == typeof(bool);
            }
          }

          arginfo.Value = val;
          
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error: Argument '{0}' could not be read ({1})", 
            argname, ex.Message, ex.GetBaseException().GetType().Name);
          haserror = true;
        }
      }
			if (haserror)
			{
				PrintHelp(argz);
        if (pauserr && !msgbox)
        {
          Console.WriteLine("Press any key to exit");
          Console.Read();
        }
				Environment.Exit(1);
			}
		}
	}
}
