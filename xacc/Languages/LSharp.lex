#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;
using LSharp;
using LexerBase = LSharp.LexerBase<LSharp.ValueType>;

//NOTE: comments are not allowed except in code blocks
%%

%class LSharpLexer

%unicode

%{
static ValueType Token(TokenClass c, Tokens type)
{
  ValueType t = new ValueType();
  t.__type = (int)type;
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
identifier             =({letter_char}({ident_char})*)("[]")?|("*"({ident_char})+"*")

atoms     =(null|true|false)

forms1    =(and|backquote|call|compile|cond|do|each|fn|for|foreach)
forms2    =(if|let|macro|or|quote|the|to|trace|try|when|while|with|"++"|"--")

func1     =(apply|append|assoc|caaar|caadr|caar|cadar|caddr|cadr|car|cdar|cdaar|cddar|cdddr|cddr|cdr)
func2     =(cons|environment|eq|eql|eval|evalstring|exit|first|import|inspect|is|length|list)
func3     =(load|macroexpand|map|nconc|new|not|nth|pr|prl|read|readstring|reference|reset|reverse|rest|throw|typeof|using)
func4     =("+"|"="|"*"|"/"|"-"|">"|">="|"<"|"<="|"&"|"^"|"|"|"!="|"==")

builtin   =(listp)

keyword               =({forms1}|{forms2}|{builtin})
function              =({func1}|{func2}|{func3}|{func4})


%state KWSTATE
%state ML_COMMENT

%%

{white_space}+        { ; }
{new_line}            { return NEWLINE;}
                      
<YYINITIAL,KWSTATE>{comment_start}       { ENTER(ML_COMMENT); return COMMENT; }                      
{line_comment}        { return COMMENT; }

"&body"               { return OTHER; }
"&rest"               { return OTHER; }

<ML_COMMENT>[^\n\|]+         { return COMMENT; }
<ML_COMMENT>{comment_end}     { EXIT(); return COMMENT; }
<ML_COMMENT>"|"               { return COMMENT; }
 
<KWSTATE>{atoms}               { return Keyword(LITERAL); } 

<KWSTATE>and               { return Keyword(AND); } 
<KWSTATE>`               { return Keyword(BACKQUOTE); } 
<KWSTATE>call               { return Keyword(CALL); } 
<KWSTATE>cond               { return Keyword(COND); } 
<KWSTATE>do               { return Keyword(DO); } 
<KWSTATE>each               { return Keyword(EACH); } 
<KWSTATE>fn               { return Keyword(FN); } 
<KWSTATE>for               { return Keyword(FOR); } 
<KWSTATE>foreach               { return Keyword(EACH); } 
<KWSTATE>if               { return Keyword(IF); } 
<KWSTATE>let               { return Keyword(LET); } 
<KWSTATE>macro               { return Keyword(MACRO); } 
<KWSTATE>or               { return Keyword(OR); } 
<KWSTATE>'               { return Keyword(QUOTE); } 
<KWSTATE>the               { return Keyword(THE); } 
<KWSTATE>to               { return Keyword(TO); } 
<KWSTATE>trace               { return Keyword(TRACE); } 
<KWSTATE>try               { return Keyword(TRY); } 
<KWSTATE>when               { return Keyword(WHEN); } 
<KWSTATE>while               { return Keyword(WHILE); } 
<KWSTATE>with               { return Keyword(WITH); } 
<KWSTATE>"--"               { return Keyword(DEC); } 
<KWSTATE>"++"               { return Keyword(INC); } 

<KWSTATE>apply               { return Type(APPLY); } 
<KWSTATE>append               { return Type(APPEND); } 
<KWSTATE>assoc               { return Type(ASSOC); } 
<KWSTATE>caaar               { return Type(CAAAR); } 
<KWSTATE>caadr               { return Type(CAADR); } 
<KWSTATE>caar               { return Type(CAAR); } 
<KWSTATE>cadar               { return Type(CADAR); } 
<KWSTATE>caddr               { return Type(CADDR); } 
<KWSTATE>cadr               { return Type(CADR); } 
<KWSTATE>car               { return Type(CAR); } 
<KWSTATE>cdar               { return Type(CDAR); } 
<KWSTATE>cdaar             { return Type(CDAAR); } 
<KWSTATE>cddar               { return Type(CDDAR); } 
<KWSTATE>cdddr               { return Type(CDDDR); } 
<KWSTATE>cddr               { return Type(CDDR); } 
<KWSTATE>cdr               { return Type(CDR); } 
<KWSTATE>cons               { return Type(CONS); } 
<KWSTATE>environment               { return Type(ENV); } 
<KWSTATE>eq               { return Type(EQ); } 
<KWSTATE>eql               { return Type(EQL); } 
<KWSTATE>eval               { return Type(EVAL); } 
<KWSTATE>evalstring               { return Type(EVALSTRING); } 
<KWSTATE>exit               { return Type(EXITFN); } 
<KWSTATE>first               { return Type(FIRST); } 
<KWSTATE>inspect               { return Type(INSPECT); } 
<KWSTATE>is               { return Type(IS); } 
<KWSTATE>length              { return Type(LENGTH); } 
<KWSTATE>list               { return Type(LIST); } 
<KWSTATE>load               { return Type(LOAD); } 
<KWSTATE>macroexpand               { return Type(MACROEXPAND); } 
<KWSTATE>map               { return Type(MAP); } 
<KWSTATE>nconc               { return Type(NCONC); } 
<KWSTATE>new               { return Type(NEW); } 
<KWSTATE>not               { return Type(NOT); } 
<KWSTATE>nth               { return Type(NTH); } 
<KWSTATE>pr               { return Type(PR); } 
<KWSTATE>prl               { return Type(PRL); } 
<KWSTATE>read               { return Type(READ); } 
<KWSTATE>readstring               { return Type(READSTRING); } 
<KWSTATE>reference               { return Type(REFERENCE); } 
<KWSTATE>reverse               { return Type(REVERSE); } 
<KWSTATE>rest               { return Type(REST); } 
<KWSTATE>throw               { return Type(THROW); } 
<KWSTATE>typeof               { return Type(TYPEOF); } 
<KWSTATE>using               { return Type(USING); } 
<KWSTATE>"+"               { return Type(ADD); } 
<KWSTATE>"="               { return Type(SETF); } 
<KWSTATE>"*"               { return Type(MUL); } 
<KWSTATE>"/"               { return Type(DIV); } 
<KWSTATE>"-"               { return Type(SUB); } 
<KWSTATE>">"               { return Type(GT); } 
<KWSTATE>">="               { return Type(GTE); } 
<KWSTATE>"<="              { return Type(LTE); } 
<KWSTATE>"<"               { return Type(LT); } 
<KWSTATE>"&"               { return Type(LOGAND); } 
<KWSTATE>"^"               { return Type(LOGXOR); } 
<KWSTATE>"|"               { return Type(LOGOR); } 
<KWSTATE>"!="               { return Type(NEQ); } 
<KWSTATE>"=="               { return Type(EQ); } 

<KWSTATE>defmacro               { return Keyword(DEFMACRO); } 
<KWSTATE>defun               { return Keyword(DEFUN); } 
<KWSTATE>listp               { return Keyword(-1); } 


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
"."                   { return OPERATOR; }
"?"                   { return OTHER; }
"`"                   { return OPERATOR; }
"'"                   { return OPERATOR; }
",@"                  { return OPERATOR; }
","                   { return OPERATOR;}

{identifier}          { return Identifier(IDENTIFIER); }


.                     { return PLAIN; }

