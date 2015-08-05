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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace IronScheme.Editor.Controls
{
	/// <summary>
	/// Summary description for GenericTypeEditor.
	/// </summary>
	class GenericTypeEditor : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
    private System.Windows.Forms.GroupBox groupBox1;
    private IronScheme.Editor.Controls.TypeEditorTextBox textBox1;

    ParameterInfo pinfo;

    public object Value
    {
      get {return textBox1.Value;}
      set {textBox1.Value = value;}
    }

    public ParameterInfo Info
    {
      get {return pinfo;}
      set 
      {
        if (pinfo != value)
        {
          pinfo = value;
          if (value != null)
          {
            groupBox1.Text = value.Name + " : " + value.ParameterType.Name;
            textBox1.Type = value.ParameterType;
            textBox1.Attributes = value.GetCustomAttributes(true);
          }
        }
      }
    }

    //fucku designer
		internal GenericTypeEditor()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.textBox1 = new IronScheme.Editor.Controls.TypeEditorTextBox();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.BackColor = System.Drawing.SystemColors.Window;
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(264, 40);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // textBox1
      // 
      this.textBox1.BackColor = System.Drawing.SystemColors.Window;
      this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox1.Location = new System.Drawing.Point(3, 16);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(258, 23);
      this.textBox1.TabIndex = 1;
      // 
      // GenericTypeEditor
      // 
      this.Controls.Add(this.groupBox1);
      this.groupBox1.Controls.Add(this.textBox1);
      this.Name = "GenericTypeEditor";
      this.Size = new System.Drawing.Size(264, 40);
      this.ResumeLayout(false);

    }
		#endregion



	}
}
