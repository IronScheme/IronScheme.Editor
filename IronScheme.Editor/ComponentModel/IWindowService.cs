#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Windows.Forms;
using IronScheme.Editor.Runtime;

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
