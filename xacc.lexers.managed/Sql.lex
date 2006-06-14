%{
#include "gram_Sql.h" 
%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option case-insensitive
%option outfile="gram_Sql.c"


white_space            [ \t]

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
character              {single_char}*|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      \'{character}\'


single_string_char     [^\\\"]
reg_string_char        {single_string_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         \"{reg_string_char}*\"
single_verbatim_char   [^\"\n]
quote_esc_seq          \"\"
verb_string_char       {single_verbatim_char}|{quote_esc_seq}
verbatim_string        @\"{verb_string_char}*\"
string_literal         {regular_string}|{verbatim_string}

verbatim_string_start  \"\"\"
verbatim_string_cont   {verb_string_char}* 
verbatim_string_end    \"\"\"

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*
brace_identifier       \[[^\n\]]+\]

rank_specifier         "["{white_space}*(","{white_space}*)*"]"


%x IN_COMMENT


%%

{white_space}+    { ; /* ignore */ }

                      /***** Comments *****/
                      
"/*"              { ENTER(IN_COMMENT); RETURN4(COMMENT); }

<IN_COMMENT>
{
\n                { RETURN4(NEWLINE); }
[^*\n]*           { RETURN4(COMMENT); }
"*"+[^*/\n]*      { RETURN4(COMMENT); }
"*"+"/"           { EXIT(); RETURN4(COMMENT); }
}

                      /***** Keywords *****/
                      
"SELECT"      {RETURN4(KW);}
"FROM"          {RETURN4(KW);}
"THEN"      {RETURN4(KW);}
"BEGIN"       {RETURN4(KW);}
"END"       {RETURN4(KW);}
"AS"       {RETURN4(KW);}
"AND"       {RETURN4(KW);}
"ON"       {RETURN4(KW);}
"OR"       {RETURN4(KW);}
"WHERE"       {RETURN4(KW);}
"ORDER BY"       {RETURN4(KW);}
"GROUP BY"       {RETURN4(KW);}
"CASE"       {RETURN4(KW);}
"INNER"       {RETURN4(KW);}
"OUTER"       {RETURN4(KW);}
"CROSS"       {RETURN4(KW);}
"JOIN"       {RETURN4(KW);}
"LEFT"       {RETURN4(KW);}
"RIGHT"       {RETURN4(KW);}
"RETURN"       {RETURN4(KW);}
"GO"        {RETURN4(KW);}
"DECLARE"   {RETURN4(KW);}
"SET"       {RETURN4(KW);}
"ELSE"      {RETURN4(KW);}
"WHEN"      {RETURN4(KW);}
"CREATE"    {RETURN4(KW);}
"ALTER"      {RETURN4(KW);}
"TABLE"      {RETURN4(KW);}


                      /***** Types *****/
"DATETIME"  {RETURN4(KW);}
"CHAR"      {RETURN4(KW);}
"VARCHAR"   {RETURN4(KW);}
"NVARCHAR"  {RETURN4(KW);}


                      /***** Functions *****/
"ISNULL"  {RETURN4(KW);}
"SUM"     {RETURN4(KW);}
"DATEADD"     {RETURN4(KW);}
"CONVERT"     {RETURN4(KW);}
"LTRIM"     {RETURN4(KW);}
"RTRIM"     {RETURN4(KW);}
"CHARINDEX"     {RETURN4(KW);}
"UPPER"     {RETURN4(KW);}
"LOWER"     {RETURN4(KW);}
"MAX"     {RETURN4(KW);}
"MIN"      {RETURN4(KW);}
"REPLICATE" {RETURN4(KW);}
"LEN" {RETURN4(KW);}
"GETDATE" {RETURN4(KW);}



                      /***** Literals *****/
                      
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(STRING); }
{string_literal}      { RETURN4(STRING); }

{brace_identifier}    { RETURN4(TYPE); }
{at_identifier}       { RETURN4(OTHER); }
{identifier}          { RETURN4(IDENTIFIER); }

"("                   { RETURN4(OP); }
")"                   { RETURN4(OP); }
"="                   { RETURN4(OP); }
"<"                   { RETURN4(OP); }
">"                   { RETURN4(OP); }
"."                   { RETURN4(OP); }
"+"                   { RETURN4(OP); }
"/"                   { RETURN4(OP); }
"-"                   { RETURN4(OP); }
"*"                   { RETURN4(OP); }
","                   { RETURN4(OP); }

\n                       { RETURN4(NEWLINE);}
.                        { RETURN4(ERROR); }
%%

 