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

namespace IronScheme.Editor.Controls
{
	/// <summary>
	/// Summary description for ProgressDialog.
	/// </summary>
	class ProgressDialog : System.Windows.Forms.Form
	{
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProgressDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

    public ProgressDialog(string infotext) : this()
    {
      label1.Text = infotext;
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
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.progressBar1.Location = new System.Drawing.Point(0, 49);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(282, 23);
      this.progressBar1.TabIndex = 0;
      this.progressBar1.Visible = false;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(282, 49);
      this.label1.TabIndex = 1;
      this.label1.Text = "No Info Provided";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // ProgressDialog
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(282, 72);
      this.ControlBox = false;
      this.Controls.Add(this.label1);
      this.Controls.Add(this.progressBar1);
      this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ProgressDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Please wait...";
      this.TopMost = true;
      this.ResumeLayout(false);

    }
		#endregion
	}
}
