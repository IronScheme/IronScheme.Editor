%{
#include "gram_Lua.h"
%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Lua.c"


keyword             (and|break|do|else|elseif|end|false|for|function|if|in|local|nil|not|or|repeat|return|then|true|until|while)
operator            ("..."|".."|"."|"=="|"="|"<="|"<"|">="|">"|"~="|"~"|"("|")"|"["|"]"|","|"+"|"-"|"*"|"/"|"{"|"}")
comment_start       "--[["
comment_end         "]]"

line_comment        "--"[^\n]*

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
character_literal      \'{character}\'

single_string_char     [^\\\"]
reg_string_char        {single_string_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         \"{reg_string_char}*\"
single_verbatim_char   [^\"\n]
quote_esc_seq          \"\"
verb_string_char       {single_verbatim_char}|{quote_esc_seq}
verbatim_string        "[["{verb_string_char}*"]]"
string_literal         {regular_string}|{verbatim_string}

verbatim_string_start  "[["
verbatim_string_cont   {verb_string_char}* 
verbatim_string_end    "]]"

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*

%x ML_COMMENT
%x VERB_STRING

%%

{verbatim_string_start}    { ENTER(VERB_STRING); RETURN4(STRING); }

<VERB_STRING>
{
{new_line}                 { RETURN4(NEWLINE); }
{verbatim_string_end}      { EXIT(); RETURN4(STRING); }
{verbatim_string_cont}     { RETURN4(STRING); }
}

{white_space}+        { ; }
                      
{comment_start}       { ENTER(ML_COMMENT); RETURN4(COMMENT); }

<ML_COMMENT>
{
{comment_end}         { EXIT(); RETURN4(COMMENT); }
{new_line}            { RETURN4(NEWLINE); }
[^\]\n]*              { RETURN4(COMMENT); }
"]"+[^\]\n]*          { RETURN4(COMMENT); }
}

{line_comment}        { RETURN4(COMMENT); }
                      
{keyword}             { RETURN4(KW); } 
                      
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(CHARACTER); }
{string_literal}      { RETURN4(STRING); }

{operator}            { RETURN4(OP); }                     

{identifier}          { RETURN4(PLAIN); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }

%%

    