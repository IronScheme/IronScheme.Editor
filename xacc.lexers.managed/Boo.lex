using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class BooLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Boo"; } }
	  public override string[] Extensions {get { return new string[]{"boo"}; } }
	  LexerBase lexer = new BooLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
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
single_line_comment    =("//"[^/\n].*)|"//"|(#{white_space}[^\n]*)

white_space            =[ \t]

preprocessor           =^{white_space}*#

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

<YYINITIAL>"/*"              { ENTER(IN_COMMENT); return COMMENT; }

<IN_COMMENT>[^*\n]*           { return COMMENT; }
<IN_COMMENT>"*"+[^*/\n]*      { return COMMENT; }
<IN_COMMENT>"*"+"/"           { EXIT(); return COMMENT; }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return DOCCOMMENT; }
<YYINITIAL>{single_line_comment} { return COMMENT; }
<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return PREPROC; }

<YYINITIAL>{regex_string_start}    { ENTER(REGEX_STRING); return OTHER; }

<DOC_COMMENT>\n                { EXIT(); return NEWLINE; }
<DOC_COMMENT>"<"[^>\n]*        { docintag = 1; return DOCCOMMENT;}
<DOC_COMMENT>"<"[^>\n]*">"     { return DOCCOMMENT;}
<DOC_COMMENT>{white_space}+    { ; /* ignore */ }
<DOC_COMMENT>">"               { return DOCCOMMENT;}
<DOC_COMMENT>[^<>\n]+          { if (docintag == 1) {docintag = 0; return DOCCOMMENT;} else return COMMENT; }

<PPTAIL>[^\n]+            { return PREPROC; }
<PPTAIL>\n                { EXIT(); EXIT(); return NEWLINE; }

<PREPROCESSOR>"define"          { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"if"              { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"else"            { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"endif"           { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"line"            { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"error"           { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"warning"         { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"region"          { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>"endregion"       { ENTER(PPTAIL); return PREPROC; }
<PREPROCESSOR>\n                { EXIT(); return NEWLINE; }
<PREPROCESSOR>.                 { return ERROR; }

<REGEX_STRING>{regex_string_end}    { EXIT(); return OTHER; }
<REGEX_STRING>{regex_string_cont}   { return OTHER;}

"do"		{return KEYWORD; }
"while"		{return KEYWORD; }
"in"		{return KEYWORD; }
"for"		{return KEYWORD; }
"import"		{return KEYWORD; }
"def"		{return KEYWORD; }
"and"		{return KEYWORD; }
"as"		{return KEYWORD; }
"is"		{return KEYWORD; }
"elif"		{return KEYWORD; }
"print"		{return KEYWORD; }
"assert"		{return KEYWORD; }
"constructor"		{return KEYWORD; }
"callable"		{return KEYWORD; }
"get"		{return KEYWORD; }
"set"		{return KEYWORD; }
"cast"		{return KEYWORD; }
"ensure"		{return KEYWORD; }
"failure"		{return KEYWORD; }
"final"		{return KEYWORD; }
"typeof"		{return KEYWORD; }
"from"		{return KEYWORD; }
"given"		{return KEYWORD; }
"isa"		{return KEYWORD; }
"not"		{return KEYWORD; }
"or"		{return KEYWORD; }
"otherwise"		{return KEYWORD; }
"pass"		{return KEYWORD; }
"raise"		{return KEYWORD; }
"retry"		{return KEYWORD; }
"self"		{return KEYWORD; }
"super"		{return KEYWORD; }
"success"		{return KEYWORD; }
"transient"		{return KEYWORD; }
"return"		{return KEYWORD; }
"break"		{return KEYWORD; }
"goto"		{return KEYWORD; }
"continue"		{return KEYWORD; }
"if"		{return KEYWORD; }
"else"		{return KEYWORD; }
"unless"		{return KEYWORD; }
"when"		{return KEYWORD; }
"yield"		{return KEYWORD; }
"using"		{return KEYWORD; }
"try"		{return KEYWORD; }
"except"		{return KEYWORD; }
"namespace"		{return KEYWORD; }
"class"		{return KEYWORD; }
"struct"		{return KEYWORD; }
"enum"		{return KEYWORD; }
"interface"		{return KEYWORD; }
"event"		{return KEYWORD; }
"int"		{return KEYWORD; }
"long"		{return KEYWORD; }
"short"		{return KEYWORD; }
"byte"		{return KEYWORD; }
"bool"		{return KEYWORD; }
"char"		{return KEYWORD; }
"decimal" 		{return KEYWORD; }
"uint"		{return KEYWORD; }
"ulong"		{return KEYWORD; }
"ushort"		{return KEYWORD; }
"sbyte"		{return KEYWORD; }
"string"		{return KEYWORD; }
"object"		{return KEYWORD; }
"float"		{return KEYWORD; }
"double"		{return KEYWORD; }
"void"		{return KEYWORD; }
"true"		{return KEYWORD; }
"false"		{return KEYWORD; }
"null"		{return KEYWORD; }
"public"		{return KEYWORD; }
"internal"		{return KEYWORD; }
"private"		{return KEYWORD; }
"protected"		{return KEYWORD; }
"abstract"		{return KEYWORD; }
"virtual"		{return KEYWORD; }
"override"		{return KEYWORD; }
"static"		{return KEYWORD; }
       

<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return STRING; }


<VERB_STRING>{verbatim_string_cont}     { return STRING; }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return STRING; }
                      
{integer_literal}     { return NUMBER; }
{real_literal}        { return NUMBER; }
{character_literal}   { return CHARACTER; }
{string_literal}      { return STRING; }

","   { return OPERATOR; }
"["   { return OPERATOR; }
"]"   { return OPERATOR; }

{rank_specifier}     { return OPERATOR; }

"+="    { return OPERATOR; }
"-="    { return OPERATOR; }
"*="    { return OPERATOR; }
"/="    { return OPERATOR; }
"%="    { return OPERATOR; }
"^="    { return OPERATOR; }
"&="    { return OPERATOR; }
"|="    { return OPERATOR; }
"<<"    { return OPERATOR; }
">>"    { return OPERATOR; }
">>="   { return OPERATOR; }
"<<="   { return OPERATOR; }
"=="    { return OPERATOR; }
"!="    { return OPERATOR; }
"<="    { return OPERATOR; }
">="    { return OPERATOR; }
"&&"    { return OPERATOR; }
"||"    { return OPERATOR; }
"++"    { return OPERATOR; }
"--"    { return OPERATOR; }
"->"    { return OPERATOR; }


{identifier}             { return PLAIN; }
{at_identifier}          { return PLAIN; }

\n                       { return NEWLINE; }
[^#]                     { return PLAIN; }

 