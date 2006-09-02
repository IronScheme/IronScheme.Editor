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
using System.Xml;
using System.Xml.Serialization;
using Xacc.CodeModel;
using Xacc.Collections;

using Utility = Xacc.Runtime.Compression;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using SR = System.Resources;

using Microsoft.Build.BuildEngine;
using BuildProject = Microsoft.Build.BuildEngine.Project;

using Microsoft.Build.Framework;

#endregion

namespace Xacc.Build
{
  /// <summary>
  /// Defines the default input extension for a Project
  /// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public class DefaultExtensionAttribute : Attribute
	{
		readonly string ext;

    /// <summary>
    /// Creates an instance of DefaultExtensionAttribute
    /// </summary>
    /// <param name="ext">the input extension</param>
		public DefaultExtensionAttribute(string ext)
		{
			this.ext = ext;
		}

    /// <summary>
    /// Gets the input extension, if any
    /// </summary>
		public string Extension 
    {
      get {return ext;}
    }
	}

  /// <summary>
  /// EventArgs for Project related events
  /// </summary>
	public class ProjectEventArgs	: EventArgs
	{
		
	}

  /// <summary>
  /// EventHandler for Project related events
  /// </summary>
	public delegate void ProjectEventHandler(object prj, ProjectEventArgs e);

  /// <summary>
  /// Base class for all Projects
  /// </summary>
  [Image("Project.Type.png")]
	public class Project
	{
    /// <summary>
    /// Gets the string representation of the project
    /// </summary>
    /// <returns>the project name</returns>
    public override string ToString()
    {
      return ProjectName;
    }

    #region Fields & Properties

    readonly BuildProject prj = new BuildProject();

    internal BuildProject MSBuildProject
    {
      get { return prj; }
    }

		readonly Hashtable sources = new Hashtable();
    readonly TreeNode rootnode = new TreeNode();
    TreeNode referencesnode;
    TreeNode propertiesnode;

 		static readonly BinaryFormatter FORMATTER = new BinaryFormatter();

    /// <summary>
    /// Event for when project is closed
    /// </summary>
		public event ProjectEventHandler Closed;

    /// <summary>
    /// Event for when project is opened
    /// </summary>
    public event ProjectEventHandler Opened;

    /// <summary>
    /// Event for when project is saved
    /// </summary>
		public event ProjectEventHandler Saved;

    readonly Hashtable actions = new Hashtable();
 
    FileSystemWatcher fsw = new FileSystemWatcher();

    internal ICodeModule[] References
    {
      get {return data.references;}
      set {data.references = value;}
    }

    bool startup = false;

    /// <summary>
    /// Gets or sets whether project is the startup project
    /// </summary>
    public bool Startup
    {
      get {return startup;}
      set 
      {
        if (startup != value)
        {
          startup = value;
          if ( rootnode.NodeFont == null)
          {
            rootnode.NodeFont = SystemInformation.MenuFont;
          }

          rootnode.NodeFont = new Font(rootnode.NodeFont, value ? FontStyle.Bold : FontStyle.Regular);
        }
      }
    }

    //readonly 
    ObjectTree projectautotree = new ObjectTree();

    internal ObjectTree ProjectAutoCompleteTree
    {
      get {return projectautotree;}
    }

    internal ObjectTree AutoCompleteTree
    {
      get {return data.autocompletetree;}
      set {data.autocompletetree = value;}
    }

    internal bool FileWatcherEnabled
    {
      get {return fsw.EnableRaisingEvents;}
      set {fsw.EnableRaisingEvents = value;}
    }

    /// <summary>
    /// Gets a breakpoint associate with the file
    /// </summary>
    /// <param name="filename">filename</param>
    /// <param name="linenr">the line number</param>
    /// <returns>the breakpoint if any</returns>
    public Breakpoint GetBreakpoint(string filename, int linenr)
    {
      Hashtable bps = GetBreakpoints(filename);
      if (bps == null)
      {
        return null;
      }
      return bps[linenr] as Breakpoint;
    }

    /// <summary>
    /// Get all breakpionts in a file
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <returns>the breakpoints</returns>
    public Hashtable GetBreakpoints(string filename)
    {
      filename = GetRelativeFilename(filename);
      return data.breakpoints[filename] as Hashtable;
    }

    internal void AddPairings(string filename, ArrayList pairings)
    {
      filename = GetRelativeFilename(filename);
      data.pairings[filename] = pairings;
    }

    /// <summary>
    /// Gets all the breakpoints in the project
    /// </summary>
    /// <returns>all the breakpoints</returns>
    public Breakpoint[] GetAllBreakpoints()
    {
      ArrayList bps = new ArrayList();

      foreach (Hashtable bph in data.breakpoints.Values)
      {
        bps.AddRange(bph.Values);
      }

      return bps.ToArray(typeof(Breakpoint)) as Breakpoint[];
    }

    /// <summary>
    /// Sets a breakpoint
    /// </summary>
    /// <param name="bp">the breakpoint to set</param>
    public void SetBreakpoint(Breakpoint bp)
    {
      string filename = GetRelativeFilename(bp.filename);
      Hashtable bps = data.breakpoints[filename] as Hashtable;
      if (bps == null)
      {
        data.breakpoints[filename] = bps = new Hashtable();
      }

      bps[bp.linenr - 1] = bp;
    }

    /// <summary>
    /// Removes a breakpoint from a file
    /// </summary>
    /// <param name="bp">the breakpoint to remove</param>
    public void RemoveBreakpoint(Breakpoint bp)
    {
      string filename = GetRelativeFilename(bp.filename);
      Hashtable bps = data.breakpoints[filename] as Hashtable;
      if (bps == null)
      {
        return;
      }

      bps.Remove(bp.linenr - 1);
    }

    /// <summary>
    /// Gets the CodelModel for the project
    /// </summary>
    public ICodeModule CodeModel
    {
      get {return data.model;}
    }

    /// <summary>
    /// Gets the root node for the project
    /// </summary>
    public TreeNode RootNode
    {
      get {return rootnode;}
    }

