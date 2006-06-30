using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class PowerShellLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "PowerShell"; } }
	  public override string[] Extensions {get { return new string[]{"ps1"}; } }
	  LexerBase lexer = new PowerShellLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class PowerShellLexer
%unicode
%ignorecase

WS		                    =[ \t]+
Keyword()                   ="if"|"elseif"|"else"|"switch"|"default"|"foreach"|"in"|"for"|"while"|"do"|"trap"|"until"|"finally"|"break"|"return"|"continue"|"function"|"filter"|"begin"|"process"|"end"|"param"
Number()                    =[0-9]+
Other()                     ="-"(eq|ne|ge|gt|lt|le|ieq|ine|ige|igt|ilt|ile|ceq|cne|cge|cgt|clt|cle|like|notlike|ilike|inotlike|clike|cnotlike|match|notmatch|imatch|inotmatch|cmatch|cnotmatch|contains|notcontains|icontains|inotcontains|ccontains|cnotcontains|isnot|is|as|replace|ireplace|creplace|[a-zA-Z]+)
String()                    =\"([^\"\n])*\"|'([^'])*'
Operator()                  ="{"|"}"|";"|"&&"|"||"|"|"|"&"|"+"|"-"|"*"|"/"|"%"|"++"|"--"|"!"|"."|":"|"::"|"["|"]"|"("|")"|".."|".*"|"2>&1"|">>"|">"|"<<"|"<"|">|"|"2>"|"2>>"|"1>>"|"="|"+="|"-="|"*="|"/="|"%="|"$("|"@("|"@{"|"-and"|"-or"|"-band"|"-bor"
LINE_COMMENT              ="#"[^\n]*
Identifier()                =[-a-zA-Z_\\][-a-zA-Z_-\\0-9:\.]*

VARIABLE                  ="$"[0-9a-zA-Z]+

%%

<YYINITIAL>{Keyword()}                  {return Keyword();}
<YYINITIAL>{Other()}                    {return Type();}
<YYINITIAL>{String()}                   {return String();}
<YYINITIAL>{Number()}                   {return Number();}
<YYINITIAL>{Operator()}                 {return Operator();}
<YYINITIAL>{Identifier()}               {return Identifier();}
<YYINITIAL>{VARIABLE}                 {return Other();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}


{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }

