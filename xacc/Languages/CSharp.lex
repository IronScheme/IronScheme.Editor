#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;
using CSharp;
using LexerBase = CSharp.LexerBase<CSharp.ValueType>;

//NOTE: comments are not allowed except in code blocks
%%

%class CSharpLexer

%unicode

%{
int docintag = 0;

static ValueType Token(TokenClass c)
{
  ValueType t = new ValueType();
  t.__type = -1;
  t.__class = c;
  return t;
}

static ValueType Token(TokenClass c, int type)
{
  ValueType t = new ValueType();
  t.__type = type;
  t.__class = c;
  return t;
}
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

<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return Token(TokenClass.Preprocessor); }
<YYINITIAL>{white_space}+    { ; /* ignore */ }

                    
<YYINITIAL>{comment_start}   { ENTER(ML_COMMENT); return Token(TokenClass.Comment); }


<ML_COMMENT>{white_space}+    { ; }
<ML_COMMENT>{new_line}        { return Token(TokenClass.NewLine); }
<ML_COMMENT>[^*\n\t]+         { return Token(TokenClass.Comment); }
<ML_COMMENT>"*"+[^*/\n\t]*    { return Token(TokenClass.Comment); }
<ML_COMMENT>{comment_end}     { EXIT(); return Token(TokenClass.Comment); }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return Token(TokenClass.DocComment); }
<YYINITIAL>{line_comment}    { return Token(TokenClass.Comment); }

<DOC_COMMENT>{new_line}        { EXIT(); return Token(TokenClass.NewLine); }
<DOC_COMMENT>"<"[^>\n]*        { docintag = 1; return Token(TokenClass.DocComment);}
<DOC_COMMENT>"<"[^>\n]*">"     { return Token(TokenClass.DocComment);}
<DOC_COMMENT>{white_space}+    { ; /* ignore */ }
<DOC_COMMENT>">"               { return Token(TokenClass.DocComment);}
<DOC_COMMENT>[^<>\n]+          { if (docintag == 1) {docintag = 0; return Token(TokenClass.DocComment);} else return Token(TokenClass.Comment); }


<PPTAIL>[^\n]+            { return Token(TokenClass.Preprocessor, PPID); }
<PPTAIL>{new_line}        { EXIT(); EXIT(); return Token(TokenClass.NewLine); }


<PREPROCESSOR>"define"          { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPDEFINE); }
<PREPROCESSOR>"if"              { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPIF); }
<PREPROCESSOR>"else"            { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPELSE); }
<PREPROCESSOR>"elif"            { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPELIF); }
<PREPROCESSOR>"endif"           { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPENDIF); }
<PREPROCESSOR>"line"            { ENTER(PPTAIL); return Token(TokenClass.Preprocessor); }
<PREPROCESSOR>"pragma"          { ENTER(PPTAIL); return Token(TokenClass.Preprocessor); }
<PREPROCESSOR>"error"           { ENTER(PPTAIL); return Token(TokenClass.Preprocessor); }
<PREPROCESSOR>"warning"         { ENTER(PPTAIL); return Token(TokenClass.Preprocessor); }
<PREPROCESSOR>"region"          { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPREGION); }
<PREPROCESSOR>"endregion"       { ENTER(PPTAIL); return Token(TokenClass.Preprocessor,PPENDREGION); }
<PREPROCESSOR>{new_line}        { EXIT(); return Token(TokenClass.NewLine); }
<PREPROCESSOR>.                 { return Token(TokenClass.Error); }

<YYINITIAL>{at_identifier}   { return Token(TokenClass.Identifier); }

