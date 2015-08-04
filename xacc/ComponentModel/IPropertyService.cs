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
using Xacc.Build;
using Xacc.Controls;

using SR = System.Resources;
using Xacc.Runtime;
#endregion

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services for managing toolbar
	/// </summary>
	public interface IPropertyService : IService
	{
    PropertyGrid Grid { get;}
	}

	sealed class PropertyService : ServiceBase, IPropertyService
	{
    Controls.Properties props = new Controls.Properties();
    internal IDockContent tbp;

    public PropertyService()
		{
      if (SettingsService.idemode)
      {
        tbp = Runtime.DockFactory.Content();
        tbp.Text = "Properties";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        tbp.Controls.Add(props);
        tbp.Show(ServiceHost.Window.Document, DockState.DockRightAutoHide);
        props.Tag = tbp;
        tbp.Hide();
        tbp.HideOnClose = true;

        Grid.SelectedObject = ServiceHost.ToolBar.ToolBar;

        props.propertyGrid1.PropertyValueChanged += new PropertyValueChangedEventHandler(propertyGrid1_PropertyValueChanged);
      }
    }

    void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
      ISelectObject so = ServiceHost.File.CurrentDocument.ActiveView as ISelectObject;
      if (so != null)
      {
        (so as Control).Refresh();
      }
    }

    #region IPropertyService Members

    public PropertyGrid Grid
    {
      get { return props.propertyGrid1; }
    }

    #endregion
  }
}
