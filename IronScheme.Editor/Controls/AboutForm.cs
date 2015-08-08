#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion



using System.Windows.Forms;
using System.Drawing;

namespace IronScheme.Editor.Controls
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
