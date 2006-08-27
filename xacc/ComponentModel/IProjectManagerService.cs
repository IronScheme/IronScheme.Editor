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
using Xacc.Build;
using Xacc.Configuration;
using System.Xml.Schema;

using Xacc.Runtime;

using Microsoft.Build.BuildEngine;
using BuildProject = Microsoft.Build.BuildEngine.Project;
using Project = Xacc.Build.Project;

using Microsoft.Build.Framework;

using SR = System.Resources;
#endregion

namespace Xacc.ComponentModel
{
  /// <summary>
  /// Provides services for managing projects
  /// </summary>
	[Name("Project manager")]
	public interface IProjectManagerService : IService
	{
    /// <summary>
    /// Gets an array of currently open projects
    /// </summary>
		Project[]			OpenProjects					{get;}

    /// <summary>
    /// Gets the currently selected project
    /// </summary>
		Project			  Current								{get;set;}

    /// <summary>
    /// Gets the startup project
    /// </summary>
    Project			  StartupProject				{get;}

    /// <summary>
    /// Opens a project from a file
    /// </summary>
    /// <param name="projfilename">the filename</param>
    /// <returns>the project instance</returns>
		Project[]		  Open									(string projfilename);

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="prjtype">the project type</param>
    /// <param name="name">the name</param>
    /// <param name="rootdir">the root directory</param>
    /// <returns>the newly created project</returns>
		Project 			Create								(Type prjtype, string name, string rootdir);

    /// <summary>
    /// Adds a new project
    /// </summary>
    /// <param name="prjtype">the project type</param>
    /// <param name="name">the name</param>
    /// <param name="rootdir">the root directory</param>
    /// <returns>the newly created project</returns>
    Project 			AddProject						(Type prjtype, string name, string rootdir);

    /// <summary>
    /// Removes a project
    /// </summary>
    /// <param name="proj">the project to remove</param>
    void 			    RemoveProject					(Project proj);

    /// <summary>
    /// Close a project
    /// </summary>
    /// <param name="proj">the project to close</param>
		void					Close									(Project[] proj);

    /// <summary>
    /// Closes all projects
    /// </summary>
    void					CloseAll();


    /// <summary>
    /// Registers a project type
    /// </summary>
    /// <param name="projecttype">the project type</param>
		void					Register							(Type projecttype);

    /// <summary>
    /// Gets the project TabPage
    /// </summary>
    IDockContent   ProjectTab            {get;}

    /// <summary>
    /// Gets the outline TabPage
    /// </summary>
    IDockContent   OutlineTab            {get;}

    /// <summary>
    /// Gets the outline view
    /// </summary>
    System.Windows.Forms.TreeView      OutlineView           {get;}

    /// <summary>
    /// Fires when a project is opened
    /// </summary>
    event EventHandler Opened;

    /// <summary>
    /// Fires when a project is closed
    /// </summary>
    event EventHandler Closed;



    /// <summary>
    /// Gets the recent projects.
    /// </summary>
    /// <value>The recent projects.</value>
    string[] RecentProjects { get;}
	}

  class BuildLogger : ILogger
  {
    public bool cancel = false;
    public void Initialize(IEventSource eventSource)
    {
      eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
      eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
      eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
      eventSource.AnyEventRaised += new AnyEventHandler(eventSource_AnyEventRaised);
    }

    void eventSource_AnyEventRaised(object sender, BuildEventArgs e)
    {
      if (cancel)
      {

        System.Threading.Thread.CurrentThread.Abort();
      }
    }

    void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
    {
      ServiceHost.Error.OutputErrors(ServiceHost.Project, 
        new ActionResult(e.Succeeded ? ActionResultType.Info : ActionResultType.Error, 0, 0, e.Message, e.ProjectFile, null));
    }

    void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
    {
      ServiceHost.Error.OutputErrors(ServiceHost.Project, 
        new ActionResult(ActionResultType.Warning, e.LineNumber, e.ColumnNumber, e.Message, e.File, e.Code));
    }

    void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
    {
      ServiceHost.Error.OutputErrors(ServiceHost.Project, 
        new ActionResult(ActionResultType.Error, e.LineNumber, e.ColumnNumber, e.Message, e.File, e.Code));
    }

