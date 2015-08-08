#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using IronScheme.Editor.Build;
using IronScheme.Editor.Controls;
using IronScheme.Editor.Runtime;
#endregion


namespace IronScheme.Editor.ComponentModel
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
