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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Reflection;
using Xacc.ComponentModel;

namespace Xacc.Controls
{
	class PictureComboBox : System.Windows.Forms.ComboBox
	{
		public PictureComboBox()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer, true);

			InitializeComponent();

			this.ItemHeight = SystemInformation.MenuHeight - 1;
		}

		private void InitializeComponent()
		{
			this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.DropDownStyle = ComboBoxStyle.DropDownList;
		}
		
		GraphicsPath gp = null;
		
		int radius = 1;

		Pen borderpen = null;

		Brush selbg;
		Brush gradb;

    static Font font = SystemInformation.MenuFont;

    protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
    {
      if (e.Index < 0)
      {
        return;
      }

      if (gp == null)
      {
        Rectangle r = e.Bounds;
        int angle = 180;

        r.Width--;
        r.Height--;

        //r.Inflate(-1, - radius/2);
        //r.Offset(0, (radius/2));

        gp = new GraphicsPath();
        // top left
        gp.AddArc(r.X, r.Y, radius, radius, angle, 90);
        angle += 90;
        // top right
        gp.AddArc(r.Right - radius, r.Y,radius, radius, angle, 90);
        angle += 90;
        // bottom right
        gp.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, angle, 90);
        angle += 90;
        // bottom left
        gp.AddArc(r.X, r.Bottom - radius, radius, radius, angle, 90);

        gp.CloseAllFigures();
      }
			
      if (borderpen == null)
      {
        borderpen = new Pen(SystemColors.Highlight, 1);
        //borderpen.Alignment = PenAlignment.Center;
      }

      bool selected = (e.State & DrawItemState.Selected) != 0;

      //normal AA looks bad
      e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      //e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
      string word1 = Text;

      Rectangle r2 = e.Bounds;
      r2.Width = r2.Height + 2;
      r2.X--;
      r2.Height += 2;
      r2.Y--;


      Rectangle r4 = e.Bounds;
      r4.Inflate(1,1);
      //r4.Offset(-1,-1);
      e.Graphics.FillRectangle(SystemBrushes.Window, r4);

      //e.DrawBackground();

      if (!(Items[e.Index] is CodeModel.ICodeElement))
      {
        if (gradb == null)
        {
          LinearGradientBrush gb = new LinearGradientBrush(r2,
            SystemColors.ButtonFace, SystemColors.ButtonFace, 0f);
          gb.SetSigmaBellShape(0.9f, 0.2f);
          gradb = gb;
        }

        e.Graphics.FillRectangle(gradb, r2);
      }

  			
      int h = SystemInformation.MenuHeight;
      Brush b = SystemBrushes.ControlText;

      if (selected)
      {
        Rectangle r = e.Bounds;
        //Console.WriteLine(r);
        r.Width -= 1;
        r.Height -= 1;

        Rectangle r3 = r;
 
        r3.Width -= SystemInformation.MenuHeight;
        r3.X += SystemInformation.MenuHeight + 1;

        if (selbg == null)
        {
          selbg = new SolidBrush(Color.FromArgb(196, 225, 255));
          //r2.X -= r.Height/2;
          //r2.Height *= 2;
          //LinearGradientBrush gb = new LinearGradientBrush(r2, 
          //  Color.FromArgb(120, SystemColors.ControlLightLight),
          //  Color.FromArgb(120, SystemColors.Highlight), 90f);
          //gb.SetSigmaBellShape(0.6f,0.9f);

          //selbg = SystemBrushes.Highlight;
        }
        e.Graphics.FillPath(selbg, gp);
        //e.Graphics.DrawPath(borderpen, gp);
      }

    {

      Rectangle r = e.Bounds;
      r.Width = r.Height;
      r.X++;r.X++;
      r.Y++;r.Y++;

      if (!selected)
      {
        //r.X++;
        //r.Y++;
      }

      IImageListProviderService ips = ServiceHost.ImageListProvider;
      if (ips != null)
      {
        int i = ips[Items[e.Index]];
        if (i >= 0)
        {
          int f = (int)((e.Bounds.Height - 16)/2f);
          if ((e.State & DrawItemState.Focus) != 0)
          {
            ips.ImageList.Draw(e.Graphics, f+3, e.Bounds.Top + f + 1, i);
          }
          else
          {
            ips.ImageList.Draw(e.Graphics, f+3, e.Bounds.Top + f + 1, i);
          }
        }
      }

      if (!selected)
      {
        //r.X--;
        //r.Y--;
      }

      
      float fh = (float)font.FontFamily.GetCellAscent(0)/font.FontFamily.GetEmHeight(0);
      float bh = (float)font.FontFamily.GetCellDescent(0)/font.FontFamily.GetEmHeight(0);

      int hh = ((int)(float)(e.Bounds.Height - (fh - bh/2)*font.Height)/2);

      Type t = Items[e.Index] as Type;

      if (t == null)
      {
        t = Items[e.Index].GetType();
      }

      Build.Project p = Items[e.Index] as Build.Project;
      if (p != null)
      {
        e.Graphics.DrawString(p.ProjectName,
          SystemInformation.MenuFont, 
          b,
          r.Right + 1, e.Bounds.Top + hh);
      }
      else
      {
        Languages.Language l = Items[e.Index] as Languages.Language;
        if (l != null)
        {
          e.Graphics.DrawString(l.Name,
            SystemInformation.MenuFont,
            b,
            r.Right + 1, e.Bounds.Top + hh);
        }
        else
        {
          CodeModel.ICodeElement cl = Items[e.Index] as CodeModel.ICodeElement;
          if (cl != null)
          {
            e.Graphics.DrawString(cl is CodeModel.ICodeType ? cl.Fullname : ((cl is CodeModel.ICodeMethod || cl is CodeModel.ICodeField || cl is CodeModel.ICodeProperty) ? cl.ToString() : cl.Name),
              SystemInformation.MenuFont,
              b,
              r.Right + 1, e.Bounds.Top + hh);
          }
          else
          {
            e.Graphics.DrawString(NameAttribute.GetName(t),
              SystemInformation.MenuFont,
              b,
              r.Right + 1, e.Bounds.Top + hh);
          }
        }
      }

      gp.Dispose();
      gp = null;

      if (gradb != null)
      {
        gradb.Dispose();
        gradb = null;
      }

      if (selbg != null)
      {
        selbg.Dispose();
        selbg = null;
      }

    }
    }
	}
}
