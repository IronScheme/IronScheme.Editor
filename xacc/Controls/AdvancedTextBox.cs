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
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Drawing.Text;
using ST = System.Threading; // dont include, messes up timers, this doesnt work on pnet
using Xacc.Algorithms;
using System.Diagnostics;
using Xacc.ComponentModel;
using Xacc.Collections;
using System.Runtime.InteropServices;
using Xacc.Languages;
using Xacc.Drawing;

using Pairing = Xacc.Languages.Language.Pairing;
#endregion

namespace Xacc.Controls 
{
	/// <summary>
	/// A damn fine control.
	/// </summary>
	[ToolboxBitmap(typeof(Resources.Fake), "CodeValueType.bmp")]
  [Name("Code Editor")]
  public sealed partial class AdvancedTextBox : Control, IEdit, IFile, IEditSpecial, IEditAdvanced, INavigate, IScroll, IHasCodeModel, IFind
	{
		#region Fields

		const int MAX24BIT = int.MaxValue >> 8;

		private System.ComponentModel.Container components = null;
		
		TextBuffer buffer;
#if TextBufferStream
		TextBufferStream bufferstream;
#endif
		internal VScrollBar vscroll = new VScrollBar();
		HScrollBar hscroll = new HScrollBar();
    ContextMenuStrip contextmenu = new ContextMenuStrip();

#if BROKEN
		Button splitbut = new Button();
#endif

		static Timers.FastTimer caret = new Timers.FastTimer(500);

    bool isreadonly = false;

		bool caretvisible = true;

		bool mousedown = false;
		bool dblclick = false;

		bool showfoldbar = false;
    bool autosave = false;



#if DEBUG
    CodeModel.Location parseloc;

    internal CodeModel.Location ParseLocation
    {
      get { return parseloc; }
      set 
      {
        if (parseloc != value)
        {
          parseloc = value;
          buffer.SelectLocation(parseloc);
          Invalidate();
        }
      }
    }
#endif


		bool linenumbers = true;

		[Flags]
			enum DrawFlags : byte
		{
			None		= 0,
			InfoBar = 1,
			Text		= 2,
			Caret   = 4,
			All			= 7,
		}

    /// <summary>
    /// Gets or sets whether the file is saved automatically (when ever IsDirty is called)
    /// </summary>
    public bool AutoSave
    {
      get {return autosave;}
      set {autosave = value;}
    }


		float infobarw;
		Brush infobarback = SystemBrushes.Menu;
		//Brush infobarfore = SystemBrushes.ControlText;
		Pen infobarborder = Factory.Pen(SystemColors.Highlight, 1);

    //clamp value (15 - 100)
		SolidBrush selbrush = Factory.SolidBrush(Color.FromArgb(Math.Max(15, Math.Min(100, -50 + (int)(SystemColors.Highlight.GetBrightness() * 200))), SystemColors.Highlight));
		Pen selpen = Factory.Pen(SystemColors.Highlight,1);

		GraphicsPath lastgp = null;

		Rectangle lastrec;
		DrawFlags drawflags = DrawFlags.All;

		ControlCollection splitviews ;

		AutoCompleteForm acform;
		bool autocomplete = false;

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.RichTextBox.SelectionColor"]/*'/>
		[Category("Appearance")]
		public Color SelectionColor
		{
			get {return selbrush.Color;}
			set {selbrush.Color = value;}
		}

    /// <summary>
    /// Gets or sets whether to show the foldbar
    /// </summary>
		[Category("Appearance")]
		public bool ShowFoldbar
		{
			get {return showfoldbar;}
			set {showfoldbar = value;}
		}

    /// <summary>
    /// Gets or sets whether the editor is readonly
    /// </summary>
    public bool ReadOnly
    {
      get {return isreadonly;}
      set 
      {
        if (isreadonly != value)
        {
          if (value)
          {
            caretvisible = false;
          }
          else
          {
            caretvisible = true;
          }

          isreadonly = value;
        }
      }
    }

		/// <summary>
		/// If true, converts TAB charaters to spaces on input. TODO!
		/// </summary>
		//TODO: URGENT TabsToSpaces
		[Category("Behavior")]
		public bool TabsToSpaces
		{
			get {return buffer.TabsToSpaces;}
			set {buffer.TabsToSpaces = value;}
		}

		/// <summary>
		/// Provides access to TextBuffer.
		/// </summary>
		/// <value>
		/// The instance of the attached TextBuffer.
		/// </value>
		//[TypeConverter(typeof(ExpandableObjectConverter))]
    [Browsable(false)]
		public TextBuffer Buffer 
    {
      get {return buffer;}
    }

    internal class LanguageTypeConvertor : TypeConverter
    {
      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
        return true;
      }

      public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
        IList ll = ServiceHost.Language.Languages;
        ArrayList sl = new ArrayList();
        foreach (Language l in ll)
        {
          sl.Add(l.Name);
        }
        sl.Sort();
        return new StandardValuesCollection(sl);
      }

      public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
      {
        return false;
      }

    }

    /// <summary>
    /// Gets or sets the editor language.
    /// </summary>
    /// <value>The editor language.</value>
    [TypeConverter(typeof(LanguageTypeConvertor))]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string EditorLanguage
    {
      get {return buffer.Language.Name;}
      set 
      {
        buffer.parsetimer.Enabled = false;
        buffer.Language = ServiceHost.Language.GetLanguage(value);
        Invalidate();
        buffer.parsetimer.Enabled = true;
      }
    }

#if TextBufferStream
		public Stream BufferStream 
		{
			get 
			{
				if (bufferstream == null)
				{
					bufferstream = new TextBufferStream(buffer);
				}
				return bufferstream;
			}
		}
