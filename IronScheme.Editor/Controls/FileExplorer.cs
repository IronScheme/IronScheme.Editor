using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

using Aga.Controls.Tree.NodeControls;
using Aga.Controls.Tree;
using System.Collections;
using System.Threading;

namespace Xacc.Controls
{
  partial class FileExplorer : UserControl
  {
    public FileExplorer()
    {
      InitializeComponent();

      _name.ToolTipProvider = new ToolTipProvider();
			_name.EditorShowing += new CancelEventHandler(_name_EditorShowing);

			_treeView.Model = new SortedTreeModel(new FolderBrowserModel());
    }

    private class ToolTipProvider: IToolTipProvider
		{
			public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
			{
				if (node.Tag is RootItem)
					return null;
				else
					return "Second click to rename node";
			}
		}

		void _name_EditorShowing(object sender, CancelEventArgs e)
		{
			if (_treeView.CurrentNode.Tag is RootItem)
				e.Cancel = true;
		}

		private void _treeView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				NodeControlInfo info = _treeView.GetNodeControlInfoAt(e.Location);
				if (info.Control != null)
				{
					Console.WriteLine(info.Bounds);
				}
			}
		}

		private void _treeView_ColumnClicked(object sender, TreeColumnEventArgs e)
		{
			TreeColumn clicked = e.Column;
			if (clicked.SortOrder == SortOrder.Ascending)
				clicked.SortOrder = SortOrder.Descending;
			else
				clicked.SortOrder = SortOrder.Ascending;

			(_treeView.Model as SortedTreeModel).Comparer = new FolderItemSorter(clicked.Header, clicked.SortOrder);
		}
  }

  abstract class BaseItem
  {
    private string _path = "";
    public string ItemPath
    {
      get { return _path; }
      set { _path = value; }
    }

    private Image _icon;
    public Image Icon
    {
      get { return _icon; }
      set { _icon = value; }
    }

    private long _size = 0;
    public long Size
    {
      get { return _size; }
      set { _size = value; }
    }

    private DateTime? _date;
    public DateTime? Date
    {
      get { return _date; }
      set { _date = value; }
    }

    public abstract string Name
    {
      get;
      set;
    }

    private BaseItem _parent;
    public BaseItem Parent
    {
      get { return _parent; }
      set { _parent = value; }
    }

    private bool _isChecked;
    public bool IsChecked
    {
      get { return _isChecked; }
      set
      {
        _isChecked = value;
        if (Owner != null)
          Owner.OnNodesChanged(this);
      }
    }

    private FolderBrowserModel _owner;
    public FolderBrowserModel Owner
    {
      get { return _owner; }
      set { _owner = value; }
    }

    /*public override bool Equals(object obj)
    {
      if (obj is BaseItem)
        return _path.Equals((obj as BaseItem).ItemPath);
      else
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return _path.GetHashCode();
    }*/

    public override string ToString()
    {
      return _path;
    }
  }

  class RootItem : BaseItem
  {
    public RootItem(string name, FolderBrowserModel owner)
    {
      ItemPath = name;
      Owner = owner;
    }

    public override string Name
    {
      get
      {
        return ItemPath;
      }
      set
      {
      }
    }
  }

  class FolderItem : BaseItem
  {
    public override string Name
    {
      get
      {
        return Path.GetFileName(ItemPath);
      }
      set
      {
        string dir = Path.GetDirectoryName(ItemPath);
        string destination = Path.Combine(dir, value);
        Directory.Move(ItemPath, destination);
        ItemPath = destination;
      }
    }

    public FolderItem(string name, BaseItem parent, FolderBrowserModel owner)
    {
      ItemPath = name;
      Parent = parent;
      Owner = owner;
    }
  }

  class FileItem : BaseItem
  {
    public override string Name
    {
      get
      {
        return Path.GetFileName(ItemPath);
      }
      set
      {
        string dir = Path.GetDirectoryName(ItemPath);
        string destination = Path.Combine(dir, value);
        File.Move(ItemPath, destination);
        ItemPath = destination;
      }
    }

    public FileItem(string name, BaseItem parent, FolderBrowserModel owner)
    {
      ItemPath = name;
      Parent = parent;
      Owner = owner;
    }
  }

  class FolderBrowserModel : ITreeModel
  {
    private BackgroundWorker _worker;
    private List<BaseItem> _itemsToRead;
    private Dictionary<string, List<BaseItem>> _cache = new Dictionary<string, List<BaseItem>>();

    public FolderBrowserModel()
    {
      _itemsToRead = new List<BaseItem>();

      _worker = new BackgroundWorker();
      _worker.WorkerReportsProgress = true;
      _worker.DoWork += new DoWorkEventHandler(ReadFilesProperties);
      _worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
    }

    static Dictionary<string, Image> icocache = new Dictionary<string, Image>();

    void ReadFilesProperties(object sender, DoWorkEventArgs e)
    {
      while (_itemsToRead.Count > 0)
      {
        BaseItem item = _itemsToRead[0];
        _itemsToRead.RemoveAt(0);

        if (item is FolderItem)
        {
          DirectoryInfo info = new DirectoryInfo(item.ItemPath);
          item.Date = info.CreationTime;
        }
        else if (item is FileItem)
        {
          FileInfo info = new FileInfo(item.ItemPath);
          item.Size = info.Length;
          item.Date = info.CreationTime;

          //string ext = Path.GetExtension(item.ItemPath);

          //if (icocache.ContainsKey(ext))
          //{
          //  item.Icon = icocache[ext];
          //}
          //else
          //{
          //  Icon ico = Icon.ExtractAssociatedIcon(item.ItemPath);
          //  Image b = ico.ToBitmap();
          //  b = b.GetThumbnailImage(16, 16, null, IntPtr.Zero);
          //  item.Icon = icocache[ext] = b;
          //}
        }
        _worker.ReportProgress(0, item);
      }
    }

    void ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      OnNodesChanged(e.UserState as BaseItem);
    }

    private TreePath GetPath(BaseItem item)
    {
      if (item == null)
        return TreePath.Empty;
      else
      {
        Stack<object> stack = new Stack<object>();
        while (item != null)
        {
          stack.Push(item);
          item = item.Parent;
        }
        return new TreePath(stack.ToArray());
      }
    }

    public System.Collections.IEnumerable GetChildren(TreePath treePath)
    {
      List<BaseItem> items = null;
      if (treePath.IsEmpty())
      {
        if (_cache.ContainsKey("ROOT"))
          items = _cache["ROOT"];
        else
        {
          items = new List<BaseItem>();
          _cache.Add("ROOT", items);
          foreach (string str in Environment.GetLogicalDrives())
          {
            DriveInfo di = new DriveInfo(str);
            if (di.DriveType == DriveType.Unknown || di.DriveType == DriveType.Network)
            {
            }
            else if (!di.IsReady)
            {
            }
            else
            {
              RootItem ri = new RootItem(str, this);
              items.Add(ri);
            }
          }
        }
      }
      else
      {
        BaseItem parent = treePath.LastNode as BaseItem;
        if (parent != null)
        {
          if (_cache.ContainsKey(parent.ItemPath))
            items = _cache[parent.ItemPath];
          else
          {
            items = new List<BaseItem>();
            try
            {
              foreach (string str in Directory.GetDirectories(parent.ItemPath))
              {
                FileAttributes fa = File.GetAttributes(str);
                if ((fa & FileAttributes.Hidden) == 0 && 0 == (FileAttributes.System & fa))
                {
                  items.Add(new FolderItem(str, parent, this));
                }
              }
              foreach (string str in Directory.GetFiles(parent.ItemPath))
              {
                FileAttributes fa = File.GetAttributes(str);
                if ((fa & FileAttributes.Hidden) == 0 && 0 == (FileAttributes.System & fa))
                {
                  items.Add(new FileItem(str, parent, this));
                }
              }
            }
            catch (IOException)
            {
              return null;
            }
            _cache.Add(parent.ItemPath, items);
            _itemsToRead.AddRange(items);
            if (!_worker.IsBusy)
              _worker.RunWorkerAsync();
          }
        }
      }
      return items;
    }

    public bool IsLeaf(TreePath treePath)
    {
      return treePath.LastNode is FileItem;
    }

    public event EventHandler<TreeModelEventArgs> NodesChanged;
    internal void OnNodesChanged(BaseItem item)
    {
      if (NodesChanged != null)
      {
        TreePath path = GetPath(item.Parent);
        NodesChanged(this, new TreeModelEventArgs(path, new object[] { item }));
      }
    }

    public event EventHandler<TreeModelEventArgs> NodesInserted;
    public event EventHandler<TreeModelEventArgs> NodesRemoved;
    public event EventHandler<TreePathEventArgs> StructureChanged;
    public void OnStructureChanged()
    {
      if (StructureChanged != null)
        StructureChanged(this, new TreePathEventArgs());
    }
  }

  class FolderItemSorter : IComparer
  {
    private string _mode;
    private SortOrder _order;

    public FolderItemSorter(string mode, SortOrder order)
    {
      _mode = mode;
      _order = order;
    }

    public int Compare(object x, object y)
    {
      BaseItem a = x as BaseItem;
      BaseItem b = y as BaseItem;
      int res = 0;

      if (a is FolderItem && b is FileItem)
      {
        return -1;
      }
      else if (b is FolderItem && a is FileItem)
      {
        return 1;
      }
      else
      {
        if (a is FileItem && b is FileItem)
        {
          if (_mode == "Date")
            res = DateTime.Compare(a.Date.Value, b.Date.Value);
          else if (_mode == "Size")
          {
            if (a.Size < b.Size)
              res = -1;
            else if (a.Size > b.Size)
              res = 1;
          }
          else
            res = string.Compare(a.Name, b.Name);
        }
        else
        {
          res = string.Compare(a.Name, b.Name);
        }
      }

      if (a is FileItem && b is FileItem)
      {
        if (_order == SortOrder.Ascending)
          return -res;
        else
          return res;
      }
      else
      {
        return res;
      }
    }

    private string GetData(object x)
    {
      return (x as BaseItem).Name;
    }
  }


}
