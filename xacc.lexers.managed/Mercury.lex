using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class MercuryLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Mercury"; } }
	  public override string[] Extensions {get { return new string[]{"m"}; } }
	  LexerBase lexer = new MercuryLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class MercuryLexer
%full

WS		                    =[ \t]+
KEYWORD                   =module|use_module|import_module|include_module|end_module|initialise|mutable|initialize|finalize|finalise|interface|implementation|pred|mode|func|type|inst|solver|is|semidet|det|nondet|multi|erroneous|failure|cc_nondet|cc_multi|typeclass|instance|where|pragma|promise|external|some|all|not|if|then|else|true|fail|try|throw|catch
PREPROC                   =inline|no_inline|type_spec|source_file|fact_table|obsolete|memo|loop_check|minimal_model|terminates|does_not_terminate|check_termination
NUMBER                    =[0-9]+
TYPE                      =string|char|int|bool|list|map|io
STRING                    =\"([^\"\n])*\"|'([^'])*'
OPERATOR                  ="<=>"|"<="|"=>"|":-"|"::"|"//"|"->"|"-->"|"--->"|"\+"|[-,\.\[\]\(\)\|=_\*\+;!<>\{\}]
LINE_COMMENT              ="%"[^\n]*
COMMENT_START             ="/*"
COMMENT_END               ="*/"
IDENTIFIER                =[a-zA-Z][_$a-zA-Z0-9]*

%state ML_COMMENT

%%

<YYINITIAL>{KEYWORD}                  {return KEYWORD;}
<YYINITIAL>{PREPROC}                  {return PREPROC;}
<YYINITIAL>{TYPE}                     {return TYPE;}
<YYINITIAL>{STRING}                   {return STRING;}
<YYINITIAL>{NUMBER}                   {return NUMBER;}
<YYINITIAL>{OPERATOR}                 {return OPERATOR;}
<YYINITIAL>{IDENTIFIER}               {return IDENTIFIER;}
<YYINITIAL>{LINE_COMMENT}             {return COMMENT;}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return COMMENT;}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return COMMENT;}
<ML_COMMENT>[^ \t\n\*]+               {return COMMENT;}
<ML_COMMENT>"*"                       {return COMMENT;}

{WS}			                            {;}
\n                                    {return NEWLINE;}
.                                     {return ERROR; }

