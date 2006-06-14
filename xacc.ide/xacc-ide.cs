
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

namespace xacc_ide
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	class ide : Form
  {
		public ide()
		{
			InitializeComponent();
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // ide
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(864, 598);
      this.Name = "ide";
      this.ResumeLayout(false);

    }
		#endregion

		#region Standalone app code

    [STAThread]
    static void Main(string[] args)
    {
      ide f = new ide();
      if (IdeSupport.KickStart(f))
      {
        Application.Run(f);
      }
    }
    #endregion

    //private void timer1_Tick(object sender, System.EventArgs e)
    //{
    //  IFileManagerService fm = ServiceHost.File; 
    //  if (fm != null)
    //  {
    //    IDocument atb = fm.CurrentControl as IDocument;
    //    if (atb != null && ((Control)atb).Focused)
    //    {
    //      if (atb.Info != null)
    //      {
    //        statusBarPanel3.Text = atb.Info;
    //      }
    //      if (fm.Current != null)
    //      {
    //        statusBarPanel7.Text = fm.Current;
    //      }
    //    }
    //    else
    //    {
    //      statusBarPanel3.Text = 
    //        statusBarPanel7.Text = "";
    //    }
    //  }
    //}

    //private static void f_Load(object sender, EventArgs e)
    //{
    //  ((ide)sender).timer1.Enabled = true;
    //}
  }
}
