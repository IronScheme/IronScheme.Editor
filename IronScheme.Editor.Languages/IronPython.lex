using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class IronPythonLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "IronPython"; } }
	  public override string[] Extensions {get { return new string[]{"py"}; } }
	  protected override LexerBase GetLexer() { return new IronPythonLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class IronPythonLexer

%full

single_line_comment    =#[^\n]*

white_space            =[ \t]

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
character_literal      =\'({character})+\'


single_string_char     =[^\\\"]
simple_esc_str         =\\[\"\\0abfnrtv]
reg_string_char        =({single_string_char}|{simple_esc_str}|{hex_esc_seq}|{uni_esc_seq})
regular_string         =\"({reg_string_char})*\"
regular_string2        =((r|R)\"({single_string_char}|\\)*\")
single_verbatim_char   =[^\"\n]
verb_string_char       =({single_verbatim_char})
string_literal         =({regular_string}|{regular_string2})

verbatim_string_start  =\"\"\"
verbatim_string_cont   =(({verb_string_char})+|\")
verbatim_string_end    =\"\"\"

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"_"|"@")
identifier             =({letter_char}|"_"){ident_char}*
at_identifier          =\@{identifier}
ws_identifier          ={identifier}(({white_space})+{identifier})*


rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

%state VERB_STRING

%%

{white_space}    { ; /* ignore */ }

<VERB_STRING>{verbatim_string_cont}     { return String(); }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return String(); }

                      
<YYINITIAL>{single_line_comment} { return Comment(); }


"and"        {return Keyword();}
"assert"              {return Keyword();}
"break"            {return Keyword();}
"class"            {return Keyword();}
"continue"           {return Keyword();}
"def"            {return Keyword();}
"del"            {return Keyword();}
"elif"           {return Keyword();}
"else"            {return Keyword();}
"except"         {return Keyword();}
"exec"           {return Keyword();}
"finally"           {return Keyword();}
"for"        {return Keyword();}
"from"        {return Keyword();}
"global"         {return Keyword();}
"if"         {return Keyword();}
"import"        {return Keyword();}
"in"              {return Keyword();}
"is"          {return Keyword();}
"lambda"            {return Keyword();}
"not"            {return Keyword();}
"or"           {return Keyword();}
"pass"        {return Keyword();}
"print"          {return Keyword();}
"raise"           {return Keyword();}
"return"         {return Keyword();}
"try"           {return Keyword();}
"while"           {return Keyword();}
"yield"             {return Keyword();}

"self"               {return Type();}

                      
<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return String(); }

<YYINITIAL>{integer_literal}     { return Number(); }
<YYINITIAL>{real_literal}        { return Number(); }
<YYINITIAL>{character_literal}   { return String(); }
<YYINITIAL>{string_literal}      { return String(); }


"+"     { return Operator(); }
"-"     { return Operator(); }
"**"    { return Operator(); }   
"+="    { return Operator(); }
"-="    { return Operator(); }
"*="    { return Operator(); }
"**="    { return Operator(); }
"//"    { return Operator(); }
"//="    { return Operator(); }
"/="    { return Operator(); }
"/"    { return Operator(); }
"%"    { return Operator(); }
"%="    { return Operator(); }
"^="    { return Operator(); }
"&="    { return Operator(); }
"|="    { return Operator(); }
"<<"    { return Operator(); }
">>"    { return Operator(); }
">>="   { return Operator(); }
"<<="   { return Operator(); }
"!="    { return Operator(); }
"<="    { return Operator(); }
">="    { return Operator(); }

"_"   { return Operator(); }
","   { return Operator(); }
"."   { return Operator(); }
";"   { return Operator(); }
":"   { return Operator(); }
"`"   { return Operator(); }
"~"   { return Operator(); }
"="   { return Operator(); }
"["   { return Operator(); }
"]"   { return Operator(); }
"<"   { return Operator(); }
">"   { return Operator(); }
"("   { return Operator(); }
")"   { return Operator(); }
"{"   { return Operator(); }
"}"   { return Operator(); }
"<>"   { return Operator(); }
"@"   { return Operator(); }


{identifier}             { return Identifier(); }

\n                       { return NewLine();}
.                        { return Plain(); }

 