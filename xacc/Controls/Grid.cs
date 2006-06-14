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
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Data;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Xacc.ComponentModel;
using Xacc.Collections;
using Xacc.Controls.Design;

using IServiceProvider = System.IServiceProvider;

using LSharp;

namespace Xacc.Controls
{
	/// <summary>
	/// Summary description for Grid.
	/// </summary>
  class Grid : Control, IServiceProvider, IWindowsFormsEditorService, IEdit, IFile, INavigate, IHasUndo
  {
    #region Fields

    private System.ComponentModel.IContainer components = null;
    StringFormat sf = new StringFormat(StringFormatFlags.NoWrap);
		
    Hashtable cellstore = new Hashtable();
    Hashtable formatstore = new Hashtable();

    readonly LSharp.Environment environment = new LSharp.Environment(TopLoop.Environment);

    Pen borderpen = SystemPens.WindowFrame;

    Brush selbrush;
    static BinaryFormatter bf = new BinaryFormatter();
    static Regex txtparse = new Regex(@"
				(?<real>^-?(\d*\.\d+)$)|
				(?<int>^-?\d+$) |
				(?<creation>^\w+(\.\w+)*:.*$) |
				(?<text>^.+$)", 
      RegexOptions.ExplicitCapture | 
      RegexOptions.Compiled | 
      RegexOptions.IgnorePatternWhitespace);

    int padding = 2;
    int w = 150;
    int h;
    Pen selpen;
    Size size;

    Cell topleft = new Cell(0,0);
    Cell fareast = new Cell(0,0);
    Cell farsouth = new Cell(0,0);
    bool isediting = false;

    TextBox editingbox	= new TextBox();

    private System.Windows.Forms.ErrorProvider errorProvider1;

    Range selrange = new Range( new Cell(0,0), new Cell(0,0));

    readonly ITypeDescriptorContext context;

    #endregion

    #region Events

    public event EventHandler CornerClick;

    protected void OnCornerClick(object sender, EventArgs e)
    {
      if (CornerClick != null)
        CornerClick(this, e);
    }

    public delegate void CellEditorHandler(object sender, ref object obj);

    public event CellEditorHandler CellEdit;

    #endregion

    #region UI Handling

    protected override void OnResize(EventArgs e)
    {
      base.OnResize (e);
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

    #endregion

    #region Cell Service

    object IServiceProvider.GetService(Type serviceType)
    {
      if (serviceType == typeof(IWindowsFormsEditorService))
      {
        return this;
      }
      return null;
    }

    Panel editorpanel = new Panel();
    Control uieditor;

    void IWindowsFormsEditorService.DropDownControl(Control uieditor)
    {
      if (this.uieditor != null)
      {
        ((IWindowsFormsEditorService)this).CloseDropDown();
      }
      this.uieditor = uieditor;
      uieditor.Tag = selrange.InitialCell;
      //normally these editors are not font friendly, so keep it default, 
      //if they wanna change they can change from with in that control.
      //control.Font = g.Font;
      Rectangle rf = rf = GetInitialCell();
			
      editorpanel.Location = new Point(rf.X, rf.Bottom);
      editorpanel.Width = rf.Width > uieditor.Width ? rf.Width : uieditor.Width;
      editorpanel.Height = uieditor.Height;
      uieditor.Dock = DockStyle.Fill;
      uieditor.KeyDown +=new KeyEventHandler(uieditor_KeyDown);
      editorpanel.Controls.Add(uieditor);
      Controls.Add(editorpanel);
      editorpanel.Visible = true;
      uieditor.Select();

      while (editorpanel.Visible)
      {
        Application.DoEvents();
        Runtime.user32.MsgWaitForMultipleObjects(1, 0, true, 250, 0xff);
      }
    }

    void IWindowsFormsEditorService.CloseDropDown()
    {
      editorpanel.Visible = false;
      editorpanel.Controls.Remove(uieditor);
      Controls.Remove(editorpanel);
      
      uieditor = null;
      Invalidate(true);
    }

    DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
    {
      return 0;
    }

    void uieditor_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        e.Handled = true;
        ((IWindowsFormsEditorService)this).CloseDropDown();
      }
      else
        if (e.Alt && e.KeyCode == Keys.Enter)
      {
        e.Handled = true;
        ((IWindowsFormsEditorService)this).CloseDropDown();
      }
    }

    #endregion

    #region Cell / Range

    public Rectangle GetInitialCell()
    {
      return GetCell(selrange.InitialCell);
    }

    [Serializable]
      internal struct Cell : IComparable
    {
      int row;
      int col;

      public int Row
      {
        get { return row; }
        set 
        { 
          if (value >= 0)
            row = value; }
      }

      public int Col
      {
        get { return col; }
        set { if (value >= 0) col = value; }
      }

      public Cell(int row, int col)
      {
        this.row = row >= 0 ? row : 0;
        this.col = col >= 0 ? col : 0;
      }

      public Cell(string v)
      {
        row = col = 0;
        int i = 0;
        while (i < v.Length && Char.IsLetter(v[i]))
        {
          col *= 26;
          col += Char.ToLower(v[i]) - 'a';
          i++;
        }
        while (i < v.Length && Char.IsDigit(v[i]))
        {
          row *= 10;
          row += v[i] - '0';
          i++;
        }
        row--;
      }

