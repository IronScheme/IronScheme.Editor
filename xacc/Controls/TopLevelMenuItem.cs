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
	class TopLevelMenuItem : System.Windows.Forms.MenuItem
	{
		System.ComponentModel.IContainer components;
    static Font font = SystemInformation.MenuFont;
    
    static float fh = (float)font.FontFamily.GetCellAscent(0)/font.FontFamily.GetEmHeight(0);
    static float bh = (float)font.FontFamily.GetCellDescent(0)/font.FontFamily.GetEmHeight(0);

		object tag = null;

		public override MenuItem CloneMenu()
		{
			TopLevelMenuItem pmi = new TopLevelMenuItem(Text);
			pmi.Tag = tag;
			return pmi;
		}

		public override void MergeMenu(Menu menuSrc)
		{
			if (menuSrc is ContextMenu)
			{
				MenuItems.Clear();
				foreach (MenuItem mi in menuSrc.MenuItems)
				{
					MenuItems.Add(mi.CloneMenu());
				}
			}
		}


		public TopLevelMenuItem(string text) : base(text)
		{
			components = new System.ComponentModel.Container();
			InitializeComponent();
		}
    
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				gp.Dispose();
				selbg.Dispose();
				selbg2.Dispose();
				borderpen.Dispose();
			}
			base.Dispose (disposing);
		}

		GraphicsPath gp = null;
		//Pen p = null;
		//Brush fillbrush = null;

		int radius = 4;

		Brush selbg;
		Brush selbg2;
		Brush gradb = SystemBrushes.Control;
		Pen borderpen = null;

		protected sealed override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
      if (gp != null)
      {
        gp.Dispose();
        gp = null;
      }

			if (e.Index < 0)
			{
				return;
			}

			if (gp == null)
			{
				Rectangle r = e.Bounds;
				int angle = 180;

				r.Inflate(-1, -radius/2 - 2);
				r.Offset(0, (radius/2) );

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

				gp.CloseAllFigures();
			}

			if (borderpen == null)
			{
				borderpen = new Pen(SystemColors.Highlight, 1);
				borderpen.Alignment = PenAlignment.Center;
			}

			bool selected = (e.State & DrawItemState.Selected) != 0;
			bool hotlite	= (e.State & DrawItemState.HotLight) != 0;
			
			e.Graphics.FillRectangle(gradb, e.Bounds);

			//normal AA looks bad
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
  			
			int h = SystemInformation.MenuHeight;
			Brush b = SystemBrushes.ControlText;

			if ((selected) && Text != string.Empty)
			{
				Rectangle r = e.Bounds;
				r.Width -= 1;
				r.Height -= 2;
				r.Y++;

				if (selbg == null)
				{
					Rectangle r2 = e.Bounds;
					r2.X -= r.Height/2;
					r2.Height *= 2;
					LinearGradientBrush gb = new LinearGradientBrush(r2, 
						Color.FromArgb(120, SystemColors.ControlLightLight),
						Color.FromArgb(120, SystemColors.Highlight), 90f);
					gb.SetSigmaBellShape(0.6f,0.9f);
					
					selbg = gb;
				}
				e.Graphics.FillPath(selbg, gp);
				e.Graphics.DrawPath(borderpen, gp);
			}

			else if ((hotlite) && Text != string.Empty)
			{
				Rectangle r = e.Bounds;
				r.Width -= 1;
				r.Height -= 2;
				r.Y++;

				if (selbg2 == null)
				{
					Rectangle r2 = e.Bounds;
					r2.X -= r.Height/2;
					r2.Height *= 2;
					LinearGradientBrush gb = new LinearGradientBrush(r2, 
						Color.FromArgb(120, SystemColors.Highlight),
						Color.FromArgb(120, SystemColors.ControlLightLight), 90f);
					
					//gb.RotateTransform(180);
					gb.SetSigmaBellShape(0.3f,0.9f);
					
					selbg2 = gb;
				}
				e.Graphics.FillPath(selbg2, gp);
				e.Graphics.DrawPath(borderpen, gp);
			}


			int hh = ((int)(float)(e.Bounds.Height - (fh - bh/2)*font.Height)/2);
        
			e.Graphics.DrawString(Text.Replace("&&", "||").Replace("&", string.Empty).Replace("||", "&"),
				font, 
				b,
				e.Bounds.Left + 2, e.Bounds.Top  + hh);
		}

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			// 
			// PictureMenuItem
			// 
			this.OwnerDraw = true;

		}

		protected override void OnMeasureItem(System.Windows.Forms.MeasureItemEventArgs e)
		{
			Font f = font;

			e.ItemHeight = (f.Height > SystemInformation.MenuHeight)
				? f.Height : SystemInformation.MenuHeight;

			//has no effect it appears
			if (e.ItemHeight < 20)
			{
				e.ItemHeight = 20;
			}

			if (Text == string.Empty)
			{
				e.ItemWidth = 100;
			}
			else
			{
				SizeF s = e.Graphics.MeasureString(Text, f);

				e.ItemWidth = (int) s.Width;
			}
		}
	}

}
