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
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace IronScheme.Editor.Controls
{
	/// <summary>
	/// Summary description for UIEditorForm.
	/// </summary>
  class UIEditorForm : System.Windows.Forms.Form, IWindowsFormsEditorService
  {
    private System.ComponentModel.Container components = null;
    Control uieditor;
    private System.Windows.Forms.Panel panel1;
    Control host;

    internal UIEditorForm() : this(null){}

    public UIEditorForm(Control host)
    {
      this.host = host;
      InitializeComponent();
      KeyPreview = true;
    }

    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams cp = base.CreateParams;
        cp.ExStyle |= 0x1;
        return cp;
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(180, 180);
      this.panel1.TabIndex = 0;
      // 
      // UIEditorForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(180, 180);
      this.ControlBox = false;
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "UIEditorForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "UIEditorForm";
      this.ResumeLayout(false);

    }
    #endregion

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        e.Handled = true;
        panel1.Controls.Remove(uieditor);
        uieditor = null;
        Invalidate(true);
        Close();
      }
      base.OnKeyDown (e);
    }

    protected override void OnClick(EventArgs e)
    {
      base.OnClick (e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown (e);
    }

    public void DropDownControl(Control uieditor)
    {
      this.uieditor = uieditor;
      //normally these editors are not font friendly, so keep it default, 
      //if they wanna change they can change from with in that control.
      //control.Font = g.Font;
      Rectangle rf = Rectangle.Empty;

      if (host is Grid)
      {
        rf = ((Grid)host).GetInitialCell();
      }
      else if (host is ListView)
      {
        ListViewItem lvi = ((ListView)host).FocusedItem;
        if (lvi != null)
        {
          rf = lvi.GetBounds(ItemBoundsPortion.ItemOnly);
        }
      }
      else 
      {
        rf.Y += host.Height;
        rf.Width = host.Width;
      }
				
      Location = host.PointToScreen(new Point((int)rf.X, (int)rf.Bottom));
      Width = rf.Width > uieditor.Width ? rf.Width : uieditor.Width;
      Height = uieditor.Height + (Height - panel1.ClientSize.Height);
				
      uieditor.Dock = DockStyle.Fill;
      panel1.Controls.Add(uieditor);
				
      DialogResult = DialogResult.Cancel;
      uieditor.Focus();
      Invalidate(true);
      DialogResult dr = ShowDialog(host.FindForm() as IWin32Window);

      System.Diagnostics.Trace.WriteLine("DropDownControl: " + dr);
    }

    public void CloseDropDown()
    {
      System.Diagnostics.Trace.WriteLine("CloseDropDown: " + DialogResult);
      DialogResult = DialogResult.OK;
      panel1.Controls.Remove(uieditor);
      uieditor = null;
      Invalidate(true);
      Close();
    }

    public DialogResult ShowDialog(Form dialog)
    {
      DialogResult res = dialog.ShowDialog(this);

      System.Diagnostics.Trace.WriteLine("ShowDialog: " + res);
      return res;
    }
  }
}
