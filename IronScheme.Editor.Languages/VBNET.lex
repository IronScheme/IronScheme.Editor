using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class VBNETLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "VBNET"; } }
	  public override string[] Extensions {get { return new string[]{"vb"}; } }
	  protected override LexerBase GetLexer() { return new VBNETLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class VBNETLexer

%unicode

keyword1 =(AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar)
keyword2 =(CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|Date|Decimal)
keyword3 =(Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For)
keyword4 =(Friend|Function|Get|GetType|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is)
keyword5 =(IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New)
keyword6 =(Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides)
keyword7 =(ParamArray|Partial|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|RemoveHandler|Resume|Return|SByte|Select|Set)
keyword8 =(Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|TryCast|TypeOf|UInteger|ULong)
keyword9 =(UShort|Using|Variant|Wend|When|While|Widening|With|WithEvents|WriteOnly|Xor)

keyword =({keyword1}|{keyword2}|{keyword3}|{keyword4}|{keyword5}|{keyword6}|{keyword7}|{keyword8}|{keyword9})	


operator =[-~!%^\*\(\)\+=\[\]\|\\:;,\./\?&<>\{\}]

comment_start          ="(*"
comment_end            ="*)"

line_comment           =("'"|"REM")[^\n]*

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

single_string_char     =[^\"\n]
string_esc_seq         =\"\"
reg_string_char        ={single_string_char}|{string_esc_seq}
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

%%

{white_space}+        { ; }

{preprocessor}[^\n].+ { return Preprocessor(); }
{line_comment}     { return Comment(); }
                      
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }


<ML_COMMENT>[^*\n]*               { return Comment(); }
<ML_COMMENT>"*"+[^*\)\n]*         { return Comment(); }
<ML_COMMENT>{comment_end}         { EXIT(); return Comment(); }

                    
{keyword}             { return Keyword(); } 
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number(); }
{character_literal}   { return Character(); }
{string_literal}      { return String(); }

{operator}            { return Operator(); }                     

{identifier}          { return Identifier(); }

{new_line}            { return NewLine();}
.                     { return Plain(); }


 