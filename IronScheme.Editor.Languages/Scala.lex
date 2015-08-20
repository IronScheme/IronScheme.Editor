using IronScheme.Editor.ComponentModel;

using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class ScalaLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Scala"; } }
	  public override string[] Extensions {get { return new string[]{"scala"}; } }
	  protected override LexerBase GetLexer() { return new ScalaLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class ScalaLexer
%full

WS		                    =[ \t]+
KEYWORD                   ="=>"|"<-"|"_"|"unit"|"Unit"|"package"|"synchronized"|"import"|"return"|"true"|"false"|"def"|"val"|"var"|"class"|"object"|"trait"|"case"|"override"|"new"|"protected"|"extends"|"if"|"null"|"throw"|"for"|"with"|"try"|"catch"|"this"|"type"|"else"|"match"|"append"|"private"|"while"|"yield"|"super"|"sealed"|"requires"|"implicit"|"finally"|"final"|"do"|"abstract"
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

<YYINITIAL>{KEYWORD}                  {return Keyword();}
<YYINITIAL>{STRING}                   {return String();}
<YYINITIAL>{CHARACTER}                {return Character();}
<YYINITIAL>{NUMBER}                   {return Number();}
<YYINITIAL>{OPERATOR}                 {return Operator();}
<YYINITIAL>{TYPE}                     {return Type();}
<YYINITIAL>{IDENTIFIER}               {return Identifier();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return Comment();}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return Comment();}
<ML_COMMENT>[^ \t\n\*]+               {return Comment();}
<ML_COMMENT>"*"                       {return Comment();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }
