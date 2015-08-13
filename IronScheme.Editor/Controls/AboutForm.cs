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
      progressBar1 = new ProgressBar();
      linkLabel1 = new LinkLabel();
      SuspendLayout();
      // 
      // progressBar1
      // 
      progressBar1.Location = new Point(6, 264);
      progressBar1.Name = "progressBar1";
      progressBar1.Size = new Size(395, 10);
      progressBar1.TabIndex = 0;
      // 
      // linkLabel1
      // 
      linkLabel1.BackColor = Color.Transparent;
      linkLabel1.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
      linkLabel1.Location = new Point(6, 257);
      linkLabel1.Name = "linkLabel1";
      linkLabel1.Size = new Size(395, 16);
      linkLabel1.TabIndex = 1;
      linkLabel1.TabStop = true;
      linkLabel1.Text = "linkLabel1";
      linkLabel1.TextAlign = ContentAlignment.MiddleCenter;
      linkLabel1.Visible = false;
      linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
      // 
      // AboutForm
      // 
      ClientSize = new Size(408, 280);
      ControlBox = false;
      Controls.Add(progressBar1);
      Controls.Add(linkLabel1);
      FormBorderStyle = FormBorderStyle.None;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "AboutForm";
      SizeGripStyle = SizeGripStyle.Hide;
      ResumeLayout(false);

    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://editor.ironscheme.net");
    }
  }
}
