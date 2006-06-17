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
	}

  [Menu("Project")]
	sealed class ProjectManager : ServiceBase, IProjectManagerService
	{
		readonly ArrayList projects = new ArrayList();
		readonly Hashtable projtypes = new Hashtable();
    readonly IDockContent tp = Runtime.DockFactory.Content();
    readonly OutlineView outlineview = new OutlineView();
    readonly IDockContent to = Runtime.DockFactory.Content();

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

    protected override void Dispose(bool disposing)
    {
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

    [MenuItem("Add new project...", Index = 14, State = ApplicationState.Project)]
    void AddNewProject()
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

    [MenuItem("Add existing project...", Index = 15, State = ApplicationState.Project)]
    void AddExistingProject()
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.CheckFileExists = true;
      ofd.CheckPathExists = true;
      ofd.AddExtension = true;
      ofd.Filter = "Xacc Project files|*.xacc";
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
      Current.Build();
    }

    [MenuItem("Build All", Index = 21, State = ApplicationState.Project, Image = "Project.Build.png", AllowToolBar = true)]
    void BuildAll()
    {
      foreach (Project p in OpenProjects)
      {
        if (!p.Build())
        {
          return;
        }
      }
    }

    [MenuItem("Run", Index = 22, State = ApplicationState.Project, Image = "Project.Run.png", AllowToolBar = true)]
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

    [MenuItem("Build Order", Index = 1000, State = ApplicationState.Project)]
    void ShowBuildOrderDialog()
    {
      ProjectBuildOrderForm bof = new ProjectBuildOrderForm();
      if (DialogResult.OK == bof.ShowDialog(ServiceHost.Window.MainForm))
      {
        projects.Clear();
        projects.AddRange(bof.listBox1.Items);
      }
    }

    [MenuItem("Properties", Index = 1001, State = ApplicationState.Project, Image = "Project.Properties.png", AllowToolBar = true)]
    void Properties()
    {
      Current.ShowProps(null, EventArgs.Empty);
    }

		public ProjectManager()
		{
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

    [MenuItem("Create...", Index = 0, Image = "Project.New.png", AllowToolBar = true)]
		void Create()
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

    [MenuItem("Open...", Index = 1, Image = "Project.Open.png", AllowToolBar = true)]
		void Open()
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.AddExtension = true;
			ofd.Filter = "Xacc Project files|*.xacc;*.sln;*.proj;*.csproj";
			ofd.Multiselect = false;
			ofd.RestoreDirectory = true;
			if (DialogResult.OK == ofd.ShowDialog(ServiceHost.Window.MainForm))
			{
        CloseAll();
        Application.DoEvents();
				Open(ofd.FileName);
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
      proj.Location = rootdir + Path.DirectorySeparatorChar + name + ".xacc";
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
      if (current != null)
      {
        current.Save();
      }
      else if (projects.Count > 0)
      {
        OpenProjects[0].Save();
      }
      Close(OpenProjects);
    }

		public Project[] Open(string prjfile)
		{

      //bp.Save(prjfile + ".proj");

      prjfile = Path.GetFullPath(prjfile);

      if (!File.Exists(prjfile))
      {
        return null;
      }

      string ext = Path.GetExtension(prjfile);

      if (ext == ".sln" || ext == ".csproj")
      {
        BuildProject bp = new BuildProject();

        bp.Load(prjfile);

        return new Project[] { new MsBuildProject(bp) };
      }
      else
      {
        XmlSerializer ser = new XmlSerializer(Configuration.Projects.SerializerType, new Type[] { typeof(RegexOptions) });

        using (Stream s = File.OpenRead(prjfile))
        {
          Configuration.Projects pp = ser.Deserialize(s) as Configuration.Projects;

          if (pp != null)
          {
            foreach (Project prj in pp.projects)
            {
              if (projects.Count == 0)
              {
                prj.Location = prjfile;
              }
              else
              {
                prj.Location = OpenProjects[0].Location;
              }

              prj.RootDirectory = Path.GetDirectoryName(prj.Location);

              Environment.CurrentDirectory = Path.GetDirectoryName(prjfile);

              current = prj;

              if (prj.Startup)
              {
                if (startupproject == null)
                {
                  startupproject = prj;
                }
                else
                {
                  prj.Startup = false;
                }
              }

              prj.ProjectCreated();

              Add(prj);

              foreach (Action a in prj.Actions)
              {
                CustomAction ca = a as CustomAction;

                if (ca != null)
                {
                  if (ca.Input != null)
                  {
                    foreach (string filename in ca.Input)
                    {
                      prj.AddFile(filename, ca);
                    }
                  }
                  foreach (Type st in ca.ActionTypes)
                  {
                    OptionAction oa = ca.GetAction(st) as OptionAction;
                    string[] vals = oa.GetOption();
                    if (vals != null)
                    {
                      foreach (string v in vals)
                      {
                        prj.AddFile(v, oa);
                      }
                    }
                  }
                }
              }

              prj.OnOpened();

              if (Opened != null)
              {
                Opened(prj, EventArgs.Empty);
              }
            }

            ProjectTab.Show();

            return OpenProjects;
          }
          else
          {
            return null;
          }
        }
      }
		}

		public Project Create(Type prjtype, string name, string rootdir)
		{
			Project proj = Activator.CreateInstance(prjtype) as Project;
			proj.RootDirectory = rootdir;
			proj.Location = rootdir + Path.DirectorySeparatorChar + name + ".xacc";
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
        string file = t.SelectedNode.Tag as string;
        if (file != null)
        {
          ServiceHost.File.BringToFront( current.OpenFile(file));
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
