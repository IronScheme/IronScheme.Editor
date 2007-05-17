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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;
using System.IO;

namespace Xacc.Drawing
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	class Utils
	{
		private Utils()
		{
		}

    public static void PaintLineHighlight( Brush b, Pen p, Graphics g, int width, int height, bool fill)
    {
      PaintLineHighlight( b, p, g, 0, width, height, fill);
    }

    public static void PaintLineHighlight( Brush b, Pen p, Graphics g, int y, int width, int height, bool fill)
    {
      PaintLineHighlight( b, p, g, y, width, height, fill, 5);
    }

    public static void PaintLineHighlight( Brush b, Pen p, Graphics g, int y, int width, int height, bool fill, int radius)
    {
      PaintLineHighlight( b, p, g, 0, y, width, height, fill, radius);
    }

    public static void PaintLineHighlight( Brush b, Pen p, Graphics g, int x, int y, int width, int height, bool fill)
    {
      PaintLineHighlight( b, p, g, x, y, width, height, fill, 5);
    }

    public static void PaintLineHighlight( Brush b, Pen p, Graphics g, int x, int y, int width, int height, bool fill, int radius)
    {
      int angle = 180;

      Rectangle r = new Rectangle(x, y, width, height);

      GraphicsPath hlpath = new GraphicsPath();

      hlpath.AddArc(r.X, r.Y, radius, radius, angle, 90);
      angle += 90;
      // top right
      hlpath.AddArc(r.Right - radius, r.Y,radius, radius, angle, 90);
      angle += 90;
      // bottom right
      hlpath.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, angle, 90);
      angle += 90;
      // bottom left
      hlpath.AddArc(r.X, r.Bottom - radius, radius, radius, angle, 90);
      angle += 90;
      hlpath.CloseAllFigures();

      if (fill)
      {
        g.FillPath(b, hlpath);
      }
      SmoothingMode sm = g.SmoothingMode;
      g.SmoothingMode = SmoothingMode.HighQuality;
      g.DrawPath(p, hlpath);
      g.SmoothingMode = sm;

      hlpath.Dispose();
    }

    static Random RANDOM = new Random(0x0EEFFACE);
    static Font SMALLFONT = new Font(System.Windows.Forms.SystemInformation.MenuFont.FontFamily, 6f); // ok 6 then 

    public static Image MakeTreeImage(Type t)
    {
      using (Stream s = typeof(Utils).Assembly.GetManifestResourceStream("Xacc.Resources.na.png"))
      {
        Image b = Image.FromStream(s);
      
        using (Graphics g = Graphics.FromImage(b))
        {
          Brush bb = Factory.SolidBrush(Color.DimGray);
        
          g.PixelOffsetMode = PixelOffsetMode.None;

          string[] names = Build.InputExtensionAttribute.GetExtensions(t);

          if (names.Length > 0)
          {
            string name = names[0];
            if (name.Length > 3)
            {
              name = name.Substring(0,3);
            }

            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
            sf.LineAlignment = sf.Alignment = StringAlignment.Center;
        
            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawString(name, SMALLFONT, bb, new Rectangle(0,0,15,15), sf);
          }
        }

        return b;
      }
    }
	}
}
