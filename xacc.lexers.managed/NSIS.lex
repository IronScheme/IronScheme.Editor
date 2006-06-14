%{
#include "gram_NSIS.h" 
%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_NSIS.c"

line_comment           (";"|"#").*

white_space            [ \t]
new_line               \n

preprocessor           ^{white_space}*"!"{white_space}*("define"|"insertmacro"|"include")

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
simple_esc_seq         \\[\'\"0abfnrtv]|\\
uni_esc_seq1           \\u{hex_digit}{4}
uni_esc_seq2           \\U{hex_digit}{8}
uni_esc_seq            {uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            \\x{hex_digit}{1,4}
character              {single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      \'{character}\'

single_string_char     [^\\\"\n]
reg_string_char        {single_string_char}|{simple_esc_seq}
ml_string_char         {single_string_char}|\\[\'\"0abfnrtv]
regular_string         \"{reg_string_char}*\"
quote_esc_seq          \"\"
string_literal         {regular_string}

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*

rank_specifier         "["{white_space}*(","{white_space}*)*"]"

keyword               (SectionSetText|SectionGetText|InstType|Page|PageExEnd|UninstPage|PageEx|PageCallbacks|LicenseText|LicenseData|LicenseForceSelection|Var|Name|XPStyle|OutFile|InstallDir|InstallDirRegKey|ShowInstDetails|ShowUnInstDetails|SetCompressor|LangString|Function|FunctionEnd|Push|Section|SectionEnd|SectionIn|ReadRegStr|Exch|Pop|IfFileExists|StrCpy|EnumRegKey|IntOp|StrCmp|StrCpy|Goto|Abort|Call|MessageBox|AddSize|SetOutPath|SetOverwrite|File|CreateDirectory|CreateShortCut|WriteUninstaller|WriteRegStr|HideWindow|Delete|RMDir|DeleteRegKey|SetAutoClose)

operator              [\~\!\%\^\*\(\)\-\+\=\[\]\|\\\:\;\,\.\/\?\&\<\>]

other                 "$"({ident_char}+|"{"{ident_char}+"}"|"("{ident_char}+")")

other2                (HKLM|RO|true|false|lzma|bzip2|MB_ICONINFORMATION|MB_OK|MB_ICONQUESTION|MB_ICONEXCLAMATION|MB_YESNO|MB_DEFBUTTON2|ifnewer|HKEY_LOCAL_MACHINE|on|off|show|hide|IDYES|IDNO)

mlstart               \"{ml_string_char}*\\
mlcont                {ml_string_char}*\\
mlend                 {ml_string_char}*\"
mlerror               {ml_string_char}+

%x MLSTRING           

%%

{preprocessor}        { RETURN4(PREPROC); }
{white_space}+        { ; }

{mlstart}             { ENTER(MLSTRING); RETURN4(STRING); }

<MLSTRING>
{
  {mlcont}            { RETURN4(STRING);}
  {mlend}             { EXIT(); RETURN4(STRING);}
  {new_line}          { RETURN4(NEWLINE);}
  {mlerror}           { EXIT(); RETURN4(ERROR);}
}            

{line_comment}        { RETURN4(COMMENT); }
                      
{keyword}             { RETURN4(KW); } 
{other2}              { RETURN4(CHARACTER); }
                      

{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(CHARACTER); }
{string_literal}      { RETURN4(STRING); }

{operator}            { RETURN4(OP); }                     

{other}               { RETURN4(OTHER); } 
{integer_literal}     { RETURN4(NUMBER); }

{identifier}          { RETURN4(PLAIN); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }
%%
