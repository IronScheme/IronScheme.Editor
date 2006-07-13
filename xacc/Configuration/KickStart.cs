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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing.Text;
using ST = System.Threading; // dont include, messes up timers, this doesnt work on pnet
using Xacc.Algorithms;
using System.Diagnostics;
using Xacc.ComponentModel;
using Xacc.Controls;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;


using ToolStripMenuItem = Xacc.Controls.ToolStripMenuItem;

using Xacc.Runtime;

#endregion

namespace Xacc.Configuration
{
  /// <summary>
  /// Start up parameter for xacc.ide
  /// </summary>
  public class IdeArgs : Utils.GetArgs
  {
    /// <summary>
    /// Starts IDE in listermode (maximized and close on ESC)
    /// </summary>
    public bool listermode = false;

    /// <summary>
    /// Starts IDE in debug mode
    /// </summary>
    public bool debug = 
#if DEBUG
      true
#else
      false
#endif
      ;

    public FormWindowState form = FormWindowState.Normal;

    /// <summary>
    /// List of files to open
    /// </summary>
    [Utils.DefaultArg]
    public string[] open;
  }

  public class ServerService : MarshalByRefObject
  {
    public void OpenFile(string filename)
    {
      ServiceHost.File.Open(filename);
    }
  }


	/// <summary>
	/// Support to bootstrap the IDE
	/// </summary>
	public class IdeSupport
	{
    static TextWriter tracelog;
    static IdeArgs args;

    internal static AboutForm about;

    static ST.Mutex real = null;

    static void InvokeClient()
    {
      IpcClientChannel client = new IpcClientChannel();
      ChannelServices.RegisterChannel(client, false);

      RemotingConfiguration.RegisterWellKnownClientType(typeof(ServerService), "ipc://XACCIDE/ss");

      ServerService s = new ServerService();
      foreach (string fn in args.open)
      {
        s.OpenFile(fn);
      }

      ChannelServices.UnregisterChannel(client);
    }


    /// <summary>
    /// Starts the IDE
    /// </summary>
    /// <param name="f">the hosting form</param>
    public static bool KickStart(Form f)
    {
      args = new IdeArgs();

      if (args.listermode)
      {
        ST.Mutex m = null;
        try
        {
          m = ST.Mutex.OpenExisting("XACCIDE");
        }
        catch
        {
          // must assign, else GC will clean up
          real = new System.Threading.Mutex(true, "XACCIDE");
        }

        if (m != null)
        {
          InvokeClient();
          return false;
        }
      }

#if !DEBUG
      Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      try
      {
#endif
        System.Threading.Thread.CurrentThread.CurrentCulture =
          System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

        if (args.listermode)
        {
          IpcServerChannel server = new IpcServerChannel("XACCIDE");
          ChannelServices.RegisterChannel(server, false);

          RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServerService), "ss", WellKnownObjectMode.Singleton);
        }

      Application.EnableVisualStyles();
      SettingsService.idemode = true;
      Application.ApplicationExit +=new EventHandler(AppExit);
      AppDomain.CurrentDomain.AssemblyResolve +=new ResolveEventHandler(CurrentDomain_AssemblyResolve);
      AppDomain.CurrentDomain.SetupInformation.LoaderOptimization = LoaderOptimization.MultiDomainHost;

      about = new AboutForm();
      about.StartPosition = f.StartPosition = FormStartPosition.CenterScreen;
      about.Show();
      Application.DoEvents();

      if (args.debug)
      {
        Diagnostics.Trace.debugmode = true;
        Trace.AutoFlush = true;
        try
        {
          Trace.Listeners.Add( new TextWriterTraceListener( tracelog = File.CreateText(Application.StartupPath + "/xacc.log")));
        }
        catch
        {
          Trace.Listeners.Add( new TextWriterTraceListener( tracelog = File.CreateText(Application.StartupPath + "/xacc1.log")));
        }
      }

      f.KeyPreview = true;


      IWindowService ws = new WindowService(f);

      about.progressBar1.Value += 10;

      Assembly thisass = typeof(IdeSupport).Assembly;

      new PluginManager().LoadAssembly(thisass);

      ((SettingsService) ServiceHost.Settings).args = args;

      IMenuService ms = ServiceHost.Menu;

      MenuStrip mm = ms.MainMenu;

      f.Font = SystemInformation.MenuFont;
      f.Closing +=new CancelEventHandler(f_Closing);
      f.AllowDrop = true;

