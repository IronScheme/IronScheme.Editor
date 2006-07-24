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
	public class Project : BuildProject
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

		readonly Hashtable sources = new Hashtable();
    readonly TreeNode rootnode = new TreeNode();

    [Obsolete]
    readonly Hashtable optionnodes = new Hashtable();

    [Obsolete]
    readonly Hashtable actiontypes = new Hashtable();

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

    NullAction nullaction = new NullAction();
    Action[] actions = {};
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
      get	{ return Normalize(GetEvaluatedProperty("MSBuildProjectFullPath"));}
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

    public string OutputPath
    {
      get { return GetEvaluatedProperty("OutputPath"); }
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
      get	{	return Normalize(GetEvaluatedProperty("MSBuildProjectDirectory"));}
      set 
      { 
        string root = Normalize(Path.GetFullPath(value));	
        //fsw.EnableRaisingEvents = false;
        fsw.Path = root;
        //fsw.EnableRaisingEvents = true;

      }
    }

    /*
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>8.0.50727</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{C197EFF6-AD4E-4E44-8601-D101E0736144}</ProjectGuid>
    <OutputType>WinExe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>SampleApp</RootNamespace>
    <AssemblyName>SampleApp</AssemblyName>
     */

    public string Configuration
    {
      get { return GetEvaluatedProperty("Configuration"); }
    }

    public string Platform
    {
      get { return GetEvaluatedProperty("Platform"); }
    }

    public string ProductVersion
    {
      get { return GetEvaluatedProperty("ProductVersion"); }
    }

    public string SchemaVersion
    {
      get { return GetEvaluatedProperty("SchemaVersion"); }
    }

    public string ProjectGuid
    {
      get { return GetEvaluatedProperty("ProjectGuid"); }
    }

    public string OutputType
    {
      get { return GetEvaluatedProperty("OutputType"); }
    }

    public string AppDesignerFolder
    {
      get { return GetEvaluatedProperty("AppDesignerFolder"); }
    }

    public string RootNamespace
    {
      get { return GetEvaluatedProperty("RootNamespace"); }
    }

    public string AssemblyName
    {
      get { return GetEvaluatedProperty("AssemblyName"); }
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
      get {return GetEvaluatedProperty("ProjectName");}
      set 
      {
        CodeModel.Name = rootnode.Text = value;
      }
    }

    public string MSBuildProjectDefaultTargets
    {
      get { return GetEvaluatedProperty("MSBuildProjectDefaultTargets"); }
    }

    public string MSBuildExtensionsPath
    {
      get { return GetEvaluatedProperty("MSBuildExtensionsPath"); }
    }
	
    #endregion

    #region Action Config

    /// <summary>
    /// Gets or sets the array of Action for this project
    /// </summary>
    [Obsolete]
    public Action[] Actions
    {
      get {return actions;}
      set 
      {
        if (value == null)
        {
          actions = new Action[0];
        }
        else
        {
          actions = value;
        }
      }
    }

    [Obsolete]
    Type[] types;

    [Obsolete]
    internal Type[] ActionTypes
    {
      get 
      {
        if (types == null)
        {
          ArrayList alltypes = new ArrayList();
        
          foreach (ArrayList l in actiontypes.Values)
          {
            alltypes.AddRange(l);
          }

          types = new Set(alltypes).ToArray(typeof(Type)) as Type[];
        }
        return types;
      }
    }

    [Obsolete]
    Type[] GetOptionActionTypes()
    {
      ArrayList l = new ArrayList();
      Type[] types = ActionTypes;
      foreach (Type t in types)
      {
        if (typeof(OptionAction).IsAssignableFrom(t))
        {
          l.Add(t);
        }
      }
      return l.ToArray(typeof(Type)) as Type[];
    }

    /// <summary>
    /// Add an action type to the project
    /// </summary>
    /// <param name="actiontype">the type of the Action</param>
    [Obsolete]
    protected void AddActionType(Type actiontype)
    {
      string[] extt = InputExtensionAttribute.GetExtensions(actiontype);
      if (extt.Length == 0)
      {
        string ext = "*";
        ArrayList exts = actiontypes[ext] as ArrayList;
        if (exts == null)
        {
          actiontypes[ext] = (exts = new ArrayList());
        }
        if (!exts.Contains(actiontype))
        {
          exts.Add(actiontype);
        }
      }
      else
      {
        foreach (string ext in extt)
        {
          ArrayList exts = actiontypes[ext] as ArrayList;
          if (exts == null)
          {
            actiontypes[ext] = (exts = new ArrayList());
          }
          if (!exts.Contains(actiontype))
          {
            exts.Add(actiontype);
          }
        }
      }
    }

    [Obsolete]
    Action this[int index]
    {
      get {return actions[index] as Action;}
      set { actions[index] = value; }
    }

    /// <summary>
    /// Gets the Action associated with a filename in the project
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <returns>the associate Action</returns>
    [Obsolete]
    public Action GetAction(string filename)
    {
      filename = Normalize(Path.GetFullPath(filename));
      if (!sources.ContainsKey(filename))
      {
        return null;
      }

      return sources[filename] as Action;
    }

    [Obsolete]
    internal Action SuggestAction(Type t)
    {
      if (t == null)
      {
        return Action.None;
      }
      Debug.Assert(actions != null);
      foreach (Action a in actions)
      {
        CustomAction ca = a as CustomAction;

        if (t == a.GetType())
        {
          if (ca != null)
          {
            if (ca.MultipleInput || ca.Input.Length == 0)
            {
              return a;
            }
          }
          else
          {
            return a;
          }
        }

        if (ca != null)
        {
          foreach (Type st in ca.ActionTypes)
          {
            if (st == t)
            {
              return ca.GetAction(t);
            }
          }
        }
      }
      return Activator.CreateInstance(t) as Action;
    }

    [Obsolete]
    Action SuggestAction(string ext)
    {
      ArrayList t = actiontypes[ext] as ArrayList;
      if (t == null || t.Count == 0)
      {
        return nullaction;
      }
      return SuggestAction(t[0] as Type);
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

        AddActionType(typeof(NullAction));

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
      Save(Location);
      // save other data
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
    /// Fires when project is opened
    /// </summary>
    internal void OnOpened()
    {
      Environment.CurrentDirectory = RootDirectory;
      DeserializeProjectData();

      foreach (BuildItem bi in EvaluatedItemsIgnoringCondition)
      {
        if (!bi.IsImported)
        {
          AddFile(bi.Include);
        }
      }



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

    void BuildThreaded(object state)
    {
      Build();
    }

    /// <summary>
    /// Builds the project
    /// </summary>
    /// <returns>true if success</returns>
    //public bool Build()
    //{
    //  ServiceHost.Error.ClearErrors(this);

    //  ServiceHost.Error.OutputErrors(this, new ActionResult(ActionResultType.Ok,0, 
    //    ProjectName + " : Build succeeded", GetRelativeFilename(Location)));
    //  return true;
    //}
		

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
		public void AddFile(string filename)
		{
			string ext = Path.GetExtension(filename).TrimStart('.');
      AddFile(filename, SuggestAction(ext));
		}

    /// <summary>
    /// Adds a file to the project
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="action">the Action to associate the file with</param>
		public void AddFile(string filename, Action action)
		{
			AddFile(filename, action, false);
		}

    /// <summary>
    /// Adds a file to the project
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="action">the Action to associate the file with</param>
    /// <param name="select">whether the file should be made active</param>
    public void AddFile(string filename, Action action, bool select)
    {
      int i = filename.LastIndexOf("*.");
      if (i >= 0)
      {
        CustomAction ca = action as CustomAction;
        ca.Input = null;
        string pattern = filename.Substring(i);
        foreach (string file in Directory.GetFiles(i == 0 ? "." : filename.Substring(0, i), pattern))
        {
          AddFile(file, SuggestAction(action.GetType()));
        }
      }
      else
      {
        string oldfilename = filename;
        filename = filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        filename = Path.GetFullPath(filename);
        filename = Normalize(filename);

        if (sources.ContainsKey(filename))
        {
          if (!(action is OptionAction))
          {
            MessageBox.Show(ServiceHost.Window.MainForm, "Project already contains file: " + filename, 
              "Error!", 0,MessageBoxIcon.Error);
          }
          return;
        }

        if (action != null)
        {
          if (action is CustomAction)
          {
            CustomAction pa = action as CustomAction;
            if (pa.Input != null)
            {
              if (Array.IndexOf(pa.Input, GetRelativeFilename(filename)) < 0)
              {
                ArrayList l = new ArrayList(pa.Input);
                l.Add(GetRelativeFilename(filename));
                pa.Input = l.ToArray(typeof(string)) as string[];
              }
            }
            else
            {
              pa.Input = new string[] { GetRelativeFilename(filename) };
            }
          }
          if (action is OptionAction)
          {
            OptionAction oa = action as OptionAction;
            string[] vals = oa.GetOption();
            if (vals == null || vals.Length == 0)
            {
              oa.SetOption(oldfilename);
              rootnode.Nodes.Insert(0, oa.OptionNode);
            }
            else
            {
              if (Array.IndexOf(vals, oldfilename) < 0)
              {
                ArrayList l = new ArrayList(vals);
                l.Add(oldfilename);
                oa.SetOption(l.ToArray(typeof(string)) as string[]);
              }
              else
              {
                // just reset the dam thing!
                if (!rootnode.Nodes.Contains(oa.OptionNode))
                {
                  rootnode.Nodes.Insert(0, oa.OptionNode);
                }
                oa.SetOption(vals);
              }
            }
          }
        }

        sources.Add(filename, action);

        if (Array.IndexOf(actions, action) < 0)
        {
          Action[] a = new Action[actions.Length + 1];
          Array.Copy(actions,a, actions.Length);
          a[actions.Length] = action;
          actions = a;
        }

        if (action is OptionAction)
        {
          return; // bye bye!
        }

        TreeNode root = rootnode;
				
        string[] reldirs = (Path.GetDirectoryName(filename) 
          + Path.DirectorySeparatorChar).Replace(RootDirectory, string.Empty).Trim(Path.DirectorySeparatorChar)
          .Split(Path.DirectorySeparatorChar);

        for (int j = 0; j < reldirs.Length; j++)
        {
          if (reldirs[j] != string.Empty)
          {
            TreeNode sub = FindNode(reldirs[j], root);
            if (sub == null)
            {
              root.Nodes.Add( sub = new TreeNode(reldirs[j],1,1) );
            }
            root = sub;
          }
        }
        
        root = root.Nodes.Add(Path.GetFileName(filename));
        root.Tag = filename;

        if (select)
        {
          root.TreeView.SelectedNode = root;
        }

        string ext = Path.GetExtension(filename).TrimStart('.');

        if (action != null)
        {
          root.SelectedImageIndex = root.ImageIndex = action.ImageIndex;
        }

        OnFileAdded(filename, root);

        root.Expand();
        root.EnsureVisible();
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
      CustomAction ca = sources[filename] as CustomAction;
      if (ca != null)
      {
        ArrayList l = new ArrayList(ca.Input);
        
        l.Remove(relfile);
        ca.Input = l.ToArray(typeof(string)) as string[];
      }
      OptionAction oa = sources[filename] as OptionAction;
      if (oa != null)
      {
        string[] v = oa.GetOption();

        if (v == null || v.Length == 0)
        {

        }
        else
        {
          ArrayList l = new ArrayList(v);
        
          l.Remove(relfile);
          oa.SetOption(l.ToArray(typeof(string)) as string[]);
        }
      }

      if (data.pairings.ContainsKey(relfile))
      {
        data.pairings.Remove(relfile);
      }

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

    internal void BuildProject(object sender, EventArgs e)
    {
      System.Threading.ThreadPool.QueueUserWorkItem( new System.Threading.WaitCallback(BuildThreaded));
    }

    internal void NewFile(object sender, EventArgs e)
    {
      NewFileWizard wiz = new NewFileWizard();
	
      Hashtable lnames = new Hashtable();

      foreach (Type l in ActionTypes)
      {
        lnames.Add(l.Name, l);
      }

      ArrayList ll = new ArrayList(lnames.Keys);
      ll.Sort();

      foreach (string lname in ll)
      {
        wiz.prjtype.Items.Add(lnames[lname]);
      }

      RESTART:

        if (wiz.ShowDialog(ServiceHost.Window.MainForm) == DialogResult.OK)
        {
          Type t = wiz.prjtype.SelectedItem as Type;
          Action a = SuggestAction(t);
          string fn = wiz.name.Text;
          string path = wiz.loc.Text.Trim();

          if (path == string.Empty)
          {
            path = Environment.CurrentDirectory;
          }

          string fullpath = path + Path.DirectorySeparatorChar + fn;

          if (Path.GetExtension(fullpath) == string.Empty)
          {
            CustomAction ca = a as CustomAction;
            if (ca != null)
            {
              fullpath += ("." + ca.InputExtension[0]);
            }
          }

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

            AddFile(fullpath, a);

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

      int count = 0;

      ab.Append("All supported files|");

      foreach (Type act in ActionTypes)
      {
        string[] extss = Xacc.Build.InputExtensionAttribute.GetExtensions(act);

        if (extss.Length > 0)
        {
          if (extss[0] != "*")
          {
            ex.AppendFormat("*.{0}", extss[0]);
            ab.AppendFormat("*.{0};", extss[0]);
          }

          for(int i = 1; i < extss.Length; i++)
          {
            if (extss[i] != "*")
            {
              ex.AppendFormat(";*.{0}", extss[i]);
              ab.AppendFormat("*.{0};", extss[i]);
            }
          }

          if (ex.Length > 0)
          {
            count++;
            sb.AppendFormat("{0} ({1})|{1}|", NameAttribute.GetName(act), ex);
            ex.Length = 0;
          }
        }
      }
      
      ab.Length--;
      ab.Append("|");
      sb.Append("Text files (*.txt)|*.txt|");
      sb.Append("All files (*.*)|*.*");

      ofd.Filter = ab.ToString() + sb.ToString();

      if (ofd.ShowDialog() == DialogResult.OK)
      {
        foreach (string file in ofd.FileNames)
        {
          AddFile(file);
        }
      }
    }

    internal void RunProject(object sender, EventArgs e)
    {
      foreach (Action a in Actions)
      {
        ProcessAction pa = a as ProcessAction;
        if (pa != null)
        {
          Option o = pa.OutputOption;
          if (o != null)
          {
            string outfile = pa.GetOptionValue(o) as string;

            if (outfile == null || outfile == string.Empty)
            {
              MessageBox.Show(ServiceHost.Window.MainForm, "No output specified.\nPlease specify an output file in the project properties",
                "Error", 0, MessageBoxIcon.Error);
              return;
            }

            outfile = Path.Combine(RootDirectory, outfile);
            
            if (Path.GetExtension(outfile) == ".exe")
            {

              bool rebuild = false;

              if (File.Exists(outfile))
              {
                DateTime build = File.GetLastWriteTime(outfile);
                foreach (string file in Sources)
                {
                  if (File.Exists(file))
                  {
                    if (File.GetLastWriteTime(file) > build || ServiceHost.File.IsDirty(file))
                    {
                      rebuild = true;
                      break;
                    }
                  }
                }
              }
              else
              {
                rebuild = true;
              }

              if (rebuild && !Build())
              {
                MessageBox.Show(ServiceHost.Window.MainForm, string.Format("Build Failed: Unable to run: {0}",
                  outfile), "Error", 0, MessageBoxIcon.Error);
                return;
              }

              try
              {
                Process.Start(outfile);
              }
              catch (Exception ex)
              {
                MessageBox.Show(ServiceHost.Window.MainForm, string.Format("Error running: {0}\nError: {1}",
                  outfile, ex.GetBaseException().Message), "Error", 0, MessageBoxIcon.Error);
              }
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
