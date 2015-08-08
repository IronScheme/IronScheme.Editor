#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Defines
/*  Defines (off by default):
 *  =========================
 *  CHECKED				- enable extra debugging & inline checks (very slow!)
 *	AUTOCOMPLETE  - i need a working parser 1st
 *  FOLDBAR				- enable foldbar (WIP)
 *  IVIZ					- IVizualizer interface (idea....)
 *  PARSER				- The parser framework (not done)
 *  FASTMEASURE		- measurestring replacement (active)
 *	NEWSTYLEPAINT - an attempt to less calls to DrawString, needs work
 */
//#define BACKGROUNDLEXER 
//#define AUTOCOMPLETE	//broken
//#define DUMPTOKENS
#define BROKEN
//#define CHECKED
#endregion

#region Includes

using System;

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;
using System.Diagnostics;
using IronScheme.Editor.ComponentModel;
using IronScheme.Editor.Collections;
using System.Runtime.InteropServices;
using IronScheme.Editor.Languages;
using IronScheme.Editor.Drawing;
using IronScheme.Editor.CodeModel;
#endregion

namespace IronScheme.Editor.Controls
{
  public partial class AdvancedTextBox
  {

    #region TextBuffer
    /// <summary>
    /// Provides Text services.
    /// </summary>
    public sealed class TextBuffer : Disposable, Language.IParserCallback, IHasUndo
    {
      #region Data Structures

      [StructLayout(LayoutKind.Sequential, Pack = 4)]
      struct LineState
      {
        string intline;

        public string line
        {
          get { return intline; }
        }

        public bool SetLine(string value, TextBuffer buffer, int linenr)
        {
          if (intline != value || userstate.Tokens == null)
          {
            intline = value;
            bool res = buffer.DoLex(ref this, linenr);
            drawcache = null;
            return res;
          }
          return false;
        }

        public TokenLine userstate;
        public DrawInfo[] drawcache;
      }

      [StructLayout(LayoutKind.Auto, Pack = 1)]
      internal class DrawInfo : IDrawInfo
      {
        public float start;
        public float end;
        public string text;

        static readonly Hashtable infomap = new Hashtable();
        static readonly ArrayList infotab = new ArrayList();

        byte fc, bc, st, bd;

        public Color forecolor
        {
          get
          {
            return (Color)infotab[fc];
          }
          set
          {
            if (value == Color.Transparent)
            {
              value = Color.Empty;
            }
            int i = 0;
            if (!infomap.ContainsKey(value))
            {
              i = infomap.Count;
              infomap.Add(value, i);
              infotab.Add(value);
            }
            else
            {
              i = (int)infomap[value];
            }

            fc = (byte)i;
          }
        }

        public Color backcolor
        {
          get
          {
            return (Color)infotab[bc];
          }
          set
          {
            if (value == Color.Transparent)
            {
              value = Color.Empty;
            }
            int i = 0;
            if (!infomap.ContainsKey(value))
            {
              i = infomap.Count;
              infomap.Add(value, i);
              infotab.Add(value);
            }
            else
            {
              i = (int)infomap[value];
            }

            bc = (byte)i;
          }
        }

        public Color bordercolor
        {
          get
          {
            return (Color)infotab[bd];
          }
          set
          {
            if (value == Color.Transparent)
            {
              value = Color.Empty;
            }
            int i = 0;
            if (!infomap.ContainsKey(value))
            {
              i = infomap.Count;
              infomap.Add(value, i);
              infotab.Add(value);
            }
            else
            {
              i = (int)infomap[value];
            }

            bd = (byte)i;
          }
        }

        public FontStyle style
        {
          get
          {
            return (FontStyle)st;
          }
          set
          {
            st = (byte)value;
          }
        }

        internal static float fontwidth = 0;

        public int Start
        {
          get { return (int)Math.Round(start / fontwidth); }
        }

        public int End
        {
          get { return (int)Math.Round(end / fontwidth); }
        }

        public string Text
        {
          get { return text; }
        }

        public Color ForeColor
        {
          get { return forecolor; }
        }

        public Color BackColor
        {
          get { return backcolor; }
        }

        public Color BorderColor
        {
          get { return bordercolor; }
        }

        public FontStyle Style
        {
          get { return style; }
        }
      }

      struct LineHint
      {
        public readonly int line;
        public readonly int index;

        public LineHint(int line, int index)
        {
          this.line = line;
          this.index = index;
        }

        public override string ToString()
        {
          return String.Format("line = {0} index = {1}", line, index);
        }
      }

      internal class TokenLine
      {
        IToken[] tokens = null;
        internal Stack state = null;

        public bool CompareState(Stack state)
        {
          Stack s1 = this.state;
          Stack s2 = state;

          if (s1 == null || s2 == null)
          {
            return false;
          }
          else
          {
            if (s1.Count != s2.Count)
            {
              return false;
            }

            for (IEnumerator r1 = s1.GetEnumerator(), r2 = s2.GetEnumerator();
              r1.MoveNext() & r2.MoveNext(); )
            {
              if (!r1.Current.Equals(r2.Current))
              {
                return false;
              }
            }
            return true;
          }
        }

        //[Browsable(false)]
        public IToken[] Tokens
        {
          get { return tokens; }
          set
          {
            if (tokens != value)
            {
#if DEBUG
              if (tokens != null)
              {
                //Console.WriteLine("{0}: Resetting tokens", Hashcode);
              }
#endif
              tokens = value;
            }
          }
        }
#if DEBUG
        public string StackString
        {
          get
          {
            string s = string.Empty;
            foreach (int i in state as Stack)
            {
              s += "[" + i + "]";
            }
            return s;
          }
        }
#endif

        public int LineLength
        {
          get
          {
            if (tokens == null)
            {
              return 0;
            }
            return tokens.Length;
          }
        }

#if DEBUG
        public int Hashcode
        {
          get { return GetHashCode(); }
        }
#endif
      }

      #endregion

      #region Token Support

      readonly object linelock = new object();

      void InsertLine(TokenLine newline, TokenLine before)
      {
        //lock(linelock)
        {
          if (before == null)
          {
            if (mlines.First == null)
            {
              Debug.Assert(mlines.Count == 0);
              mlines.Add(newline);
            }
            else
            {
              mlines.InsertBefore(mlines.First, newline);
            }
          }
          else
          {
            DoubleLinkedList<TokenLine>.IPosition iprev = mlines.PositionOf(before);
            if (iprev == null)
            {
              mlines.Add(newline);
            }
            else
            {
              mlines.InsertAfter(iprev, newline);
            }
          }
        }
      }

      void AddLine(TokenLine newline)
      {
        //lock(linelock)
        {
          mlines.Add(newline);
        }
      }

      internal TokenLine InsertLineAfter(TokenLine state)
      {
        TokenLine tl = new TokenLine();
        InsertLine(tl, state as TokenLine);
        return tl;
      }

      internal void RemoveLine(TokenLine state)
      {
        //lock(linelock)
        {
          mlines.Remove(state);
        }
      }

      #endregion

      #region Undo / Redo

      readonly object stacklock = new object();

      object IHasUndo.GetUndoState()
      {
        TextBufferState state = new TextBufferState();
        state.caretindex = caretindex;
        state.selstart = selectionstart;
        return state;
      }

      void IHasUndo.RestoreUndoState(object state)
      {
        CaretIndex = ((TextBufferState)state).caretindex;
        selectionstart = ((TextBufferState)state).selstart;
      }

      /// <summary>
      /// Stores a minimun snapshot of the TextBuffer state
      /// </summary>
      class TextBufferState
      {
        public int caretindex, selstart;
      }

      abstract class BufferOperation : Operation
      {
        internal protected string value;

        public TextBuffer Buffer
        {
          get { return (TextBuffer)buffer; }
        }

        protected BufferOperation(object before, object after, string value)
          : base(before, after)
        {
          this.value = value;
        }
      }

      sealed class InsertOperation : BufferOperation
      {
        public InsertOperation(object before, object after, string value)
          : base(before, after, value) { }

        protected override void Redo()
        {
          if (value.Length == 1)
          {
            if (value == "\n")
            {
              Buffer.InsertLineAfterCaret();
            }
            else
            {
              Buffer.InsertCharacter(value[0]);
            }
          }
          else
          {
            Buffer.InsertString(value);
          }
        }

        protected override void Undo()
        {
          Buffer.Select(Buffer.CaretIndex - value.Length, value.Length);
          Buffer.RemoveSelection();
        }

        public override string ToString()
        {
          return string.Format("Insert({0})", value);
        }
      }

      sealed class RemoveOperation : BufferOperation
      {
        public RemoveOperation(object before, object after, string value)
          : base(before, after, value) { }

        protected override void Redo()
        {
          Buffer.Select(Buffer.CaretIndex - value.Length, value.Length);
          Buffer.RemoveSelection();
        }

        protected override void Undo()
        {
          if (value.Length == 1)
          {
            if (value == "\n")
            {
              Buffer.InsertLineAfterCaret();
            }
            else
            {
              Buffer.InsertCharacter(value[0]);
            }
          }
          else
          {
            Buffer.InsertString(value);
          }
        }

        public override string ToString()
        {
          return string.Format("Remove({0})", value);
        }
      }

      sealed class RemoveLineOperation : BufferOperation
      {
        int linenr;

        public RemoveLineOperation(object before, object after, string value, int linenr)
          : base(before, after, value) 
        {
          this.linenr = linenr;
        }

        protected override void Redo()
        {
          Buffer.Remove(linenr);
        }

        protected override void Undo()
        {
          Buffer.Insert(linenr, value);
        }

        public override string ToString()
        {
          return string.Format("RemoveLine({0})", value);
        }
      }

      sealed class ReplaceLineOperation : BufferOperation
      {
        string oldvalue, newvalue;

        public ReplaceLineOperation(object before, object after, string oldvalue, string newvalue)
          : base(before, after, newvalue) //fix
        {
          this.oldvalue = oldvalue;
          this.newvalue = newvalue;
        }

        protected override void Redo()
        {
          Buffer.SetLine(Buffer.CurrentLine, newvalue);
        }

        protected override void Undo()
        {
          Buffer.SetLine(Buffer.CurrentLine, oldvalue);
        }

        public override string ToString()
        {
          return string.Format("ReplaceLine({0},{1})", oldvalue, newvalue);
        }
      }

      sealed class ReplaceLinesOperation : BufferOperation
      {
        string[] oldvalue, newvalue;

        public ReplaceLinesOperation(object before, object after, string[] oldvalue, string[] newvalue)
          : base(before, after, string.Empty) //fix
        {
          this.oldvalue = oldvalue;
          this.newvalue = newvalue;
        }

        protected override void Redo()
        {
          Buffer.SetLines(Buffer.currentline - newvalue.Length + 1, newvalue);
        }

        protected override void Undo()
        {
          Buffer.SetLines(Buffer.currentline - oldvalue.Length + 1, oldvalue);
        }

        public override string ToString()
        {
          return string.Format("ReplaceLines({0},{1})", oldvalue, newvalue);
        }
      }

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
        get { return undo.CanUndo; }
      }

      /// <summary>
      /// Checks if the TextBuffer can be redone to a previously undone state.
      /// </summary>
      /// <value>
      /// Returns true if possible.
      /// </value>
      public bool CanRedo
      {
        get { return undo.CanRedo; }
      }

      /// <summary>
      /// Clears the undo stack.
      /// </summary>
      public void ClearUndo()
      {
        undo.Clear();
      }

      #endregion

