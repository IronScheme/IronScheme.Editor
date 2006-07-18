using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class FSharpLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "FSharp"; } }
	  public override string[] Extensions {get { return new string[]{"fs","fsi"}; } }
	  LexerBase lexer = new FSharpLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class FSharpLexer

%full

keyword =(abstract|and|as|assert|asr|begin|class|constraint|default|delegate|do|done|downcast|downto|else|end|enum|exception|false|finally|for|foreach|fun|function|if|ignore|in|inherit|interface|land|lazy|let|lor|lsl|lsr|lxor|match|member|mod|module|mutable|new|null|of|open|or|override|rec|sig|static|struct|then|to|true|try|type|val|when|inline|upcast|while|with)
operator =[-~!%^\*\(\)\+=\[\]\|\\:;,\./\?&<>\{\}]|"<@@"|"@@>"

comment_start          ="(*"
comment_end            ="*)"

singlelinecomment      ="//".*

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

%state ML_COMMENT
%state ML_STRING

%%
{white_space}         { ; }
{new_line}            { return NewLine();}

<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }

<YYINITIAL>\"                    { ENTER(ML_STRING); return String(); }

<ML_STRING>({reg_string_char})+  { return String() ; }
<ML_STRING>\"                     { EXIT(); return String() ; }

<ML_COMMENT>[^*\t\n]+             { return Comment(); }
<ML_COMMENT>"*"                   { return Comment(); }
<ML_COMMENT>{comment_end}         { EXIT(); return Comment(); }

<YYINITIAL>{preprocessor}[^\n].+ { return Preprocessor(); }

<YYINITIAL>{singlelinecomment}   { return Comment(); }                      
                    
{keyword}             { return Keyword(); } 
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number();}

{operator}            { return Operator(); }                     

{identifier}          { return Identifier(); }

.                     { return Plain(); }

 