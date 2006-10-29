using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class BooLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Boo"; } }
	  public override string[] Extensions {get { return new string[]{"boo"}; } }
	  protected override LexerBase GetLexer() { return new BooLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class BooLexer

%unicode

%{
int docintag = 0;
%}

doc_comment            ="///"
lc                     =^({white_space})*#{white_space}[^\n\t]*
single_line_comment    =("//"[^/\t\n].*)|"//"|(#{white_space}[^\n\t]*)

white_space            =[ \t]

preprocessor           =^({white_space})*#

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

verbatim_string_start  =\"\"\"
verbatim_string_cont   =({verb_string_char})* 
verbatim_string_end    =\"\"\"

regex_string_start     =\\"/"
regex_string_cont      =((\\"/")|([^/\n\t ]))+
regex_string_end       ="/"

letter_char            =[A-Za-z]
ident_char             ={dec_digit}|{letter_char}|"_"|"@"
identifier             =({letter_char}|"_")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

%state IN_COMMENT
%state VERB_STRING
%state REGEX_STRING
%state PPTAIL
%state PREPROCESSOR
%state DOC_COMMENT

%%


({white_space})+    { ; /* ignore */ }

<YYINITIAL>"/*"              { ENTER(IN_COMMENT); return Comment(); }

<IN_COMMENT>[^*\n]*           { return Comment(); }
<IN_COMMENT>"*"+[^*/\n]*      { return Comment(); }
<IN_COMMENT>"*"+"/"           { EXIT(); return Comment(); }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return DocComment(); }
<YYINITIAL>{lc}               { return Comment(); }
<YYINITIAL>{single_line_comment} { return Comment(); }
<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return Preprocessor(); }

<YYINITIAL>{regex_string_start}    { ENTER(REGEX_STRING); return Other(); }

<DOC_COMMENT>\n                { EXIT(); return NewLine(); }
<DOC_COMMENT>"<"[^>\n\t]*        { docintag = 1; return DocComment();}
<DOC_COMMENT>"<"[^>\n\t]*">"     { return DocComment();}
<DOC_COMMENT>{white_space}+    { ; /* ignore */ }
<DOC_COMMENT>">"               { return DocComment();}
<DOC_COMMENT>[^<>\n\t]+          { if (docintag == 1) {docintag = 0; return DocComment();} else return Comment(); }

<PPTAIL>[^\n]+            { return Preprocessor(); }
<PPTAIL>\n                { EXIT(); EXIT(); return NewLine(); }

<PREPROCESSOR>"define"          { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"if"              { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"else"            { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"endif"           { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"line"            { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"error"           { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"warning"         { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"region"          { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>"endregion"       { ENTER(PPTAIL); return Preprocessor(); }
<PREPROCESSOR>\n                { EXIT(); return NewLine(); }
<PREPROCESSOR>.                 { return Error(); }

<REGEX_STRING>{regex_string_end}    { EXIT(); return Other(); }
<REGEX_STRING>{regex_string_cont}   { return Other();}

"do"		{return Keyword(); }
"while"		{return Keyword(); }
"in"		{return Keyword(); }
"for"		{return Keyword(); }
"import"		{return Keyword(); }
"def"		{return Keyword(); }
"and"		{return Keyword(); }
"as"		{return Keyword(); }
"is"		{return Keyword(); }
"elif"		{return Keyword(); }
"print"		{return Keyword(); }
"assert"		{return Keyword(); }
"constructor"		{return Keyword(); }
"callable"		{return Keyword(); }
"get"		{return Keyword(); }
"set"		{return Keyword(); }
"cast"		{return Keyword(); }
"ensure"		{return Keyword(); }
"failure"		{return Keyword(); }
"final"		{return Keyword(); }
"typeof"		{return Keyword(); }
"from"		{return Keyword(); }
"given"		{return Keyword(); }
"isa"		{return Keyword(); }
"not"		{return Keyword(); }
"or"		{return Keyword(); }
"otherwise"		{return Keyword(); }
"pass"		{return Keyword(); }
"raise"		{return Keyword(); }
"retry"		{return Keyword(); }
"self"		{return Keyword(); }
"super"		{return Keyword(); }
"success"		{return Keyword(); }
"transient"		{return Keyword(); }
"return"		{return Keyword(); }
"break"		{return Keyword(); }
"goto"		{return Keyword(); }
"continue"		{return Keyword(); }
"if"		{return Keyword(); }
"else"		{return Keyword(); }
"unless"		{return Keyword(); }
"when"		{return Keyword(); }
"yield"		{return Keyword(); }
"using"		{return Keyword(); }
"try"		{return Keyword(); }
"except"		{return Keyword(); }
"namespace"		{return Keyword(); }
"class"		{return Keyword(); }
"struct"		{return Keyword(); }
"enum"		{return Keyword(); }
"interface"		{return Keyword(); }
"event"		{return Keyword(); }
"int"		{return Keyword(); }
"long"		{return Keyword(); }
"short"		{return Keyword(); }
"byte"		{return Keyword(); }
"bool"		{return Keyword(); }
"char"		{return Keyword(); }
"decimal" 		{return Keyword(); }
"uint"		{return Keyword(); }
"ulong"		{return Keyword(); }
"ushort"		{return Keyword(); }
"sbyte"		{return Keyword(); }
"string"		{return Keyword(); }
"object"		{return Keyword(); }
"float"		{return Keyword(); }
"double"		{return Keyword(); }
"void"		{return Keyword(); }
"true"		{return Keyword(); }
"false"		{return Keyword(); }
"null"		{return Keyword(); }
"public"		{return Keyword(); }
"internal"		{return Keyword(); }
"private"		{return Keyword(); }
"protected"		{return Keyword(); }
"abstract"		{return Keyword(); }
"virtual"		{return Keyword(); }
"override"		{return Keyword(); }
"static"		{return Keyword(); }
       

<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return String(); }


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


{identifier}             { return Identifier(); }
{at_identifier}          { return Identifier(); }

\n                       { return NewLine(); }
[^#]                     { return Plain(); }

 