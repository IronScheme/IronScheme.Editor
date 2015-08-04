using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Xacc.Controls
{
  /// <summary>
  /// 
  /// </summary>
  public class ToolStripMenuItem : System.Windows.Forms.ToolStripMenuItem
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ToolStripMenuItem"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="img">The img.</param>
    /// <param name="e">The e.</param>
    public ToolStripMenuItem(string text, Image img, EventHandler e)
      : base(text, img, e)
    {
      clonedfrom = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ToolStripMenuItem"/> class.
    /// </summary>
    /// <param name="text">The text.</param>
    public ToolStripMenuItem(string text)
      : base(text)
    {
      clonedfrom = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ToolStripMenuItem"/> class.
    /// </summary>
    public ToolStripMenuItem()
      : base()
    {
      clonedfrom = this;
    }

    string image;

    /// <summary>
    /// Gets or sets the object that contains data about the item.
    /// </summary>
    /// <value></value>
    /// <returns>An <see cref="T:System.Object"></see> that contains data about the control. The default is null.</returns>
    public new object Tag
    {
      get { return image; }
      set
      {
        image = value as string;
        Image = ComponentModel.ServiceHost.ImageListProvider.GetImage(image);
      }
    }

    internal ToolStripMenuItem clonedfrom;

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns></returns>
    public ToolStripMenuItem Clone()
    {
      ToolStripMenuItem cmi = MemberwiseClone() as ToolStripMenuItem;
      cmi.clonedfrom = this;
      return cmi;
    }
  }
}
