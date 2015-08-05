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
using System.Drawing;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Provides services for managing toolbar
  /// </summary>
  public interface IToolBarService : IService
	{
    /// <summary>
    /// Gets or sets a value indicating whether [tool bar visible].
    /// </summary>
    /// <value><c>true</c> if [tool bar visible]; otherwise, <c>false</c>.</value>
    bool ToolBarVisible {get;set;}

    /// <summary>
    /// Gets the tool bar.
    /// </summary>
    /// <value>The tool bar.</value>
    Control ToolBar { get;}

  }

  sealed class ToolBarService : ServiceBase, IToolBarService
  {
    readonly Dictionary<ToolStripMenuItem, int> map = new Dictionary<ToolStripMenuItem, int>();
    readonly List<ToolStrip> toplevel = new List<ToolStrip>();
    readonly Panel toolbar = new Panel();

    public Control ToolBar
    {
      get { return toolbar; }
    }

    class ColorTable : ProfessionalColorTable
    {
      static Color menubg = Color.FromArgb(246, 246, 246);
      static Color topbg = Color.FromArgb(240, 240, 240);
      static Color menubdr = Color.FromArgb(204, 206, 219);
      static Color menusep = Color.FromArgb(224, 227, 230);

      public override Color MenuItemPressedGradientBegin
      {
        get
        {
          return menubg;
        }
      }

      public override Color MenuItemPressedGradientMiddle
      {
        get
        {
          return menubg;
        }
      }

      public override Color MenuItemPressedGradientEnd
      {
        get
        {
          return menubg;
        }
      }

      public override Color StatusStripGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color StatusStripGradientEnd
      {
        get
        {
          return topbg;
        }
      }

      public override Color OverflowButtonGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color OverflowButtonGradientMiddle
      {
        get
        {
          return topbg;
        }
      }

      public override Color OverflowButtonGradientEnd
      {
        get
        {
          return topbg;
        }
      }

      public override Color RaftingContainerGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color RaftingContainerGradientEnd
      {
        get
        {
          return topbg;
        }
      }

      public override Color ToolStripPanelGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color ToolStripPanelGradientEnd
      {
        get
        {
          return topbg;
        }
      }


      public override Color ToolStripDropDownBackground
      {
        get
        {
          return menubg;
        }
      }

      public override Color MenuItemBorder
      {
        get
        {
          return Color.Transparent;
        }
      }

      public override Color MenuStripGradientEnd
      {
        get
        {
          return topbg;
        }
      }

      public override Color MenuStripGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color MenuBorder
      {
        get
        {
          return menubdr;
        }
      }

      public override Color ImageMarginGradientBegin
      {
        get
        {
          return Color.Transparent;
        }
      }

      public override Color ImageMarginGradientEnd
      {
        get
        {
          return Color.Transparent;
        }
      }

      public override Color ImageMarginGradientMiddle
      {
        get
        {
          return Color.Transparent;
        }
      }

      public override Color SeparatorDark
      {
        get
        {
          return menusep;
        }
      }

      public override Color ToolStripGradientBegin
      {
        get
        {
          return topbg;
        }
      }

      public override Color ToolStripGradientMiddle
      {
        get
        {
          return topbg;
        }
      }

      public override Color ToolStripGradientEnd
      {
        get
        {
          return topbg;
        }
      }

      public override Color ButtonSelectedBorder
      {
        get
        {
          return ButtonSelectedGradientBegin;
        }
      }

      public override Color ButtonPressedBorder
      {
        get
        {
          return Color.FromArgb(153, 204, 255);
        }
      }

    }

    public ToolBarService()
    {
      ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new ColorTable());// IronScheme.Editor.Controls.Office2007Renderer();
      ServiceHost.StateChanged += new EventHandler(ServiceHost_StateChanged);
      toolbar.Dock = DockStyle.Top;
      toolbar.Height = 19;

      ServiceHost.Window.MainForm.Controls.Add(toolbar);
      ServiceHost.Window.MainForm.Controls.Add(ServiceHost.Window.Document as Control);
    }

    public bool ToolBarVisible
    {
      get { return toolbar.Visible; }
      set { toolbar.Visible = value; }
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

    static string MnemonicEscape(string s)
    {
      return s.Replace("&&", "||").Replace("&", string.Empty).Replace("||", "&");
    }

    public bool Add(ToolStripMenuItem parent, MenuItemAttribute mia)
    {
      if (!map.ContainsKey(parent))
      {
        ToolStrip ts = new ToolStrip();
        //ts.Stretch = false;
        ts.GripStyle = ToolStripGripStyle.Visible;
        //ts.AllowItemReorder = true;
        //ts.Dock = DockStyle.None;
        //ts.Anchor = AnchorStyles.None;
        //ts.Dock = DockStyle.Top;
        ts.Name = MnemonicEscape(parent.Text);
        ts.ImageList = ServiceHost.ImageListProvider.ImageList;
        ts.TabIndex = (map[parent] = toplevel.Count) + 1;
        toplevel.Add(ts);
        ts.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        //((HorLayoutSettings)ts.LayoutSettings).FlowDirection = FlowDirection.LeftToRight;
        toolbar.Controls.Add(ts);
        ts.BringToFront();
        ts.Visible = false;

      }

      if (mia != null)
      {
        ToolStrip sm = toplevel[map[parent]];
        ToolStripButton tbb = new ToolStripButton();
        tbb.Name = mia.invoke.Name;
        tbb.Click += new EventHandler(ButtonDefaultHandler);
        tbb.ImageIndex = ServiceHost.ImageListProvider[mia.Image];
        tbb.Tag = mia;
        tbb.ToolTipText = mia.Text;
        sm.Items.Add(tbb);
        sm.Dock = DockStyle.Left;
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
