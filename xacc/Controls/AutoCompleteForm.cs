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
using Xacc.Collections;

using SR = System.Resources;
using ST = System.Threading;


#endregion


namespace Xacc.Controls
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

      Height = 0;
      DialogResult = DialogResult.Cancel;
      choices.Items.Clear();

      lasthint = hint;

      System.Diagnostics.Trace.WriteLine(Join(this.filters),     "Filters       ");      

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

      if (choices.Items.Count < 12)
      {
        Height = choices.ItemHeight * choices.Items.Count + 2;
      }
      else
      {
        Height = choices.ItemHeight * 12 + 2;
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
      FormBorderStyle = FormBorderStyle.None;
      this.ControlBox = false;
      choices.BorderStyle = BorderStyle.FixedSingle;
      choices.KeyDown += new KeyEventHandler(choices_KeyDown);
      choices.DoubleClick +=new EventHandler(choices_DoubleClick);
      choices.Font = SystemInformation.MenuFont;
      choices.Dock = DockStyle.Fill;
      Controls.Add(choices);
      Width = 200;
      
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
