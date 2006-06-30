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

%state ML_COMMENT
%state VERB_STRING
%state PREPROCESSOR
%state DOC_COMMENT
%state PPTAIL

%%

<YYINITIAL>{preprocessor}    { ENTER(PREPROCESSOR); return Preprocessor(); }

{white_space}+    { ; /* ignore */ }
                    
<YYINITIAL>{comment_start}   { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^*\n\t]+         { return Comment(); }
<ML_COMMENT>"*"+[^*/\n\t]*    { return Comment(); }
<ML_COMMENT>{comment_end}     { EXIT(); return Comment(); }

<YYINITIAL>{doc_comment}     { ENTER(DOC_COMMENT); return DocComment(); }
<YYINITIAL>{line_comment}    { return Comment(); }

<DOC_COMMENT>{new_line}        { EXIT(); return NewLine(); }
<DOC_COMMENT>"<"[^>\n]*        { docintag = 1; return DocComment();}
<DOC_COMMENT>"<"[^>\n]*">"     { return DocComment();}
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

<YYINITIAL>{at_identifier}   { return Identifier(IDENTIFIER); }

<YYINITIAL>"abstract"        {return Keyword(ABSTRACT);}
<YYINITIAL>"as"              {return Keyword(AS);}
<YYINITIAL>"base"            {return Keyword(BASE);}
<YYINITIAL>"bool"            {return Keyword(BOOL);}
<YYINITIAL>"break"           {return Keyword(BREAK);}
<YYINITIAL>"byte"            {return Keyword(BYTE);}
<YYINITIAL>"case"            {return Keyword(CASE);}
<YYINITIAL>"catch"           {return Keyword(CATCH);}
<YYINITIAL>"char"            {return Keyword(CHAR);}
<YYINITIAL>"checked"         {return Keyword(CHECKED);}
<YYINITIAL>"class"           {return Keyword(CLASS);}
<YYINITIAL>"const"           {return Keyword(CONST);}
<YYINITIAL>"continue"        {return Keyword(CONTINUE);}
<YYINITIAL>"decimal"         {return Keyword(DECIMAL);}
<YYINITIAL>"default"         {return Keyword(DEFAULT);}
<YYINITIAL>"delegate"        {return Keyword(DELEGATE);}
<YYINITIAL>"do"              {return Keyword(DO);}
<YYINITIAL>"double"          {return Keyword(DOUBLE);}
<YYINITIAL>"else"            {return Keyword(ELSE);}
<YYINITIAL>"enum"            {return Keyword(ENUM);}
<YYINITIAL>"event"           {return Keyword(EVENT);}
<YYINITIAL>"explicit"        {return Keyword(EXPLICIT);}
<YYINITIAL>"extern"          {return Keyword(EXTERN);}
<YYINITIAL>"false"           {return Keyword(FALSE);}
<YYINITIAL>"finally"         {return Keyword(FINALLY);}
<YYINITIAL>"fixed"           {return Keyword(FIXED);}
<YYINITIAL>"float"           {return Keyword(FLOAT);}
<YYINITIAL>"for"             {return Keyword(FOR);}
<YYINITIAL>"foreach"         {return Keyword(FOREACH);}
<YYINITIAL>"goto"            {return Keyword(GOTO);}
<YYINITIAL>"if"              {return Keyword(IF);}
<YYINITIAL>"implicit"        {return Keyword(IMPLICIT);}
<YYINITIAL>"in"              {return Keyword(IN);}
<YYINITIAL>"int"             {return Keyword(INT);}
<YYINITIAL>"interface"       {return Keyword(INTERFACE);}
<YYINITIAL>"internal"        {return Keyword(INTERNAL);}
<YYINITIAL>"is"              {return Keyword(IS);}
<YYINITIAL>"lock"            {return Keyword(LOCK);}
<YYINITIAL>"long"            {return Keyword(LONG);}
<YYINITIAL>"namespace"       {return Keyword(NAMESPACE);}
<YYINITIAL>"new"             {return Keyword(NEW);}
<YYINITIAL>"null"            {return Keyword(NULL_LITERAL);}
<YYINITIAL>"object"          {return Keyword(OBJECT);}
<YYINITIAL>"operator"        {return Keyword(OPERATOR);}
<YYINITIAL>"out"             {return Keyword(OUT);}
<YYINITIAL>"override"        {return Keyword(OVERRIDE);}
<YYINITIAL>"params"          {return Keyword(PARAMS);}
<YYINITIAL>"private"         {return Keyword(PRIVATE);}
<YYINITIAL>"protected"       {return Keyword(PROTECTED);}
<YYINITIAL>"public"          {return Keyword(PUBLIC);}
<YYINITIAL>"readonly"        {return Keyword(READONLY);}
<YYINITIAL>"ref"             {return Keyword(REF);}
<YYINITIAL>"return"          {return Keyword(RETURN);}
<YYINITIAL>"sbyte"           {return Keyword(SBYTE);}
<YYINITIAL>"sealed"          {return Keyword(SEALED);}
<YYINITIAL>"short"           {return Keyword(SHORT);}
<YYINITIAL>"sizeof"          {return Keyword(SIZEOF);}
<YYINITIAL>"stackalloc"      {return Keyword(STACKALLOC);}
<YYINITIAL>"static"          {return Keyword(STATIC);}
<YYINITIAL>"string"          {return Keyword(KW_STRING);}
<YYINITIAL>"struct"          {return Keyword(STRUCT);}
<YYINITIAL>"switch"          {return Keyword(SWITCH);}
<YYINITIAL>"this"            {return Keyword(THIS);}
<YYINITIAL>"throw"           {return Keyword(THROW);}
<YYINITIAL>"true"            {return Keyword(TRUE);}
<YYINITIAL>"try"             {return Keyword(TRY);}
<YYINITIAL>"typeof"          {return Keyword(TYPEOF);}
<YYINITIAL>"uint"            {return Keyword(UINT);}
<YYINITIAL>"ulong"           {return Keyword(ULONG);}
<YYINITIAL>"unchecked"       {return Keyword(UNCHECKED);}
<YYINITIAL>"unsafe"          {return Keyword(UNSAFE);}
<YYINITIAL>"ushort"          {return Keyword(USHORT);}
<YYINITIAL>"using"           {return Keyword(USING);}
<YYINITIAL>"virtual"         {return Keyword(VIRTUAL);}
<YYINITIAL>"void"            {return Keyword(VOID);}
<YYINITIAL>"volatile"        {return Keyword(VOLATILE);}
<YYINITIAL>"while"           {return Keyword(WHILE);}
<YYINITIAL>"value"           {return Keyword(IDENTIFIER);}   

