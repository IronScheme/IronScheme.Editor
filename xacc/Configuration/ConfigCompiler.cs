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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Reflection;
using System.Diagnostics;

using Xacc.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Trace = Xacc.Diagnostics.Trace;

using Compiler = Xacc.Runtime.Compiler;

namespace Xacc.Configuration
{
  class ConfigCompiler
  {
    static string Cap(object a)
    {
      return Cap(a.ToString());
    }

    readonly static string PFILES = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    readonly static string STARTUP = Application.StartupPath;

    static string SubProgram(string input)
    {
      IDiscoveryService dis = ServiceHost.Discovery;
      input = input.Replace("${NETFX}", dis.NetRuntimeRoot).
        Replace("${PFILES}", PFILES).
        Replace("${XACC}", STARTUP);

      if (dis.NSISInstalled)
      {
        input = input.Replace("${NSIS}", dis.NSISPath);
      }

      if (dis.NetRuntimeSDK != null)
      {
        input = input.Replace("${NETSDK}", dis.NetRuntimeSDK);
      }

      if (dis.VCInstallRoot != null)
      {
        input = input.Replace("${VC}", dis.VCInstallRoot);
      }

      return input;
    }

    static string Stringify(object o)
    {
      if (o == null)
      {
        return EMPTY;
      }
      string v = o.ToString();
      if (v.Length == 0)
      {
        return EMPTY;
      }
      return string.Format("\"{0}\"", v);
    }

    const string EMPTY  = "string.Empty";
    const string OUTASS = "xacc.config.dll";
    const string INFILE = "xacc.config.xml";
    const string TMPFILE = "xacc.config.cs";
    const string XSDFILE = "xacc.config.xsd";
    const string XSDBUILD = "xacc.build.xsd";

    static string Cap(string a)
    {
      return char.ToUpper(a[0], System.Globalization.CultureInfo.InvariantCulture) + a.Substring(1);
    }

