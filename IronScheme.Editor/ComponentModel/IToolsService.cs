#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using IronScheme.Editor.Controls;
using System.IO;

using ToolStripMenuItem = IronScheme.Editor.Controls.ToolStripMenuItem;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Service for managing tools
  /// </summary>
	public interface IToolsService : IService
	{
    /// <summary>
    /// Adds a tool to the tools menu
    /// </summary>
    /// <param name="name">the name of the tool</param>
    /// <param name="command">the commandline of the tool</param>
    /// <param name="defaultargs">the default arguments, if any</param>
    void AddTool(string name, string command, params string[] defaultargs);

    /// <summary>
    /// Runs a tool
    /// </summary>
    /// <param name="name">the name of the tool</param>
    /// <param name="args">the command line arguments, if any</param>
    /// <returns>true if tool returned successfully</returns>
    bool RunTool(string name, params string[] args);

    /// <summary>
    /// Gets the list of registered tool names
    /// </summary>
    string[] Tools {get;}
	}

  //[Menu("Tools")]
  sealed class ToolsService : ServiceBase, IToolsService
  {
    readonly Hashtable tools = new Hashtable();

    public void AddTool(string name, string command, params string[] defaultargs)
    {
      tools.Add(name, new ProcessStartInfo(command, string.Join(" ", defaultargs)));
    }

    protected override void Initialize()
    {
      /*
      ToolStripMenuItem tl = ServiceHost.Menu["Tools"];

      foreach (string name in tools.Keys)
      {
        ToolStripMenuItem mi = new ToolStripMenuItem(name, null, new EventHandler(InvokeRun));
        //mi.Index = tl.MenuItems.Count;
        mi.Enabled = File.Exists(((ProcessStartInfo)tools[name]).FileName);
        tl.DropDownItems.Add(mi);
      }
      //LoadView();
      */
    }

    protected override void Dispose(bool disposing)
    {
      
    }



    void InvokeRun(object sender, EventArgs e)
    {
      RunTool(((ToolStripMenuItem)sender).Text);
    }

    public bool RunTool(string name, params string[] args)
    {
      ProcessStartInfo psi = tools[name] as ProcessStartInfo;

      if (psi != null && File.Exists(psi.FileName))
      {
#warning TODO: RUN PROCESS IN BACKGROUND
        Process p = Process.Start(psi);
        p.WaitForExit();

        return p.ExitCode == 0;
      }
      return false;
    }

    public string[] Tools 
    {
      get 
      {
        ArrayList names = new ArrayList(tools.Keys);
        names.Sort();
        return names.ToArray(typeof(string)) as string[];
      }
    }

    readonly static string VIEWFILE = Application.StartupPath + Path.DirectorySeparatorChar + "view.xml";

    //[MenuItem("Save view", Index = 998, State = 0)]
    //public void SaveView()
    //{
    //  ServiceHost.Window.Document.Save(VIEWFILE);
    //}

    //[MenuItem("Load view", Index = 999, State = 0)]
    //void LoadView()
    //{
    //  if (File.Exists(VIEWFILE))
    //  {
    //    try
    //    {
    //      ServiceHost.Window.Document.Load(VIEWFILE);
    //    }
    //    catch 
    //    {
    //      File.Delete(VIEWFILE);
    //    }
    //  }
    //}
//
//    [MenuItem("Options", Index = 1000, State = 1, Image = "Tools.Options.png")]
//    void ShowOptions()
//    {
//      
    //    }

    void InvokeLangTool(object sender, EventArgs e)
    {
      //Build.ProcessAction pa = ((ToolStripMenuItem)sender).Tag as Build.ProcessAction;
      //bool res = pa.Invoke(ServiceHost.File.Current);
    }

    Hashtable langtools = new Hashtable();
    ToolStripMenuItem[] lastset;

    void tl_Popup(object sender, EventArgs e)
    {
      AdvancedTextBox atb = ServiceHost.File[ServiceHost.File.Current] as AdvancedTextBox;
      if (atb != null)
      {
        Languages.Language l = atb.Buffer.Language;
        ToolStripMenuItem[] mma = langtools[l] as ToolStripMenuItem[];
        ToolStripMenuItem tl = ServiceHost.Menu["Tools"];

        if (mma == null)
        {
          ArrayList mm = new ArrayList();

          foreach (Type t in l.actions)
          {
            //Build.ProcessAction pa = Activator.CreateInstance(t) as Build.ProcessAction;
            //ToolStripMenuItem mi = new ToolStripMenuItem(pa.Name, null, new EventHandler(InvokeLangTool));
            ////mi.Index = tl.MenuItems.Count;
            //mi.Enabled = pa.IsAvailable;
            //mi.Tag = pa;
            //mm.Add(mi);
          }

          mma = mm.ToArray(typeof(ToolStripMenuItem)) as ToolStripMenuItem[];
          langtools[l] = mma;
        }

        if (lastset != null)
        {
          foreach (ToolStripMenuItem mi in lastset)
          {
            tl.DropDownItems.Remove(mi);
          }
          lastset = null;
        }
        tl.DropDownItems.AddRange(lastset = mma);
      }
    }
  }
}
