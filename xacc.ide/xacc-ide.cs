
#region Includes

using System;
using System.Windows.Forms;
using Xacc.Configuration;


#endregion

class ide : Form
  {
		public ide()
		{
      this.DoubleBuffered = true;
      this.SuspendLayout();
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(864, 598);
      this.Name = "ide";
      
		}

    [STAThread]
    static void Main(string[] args)
    {
      ide f = new ide();

      if (IdeSupport.KickStart(f))
      {
        f.ResumeLayout(false);
        Application.Run(f);
      }
    }
  }

