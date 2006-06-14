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
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Xacc.Controls
{
	/// <summary>
	/// Summary description for DialogInvokeActionForm.
	/// </summary>
	class DialogInvokeActionForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

    public DialogInvokeActionForm() : this(null)
    {
    }

    delegate void MethodNameHandler(string p1, int p2, Color p3, AnchorStyles p4);

    public void MethodName(string p1, int p2, Color p3, AnchorStyles p4){}

		public DialogInvokeActionForm(MethodInfo mi)
		{
      if (mi == null)
      {
        mi = new MethodNameHandler(MethodName).Method;
      }
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
      ShowInTaskbar = false;
      
      groupBox1.Text = mi.DeclaringType.Name + " : " + mi.ToString();

      int c = 0;
      int h = 0;

      foreach (ParameterInfo pi in mi.GetParameters())
      {
        GenericTypeEditor gte = new GenericTypeEditor();
        gte.Info = pi;
        gte.TabIndex = c;
        gte.Dock = DockStyle.Top;
        h = gte.Height;
        groupBox1.Controls.Add(gte);
        c++;
        if (c == 1)
        {
          gte.Focus();
        }
      }

      Height = c * h + 58;
      
		}

    public object[] GetValues()
    {
      ArrayList values = new ArrayList();
      foreach (Control c in groupBox1.Controls)
      {
        if ( c is GenericTypeEditor)
        {
          values.Add(((GenericTypeEditor)c).Value);
        }
      }
      return values.ToArray();
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.button3 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Controls.Add(this.groupBox1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.DockPadding.All = 4;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(254, 206);
      this.panel1.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.BackColor = System.Drawing.SystemColors.Window;
      this.groupBox1.Controls.Add(this.button3);
      this.groupBox1.Controls.Add(this.button2);
      this.groupBox1.Controls.Add(this.button1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(244, 196);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "groupBox1";
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button3.Location = new System.Drawing.Point(4, 169);
      this.button3.Name = "button3";
      this.button3.TabIndex = 3;
      this.button3.TabStop = false;
      this.button3.Text = "OK";
      this.button3.Visible = false;
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button2.Location = new System.Drawing.Point(164, 169);
      this.button2.Name = "button2";
      this.button2.TabIndex = 12;
      this.button2.Text = "Cancel";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(84, 169);
      this.button1.Name = "button1";
      this.button1.TabIndex = 11;
      this.button1.Text = "OK";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // DialogInvokeActionForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.BackColor = System.Drawing.SystemColors.Window;
      this.ClientSize = new System.Drawing.Size(254, 206);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.HelpButton = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DialogInvokeActionForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Info required";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
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
	}
}