      #region Tabs

#if CHECKED
			[TypeConverter(typeof(ExpandableObjectConverter))]
			public StringFormat StringFormat
			{
				get {return sf;}
			}
#endif
      /// <summary>
      /// If true, converts TAB charaters to spaces on input.
      /// </summary>
      public bool TabsToSpaces
      {
        get { return tabtospace; }
        set { tabtospace = value; }
      }

      /// <summary>
      /// Gets or sets the tabs size.
      /// </summary>
      /// <value>
      /// The tab size in measured in number of spaces.
      /// </value>
      public int TabSize
      {
        get { return tabsize; }
        set
        {
          if (tabsize != value)
          {
            tabs = null;
            tabsize = value;
            AdjustTabs();
          }
        }
      }

      /// <summary>
      /// Determine of tabs has been initialized
      /// </summary>
      public bool Initialized
      {
        get { return tabs != null; }
      }

      internal bool IsMonospaced(Font font, out float width)
      {
        using (Bitmap b = new Bitmap(100, 100))
        {
          using (Graphics g = Graphics.FromImage(b))
          {
            return IsMonospaced(g, font, out width);
          }
        }
      }

      internal void AdjustTabs()
      {
        using (Bitmap b = new Bitmap(100, 100))
        {
          using (Graphics g = Graphics.FromImage(b))
          {
            AdjustTabs(g);
          }
        }
      }

      void AdjustTabs(Graphics g)
      {
        const float first = 0;

        tabs = new float[120];
        DrawInfo.fontwidth = fontwidth = (float)Math.Round(g.MeasureString("//////////", font, MAX24BIT, sf).Width / 10, 0);

        float tabsizef = fontwidth * tabsize;

        for (int i = 0; i < tabs.Length; i++)
        {
          tabs[i] = tabsizef;
        }

        sf.SetTabStops(first, tabs);
      }

      #endregion

      #region Metrics

      /// <summary>
      /// The current (caret) index. This can value can range from 0 (zero) to TextLength inclusive.<br />
      /// Seen otherwise: 0 &lt;= CaretIndex &lt;= TextLength .
      /// </summary>
      /// <value>
      /// The caret index.
      /// </value>
      public int CaretIndex
      {
        get { return caretindex; }
        set
        {
          if (value >= 0)
          {
            if (value > TextLength)
              value = TextLength;
            int ci;
            caretindex = value;
            owner.caretvisible = true;
            GetInfoFromCaretIndex(caretindex, out currentline, out ci);
            if (!select)
              selectionstart = caretindex;
          }
        }
      }

      void GetInfoFromCaretIndex(int caretindex, out int line, out int charindex, int hintline, int hintindex)
      {
        int oldindex = caretindex;
        charindex = 0;

        if (hintline > LineCount)
        {
          hintline = hintindex = 0;
        }

        if (caretindex >= hintindex)
        {
          caretindex = caretindex - hintindex;
          for (line = hintline; line < LineCount; line++)
          {
            int l = this[line].Length + nllen;
            caretindex -= l;
            if (caretindex < 0)
            {
              charindex = caretindex + l;
#if CHECKED
							int cl, ci;
							TestGetInfoFromCaretIndex(oldindex, out cl, out ci);
							Debug.Assert(cl == line && ci == charindex, "GetInfoFromCaretIndex",
								String.Format("cl\t\t== {0}\nline\t\t== {1}\nci\t\t== {2}\ncharindex\t\t== {3}", 
								cl ,line, ci, charindex));
#endif
              return;
            }
          }
          line--;
        }
        else
        {
          caretindex = hintindex - caretindex;
          for (line = hintline - 1; line >= 0; line--)
          {
            int l = this[line].Length + nllen;
            caretindex -= l;
            if (caretindex <= 0)
            {
              charindex = -caretindex;
#if CHECKED
							int cl, ci;
							TestGetInfoFromCaretIndex(oldindex, out cl, out ci);
							Debug.Assert(cl == line && ci == charindex, "GetInfoFromCaretIndex",
								String.Format("cl\t\t== {0}\nline\t\t== {1}\nci\t\t== {2}\ncharindex\t\t== {3}", 
								cl ,line, ci, charindex));
#endif
              return;
            }
          }
          line++;
        }
      }

      internal void GetInfoFromCaretIndex(int caretindex, out int line, out int charindex)
      {
        GetInfoFromCaretIndex(caretindex, out line, out charindex, linehint.line, linehint.index);
      }

#if CHECKED
      void TestGetInfoFromCaretIndex(int caretindex, out int line, out int charindex)
      {
				charindex = 0;
				for (line = 0; line < LineCount; line++)
				{
					int l = this[line].Length + nllen;
					caretindex -= l;
					if (caretindex < 0)
					{
						charindex = caretindex + l;
						return;
					}
				}
				if (line > 0)
					line--;
      }
#endif

      int GetCaretIndexFromLine(int line, int hintline, int hintindex)
      {
        int index = hintindex;
        if (line >= hintline)
        {
          for (int i = hintline; i < LineCount; i++)
          {
            if (i == line)
              break;
            index += this[i].Length + nllen;
          }
        }
        else if (hintline >= LineCount)
        {
          return GetCaretIndexFromLine(line, 0, 0);
        }
        else
        {
          for (int i = hintline - 1; i >= 0; i--)
          {
            index -= this[i].Length + nllen;
            if (i == line)
            {
              break;
            }
          }
        }
#if CHECKED
				int res = TestGetCaretIndexFromLine(line);

				Debug.Assert(res == index, "GetCaretIndexFromLine", 
					String.Format("res == {0} index == {1}", res, index));
#endif
        return index;
      }

      /// <summary>
      /// Returns the caret index at the start of a given line.
      /// </summary>
      /// <param name="line">the line number</param>
      /// <returns>the caret index at the start of the line</returns>
      public int GetCaretIndexFromLine(int line)
      {
        LineHint lh = GetBestLineHint(line);
        int res = GetCaretIndexFromLine(line, lh.line, lh.index);
        return res;
      }

#if CHECKED
			int TestGetCaretIndexFromLine(int line)
			{
				int index = 0;
				for (int i = 0; i < LineCount; i++)
				{
					if (i == line)
						return index;
					index += this[i].Length + nllen;
				}
				return index;
			}
#endif

      LineHint GetBestLineHint(int line)
      {
        if (linehint.line / 2 > line)
        {
          return STARTHINT;
        }
        if ((LineCount - linehint.line / 2) < (LineCount - line))
        {
          return new LineHint(LineCount - 1, textlength - this[LineCount - 1].Length - nllen);
        }
        return linehint;
      }

      LineHint GetBestLineHintIndex(int index)
      {
        if (linehint.index / 2 > index)
        {
          return STARTHINT;
        }
        if ((textlength - this[LineCount - 1].Length - linehint.index) / 2 < index)
        {
          return new LineHint(LineCount - 1, textlength - this[LineCount - 1].Length - nllen);
        }
        return linehint;
      }

      /// <summary>
      /// Gets the current line of the caret. The index is zero based.
      /// </summary>
      /// <value>
      /// The current line.
      /// </value>
      public int CurrentLine
      {
        get
        {
#if CHECKED
          int cl, ci;
          TestGetInfoFromCaretIndex(caretindex, out cl, out ci);

          int cl2, ci2;
          TestGetInfoFromCaretIndex(caretindex - 1, out cl2, out ci2);

          Debug.Assert(cl == currentline, "CurrentLine", String.Format("Expected: {0} != {1} :Actual", cl ,currentline));
          return cl;
#else
          return currentline;
#endif
        }
        set
        {
          if (currentline != value)
          {
            if (value < 0)
            {
              CurrentLine = 0;
            }
            else if (value < LineCount)
            {
              int coli = LineColumnIndex, ci;
              CaretIndex = GetCaretIndexFromLine(value);
              LineColumnIndex = coli;
              //update the currentline field
              GetInfoFromCaretIndex(caretindex, out this.currentline, out ci);
#if DEBUG
              object state = GetUserState(currentline);
              if (state == null)
              {
                state = new object();
              }
#endif
            }
            else
            {
              CurrentLine = LineCount - 1;
            }

            if (!select)
              selectionstart = CaretIndex;


          }
        }
      }

      /// <summary>
      /// The current line character index of the caret.
      /// </summary>
      [ReadOnly(true)]
      public int LineCharacterIndex
      {
        get
        {
          int cl, ci;
          GetInfoFromCaretIndex(caretindex, out cl, out ci);
          return ci;
        }
        set
        {
          int cl = CurrentLine;
          int ci = GetCaretIndexFromLine(cl);
          if (value < 0)
          {
            CaretIndex = ci + this[cl].Length;
          }
          else
          {
            CaretIndex = ci + value;
          }
        }
      }

      /// <summary>
      /// Gets the line column index (iow as if tabs were spaces) at a given index.
      /// </summary>
      /// <param name="caretindex">the index</param>
      /// <returns>the column index</returns>
      public int GetLineColumnIndex(int caretindex)
      {
        int line, index;
        int length = 0;

        GetInfoFromCaretIndex(caretindex, out line, out index);

        string l = this[line];

        if (l == null)
          return 0;

        int tadj = 0;

        foreach (string tok in l.Split('\t'))
        {
          tadj = tabsize - tok.Length % tabsize;
          length += tok.Length + (tadj == 0 ? tabsize : tadj);
          index -= tok.Length + 1;
          if (index < 0)
          {
            break;
          }
        }
        return length - (tadj == 0 ? tabsize : tadj) + index + 1; //remove something???
      }

      /// <summary>
      /// The line column index of the current line
      /// </summary>
      [ReadOnly(true)]
      public int LineColumnIndex
      {
        get { return GetLineColumnIndex(caretindex); }
        set
        {
          int cl = CurrentLine;
          if (LineColumnIndex > value)
          {
            while (LineColumnIndex > value)
            {
              if (CurrentLine != cl)
              {
                CaretIndex++;
                break;
              }
              else if (CaretIndex == 0)
              {
                break;
              }
              else
              {
                CaretIndex--;
              }
            }
          }
          else if (LineColumnIndex < value)
          {
            while (LineColumnIndex < value)
            {
              if (CurrentLine != cl)
              {
                CaretIndex--;
                break;
              }
              else if (CaretIndex >= TextLength - 1)
              {
                break;
              }
              else
                CaretIndex++;
            }
          }
        }
      }

      /// <summary>
      /// Updates the line hint of a certain line, normally the first visible line.
      /// </summary>
      /// <param name="line">the line to use</param>
      internal void UpdateLineHint(int line)
      {
        linehint = new LineHint(line, GetCaretIndexFromLine(line));
#if CHECKED
				int res = TestGetCaretIndexFromLine(line);
				Debug.Assert(linehint.index == res, "UpdateLineHint", 
					String.Format("{0} != {1}",res, linehint.index));
#endif
      }

      #endregion

      #region Font

      float fontdescent = 0;

      /// <summary>
      /// Gets the font descent.
      /// </summary>
      /// <value>The font descent.</value>
      public float FontDescent
      {
        get { return fontdescent; }
      }

