using Xacc.ComponentModel;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class PatchLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Patch"; } }
	  public override string[] Extensions {get { return new string[]{"patch", "diff"}; } }
	  protected override LexerBase GetLexer() { return new PatchLexer(); }
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
{RANGE}                  { return Type(); }
{ADD}|{ADDFILE}          { return Keyword(); }
{REM}|{REMFILE}          { return Comment(); }
{CMD}                    { return Other(); }
\n                       { return NewLine(); }
[^ \t\n]+                { return DocComment(); }




