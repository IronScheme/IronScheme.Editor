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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Data;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Xacc.ComponentModel;
using Xacc.Controls;

using IServiceProvider = System.IServiceProvider;

using LSharp;

using Runtime = LSharp.Runtime;

namespace Xacc.Controls.Design
{
  sealed class LSharpConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return sourceType == typeof(string);
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      return destinationType == typeof(string);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      string sv = value as string;
      if (value == null)
      {
        return null;
      }
      object o = LSharp.Runtime.ReadString(sv, TopLoop.Environment);

      return o;
    }

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        try
        {
          object o = LSharp.Runtime.Eval(value, (context.Instance as Grid).Environment);
          if (o == null)
          {
            return "<null>";
          }
          return Printer.WriteToString(o);
        }
        catch (Exception ex)
        {
          System.Diagnostics.Trace.WriteLine(ex);
          return "ERROR!";
        }
      }
      else if (destinationType == typeof(object))
      {
        try
        {
          object o = LSharp.Runtime.Eval(value, (context.Instance as Grid).Environment);
          return o;
        }
        catch (Exception ex)
        {
          System.Diagnostics.Trace.WriteLine(ex);
          return null;
        }
      }
      else
      {
        return value;
      }
    }
  }

  sealed class LSharpUIEditor : UITypeEditor
  {
    const RegexOptions REOPTS = RegexOptions.Compiled | RegexOptions.IgnoreCase;
    internal readonly static Hashtable conshistory = new Hashtable();
    readonly static Regex rangematch    = new Regex(@"[a-z]+\d+\.\.[a-z]+\d+", REOPTS);
    readonly static Regex rangematch2   = new Regex(@"([a-z]+\d+)(\s[a-z]+\d+)+", REOPTS);

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      IWindowsFormsEditorService iwes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

      AdvancedTextBox atb = new AdvancedTextBox();
      atb.normalmode = false;
      atb.Buffer.Language = ComponentModel.ServiceHost.Language["ls"];
      atb.ProjectHint = (ServiceHost.Scripting as ScriptingService).proj;

      string sv = conshistory[value] as string;
      if (sv == null)
      {
        if (value is Symbol && value == LSharp.Runtime.Eval(value, (context.Instance as Grid).Environment))
        {
        }
        else
        {
          sv = Printer.WriteToString(value);
        }
      }
      if (sv == null)
      {
        sv = string.Empty;
      }

      if (sv.Length > 0)
      {
        // convert ranges
        ArrayList reps = new ArrayList();

        foreach (Match m in rangematch2.Matches(sv))
        {
          string[] sr = m.Value.Split(' ');
          int l = sr.Length;
          if (sr.Length > 1)
          {
            Grid.Range r = new Grid.Range(sr[0] + ".." + sr[l - 1]);
            if (r.Count == l)
            {
              int i = 0;
              foreach (Grid.Cell c in r)
              {
                if (c.ToString().ToLower() != sr[i])
                {
                  if (i > 0)
                  {
                    r = new Grid.Range(sr[0] + ".." + sr[i - 1]);
                    sv = sv.Replace(string.Join(" ", sr, 0, i), r.ToString().ToLower());
                  }
                  goto NEXTMATCH;
                }
                i++;
              }
              sv = sv.Replace(m.Value, r.ToString().ToLower());
            }
          }
        NEXTMATCH:;
        }
      }

      atb.Buffer.InsertString(sv);

      atb.Width = 350;
      atb.Height = 200;

      // HACK HACK HACK HACK
      FileManager fm = ServiceHost.File as FileManager;
      //fm.buffers.Add("$hack$", atb);
      string ob = fm.current;
      fm.current = "$hack$";
      ApplicationState os = ServiceHost.State;
      ServiceHost.State |= (ApplicationState.Edit | ApplicationState.Buffer);
      ServiceHost.State &= ~(ApplicationState.Grid);

      iwes.DropDownControl(atb);
      fm.buffers.Remove("$hack$");
      ServiceHost.State = os;
      fm.current = ob;
      object o = null;
      string text = atb.Text.Trim();
      if (text.Length > 0)
      {
        // convert ranges
        ArrayList reps = new ArrayList();

        foreach (Match m in rangematch.Matches(text))
        {
          Grid.Range r = new Grid.Range(m.Value);
          ArrayList cs = new ArrayList();
          foreach (Grid.Cell c in r)
          {
            cs.Add(c.ToString());
          }
          string[] sr = cs.ToArray(typeof(string)) as string[];
          
          text = text.Replace(m.Value, string.Join(" ", sr));
        }

        if (text.StartsWith("="))
        {
          o = LSharp.Runtime.EvalString(text.TrimStart('='), (context.Instance as Grid).Environment);
        }
        else
        {
          o = LSharp.Runtime.ReadString(text, (context.Instance as Grid).Environment);
          conshistory[o] = atb.Text;
        }
      }
      atb.Dispose();
      return o;
    }

    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override void PaintValue(PaintValueEventArgs e)
    {
      base.PaintValue (e);
      e.Graphics.DrawString("L#", SystemInformation.MenuFont, SystemBrushes.WindowText, e.Bounds);
    }
  }

  sealed class EnumEditor : UITypeEditor
  {
    object value = null;
    IWindowsFormsEditorService iwes;
    ListBox lb;
    Type type;

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      iwes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

      type = value.GetType();

      lb = new ListBox();
      lb.IntegralHeight = false;
      lb.BorderStyle = BorderStyle.FixedSingle;
      lb.SelectedIndexChanged +=new EventHandler(lb_SelectedIndexChanged);
				
      foreach (object o in Enum.GetNames(type))
      {
        lb.Items.Add(o);
      }

      lb.Height = lb.ItemHeight * lb.Items.Count;

      iwes.DropDownControl(lb);

      if (this.value == null)
        this.value = value;

      return this.value;
    }

    private void lb_SelectedIndexChanged(object sender, EventArgs e)
    {
      TypeConverter tc = TypeDescriptor.GetConverter(type);
      value = tc.ConvertFromString(lb.SelectedItem.ToString());
      iwes.CloseDropDown();
    }
  }

  sealed class FlaggedEnumEditor : UITypeEditor
  {
    object value = null;
    IWindowsFormsEditorService iwes;
    CheckedListBox lb;
    Type type;

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      iwes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

      type = value.GetType();

      TypeConverter tc = TypeDescriptor.GetConverter(type);

      lb = new CheckedListBox();
      lb.IntegralHeight = false;
      lb.BorderStyle = BorderStyle.FixedSingle;
      lb.SelectedIndexChanged +=new EventHandler(lb_SelectedIndexChanged);
				
      foreach (string o in Enum.GetNames(type))
      {
        lb.Items.Add(o, ((int) value & (int) tc.ConvertFromString(o)) != 0);
      }

      lb.Height = lb.ItemHeight * lb.Items.Count;

      iwes.DropDownControl(lb);

      if (this.value == null)
        this.value = value;

      return this.value;
    }

    private void lb_SelectedIndexChanged(object sender, EventArgs e)
    {
      //				TypeConverter tc = TypeDescriptor.GetConverter(type);
      //
      //				value = tc.ConvertFromString(lb.SelectedItem.ToString());
      //
      //				iwes.CloseDropDown();
    }
  }

  sealed class SupportedValuesEditor : UITypeEditor
  {
    object value = null;
    IWindowsFormsEditorService iwes;
    ListBox lb;
    Type type;

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      iwes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

      type = value.GetType();

      TypeConverter tc = TypeDescriptor.GetConverter(type);

      lb = new ListBox();
      lb.IntegralHeight = false;
      lb.BorderStyle = BorderStyle.FixedSingle;
      lb.SelectedIndexChanged +=new EventHandler(lb_SelectedIndexChanged);
				
      foreach (object o in tc.GetStandardValues())
      {
        lb.Items.Add(o);
      }

      lb.Height = lb.ItemHeight * lb.Items.Count;

      iwes.DropDownControl(lb);

      if (this.value == null)
        this.value = value;

      return this.value;
    }

    private void lb_SelectedIndexChanged(object sender, EventArgs e)
    {
      TypeConverter tc = TypeDescriptor.GetConverter(type);

      value = tc.ConvertFromString(lb.SelectedItem.ToString());

      iwes.CloseDropDown();
    }
  }
}
