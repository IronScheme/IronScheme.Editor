namespace Xacc.Controls
{
  partial class NavigationBar
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
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.classes = new Xacc.Controls.PictureComboBox();
      this.members = new Xacc.Controls.PictureComboBox();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.classes);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.members);
      this.splitContainer1.Size = new System.Drawing.Size(935, 25);
      this.splitContainer1.SplitterDistance = 470;
      this.splitContainer1.SplitterWidth = 3;
      this.splitContainer1.TabIndex = 1;
      // 
      // classes
      // 
      this.classes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.classes.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.classes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.classes.FormattingEnabled = true;
      this.classes.ItemHeight = 19;
      this.classes.Location = new System.Drawing.Point(0, 0);
      this.classes.Name = "classes";
      this.classes.Size = new System.Drawing.Size(470, 25);
      this.classes.TabIndex = 0;
      // 
      // members
      // 
      this.members.Dock = System.Windows.Forms.DockStyle.Fill;
      this.members.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.members.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.members.FormattingEnabled = true;
      this.members.ItemHeight = 19;
      this.members.Location = new System.Drawing.Point(0, 0);
      this.members.Name = "members";
      this.members.Size = new System.Drawing.Size(462, 25);
      this.members.TabIndex = 0;
      // 
      // NavigationBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.Name = "NavigationBar";
      this.Size = new System.Drawing.Size(935, 25);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private PictureComboBox classes;
    private PictureComboBox members;

  }
}
