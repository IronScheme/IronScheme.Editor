%{
#include "gram_IronPython.h" 

static int ppmode = 0;
static int docintag = 0;

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_IronPython.c"

single_line_comment    #[^\n]*

white_space            [ \t]

preprocessor           ^{white_space}*#{white_space}*

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

single_char            [^\']
simple_esc_seq         \\[\'\"\\0abfnrtv]
uni_esc_seq1           \\u{hex_digit}{4}
uni_esc_seq2           \\U{hex_digit}{8}
uni_esc_seq            {uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            \\x{hex_digit}{1,4}
character              {single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      \'{character}+\'


single_string_char     [^\\\"]
reg_string_char        {single_string_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         \"{reg_string_char}*\"
regular_string2        (r|R)\"({single_string_char}|\\)*\"
single_verbatim_char   [^\"\n]
verb_string_char       {single_verbatim_char}
string_literal         {regular_string}|{regular_string2}

verbatim_string_start  \"\"\"
verbatim_string_cont   {verb_string_char}+|\"
verbatim_string_end    \"\"\"

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*


rank_specifier         "["{white_space}*(","{white_space}*)*"]"

%x VERB_STRING

%%

{white_space}+    { ; /* ignore */ }

                      /***** Comments *****/
                      
{single_line_comment} { RETURN4(COMMENT); }


                      /***** Keywords *****/

"and"        {RETURN4(KW);}
"assert"              {RETURN4(KW);}
"break"            {RETURN4(KW);}
"class"            {RETURN4(KW);}
"continue"           {RETURN4(KW);}
"def"            {RETURN4(KW);}
"del"            {RETURN4(KW);}
"elif"           {RETURN4(KW);}
"else"            {RETURN4(KW);}
"except"         {RETURN4(KW);}
"exec"           {RETURN4(KW);}
"finally"           {RETURN4(KW);}
"for"        {RETURN4(KW);}
"from"        {RETURN4(KW);}
"global"         {RETURN4(KW);}
"if"         {RETURN4(KW);}
"import"        {RETURN4(KW);}
"in"              {RETURN4(KW);}
"is"          {RETURN4(KW);}
"lambda"            {RETURN4(KW);}
"not"            {RETURN4(KW);}
"or"           {RETURN4(KW);}
"pass"        {RETURN4(KW);}
"print"          {RETURN4(KW);}
"raise"           {RETURN4(KW);}
"return"         {RETURN4(KW);}
"try"           {RETURN4(KW);}
"while"           {RETURN4(KW);}
"yield"             {RETURN4(KW);}

"self"               {RETURN4(TYPE);}

                      /***** Literals *****/
                      
{verbatim_string_start}                 { ENTER(VERB_STRING); RETURN4(STRING); }

<VERB_STRING>
{
\n                         { RETURN4(NEWLINE); }
{verbatim_string_cont}     { RETURN4(STRING); }
{verbatim_string_end}      { EXIT(); RETURN4(STRING); }
}                                           
                
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(STRING); }
{string_literal}      { RETURN4(STRING); }


                      /*** Multi-Character Operators ***/

"+"     { RETURN4(OP); }
"-"     { RETURN4(OP); }
"**"    { RETURN4(OP); }   
"+="    { RETURN4(OP); }
"-="    { RETURN4(OP); }
"*="    { RETURN4(OP); }
"**="    { RETURN4(OP); }
"//"    { RETURN4(OP); }
"//="    { RETURN4(OP); }
"/="    { RETURN4(OP); }
"/"    { RETURN4(OP); }
"%"    { RETURN4(OP); }
"%="    { RETURN4(OP); }
"^="    { RETURN4(OP); }
"&="    { RETURN4(OP); }
"|="    { RETURN4(OP); }
"<<"    { RETURN4(OP); }
">>"    { RETURN4(OP); }
">>="   { RETURN4(OP); }
"<<="   { RETURN4(OP); }
"!="    { RETURN4(OP); }
"<="    { RETURN4(OP); }
">="    { RETURN4(OP); }

"_"   { RETURN4(OP); }
","   { RETURN4(OP); }
"."   { RETURN4(OP); }
";"   { RETURN4(OP); }
":"   { RETURN4(OP); }
"`"   { RETURN4(OP); }
"~"   { RETURN4(OP); }
"="   { RETURN4(OP); }
"["   { RETURN4(OP); }
"]"   { RETURN4(OP); }
"<"   { RETURN4(OP); }
">"   { RETURN4(OP); }
"("   { RETURN4(OP); }
")"   { RETURN4(OP); }
"{"   { RETURN4(OP); }
"}"   { RETURN4(OP); }
"<>"   { RETURN4(OP); }
"@"   { RETURN4(OP); }

                      /*** Those context-sensitive "keywords" ***/

{identifier}             { RETURN4(PLAIN); }

\n                       { RETURN4(NEWLINE);}
.                        { RETURN4(PLAIN); }
%%

 