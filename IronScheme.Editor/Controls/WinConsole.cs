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
using System.Data;
using System.Windows.Forms;

namespace Xacc.Controls
{
	/// <summary>
	/// Provides a simple control to intercept the console in a Windows.Forms application.
	/// </summary>
	/// <remarks>
	/// All that is required is to create an instance of the class, and the console will be redirected.
	/// </remarks>
  class WinConsole : TextBox
  {
    class MessageWriter : TextWriter
    {
      public override System.Text.Encoding Encoding
      {
        get	{	return System.Text.Encoding.Default;}
      }

      public delegate void MessageWriterHandler(string text);

      MessageWriterHandler handler;
			string prefix = null;
			Control host;

      public MessageWriter(string prefix, MessageWriterHandler handler, Control host)
      {
#if CHECKED
				this.prefix = prefix;
#endif
				this.host = host;
        this.handler = handler;
      }

      public override void Write(char value)
      {
        Write(value.ToString());
      }

      public override void WriteLine(string format)
      {
        Write(format + NewLine);
      }

      public override void Write(string format)
      {
        if (handler != null)
        {
          if (prefix != null)
          {
            format = string.Format("{0,-10}{1}{2}", prefix, format, NewLine);
          }

          if (host.InvokeRequired)
          {
            host.Invoke(handler, new object[]{format});
          }
          else
          {
            handler(format);
          }
        }
      }
    }

    System.ComponentModel.Container components = null;

    public WinConsole()
    {
      InitializeComponent();
      SetStyle(ControlStyles.Selectable, false);
			UpdateStyles();

      Console.SetOut(new MessageWriter("OUT", new MessageWriter.MessageWriterHandler(Messg), this));
			Console.SetError(new MessageWriter("ERROR", new MessageWriter.MessageWriterHandler(Messg), this));
#if TRACE
			Console.Out.WriteLine("Hijacking Console.Out");
			Console.Error.WriteLine("Hijacking Console.Error");
#else
			Console.Out.Write(string.Empty);
			Console.Error.Write(string.Empty);
#endif
    }

    Control GetFocus(Control p)
    {
			if (p != null)
			{
				Control sender = null;
				foreach (Control c in  p.Controls)
				{
					if (c.Focused)
						return c;

					if (c.HasChildren)
					{
						sender = GetFocus(c);
						if (sender != null)
						{
							return sender;
						}
					}
				}
			}
      return null;
    }

    void Messg(string text)
    {
      try
      {
        Control sender = GetFocus(TopLevelControl);
        Select();

        if (text == "CLEAR")
        {
          Text = string.Empty;
        }
        else
        {
          AppendText(text);
        }
        SelectionStart = TextLength;
        ScrollToCaret();
        if (sender != null)
          sender.Select();
      }
      catch (ObjectDisposedException)
      {
        //docking...
      }
    }

    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if( components != null )
          components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Component Designer generated code

    void InitializeComponent()
    {
      BackColor = System.Drawing.SystemColors.Window;
      BorderStyle = System.Windows.Forms.BorderStyle.None;
      ReadOnly = true;
      WordWrap = true;
			Multiline = true;
			ScrollBars = ScrollBars.Vertical;
			Font = new Font(Font.FontFamily, 8.25f);
    }
    #endregion
  }
}
