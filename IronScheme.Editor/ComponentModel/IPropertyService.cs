#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion



#region Includes
using System.Windows.Forms;
using IronScheme.Editor.Runtime;
#endregion

namespace IronScheme.Editor.ComponentModel
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

        //Grid.SelectedObject = ServiceHost.ToolBar.ToolBar;

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