    string param;

    public string Parameters
    {
      get{return param;}
      set{param = value;}
    }

    public void Shutdown()
    {
    }

    LoggerVerbosity verb = LoggerVerbosity.Normal;

    public LoggerVerbosity Verbosity
    {
      get{return verb;}
      set{verb = value;}
    }
  }


  [Menu("Project")]
	sealed class ProjectManager : ServiceBase, IProjectManagerService
	{
		internal readonly ArrayList projects = new ArrayList();
		readonly Hashtable projtypes = new Hashtable();
    readonly IDockContent tp = Runtime.DockFactory.Content();
    readonly OutlineView outlineview = new OutlineView();
    readonly IDockContent to = Runtime.DockFactory.Content();

    ArrayList recentfiles = new ArrayList();

    Project current;

    readonly Engine buildengine = Engine.GlobalEngine;

    public event EventHandler Opened;
    public event EventHandler Closed;
		
    public System.Windows.Forms.TreeView OutlineView
    {
      get {return outlineview;}
    }

    public IDockContent ProjectTab
    {
      get {return tp;}
    }

    public IDockContent OutlineTab
    {
      get {return to;}
    }


		public void Register(Type projtype)
		{
			if (!projtypes.ContainsKey(projtype))
			{
        Trace.WriteLine("Registering project type: {0}", NameAttribute.GetName(projtype));
				projtypes.Add(projtype, null);
			}
		}

    public string[] RecentProjects
    {
      get
      {
        recentfiles.Sort();

        if (recentfiles.Count > 10)
        {
          recentfiles.RemoveRange(10, recentfiles.Count - 10);
        }
        string[] rec = new string[recentfiles.Count];
        for (int i = 0; i < recentfiles.Count; i++)
        {
          rec[i] = (recentfiles[i] as MRUFile).filename;
        }
        return rec;
      }
    }

    protected override void Dispose(bool disposing)
    {
			if (disposing)
			{
				TextWriter writer = File.CreateText( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + 
          "recentprojects.ini");

				//reverse the list so it gets loaded in reverse
				string[] rc = RecentProjects;
				Array.Reverse(rc);

				foreach (string file in rc)
				{
					writer.WriteLine(file);
				}

				writer.Flush();
				writer.Close();
			}

      CloseAll();
      base.Dispose (disposing);
    }

    public Project StartupProject 
    {
      get 
      {
        foreach (Project p in OpenProjects)
        {
          if (p.Startup)
          {
            Debug.Assert(p == startupproject);
            return p;
          }
        }
        return null;
      }
    }


    [MenuItem("Add new file...", Index = 10, State = ApplicationState.Project, Image = "Project.Add.png", AllowToolBar = true)]
    void AddNewFile()
    {
      Current.NewFile(null, EventArgs.Empty);
    }

    [MenuItem("Add existing file...", Index = 11, State = ApplicationState.Project, Image = "File.Open.png", AllowToolBar = true)]
    void AddExistingFile()
    {
      Current.ExistingFile(null, EventArgs.Empty);
    }

    internal void AddNewProject()
    {
      Wizard wiz = new Wizard();

      ArrayList keys = new ArrayList(projtypes.Keys);
      keys.Sort(TypeComparer.Default);

      foreach (Type prj in keys)
      {
        wiz.prjtype.Items.Add(prj);
      }

      if (wiz.ShowDialog(ServiceHost.Window.MainForm) == DialogResult.OK)
      {
        Project prj = wiz.Tag as Project;

        Add(prj);
        prj.ProjectCreated();
        prj.OnOpened();

        if (Opened != null)
        {
          Opened(prj, EventArgs.Empty);
        }
      }
    }

    //[MenuItem("Add existing project...", Index = 15, State = ApplicationState.Project)]
    void AddExistingProject()
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.CheckFileExists = true;
      ofd.CheckPathExists = true;
      ofd.AddExtension = true;
      ofd.Filter = "MSBuild Project files|*.*proj";
      ofd.Multiselect = false;
      ofd.RestoreDirectory = true;
      if (DialogResult.OK == ofd.ShowDialog(ServiceHost.Window.MainForm))
      {
        Application.DoEvents();
        Open(ofd.FileName);
      }
    }

