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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services for managing toolbar
	/// </summary>
	public interface IStatusBarService : IService
	{
    /// <summary>
    /// Gets the status bar.
    /// </summary>
    /// <value>The status bar.</value>
    StatusStrip StatusBar {get;}

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    /// <value>The progress.</value>
    float Progress { get; set;}
	}

	sealed class StatusBarService : ServiceBase, IStatusBarService
	{
    readonly StatusStrip status = new StatusStrip();
    readonly ToolStripProgressBar progress = new ToolStripProgressBar();

    const float MAX = 20f;

    public StatusStrip StatusBar 
    {
      get { return status; }
    }

    public StatusBarService()
		{
      status.Dock = DockStyle.Bottom;
      status.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
      ServiceHost.Window.MainForm.Controls.Add(status);
      progress.Alignment = ToolStripItemAlignment.Right;
      progress.AutoSize = false;
      progress.Style = ProgressBarStyle.Continuous;
      progress.Width = 200;
      progress.Maximum = (int)MAX;
      progress.Minimum = 0;
      status.RenderMode = ToolStripRenderMode.ManagerRenderMode;
      status.Items.Add(progress);
    }

    int current = 0;

    public float Progress
    {
      get
      {
        return progress.Value / MAX;
      }
      set
      {
        int current = (int)(value * MAX);
        if (current != this.current)
        {
          SetValue(this.current = current);
        }
      }
    }

    delegate void SV(int v);

    void SetValue(int value)
    {
      if (InvokeRequired)
      {
        BeginInvoke(new SV(SetValue), new object[] { value });
        return;
      }
      progress.Value = current;
    }
  }
}
