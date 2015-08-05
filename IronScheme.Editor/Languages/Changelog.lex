#pragma warning disable 162
using IronScheme.Editor.ComponentModel;
using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class Changelog : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "ChangeLog"; } }
	  public override string[] Extensions {get { return new string[]{"ChangeLog"}; } }
	  protected override LexerBase GetLexer() { return new ChangelogLexer(); }
	  
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
VER                   =({INT}("."{INT})+("-"[^ \n\t]+)?)|({INT}("."{INT})+(" SVN rev: "{INT}))
TAIL                  =("-"|\t|"  ")[^\n]+
HEADING               =[^\n]+":"

%%

<EXPDATE>{TAIL}       { BEGIN(YYINITIAL); return Type(); }

{VER}                 { BEGIN(EXPDATE); return Keyword();}
{HEADING}             { return Preprocessor(); }
{TAIL}                { return Plain(); } 

[ \t]+                { ; }
\n                    { return NewLine();}
.                     { return Error(); }








