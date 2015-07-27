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
using System.Drawing;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using Xacc.Controls;
using TextBuffer = Xacc.Controls.AdvancedTextBox.TextBuffer;

namespace Xacc.ComponentModel
{
  /// <summary>
  /// Define color and font info for token types
  /// </summary>
	public struct ColorInfo
	{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:ColorInfo"/> class.
    /// </summary>
    /// <param name="forecolor">The forecolor.</param>
    /// <param name="backcolor">The backcolor.</param>
    /// <param name="bordercolor">The bordercolor.</param>
    /// <param name="style">The style.</param>
    public ColorInfo(Color forecolor, Color backcolor, Color bordercolor, FontStyle style)
    {
      this.BorderColor = bordercolor.Name == "0" ? Color.Empty : bordercolor;
      this.ForeColor = forecolor;
      this.BackColor = backcolor.Name == "0" ? Color.Empty : backcolor;
      this.Style = style;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ColorInfo"/> class.
    /// </summary>
    /// <param name="forecolor">The forecolor.</param>
    /// <param name="backcolor">The backcolor.</param>
    /// <param name="style">The style.</param>
    public ColorInfo(Color forecolor, Color backcolor, FontStyle style)
    {
      this.BorderColor = Color.Empty;
      this.ForeColor = forecolor;
      this.BackColor = backcolor;
      this.Style = style;
    }
    /// <summary>
    /// The style to use
    /// </summary>
		public FontStyle		Style;		

    /// <summary>
    /// The foreground color to use
    /// </summary>
		public Color				ForeColor;

    /// <summary>
    /// The background color to use
    /// </summary>
		public Color				BackColor;

    /// <summary>
    /// The border color to use
    /// </summary>
    public Color BorderColor;
	
    /// <summary>
    /// Represents an empty ColorInfo
    /// </summary>
		public static readonly ColorInfo Empty = new ColorInfo(Color.Empty, Color.Empty, 0);

    /// <summary>
    /// Represents an invalid ColorInfo
    /// </summary>
    public static readonly ColorInfo Invalid = new ColorInfo(Color.Empty, Color.Empty, 0);

		static ColorInfo()
		{
			Invalid.BackColor = Color.LemonChiffon;
			Invalid.ForeColor = Color.Red;
		}

#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}", ForeColor.Name);
		}
#endif
	}
}


