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

    class ViewConverter : System.ComponentModel.TypeConverter
    {
      public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
      {
        return true;
      }

      public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
      {
        Document c = ServiceHost.File.CurrentDocument as Document;
        ArrayList vals = new ArrayList();
        if (c != null && c.Views != null)
        {
          foreach (IDocument v in c.Views)
          {
            NameAttribute na = Attribute.GetCustomAttribute(v.GetType(), typeof(NameAttribute)) as NameAttribute;
            if (na != null)
            {
              vals.Add(na.Name);
            }
            else
            {
              vals.Add( c.GetType().Name);
            }
          }
        }
        return new StandardValuesCollection(vals);
      }
    }

    [MenuItem("Switch View", Index = 5, Converter = typeof(ViewConverter), State = ApplicationState.File)]
    public string CurrentView
    {
      get
      {
        Control c = ServiceHost.File.CurrentControl;
        if (c != null)
        {
          NameAttribute na = Attribute.GetCustomAttribute(c.GetType(), typeof(NameAttribute)) as NameAttribute;
          if (na != null)
          {
            return na.Name;
          }
          return c.GetType().Name;
        }
        return null;
      }
      set
      {
        Document c = ServiceHost.File.CurrentDocument;
        if (c != null && c.Views != null && c.Views.Length > 1)
        {
          foreach (Control v in c.Views)
          {
            NameAttribute na = Attribute.GetCustomAttribute(v.GetType(), typeof(NameAttribute)) as NameAttribute;
            if ((na != null && na.Name == value) || (v.GetType().Name == value))
            {
              IDockContent dc = c.ActiveView.Tag as IDockContent;
              dc.Controls.Remove(c.ActiveView as Control);
              v.Dock = DockStyle.Fill;
              v.Tag = dc;
              dc.Controls.Add(v);
              c.SwitchView(v as IDocument);

#warning BUG: fix activation of control some how, been issue for too long
              return;
            }
          }
        }
      }
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

    [MenuItem("File Explorer", Index = 11, Image = "Project.Type.png")]
    bool ShowFileExplorer
    {
      get
      {
        IDockContent dc = ServiceHost.File.FileTab;
        return dc.DockState != DockState.Hidden;
      }
      set
      {
        if (!ShowProjectExplorer)
        {
          ServiceHost.File.FileTab.Activate();
        }
        else
        {
          ServiceHost.File.FileTab.Hide();
        }
      }
    }

    [MenuItem("Outline", Index = 12, Image="CodeValueType.png")]
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
