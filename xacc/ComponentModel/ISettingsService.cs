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
using System.Drawing;
using Xacc.Controls;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Xacc.Configuration;

using Xacc.Runtime;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides service for IDE settings
	/// </summary>
	public interface ISettingsService : IService
	{
    /// <summary>
    /// Gets or sets the editor font name
    /// </summary>
    string EditorFontName {get;set;}

    /// <summary>
    /// Gets or sets the editor font
    /// </summary>
    Font EditorFont       {get;}

    /// <summary>
    /// Gets or sets the editor font size
    /// </summary>
    double EditorFontSize    {get;set;}

    /// <summary>
    /// Gets or sets the general font name
    /// </summary>
    string GeneralFontName {get;set;}

    /// <summary>
    /// Gets or sets the general font size
    /// </summary>
    double GeneralFontSize    {get;set;}

    /// <summary>
    /// Gets or sets the general font
    /// </summary>
    Font GeneralFont      {get;}

    /// <summary>
    /// Gets or sets the tabsize in space count
    /// </summary>
    int TabSize           {get;set;}

    /// <summary>
    /// Gets the commandline arguments.
    /// </summary>
    IdeArgs Args          {get;}
	}

  sealed class SettingsService : ServiceBase, ISettingsService
  {
    Font editorfont, generalfont;
    int tabsize = 2;
    internal IdeArgs args;

    public static bool idemode = false;

    public SettingsService()
    {
#if USEBZIP2
      editorfont = new Font(ServiceHost.Font.InstalledFonts[0], 10);
#else
      editorfont = new Font("Lucida Console", 10);
#endif
      generalfont = SystemInformation.MenuFont;
    }

    public IdeArgs Args  
    {
      get {return args;}
    }

    #region ISettingsService Members

    public string EditorFontName
    {
      get {return editorfont.Name;}
      set 
      {
        if (value != EditorFontName)
        {
          Font newf = new Font(value, (float) EditorFontSize);
          Font oldfont = editorfont;
          editorfont = newf;

          if (oldfont != null)
          {
            //oldfont.Dispose();
          }
          
        }
      }
    }

    public string GeneralFontName
    {
      get {return generalfont.Name;}
      set 
      {
        if (value != GeneralFontName)
        {
          Font newf = new Font(value, (float) GeneralFontSize);
          if (generalfont != null)
          {
            generalfont.Dispose();
          }
          generalfont = newf;
        }
      }
    }


    public Font EditorFont
    {
      get
      {
        return editorfont;
      }
    }

    public double EditorFontSize
    {
      get
      {
        return editorfont.SizeInPoints;
      }
      set
      {
        if (value != EditorFontSize)
        {
          Font newf = new Font(editorfont.FontFamily, (float)value);
          Font oldfont = editorfont;
          editorfont = newf;
          if (oldfont != null)
          {
            //oldfont.Dispose();
          }
        }
      }
    }

    public double GeneralFontSize
    {
      get
      {
        return generalfont.SizeInPoints;
      }
      set
      {
        if (value != GeneralFontSize)
        {
          Font newf = new Font(generalfont.FontFamily, (float)value);
          if (generalfont != null)
          {
            generalfont.Dispose();
          }
          generalfont = newf;
        }
      }
    }

    public Font GeneralFont
    {
      get
      {
        return generalfont;
      }
    }

    public int TabSize
    {
      get
      {
        return tabsize;
      }
      set
      {
        tabsize = value;
      }
    }

    #endregion
  }

}
