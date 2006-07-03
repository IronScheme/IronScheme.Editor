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
      System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Node1");
      System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Node5");
      System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Node6");
      System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Node7");
      System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Node2", new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode3,
            treeNode4});
      System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Node3");
      System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Node4");
      System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Node0", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode5,
            treeNode6,
            treeNode7});
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExplorer));
      this.treeView1 = new System.Windows.Forms.TreeView();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
      this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
      this.toolStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView1
      // 
      this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView1.LabelEdit = true;
      this.treeView1.Location = new System.Drawing.Point(0, 25);
      this.treeView1.Name = "treeView1";
      treeNode1.Name = "Node1";
      treeNode1.Text = "Node1";
      treeNode2.Name = "Node5";
      treeNode2.Text = "Node5";
      treeNode3.Name = "Node6";
      treeNode3.Text = "Node6";
      treeNode4.Name = "Node7";
      treeNode4.Text = "Node7";
      treeNode5.Name = "Node2";
      treeNode5.Text = "Node2";
      treeNode6.Name = "Node3";
      treeNode6.Text = "Node3";
      treeNode7.Name = "Node4";
      treeNode7.Text = "Node4";
      treeNode8.Name = "Node0";
      treeNode8.Text = "Node0";
      this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode8});
      this.treeView1.ShowNodeToolTips = true;
      this.treeView1.Size = new System.Drawing.Size(252, 500);
      this.treeView1.TabIndex = 0;
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
      // FileExplorer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.treeView1);
      this.Controls.Add(this.toolStrip1);
      this.Name = "FileExplorer";
      this.Size = new System.Drawing.Size(252, 525);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TreeView treeView1;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripButton toolStripButton1;
    private System.Windows.Forms.ToolStripButton toolStripButton2;
  }
}
