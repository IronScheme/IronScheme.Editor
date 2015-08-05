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
using System.Windows.Forms;
using IronScheme.Editor.Controls;
using IronScheme.Editor.Runtime;
using System.Drawing;

namespace IronScheme.Editor.ComponentModel
{
	/// <summary>
	/// Provides services for managing windows
	/// </summary>
	[Name("Window services")]
	public interface IWindowService : IService
	{
    /// <summary>
    /// Gets the main form
    /// </summary>
		Form MainForm {get;}

    /// <summary>
    /// Gets the main document window
    /// </summary>
    IDockPanel   Document  {get;}
	}

	class WindowService : ServiceBase, IWindowService
	{
    IDockPanel d;
		Form main;

		public WindowService(Form main)
		{
      this.main = main;
      if (SettingsService.idemode)
      {
        d = Runtime.DockFactory.Panel();
      }
		}

    public IDockPanel Document
    {
      get {return d;}
    }

 		public Form MainForm
		{
			get {return main;}
		}
	}
}
