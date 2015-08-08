#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using System;
using System.Collections;
using System.Drawing;
using IronScheme.Editor.ComponentModel;
using System.Windows.Forms;


#endregion


namespace IronScheme.Editor.Controls
{
  class AutoCompleteForm : Form
  {
    internal ListBox choices = new PictureListBox();
    
    internal string lasthint = string.Empty;
    internal Type[] filters;
    internal string lastguess = string.Empty;

    static string Join(Type[] t)
    {
      string[] s = new string[t.Length];
      for (int i = 0; i < s.Length; i++)
      {
        s[i] = t[i].Name;
      }
      return string.Join(", ", s);
    }

    class CodeElementComparer: IComparer
    {
      public int Compare(object x, object y)
      {
        CodeModel.CodeElement a = x as CodeModel.CodeElement;
        CodeModel.CodeElement b = y as CodeModel.CodeElement;
        return a.Name.CompareTo(b.Name);
      }
    }

    static IComparer CODEELEMCOMPARER = new CodeElementComparer();

    public bool Show(Point location, string hint, CodeModel.ICodeElement[] hints, int fontheight, Type[] filters)
    {
      SuspendLayout();

      if (this.filters == null)
      {
        this.filters = filters;
      }

      DialogResult = DialogResult.Cancel;
      choices.Items.Clear();

      lasthint = hint;

      //System.Diagnostics.Trace.WriteLine(Join(this.filters),     "Filters       ");      

      IImageListProviderService ims = ServiceHost.ImageListProvider;

      ArrayList all = new ArrayList();

      foreach (CodeModel.ICodeElement s in hints)
      {
        if (Languages.Language.FilterType(this.filters, s))
        {
          all.Add(s);
        }
      }

      all.Sort(CODEELEMCOMPARER);
      choices.Items.AddRange(all.ToArray());

      if (choices.Items.Count == 0)
      {
        ResumeLayout();
        if (Visible)
        {
          Hide();
          
          return true;
        }
        else
        {
          DialogResult = DialogResult.No;  
          return false;
        }
      }

      choices.SelectedIndex = 0;

      if (choices.Items.Count == 1)
      {
        if (this.filters == filters || lastguess == ((CodeModel.ICodeElement) (choices.SelectedItem)).Fullname)
        {
          ResumeLayout();
          DialogResult = DialogResult.OK;
          if (Visible)
          {
            Hide();
            return true;
          }
          else
          {
            return false;
          }
        }

        lastguess = ((CodeModel.ICodeElement) (choices.SelectedItem)).Fullname;
      }

      int diff = Height - ClientSize.Height;

      if (choices.Items.Count < 12)
      {
        Height = choices.ItemHeight * choices.Items.Count + diff + 2;
      }
      else
      {
        Height = choices.ItemHeight * 12 + diff + 2;
      }

      Screen ss = Screen.FromPoint(location);

      //x

      if (location.X + Width > ss.WorkingArea.Width)
      {
        location.X = ss.WorkingArea.Width - Width;
      }

      //y

      if (location.Y + Height > ss.WorkingArea.Bottom)
      {
        location.Y = location.Y - fontheight - Height;
      }

      Location = location;

      ResumeLayout();

      if (!Visible)
      {
        Show();
      }
      return true;
    }

    public AutoCompleteForm(AdvancedTextBox parent)
    {
      Opacity = .9;
      DoubleBuffered = true;
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.ControlBox = false;
      choices.BorderStyle = BorderStyle.FixedSingle;
      choices.KeyDown += new KeyEventHandler(choices_KeyDown);
      choices.DoubleClick +=new EventHandler(choices_DoubleClick);
      choices.Font = SystemInformation.MenuFont;
      choices.Dock = DockStyle.Fill;
      choices.IntegralHeight = false;

      Controls.Add(choices);
      Width = 250;
      
      this.ShowInTaskbar = false;
      this.SizeGripStyle = SizeGripStyle.Hide;
      this.StartPosition = FormStartPosition.Manual;
    }

    void choices_KeyDown(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Enter:
          DialogResult = DialogResult.OK;
          Hide();
          break;
        case Keys.Escape:
          DialogResult = DialogResult.Cancel;
          Hide();
          break;
      }
    }

    void choices_DoubleClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Hide();
    }
  }
}
