using IronScheme.Editor.ComponentModel;
using System.Drawing;
using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class NemerleLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Nemerle"; } }
	  public override string[] Extensions {get { return new string[]{"n"}; } }
	  protected override LexerBase GetLexer() { return new NemerleLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class NemerleLexer

%full

%{

int ppmode = 0;
int docintag = 0;

public const int PPSTART=213;
public const int PPDEFINE=214;
public const int PPIF=215;
public const int PPELSE=216;
public const int PPENDIF=217;
public const int PPREGION=218;
public const int PPENDREGION=219;
public const int PPID=220;
public const int PPELIF=221;

%}



doc_comment            ="///"
line_comment           =("//"[^/\n].*)|"//"

comment_start          ="/*"
comment_end            ="*"+"/"

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
ident_char             ={dec_digit}|{letter_char}|"_"|"@"
identifier             =({letter_char}|"_"|"@")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

%state ML_COMMENT
%state VERB_STRING
%state PREPROCESSOR
%state DOC_COMMENT
%state PPTAIL

%%

{white_space}+                { ; /* ignore */ }


<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return Preprocessor(); }
<YYINITIAL>{comment_start}   { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^*\n\t]+         { return Comment(); }
<ML_COMMENT>"*"+[^*/\n\t]*    { return Comment(); }
<ML_COMMENT>{comment_end}     { EXIT(); return Comment(); }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return DocComment(); }
<YYINITIAL>{line_comment}    { return Comment(); }

<DOC_COMMENT>{new_line}        { EXIT(); return NewLine(); }
<DOC_COMMENT>"<"[^>\n]*        { docintag = 1; return DocComment();}
<DOC_COMMENT>"<"[^>\n]*">"     { return DocComment();}
<DOC_COMMENT>{white_space}+    { ; /* ignore */ }
<DOC_COMMENT>">"               { return DocComment();}
<DOC_COMMENT>[^<>\n]+          { if (docintag == 1) {docintag = 0; return DocComment();} else return Comment(); }

<PPTAIL>[^\n]+            { return Preprocessor( PPID); }
<PPTAIL>{new_line}        { EXIT(); EXIT(); return NewLine(); }

