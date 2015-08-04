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
using Xacc.Controls;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Xacc.Runtime;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides service for scripting
	/// </summary>
	public interface IShellService : IService
	{
    /// <summary>
    /// Inits the command.
    /// </summary>
    void InitCommand();
    void RunCurrentFile();

    void RunCommand(string p);
  }

  [Menu("Scheme")]
  sealed class ShellService : ServiceBase, IShellService
  {
    ShellTextBox atb;

    internal IDockContent tbp;

    List<string> history = new List<string>();

    class ShellTextBox : AdvancedTextBox
    {
      internal ShellService svc;
      internal int last = 0;

      public override void NavigateUp()
      {
        if (Buffer.CaretIndex == TextLength - 1)
        {
          if (svc.history.Count > 0)
          {
            last--;
            if (last == -1)
            {
              last = svc.history.Count - 1;
            }
            string line = Buffer[Buffer.CurrentLine];
            if (line == "> ")
            {
              AppendText(svc.history[last]);
            }
            else
            {
              Buffer.SetLine(Buffer.CurrentLine, "> " + svc.history[last]);
            }
            Invalidate();
          }
        }
        else
        {
          base.NavigateUp();
        }
      }

      public override void NavigateDown()
      {
        if (Buffer.CaretIndex == TextLength - 1)
        {
          if (svc.history.Count > 0)
          {
            last++;
            if (last == svc.history.Count)
            {
              Buffer.SetLine(Buffer.CurrentLine, "> ");
            }
            else
            {
              if (last >= svc.history.Count)
              {
                last = 0;
              }
              string line = Buffer[Buffer.CurrentLine];
              if (line == "> ")
              {
                AppendText(svc.history[last]);
              }
              else
              {
                Buffer.SetLine(Buffer.CurrentLine, "> " + svc.history[last]);
              }
            }
            Invalidate();

          }
        }
        else
        {
          base.NavigateDown();
        }
      }
    }
  

    public void InitCommand()
    {

      if (SettingsService.idemode)
      {
        atb = ServiceHost.File.Open <ShellTextBox>(Application.StartupPath + "/shell", DockState.DockBottom);
        atb.svc = this;
        atb.Clear();

        ServiceHost.File.Closing += delegate (object sender, FileEventArgs e)
        {
          e.Cancel |= (StringComparer.InvariantCultureIgnoreCase.Compare(e.FileName, Application.StartupPath + "\\shell") == 0);
        };

        atb.LineInserted += new AdvancedTextBox.LineInsertNotify(atb_LineInserted);
        atb.AutoSave = true;

        tbp = atb.Parent as IDockContent;
        tbp.Text = "Scheme Shell ";
        tbp.HideOnClose = true;
        tbp.Hide();

        InitializeShell();

      }
    }

    [MenuItem("Run current file", Index = 0, Image = "IronScheme.png")]
    public void RunCurrentFile()
    {
      if (Array.IndexOf(ServiceHost.File.DirtyFiles, ServiceHost.File.Current) >= 0)
      {
        ServiceHost.File.Save(ServiceHost.File.Current);
      }
      RunFile(ServiceHost.File.Current);
    }

    void RunFile(string filename)
    {
      if (In != null)
      {
        string ext = Path.GetExtension(filename);
        switch (ext)
        {
          case ".ss":
          case ".sls":
          case ".scm":
            string cmd = string.Format("(load \"{0}\")\n", filename.Replace("\\", "/"));
            atb.AppendText(cmd);
            atb.Invalidate();
            AddCommand(cmd.TrimEnd('\n'));
            In.Write(cmd);
            In.Flush();
            break;
        }
      }
    }

    void atb_LineInserted(string line)
    {
      if (In != null)
      {
        int i = line.IndexOf("> ");
        if (i < 0)
        {
          i = line.IndexOf(". ");
        }

        if (i >= 0)
        {
          atb.ReadOnly = true;
          string cmd = line.Substring(i + 2);
          AddCommand(cmd);
          In.WriteLine(cmd);
          In.Flush();
        }
      }
    }

    void AddCommand(string cmd)
    {
      if (history.Count == 0 || cmd != history[history.Count - 1])
      {
        if ("" != cmd.Trim())
        {
          history.Add(cmd);
        }
      }
      atb.last = 0;
    }

    public void RunCommand(string p)
    {
      string cmd = p + "\n";
      atb.AppendText(cmd);
      atb.Invalidate();
      AddCommand(cmd.TrimEnd('\n'));
      In.Write(cmd);
      In.Flush();
    }


    [MenuItem("Restart shell", Index=1)]
    void RestartShell()
    {
      StopShell();
      InitializeShell();
    }

    [MenuItem("Stop shell", Index=2)]
    void StopShell()
    {
      StopShell(true);
    }

    ManualResetEvent stopshell;

    void StopShell(bool print)
    {
      if (p != null)
      {
        stopshell = new ManualResetEvent(false);

        p.Kill();

        reading = false;

        stopshell.WaitOne();

        if (print)
        {
          atb.Text = "Shell stopped\n";
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      StopShell(false);
      base.Dispose(disposing);
    }


    StreamReader Out, Error;
    StreamWriter In;

    Thread readthread, errorthread;

    void Read()
    {
      StringBuilder sb = new StringBuilder();
      while (reading)
      {
        while (printingerror)
        {
          Thread.Sleep(10);
        }
        int c = Out.Read();

        if (c >= 0 && c != '\r')
        {
          sb.Append((char)c);
          if (sb.Length > 1)
          {
            char prev = sb[sb.Length - 2];
            if ((c == ' ' && (prev == '>' || prev == '.')) || c == '\n')
            {
              atb.Invoke(new U(UpdateText), sb.ToString());
              sb.Length = 0;
            }
          }
        }
      }
    }

    bool printingerror = false;

    void ReadError()
    {
      StringBuilder sb = new StringBuilder();
      while (reading)
      {
        printingerror = false;
        int c = Error.Read();
        printingerror = true;

        if (c >= 0 && c != '\r')
        {
          sb.Append((char)c);
          if (sb.Length > 1)
          {
            char prev = sb[sb.Length - 2];
            if ((c == ' ' && (prev == '>' || prev == '.')) || c == '\n')
            {
              atb.Invoke(new U(UpdateText), sb.ToString());
              sb.Length = 0;
            }
          }
        }
      }
    }

    Process p;

    void InitializeShell()
    {
      string path = ConfigurationManager.AppSettings["ShellDirectory"];

      if (path == null)
      {
        ShellExeNotFound(path);
        return;
      }

      string filename = ConfigurationManager.AppSettings["ShellExecutable"];

      string fn = Path.Combine(path, filename);

      if (!File.Exists(fn))
      {
        ShellExeNotFound(fn);
        return;
      }

      p = new Process();

      readthread = new Thread(Read);
      errorthread = new Thread(ReadError);

      string args = ConfigurationManager.AppSettings["ShellArguments"];

      ProcessStartInfo psi = new ProcessStartInfo(fn);
      psi.Arguments = args ?? "";
      psi.WorkingDirectory = path;
      psi.CreateNoWindow = true;
      psi.RedirectStandardError = psi.RedirectStandardInput = psi.RedirectStandardOutput = true;
      psi.UseShellExecute = false;


      p.StartInfo = psi;
      p.EnableRaisingEvents = true;
      p.Exited += new EventHandler(p_Exited);

      p.Start();

      Error = p.StandardError;
      Out = p.StandardOutput;
      In = p.StandardInput;

      reading = true;

      errorthread.Start();
      readthread.Start();

    }

    private void ShellExeNotFound(string filepath)
    {
      atb.ReadOnly = true;
      atb.AppendText(filepath + " not found");
    }

    bool reading = false;

    delegate void U(string t);

    void UpdateText(string text)
    {
      atb.AppendText(text);
      atb.ScrollToCaret();
      atb.ReadOnly = false;
    }

    void p_Exited(object sender, EventArgs e)
    {
      readthread.Abort();
      errorthread.Abort();
      In = null;
      Out = null;
      Error = null;
      p = null;
      stopshell.Set();
    }

  }

}
