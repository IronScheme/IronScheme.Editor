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
using Xacc.Build;
using Xacc.Controls;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;
using Xacc.Collections;

namespace Xacc.Controls
{
  class ErrorView : ListView
  {
    ImageList images = new ImageList();

    public ErrorView()
    {
      FullRowSelect = true;
      GridLines = true;
      HeaderStyle = ColumnHeaderStyle.Nonclickable;
      HideSelection = false;
      BorderStyle = BorderStyle.None;
  
      images.ColorDepth = ColorDepth.Depth32Bit;
      images.ImageSize = new Size(16,16);
      images.TransparentColor = Color.Transparent;

      ContextMenuStrip = new ContextMenuStrip();
      ContextMenuStrip.Items.Add(new ToolStripMenuItem("Clear Errors", null,new EventHandler(Clear)));

      Assembly ass = typeof(AdvancedTextBox).Assembly;
				
#if VS
      Stream s = ass.GetManifestResourceStream("IronScheme.Editor.Resources.resultstrip.png");
#else
			Stream s = ass.GetManifestResourceStream("resultstrip.png");
#endif	
      Image i = Bitmap.FromStream(s);
      images.Images.AddStrip(i);
      // out of memory bug again
      //s.Close();
				
      View = View.Details;
      Dock = DockStyle.Bottom;
      Columns.Add("Message"	, 650, 0);
      Columns.Add("File"		, 150, 0);
      Columns.Add("Line"		,  70, 0);
			
      SmallImageList = ServiceHost.ImageListProvider.ImageList;
      StateImageList = images;

      this.DoubleClick +=new EventHandler(ErrorView_DoubleClick);
      //this.VisibleChanged +=new EventHandler(ErrorView_SizeChanged);

      //this.SizeChanged +=new EventHandler(ErrorView_SizeChanged);

      Dock = DockStyle.Fill;
    }

    void Clear(object sender, EventArgs e)
    {
      ClearErrors(null);
    }

    delegate void VOIDVOID(object caller);

    public void ClearErrors(object caller)
    {
      if (InvokeRequired)
      {
        Invoke(new VOIDVOID(ClearErrors), new object[] {caller});
      }
      else
      {
        if (caller == null)
        {
          lasterrmap.Clear();
          Items.Clear();
        }
        else
        {
          Hashtable lasterrors = lasterrmap[caller] as Hashtable;
          if (lasterrors != null)
          {
            foreach (ListViewItem lvi in lasterrors.Values)
            {
              Items.Remove(lvi);
            }
            lasterrors.Clear();
          }
        }
        
      }
    }

    delegate void VOIDVOID2(object caller, params ActionResult[] results);

    Hashtable lasterrmap = new Hashtable();

    public void OutputErrors(object caller, params ActionResult[] results)
    {
      if (InvokeRequired)
      {
        Invoke(new VOIDVOID2(OutputErrors), new object[] {caller, results});
      }
      else
      {
        foreach (ActionResult ar in results)
        {
          if (ar.Message != null)
          {
            Hashtable lasterrors = lasterrmap[caller] as Hashtable;

            if (lasterrors == null)
            {
              lasterrmap.Add(caller, lasterrors = new Hashtable());
            }

            if (lasterrors.Contains(ar))
            {
              continue;
            }
          
            ListViewItem lvi = new ListViewItem(ar.Message.TrimEnd('\r'));
            lvi.Tag = ar;

            lasterrors.Add(ar, lvi);
					
            switch (ar.Type)
            {
              case ActionResultType.Info:
                lvi.StateImageIndex = 1; //green
                break;
              case ActionResultType.Ok:
                lvi.StateImageIndex = 0; //blue
                break;
              case ActionResultType.Warning:
                lvi.StateImageIndex = 2; //orange
                break;
              case ActionResultType.Error:
                lvi.StateImageIndex = 3; //red
                break;
            }
            lvi.ImageIndex = ServiceHost.ImageListProvider[caller];
            lvi.SubItems.Add(ar.Location.Filename);
            lvi.SubItems.Add(ar.Location.LineNumber == 0 ? string.Empty : ar.Location.LineNumber.ToString() 
              + (ar.Location.Column == 0 ? string.Empty : (":" + ar.Location.Column)));
            Items.Add(lvi);
            lvi.EnsureVisible();
          }
        }

        //ErrorView_SizeChanged(this, EventArgs.Empty);
      }
    }

    void ErrorView_DoubleClick(object sender, EventArgs e)
    {
      if (SelectedItems.Count == 1)
      {
        ActionResult ar = (ActionResult) SelectedItems[0].Tag;

        if (!File.Exists(ar.Location.Filename))
        {
          return;
        }
        
        AdvancedTextBox atb = (ServiceHost.Project.Current == null ?
          ServiceHost.File.Open(ar.Location.Filename)
          : ServiceHost.Project.Current.OpenFile(ar.Location.Filename)) as AdvancedTextBox;

        if (atb != null)
        {
          if (ar.Location.LineNumber > 0)
          {
            atb.Buffer.SelectLocation(ar.Location);
            atb.ScrollToCaret();
          }
          ServiceHost.File.BringToFront(atb);
        }
      }
    }

//    void ErrorView_SizeChanged(object sender, EventArgs e)
//    {
//      Columns[0].Width = Width - Columns[1].Width - 
//        Columns[2].Width - ((Items.Count > 6) ? SystemInformation.VerticalScrollBarWidth : 0);
//
//      if (Items.Count > 0)
//      {
//        Items[Items.Count - 1].EnsureVisible();
//      }
//    }
  }
}
