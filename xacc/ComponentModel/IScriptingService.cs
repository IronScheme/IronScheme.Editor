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
using LSharp;
using Xacc.Controls;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Xacc.Runtime;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides service for scripting
	/// </summary>
	public interface IScriptingService : IService
	{
    /// <summary>
    /// Runs the script
    /// </summary>
    /// <param name="script">the script text to run</param>
    void Run(string script);

    void InitCommand();
	}

  [Menu("Script")]
  sealed class ScriptingService : ServiceBase, IScriptingService
  {
    readonly TopLoop l = new TopLoop();
    AdvancedTextBox atb;
    internal readonly ScriptProject proj = new ScriptProject();

    internal IDockContent tbp;

    sealed internal class ScriptProject : Build.Project
    {
      public ScriptProject()
      {
        Location = Application.StartupPath + Path.DirectorySeparatorChar + "env.xacc"; //fake
        RootDirectory = Application.StartupPath;

        ServiceHost.File.Closing +=new FileManagerEventHandler(FileManager_Closing);
      }

      void FileManager_Closing(object sender, FileEventArgs e)
      {
        e.Cancel |= (e.FileName.ToLower() == (RootDirectory + Path.DirectorySeparatorChar + "command.ls").ToLower());
      }
    }
  

    public void InitCommand()
    {
#if !DEBUG
      //proj.DeserializeProjectData();
#endif
      string fn = (Application.StartupPath + Path.DirectorySeparatorChar + "profile.ls").Replace("\\", @"\\");
      
      if (!File.Exists(fn))
      {
        using (Stream i = typeof(ScriptingService).Assembly.GetManifestResourceStream("Xacc.Resources.profile.ls"))
        {
          using (Stream o = File.Create(fn))
          {
            byte[] b = new byte[i.Length];
            i.Read(b, 0, b.Length);
            o.Write(b,0, b.Length);
          }
        }
      }

      using (TextReader r = File.OpenText(fn))
      {
        try
        {
          LSharp.Runtime.EvalString("(do " + r.ReadToEnd() + ")");
        }
        catch (Exception ex)
        {
          Trace.WriteLine("Init exception: {0}", ex);
        }
      }

      if (SettingsService.idemode)
      {
        atb = ServiceHost.File.Open(Application.StartupPath + "/command.ls", DockState.DockBottom)
          as AdvancedTextBox;

        atb.AutoSave = true;

        tbp = atb.Parent as IDockContent;
        tbp.HideOnClose = true;
        tbp.Hide();

        atb.ProjectHint = proj;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
#if !DEBUG
        //proj.SerializeProjectData();
#endif
      }
      base.Dispose (disposing);
    }


    [MenuItem("Run", Index = 0, Image="Script.Run.png")]
    internal void Run()
    {
      if (atb.Focused)
      {
        Run(atb.Text.Trim(), false, false);
      }
      else
      {
        IFileManagerService fm = ServiceHost.File;
        AdvancedTextBox atb2 = fm[fm.Current] as AdvancedTextBox;
        if (atb2 != null)
        {
          if (atb2.Buffer.Language.Name == "LSharp")
          {
            Run(atb2.Text.Trim());
          }
          if (atb2.EditorLanguage == "R6RS Scheme")
          {
            ServiceHost.Shell.RunCurrentFile();
          }
        }
      }
    }

    [MenuItem("Run Selected", Index = 1, Image="Script.Run.png")]
    internal void RunSelected()
    {
      if (atb.Focused)
      {
        Run(atb.SelectionText.Trim(), false, false);
      }
      else
      {
        IFileManagerService fm = ServiceHost.File;
        AdvancedTextBox atb2 = fm[fm.Current] as AdvancedTextBox;
        if (atb2 != null)
        {
          if (atb2.Buffer.Language.Name == "LSharp")
          {
            Run(atb2.SelectionText.Trim());
          }
          if (atb2.EditorLanguage == "R6RS Scheme")
          {
            ServiceHost.Shell.RunCommand(atb2.SelectionText.Trim());
          }
        }
      }
    }

    [MenuItem("Trace Call", Index = 10)]
    bool TraceRun
    {
      get {return TopLoop.TraceCall;}
      set {TopLoop.TraceCall = value;}
    }

    [MenuItem("Trace Return", Index = 11)]
    bool TraceReturn
    {
      get {return TopLoop.TraceReturn;}
      set {TopLoop.TraceReturn = value;}
    }

    [MenuItem("Edit Profile", Index = 20, Image="Script.EditProfile.png")]
    void EditProfile()
    {
      IFileManagerService fm = ServiceHost.File;
      AdvancedTextBox atb =
        fm.Open(Application.StartupPath + Path.DirectorySeparatorChar + "profile.ls")
        as AdvancedTextBox;

      atb.AutoSave = true;
      atb.ProjectHint = proj;
    }

    void ThreadRun()
    {
      l.Run();
      Console.SetIn(old);
    }

    TextReader old;

    public void Run(string script)
    {
      Run(script, false, false);
    }

    void Run(string script, bool showconsole, bool threaded)
    {
      TextReader r = new StringReader(script);

      old = Console.In;
      Console.SetIn(r);

      if (showconsole)
      {
        (ServiceHost.Console as StandardConsole).tbp.Show();
      }
      
      if (threaded)
      {
        new Thread( new ThreadStart(ThreadRun)).Start();
      }
      else
      {
        l.Run();
        Console.SetIn(old);
      }
    }
  }

}
