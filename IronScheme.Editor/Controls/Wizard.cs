#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using IronScheme.Editor.ComponentModel;
using IronScheme.Editor.Build;
using System.IO;


namespace IronScheme.Editor.Controls
{
  class Wizard : Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;

		internal PictureComboBox prjtype = new PictureComboBox();

		const string DBLCLICKFORFOLDER = "double click to browse for folder";

		TextBox name;
		TextBox loc;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Wizard()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Font = SystemInformation.MenuFont;
			Size = new Size(312, 170);

			button3.Enabled = false;
			button2.Text = "Create";

			Text = "Create new project...";

			Label p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,2);
			p.Size = new Size(100, 32);
			p.Text = "Project type";

			Controls.Add(p);

			prjtype.Location = new Point(102,2);
			prjtype.Width = 200;
			Controls.Add(prjtype);

			p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,37);
			p.Size = new Size(100, 30);
			p.Text = "Name";

			Controls.Add(p);

			p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,66);
			p.Size = new Size(100, 30);
			p.Text = "Location";

			Controls.Add(p);


			name = new TextBox();
			name.Location = new Point(102,37);
			name.Width = 200;

			Controls.Add(name);

			loc = new TextBox();
			loc.Location = new Point(102,66);
			loc.Width = 200;
			loc.Text = DBLCLICKFORFOLDER;
			loc.ReadOnly = true;

			loc.DoubleClick +=new EventHandler(loc_DoubleClick);

			Controls.Add(loc);

			button2.Click +=new EventHandler(button2_Click);
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
			this.button3 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(415, 327);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 24);
			this.button1.TabIndex = 0;
			this.button1.Text = "Cancel";
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(319, 327);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(80, 24);
			this.button2.TabIndex = 1;
			this.button2.Text = "Next";
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button3.Location = new System.Drawing.Point(223, 327);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(80, 24);
			this.button3.TabIndex = 2;
			this.button3.Text = "Back";
			// 
			// Wizard
			// 
			this.AcceptButton = this.button2;
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(511, 359);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Wizard";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Wizard";
			this.ResumeLayout(false);

		}
		#endregion

		void loc_DoubleClick(object sender, EventArgs e)
		{
			loc.ReadOnly = false;
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = "Select the location to create the new project";
			fbd.SelectedPath = Environment.CurrentDirectory;
			if (fbd.ShowDialog(this) == DialogResult.OK)
			{
				 loc.Text = fbd.SelectedPath;
			}
		}

		void button2_Click(object sender, EventArgs e)
		{
			if (loc.Text.Trim() == string.Empty || name.Text.Trim() == string.Empty || prjtype.SelectedItem == null)
			{
				return;
			}

      if (loc.Text == DBLCLICKFORFOLDER)
      {
        loc.Text = Environment.CurrentDirectory;
      }

      if (!Directory.Exists(loc.Text))
      {
        if(DialogResult.OK == MessageBox.Show(ServiceHost.Window.MainForm, "Directory does not exist, do you want to create it?", 
          "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
        {
          Directory.CreateDirectory(loc.Text);
        }
        else
        {
          return;
        }
      }

			Project p = ServiceHost.Project.Create(prjtype.SelectedItem as Type, name.Text, loc.Text);

			if (loc.Text == DBLCLICKFORFOLDER)
			{
				p.RootDirectory = Environment.CurrentDirectory;
			}
			else
			{
				p.RootDirectory = loc.Text;
			}
			p.ProjectName = name.Text;
			p.Location = p.RootDirectory + System.IO.Path.DirectorySeparatorChar + name.Text + ".xacc";

      Tag = p;

			DialogResult = DialogResult.OK;
			Close();
		}
	}

	class NewFileWizard : Form
	{
		Button button1;
		Button button2;
		Button button3;

		internal PictureComboBox prjtype = new PictureComboBox();

    const string DBLCLICKFORFOLDER = "double click to browse for folder";

		internal TextBox name;
		internal TextBox loc;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		Container components = null;

		public NewFileWizard()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Font = SystemInformation.MenuFont;
			Size = new Size(312, 200);

			button3.Enabled = false;
			button2.Text = "Create";

			Text = "Create new file...";

			prjtype.Location = new Point(102,2);
			prjtype.Width = 200;
			Controls.Add(prjtype);

			Label p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,2);
			p.Size = new Size(100, 30);
			p.Text = "File type";

			Controls.Add(p);

			p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,32);
			p.Size = new Size(100, 30);
			p.Text = "Name";

			Controls.Add(p);

			p = new Label();
			p.TextAlign = ContentAlignment.MiddleLeft;
			p.Location = new Point(2,62);
			p.Size = new Size(100, 30);
			p.Text = "Location";

			Controls.Add(p);


			name = new TextBox();
			name.Location = new Point(102,32);
			name.Width = 200;
      

			Controls.Add(name);

			loc = new TextBox();
			loc.Location = new Point(102,62);
			loc.Width = 200;
      loc.Text = DBLCLICKFORFOLDER;

			loc.DoubleClick +=new EventHandler(loc_DoubleClick);

			Controls.Add(loc);
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
			this.button3 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Location = new System.Drawing.Point(415, 327);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 24);
			this.button1.TabIndex = 0;
			this.button1.Text = "Cancel";
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button2.Location = new System.Drawing.Point(319, 327);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(80, 24);
			this.button2.TabIndex = 1;
			this.button2.Text = "Next";
			this.button2.Click +=new EventHandler(button2_Click);
			// 
			// button3
			// 
			this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button3.Location = new System.Drawing.Point(223, 327);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(80, 24);
			this.button3.TabIndex = 2;
			this.button3.Text = "Back";
			// 
			// Wizard
			// 
			this.AcceptButton = this.button2;
			this.CancelButton = this.button1;
			this.ClientSize = new System.Drawing.Size(511, 359);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Wizard";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Wizard";
			this.ResumeLayout(false);

		}
		#endregion

		void loc_DoubleClick(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = "Select the location to create the new project";
			fbd.SelectedPath = Environment.CurrentDirectory;
			if (fbd.ShowDialog(this) == DialogResult.OK)
			{
				loc.Text = fbd.SelectedPath;
			}
		}

		void button2_Click(object sender, EventArgs e)
		{
      if (name.Text == null || name.Text == string.Empty || prjtype.SelectedItem == null)
      {
        return;
      }

      if (loc.Text == DBLCLICKFORFOLDER)
      {
        loc.Text = Environment.CurrentDirectory;
      }

      if (!Directory.Exists(loc.Text))
      {
        if(DialogResult.OK == MessageBox.Show(ServiceHost.Window.MainForm, "Directory does not exist, do you want to create it?", 
          "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
        {
          Directory.CreateDirectory(loc.Text);
        }
        else
        {
          return;
        }
      }

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
