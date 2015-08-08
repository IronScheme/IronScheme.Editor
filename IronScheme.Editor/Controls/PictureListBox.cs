#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.Controls
{
  sealed class PictureListBox : ListBox
  {
    GraphicsPath gp = null;
    Pen borderpen = null;
    int radius = 1;

    static Font font = SystemInformation.MenuFont;
    static float fh = (float)font.FontFamily.GetCellAscent(0)/font.FontFamily.GetEmHeight(0);
    static float bh = (float)font.FontFamily.GetCellDescent(0)/font.FontFamily.GetEmHeight(0);
    static StringFormat sf;

    Brush selbg;

    private System.ComponentModel.IContainer components;


    static PictureListBox()
    {
      sf = new StringFormat(StringFormatFlags.NoWrap);
      sf.LineAlignment = StringAlignment.Center;
      sf.Trimming = StringTrimming.EllipsisCharacter;
    }

    public PictureListBox()
    {
      DrawMode = DrawMode.OwnerDrawFixed;
      ItemHeight = 18;
      IntegralHeight = true;

      SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
      UpdateStyles();

      InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (gp != null)
        {
          gp.Dispose();
        }
        if (selbg != null)
        {
          selbg.Dispose();
        }
        if (borderpen != null)
        {
          borderpen.Dispose();
        }
      }
      base.Dispose (disposing);
    }

    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
    }

    protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
    {
      if (e.Index < 0 || e.Index >= Items.Count)
      {
        return;
      }
			
      if (borderpen == null)
      {
        borderpen = new Pen(SystemColors.Highlight, 1);
      }

      bool selected = (e.State & DrawItemState.Selected) != 0;
      bool focus = (e.State & DrawItemState.Focus) != 0;

      //normal AA looks bad
      e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      e.Graphics.PixelOffsetMode = PixelOffsetMode.None;

      Rectangle r2 = e.Bounds;
      r2.Width = SystemInformation.MenuHeight + 2;
      r2.X--;

      Rectangle bounds = e.Bounds;
      bounds.Inflate(1,1);

      e.Graphics.FillRectangle(SystemBrushes.Window, bounds);

      int h = SystemInformation.MenuHeight;
      Brush b = SystemBrushes.ControlText;

      if (selected)
      {
        Rectangle r3 = e.Bounds;
 
        r3.Width -= SystemInformation.MenuHeight;
        r3.X += SystemInformation.MenuHeight + 1;

        if (gp == null)
        {
          Rectangle r = e.Bounds;
          int angle = 180;

          r.Inflate(-2, -1 - radius/2);
          r.Offset(-1, (radius/2));
          r.Height--;

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


        if (selbg == null)
        {
          //r2.X -= e.Bounds.Height/2;
          //r2.Height *= 2;
          //LinearGradientBrush gb = new LinearGradientBrush(r2, 
          //  Color.FromArgb(120, SystemColors.ControlLightLight),
          //  Color.FromArgb(120, SystemColors.Highlight), 90f);
          //gb.SetSigmaBellShape(0.6f,0.9f);
					
          selbg = SystemBrushes.Highlight.Clone() as Brush;
        }
        e.Graphics.FillPath(selbg, gp);
        
        if (selected)
        {
          e.Graphics.DrawPath(borderpen, gp);
        }

        selbg.Dispose();
        selbg = null;

        gp.Dispose();
        gp = null;
      }

      if (Items[e.Index] as string == "-")
      {
        Rectangle r3 = e.Bounds;
        r3.Width -= SystemInformation.MenuHeight;
        r3.X += SystemInformation.MenuHeight + 1;
        e.Graphics.FillRectangle(SystemBrushes.Menu, r3);

        Rectangle r = e.Bounds;
        r.Width = SystemInformation.MenuHeight;

        e.Graphics.DrawLine(SystemPens.ControlDark, r.Right + 5, e.Bounds.Y + 1, 
          e.Bounds.Right, e.Bounds.Y + 1);
      }
      else
      {
        Rectangle r = e.Bounds;
        r.Width = r.Height;
        r.X++;r.X++;
        r.Y++;

        object usr = Items[e.Index];

        int img = ServiceHost.ImageListProvider[usr];

        ServiceHost.ImageListProvider.ImageList.Draw(e.Graphics,r.Location, img);

        r = e.Bounds;
				
        r.X += r.Height;
        r.Width = r.Width - r.Height;
        
        e.Graphics.DrawString(usr.ToString(),
          font, 
          b,
          r,
          sf);
      }
    }

    // this is never called....
    protected override void OnMeasureItem(System.Windows.Forms.MeasureItemEventArgs e)
    {
    }
  }
}