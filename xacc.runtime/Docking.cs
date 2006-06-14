//#define MDI
#define WEIFENLUO
//#define TABCONTROL
//#define MDI
//#define NONE

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

#if WEIFENLUO
using W = WeifenLuo.WinFormsUI;
#endif

namespace Xacc.Runtime
{
  public enum DockState
  {
    Unknown = 0,
    Float = 1,
    DockTopAutoHide = 2,
    DockLeftAutoHide = 3,
    DockBottomAutoHide = 4,
    DockRightAutoHide = 5,
    Document = 6,
    DockTop = 7,
    DockLeft = 8,
    DockBottom = 9,
    DockRight = 10,
    Hidden = 11
  }

  public sealed class DockFactory
  {
    DockFactory(){}

    public static IDockContent Content()
    {
      return new DockContent();
    }

    public static IDockPanel Panel()
    {
      return new DockPanel();
    }

  }

  #region Concrete interfaces, DO NOT CHANGE!!!

  public interface IControl
  {
    Control.ControlCollection Controls {get;}
    void Hide();
    string Text {get;set;}
    object Tag {get;set;}
    bool AllowDrop {get;set;}
    void BringToFront();

    event DragEventHandler DragEnter;
    event DragEventHandler DragDrop;
  }

  public interface IDockContent : IControl
  {
    DockState DockState {get;}
    void Show();
    void Activate();
    void Close();
    Icon Icon {get;set;}
    bool HideOnClose {get;set;}
    ContextMenu TabPageContextMenu {get;set;}
    ContextMenuStrip TabPageContextMenuStrip { get;set;}
    void Show(IDockPanel panel, DockState state);
    event System.ComponentModel.CancelEventHandler Closing;
  }

  public interface IDockPanel : IControl
  {
    event EventHandler ActiveContentChanged;

    IDockContent ActiveContent {get;}
    IDockContent ActiveDocument {get;}
    IDockContent[] Documents {get;}

    void Save(string filename);
    void Load(string filename);
  }

  #endregion

#if WEIFENLUO
  class DockContent : W.DockContent, IDockContent
  {
    public new DockState DockState 
    {
      get {return (DockState) base.DockState;}
    }

    public void Show(IDockPanel panel, DockState state)
    {
      Show(panel as DockPanel, (W.DockState) state);
    }
  }

  class DockPanel : W.DockPanel, IDockPanel
  {
    public DockPanel()
    {
      ActiveAutoHideContent = null;
      BackColor = Color.FromArgb(245,245,245);
      Dock = DockStyle.Fill;
    }

    IDockContent[] IDockPanel.Documents
    {
      get {return new ArrayList(base.Documents).ToArray(typeof(IDockContent)) as IDockContent[];}
    }

    public void Save(string filename)
    {
      SaveAsXml(filename);
    }

    public void Load(string filename)
    {
      LoadFromXml(filename, null);
    }

    IDockContent IDockPanel.ActiveContent
    {
      get { return base.ActiveContent as IDockContent; }
    }

    IDockContent IDockPanel.ActiveDocument
    {
      get { return base.ActiveDocument as IDockContent; }
    }

  }
#endif

#if TABCONTROL
  class DockContent : TabPage, IDockContent
  {
    public DockState DockState 
    {
      get {return state;}
    }

    public DockContent()
    {

    }

    public void Activate()
    {
      this.Select();
    }

    public void Close()
    {
      bool cancel = false;
      if (Closing != null)
      {
        System.ComponentModel.CancelEventArgs e = new System.ComponentModel.CancelEventArgs();
        Closing(this, e);
        cancel = e.Cancel;
      }

      if (!cancel)
      {
        (Parent as TabControl).TabPages.Remove(this);
      }
    }

    public Icon Icon
    {
      get {return null;}
      set {;}
    }

    DockState state;
    bool hoc = false;

    public bool HideOnClose
    {
      get {return hoc;}
      set {hoc = value;}
    }

    public ContextMenu TabPageContextMenu
    {
      get {return ContextMenu;}
      set {ContextMenu = value;}
    }

    public void Show(IDockPanel panel, DockState state)
    {
      DockPanel dp = panel as DockPanel;
      TabControl target = null;

      switch (state)
      {
        case DockState.Document:
          target = dp.document;
          break;
        case DockState.DockLeft:
        case DockState.DockLeftAutoHide:
          target = dp.left;
          break;
        case DockState.DockRight:
        case DockState.DockRightAutoHide:
          target = dp.right;
          break;
        case DockState.DockTop:
        case DockState.DockTopAutoHide:
          target = dp.top;
          break;
        case DockState.DockBottom:
        case DockState.DockBottomAutoHide:
          target = dp.bottom;
          break;
      }

      if (target.TabPages.Contains(this))
      {
        target.SelectedTab = this;
      }
      else
      {
        target.TabPages.Add(this);
        target.SelectedTab = this;
      }

      if (target.TabPages.Count == 0 && target != dp.document)
      {
        target.Visible = false;
      }
      else
      {
        target.Visible = true;
      }

      this.state = state;

      dp.SelectedIndexChanged(dp, EventArgs.Empty);
    }

    public event System.ComponentModel.CancelEventHandler Closing;
  }

  class DockPanel : Panel, IDockPanel
  {
    public event EventHandler ActiveContentChanged;

