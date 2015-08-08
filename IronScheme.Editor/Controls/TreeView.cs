#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


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
