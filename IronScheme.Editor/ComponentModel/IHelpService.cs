#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Windows.Forms;
using System.IO;
using IronScheme.Editor.Controls;



namespace IronScheme.Editor.ComponentModel
{
  public interface IHelpService : IService
  {
    void ChangeLog();
    void ReadMe();
    void ShowAbout();
    void TraceLog();
  }

	[Menu("Help")]
	sealed class HelpService : ServiceBase, IHelpService
  {
    [MenuItem("ReadMe.txt", Index = 1)]
    public void ReadMe()
    {
      AdvancedTextBox atb = ServiceHost.File.Open(Application.StartupPath + Path.DirectorySeparatorChar + "ReadMe.txt")
        as AdvancedTextBox;

      atb.ReadOnly = true;
    }

    [MenuItem("ChangeLog.txt", Index = 2)]
    public void ChangeLog()
    {
      AdvancedTextBox atb = ServiceHost.File.Open(Application.StartupPath + Path.DirectorySeparatorChar + "ChangeLog.txt")
        as AdvancedTextBox;

      atb.ReadOnly = true;
    }

    //[MenuItem("Submit Bug", Index = 10, Image="Help.SubmitBug.png")]
    //public void SubmitBug()
    //{
    //  ExceptionForm exf = new ExceptionForm();
    //  exf.ShowDialog(ServiceHost.Window.MainForm);
    //}

    [MenuItem("Trace Log", Index = 11)]
    public void TraceLog()
    {
      AdvancedTextBox atb = ServiceHost.File.Open(Application.StartupPath + Path.DirectorySeparatorChar + "TraceLog.txt")
  as AdvancedTextBox;

      atb.Text = Diagnostics.Trace.GetFullTrace(); 

      atb.ReadOnly = true;
    }


    [MenuItem("About", Index = 1000, Image = "Help.About.png", AllowToolBar = true)]
    public void ShowAbout()
    {
      AboutForm f = new Controls.AboutForm();
      f.progressBar1.Visible = false;
      f.linkLabel1.Visible = true;
      f.linkLabel1.Text = "(c) 2003-2015 Llewellyn Pritchard";
      f.Click += delegate { f.Close(); };
      f.ShowDialog(ServiceHost.Window.MainForm);
    }
  }
}
