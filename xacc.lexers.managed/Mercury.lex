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
Keyword()                   =module|use_module|import_module|include_module|end_module|initialise|mutable|initialize|finalize|finalise|interface|implementation|pred|mode|func|type|inst|solver|is|semidet|det|nondet|multi|erroneous|failure|cc_nondet|cc_multi|typeclass|instance|where|pragma|promise|external|some|all|not|if|then|else|true|fail|try|throw|catch
PREPROC                   =inline|no_inline|type_spec|source_file|fact_table|obsolete|memo|loop_check|minimal_model|terminates|does_not_terminate|check_termination
Number()                    =[0-9]+
Type()                      =string|char|int|bool|list|map|io
String()                    =\"([^\"\n])*\"|'([^'])*'
Operator()                  ="<=>"|"<="|"=>"|":-"|"::"|"//"|"->"|"-->"|"--->"|"\+"|[-,\.\[\]\(\)\|=_\*\+;!<>\{\}]
LINE_COMMENT              ="%"[^\n]*
COMMENT_START             ="/*"
COMMENT_END               ="*/"
Identifier()                =[a-zA-Z][_$a-zA-Z0-9]*

%state ML_COMMENT

%%

<YYINITIAL>{Keyword()}                  {return Keyword();}
<YYINITIAL>{PREPROC}                  {return Preprocessor();}
<YYINITIAL>{Type()}                     {return Type();}
<YYINITIAL>{String()}                   {return String();}
<YYINITIAL>{Number()}                   {return Number();}
<YYINITIAL>{Operator()}                 {return Operator();}
<YYINITIAL>{Identifier()}               {return Identifier();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return Comment();}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return Comment();}
<ML_COMMENT>[^ \t\n\*]+               {return Comment();}
<ML_COMMENT>"*"                       {return Comment();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }

