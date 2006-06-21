#pragma warning disable 162
using Xacc.ComponentModel;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class PlainText : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Plain Text"; } }
	  public override string[] Extensions {get { return new string[]{"*"}; } }
	  LexerBase lexer = new PlainTextLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}

%%

%class PlainTextLexer
%unicode

ws		                    =[ \t]+
identifier                =[^ \t\n\.,\(\)!\?]+


%%

{identifier}              {return PLAIN; }
{ws}			                {;}
\n                        {return NEWLINE;}

.                         {return PLAIN; }
