using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class JavaScriptLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "JavaScript"; } }
	  public override string[] Extensions {get { return new string[]{"js"}; } }
	  protected override LexerBase GetLexer() { return new JavaScriptLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class JavaScriptLexer

%unicode

types    =(Undefined|Null|Boolean|String|Number|Object|Array)
objectatts  =(ReadOnly|DontEnum|DontDelete|Internal)
basetype =(null|true|false)
keyword  ={basetype}|(break|case|catch|continue|default|delete|do|else|finally|for|function|if|in|instanceof|new|prototype|return|switch|throw|this|try|typeof|var|void|while|with)
reserved ={reserved2}|(abstract|class|const|debugger|enum|export|extends|final|goto|implements|interface|native|package|private|protected|public|static|super|synchronized|throws|transient|volatile)
reserved2 =(int|byte|boolean|char|long|float|double|short)

operator =("-"|"~"|"!"|"%"|"^"|"*"|"("|")"|"+"|"["|"]"|"|"|"\\""|":"|";"|","|"."|"/"|"?"|"&"|"<"|">")

comment_start          ="/*"
comment_end            ="*"+"/"

line_comment           ="//".*

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
simple_esc_seq         =\\['\\0abfnrtv]
uni_esc_seq1           =\\u{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq2           =\\U{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}{hex_digit}
uni_esc_seq            ={uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            =\\x({hex_digit})?({hex_digit})?({hex_digit})?{hex_digit}
character              ={single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      ='({character})+'

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
error_string           =\"({reg_string_char})*
string_literal         ={regular_string}

single_re_char         =[^\\/ \t\n]
single_re_char2        =[^\\/ \*\t\n]
re_esc_seq             =(\\[\\\?/\.$^\(\)\{\}\*\+\-\|sbrntWwDdS\[\],]|{re_esc_seq2}|{hex_esc_seq})
re_esc_seq2            =(\\({dec_digit})+)
re_string_char         =({single_re_char}|{re_esc_seq})
re_string_char2        =({single_re_char2}|{re_esc_seq})
re_string              ="/"{re_string_char2}({re_string_char})*"/"[gim]*

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"_"|"@")
identifier             =({letter_char}|"_"|"$")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

%state ML_COMMENT

%%

({white_space})+      { ; }
                      
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^\*\n]*               { return Comment(); }
<ML_COMMENT>"*"+[^\*/\n]*          { return Comment(); }
<ML_COMMENT>{comment_end}         { EXIT(); return Comment(); }

{line_comment}        { return Comment(); }

{types}               { return Type(); } 
{keyword}             { return Keyword(); } 
{reserved}            { return Error(); } 
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number(); }
{character_literal}   { return String(); }
{string_literal}      { return String(); }
{re_string}           { return Other(); }

{operator}            { return Operator(); }                     

{identifier}          { return Plain(); }

{new_line}            { return NewLine();}
.                     { return Plain(); }


