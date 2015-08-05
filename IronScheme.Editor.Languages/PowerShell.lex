using IronScheme.Editor.ComponentModel;
using System.Drawing;

using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class PowerShellLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "PowerShell"; } }
	  public override string[] Extensions {get { return new string[]{"ps1","msh"}; } }
	  protected override LexerBase GetLexer() { return new PowerShellLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class PowerShellLexer
%unicode
%ignorecase

dec_digit              =[0-9]
hex_digit              =[0-9A-Fa-f]
int_suffix             =[UuLl]|[Uu][Ll]|[Ll][Uu]
dec_literal            =({dec_digit})+({int_suffix})?
hex_literal            =0[xX]({hex_digit})+({int_suffix})?
integer_literal        ={dec_literal}|{hex_literal}

real_suffix            =[FfDdMm]
sign                   =[-\+]
exponent_part          =[eE]({sign})?({dec_digit})+
whole_real1            =({dec_digit})+{exponent_part}({real_suffix})?
whole_real2            =({dec_digit})+{real_suffix}
part_real              =({dec_digit})*\.({dec_digit})+({exponent_part})?({real_suffix})?
real_literal           ={whole_real1}|{whole_real2}|{part_real}

single_char            =[^'\\\n]
simple_esc_seq         =\\['\\0abfnrtv]
uni_esc_seq1           =\\u{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq2           =\\U{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq            ={uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            =\\x({hex_digit})?({hex_digit})?({hex_digit})?{hex_digit}
character              ={single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      ='({character})'

single_string_char     =[^`\"\n]
string_esc_seq         ="`"[\"]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
string_literal         ={regular_string}

verb_string_start      =\"({reg_string_char})*"`"

WS		                    =[ \t]+
KEYWORD                   ="if"|"elseif"|"else"|"switch"|"default"|"foreach"|"in"|"for"|"while"|"do"|"trap"|"until"|"finally"|"break"|"return"|"continue"|"function"|"filter"|"begin"|"process"|"end"|"param"
NUMBER                    =[0-9]+
OTHER                     ="-"(eq|ne|ge|gt|lt|le|ieq|ine|ige|igt|ilt|ile|ceq|cne|cge|cgt|clt|cle|like|notlike|ilike|inotlike|clike|cnotlike|match|notmatch|imatch|inotmatch|cmatch|cnotmatch|contains|notcontains|icontains|inotcontains|ccontains|cnotcontains|isnot|is|as|replace|ireplace|creplace|[a-zA-Z]+)
STRING                    =\"([^\"\n])*\"|'([^'])*'
OPERATOR                  ="{"|"}"|";"|"&&"|"||"|"|"|"&"|"+"|"-"|"*"|"/"|"%"|"++"|"--"|"!"|","|"."|":"|"::"|"["|"]"|"("|")"|".."|".*"|"2>&1"|">>"|">"|"<<"|"<"|">|"|"2>"|"2>>"|"1>>"|"="|"+="|"-="|"*="|"/="|"%="|"$("|"@("|"@{"|"-and"|"-or"|"-band"|"-bor"
LINE_COMMENT              ="#"[^\n]*
IDENTIFIER                =([-a-zA-Z_\\][-a-zA-Z_-\\0-9:]*)

SPEC_VAR                  ="$"([$?^]|"Args"|"Error"|"ExecutionContext"|"foreach"|"HOME"|"Input"|"Match"|"MyInvocation"|"PSHome"|"Host"|"LastExitCode"|"true"|"false"|"null"|"this"|"OFS"|"ShellID"|"StackTrace")
VARIABLE                  ="$"({IDENTIFIER})

%state VERBSTR

%%

<YYINITIAL>{verb_string_start}      {ENTER(VERBSTR); return String(); }

<VERBSTR>({reg_string_char})*\"    {EXIT(); return String();}
<VERBSTR>({reg_string_char})*"`"    { return String(); }

<YYINITIAL>{KEYWORD}                  {return Keyword();}
<YYINITIAL>{OTHER}                    {return Operator();}
<YYINITIAL>{STRING}                   {return String();}
<YYINITIAL>{string_literal}              {return String();}
<YYINITIAL>{NUMBER}                   {return Number();}
<YYINITIAL>{OPERATOR}                 {return Operator();}
<YYINITIAL>{IDENTIFIER}               {return Identifier();}
<YYINITIAL>{SPEC_VAR}                 {return Other();}
<YYINITIAL>{VARIABLE}                 {return Type();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }

