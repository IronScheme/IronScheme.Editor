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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;

using SR = System.Resources;
#endregion

namespace Xacc.Controls
{
	class FindDialog : Form
	{
		AdvancedTextBox atb;
		internal string lastfind = string.Empty;
		Label label1;
		ComboBox comboBox1;
		CheckBox checkBox1;
		CheckBox checkBox2;
		CheckBox checkBox3;
		CheckBox checkBox4;
		GroupBox groupBox1;
		RadioButton radioButton1;
		RadioButton radioButton2;
		Button button1;
		Button button2;
		Button button3;
		ComboBox comboBox2;
		Label label2;
		Button button5;
		RadioButton radioButton3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FindDialog(AdvancedTextBox atb) : this()
		{
			this.atb = atb;
		}

		public FindDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			Font = new Font(SystemInformation.MenuFont.FontFamily, 8.25f);
			InitializeComponent();

			checkBox1.CheckedChanged +=new EventHandler(checkBox1_CheckedChanged);
			checkBox2.CheckedChanged +=new EventHandler(checkBox1_CheckedChanged);
			checkBox3.CheckedChanged +=new EventHandler(checkBox1_CheckedChanged);
			checkBox4.CheckedChanged +=new EventHandler(checkBox4_CheckedChanged);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
			base.OnClosing (e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible)
			{
				if (lastfind == string.Empty)
				{
					comboBox1.Text = atb.SelectionText;
				}
				button1.Enabled = comboBox1.Text != string.Empty;
				button2.Enabled = false;
				button5.Enabled = false;
			}
			base.OnVisibleChanged (e);
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
      this.label1 = new System.Windows.Forms.Label();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.checkBox3 = new System.Windows.Forms.CheckBox();
      this.checkBox4 = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.comboBox2 = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.button5 = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label1.Location = new System.Drawing.Point(5, 5);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(72, 16);
      this.label1.TabIndex = 11;
      this.label1.Text = "Find what:";
      // 
      // comboBox1
      // 
      this.comboBox1.Items.AddRange(new object[] {
            "public",
            "get",
            "Entity",
            "ess"});
      this.comboBox1.Location = new System.Drawing.Point(85, 2);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(211, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.Text = "public";
      this.comboBox1.TextChanged += new System.EventHandler(this.comboBox1_TextChanged);
      // 
      // checkBox1
      // 
      this.checkBox1.Checked = true;
      this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox1.Location = new System.Drawing.Point(6, 48);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(128, 22);
      this.checkBox1.TabIndex = 2;
      this.checkBox1.Text = "Match case";
      // 
      // checkBox2
      // 
      this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox2.Location = new System.Drawing.Point(6, 67);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(128, 22);
      this.checkBox2.TabIndex = 3;
      this.checkBox2.Text = "Match token";
      // 
      // checkBox3
      // 
      this.checkBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox3.Location = new System.Drawing.Point(6, 86);
      this.checkBox3.Name = "checkBox3";
      this.checkBox3.Size = new System.Drawing.Size(128, 22);
      this.checkBox3.TabIndex = 4;
      this.checkBox3.Text = "Search Up";
      // 
      // checkBox4
      // 
      this.checkBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox4.Location = new System.Drawing.Point(6, 105);
      this.checkBox4.Name = "checkBox4";
      this.checkBox4.Size = new System.Drawing.Size(128, 22);
      this.checkBox4.TabIndex = 5;
      this.checkBox4.Text = "Regular Expression";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.radioButton3);
      this.groupBox1.Controls.Add(this.radioButton2);
      this.groupBox1.Controls.Add(this.radioButton1);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(131, 48);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(162, 76);
      this.groupBox1.TabIndex = 6;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Search";
      // 
      // radioButton3
      // 
      this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton3.Location = new System.Drawing.Point(4, 56);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(104, 16);
      this.radioButton3.TabIndex = 2;
      this.radioButton3.Text = "All Documents";
      // 
      // radioButton2
      // 
      this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton2.Location = new System.Drawing.Point(4, 36);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(104, 16);
      this.radioButton2.TabIndex = 1;
      this.radioButton2.Text = "Selection";
      // 
      // radioButton1
      // 
      this.radioButton1.Checked = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton1.Location = new System.Drawing.Point(4, 16);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(128, 16);
      this.radioButton1.TabIndex = 0;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "Current Document";
      // 
      // button1
      // 
      this.button1.BackColor = System.Drawing.SystemColors.Control;
      this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button1.Location = new System.Drawing.Point(301, 2);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 26);
      this.button1.TabIndex = 7;
      this.button1.Text = "Find next";
      this.button1.UseVisualStyleBackColor = false;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button2.Location = new System.Drawing.Point(301, 34);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 26);
      this.button2.TabIndex = 8;
      this.button2.Text = "Replace";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button3.Location = new System.Drawing.Point(301, 98);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 26);
      this.button3.TabIndex = 10;
      this.button3.Text = "Close";
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // comboBox2
      // 
      this.comboBox2.Location = new System.Drawing.Point(85, 26);
      this.comboBox2.Name = "comboBox2";
      this.comboBox2.Size = new System.Drawing.Size(211, 21);
      this.comboBox2.TabIndex = 1;
      this.comboBox2.TextChanged += new System.EventHandler(this.comboBox2_TextChanged);
      // 
      // label2
      // 
      this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2.Location = new System.Drawing.Point(5, 30);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 16);
      this.label2.TabIndex = 10;
      this.label2.Text = "Replace with:";
      // 
      // button5
      // 
      this.button5.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.button5.Location = new System.Drawing.Point(301, 66);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(75, 26);
      this.button5.TabIndex = 9;
      this.button5.Text = "Replace all";
      // 
      // FindDialog
      // 
      this.AcceptButton = this.button1;
      this.CancelButton = this.button3;
      this.ClientSize = new System.Drawing.Size(380, 127);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.comboBox2);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.checkBox4);
      this.Controls.Add(this.checkBox3);
      this.Controls.Add(this.checkBox2);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FindDialog";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Find";
      this.TopMost = true;
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

		}
		#endregion

		private void button3_Click(object sender, System.EventArgs e)
		{
			Hide();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			button1.BackColor = SystemColors.Control;
			RichTextBoxFinds options = 0;
			int end = atb.TextLength - 1;
			int start = atb.SelectionStart + atb.SelectionLength;
			if (checkBox1.Checked)
			{
				options |= RichTextBoxFinds.MatchCase;
			}
			if(checkBox2.Checked)
			{
				options |= RichTextBoxFinds.WholeWord;
			}
			if(checkBox3.Checked)
			{
				options |= RichTextBoxFinds.Reverse;
				end = start - 1;
				start = 0;
			}
			if(checkBox4.Checked)
			{
				options |= RichTextBoxFinds.NoHighlight;
			}

			int pos;
			string pat = lastfind = comboBox1.Text;
				
			pos = atb.Find(pat, start, end, options);
			if (pos != -1)
			{
				atb.Select(pos, pat.Length);
				atb.ScrollToCaret();
			}
			else
			{
				button1.Enabled = false;
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if (atb.SelectionLength > 0)
			{
				atb.SelectionText = comboBox2.Text;
				atb.Invalidate();
			}
		}

		private void comboBox1_TextChanged(object sender, EventArgs e)
		{
			button1.Enabled = comboBox1.Text != string.Empty;
		}

		private void comboBox2_TextChanged(object sender, EventArgs e)
		{
			button5.Enabled = button2.Enabled = comboBox2.Text != string.Empty;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			button1.Enabled = comboBox1.Text != string.Empty;
		}

		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			button1.Enabled = comboBox1.Text != string.Empty;

			if (checkBox4.Checked)
			{
				checkBox1.Enabled = false;
				checkBox2.Enabled = false;
			}
			else
			{
				checkBox1.Enabled = true;
				checkBox2.Enabled = true;
			}
		}
	}
}
