%{
#include "gram_Boo.h" 

static int ppmode = 0;
static int docintag = 0;

%}


%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Boo.c"

doc_comment            "///"
single_line_comment    ("//"[^/\n].*)|"//"|(#{white_space}[^\n]*)

white_space            [ \t]

preprocessor           ^{white_space}*#[^ \t\n]

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

verbatim_string_start  \"\"\"
verbatim_string_cont   {verb_string_char}* 
verbatim_string_end    \"\"\"

regex_string_start     \\"/"
regex_string_cont      ((\\"/")|([^/\n\t ]))+
regex_string_end       "/"

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*


rank_specifier         "["{white_space}*(","{white_space}*)*"]"


%x IN_COMMENT
%x VERB_STRING
%x REGEX_STRING
%x PPTAIL
%x PREPROCESSOR
%x DOC_COMMENT

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

{doc_comment}     { ENTER(DOC_COMMENT); RETURN4(DOCCOMMENT); }
{single_line_comment} { RETURN4(COMMENT); }
{preprocessor}    { ENTER(PREPROCESSOR); yyless(1); RETURN4(PREPROC); }

{regex_string_start}    { ENTER(REGEX_STRING); RETURN4(OTHER); }

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

<REGEX_STRING>
{
{regex_string_end}    { EXIT(); RETURN4(OTHER); }
{white_space}+        { ; /* ignore */ }
\n                    { RETURN4(NEWLINE); }
{regex_string_cont}   { RETURN4(OTHER);}
}

                      /***** Keywords *****/

"do"		{RETURN4(KW);}
"while"		{RETURN4(KW);}
"in"		{RETURN4(KW);}
"for"		{RETURN4(KW);}
"import"		{RETURN4(KW);}
"def"		{RETURN4(KW);}
"and"		{RETURN4(KW);}
"as"		{RETURN4(KW);}
"is"		{RETURN4(KW);}
"elif"		{RETURN4(KW);}
"print"		{RETURN4(KW);}
"assert"		{RETURN4(KW);}
"constructor"		{RETURN4(KW);}
"callable"		{RETURN4(KW);}
"get"		{RETURN4(KW);}
"set"		{RETURN4(KW);}
"cast"		{RETURN4(KW);}
"ensure"		{RETURN4(KW);}
"failure"		{RETURN4(KW);}
"final"		{RETURN4(KW);}
"typeof"		{RETURN4(KW);}
"from"		{RETURN4(KW);}
"given"		{RETURN4(KW);}
"isa"		{RETURN4(KW);}
"not"		{RETURN4(KW);}
"or"		{RETURN4(KW);}
"otherwise"		{RETURN4(KW);}
"pass"		{RETURN4(KW);}
"raise"		{RETURN4(KW);}
"retry"		{RETURN4(KW);}
"self"		{RETURN4(KW);}
"super"		{RETURN4(KW);}
"success"		{RETURN4(KW);}
"transient"		{RETURN4(KW);}
"return"		{RETURN4(KW);}
"break"		{RETURN4(KW);}
"goto"		{RETURN4(KW);}
"continue"		{RETURN4(KW);}
"if"		{RETURN4(KW);}
"else"		{RETURN4(KW);}
"unless"		{RETURN4(KW);}
"when"		{RETURN4(KW);}
"yield"		{RETURN4(KW);}
"using"		{RETURN4(KW);}
"try"		{RETURN4(KW);}
"except"		{RETURN4(KW);}
"namespace"		{RETURN4(KW);}
"class"		{RETURN4(KW);}
"struct"		{RETURN4(KW);}
"enum"		{RETURN4(KW);}
"interface"		{RETURN4(KW);}
"event"		{RETURN4(KW);}
"int"		{RETURN4(KW);}
"long"		{RETURN4(KW);}
"short"		{RETURN4(KW);}
"byte"		{RETURN4(KW);}
"bool"		{RETURN4(KW);}
"char"		{RETURN4(KW);}
"decimal" 		{RETURN4(KW);}
"uint"		{RETURN4(KW);}
"ulong"		{RETURN4(KW);}
"ushort"		{RETURN4(KW);}
"sbyte"		{RETURN4(KW);}
"string"		{RETURN4(KW);}
"object"		{RETURN4(KW);}
"float"		{RETURN4(KW);}
"double"		{RETURN4(KW);}
"void"		{RETURN4(KW);}
"true"		{RETURN4(KW);}
"false"		{RETURN4(KW);}
"null"		{RETURN4(KW);}
"public"		{RETURN4(KW);}
"internal"		{RETURN4(KW);}
"private"		{RETURN4(KW);}
"protected"		{RETURN4(KW);}
"abstract"		{RETURN4(KW);}
"virtual"		{RETURN4(KW);}
"override"		{RETURN4(KW);}
"static"		{RETURN4(KW);}
       

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


{identifier}             { RETURN4(PLAIN); }
{at_identifier}          { RETURN4(PLAIN); }

\n                       { RETURN4(NEWLINE);}
[^#]                     { RETURN4(PLAIN); }
%%

 