<YYINITIAL>"abstract"        {return Token(TokenClass.Keyword,ABSTRACT);}
<YYINITIAL>"as"              {return Token(TokenClass.Keyword,AS);}
<YYINITIAL>"base"            {return Token(TokenClass.Keyword,BASE);}
<YYINITIAL>"bool"            {return Token(TokenClass.Keyword,BOOL);}
<YYINITIAL>"break"           {return Token(TokenClass.Keyword,BREAK);}
<YYINITIAL>"byte"            {return Token(TokenClass.Keyword,BYTE);}
<YYINITIAL>"case"            {return Token(TokenClass.Keyword,CASE);}
<YYINITIAL>"catch"           {return Token(TokenClass.Keyword,CATCH);}
<YYINITIAL>"char"            {return Token(TokenClass.Keyword,CHAR);}
<YYINITIAL>"checked"         {return Token(TokenClass.Keyword,CHECKED);}
<YYINITIAL>"class"           {return Token(TokenClass.Keyword,CLASS);}
<YYINITIAL>"const"           {return Token(TokenClass.Keyword,CONST);}
<YYINITIAL>"continue"        {return Token(TokenClass.Keyword,CONTINUE);}
<YYINITIAL>"decimal"         {return Token(TokenClass.Keyword,DECIMAL);}
<YYINITIAL>"default"         {return Token(TokenClass.Keyword,DEFAULT);}
<YYINITIAL>"delegate"        {return Token(TokenClass.Keyword,DELEGATE);}
<YYINITIAL>"do"              {return Token(TokenClass.Keyword,DO);}
<YYINITIAL>"double"          {return Token(TokenClass.Keyword,DOUBLE);}
<YYINITIAL>"else"            {return Token(TokenClass.Keyword,ELSE);}
<YYINITIAL>"enum"            {return Token(TokenClass.Keyword,ENUM);}
<YYINITIAL>"event"           {return Token(TokenClass.Keyword,EVENT);}
<YYINITIAL>"explicit"        {return Token(TokenClass.Keyword,EXPLICIT);}
<YYINITIAL>"extern"          {return Token(TokenClass.Keyword,EXTERN);}
<YYINITIAL>"false"           {return Token(TokenClass.Keyword,FALSE);}
<YYINITIAL>"finally"         {return Token(TokenClass.Keyword,FINALLY);}
<YYINITIAL>"fixed"           {return Token(TokenClass.Keyword,FIXED);}
<YYINITIAL>"float"           {return Token(TokenClass.Keyword,FLOAT);}
<YYINITIAL>"for"             {return Token(TokenClass.Keyword,FOR);}
<YYINITIAL>"foreach"         {return Token(TokenClass.Keyword,FOREACH);}
<YYINITIAL>"goto"            {return Token(TokenClass.Keyword,GOTO);}
<YYINITIAL>"if"              {return Token(TokenClass.Keyword,IF);}
<YYINITIAL>"implicit"        {return Token(TokenClass.Keyword,IMPLICIT);}
<YYINITIAL>"in"              {return Token(TokenClass.Keyword,IN);}
<YYINITIAL>"int"             {return Token(TokenClass.Keyword,INT);}
<YYINITIAL>"interface"       {return Token(TokenClass.Keyword,INTERFACE);}
<YYINITIAL>"internal"        {return Token(TokenClass.Keyword,INTERNAL);}
<YYINITIAL>"is"              {return Token(TokenClass.Keyword,IS);}
<YYINITIAL>"lock"            {return Token(TokenClass.Keyword,LOCK);}
<YYINITIAL>"long"            {return Token(TokenClass.Keyword,LONG);}
<YYINITIAL>"namespace"       {return Token(TokenClass.Keyword,NAMESPACE);}
<YYINITIAL>"new"             {return Token(TokenClass.Keyword,NEW);}
<YYINITIAL>"null"            {return Token(TokenClass.Keyword,NULL_LITERAL);}
<YYINITIAL>"object"          {return Token(TokenClass.Keyword,OBJECT);}
<YYINITIAL>"operator"        {return Token(TokenClass.Keyword,OPERATOR);}
<YYINITIAL>"out"             {return Token(TokenClass.Keyword,OUT);}
<YYINITIAL>"override"        {return Token(TokenClass.Keyword,OVERRIDE);}
<YYINITIAL>"params"          {return Token(TokenClass.Keyword,PARAMS);}
<YYINITIAL>"private"         {return Token(TokenClass.Keyword,PRIVATE);}
<YYINITIAL>"protected"       {return Token(TokenClass.Keyword,PROTECTED);}
<YYINITIAL>"public"          {return Token(TokenClass.Keyword,PUBLIC);}
<YYINITIAL>"readonly"        {return Token(TokenClass.Keyword,READONLY);}
<YYINITIAL>"ref"             {return Token(TokenClass.Keyword,REF);}
<YYINITIAL>"return"          {return Token(TokenClass.Keyword,RETURN);}
<YYINITIAL>"sbyte"           {return Token(TokenClass.Keyword,SBYTE);}
<YYINITIAL>"sealed"          {return Token(TokenClass.Keyword,SEALED);}
<YYINITIAL>"short"           {return Token(TokenClass.Keyword,SHORT);}
<YYINITIAL>"sizeof"          {return Token(TokenClass.Keyword,SIZEOF);}
<YYINITIAL>"stackalloc"      {return Token(TokenClass.Keyword,STACKALLOC);}
<YYINITIAL>"static"          {return Token(TokenClass.Keyword,STATIC);}
<YYINITIAL>"string"          {return Token(TokenClass.Keyword,KW_STRING);}
<YYINITIAL>"struct"          {return Token(TokenClass.Keyword,STRUCT);}
<YYINITIAL>"switch"          {return Token(TokenClass.Keyword,SWITCH);}
<YYINITIAL>"this"            {return Token(TokenClass.Keyword,THIS);}
<YYINITIAL>"throw"           {return Token(TokenClass.Keyword,THROW);}
<YYINITIAL>"true"            {return Token(TokenClass.Keyword,TRUE);}
<YYINITIAL>"try"             {return Token(TokenClass.Keyword,TRY);}
<YYINITIAL>"typeof"          {return Token(TokenClass.Keyword,TYPEOF);}
<YYINITIAL>"uint"            {return Token(TokenClass.Keyword,UINT);}
<YYINITIAL>"ulong"           {return Token(TokenClass.Keyword,ULONG);}
<YYINITIAL>"unchecked"       {return Token(TokenClass.Keyword,UNCHECKED);}
<YYINITIAL>"unsafe"          {return Token(TokenClass.Keyword,UNSAFE);}
<YYINITIAL>"ushort"          {return Token(TokenClass.Keyword,USHORT);}
<YYINITIAL>"using"           {return Token(TokenClass.Keyword,USING);}
<YYINITIAL>"virtual"         {return Token(TokenClass.Keyword,VIRTUAL);}
<YYINITIAL>"void"            {return Token(TokenClass.Keyword,VOID);}
<YYINITIAL>"volatile"        {return Token(TokenClass.Keyword,VOLATILE);}
<YYINITIAL>"while"           {return Token(TokenClass.Keyword,WHILE);}
<YYINITIAL>"value"           {return Token(TokenClass.Keyword,IDENTIFIER);}   

