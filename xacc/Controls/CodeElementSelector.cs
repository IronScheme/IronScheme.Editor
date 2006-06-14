using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Xacc.Controls
{
	/// <summary>
	/// Summary description for CodeElementSelector.
	/// </summary>
	public class CodeElementSelector : System.Windows.Forms.UserControl
	{
    private PictureComboBox comboBox1;
    private PictureComboBox comboBox2;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CodeElementSelector()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

    protected override void OnResize(EventArgs e)
    {
      comboBox2.Left = comboBox2.Width = comboBox1.Width = Width/2;
      base.OnResize (e);
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.comboBox1 = new PictureComboBox();
      this.comboBox2 = new PictureComboBox();
      this.SuspendLayout();
      // 
      // comboBox1
      // 
      this.comboBox1.Dock = System.Windows.Forms.DockStyle.Left;
      this.comboBox1.Location = new System.Drawing.Point(0, 0);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(300, 21);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.Text = string.Empty;
      // 
      // comboBox2
      // 
      this.comboBox2.Dock = System.Windows.Forms.DockStyle.Right;
      this.comboBox2.Location = new System.Drawing.Point(300, 0);
      this.comboBox2.Name = "comboBox2";
      this.comboBox2.Size = new System.Drawing.Size(300, 21);
      this.comboBox2.TabIndex = 1;
      this.comboBox2.Text = string.Empty;
      // 
      // CodeElementSelector
      // 
      this.Controls.Add(this.comboBox2);
      this.Controls.Add(this.comboBox1);
      this.Name = "CodeElementSelector";
      this.Size = new System.Drawing.Size(600, 22);
      this.ResumeLayout(false);

    }
		#endregion
	}
}
