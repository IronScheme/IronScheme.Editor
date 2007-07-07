
#region Includes

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing.Text;
using ST = System.Threading; // dont include, messes up timers, this doesnt work on pnet
using Xacc.Algorithms;
using System.Diagnostics;
using Xacc.CodeModel;
using Xacc.ComponentModel;
using Xacc.Configuration;
using Xacc.Collections;
using Xacc.Controls;


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