<YYINITIAL>"partial"         {return Token(TokenClass.Keyword);}
<YYINITIAL>"yield"           {return Token(TokenClass.Keyword);}

                      
<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return Token(TokenClass.String,MLSTRING_LITERAL); }

<VERB_STRING>{new_line}                 { return Token(TokenClass.NewLine); }
<VERB_STRING>{verbatim_string_cont}     { return Token(TokenClass.String,MLSTRING_LITERAL); }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return Token(TokenClass.String,MLSTRING_LITERAL); }
                      
<YYINITIAL>{integer_literal}     { return Token(TokenClass.Number,INTEGER_LITERAL); }
<YYINITIAL>{real_literal}        { return Token(TokenClass.Number,REAL_LITERAL); }
<YYINITIAL>{character_literal}   { return Token(TokenClass.Character,CHARACTER_LITERAL); }
<YYINITIAL>{string_literal}      { return Token(TokenClass.String,MLSTRING_LITERAL); }

<YYINITIAL>{rank_specifier}      { return Token(TokenClass.Operator,RANK_SPECIFIER); }

                      
<YYINITIAL>"+="    { return Token(TokenClass.Operator,PLUSEQ); }
<YYINITIAL>"-="    { return Token(TokenClass.Operator,MINUSEQ); }
<YYINITIAL>"*="    { return Token(TokenClass.Operator,STAREQ); }
<YYINITIAL>"/="    { return Token(TokenClass.Operator,DIVEQ); }
<YYINITIAL>"%="    { return Token(TokenClass.Operator,MODEQ); }
<YYINITIAL>"^="    { return Token(TokenClass.Operator,XOREQ); }
<YYINITIAL>"&="    { return Token(TokenClass.Operator,ANDEQ); }
<YYINITIAL>"|="    { return Token(TokenClass.Operator,OREQ); }
<YYINITIAL>"<<"    { return Token(TokenClass.Operator,LTLT); }
<YYINITIAL>">>"   	{ return Token(TokenClass.Operator,GTGT); }
<YYINITIAL>">>="   { return Token(TokenClass.Operator,GTGTEQ); }
<YYINITIAL>"<<="   { return Token(TokenClass.Operator,LTLTEQ); }
<YYINITIAL>"=="    { return Token(TokenClass.Operator,EQEQ); }
<YYINITIAL>"!="    { return Token(TokenClass.Operator,NOTEQ); }
<YYINITIAL>"<="    { return Token(TokenClass.Operator,LEQ); }
<YYINITIAL>">="    { return Token(TokenClass.Operator,GEQ); }
<YYINITIAL>"&&"    { return Token(TokenClass.Operator,ANDAND); }
<YYINITIAL>"||"    { return Token(TokenClass.Operator,OROR); }
<YYINITIAL>"++"    { return Token(TokenClass.Operator,PLUSPLUS); }
<YYINITIAL>"--"    { return Token(TokenClass.Operator,MINUSMINUS); }

<YYINITIAL>"->"    { return Token(TokenClass.Operator,ARROW); }
<YYINITIAL>"."     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"("     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>")"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"["     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"]"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"{"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"}"     { return Token(TokenClass.Operator,YYCHAR); }

<YYINITIAL>"+"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"-"     { return Token(TokenClass.Operator,YYCHAR); }

<YYINITIAL>"="     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>";"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"!"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"?"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"*"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"%"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"^"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"&"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"/"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"|"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"<"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>">"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>"~"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>":"     { return Token(TokenClass.Operator,YYCHAR); }
<YYINITIAL>","     { return Token(TokenClass.Operator,YYCHAR); }


<YYINITIAL>"get"   { return Token(TokenClass.Keyword,GET); }
<YYINITIAL>"set"   { return Token(TokenClass.Keyword,SET); }

<YYINITIAL>{error_string}           { return Token(TokenClass.Error,STRING_LITERAL); }

<YYINITIAL>{identifier}             { return Token(TokenClass.Identifier,IDENTIFIER); }


<YYINITIAL>{new_line}               { return Token(TokenClass.NewLine);}
<YYINITIAL>{attr}                   { return Token(TokenClass.Operator,'['); }
<YYINITIAL>.                        { return Token(TokenClass.Error, error); }