    [MenuItem("Remove project", Index = 16, State = ApplicationState.Project)]
    void RemoveProject()
    {
      Close(new Project[] {Current });
    }
    
    [MenuItem("Set as Startup", Index = 17, State = ApplicationState.Project)]
    void SetAsStartup()
    {
      foreach (Project p in OpenProjects)
      {
        p.Startup = false;
      }
      Current.Startup = true;
      startupproject = Current;
    }

    [MenuItem("Build", Index = 20, State = ApplicationState.Project, Image = "Project.Build.png", AllowToolBar = true)]
    void Build()
    {
      // MSBuild complains about MTA... weird shit
      //System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
      //{
      (ServiceHost.File as FileManager).SaveDirtyFiles();

        ConsoleLogger l = new ConsoleLogger();
        l.Verbosity = ServiceHost.Build.LoggerVerbosity;
        BuildLogger bl = new BuildLogger();
        buildengine.RegisterLogger(l);
        buildengine.RegisterLogger(bl);
        bool res = Current.Build();
        buildengine.UnregisterAllLoggers();
      //});
    }

    [MenuItem("Rebuild", Index = 21, State = ApplicationState.Project, Image = "Project.Build.png", AllowToolBar = true)]
    void Rebuild()
    {
      // MSBuild complains about MTA... weird shit
      //System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
      //{
      (ServiceHost.File as FileManager).SaveDirtyFiles();
      ConsoleLogger l = new ConsoleLogger();
      l.Verbosity = ServiceHost.Build.LoggerVerbosity;
      BuildLogger bl = new BuildLogger();
      buildengine.RegisterLogger(l);
      buildengine.RegisterLogger(bl);
      bool res = Current.Rebuild();
      buildengine.UnregisterAllLoggers();
      //});
    }

    [MenuItem("Clean", Index = 22, State = ApplicationState.Project, Image = "Project.Build.png", AllowToolBar = true)]
    void Clean()
    {
      // MSBuild complains about MTA... weird shit
      //System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
      //{
      (ServiceHost.File as FileManager).SaveDirtyFiles();
      ConsoleLogger l = new ConsoleLogger();
      l.Verbosity = ServiceHost.Build.LoggerVerbosity;
      BuildLogger bl = new BuildLogger();
      buildengine.RegisterLogger(l);
      buildengine.RegisterLogger(bl);
      bool res = Current.Clean();
      buildengine.UnregisterAllLoggers();
      //});
    }



