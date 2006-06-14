#region License
 /*	  xacc                																											*
 	*		Copyright (C) 2003-2006  Llewellyn@Pritchard.org                          *
 	*																																							*
	*		This program is free software; you can redistribute it and/or modify			*
	*		it under the terms of the GNU Lesser General Public License as            *
  *   published by the Free Software Foundation; either version 2.1, or					*
	*		(at your option) any later version.																				*
	*																																							*
	*		This program is distributed in the hope that it will be useful,						*
	*		but WITHOUT ANY WARRANTY; without even the implied warranty of						*
	*		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the							*
	*		GNU Lesser General Public License for more details.												*
	*																																							*
	*		You should have received a copy of the GNU Lesser General Public License	*
	*		along with this program; if not, write to the Free Software								*
	*		Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
#endregion

using System;
using System.Collections;
using System.Windows.Forms;
using System.IO;

using System.Reflection;
using Xacc.Controls;



namespace Xacc.ComponentModel
{
  interface IHelpService : IService
  {

  }

	[Menu("Help")]
	sealed class HelpService : ServiceBase, IHelpService
	{
    [MenuItem("ReadMe", Index = 1)]
    public void ReadMe()
    {
      AdvancedTextBox atb = ServiceHost.File.Open(Application.StartupPath + Path.DirectorySeparatorChar + "ReadMe.txt")
        as AdvancedTextBox;

      atb.ReadOnly = true;
    }

    [MenuItem("ChangeLog", Index = 2)]
    public void ChangeLog()
    {
      AdvancedTextBox atb = ServiceHost.File.Open(Application.StartupPath + Path.DirectorySeparatorChar + "ChangeLog.txt")
        as AdvancedTextBox;

      atb.ReadOnly = true;
    }

    [MenuItem("Submit Bug", Index = 10, Image="Help.SubmitBug.png")]
    public void SubmitBug()
    {
      ExceptionForm exf = new ExceptionForm();
      exf.ShowDialog(ServiceHost.Window.MainForm);
    }


    [MenuItem("About", Index = 1000, Image = "Help.About.png", AllowToolBar = true)]
    public void ShowAbout()
    {
      Form f = new Controls.AboutForm();
      f.Click += new EventHandler(CloseAbout);
      f.ShowDialog(ServiceHost.Window.MainForm);
    }

    void CloseAbout(object sender, EventArgs e)
    {
      Form f = sender as Form;
      f.Close();
    }
  }
}
