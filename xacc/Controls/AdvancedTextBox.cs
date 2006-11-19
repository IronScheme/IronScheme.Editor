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
	public sealed class AdvancedTextBox : Control, IEdit, IFile, IEditSpecial, IEditAdvanced, INavigate, IScroll
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
		SolidBrush selbrush = Factory.SolidBrush(Color.FromArgb(50, SystemColors.Highlight));
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
        w.Write("<pre style='color:black;background-color:white;font-family:Consolas,Bitstream Vera Sans Mono,Lucida Console,Courier New;'>");
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
        ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
        ControlStyles.Opaque |
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
      if (charCode > 0 && (charCode < 28 || charCode == 127))
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
      SystemBrushesHighlight = SystemBrushes.Highlight;

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.BackColorChanged"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
    protected override void OnBackColorChanged(EventArgs e)
    {
      base.OnBackColorChanged (e);

      bgbrush = Factory.SolidBrush(BackColor);
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
        g.FillRectangle(selbrush, 0, (j - firstline) * fh, fw5, fh - 1);
        
        g.DrawLine(SystemPensHighlight, fw5, 0, fw5, h);

        int diff = (int)((fh-menufont.Height)/2f);

        while (lr <= ll && lr < vlinecount)
        {
          j = lines[lr];

          g.DrawString(
            (j
#if !CHECKED 
            // fix up for real use, 1 based indices are horrible!
            + 1
#endif
            ).ToString(), menufont, 
            j == buffer.CurrentLine ? SystemBrushesHotTrack:  
            j%10 == 9 ? SystemBrushesControlText :	SystemBrushesHighlight, 
            hfw, (lr - firstline)* fh + diff, buffer.sf);
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


      g.FillRectangle(bgbrush, gclip);

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
          g.SmoothingMode = SmoothingMode.HighQuality;
          g.FillPath(selbrush, gp);
          g.DrawPath(selpen, gp);
          g.SmoothingMode = sm;
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
            Drawing.Utils.PaintLineHighlight(deb, dep, g, fh * i - 1, bpw, fh, true);
          }
          else
          {
            Drawing.Utils.PaintLineHighlight(db, dp, g, fh * i - 1, bpw, fh, true);
          }
        }
        else if (bps != null)
        {
          Breakpoint bp = bps[j] as Breakpoint;
          if (bp != null)
          {
            if (bp.bound)
            {
              Drawing.Utils.PaintLineHighlight(bpb, bpp, g, fh * i - 1, bpw, fh, bp.enabled);
            }
            else
            {
              Drawing.Utils.PaintLineHighlight(ubbpb, ubbpp, g, fh * i - 1, bpw, fh, bp.enabled);
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
				Cursor = HitTest(e.X, e.Y) && (
					(lastgp == null || !lastgp.IsVisible( 
					new PointF(e.X - infobarw - (showfoldbar ? 14 : 0) + 
					buffer.FontWidth * hscroll.Value, e.Y + vscroll.Value * buffer.FontHeight))))
					? Cursors.IBeam 
					: Cursors.Arrow;
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
      GoUpOnePage();
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
      GoDownOnePage();
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
      GoUpOneLine();
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
      GoDownOneLine();
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
      }
		}

		void s_DoubleClick(object sender, EventArgs e)
		{
			Join();
		}

		void coolbutt_MouseUp(object sender, MouseEventArgs e)
		{
      if (normalmode)
      {
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

    #endregion

		#region TextBuffer
		/// <summary>
		/// Provides Text services.
		/// </summary>
    public sealed class TextBuffer : Disposable, Language.IParserCallback, IHasUndo
    {
      #region Data Structures

      [StructLayout(LayoutKind.Sequential, Pack=4)]
      struct LineState
      {
        string intline;

        public string     line
        {
          get {return intline;}
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

        public TokenLine  userstate;
        public DrawInfo[]	drawcache;
      }

      [StructLayout(LayoutKind.Auto, Pack=1)]
      internal class DrawInfo : IDrawInfo
      {
        public float				start;
        public float				end;
        public string				text;

        static readonly Hashtable infomap = new Hashtable();
        static readonly ArrayList infotab = new ArrayList();

        byte fc,bc,st,bd;

        public Color				forecolor
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
              infomap.Add(value,i);
              infotab.Add(value);
            }
            else
            {
              i = (int)infomap[value];
            }

            fc = (byte) i;
          }
        }

        public Color				backcolor
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
              infomap.Add(value,i);
              infotab.Add(value);
            }
            else
            {
              i = (int)infomap[value];
            }

            bc = (byte) i;
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

        public FontStyle		style
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
          get {return (int)Math.Round(start/fontwidth); }
        }

        public int End
        {
          get {return (int)Math.Round(end/fontwidth); }
        }

        public string Text
        {
          get {return text; }
        }

        public Color ForeColor
        {
          get {return forecolor; }
        }

        public Color BackColor
        {
          get {return backcolor; }
        }

        public Color BorderColor
        {
          get { return bordercolor; }
        }

        public FontStyle Style
        {
          get {return style; }
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
              r1.MoveNext() & r2.MoveNext();)
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
        public IToken[]		 Tokens
        {
          get {return tokens;}
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
          get {return GetHashCode();}
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
          get {return (TextBuffer)buffer ;}
        }

        protected BufferOperation(object before, object after, string value)
          : base (before, after)
        {
          this.value = value;
        }
      }

      sealed class InsertOperation : BufferOperation
      {
        public InsertOperation(object before, object after, string value)
          : base(before, after, value){}

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
          : base(before, after, value){}

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
        get {return tabtospace;}
        set {tabtospace = value;}
      }

      /// <summary>
      /// Gets or sets the tabs size.
      /// </summary>
      /// <value>
      /// The tab size in measured in number of spaces.
      /// </value>
      public int TabSize
      {
        get {return tabsize;}
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
        get {return tabs != null;}
      }

      internal bool IsMonospaced(Font font, out float width)
      {
        using (Bitmap b = new Bitmap(100,100))
        {
          using (Graphics g = Graphics.FromImage(b))
          {
            return IsMonospaced(g, font, out width);
          }
        }
      }

      internal void AdjustTabs()
      {
        using (Bitmap b = new Bitmap(100,100))
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
        DrawInfo.fontwidth = fontwidth = g.MeasureString("//////////", font, MAX24BIT, sf).Width/10;

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
        get {return caretindex;}
        set 
        {
          if (value >= 0)
          {
            if (value > TextLength)
              value = TextLength;
            int ci;
            caretindex = value;
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
        if (linehint.line/2 > line)
        {
          return STARTHINT;
        }
        if ((LineCount - linehint.line/2) < (LineCount - line))
        {
          return new LineHint(LineCount - 1, textlength - this[LineCount - 1].Length - nllen);
        }
        return linehint;
      }

      LineHint GetBestLineHintIndex(int index)
      {
        if (linehint.index/2 > index)
        {
          return STARTHINT;
        }
        if ((textlength - this[LineCount - 1].Length - linehint.index)/2 < index)
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
          tadj = tabsize - tok.Length%tabsize;
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
        get {return GetLineColumnIndex(caretindex);}
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
              else if (CaretIndex == TextLength)
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
        get {return fontdescent;}
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
            float ph = (float)System.Math.Round(value.SizeInPoints / 0.75, 0) * 0.75f;
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

            //ph *= adjustment; // this just rounds up font to have exactly integer width

            if (ph != font.SizeInPoints)
            {
              //font = new Font(font.FontFamily, ph);
            }

            // this will be the prefered value from the font, best to use it
            fontheight = font.Height;
            this.font = font;
            fontdescent = font.FontFamily.GetCellDescent(0) / (float)font.FontFamily.GetEmHeight(0)
              * fontheight;
            AdjustTabs();



          }
        }
      }


      /// <summary>
      /// The pixel height of the font.
      /// </summary>
      public int FontHeight
      {
        get {return fontheight;}
      }

      /// <summary>
      /// Gets the font width
      /// </summary>
      public float FontWidth
      {
        get {return fontwidth;}
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
            TokenLine tl = GetUserState(l - 1);
            TokenLine cl = GetUserState(l);
            Stack s = cl == null ? null : cl.state;
            IToken[] tokens = lang.Lex(this[l], ref tl, mlines);

            foreach (IToken t in tokens)
            {
              t.Location.LineNumber = l + 1;
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
        TokenLine before = null;
        Stack tl = state.userstate.state;
        if (prev != null)
        {
          before = prev.Data;
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
        }
        else
        {
          owner.viewlines = null;
        }

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

      #endregion

      #region Tracing

      class Trace
      {
        [Conditional("TRACE")]
        public static void WriteLine(string format, params object[] args)
        {
          Diagnostics.Trace.WriteLine("TextBuffer", format, args);
        }
      }

      class Debug
      {
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
          Diagnostics.Trace.WriteLine("TextBuffer", format, args);
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
        Drawing.Utils.PaintLineHighlight(gbb, gpp, g, indent, FontHeight * line - 1, 
          (int)((text.Length + 1) * FontWidth), FontHeight, true);
        g.DrawString(text, Font, gb, indent + 2 ,FontHeight * line, sf);
      }

       /// <summary>
      /// New style painter that implements JIT techniques.
      /// </summary>
      class TextBufferPainter
      {
        readonly TextBuffer buffer;

#if DEBUG
        static int stat_total = 0;
        static int stat_hit		= 0;
        static int stat_miss	= 0;

        static float HitRatio 
        {
          get { return stat_total == 0 ? 0f : (float)stat_hit/stat_total; }
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

          string l					= buffer[vline];
          Font font         = buffer.font;
          
          int fh						= 0, fh2= 0;
          float fd          = 0;
          try
          {
            fh = line * font.Height;
            fh2 = font.Height;
            fd = (fh2 - font.GetHeight()) / 2;
          }
          catch (Exception ex)
          {
            Debugger.Break();
            Console.WriteLine(ex);
            
          }
          DrawInfo[] dis		= null;
          TokenLine tl      = null;
          IToken[] tokens   = null;
#if DEBUG
          stat_total++;
#endif
          dis		   = buffer.GetDrawCache(vline);
          tl			 = buffer.GetUserState(vline);
          tokens   = tl.Tokens;

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
                  Brush bg = Factory.SolidBrush(di.backcolor);
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
                    (int)(offset + w),Color.Red);
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
                float start, end = MeasureString(linetext, c , c + tok.Length, out start);

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
          offset =  0;
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
              int x = tabsize - length%tabsize;
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
            int x = tabsize - length%tabsize;
            length += x;
            start += (prelen + 1);
          }
          length += endindex - start;

          return length - offset;
        }
        }

        public float MeasureString(string line, int startindex, int endindex, out float offset)
        {
          offset =  0f;
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
          Color invert = Color.FromArgb(r,g,b);
          return invert;
        }
      }

      static int IndexOfTab(string line, int start, int end)
      {
        while ( start < end)
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
        width = w/10f;
        return (System.Math.Abs(w-l) < 0.005f);
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
        offset =  0f;
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
        get {	return maxwidth;}
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
        ArrayList p  = new ArrayList();
        y--;
        int i = 1;

        for (bool up = false; i - 1 <= end - start; up = !up)
        {
          if (up)
          {
            p.Add( new Point(start + i, y));
          }
          else
          {
            p.Add( new Point(start + i, y - 2));
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
      GraphicsPath lastsel;

      //used with fancy selection, o god im glad this over....
      internal GraphicsPath GetPath(int selectionstart, int selectionlength, int firstline, int lastline)
      {
        if (selectionstart == lastselstart &&
          selectionlength == lastsellen &&
          firstline == lastfirst && lastline == lastlast && lastsel != null)
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

        int[] lines = owner.viewlines;

        const int radius = 6;

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

            RectangleF rr = prev = new RectangleF(offset - 1, startline * fontheight, pos + 1 + fontwidth, fontheight);
            
            l.AddArc(rr.X, rr.Y, radius, radius, 270, -90);
            r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);

          }

          while (++startline < curline)
          {
            if (startline >= firstline && startline <= lastline)
            {
              pos = MeasureString(lines[startline], 0, -1, out offset);

              RectangleF rr = new RectangleF(offset - 1, startline * fontheight, pos + 1 + fontwidth, fontheight);

              if (rr.Left < prev.Left)
              {
                l.AddArc(prev.Left - radius, prev.Bottom - radius, radius, radius, 0, 90);
                l.AddArc(rr.Left, rr.Y, radius, radius, 270, -90);
              }

              if (rr.Right > prev.Right)
              {
                r.AddArc(prev.Right, prev.Bottom - radius, radius, radius, 180, -90);
                r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);
              }
              else if (rr.Right < prev.Right)
              {
                r.AddArc(prev.Right - radius, prev.Bottom - radius, radius, radius, 0, 90);
                r.AddArc(rr.Right, rr.Y, radius, radius, 270, -90);
              }

              prev = rr;
            }
          }
          if (startline >= firstline && startline <= lastline)
          {
            pos = MeasureString(lines[startline], 0, curlineindex, out offset);

            RectangleF rr = new RectangleF(offset - 1, startline * fontheight, pos + 1, fontheight);

            if (rr.Left < prev.Left)
            {
              l.AddArc(prev.Left - radius, prev.Bottom - radius, radius, radius, 0, 90);
              l.AddArc(rr.Left, rr.Y, radius, radius, 270, -90);
            }

            if (rr.Right > prev.Right)
            {
              r.AddArc(prev.Right, prev.Bottom - radius, radius, radius, 180, -90);
              r.AddArc(rr.Right - radius, rr.Y, radius, radius, 270, 90);
            }
            else if (rr.Right < prev.Right)
            {
              r.AddArc(prev.Right - radius, prev.Bottom - radius, radius, radius, 0, 90);
              r.AddArc(rr.Right, rr.Y, radius, radius, 270, -90);
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
            RectangleF rr = new RectangleF(offset - 1, startline * fontheight, pos + 1, fontheight);
            
            // top left
            r.AddArc(rr.X, rr.Y, radius, radius, angle, 90);
            angle += 90;
            // top right
            r.AddArc(rr.Right - radius, rr.Y,radius, radius, angle, 90);
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
          int res = lang.Parse(mlines, filename, this);
          hp.Stop();

          Trace.WriteLine(string.Format("Parsing completed {0}successfully in {1:f1}ms", (res == 0 ? string.Empty : "un" ), hp.Duration));

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
            if (owner.ProjectHint != null)
            {
              owner.ProjectHint.CodeModel.Add(lang.CodeModel);
              owner.ProjectHint.GenerateProjectTree();
              ServiceHost.CodeModel.Run(owner.ProjectHint);
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

          for (int i = 0; i < tl.Tokens.Length; i++)
          {
            if (tl.Tokens[i].Location == loc)
            {
              loc.callback(tl.Tokens[i]);
              break;
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
        int line = location.LineNumber - 1;
        int linecount = location.LineCount;
        int col = location.Column;
        int endcol = location.EndColumn;
        int selstart = 0,sellen = 0;

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

          if (col >= 0)
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
        get {return select;}
        set {select = value;}
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
        get { return (selectionstart > caretindex) ? caretindex : selectionstart;}
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
						sb.Append( this[startline].Substring(startci) + "\n");

						while (endline > ++curline)
						{
							sb.Append(this[curline] + "\n");
						}
						sb.Append( this[endline].Substring(0, endci));
					}

					else
					{
						sb.Append( this[curline].Substring(startci, endci - startci));
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

          Debug.WriteLine("Start: {0} Length: {1} Text: '{2}'", selstart, sellen, SelectionText);
					
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
            string ss = this[startline].Substring(0,startlineindex);
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
            undo.Push( new RemoveOperation(before, after, value));
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

          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, ci);

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
            undo.Push( new RemoveOperation(before, after, value));
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

          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, ci);

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
            undo.Push( new RemoveOperation(before, after, value));
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

          Debug.WriteLine("Line: {0} CharIndex: {1} Value: '{2}'", cl, ci, text);

          string l = this[cl];

          if (recording && text.Length > 0)
          {
            before = ((IHasUndo)this).GetUndoState();
          }

          string front = l.Substring(0, ci);
          string back  = l.Substring(ci);
        
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
            undo.Push( new InsertOperation(before, after, value));
          }

          SendProbe();
        }
      }

			int GetIndent(string text)
			{
				if (text == null) return 0;
				const char SPACE	= ' '	;
				const char TAB		= '\t';
				int indent = 0, pos = 0, count = 0;
				while (pos < text.Length)
				{
					switch(text[pos++])
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
							return indent;
					}
				}
				return indent;
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

          Debug.WriteLine("Value: '{2}' Line: {0} CharIndex: {1}", cl, ci, c);

          if (recording)
          {
            before = ((IHasUndo)this).GetUndoState();
            value = c.ToString();
          }

          string l = this[cl];

          StringBuilder sb = new StringBuilder(l);
          sb.Insert(ci, c.ToString());

          this[cl] = sb.ToString();
          CaretIndex++;

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push( new InsertOperation(before, after, value));
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

          Debug.WriteLine("Line: {0} CharIndex: {1}", cl, lci);

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
            string rest =  new string('\t', indent) + l.Substring(lci);
            value += new string('\t', indent);

            Insert(cl + 1, rest);

            CaretIndex += indent + 1;
          }

          if (recording)
          {
            object after = ((IHasUndo)this).GetUndoState();
            undo.Push( new InsertOperation(before, after, value));
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

			bool tabtospace = false;

			bool recording = true;

      int caretindex;
      int textlength;
      int currentline;
      int tabsize = 2;
      int fontheight;
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

			Timers.HiPerfTimer	hp	= new Timers.HiPerfTimer();
			
			Language lang = Language.Default;
      internal Timers.FastTimer parsetimer = new Timers.FastTimer(2000);
			
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
          try
          {
            //Font = new Font( "Bitstream Vera Sans Mono", 9.75f);
            Font = new Font(ServiceHost.Font.InstalledFonts[0], 9.75f);
          }
          catch (Exception)
          {
            Font = new Font(FontFamily.GenericMonospace, 9.75f);
          }
        }
				// .NET 1.0 dont throw an exception when a font is not found.
				// Thanx to Nnamdi Onyeyiri for pointing it out, and helping to fix the bug.
				if (Font == null)
				{
					Font = new Font(FontFamily.GenericMonospace, 9.75f);
				}

				painter = new TextBufferPainter(this);
				caret = Factory.Pen(Color.Black, 2);
        parsetimer.Tick +=new EventHandler(parsetimer_Tick);

			}

			/// <summary>
			/// Gets or sets the Language associated with the buffer.
			/// </summary>
			[TypeConverter(typeof(ExpandableObjectConverter))]
			public Language Language
			{
				get {return lang;}
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
							sb.Append((char)r.Next('a','z'));
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
				GC.Collect();
			}

			#endregion

      #region Basic operations

      /// <summary>
      /// Clears the TextBuffer and reset itself.
      /// </summary>
      public void Clear()
      {
				textlength = 0;
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
				GC.Collect();
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
				get {return this[(int)index];}
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
          object after =((IHasUndo)this).GetUndoState();
          undo.Push( new ReplaceLinesOperation(before, after, prev, value));
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

        Debug.WriteLine("old: {0} new: {1}", l, value);

        LineCharacterIndex += (value.Length - l.Length); 

        if (recording)
        {
          object after = ((IHasUndo)this).GetUndoState();
          undo.Push( new ReplaceLineOperation(before, after, l, value));
        }
        
        ignoretrigger = false;
			}

			// internal hack, this seems to concur nicely with the Region limit 
      string this[int index]
      {
        get
        {
          
          if(index < 0 || index >= numLines)
          {
            return string.Empty;
          }
          else if(index < gapBottom)
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
          if(index < 0 || index >= numLines)
          {
            return;
          }

          Debug.Assert(value != null);

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
          if(index < gapBottom)
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

			/// <summary>
			/// Get the user state for a particular line. 
			/// </summary>
			/// <param name="index">the line index</param>
			/// <returns>the user object</returns>
      internal TokenLine GetUserState(int index)
      {
        if(index < 0 || index >= numLines)
        {
          return null;
        }
        else if(index < gapBottom)
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
        if(index < 0 || index >= numLines || userstate == null)
        {
          return;
        }
        else 
        {
          if(index < gapBottom)
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
				if(index < 0 || index >= numLines)
				{
					return null;
				}
				else if(index < gapBottom)
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
				if(index < 0 || index >= numLines)
				{
					return;
				}
				
				if(index < gapBottom)
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
        if(index < 0)
        {
          index = 0;
        }
        else if(index > numLines)
        {
          index = numLines;
        }
        if(count < 0)
        {
          count = 0;
        }

        // Determine if we need to enlarge the buffer.
        if((numLines + count) > lines.Length)
        {
          len = lines.Length;
          size = len * 2;
          while(size < (numLines + count))
          {
            size *= 2;
          }
          LineState[] newLines = new LineState[size];
          if(index < gapBottom)
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
        if(index < gapBottom)
        {
          // Shift lines from the bottom up to the top.
          num = gapBottom - index;
          Array.Copy(lines, index, lines, gapTop - num, num);
          gapBottom -= num;
          gapTop -= num;
        }
        else if(index > gapBottom)
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
          if(lines == null || lines.Length == 0 || length == 0)
          {
            return;
          }

          // Shift lines in the buffer to make room for the insertion.
          int num = length;
          SetOptimumInsertPosition(index, num);

          owner.viewlines = null;
          //owner.preprocess = true;

          //adjustment for LineState
          LineState[] ls = new LineState[num];

          TokenLine state = GetUserState(index - 1); //???? perhaps ia one index, been to long to remember

          SendProbe();

          bool res = false;
				
          for (int i = 0; i < num; i++)
          {
            textlength += lines[i + startindex].Length + nllen;
            ls[i].userstate =  state = InsertLineAfter(state);
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
          if((start + count) > numLines)
          {
            count = numLines - start;
          }
          if(start < 0 || start >= numLines || count <= 0)
          {
            return;
          }

          SendProbe();

          for (int i = 0; i < count;i++)
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
        if(reader == null)
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

          while((line = reader.ReadLine()) != null)
          {
            Add(line);
            if (input != null)
            {
              ServiceHost.StatusBar.Progress = input.Position/(float)input.Length;
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
        if(encoding == null)
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
         if(writer == null)
        {
          throw new ArgumentNullException("writer");
        }
        int count = LineCount;
        int line;
        for(line = 0; line < count; ++line)
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

				lastsavelevel = undo.CurrentLevel;
      }

			/// <summary>
			/// Save the contents of this buffer to a text file. 
			/// </summary>
			/// <param name="stream">the text destination</param>
			public void Save(Stream stream)
      {
				Save(stream , CurrentEncoding ?? Encoding.Default);
      }

			/// <summary>
			/// Save the contents of this buffer to a text file. 
			/// </summary>
			/// <param name="stream">the text destination</param>
			/// <param name="encoding">the encoding to use</param>
      public void Save(Stream stream, Encoding encoding)
      {
        if(encoding == null)
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
        if(encoding == null)
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



