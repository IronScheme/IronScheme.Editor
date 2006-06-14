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
using System.Windows.Forms;
using Xacc.Runtime;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services for managing views
	/// </summary>
	public interface IViewService : IService
	{
    /// <summary>
    /// Gets the outline panel
    /// </summary>
    Panel OutlinePanel {get;}

    /// <summary>
    /// Whether to show toolbar
    /// </summary>
    bool ShowToolbar {get;set;}
	}

  [Menu("View")]
  sealed class ViewService : ServiceBase, IViewService
  {
    readonly Panel outlinepanel = new Panel();

    public Panel OutlinePanel
    {
      get {return outlinepanel;}
    }
    
    [MenuItem("Toolbar", Index = 0)]
    public bool ShowToolbar
    {
      get { return ServiceHost.ToolBar.ToolBarVisible;}
      set { ServiceHost.ToolBar.ToolBarVisible = value;}
    }

    [MenuItem("Project Explorer", Index = 10, Image="Project.Type.png")]
    bool ShowProjectExplorer
    {
      get 
      { 
        IDockContent dc = ServiceHost.Project.ProjectTab;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ShowProjectExplorer)
        {
          ServiceHost.Project.ProjectTab.Activate();
        }
        else
        {
          ServiceHost.Project.ProjectTab.Hide();
        }
      }
    }

    [MenuItem("Outline", Index = 11, Image="CodeValueType.png")]
    bool ShowOutline
    {
      get 
      { 
        IDockContent dc = ServiceHost.Project.OutlineTab;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ShowOutline)
        {
          ServiceHost.Project.OutlineTab.Activate();
        }
        else
        {
          ServiceHost.Project.OutlineTab.Hide();
        }
      }
    }

    [MenuItem("Results", Index = 20, Image="Help.Info.png")]
    bool ShowResults
    {
      get 
      { 
        IDockContent dc = (ServiceHost.Error as ErrorService).tbp;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ShowResults)
        {
          (ServiceHost.Error as ErrorService).tbp.Activate();
        }
        else
        {
          (ServiceHost.Error as ErrorService).tbp.Hide();
        }
      }
    }

    [MenuItem("Output", Index = 21, Image="console.png")]
    bool ShowConsole
    {
      get 
      { 
        IDockContent dc = (ServiceHost.Console as StandardConsole).tbp;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ShowConsole)
        {
          (ServiceHost.Console as StandardConsole).tbp.Activate();
        }
        else
        {
          (ServiceHost.Console as StandardConsole).tbp.Hide();
        }
      }
    }

    [MenuItem("Command window", Index = 22, Image="Project.Run.png")]
    bool ShowCommand
    {
      get 
      { 
        IDockContent dc = (ServiceHost.Scripting as ScriptingService).tbp;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ShowCommand)
        {
          (ServiceHost.Scripting as ScriptingService).tbp.Activate();
        }
        else
        {
          (ServiceHost.Scripting as ScriptingService).tbp.Hide();
        }
      }
    }
  }
}
