#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class LSharpLanguage : CSLex.Language
  {
	  public override string Name {get {return "L#"; } }
	  public override string[] Extensions {get { return new string[]{"ls"}; } }
	  LexerBase lexer = new LSharpLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class LSharpLexer

%unicode

line_comment           =";"[^\n]*

comment_start          ="#|"
comment_end            ="|#"

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
character_literal      =#\\{character}

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
error_string           =\"({reg_string_char})*
single_verbatim_char   =[^\"\n]
quote_esc_seq          =\"\"
verb_string_char       ={single_verbatim_char}|{quote_esc_seq}
string_literal         ={regular_string}

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"-"|"_"|"!"|"+")
identifier             =({letter_char}({ident_char})*)("[]")?|("*"({ident_char})+"*")

atoms     =(null|true|false)
forms1    =(and|backquote|call|compile|cond|do|each|fn|for|foreach)
forms2    =(if|let|macro|or|quote|the|to|trace|try|when|while|with|"++"|"--")
func1     =(apply|append|assoc|caaar|caadr|caar|cadar|caddr|cadr|car|cdar|cdaar|cddar|cdddr|cddr|cdr)
func2     =(cons|environment|eq|eql|eval|evalstring|exit|first|import|inspect|is|length|list)
func3     =(load|macroexpand|map|nconc|new|not|nth|pr|prl|read|readstring|reference|reset|reverse|rest|throw|typeof|using)
func4     =("+"|"="|"*"|"/"|"-"|">"|">="|"<"|"<="|"&"|"^"|"|"|"!="|"==")
builtin   =(defun|defmacro|listp)

keyword               =({forms1}|{forms2}|{builtin})
function              =({func1}|{func2}|{func3}|{func4})


%state KWSTATE
%state MACRO
%state ML_COMMENT

%%

{white_space}+        { ; }
                      
{comment_start}       { ENTER(ML_COMMENT); return COMMENT; }                      
{line_comment}        { return COMMENT; }

"&body"               { return OTHER; }
"&rest"               { return OTHER; }

<MACRO>",@"                  { return OPERATOR; }
<MACRO>","                   { return OPERATOR; }
<MACRO>"("                   { ENTER(KWSTATE); return OPERATOR; }                     
<MACRO>")"                   { EXIT(); return OPERATOR; }  

<ML_COMMENT>{white_space}+    { ; }
<ML_COMMENT>{new_line}        { return NEWLINE; }
<ML_COMMENT>[^\n\|]+         { return COMMENT; }
<ML_COMMENT>{comment_end}     { EXIT(); return COMMENT; }
<ML_COMMENT>"|"               { return COMMENT; }


 
<KWSTATE>{white_space}+        { ; }
                      
<KWSTATE>{line_comment}        { return COMMENT; }

<KWSTATE>"&body"               { return OTHER; }
<KWSTATE>"&rest"               { return OTHER; }

<KWSTATE>{atoms}               { return KEYWORD; } 
<KWSTATE>{keyword}             { EXIT(); return KEYWORD; } 
<KWSTATE>{function}            { EXIT(); return TYPE; }
<KWSTATE>{identifier}          { EXIT(); return IDENTIFIER; }
<KWSTATE>{character_literal}   { EXIT(); return CHARACTER; }
<KWSTATE>{integer_literal}     { EXIT(); return NUMBER; }
<KWSTATE>{real_literal}        { EXIT(); return NUMBER; }
<KWSTATE>{string_literal}      { EXIT(); return STRING; }

<KWSTATE>"("                   { ENTER(KWSTATE); return OPERATOR; }                     
<KWSTATE>")"                   { return OPERATOR;  }                     
<KWSTATE>"`"                   { ENTER(MACRO); return OPERATOR;  }
<KWSTATE>"?"                   { return OPERATOR;  }
<KWSTATE>"'"                   { return OPERATOR;  }
<KWSTATE>"."                   { return OPERATOR; }
<KWSTATE>",@"                  { return OPERATOR;  }
<KWSTATE>","                   { return OPERATOR; }

<KWSTATE>{new_line}            { return NEWLINE;}
<KWSTATE>.                     { return PLAIN; }

{keyword}             { EXIT(); return KEYWORD; } 
{function}            { EXIT(); return TYPE; }
{atoms}               { return KEYWORD; } 

{character_literal}   { return CHARACTER; }                      
{integer_literal}     { return NUMBER; }
{real_literal}        { return NUMBER; }
{string_literal}      { return STRING; }

"("                   { ENTER(KWSTATE); return OPERATOR; }                     
")"                   { return OPERATOR;}                     
"."                   { return OPERATOR; }
"?"                   { return OTHER; }
"`"                   { ENTER(MACRO); return OPERATOR; }
"'"                   { return OPERATOR; }
",@"                  { return OPERATOR; }
","                   { return OPERATOR;}

{identifier}          { return IDENTIFIER; }

{new_line}            { return NEWLINE;}
.                     { return PLAIN; }