    public static bool CompileConfig()
    {
      Environment.CurrentDirectory = Path.GetDirectoryName(typeof(ConfigCompiler).Assembly.Location);

      try
      {
        if (File.Exists(TMPFILE))
        {
          File.Delete(TMPFILE);
        }
      }
      catch
      {
        //VISTA issue
      }

      if (File.Exists(INFILE))
      {
        if (File.Exists(OUTASS))
        {
          if (File.GetLastWriteTime(INFILE) > File.GetLastWriteTime(OUTASS) 
            || File.GetLastWriteTime(OUTASS) < File.GetLastWriteTime("xacc.dll")
            || File.GetLastWriteTime(INFILE) < File.GetLastWriteTime("xacc.ide.exe.config"))
          {
          }
          else
          {
            Trace.WriteLine("Config compiler", "Config not changed - skipping recompiling");
            goto LOADASS;
          }
        }
        else
        {

        }
      }
      else
      {
        try
        {
          using (Stream s = typeof(ConfigCompiler).Assembly.GetManifestResourceStream("Xacc.Resources.xacc.config.xml"))
          {
            using (Stream o = File.Create(INFILE))
            {
              byte[] buffer = new byte[s.Length];
              s.Read(buffer, 0, buffer.Length);
              o.Write(buffer, 0, buffer.Length);
            }
          }
        }
        catch (Exception ex)
        {
          Trace.WriteLine("Config compiler", "Could not compile - " + ex);
          return false;
        }
      }

      int res = Compile(INFILE);

      if (res > 0)
      {
        Trace.WriteLine("Config compiler", "Could not compile - xml compilation failed");
        return false;
      }

      try
      {

        bool dbg = Debugger.IsAttached;

        string cmd = Compiler.Command;
        if (ServiceHost.Discovery.NetRuntimeRoot != string.Empty)
        {
          cmd = ServiceHost.Discovery.NetRuntimeRoot + Path.DirectorySeparatorChar + cmd;
        }
        string args = Compiler.DefaultArgs;

        args += string.Format(" {0} {1} {4} {3} {2}", 
          Compiler.MakeOut(OUTASS), Compiler.MakeDebug(dbg), TMPFILE, Compiler.MakeReference("xacc.dll"),
          Compiler.MakeTarget("library"));

        Trace.WriteLine("C# config compiler", "stdin  : {0} {1}",cmd, args);

        ProcessStartInfo psi = new ProcessStartInfo(cmd, args);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;

        Process p = Process.Start(psi);

        p.WaitForExit();

        Trace.WriteLine("C# config compiler", "stderr : {0}", p.StandardError.ReadToEnd());
        Trace.WriteLine("C# config compiler", "stdout : {0}", p.StandardOutput.ReadToEnd());

        res = p.ExitCode;


        p.Dispose();

        if (res > 0)
        {
          Trace.WriteLine("C# config compiler", "Could not compile - C# compilation failed");
          return false;
        }

        try
        {
          Stream s = typeof(ConfigCompiler).Assembly.GetManifestResourceStream("Xacc.Configuration.Config.xsd");
          Stream o = File.Create(XSDFILE);
          byte[] buffer = new byte[s.Length];
          s.Read(buffer, 0, buffer.Length);
          o.Write(buffer, 0, buffer.Length);
          s.Close();
          o.Close();
        }
        catch (UnauthorizedAccessException)
        {
          // hmmm ......??
          Trace.WriteLine("Config compiler", "Error writing " + XSDFILE);
        }

        if (File.Exists(XSDBUILD))
        {
          File.Delete(XSDBUILD);
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine("Config compiler", "C# process call failed : {0}", ex);
      }


    LOADASS:
      // better NOT to load as byte to prevent locking
      if (File.Exists(OUTASS))
      {
        Assembly ass = Assembly.LoadFile(Path.GetFullPath(OUTASS));
        ComponentModel.ServiceHost.Plugin.LoadAssembly(ass);
        
        if (!File.Exists(XSDBUILD))
        {
          try
          {
            Schema.ExportSchema(XSDBUILD);
          }
          catch (Exception ex)
          {
            Trace.WriteLine("Config compiler", "Export build schema failed : {0}", ex);
          }
        }
        return true;
      }
      else
      {
        Trace.WriteLine("Config compiler", "Could not compile - " + OUTASS + " missing");
        return false;
      }
      
    }

    static int Compile(string fn) 
    {
      if (!File.Exists(fn))
      {
        return 1;
      }

      XmlSerializer ser = new XmlSerializer(typeof(config));

      ser.UnknownElement +=new XmlElementEventHandler(ser_UnknownElement);
      ser.UnknownNode +=new XmlNodeEventHandler(ser_UnknownNode);
      ser.UnreferencedObject +=new UnreferencedObjectEventHandler(ser_UnreferencedObject);

      using (Stream s = File.OpenRead(fn))
      {
        try
        {
          object res = ser.Deserialize(s);

          if (res != null)
          {
            config b = res as config;

            using (TextWriter output = new StreamWriter(Path.ChangeExtension(fn, "cs")))
            {
              output.WriteLine(@"
// Autogenerated file, do not edit!
using System;
using System.IO;
using System.Reflection;
using Xacc.Build;
using Xacc.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
              
[assembly: AssemblyTitle(""xacc.config"")]
[assembly: AssemblyVersion(""0.1.3.*"")]
[assembly:PluginProvider(typeof(Xacc.Serialization.AssemblyLoader))]

namespace Xacc.Serialization 
{
class AssemblyLoader : AssemblyPluginProvider 
{
  public override void LoadAll(IPluginManagerService svc)
  {
    Xacc.Configuration.Projects.SerializerType = typeof(ConfigProjects);
    PropertyInfo pi = typeof(ConfigProjects).GetProperty(""Projects"");

    foreach (XmlElementAttribute ea in pi.GetCustomAttributes(typeof(XmlElementAttribute), false))
    {
      Xacc.ComponentModel.ServiceHost.Project.Register(ea.Type);
    }
");
              if (b.plugin != null)
              {
                output.WriteLine("Assembly ass = null;");
                foreach (string plugin in b.plugin)
                {
                  if (File.Exists(plugin))
                  {
                    output.WriteLine("ass = Assembly.LoadFile(Path.GetFullPath({0}));", Stringify(plugin));
                    output.WriteLine("Xacc.ComponentModel.ServiceHost.Plugin.LoadAssembly(ass);");
                  }
                }
              }

              if (b.tool != null)
              {
                output.WriteLine("IToolsService ts = ServiceHost.Tools;");
                foreach (tool t in b.tool)
                {
                  output.WriteLine("ts.AddTool({0},@{1});", Stringify(t.name), Stringify(SubProgram(t.command)));
                }
              }

              if (b.language != null)
              {
                foreach (language l in b.language)
                {
                  if (l.action != null)
                  {
                    foreach (projectaction a in l.action)
                    {
                      output.WriteLine("ServiceHost.Language.AddAction({0}, typeof(Action_{1}));", Stringify(l.name), a.@ref);
                    }
                  }
                }
              }


              output.WriteLine(@"
  } 
}"
                );

              output.WriteLine();

              Hashtable invalid = new Hashtable();

              #region Actions

              if (b.action != null)
              {

                foreach (action a in b.action)
                {
                  //create a defined class for each action, if program is available
                  a.program = SubProgram(a.program);


                  output.WriteLine(@"[XmlType(""{0}"", Namespace=""xacc:build"")]", a.id);
                  output.WriteLine(@"[Name(""{0}"",""{1}"")]", a.name, a.description);
                  output.WriteLine(@"[Image(""{0}"")]", a.image);

                  if (a.inputext != null)
                  {
                    string[] iex = a.inputext.Split(';');
                    foreach (string ext in iex)
                    {
                      output.WriteLine("[InputExtension(\"{0}\")]", ext);
                    }
                  }

                  if (a.outputext != null)
                  {
                    output.WriteLine("[OutputExtension(\"{0}\")]", a.outputext);
                  }

                  if (a.multipleinputSpecified)
                  {
                    output.WriteLine("[MultipleInput({0})]", a.multipleinput.ToString().ToLower());
                  }

                  output.WriteLine("public sealed class Action_{0} : ProcessAction", a.id);
                  output.WriteLine("{");

                  if (a.outparser != null)
                  {
                    output.WriteLine("static readonly Regex outparser = new Regex(@\"{0}\",REOPTS);", a.outparser.Value);
                    output.WriteLine("protected override Regex OutParser { get {return outparser;} }");
                    output.WriteLine();
                  }

                  if (a.errparser != null)
                  {
                    output.WriteLine("static readonly Regex errparser = new Regex(@\"{0}\",REOPTS);", a.errparser.Value);
                    output.WriteLine("protected override Regex ErrorParser { get {return errparser;} }");
                    output.WriteLine();
                  }

                  if (a.script != null)
                  {
                    output.WriteLine(a.script.Value);
                    output.WriteLine();
                  }

                  output.WriteLine("public override string Program {{get {{return @\"{0}\";}}}}", a.program); //dont use stringify here
                  output.WriteLine();


                  if (a.defaultargs != null)
                  {
                    output.WriteLine("public override string DefaultArguments	{{get {{return @{0}; }}}}", Stringify(a.defaultargs));
                    output.WriteLine();
                  }

                  options opts = a.options;

                  ArrayList optionactions = new ArrayList();

                  if (opts != null)
                  {

                    foreach (optionsCategory cat in opts.category)
                    {
                      foreach (option o in cat.option)
                      {
                        if (o.extensions != null && o.type == optionType.input)
                        {
                          optionactions.Add(o);
                        }

                        if (o.argprefix == null || o.argprefix == string.Empty)
                        {
                          o.argprefix = opts.argprefix;
                        }
                        if (o.prefix == null || o.prefix == string.Empty)
                        {
                          o.prefix = opts.prefix;
                        }

                        string argtype = "string";

                        switch (o.argtype)
                        {
                          case optionArgtype.@bool:
                            argtype = "bool";
                            break;
                          case optionArgtype.@int:
                            argtype = "int";
                            break;
                        }

                        output.WriteLine(@"static readonly Option __{0} = new Option({1},
      {2},
      {3},
      {4},
      {5},
      {6},
      {7},
      {8},
      {9},
      {10},
      OptionType.{11}", o.form, Stringify(o.name), Stringify(o.form), Stringify(argtype), Stringify(cat.name), 
                          Stringify(o.description), Stringify(o.prefix), o.required.ToString().ToLower(), 
                          Stringify(o.argquote == "\"" ? "\\\"" : o.argquote ), 
                          Stringify(o.argseperator), Stringify(o.argprefix), Cap(o.type));

                        bool isarray = o.argseperator.Length > 0 || o.extensions != null;

                        if (o.extensions != null)
                        {
                          output.WriteLine(@", ""{0}"".Split('|')", o.extensions);
                        }
                        else
                        {
                          output.WriteLine(@", ZEROARRAY");
                        }

                        if (o.allowedvalues != null)
                        {
                          output.WriteLine(@",""{0}""", string.Join("\",\"", o.allowedvalues.Split('|')));
                        }
                        output.WriteLine(");");
                        output.WriteLine("");

                        if (o.form == null || o.form == string.Empty)
                        {
                          o.form = "custom";
                        }

                        if (o.type != optionType.output)
                        {
                          if (!isarray) // not an array
                          {
                            output.WriteLine(@"[XmlElement(""{0}"", IsNullable={1})]", 
                              o.form, (!o.required).ToString().ToLower());
                          }
                          else
                          {
                            output.WriteLine(@"[XmlElement(""{0}"")]", o.form);
                          }
                  
                          output.WriteLine("public string{0} {1}", (isarray ? "[]" : "") , Cap(o.form));
                          output.WriteLine("{");
                          output.WriteLine("get {{return (string{0}) options[__{1}];}}", (isarray ? "[]" : "") , o.form);
                          output.WriteLine("set {{options[__{0}] = value;}}", o.form);
                          output.WriteLine("}");
                          output.WriteLine("");
                        }
                      }
                    }
                  }

                  output.WriteLine("public Action_{0}()", a.id);
                  output.WriteLine("{");
                  if (!File.Exists(a.program))
                  {
                    output.WriteLine("IsAvailable = false;");  
                  }
                  if (a.envvars != null)
                  {
                    foreach (envvar ev in a.envvars)
                    {
                      output.WriteLine("EnvironmentVariables.Add(@{0}, @{1});", Stringify(ev.name), Stringify(SubProgram(ev.Value)));
                    }
                  }
                  foreach (option o in optionactions)
                  {
                    output.WriteLine("AddOptionAction( new OptionAction_{0}(this, __{0}));", o.form);
                  }
                  output.WriteLine("}");

                  output.WriteLine();
                
                  foreach (option o in optionactions)
                  {
                    output.WriteLine("[Name({0},{1})]", Stringify(o.name), Stringify(o.description));
                    output.WriteLine("sealed class OptionAction_{0} : OptionAction", o.form);
                    output.WriteLine("{");
                    output.WriteLine("public OptionAction_{0}(Action_{1} ca, Option o) : base(ca,o){{}}", o.form, a.id);
                    output.WriteLine("}");
                    output.WriteLine();
                  }

                  output.WriteLine("}");

                }
              }
              #endregion

              #region Projects

              ArrayList plist = new ArrayList();
              
              if (b.project != null)
              {

            
                foreach (project p in b.project)
                {
                  output.WriteLine(@"[Name(""{0}"",""{1}"")]", p.name, p.description);
                  output.WriteLine(@"[XmlType(""{0}"", Namespace=""xacc:build"")]", p.id);
                  if (p.image != null && p.image != string.Empty)
                  {
                    output.WriteLine(@"[Image(""{0}"")]", p.image);
                  }
                  output.WriteLine("public sealed class Project_{0} : Project", p.id);
                  output.WriteLine("{");
                  output.WriteLine("public Project_{0}()", p.id);
                  output.WriteLine("{");

                  plist.Add(p.id);


                  if (p.action != null)
                  {
                    foreach (projectaction pa in p.action)
                    {
                      if (!invalid.ContainsKey(pa.@ref))
                      {
                        output.WriteLine(@"AddActionType(typeof(Action_{0}));", pa.@ref);
                      }
                    }
                  }
                  output.WriteLine("}");
                  output.WriteLine();

                  if (p.action != null)
                  {
                    foreach (projectaction pa in p.action)
                    {
                      if (!invalid.ContainsKey(pa.@ref))
                      {
                        output.WriteLine(@"[XmlElement(""{0}"", typeof(Action_{0}))]", pa.@ref);
                      }
                    }
                  }
                  output.WriteLine(@"[XmlElement(""null"", typeof(NullAction))]");
                  output.WriteLine("public Action[] ProjectActions { get {return Actions; } set {Actions = value;} }");
                  output.WriteLine("}");
                  output.WriteLine();
                }
              }
            
              #endregion

              output.WriteLine(@"[XmlRoot(""projects"",Namespace=""xacc:build"")]");
              output.WriteLine("public sealed class ConfigProjects : Xacc.Configuration.Projects");
              output.WriteLine("{");
              output.WriteLine();

              foreach (string id in plist)
              {
                output.WriteLine(@"[XmlElement(""{0}"", typeof(Project_{0}))]", id);
              }
              output.WriteLine("public Project[] Projects { get {return projects;} set {projects = value;} }");
              output.WriteLine("}");

              output.WriteLine();

            
              output.WriteLine("}");

            }
            Console.WriteLine(b);
          }
        }
        catch (Exception ex)
        {
          Trace.WriteLine("C# code generation failed", ex.ToString());
          return 1;
        }
      }
      return 0;
    }

    static void ser_UnknownElement(object sender, XmlElementEventArgs e)
    {
      Trace.WriteLine("XML Deserialization", "Unknown Element : {0}", e.Element);
    }

    static void ser_UnknownNode(object sender, XmlNodeEventArgs e)
    {
      Trace.WriteLine("XML Deserialization", "Unknown Node : {1} : {0}", e.Text, e.Name);
    }

    static void ser_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
    {
      Trace.WriteLine("XML Deserialization", "Unreferenced Object : {0}", e.UnreferencedObject);
    }
	}
}