<YYINITIAL>"partial"         {return Keyword();}
<YYINITIAL>"yield"           {return Keyword();}

<YYINITIAL>{verbatim_string_start}                 { ENTER(VERB_STRING); return String(MLSTRING_LITERAL); }

<VERB_STRING>{verbatim_string_cont}     { return String(MLSTRING_LITERAL); }
<VERB_STRING>{verbatim_string_end}      { EXIT(); return String(MLSTRING_LITERAL); }
                      
<YYINITIAL>{integer_literal}     { return Number(INTEGER_LITERAL); }
<YYINITIAL>{real_literal}        { return Number(REAL_LITERAL); }
<YYINITIAL>{character_literal}   { return Character(CHARACTER_LITERAL); }
<YYINITIAL>{string_literal}      { return String(MLSTRING_LITERAL); }

<YYINITIAL>{rank_specifier}      { return Operator(RANK_SPECIFIER); }

<YYINITIAL>"+="    { return Operator(PLUSEQ); }
<YYINITIAL>"-="    { return Operator(MINUSEQ); }
<YYINITIAL>"*="    { return Operator(STAREQ); }
<YYINITIAL>"/="    { return Operator(DIVEQ); }
<YYINITIAL>"%="    { return Operator(MODEQ); }
<YYINITIAL>"^="    { return Operator(XOREQ); }
<YYINITIAL>"&="    { return Operator(ANDEQ); }
<YYINITIAL>"|="    { return Operator(OREQ); }
<YYINITIAL>"<<"    { return Operator(LTLT); }
<YYINITIAL>">>"    { return Operator(GTGT); }
<YYINITIAL>">>="   { return Operator(GTGTEQ); }
<YYINITIAL>"<<="   { return Operator(LTLTEQ); }
<YYINITIAL>"=="    { return Operator(EQEQ); }
<YYINITIAL>"!="    { return Operator(NOTEQ); }
<YYINITIAL>"<="    { return Operator(LEQ); }
<YYINITIAL>">="    { return Operator(GEQ); }
<YYINITIAL>"&&"    { return Operator(ANDAND); }
<YYINITIAL>"||"    { return Operator(OROR); }
<YYINITIAL>"++"    { return Operator(PLUSPLUS); }
<YYINITIAL>"--"    { return Operator(MINUSMINUS); }

<YYINITIAL>"->"    { return Operator(ARROW); }
<YYINITIAL>"."     { return Operator(YYCHAR); }
<YYINITIAL>"("     { return Operator(YYCHAR); }
<YYINITIAL>")"     { return Operator(YYCHAR); }
<YYINITIAL>"["     { return Operator(YYCHAR); }
<YYINITIAL>"]"     { return Operator(YYCHAR); }
<YYINITIAL>"{"     { return Operator(YYCHAR); }
<YYINITIAL>"}"     { return Operator(YYCHAR); }

<YYINITIAL>"+"     { return Operator(YYCHAR); }
<YYINITIAL>"-"     { return Operator(YYCHAR); }

<YYINITIAL>"="     { return Operator(YYCHAR); }
<YYINITIAL>";"     { return Operator(YYCHAR); }
<YYINITIAL>"!"     { return Operator(YYCHAR); }
<YYINITIAL>"?"     { return Operator(YYCHAR); }
<YYINITIAL>"*"     { return Operator(YYCHAR); }
<YYINITIAL>"%"     { return Operator(YYCHAR); }
<YYINITIAL>"^"     { return Operator(YYCHAR); }
<YYINITIAL>"&"     { return Operator(YYCHAR); }
<YYINITIAL>"/"     { return Operator(YYCHAR); }
<YYINITIAL>"|"     { return Operator(YYCHAR); }
<YYINITIAL>"<"     { return Operator(YYCHAR); }
<YYINITIAL>">"     { return Operator(YYCHAR); }
<YYINITIAL>"~"     { return Operator(YYCHAR); }
<YYINITIAL>":"     { return Operator(YYCHAR); }
<YYINITIAL>","     { return Operator(YYCHAR); }

<YYINITIAL>"get"   { return Keyword(GET); }
<YYINITIAL>"set"   { return Keyword(SET); }

<YYINITIAL>{error_string}           { return Error(STRING_LITERAL); }
<YYINITIAL>{identifier}             { return Identifier(IDENTIFIER); }

{new_line}               { return NewLine();}
<YYINITIAL>{attr}                   { return Operator('['); }
.                        { return Error(); }