      public static Cell operator ++(Cell c)
      {
        c.row++;
        return c;
      }

      public static Cell operator --(Cell c)
      {
        c.row--;
        return c;
      }

      public static Cell operator <<(Cell c, int i)
      {
        c.col -= i;
        return c;
      }

      public static Cell operator >>(Cell c, int i)
      {
        c.col += i;
        return c;
      }

#if DEBUG
      public string DebugInfo
      {
        get {return ToString();}
      }
#endif


      public override string ToString()
      {
        return string.Format("{0}{1}", (char) (col + 'a'), row + 1);
      }

      public int CompareTo(object o)
      {
        if (!(o is Cell))
        {
          return 1;
        }
        
        Cell a = (Cell) o;

        if (a.col == col)
        {
          return row.CompareTo(a.row);
        }
        
        return col.CompareTo(a.col);
      }
    }

    [Serializable]
    internal struct Range : IEnumerable
    {
      internal Cell start;
      internal Cell finish;

      public Range(Cell start, Cell finish)
      {
        this.start = start;
        this.finish = finish;
      }

      public Range(string v)
      {
        string[] tokens = v.Split('.');
        start = new Cell(tokens[0]);
        finish = new Cell(tokens[2]);
      }

#if DEBUG
      public string DebugInfo
      {
        get {return ToString();}
      }
#endif

      public bool Contains(Cell cell)
      {
        return (TopLeft.Col >= cell.Col && cell.Col <= BottomRight.Col) && 
          (TopLeft.Row >= cell.Row && cell.Row <= BottomRight.Row);
      }

      public Cell InitialCell
      {
        get {return start;}
      }

      public Cell TopLeft
      {
        get 
        {
          return new Cell(start.Row > finish.Row ? finish.Row : start.Row ,
                start.Col > finish.Col ? finish.Col : start.Col)	;	}
      }

      public Cell BottomRight
      {
        get 
        {
          return new Cell(start.Row < finish.Row ? finish.Row : start.Row ,
                start.Col < finish.Col ? finish.Col : start.Col)	;	}
      }

      public int Width
      {
        get {return System.Math.Abs(finish.Col - start.Col)  + 1;}
      }

      public int Height
      {
        get {return System.Math.Abs(finish.Row - start.Row)  + 1;}
      }

      public int Count
      {
        get {return Height * Width;}
      }

      public override string ToString()
      {
        return String.Format("{0}..{1}", TopLeft, BottomRight);
      }

      public IEnumerator GetEnumerator()
      {
        ArrayList enu = new ArrayList();
        for (int c = TopLeft.Col; c <= BottomRight.Col; c++)
        {
          for (int r = TopLeft.Row; r <= BottomRight.Row; r++)
          {
            enu.Add(new Cell(r,c));
          }
        }
        return enu.GetEnumerator();
      }
    }

    #endregion

    #region IFile Members

    int lastsavelevel = 0;

    public bool IsDirty
    {
      get
      {
        return undo.CurrentLevel != lastsavelevel;;
      }
    }

    void IDocument.Close()
    {
      
    }

    string IDocument.Info
    {
      get 
      {
        if (selrange.Count > 1)
        {
          return selrange.ToString();
        }
        else
        {
          return selrange.InitialCell + " : " + Printer.WriteToString(this[selrange.InitialCell]);
        }
      }
    }


    #endregion

    #region Clipboard

    readonly static Hashtable clipboard = new Hashtable();

    bool recording = true;
    readonly UndoRedoStack undo;

    /// <summary>
    /// Undo the last action. The state is as if you never had the action at all.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The behaviour is recursive.</item>
    /// <item>CanUndo is called from within function.</item>
    /// </list> 
    /// </remarks>
    public void Undo()
    {
      if (CanUndo)
      {
        recording = false;
        undo.Pop().CallUndo();
        recording = true;

        Invalidate();
      }
    }

    /// <summary>
    /// Redo the last undo action. The state is as if you never had the undo action at all.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The behaviour is recursive.</item>
    /// <item>CanRedo is called from within function.</item>
    /// </list> 
    /// </remarks>
    public void Redo()
    {
      if (CanRedo)
      {
        undo.CurrentLevel++;
        if (undo.Top != null)
        {
          recording = false;
          undo.Top.CallRedo();
          recording = true;
          Invalidate();
        }
      }
    }

    /// <summary>
    /// Checks if the TextBuffer can be undone to a previous state.
    /// </summary>
    /// <value>
    /// Returns true if possible.
    /// </value>
    public bool CanUndo
    {
      get {return undo.CanUndo;}
    }

    /// <summary>
    /// Checks if the TextBuffer can be redone to a previously undone state.
    /// </summary>
    /// <value>
    /// Returns true if possible.
    /// </value>
    public bool CanRedo
    {
      get {return undo.CanRedo;}
    }

    /// <summary>
    /// Clears the undo stack.
    /// </summary>
    public void ClearUndo()
    {
      undo.Clear();
    }

  
    object IHasUndo.GetUndoState()
    {
      return selrange;
    }

    void IHasUndo.RestoreUndoState(object state)
    {
      //selrange = (Range)state;
    }

