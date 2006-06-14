#pragma warning disable 162
using Xacc.ComponentModel;

namespace Xacc.Languages
{
  sealed class Changelog : CSLex.Language
  {
	  public override string Name {get {return "ChangeLog"; } }
	  public override string[] Extensions {get { return new string[]{"ChangeLog"}; } }
	  LexerBase lexer = new ChangelogLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
	  
	  public override bool Match(string filename)
    {
      return filename.ToLower() == "changelog.txt";
    }
  }
}

%%

%class ChangelogLexer
%state EXPDATE

INT                   =[0-9]+
VER                   ={INT}("."{INT})+("-"[^ \n\t]+)?
TAIL                  =("-"|\t|"  ")[^\n]+
HEADING               =[^\n]+":"

%%

<EXPDATE>{TAIL}       { BEGIN(YYINITIAL); return TYPE; }

{VER}                 { BEGIN(EXPDATE); return KEYWORD;}
{HEADING}             { return PREPROC; }
{TAIL}                { return PLAIN; } 

[ \t]+                { ; }
\n                    { return NEWLINE;}
.                     { return ERROR; }








