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


using Microsoft.Build.BuildEngine;
using Xacc.Build;

using Project = Xacc.Build.Project;

using SR = System.Resources;
#endregion

namespace Xacc.Controls
{
	sealed class OutlineView : TreeView
	{
		public OutlineView()
		{
      Dock = DockStyle.Fill;
      Sorted = true;

			PathSeparator = Path.DirectorySeparatorChar.ToString();
			Font = SystemInformation.MenuFont;

      TreeViewNodeSorter = new TreeViewComparer();
		}

    class TreeViewComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        TreeNode a = x as TreeNode;
        TreeNode b = y as TreeNode;

        if (a == null)
        {
          return 1;
        }
        if (b == null)
        {
          return -1;
        }
        BuildItem locabi = a.Tag as BuildItem;
        BuildItem locbbi = b.Tag as BuildItem;

        string loca = locabi != null ? locabi.Include : null;
        string locb = locbbi != null ? locbbi.Include : null;

        if (a.Text == "Properties")
        {
          return -1;
        }
        if (b.Text == "Properties")
        {
          return 1;
        }

        if (a.Text == "References")
        {
          return -1;
        }
        if (b.Text == "References")
        {
          return 1;
        }

        if (loca == null)
        {
          if (locb == null)
          {
            return a.Text.CompareTo(b.Text);
          }
          return -1;
        }
        if (locb == null)
        {
          return 1;
        }

        if (Directory.Exists(loca))
        {
          if (Directory.Exists(locb))
          {
            return loca.CompareTo(locb);
          }
          else
          {
            return -1;
          }
        }
        else
        {
          if (Directory.Exists(locb))
          {
            return -1;
          }
          else
          {
            return loca.CompareTo(locb);
          }
        }

      }
    }

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			if (e.Node.ImageIndex == 1)
			{
				e.Node.ImageIndex = e.Node.SelectedImageIndex = 2;
			}
			base.OnBeforeExpand (e);
		}

		protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			if (e.Node.ImageIndex == 2)
			{
				e.Node.ImageIndex = e.Node.SelectedImageIndex = 1;
			}
			base.OnBeforeCollapse (e);
		}

    protected override void Dispose(bool disposing)
    {
      base.Dispose (disposing);
    }

		protected override void OnMouseDown(MouseEventArgs e)
		{
      base.OnMouseDown (e);
      
      TreeNode r = SelectedNode;
      if (r != null)
      {
        while (r.Parent != null)
        {
          r = r.Parent;
        }
        ServiceHost.Project.Current = r.Tag as Project;
      }
		}

    protected override void OnMouseUp(MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        if (SelectedNode == null)
        {
          return;
        }

        ContextMenuStrip cm = new ContextMenuStrip();

        object tag = SelectedNode.Tag;

        if (tag is Project)
        {
          Project p = tag as Project;

          IMenuService ms = ServiceHost.Menu;

          ToolStripMenuItem pm = ms["Project"];

          foreach (ToolStripItem mi in pm.DropDownItems)
          {
            if (mi is ToolStripSeparator)
            {
              cm.Items.Add(new ToolStripSeparator());
            }
            else
            {
              cm.Items.Add(((ToolStripMenuItem)mi).Clone());
            }
          }
        }
        else if (tag is BuildItem)
        {
          ToolStripMenuItem pmi = new ToolStripMenuItem("Remove", null,
            new EventHandler(RemoveFile));


          IImageListProviderService ims = ServiceHost.ImageListProvider;

          cm.ImageList = ims.ImageList;

          _RemoveFile rf = new _RemoveFile();
          rf.value = tag;
          pmi.ImageIndex = ims[rf];
          cm.Items.Add(pmi);

          cm.Items.Add(new ToolStripSeparator());

          pmi = new ToolStripMenuItem("Action");
          cm.Items.Add(pmi);

          Project proj = ServiceHost.Project.Current;

          foreach (string action in proj.Actions)
          {
            ToolStripMenuItem am = new ToolStripMenuItem(action, null, new EventHandler(ChangeAction));
            pmi.DropDownItems.Add(am);

            string dd = (tag as BuildItem).Name;
            if (dd == action)
            {
              am.Checked = true;
            }
          }
        }

        cm.Show(this, new Point(e.X, e.Y));
      }
      base.OnMouseUp (e);
    }

		[Name("Remove"), Image("File.Close.png")]
		class _RemoveFile
		{
			public object value;
		}

		void ChangeAction(object sender, EventArgs e)
		{
      BuildItem file = SelectedNode.Tag as BuildItem;
      string action = (sender as ToolStripMenuItem).Text as string;

      Project proj = ServiceHost.Project.Current;

			proj.RemoveFile(file.Include);

      if (SelectedNode.Nodes.Count == 0)
      {
        SelectedNode.Remove();
      }

			proj.AddFile(file.Include, action, true);
		}

		void RemoveFile(object sender, EventArgs e)
		{
      BuildItem file = (SelectedNode.Tag) as BuildItem;

			if (file != null)
			{
				if (DialogResult.OK == MessageBox.Show(this, "Remove '" + file + "' from project?",
					"Confirmation", MessageBoxButtons.OKCancel))
				{
          Project proj = ServiceHost.Project.Current;
					SelectedNode.Remove();
					proj.RemoveFile(file.Include);
				}
			}
		}
	}
}
