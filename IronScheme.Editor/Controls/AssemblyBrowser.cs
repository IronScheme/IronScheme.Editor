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

using System.Windows.Forms;
using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.Controls
{
  /// <summary>
  /// Summary description for AssemblyBrowser.
  /// </summary>
  class AssemblyBrowser : UserControl, IDocument
	{
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Splitter splitter2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AssemblyBrowser()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.panel1 = new System.Windows.Forms.Panel();
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.panel2 = new System.Windows.Forms.Panel();
      this.splitter2 = new System.Windows.Forms.Splitter();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 480);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(880, 144);
      this.panel1.TabIndex = 0;
      // 
      // treeView1
      // 
      this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
      this.treeView1.ImageIndex = -1;
      this.treeView1.Location = new System.Drawing.Point(0, 0);
      this.treeView1.Name = "treeView1";
      this.treeView1.SelectedImageIndex = -1;
      this.treeView1.Size = new System.Drawing.Size(216, 477);
      this.treeView1.TabIndex = 1;
      // 
      // splitter1
      // 
      this.splitter1.Location = new System.Drawing.Point(216, 0);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(3, 477);
      this.splitter1.TabIndex = 2;
      this.splitter1.TabStop = false;
      // 
      // panel2
      // 
      this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(219, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(661, 477);
      this.panel2.TabIndex = 3;
      // 
      // splitter2
      // 
      this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.splitter2.Location = new System.Drawing.Point(0, 477);
      this.splitter2.Name = "splitter2";
      this.splitter2.Size = new System.Drawing.Size(880, 3);
      this.splitter2.TabIndex = 4;
      this.splitter2.TabStop = false;
      // 
      // AssemblyBrowser
      // 
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.splitter1);
      this.Controls.Add(this.treeView1);
      this.Controls.Add(this.splitter2);
      this.Controls.Add(this.panel1);
      this.Name = "AssemblyBrowser";
      this.Size = new System.Drawing.Size(880, 624);
      this.ResumeLayout(false);

    }
		#endregion

    #region IDocument Members

    void IDocument.Open(string filename)
    {
      // TODO:  Add AssemblyBrowser.Open implementation
    }

    void IDocument.Close()
    {
      // TODO:  Add AssemblyBrowser.Close implementation
    }

    string IDocument.Info
    {
      get {return "";}
    }

    #endregion
  }
}
