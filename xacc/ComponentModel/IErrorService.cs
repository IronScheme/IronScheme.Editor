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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Build;
using Xacc.Controls;

using SR = System.Resources;
using Xacc.Runtime;
#endregion


namespace Xacc.ComponentModel
{
  /// <summary>
  /// Provides services to display errors/warining/info to user
  /// </summary>
	[Name("Error reporting")]
	public interface IErrorService : IService
	{
    /// <summary>
    /// Outputs a list of results
    /// </summary>
    /// <param name="caller">the caller</param>
    /// <param name="results">the results</param>
    /// <remarks>This method is thread-safe</remarks>
		void						OutputErrors					(object caller, params ActionResult[] results);

    /// <summary>
    /// Clears all displayed results
    /// </summary>
    /// <param name="caller">the caller</param>
    /// <remarks>This method is thread-safe</remarks>
		void						ClearErrors(object caller);
	}

  sealed class ErrorService : ServiceBase, IErrorService
  {
    readonly ErrorView view = new ErrorView();
    internal IDockContent tbp;

    public void OutputErrors(object caller, params ActionResult[] results)
    {
      view.OutputErrors(caller, results);
    }

    public void ClearErrors(object caller)
    {
      view.ClearErrors(caller);
    }

    public ErrorService()
    {
      if (SettingsService.idemode)
      {
        tbp = Runtime.DockFactory.Content();
        tbp.Text = "Results";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        tbp.Controls.Add(view);
        tbp.Show(ServiceHost.Window.Document, DockState.DockBottom);
        view.Tag = tbp;
        tbp.Hide();
        tbp.HideOnClose = true;
      }
    }
  }




}