    sealed class SetCellsOperation : Operation
    {
      Grid g;
      Range r;
      object[,] newvalue, oldvalue;

      public SetCellsOperation(Grid g, Range r, object[,] newvalue, object[,] oldvalue) : 
        base(null, null)
      {
        this.g = g;
        this.r = r;
        this.newvalue = newvalue;
        this.oldvalue = oldvalue;
      }

      protected override void Redo()
      {
        g.selrange = r;
        g.SelectedObjects = newvalue;
      }

      protected override void Undo()
      {
        g.selrange = r;
        g.SelectedObjects = oldvalue;
      }
    }

    sealed class SetCellOperation : Operation
    {
      Grid g;
      Cell c;
      object newvalue, oldvalue;

      public SetCellOperation(Grid g, Cell c, object newvalue, object oldvalue) : 
        base(null, null)
      {
        this.g = g;
        this.c = c;
        this.newvalue = newvalue;
        this.oldvalue = oldvalue;
      }

      protected override void Redo()
      {
        g[c] = newvalue;
      }

      protected override void Undo()
      {
        g[c] = oldvalue;
      }
    }


    [Serializable]
    class CellClip
    {
      public string value;
      public string typeconv;
    }

    
    [Serializable]
    class Reference
    {
      public Guid key = Guid.NewGuid();
    }

    [ClassInterface(ClassInterfaceType.None)]
    class CellDataObject : IDataObject
    {
      readonly Hashtable data = new Hashtable();

      public CellDataObject(Grid g)
      {
        object o = g.SelectedObject;

        if (g.SelectedObjects.Length > 1)
        {
          Reference r = new Reference();
          clipboard.Add(r.key, g.SelectedObjects);
          data[DataFormats.Serializable] = r;
        }
        else
        {
          CellClip cc = new CellClip();

          TypeConverter tc = TypeDescriptor.GetConverter(o);
          cc.typeconv = tc.GetType().AssemblyQualifiedName;

          if (tc is LSharpConverter)
          {
            cc.value = Printer.WriteToString(o);
          }
          else
          {
            cc.value = tc.ConvertToString(o);
          }
          data[DataFormats.Text]  = cc.value;
          data[DataFormats.Serializable] = cc;
        }
      }

      public bool GetDataPresent(Type format)
      {
        return (format == typeof(string));
      }

      public bool GetDataPresent(string format)
      {
        return GetDataPresent(format, false);
      }

      public bool GetDataPresent(string format, bool autoConvert)
      {
        return data.ContainsKey(format);
      }

      public object GetData(Type format)
      {
        if (format == typeof(string))
        {
          return data[DataFormats.Text];
        }
        return null;
      }

      public object GetData(string format)
      {
        return GetData(format, false);
      }

      public object GetData(string format, bool autoConvert)
      {
        return data[format];
      }

      static string[] formats = { DataFormats.Text, DataFormats.Rtf, DataFormats.Html };

      public string[] GetFormats()
      {
        return GetFormats(false);
      }

      public string[] GetFormats(bool autoConvert)
      {
        return formats;
      }

      public void SetData(object data)
      {
        if (data is string)
        {
          this.data[DataFormats.Text] = data;
        }
      }

      public void SetData(Type format, object data)
      {
        if (format == typeof(string))
        {
          this.data[DataFormats.Text] = data;
        }
      }

      public void SetData(string format, object data)
      {
        SetData(format, false, data);
      }

      public void SetData(string format, bool autoConvert, object data)
      {
        this.data[format] = data;
      }
    }

    public void Cut()
    {
      Clipboard.SetDataObject( new CellDataObject(this), false);
      DeleteSelected();
    }

    public void Copy()
    {
      Clipboard.SetDataObject( new CellDataObject(this), false);
    }

    public void Paste()
    {
      IDataObject cdo = Clipboard.GetDataObject();
      if (cdo != null)
      {
        object o = cdo.GetData(DataFormats.Serializable);
        if (o is CellClip)
        {
          CellClip cc = o as CellClip;
          Type t = Type.GetType(cc.typeconv);
          TypeConverter tc = Activator.CreateInstance(t) as TypeConverter;
          SelectedObject = tc.ConvertFrom(cc.value);
          LSharpUIEditor.conshistory[SelectedObject] = cc.value;
        }
        else if (o is Reference)
        {
          Reference r = o as Reference;
          object[,] so = clipboard[r.key] as object[,];

          SelectedObjects = so;
        }
        Invalidate();
      }
    }

    public void DeleteSelected()
    {
      SelectedObjects = new object[selrange.Width,selrange.Height];
      Invalidate();
    }

    void IEdit.SelectAll()
    {
      // TODO:  Add Grid.SelectAll implementation
    }

    #endregion

		#region Initialization/Dispose

		public Grid()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			errorProvider1.ContainerControl = FindForm();

			editingbox.BorderStyle = BorderStyle.Fixed3D;
			editingbox.AutoSize = false;
			editingbox.Font = Font;
      editingbox.VisibleChanged+=new EventHandler(editingbox_VisibleChanged);

      editorpanel.BorderStyle = BorderStyle.FixedSingle;
      editorpanel.Visible = false;

