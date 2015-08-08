#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Drawing;
using System.Drawing.Drawing2D;

namespace IronScheme.Editor.Drawing
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
	}
}
