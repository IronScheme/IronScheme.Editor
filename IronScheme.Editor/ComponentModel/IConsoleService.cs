#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
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
