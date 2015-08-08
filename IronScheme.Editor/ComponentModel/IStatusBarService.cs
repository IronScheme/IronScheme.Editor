#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Windows.Forms;

namespace IronScheme.Editor.ComponentModel
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

    string StatusText { get; set;}
	}

	sealed class StatusBarService : ServiceBase, IStatusBarService
	{
    readonly StatusStrip status = new StatusStrip();
    readonly ToolStripStatusLabel label = new ToolStripStatusLabel();
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

      label.Alignment = ToolStripItemAlignment.Left;
      status.Items.Add(label);

      progress.Alignment = ToolStripItemAlignment.Right;
      progress.AutoSize = false;
      progress.Style = ProgressBarStyle.Continuous;
      progress.Width = 25;
      progress.Maximum = (int)MAX;
      progress.Minimum = 0;
      progress.Height = 6;
      
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

    delegate void SW(string v);

    void SetStatusText(string text)
    {
      if (InvokeRequired)
      {
        Invoke(new SW(SetStatusText), new object[] { text });
        return;
      }
      label.Text = text;
    }

    public string StatusText
    {
      get { return label.Text; }
      set
      {
        if (value != label.Text)
        {
          SetStatusText(value);
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