<PREPROCESSOR>"define"          { ENTER(PPTAIL); return Preprocessor(PPDEFINE); }
<PREPROCESSOR>"if"              { ENTER(PPTAIL); return Preprocessor(PPIF); }
<PREPROCESSOR>"else"            { ENTER(PPTAIL); return Preprocessor(PPELSE); }
<PREPROCESSOR>"elif"            { ENTER(PPTAIL); return Preprocessor(PPELIF); }
<PREPROCESSOR>"endif"           { ENTER(PPTAIL); return Preprocessor(PPENDIF); }
<PREPROCESSOR>"line"            { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"pragma"          { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"error"           { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"warning"         { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"region"          { ENTER(PPTAIL); return Preprocessor(PPREGION); }
<PREPROCESSOR>"endregion"       { ENTER(PPTAIL); return Preprocessor(PPENDREGION); }
<PREPROCESSOR>{new_line}        { EXIT(); return NewLine(); }
<PREPROCESSOR>.                 { return Error(); }

"abstract"        {return Keyword();}
"as"              {return Keyword();}
"base"            {return Keyword();}
"bool"            {return Keyword();}
"break"           {return Keyword();}
"byte"            {return Keyword();}
"case"            {return Keyword();}
"catch"           {return Keyword();}
"char"            {return Keyword();}
"const"           {return Keyword();}
"and"             {return Keyword();}
"class"           {return Keyword();}
"array"           {return Keyword();}
"continue"        {return Keyword();}
"decimal"         {return Keyword();}
"default"         {return Keyword();}
"delegate"        {return Keyword();}
"do"              {return Keyword();}
"double"          {return Keyword();}
"else"            {return Keyword();}
"enum"            {return Keyword();}
"event"           {return Keyword();}
"explicit"        {return Keyword();}
"extern"          {return Keyword();}
"false"           {return Keyword();}
"finally"         {return Keyword();}
"regexp"          {return Keyword();}
"float"           {return Keyword();}
"for"             {return Keyword();}
"foreach"         {return Keyword();}
"goto"            {return Keyword();}
"if"              {return Keyword();}
"implicit"        {return Keyword();}
"in"              {return Keyword();}
"int"             {return Keyword();}
"interface"       {return Keyword();}
"internal"        {return Keyword();}
"is"              {return Keyword();}
"let"            {return Keyword();}
"long"            {return Keyword();}
"namespace"       {return Keyword();}
"new"             {return Keyword();}
"null"            {return Keyword();}
"object"          {return Keyword();}
"operator"        {return Keyword();}
"out"             {return Keyword();}
"override"        {return Keyword();}
"macro"          {return Keyword();}
"private"         {return Keyword();}
"protected"       {return Keyword();}
"public"          {return Keyword();}
"syntax"        {return Keyword();}
"ref"             {return Keyword();}
"return"          {return Keyword();}
"sbyte"           {return Keyword();}
"sealed"          {return Keyword();}
"short"           {return Keyword();}
"repeat"          {return Keyword();}
"match"      {return Keyword();}
"static"          {return Keyword();}
"string"          {return Keyword();}
"struct"          {return Keyword();}
"switch"          {return Keyword();}
"this"            {return Keyword();}
"throw"           {return Keyword();}
"true"            {return Keyword();}
"try"             {return Keyword();}
"typeof"          {return Keyword();}
"uint"            {return Keyword();}
"ulong"           {return Keyword();}
"module"       {return Keyword();}
"mutable"          {return Keyword();}
"ushort"          {return Keyword();}
"using"           {return Keyword();}
"virtual"         {return Keyword();}
"void"            {return Keyword();}
"volatile"        {return Keyword();}
"while"           {return Keyword();}
"when"           {return Keyword();}
"ignore"           {return Keyword();}
"unless"           {return Keyword();}            

"_"           {return Keyword();}            
"ensure"           {return Keyword();}            
"fun"           {return Keyword();}            
"trymatch"        {return Keyword();}            
"implements"           {return Keyword();}            
"variant"           {return Keyword();}            
"require"           {return Keyword();}            

"then"           {return Keyword();}            
"with"           {return Keyword();}            
"option"           {return Keyword();}            
"var"           {return Keyword();}            
"lock"           {return Keyword();}            
"def"      {return Keyword();}            

{verbatim_string_start}                 { ENTER(VERB_STRING); return String(); }

<VERB_STRING>{verbatim_string_cont}     { return String(); }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return String(); }
                      
{integer_literal}     { return Number(); }
{real_literal}        { return Number(); }
{character_literal}   { return Character(); }
{string_literal}      { return String(); }

","   { return Operator(); }
"["   { return Operator(); }
"]"   { return Operator(); }

{rank_specifier}     { return Operator(); }

"+="    { return Operator(); }
"-="    { return Operator(); }
"*="    { return Operator(); }
"/="    { return Operator(); }
"%="    { return Operator(); }
"^="    { return Operator(); }
"&="    { return Operator(); }
"|="    { return Operator(); }
"<<"    { return Operator(); }
">>"    { return Operator(); }
">>="   { return Operator(); }
"<<="   { return Operator(); }
"=="    { return Operator(); }
"!="    { return Operator(); }
"<="    { return Operator(); }
">="    { return Operator(); }
"&&"    { return Operator(); }
"||"    { return Operator(); }
"++"    { return Operator(); }
"--"    { return Operator(); }
"->"    { return Operator(); }


"get"    { return Keyword(); }
"set"    { return Keyword(); }

@[^ \t\n]+               { return Identifier(); }

{identifier}             { return Identifier(); }

\n                            { return NewLine();}

.                        { return Plain(); }

 