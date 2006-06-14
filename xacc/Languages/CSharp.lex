#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class CSharpLanguage : CSLex.Language
  {
	  public override string Name {get {return "C#"; } }
	  public override string[] Extensions {get { return new string[]{"cs"}; } }
	  LexerBase lexer = new CSharpLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CSharpLexer

%unicode

%{
int docintag = 0;
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
string_esc_seq         =\\[\"\\abfnrtv]
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

%state ML_COMMENT
%state VERB_STRING
%state PREPROCESSOR
%state DOC_COMMENT
%state PPTAIL

%%

<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return PREPROC; }
<YYINITIAL>{white_space}+    { ; /* ignore */ }

                    
<YYINITIAL>{comment_start}   { ENTER(ML_COMMENT); return COMMENT; }


<ML_COMMENT>{white_space}+    { ; }
<ML_COMMENT>{new_line}        { return NEWLINE; }
<ML_COMMENT>[^*\n\t]+         { return COMMENT; }
<ML_COMMENT>"*"+[^*/\n\t]*    { return COMMENT; }
<ML_COMMENT>{comment_end}     { EXIT(); return COMMENT; }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return DOCCOMMENT; }
<YYINITIAL>{line_comment}    { return COMMENT; }

<DOC_COMMENT>{new_line}        { EXIT(); return NEWLINE; }
<DOC_COMMENT>"<"[^>\n]*        { docintag = 1; return DOCCOMMENT;}
<DOC_COMMENT>"<"[^>\n]*">"     { return DOCCOMMENT;}
<DOC_COMMENT>{white_space}+    { ; /* ignore */ }
<DOC_COMMENT>">"               { return DOCCOMMENT;}
<DOC_COMMENT>[^<>\n]+          { if (docintag == 1) {docintag = 0; return DOCCOMMENT;} else return COMMENT; }


<PPTAIL>[^\n]+            { return PREPROC; }
<PPTAIL>{new_line}        { EXIT(); EXIT(); return NEWLINE; }


<PREPROCESSOR>"define"          { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"if"              { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"else"            { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"endif"           { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"line"            { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"error"           { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"warning"         { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"region"          { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"endregion"       { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>{new_line}        { EXIT(); return NEWLINE; }
<PREPROCESSOR>.                 { return ERROR; }

<YYINITIAL>{at_identifier}   { return IDENTIFIER; }

<YYINITIAL>"abstract"        {return KEYWORD;}
<YYINITIAL>"as"              {return KEYWORD;}
<YYINITIAL>"base"            {return KEYWORD;}
<YYINITIAL>"bool"            {return KEYWORD;}
<YYINITIAL>"break"           {return KEYWORD;}
<YYINITIAL>"byte"            {return KEYWORD;}
<YYINITIAL>"case"            {return KEYWORD;}
<YYINITIAL>"catch"           {return KEYWORD;}
<YYINITIAL>"char"            {return KEYWORD;}
<YYINITIAL>"checked"         {return KEYWORD;}
<YYINITIAL>"class"           {return KEYWORD;}
<YYINITIAL>"const"           {return KEYWORD;}
<YYINITIAL>"continue"        {return KEYWORD;}
<YYINITIAL>"decimal"         {return KEYWORD;}
<YYINITIAL>"default"         {return KEYWORD;}
<YYINITIAL>"delegate"        {return KEYWORD;}
<YYINITIAL>"do"              {return KEYWORD;}
<YYINITIAL>"double"          {return KEYWORD;}
<YYINITIAL>"else"            {return KEYWORD;}
<YYINITIAL>"enum"            {return KEYWORD;}
<YYINITIAL>"event"           {return KEYWORD;}
<YYINITIAL>"explicit"        {return KEYWORD;}
<YYINITIAL>"extern"          {return KEYWORD;}
<YYINITIAL>"false"           {return KEYWORD;}
<YYINITIAL>"finally"         {return KEYWORD;}
<YYINITIAL>"fixed"           {return KEYWORD;}
<YYINITIAL>"float"           {return KEYWORD;}
<YYINITIAL>"for"             {return KEYWORD;}
<YYINITIAL>"foreach"         {return KEYWORD;}
<YYINITIAL>"goto"            {return KEYWORD;}
<YYINITIAL>"if"              {return KEYWORD;}
<YYINITIAL>"implicit"        {return KEYWORD;}
<YYINITIAL>"in"              {return KEYWORD;}
<YYINITIAL>"int"             {return KEYWORD;}
<YYINITIAL>"interface"       {return KEYWORD;}
<YYINITIAL>"internal"        {return KEYWORD;}
<YYINITIAL>"is"              {return KEYWORD;}
<YYINITIAL>"lock"            {return KEYWORD;}
<YYINITIAL>"long"            {return KEYWORD;}
<YYINITIAL>"namespace"       {return KEYWORD;}
<YYINITIAL>"new"             {return KEYWORD;}
<YYINITIAL>"null"            {return KEYWORD;}
<YYINITIAL>"object"          {return KEYWORD;}
<YYINITIAL>"operator"        {return KEYWORD;}
<YYINITIAL>"out"             {return KEYWORD;}
<YYINITIAL>"override"        {return KEYWORD;}
<YYINITIAL>"params"          {return KEYWORD;}
<YYINITIAL>"private"         {return KEYWORD;}
<YYINITIAL>"protected"       {return KEYWORD;}
<YYINITIAL>"public"          {return KEYWORD;}
<YYINITIAL>"readonly"        {return KEYWORD;}
<YYINITIAL>"ref"             {return KEYWORD;}
<YYINITIAL>"return"          {return KEYWORD;}
<YYINITIAL>"sbyte"           {return KEYWORD;}
<YYINITIAL>"sealed"          {return KEYWORD;}
<YYINITIAL>"short"           {return KEYWORD;}
<YYINITIAL>"sizeof"          {return KEYWORD;}
<YYINITIAL>"stackalloc"      {return KEYWORD;}
<YYINITIAL>"static"          {return KEYWORD;}
<YYINITIAL>"string"          {return KEYWORD;}
<YYINITIAL>"struct"          {return KEYWORD;}
<YYINITIAL>"switch"          {return KEYWORD;}
<YYINITIAL>"this"            {return KEYWORD;}
<YYINITIAL>"throw"           {return KEYWORD;}
<YYINITIAL>"true"            {return KEYWORD;}
<YYINITIAL>"try"             {return KEYWORD;}
<YYINITIAL>"typeof"          {return KEYWORD;}
<YYINITIAL>"uint"            {return KEYWORD;}
<YYINITIAL>"ulong"           {return KEYWORD;}
<YYINITIAL>"unchecked"       {return KEYWORD;}
<YYINITIAL>"unsafe"          {return KEYWORD;}
<YYINITIAL>"ushort"          {return KEYWORD;}
<YYINITIAL>"using"           {return KEYWORD;}
<YYINITIAL>"virtual"         {return KEYWORD;}
<YYINITIAL>"void"            {return KEYWORD;}
<YYINITIAL>"volatile"        {return KEYWORD;}
<YYINITIAL>"while"           {return KEYWORD;}
<YYINITIAL>"value"           {return KEYWORD;}   

<YYINITIAL>"partial"         {return KEYWORD;}
<YYINITIAL>"yield"           {return KEYWORD;}

                      
<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return STRING; }

<VERB_STRING>{new_line}                 { return NEWLINE; }
<VERB_STRING>{verbatim_string_cont}     { return STRING; }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return STRING; }
                      
<YYINITIAL>{integer_literal}     { return NUMBER; }
<YYINITIAL>{real_literal}        { return NUMBER; }
<YYINITIAL>{character_literal}   { return CHARACTER; }
<YYINITIAL>{string_literal}      { return STRING; }

<YYINITIAL>{rank_specifier}      { return OPERATOR; }

                      
<YYINITIAL>"+="    { return OPERATOR; }
<YYINITIAL>"-="    { return OPERATOR; }
<YYINITIAL>"*="    { return OPERATOR; }
<YYINITIAL>"/="    { return OPERATOR; }
<YYINITIAL>"%="    { return OPERATOR; }
<YYINITIAL>"^="    { return OPERATOR; }
<YYINITIAL>"&="    { return OPERATOR; }
<YYINITIAL>"|="    { return OPERATOR; }
<YYINITIAL>"<<"    { return OPERATOR; }
<YYINITIAL>">>"   	{ return OPERATOR; }
<YYINITIAL>">>="   { return OPERATOR; }
<YYINITIAL>"<<="   { return OPERATOR; }
<YYINITIAL>"=="    { return OPERATOR; }
<YYINITIAL>"!="    { return OPERATOR; }
<YYINITIAL>"<="    { return OPERATOR; }
<YYINITIAL>">="    { return OPERATOR; }
<YYINITIAL>"&&"    { return OPERATOR; }
<YYINITIAL>"||"    { return OPERATOR; }
<YYINITIAL>"++"    { return OPERATOR; }
<YYINITIAL>"--"    { return OPERATOR; }

<YYINITIAL>"->"    { return OPERATOR; }
<YYINITIAL>"."     { return OPERATOR; }
<YYINITIAL>"("     { return OPERATOR; }
<YYINITIAL>")"     { return OPERATOR; }
<YYINITIAL>"["     { return OPERATOR; }
<YYINITIAL>"]"     { return OPERATOR; }
<YYINITIAL>"{"     { return OPERATOR; }
<YYINITIAL>"}"     { return OPERATOR; }

<YYINITIAL>"+"     { return OPERATOR; }
<YYINITIAL>"-"     { return OPERATOR; }

<YYINITIAL>"="     { return OPERATOR; }
<YYINITIAL>";"     { return OPERATOR; }
<YYINITIAL>"!"     { return OPERATOR; }
<YYINITIAL>"?"     { return OPERATOR; }
<YYINITIAL>"*"     { return OPERATOR; }
<YYINITIAL>"%"     { return OPERATOR; }
<YYINITIAL>"^"     { return OPERATOR; }
<YYINITIAL>"&"     { return OPERATOR; }
<YYINITIAL>"/"     { return OPERATOR; }
<YYINITIAL>"|"     { return OPERATOR; }
<YYINITIAL>"<"     { return OPERATOR; }
<YYINITIAL>">"     { return OPERATOR; }
<YYINITIAL>"~"     { return OPERATOR; }
<YYINITIAL>":"     { return OPERATOR; }
<YYINITIAL>","     { return OPERATOR; }


<YYINITIAL>"get"   { return KEYWORD; }
<YYINITIAL>"set"   { return KEYWORD; }

<YYINITIAL>{error_string}           { return ERROR; }

<YYINITIAL>{identifier}             { return IDENTIFIER; }


<YYINITIAL>{new_line}               { return NEWLINE;}
<YYINITIAL>{attr}                   { return OPERATOR; }
<YYINITIAL>.                        { return ERROR; }
