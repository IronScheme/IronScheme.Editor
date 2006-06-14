
using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class ScalaLang : CSLex.Language
  {
	  public override string Name {get {return "Scala"; } }
	  public override string[] Extensions {get { return new string[]{"scala"}; } }
	  LexerBase lexer = new ScalaLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class ScalaLexer

WS		                    =[ \t]+
KEYWORD                   ="=>"|"<-"|"_"|"unit"|"Unit"|"package"|"synchronized"|"import"|"return"|"true"|"false"|"def"|"val"|"var"|"class"|"object"|"trait"|"case"|"override"|"new"|"protected"|"extends"|"if"|"null"|"throw"|"for"|"with"|"try"|"catch"|"this"|"type"|"else"|"match"|"append"|"private"|"while"|"yield"|"super"|"sealed"|"requires"|"implicit"|"finally"|"final"|"do"|"abstract"
PREPROC                   =[^.]
NUMBER                    =[0-9]+
STRING                    =\"([^\"\n])*\"
CHARACTER                 ='([^'])+'
TYPE                      =String|Int|Any|Boolean|List|Array|Character|Type|Pair|int|char|AnyRef
OPERATOR                  =[-:\(\),\.!=&\|\[\];><\{\}\+\*/@#%]
LINE_COMMENT              =//[^\n]*
COMMENT_START             ="/*"
COMMENT_END               ="*/"
IDENTIFIER                =[a-zA-Z][_$a-zA-Z0-9]*

%state ML_COMMENT

%%

<YYINITIAL>{KEYWORD}                  {return KEYWORD;}
<YYINITIAL>{PREPROC}                  {return PREPROC;}
<YYINITIAL>{STRING}                   {return STRING;}
<YYINITIAL>{CHARACTER}                {return CHARACTER;}
<YYINITIAL>{NUMBER}                   {return NUMBER;}
<YYINITIAL>{OPERATOR}                 {return OPERATOR;}
<YYINITIAL>{TYPE}                     {return TYPE;}
<YYINITIAL>{IDENTIFIER}               {return IDENTIFIER;}
<YYINITIAL>{LINE_COMMENT}             {return COMMENT;}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return COMMENT;}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return COMMENT;}
<ML_COMMENT>[^ \t\n\*]+               {return COMMENT;}
<ML_COMMENT>"*"                       {return COMMENT;}

{WS}			                            {;}
\n                                    {return NEWLINE;}
.                                     {return ERROR; }