      /// <summary>
      /// The font being used in the current textbuffer.
      /// </summary>
      public Font Font
      {
        get { return font; }
        set
        {
          lock (this)
          {
            //this best is not to mess with the font.OK it is necessary
            //font needs to be rounded to a factor of 0.75pt
            float ph = value.SizeInPoints; // (float)System.Math.Round(value.SizeInPoints / 0.75, 0) * 0.75f;
            Font font = null;

            //if (ph != value.SizeInPoints)
            //{
            //  font = new Font(value.FontFamily, ph);
            //}
            //else
            {
              font = value;
            }

            float width;
            if (!IsMonospaced(font, out width))
            {
              Trace.WriteLine("Font '{0}' is not monospaced", value);
              font = new Font(FontFamily.GenericMonospace, value.SizeInPoints);
              Trace.WriteLine("Using system default monospace Font '{0}'", font);
            }
            float iwidth = (float)System.Math.Round(width, 0);
            float adjustment = iwidth / width;

            ph *= adjustment; // this just rounds up font to have exactly integer width

            if (ph != font.SizeInPoints)
            {
              font = new Font(font.FontFamily, ph);
            }

            IsMonospaced(font, out width);

            int ls = font.FontFamily.GetLineSpacing(FontStyle.Regular);

            // this will be the prefered value from the font, best to use it
            fontheight = font.Height;
            if (font.Name == "Lucida Console")
            {
              fontheight++;
            }
            ffontheight = (fontheight - font.GetHeight());
            this.font = font;
            fontdescent = (float)font.FontFamily.GetCellDescent(0) / font.FontFamily.GetLineSpacing(0) * font.GetHeight();
            AdjustTabs();



          }
        }
      }


      /// <summary>
      /// The pixel height of the font.
      /// </summary>
      public int FontHeight
      {
        get { return fontheight; }
      }

      /// <summary>
      /// Gets the font width
      /// </summary>
      public float FontWidth
      {
        get { return fontwidth; }
      }

#if CHECKED
			public float FontPixelHeight
			{
				get {return font.GetHeight();}
			}

			public int FontEmAscent
			{
				get {return font.FontFamily.GetCellAscent(font.Style);}
			}

			public int FontEmHeight
			{
				get {return font.FontFamily.GetEmHeight(font.Style);}
			}

			public int FontEmDescent
			{
				get {return font.FontFamily.GetCellDescent(font.Style);}
			}

			public int FontEmLineSpacing
			{
				get {return font.FontFamily.GetLineSpacing(font.Style);}
			}

#endif

      #endregion

      #region Lexing helpers

      // TODO: try refactor this and the next function
      internal void LexInvalidatedLines()
      {
        while (invalidatedlines.Count > 0)
        {
          int[] lines = invalidatedlines.ToArray(typeof(int)) as int[];
          invalidatedlines.Clear();
          foreach (int l in lines)
          {
            if (l >= LineCount)
            {
              break;
            }
            TokenLine tl = GetUserState(l - 1), before2 = tl;
            TokenLine cl = GetUserState(l);
            Stack s = cl == null ? null : cl.state;

            bool hadpp = false;

            IToken[] tokens = cl.Tokens;

            if (tokens != null)
            {
              foreach (IToken t in tokens)
              {
                if (t.Class == TokenClass.Preprocessor)
                {
                  hadpp = true;
                  break;
                }
              }
            }


            tokens = lang.Lex(this[l], ref tl, mlines);

            foreach (IToken t in tokens)
            {
              t.Location.LineNumber = l + 1;
            }

            if (!hadpp)
            {
              foreach (IToken t in tokens)
              {
                if (t.Class == TokenClass.Preprocessor)
                {
                  owner.viewlines = null;
                  break;
                }
              }
              if (owner.viewlines != null)
              {
                Location loc = GetLastLocation(before2);
                if (loc != null && loc.Disabled)
                {
                  owner.viewlines = null;
                }
              }
            }
            else
            {
              owner.viewlines = null;
            }


            if (!tl.CompareState(s))
            {
              SetTokens(l + 1, null);
            }


          }
        }
      }
      
      bool ignoretrigger = false;

      bool DoLex(ref LineState state, int linenr)
      {
        if (lang == null)
        {
          return false;
        }

        bool parserwason = parsetimer.Enabled;

        parsetimer.Enabled = false;

        DoubleLinkedList<TokenLine>.IPosition pos = mlines.PositionOf(state.userstate);
        DoubleLinkedList<TokenLine>.IPosition prev = pos.Previous;
        TokenLine before = null, before2 = null;
        Stack tl = state.userstate.state;
        if (prev != null)
        {
          before2 = before = prev.Data;
        }

        bool hadpp = false;

        IToken[] tokens = state.userstate.Tokens;

        if (tokens != null)
        {
          foreach (IToken t in tokens)
          {
            if (t.Class == TokenClass.Preprocessor)
            {
              hadpp = true;
              break;
            }
          }
        }

        tokens = lang.Lex(state.line, ref before, mlines);

        foreach (IToken t in tokens)
        {
          t.Location.LineNumber = linenr + 1;
        }

        if (!hadpp)
        {
          foreach (IToken t in tokens)
          {
            if (t.Class == TokenClass.Preprocessor)
            {
              owner.viewlines = null;
              break;
            }
          }
          if (owner.viewlines != null)
          {
            Location loc = GetLastLocation(before2);
            if (loc != null && loc.Disabled)
            {
              owner.viewlines = null;
            }
          }
        }
        else
        {
          owner.viewlines = null;
        }

#if AUTOCOMPLETE
        IToken curt = GetTokenAtCaret2();

        if (curt != null && curt.Class == TokenClass.Pair)
        {
          foreach (IToken t in tokens)
          {
            if (t == curt)
            {
              if (!ignoretrigger && !owner.autocomplete)
              {
                //trigger
                //if (parserwason)
                {
                  //BackgroundParser();
                }
                Trace.WriteLine("TRIGGER: {0}", t.Location);
                //owner.autocomplete = true;
                //owner.UpdateAutoComplete();
                return false;
              }
            }
          }
        }
#endif

        if (before != null)
        {
          if (before.state == null)
          {

            if (tl != null)
            {
              Trace.WriteLine("ERROR: Invalid state");
            }
          }
          else
          {
            if (!before.CompareState(tl))
            {
              parsetimer.Enabled = true;
              return true;
            }
          }
        }

        parsetimer.Enabled = true;
        return false;
      }

      Location GetLastLocation(TokenLine before)
      {
        if (before == null)
        {
          return null;
        }
        if (before.Tokens.Length > 1)
        {
          return before.Tokens[before.Tokens.Length - 2].Location;
        }
        DoubleLinkedList<TokenLine>.IPosition pos = mlines.PositionOf(before);
        DoubleLinkedList<TokenLine>.IPosition prev = pos.Previous;
        return GetLastLocation(prev.Data);
      }

      #endregion

      #region Tracing

      class Trace
      {
        [Conditional("TRACE")]
        public static void WriteLine(string format, params object[] args)
        {
          Diagnostics.Trace.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " " + "TextBuffer", format, args);
        }
      }

      class Debug
      {
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
          Diagnostics.Trace.WriteLine(DateTime.Now.ToString("hh:mm:ss.fff") + " " + "TextBuffer", format, args);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool c)
        {
          System.Diagnostics.Debug.Assert(c);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool c, string msg)
        {
          System.Diagnostics.Debug.Assert(c, msg);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool c, string msg, string detail)
        {
          System.Diagnostics.Debug.Assert(c, msg, detail);
        }

