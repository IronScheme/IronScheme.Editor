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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Xacc.Build;
using Xacc.ComponentModel;

namespace Xacc.Controls
{
	class ProjectBuildOrderForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    internal PictureListBox listBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectBuildOrderForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

      StartPosition = FormStartPosition.CenterParent;
      
      foreach (Project p in ServiceHost.Project.OpenProjects)
      {
        listBox1.Items.Add(p);
      }
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.listBox1 = new Xacc.Controls.PictureListBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(125, 247);
      this.button1.Name = "button1";
      this.button1.TabIndex = 0;
      this.button1.Text = "OK";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button2.Location = new System.Drawing.Point(205, 247);
      this.button2.Name = "button2";
      this.button2.TabIndex = 1;
      this.button2.Text = "Cancel";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // listBox1
      // 
      this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.listBox1.ItemHeight = 21;
      this.listBox1.Location = new System.Drawing.Point(8, 8);
      this.listBox1.Name = "listBox1";
      this.listBox1.Size = new System.Drawing.Size(269, 233);
      this.listBox1.TabIndex = 2;
      this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
      // 
      // ProjectBuildOrderForm
      // 
      this.AcceptButton = this.button1;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.button2;
      this.ClientSize = new System.Drawing.Size(285, 277);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "ProjectBuildOrderForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Project Build Order";
      this.ResumeLayout(false);

    }
		#endregion

    private void button1_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void listBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if (e.Alt)
      {
        if (e.KeyCode == Keys.Up)
        {
          object o = listBox1.SelectedItem;
          int i = listBox1.SelectedIndex;
          if (i > 0)
          {
            listBox1.Items.RemoveAt(i);
            listBox1.Items.Insert(--i, o);
            listBox1.SelectedIndex = i;
          }
        }
        else
          if (e.KeyCode == Keys.Down)
        {
          object o = listBox1.SelectedItem;
          int i = listBox1.SelectedIndex;
          if (i >= 0 && i < listBox1.Items.Count - 2)
          {
            listBox1.Items.RemoveAt(i);
            listBox1.Items.Insert(++i, o);
            listBox1.SelectedIndex = i;
          }
        }
      }
    }
	}
}