      f.DragEnter+=new DragEventHandler(f_DragEnter);
      f.DragDrop+=new DragEventHandler(f_DragDrop);

      Version ver = typeof(IdeSupport).Assembly.GetName().Version;

      string verstr = ver.ToString(4);

      f.Text = "xacc.ide " + verstr + (args.debug ? " - DEBUG MODE" : string.Empty) ;
      f.Size = new Size(900, 650);

      ToolStripMenuItem view = ms["View"];

      f.Icon = new Icon(
        thisass.GetManifestResourceStream(
#if VS
        "Xacc.Resources." + 
#endif
        "atb.ico"));

      ServiceHost.Window.Document.AllowDrop = true;
      ServiceHost.Window.Document.DragEnter +=new DragEventHandler(f_DragEnter);
      ServiceHost.Window.Document.DragDrop +=new DragEventHandler(f_DragDrop);

      ServiceHost.Window.Document.BringToFront();
      new ViewService();
      about.progressBar1.Value += 5;
      new KeyboardHandler();

      ServiceHost.State = ApplicationState.Normal;

      about.progressBar1.Value += 5;


      //after everything has been loaded
      ServiceHost.Initialize();

      about.progressBar1.Value += 5;

      try
      {
        ServiceHost.Scripting.InitCommand();
      }
      catch (Exception ex) // MONO
      {
        Trace.WriteLine(ex);
      }

      (ServiceHost.ToolBar as ToolBarService).ValidateToolBarButtons();

      ToolStripManager.LoadSettings(f);

      if (args.open != null)
      {
        foreach (string of in args.open)
        {
          StringBuilder sb = new StringBuilder(256);
          int len = kernel32.GetLongPathName(of, sb, 255);
          try
          {
            ServiceHost.File.Open(sb.ToString());
          }
          catch (Exception ex)
          {
            Trace.WriteLine("Could not load file: " + sb + " Message: " + ex.Message);
          }
        }
      }
#if !DEBUG
      }
      catch (Exception ex)
      {
        HandleException(ex, true);
        return false;
      }
#endif
      f.WindowState = args.form;
#if DEBUG
      f.WindowState = FormWindowState.Maximized;
#endif
      if (args.listermode)
      {
        f.WindowState = FormWindowState.Maximized;
      }

      about.Close();
      return true;
    }



		static void f_Closing(object sender, CancelEventArgs e)
		{
      

      IToolsService its = ServiceHost.Tools;
      if (its != null)
      {
        (its as ToolsService).SaveView();
      }

      IProjectManagerService pms = ServiceHost.Project;
      if (pms != null)
      {
        pms.CloseAll();
      }

			IFileManagerService fms = ServiceHost.File;
			if (fms != null)
			{
        if (!fms.CloseAll())
        {
          e.Cancel = true;
        }
			}

      Application.Exit();
		}

		static void AppExit(object sender, EventArgs e)
		{
      ToolStripManager.SaveSettings(ServiceHost.Window.MainForm);
			((IDisposable) ServiceHost.INSTANCE).Dispose();
      if (tracelog != null)
      {
        Trace.Close();
      }
		}

    static void f_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        e.Effect = DragDropEffects.All;
      }
    }

    static void f_DragDrop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        string[] filenames = (e.Data.GetData(DataFormats.FileDrop) as string[]);
        foreach (string fn in filenames)
        {
          ServiceHost.File.Open(fn);
        }
      }
    }

    static string LINE = "================================================================================";
    static string EXCP = "Unhandled exception";

    static void HandleException(Exception ex, bool appstateinvalid)
    {
      Trace.WriteLine(LINE, EXCP);
      Trace.WriteLine(ex, EXCP);
      Trace.WriteLine(LINE, EXCP);

      if (!(ex is System.Threading.ThreadAbortException))
      {
        ExceptionForm exform = new ExceptionForm();
        exform.Exception = ex;
        exform.ApplicationStateInvalid = appstateinvalid;
        exform.ShowDialog(ServiceHost.Window.MainForm);
      }
    }

    static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
      HandleException(e.Exception, false);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (!e.IsTerminating)
      {
        HandleException(e.ExceptionObject as Exception, false);
      }
    }

    static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.StartsWith("xacc"))
      {
        return typeof(IdeSupport).Assembly;
      }
      return null;
    }
  }
}

