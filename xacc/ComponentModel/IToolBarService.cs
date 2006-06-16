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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services for managing toolbar
	/// </summary>
	public interface IToolBarService : IService
	{
    bool ToolBarVisible {get;set;}
    ToolStripContainer ToolBar { get;}
	}

	sealed class ToolBarService : ServiceBase, IToolBarService
	{
    readonly Dictionary<ToolStripMenuItem, int> map = new Dictionary<ToolStripMenuItem, int>();
    readonly List<ToolStrip> toplevel = new List<ToolStrip>();
    readonly ToolStripContainer toolbar = new ToolStripContainer();

    public ToolStripContainer ToolBar 
    {
      get { return toolbar; }
    }

		public ToolBarService()
		{
      ServiceHost.StateChanged += new EventHandler(ServiceHost_StateChanged);
      toolbar.Dock = DockStyle.Fill;

      ServiceHost.Window.MainForm.Controls.Add(toolbar);
      toolbar.ContentPanel.Controls.Add(ServiceHost.Window.Document as Control);
      //toolbar.TopToolStripPanel.Controls.Add(ServiceHost.Menu.MainMenu);
      ServiceHost.Window.MainForm.Controls.Add(ServiceHost.Menu.MainMenu);
    }

    public bool ToolBarVisible
    {
      get {return toolbar.TopToolStripPanelVisible;}
      set { toolbar.TopToolStripPanelVisible = value; }
    }

    internal void ValidateToolBarButtons()
    {
      foreach (ToolStrip ts in toplevel)
      {
        foreach (ToolStripItem tbb in ts.Items)
        {
          MenuItemAttribute mia = tbb.Tag as MenuItemAttribute;
          if (mia != null)
          {
            tbb.Enabled = ((mia.ctr.MenuState & mia.State) == mia.State);
          }
        }
      }
    }

    public bool Add(ToolStripMenuItem parent, MenuItemAttribute mia)
    {
      if (mia == null)
      {
        if (!map.ContainsKey(parent))
        {
          ToolStrip ts = new ToolStrip();
          ts.ImageList = ServiceHost.ImageListProvider.ImageList;

          ts.TabIndex = (map[parent] = toplevel.Count) + 1;
          toplevel.Add(ts);
          ts.Visible = false;

          toolbar.TopToolStripPanel.Controls.Add(ts);
        }
      }
      else
      {
        if (!map.ContainsKey(parent))
        {
          ToolStrip ts = new ToolStrip();
          ts.ImageList = ServiceHost.ImageListProvider.ImageList;

          ts.TabIndex = (map[parent] = toplevel.Count) + 1;
          toplevel.Add(ts);
          ts.Visible = false;

          toolbar.TopToolStripPanel.Controls.Add(ts);
        }

        ToolStrip sm = toplevel[map[parent]];
        ToolStripButton tbb = new ToolStripButton();
        tbb.Click += new EventHandler(ButtonDefaultHandler);
        tbb.ImageIndex = ServiceHost.ImageListProvider[mia.Image];
        tbb.Tag = mia;
        tbb.ToolTipText = mia.Text;
        sm.Items.Add(tbb);
        sm.Visible = true;
      }
      return true;
    }

		void ButtonDefaultHandler(object sender, EventArgs e)
		{
			MenuItemAttribute mia = (sender as ToolStripButton).Tag as MenuItemAttribute;
      if (mia != null)
      {
        mia.ctr.DefaultHandler(mia.mi, EventArgs.Empty);
      }
    }

    void ServiceHost_StateChanged(object sender, EventArgs e)
    {
      ValidateToolBarButtons();
    }
  }
}
