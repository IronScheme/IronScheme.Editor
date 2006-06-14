using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Xacc.Controls
{
  public partial class ToolStripMenuItem : System.Windows.Forms.ToolStripMenuItem
  {

    public ToolStripMenuItem(string text, Image img, EventHandler e)
      : base(text, img, e)
    {
      clonedfrom = this;
    }

    public ToolStripMenuItem(string text)
      : base(text)
    {
      clonedfrom = this;
    }

    public ToolStripMenuItem()
      : base()
    {
      clonedfrom = this;
    }

    string image;

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

    public ToolStripMenuItem Clone()
    {
      ToolStripMenuItem cmi = MemberwiseClone() as ToolStripMenuItem;
      cmi.clonedfrom = this;
      return cmi;
    }
  }
}
