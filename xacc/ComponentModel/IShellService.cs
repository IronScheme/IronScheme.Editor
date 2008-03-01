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
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Configuration;

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
	}

  [Menu("Tools")]
  sealed class ShellService : ServiceBase, IShellService
  {
    AdvancedTextBox atb;

    internal IDockContent tbp;
  

    public void InitCommand()
    {

      if (SettingsService.idemode)
      {
        atb = ServiceHost.File.Open(Application.StartupPath + "/shell", DockState.DockBottom)
          as AdvancedTextBox;

        atb.Clear();

        atb.LineInserted += new AdvancedTextBox.LineInsertNotify(atb_LineInserted);
        atb.AutoSave = true;


        tbp = atb.Parent as IDockContent;
        tbp.Text = "IronScheme Shell ";
        tbp.HideOnClose = true;
        tbp.Hide();

        InitializeShell();

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
          In.WriteLine(line.Substring(i + 2));
          In.Flush();
        }
      }
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


    void StopShell(bool print)
    {
      if (print)
      {
        atb.Text = "Shell stopped\n";
      }
      reading = false;
      if (In != null)
      {
        In.Close();
        In = null;
      }
      if (p != null)
      {
        p.Kill();
        p = null;
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
      In = null;
      Out = null;
      p = null;

    }

  }

}
