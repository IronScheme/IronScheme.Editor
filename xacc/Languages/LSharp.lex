#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;
using LSharp;
using LexerBase = Xacc.Languages.LSharp.LexerBase<Xacc.Languages.LSharp.ValueType>;

//NOTE: comments are not allowed except in code blocks
%%

%class LSharpLexer

%unicode

line_comment           =";"[^\n]*

comment_start          ="#|"
comment_end            ="|#"

white_space            =[ \t]
new_line               =\n

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
character_literal      =#\\{character}

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
error_string           =\"({reg_string_char})*
single_verbatim_char   =[^\"\n]
quote_esc_seq          =\"\"
verb_string_char       ={single_verbatim_char}|{quote_esc_seq}
string_literal         ={regular_string}

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"-"|"_"|"!"|"+")
identifier2            =(({letter_char}({ident_char})*)("[]")?|("*"({ident_char})+"*"))
identifier             ={identifier2}("."{identifier2})*

atoms     =(null|true|false)

%state KWSTATE
%state ML_COMMENT

%%

{white_space}+        { ; }
{new_line}            { return NewLine();}
                      
<YYINITIAL,KWSTATE>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }                      
{line_comment}        { return Comment(); }

"&body"               { return Other(); }
"&rest"               { return Other(ARGREST); }

<ML_COMMENT>[^\n\|]+         { return Comment(); }
<ML_COMMENT>{comment_end}     { EXIT(); return Comment(); }
<ML_COMMENT>"|"               { return Comment(); }
 
{atoms}               { EXIT(); return Keyword(LITERAL); } 

<KWSTATE>and               { EXIT(); return Keyword(AND); } 
<KWSTATE>call               { EXIT(); return Keyword(CALL); } 
<KWSTATE>cond               { EXIT(); return Keyword(COND); } 
<KWSTATE>do               {EXIT();  return Keyword(DO); } 
<KWSTATE>each               { EXIT(); return Keyword(EACH); } 
<KWSTATE>fn               { EXIT(); return Keyword(FN); } 
<KWSTATE>for               { EXIT(); return Keyword(FOR); } 
<KWSTATE>foreach               { EXIT(); return Keyword(EACH); } 
<KWSTATE>if               { EXIT(); return Keyword(IF); } 
<KWSTATE>let               { EXIT(); return Keyword(LET); } 
<KWSTATE>macro               { EXIT(); return Keyword(MACRO); } 
<KWSTATE>or               { EXIT(); return Keyword(OR); } 
<KWSTATE>the               { EXIT(); return Keyword(THE); } 
<KWSTATE>to               { EXIT(); return Keyword(TO); } 
<KWSTATE>trace               { EXIT(); return Keyword(TRACE); } 
<KWSTATE>try               { EXIT(); return Keyword(TRY); } 
<KWSTATE>when               { EXIT(); return Keyword(WHEN); } 
<KWSTATE>while               { EXIT(); return Keyword(WHILE); } 
<KWSTATE>with               { EXIT(); return Keyword(WITH); } 
<KWSTATE>"--"               { EXIT(); return Keyword(DEC); } 
<KWSTATE>"++"               { EXIT(); return Keyword(INC); } 

