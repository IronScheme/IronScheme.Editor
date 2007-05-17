namespace Xacc.Controls
{
  partial class FileExplorer
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExplorer));
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
      this._treeView = new Aga.Controls.Tree.TreeViewAdv();
      this.treeColumn1 = new Aga.Controls.Tree.TreeColumn();
      this.treeColumn2 = new Aga.Controls.Tree.TreeColumn();
      this.treeColumn3 = new Aga.Controls.Tree.TreeColumn();
      this._icon = new Aga.Controls.Tree.NodeControls.NodeStateIcon();
      this._name = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this._size = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this._date = new Aga.Controls.Tree.NodeControls.NodeTextBox();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolStrip1
      // 
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.Size = new System.Drawing.Size(252, 25);
      this.toolStrip1.TabIndex = 1;
      this.toolStrip1.Text = "toolStrip1";
      // 
      // toolStripButton1
      // 
      this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
      this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton1.Name = "toolStripButton1";
      this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
      this.toolStripButton1.Text = "toolStripButton1";
      // 
      // toolStripButton2
      // 
      this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
      this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton2.Name = "toolStripButton2";
      this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
      this.toolStripButton2.Text = "toolStripButton2";
      // 
      // _treeView
      // 
      this._treeView.AllowColumnReorder = true;
      this._treeView.BackColor = System.Drawing.SystemColors.Window;
      this._treeView.Columns.Add(this.treeColumn1);
      this._treeView.Columns.Add(this.treeColumn2);
      this._treeView.Columns.Add(this.treeColumn3);
      this._treeView.Cursor = System.Windows.Forms.Cursors.Default;
      this._treeView.DefaultToolTipProvider = null;
      this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this._treeView.DragDropMarkColor = System.Drawing.Color.Black;
      this._treeView.FullRowSelect = true;
      this._treeView.LineColor = System.Drawing.SystemColors.ControlDark;
      this._treeView.LoadOnDemand = true;
      this._treeView.Location = new System.Drawing.Point(0, 25);
      this._treeView.Model = null;
      this._treeView.Name = "_treeView";
      this._treeView.NodeControls.Add(this._icon);
      this._treeView.NodeControls.Add(this._name);
      this._treeView.NodeControls.Add(this._size);
      this._treeView.NodeControls.Add(this._date);
      this._treeView.SelectedNode = null;
      this._treeView.ShowNodeToolTips = true;
      this._treeView.Size = new System.Drawing.Size(252, 500);
      this._treeView.TabIndex = 0;
      this._treeView.UseColumns = true;
      this._treeView.ColumnClicked += new System.EventHandler<Aga.Controls.Tree.TreeColumnEventArgs>(this._treeView_ColumnClicked);
      this._treeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._treeView_MouseClick);
      // 
      // treeColumn1
      // 
      this.treeColumn1.Header = "Name";
      this.treeColumn1.SortOrder = System.Windows.Forms.SortOrder.None;
      this.treeColumn1.Width = 250;
      // 
      // treeColumn2
      // 
      this.treeColumn2.Header = "Size";
      this.treeColumn2.IsVisible = false;
      this.treeColumn2.SortOrder = System.Windows.Forms.SortOrder.None;
      this.treeColumn2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.treeColumn2.Width = 100;
      // 
      // treeColumn3
      // 
      this.treeColumn3.Header = "Date";
      this.treeColumn3.SortOrder = System.Windows.Forms.SortOrder.None;
      this.treeColumn3.Width = 80;
      // 
      // _icon
      // 
      this._icon.DataPropertyName = "Icon";
      this._icon.IncrementalSearchEnabled = false;
      this._icon.ParentColumn = this.treeColumn1;
      // 
      // _name
      // 
      this._name.DataPropertyName = "Name";
      this._name.EditEnabled = true;
      this._name.ParentColumn = this.treeColumn1;
      this._name.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
      // 
      // _size
      // 
      this._size.DataPropertyName = "Size";
      this._size.ParentColumn = this.treeColumn2;
      this._size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // _date
      // 
      this._date.DataPropertyName = "Date";
      this._date.ParentColumn = this.treeColumn3;
      // 
      // FileExplorer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._treeView);
      this.Controls.Add(this.toolStrip1);
      this.Name = "FileExplorer";
      this.Size = new System.Drawing.Size(252, 525);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Aga.Controls.Tree.NodeControls.NodeStateIcon _icon;
    private Aga.Controls.Tree.NodeControls.NodeTextBox _name;
    private Aga.Controls.Tree.NodeControls.NodeTextBox _size;
    private Aga.Controls.Tree.NodeControls.NodeTextBox _date;
    private Aga.Controls.Tree.TreeColumn treeColumn1;
    private Aga.Controls.Tree.TreeColumn treeColumn2;
    private Aga.Controls.Tree.TreeColumn treeColumn3;

    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButton1;
    private System.Windows.Forms.ToolStripButton toolStripButton2;
    private Aga.Controls.Tree.TreeViewAdv _treeView;
  }
}
