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
using System.IO;
using System.Collections;

using Xacc.CodeModel;
using System.Windows.Forms;

#endregion

namespace Xacc.ComponentModel
{
  public interface IHasCodeModel
  {
    ICodeFile CodeModel { get;}
  }
  /// <summary>
  /// Provides services for diplaying CodeModel
  /// </summary>
  [Name("CodeModel manager")]
  public interface ICodeModelManagerService : IService
  {
    /// <summary>
    /// The tree view
    /// </summary>
    TreeView Tree				{get;}

    /// <summary>
    /// Registers codemodel type
    /// </summary>
    /// <param name="ext"></param>
    /// <param name="codemodeltype"></param>
    [Obsolete("Used?")]
    void Register(string ext, Type codemodeltype);

    /// <summary>
    /// Generates the Codemodel
    /// </summary>
    /// <param name="root">the project</param>
    void Run(Build.Project root);
    void Run(ICodeElement rootelement);
  }

  class CodeModelManager : ServiceBase, ICodeModelManagerService
  {
    readonly TreeView tree = new TreeView();
    readonly Hashtable cache	= new Hashtable();
    readonly Hashtable resolv = new Hashtable();

    public CodeModelManager()
    {
      tree.SuspendLayout();
      tree.Dock = DockStyle.Fill;
      tree.BorderStyle = BorderStyle.None;
      tree.Font = SystemInformation.MenuFont;
      tree.ImageList = ServiceHost.ImageListProvider.ImageList;
      tree.ShowRootLines = false;
      tree.DoubleClick +=new EventHandler(tree_DoubleClick);
      tree.Dock = DockStyle.Fill;
      tree.Width = 100;
      tree.Scrollable = true;
      tree.Sorted = false;

      tree.ShowPlusMinus = true;
      tree.ResumeLayout();
    }

    public void Register(string ext, Type codemodeltype)
    {
      resolv.Add(ext, codemodeltype);
    }

    public TreeView Tree
    {
      get{ return tree;} 
    }

    delegate void Rerun(ICodeElement e);

    Rerun rerun;

    public void Run(ICodeElement rootelem)
    {
      if (rootelem == null)
      {
        return;
      }

      // HACK
      if (rootelem.Name.EndsWith("ironscheme.shell.txt"))
      {
        return;
      }

      if (tree.InvokeRequired)
      {
        if (rerun == null)
        {
          rerun = new Rerun(Run);
        }

        tree.BeginInvoke(rerun, new object[] { rootelem });
        return;
      }

      TreeNode root = null;

      tree.SuspendLayout();

      foreach (TreeNode rn in tree.Nodes)
      {
        if (rn.Tag.ToString() == rootelem.Fullname)
        {
          tree.Nodes.Remove(rn);
          break;
        }
      }

      root = tree.Nodes.Add(rootelem.ToString());
      root.Tag = rootelem;
      root.ImageIndex = root.SelectedImageIndex = ServiceHost.ImageListProvider[rootelem];

      if (rootelem is ICodeContainerElement)
      {
        foreach (ICodeElement ce in ((ICodeContainerElement)rootelem).Elements)
        {
          AddElement(ce, root);
        }
      }

      root.ExpandAll();
      tree.ResumeLayout();
    }

    public void Run(Build.Project prj)
    {
      if (prj.IsInvisible)
      {
        return;
      }

      Run(prj.CodeModel);
    }

    void CheckElement(ICodeElement parent, TreeNode node)
    {
      ICodeContainerElement icc = parent as ICodeContainerElement;
 
      if (icc != null)
      {
        ICodeElement ice = icc[node.Text];
        if (ice != null)
        {
          ICodeElement ice2 = node.Tag as ICodeElement;
          if (ice2 != null && ice2.Fullname == ice.Fullname)
          {
            foreach (TreeNode n in node.Nodes)
            {
              CheckElement(ice, n);
            }
            return;
          }
        }
        TreeNode nparent = node.Parent;
        node.Remove();
        AddElement( ice, nparent);
      }
    }

    void AddElement(ICodeElement elem, TreeNode parent)
    {
      ICodeNamespace cns = elem as ICodeNamespace;
      if (cns != null && cns.ElementCount == 0)
      {
        return;
      }
      parent = parent.Nodes.Add(elem.ToString());
      parent.Tag = elem;

      parent.ImageIndex = parent.SelectedImageIndex = ServiceHost.ImageListProvider[elem];

      ICodeContainerElement icc = elem as ICodeContainerElement;
 
      if (icc != null)
      {
        foreach (ICodeElement ce in icc.Elements)
        {
          AddElement(ce, parent);
        }
      }
    }

		void tree_DoubleClick(object sender, EventArgs e)
		{
			TreeNode cur = tree.SelectedNode;
			if (cur != null && cur.Tag != null)
			{
				ICodeElement elem = cur.Tag as ICodeElement;

        if (elem.Location != null)
        {
          string filename = elem.Location.Filename;

          if (ServiceHost.Project.Current != null)
          {
            Controls.AdvancedTextBox atb = ServiceHost.Project.Current.OpenFile(filename) as Controls.AdvancedTextBox;

            if (atb != null)
            {
              atb.Buffer.SelectLocation(elem.Location);
              atb.MoveCaretIntoView();
              ServiceHost.File.BringToFront(atb);
            }
          }
          else
          {
            Controls.AdvancedTextBox atb = ServiceHost.File.Open(filename) as Controls.AdvancedTextBox;
            if (atb != null)
            {
              atb.Buffer.SelectLocation(elem.Location);
              atb.MoveCaretIntoView();
              ServiceHost.File.BringToFront(atb);
            }

          }
        }
			}
		}
	}
}
