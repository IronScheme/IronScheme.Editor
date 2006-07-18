using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Xacc.Controls
{
  public partial class FileExplorer : UserControl
  {
    public FileExplorer()
    {
      InitializeComponent();
      treeView1.ImageList = images;
      images.ImageSize = new Size(16, 16);
      images.ColorDepth = ColorDepth.Depth32Bit;
      images.Images.Add(ComponentModel.ServiceHost.ImageListProvider.GetImage("Folder.Open.png"));
      images.Images.Add(ComponentModel.ServiceHost.ImageListProvider.GetImage("Folder.Closed.png"));
      images.Images.Add(ComponentModel.ServiceHost.ImageListProvider.GetImage("File.Type.NA.png"));
    }

    string folder;

    public string Folder
    {
      get { return folder; }
      set 
      {
        if (folder != value)
        {
          folder = value;

          TreeNode root = new TreeNode(folder);
          root.SelectedImageIndex = root.ImageIndex = 1;

          treeView1.Nodes.Clear();
          treeView1.Nodes.Add(root);

          root.Expand();

          AddFolder(folder, root);
        }
      }
    }

    Dictionary<string, int> extmap = new Dictionary<string, int>();

    ImageList images = new ImageList();

    void AddFolder(string folder, TreeNode parent)
    {
      foreach (string dir in Directory.GetDirectories(folder))
      {
        TreeNode dirnode = new TreeNode(Path.GetFileName(dir));

        dirnode.SelectedImageIndex = dirnode.ImageIndex = 1;

        parent.Nodes.Add(dirnode);
        AddFolder(dir, dirnode);
      }

      foreach (string file in Directory.GetFiles(folder))
      {
        TreeNode filenode = new TreeNode(Path.GetFileName(file));

        try
        {
          string ext = Path.GetExtension(file);
          if (extmap.ContainsKey(ext))
          {
            filenode.SelectedImageIndex = filenode.ImageIndex = extmap[ext];
          }
          else
          {
            extmap[ext] = images.Images.Count;
            images.Images.Add(Icon.ExtractAssociatedIcon(file));
            filenode.SelectedImageIndex = filenode.ImageIndex = images.Images.Count - 1;
          }
          
        }
        catch
        {
          filenode.SelectedImageIndex = filenode.ImageIndex = 2;
        }

        filenode.Tag = file;
        
        parent.Nodes.Add(filenode);
      }
    }

    void treeView1_DoubleClick(object sender, EventArgs e)
    {
      TreeNode n = treeView1.SelectedNode;
      if (n != null)
      {
        ComponentModel.ServiceHost.File.Open(n.Tag as string);
      }
    }

    void treeView1_MouseDown(object sender, MouseEventArgs e)
    {
      treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
    }
  }
}