    public TabControl document = new TabControl();
    public TabControl left = new TabControl();
    public TabControl right = new TabControl();
    public TabControl top = new TabControl();
    public TabControl bottom = new TabControl();

    public void SelectedIndexChanged(object sender, EventArgs e)
    {
      if (ActiveContentChanged != null)
      {
        ActiveContentChanged(this, e);
      }
    }

    public TabControl ActiveTabControl
    {
      get
      {
        foreach (TabControl tc in tabs)
        {
          if (tc.ContainsFocus)
          {
            return tc;
          }
        }
        return null;
      }
    }

    readonly Control[] tabs;

    public DockPanel()
    {
      Dock = DockStyle.Fill;

      document.Dock = DockStyle.Fill;
      left.Dock = DockStyle.Left;
      right.Dock = DockStyle.Right;
      top.Dock = DockStyle.Top;
      bottom.Dock = DockStyle.Bottom;

      Controls.AddRange( tabs = new Control[] { document, left, right, top, bottom });

      foreach (TabControl tc in tabs)
      {
        tc.SelectedIndexChanged +=new EventHandler(SelectedIndexChanged);
        tc.GotFocus +=new EventHandler(SelectedIndexChanged);
        tc.LostFocus +=new EventHandler(SelectedIndexChanged);
        tc.Visible = false;
        tc.Alignment = TabAlignment.Bottom;
      }

      top.Height = bottom.Height = 200;

      document.Visible = true;
      document.Alignment = TabAlignment.Top;
    }

    public IDockContent ActiveContent
    {
      get {return ActiveTabControl == null ? null : ActiveTabControl.SelectedTab as IDockContent;}
    }

    public IDockContent ActiveDocument
    {
      get {return ActiveTabControl == document ? null : document.SelectedTab as IDockContent;}
    }

    public IDockContent[] Documents
    {
      get {return new ArrayList(document.TabPages).ToArray(typeof(IDockContent)) as IDockContent[];}
    }

    public void Save(string filename)
    {
    }

    public void Load(string filename)
    {
    }
  }
#endif

#if MDI
  class DockContent : Form, IDockContent
  {
    public DockState DockState 
    {
      get {return state;}
    }

    public DockContent()
    {
      WindowState = FormWindowState.Maximized;
      KeyPreview = true;
    }

    protected override void OnGotFocus(EventArgs e)
    {
      base.OnGotFocus (e);
      (panel as DockPanel).OnActiveContentChanged(this);
    }

    protected override void OnLostFocus(EventArgs e)
    {
      (panel as DockPanel).OnActiveContentChanged(null);
      base.OnLostFocus (e);
    }



    DockState state;
    bool hoc = false;
    ContextMenu cm;

    public bool HideOnClose
    {
      get {return hoc;}
      set {hoc = value;}
    }

    public ContextMenu TabPageContextMenu
    {
      get {return cm;}
      set {cm = value;}
    }

    IDockPanel panel;

    public void Show(IDockPanel panel, DockState state)
    {
      this.panel = panel;
      Form p = (panel as DockPanel).FindForm();
      p.IsMdiContainer = true;
      this.MdiParent = p;

      Show();
    }

    private void DockContent_Activated(object sender, EventArgs e)
    {
      (panel as DockPanel).OnActiveContentChanged(this);
    }
  }

  class DockPanel : Panel, IDockPanel
  {
    public event EventHandler ActiveContentChanged;

    IDockContent current;

    public void OnActiveContentChanged(IDockContent current)
    {
      this.current = current;
      if (ActiveContentChanged != null)
      {
        ActiveContentChanged(this, EventArgs.Empty);
      }
    }

    public DockPanel()
    {
      Visible = false;
    }

    public IDockContent ActiveContent
    {
      get {return current;}
    }

    public IDockContent ActiveDocument
    {
      get {return current;}
    }

    public IDockContent[] Documents
    {
      get {return null;}
    }

    public void Save(string filename)
    {
    }

    public void Load(string filename)
    {
    }
  }
#endif

#if NONE


  class DockContent : Control, IDockContent
  {
    public DockState DockState 
    {
      get {return state;}
    }

    public DockContent()
    {

    }

    public void Activate()
    {

    }

    public void Close()
    {
      bool cancel = false;
      if (Closing != null)
      {
        System.ComponentModel.CancelEventArgs e = new System.ComponentModel.CancelEventArgs();
        Closing(this, e);
        cancel = e.Cancel;
      }

      if (!cancel)
      {
        
      }
    }

    public Icon Icon
    {
      get {return null;}
      set {;}
    }

    DockState state;
    bool hoc = false;
    ContextMenu cm;

    public bool HideOnClose
    {
      get {return hoc;}
      set {hoc = value;}
    }

    public ContextMenu TabPageContextMenu
    {
      get {return cm;}
      set {cm = value;}
    }

    public void Show(IDockPanel panel, DockState state)
    {

    }

    public event System.ComponentModel.CancelEventHandler Closing;
  }

  class DockPanel : Control, IDockPanel
  {
    public event EventHandler ActiveContentChanged;

    public DockPanel()
    {
      
    }

    public IDockContent ActiveContent
    {
      get {return null;}
    }

    public IDockContent ActiveDocument
    {
      get {return null;}
    }

    public IDockContent[] Documents
    {
      get {return null;}
    }

    public void Save(string filename)
    {
    }

    public void Load(string filename)
    {
    }
  }
#endif
}