        [Conditional("DEBUG")]
        public static void Fail(string m)
        {
          System.Diagnostics.Debug.Fail(m);
        }
      }

      #endregion

      #region Drawing / Measuring

      CodeModel.Location tp = null, pp = null;

      IToken GetTokenAtCaret2()
      {
        int ci = LineCharacterIndex + 1;
        int cl = CurrentLine;

        IToken[] tokens = GetTokens(cl);

        if (tokens != null)
        {
          tokens = tokens.Clone() as IToken[];
          Array.Reverse(tokens);
          foreach (IToken t in tokens)
          {
            CodeModel.Location loc = t.Location;
            if (loc.Column < ci && ci <= loc.EndColumn)
            {
              return t;
            }
          }
        }
        return null;
      }

      IToken GetTokenAtCaret()
      {
        int ci = LineCharacterIndex;
        int cl = CurrentLine;

        IToken[] tokens = GetTokens(cl);

        if (tokens != null)
        {
          tokens = tokens.Clone() as IToken[];
          Array.Reverse(tokens);
          foreach (IToken t in tokens)
          {
            CodeModel.Location loc = t.Location;
            if (loc.Column < ci && ci <= loc.EndColumn)
            {
              return t;
            }
          }
        }
        return null;
      }

      internal void DoBraceLayout(int start, int end)
      {
        IToken t = GetTokenAtCaret();
        if (t != null)
        {
          CodeModel.Location loc = t.Location;
          CodeModel.Location p = braces[loc] as CodeModel.Location;

          if (p != null)
          {
            loc.Paired = true;
            p.Paired = true;

            Invalidate(loc, p);

            if (tp != null)
            {
              pp.Paired = false;
              tp.Paired = false;

              Invalidate(pp, tp);
            }

            tp = p;
            pp = loc;
          }
        }
      }

      void Invalidate(params CodeModel.Location[] loc)
      {
        foreach (CodeModel.Location l in loc)
        {
          SetDrawCache(l.LineNumber - 1, null);
        }
      }

      internal void DrawFoldedText(Graphics g, string text, Color c, int line, int vline)
      {
        Brush gbb = Factory.SolidBrush(Color.FromArgb(40, c));
        Pen gpp = Factory.Pen(c, 1);
        int indent = (int)(GetIndent(this[vline]) * TabSize * fontwidth);
        Brush gb = Factory.SolidBrush(c);
        Drawing.Utils.PaintLineHighlight(gbb, gpp, g, indent, FontHeight * line + 1,
          (int)((text.Length + 1) * FontWidth), FontHeight - 1, true);
        g.DrawString(text, Font, gb, indent + 2, FontHeight * line + 1, sf);
      }

      /// <summary>
      /// New style painter that implements JIT techniques.
      /// </summary>
      class TextBufferPainter
      {
        readonly TextBuffer buffer;

#if DEBUG
        static int stat_total = 0;
        static int stat_hit = 0;
        static int stat_miss = 0;

        static float HitRatio
        {
          get { return stat_total == 0 ? 0f : (float)stat_hit / stat_total; }
        }
#endif

        public TextBufferPainter(TextBuffer buffer)
        {
          this.buffer = buffer;
        }

        public bool DrawLine(Graphics g, int line, int vline)
        {
          if (vline >= buffer.LineCount || vline == -1)
          {
            return false;
          }

          bool nextlinenulled = false;

          string l = buffer[vline];
          Font font = buffer.font;

          int fh = 0, fh2 = 0;
          float fd = 0;

          fh = line * (fh2 = buffer.fontheight);
          fd = buffer.ffontheight;

          DrawInfo[] dis = null;
          TokenLine tl = null;
          IToken[] tokens = null;
#if DEBUG
          stat_total++;
#endif
          dis = buffer.GetDrawCache(vline);
          tl = buffer.GetUserState(vline);
          tokens = tl.Tokens;

          if (tokens == null)
          {
            return false;
          }

          if (dis == null)
          {
#if DEBUG
            stat_miss++;
#endif

            dis = Simplify(tokens, l);

            if (dis != null)
            {
              //single line disable line caching, good to turn off when debugging editor issues.
              buffer.SetDrawCache(vline, dis);
            }
          }
#if DEBUG
          else
          {
            stat_hit++;
          }
#endif
          if (g != null)
          {
            if (dis != null)
            {

              foreach (DrawInfo di in dis)
              {
                Font f = Factory.Font(font, di.style);
                Brush b = Factory.SolidBrush(di.forecolor);

                if (di.backcolor != Color.Empty)
                {
                  Brush bg = Factory.SolidBrush(Color.FromArgb(127, di.backcolor));
                  g.FillRectangle(bg, di.start, fh + 1, di.end - di.start, fh2);
                }
                if (di.bordercolor != Color.Empty)
                {
                  Pen bp = Factory.Pen(di.bordercolor, 1);
                  g.DrawRectangle(bp, di.start, fh + 1, di.end - di.start, fh2);
                }

                g.DrawString(di.text, f, b, di.start, fh + fd, buffer.sf);
              }

              foreach (IToken t in tokens)
              {
                CodeModel.Location loc = t.Location;

                if (loc.Error)
                {
                  float offset, w = MeasureString(l, loc.Column, loc.EndColumn, out offset);
                  buffer.DrawSquiggle(g, (line + 1) * fh2, (int)(offset),
                    (int)(offset + w), Color.Red);
                }
                else if (loc.Warning)
                {
                  float offset, w = MeasureString(l, loc.Column, loc.EndColumn, out offset);
                  buffer.DrawSquiggle(g, (line + 1) * fh2, (int)(offset),
                    (int)(offset + w), Color.Blue);
                }
              }
            }
            else
            {
              g.DrawString(l, font, Brushes.Black, 0, fh + fd, buffer.sf);
            }
          }

          return nextlinenulled;
        }

        public DrawInfo[] Simplify(IToken[] tokens, string linetext)
        {
          ArrayList dis = new ArrayList(tokens.Length);
          Language lang = buffer.lang;
          DrawInfo di = null;
          int laststart = 0;

          foreach (IToken tok in tokens)
          {
            if (tok.Class == TokenClass.NewLine)
            {
              break;
            }
            if (tok.Length == 0)
            {
              continue;
            }

            CodeModel.Location loc = tok.Location;
            ColorInfo ci = Language.GetColorInfo(loc.Paired ? TokenClass.Pair : tok.Class);

            if (loc.Disabled)
            {
              ci.ForeColor = Color.FromArgb(127, ci.ForeColor);
            }

          NEWRANGE:
            if (di == null)
            {
              di = new DrawInfo();
              di.style = ci.Style;
              laststart = loc.Column;

              di.forecolor = ci.ForeColor;
              di.backcolor = ci.BackColor;
              di.bordercolor = ci.BorderColor;

              float start, end = MeasureString(linetext, laststart, laststart + tok.Length, out start);

              di.start = start;
              di.end = end + start;

              try
              {
                di.text = linetext.Substring(laststart, tok.Length);
              }
              catch
              {
                Trace.WriteLine("Error: this indicates a fault in your lexer. Last token was: {0}", tok.Class);
              }

            }
            else
            {
              int c = loc.Column;

              if (di.forecolor == ci.ForeColor && di.backcolor == ci.BackColor && di.bordercolor == ci.BorderColor &&
                di.style == ci.Style && (c > 0 && linetext[c - 1] != '\t'))
              {
                //the same
                float start, end = MeasureString(linetext, c, c + tok.Length, out start);

                try
                {
                  di.text = linetext.Substring(laststart, c + tok.Length - laststart);
                }
                catch
                {
                  Trace.WriteLine("Error: this indicates a fault in your lexer. Last token was: {0}", tok.Class);
                }

                di.end = end + start;
              }
              else
              {
                //different
                dis.Add(di);
                di = null;
                goto NEWRANGE;
              }
            }
          }

          //add last drawinfo
          if (di != null)
          {
            dis.Add(di);
          }

          return dis.ToArray(typeof(DrawInfo)) as DrawInfo[];
        }

        public int MeasureStringRange(string line, int startindex, int endindex, out int offset)
        {
          offset = 0;
          int length = 0, linelen = line.Length;
          int tabsize = buffer.tabsize;

          string l = line;

          if (startindex > linelen)
          {
            startindex = linelen;
          }
          if (endindex == -1 || endindex > linelen)
          {
            endindex = linelen;
          }

          if (startindex > 0)
          {
            int index = 0, start = 0;

            while ((index = IndexOfTab(l, start, startindex)) >= 0)
            {
              int prelen = (index - start);
              length += prelen;
              int x = tabsize - length % tabsize;
              length += x;
              start += (prelen + 1);
            }
            length += startindex - start;

            offset = length;
          }
          {
            //set up start and cache previous info, if any
            int index, start = startindex;

            while ((index = IndexOfTab(l, start, endindex)) >= 0)
            {
              int prelen = (index - start);
              length += prelen;
              int x = tabsize - length % tabsize;
              length += x;
              start += (prelen + 1);
            }
            length += endindex - start;

            return length - offset;
          }
        }

        public float MeasureString(string line, int startindex, int endindex, out float offset)
        {
          offset = 0f;
          int ioffset;
          float fontwidth = buffer.fontwidth;

          int r = MeasureStringRange(line, startindex, endindex, out ioffset);

          offset = ioffset * fontwidth;

          float sizew = r * fontwidth + offset;

          if (buffer.maxwidth < sizew)
          {
            buffer.maxwidth = sizew;
          }

          return sizew - offset;
        }

        static Color InvertColor(Color col)
        {
          int r = 255 - col.R;
          int g = 255 - col.G;
          int b = 255 - col.B;
          Color invert = Color.FromArgb(r, g, b);
          return invert;
        }
      }

      static int IndexOfTab(string line, int start, int end)
      {
        while (start < end)
        {
          if (line[start] == '\t')
          {
            return start;
          }
          start++;
        }
        return -1;
      }

      bool IsMonospaced(Graphics g, Font font, out float width)
      {
        //is font monospace? what about pnet?
        float w = g.MeasureString("w", font, MAX24BIT, sf).Width * 10f;
        float l = g.MeasureString("llllllllll", font, MAX24BIT, sf).Width;
        width = w / 10f;
        return (System.Math.Abs(w - l) < 0.005f);
      }

      float MeasureString(int line)
      {
        return MeasureString(line, -1);
      }

      float MeasureString(int line, int endindex)
      {
        float offset;
        return MeasureString(line, 0, endindex, out offset);
      }

      float MeasureStringEnd(int line, int startindex, out float offset)
      {
        return MeasureString(line, startindex, -1, out offset);
      }

      float MeasureString(int line, int startindex, int endindex, out float offset)
      {
        offset = 0f;
        if (endindex == 0)
        {
          return 0f;
        }
        string l = this[line];
        if (l != null)
        {
          return painter.MeasureString(l, startindex, endindex, out offset);
        }
        return 0;
      }

      internal float LongestStringWidth
      {
        get { return maxwidth; }
      }

      internal RectangleF lastcr = RectangleF.Empty;

      internal RectangleF GetCaretRectF()
      {
        int line = CurrentLine, index = LineCharacterIndex;
        float x = MeasureString(line, index);
        return (lastcr = new RectangleF(x - 1, (line * fontheight) + 1, 2, fontheight + 1));
      }

      /// <summary>
      /// Draws the caret in a given area.
      /// </summary>
      /// <param name="g">the graphics context</param>
      /// <param name="area">the area to use</param>
      internal void DrawCaret(Graphics g, RectangleF area)
      {
        g.SmoothingMode = SmoothingMode.HighSpeed;

        g.DrawLine(caret, area.Left + 1, area.Y,
          area.Left + 1, area.Bottom - 1);

        g.SmoothingMode = SmoothingMode.Default;
      }

      internal void DrawSquiggle(Graphics g, int y, int start, int end, Color color)
      {
        if (end - start < 2)
        {
          start = 0;
        }
        ArrayList p = new ArrayList();
        y--;
        int i = 1;

        for (bool up = false; i - 1 <= end - start; up = !up)
        {
          if (up)
          {
            p.Add(new Point(start + i, y));
          }
          else
          {
            p.Add(new Point(start + i, y - 2));
          }
          i += 2;
        }

        g.DrawLines(Factory.Pen(color, 1), p.ToArray(typeof(Point)) as Point[]);
      }

      /// <summary>
      /// Draws a single line of text.
      /// </summary>
      /// <param name="g">the graphics context</param>
      /// <param name="line">the line number</param>
      /// <param name="vline">the virtual line number</param>
      internal bool DrawString(Graphics g, int line, int vline)
      {
        return painter.DrawLine(g, line, vline);
      }

      int lastselstart, lastsellen, lastfirst, lastlast;
      int[] lastlines = null;
      GraphicsPath lastsel;

      //used with fancy selection, o god im glad this over....
      internal GraphicsPath GetPath(int selectionstart, int selectionlength, int firstline, int lastline)
      {
        if (selectionstart == lastselstart &&
          selectionlength == lastsellen &&
          firstline == lastfirst && lastline == lastlast && lastsel != null && lastlines == owner.viewlines)
        {
          return lastsel;
        }
        else
        {
          if (lastsel != null)
          {
            lastsel.Dispose();
            lastsel = null;
          }
          lastselstart = selectionstart;
          lastsellen = selectionlength;
          lastfirst = firstline;
          lastlast = lastline;
        }

        GraphicsPath r = new GraphicsPath(), l = new GraphicsPath();

        int[] lines = lastlines = owner.viewlines;

        const int radius = 1;

        int selstart = selectionstart, sellen = selectionlength;
        int startline, startlineindex;
        GetInfoFromCaretIndex(selstart, out startline, out startlineindex);
        int curline, curlineindex;
        GetInfoFromCaretIndex(selstart + sellen, out curline, out curlineindex);

        startline = Array.BinarySearch(lines, startline);
        curline = Array.BinarySearch(lines, curline);

        float offset, pos;

        if (curline > startline)
        {
          RectangleF prev = new RectangleF();

          if (startline >= firstline && startline <= lastline)
          {
            pos = MeasureStringEnd(lines[startline], startlineindex, out offset);

            RectangleF rr = prev = new RectangleF(offset - 1, startline * fontheight + 1, pos + 1 + fontwidth, fontheight - 1);

            l.AddArc(rr.X, rr.Y, radius, radius, 270, -90);
            r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);
          }

          while (++startline < curline)
          {
            if (startline >= firstline && startline <= lastline)
            {
              pos = MeasureString(lines[startline], 0, -1, out offset);

              RectangleF rr = new RectangleF(offset - 1, startline * fontheight + 1, pos + 1 + fontwidth, fontheight - 1);

              if (rr.Left < prev.Left)
              {

                l.AddArc(prev.Left - radius, prev.Bottom - radius, radius, radius, 0, 90);
                l.AddArc(rr.Left, rr.Y - 1, radius, radius, 270, -90);

              }

              if (rr.Right > prev.Right)
              {
                r.AddArc(prev.Right, prev.Bottom - radius + 1, radius, radius, 180, -90);
                r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);
              }
              else if (rr.Right < prev.Right)
              {
                r.AddArc(prev.Right - radius, prev.Bottom - radius, radius, radius, 0, 90);
                r.AddArc(rr.Right, rr.Y - 1, radius, radius, 270, -90);
              }

              prev = rr;

            }
          }
          if (startline >= firstline && startline <= lastline)
          {
            pos = MeasureString(lines[startline], 0, curlineindex, out offset);

            RectangleF rr = new RectangleF(offset - 1, startline * fontheight + 1, pos + 1, fontheight - 1);

            if (rr.Left < prev.Left)
            {
              l.AddArc(prev.Left - radius, prev.Bottom - radius, radius, radius, 0, 90);
              l.AddArc(rr.Left, rr.Y - 1, radius, radius, 270, -90);
            }

            if (rr.Right > prev.Right)
            {
              r.AddArc(prev.Right, prev.Bottom - radius + 1, radius, radius, 180, -90);
              r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);
            }
            else if (rr.Right < prev.Right)
            {
              r.AddArc(prev.Right - radius, prev.Bottom - radius, radius, radius, 0, 90);
              r.AddArc(rr.Right, rr.Y - 1, radius, radius, 270, -90);
            }

            prev = rr;
          }

          if (l.PointCount > 0 && r.PointCount > 0)
          {
            l.AddArc(prev.X, prev.Bottom - radius, radius, radius, 180, -90);
            r.AddArc(prev.Right - radius, prev.Bottom - radius, radius, radius, 0, 90);

            l.Reverse();
            r.AddPath(l, true);
            r.CloseAllFigures();
          }
        }

        else
        {
          if (startline >= firstline && startline <= lastline)
          {
            int angle = 180;

            pos = MeasureString(lines[startline], startlineindex, startlineindex + sellen, out offset);
            RectangleF rr = new RectangleF(offset - 1, startline * fontheight + 1, pos + 1, fontheight - 1);

            // top left
            r.AddArc(rr.X, rr.Y, radius, radius, angle, 90);
            angle += 90;
            // top right
            r.AddArc(rr.Right - radius, rr.Y, radius, radius, angle, 90);
            angle += 90;
            // bottom right
            r.AddArc(rr.Right - radius, rr.Bottom - radius, radius, radius, angle, 90);
            angle += 90;
            // bottom left
            r.AddArc(rr.X, rr.Bottom - radius, radius, radius, angle, 90);
            angle += 90;
   
            r.CloseAllFigures();
          }
        }

        return lastsel = r;
      }
      #endregion

      #region Preprocessor / Parser

      /* This should be called whenever the backgroud lexer completes or
       * tokens has been reset on a line, however we dont wanna go mad, 
       * and call it constantly, it should only start if tokens were reset
       * and the app goes into 'idle' state (about 2 seconds i guess), it should
       * also not interupt the parser if it is running already.
       */

      void parsetimer_Tick(object state, EventArgs e)
      {
        BackgroundParser();
      }

      readonly object TOKENENUMLOCK = new object();

      internal void Preprocessor()
      {
        if (lang == null)
        {
          return;
        }

        bool parserenabled = parsetimer.Enabled;
        parsetimer.Enabled = false;
        string[] vals = new string[0];
        if (owner.ProjectHint != null)
        {
          //Build.CustomAction a = owner.ProjectHint.GetAction(FileName) as Build.CustomAction;
          //if (a != null)
          //{
          //  Build.Option o = a.GetOption("Defines");
          //  if (o != null)
          //  {
          //    vals = new string[] { a.GetOptionValue(o) as string };
          //    Trace.WriteLine(string.Format("Preprocessor defines: {0}", string.Join(";", vals)));
          //  }
          //}
        }
        string filename = owner.ProjectHint == null ? FileName : owner.ProjectHint.GetRelativeFilename(FileName);
        hp.Start();
        lang.Preprocess(mlines, filename, this, owner.pairings, vals);
        hp.Stop();

        if (owner.ProjectHint != null)
        {
          owner.ProjectHint.AddPairings(filename, owner.pairings);
        }

        Trace.WriteLine(string.Format("Preprocessor completed in {0:f3}ms", hp.Duration));

        parsetimer.Enabled = parserenabled;
      }

      readonly Hashtable braces = new Hashtable();

      internal void BackgroundParser()
      {
        parsetimer.Enabled = false;
        if (owner.autocomplete)
        {
          return;
        }
        //if (owner.ProjectHint != null)
        {
          string filename = owner.ProjectHint != null ? owner.ProjectHint.GetRelativeFilename(FileName) : FileName;

          lang.braces = braces;

          hp.Start();
          int res = lang.Parse(mlines, FileName, this);
          hp.Stop();

          Trace.WriteLine(string.Format("Parsing completed {0}successfully in {1:f1}ms", (res == 0 ? string.Empty : "un"), hp.Duration));

          if (res > 0)
          {
            CodeModel.Location loc = null;
            IToken t = lang.LastToken;
            if (t == null)
            {
              loc = null;
            }
            else
            {
              loc = t.Location;
              loc.Error = true;
              ((Language.IParserCallback)this).Invoke(loc);
            }
          }
          else
          {
            owner.CodeFile = lang.CodeModel;
            if (owner.ProjectHint != null)
            {
              owner.ProjectHint.CodeModel.Add(lang.CodeModel);
              owner.ProjectHint.GenerateProjectTree();
              ServiceHost.CodeModel.Run(owner.ProjectHint);
            }
            else
            {
              ServiceHost.CodeModel.Run(lang.CodeModel);
            }
          }
          owner.Invalidate();
        }
      }

      delegate void VOIDPROJECT(Build.Project p);


      void Language.IParserCallback.Invoke(System.Collections.Generic.Stack<CodeModel.Location> locstack)
      {

      }

      void Language.IParserCallback.Invoke(CodeModel.Location loc)
      {
        if (loc.callback != null)
        {
          TokenLine tl = GetUserState(loc.LineNumber - 1);

          if (tl != null)
          {
            for (int i = tl.Tokens.Length - 1; i >= 0; i--)
            {
              //if (tl.Tokens[i].Location == loc)
              //{
              //  loc.callback(tl.Tokens[i]);
              //  break;
              //}
              //else 
              if (tl.Tokens[i].Location.IsIn(loc))
              {
                if (tl.Tokens[i].Class != TokenClass.Operator)
                {
                  loc.callback(tl.Tokens[i]);
                  break;
                }
              }
            }
          }
          loc.callback = null;
        }
        SetDrawCache(loc.LineNumber - 1, null);
      }

      #endregion

      #region Selection

      /// <summary>
      /// Gets the selection as IDrawInfo, for transformation
      /// </summary>
      /// <param name="startline">the start line</param>
      /// <returns>an array of an array of IDrawInfo</returns>
      public IDrawInfo[][] GetSelectedDrawInfo(out int startline)
      {
        ArrayList lines = new ArrayList();
        int startidx;
        int endline, endidx;

        GetInfoFromCaretIndex(SelectionStart, out startline, out startidx);
        GetInfoFromCaretIndex(SelectionStart + SelectionLength, out endline, out endidx);

        for (int i = startline; i <= endline; i++)
        {
          DrawInfo[] dis = GetDrawCache(i);
          if (dis == null)
          {
            IToken[] tokens = GetTokens(i);
            if (tokens != null)
            {
              dis = painter.Simplify(tokens, this[i]);

              if (dis != null)
              {
                SetDrawCache(i, dis);
              }
              else
              {
                Debug.Fail("Invalid State");
              }
            }
            else
            {
              Debug.Fail("Invalid State");
            }
          }
          lines.Add(dis);
        }

        return lines.ToArray(typeof(IDrawInfo[])) as IDrawInfo[][];
      }

      /// <summary>
      /// Gets the selected tokens
      /// </summary>
      /// <returns>the selected tokens</returns>
      public IToken[][] GetSelectedTokens()
      {
        ArrayList lines = new ArrayList();
        int startline, startidx;
        int endline, endidx;

        GetInfoFromCaretIndex(SelectionStart, out startline, out startidx);
        GetInfoFromCaretIndex(SelectionStart + SelectionLength, out endline, out endidx);

        for (int i = startline; i <= endline; i++)
        {
          lines.Add(GetTokens(i));
        }

        return lines.ToArray(typeof(IToken[])) as IToken[][];
      }

      /// <summary>
      /// Selects a specific Location
      /// </summary>
      /// <param name="location">the Location to select</param>
      public void SelectLocation(CodeModel.Location location)
      {
        if (location == null)
        {
          return;
        }
        int line = location.LineNumber - 1;
        int linecount = location.LineCount;
        int col = location.Column;
        int endcol = location.EndColumn;
        int selstart = 0, sellen = 0;

        if (line >= LineCount)
        {
          return;
        }

        IToken[] tokens = GetTokens(line);

        if (tokens == null)
        {
          Debug.Fail("Invalid state");
        }

        int ci = GetCaretIndexFromLine(line);

        if (col > 0)
        {
          for (int i = 0; i < tokens.Length; i++)
          {
            int c = tokens[i].Location.Column;
            if (c <= col && c + tokens[i].Length > col)
            {
              selstart = ci + c;
              if (tokens[i].Class == TokenClass.NewLine)
              {
                sellen = 0;
                endcol = col;
              }
              else
              {
                sellen = tokens[i].Length;
              }

              if (location.Error)
              {
                tokens[i].Location.Error = true;
              }
              else if (location.Warning)
              {
                tokens[i].Location.Warning = true;
              }
              break;
            }

            if (i == tokens.Length - 1)
            {
              selstart = ci + c;
              if (tokens[i].Class == TokenClass.NewLine)
              {
                sellen = 0;
                endcol = col;
              }
              else
              {
                sellen = tokens[i].Length;
              }

              if (location.Error)
              {
                tokens[i].Location.Error = true;
              }
              else if (location.Warning)
              {
                tokens[i].Location.Warning = true;
              }
              break;
            }
          }
        }
        else
        {
          selstart = ci;
        }

        if (linecount > 0)
        {
          line += linecount;
          col = endcol;

          if (line >= LineCount)
          {
            return;
          }

          tokens = GetTokens(line);

          if (tokens == null)
          {
            Debug.Fail("Invalid state");
          }

          ci = GetCaretIndexFromLine(line);

          if (col > 0)
          {
            for (int i = 0; i < tokens.Length; i++)
            {
              int c = tokens[i].Location.Column;
              if (c <= col && c + tokens[i].Length > col)
              {
                sellen = ci + c - selstart;
                break;
              }

              if (i == tokens.Length - 1)
              {
                sellen = ci + c - selstart;
                break;
              }
            }
          }
          else
          {
            sellen = ci - selstart;
          }
        }
        else
        {
          if (endcol > col)
          {
            sellen = endcol - col;
          }
        }

        Select(selstart, sellen);
      }

      /// <summary>
      /// Selects a token
      /// </summary>
      /// <param name="line">the line number</param>
      /// <param name="col">the column</param>
      /// <param name="markerror">if error</param>
      /// <param name="markwarning">if warning</param>
      public void SelectTokenAt(int line, int col, bool markerror, bool markwarning)
      {
        if (line >= LineCount)
        {
          return;
        }

        IToken[] tokens = GetTokens(line);

        if (tokens == null)
        {
          Debug.Fail("Invalid state");
        }

        int ci = GetCaretIndexFromLine(line);

        if (col >= 0)
        {
          for (int i = 0; i < tokens.Length; i++)
          {
            int c = tokens[i].Location.Column;
            if (c <= col && c + tokens[i].Length >= col)
            {
              if (markerror)
              {
                tokens[i].Location.Error = true;
                SetTokens(line, tokens);
              }
              else
                if (markwarning)
                {
                  tokens[i].Location.Warning = true;
                  SetTokens(line, tokens);
                }
              Select(ci + c, tokens[i].Length);
              return;
            }

            if (i == tokens.Length - 1)
            {
              tokens[i].Location.Error = markerror;
              tokens[i].Location.Warning = markwarning;
              SetTokens(line, tokens);
              Select(ci + c, tokens[i].Length);
            }
          }
          return;
        }
        else
        {
          Select(ci, this[line].Length);
          return;
        }
      }

      /// <summary>
      /// Checks whether text is currently selecting.
      /// </summary>
      /// <value>true if selecting otherwise false</value>
      [ReadOnly(true)]
      public bool IsSelecting
      {
        get { return select; }
        set { select = value; }
      }

      /// <summary>
      /// Selects a range of text.
      /// </summary>
      /// <param name="startindex">the start index</param>
      /// <param name="length">the length of the selected text</param>
      public void Select(int startindex, int length)
      {
        select = false;
        CaretIndex = startindex;
        select = true;
        CaretIndex = startindex + length;
        select = false;
      }

      /// <summary>
      /// The start of the current selection or the caret index if no text is selected.
      /// </summary>
      /// <value>
      /// The start index of the selection
      /// </value>
      public int SelectionStart
      {
        get { return (selectionstart > caretindex) ? caretindex : selectionstart; }
      }

      /// <summary>
      /// The length of the selected text.
      /// </summary>
      /// <value>
      /// The length of the selection
      /// </value>
      public int SelectionLength
      {
        get
        {
          int i = caretindex == textlength ? caretindex - 1 : caretindex;
          return System.Math.Abs(selectionstart - i);
        }
      }

      /// <summary>
      /// The selected text.
      /// </summary>
      /// <value>
      /// The selected text.
      /// </value>
      public string SelectionText
      {
        get
        {

          int selstart = SelectionStart, sellen = SelectionLength, selend = selstart + sellen,
            startline, startci, endline, endci, curline;

          GetInfoFromCaretIndex(selstart, out startline, out startci);
          GetInfoFromCaretIndex(selend, out endline, out endci);

          curline = startline;

          StringBuilder sb = new StringBuilder((endline - startline) * nllen + sellen);

          if (endline > startline)
          {
            sb.Append(this[startline].Substring(startci) + "\n");

            while (endline > ++curline)
            {
              sb.Append(this[curline] + "\n");
            }
            sb.Append(this[endline].Substring(0, endci));
          }

          else
          {
            sb.Append(this[curline].Substring(startci, endci - startci));
          }

          return sb.ToString();
        }
      }

      #endregion

      #region Text

      /// <summary>
      /// Returns the full text of the textbuffer. 
      /// </summary>
      /// <remarks>
      /// Use with caution. Its an expensive operation.
      /// </remarks>
      public string Text
      {
        get
        {
          if (LineCount == 0)
          {
            return string.Empty;
          }
          StringWriter writer = new StringWriter();
          //writer.NewLine = "\n";
          for (int i = 0; i < LineCount; i++)
          {
            writer.WriteLine(this[i]);
          }
          StringBuilder sb = writer.GetStringBuilder();
          return sb.ToString(0, sb.Length - writer.NewLine.Length);
        }
      }

      /// <summary>
      /// Checks whether the text has been modified.
      /// </summary>
      /// <remarks>Loading does not modify this</remarks>
      public bool IsDirty
      {
        get
        {
          bool res = undo.CurrentLevel != lastsavelevel;
          if (owner.autosave)
          {
            if (res)
            {
              Save(FileName);
            }
            return false;
          }
          return res;
        }
      }

      int lastsavelevel = 0;

      /// <summary>
      /// Returns the length of the text in the TextBuffer.
      /// </summary>
      public int TextLength
      {
        get
        {
#if CHECKED
          int length = 0;
          for (int i = 0; i < LineCount; i++)
          {
            length += this[i].Length + nllen;
          }
					length -= nllen;
          Debug.Assert(length == textlength - nllen, String.Format("{0} != {1}", length , textlength));
          return length;
#else
          return textlength - nllen;
#endif
        }
      }

      #endregion

      #region Removal operations

      /// <summary>
      /// Removes the currently selected text.
      /// </summary>
      public void RemoveSelection()
      {
        if (!owner.ReadOnly)
        {
          SendProbe();

          object before = null;
          string value = null;

          int selstart = SelectionStart, sellen = SelectionLength;
#if CHECK
          Debug.WriteLine("Start: {0} Length: {1} Text: '{2}'", selstart, sellen, SelectionText);
#endif

          int startline, startlineindex;
          GetInfoFromCaretIndex(selstart, out startline, out startlineindex);
          int curline, curlineindex;
          GetInfoFromCaretIndex(selstart + sellen, out curline, out curlineindex);
          int count = curline - startline;
          int rcount = count;

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
            value = SelectionText;
          }

          if (curline > startline)
          {
            string ss = this[startline].Substring(0, startlineindex);
            //            if (startlineindex > 0)
            //            {
            //              this[startline] = ss;
            //              startline++;
            //            }

            string tail = this[curline].Substring(curlineindex);

            Remove(curline - count, count);

            if (startlineindex > 0)
            {
              this[startline] = ss + tail;
            }
            else
            {
              this[startline] = tail;
            }
          }
          else
          {
            this[startline] = this[startline].Remove(startlineindex, sellen);
          }
          linehint = STARTHINT;
          CaretIndex = SelectionStart;

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new RemoveOperation(before, after, value));
          }

          SendProbe();
        }
      }

      /// <summary>
      /// Removes the character before the caret (as if pressing backspace).
      /// </summary>
      public void RemoveBeforeCaret()
      {
        if (!owner.ReadOnly)
        {
          ignoretrigger = true;
          SendProbe();
          if (SelectionLength > 0)
          {
            RemoveSelection();
            return;
          }

          object before = null;
          string value = null;

          int cl = CurrentLine;
          int ci = LineCharacterIndex;

          if (cl == 0 && ci == 0)
          {
            return;
          }
#if CHECK
          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, ci);
#endif

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
          }

          string l = this[cl];

          StringBuilder sb = new StringBuilder();

          if (ci == 0 && cl > 0 && this[cl - 1].Length == 0)
          {
            value = "\n";
            Remove(cl - 1);
          }
          else
            if (ci > 0)
            {
              sb.Append(l);
              value = sb[ci - 1].ToString();
              sb.Remove(ci - 1, 1);
              this[cl] = sb.ToString();
            }
            else
            {
              string p = this[cl - 1];
              value = "\n";
              sb.Append(p);
              sb.Append(l);
              Remove(cl, 1);
              this[cl - 1] = sb.ToString();
            }
          CaretIndex--;

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new RemoveOperation(before, after, value));
          }
          SendProbe();

          ignoretrigger = false;
        }
      }



      /// <summary>
      /// Removes the character after the caret (as if pressing Delete).
      /// </summary>
      public void RemoveAfterCaret()
      {
        if (!owner.ReadOnly)
        {
          SendProbe();
          if (SelectionLength > 0)
          {
            RemoveSelection();
            return;
          }

          if (caretindex >= TextLength)
          {
            return;
          }

          object before = null;
          string value = null;

          int cl = CurrentLine;
          int ci = LineCharacterIndex;
#if CHECK
          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, ci);
#endif
          string l = this[cl];

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
          }

          StringBuilder sb = new StringBuilder(l);
          if (ci == 0 && l.Length == 0)
          {
            value = "\n";
            Remove(cl);
          }
          else
            if (ci == l.Length)
            {
              value = "\n";
              sb.Append(this[cl + 1]);
              Remove(cl + 1);
              this[cl] = sb.ToString();
            }
            else
            {
              value = sb[ci].ToString();
              sb.Remove(ci, 1);
              this[cl] = sb.ToString();
            }

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new RemoveOperation(before, after, value));
          }
          SendProbe();
        }
      }

      public void RemoveCurrentLine()
      {
        if (!owner.ReadOnly)
        {
          SendProbe();

          object before = null;
          string value = null;

          int cl = CurrentLine;

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
            value = this[cl];
          }

          Remove(cl);

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new RemoveLineOperation(before, after, value, cl));
          }
          SendProbe();
        }
      }

      #endregion

      #region Insert operations

      /// <summary>
      /// Inserts a string after the caret and moves the caret index to the end of the newly inserted text.
      /// </summary>
      /// <param name="text">the text to insert, can be multiline</param>
      public void InsertString(string text)
      {
        if (!owner.ReadOnly)
        {
          SendProbe();
          if (SelectionLength > 0 && LineCount > 0)
          {
            RemoveSelection();
          }
          object before = null;
          string value = null;

          int cl = CurrentLine;
          int ci = LineCharacterIndex;
#if CHECK
          Debug.WriteLine("Line: {0} CharIndex: {1} Value: '{2}'", cl, ci, text);
#endif
          string l = this[cl];

          if (recording && text.Length > 0)
          {
            before = ((IHasUndo)this).GetUndoState();
          }

          string front = l.Substring(0, ci);
          string back = l.Substring(ci);

          Remove(cl);

          text = text.Replace("\r", string.Empty);

          if (recording)
          {
            value = text;
          }

          string[] tokens = text.Split('\n');

          int toklen = tokens.Length;

          string nl;

          if (toklen == 1)
          {
            nl = front + tokens[0] + back;
            Insert(cl, nl);
          }
          else
          {
            Insert(cl++, front + tokens[0]);

            int len = toklen - 2;
            Insert(cl, tokens, 1, len);
            cl += len;

            nl = tokens[len + 1] + back;
            Insert(cl, nl);

            CurrentLine = cl;
          }

          LineCharacterIndex = nl.Length - back.Length;

          if (recording && text.Length > 0)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new InsertOperation(before, after, value));
          }

          SendProbe();
        }
      }

      int GetIndent(string text)
      {
        int langindent = Language.GetIndentation(text, TabSize);
        if (langindent != 0)
        {
          return langindent;
        }
        if (text == null) return 0;
        const char SPACE = ' ';
        const char TAB = '\t';
        int indent = 0, pos = 0, count = 0;
        while (pos < text.Length)
        {
          switch (text[pos++])
          {
            case SPACE:
              count++;
              if (count == tabsize)
              {
                indent++;
                count = 0;
              }
              break;
            case TAB:
              indent++;
              count = 0;
              break;
            default:
              return indent * TabSize;
          }
        }
        return indent * TabSize;
      }

      /// <summary>
      /// Inserts a single character after the caret index and advances the caret index (as if pressing a key).
      /// </summary>
      /// <param name="c">
      /// The chararter to insert into the TextBuffer.
      /// </param>
      public void InsertCharacter(char c)
      {
        if (!owner.ReadOnly)
        {
          if (SelectionLength > 0 && LineCount > 0)
          {
            RemoveSelection();
          }
          object before = null;
          string value = null;

          int cl = CurrentLine;
          int ci = LineCharacterIndex;

          string ns = c.ToString();

          if (c == '\t' && TabsToSpaces)
          {
            int td = TabSize - ci % TabSize;
            ns = new string(' ', td);
          }

#if CHECK
          Debug.WriteLine("Value: '{2}' Line: {0} CharIndex: {1}", cl, ci, c);
#endif
          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
            value = ns;
          }

          string l = this[cl];

          StringBuilder sb = new StringBuilder(l);
          sb.Insert(ci, ns);

          this[cl] = sb.ToString();
          CaretIndex += ns.Length;

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new InsertOperation(before, after, value));
          }
        }
      }

      /// <summary>
      /// Inserts a newline after the caret (as if pressing Enter/Return).
      /// </summary>
      public void InsertLineAfterCaret()
      {
        if (!owner.ReadOnly)
        {
          SendProbe();
          if (SelectionLength > 0 && LineCount > 0)
          {
            RemoveSelection();
          }
          object before = null;
          string value = null;

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
            value = "\n";
          }

          int cl = CurrentLine;

          string l = this[cl];

          int lci = LineCharacterIndex;
#if CHECK
          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, lci);
