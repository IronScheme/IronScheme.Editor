#pragma warning disable 162
using Xacc.ComponentModel;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
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
