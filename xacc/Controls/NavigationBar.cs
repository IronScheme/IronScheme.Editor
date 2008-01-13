using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Xacc.CodeModel;
using Xacc.ComponentModel;

namespace Xacc.Controls
{
  partial class NavigationBar : UserControl
  {
    public NavigationBar()
    {
      InitializeComponent();

      classes.SelectedIndexChanged += new EventHandler(classes_SelectedIndexChanged);
      members.SelectedIndexChanged += new EventHandler(members_SelectedIndexChanged);

      Height = classes.Height + 5;
    }

    void members_SelectedIndexChanged(object sender, EventArgs e)
    {
      AdvancedTextBox atb = ServiceHost.File[codefile.Fullname] as AdvancedTextBox;
      if (atb != null)
      {
        if (!binding)
        {
          atb.Buffer.SelectLocation((members.SelectedItem as ICodeMember).Location);
          atb.ScrollToCaretUpper();
          atb.Select();
        }
      }
    }

    bool binding = false;

    void classes_SelectedIndexChanged(object sender, EventArgs e)
    {

      AdvancedTextBox atb = ServiceHost.File[codefile.Fullname] as AdvancedTextBox;
      if (atb != null)
      {

        ICodeType ct = classes.SelectedItem as ICodeType;

        if (ct != null)
        {
          List<ICodeMember> mems = new List<ICodeMember>();

          foreach (ICodeMember cm in ct.Members)
          {
            if (!(cm is ICodeType))
            {
              mems.Add(cm);
            }
          }

          mems.Sort(delegate(ICodeMember a, ICodeMember b) { return a.Name.CompareTo(b.Name); });

          members.DataSource = mems;

          if (!binding)
          {
            atb.Buffer.SelectLocation(ct.Location);
            atb.ScrollToCaretUpper();
            atb.Select();
          }
        }
      }
    }

    ICodeFile codefile;

    void AddRecursive(ICodeType type, List<ICodeType> types)
    {
      foreach (ICodeMember cm in type.Members)
      {
        ICodeType ct = cm as ICodeType;
        if (ct != null)
        {
          types.Add(ct);

          AddRecursive(ct, types);
        }
      }
    }

    void AddRecursiveNamespace(ICodeNamespace cns, List<ICodeType> types)
    {
      foreach (ICodeType type in cns.Types)
      {
        types.Add(type);
        AddRecursive(type, types);
      }

      foreach (ICodeNamespace ncns in cns.Namespaces)
      {
        AddRecursiveNamespace(ncns, types);
      }
    }



    delegate void VOIDVOID();

    void UpdateCodeFile()
    {
      //classes.Items.Clear();

      List<ICodeType> types = new List<ICodeType>();

      foreach (ICodeNamespace cns in codefile.Namespaces)
      {
        AddRecursiveNamespace(cns, types);
      }

      foreach (ICodeElement e in codefile.Elements)
      {
        if (e is ICodeType)
        {
          ICodeType type = (ICodeType)e;
          types.Add(type);
          AddRecursive(type, types);
        }
      }

      types.Sort(delegate(ICodeType a, ICodeType b) { return a == null ? -1 : b == null ? 1 : a.Fullname.CompareTo(b.Fullname); });

      binding = true;
      classes.DataSource = types;
      binding = false;

      //if (types.Count == 0)
      //{
      //  classes.DropDownHeight = SystemInformation.MenuHeight;
      //}
    }

    public ICodeFile CodeFile
    {
      get { return codefile; }
      set 
      { 
        codefile = value;

        if (InvokeRequired)
        {
          BeginInvoke(new VOIDVOID(UpdateCodeFile));
        }
      }
    }

  }
}
