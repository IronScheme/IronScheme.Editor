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
using Xacc.Build;


using SR = System.Resources;
#endregion
#if OLDBUILD
namespace Xacc.Controls
{
  [Obsolete("Moving to MSBuild")]
	class ProcessActionDialog : Form
	{
		TabControl tc;

		protected override void OnClosing(CancelEventArgs e)
		{
			foreach (Control c in Controls)
			{
				c.Dispose();
			}
			base.OnClosing (e);
		}

    public ProcessActionDialog(Project proj)
    {
      Font = SystemInformation.MenuFont;
      FormBorderStyle = FormBorderStyle.FixedToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;

      ShowInTaskbar = false;
      Size = new Size(500, 300);
      StartPosition = FormStartPosition.CenterParent;
      tc = new TabControl();
      tc.Dock = DockStyle.Fill;
      Controls.Add(tc);
      IImageListProviderService ips = ServiceHost.ImageListProvider;
      if (ips != null)
      {
        tc.ImageList = ips.ImageList;
      }
      else
      {
        tc.ImageList = new ImageList();
        tc.ImageList.ColorDepth = ColorDepth.Depth32Bit;
      }
      string name = proj.ProjectName;

      Text = "Project Properties: " + name;

      foreach (string ba in proj.Actions)
      {
        //if (ba is NullAction)
        //{
        //  continue;
        //}

        //CustomAction ca = ba as CustomAction;
        //if (ca != null && ca.Options.Length > 0)
        //{
        //  TabControl tcc = new TabControl();
        //  tcc.Multiline = true;
        //  tcc.SizeMode = TabSizeMode.FillToRight;
        //  tcc.Dock = DockStyle.Fill;

        //  name = NameAttribute.GetName(ba.GetType());

        //  TabPage tpp = new TabPage(name);
        //  tpp.BackColor = Color.FromArgb(252,252,254);

        //  if (ips != null)
        //  {
        //    tpp.ImageIndex = ips[ba];
        //  }

        //  tc.TabPages.Add(tpp);

        //  Hashtable cats = new Hashtable();

        //  tpp.Controls.Add(tcc);

        //  foreach (Option o in ca.Options)
        //  {
        //    if (!cats.ContainsKey(o.Category))
        //    {
        //      TabPage tp = new TabPage(o.Category);
        //      tp.BackColor = Color.FromArgb(252,252,254);
        //      cats.Add(o.Category, tp);
        //    }
        //  }

        //  foreach (Option o in ca.Options)
        //  {
        //    TabPage tp = cats[o.Category] as TabPage;
				
        //    tp.Controls.Add( new OptionPanel(o, ca));
        //  }

        //  ArrayList keys = new ArrayList(cats.Keys);
        //  keys.Sort();

        //  foreach (string k in keys)
        //  {
        //    tcc.TabPages.Add(cats[k] as TabPage);
        //  }
        //}
      }
    }

    [Obsolete("Moving to MSBuild")]
		class OptionPanel : Panel
		{
			Option option;
      CustomAction proj;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
          proj.SetOptionValue(option, Tag);
				}
				base.Dispose (disposing);
			}


			public OptionPanel(Option o, CustomAction proj)
			{
				option = o;
        this.proj = proj;
  			Tag = proj.GetOptionValue(o);
        DockPadding.All = 1;
				Dock = DockStyle.Top;
        BackColor = Color.FromArgb(252,252,254);
				Label l = new Label();
				
				l.Text = o.Name;
				l.Dock = DockStyle.Left;
				l.Width = 130;
        l.TextAlign = ContentAlignment.MiddleLeft;

				switch (o.ArgumentType)
				{
					case "string":
						if (o.AllowedValues.Length > 0)
						{
							ComboBox cb = new ComboBox();
							cb.DropDownStyle = ComboBoxStyle.DropDownList;
							cb.SelectionChangeCommitted +=new EventHandler(cb_CheckedChanged);
							foreach (string av in o.AllowedValues)
							{
								cb.Items.Add(av);
							}
							cb.Dock = DockStyle.Fill;
							Controls.Add(cb);
							if (Tag != null)
							{
								cb.SelectedItem = Tag;
							}
						}
						else
						{
							TextBox tb = new TextBox();
							tb.Dock = DockStyle.Fill;
              tb.CausesValidation = true;
              tb.Validated +=new EventHandler(cb_CheckedChanged);
							Controls.Add(tb);
							if (Tag != null)
							{
                if (Tag is Array)
                {
                  string[] vals = Tag as string[];
                  string sep = ";";
                  tb.Text = string.Join(sep, vals);
                }
                else
                {
                  tb.Text = Tag.ToString();
                }
							}
						}
						break;
					case "int":
					{
						TextBox tb = new TextBox();
						tb.TextChanged +=new EventHandler(cb_CheckedChanged);
						tb.Dock = DockStyle.Fill;
						Controls.Add(tb);
						if (Tag != null)
						{
							tb.Text = Tag as string;
						}

						break;
					}
          case "bool":
					{
						CheckBox cb = new CheckBox();
						cb.CheckedChanged +=new EventHandler(cb_CheckedChanged);
						Controls.Add(cb);
						cb.Dock = DockStyle.Fill;
						cb.FlatStyle = FlatStyle.System;
						if (Tag != null)
						{
							cb.Checked = true;
						}
						break;
					}
					default:
						Debug.Fail("Dont know " + o.ArgumentType + "!");
						break;
				}
				Controls.Add( l);

				Height = FontHeight + 11; //WTF?

				ToolTip tt = new ToolTip();

				foreach (Control c in Controls)
				{
					tt.SetToolTip(c, o.Description);
				}
			}

			private void cb_CheckedChanged(object sender, EventArgs e)
			{
				CheckBox cb = sender as CheckBox;
				if (cb != null)
				{
					Tag = (cb.Checked ? "true" : null);
					return;
				}
				TextBox tb = sender as TextBox;
				if (tb != null)
				{
          if (Tag is Array)
          {
            char sep = ';';
            string[] vals = tb.Text.Split(sep);
            Tag = vals;
          }
          else
          {
            Tag = tb.Text;
          }
 
					return;
				}
				ComboBox cbb = sender as ComboBox;
				if (cbb != null)
				{
					Tag = cbb.SelectedItem;
					return;
				}
			}
		}
	}
}
#endif