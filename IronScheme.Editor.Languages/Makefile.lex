using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.Languages
{
  sealed class MakefileLanguage : CSLex.Language
  {
	  public override string Name {get {return "Makefile"; } }
	  public override string[] Extensions {get { return new string[]{"Makefile"}; } }
	  LexerBase lexer = new MakefileLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class MakefileLexer

ws		                    =[ ]+
comment                   ="#".*   

identifier                =[\._A-Za-z0-9]+
vardec                    =({identifier}({ws})*"=")
varstart                  ="$"\(
varend                    =\)
varref                    =({varstart}{identifier}{varend})
ruledec                   =({identifier}|{varref})({ws})*":"

%state COMMAND
%state TARGET
%state RULE

%%


<COMMAND>{varref}                {return PREPROC;}
<COMMAND>{identifier}            {return PLAIN;}
<COMMAND>\n                      {EXIT(); return NEWLINE; }
<COMMAND>[^\n ]                  {return PLAIN;}


<TARGET>\t+                     {ENTER(COMMAND);}
<TARGET>.                       { /*yyless(0);*/ EXIT(); }

<RULE>{identifier}            {return STRING);}
<RULE>\n                      {ENTER(TARGET); return NEWLINE; }

{comment}                 {return COMMENT;}
{ws}			                {;}

{vardec}                  {return OTHER;}
{varref}                  {return PREPROC;}
{ruledec}                 {ENTER(RULE); return KEYWORD;}

{varstart}                {return OPERATOR;}
{varend}                  {return OPERATOR;}

\n                        {return NEWLINE;}
.                         {return PLAIN; }

