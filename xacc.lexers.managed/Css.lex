using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class CssLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "CSS"; } }
	  public override string[] Extensions {get { return new string[]{"css"}; } }
	protected override LexerBase GetLexer() { return new CssLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CssLexer

%ignorecase
%unicode

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

<YYINITIAL>{comment_start}    { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^ \t\n\*]+		    { return Comment();}
<ML_COMMENT>{comment_end}	    { EXIT(); return Comment();}
<ML_COMMENT>\*		            { return Comment();}

<YYINITIAL>"{"                { ENTER(INCLASS); return Operator();}

<INCLASS>{comment_start}      { ENTER(ML_COMMENT); return Comment(); }
<INCLASS>":"                  { ENTER(INDEF); return Operator();}
<INCLASS>"}"                  { EXIT(); return Operator(); }  
<INCLASS>{ident}              { return Number();}

<INDEF>{comment_start}        { ENTER(ML_COMMENT); return Comment(); }
<INDEF>";"                    { EXIT(); return Operator(); }
<INDEF>"}"                    { EXIT(); EXIT(); return Operator();}  
<INDEF>[^ \n\t;\}]+           { return Keyword();}

<YYINITIAL>"A:"{ident}        { return String();}
<YYINITIAL>"#"{ident}         { return String();}
<YYINITIAL>"."{ident}         { return String();}
<YYINITIAL>{ident}            { return String();}

{nl}               { return NewLine();}
.                  { return Plain();}

 