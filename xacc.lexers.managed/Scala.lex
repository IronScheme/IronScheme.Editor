
using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class ScalaLang : CSLex.Language<CSLex.Yytoken>
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
%full

WS		                    =[ \t]+
Keyword()                   ="=>"|"<-"|"_"|"unit"|"Unit"|"package"|"synchronized"|"import"|"return"|"true"|"false"|"def"|"val"|"var"|"class"|"object"|"trait"|"case"|"override"|"new"|"protected"|"extends"|"if"|"null"|"throw"|"for"|"with"|"try"|"catch"|"this"|"type"|"else"|"match"|"append"|"private"|"while"|"yield"|"super"|"sealed"|"requires"|"implicit"|"finally"|"final"|"do"|"abstract"
PREPROC                   =[^.]
Number()                    =[0-9]+
String()                    =\"([^\"\n])*\"
Character()                 ='([^'])+'
Type()                      =String|Int|Any|Boolean|List|Array|Character|Type|Pair|int|char|AnyRef
Operator()                  =[-:\(\),\.!=&\|\[\];><\{\}\+\*/@#%]
LINE_COMMENT              =//[^\n]*
COMMENT_START             ="/*"
COMMENT_END               ="*/"
Identifier()                =[a-zA-Z][_$a-zA-Z0-9]*

%state ML_COMMENT

%%

<YYINITIAL>{Keyword()}                  {return Keyword();}
<YYINITIAL>{PREPROC}                  {return Preprocessor();}
<YYINITIAL>{String()}                   {return String();}
<YYINITIAL>{Character()}                {return Character();}
<YYINITIAL>{Number()}                   {return Number();}
<YYINITIAL>{Operator()}                 {return Operator();}
<YYINITIAL>{Type()}                     {return Type();}
<YYINITIAL>{Identifier()}               {return Identifier();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return Comment();}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return Comment();}
<ML_COMMENT>[^ \t\n\*]+               {return Comment();}
<ML_COMMENT>"*"                       {return Comment();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return ERROR; }
