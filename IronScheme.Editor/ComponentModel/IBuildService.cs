#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using IronScheme.Editor.Controls;
#endregion

using Microsoft.Build.BuildEngine;

using BuildProject = Microsoft.Build.BuildEngine.Project;

using Microsoft.Build.Framework;
using IronScheme.Editor.Build;

using System.Threading;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Provides service for debugging
  /// </summary>
  [Name("Build service")]
  public interface IBuildService : IService
  {
    /// <summary>
    /// Gets or sets the logger verbosity.
    /// </summary>
    /// <value>The logger verbosity.</value>
    LoggerVerbosity LoggerVerbosity { get;set;}

    string Configuration { get; set; }
    string Current { get; }

    void BuildAll();
    void BuildCurrent();
    void BuildOrder();
    void CancelBuild();
    void CleanAll();
    void CleanCurrent();
    void RebuildAll();
    void RebuildCurrent();
  }

  [Menu("Build")]
  sealed class BuildService : ServiceBase, IBuildService
  {
    internal BuildProject solution;
    readonly Engine buildengine = Engine.GlobalEngine;

    LoggerVerbosity verbosity = LoggerVerbosity.Minimal;

    public LoggerVerbosity LoggerVerbosity
    {
      get { return verbosity; }
      set { verbosity = value; }
    }

    static Thread buildthread;

    IDictionary SolutionProperties
    {
      get
      {
        Hashtable props = new Hashtable();
        if (solution != null)
        {
          foreach (BuildProperty bp in solution.EvaluatedProperties)
          {
            if (!bp.IsImported)
            {
              props.Add(bp.Name, bp.Value);
            }
          }
        }
        return props;
      }
    }

    public string Current
    {
      get
      {
        Build.Project p = ServiceHost.Project.Current;
        if (p == null)
        {
          return null;
        }
        return p.ProjectName;
      }
    }

    class ConfigurationConvertor : TypeConverter
    {
      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
        return true;
      }

      public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
        return new StandardValuesCollection(new string[] { "Debug", "Release" });
      }
    }



    [MenuItem("Configuration", Index = 1, Converter = typeof(ConfigurationConvertor), State = ApplicationState.Project)]
    public string Configuration
    {
      get { return solution == null ? ServiceHost.Project.Current == null ? "Debug" : ServiceHost.Project.Current.Configuration : solution.GetEvaluatedProperty("Configuration"); }
      set
      {
        if (solution == null)
        {
          ServiceHost.Project.Current.Configuration = value;
        }
        else
        {
          solution.SetProperty("Configuration", value);
        }
      }
    }
    
    BuildLogger bl;

    void BuildStarted(ThreadStart t)
    {
      if (buildthread == null)
      {
        buildthread = new Thread(t);
        buildthread.IsBackground = true;
        (ServiceHost.File as FileManager).SaveDirtyFiles();
        ServiceHost.State |= ApplicationState.Build;
        ConsoleLogger l = new ConsoleLogger();
        l.Verbosity = verbosity;
        bl = new BuildLogger();
        buildengine.RegisterLogger(l);
        buildengine.RegisterLogger(bl);

        buildthread.SetApartmentState(ApartmentState.STA);

        buildthread.Start();
      }


    }

    readonly Hashtable outputs = new Hashtable();

    internal void BuildInternal(BuildProject project, params string[] targets)
    {
      if (project != null)
      {
        BuildProperty platform = project.GlobalProperties["Platform"];
        if (platform == null)
        {
          project.GlobalProperties["Platform"] = platform = new BuildProperty("Platform", "AnyCPU");
        }
        if (string.IsNullOrEmpty( platform.Value))
        {
          platform.Value = "AnyCPU";
        }

        project.ParentEngine.GlobalProperties["SolutionDir"] = solution.GlobalProperties["SolutionDir"];
        
        BuildStarted(delegate()
        {
          outputs.Clear();
          try
          {
            bool res = project.Build(targets, outputs);
          }
          catch
          {
            if (!bl.cancel)
            {
              throw;
            }
          }
          InvokeBuildCompleted();
        });
      }
    }

    delegate void VOIDVOID();

    void InvokeBuildCompleted()
    {
      Invoke(new VOIDVOID(BuildCompleted), null);
    }

    void BuildCompleted()
    {
      if (buildthread != null)
      {
        buildthread = null;
      }
      buildengine.UnregisterAllLoggers();
      ServiceHost.State &= ~ApplicationState.Build;
    }

    [MenuItem("Build All", Index = 11, State = ApplicationState.Project, Image = "Project.Build.png")]
    public void BuildAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject);
    }

    [MenuItem("Rebuild All", Index = 12, State = ApplicationState.Project)]
    public void RebuildAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject, "Rebuild");
    }

    [MenuItem("Clean All", Index = 13, State = ApplicationState.Project)]
    public void CleanAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject, "Clean");
    }

    [MenuItem("Build {Current}", Index = 21, State = ApplicationState.Project, Image = "Project.Build.png")]
    public void BuildCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject);
    }

    [MenuItem("Rebuild {Current}", Index = 22, State = ApplicationState.Project)]
    public void RebuildCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject, "Rebuild");
    }

    [MenuItem("Clean {Current}", Index = 23, State = ApplicationState.Project)]
    public void CleanCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject, "Clean");
    }

    [MenuItem("Cancel Build", Index = 900, State = ApplicationState.Project | ApplicationState.Build, Image = "Build.Cancel.png")]
    public void CancelBuild()
    {
      if (buildthread != null)
      {
        bl.cancel = true;
        Console.WriteLine("User cancelled build");
        ServiceHost.Error.OutputErrors(ServiceHost.Project,
          new ActionResult(ActionResultType.Warning, 0, 0, "User cancelled build", null, null));
        BuildCompleted();
      }
    }

    [MenuItem("Build Order", Index = 1000, State = ApplicationState.Project)]
    public void BuildOrder()
    {
      ProjectManager pm = ServiceHost.Project as ProjectManager;
      ProjectBuildOrderForm bof = new ProjectBuildOrderForm();
      if (DialogResult.OK == bof.ShowDialog(ServiceHost.Window.MainForm))
      {
        pm.projects.Clear();
        pm.projects.AddRange(bof.listBox1.Items);
      }
    }
  }
}
