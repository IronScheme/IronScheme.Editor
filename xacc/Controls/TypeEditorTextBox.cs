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
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;

namespace Xacc.Controls
{
	/// <summary>
	/// Summary description for TypeEditorTextBox.
	/// </summary>
  class TypeEditorTextBox : Control, IServiceProvider
  {
    TextBox editor = new TextBox();
    object _value = null;
    StringFormat sf = StringFormat.GenericDefault;
    UITypeEditor uieditor;
    TypeConverter typeconv;
    Type type;
    ErrorProvider error;
    object[] attributes;

    public TypeEditorTextBox()
    {
      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
        ControlStyles.Selectable | ControlStyles.StandardClick |
        ControlStyles.ResizeRedraw | ControlStyles.StandardDoubleClick, true);
      editor.Dock = DockStyle.Fill;
      editor.AutoSize = false;
      editor.Visible = false;
      editor.AcceptsReturn = true;
      editor.AcceptsTab = true;
      editor.KeyDown +=new KeyEventHandler(editor_KeyDown);
      Controls.Add(editor);
      sf.LineAlignment = StringAlignment.Center;
      sf.Trimming = StringTrimming.EllipsisCharacter;
      sf.FormatFlags |= StringFormatFlags.NoWrap;
      BackColor = SystemColors.Window;
      error = new ErrorProvider();
    }

    public bool IsEditing
    {
      get {return editor.Visible;}
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object[] Attributes
    {
      get {return attributes;}
      set 
      {
        attributes = value;
        foreach (Attribute at in value)
        {
          if (at is EditorAttribute)
          {
            Type t = Type.GetType(((EditorAttribute)at).EditorTypeName);
            uieditor = Activator.CreateInstance(t) as UITypeEditor;
            break;
          }
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Type Type
    {
      get {return type;}
      set 
      {
        if (type != value)
        {
          type = value;
          if (value != null)
          {
            typeconv = TypeDescriptor.GetConverter(value);
            uieditor = TypeDescriptor.GetEditor(value, typeof(UITypeEditor)) as UITypeEditor;
          }
        }
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object Value
    {
      get {return _value;}
      set 
      {
        if (_value != value)
        {
          _value = value;
          if (value != null)
          {
            Type = value.GetType();
          }
        }
      }
    }

    protected override void OnLostFocus(EventArgs e)
    {
      Invalidate();
      base.OnLostFocus (e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
      Invalidate();
      base.OnGotFocus (e);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
      pevent.Graphics.Clear(BackColor);
      base.OnPaintBackground (pevent);
    }

    protected override void OnClick(EventArgs e)
    {
      Focus();
      base.OnClick (e);
    }

    protected override void OnDoubleClick(EventArgs e)
    {
      if (uieditor != null)
      {
        Value = uieditor.EditValue(this, Value);
      }
      else
      {
        editor.Visible = true;
        editor.Focus();
      }
      Invalidate();
      base.OnDoubleClick (e);
    }

    object IServiceProvider.GetService(Type serviceType)
    {
      if (serviceType == typeof(System.Windows.Forms.Design.IWindowsFormsEditorService))
      {
        return new UIEditorForm(this);
      }
      return null;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.F2:
          //Value = uieditor.EditValue(this, Value);
          editor.Text = typeconv.ConvertToString(Value);
          editor.Visible = true;
          editor.Focus();
          editor.LostFocus+=new EventHandler(editor_LostFocus);
          e.Handled = true;
          break;
      }
      base.OnKeyDown (e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (!IsEditing)
      {
        Graphics g = e.Graphics;
        Rectangle r = ClientRectangle;
        r.Inflate(-2,-2);
        int rh = r.Height;
        if (rh < 19)
        {
          rh = 19;
        }
        Rectangle rr = new Rectangle(r.X, r.Y, r.Width - rh, r.Height);
        Rectangle rrr = new Rectangle(r.X + rr.Width, r.Y, rh, r.Height);

        //do the custom rendering process here, albeit call it
        if (typeconv.CanConvertTo(typeof(double)))
        {
          sf.Alignment = StringAlignment.Far;
        }
        else
        {
          sf.Alignment = StringAlignment.Near;
        }

        if (Focused)
        {
          Rectangle cr = ClientRectangle;
          Drawing.Utils.PaintLineHighlight(null, SystemPens.Highlight, g, r.X-1, r.Y-1, r.Width + 1, r.Height + 1, false);
        }

        if (Value == null)
        {
          sf.Alignment = StringAlignment.Center;
          g.DrawString("Press F2 or double-click to edit", Font, Drawing.Factory.SolidBrush(SystemColors.GrayText), r, sf);
        }
        else
        {
          if (uieditor != null && uieditor.GetPaintValueSupported())
          {
            uieditor.PaintValue(Value, g, rrr);
            g.DrawRectangle(SystemPens.WindowFrame, rrr);
            g.DrawString(typeconv.ConvertToString(Value), Font, SystemBrushes.ControlText, rr, sf);
          }
          else
          {
            g.DrawString(typeconv.ConvertToString(Value), Font, SystemBrushes.ControlText, r, sf);
          }
        }
      }
      base.OnPaint (e);
    }

    private void editor_KeyDown(object sender, KeyEventArgs e)
    {
      
      if (e.KeyCode == Keys.Enter)
      {
        try
        {
          Value = typeconv.ConvertFromString(editor.Text);
          editor.Visible = false;
          error.SetError(editor, "");
          Focus();
        }
        catch (Exception ex)
        {
          error.SetError(editor, ex.Message);
        }

        e.Handled = true;
      }
      if (e.KeyCode == Keys.Escape)
      {
        editor.Visible = false;
        e.Handled = true;
        error.SetError(editor, "");
        Focus();
      }
      if (e.Alt && e.KeyCode == Keys.Right)
      {
        object newval = Value;
        Value = uieditor.EditValue(this, newval);
        editor.Visible = false;
        error.SetError(editor, "");
        e.Handled = true;
        Focus();
        Invalidate();
      }
    }

    private void editor_LostFocus(object sender, EventArgs e)
    {
      editor.Visible = false;
    }
  }
}
