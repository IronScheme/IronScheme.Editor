using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using Xacc.Controls;

namespace Xacc.LanguageDesigner
{

	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
    private Xacc.Controls.AdvancedTextBox advancedTextBox1;
    private System.Windows.Forms.MainMenu mainMenu1;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem menuItem4;
    private System.Windows.Forms.MenuItem menuItem5;
    private System.Windows.Forms.MenuItem menuItem6;
    private System.Windows.Forms.MenuItem menuItem7;
    private System.Windows.Forms.MenuItem menuItem8;
    private System.Windows.Forms.MenuItem menuItem9;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.MenuItem menuItem10;
    private Xacc.Controls.AdvancedTextBox advancedTextBox2;
    private System.Windows.Forms.PropertyGrid propertyGrid2;
    private System.Windows.Forms.PropertyGrid propertyGrid1;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
      propertyGrid1.SelectedObject = new Template();
      advancedTextBox1.Invalidated +=new InvalidateEventHandler(advancedTextBox1_Invalidated);
      propertyGrid2.SelectedGridItemChanged +=new SelectedGridItemChangedEventHandler(propertyGrid2_SelectedGridItemChanged);
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
				if (components != null) 
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
      this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
      this.advancedTextBox1 = new Xacc.Controls.AdvancedTextBox();
      this.mainMenu1 = new System.Windows.Forms.MainMenu();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItem6 = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.menuItem7 = new System.Windows.Forms.MenuItem();
      this.menuItem8 = new System.Windows.Forms.MenuItem();
      this.menuItem5 = new System.Windows.Forms.MenuItem();
      this.menuItem4 = new System.Windows.Forms.MenuItem();
      this.menuItem10 = new System.Windows.Forms.MenuItem();
      this.menuItem9 = new System.Windows.Forms.MenuItem();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.advancedTextBox2 = new Xacc.Controls.AdvancedTextBox();
      this.propertyGrid2 = new System.Windows.Forms.PropertyGrid();
      this.SuspendLayout();
      // 
      // propertyGrid1
      // 
      this.propertyGrid1.CommandsVisibleIfAvailable = true;
      this.propertyGrid1.Cursor = System.Windows.Forms.Cursors.HSplit;
      this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Left;
      this.propertyGrid1.LargeButtons = false;
      this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
      this.propertyGrid1.Name = "propertyGrid1";
      this.propertyGrid1.Size = new System.Drawing.Size(288, 596);
      this.propertyGrid1.TabIndex = 0;
      this.propertyGrid1.Text = "propertyGrid1";
      this.propertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
      this.propertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
      // 
      // advancedTextBox1
      // 
      this.advancedTextBox1.AutoSave = false;
      this.advancedTextBox1.BackColor = System.Drawing.Color.White;
      this.advancedTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.advancedTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.advancedTextBox1.EditorLanguage = "Plain Text";
      this.advancedTextBox1.Location = new System.Drawing.Point(288, 0);
      this.advancedTextBox1.Name = "advancedTextBox1";
      this.advancedTextBox1.ReadOnly = false;
      this.advancedTextBox1.SelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(62)), ((System.Byte)(49)), ((System.Byte)(106)), ((System.Byte)(197)));
      this.advancedTextBox1.ShowFoldbar = false;
      this.advancedTextBox1.ShowLineNumbers = true;
      this.advancedTextBox1.Size = new System.Drawing.Size(560, 332);
      this.advancedTextBox1.TabIndex = 1;
      this.advancedTextBox1.TabsToSpaces = false;
      this.advancedTextBox1.Text = "enter or paste sample text here";
      // 
      // mainMenu1
      // 
      this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.menuItem1});
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 0;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.menuItem6,
                                                                              this.menuItem2,
                                                                              this.menuItem7,
                                                                              this.menuItem8,
                                                                              this.menuItem5,
                                                                              this.menuItem4,
                                                                              this.menuItem10,
                                                                              this.menuItem9,
                                                                              this.menuItem3});
      this.menuItem1.Text = "File";
      // 
      // menuItem6
      // 
      this.menuItem6.Index = 0;
      this.menuItem6.Text = "New";
      this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 1;
      this.menuItem2.Text = "Open";
      this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
      // 
      // menuItem7
      // 
      this.menuItem7.Index = 2;
      this.menuItem7.Text = "Save";
      this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
      // 
      // menuItem8
      // 
      this.menuItem8.Index = 3;
      this.menuItem8.Text = "-";
      // 
      // menuItem5
      // 
      this.menuItem5.Index = 4;
      this.menuItem5.Text = "Generate";
      this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
      // 
      // menuItem4
      // 
      this.menuItem4.Index = 5;
      this.menuItem4.Text = "Compile";
      this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
      // 
      // menuItem10
      // 
      this.menuItem10.Index = 6;
      this.menuItem10.Text = "Install";
      this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
      // 
      // menuItem9
      // 
      this.menuItem9.Index = 7;
      this.menuItem9.Text = "-";
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 8;
      this.menuItem3.Text = "Exit";
      this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.Filter = "Language files|*.lang";
      // 
      // advancedTextBox2
      // 
      this.advancedTextBox2.AutoSave = false;
      this.advancedTextBox2.BackColor = System.Drawing.Color.White;
      this.advancedTextBox2.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.advancedTextBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.advancedTextBox2.EditorLanguage = "CS Lex";
      this.advancedTextBox2.Location = new System.Drawing.Point(288, 332);
      this.advancedTextBox2.Name = "advancedTextBox2";
      this.advancedTextBox2.ReadOnly = true;
      this.advancedTextBox2.SelectionColor = System.Drawing.Color.FromArgb(((System.Byte)(62)), ((System.Byte)(49)), ((System.Byte)(106)), ((System.Byte)(197)));
      this.advancedTextBox2.ShowFoldbar = false;
      this.advancedTextBox2.ShowLineNumbers = true;
      this.advancedTextBox2.Size = new System.Drawing.Size(800, 264);
      this.advancedTextBox2.TabIndex = 2;
      this.advancedTextBox2.TabsToSpaces = false;
      // 
      // propertyGrid2
      // 
      this.propertyGrid2.Dock = System.Windows.Forms.DockStyle.Right;
      this.propertyGrid2.HelpVisible = false;
      this.propertyGrid2.LargeButtons = false;
      this.propertyGrid2.LineColor = System.Drawing.SystemColors.ScrollBar;
      this.propertyGrid2.Location = new System.Drawing.Point(848, 0);
      this.propertyGrid2.Name = "propertyGrid2";
      this.propertyGrid2.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
      this.propertyGrid2.Size = new System.Drawing.Size(240, 332);
      this.propertyGrid2.TabIndex = 4;
      this.propertyGrid2.Text = "propertyGrid2";
      this.propertyGrid2.ToolbarVisible = false;
      this.propertyGrid2.ViewBackColor = System.Drawing.SystemColors.Window;
      this.propertyGrid2.ViewForeColor = System.Drawing.SystemColors.WindowText;
      // 
      // Form1
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(1088, 596);
      this.Controls.Add(this.advancedTextBox1);
      this.Controls.Add(this.propertyGrid2);
      this.Controls.Add(this.advancedTextBox2);
      this.Controls.Add(this.propertyGrid1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.Menu = this.mainMenu1;
      this.Name = "Form1";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Language Designer";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
      Application.EnableVisualStyles();
      Application.DoEvents();
			Application.Run(new Form1());
		}

    private void menuItem3_Click(object sender, System.EventArgs e)
    {
      Application.Exit();
    }

    private void menuItem4_Click(object sender, System.EventArgs e)
    {
      Template t = propertyGrid1.SelectedObject as Template;
      
      Languages.Language l = t.Compile();
      advancedTextBox1.Buffer.Language = l;
      advancedTextBox2.Text = t.generated;
    }

    private void menuItem5_Click(object sender, System.EventArgs e)
    {
      Template t = propertyGrid1.SelectedObject as Template;
      
      t.Generate();

      advancedTextBox2.Text = t.generated;
    }

    private void menuItem7_Click(object sender, System.EventArgs e)
    {
      //save
      Template t = propertyGrid1.SelectedObject as Template;
      t.Save(advancedTextBox1.Text);
    }

    private void menuItem2_Click(object sender, System.EventArgs e)
    {
      //open
      if (DialogResult.OK == openFileDialog1.ShowDialog(this))
      {
        Template t = Template.Open(openFileDialog1.FileName);
        propertyGrid1.SelectedObject = t;
        advancedTextBox1.Buffer.Language = t.Compile();
        advancedTextBox1.Text = t.sampletext;
        advancedTextBox2.Text = t.generated;
      }
    }

    private void menuItem6_Click(object sender, System.EventArgs e)
    {
      //new
      Template t =  new Template();
      propertyGrid1.SelectedObject = t;
      advancedTextBox1.Buffer.Language = t.Compile();
      advancedTextBox2.Text = t.generated;
    }

    private void menuItem10_Click(object sender, System.EventArgs e)
    {
      //install
      Template t = propertyGrid1.SelectedObject as Template;
      
      Languages.Language l = t.Install();
      advancedTextBox1.Buffer.Language = l;
      advancedTextBox2.Text = t.generated;
      
    }

    bool ignore = false;

    private void advancedTextBox1_Invalidated(object sender, InvalidateEventArgs e)
    {
      if (!ignore)
      {
        try
        {
          ArrayList val = new ArrayList();
          foreach (ComponentModel.IToken t in advancedTextBox1.Buffer.GetTokens(advancedTextBox1.Buffer.CurrentLine))
          {
            val.Add(t.Class.ToString() + " (" + t.Location.Column + "," + (t.Location.EndColumn - 1) + ")");
          }
          propertyGrid2.SelectedObject = val.ToArray(typeof(string)) ;
        
        }
        catch{}
      }
      ignore = false;
    }

    private void propertyGrid2_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
    {
      if (e.NewSelection != e.OldSelection && e.OldSelection != null)
      {
        string lbl = e.NewSelection.Label;
        lbl = lbl.Trim('[',']');
        int index = Convert.ToInt32(lbl);
        ComponentModel.IToken t = advancedTextBox1.Buffer.GetTokens(advancedTextBox1.Buffer.CurrentLine)[index];
        advancedTextBox1.Buffer.SelectLocation(t.Location);
        ignore = true;
        advancedTextBox1.Invalidate();
      }
    }
  }
}