    [MenuItem("Run", Index = 25, State = ApplicationState.Project, Image = "Project.Run.png", AllowToolBar = true)]
    void Run()
    {
      if (StartupProject != null)
      {
        StartupProject.RunProject(null, EventArgs.Empty);
      }
      else
      {
        MessageBox.Show(ServiceHost.Window.MainForm, "No startup project has been selected", "Error", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }

    [MenuItem("Properties", Index = 1001, State = ApplicationState.Project, Image = "Project.Properties.png", AllowToolBar = true)]
    void Properties()
    {
      Current.ShowProps(null, EventArgs.Empty);
    }

		public ProjectManager()
		{
      string recfn = Application.StartupPath + Path.DirectorySeparatorChar + "recentprojects.ini";
      if (File.Exists(recfn))
      {
        TextReader reader = File.OpenText(recfn);
        string rf;
        //fast CPU hack ;p
        int priority = 0;

        while ((rf = reader.ReadLine()) != null)
        {
          if (File.Exists(rf))
          {
            recentfiles.Add(new MRUFile(rf, priority++));
          }
        }
        reader.Close();
      }

      if (SettingsService.idemode)
      {
        tp.Text = "Project Explorer";
        tp.Icon = ServiceHost.ImageListProvider.GetIcon("Project.Type.png");
        to.Text = "Outline";
        to.Icon = ServiceHost.ImageListProvider.GetIcon("CodeValueType.png");
        tp.Controls.Add(outlineview);
        to.Controls.Add(ServiceHost.CodeModel.Tree);

        IWindowService ws = ServiceHost.Window;
        tp.Show(ws.Document, DockState.DockRightAutoHide);
        to.Show(ws.Document, DockState.DockRightAutoHide);
        to.Hide();
        tp.Hide();
        to.HideOnClose = true;
        tp.HideOnClose = true;


        OutlineView.DoubleClick +=new EventHandler(Tree_DoubleClick);

        buildengine.BinPath = ServiceHost.Discovery.NetRuntimeRoot;
      }

      Opened +=new EventHandler(ProjectManagerEvent);
      Closed +=new EventHandler(ProjectManagerEvent);
		}

 
    bool ProjectNameExists(string name)
    {
      foreach (Project p in projects)
      {
        if (p.ProjectName == name)
        {
          return true;
        }
      }
      return false;
    }

		public void Add(Project prj)
		{
      if (ProjectNameExists(prj.ProjectName))
      {
        prj.ProjectName += "_new";
      }
			projects.Add(prj);
      if (current != null)
      {
        prj.Location = current.Location;
      }
		}

    public void Remove(Project prj)
    {
      projects.Remove(prj);
      if (prj == current)
      {
        current = null;
      }
      if (prj == startupproject)
      {
        startupproject = null;
      }
    }

		public Project Current
		{
      get { return current; }
      set { current = value;}
		}

		internal class TypeComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return (x as Type).Name.CompareTo((y as Type).Name);
			}

			public readonly static IComparer Default = new TypeComparer();
		}

    
		internal void Create()
		{
			//show wizard thingy
			Wizard wiz = new Wizard();

			ArrayList keys = new ArrayList(projtypes.Keys);
			keys.Sort(TypeComparer.Default);

			foreach (Type prj in keys)
			{
				wiz.prjtype.Items.Add(prj);
			}

			if (wiz.ShowDialog(ServiceHost.Window.MainForm) == DialogResult.OK)
			{
				Project p = wiz.Tag as Project;
        p.Startup = true;
        Add(p);
        CloseAll();

				Open(p.Location);
			}
		}



		public Project[] OpenProjects
		{
			get {return projects.ToArray(typeof(Project)) as Project[];}
		}


    public Project AddProject(Type prjtype, string name, string rootdir)
    {
      Project proj = Activator.CreateInstance(prjtype) as Project;
      proj.RootDirectory = rootdir;
      proj.Location = rootdir + Path.DirectorySeparatorChar + name + ".proj";
      proj.ProjectName = name;
			
      proj.ProjectCreated();

      Add(proj);

      return proj;
    }

    Project startupproject;

    public void RemoveProject(Project proj)
    {
      Remove(proj);
    }

    [MenuItem("Close all", Index = 25, State = ApplicationState.Project)]
    public void	CloseAll()
    {
      //if (solution != null)
      //{
      //  //solution.Save(
      //}
      foreach (Project p in OpenProjects)
      {
        p.Save();
      }
      Close(OpenProjects);
    }

