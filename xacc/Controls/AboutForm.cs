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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Xacc.ComponentModel;

namespace Xacc.Controls
{
  class AboutForm : Form
  {
    public LinkLabel linkLabel1;
    public ProgressBar progressBar1;
  
    public AboutForm()
    {
      Size = new Size(408, 280);
      ShowInTaskbar = false;
      //FormBorderStyle = FormBorderStyle.None;
      StartPosition = FormStartPosition.CenterParent;

      Image i = Image.FromStream(typeof(AboutForm).Assembly.GetManifestResourceStream(
#if VS
        "IronScheme.Editor.Resources." +
#endif
        "splash.png"));

      Bitmap b = new Bitmap(i);

      BackgroundImage = b;

      SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
      UpdateStyles();
      InitializeComponent();
    }

    private void InitializeComponent()
    {
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.SuspendLayout();
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(6, 264);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(395, 10);
      this.progressBar1.TabIndex = 0;
      // 
      // linkLabel1
      // 
      this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
      this.linkLabel1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.linkLabel1.Location = new System.Drawing.Point(6, 257);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(395, 16);
      this.linkLabel1.TabIndex = 1;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "linkLabel1";
      this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.linkLabel1.Visible = false;
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      // 
      // AboutForm
      // 
      this.ClientSize = new System.Drawing.Size(408, 280);
      this.ControlBox = false;
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.linkLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.ResumeLayout(false);

    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://editor.ironscheme.net");
    }
  }
}
