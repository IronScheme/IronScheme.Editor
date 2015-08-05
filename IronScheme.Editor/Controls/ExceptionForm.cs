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
using System.Windows.Forms;

namespace IronScheme.Editor.Controls
{

  class ExceptionForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox messageBox;
    private System.Windows.Forms.TextBox detailsBox;
    private System.Windows.Forms.TextBox infoBox;
    private System.Windows.Forms.TextBox emailBox;
    private System.Windows.Forms.TextBox nameBox;
    private System.Windows.Forms.Button reportBut;
    private System.Windows.Forms.Button ignoreBut;
    private System.Windows.Forms.Button igallwaysBut;
    private System.Windows.Forms.Button exitBut;
    private System.Windows.Forms.Button checkBut;
    private System.Windows.Forms.Button helpBut;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExceptionForm()
		{
			InitializeComponent();

      reportBut.Enabled = false;

      helpBut.Enabled = false;
      detailsBox.MaxLength = 1000;
      infoBox.MaxLength = 1000;

      if (ComponentModel.ServiceHost.Discovery != null)
      {
        helpBut.Enabled = ComponentModel.ServiceHost.Discovery.NetRuntimeSDK != null;
      }

      igallwaysBut.Enabled = false;
      nameBox.Text = Environment.UserName;
      emailBox.Text = Environment.UserName + "@bugreportersanonymous.com";
      infoBox.Text = Diagnostics.Trace.SystemInfo;
      detailsBox.ReadOnly = false;
      messageBox.Text = "Please enter your bug details below.";
      exitBut.Text = "Cancel";
		}
    
    bool appstateinvalid = false;
    Exception exception;

    public Exception Exception
    {
      get {return exception;}
      set 
      {
        exception = value;
        this.Text = "An unhandled exception has occured";
        Exception inner = value;
        messageBox.Text = inner.GetType() + Environment.NewLine + Environment.NewLine + inner.Message;
        detailsBox.Text = value.ToString();
        infoBox.Text = Diagnostics.Trace.GetTrace();
        exitBut.Text = "Exit";
        detailsBox.ReadOnly = true;
      }
    }

