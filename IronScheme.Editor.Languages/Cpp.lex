using IronScheme.Editor.ComponentModel;
using System.Drawing;
using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class CppLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Cpp"; } }
	  public override string[] Extensions {get { return new string[]{"c","cc","cpp","h","hh","hpp"}; } }
	  protected override LexerBase GetLexer() { return new CppLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CppLexer

%full

%{
int inproc;
%}


line_comment           =("//".*)

comment_start          ="/*"
comment_end            ="*"+"/"

white_space            =[ \t]
new_line               =\n

preprocessor           =^({white_space})*#({white_space})*

attr                   =\[({white_space})*(assembly|return)({white_space})*:

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

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\0abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
error_string           =\"({reg_string_char})*
single_verbatim_char   =[^\"\n]
quote_esc_seq          =\"\"
verb_string_char       ={single_verbatim_char}|{quote_esc_seq}
string_literal         ={regular_string}

verbatim_string_start  =\@\"
verbatim_string_cont   =({verb_string_char})+ 
verbatim_string_end    =\"


letter_char            =[A-Za-z]
ident_char             ={dec_digit}|{letter_char}|"_"
identifier             =({letter_char}|"_")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"


keyword1 =(asm|auto|break|case|catch|class|const|const_cast|continue|default)
keyword2 =(delete|do|dynamic_cast|else|enum|explicit|export|extern|false|for)
keyword3 =(friend|goto|if|inline|mutable|namespace|new|operator|private)
keyword4 =(protected|public|register|reinterpret_cast|return|sizeof|static)
keyword5 =(static_cast|struct|switch|template|throw|this|true|try|typedef)
keyword6 =(typeid|typename|union|using|virtual|volatile|while|void|__cdecl|inline|__inline)
mc_kw    =(__property|__value|__gc|__nogc|__abstract|__sealed|__box|__event)
basetype =(bool|char|double|float|int|long|short|signed|unsigned|wchar_t)

keyword               =({keyword1}|{keyword2}|{keyword3}|{keyword4}|{keyword5}|{keyword6}|{mc_kw}|{basetype})

operator =[-~!%^\*\(\)\+=\[\]\|\\:;,\./\?&<>\{\}]

%state ML_COMMENT
%state PREPROCESSOR

%%

<YYINITIAL>{preprocessor}        { ENTER(PREPROCESSOR); return Preprocessor(); }

<PREPROCESSOR>\\                  { inproc = 1; return DocComment(); }
<PREPROCESSOR>\n                  { if (inproc == 0) EXIT(); return NewLine(); }
<PREPROCESSOR>[^\n\\]+            { inproc = 0; return Preprocessor(); }

({white_space})+        { ; }
                      
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^*\n\t]+         { return Comment(); }
<ML_COMMENT>"*"+[^*/\n\t]*    { return Comment(); }
<ML_COMMENT>{comment_end}     { EXIT(); return Comment(); }

<YYINITIAL>{line_comment}        { return Comment(); }
                      
{keyword}             { return Keyword(); } 
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number(); }
{character_literal}   { return Character(); }
{string_literal}      { return String(); }

{operator}            { return Operator(); }                     

{identifier}          { return Identifier(); }
{at_identifier}       { return Identifier(); }

{new_line}            { return NewLine();}
.                     { return Plain(); }