#endif

		#endregion

		#region TextBox compatibility

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.AppendText(System.String)"]/*'/>
		public void AppendText(string text)
		{
			buffer.CaretIndex = TextLength - 1;
			buffer.InsertString(text);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Clear"]/*'/>
		public void Clear()
		{
			buffer.Clear();
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.CanUndo"]/*'/>
		[Browsable(false)]
		public bool CanUndo
		{
			get {return buffer.CanUndo;}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.Lines"]/*'/>
		[Category("Appearance")]
    [ReadOnly(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string[] Lines
		{
			get {return Text.Split('\n');}
			set 
			{
				buffer.Clear();
				buffer.InsertString( String.Join("\n", value));
			}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.CanPaste(System.Windows.Forms.DataFormats.Format)"]/*'/>
		public bool CanPaste(DataFormats.Format clipFormat)
		{
			return (clipFormat.Name == DataFormats.Text);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Undo"]/*'/>
		public void Undo()
		{
			buffer.Undo();
      ScrollToCaret();
		}

    /// <summary>
    /// Redos the previously undone operation
    /// </summary>
    public void Redo()
    {
      buffer.Redo();
      ScrollToCaret();
    }

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.ClearUndo"]/*'/>
		public void ClearUndo()
		{
			buffer.ClearUndo();
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.ScrollToCaret"]/*'/>
		public void ScrollToCaret()
		{
			MoveCaretIntoView();
			Invalidate();
		}

    public void ScrollToCaretUpper()
    {
      MoveCaretIntoViewUpper();
      Invalidate();
    }



		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Select(System.Int32,System.Int32)"]/*'/>
		public void Select(int start, int length)
		{
			buffer.Select(start, length);
      ScrollToCaret();
			// question ? to invalidate : not to invalidate; // yes
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.SelectAll"]/*'/>
		public void SelectAll()
		{
			Select(0, TextLength);
      ScrollToCaret();
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.Text"]/*'/>
		public override string Text
		{
			get {return buffer.Text;}
			set
			{
        bool ro = isreadonly;
        isreadonly = false;
				buffer.Clear();
				buffer.InsertString(value);
        isreadonly = ro;
        Invalidate();
			}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.SelectedText"]/*'/>
		[Browsable(false)]
    [ReadOnly(true)]
		public string SelectionText
		{
			get {return buffer.SelectionText.Replace("\n", Environment.NewLine);}
			set 
			{
				buffer.RemoveSelection();
				int s = buffer.CaretIndex;
				buffer.InsertString(value);
				buffer.Select(s, value.Length);
        ScrollToCaret();
			}
		}

    /// <summary>
    /// Gets the selected text in HTML format
    /// </summary>
    [Browsable(false)]
    public string SelectionHtml
    {
      get 
      {
        int startline;
        IDrawInfo[][] dis = buffer.GetSelectedDrawInfo(out startline);
        StringWriter w = new StringWriter();
        w.Write("<pre style='color:black;border:1 solid windowtext;background-color:white;font-family:Consolas,Bitstream Vera Sans Mono,Lucida Console,Courier New;'>");
        for (int i = 0; i < dis.Length; i++)
        {
          IDrawInfo[] line = dis[i];
          int lastindex = 0;
          foreach (IDrawInfo di in line)			
          {
            if (di.Start > lastindex)
            {
              w.Write(new string(' ',di.Start - lastindex));
            }
            lastindex = di.End;
            string t = di.Text.Replace("&", "&amp;").Replace("<","&lt;").Replace(">","&gt;");

            if (di.ForeColor == Color.Black && di.BackColor == Color.Empty)
            {
              w.Write("{0}", t);
            }
            else
            {
              w.Write("<FONT color={1}>{0}</FONT>", t, ColorTranslator.ToHtml(di.ForeColor));
            }
          }
          if (i < dis.Length - 1)
          {
            w.WriteLine();
          }
        }
        w.Write("</pre>");
        return w.ToString();
      }
    }


    /// <summary>
    /// Gets the selected text in RichTextFormat (RTF)
    /// </summary>
    [Browsable(false)]
    public string SelectionRtf
    {
      get 
      {
        int startline;
        string rtfhead = string.Format(@"{{\rtf1\ansi\ansicpg1252\uc1 \deff0{{\fonttbl{{\f0\fnil\fcharset0\fprq1 {0};}}}}", Font.Name);
        
        string rtftail = @"}";
        IDrawInfo[][] lines = buffer.GetSelectedDrawInfo(out startline);

        Hashtable colors = new Hashtable();
        ArrayList coltab = new ArrayList();

        for (int i = 0; i < lines.Length; i++)
        {
          IDrawInfo[] di = lines[i];
          for (int j = 0; j < di.Length; j++)
          {
            if (!colors.ContainsKey(di[j].ForeColor))
            {
              coltab.Add(di[j].ForeColor);
              colors.Add(di[j].ForeColor, colors.Count);
            }
          }
        }

        StringBuilder sb = new StringBuilder();

        foreach (Color c in coltab)
        {
          sb.AppendFormat(@"\red{0}\green{1}\blue{2};", c.R,c.G,c.B);
        }

        string clrtab = string.Format(@"{{\colortbl{0}}}", sb.ToString());

        string pretext = string.Format(@"\fs{0} ", (int)(Font.SizeInPoints * .8f * 2)); // font size

        sb.Length = 0;
        sb.Append(rtfhead);
        sb.Append(clrtab);
        sb.Append(pretext);

        for (int i = 0; i < lines.Length; i++)
        {
          int lastindex = 0;
          IDrawInfo[] dis = lines[i];
          for (int j = 0; j < dis.Length; j++)
          {
            IDrawInfo di = dis[j];
            if (di.Start > lastindex)
            {
              sb.Append(new string(' ',di.Start - lastindex));
            }
            lastindex = di.End;

            string t = di.Text.Replace("\\", @"\\").Replace("\t", @"\tab ").Replace("}", @"\}").Replace("{", @"\{");

            sb.AppendFormat(@"\cf{0} {1}", colors[di.ForeColor], t);
          }
          if (i < lines.Length - 1)
          {
            sb.Append("\\par \r\n");
          }
        }

        sb.Append(rtftail);

        return sb.ToString();
      }
    }

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.SelectionStart"]/*'/>
		[Browsable(false)]
    [ReadOnly(true)]
		public int SelectionStart
		{
			get {return buffer.SelectionStart;}
			set {buffer.CaretIndex = value;}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.SelectionLength"]/*'/>
		[Browsable(false)]
    [ReadOnly(true)]
		public int SelectionLength
		{
			get {return buffer.SelectionLength;}
			set {Select(SelectionStart, value);}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.TextBoxBase.TextLength"]/*'/>
		[Category("Appearance")]
		public int TextLength
		{
			get {return buffer.TextLength;}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.Char[])"]/*'/>
		public int Find(char[] characterSet)
		{
			return Find(characterSet, 0);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.Char[],System.Int32)"]/*'/>
		public int Find(char[] characterSet, int start)
		{
			return Find(characterSet, start, TextLength - 1);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.Char[],System.Int32,System.Int32)"]/*'/>
		public int Find(char[] characterSet, int start, int end)
		{
			return Find( new string(characterSet), start, end, 0);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find"]/*'/>
		public int Find(string str)
		{
			return Find(str, 0);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.String,System.Int32,System.Int32,System.Windows.Forms.RichTextBoxFinds)"]/*'/>
		public int Find(string str, int start, int end, RichTextBoxFinds options)
		{
			int startline, endline, startci, endci;

			bool matchcase = (options & RichTextBoxFinds.MatchCase) == RichTextBoxFinds.MatchCase;
			
			buffer.GetInfoFromCaretIndex(start, out startline, out startci);
			buffer.GetInfoFromCaretIndex(end, out endline, out endci);

			if (!matchcase)
			{
				str = str.ToLower();
			}

			if ((options & RichTextBoxFinds.Reverse) == RichTextBoxFinds.Reverse)
			{
				for (int i = endline; i >= startline; i--)
				{
					string line = buffer[i];
					if (i == startline)
					{
						line = line.Substring(startci);
					}
					else if (i == endline)
					{
						line = line.Substring(0,endci);
					}

					if (!matchcase)
					{
						line = line.ToLower();
					}
					
					int index = BoyerMoore.LastIndexOf(str, line);
#if CHECKED
					Debug.Assert(index == line.LastIndexOf(str));
#endif
				
					end -= line.Length + 1; //newline

					if ( index != -1)
					{
						return end + index + 1;
					}
				}
			}
			else
			{
				for (int i = startline; i <= endline; i++)
				{
					string line = buffer[i];
					if (i == startline)
					{
						line = line.Substring(startci);
					}
					else if (i == endline)
					{
						line = line.Substring(0,endci);
					}

					if (!matchcase)
					{
						line = line.ToLower();
					}

					int index = BoyerMoore.IndexOf(str, line);
#if CHECKED
					Debug.Assert(index == line.IndexOf(str));
#endif

					if ( index != -1)
					{
						return start + index;
					}
				
					start += line.Length + 1; //newline
				}
			}
			
			return -1;
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.String,System.Int32,System.Windows.Forms.RichTextBoxFinds)"]/*'/>
		public int Find(string str, int start, RichTextBoxFinds options)
		{
			return Find(str, start, TextLength - 1, options);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.String,System.Int32,System.Int32)"]/*'/>
		public int Find(string str, int start, int end)
		{
			return Find(str, start, end, 0);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.String,System.Int32)"]/*'/>
		public int Find(string str, int start)
		{
			return Find(str, start, TextLength - 1);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.Find(System.String,System.Windows.Forms.RichTextBoxFinds)"]/*'/>
		public int Find(string str, RichTextBoxFinds options)
		{
			return Find(str, 0, options);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.GetCharFromPosition(System.Drawing.Point)"]/*'/>
		//TODO: public char GetCharFromPosition(Point pt)
		public char GetCharFromPosition(Point pt)		
		{
			throw new NotImplementedException(MethodBase.GetCurrentMethod().ToString());
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.GetCharIndexFromPosition(System.Drawing.Point)"]/*'/>
		//TODO: public int GetCharIndexFromPosition(Point pt)		
		public int GetCharIndexFromPosition(Point pt)		
		{
			throw new NotImplementedException(MethodBase.GetCurrentMethod().ToString());
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.GetLineFromCharIndex(System.Int32)"]/*'/>
		public int GetLineFromCharIndex(int index)		
		{
			int line, ci;
			buffer.GetInfoFromCaretIndex(index, out line, out ci);
			return line;
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.GetPositionFromCharIndex(System.Int32)"]/*'/>
		//TODO: public Point GetPositionFromCharIndex(int index)		
		public Point GetPositionFromCharIndex(int index)		
		{
			throw new NotImplementedException(MethodBase.GetCurrentMethod().ToString());
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.LoadFile(System.IO.Stream,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void LoadFile(Stream data)		
		{
			LoadFile(data, RichTextBoxStreamType.PlainText);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.LoadFile(System.IO.Stream,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void LoadFile(Stream data, RichTextBoxStreamType fileType)		
		{
			if (fileType == RichTextBoxStreamType.PlainText || 
				fileType == RichTextBoxStreamType.UnicodePlainText)
			{
				buffer.Load(data);
				data.Close();
			}
			else
			{
				throw new NotSupportedException("Only plain text is supported");
			}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.LoadFile(System.String)"]/*'/>
		public void LoadFile(string path)		
		{
			LoadFile(path, RichTextBoxStreamType.PlainText);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.LoadFile(System.String,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void LoadFile(string path, RichTextBoxStreamType fileType)		
		{
			if (fileType == RichTextBoxStreamType.PlainText || 
				fileType == RichTextBoxStreamType.UnicodePlainText)
			{
				buffer.Load(path);
        lastsavetime = DateTime.Now;
			}
			else
			{
				throw new NotSupportedException("Only plain text is supported");
			}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.SaveFile(System.IO.Stream,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void SaveFile(Stream data)		
		{
			buffer.Save(data);
		}

    DateTime lastsavetime;

    /// <summary>
    /// Gets the date the buffer was last saved
    /// </summary>
    [Browsable(false)]
    public DateTime LastSaveTime
    {
      get {return lastsavetime;}
    }

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.SaveFile(System.IO.Stream,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void SaveFile(Stream data, RichTextBoxStreamType fileType)		
		{
			if (fileType == RichTextBoxStreamType.PlainText || 
				fileType == RichTextBoxStreamType.UnicodePlainText)
			{
				buffer.Save(data);
			}
			else
			{
				throw new NotSupportedException("Only plain text is supported");
			}
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.SaveFile(System.String)"]/*'/>
		public void SaveFile(string path)		
		{
			SaveFile(path, RichTextBoxStreamType.PlainText);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.RichTextBox.SaveFile(System.String,System.Windows.Forms.RichTextBoxStreamType)"]/*'/>
		public void SaveFile(string path, RichTextBoxStreamType fileType)		
		{
			if (fileType == RichTextBoxStreamType.PlainText || 
				fileType == RichTextBoxStreamType.UnicodePlainText)
			{
        lastsavetime = DateTime.MaxValue;
				buffer.Save(path);
        lastsavetime = DateTime.Now;
			}
			else
			{
				throw new NotSupportedException("Only plain text is supported");
			}
		}

		#endregion
 
		#region Control override behaviour

    /// <summary>
    /// Gets or sets the font of the text displayed by the control.
    /// </summary>
    /// <value></value>
    /// <returns>The <see cref="T:System.Drawing.Font"></see> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont"></see> property.</returns>
    /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Font Font
    {
      get
      {
        return base.Font;
      }
      set
      {
        base.Font = value;
      }
    }


    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.ParentChanged"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      if (!SettingsService.idemode)
      {
        Form f = FindForm();
        if (ServiceHost.Window == null && f != null)
        {
          f.KeyPreview = true;
          new WindowService(f);

          //new ViewService();
          new KeyboardHandler();

          ServiceHost.State = ApplicationState.Edit | ApplicationState.Buffer | ApplicationState.Navigate | 
            ApplicationState.Scroll;

          //after everything has been loaded
          ServiceHost.Initialize();
          ServiceHost.Scripting.InitCommand();
        }
      }
      base.OnParentChanged (e);
    }


		/// <summary>
		/// Creates a new instance of the AdvancedTextBox control.
		/// </summary>
		public AdvancedTextBox()
		{
      SetStyle(
        //this has been said to increase performance, it does not in fact, well it depends
        ControlStyles.AllPaintingInWmPaint |
        ControlStyles.OptimizedDoubleBuffer | 
        ControlStyles.UserPaint |
        //ControlStyles.Opaque |
        //ControlStyles.SupportsTransparentBackColor |
        ControlStyles.Selectable |
        //ControlStyles.ContainerControl | //???
        ControlStyles.StandardClick |
        ControlStyles.ResizeRedraw, true);
      UpdateStyles();

      buffer = new TextBuffer(this);

      dep.DashStyle = DashStyle.Dot;

			InitializeComponent();

			selpen.LineJoin = LineJoin.Round;
			selpen.Alignment = PenAlignment.Outset;

			caret.Tick +=new EventHandler(UpdateCaret);

      acform.VisibleChanged+=new EventHandler(acform_VisibleChanged);

      buffer.InsertString(string.Empty);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnFontChanged(System.EventArgs)"]/*'/>
		protected override void OnFontChanged(EventArgs e)
		{
			buffer.Font = Font;
			base.OnFontChanged (e);
		}

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnHandleCreated(System.EventArgs)"]/*'/>
    protected override void OnHandleCreated(EventArgs e)
    {
      base.OnHandleCreated (e);
      caret.Enabled = true;
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnHandleDestroyed(System.EventArgs)"]/*'/>
    protected override void OnHandleDestroyed(EventArgs e)
    {
      caret.Enabled = false;
      base.OnHandleDestroyed (e);
    }

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.Dispose(System.Boolean)"]/*'/>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
				{
					components.Dispose();
				}
#if BROKEN
				if (!splitted)
				{
					buffer.Dispose();
				}
#else 
        buffer.Dispose();
#endif

			}
			base.Dispose( disposing );
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.IsInputKey(System.Windows.Forms.Keys)"]/*'/>
		protected override bool IsInputKey(Keys keyData)
		{
			return (keyData & Keys.Control) != Keys.Control;
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.IsInputChar(System.Char)"]/*'/>
		protected override bool IsInputChar(char charCode)
		{
      if (charCode > 0 && (charCode < 30 || charCode == 127))
      {
        return false;
      }
			switch ((int) charCode)
			{
					//these are CTRL + char = (int) char
				case 1:  //CTRL + A
				case 3:  //CTRL + C
				case 6:  //CTRL + F
				case 22: //CTRL + V
				case 24: //CTRL + X
				case 26: //CTRL + Z
				case 27: //ALT + right, i think
				case '\t': 
				case 8:  
				case 10: //CTRL + D == Enter
				case 13: 
					return false;
			}
			
			return true;
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="P:System.Windows.Forms.Control.DefaultSize"]/*'/>
		protected override Size DefaultSize
		{
			get	{	return new Size(300, 200);}
		}

		void InitializeComponent()
		{
			SuspendLayout();

			components = new System.ComponentModel.Container();
			splitviews = new ControlCollection(this);
			BackColor = Color.White;

			Font = buffer.Font;

      if (SettingsService.idemode)
      {
        ToolStripMenuItem mi = ServiceHost.Menu["Edit"];

        foreach (ToolStripItem m in mi.DropDownItems)
        {
          ToolStripMenuItem pmi = m as ToolStripMenuItem;
          if (pmi != null)
          {
            ToolStripMenuItem cmi = pmi.Clone();
            cmi.Enabled = true;
            contextmenu.Items.Add(cmi);
          }
          else
          {
            contextmenu.Items.Add(new ToolStripSeparator());
          }
        }

        ContextMenuStrip = contextmenu;
      
        //ContextMenu = contextmenu;
      }

			vscroll.Location = new Point(Width - vscroll.Width, 0);
			vscroll.Height = Height - hscroll.Height;
			hscroll.Location = new Point( 0, Height - hscroll.Height);
			hscroll.Width = Width - vscroll.Width;

			vscroll.Cursor = Cursors.Arrow;
			hscroll.Cursor = Cursors.Arrow;

			vscroll.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
			hscroll.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

			vscroll.ValueChanged +=new EventHandler(vscroll_ValueChanged);
			hscroll.ValueChanged +=new EventHandler(hscroll_ValueChanged);
 
			vscroll.Minimum = 0;
			vscroll.SmallChange = 1;

			hscroll.Minimum = 0;
			hscroll.SmallChange = 1;
			hscroll.LargeChange = 50;
			hscroll.Maximum = 120;

			Controls.Add(hscroll);
			Controls.Add(vscroll);

 #if BROKEN
			splitbut.TabStop = false;
			splitbut.FlatStyle = FlatStyle.System;
			splitbut.Location = new Point(Width - vscroll.Width, Height - hscroll.Height);
			splitbut.Size = new Size(vscroll.Width, hscroll.Height);
			splitbut.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      splitbut.Cursor = Cursors.Arrow;
			splitbut.MouseDown +=new MouseEventHandler(coolbutt_MouseDown);
			splitbut.MouseUp +=new MouseEventHandler(coolbutt_MouseUp);

			Controls.Add(splitbut);
#endif			
			acform = new AutoCompleteForm(this);
			components.Add(acform);

			ResumeLayout();

			Cursor = Cursors.IBeam;
		}

		#endregion

    #region FileSupport

    void IFile.Save(string filename)
    {
      SaveFile(filename, RichTextBoxStreamType.PlainText);
    }

    void IDocument.Open(string filename)
    {
      caret.Enabled = false;
      LoadFile(filename);
      caret.Enabled = true;
    }

    void IDocument.Close()
    {
      if (ProjectHint != null)
      {
        ProjectHint.CloseFile(buffer.FileName);
      }
    }

    string IDocument.Info
    {
      get {return string.Format("Line: {0}   Col: {1}   Char: {2}   Index: {3}", buffer.CurrentLine + 1,
             buffer.LineColumnIndex + 1, buffer.LineCharacterIndex + 1, buffer.CaretIndex);}
    }

    bool IFile.IsDirty
    {
      get {return buffer.IsDirty;}
    }

    #endregion

		#region Clipboard Operations

    [ClassInterface(ClassInterfaceType.None)]
    class DataObject : IDataObject
    {
      readonly Hashtable data = new Hashtable();

      public DataObject(string text, string rtf, string html)
      {
        if (text != null)
        {
          data[DataFormats.Text] = text;
        }
        if (rtf != null)
        {
          data[DataFormats.Rtf] = rtf;
        }
        if (html != null)
        {
          data[DataFormats.Html] = html;
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


		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Copy"]/*'/>
		public void Copy()
		{
			Clipboard.SetDataObject( new DataObject(SelectionText, SelectionRtf, SelectionHtml), true);
		}

    /// <summary>
    /// Copies the selected text to the clipboard in text Dataformat as Text
    /// </summary>
    public void CopyToText()
    {
      Clipboard.SetDataObject( new DataObject(SelectionText, null, null), true);
    }

    /// <summary>
    /// Copies the selected text to the clipboard in text Dataformat as Html
    /// </summary>
    public void CopyToHtml()
    {
      Clipboard.SetDataObject( new DataObject(SelectionHtml, null, SelectionHtml), true);
    }

    /// <summary>
    /// Copies the selected text to the clipboard in text Dataformat as Rtf
    /// </summary>
    public void CopyToRtf()
    {
      Clipboard.SetDataObject( new DataObject(SelectionRtf, SelectionRtf, null), true);
    }

    /// <summary>
    /// Strips trailing whitespace from all the end of lines of the current content
    /// </summary>
    public void StripTrailingWhiteSpace()
    {
      if (autocomplete)
      {
        acform.Hide();
        autocomplete = false;
      }

      string t = Text;
      string[] lines = t.Split('\n');
      for (int i = 0; i < lines.Length; i++)
      {
        string l = lines[i];
        l = l.TrimEnd(' ','\t','\r');
        lines[i] = l;
      }

      t = string.Join("\n", lines);

      SelectAll();
      buffer.InsertString(t);
    }

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Cut"]/*'/>
		public void Cut()
		{
      if (autocomplete)
      {
        acform.Hide();
        autocomplete = false;
      }

      Clipboard.SetDataObject( new DataObject(SelectionText, SelectionRtf, SelectionHtml), true);

			buffer.RemoveSelection();
      ScrollToCaret();
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.TextBoxBase.Paste"]/*'/>
		public void Paste()
		{
      if (autocomplete)
      {
        acform.Hide();
        autocomplete = false;
      }

			buffer.RemoveSelection();
      IDataObject da = Clipboard.GetDataObject();

			string cbt = da.GetData(DataFormats.Text) as string;
      
			if (cbt != null)
			{
				buffer.InsertString(cbt);
			}

      ScrollToCaret();
		}

    /// <summary>
    /// Deletes the current selected text
    /// </summary>
		public void DeleteSelected()
		{
      if (autocomplete)
      {
        acform.Hide();
        autocomplete = false;
      }

			buffer.RemoveSelection();
			ScrollToCaret();
		}

		#endregion

    #region Fold Support

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnGotFocus(System.EventArgs)"]/*'/>
		protected override void OnGotFocus(EventArgs e)
		{
      if (!SettingsService.idemode)
      {
        (ServiceHost.File as FileManager).ToggleCurrent(this);
      }
      caret.Enabled = true;
			drawflags |= DrawFlags.All;
			if (Visible)
			{
				Invalidate();
			}
			base.OnGotFocus (e);
		}

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      caret.Enabled = false;
      drawflags |= DrawFlags.All;
      if (Visible)
      {
        Invalidate();
      }
      base.OnLostFocus (e);
    }

    /// <summary>
    /// Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"></see> event.
    /// </summary>
    protected override void OnCreateControl()
    {
      if (!SettingsService.idemode)
      {
        ToolStripMenuItem mi = ServiceHost.Menu["Edit"];

        foreach (ToolStripItem m in mi.DropDownItems)
        {
          ToolStripMenuItem pmi = m as ToolStripMenuItem;
          if (pmi != null)
          {
            ToolStripMenuItem cmi = pmi.Clone();
            cmi.Enabled = true;
            contextmenu.Items.Add(cmi);
          }
          else
          {
            contextmenu.Items.Add(new ToolStripSeparator());
          }
        }
      
        ContextMenuStrip = contextmenu;

        (ServiceHost.File as FileManager).HackCurrent(this);
      }
      base.OnCreateControl ();
    }



		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnResize(System.EventArgs)"]/*'/>
		protected override void OnResize(EventArgs e)
		{
			drawflags |= DrawFlags.All;
			Invalidate();
			base.OnResize (e);
		}

    /// <summary>
    /// Whether to show line numbers
    /// </summary>
		[Category("Appearance")]
		public bool ShowLineNumbers
		{
			get {return linenumbers;}
			set 
			{
				if (value != linenumbers)
				{
					linenumbers = value;
					Invalidate();
				}
			}
		}

    // sorted list of pairings
    ArrayList pairings = new ArrayList();

    // map<int,Pairing>
    readonly Hashtable pairmap = new Hashtable();

    internal void LoadPairings(ArrayList p)
    {
      pairings = p;
      viewlines = null;
    }

    FoldStyle GetFoldStyle(int line)
    {
      line++;
      foreach (Pairing p in pairings)
      {
        if (p.start.LineNumber == line)
        {
          return p.hidden ? FoldStyle.Plus : FoldStyle.Minus;
        }
        if (p.end != null)
        {
          if (p.end.LineNumber + p.end.LineCount == line)
          {
            return p.hidden ? FoldStyle.Plus : FoldStyle.Exit;
          }
        }
      }
      return FoldStyle.None;
    }

    void TogglePairAt(int line)
    {
      line++;
      Pairing p = pairmap[line] as Pairing;
      if (p != null)
      {
        p.hidden = !p.hidden;
      }
    }

    string GetPairText(int line)
    {
      line++;
      Pairing p = pairmap[line] as Pairing;
      if (p != null && p.hidden)
      {
        return p.text;
      }
      return null;
    }

    int GetFoldDepthAt(int line)
    {
      line++;
      foreach (Pairing p in pairings)
      {
        if (p.IsInside(line))
        {
          return 1;
        }
      }
      return 0;
    }

    int IsHidden(int line)
    {
      
      while (line < linestate.Length && linestate[line] == -1)
      {
        line++;
      }
      return line;
    }

    int FindHiddenStart(int line)
    {
      while (line >= 0 && linestate[line] == -1)
      {
        line--;
      }
      return line;
    }

    int[] linestate;

    int GetVirtualLineCount(int count)
    {
      pairmap.Clear();
      linestate = new int[count];
      Set hiddens = new Set();
      foreach (Pairing p in pairings)
      {
        pairmap.Add(p.start.LineNumber, p);
        if (p.hidden && p.end != null)
        {
          for (int i = p.start.LineNumber; i < p.end.LineNumber + p.end.LineCount; i++)
          {
            hiddens.Add(i);
            linestate[i] = -1;
          }
        }
      }
      return count - hiddens.Count;
    }

    int[] BuildLines(int length)
    {
      int start = 0;
      int[] lines = new int[length];

      for (int i = 0; i < lines.Length; i++)
      {
        int cline = i + start; 
        int rline = IsHidden(cline);
        start += (rline - cline);
        lines[i] = rline;
      }

      return lines;
    }

    Color GetPairingColor(int line)
    {
      line++;
      Pairing p = pairmap[line] as Pairing;
      if (p != null)
      {
        if (p is Language.Region)
        {
          return Color.DimGray;
        }
        if (p is Language.Conditional)
        {
          if (((Language.Conditional)p).disabled)
          {
            return Color.Orange;
          }
          else
          {
            return Color.Green;
          }
        }
      }
      return Color.Empty;
    }

    int[] viewlines;
    bool preprocess = true;

    void DoViewLineInit()
    {
      if (preprocess)
      {
        buffer.Preprocessor();
      }
      preprocess = true;
      vlinecount = GetVirtualLineCount(buffer.LineCount);
      viewlines = BuildLines(vlinecount);
    }

    #endregion
    
    #region Painting

    readonly static Font menufont = SystemInformation.MenuFont;

    Brush bgbrush = null;
    static readonly Brush menubrush = Factory.SolidBrush(SystemColors.Menu);
    static readonly Pen SystemPensHighlight = SystemPens.Highlight;
    static readonly Brush SystemBrushesHotTrack = SystemBrushes.HotTrack,
      SystemBrushesControlText = SystemBrushes.ControlText,
      SystemBrushesHighlight = SystemBrushes.Highlight,
      SystemBrushesButtonShadow = SystemBrushes.ButtonShadow;

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
    protected override void OnBackColorChanged(EventArgs e)
    {
      base.OnBackColorChanged (e);

      bgbrush = Factory.SolidBrush(BackColor);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
      pevent.Graphics.FillRectangle(bgbrush, pevent.ClipRectangle);
      base.OnPaintBackground(pevent);
    }


    static readonly SettingsService settings = ServiceHost.Settings as SettingsService;


 		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnPaint(System.Windows.Forms.PaintEventArgs)"]/*'/>
    protected sealed override void OnPaint(PaintEventArgs e)
    {
      /* How DoubleBuffering works from my experiences
         * 
         * - DB clears the background on everyflip
         * - DB forces you to redraw the whole screen
         * 
         * Clipping approach
         * 
         * - Draw in samefashion as DB, but clip changed regions
         * 
         * Conclusion
         * 
         * - These 2 cannot co-exist.
         */

      caret.Enabled = false;
#if BROKEN
      if (splitted && Focused)
      {
        splitview.drawflags = drawflags;
        splitview.Invalidate();
      }
#endif

      Graphics g = e.Graphics;

      g.SetClip(e.ClipRectangle);

      int firstline = vscroll.Value;
      int lr = firstline;
      int fh = buffer.FontHeight;
      int h = Height - hscroll.Height;
      if (h < 0)
      {
        h = 0;
      }
      int w = Width - vscroll.Width;
      if (w < 0)
      {
        w = 0;
      }
      int ll = (int)(h/fh) + lr;

      buffer.LexInvalidatedLines();
 
      if (viewlines == null)
      {
        DoViewLineInit();
      }

      int vlinecount = this.vlinecount;
      int[] lines = viewlines;

      // adjust the width of the line numbers
      int lnw = 50; 
      
      try
      {
        lnw = lines.Length == 0 ? 0 
          : (ll < lines.Length ? (lines[ll] + 1) : (lines[lines.Length - 1] + 1)).ToString().Length;
      }
      catch (IndexOutOfRangeException)
      {
        Debugger.Break();
      }


      float fw5 = linenumbers ? buffer.FontWidth * (lnw + 1) : 0;
      float hfw = buffer.FontWidth * .5f;
      float xoffset = hscroll.Value * buffer.FontWidth;
      int cl = buffer.CurrentLine;
      int yoffset = -firstline * fh;

      Brush bgbrush = this.bgbrush;

      g.TextRenderingHint = settings.ClearType ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.SystemDefault;

      lastrec = e.ClipRectangle;

      infobarw = fw5 + hfw;

      DoLayout(g, firstline, ll, vlinecount);

      RectangleF tr = new RectangleF(	xoffset - 1, 0,
        w - infobarw + (showfoldbar ? 12 : 0)
        , MAX24BIT
        ); //NOTE: it appears the max width is 24bits, anything beyond fails
      RectangleF lcr = buffer.lastcr;
      RectangleF cr = buffer.GetCaretRectF();

      int j = Array.BinarySearch(lines, cl);
      cr.Offset(0, (j - cl) * fh);
      vline = j;

      RectangleF lclhi = new RectangleF(0, lcr.Y + yoffset,fw5, fh);
      RectangleF clhi = new RectangleF(0, cr.Y + yoffset,fw5, fh);

      RectangleF infobarr = new RectangleF(0, 0, infobarw, h);

      //necesary?
      if (cl <= ll && cl >= lr && lclhi != clhi)
      {
        g.SetClip(lclhi, CombineMode.Union);
        g.SetClip(clhi, CombineMode.Union);
      }

      // draw infobar + line numbers				
      g.FillRectangle(menubrush, infobarr);
      g.FillRectangle(bgbrush, fw5, 0, hfw, h);

      if (linenumbers)
      {
        //hilite current line
        //j = Array.BinarySearch(lines, cl); //already done
        g.FillRectangle(selbrush, 0, (j - firstline) * fh + 1, fw5, fh);
        
        g.DrawLine(SystemPensHighlight, fw5, 0, fw5, h);

         while (lr <= ll && lr < vlinecount)
        {
          j = lines[lr];

          g.DrawString(
            (j
#if !CHECKED 
            // fix up for real use, 1 based indices are horrible!
            + 1
#endif
            ).ToString(), buffer.Font, 
            j == buffer.CurrentLine ? SystemBrushesHotTrack:  
            j%10 == 9 ? SystemBrushesControlText :	SystemBrushesButtonShadow, 
            hfw, (lr - firstline)* fh + 1, buffer.sf);
          lr++;
        }
			
        //if ((drawflags & DrawFlags.InfoBar) == DrawFlags.InfoBar)
        {
          RectangleF ib = infobarr;
          ib.Width -= 1;
          g.SetClip(ib, CombineMode.Exclude);
        }
      }

      //adjust the "rendering origin"
      g.TranslateTransform(infobarw - hscroll.Value * buffer.FontWidth + (showfoldbar ? 13 : 1) , yoffset);

      // adjust left margin
      tr.X -= 1;
      tr.Width += 1;
      g.IntersectClip(tr);

      bool lcrt = lcr.IntersectsWith(tr);
      bool crt = cr.IntersectsWith(tr);

      if (lcrt)
      {
        g.SetClip(lcr, CombineMode.Union);
      }
      if (crt)
      {
        g.SetClip(cr, CombineMode.Union);
      }

      // fill with default backcolor
      RectangleF gclip = g.Clip.GetBounds(g);

      //Trace.WriteLine(string.Format("gclip: {0}", gclip));
      //Trace.WriteLine(string.Format("tr:    {0}", tr));


      //g.FillRectangle(bgbrush, gclip);

      if ((drawflags & DrawFlags.Text) != DrawFlags.Text)
      {
        g.Clip.Exclude(tr);
      }

      if (SelectionLength > 0)
      {
        GraphicsPath gp = buffer.GetPath(buffer.SelectionStart, buffer.SelectionLength, firstline, ll);

        Rectangle bounds = Rectangle.Ceiling(gp.GetBounds());
        if (!bounds.IsEmpty)
        {
          SmoothingMode sm = g.SmoothingMode;
          PixelOffsetMode pm = g.PixelOffsetMode;
          g.PixelOffsetMode = PixelOffsetMode.None;
          g.SmoothingMode = SmoothingMode.HighQuality;
          g.FillPath(selbrush, gp);
          g.DrawPath(selpen, gp);
          g.SmoothingMode = sm;
          g.PixelOffsetMode = pm;
        }

        lastgp = gp;
      }
      else
      {
        lastgp = null;
      }

      bool bglex = false;
      Hashtable bps = GetBreakpoints();
      int i = 0;

      int bpw = (int)(w - infobarw - hfw*2);
      for(i = firstline; i <= ll && i < vlinecount; i++)
      {
        j = lines[i];
        
        if (debugline == j)
        {
          if (debugexcept)
          {
            Drawing.Utils.PaintLineHighlight(deb, dep, g, fh * i + 1, bpw, fh - 1, true);
          }
          else
          {
            Drawing.Utils.PaintLineHighlight(db, dp, g, fh * i + 1, bpw, fh - 1, true);
          }
        }
        else if (bps != null)
        {
          Breakpoint bp = bps[j] as Breakpoint;
          if (bp != null)
          {
            if (bp.bound)
            {
              Drawing.Utils.PaintLineHighlight(bpb, bpp, g, fh * i + 1, bpw, fh - 1, bp.enabled);
            }
            else
            {
              Drawing.Utils.PaintLineHighlight(ubbpb, ubbpp, g, fh * i + 1, bpw, fh - 1, bp.enabled);
            }
          }
        }
        string ptext = GetPairText(j);
        if (ptext != null)
        {
          Color c = GetPairingColor(j);
          buffer.DrawFoldedText(g, ptext, c, i, j);
          
        }
        else
        {
          bglex = buffer.DrawString(g, i, j);        
        }
      }

      if (Focused && caretvisible && !ReadOnly)
      {
#if SCOPEOUTLINER
				if (false)
				{
					//draw scope outline

				}
#endif
        buffer.DrawCaret(g,cr);
      }

      if (showfoldbar)
      {
        // instead of scaling, we'll use a constant +/- box (like VS.NET)
        g.TranslateTransform((hscroll.Value * buffer.FontWidth - 10 - 5), 0);
        int cheight = (ll - firstline + 2) * fh;
        if (cheight < Height)
        {
          cheight = Height;
        }
        g.SetClip( new RectangleF(
          0, firstline * fh, 13, cheight));

        g.FillRegion(bgbrush, g.Clip);

        Pen fpen = Factory.Pen(Color.DimGray, 1);

        float d = buffer.FontDescent;
	
        for(i = firstline; i <= ll + 1 && i < vlinecount; i++)
        {
          j = lines[i];

          int y = (i + 1) * fh;
          FoldStyle fs = GetFoldStyle(j);

          switch (fs)
          {
            case FoldStyle.None:
              if (GetFoldDepthAt(j) > 0)
              {
                g.DrawLine(fpen, 5, y + (j == 0 ? 4 : 0), 5, y - fh - (j == buffer.LineCount - 1 ? 4 : 0));
              }
              break;
            case FoldStyle.Plus:
            {
              int r = (int)(y - d - 8) ;
              Rectangle x = new Rectangle(1,r,8,8);
              g.DrawRectangle(fpen, x);
              g.DrawLine(fpen, 3, r + 4, 7, r + 4);
              g.DrawLine(fpen, 5, r + 2, 5, r + 6);
              break;
            }
            case FoldStyle.Minus:
            {
              int r = (int)(y - d - 8) ;
              Rectangle x = new Rectangle(1,r,8,8);
              g.DrawRectangle(fpen, x);
              g.DrawLine(fpen, 3, r + 4, 7, r + 4);
              break;
            }
            case FoldStyle.Exit:
              g.DrawLine(fpen, 5, y -fh + fh/2, 5, y - fh - (j == buffer.LineCount - 1 ? 4 : 0));
              g.DrawLine(fpen, 5, y -fh + fh/2, 8, y -fh + fh/2);
              break;
          }
        }

      }

      if (Runtime.Compiler.CLRRuntime == Runtime.CLR.Mono)
      {
        //g.TranslateTransform(-(infobarw - hscroll.Value * buffer.FontWidth + (showfoldbar ? 13 : 1)) , -yoffset);
      }


      drawflags = DrawFlags.None;
      caret.Enabled = true; 

      //Trace.WriteLine("OnPaint completed");
    }

#if CHECKED
		SmoothingMode smooth;
		public SmoothingMode Smooth { get {return smooth;} set {smooth = value;}}
		PixelOffsetMode pixel;
		public PixelOffsetMode Pixel { get {return pixel;} set {pixel = value;}}

#endif

		#endregion

		#region Mouse Handling

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnDoubleClick(System.EventArgs)"]/*'/>
		protected override void OnDoubleClick(EventArgs e)
		{
			int ci = buffer.LineCharacterIndex, cl = buffer.CurrentLine; 
			foreach (IToken t in buffer.GetTokens(cl))
			{
        int col = t.Location.Column;
				if (col <= ci && col + t.Length > ci)
				{
					int caretindex = buffer.GetCaretIndexFromLine(cl);
					buffer.Select(caretindex + col, t.Length);
					break;
				}
			}
			dblclick = true;
			Invalidate();
			base.OnDoubleClick (e);
		}

		//TODO: int GetCaretIndexFromPoint(int x, int y)
		int GetCaretIndexFromPoint(int x, int y)
		{
			SizeF size;
			int cl = (int)(y/buffer.FontHeight) + vscroll.Value;
			if (cl < 0) 
			{
				cl = 0;
			}

			using (Graphics g = CreateGraphics())
			{
				int cfitted, lfilled;
				string txt = buffer[cl];

				size = g.MeasureString(txt, buffer.Font, 
					new SizeF(x - infobarw
					, buffer.FontHeight) , buffer.sf, out cfitted, out lfilled);

				return buffer.GetCaretIndexFromLine(cl) + cfitted;
			}
		}

		void MousePositionTranslate(int x, int y, bool repaint)
		{
      if (viewlines == null)
      {
        preprocess = false;
        DoViewLineInit();
      }
      if (viewlines.Length > 0)
      {
        //Focus the control if not focused
        if (!Focused)
        {
          Focus();
        }

        int yoffset = (int)(y/buffer.FontHeight);

        int vcl = vline = yoffset + vscroll.Value;
        if (vcl >= vlinecount)
        {
          vcl = vline = vlinecount - 1;
        }
        int cl = 0;
        if (vcl < 0)
        {
          cl = viewlines[0];
        }
        else if (vcl >= viewlines.Length)
        {
          cl = viewlines[viewlines.Length - 1];
        }
        else
        {
          cl = viewlines[vcl];// + vscroll.Value;
        }

        if (cl < 0) 
        {
          return;
        }

        if (cl >= buffer.LineCount)
        {
          cl = buffer.LineCount - 1;
        }


        buffer.CurrentLine = cl;
        buffer.LineColumnIndex = (int)( ((x - infobarw - (showfoldbar ? 12 : 0))/buffer.FontWidth) +.5f
          + hscroll.Value);


        if (vcl - vscroll.LargeChange + vscroll.SmallChange > vscroll.Value 
          && vscroll.Maximum >  vcl - vscroll.LargeChange + vscroll.SmallChange)
        {
          //this will invalidate 
          vscroll.Value = vcl - vscroll.LargeChange + vscroll.SmallChange + 1;
        }
        else if (vcl < vscroll.Value && vscroll.Minimum <= vcl)
        {
          vscroll.Value = vcl;
        }
        else if (repaint) 
        {
          drawflags |= DrawFlags.Caret;
          Invalidate();
        }
      }
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnMouseWheel(System.Windows.Forms.MouseEventArgs)"]/*'/>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
      autocomplete = acform.Visible = false;

			caretvisible = true;
			int p = vscroll.Value - e.Delta/40;
			if (p < 0)
			{
				vscroll.Value = 0;
			}
			else if (p > vscroll.Maximum - vscroll.LargeChange)
			{
				vscroll.Value = vscroll.Maximum - vscroll.LargeChange + vscroll.SmallChange; //why???
			}
			else
			{
				vscroll.Value = p;
			}
			if (e.Button == MouseButtons.Left && mousedown)
			{
				MousePositionTranslate(e.X, e.Y, true);
			}
			base.OnMouseWheel (e);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnMouseDown(System.Windows.Forms.MouseEventArgs)"]/*'/>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			mousedown = true;
			caret.Enabled = false;
			caretvisible = true;
      autocomplete = acform.Visible = false;
     
			if (e.Button == MouseButtons.Left)
			{
				MousePositionTranslate(e.X, e.Y, true);

				if (e.X < infobarw - buffer.FontWidth/2 + (showfoldbar ? 12 : 0))
				{
          if (e.X >= infobarw - buffer.FontWidth/2)
          {
            TogglePairAt(buffer.CurrentLine);
            preprocess = false;
            viewlines = null;
          }
          else
          {
            buffer.LineCharacterIndex = 0;
            buffer.IsSelecting = true;
            buffer.LineCharacterIndex = -1;
          }
				}
				else
				{
					buffer.IsSelecting = true;
				}
			}
			base.OnMouseDown (e);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnMouseUp(System.Windows.Forms.MouseEventArgs)"]/*'/>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.X < infobarw - buffer.FontWidth/2 + (showfoldbar ? 12 : 0))
				{
					buffer.IsSelecting = false;
				}
				else
				{
					if (dblclick) 
					{
						dblclick = false;
					}
					buffer.IsSelecting = false;
				}
			}
			
      mousedown = false;
			caretvisible = true;
			caret.Enabled = true;

			base.OnMouseUp (e);
		}

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnMouseMove(System.Windows.Forms.MouseEventArgs)"]/*'/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!mousedown)
			{
        if (!IsSplitted)
        {
          Cursor = HitTest(e.X, e.Y) && (
            (lastgp == null || !lastgp.IsVisible(
            new PointF(e.X - infobarw - (showfoldbar ? 14 : 0) +
            buffer.FontWidth * hscroll.Value, e.Y + vscroll.Value * buffer.FontHeight))))
            ? Cursors.IBeam
            : Cursors.Arrow;
        }
			}

			if (e.Button == MouseButtons.Left && mousedown)
			{
				MousePositionTranslate(e.X, e.Y, true);
			}

			base.OnMouseMove (e);
		}

		bool HitTest(int x, int y)
		{
			return !((x >= 0 && x < infobarw - buffer.FontWidth/2	+ (showfoldbar ? 14 : 0))&&
				(y < Height - hscroll.Height && y >= 0));
		}

		#endregion

		#region Keyboard Handling

		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnKeyUp(System.Windows.Forms.KeyEventArgs)"]/*'/>
		protected override void OnKeyUp(KeyEventArgs e)
		{
      switch (e.KeyCode)
      {
        case Keys.ShiftKey:
          buffer.IsSelecting = false;
          break;
      }
			base.OnKeyUp (e);
		}

		int[] GetCurrentLineIndices()
		{
			IToken[] tokens = buffer.GetTokens(buffer.CurrentLine);

			int[] indices = new int[(tokens.Length + 1) * 2];

			for (int i = 0; i < tokens.Length; i++)
			{
				indices[1 + i*2] = tokens[i].Location.Column;
				indices[2 + i*2] = tokens[i].Location.Column + tokens[i].Length;
			}

			indices[indices.Length - 1] = buffer[buffer.CurrentLine].Length + 1;
			return indices;
		}

    int CurrentLine 
    {
      get { return buffer.CurrentLine; }
      set 
      {
        int cl = buffer.CurrentLine;
        if (value != cl)
        {
          int i = Array.BinarySearch(viewlines, cl);
          int j = i + value - cl;
          if (j >= viewlines.Length)
          {
            buffer.CurrentLine = viewlines[viewlines.Length - 1];
          }
          else if (j < 0)
          {
            buffer.CurrentLine = viewlines[0];
          }
          else
          {
            buffer.CurrentLine = viewlines[j];
          }
        }
      }
    }

    #region Utility methods for buffer

    void INavigate.NavigateHome()
    {
      GotoLineStart();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoLineStart()
    {
      ResetCaret();
      string line = buffer[buffer.CurrentLine];
      int i = 0;
      while (i < line.Length && Char.IsWhiteSpace(line[i]))
      {
        i++;
      }
      int oi = buffer.LineCharacterIndex;
      if (oi == i)
      {
        buffer.LineCharacterIndex = 0;
      }
      else
      {
        buffer.LineCharacterIndex = i;
      }
      ScrollToCaret();
    }

    void INavigate.NavigateEnd()
    {
      GotoLineEnd();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoLineEnd()
    {
      ResetCaret();
      Buffer.LineCharacterIndex = -1;
      ScrollToCaret();
    }


    /// <summary>
    /// 
    /// </summary>
    public void GotoFirstLine()
    {
      ResetCaret();
      Buffer.CaretIndex = 0;
      ScrollToCaret();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoLastLine()
    {
      ResetCaret();
      Buffer.CaretIndex = Buffer.TextLength;
      ScrollToCaret();
    }

    void INavigate.NavigatePageUp()
    {
      if (!acform.Visible)
      {
        GoUpOnePage();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoUpOnePage()
    {
      ResetCaret();
      if (vscroll.Value >= 0)
      {
        int lc = vscroll.LargeChange;
        if (vscroll.Value < lc)
        {
          vscroll.Value = 0;
        }
        else
        {
          vscroll.Value -= lc;
        }
        if (buffer.CurrentLine > lc) 
        {
          CurrentLine -= lc;
        }
        else
        {
          CurrentLine = 0;
        }
      }
      ScrollToCaret();
    }

    void INavigate.NavigatePageDown()
    {
      if (!acform.Visible)
      {
        GoDownOnePage();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoDownOnePage()
    {
      ResetCaret();
      if (vscroll.Value <= vscroll.Maximum)
      {
        int lc = vscroll.LargeChange;
        if (vscroll.Maximum - vscroll.Value - lc <= lc + 1)
        {
          vscroll.Value = vscroll.Maximum - lc + 1;
        }
        else
        {
          vscroll.Value += lc;
        }

        if (buffer.CurrentLine + lc < buffer.LineCount) 
        {
          CurrentLine += lc;
        }
        else
        {
          CurrentLine = buffer.LineCount - 1;
        }
      }
      ScrollToCaret();
    }

    void INavigate.NavigateUp()
    {
      if (!acform.Visible)
      {
        GoUpOneLine();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoUpOneLine()
    {
      ResetCaret();
      Buffer.CurrentLine--;
      ScrollToCaret();
    }

    void INavigate.NavigateDown()
    {
      if (!acform.Visible)
      {
        GoDownOneLine();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void GoDownOneLine()
    {
      ResetCaret();
      Buffer.CurrentLine++;
      ScrollToCaret();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollToFirstLine()
    {
      vscroll.Value = 0;
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollToLastLine()
    {
      int lc = vscroll.LargeChange;
      if (vscroll.Maximum - vscroll.Value - lc <= lc + 1)
      {
        vscroll.Value = vscroll.Maximum - lc + 1;
      }
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollPageUp()
    {
      if (vscroll.Value >= 0)
      {
        int lc = vscroll.LargeChange;
        if (vscroll.Value < lc)
        {
          vscroll.Value = 0;
        }
        else
        {
          vscroll.Value -= lc;
        }
      }
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollPageDown()
    {
      if (vscroll.Value <= vscroll.Maximum)
      {
        int lc = vscroll.LargeChange;
        if (vscroll.Maximum - vscroll.Value - lc <= lc + 1)
        {
          vscroll.Value = vscroll.Maximum - lc + 1;
        }
        else
        {
          vscroll.Value += lc;
        }
      }
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollUp()
    {
      if(vscroll.Value > 0)
      {
        vscroll.Value--;
      }
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ScrollDown()
    {
      if (vscroll.Value < vscroll.Maximum - vscroll.LargeChange)
      {
        vscroll.Value++;
      }
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoNextToken()
    {
      ResetCaret();
      int ci = buffer.LineCharacterIndex;
      int[] indices = GetCurrentLineIndices();

      for (int i = 0; i < indices.Length; i++)
      {
        if (indices[i] > ci)
        {
          if (i == indices.Length - 1)
          {
            if (buffer.CurrentLine < buffer.LineCount - 1)
            {
              CurrentLine++;
              buffer.LineCharacterIndex = 0;
            }
          }
          else
          {
            buffer.LineCharacterIndex = indices[i];
          }
          break;
        }
      }
      ScrollToCaret();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoPreviousToken()
    {
      ResetCaret();
      int ci = buffer.LineCharacterIndex;
      int[] indices = GetCurrentLineIndices();
      for (int i = 0; i < indices.Length; i++)
      {
        if (indices[i] >= ci)
        {
          if (i == 0)
          {
            if (buffer.CurrentLine > 0)
            {
              CurrentLine--;
              buffer.LineCharacterIndex = -1;
            }
          }
          else
          {
            buffer.LineCharacterIndex = indices[i - 1];
          }
          break;
        }
      }
      ScrollToCaret();
    }

    void INavigate.NavigateLeft()
    {
      GotoOneLess();
    }

    /// <summary>
    /// 
    /// </summary>
    public void GotoOneLess()
    {
      ResetCaret();
      int cl = CurrentLine;
      buffer.CaretIndex--;
      if (CurrentLine < cl)
      {
        CurrentLine = cl;
        CurrentLine--;
        buffer.LineCharacterIndex = -1;
      }
      ScrollToCaret();
    }

    void INavigate.NavigateRight()
    {
      GotoOneMore();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void GotoOneMore()
    {
      ResetCaret();
      int cl = CurrentLine;
      buffer.CaretIndex++;
      if (CurrentLine > cl)
      {
        CurrentLine = cl;
        CurrentLine++;
      }
      ScrollToCaret();
      
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveBefore()
    {
      ResetCaret();
      if (buffer.SelectionLength == 0)
      {
        buffer.RemoveBeforeCaret();
      }
      else
      {
        buffer.RemoveSelection();
      }
      ScrollToCaret();
      UpdateAutoComplete();
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveAfter()
    {
      ResetCaret();
      if (buffer.SelectionLength == 0)
      {
        buffer.RemoveAfterCaret();
      }
      else
      {
        buffer.RemoveSelection();
      }
      ScrollToCaret();
      UpdateAutoComplete();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SelectLine()
    {
      ResetCaret();
      buffer.LineCharacterIndex = 0;
      buffer.IsSelecting = true;
      buffer.LineCharacterIndex = -1;
      buffer.IsSelecting = false;
      ScrollToCaret();
    }

    /// <summary>
    /// 
    /// </summary>
    public void InsertLine()
    {
      ResetCaret();
      buffer.InsertLineAfterCaret();
      ScrollToCaret();
    }

    void ResetCaret()
    {
      caret.Enabled = false;
      caret.Enabled = true;
      drawflags |= DrawFlags.Caret;
      caretvisible = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowAutoComplete()
    {
      if (buffer.parsetimer.Enabled)
      {
        buffer.parsetimer.Enabled = false;
        buffer.BackgroundParser();
      }
      autocomplete = true;
      UpdateAutoComplete();
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideAutoComplete()
    {
      if (autocomplete)
      {
        acform.Hide();
        autocomplete = false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoCompleteNextChoice()
    {
      if (autocomplete)
      {
        if (acform.choices.SelectedIndex < acform.choices.Items.Count - 1)
          acform.choices.SelectedIndex++;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoCompletePreviousChoice()
    {
      if (autocomplete)
      {
        if (acform.choices.SelectedIndex > 0)
          acform.choices.SelectedIndex--;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoCompleteSelectChoice()
    {
      if (autocomplete)
      {
        acform.DialogResult = DialogResult.OK;
        acform.Hide();
        ScrollToCaret();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoCompleteNextPage()
    {
      if (autocomplete)
      {
        ListBox lb = acform.choices;
        int c = lb.Height/lb.ItemHeight;
        if (lb.Items.Count - c  > lb.SelectedIndex)
        {
          lb.SelectedIndex += c;
        }
        else
        {
          lb.SelectedIndex = lb.Items.Count - 1;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SelectionToLower()
    {
      buffer.InsertString(SelectionText.ToLower());
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SelectionToUpper()
    {
      buffer.InsertString(SelectionText.ToUpper());
      Invalidate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoCompletePreviousPage()
    {
      if (autocomplete)
      {
        ListBox lb = acform.choices;
        int c = lb.Height/lb.ItemHeight;
        if (lb.SelectedIndex - c  >= 0)
        {
          lb.SelectedIndex -= c;
        }
        else
        {
          lb.SelectedIndex = 0;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CommentSelection()
    {
      string[] lines = SelectionText.Split('\n');
      string[] newlines = buffer.Language.CommentLines(lines);
      if (newlines != lines)
      {
        buffer.InsertString(string.Join(Environment.NewLine, newlines));
        Invalidate();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UnCommentSelection()
    {
      string[] lines = SelectionText.Split('\n');
      string[] newlines = buffer.Language.UnCommentLines(lines);
      if (newlines != lines)
      {
        buffer.InsertString(string.Join(Environment.NewLine, newlines));
        Invalidate();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void IncreaseIndent()
    {
      if (buffer.SelectionLength > 0)
      {
        int line, ci, selstart = buffer.SelectionStart, sellen = buffer.SelectionLength;
        buffer.GetInfoFromCaretIndex(selstart, out line, out ci);

        ArrayList newlines = new ArrayList();
            
        int firstline = line, startline = line, caretindex = ci;
        //check where the caret is
        buffer.GetInfoFromCaretIndex(selstart + sellen,	out line, out ci);
        if (ci == 0)
        {
          sellen += line - firstline;
          while(firstline < line)
          {
            newlines.Add("\t" + buffer[firstline]);
            firstline++;
          }
          buffer.SetLines(startline, newlines.ToArray(typeof(string)) as string[]);
          buffer.Select(selstart + sellen, -sellen);
        }
        else
        {
          sellen += line - firstline;
          while(firstline <= line)
          {
            newlines.Add("\t" + buffer[firstline]);
            firstline++;
          }
          sellen++;

          buffer.SetLines(startline, newlines.ToArray(typeof(string)) as string[]);
          buffer.Select(selstart, sellen);
        }
      }
      else
      {
        buffer.IsSelecting = false;
        buffer.InsertCharacter('\t');
      }
      Invalidate();

    }

    /// <summary>
    /// 
    /// </summary>
    public void DecreaseIndent()
    {
      buffer.IsSelecting = true;
      if (buffer.SelectionLength > 0)
      {
        int line, ci, selstart = buffer.SelectionStart, sellen = buffer.SelectionLength;
        buffer.GetInfoFromCaretIndex(selstart, out line, out ci);

        ArrayList newlines = new ArrayList();
            
        int firstline = line, startline = line, caretindex = ci;
        //check where the caret is
        buffer.GetInfoFromCaretIndex(selstart + sellen,	out line, out ci);
        if (ci == 0)
        {
          sellen -= line - firstline;
										
          while(firstline < line)
          {
            string l = buffer[firstline];
            if (l.StartsWith("\t"))
            {
              l = l.Substring(1);
            }
            else if (l.StartsWith(String.Empty.PadLeft(buffer.TabSize)))
            {
              l = l.Substring(buffer.TabSize);
              sellen -= buffer.TabSize - 1;
            }
            else if (l.StartsWith(" "))
            {
              l = l.Substring(1);
            }
            else
            {
              sellen++;
            }
            newlines.Add(l);
            firstline++;
          }
          buffer.SetLines(startline, newlines.ToArray(typeof(string)) as string[]);
          buffer.Select(selstart + sellen, -sellen);
          buffer.IsSelecting = true;
        }
        else
        {
          for (int i = firstline; i <= line; i++)
          {
            string l = buffer[i];
            if (l.StartsWith("\t"))
            {
              l = l.Substring(1);
              sellen--;
            }
            else if (l.StartsWith(String.Empty.PadLeft(buffer.TabSize)))
            {
              l = l.Substring(buffer.TabSize);
              sellen -= buffer.TabSize;
            }
            else if (l.StartsWith(" "))
            {
              l = l.Substring(1);
              sellen--;
            }
            else if (i == firstline && caretindex != 0)
            {
              selstart++;
              sellen--;
            }
            newlines.Add(l);
          }
          if (caretindex != 0)
          {
            selstart--;
            sellen++;
          }
								
          buffer.SetLines(startline, newlines.ToArray(typeof(string)) as string[]);
          buffer.Select(selstart, sellen);
          buffer.IsSelecting = true;
        }
      }
      else
      {
        buffer.IsSelecting = false;
        buffer.InsertCharacter('\t');

      }
      Invalidate();
    }

    #endregion


		///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
		///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnKeyDown(System.Windows.Forms.KeyEventArgs)"]/*'/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
      ResetCaret();

			bool shift = buffer.IsSelecting = e.Shift;
			bool ctrl = e.Control;
			bool alt  = e.Alt;

			switch (e.KeyCode)
			{
				case Keys.ShiftKey: case Keys.ControlKey:
					return;
				default:
					buffer.IsSelecting = false;
          MoveCaretIntoView();
					break;
			}

      Invalidate();

      base.OnKeyDown (e);
		}

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\System.Windows.Forms.xml' 
    ///	path='doc/members/member[@name="M:System.Windows.Forms.Control.OnKeyPress(System.Windows.Forms.KeyPressEventArgs)"]/*'/>
    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      if (!e.Handled && IsInputChar(e.KeyChar))
      {
        buffer.InsertCharacter(e.KeyChar);

#if AUTOCOMPLETE
        UpdateAutoComplete();
#endif
      }
      base.OnKeyPress (e);
    }


		#endregion

		#region ScrollBar/Caret Handling

		void DoLayout(Graphics g, int startline, int endline, int vlinecount)
		{
      buffer.DoBraceLayout(startline, endline);

			int vh = (int)( (Height - hscroll.Height)/buffer.FontHeight);
			int th = vlinecount;

 			int lc = vh + vscroll.SmallChange;
			if (lc >= 0)
			{
				vscroll.LargeChange = lc;
			}
			if (th >= 0)
			{
				vscroll.Maximum = th;
			}

      if(!(vscroll.Enabled = vscroll.LargeChange <= vscroll.Maximum)) 
      {
        vscroll.Value = 0;
      }

			if (g != null)
			{
				double tw = System.Math.Ceiling(buffer.LongestStringWidth/buffer.FontWidth);
				double ww = System.Math.Floor((Width - vscroll.Width - infobarw - (showfoldbar ? 15 : 0))/buffer.FontWidth);
      
				lc = (int)ww + hscroll.SmallChange;
				if (lc >= 0)
				{
					hscroll.LargeChange = lc;
				}
				if ((int)tw >= 0)
				{
					hscroll.Maximum = (int)tw;
				}

        if (!(hscroll.Enabled = hscroll.LargeChange <= hscroll.Maximum))
        {
          hscroll.Value = 0;
        }
			}

      Control p = Tag as Control;

      if (p != null)
      {
        string pt = p.Text;
        if (buffer.IsDirty && !pt.EndsWith("*"))
        {
          p.Text += "*";
        }
        if (!buffer.IsDirty && pt.EndsWith("*"))
        {
          p.Text = pt.TrimEnd('*');
        }
      }
		}

    int vline = 0;
    int vlinecount = 0;

    private void MoveCaretIntoViewUpper()
    {
      if (viewlines == null)
      {
        DoViewLineInit();
      }

      int cl = CurrentLine;


      if (cl != IsHidden(cl))
      {
        int s = FindHiddenStart(cl);
        TogglePairAt(s);
        preprocess = false;
        DoViewLineInit();
      }

      int i = Array.BinarySearch(viewlines, cl);
      if (i >= 0)
      {
        cl = i;
        // now bring the caret into view in a nice way
        vscroll.Maximum = vlinecount;
        // top
        if (cl - vscroll.Value <= 0 && vscroll.Value > 0)
        {
          int v = cl - 3;
          if (v < 0)
          {
            v = 0;
          }
          if (v >= vscroll.Minimum)
          {
            vscroll.Value = v;
          }
        }
        // bottom
        if (cl - vscroll.Value + 1 > (Height - hscroll.Height) / buffer.FontHeight)
        {
          int v = cl - 3;
          if (v < 0)
          {
            v = 0;
          }
          if (v >= vscroll.Minimum)
          {
            vscroll.Value = v;
          }
        }
        // left
        int lci = buffer.LineColumnIndex;
        if (lci - hscroll.Value - 2 < 0 && hscroll.Value > 0)
        {
          hscroll.Value = lci;
        }
        // right
        float ww = (Width - vscroll.Width - infobarw) / buffer.FontWidth;
        if (lci - hscroll.Value + 1 >= ww)
        {
          int v = (int)((lci - ww + 2));
          if (v > hscroll.Maximum)
          {
            v = hscroll.Maximum;
          }
          hscroll.Value = v;
        }
      }
    }

    internal void MoveCaretIntoView()
    {
      MoveIntoView(buffer.CurrentLine);
    }

    internal void MoveIntoView(int cl)
    {
      // how the scrollbar works, basically
      //T|-----|---LargeChange---|---|B
      //T|----------Maximum----------|B
      //T|Value|---------------------|B
      // translated
      // LargeChange  = VisibleLines
      // Maximum      = number of line
      // Value        = first line
      if (viewlines == null)
      {
        DoViewLineInit();
      }


      if (cl != IsHidden(cl))
      {
        int s = FindHiddenStart(cl);
        TogglePairAt(s);
        preprocess = false;
        DoViewLineInit();
      }

      int i = Array.BinarySearch(viewlines,cl);
      if (i >= 0)
      {
        cl = i;
        // now bring the caret into view in a nice way
        vscroll.Maximum = vlinecount;
        // top
        if (cl - vscroll.Value <= 0 && vscroll.Value > 0)
        {
          int v = cl - 1;
          if (v < 0)
          {
            v = 0;
          }
          if (v >= vscroll.Minimum)
          {
            vscroll.Value = v;
          }
        }
        // bottom
        if (cl - vscroll.Value + 1 > (Height - hscroll.Height)/buffer.FontHeight )
        {
          if (cl < vlinecount)
          {
            int v = cl - vscroll.LargeChange + 2;
            vscroll.Value = v;
          }
        }
        // left
        int lci = buffer.LineColumnIndex;
        if (lci - hscroll.Value - 2 < 0 && hscroll.Value > 0) 
        {
          hscroll.Value = lci;
        }
        // right
        float ww = (Width - vscroll.Width - infobarw)/buffer.FontWidth;
        if (lci - hscroll.Value + 1 >= ww)
        {
          int v = (int)((lci - ww + 2));
          if (v > hscroll.Maximum)
          {
            v = hscroll.Maximum;
          }
          hscroll.Value = v;
        }
      }
    }

		void UpdateCaret(object sender, EventArgs e)
		{
      if (!Disposing && Created && Visible && !ReadOnly)
      {
        if (InvokeRequired)
        {
          if (Created)
          {
            BeginInvoke(new EventHandler(UpdateCaret), new object[] { sender, e} );
          }
          return;
        }
        caretvisible = !caretvisible;
        drawflags = DrawFlags.Caret;
        Invalidate();
      }
		}

		void vscroll_ValueChanged(object sender, System.EventArgs e)
		{
			drawflags |= DrawFlags.InfoBar | DrawFlags.Text;
			//update linehint
			buffer.UpdateLineHint(vscroll.Value);
			Invalidate();
		}

		void hscroll_ValueChanged(object sender, System.EventArgs e)
		{
			drawflags |= DrawFlags.Text;
			Invalidate();
		}

		#endregion

		#region Auto Complete

    void acform_VisibleChanged(object sender, EventArgs e)
    {
      autocomplete = acform.Visible;
      System.Diagnostics.Trace.WriteLine(autocomplete ,"Visible       ");

      if (autocomplete)
      {
        ServiceHost.State |= ApplicationState.AutoComplete;
        ServiceHost.State &= ~ApplicationState.Buffer;
        ServiceHost.State &= ~ApplicationState.Navigate;
      }
      else
      {
        ServiceHost.State &= ~ApplicationState.AutoComplete;
        ServiceHost.State |= ApplicationState.Buffer;
        ServiceHost.State |= ApplicationState.Navigate;
      }

      if (!autocomplete)
      {
        acform.filters = null;
        acform.lastguess = string.Empty;

        switch (acform.DialogResult)
        {
          case DialogResult.OK:
          {
            object lbo = acform.choices.SelectedItem;
            string nt = lbo.ToString();
            if (nt != null)
            {
              CodeModel.ICodeElement elem = lbo as CodeModel.ICodeElement;
              string h = elem.Tag as string;
              System.Diagnostics.Trace.WriteLine("'" + elem.Fullname + '"' ,"Element       ");
              System.Diagnostics.Trace.WriteLine("'" + h + "'" ,"Selected hint ");
              int ci = buffer.CaretIndex;
              int lci = buffer.LineCharacterIndex;
              string line = buffer[buffer.CurrentLine];
              string ee = elem.Fullname ;
              string lh = acform.lasthint;

              if (acform.lasthint.Length == 0)
              {
                line = line.Substring(0, lci) + ee + line.Substring(lci);
              }
              else
              {
                int i = line.LastIndexOf(lh, lci);
                int j = i + lh.Length;
                string tail = (j < line.Length ? line.Substring(j) : string.Empty);
                int ti = tail.Length;
 
                string remt = h.Remove(h.Length - lh.Length, lh.Length);

                ee = ee.Remove(0, remt.Length);

                while (ti > 0 && !ee.EndsWith(tail.Substring(0, ti)))
                {
                  ti--;
                }

                line = line.Substring(0, i) + ee + tail.Substring(ti);
              }
              buffer.SetLine(buffer.CurrentLine, line);
              buffer.CaretIndex = ci + (ee.Length - acform.lasthint.Length);
            }
          }
          break;

          case DialogResult.Cancel:
          {
            buffer.parsetimer.Enabled = true;
          }
          break;

        }

      }
    }

		void UpdateAutoComplete()
		{
			if (autocomplete)
			{
      	int cl = buffer.CurrentLine;
				string line = buffer[cl];
				int lci = buffer.LineCharacterIndex;

				IToken[] tokens = buffer.GetTokens(cl);
        
				if (tokens == null)
				{
					Debug.Fail("Invalid state");
				}

        string hint;
        Type[] filters;

        CodeModel.ICodeElement[] result = buffer.Language.AutoComplete(tokens, line, lci, ProjectHint, buffer.FileName, out hint, out filters);

        if (acform.Parent == null)
        {
          FindForm().AddOwnedForm(acform);          
        }

				bool autocomplete2 = acform.Show( PointToScreen(
					new Point((int)(buffer.LineColumnIndex * buffer.FontWidth + infobarw + (showfoldbar ? 12 : 0) - hscroll.Value - 21), 
					(int)( buffer.FontHeight * (cl - vscroll.Value + 1)))), hint, result, buffer.FontHeight, filters);

        if (!autocomplete2)
        {
          acform_VisibleChanged(null, EventArgs.Empty);
        }

				Focus();
			}
		}

		#endregion

		#region Split View
#if BROKEN
#pragma warning disable 414
    bool splitting = false; //why does this generate a warning?????
#pragma warning restore 414
    bool splitted = false;
		AdvancedTextBox splitview;
    internal bool normalmode = true;

		void coolbutt_MouseDown(object sender, MouseEventArgs e)
		{

		}

		void s_DoubleClick(object sender, EventArgs e)
		{
			Join();
		}

		void coolbutt_MouseUp(object sender, MouseEventArgs e)
		{
      if (normalmode)
      {
        splitting = true;
        if (splitted)
        {
          Join();
        }
        else
        {
          Split();
        }
        splitting = false;
      }
      else
      {
        ((Parent.Parent as Grid) as IWindowsFormsEditorService).CloseDropDown();
      }
		}

		/// <summary>
		/// Split a the textbox's view in 2. So u get 2 controls with a singular buffer.
		/// </summary>
		public void Split()
		{
			// to make things extra complicated, plus gaining mucho WOW! and OOOH! 
			// 1. Place this control in a panel
			// 2. Replace this with the panel in parent
			// 3. Add this (bottom dock) + splitter (bottom dock) + clone (fill dock) in panel 
			// things to remember: dockstyle+anchors+size+location of this -> panel

			if (splitted) 
			{
				return;
			}

			int height = Height/2;

			Splitter s	= new Splitter();
			s.Width			= 4;
			s.BackColor = Color.DimGray;
			s.Dock			= DockStyle.Bottom;
			
			Panel p			= new Panel();
			
			p.Size			= Size;
			p.Location	= Location;
			p.Anchor		= Anchor;
			p.Dock			= Dock;
      

			Control parent = Parent;
			int i = parent.Controls.IndexOf(this);

			AdvancedTextBox clone = new AdvancedTextBox();

			splitview = clone;
			clone.splitview = this;
      clone.proj = proj;
      clone.LoadPairings(pairings);
      clone.showfoldbar = showfoldbar;

			s.DoubleClick +=new EventHandler(clone.s_DoubleClick);

			clone.Dock	= DockStyle.Fill;
      clone.buffer.Dispose();
			clone.buffer = buffer;
			clone.splitted = true;
			clone.linenumbers = linenumbers; // why doesnt this get cloned?, because im not cloning.... duh!
			clone.MoveCaretIntoView();
			
			Dock = DockStyle.Bottom;

			p.Controls.Add(clone);
			p.Controls.Add(s);
			p.Controls.Add(this);

			parent.Controls.Remove(this);
			parent.Controls.Add(p);
			parent.Controls.SetChildIndex(p, i);

			clone.Height = Height = height;
			clone.splitbut.Cursor = Cursors.NoMoveVert;
			splitbut.Cursor = Cursors.NoMoveVert;

			s.SplitPosition = (int)(p.Height/2);

			splitted = true;
		}

		/// <summary>
		/// Join a previously splitted textbox.
		/// </summary>
		/// <remarks>
		/// This function works a bit <see href="http://www.aisto.com/roeder/dotnet/">lobsterish.</see>
		/// The other view will become the main view and this instance gets joined (disposed) into that one.
		/// </remarks>
		public void Join()
		{
			// Just read this backwards: 
			// to make things extra complicated, plus gaining mucho WOW! and OOOH! 
			// 1. Place this control in a panel
			// 2. Replace this with the panel in parent
			// 3. Add this (bottom dock) + splitter (bottom dock) + clone (fill dock) in panel 
			// things to remember: dockstyle+anchors+size+location of this -> panel

			if (!splitted) return;
			
			Control parent		= Parent;
			Control newparent = parent.Parent;
			
			AdvancedTextBox newmain = null;
			int i = 0;

			foreach (Control c in parent.Controls)
			{
				if (c is AdvancedTextBox && c != this)
				{
					newmain = c as AdvancedTextBox;
					break;
				}
				i++;
			}

			parent.Controls.RemoveAt(i);
			i = newparent.Controls.IndexOf(parent);
			
			newmain.Dock			= parent.Dock;
			newmain.Size			= parent.Size;
			newmain.Location	= parent.Location;
			newmain.Anchor		= parent.Anchor;
			newmain.splitted	= false;
			newmain.splitview = null;
			newmain.splitbut.Cursor = Cursors.HSplit;

			newparent.Controls.RemoveAt(i);
			newparent.Controls.Add( newmain);
			newparent.Controls.SetChildIndex(newmain, i);

			parent.Dispose();
			this.Dispose();

			splitted = false;
		}

		/// <summary>
		/// Checks whether the textbox is in split view mode.
		/// </summary>
		[Browsable(false)]
		public bool IsSplitted
		{
			get {return splitted;}
		}
#endif
		#endregion

    #region TextBufferStream

#if TextBufferStream
		class TextBufferStream : Stream
		{
			int streampos = 0;
			TextBuffer textbuffer;

			public TextBufferStream(TextBuffer buffer)
			{
				textbuffer = buffer;
			}

			public override bool CanRead
			{
				get	{return true;}
			}

			public override bool CanSeek
			{
				get	{return true;}
			}

			public override bool CanWrite
			{
				get	{return false;}
			}

			public override long Position
			{
				get	{return streampos;}
				set {streampos = (int)value;}
			}

			public override void Flush()
			{

			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				switch (origin)
				{
					case SeekOrigin.Begin:
						if (offset < 0)
						{
							throw new IndexOutOfRangeException();
						}
						streampos = (int) offset;
						break;
					case SeekOrigin.Current:
						streampos += (int) offset;
						break;
					case SeekOrigin.End:
						if (offset > 0)
						{
							throw new IndexOutOfRangeException();
						}
						streampos = (int)(Length + offset);
						break;
				}
				return streampos;
			}

			public override long Length
			{
				get	{return textbuffer.TextLength;}
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				int s = streampos + offset;
				int e = s + count;
				if (buffer == null || buffer.Length < count)
				{
					throw new Exception("be a man");
				}
				int sline, scharindex, eline, echarindex;
				textbuffer.GetInfoFromCaretIndex(s, out sline, out scharindex);
				textbuffer.GetInfoFromCaretIndex(e, out eline, out echarindex);

				StringWriter writer = new StringWriter();
				writer.NewLine = "\n";
				
				if (sline < eline)
				{
					int line = sline;

					while (line <= eline)
					{
						if (line == sline)
						{
							writer.WriteLine(textbuffer[line].Substring(scharindex));
						}
						else
						if (line == eline)
						{
							writer.Write(textbuffer[line].Substring(0, echarindex));
						}
						else
						{
							writer.WriteLine(textbuffer[line]);
						}
						
						line++;
					}
				}
				else
				{
					writer.Write(textbuffer[sline].Substring(scharindex, echarindex - scharindex));
				}

				writer.Flush();
				string finalout = writer.ToString();

				count = Encoding.Default.GetBytes(finalout,0, finalout.Length, buffer, 0);
				streampos += count;
				return count;
			}
		}
#endif

		#endregion

    #region Project Related

    internal int debugline = -1;
    internal bool debugexcept = false;

    static Brush bpb = Factory.SolidBrush(Color.FromArgb(40, Color.Red));
    static Pen bpp = Factory.Pen(Color.Red, 1);

    static Brush ubbpb = Factory.SolidBrush(Color.FromArgb(40, Color.Orange));
    static Pen ubbpp = Factory.Pen(Color.Orange, 1);

    static Brush db = Factory.SolidBrush(Color.FromArgb(40, Color.LimeGreen));
    static Pen dp = Factory.Pen(Color.LimeGreen, 1);

    static Brush deb = Factory.SolidBrush(Color.FromArgb(40, Color.Yellow));
    static Pen dep = Factory.Pen(Color.Red, 2);

    internal Hashtable GetBreakpoints()
    {
      if (proj == null)
      {
        return null;
      }
      return proj.GetBreakpoints(buffer.FileName);
    }

    Build.Project proj;

    internal Build.Project ProjectHint
    {
      get {return proj;}
      set 
      {
        proj = value;
      }
    }

    internal NavigationBar navbar;
    CodeModel.ICodeFile codefile;

    internal CodeModel.ICodeFile CodeFile
    {
      get { return codefile; }
      set 
      { 
        codefile = value;
        if (navbar != null)
        {
          navbar.CodeFile = value;
        }
      }
    }



    #region IHasCodeModel Members

    public Xacc.CodeModel.ICodeFile CodeModel
    {
      get { return codefile; }
    }

    #endregion

    #endregion



    #region IFind Members

    Xacc.CodeModel.Location[] IFind.Find(string text, FindOptions lookin)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    void IFind.SelectLocation(Xacc.CodeModel.Location loc)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
}