    /// <summary>
    /// Gets the default extension of the project
    /// </summary>
    public string DefaultExtension
    {
      get
      {
        foreach (DefaultExtensionAttribute e in GetType().GetCustomAttributes(typeof(DefaultExtensionAttribute), true))
        {
          return e.Extension;
        }
        return "*";
      }
    }

    /// <summary>
    /// Gets or sets the path of the project filename
    /// </summary>
    public string Location
    {
      get	{ return Normalize(prj.GetEvaluatedProperty("MSBuildProjectFullPath"));}
      set { string location = Normalize(Path.GetFullPath(value));	}
    }

    internal string DataLocation
    {
      get 
      {
        string l = RootDirectory + "\\obj";
        if (!Directory.Exists(l))
        {
          Directory.CreateDirectory(l);
        }
        return l;
      }
    }

    /// <summary>
    /// Gets the output path.
    /// </summary>
    /// <value>The output path.</value>
    public string OutputPath
    {
      get { return prj.GetEvaluatedProperty("OutputPath"); }
    }

    internal string OutputLocation
    {
      get 
      {
        if (OutputPath == null)
        {
          return ".";
        }

        string l = Path.Combine(RootDirectory, OutputPath);

        if (!Directory.Exists(l))
        {
          Directory.CreateDirectory(l);
        }
        return OutputPath;
      }
    }


    /// <summary>
    /// Gets or sets the root directopry of the project
    /// </summary>
    public string RootDirectory
    {
      get	{	return Normalize(prj.GetEvaluatedProperty("MSBuildProjectDirectory"));}
      set 
      { 
        string root = Normalize(Path.GetFullPath(value));	
        //fsw.EnableRaisingEvents = false;
        fsw.Path = root;
        //fsw.EnableRaisingEvents = true;

      }
    }

    /*
$    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
$    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>8.0.50727</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{C197EFF6-AD4E-4E44-8601-D101E0736144}</ProjectGuid>
$    <OutputType>WinExe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>SampleApp</RootNamespace>
    <AssemblyName>SampleApp</AssemblyName>
     */