			editingbox.KeyDown +=new KeyEventHandler(tb_KeyUp);

			Color selcolor = SystemColors.Highlight;
			
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;
      sf.Trimming = StringTrimming.EllipsisWord;

			selpen = new Pen(selcolor, 1f);
			selpen.Alignment = PenAlignment.Outset;
			selpen.LineJoin = LineJoin.Round;

			selbrush = new SolidBrush(Color.FromArgb(40, selcolor));

			SetStyle( ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.StandardClick |
        ControlStyles.StandardClick | ControlStyles.ContainerControl |
				ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.Selectable , true);

			h = Font.Height + padding * 2;

      using (Graphics g = CreateGraphics())
        size = Size.Ceiling(g.MeasureString("000", Font));

      context = new GridContext(this);

      undo = new UndoRedoStack(this);

      environment.SymbolAssigned +=new EnvironmentEventHandler(environment_SymbolAssigned);
      environment.SymbolChanged +=new EnvironmentEventHandler(environment_SymbolChanged);
      environment.SymbolRemoved +=new EnvironmentEventHandler(environment_SymbolRemoved);
   }

		#endregion

    #region ITypeDescriptorContext

    class GridContext : ITypeDescriptorContext
    {
      readonly Grid grid;

      public GridContext(Grid g)
      {
        grid = g;
      }

      public void OnComponentChanged()
      {
        
      }

      public IContainer Container
      {
        get { return null; }
      }

      public bool OnComponentChanging()
      {
        return false;
      }

      public object Instance
      {
        get { return grid; }
      }

      public PropertyDescriptor PropertyDescriptor
      {
        get {return null;}
      }

      public object GetService(Type serviceType)
      {
        return ((IServiceProvider)grid).GetService(serviceType);
      }
    }

    internal LSharp.Environment Environment 
    {
      get {return environment;}
    }

    #endregion

		#region Grid Painting

		protected override void OnPaint(PaintEventArgs e)
		{
      base.OnPaint(e);
			DrawRules(e.Graphics);
		}
	
		void DrawRules(Graphics g)
		{
			//g.CompositingQuality = CompositingQuality.HighQuality;
			//g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			//g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			//g.SmoothingMode = SmoothingMode.HighSpeed;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

			sf.Alignment = StringAlignment.Center;

      //vertical band
      g.FillRectangle(SystemBrushes.Control, 0, 0, size.Width, Height);

      //horizontal band
      g.FillRectangle(SystemBrushes.Control, 0, 0, Width, h);

      Rectangle cell = CurrentSelection;

      //vertical selection
      g.FillRectangle(Brushes.DarkGray, 0, cell.Y, size.Width, cell.Height);

      //horizontal selection
      g.FillRectangle(Brushes.DarkGray, cell.X, 0, cell.Width, h);

      //horizontal lines ====
      int i = topleft.Row;
      for (float height = 0; height < Height; height += h, i++)
      {
        g.DrawLine(SystemPens.ControlDark, 0, height, Width, height);

        if (i != (topleft.Row))
        {
          RectangleF r = new RectangleF(0, height, size.Width, h);
          g.DrawString(i.ToString(), Font, SystemBrushes.ControlText, r, sf);
        }
      }

      //vertical lines ||||||
      char row = (char)(topleft.Col + 'A');
      for (float width = size.Width; width < Width; width += w, row++)
      {
        g.DrawLine(SystemPens.ControlDark, width, 0, width, Height);
        RectangleF r = new RectangleF(width, 0, w , h);
        g.DrawString(row.ToString(), Font, SystemBrushes.ControlText, r, sf);
      }

			//draw selected cell/range
			Rectangle sr = GetCell(selrange.InitialCell);
			cell = CurrentSelection;
			
			foreach (DictionaryEntry de in cellstore)
			{
				Cell c = (Cell)de.Key;
				Rectangle r = GetCell(c);

        UITypeEditor uieditor = TypeDescriptor.GetEditor(de.Value, typeof(UITypeEditor)) as UITypeEditor;
				TypeConverter typeconv = TypeDescriptor.GetConverter(de.Value);

        r.Inflate(-2,-2);
        int rh = r.Height;
        if (rh < 19)
        {
          rh = 19;
        }

        Rectangle rr = new Rectangle(r.X + rh, r.Y, r.Width - rh, r.Height);
        Rectangle rrr = new Rectangle(r.X, r.Y, rh, r.Height);

        //do the custom rendering process here, albeit call it
        if (typeconv.CanConvertTo(context, typeof(double)))
        {
          sf.Alignment = StringAlignment.Far;
        }
        else
        {
          sf.Alignment = StringAlignment.Near;
        }

        if (de.Value == null)
        {
          sf.Alignment = StringAlignment.Center;
          g.DrawString("Press F2 or double-click to edit", Font, Drawing.Factory.SolidBrush(SystemColors.GrayText), r, sf);
        }
        else
        {
          string sv = typeconv.ConvertTo(context, null, de.Value, typeof(string)) as string;
          if (de.Value is Cons || de.Value is Symbol)
          {
            object v = typeconv.ConvertTo(context, null, de.Value, typeof(object));
            if (v != null)
            {
              if (TypeDescriptor.GetConverter(v).CanConvertTo(context, typeof(double)))
              {
                sf.Alignment = StringAlignment.Far;
              }
              else
              {
                sf.Alignment = StringAlignment.Near;
              }
            }
          }
          if (uieditor != null && uieditor.GetPaintValueSupported())
          {
//            uieditor.PaintValue(de.Value, g, rrr);
//            g.DrawRectangle(SystemPens.WindowFrame, rrr);
//            g.DrawString(sv, Font, SystemBrushes.ControlText, rr, sf);
            Rectangle r2 = r;
            r2.Inflate(1,1);
            r2.Height++;
            r2.Width++;
            g.FillRectangle(SystemBrushes.Info, r2);
            g.DrawString(sv, Font, SystemBrushes.ControlText, r, sf);
          }
          else
          {
            g.DrawString(sv, Font, SystemBrushes.ControlText, r, sf);
          }
        }
			}

      Drawing.Utils.PaintLineHighlight(selbrush, selpen, g, cell.X-1, cell.Y-1, cell.Width+2, cell.Height+2, true);
		}

		#endregion

		#region Key Handling

    protected override void OnControlAdded(ControlEventArgs e)
    {
      if (e.Control == editingbox)
      {
        ServiceHost.State &= ~ApplicationState.Navigate;
      }
      base.OnControlAdded (e);
    }

    protected override void OnControlRemoved(ControlEventArgs e)
    {
      if (e.Control == editingbox)
      {
        ServiceHost.State |= ApplicationState.Navigate;
      }
      base.OnControlRemoved (e);
    }

    public void NavigateUp()
    {
      if (shift)
      {
        selrange.finish.Row--;
      }
      else
      {
        selrange.start.Row--;
        selrange.finish = selrange.start;
      }
      Invalidate();
    }

    public void NavigateDown()
    {
      if (shift)
      {
        selrange.finish.Row++;
      }
      else
      {
        selrange.start.Row++;
        selrange.finish = selrange.start;
      }
      Invalidate();
    }

    public void NavigateLeft()
    {
      if (shift)
      {
        selrange.finish.Col--;
      }
      else
      {
        selrange.start.Col--;
        selrange.finish = selrange.start;
      }
      Invalidate();
    }

    public void NavigateRight()
    {
      if (shift)
      {
        selrange.finish.Col++;
      }
      else
      {
        selrange.start.Col++;
        selrange.finish = selrange.start;
      }
      Invalidate();
    }

    public void NavigateHome()
    {
      Invalidate();
    }

    public void NavigateEnd()
    {
      Invalidate();
    }

    public void NavigatePageUp()
    {
      Invalidate();
    }

    public void NavigatePageDown()
    {
      Invalidate();
    }
    
    bool shift = false;

    protected override void OnKeyDown(KeyEventArgs e)
    {
      shift = e.Shift;

      switch (e.KeyCode)
      {
        case Keys.Right:
          if (e.Alt)
          {
            object p = SelectedObject;
            if (p == null)
            {
              p = Symbol.FromName(selrange.InitialCell.ToString());
            }
            OnCellEdit( ref p);
            e.Handled = true;
          }
          break;

        case Keys.Escape:
          if (editorpanel.Visible)
          {
            ((IWindowsFormsEditorService)this).CloseDropDown();
          }
          else
          {
            selrange.finish = selrange.InitialCell;
          }
          e.Handled = true;
          Invalidate();
          break;

        case Keys.F2:
          object o = this[selrange.InitialCell];
          OnCellEdit( ref o);
          e.Handled = true;
          break;

        case Keys.Delete:
          DeleteSelected();
          e.Handled = true;
          break;
      }
      base.OnKeyDown (e);
    }

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
      if (e.KeyChar >= ' ' && e.KeyChar != '=')
      {
        object o = e.KeyChar.ToString();
        OnCellEdit(ref o);
      }
      base.OnKeyPress (e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
      switch (e.KeyCode)
      {
        case Keys.ShiftKey:
          shift = false;
          break;
      }
			base.OnKeyUp (e);
		}

    private void tb_KeyUp(object sender, KeyEventArgs e)
    {
      Cell origcell = selrange.InitialCell;
      switch (e.KeyCode)
      {
        case Keys.Up:            
          selrange.start.Row--;
          selrange.finish = selrange.start;
          goto case Keys.Enter;

        case Keys.Down:
          selrange.start.Row++;
          selrange.finish = selrange.start;
          goto case Keys.Enter;

        case Keys.Enter:
          bool error = false;
          TextBox tb = sender as TextBox;


            
          Match m = txtparse.Match(tb.Text);
          object o = null;
          if (m.Groups["creation"].Success)
          {
            string[] tokens = tb.Text.Split(':');
            if (tokens.Length == 2 || tokens.Length == 1)
            {
              string typename = tokens[0].Trim();
              Type t = Type.GetType(typename, false, true);
              if (t == null)
              {
                bool found = false;
                CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
                foreach(Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                  if (found)
                    break;
                  if (ass is System.Reflection.Emit.AssemblyBuilder)
                    continue;
                  foreach (Type asst in ass.GetExportedTypes())
                  {
                    if (cic.Compare(typename,asst.Name) == 0)
                    {
                      t = asst;
                      found = true;
                      break;
                    }
                  }
                }
              }
              // force the exception assume the user is an idiot at all times
              if (t != null && !t.IsAbstract && !t.IsInterface)
              {
                try
                {
                  string s = tokens.Length < 2 ? "0" : tokens[1];
                  s = s.Trim();
                  TypeConverter tc = TypeDescriptor.GetConverter(t);
                  if (tc != null && tc.CanConvertFrom(typeof(string)))
                  {
                    if (s.Length == 0)
                      s = "0"; //default value
                    o = tc.ConvertFromString(s);
                  }
                  else
                  {
                    o = Activator.CreateInstance(t);
                  }
                }
                catch (Exception ex)
                {
                  errorProvider1.SetError(tb, ex.Message);
                  error = true;
                }
              }
              else
              {
                errorProvider1.SetError(tb, String.Format("Type: '{0}' was not found", typename));
                error = true;
              }
            }
          }

          else if (m.Groups["int"].Success)
          {
            o = Convert.ToInt32(tb.Text);
          }
          else if (m.Groups["real"].Success)
          {
            o = Convert.ToSingle(tb.Text);
          }
          else if (m.Groups["text"].Success)
          {
            o = tb.Text;
          }
          else
          {
            if (tb.Text.Trim().Length > 0)
            {
              //throw new FormatException("Insano user error!");
              errorProvider1.SetError(tb, String.Format("Formatting error"));
              error = true;
            }
          }

          if (o != null)
            this[origcell] = o;

          if (!error)
          {
            Controls.Remove(tb);
            isediting = false;
          }
          Invalidate();
          e.Handled = true;
          break;
        case Keys.Escape:
          Controls.Remove(sender as Control);
          isediting = false;
          e.Handled = true;
          break;
      }
    }

		#endregion

		#region Mouse Handling

    void editingbox_VisibleChanged(object sender, EventArgs e)
    {
      if (!editingbox.Visible)
      {
        errorProvider1.SetError(editingbox, "");
      }
    }

		protected override void OnDoubleClick(EventArgs e)
		{
      base.OnDoubleClick(e);

      object o = this[selrange.InitialCell];
      Capture = false; // HOLY MOSES!!!
			OnCellEdit(ref o);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (mousedown)
				{
					selrange.finish = GetCellAt(e.X, e.Y);
					Invalidate();
				}
			}
			base.OnMouseMove (e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel (e);
		}

    ContextMenuStrip contextmenu;

    protected override void OnGotFocus(EventArgs e)
    {
      base.OnGotFocus (e);

      if (contextmenu == null)
      {
        contextmenu = new ContextMenuStrip();

        ToolStripMenuItem mi = ServiceHost.Menu["Edit"];

        Hashtable attrmap = (ServiceHost.Menu as MenuService).GetAttributeMap(mi);

        foreach (ToolStripMenuItem m in mi.DropDownItems)
        {
          ToolStripMenuItem pmi = m as ToolStripMenuItem;
          if (pmi != null)
          {
            MenuItemAttribute mia = attrmap[pmi] as MenuItemAttribute;
            if (mia == null) 
            {
            }
            else
              if ((mia.State & (ApplicationState.Edit)) != 0)
            {
              contextmenu.Items.Add(m);
            }
          }
        }

        ContextMenuStrip = contextmenu;
      }
    }


		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mousedown = false;
				selrange.finish = GetCellAt(e.X, e.Y);
				Invalidate();
			}
			base.OnMouseUp (e);
		}

		volatile bool mousedown = false;

		protected override void OnMouseDown(MouseEventArgs e)
		{
      base.OnMouseDown (e);

      if (editorpanel.Visible)
      {
        ((IWindowsFormsEditorService)this).CloseDropDown();
      }

      if (!Focused)
      {
        Focus();
      }

			if (e.Button == MouseButtons.Left)
			{
        if (isediting)
        {
          bool error = false;
          TextBox tb = editingbox as TextBox;

          if (tb.Text.Length == 0)
          {
            Controls.Remove(tb);
            isediting = false;
          }
          else
          {
            Match m = txtparse.Match(tb.Text);
            object o = null;
            if (m.Groups["creation"].Success)
            {
              string[] tokens = tb.Text.Split(':');
              if (tokens.Length == 2 || tokens.Length == 1)
              {
                string typename = tokens[0].Trim();
                Type t = Type.GetType(typename, false, true);
                if (t == null)
                {
                  bool found = false;
                  CaseInsensitiveComparer cic = new CaseInsensitiveComparer();
                  foreach(Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
                  {
                    if (found)
                      break;
                    if (ass is System.Reflection.Emit.AssemblyBuilder)
                      continue;
                    foreach (Type asst in ass.GetExportedTypes())
                    {
                      if (cic.Compare(typename,asst.Name) == 0)
                      {
                        t = asst;
                        found = true;
                        break;
                      }
                    }
                  }
                }
                // force the exception assume the user is an idiot at all times
                if (t != null && !t.IsAbstract && !t.IsInterface)
                {
                  try
                  {
                    string s = tokens.Length < 2 ? "0" : tokens[1];
                    s = s.Trim();
                    TypeConverter tc = TypeDescriptor.GetConverter(t);
                    if (tc != null && tc.CanConvertFrom(typeof(string)))
                    {
                      if (s.Length == 0)
                        s = "0"; //default value
                      o = tc.ConvertFromString(s);
                    }
                    else
                    {
                      o = Activator.CreateInstance(t);
                    }
                  }
                  catch (Exception ex)
                  {
                    errorProvider1.SetError(tb, ex.Message);
                    error = true;
                  }
                }
                else
                {
                  errorProvider1.SetError(tb, String.Format("Type: '{0}' was not found", typename));
                  error = true;
                }
              }
            }

            else if (m.Groups["int"].Success)
            {
              o = Convert.ToInt32(tb.Text);
            }
            else if (m.Groups["real"].Success)
            {
              o = Convert.ToSingle(tb.Text);
            }
            else if (m.Groups["text"].Success)
            {
              o = tb.Text;
            }
            else
            {
              if (tb.Text.Trim().Length > 0)
              {
                //throw new FormatException("Insano user error!");
                errorProvider1.SetError(tb, String.Format("Formatting error"));
                error = true;
              }
            }

            if (o != null)
              this[selrange.InitialCell] = o;

            if (!error)
            {
              Controls.Remove(tb);
              isediting = false;
            }
          }
        }

				mousedown = true;

        if (shift)
        {
			    selrange.finish = GetCellAt(e.X, e.Y);
        }
        else
        {
          selrange.start = GetCellAt(e.X, e.Y);
          selrange.finish = selrange.start;
        }

				Invalidate();
			}
		}

		#endregion
		
		#region Cell Metrics

    public int CellHeight
    {
      get {return h;}
      set {h = value;}
    }

    public int CellWidth
    {
      get {return w;}
      set {w = value;}
    }

		public int VisibleColumns
		{
			get {return (int)(ClientSize.Width - size.Width)/(int)w ;}
		}

		public int VisibleRows
		{
			get {return (int)(ClientSize.Height - h - size.Height)/(int)h ;}
		}

		Rectangle GetRange(Range r)
		{
			return new Rectangle(size.Width + w * (r.TopLeft.Col - topleft.Col), 
				h * (r.TopLeft.Row - topleft.Row + 1), w * r.Width, h * r.Height);
		}

		Rectangle GetCell(Cell r)
		{
			return new Rectangle(size.Width + w * (r.Col - topleft.Col), h * (r.Row - topleft.Row + 1), w, h);
		}


		Rectangle SelectedCell
		{
			get {return GetCell(selrange.start);}
		}

		Rectangle CurrentSelection
		{
			get {return GetRange(selrange);}
		}

		void UpdateFar(Cell newmax)
		{
			UpdateFarEast(newmax);
			UpdateFarSouth(newmax);
		}

		void UpdateFarEast(Cell newmax)
		{
			if ( newmax.Col > fareast.Col)
				fareast = newmax;
		}

		void UpdateFarSouth(Cell newmax)
		{
			if ( newmax.Row > farsouth.Row)
				farsouth = newmax;
		}

		Cell FarSouthEast
		{
			get {return new Cell(farsouth.Row, fareast.Col);}
		}

		Cell BottomLeft
		{
			get {return new Cell(topleft.Row + VisibleRows, topleft.Col + VisibleColumns);}
		}

		Cell GetCellAt(int x, int y)
		{
			return new Cell((int)((y - h)/h) + topleft.Row, (int)((x - size.Width)/w) + topleft.Col);
		}

		#endregion

		#region Load/Save

		public void Save(string filename)
		{
      SuspendLayout();
			Stream s = File.Create(filename);
      TextWriter w = new StreamWriter(s);

      w.WriteLine("(=");

      ArrayList keys = new ArrayList(cellstore.Keys);
      keys.Sort();

      foreach (Cell c in keys)
      {
        object v = cellstore[c];

        if (v is Symbol || v is Cons)
        {
          w.WriteLine("  {0,-4} '{1}", c, Printer.WriteToString(v));
        }
        else
        {
          w.WriteLine("  {0,-4} {1}", c, Printer.WriteToString(v));
        }
      }

      w.WriteLine(")");

      lastsavelevel = undo.CurrentLevel;

      w.Flush();
      w.Close();
		}

		public void Open(string filename)
		{
      if (!File.Exists(filename))
      {
        System.Diagnostics.Trace.WriteLine("File does not exist: " + filename);
        return;
      }
			Stream s = null;
			try 
			{
				s = File.OpenRead(filename);

        TextReader r = new StreamReader(s);

        string t = r.ReadToEnd();
        s.Close();
        s = null;

        recording = false;
        object res = LSharp.Runtime.EvalString(t, environment);
        recording = true;

        selrange = new Range("a1..a1");

				foreach (Cell c in cellstore.Keys)
				{
					UpdateFar(c);
				}
        ResumeLayout();
				Invalidate();
			}
			catch (Exception ex)
			{
        System.Diagnostics.Trace.WriteLine(ex);
				if (s != null)
					s.Close();
			}
		}
		
		#endregion

		#region	External Cell Access

    public object this[string c]
    {
      get {return this[new Cell(c)];}
      set{this[new Cell(c)] = value;}
    }

		public int SelectedObjectCount
		{
			get {return selrange.Count;}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object SelectedObject
		{
			get {return this[selrange.InitialCell];}
      set {this[selrange.InitialCell] = value;}
		}

		public object[,] SelectedObjects
		{
			get 
			{
				//make this the enumerator
				object[,] so = new object[selrange.Width, selrange.Height];
				for (int x = 0; x < selrange.Width; x++)
				{
					for (int y = 0; y < selrange.Height; y++)
					{
						Cell c = new Cell( selrange.TopLeft.Row + y, selrange.TopLeft.Col + x);
						so[x,y] = this[c];
					}
				}
				return so;
			}
      set
      {
        object[,] so = value;

        Cell c = selrange.TopLeft;
        Cell end = new Cell(c.Row + so.GetLength(1) - 1, c.Col + so.GetLength(0) - 1);
        selrange = new Range(c, end);

        object[,] old = SelectedObjects;

        for (int j = 0; j < so.GetLength(0); j++, c = selrange.TopLeft >> j)
        {
          for (int i = 0; i < so.GetLength(1); i++, c++)
          {
            object ov = cellstore[c];
            object nv = so[j,i];

            if (!object.Equals(ov, nv))
            {
              if (nv == null && ov != null)
              {
                cellstore.Remove(c);
              }
              else
              {
                cellstore[c] = nv;
              }
            }
            environment.AssignLocal(Symbol.FromName(c.ToString()), nv);
          }
        }

        if (recording)
        {
          undo.Push( new SetCellsOperation(this, selrange , so, old));
        }
      }
		}
#if DEBUG
		public Hashtable WTF
		{
			get {return cellstore;}
		}
#endif

		#endregion

		#region Cell Editing

    void environment_SymbolAssigned(object sender, EnvironmentEventArgs e)
    {
      this[e.SymbolName] = e.NewValue;
      Invalidate();
    }

    void environment_SymbolChanged(object sender, EnvironmentEventArgs e)
    {
      this[e.SymbolName] = e.NewValue;
      Invalidate();
    }

    void environment_SymbolRemoved(object sender, EnvironmentEventArgs e)
    {
      this[e.SymbolName] = e.NewValue;
      Invalidate();
    }

    public object this[int col, int row]
    {
      get {return this[new Cell(row, col)];}
      set {this[new Cell(row, col)] = value;}
    }

    public event EventHandler CellValueChanged;

    object this[Cell c]
    {
      get {return cellstore[c];}
      set 
      {
        object ov = cellstore[c];

        if (!object.Equals(ov, value))
        {
          if (value == null && ov != null)
          {
            cellstore.Remove(c);
          }
          else
          {
            cellstore[c] = value;
          }
          environment.AssignLocal(Symbol.FromName(c.ToString()), value);
          if (CellValueChanged != null)
          {
            CellValueChanged(this, EventArgs.Empty);
          }
          if (recording)
          {
            undo.Push( new SetCellOperation(this, c, value, ov));
          }
          Invalidate();
        }
      }
    }

		protected void OnCellEdit(ref object obj)
		{
			//check object type etc
			if (obj == null)
			{
				//popup lame object creation dialog ..... maybe.....
				isediting = true;
				Rectangle cc = GetCell(selrange.InitialCell);
				cc.Inflate(1,1);
        cc.Height++;
        cc.Width++;
				editingbox.Text = "";
				editingbox.Bounds = cc;
					
				Controls.Add(editingbox);
				editingbox.Focus();
        editingbox.Select(editingbox.TextLength, 0);
			}
			else
			{
				Type type = obj.GetType();
				object uied = TypeDescriptor.GetEditor(type, typeof(UITypeEditor));
				TypeConverter tc = TypeDescriptor.GetConverter(obj);

				if (uied == null)
				{
					if (typeof(Enum).IsAssignableFrom(type))
					{
						if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
						{
							uied = new FlaggedEnumEditor();
						}
						else
						{
							uied = new EnumEditor();
						}
					}

					else if (tc != null && tc.GetStandardValuesSupported())
					{
						uied = new SupportedValuesEditor();
					}
				}

				if (uied != null)
				{
					UITypeEditor uie = uied as UITypeEditor;
  				this[selrange.InitialCell] = uie.EditValue(context, this, obj);
					return;
				}

				if (tc != null)
				{	
					if (tc.CanConvertFrom(typeof(string)) && tc.CanConvertTo(typeof(string)))
					{
						//display textbox;
						isediting = true;
						Rectangle cc = GetCell(selrange.InitialCell);
					  cc.Inflate(1,1);
            cc.Height++;
            cc.Width++;
						editingbox.Text = tc.ConvertTo(obj, typeof(string)) as string;
						editingbox.Tag = tc;
						editingbox.Bounds = cc;
					
						Controls.Add(editingbox);
						editingbox.Focus();
            editingbox.Select(editingbox.TextLength, 0);
					}
				}
				else if (CellEdit != null)
				{
					CellEdit(this, ref obj);
				}
			}
		}

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			// 
			// Grid
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Font = SystemInformation.MenuFont;
			this.Size = new System.Drawing.Size(448, 416);
		}
    #endregion

  }
}


