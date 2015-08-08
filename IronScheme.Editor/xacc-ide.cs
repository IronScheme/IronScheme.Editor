#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion

#region Includes

using System;
using System.Windows.Forms;
using IronScheme.Editor.Configuration;


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

