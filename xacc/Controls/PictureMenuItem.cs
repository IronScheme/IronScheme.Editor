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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using Xacc.ComponentModel;

namespace Xacc.Controls
{
	class PictureMenuItem : System.Windows.Forms.MenuItem
	{
		System.ComponentModel.IContainer components;
		EventHandler ev;
    static Font font = SystemInformation.MenuFont;

    static float fh = (float)font.FontFamily.GetCellAscent(0)/font.FontFamily.GetEmHeight(0);
    static float bh = (float)font.FontFamily.GetCellDescent(0)/font.FontFamily.GetEmHeight(0);

		object tag = null;
    internal PictureMenuItem clonedfrom;

		public override MenuItem CloneMenu()
		{
			PictureMenuItem pmi = new PictureMenuItem(Text, ev);
			pmi.Tag = tag;
      pmi.clonedfrom = this;

      foreach (MenuItem cm in MenuItems)
      {
        pmi.MenuItems.Add( cm.CloneMenu());
      }

			return pmi;
		}

		public PictureMenuItem(string text, EventHandler eventh) : base(text, eventh)
		{
			ev = eventh;
			components = new System.ComponentModel.Container();
			InitializeComponent();
		}
    
		public PictureMenuItem(string text) : this(text, null)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				gp.Dispose();
				gp2.Dispose();
				chkb.Dispose();
				selbg.Dispose();
				gradb.Dispose();
			}
			base.Dispose (disposing);
		}

		GraphicsPath gp = null, gp2 = null;
		//Pen p = null;
		
		int radius = 4;

		Pen borderpen = null;

		Brush selbg, chkb;
		Brush gradb;
    static Brush bgbrush   = SystemBrushes.Menu,
                 normbrush = SystemBrushes.ControlText,
                 inactiveb = Drawing.Factory.SolidBrush(Color.FromArgb(127, SystemColors.ControlText));

    static Pen chkpen = Pens.Orange,
                darkpen = SystemPens.ControlDark;

    static int menuheight = SystemInformation.MenuHeight;

		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

		{
			Rectangle r = e.Bounds;
			int angle = 180;

			r.Inflate(-1, -1 - radius/2);
			r.Offset(0, (radius/2));

			//r.Y--;
			//r.Height++;

			if (gp == null)
			{
				gp = new GraphicsPath();
				// top left
				gp.AddArc(r.X, r.Y - radius, radius, radius, angle, 90);
				angle += 90;
				// top right
				gp.AddArc(r.Right - radius, r.Y - radius,radius, radius, angle, 90);
				angle += 90;
				// bottom right
				gp.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, angle, 90);
				angle += 90;
				// bottom left
				gp.AddArc(r.X, r.Bottom - radius, radius, radius, angle, 90);
				angle += 90;
				gp.CloseAllFigures();
			}

			if (gp2 == null)
			{
				r.Width -= r.Height + 8;
				r.X += r.Height + 8;

				gp2 = new GraphicsPath();
				// top left
				gp2.AddArc(r.X, r.Y - radius, radius, radius, angle, 90);
				angle += 90;
				// top right
				gp2.AddArc(r.Right - radius, r.Y - radius,radius, radius, angle, 90);
				angle += 90;
				// bottom right
				gp2.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, angle, 90);
				angle += 90;
				// bottom left
				gp2.AddArc(r.X, r.Bottom - radius, radius, radius, angle, 90);

				gp2.CloseAllFigures();
			}
		}
			
			if (borderpen == null)
			{
				borderpen = new Pen(SystemColors.Highlight, 1);
				//borderpen.Alignment = PenAlignment.Center;
			}

      //System.Diagnostics.Trace.WriteLine(string.Format("{0}",e.State));

			bool selected = (e.State & DrawItemState.Selected) != 0;

			//normal AA looks bad
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			//e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			string word1 = Text;

			Rectangle r2 = e.Bounds;
			r2.Width = menuheight + 2;
			r2.X--;
			r2.Height += 2;
			r2.Y--;

			Rectangle bgr = e.Bounds;
			//bgr.Height +=1;
			//bgr.Y--;
			e.Graphics.FillRectangle(bgbrush,bgr);

			//e.DrawBackground();


			if (gradb == null)
			{
				LinearGradientBrush gb = new LinearGradientBrush(r2, 
					SystemColors.Control, SystemColors.ControlDark, 0f);
				gb.SetSigmaBellShape(0.9f,0.2f);
				gradb = gb;
			}

			e.Graphics.FillRectangle(gradb, r2);

			if (Checked)
			{
				Rectangle cr = e.Bounds;
        
				cr.Width -= r2.Width - 2;
				cr.X += r2.Width + 1;
				cr.Height -= 2;
				cr.Y++;
				if (chkb == null)
				{
					chkb =  new SolidBrush(Color.FromArgb(127, Color.Orange));
				}
				e.Graphics.FillPath( chkb, gp2);
				e.Graphics.DrawPath( chkpen, gp2);
			}
  			
			int h = menuheight;
			Brush b = Enabled ? normbrush : inactiveb;

			if (selected && Text != "-")
			{
				Rectangle r = e.Bounds;
				r.Width -= 1;
				r.Height -= 1;

				Rectangle r3 = r;
 
				r3.Width -= menuheight;
				r3.X += menuheight + 1;

				if (selbg == null)
				{
					r2.X -= r.Height/2;
					r2.Height *= 2;
					LinearGradientBrush gb = new LinearGradientBrush(r2, 
						Color.FromArgb(120, SystemColors.ControlLightLight),
						Color.FromArgb(120, SystemColors.Highlight), 90f);
					gb.SetSigmaBellShape(0.6f,0.9f);
					
					selbg = gb;
				}
        if (Enabled )
        {
          e.Graphics.FillPath(selbg, gp);
        }
				e.Graphics.DrawPath(borderpen, gp);
			}
			else
			{
        
			}

			if (Text == "-")
			{
				Rectangle r3 = e.Bounds;
				r3.Width -= menuheight;
				r3.X += menuheight + 1;
				//e.Graphics.FillRectangle(SystemBrushes.Menu, r3);

				Rectangle r = e.Bounds;
				r.Width = menuheight;

				e.Graphics.DrawLine(darkpen, r.Right + 5, e.Bounds.Y + 1, 
					e.Bounds.Right, e.Bounds.Y + 1);
			}
			else
			{
				Rectangle r = e.Bounds;

        
				r.Width = r.Height;
				r.X++;r.X++;
				r.Y++;r.Y++;
				r.X++;//r.X++;

				if (!selected)
				{
					//r.X++;
					//r.Y++;
				}

				IImageListProviderService ips = ServiceHost.ImageListProvider;
				if (ips != null && Tag != null)
				{
          int imageindex = -1;
          if (Tag is string)
          {
            string strtag = Tag as string;
            imageindex = ips[strtag];
          }
          else
          {
            imageindex = ips[Tag];
          }
					if (imageindex >= 0)
					{
						ips.ImageList.Draw(e.Graphics, r.Location, imageindex);
					}
				}

				if (!selected)
				{
					//r.X--;
					//r.Y--;
				}

				int hh = ((int)(float)(e.Bounds.Height - (fh - bh/2)*font.Height)/2);
        
				e.Graphics.DrawString(Text.Replace("&&", "||").Replace("&", string.Empty).Replace("||", "&"),
					font, 
					b,
					r.Right + 1, e.Bounds.Top  + hh);
			}
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			// 
			// PictureMenuItem
			// 
			this.OwnerDraw = true;

		}

    int GetWidth(Graphics g, string text)
    {
      SizeF s = g.MeasureString(text, font);
      return (int) s.Width;
    }

		protected override void OnMeasureItem(System.Windows.Forms.MeasureItemEventArgs e)
		{
			if (Text == "-")
			{
				e.ItemHeight = 3;
			}
			else
			{
				e.ItemHeight = (font.Height > menuheight)
					? font.Height : menuheight;
				if (e.ItemHeight < 20)
				{
					e.ItemHeight = 20;
				}

				e.ItemWidth = GetWidth(e.Graphics, Text) + e.ItemHeight * 2;

				if (e.ItemWidth < 100)
				{
					e.ItemWidth = 100;
				}
			}
		}
	}

}
