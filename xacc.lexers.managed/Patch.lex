using Xacc.ComponentModel;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class PatchLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Patch"; } }
	  public override string[] Extensions {get { return new string[]{"patch", "diff"}; } }
	  LexerBase lexer = new PatchLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}

%%

%class PatchLexer
%unicode

WS          =[ \t]+
ADD         ="+"[^\n]*
REM         ="-"[^\n]*
ADDFILE     ="+++"[^\n]+
REMFILE     ="---"[^\n]+
CMD         ="diff"[^\n]+
ATAT        ="@@"
RANGE       ={ATAT}[^@\n]+{ATAT}

%%

{WS}                     { ; }
{RANGE}                  { return TYPE; }
{ADD}|{ADDFILE}          { return KEYWORD; }
{REM}|{REMFILE}          { return COMMENT; }
{CMD}                    { return OTHER; }
\n                       { return NEWLINE; }
[^ \t\n]+                { return DOCCOMMENT; }




