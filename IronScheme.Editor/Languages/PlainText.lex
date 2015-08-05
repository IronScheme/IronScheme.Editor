#pragma warning disable 162
using IronScheme.Editor.ComponentModel;
using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class PlainText : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Plain Text"; } }
	  public override string[] Extensions {get { return new string[]{"*"}; } }
	  protected override LexerBase GetLexer() { return new PlainTextLexer(); } 
  }
}

%%

%class PlainTextLexer
%unicode

ws		                    =[ \t]+
identifier                =[^ \t\n\.,\(\)!\?]+


%%

{identifier}              {return Plain(); }
{ws}			                {;}
\n                        {return NewLine();}

.                         {return Plain(); }