#endif
          if (lci == 0)
          {
            Insert(cl, string.Empty);
            CaretIndex++;
          }
          else
          {
            string start = l.Substring(0, lci);

            this[cl] = start;

            int indent = GetIndent(start);
            string ins = new string(' ', indent);

            if (!TabsToSpaces)
            {
              int mod = indent % TabSize;
              ins = new string('\t', indent/TabSize);
              indent /= TabSize;

              if (mod > 0)
              {
                indent += mod;
                ins += new string(' ', mod);
              }
            }

            string rest = ins + l.Substring(lci);
            value += ins;

            Insert(cl + 1, rest);

            CaretIndex += indent + 1;
          }

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push(new InsertOperation(before, after, value));
          }
          SendProbe();
        }
      }




      #endregion

      #region Fields

      // Internal state.
      LineState[] lines;
      int gapBottom, gapTop;
      int numLines;
      TextBufferPainter painter;
      internal readonly ArrayList invalidatedlines = ArrayList.Synchronized(new ArrayList());

      bool tabtospace = true;

      bool recording = true;

      int caretindex;
      int textlength;
      int currentline;
      int tabsize = 2;
      int fontheight;
      float ffontheight;
      float fontwidth;
      internal float maxwidth;
      Font font;
      Pen caret;
      int selectionstart;
      bool select = false;
      //Region lastselection = null;

      readonly DoubleLinkedList<TokenLine> mlines = new FastDoubleLinkedList<TokenLine>();

      [Conditional("PROBE")]
      void MiniProbe()
      {
#if PROBE
#warning MINIPROBE ENABLED
        mlines.SendProbe();
#endif
      }

      [Conditional("PROBE")]
      internal void SendProbe()
      {
        MiniProbe();
#if PROBE
#warning PROBE ENABLED
        int i = 0;

        foreach (TokenLine tl in mlines)
        {
          TokenLine lt = GetUserState(i);
          Debug.Assert(tl == lt);
          i++;
        }
#endif
      }

      float[] tabs;

      readonly internal StringFormat sf;

      Timers.HiPerfTimer hp = new Timers.HiPerfTimer();

      Language lang = Language.Default;
      internal Timers.FastTimer parsetimer = new Timers.FastTimer(1000);

      LineHint linehint;
      static readonly LineHint STARTHINT = new LineHint();

      UndoRedoStack undo;

      string filename = null;

      const int nllen = 1;

      // rhys's stuff starts here, string[] lines was changed to LineState[] to carry linestate too
      // and many other changes, not much left except the basic structure

      // Default number of lines to create when empty.
      const int DefaultNumLines = 128;

      AdvancedTextBox owner;

      internal TextBuffer(AdvancedTextBox owner)
      {
        this.owner = owner;
        sf = StringFormat.GenericTypographic.Clone() as StringFormat;
        sf.FormatFlags = StringFormatFlags.NoWrap |
          StringFormatFlags.MeasureTrailingSpaces |
          StringFormatFlags.FitBlackBox //important for measurestring
          //| StringFormatFlags.NoClip;
          //| StringFormatFlags.NoFontFallback
          ;

        //sf.Trimming = StringTrimming.EllipsisCharacter;

        undo = new UndoRedoStack(this);
        Clear();

        try
        {
          Font = ServiceHost.Settings.EditorFont;
        }
        catch
        {
          // this will do tab init
          Font = new Font(FontFamily.GenericMonospace, 10f);
        }

        painter = new TextBufferPainter(this);
        caret = Factory.Pen(Color.Black, 2);
        parsetimer.Tick += new EventHandler(parsetimer_Tick);

      }

      /// <summary>
      /// Gets or sets the Language associated with the buffer.
      /// </summary>
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public Language Language
      {
        get { return lang; }
        set
        {
          if (lang != value)
          {
            lang = value;
            for (int i = 0; i < LineCount; i++)
            {
              SetTokens(i, null);
            }
          }
        }
      }

      /// <summary>
      /// Gets the file name of the loaded file, or a temp name if filename does not exist
      /// </summary>
      public string FileName
      {
        get
        {
          if (filename == null)
          {
            StringBuilder sb = new StringBuilder(12);
            Random r = new Random();

            for (int i = 0; i < 8; i++)
            {
              sb.Append((char)r.Next('a', 'z'));
            }

            if (Language == null)
            {
              sb.Append(".tmp");
            }
            else
            {
              sb.AppendFormat(".{0}", Language.DefaultExtension);
            }
            filename = sb.ToString();
          }
          return filename;
        }
      }

      #endregion

      #region IDisposable Members

      /// <summary>
      /// Disposes this instance
      /// </summary>
      protected override void Dispose(bool disposing)
      {
        Clear();
        sf.Dispose();
        parsetimer.Dispose();
        //font.Dispose();
        //GC.Collect();
      }

      #endregion

      #region Basic operations

      /// <summary>
      /// Clears the TextBuffer and reset itself.
      /// </summary>
      public void Clear()
      {
        textlength = 1;
        caretindex = 0;
        currentline = 0;
        linehint = STARTHINT;
        lines = new LineState[DefaultNumLines];
        gapBottom = 0;
        gapTop = DefaultNumLines;
        numLines = 0;
        selectionstart = 0;
        mlines.Clear();

        ClearUndo();
        recording = true;
      }

      /// <summary>
      /// Get the number of lines in this buffer.
      /// </summary>
      /// <value>
      /// The number of lines.
      /// </value>
      public int LineCount
      {
        get { return numLines; }
      }

      /// <summary>
      /// Get a particular line in the buffer.
      /// </summary>
      /// <param name="index">
      /// The line number. Zero based.
      /// </param>
      /// <value>
      /// The string representing the line.
      /// </value>
      public string this[long index]
      {
        get { return this[(int)index]; }
      }

      /// <summary>
      /// Set a particular line in the buffer.
      /// </summary>
      /// <param name="index">The line number. Zero based.</param>
      /// <param name="value">The new string</param>
      /// <remarks>
      /// This operations is recorded.
      /// </remarks>
      public void SetLines(long index, string[] value)
      {
        if (linehint.line > index)
        {
          UpdateLineHint((int)index - 1);
        }
        ignoretrigger = true;

        object before = null;

        if (recording)
        {
          before = ((IHasUndo)this).GetUndoState();
        }

        int i = 0;
        string[] prev = new string[value.Length];

        while (i < value.Length)
        {
          CurrentLine = (int)index + i;
          string l = prev[i] = this[index + i];
          this[CurrentLine] = value[i];

          i++;
        }

        LineCharacterIndex += (value[value.Length - 1].Length - prev[prev.Length - 1].Length);

        if (recording)
        {
          object after = ((IHasUndo)this).GetUndoState();
          undo.Push(new ReplaceLinesOperation(before, after, prev, value));
        }

        ignoretrigger = false;
      }

      /// <summary>
      /// Set a particular line in the buffer.
      /// </summary>
      /// <param name="index">The line number. Zero based.</param>
      /// <param name="value">The new string</param>
      /// <remarks>
      /// This operations is recorded.
      /// </remarks>
      public void SetLine(long index, string value)
      {
        ignoretrigger = true;

        object before = null;

        if (recording)
        {
          before = ((IHasUndo)this).GetUndoState();
        }

        CurrentLine = (int)index;

        string l = this[index];
        this[CurrentLine] = value;
#if CHECK
        Debug.WriteLine("old: {0} new: {1}", l, value);
#endif
        LineCharacterIndex += (value.Length - l.Length);

        if (recording)
        {
          object after = ((IHasUndo)this).GetUndoState();
          undo.Push(new ReplaceLineOperation(before, after, l, value));
        }

        ignoretrigger = false;
      }

      // internal hack, this seems to concur nicely with the Region limit 
      string this[int index]
      {
        get
        {

          if (index < 0 || index >= numLines)
          {
            return string.Empty;
          }
          else if (index < gapBottom)
          {
            string rvalue = lines[index].line;
            return rvalue == null ? string.Empty : rvalue;
          }
          else
          {
            string rvalue = lines[gapTop + index - gapBottom].line;
            return rvalue == null ? string.Empty : rvalue;
          }
        }
        set
        {
          if (index < 0 || index >= numLines)
          {
            return;
          }

          Debug.Assert(value != null);

          if (TabsToSpaces)
          {
            value = ConvertTabsToSpaces(value);
          }


          string orig = this[index];

          int start = 0, end = 0;

          if (orig.Length > value.Length)
          {
            start = value.Length;
            end = orig.Length;

            while (!orig.StartsWith(value.Substring(0, start)))
            {
              start--;
            }
          }
          else
          {
            start = orig.Length;
            end = value.Length;

            while (!value.StartsWith(orig.Substring(0, start)))
            {
              start--;
            }
          }

          int len = value.Length - orig.Length;
          //caretindex += len;
          if (index < gapBottom)
          {
            if (lines[index].SetLine(value, this, index))
            {
              SetTokens(index + 1, null);
            }
          }
          else
          {
            if (lines[gapTop + index - gapBottom].SetLine(value, this, index))
            {
              SetTokens(index + 1, null);
            }
          }

          textlength += len;
        }
      }

      string ConvertTabsToSpaces(string input)
      {
        int tw = TabSize;

        StringBuilder output = new StringBuilder(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
          char c = input[i];

          if (c == '\t')
          {
            int td = tw - i % tw;
            string ns = new string(' ', td);

            output.Append(ns);
          }
          else
          {
            output.Append(c);
          }
        }

        return output.ToString();
      }

      /// <summary>
      /// Get the user state for a particular line. 
      /// </summary>
      /// <param name="index">the line index</param>
      /// <returns>the user object</returns>
      internal TokenLine GetUserState(int index)
      {
        if (index < 0 || index >= numLines)
        {
          return null;
        }
        else if (index < gapBottom)
        {
          return lines[index].userstate;
        }
        else
        {
          return lines[gapTop + index - gapBottom].userstate;
        }
      }

      /// <summary>
      /// Set the user state for a particular line. 
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="userstate">the user state</param>
      [Obsolete]
      internal void SetUserState(int index, TokenLine userstate)
      {
        if (index < 0 || index >= numLines || userstate == null)
        {
          return;
        }
        else
        {
          if (index < gapBottom)
          {
            lines[index].userstate = userstate;
          }
          else
          {
            lines[gapTop + index - gapBottom].userstate = userstate;
          }
        }
      }

      /// <summary>
      /// Get the lexxed tokens for a particular line.
      /// </summary>
      /// <param name="index">the line index</param>
      /// <returns>The lexxed tokens or null if dirty</returns>
      public IToken[] GetTokens(int index)
      {
        TokenLine tl = GetUserState(index);
        if (tl == null)
        {
          return null;
        }
        return tl.Tokens;
      }

      /// <summary>
      /// Set the lexxed tokens for a particular line.
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="tokens">the lexxed tokens</param>
      internal void SetTokens(int index, IToken[] tokens)
      {
        if (tokens == null)
        {
          invalidatedlines.Add(index);
        }
        TokenLine tl = GetUserState(index);
        //this should never be null
        if (tl != null)
        {
          tl.Tokens = tokens;
          SetDrawCache(index, null);
          owner.viewlines = null;
        }
      }

      /// <summary>
      /// Get the lexxed tokens for a particular line.
      /// </summary>
      /// <param name="index">the line index</param>
      /// <returns>The lexxed tokens or null if dirty</returns>
      internal DrawInfo[] GetDrawCache(int index)
      {
        if (index < 0 || index >= numLines)
        {
          return null;
        }
        else if (index < gapBottom)
        {
          return lines[index].drawcache;
        }
        else
        {
          return lines[gapTop + index - gapBottom].drawcache;
        }
      }

      /// <summary>
      /// Set the lexxed tokens for a particular line.
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="drawcache">the lexxed tokens</param>
      internal void SetDrawCache(int index, DrawInfo[] drawcache)
      {
        if (index < 0 || index >= numLines)
        {
          return;
        }

        if (index < gapBottom)
        {
          lines[index].drawcache = drawcache;
        }
        else
        {
          lines[gapTop + index - gapBottom].drawcache = drawcache;
        }
      }

      /// <summary>
      /// Add a new line to the end of the buffer. 
      /// </summary>
      /// <param name="line">the new text to add</param>
      void Add(string line)
      {
        SendProbe();
        line = line.Replace("\r", string.Empty);
        textlength += line.Length;
        string[] newlines = line.Split('\n');

        // Shift lines in the buffer to make room at the end.
        SetOptimumInsertPosition(numLines, newlines.Length);

        foreach (string s in newlines)
        {
          // Add the new line to the buffer.
          TokenLine nl = new TokenLine();
          AddLine(nl);
          lines[gapBottom].userstate = nl;
          lines[gapBottom].SetLine(s, this, numLines);
          ++gapBottom;
          ++numLines;
          //add new line
          ++textlength;
          MiniProbe();
        }
        SendProbe();
      }

      // Rearrange the buffer for optimum insertion of "count" lines before
      // the specified line labelled "index".
      void SetOptimumInsertPosition(int index, int count)
      {
        int num, len, size, top;

        // Range-check the line index value and count.
        if (index < 0)
        {
          index = 0;
        }
        else if (index > numLines)
        {
          index = numLines;
        }
        if (count < 0)
        {
          count = 0;
        }

        // Determine if we need to enlarge the buffer.
        if ((numLines + count) > lines.Length)
        {
          len = lines.Length;
          size = len * 2;
          while (size < (numLines + count))
          {
            size *= 2;
          }
          LineState[] newLines = new LineState[size];
          if (index < gapBottom)
          {
            num = gapBottom - index;
            top = size - (len - gapTop) - num;
            Array.Copy(lines, 0, newLines, 0, index);
            Array.Copy(lines, gapBottom, newLines, top, num);
            Array.Copy(lines, gapTop, newLines, top + num, len - gapTop);
            gapBottom -= num;
            gapTop = top;
          }
          else
          {
            num = index - gapBottom;
            top = size - (len - gapTop) + num;
            Array.Copy(lines, 0, newLines, 0, gapBottom);
            Array.Copy(lines, gapTop, newLines, gapBottom, num);
            Array.Copy(lines, gapTop + num, newLines, top, len - (gapTop + num));
            gapBottom += num;
            gapTop = top;
          }
          lines = newLines;
          return;
        }

        // Check for which way we need to shift the existing lines.
        if (index < gapBottom)
        {
          // Shift lines from the bottom up to the top.
          num = gapBottom - index;
          Array.Copy(lines, index, lines, gapTop - num, num);
          gapBottom -= num;
          gapTop -= num;
        }
        else if (index > gapBottom)
        {
          // Shift lines from the top down to the bottom.
          num = index - gapBottom;
          Array.Copy(lines, gapTop, lines, gapBottom, num);
          gapBottom += num;
          gapTop += num;
        }
      }

      /// <summary>
      /// Insert a new line into the buffer at a particular position. 
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="line">the text to add</param>
      void Insert(int index, string line)
      {
        if (!owner.ReadOnly)
        {
          SendProbe();
          TokenLine userstate = GetUserState(index - 1); //????
          if (userstate == null && index < 0)
          {
            Debug.Fail("Invalid state");
          }
          userstate = InsertLineAfter(userstate) as TokenLine;

          // Shift lines in the buffer to make room for the insertion.
          SetOptimumInsertPosition(index, 1);

          owner.viewlines = null;
          //owner.preprocess = true;

          textlength += line.Length + nllen;
          // Add the new line to the buffer.
          lines[gapBottom].userstate = userstate;
          bool res = lines[gapBottom].SetLine(line, this, index);

          ++gapBottom;
          ++numLines;

          if (res)
          {
            SetTokens(index + 1, null);
          }

          SendProbe();
        }
      }

      /// <summary>
      /// Insert a group of lines into the buffer at a particular position. 
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="lines">the lines to insert</param>
      void Insert(int index, string[] lines)
      {
        Insert(index, lines, 0, lines.Length);
      }

      /// <summary>
      /// Insert a group of lines into the buffer at a particular position. 
      /// </summary>
      /// <param name="index">the line index</param>
      /// <param name="lines">the lines to insert</param>
      /// <param name="startindex">the start index within lines</param>
      /// <param name="length">the length of the text to insert</param>
      void Insert(int index, string[] lines, int startindex, int length)
      {
        if (!owner.ReadOnly)
        {
          // Bail out if there are no lines.
          if (lines == null || lines.Length == 0 || length == 0)
          {
            return;
          }

          // Shift lines in the buffer to make room for the insertion.
          int num = length;
          SetOptimumInsertPosition(index, num);

          owner.viewlines = null;
          owner.preprocess = true;

          //adjustment for LineState
          LineState[] ls = new LineState[num];

          TokenLine state = GetUserState(index - 1); //???? perhaps ia one index, been to long to remember

          SendProbe();

          bool res = false;

          for (int i = 0; i < num; i++)
          {
            textlength += lines[i + startindex].Length + nllen;
            ls[i].userstate = state = InsertLineAfter(state);
            res = ls[i].SetLine(lines[i + startindex], this, i + startindex);
          }

          // Add the new lines to the buffer.
          Array.Copy(ls, 0, this.lines, gapBottom, num);
          gapBottom += num;
          numLines += num;

          if (res)
          {
            SetTokens(index + length, null);
          }

          SendProbe();
        }
      }

      int GetLength(int line, int count)
      {
        int len = 0;
        for (int i = 0; i < count; i++)
        {
          len += this[line + i].Length + nllen;
        }
        return len;
      }

      /// <summary>
      /// Remove a line from the buffer.
      /// </summary>
      /// <param name="line">the line to remove</param>
      void Remove(int line)
      {
        Remove(line, 1);
      }

      /// <summary>
      /// Removes a sequence of lines from the buffer.
      /// </summary>
      /// <param name="start">the start line index</param>
      /// <param name="count">the number of lines to remove</param>
      void Remove(int start, int count)
      {
        if (!owner.ReadOnly)
        {
          // Range-check the values.
          if ((start + count) > numLines)
          {
            count = numLines - start;
          }
          if (start < 0 || start >= numLines || count <= 0)
          {
            return;
          }

          SendProbe();

          for (int i = 0; i < count; i++)
          {
            RemoveLine(GetUserState(i + start));
            MiniProbe();
          }

          textlength -= GetLength(start, count);

          // Shift lines in the buffer so that the gap is at "start".
          SetOptimumInsertPosition(start, 0);

          owner.viewlines = null;
          //owner.preprocess = true;

          // Adjust the gap to remove the lines.
          gapTop += count;
          numLines -= count;

          //reset the next line so it can be relexed
          SetTokens(start, null);

          SendProbe();
        }
      }

      #endregion

      #region Load / Save

      /// <summary>
      /// Load a text file into this buffer. 
      /// </summary>
      /// <param name="reader">the text source</param>
      public void Load(TextReader reader)
      {
        string line;
        if (reader == null)
        {
          throw new ArgumentNullException("reader");
        }
        try
        {
          Clear();
          int c = 0;

          recording = false;

          Stream input = null;

          if (reader is StreamReader)
          {
            input = ((StreamReader)reader).BaseStream;
            ServiceHost.StatusBar.Progress = 0;
          }

          while ((line = reader.ReadLine()) != null)
          {
            Add(line);
            if (input != null)
            {
              ServiceHost.StatusBar.Progress = input.Position / (float)input.Length;
            }
            c++;
          }

          Add(string.Empty);

          recording = true;

          owner.ReadOnly = lang.ReadOnly;
        }
        finally
        {
          if (reader is StreamReader)
          {
            encoding = ((StreamReader)reader).CurrentEncoding;
          }
        }
      }

      /// <summary>
      /// Load a text file into this buffer. 
      /// </summary>
      /// <param name="stream">the text source</param>
      /// <remarks>The stream will be closed when done loading</remarks>
      public void Load(Stream stream)
      {
        Load(stream, Encoding.Default);
      }

      /// <summary>
      /// Load a text file into this buffer with a specific encoding. 
      /// </summary>
      /// <param name="stream">the text source</param>
      /// <param name="encoding">the encoding to use</param>
      /// <remarks>The stream will be closed when done loading</remarks>
      public void Load(Stream stream, Encoding encoding)
      {
        if (encoding == null)
        {
          encoding = Encoding.Default;
        }
        StreamReader reader = new StreamReader(stream, encoding, true);
        try
        {
          Load(reader);
        }
        finally
        {
          reader.Close();
        }
      }

      /// <summary>
      /// Load a text file into this buffer.
      /// </summary>
      /// <param name="filename">the file to open</param>
      public void Load(string filename)
      {
        Load(filename, Encoding.Default);
      }

      Encoding encoding;

      /// <summary>
      /// Gets or sets the current encoding.
      /// </summary>
      /// <value>The current encoding.</value>
      public Encoding CurrentEncoding
      {
        get { return encoding; }
        set { encoding = value; }
      }

      /// <summary>
      /// Load a text file into this buffer with a specific encoding.
      /// </summary>
      /// <param name="filename">the file to open</param>
      /// <param name="encoding">the encoding to use</param>
      public void Load(string filename, Encoding encoding)
      {
        this.filename = filename;

        string ext = Path.GetExtension(filename).TrimStart('.').ToLower();

        ILanguageService ls = ServiceHost.Language;
        lang = ls.Suggest(filename);

        if (lang.HasFoldInfo)
        {
          owner.ShowFoldbar = lang.HasFoldInfo;
        }

        if (File.Exists(filename))
        {
          Stream s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          Load(s, encoding);
        }
      }

      /// <summary>
      /// Save the contents of this buffer to a text file. 
      /// </summary>
      /// <param name="writer">the text destination</param>
      public void Save(TextWriter writer)
      {
        SaveInternal(writer, true);
      }

      internal void SaveInternal(TextWriter writer, bool resetdirtystate)
      {
        if (writer == null)
        {
          throw new ArgumentNullException("writer");
        }
        int count = LineCount;
        int line;
        for (line = 0; line < count; ++line)
        {
          string tl = this[line];
          if (line == count - 1)
          {
            writer.Write(tl);
          }
          else
          {
            writer.WriteLine(tl);
          }
        }

        if (resetdirtystate)
        {
          lastsavelevel = undo.CurrentLevel;
        }
      }

      /// <summary>
      /// Save the contents of this buffer to a text file. 
      /// </summary>
      /// <param name="stream">the text destination</param>
      public void Save(Stream stream)
      {
        Save(stream, CurrentEncoding ?? Encoding.Default);
      }

      /// <summary>
      /// Save the contents of this buffer to a text file. 
      /// </summary>
      /// <param name="stream">the text destination</param>
      /// <param name="encoding">the encoding to use</param>
      public void Save(Stream stream, Encoding encoding)
      {
        if (encoding == null)
        {
          encoding = CurrentEncoding ?? Encoding.Default;
        }
        StreamWriter writer = new StreamWriter(stream, encoding);
        try
        {
          Save(writer);
        }
        finally
        {
          writer.Close();
        }
      }

      /// <summary>
      /// Save the contents of this buffer to a text file. 
      /// </summary>
      /// <param name="filename">the text destination</param>
      public void Save(string filename)
      {
        Save(filename, CurrentEncoding ?? Encoding.Default);
      }

      /// <summary>
      /// Save the contents of this buffer to a text file. 
      /// </summary>
      /// <param name="filename">the text destination</param>
      /// <param name="encoding">the encoding to use</param>
      public void Save(string filename, Encoding encoding)
      {
        if (encoding == null)
        {
          encoding = CurrentEncoding ?? Encoding.Default;
        }
        StreamWriter writer = new StreamWriter(filename, false, encoding);
        try
        {
          Save(writer);
        }
        finally
        {
          writer.Close();
        }
      }

      #endregion





    }; // class TextBuffer

    #endregion
  }
}