    public bool ApplicationStateInvalid
    {
      get {return appstateinvalid;}
      set 
      {
        appstateinvalid = value;
        if (value)
        {
          igallwaysBut.Enabled = false;
          ignoreBut.Enabled = false;
        }
      }
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.messageBox = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.detailsBox = new System.Windows.Forms.TextBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label3 = new System.Windows.Forms.Label();
      this.infoBox = new System.Windows.Forms.TextBox();
      this.emailBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.nameBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.reportBut = new System.Windows.Forms.Button();
      this.ignoreBut = new System.Windows.Forms.Button();
      this.igallwaysBut = new System.Windows.Forms.Button();
      this.exitBut = new System.Windows.Forms.Button();
      this.checkBut = new System.Windows.Forms.Button();
      this.helpBut = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.messageBox);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(616, 83);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Message";
      // 
      // messageBox
      // 
      this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.messageBox.Location = new System.Drawing.Point(8, 18);
      this.messageBox.Multiline = true;
      this.messageBox.Name = "messageBox";
      this.messageBox.ReadOnly = true;
      this.messageBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.messageBox.Size = new System.Drawing.Size(600, 56);
      this.messageBox.TabIndex = 1;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.detailsBox);
      this.groupBox2.Location = new System.Drawing.Point(8, 83);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(616, 249);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Details";
      // 
      // detailsBox
      // 
      this.detailsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.detailsBox.Location = new System.Drawing.Point(8, 18);
      this.detailsBox.Multiline = true;
      this.detailsBox.Name = "detailsBox";
      this.detailsBox.ReadOnly = true;
      this.detailsBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.detailsBox.Size = new System.Drawing.Size(600, 222);
      this.detailsBox.TabIndex = 0;
      this.detailsBox.WordWrap = false;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.label3);
      this.groupBox3.Controls.Add(this.infoBox);
      this.groupBox3.Controls.Add(this.emailBox);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.nameBox);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Location = new System.Drawing.Point(8, 332);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(616, 305);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Optional";
      // 
      // label3
      // 
      this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label3.Location = new System.Drawing.Point(11, 77);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(599, 19);
      this.label3.TabIndex = 5;
      this.label3.Text = "Please supply any additional information you feel is deemed necessary (info below" +
          " can be deleted at your discretion)";
      // 
      // infoBox
      // 
      this.infoBox.Location = new System.Drawing.Point(8, 102);
      this.infoBox.Multiline = true;
      this.infoBox.Name = "infoBox";
      this.infoBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.infoBox.Size = new System.Drawing.Size(600, 193);
      this.infoBox.TabIndex = 4;
      this.infoBox.WordWrap = false;
      // 
      // emailBox
      // 
      this.emailBox.Location = new System.Drawing.Point(80, 46);
      this.emailBox.Name = "emailBox";
      this.emailBox.Size = new System.Drawing.Size(528, 22);
      this.emailBox.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2.Location = new System.Drawing.Point(12, 50);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(48, 18);
      this.label2.TabIndex = 2;
      this.label2.Text = "Email";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // nameBox
      // 
      this.nameBox.Location = new System.Drawing.Point(80, 18);
      this.nameBox.Name = "nameBox";
      this.nameBox.Size = new System.Drawing.Size(528, 22);
      this.nameBox.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label1.Location = new System.Drawing.Point(11, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 18);
      this.label1.TabIndex = 0;
      this.label1.Text = "Name";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // reportBut
      // 
      this.reportBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.reportBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.reportBut.Location = new System.Drawing.Point(216, 646);
      this.reportBut.Name = "reportBut";
      this.reportBut.Size = new System.Drawing.Size(96, 27);
      this.reportBut.TabIndex = 3;
      this.reportBut.Text = "Report error";
      this.reportBut.Click += new System.EventHandler(this.reportBut_Click);
      // 
      // ignoreBut
      // 
      this.ignoreBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ignoreBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.ignoreBut.Location = new System.Drawing.Point(320, 646);
      this.ignoreBut.Name = "ignoreBut";
      this.ignoreBut.Size = new System.Drawing.Size(96, 27);
      this.ignoreBut.TabIndex = 4;
      this.ignoreBut.Text = "Ignore";
      this.ignoreBut.Click += new System.EventHandler(this.ignoreBut_Click);
      // 
      // igallwaysBut
      // 
      this.igallwaysBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.igallwaysBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.igallwaysBut.Location = new System.Drawing.Point(424, 646);
      this.igallwaysBut.Name = "igallwaysBut";
      this.igallwaysBut.Size = new System.Drawing.Size(96, 27);
      this.igallwaysBut.TabIndex = 5;
      this.igallwaysBut.Text = "Ignore Allways";
      this.igallwaysBut.Click += new System.EventHandler(this.igallwaysBut_Click);
      // 
      // exitBut
      // 
      this.exitBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.exitBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.exitBut.Location = new System.Drawing.Point(528, 646);
      this.exitBut.Name = "exitBut";
      this.exitBut.Size = new System.Drawing.Size(96, 27);
      this.exitBut.TabIndex = 6;
      this.exitBut.Text = "Exit";
      this.exitBut.Click += new System.EventHandler(this.exitBut_Click);
      // 
      // checkBut
      // 
      this.checkBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.checkBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBut.Location = new System.Drawing.Point(112, 646);
      this.checkBut.Name = "checkBut";
      this.checkBut.Size = new System.Drawing.Size(96, 27);
      this.checkBut.TabIndex = 7;
      this.checkBut.Text = "Check error";
      this.checkBut.Click += new System.EventHandler(this.checkBut_Click);
      // 
      // helpBut
      // 
      this.helpBut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.helpBut.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.helpBut.Location = new System.Drawing.Point(8, 646);
      this.helpBut.Name = "helpBut";
      this.helpBut.Size = new System.Drawing.Size(96, 27);
      this.helpBut.TabIndex = 8;
      this.helpBut.Text = "Break";
      this.helpBut.Click += new System.EventHandler(this.helpBut_Click);
      // 
      // ExceptionForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 15);
      this.ClientSize = new System.Drawing.Size(634, 680);
      this.Controls.Add(this.helpBut);
      this.Controls.Add(this.checkBut);
      this.Controls.Add(this.exitBut);
      this.Controls.Add(this.igallwaysBut);
      this.Controls.Add(this.ignoreBut);
      this.Controls.Add(this.reportBut);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ExceptionForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Submit error or bug";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }
		#endregion

    void exitBut_Click(object sender, System.EventArgs e)
    {
      if (exception != null)
      {
        Application.Exit();
      }
      else
      {
        Close();
      }
    }

    void ignoreBut_Click(object sender, System.EventArgs e)
    {
      Close();
    }

    void igallwaysBut_Click(object sender, System.EventArgs e)
    {
      Close();
    }

    void reportBut_Click(object sender, System.EventArgs e)
    {
      try
      {
        Diagnostics.ErrorReport re = new IronScheme.Editor.Diagnostics.ErrorReport();
        string t = exception == null ? detailsBox.Text : exception.ToString();
        int id = re.ReportError(nameBox.Text, emailBox.Text, infoBox.Text, t);
        MessageBox.Show(this, "Thank you! Your reference is: " + id);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex);
        MessageBox.Show(this, "There was a problem submitting the error/bug.\r\nPlease try again later, if you can and dont mind.");
      }
    }

    void helpBut_Click(object sender, System.EventArgs e)
    {
      System.Diagnostics.Debugger.Break();
    }

    void checkBut_Click(object sender, System.EventArgs e)
    {
      System.Diagnostics.Process.Start("http://xacc.no-ip.info:8888/xaccerror/errors.aspx");
      //ReportedErrorsDialog d = new ReportedErrorsDialog();
      //d.Show();
    }
	}
}