    /// <summary>
    /// Gets or sets the solution dir.
    /// </summary>
    /// <value>The solution dir.</value>
    public string SolutionDir
    {
      get { return prj.GetEvaluatedProperty("SolutionDir"); }
      set { prj.SetProperty("SolutionDir", value); }
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public string Configuration
    {
      get { return prj.GetEvaluatedProperty("Configuration"); }
      set { prj.SetProperty("Configuration", value); }
    }

    /// <summary>
    /// Gets the platform.
    /// </summary>
    /// <value>The platform.</value>
    public string Platform
    {
      get { return prj.GetEvaluatedProperty("Platform"); }
    }

    /// <summary>
    /// Gets the product version.
    /// </summary>
    /// <value>The product version.</value>
    public string ProductVersion
    {
      get { return prj.GetEvaluatedProperty("ProductVersion"); }
    }

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>The schema version.</value>
    public string SchemaVersion
    {
      get { return prj.GetEvaluatedProperty("SchemaVersion"); }
    }

    /// <summary>
    /// Gets the project GUID.
    /// </summary>
    /// <value>The project GUID.</value>
    public string ProjectGuid
    {
      get { return prj.GetEvaluatedProperty("ProjectGuid"); }
    }

    /// <summary>
    /// Gets the type of the output.
    /// </summary>
    /// <value>The type of the output.</value>
    public string OutputType
    {
      get { return prj.GetEvaluatedProperty("OutputType"); }
    }

    /// <summary>
    /// Gets the app designer folder.
    /// </summary>
    /// <value>The app designer folder.</value>
    public string AppDesignerFolder
    {
      get { return prj.GetEvaluatedProperty("AppDesignerFolder"); }
    }

    /// <summary>
    /// Gets the root namespace.
    /// </summary>
    /// <value>The root namespace.</value>
    public string RootNamespace
    {
      get { return prj.GetEvaluatedProperty("RootNamespace"); }
    }

    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    /// <value>The name of the assembly.</value>
    public string AssemblyName
    {
      get { return prj.GetEvaluatedProperty("AssemblyName"); }
    }

    /// <summary>
    /// Gets a list of input files
    /// </summary>
    public string[] Sources
    {
      get	{	return new ArrayList(sources.Keys).ToArray(typeof(string)) as string[];}
    }

    /// <summary>
    /// Gets or sets the project name
    /// </summary>
    public string ProjectName
    {
      get { return prj.GetEvaluatedProperty("MSBuildProjectName"); }
      set 
      {
        CodeModel.Name = rootnode.Text = value;
      }
    }

    /// <summary>
    /// Gets the project default targets.
    /// </summary>
    /// <value>The project default targets.</value>
    public string ProjectDefaultTargets
    {
      get { return prj.GetEvaluatedProperty("MSBuildProjectDefaultTargets"); }
    }

    /// <summary>
    /// Gets the extensions path.
    /// </summary>
    /// <value>The extensions path.</value>
    public string ExtensionsPath
    {
      get { return prj.GetEvaluatedProperty("MSBuildExtensionsPath"); }
    }
	
    #endregion

    #region Action Config

    /// <summary>
    /// Gets or sets the array of Action for this project
    /// </summary>
    public string[] Actions
    {
      get 
      {
        ArrayList actions = new ArrayList();
        foreach (string k in this.actions.Keys)
        {
          if (!(bool)this.actions[k])
          {
            actions.Add(k);
          }
        }
        return actions.ToArray(typeof(string)) as string[];
      }
    }


    #endregion

    #region Constructor

    static Project()
    {
    }

    readonly bool invisible;

    internal bool IsInvisible
    {
      get {return invisible;}
    }

    /// <summary>
    /// Creates an instance of Project
    /// </summary>
		public Project()
		{
      //new string[] { "Compile", "EmbeddedResource", "Content", "None" }
      actions.Add("Compile", false);
      actions.Add("Content", false);
      actions.Add("EmbeddedResource", false);
      actions.Add("None", false);
      
      actions.Add("Reference", true);
      actions.Add("ProjectReference", true);
      actions.Add("COMReference", true);
      actions.Add("COMFileReference", true);
      actions.Add("NativeReference", true);
      actions.Add("WebReferences", true);
      actions.Add("WebReferenceUrl", true);
      actions.Add("PublishFile", true);
      actions.Add("Folder", true);
      actions.Add("Import", true);
      actions.Add("Service", true);
      actions.Add("BootstrapperFile", true);
      actions.Add("BaseApplicationManifest", true);
      
      invisible = GetType() == typeof(ScriptingService.ScriptProject);

      if (invisible)
      {
        rootnode = null;
      }
      else
      {

#if TRACE
        Opened	+=	new ProjectEventHandler(TraceOpened);
        Saved		+=	new ProjectEventHandler(TraceSaved);
        Closed	+=	new ProjectEventHandler(TraceClosed);
#endif
        IImageListProviderService ips = ServiceHost.ImageListProvider;
        if (ips != null)
        {
          ips.Add(GetType());
        }

        rootnode.SelectedImageIndex = rootnode.ImageIndex = ServiceHost.ImageListProvider[this];

        ServiceHost.Project.OutlineView.Nodes.Add(rootnode);

        rootnode.Tag = this;

        //AddActionType(typeof(NullAction));

        fsw.IncludeSubdirectories = true;
        fsw.NotifyFilter = NotifyFilters.LastWrite;
        fsw.Changed +=new FileSystemEventHandler(fsw_Changed);
      }
		}

    void fsw_Changed(object sender, FileSystemEventArgs e)
    {
      if (ServiceHost.Window.MainForm.InvokeRequired)
      {
        try
        {
          ServiceHost.Window.MainForm.Invoke( new FileSystemEventHandler(fsw_Changed), new object[] { sender, e});
        }
        catch (ObjectDisposedException)
        {
          //stupid parking window on shutdown...
        }
        return;
      }

      AdvancedTextBox atb = ServiceHost.File[e.FullPath] as AdvancedTextBox;

      if (atb != null)
      {
        try
        {
          if (atb.LastSaveTime < File.GetLastWriteTime(e.FullPath))
          {
            atb.LoadFile(e.FullPath);
          }
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
      }
    }

    #endregion
    
    #region Serialization

    internal void DeserializeProjectData()
    {
      string filename = DataLocation + "\\" + ProjectName + ".projectdata";
      if (File.Exists(filename))
      {
        try
        {
          using (System.IO.Stream s = System.IO.File.OpenRead(filename))
          {
            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);

            buffer = Utility.Decompress(buffer);

            using (System.IO.Stream s2 = new System.IO.MemoryStream(buffer))
            {
              data = FORMATTER.Deserialize(s2) as ProjectData;

              EventHandler ev = (ServiceHost.Debug as DebugService).bpboundchange;

              foreach (Breakpoint bp in GetAllBreakpoints())
              {
                bp.bound = true;
                bp.boundchanged = ev;
              }
            }
          }
        }
        catch (Exception ex)
        {
          System.Diagnostics.Trace.WriteLine(ex.Message, "Project data could not be loaded.");
        }
      }
      ServiceHost.CodeModel.Run(this);

      foreach (string file in data.openfiles)
      {
        OpenFile(file);
      }
    }

    internal void SerializeProjectData()
    {
      try
      {
        string filename = DataLocation + "\\" + ProjectName + ".projectdata";
        using (Stream s = System.IO.File.Create(filename))
        {
          using (MemoryStream s2 = new MemoryStream())
          {
            FORMATTER.Serialize(s2, data);

            byte[] buffer = s2.ToArray();

            buffer = Utility.Compress(buffer);

            s.Write(buffer, 0, buffer.Length);
          }
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex.Message, "Project data not serializable.");
      }
    }

    /// <summary>
    /// Saves the project
    /// </summary>
    public void Save()
    {
      prj.Save(Location);
      // save other data
      SerializeProjectData();
    }

    #endregion

    #region Persisted data

    /// <summary>
    /// Adds references and generates project tree
    /// </summary>
    /// <param name="refs">the references</param>
    public void AddReferencesAndGenerateTree(params ICodeModule[] refs)
    {
      if (References == null)
      {
        References = refs;
      }
      else
      {
        ICodeModule[] newrefs = new ICodeModule[References.Length + refs.Length];
        Array.Copy(References, newrefs, References.Length);
        Array.Copy(refs, 0, newrefs, References.Length, refs.Length);

        References = newrefs;
      }

      Tree tree = new Tree();
      tree.tree = AutoCompleteTree;

      foreach (ICodeElement ele in refs)
      {
        GenerateTree(tree, ele);
      }
    }

    /// <summary>
    /// Generates the project tree
    /// </summary>
    public void GenerateProjectTree()
    {
      Tree tree = new Tree();
      projectautotree = new ObjectTree();
      tree.tree = ProjectAutoCompleteTree;

      foreach (ICodeElement ele in (CodeModel as ICodeContainerElement).Elements)
      {
        GenerateTree(tree, ele);
      }
    }

    void GenerateTree(Tree tree, ICodeElement ele)
    {
      if (!(ele is ICodeModule) && ele.Fullname.Length > 0)
      {
        tree.Add(ele.Fullname, ele);
      }

      ICodeContainerElement cce = ele as ICodeContainerElement;
      if (cce != null)
      {
        foreach (ICodeElement e in cce.Elements)
        {
          GenerateTree(tree, e);
        }
      }
    }

    ProjectData data = new ProjectData();

    [Serializable]
    class ProjectData
    {
      public CodeModule model  = new CodeModule(string.Empty);
      public ObjectTree autocompletetree = new ObjectTree();
      public ICodeModule[] references;
      public Hashtable breakpoints = new Hashtable();
      public Hashtable pairings    = new Hashtable();
      public ArrayList openfiles   = new ArrayList();
    }

    class AssemblyLoader : MarshalByRefObject
    {
      ICodeType LoadCodeType(ICodeModule cm, Type type)
      {
        string ns = type.Namespace;
        if (ns == null)
        {
          ns = string.Empty;
        }
        CodeNamespace cns = cm[ns] as CodeNamespace;
        if (cns == null)
        {
          cns = new CodeNamespace(ns);
          cm.Add(cns);
        }

        CodeType ct = null;
        if (type.IsClass)
        {
          ct = new CodeRefType(type.Name);
        }
        else if (type.IsEnum)
        {
          ct = new CodeEnum(type.Name);
        }
        else if (type.IsInterface)
        {
          ct = new CodeInterface(type.Name);
        }
        else if (type.IsValueType)
        {
          ct = new CodeValueType(type.Name);
        }
        else
        {
          ct = new CodeValueType(type.Name);
        }

        ct.Namespace = cns;
        return ct;
      }

      ICodeModule LoadAssembly(Assembly ass)
      {
        try 
        {
          ICodeModule cm = new CodeModule(Path.GetFileName(ass.CodeBase));
          foreach (Type type in ass.GetExportedTypes())
          {

            LoadCodeType(cm, type);
  
          }
          return cm;
        }
        catch (NotSupportedException)
        {
        
        }

        return null;
      }

      public ICodeModule[] LoadAssemblies(string[] names, params string[] path)
      {
        ArrayList modules = new ArrayList();
        foreach (string name in names)
        {
          string fn = Path.GetFullPath(name);
          for (int i = 0; i < path.Length & !File.Exists(fn); i++)
          {
            fn = path[i] + Path.DirectorySeparatorChar + name;
          }
          if (File.Exists(fn))
          {
            Assembly ass = Assembly.LoadFile(fn);
            ICodeModule mod = LoadAssembly(ass);
            modules.Add(mod);
          }
        }
        return modules.ToArray(typeof(ICodeModule)) as ICodeModule[];
      }
    }

    class Tree
    {
      internal ObjectTree tree;

      static string[] Tokenize(string name, params string[] delimiters)
      {
        return Algorithms.XString.Tokenize(name, delimiters);
      }

      public void Add(string name, ICodeElement o)
      {
        string[] b = Tokenize(name, ".");
//        Trace.WriteLine(string.Format("{0} : {1,-35} : {3,-15} : {4,-40} : {2}", 
//          o.GetHashCode(), o.Name, o.Fullname, o.GetType().Name, name));
//        Trace.WriteLine(o.Fullname);
        tree.Add(b, o);
      }

      public object this[string name]
      {
        get {return tree.Accepts(Tokenize(name, ".")); }
      }
    }

    /// <summary>
    /// Loads assemblies and add to project tree
    /// </summary>
    /// <param name="assnames">the names of the assemblys</param>
    public void LoadAssemblies(params string[] assnames)
    {
      if (assnames.Length > 0)
      {
        assnames = new Set(assnames).ToArray(typeof(string)) as string[];
        AppDomain assloader = AppDomain.CreateDomain("Assembly Loader");
        assloader.SetupInformation.LoaderOptimization = LoaderOptimization.MultiDomainHost;
        AssemblyLoader aa = assloader.CreateInstanceAndUnwrap("xacc", "Xacc.Build.Project+AssemblyLoader") as AssemblyLoader;
        ICodeModule[] mods = aa.LoadAssemblies(assnames, ServiceHost.Discovery.NetRuntimeRoot);
        AppDomain.Unload(assloader);

        AddReferencesAndGenerateTree(mods);
      }
    }

    #endregion

    #region Event Invokers

    /// <summary>
    /// Fires when file has been added
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="root">the treenode</param>
    protected virtual void OnFileAdded(string filename, TreeNode root)
    {

    }

    /// <summary>
    /// Fires when a file has been removed
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="root">the treenode</param>
    protected virtual void OnFileRemoved(string filename, TreeNode root)
    {

    }

    /// <summary>
    /// Loads the specified filename.
    /// </summary>
    /// <param name="filename">The filename.</param>
    public void Load(string filename)
    {
      prj.Load(filename);
    }

    /// <summary>
    /// Fires when project is opened
    /// </summary>
    internal void OnOpened()
    {
      sources.Clear();
      rootnode.Nodes.Clear();
      Environment.CurrentDirectory = RootDirectory;
      DeserializeProjectData();

      rootnode.TreeView.BeginUpdate();

      if (AppDesignerFolder != null && Directory.Exists(AppDesignerFolder))
      {
        propertiesnode = new TreeNode(GetRelativeFilename(AppDesignerFolder));
        propertiesnode.SelectedImageIndex = propertiesnode.ImageIndex = ServiceHost.ImageListProvider[typeof(PropertiesFolder)];
        rootnode.Nodes.Add(propertiesnode);
      }

      foreach (BuildItem bi in prj.EvaluatedItemsIgnoringCondition)
      {
        if (!bi.IsImported)
        {
           AddFileLoad(bi);
        }
      }

      rootnode.TreeView.EndUpdate();
      rootnode.Expand();
      
      //fsw.EnableRaisingEvents = true;
      if (Opened != null)
      {
        Opened(this, null);
      }
    }

    /// <summary>
    /// Fires when Project has been created
    /// </summary>
    internal void ProjectCreated()
    {
      //foreach (Type t in GetOptionActionTypes())
      //{
      //  optionnodes.Add(t, t);
      //}
      rootnode.Text = ProjectName;
    }
    #endregion

    #region Misc

    /// <summary>
    /// Closes the project
    /// </summary>
 	  public void Close()
		{
      fsw.EnableRaisingEvents = false;
      fsw.Dispose();
			if (Closed != null)
			{
				Closed(this, null);
			}
		}

    /// <summary>
    /// Builds the project
    /// </summary>
    /// <returns>true if success</returns>
    public bool Build()
    {
      (ServiceHost.Build as BuildService).BuildInternal(prj);
      output.Clear();
      return true;
    }

    /// <summary>
    /// Rebuilds this project.
    /// </summary>
    /// <returns></returns>
    public bool Rebuild()
    {
      (ServiceHost.Build as BuildService).BuildInternal(prj, "Rebuild");
      output.Clear();
      return true;
    }

    /// <summary>
    /// Cleans this project.
    /// </summary>
    /// <returns></returns>
    public bool Clean()
    {
      (ServiceHost.Build as BuildService).BuildInternal(prj, "Clean");
      output.Clear();
      return true;
    }

    readonly Hashtable output = new Hashtable();
		

    #endregion

    #region Source / file

    /// <summary>
    /// Gets a relative filename
    /// </summary>
    /// <param name="filename">the file path</param>
    /// <returns>the relative path</returns>
    public string GetRelativeFilename(string filename)
    {
      return Normalize(Path.GetFullPath(filename)).Replace(RootDirectory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
    }

    static string Normalize(string filename)
    {
      if (filename.Length > 1)
      {
        if (filename[1] == ':')
        {
          return char.ToLower(filename[0])+filename.Substring(1);
        }
      }
      return filename;
    }

    /// <summary>
    /// Gets the action.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public string GetAction(string filename)
    {
      BuildItem bi = GetBuildItem(filename);
      if (bi != null)
      {
        return bi.Name;
      }
      return null;
    }

    BuildItem GetBuildItem(string filename)
    {
      filename = Normalize(Path.GetFullPath(filename));
      return sources[filename] as BuildItem;
    }

    /// <summary>
    /// Opens a file, and associates the file with the project
    /// </summary>
    /// <param name="relfilename">the filename</param>
    /// <returns>the Control hosting the file</returns>
    public Control OpenFile(string relfilename)
    {
      if (relfilename == null || relfilename == string.Empty)
      {
        return null;
      }

      relfilename = GetRelativeFilename(relfilename);
      relfilename = Normalize(relfilename);
      string filename = Path.Combine(RootDirectory, relfilename);

      IFileManagerService fms = ServiceHost.File;
      Control c = fms[filename];
      if (c == null)
      {
        c = fms.Open(filename);
        AdvancedTextBox atb = c as AdvancedTextBox;
        if (atb != null)
        {
          atb.ProjectHint = this;
          if (!data.openfiles.Contains(relfilename))
          {
            data.openfiles.Add(relfilename);
          }
          if (data.pairings.ContainsKey(relfilename))
          {
            ArrayList ppp = data.pairings[relfilename] as ArrayList; 
            atb.LoadPairings(ppp);
          }
        }
      }
      return c;
    }

    /// <summary>
    /// Closes the file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    public void CloseFile(string filename)
    {
      filename = GetRelativeFilename(filename);
      if (data.openfiles.Contains(filename))
      {
        data.openfiles.Remove(filename);
      }
    }

    /// <summary>
    /// Adds a file to the project
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="action">the Action to associate the file with</param>
		public void AddFile(string filename, string action)
		{
			AddFile(filename, action, false);
		}

    /// <summary>
    /// Gets the well known item types.
    /// </summary>
    /// <value>The well known item types.</value>
    public static string[] WellKnownItemTypes
    {
      get
      {
        return Enum.GetNames(typeof(BuildItemType));
      }
    }

    /// <summary>
    /// Gets the well known metadata.
    /// </summary>
    /// <param name="itemtype">The itemtype.</param>
    /// <returns></returns>
    public static string[] GetWellKnownMetadata(string itemtype)
    {
      try
      {
        BuildItemType bit = (BuildItemType)BITC.ConvertFromString(itemtype);
        return GetWellKnownMetadata(bit);
      }
      catch
      {
        return EMPTY;
      }
    }

    internal static Type GetWellKnownMetadataType(string itemtype)
    {
      try
      {
        BuildItemType bit = (BuildItemType)BITC.ConvertFromString(itemtype);
        return GetWellKnownMetadataType(bit);
      }
      catch
      {
        return null;
      }
    }

    static readonly TypeConverter BITC = TypeDescriptor.GetConverter(typeof(BuildItemType));
    static readonly string[] EMPTY = { };

    static string[] GetWellKnownMetadata(BuildItemType t)
    {
      return Enum.GetNames(GetWellKnownMetadataType(t));
    }

    static Type GetWellKnownMetadataType(BuildItemType t)
    {
      switch (t)
      {
        case BuildItemType.BaseApplicationManifest:
          return typeof(BaseApplicationManifestType);
        case BuildItemType.Folder:
          return typeof(FolderType);
        case BuildItemType.Import:
          return typeof(ImportType);
        case BuildItemType.Service:
          return typeof(ServiceType);
        case BuildItemType.WebReferences:
          return typeof(WebReferencesType);
        case BuildItemType.BootstrapperFile:
          return typeof(BootstrapperFileType);
        case BuildItemType.COMFileReference:
          return typeof(COMFileReferenceType);
        case BuildItemType.Compile:
          return typeof(CompileType);
        case BuildItemType.COMReference:
          return typeof(COMReferenceType);
        case BuildItemType.Content:
          return typeof(ContentType);
        case BuildItemType.EmbeddedResource:
          return typeof(EmbeddedResourceType);
        case BuildItemType.NativeReference:
          return typeof(NativeReferenceType);
        case BuildItemType.None:
          return typeof(NoneType);
        case BuildItemType.ProjectReference:
          return typeof(ProjectReferenceType);
        case BuildItemType.PublishFile:
          return typeof(PublishFileType);
        case BuildItemType.Reference:
          return typeof(ReferenceType);
        case BuildItemType.WebReferenceUrl:
          return typeof(WebReferenceUrlType);
        default:
          return null;
      }
    }



    enum BuildItemType
    {
      BaseApplicationManifest, //simpletype
      BootstrapperFile,
      COMFileReference,
      COMReference,
      Compile,
      Content,
      EmbeddedResource,
      Folder, //simpletype
      Import, //simpletype
      NativeReference,
      None,
      ProjectReference,
      PublishFile,
      Reference,
      Service, //simpletype
      WebReferenceUrl,
      WebReferences //simpletype
    }

    /*
Item Metadata   Description  
%(FullPath)     Contains the full path of the item.
%(RootDir)      Contains the root directory of the item.
%(Filename)     Contains the file name of the item, without the extension. 
%(Extension)    Contains the file name extension of the item. 
%(RelativeDir)  Contains the directory path relative to the current working directory.
%(Directory)    Contains the directory of the item, without the root directory. 
%(RecursiveDir) If the Include attribute contains the wildcard **, this metadata specifies the directory that replaced the wildcard to find the item. 
%(Identity)     The item specified in the Include attribute.
%(ModifiedTime) Contains the timestamp from the last time the item was modified. 
%(CreatedTime)  Contains the timestamp from when the item was created. 
%(AccessedTime) Contains the timestamp from the last time the time was accessed.
     */

    [Image("Folder.png")]
    enum FolderType { }

    [Image("Import.png")]
    enum ImportType { }

    [Image("WebReference.png")]
    enum WebReferencesType { }

    [Image("Service.png")]
    enum ServiceType { }

    [Image("BaseApplicationManifest.png")]
    enum BaseApplicationManifestType { }

    [Image("BootstrapperFile.png")]
    enum BootstrapperFileType
    {
      Install,
      ProductName,
      Visible
    }

    [Image("COMFileReference.png")]
    enum COMFileReferenceType
    {
      WrapperTool
    }

    [Image("Compile.Generated.png")]
    class CompileGenerated
    {
    }

    [Image("Compile.png")]
    enum CompileType
    {
      AutoGen,
      CopyToOutputDirectory,
      DependentUpon,
      DesignTime,
      DesignTimeSharedInput,
      Link,
      SubType,
      Visible
    }

    [Image("COMReference.png")]
    enum COMReferenceType
    {
      Guid,
      Isolated,
      Lcid,
      Name,
      VersionMajor,
      VersionMinor,
      WrapperTool
    }

    [Image("Content.png")]
    enum ContentType
    {
      CopyToOutputDirectory,
      CustomToolNamespace,
      DependentUpon,
      Generator,
      Group,
      IsAssembly,
      LastGenOutput,
      Link,
      PublishState,
      SubType,
      Visible
    }

    [Image("EmbeddedResource.png")]
    enum EmbeddedResourceType
    {
      CopyToOutputDirectory,
      CustomToolNamespace,
      DependentUpon,
      Generator,
      LastGenOutput,
      Link,
      LogicalName,
      SubType,
      Visible
    }

    [Image("NativeReference.png")]
    enum NativeReferenceType
    {
      HintPath,
      Name
    }

    [Image("None.png")]
    enum NoneType
    {
      CopyToOutputDirectory,
      CustomToolNamespace,
      DependentUpon,
      Generator,
      LastGenOutput,
      Link,
      Visible
    }

    [Image("ProjectReference.png")]
    enum ProjectReferenceType
    {
      Name,
      Package,
      Project
    }

    [Image("PublishFile.png")]
    enum PublishFileType
    {
      Group,
      IsAssembly,
      PublishState,
      Visible
    }

    [Image("Reference.png")]
    enum ReferenceType
    {
      Aliases,
      FusionName,
      HintPath,
      Name,
      Private,
      SpecificVersion
    }

    [Image("WebReferenceUrl.png")]
    enum WebReferenceUrlType
    {
      CachedAppSettingsObjectName,
      CachedDynamicPropName,
      CachedSettingsPropName,
      RelPath,
      ServiceLocationURL,
      UpdateFromURL,
      UrlBehavior
    }


    enum BuildPropertyType
    {
      AllowUnsafeBlocks,
      AppDesignerFolder,
      ApplicationIcon,
      ApplicationRevision,
      ApplicationVersion,
      AspNetConfiguration,
      AssemblyKeyContainerName,
      AssemblyKeyProviderName,
      AssemblyName,
      AssemblyOriginatorKeyFile,
      AssemblyOriginatorKeyFileType,
      AssemblyOriginatorKeyMode,
      AssemblyType,
      AutorunEnabled,
      BaseAddress,
      BootstrapperComponentsLocation,
      BootstrapperComponentsUrl,
      BootstrapperEnabled,
      CheckForOverflowUnderflow,
      CodeAnalysisInputAssembly,
      CodeAnalysisLogFile,
      CodeAnalysisModuleSuppressionsFile,
      CodeAnalysisProjectFile,
      CodeAnalysisRuleAssemblies,
      CodeAnalysisRules,
      CodeAnalysisUseTypeNameInSuppression,
      CodePage,
      Configuration,
      ConfigurationName,
      ConfigurationOverrideFile,
      CreateWebPageOnPublish,
      CurrentSolutionConfigurationContents,
      DebugSecurityZoneURL,
      DebugSymbols,
      DebugType,
      DefaultClientScript,
      DefaultHTMLPageLayout,
      DefaultTargetSchema,
      DefineConstants,
      DefineDebug,
      DefineTrace,
      DelaySign,
      DeployDirSuffix,
      DisableLangXtns,
      DisallowUrlActivation,
      DocumentationFile,
      EnableASPDebugging,
      EnableASPXDebugging,
      EnableSQLServerDebugging,
      EnableSecurityDebugging,
      EnableUnmanagedDebugging,
      ErrorReport,
      ExcludedPermissions,
      FallbackCulture,
      FileAlignment,
      FileUpgradeFlags,
      FormFactorID,
      GenerateManifests,
      GenerateSerializationAssemblies,
      Install,
      InstallFrom,
      InstallUrl,
      IsWebBootstrapper,
      JCPA,
      LangVersion,
      ManifestCertificateThumbprint,
      ManifestKeyFile,
      MapFileExtensions,
      MinimumRequiredVersion,
      MyType,
      NoConfig,
      NoStandardLibraries,
      NoStdLib,
      NoWarn,
      OSVersion,
      OpenBrowserOnPublish,
      Optimize,
      OptionCompare,
      OptionExplicit,
      OptionStrict,
      OutputPath,
      OutputType,
      Platform,
      PlatformFamilyName,
      PlatformID,
      PlatformName,
      PlatformTarget,
      PostBuildEvent,
      PreBuildEvent,
      ProductName,
      ProductVersion,
      ProjectGuid,
      ProjectType,
      ProjectTypeGuids,
      PublishUrl,
      PublisherName,
      RecursePath,
      ReferencePath,
      RegisterForComInterop,
      RemoteDebugEnabled,
      RemoteDebugMachine,
      RemoveIntegerChecks,
      ResponseFile,
      RootNamespace,
      RunCodeAnalysis,
      RunPostBuildEvent,
      SchemaVersion,
      SecureScoping,
      SignAssembly,
      SignManifests,
      SolutionDir,
      SolutionExt,
      SolutionFileName,
      SolutionName,
      SolutionPath,
      StartAction,
      StartArguments,
      StartPage,
      StartProgram,
      StartURL,
      StartWithIE,
      StartWorkingDirectory,
      StartupObject,
      SupportUrl,
      TargetCulture,
      TargetFrameworkVersion,
      TargetZone,
      TreatWarningsAsErrors,
      TrustUrlParameters,
      TypeComplianceDiagnostics,
      UTF8OutPut,
      UpdateEnabled,
      UpdateInterval,
      UpdateIntervalUnits,
      UpdateMode,
      UpdatePeriodically,
      UpdateRequired,
      UpdateUrl,
      UseVSHostingProcess,
      VSTO_TrustAssembliesLocation,
      WarningLevel,
      WarningsAsErrors,
      WebPage,
      Win32ResourceFile
    }
    
    /// <summary>
    /// Adds a file to the project
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="action">the Action to associate the file with</param>
    /// <param name="select">whether the file should be made active</param>
    public void AddFile(string filename, string action, bool select)
    {
      AddFile(filename, action, select, null);
    }

    void AddFileLoad(BuildItem bi)
    {
      AddFile(bi.Include, bi.Name, false, bi);
    }

    [Image("Folder.References.png")]
    class ReferencesFolder
    {
    }

    [Image("Folder.Properties.png")]
    class PropertiesFolder
    {
    }

    Hashtable deps = new Hashtable();
    Hashtable sourcenodes = new Hashtable();


    void AddFile(string filename, string action, bool select, BuildItem bi)
    {
      int i = filename.LastIndexOf("*.");
      if (i >= 0)
      {
        string pattern = filename.Substring(i);
        foreach (string file in Directory.GetFiles(i == 0 ? "." : filename.Substring(0, i), pattern))
        {
          AddFile(file, action);
        }
        return;
      }
      else
      {
        string oldfilename = filename;
        filename = filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        filename = Path.GetFullPath(filename);
        filename = Normalize(filename);

        if (sources.ContainsKey(filename))
        {
          if (bi != null && bi.Name == "Folder")
          {
          }
          else if (Array.IndexOf(Actions, action) < 0)
          {
            //not user added, VS garbage
          }
          else
          {
            MessageBox.Show(ServiceHost.Window.MainForm, "Project already contains file: " + filename,
              "Error!", 0, MessageBoxIcon.Error);
          }
          return;
        }

        if (bi.HasMetadata("Visible") && bi.GetMetadata("Visible") == "false")
        {
          return;
        }

        if (action == "BootstrapperFile" || action == "BootstrapperPackage" || action == "Service" || action == "Import")
        {
          // dunno how to handle :(, marked invisible
          return;
        }

        if (bi == null)
        {
          bi = prj.AddNewItem(action, GetRelativeFilename(filename));
          prj.MarkProjectAsDirty();
        }

        if (!actions.ContainsKey(action))
        {
          actions.Add(action, false);
        }

        TreeNode root = rootnode;

        if (bi.HasMetadata("DependentUpon"))
        {
          string du = bi.GetMetadata("DependentUpon");
          string path = Path.GetDirectoryName(Path.Combine(RootDirectory, bi.Include));
          du = Normalize(Path.Combine(path, du));

          if (sources.ContainsKey(du))
          {
            root = sourcenodes[du] as TreeNode;
            action += "Generated";
          }
          else
          {
            ArrayList sd = deps[du] as ArrayList;
            if (sd == null)
            {
              deps[du] = (sd = new ArrayList());
            }

            sd.Add(bi);
            return;
          }
        }

        sources.Add(filename, bi);

        if (!action.EndsWith("Generated"))
        {
          if (!action.EndsWith("Reference"))
          {
            string[] reldirs = (Path.GetDirectoryName(filename)
              + Path.DirectorySeparatorChar).Replace(RootDirectory, string.Empty).Trim(Path.DirectorySeparatorChar)
              .Split(Path.DirectorySeparatorChar);

            if (reldirs.Length > 0 && reldirs[0].EndsWith(":"))
            {
              //shortcut
            }
            else
            {
              for (int j = 0; j < reldirs.Length; j++)
              {
                if (reldirs[j] != string.Empty)
                {
                  TreeNode sub = FindNode(reldirs[j], root);
                  if (sub == null)
                  {
                    root.Nodes.Add(sub = new TreeNode(reldirs[j], 1, 1));
                  }
                  root = sub;
                }
              }
            }
          }
          else
          {
            if (referencesnode == null)
            {
              referencesnode = new TreeNode("References");
              referencesnode.SelectedImageIndex = referencesnode.ImageIndex = ServiceHost.ImageListProvider[typeof(ReferencesFolder)];
              rootnode.Nodes.Add(referencesnode);
            }

            root = referencesnode;
          }
        }

        string fn = Path.GetFileName(filename);

        if (string.IsNullOrEmpty(fn))
        {
          return;
        }
        
        TreeNode nnode = new TreeNode(fn);

        if (action == "ProjectReference" && bi.HasMetadata("Name"))
        {
          nnode.Text = bi.GetMetadata("Name");
        }
        nnode.Tag = bi;
        root.Nodes.Add(nnode);

        sourcenodes[filename] = nnode;

        if (deps.ContainsKey(filename))
        {
          ArrayList l = deps[filename] as ArrayList;
          deps.Remove(filename);

          foreach (BuildItem sbi in l)
          {
            AddFileLoad(sbi);
          }
        }

        root = nnode;
        
        if (select)
        {
          root.TreeView.SelectedNode = root;
        }

        string ext = Path.GetExtension(filename).TrimStart('.');

        if (action != null)
        {
          int ii = ServiceHost.ImageListProvider[action == "CompileGenerated" ? typeof(CompileGenerated) : GetWellKnownMetadataType(bi.Name)];
          root.SelectedImageIndex = root.ImageIndex = ii;
        }

        OnFileAdded(filename, root);

        if (select)
        {
          root.Expand();
          root.EnsureVisible();
        }
      }
    }

    static TreeNode FindNode(string name, TreeNode parent)
    {
      foreach (TreeNode child in parent.Nodes)
      {
        if (child.Text == name)
        {
          return child;
        }
      }
      return null;
    }

    /// <summary>
    /// Removes a file from the project
    /// </summary>
    /// <param name="filename">the filename to remove</param>
		public void RemoveFile(string filename)
		{
			filename = Path.GetFullPath(filename);
      filename = Normalize(filename);
      string relfile = GetRelativeFilename(filename);
      BuildItem bi = GetBuildItem(filename);

      prj.RemoveItem(bi);

      if (data.pairings.ContainsKey(relfile))
      {
        data.pairings.Remove(relfile);
      }

      sourcenodes.Remove(filename);
      sources.Remove(filename);
      OnFileRemoved(filename, null);
		}


    #endregion

    #region Event Handling

#if TRACE
    void TraceSaved(object p, ProjectEventArgs e)
    {
      Trace.WriteLine(string.Format("Saved({0})",(p as Project).ProjectName), "Project");
    }
    void TraceOpened(object p, ProjectEventArgs e)
    {
      Trace.WriteLine(string.Format("Opened({0})",(p as Project).ProjectName), "Project");
    }
    void TraceClosed(object p, ProjectEventArgs e)
    {
      Trace.WriteLine(string.Format("Closed({0})",(p as Project).ProjectName), "Project");
    }
#endif

    internal void ShowProps(object sender, EventArgs e)
    {
      ProcessActionDialog pad = new ProcessActionDialog(this);
      pad.ShowDialog(ServiceHost.Window.MainForm);
    }

    internal void NewFile(object sender, EventArgs e)
    {
      NewFileWizard wiz = new NewFileWizard();
	
      foreach (string lname in Actions)
      {
        wiz.prjtype.Items.Add(lname);
      }

      RESTART:

        if (wiz.ShowDialog(ServiceHost.Window.MainForm) == DialogResult.OK)
        {
          Type t = wiz.prjtype.SelectedItem as Type;

          string fn = wiz.name.Text;
          string path = wiz.loc.Text.Trim();

          if (path == string.Empty)
          {
            path = Environment.CurrentDirectory;
          }

          string fullpath = path + Path.DirectorySeparatorChar + fn;


          bool overwrite = true;

          if (File.Exists(fullpath))
          {
            switch ( MessageBox.Show(ServiceHost.Window.MainForm, "File already exists. Overwrite?", "Confirmation",
              MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
              case DialogResult.Yes:
                overwrite = true;
                break;
              case DialogResult.No:
                overwrite = false;
                break;
              case DialogResult.Cancel:
                goto RESTART;
            }
          }

          if (overwrite)
          {
            using (StreamWriter w = new StreamWriter(File.Create(fullpath)))
            {
              string ext = Path.GetExtension(fullpath).TrimStart('.');
              w.WriteLine("");
              w.Flush();
            }

            AddFile(fullpath, "None");

            ServiceHost.File.Open(fullpath);
          }
        }
    }

    internal void ExistingFile(object sender, EventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.InitialDirectory = Environment.CurrentDirectory;
      ofd.CheckFileExists = true;
      ofd.CheckPathExists = true;
      ofd.Multiselect = true;
      ofd.DereferenceLinks = true;
      ofd.RestoreDirectory = true;

      StringBuilder sb = new StringBuilder();
      StringBuilder ex = new StringBuilder();
      StringBuilder ab = new StringBuilder();

      string defext = DefaultExtension;
      Hashtable actions = new Hashtable();

      //int count = 0;

      //ab.Append("All supported files|");

      //foreach (Type act in ActionTypes)
      //{
      //  string[] extss = Xacc.Build.InputExtensionAttribute.GetExtensions(act);

      //  if (extss.Length > 0)
      //  {
      //    if (extss[0] != "*")
      //    {
      //      ex.AppendFormat("*.{0}", extss[0]);
      //      ab.AppendFormat("*.{0};", extss[0]);
      //    }

      //    for(int i = 1; i < extss.Length; i++)
      //    {
      //      if (extss[i] != "*")
      //      {
      //        ex.AppendFormat(";*.{0}", extss[i]);
      //        ab.AppendFormat("*.{0};", extss[i]);
      //      }
      //    }

      //    if (ex.Length > 0)
      //    {
      //      count++;
      //      sb.AppendFormat("{0} ({1})|{1}|", NameAttribute.GetName(act), ex);
      //      ex.Length = 0;
      //    }
      //  }
      //}
      
      //ab.Length--;
      //ab.Append("|");
      sb.Append("Text files (*.txt)|*.txt|");
      sb.Append("All files (*.*)|*.*");

      ofd.Filter = ab.ToString() + sb.ToString();

      if (ofd.ShowDialog() == DialogResult.OK)
      {
        foreach (string file in ofd.FileNames)
        {
          AddFile(file, "None");
        }
      }
    }

    internal void RunProject(object sender, EventArgs e)
    {
      if (Build())
      {
        foreach (ITaskItem item in output["Build"] as ITaskItem[])
        {
          string name = item.ItemSpec;
          if (File.Exists(name))
          {
            if (Path.GetExtension(name) == ".exe")
            {
              //run process
              System.Threading.ThreadPool.QueueUserWorkItem(delegate(object s) { Process.Start(name); });
              return;
            }
          }
        }

      }
    }

    internal void DebugProject(object sender, EventArgs e)
    {
      if (((MenuItem)sender).Checked)
      {
        ServiceHost.Debug.Exit();
      }
      else
      {
        ServiceHost.Debug.Start(this);
      }
    }



    #endregion

  }
}