<KWSTATE>apply               { EXIT(); return Type(APPLY); } 
<KWSTATE>append               {EXIT();  return Type(APPEND); } 
<KWSTATE>assoc               { EXIT(); return Type(ASSOC); } 
<KWSTATE>caaar               { EXIT(); return Type(CAAAR); } 
<KWSTATE>caadr               { EXIT(); return Type(CAADR); } 
<KWSTATE>caar               { EXIT(); return Type(CAAR); } 
<KWSTATE>cadar               { EXIT(); return Type(CADAR); } 
<KWSTATE>caddr               { EXIT(); return Type(CADDR); } 
<KWSTATE>cadr               { EXIT(); return Type(CADR); } 
<KWSTATE>car               { EXIT(); return Type(CAR); } 
<KWSTATE>cdar               { EXIT(); return Type(CDAR); } 
<KWSTATE>cdaar             { EXIT(); return Type(CDAAR); } 
<KWSTATE>cddar               { EXIT(); return Type(CDDAR); } 
<KWSTATE>cdddr               { EXIT(); return Type(CDDDR); } 
<KWSTATE>cddr               { EXIT(); return Type(CDDR); } 
<KWSTATE>cdr               { EXIT(); return Type(CDR); } 
<KWSTATE>cons               { EXIT(); return Type(CONS); } 
<KWSTATE>environment               { EXIT(); return Type(ENV); } 
<KWSTATE>eq               { EXIT(); return Type(EQ); } 
<KWSTATE>eql               { EXIT(); return Type(EQL); } 
<KWSTATE>eval               { EXIT(); return Type(EVAL); } 
<KWSTATE>evalstring               { EXIT(); return Type(EVALSTRING); } 
<KWSTATE>exit               { EXIT(); return Type(EXITFN); } 
<KWSTATE>first               { EXIT(); return Type(FIRST); } 
<KWSTATE>inspect               { EXIT(); return Type(INSPECT); } 
<KWSTATE>is               { EXIT(); return Type(IS); } 
<KWSTATE>length              { EXIT(); return Type(LENGTH); } 
<KWSTATE>list               { EXIT(); return Type(LIST); } 
<KWSTATE>load               { EXIT(); return Type(LOAD); } 
<KWSTATE>macroexpand               { EXIT(); return Type(MACROEXPAND); } 
<KWSTATE>map               { EXIT(); return Type(MAP); } 
<KWSTATE>nconc               { EXIT(); return Type(NCONC); } 
<KWSTATE>new               { EXIT(); return Type(NEW); } 
<KWSTATE>not               { EXIT(); return Type(NOT); } 
<KWSTATE>nth               { EXIT(); return Type(NTH); } 
<KWSTATE>pr               { EXIT(); return Type(PR); } 
<KWSTATE>prl               { EXIT(); return Type(PRL); } 
<KWSTATE>read               { EXIT(); return Type(READ); } 
<KWSTATE>readstring               { EXIT(); return Type(READSTRING); } 
<KWSTATE>reference               { EXIT(); return Type(REFERENCE); } 
<KWSTATE>reverse               { EXIT(); return Type(REVERSE); } 
<KWSTATE>rest               { EXIT(); return Type(REST); } 
<KWSTATE>throw               { EXIT(); return Type(THROW); } 
<KWSTATE>typeof               { EXIT(); return Type(TYPEOF); } 
<KWSTATE>using               { EXIT(); return Type(USING); } 
<KWSTATE>"+"               { EXIT(); return Type(ADD); } 
<KWSTATE>"="               { EXIT(); return Type(SETF); } 
<KWSTATE>"*"               { EXIT(); return Type(MUL); } 
<KWSTATE>"/"               { EXIT(); return Type(DIV); } 
<KWSTATE>"-"               { EXIT(); return Type(SUB); } 
<KWSTATE>">"               { EXIT(); return Type(GT); } 
<KWSTATE>">="               { EXIT(); return Type(GTE); } 
<KWSTATE>"<="              { EXIT(); return Type(LTE); } 
<KWSTATE>"<"               { EXIT(); return Type(LT); } 
<KWSTATE>"&"               { EXIT(); return Type(LOGAND); } 
<KWSTATE>"^"               { EXIT(); return Type(LOGXOR); } 
<KWSTATE>"|"               { EXIT(); return Type(LOGOR); } 
<KWSTATE>"!="               { EXIT(); return Type(NEQ); } 
<KWSTATE>"=="               { EXIT(); return Type(EQ); } 

<KWSTATE>defmacro               { EXIT(); return Keyword(DEFMACRO); } 
<KWSTATE>defun               { EXIT(); return Keyword(DEFUN); } 
<KWSTATE>listp               { EXIT(); return Keyword(); } 


<KWSTATE>{identifier}          { EXIT(); return Identifier(IDENTIFIER); }
<KWSTATE>{character_literal}   { EXIT(); return Character(LITERAL); }
<KWSTATE>{integer_literal}     { EXIT(); return Number(INTEGER); }
<KWSTATE>{real_literal}        { EXIT(); return Number(LITERAL); }
<KWSTATE>{string_literal}      { EXIT(); return String(STRING); }

{character_literal}   { return Character(LITERAL); }                      
{integer_literal}     { return Number(INTEGER); }
{real_literal}        { return Number(LITERAL); }
{string_literal}      { return String(STRING); }

"("                   { ENTER(KWSTATE); return Operator(LBRACE); }                     
")"                   { return Operator(RBRACE); } 
"?"                   { return Other(); }
"`"                   { return Operator(BACKQUOTE); }
"'"                   { return Operator(QUOTE); }
",@"                  { return Operator(SPLICE); }
","                   { return Operator(UNQUOTE);}

{identifier}          { return Identifier(IDENTIFIER); }

[-\+|!=><*/^&]+       { return Identifier(IDENTIFIER); }

.                     { return Error(); }

