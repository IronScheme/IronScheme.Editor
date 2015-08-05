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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.Controls
{
	/// <summary>
	/// Summary description for TreeView.
	/// </summary>
	class TreeView : System.Windows.Forms.TreeView
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TreeView()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
      
      IImageListProviderService ips = ServiceHost.ImageListProvider;
      if (ips != null)
      {
        ImageList = ips.ImageList;
      }
      else
      {
        ImageList = new ImageList();
        ImageList.ColorDepth = ColorDepth.Depth32Bit;
      }

		}

    protected override void OnMouseDown(MouseEventArgs e)
    {
      SelectedNode = GetNodeAt(e.X, e.Y);
      base.OnMouseDown (e);
    }

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
