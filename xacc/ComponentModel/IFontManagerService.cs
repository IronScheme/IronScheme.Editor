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

#region Includes
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;
using System.Drawing.Text;
using System.Runtime.InteropServices;

using SR = System.Resources;
#endregion

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides services for managing fonts
	/// </summary>
	[Name("Font manager", "Allows application based fonts")]
	public interface IFontManagerService : IService
	{
    /// <summary>
    /// Gets an array of installed fonts
    /// </summary>
		FontFamily[] InstalledFonts {get;}

    /// <summary>
    /// Installs a font
    /// </summary>
    /// <param name="path">the path of the font</param>
		void InstallFont(string path);
	}

	sealed class FontManager : ServiceBase, IFontManagerService
	{
		PrivateFontCollection col = new PrivateFontCollection();

    public FontManager()
    {
      // .NET 2 only issue here!!!
      try
      {
        Stream fontStream = GetType().Assembly.GetManifestResourceStream("Xacc.Resources.VeraMono.ttf.bz2");

        byte[] fontdata = new byte[fontStream.Length];
        fontStream.Read(fontdata,0,(int)fontStream.Length);
        fontStream.Close();

        fontdata = Runtime.Compression.Decompress(fontdata);

        GCHandle gc = GCHandle.Alloc(fontdata, GCHandleType.Pinned);

        col.AddMemoryFont(gc.AddrOfPinnedObject(),fontdata.Length);

        gc.Free();
      }
      catch
      {
        // assembly not found
      }
    }

		public FontFamily[] InstalledFonts
		{
			get	{return col.Families;}
		}

		public void InstallFont(string path)
		{
			Trace.WriteLine("Loading font: {0}", path);
		}
	}

}
