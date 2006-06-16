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
    StatusStrip StatusBar {get;}
    float Progress { get; set;}
	}

	sealed class StatusBarService : ServiceBase, IStatusBarService
	{
    readonly StatusStrip status = new StatusStrip();
    readonly ToolStripProgressBar progress = new ToolStripProgressBar();

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
      progress.Maximum = 5000;
      progress.Minimum = 0;
      status.Items.Add(progress);
    }

    public float Progress
    {
      get
      {
        return progress.Value / (float)progress.Maximum;
      }
      set
      {
        progress.Value = (int)(value * progress.Maximum);
      }
    }
  }
}
