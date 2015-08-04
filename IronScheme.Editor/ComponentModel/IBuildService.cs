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
using System.Drawing.Text;
using Xacc.Collections;
using Xacc.Runtime;

using SR = System.Resources;
#endregion

using Microsoft.Build.BuildEngine;

using BuildProject = Microsoft.Build.BuildEngine.Project;

using Microsoft.Build.Framework;

using System.Runtime.InteropServices;
using Xacc.Build;
 
using System.Threading;

namespace Xacc.ComponentModel
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

    string Current
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
    string Configuration
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
    void BuildAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject);
    }

    [MenuItem("Rebuild All", Index = 12, State = ApplicationState.Project)]
    void RebuildAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject, "Rebuild");
    }

    [MenuItem("Clean All", Index = 13, State = ApplicationState.Project)]
    void CleanAll()
    {
      BuildInternal(solution ?? ServiceHost.Project.Current.MSBuildProject, "Clean");
    }

    [MenuItem("Build {Current}", Index = 21, State = ApplicationState.Project, Image = "Project.Build.png")]
    void BuildCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject);
    }

    [MenuItem("Rebuild {Current}", Index = 22, State = ApplicationState.Project)]
    void RebuildCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject, "Rebuild");
    }

    [MenuItem("Clean {Current}", Index = 23, State = ApplicationState.Project)]
    void CleanCurrent()
    {
      BuildInternal(ServiceHost.Project.Current.MSBuildProject, "Clean");
    }

    [MenuItem("Cancel Build", Index = 900, State = ApplicationState.Project | ApplicationState.Build, Image = "Build.Cancel.png")]
    void CancelBuild()
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
    void BuildOrder()
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
