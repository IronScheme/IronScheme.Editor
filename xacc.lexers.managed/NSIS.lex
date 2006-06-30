using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class NSISLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "NSIS"; } }
	  public override string[] Extensions {get { return new string[]{"nsi"}; } }
	  LexerBase lexer = new NSISLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class NSISLexer

%full

line_comment           =(";"|"#").*

white_space            =[ \t]
new_line               =\n

preprocessor           =^({white_space})*"!"({white_space})*("define"|"insertmacro"|"include")

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

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\0abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
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

keyword               =(SectionSetText|SectionGetText|InstType|Page|PageExEnd|UninstPage|PageEx|PageCallbacks|LicenseText|LicenseData|LicenseForceSelection|Var|Name|XPStyle|OutFile|InstallDir|InstallDirRegKey|ShowInstDetails|ShowUnInstDetails|SetCompressor|LangString|Function|FunctionEnd|Push|Section|SectionEnd|SectionIn|ReadRegStr|Exch|Pop|IfFileExists|StrCpy|EnumRegKey|IntOp|StrCmp|StrCpy|Goto|Abort|Call|MessageBox|AddSize|SetOutPath|SetOverwrite|File|CreateDirectory|CreateShortCut|WriteUninstaller|WriteRegStr|HideWindow|Delete|RMDir|DeleteRegKey|SetAutoClose)

operator =[-~%^\*\(\)\+=\[\]\|\\:;,\./\?&<>\{\}]

other                 =("$"(({ident_char})+|"{"({ident_char})+"}"|"("({ident_char})+")"))

other2                =(HKLM|RO|true|false|lzma|bzip2|MB_ICONINFORMATION|MB_OK|MB_ICONQUESTION|MB_ICONEXCLAMATION|MB_YESNO|MB_DEFBUTTON2|ifnewer|HKEY_LOCAL_MACHINE|on|off|show|hide|IDYES|IDNO)

ml_string_char        =({single_string_char}|\\[\'\"0abfnrtv])

mlstart               =\"({ml_string_char})*\\
mlcont                =({ml_string_char})*\\
mlend                 =({ml_string_char})*\"
mlerror               =({ml_string_char})+

%state MLSTRING           

%%

{preprocessor}        { return Preprocessor(); }
{white_space}+        { ; }

<YYINITIAL>{mlstart}             { ENTER(MLSTRING); return String(); }

<MLSTRING>{mlcont}            { return String();}
<MLSTRING>{mlend}             { EXIT(); return String();}
<MLSTRING>{mlerror}           { EXIT(); return Error();}

{line_comment}        { return Comment(); }
                      
{keyword}             { return Keyword(); } 
{other2}              { return Character(); }
                      

{real_literal}        { return Number(); }
{character_literal}   { return Character(); }
{string_literal}      { return String(); }

{operator}            { return Operator(); }                     

{other}               { return Other(); } 
{integer_literal}     { return Number(); }

{identifier}          { return Identifier(); }

{new_line}            { return NewLine();}
.                     { return Plain(); }


