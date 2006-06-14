%{
#include "gram_Nemerle.h" 

static int ppmode = 0;
static int docintag = 0;

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Nemerle.c"

doc_comment            "///"
single_line_comment    ("//"[^/\n].*)|"//"

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
verbatim_string        @\"{verb_string_char}*\"
string_literal         {regular_string}|{verbatim_string}

verbatim_string_start  @\"
verbatim_string_cont   {verb_string_char}* 
verbatim_string_end    \"

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*


rank_specifier         "["{white_space}*(","{white_space}*)*"]"


%x IN_COMMENT
%x VERB_STRING
%x PPTAIL
%x PREPROCESSOR
%x DOC_COMMENT

%%

{preprocessor}    { ENTER(PREPROCESSOR); RETURN4(PREPROC); }
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

{doc_comment}     { ENTER(DOC_COMMENT); RETURN4(DOCCOMMENT); }
{single_line_comment} { RETURN4(COMMENT); }

<DOC_COMMENT>
{
\n                { EXIT(); RETURN4(NEWLINE); }
"<"[^>\n]*        { docintag = 1; RETURN4(DOCCOMMENT);}
"<"[^>\n]*">"     { RETURN4(DOCCOMMENT);}
{white_space}+    { ; /* ignore */ }
">"               { RETURN4(DOCCOMMENT);}
[^<>\n]+          { if (docintag) {docintag = 0; RETURN4(DOCCOMMENT);} else RETURN4(COMMENT); }
}


<PPTAIL>
{
[^\n]+            { RETURN3(PREPROC,PPID); }
\n                { EXIT(); EXIT(); RETURN4(NEWLINE); }
}


<PREPROCESSOR>
{
"define"          { ENTER(PPTAIL); RETURN3(PREPROC,PPDEFINE); }
"if"              { ENTER(PPTAIL); RETURN3(PREPROC,PPIF); }
"else"            { ENTER(PPTAIL); RETURN3(PREPROC,PPELSE); }
"endif"           { ENTER(PPTAIL); RETURN3(PREPROC,PPENDIF); }
"line"            { ENTER(PPTAIL); RETURN4(PREPROC); }
"error"           { ENTER(PPTAIL); RETURN4(PREPROC); }
"warning"         { ENTER(PPTAIL); RETURN4(PREPROC); }
"region"          { ENTER(PPTAIL); RETURN3(PREPROC,PPREGION); }
"endregion"       { ENTER(PPTAIL); RETURN3(PREPROC,PPENDREGION); }
\n                { EXIT(); RETURN4(NEWLINE); }
.                 { RETURN4(ERROR); }
}

                      /***** Keywords *****/

"abstract"        {RETURN4(KW);}
"as"              {RETURN4(KW);}
"base"            {RETURN4(KW);}
"bool"            {RETURN4(KW);}
"break"           {RETURN4(KW);}
"byte"            {RETURN4(KW);}
"case"            {RETURN4(KW);}
"catch"           {RETURN4(KW);}
"char"            {RETURN4(KW);}
"const"           {RETURN4(KW);}
"and"             {RETURN4(KW);}
"class"           {RETURN4(KW);}
"array"           {RETURN4(KW);}
"continue"        {RETURN4(KW);}
"decimal"         {RETURN4(KW);}
"default"         {RETURN4(KW);}
"delegate"        {RETURN4(KW);}
"do"              {RETURN4(KW);}
"double"          {RETURN4(KW);}
"else"            {RETURN4(KW);}
"enum"            {RETURN4(KW);}
"event"           {RETURN4(KW);}
"explicit"        {RETURN4(KW);}
"extern"          {RETURN4(KW);}
"false"           {RETURN4(KW);}
"finally"         {RETURN4(KW);}
"regexp"          {RETURN4(KW);}
"float"           {RETURN4(KW);}
"for"             {RETURN4(KW);}
"foreach"         {RETURN4(KW);}
"goto"            {RETURN4(KW);}
"if"              {RETURN4(KW);}
"implicit"        {RETURN4(KW);}
"in"              {RETURN4(KW);}
"int"             {RETURN4(KW);}
"interface"       {RETURN4(KW);}
"internal"        {RETURN4(KW);}
"is"              {RETURN4(KW);}
"let"            {RETURN4(KW);}
"long"            {RETURN4(KW);}
"namespace"       {RETURN4(KW);}
"new"             {RETURN4(KW);}
"null"            {RETURN4(KW);}
"object"          {RETURN4(KW);}
"operator"        {RETURN4(KW);}
"out"             {RETURN4(KW);}
"override"        {RETURN4(KW);}
"macro"          {RETURN4(KW);}
"private"         {RETURN4(KW);}
"protected"       {RETURN4(KW);}
"public"          {RETURN4(KW);}
"syntax"        {RETURN4(KW);}
"ref"             {RETURN4(KW);}
"return"          {RETURN4(KW);}
"sbyte"           {RETURN4(KW);}
"sealed"          {RETURN4(KW);}
"short"           {RETURN4(KW);}
"repeat"          {RETURN4(KW);}
"match"      {RETURN4(KW);}
"static"          {RETURN4(KW);}
"string"          {RETURN4(KW);}
"struct"          {RETURN4(KW);}
"switch"          {RETURN4(KW);}
"this"            {RETURN4(KW);}
"throw"           {RETURN4(KW);}
"true"            {RETURN4(KW);}
"try"             {RETURN4(KW);}
"typeof"          {RETURN4(KW);}
"uint"            {RETURN4(KW);}
"ulong"           {RETURN4(KW);}
"module"       {RETURN4(KW);}
"mutable"          {RETURN4(KW);}
"ushort"          {RETURN4(KW);}
"using"           {RETURN4(KW);}
"virtual"         {RETURN4(KW);}
"void"            {RETURN4(KW);}
"volatile"        {RETURN4(KW);}
"while"           {RETURN4(KW);}            

"_"           {RETURN4(KW);}            
"ensure"           {RETURN4(KW);}            
"fun"           {RETURN4(KW);}            
"trymatch"           {RETURN4(KW);}            
"implements"           {RETURN4(KW);}            
"variant"           {RETURN4(KW);}            
"require"           {RETURN4(KW);}            

"then"           {RETURN4(KW);}            
"with"           {RETURN4(KW);}            
"option"           {RETURN4(KW);}            
"var"           {RETURN4(KW);}            
"lock"           {RETURN4(KW);}            
"def"           {RETURN4(KW);}            

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
{character_literal}   { RETURN4(CHARACTER); }
{string_literal}      { RETURN4(STRING); }

                      /*** Punctuation and Single-Character Operators ***/
                      
","   { RETURN4(OP); }
"["   { RETURN4(OP); }
"]"   { RETURN4(OP); }

{rank_specifier}     { RETURN4(OP); }

                      /*** Multi-Character Operators ***/
                      
"+="    { RETURN4(OP); }
"-="    { RETURN4(OP); }
"*="    { RETURN4(OP); }
"/="    { RETURN4(OP); }
"%="    { RETURN4(OP); }
"^="    { RETURN4(OP); }
"&="    { RETURN4(OP); }
"|="    { RETURN4(OP); }
"<<"    { RETURN4(OP); }
">>"    { RETURN4(OP); }
">>="   { RETURN4(OP); }
"<<="   { RETURN4(OP); }
"=="    { RETURN4(OP); }
"!="    { RETURN4(OP); }
"<="    { RETURN4(OP); }
">="    { RETURN4(OP); }
"&&"    { RETURN4(OP); }
"||"    { RETURN4(OP); }
"++"    { RETURN4(OP); }
"--"    { RETURN4(OP); }
"->"    { RETURN4(OP); }

                      /*** Those context-sensitive "keywords" ***/


"get"    { RETURN4(KW); }
"set"    { RETURN4(KW); }

{identifier}             { RETURN4(PLAIN); }
{at_identifier}          { RETURN4(PLAIN); }

\n                       { RETURN4(NEWLINE);}
.                        { RETURN4(PLAIN); }
%%

 