		public Project[] Open(string prjfile)
		{
      prjfile = Path.GetFullPath(prjfile);

      if (!File.Exists(prjfile))
      {
        return null;
      }

      Environment.CurrentDirectory = Path.GetDirectoryName(prjfile);

      foreach (MRUFile mru in recentfiles)
      {
        if (mru.filename == prjfile)
        {
          mru.Update();
          goto DONE;
        }
      }
      recentfiles.Add(new MRUFile(prjfile));

    DONE:

      string ext = Path.GetExtension(prjfile);

      if (ext == ".xaccproj")
      {
        BuildService bm = ServiceHost.Build as BuildService;
        bm.solution = new Microsoft.Build.BuildEngine.Project();
        bm.solution.Load(prjfile);


        ArrayList projects = new ArrayList();

        foreach (BuildItem prj in bm.solution.GetEvaluatedItemsByName("BuildProject"))
        {
          Environment.CurrentDirectory = Path.GetDirectoryName(prjfile);

          Project bp = new Project();

          bp.Load(prj.Include);

          bp.SolutionDir = Path.GetDirectoryName(prjfile) + Path.DirectorySeparatorChar;
          bp.ProjectCreated();
          Add(bp);
          bp.OnOpened();

          projects.Add(bp);

          if (Opened != null)
          {
            Opened(bp, EventArgs.Empty);
          }
        }

        ProjectTab.Show();
        return projects.ToArray(typeof(Project)) as Project[];
      }
      else if (ext.EndsWith("proj"))
      {
        Project bp = new Project();

        bp.Load(prjfile);
        bp.ProjectCreated();
        Add(bp);
        bp.OnOpened();

        if (Opened != null)
        {
          Opened(bp, EventArgs.Empty);
        }
        ProjectTab.Show();

        return new Project[] { bp };
      }
      else if (ext == ".sln")
      {
        BuildService bm = ServiceHost.Build as BuildService;
        bm.solution = new Microsoft.Build.BuildEngine.Project();

        bm.solution.SetProperty("SolutionDir", Path.GetDirectoryName(prjfile));

        using (TextReader r = File.OpenText(prjfile))
        {
          string all = r.ReadToEnd();

          foreach (Match m in SLNPARSE.Matches(all))
          {
            string name = m.Groups["name"].Value;
            if (name == "Solution Items")
            {
              continue;
            }
            string location = m.Groups["location"].Value;

            bm.solution.AddNewItem("BuildProject", location);
          }
        }

        bm.solution.AddNewImport(Path.Combine(Application.StartupPath, "xacc.imports"), "");

        bm.solution.Save(Path.ChangeExtension(prjfile, ".xaccproj"));

        ArrayList projects = new ArrayList();

        foreach (BuildItem prj in bm.solution.GetEvaluatedItemsByName("BuildProject"))
        {
          Environment.CurrentDirectory = Path.GetDirectoryName(prjfile);

          Project bp = new Project();

          bp.Load(prj.Include);
          bp.ProjectCreated();

          bp.SolutionDir = Path.GetDirectoryName(prjfile) + Path.DirectorySeparatorChar;

          Add(bp);
          bp.OnOpened();

          projects.Add(bp);

          if (Opened != null)
          {
            Opened(bp, EventArgs.Empty);
          }
        }
        ProjectTab.Show();

        return projects.ToArray(typeof(Project)) as Project[];
      }
      else
      {
        return null;
      }
		}

    static Regex SLNPARSE = new Regex(@"Project\([^\)]+\)\s=\s""(?<name>[^""]+)"",\s""(?<location>[^""]+)"",\s""(?<guid>[^""]+)""",
      RegexOptions.Compiled);

    //BuildProject solution;

		public Project Create(Type prjtype, string name, string rootdir)
		{
			Project proj = new Project();
			proj.RootDirectory = rootdir;
			proj.Location = rootdir + Path.DirectorySeparatorChar + name + ".proj";
			proj.ProjectName = name;
			
      proj.ProjectCreated();

			return proj;
		}

    public void Close(Project[] projs)
    {
      ServiceHost.Error.ClearErrors(null);

      foreach (Project proj in projs)
      {
        projects.Remove(proj.ProjectName);
        
        proj.Close();
        Remove(proj);

        if (proj.Startup && startupproject == null)
        {
          startupproject = null;
        }
 
        if (Closed != null)
        {
          Closed(proj, EventArgs.Empty);
        }

        try
        {
          outlineview.Nodes.Remove(proj.RootNode);
        }
        catch (ObjectDisposedException)
        {
          //silly docking thing...
        }
      }
    }

    void Tree_DoubleClick(object sender, EventArgs e)
    {
      System.Windows.Forms.TreeView t = sender as System.Windows.Forms.TreeView;
      if (t.SelectedNode != null)
      {
        BuildItem file = t.SelectedNode.Tag as BuildItem;
        if (file != null)
        {
          ServiceHost.File.BringToFront( current.OpenFile(Path.Combine(current.RootDirectory, file.Include)));
          return;
        }
      }
    }

    void ProjectManagerEvent(object sender, EventArgs e)
    {
      if (projects.Count == 0)
      {
        current = null;
        startupproject = null;
        ServiceHost.State &= ~ApplicationState.Project;
      }
      else
      {
        ServiceHost.State |= ApplicationState.Project;
      }
    }
  }
}
