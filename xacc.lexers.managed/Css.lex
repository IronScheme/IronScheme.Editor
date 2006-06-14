using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class CssLang : CSLex.Language
  {
	  public override string Name {get {return "CSS"; } }
	  public override string[] Extensions {get { return new string[]{"css"}; } }
	  LexerBase lexer = new CssLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CssLexer

%ignorecase

nmstart		      =([_a-z])
nmchar		      =([_a-zA-Z0-9-])

ident		        ={nmstart}({nmchar})*

s		            =[ \t]
w		            =({s})*
nl		          =\n|\r\n|\r|\f

comment_start   ="/*"
comment_end     ="*"+"/"

%state INCLASS
%state INDEF
%state ML_COMMENT

%%

({s})+  	                    {;}

<YYINITIAL>{comment_start}    { ENTER(ML_COMMENT); return COMMENT; }

<ML_COMMENT>[^ \t\n\*]+		    { return COMMENT;}
<ML_COMMENT>{comment_end}	    { EXIT(); return COMMENT;}
<ML_COMMENT>\*		            { return COMMENT;}

<YYINITIAL>"{"                { ENTER(INCLASS); return OPERATOR;}

<INCLASS>{comment_start}      { ENTER(ML_COMMENT); return COMMENT; }
<INCLASS>":"                  { ENTER(INDEF); return OPERATOR;}
<INCLASS>"}"                  { EXIT(); return OPERATOR; }  
<INCLASS>{nl}                 { return NEWLINE;}
<INCLASS>{ident}              { return NUMBER;}
<INCLASS>.                    { return PLAIN;}


<INDEF>{comment_start}        { ENTER(ML_COMMENT); return COMMENT; }
<INDEF>";"                    { EXIT(); return OPERATOR; }
<INDEF>"}"                    { EXIT(); EXIT(); return OPERATOR;}  
<INDEF>{nl}                   { return NEWLINE;}
<INDEF>[^ \n\t;\}]+           { return KEYWORD;}

<YYINITIAL>{nl}               { return NEWLINE;}

<YYINITIAL>"A:"{ident}        { return STRING;}
<YYINITIAL>"#"{ident}         { return STRING;}
<YYINITIAL>"."{ident}         { return STRING;}
<YYINITIAL>{ident}            { return STRING;}
<YYINITIAL>.                  { return PLAIN;}

 