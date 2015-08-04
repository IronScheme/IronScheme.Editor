%{
#include "gram_Scheme.h" 

static inmacro = 0;
%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Scheme.c"

line_comment           ";"[^\n]*

comment_start          "#|"
comment_end            "|#"

white_space            [ \t]
new_line               \n

dec_digit              [0-9]
hex_digit              [0-9A-Fa-f]
int_suffix             [UuLl]|[Uu][Ll]|[Ll][Uu]
dec_literal            {dec_digit}+{int_suffix}?
hex_literal            0[xX]{hex_digit}+{int_suffix}?
integer_literal        {dec_literal}|{hex_literal}

real_suffix            [FfDdMm]
sign                   [+\-]
exponent_part          [eE]{sign}?{dec_digit}+
whole_real1            {dec_digit}+{exponent_part}{real_suffix}?
whole_real2            {dec_digit}+{real_suffix}
part_real              {dec_digit}*\.{dec_digit}+{exponent_part}?{real_suffix}?
real_literal           {whole_real1}|{whole_real2}|{part_real}

single_char            [^\\\']
simple_esc_seq         \\[\'\"\\0abfnrtv]
uni_esc_seq1           \\u{hex_digit}{4}
uni_esc_seq2           \\U{hex_digit}{8}
uni_esc_seq            {uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            \\x{hex_digit}{1,4}
character              {single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      #\\{character}

single_string_char     [^\\\"]
reg_string_char        {single_string_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         \"{reg_string_char}*\"
quote_esc_seq          \"\"
string_literal         {regular_string}

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"-"|"_"|"!"|"+"|"?"|"*"
identifier             ({letter_char}{ident_char}*)("[]")?|("*"{ident_char}+"*")

atoms     (null|true|false)
forms1    (and|backquote|call|compile|cond|do|each|define|lambda|for|for-each|else|loop|case)
forms2    (if|let|macro|or|quote|the|to|trace|try|when|while|with|"++"|"--"|begin|return)
func1     (apply|append|assoc|caaar|caadr|caar|cadar|caddr|cadr|car|cdar|cdaar|cddar|cdddr|cddr|cdr)
func2     (cons|environment|eq|eql|eval|evalstring|exit|first|import|inspect|is|length|list)
func3     (load|macroexpand|map|nconc|new|not|nth|pr|prl|read|readstring|reference|reset|reverse|rest|throw|typeof|using)
func4     ("+"|"="|"*"|"/"|"-"|">"|">="|"<"|"<="|"&"|"^"|"|"|"!="|"==")
builtin   (defun|defmacro|listp)

keyword               ({forms1}|{forms2}|{builtin})
function              ({func1}|{func2}|{func3}|{func4})


%x KWSTATE
%s MACRO
%x ML_COMMENT

%%

{white_space}+        { ; }
                      
{comment_start}   { ENTER(ML_COMMENT); RETURN4(COMMENT); }                      
{line_comment}        { RETURN4(COMMENT); }

"&body"               { RETURN4(OTHER); }
"&rest"               { RETURN4(OTHER); }

<MACRO>
{
",@"                  { RETURN4(OP); }
","                   { RETURN4(OP); }
"("                   { ENTER(KWSTATE); RETURN3(OP,LBRACE); }                     
")"                   { EXIT(); RETURN3(OP,RBRACE); }  

}

<ML_COMMENT>
{
{white_space}+    { ; }
{new_line}        { RETURN4(NEWLINE); }
[^\n\|]+         { RETURN4(COMMENT); }
{comment_end}     { EXIT(); RETURN4(COMMENT); }
"|"               { RETURN4(COMMENT); }
}

<KWSTATE>
{ 
{white_space}+        { ; }
                      
{line_comment}        { RETURN4(COMMENT); }

"&body"               { RETURN4(OTHER); }
"&rest"               { RETURN4(OTHER); }

{atoms}               { RETURN3(KW,LITERAL); } 
{keyword}             { EXIT(); RETURN3(KW,KEYWORD); } 
{function}            { EXIT(); RETURN3(TYPE,FUNCTION); }
{identifier}          { EXIT(); RETURN3(IDENTIFIER,IDENTIFIER); }
{character_literal}   { EXIT(); RETURN3(CHARACTER,LITERAL); }
{integer_literal}     { EXIT(); RETURN3(NUMBER,LITERAL); }
{real_literal}        { EXIT(); RETURN3(NUMBER,LITERAL); }
{string_literal}      { EXIT(); RETURN3(STRING,LITERAL); }

"("                   { ENTER(KWSTATE); RETURN3(OP,LBRACE); }                     
")"                   { RETURN3(OP,RBRACE); }                     
"`"                   { ENTER(MACRO); RETURN4(OP); }
"'"                   { RETURN4(OP); }
"."                   { RETURN4(OP); }
",@"                  { RETURN4(OP); }
","                   { RETURN4(OP); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }
} 

{keyword}             { EXIT(); RETURN3(KW,KEYWORD); } 
{function}            { EXIT(); RETURN3(TYPE,FUNCTION); }
{atoms}               { RETURN3(KW,LITERAL); } 

{character_literal}   { RETURN3(CHARACTER,LITERAL); }                      
{integer_literal}     { RETURN3(NUMBER,LITERAL); }
{real_literal}        { RETURN3(NUMBER,LITERAL); }
{string_literal}      { RETURN3(STRING,LITERAL); }
                  

"("                   { ENTER(KWSTATE); RETURN3(OP,LBRACE); }                     
")"                   { RETURN3(OP,RBRACE);}                     
"."                   { RETURN4(PAIR); }
"`"                   { ENTER(MACRO); RETURN4(OP); }
"'"                   { RETURN4(OP); }
",@"                  { RETURN4(OP); }
","                   { RETURN4(OP); }



{identifier}          { RETURN3(IDENTIFIER,IDENTIFIER); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }

%%
