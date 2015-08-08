#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


#region Includes
using System.Drawing;
#endregion

using IronScheme.Editor.CodeModel;
using System;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Fold styles for code folding
  /// </summary>
	public enum FoldStyle : short
	{
    /// <summary>
    /// None
    /// </summary>
		None						= 0x0,
    /// <summary>
    /// Plus
    /// </summary>
		Plus      			= 0x1,
    /// <summary>
    /// Minus
    /// </summary>
		Minus     			= 0x2,
    /// <summary>
    /// Exit
    /// </summary>
		Exit    				= 0x3,
	}

  /// <summary>
  /// Interface for tokens
  /// </summary>
	public interface IToken
	{
    /// <summary>
    /// Gets the token type
    /// </summary>
    int Type { get;set;}

    /// <summary>
    /// Gets the token class
    /// </summary>
    TokenClass Class { get;set;}

    /// <summary>
    /// Gets the token length
    /// </summary>
		int		    Length		{get;}

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    string Text { get;set;}

    /// <summary>
    /// Gets the token Location
    /// </summary>
    Location  Location  {get;set;}

#if DEBUG
    [Obsolete("Is this used?")]
    object Value { get;}
#endif


	}

  /// <summary>
  /// Define token types
  /// </summary>
  public enum TokenClass
  {
    EOF          = -8,
    /// <summary>
    /// Preprocessor
    /// </summary>
    Preprocessor = -7,
    /// <summary>
    /// Warning
    /// </summary>
    Warning   = -6,
    /// <summary>
    /// Ignore
    /// </summary>
    Ignore    = -5,
    /// <summary>
    /// Documentation comment
    /// </summary>
    DocComment= -4,
    /// <summary>
    /// Error
    /// </summary>
    Error     = -3,
    /// <summary>
    /// New line/EOF
    /// </summary>
    NewLine   = -2,
    /// <summary>
    /// Comment
    /// </summary>
    Comment   = -1,	
    /// <summary>
    /// Normal
    /// </summary>
    Any       =  0,
    /// <summary>
    /// Indentifier
    /// </summary>
    Identifier,
    /// <summary>
    /// Type
    /// </summary>
    Type,
    /// <summary>
    /// Keyword
    /// </summary>
    Keyword,
    /// <summary>
    /// Operator
    /// </summary>
    Operator,
    /// <summary>
    /// Pair, same as Operator, but acts as a trigger
    /// </summary>
    Pair,
    /// <summary>
    /// Number
    /// </summary>
    Number,		
    /// <summary>
    /// String
    /// </summary>
    String,		
    /// <summary>
    /// Character
    /// </summary>
    Character,
    /// <summary>
    /// Other
    /// </summary>
    Other,

    /// <summary>
    /// Custom flag
    /// </summary>
    Custom = 0x01000000,
  }


  /// <summary>
  /// Provides info to convert syntax to other formats
  /// </summary>
  public interface IDrawInfo
  {
    /// <summary>
    /// Gets the start column
    /// </summary>
    int Start {get;}
    /// <summary>
    /// Gets the end column
    /// </summary>
    int End {get;}
    /// <summary>
    /// Gets the text
    /// </summary>
    string Text {get;}

    /// <summary>
    /// Gets the foreground color
    /// </summary>
    Color ForeColor {get;}

    /// <summary>
    /// Gets the backgound color
    /// </summary>
    Color BackColor {get;}

    /// <summary>
    /// Gets the border color
    /// </summary>
    Color BorderColor { get;}

    /// <summary>
    /// Gets the font style
    /// </summary>
    FontStyle Style {get;}
  }
}
