using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class PowerShellLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "PowerShell"; } }
	  public override string[] Extensions {get { return new string[]{"ps1","msh"}; } }
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
KEYWORD                   ="if"|"elseif"|"else"|"switch"|"default"|"foreach"|"in"|"for"|"while"|"do"|"trap"|"until"|"finally"|"break"|"return"|"continue"|"function"|"filter"|"begin"|"process"|"end"|"param"
NUMBER                    =[0-9]+
OTHER                     ="-"(eq|ne|ge|gt|lt|le|ieq|ine|ige|igt|ilt|ile|ceq|cne|cge|cgt|clt|cle|like|notlike|ilike|inotlike|clike|cnotlike|match|notmatch|imatch|inotmatch|cmatch|cnotmatch|contains|notcontains|icontains|inotcontains|ccontains|cnotcontains|isnot|is|as|replace|ireplace|creplace|[a-zA-Z]+)
STRING                    =\"([^\"\n])*\"|'([^'])*'
OPERATOR                  ="{"|"}"|";"|"&&"|"||"|"|"|"&"|"+"|"-"|"*"|"/"|"%"|"++"|"--"|"!"|"."|":"|"::"|"["|"]"|"("|")"|".."|".*"|"2>&1"|">>"|">"|"<<"|"<"|">|"|"2>"|"2>>"|"1>>"|"="|"+="|"-="|"*="|"/="|"%="|"$("|"@("|"@{"|"-and"|"-or"|"-band"|"-bor"
LINE_COMMENT              ="#"[^\n]*
IDENTIFIER                =[-a-zA-Z_\\][-a-zA-Z_-\\0-9:\.]*

VARIABLE                  ="$"[0-9a-zA-Z]+

%%

<YYINITIAL>{KEYWORD}                  {return Keyword();}
<YYINITIAL>{OTHER}                    {return Type();}
<YYINITIAL>{STRING}                   {return String();}
<YYINITIAL>{NUMBER}                   {return Number();}
<YYINITIAL>{OPERATOR}                 {return Operator();}
<YYINITIAL>{IDENTIFIER}               {return Identifier();}
<YYINITIAL>{VARIABLE}                 {return Other();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }

