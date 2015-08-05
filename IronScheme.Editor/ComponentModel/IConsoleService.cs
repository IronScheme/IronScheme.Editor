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
using System.IO;
using System.Windows.Forms;
using IronScheme.Editor.Controls;

using IronScheme.Editor.Runtime;
#endregion


namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Interface for Console related services
  /// </summary>
  [Name("Standard I/O console")]
	public interface IConsoleService : IService
	{
		/// <summary>
		/// The input stream. 
		/// </summary>
		TextReader			In										{get;}
		/// <summary>
		/// The output stream.
		/// </summary>
		TextWriter			Out										{get;}
		/// <summary>
		/// The error stream.
		/// </summary>
		TextWriter			Error									{get;}
	}
	
	sealed class StandardConsole : ServiceBase, IConsoleService
	{
    internal IDockContent tbp;
    WinConsole wcon = new WinConsole();

    public StandardConsole()
    {
      if (SettingsService.idemode)
      {
        tbp = Runtime.DockFactory.Content();
        tbp.Text = "Output";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        wcon.Dock = DockStyle.Fill;
        tbp.Controls.Add(wcon);
        tbp.Show(ServiceHost.Window.Document, DockState.DockBottom);
        tbp.Hide();
        tbp.HideOnClose = true;
      }
    }

		public TextReader In
		{
			get {return Console.In;}
			set {Console.SetIn(value);}
		}

		public TextWriter Out
		{
			get{return Console.Out;}
			set{Console.SetOut(value);}
		}

		public TextWriter Error
		{
			get{return Console.Error;}
			set{Console.SetError(value);}
		}
	}

}
