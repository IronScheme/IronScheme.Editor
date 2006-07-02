using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class RubyLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Ruby"; } }
	  public override string[] Extensions {get { return new string[]{"rb"}; } }
	  LexerBase lexer = new RubyLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class RubyLexer

%full

keyword1 =(alias|begin|BEGIN|break|case|defined?|do|else|elsif|end|END|ensure|for|if|in|loop|next|raise|redo|rescue|retry|return|super|then|undef|unless|until|when|while|yield)
keyword2 =(false|nil|self|true|__FILE__|__LINE__)
keyword3 =(and|not|or)
keyword4 =(def|class|module)
keyword5 =(catch|fail|include|load|require|throw)

keyword =({keyword1}|{keyword2}|{keyword3}|{keyword4}|{keyword5})

function =(lambda|proc|eval)

operator =[-~!%^\*\(\)\+=\[\]\|\\:;,\./\?&<>\{\}]

comment_start           =^"=begin"
comment_end             =^"=end"

line_comment            ="#"[^\n]*

white_space            =[ \t]
new_line               =\n

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
simple_esc_seq         =(\\(['\\0abfnrtv]|[0-9]+))
uni_esc_seq1           =\\u{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq2           =\\U{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq            ={uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            =\\x({hex_digit})?({hex_digit})?({hex_digit})?{hex_digit}
character              ={single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      ='({character})+'

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

single_re_char         =[^\\/ \t\n]
single_re_char2        =[^\\/ \*\t\n]
re_esc_seq             =(\\[\\\?/\.$^\(\)\{\}\*\+\-\|sbrntWwDdS\[\],]|{re_esc_seq2}|{hex_esc_seq})
re_esc_seq2            =(\\({dec_digit})+)
re_string_char         =({single_re_char}|{re_esc_seq})
re_string_char2        =({single_re_char2}|{re_esc_seq})
re_string              ="/"{re_string_char2}({re_string_char})*"/"[gim]*|"//"


letter_char            =[A-Za-z]
ident_char             ={dec_digit}|{letter_char}|"_"|"@"|"$"
identifier             =({letter_char}|"_"|"$"|"@")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"
 
%state ML_COMMENT

%%

{white_space}+        { ; }
{new_line}            { return NewLine();}  
                    
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>{comment_end}         { EXIT(); return Comment();  }
<ML_COMMENT>"="[^=\n]+            { return Comment(); }
<ML_COMMENT>[^\n=]+               { return Comment(); }

`[^`\n]*`             { return Custom(KnownColor.Black, KnownColor.Yellow); }

{line_comment}        { return Comment(); }
                    
{re_string}           { return Other(); }                    
{function}            { return Type(); }                     
{keyword}             { return Keyword(); } 
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number(); }
{character_literal}   { return String(); }
{string_literal}      { return String(); }

{operator}            { return Operator(); }                     

{identifier}          { return Plain(); }

.                     { return Plain(); }

