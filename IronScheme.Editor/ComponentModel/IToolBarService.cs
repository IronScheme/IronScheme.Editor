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
using System.Xml.Serialization;
using System.IO;

namespace Xacc.ComponentModel
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
    ToolStripContainer ToolBar { get;}

    void Save();

    void Load();
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

    class ColorTable : ProfessionalColorTable
    {
      static Color menubg = Color.FromArgb(246, 246, 246);
      static Color topbg =  Color.FromArgb(240, 240, 240);
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
      ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new ColorTable());// Xacc.Controls.Office2007Renderer();
      ServiceHost.StateChanged += new EventHandler(ServiceHost_StateChanged);
      toolbar.Dock = DockStyle.Fill;

      toolbar.Name = "Container";
      ServiceHost.Window.MainForm.Controls.Add(toolbar);
      toolbar.ContentPanel.Controls.Add(ServiceHost.Window.Document as Control);

      //toolbar.TopToolStripPanel.Name = "Toolbar";
      //toolbar.TopToolStripPanel.
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
        toolbar.TopToolStripPanel.Join(ts, 0);
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

    #region IToolBarService Members

    static readonly XmlSerializer SER = new XmlSerializer(typeof(ToolbarSettings));


    public void Save()
    {
      ToolbarSettings tbs = new ToolbarSettings();

      foreach (ToolStrip ts in toplevel)
      {
        ToolbarSetting tbss = new ToolbarSetting();
        if (ts.Visible)
        {
          tbss.Name = ts.Name;
          tbss.Bounds = ts.Bounds;
          tbs.Settings.Add(tbss);
        }

        
      }

      using (Stream s = File.Create("toolbarsettings.xml"))
      {
        SER.Serialize(s, tbs);
      }

    }

    public void Load()
    {
      if (File.Exists("toolbarsettings.xml"))
      {
        using (Stream s = File.OpenRead("toolbarsettings.xml"))
        {
          ToolbarSettings tbs =  SER.Deserialize(s) as ToolbarSettings;

          if (tbs != null)
          {
            toolbar.TopToolStripPanel.SuspendLayout();
            foreach (ToolbarSetting tbss in tbs.Settings)
            {
              foreach (ToolStrip ts in toplevel)
              {
                if (ts.Name == tbss.Name)
                {
                  
                  ts.Size = tbss.Bounds.Size;
                  ts.Location = tbss.Bounds.Location;
                  //ts.Bounds = tbss.Bounds;
                }
              }
            }
            toolbar.TopToolStripPanel.ResumeLayout();
          }
        }
      }
    }

    #endregion
  }

  public class ToolbarSetting
  {
    public string Name;
    public Rectangle Bounds;

    public override string ToString()
    {
      return string.Format("{0} : {1}", Name, Bounds);
    }
  }

  public class ToolbarSettings
  {
    public List<ToolbarSetting> Settings = new List<ToolbarSetting>();
  }
}
