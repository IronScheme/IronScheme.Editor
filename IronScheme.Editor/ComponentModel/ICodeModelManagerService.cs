#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using System;
using System.Collections;

using IronScheme.Editor.CodeModel;
using System.Windows.Forms;

#endregion

namespace IronScheme.Editor.ComponentModel
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
