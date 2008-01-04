#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.IronScheme.LexerBase<Xacc.Languages.IronScheme.ValueType>;

//NOTE: comments are not allowed except in code blocks
%%

%class IronSchemeLexer

%unicode

line_comment           =";"[^\n]*

comment_start          ="#|"
comment_end            ="|#"

white_space            =[ \t]
new_line               =\n

digit                  =[0-9]
letter                 =[a-zA-Z]
idinitial              ={letter}|[!$%\*/:<=>\?~_^&]
subsequent             ={idinitial}|{digit}|[\.\+@]|"-"
identifier             =(({idinitial})({subsequent})*)|"+"|"..."|"-"



digit2                 =[01]
digit8                 =[0-8]
digit10                ={digit}
digit16                ={digit10}|[a-fA-F]

radix2                 =#[bB]
radix8                 =#[oO]
radix10                =(#[dD])?
radix16                =#[xX]

exactness              =(\#[iIeE])?

sign                   =("-"|"+")?

exponentmarker         =[eEsSfFdDlL]

suffix                 =({exponentmarker}{sign}({digit10})+)?

prefix2                =({radix2}{exactness})|({exactness}{radix2})
prefix8                =({radix8}{exactness})|({exactness}{radix8})
prefix10               =({radix10}{exactness})|({exactness}{radix10})
prefix16               =({radix16}{exactness})|({exactness}{radix16})

uinteger2              =({digit2})+
uinteger8              =({digit8})+
uinteger10             =({digit10})+
uinteger16             =({digit16})+

decimal10              =({uinteger10}{suffix})|("."({digit10})+{suffix})|(({digit10})+"."({digit10})*{suffix})|(({digit10})+"."{suffix})

ureal2                 =({uinteger2})|({uinteger2}"/"{uinteger2})
ureal8                 =({uinteger8})|({uinteger8}"/"{uinteger8})
ureal10                =({uinteger10})|({uinteger10}"/"{uinteger10})|({decimal10})
ureal16                =({uinteger16})|({uinteger16}"/"{uinteger16})

real2                  =({sign}{ureal2})
real8                  =({sign}{ureal8})
real10                 =({sign}{ureal10})
real16                 =({sign}{ureal16})

complex2               =({real2}|({real2}"@"{real2})|({real2}"+"{real2}"i")|({real2}"-"{real2}"i")|({real2}"+i")|({real2}"-i")|("+"{real2}"i")|("-"{real2}"i")|("+i")|("-i"))
complex8               =({real8}|({real8}"@"{real8})|({real8}"+"{real8}"i")|({real8}"-"{real8}"i")|({real8}"+i")|({real8}"-i")|("+"{real8}"i")|("-"{real8}"i")|("+i")|("-i"))
complex10              =({real10}|({real10}"@"{real10})|({real10}"+"{real10}"i")|({real10}"-"{real10}"i")|({real10}"+i")|({real10}"-i")|("+"{real10}"i")|("-"{real10}"i")|("+i")|("-i"))
complex16              =({real16}|({real16}"@"{real16})|({real16}"+"{real16}"i")|({real16}"-"{real16}"i")|({real16}"+i")|({real16}"-i")|("+"{real16}"i")|("-"{real16}"i")|("+i")|("-i"))

num2                   =({prefix2}{complex2})
num8                   =({prefix8}{complex8})
num10                  =({prefix10}{complex10})
num16                  =({prefix16}{complex16})

number                 =({num2}|{num8}|{num10}|{num16})



single_char            =[^\n ]
character              ={single_char}|([Nn][Ee][Ww][Ll][Ii][Nn][Ee])|([Ss][Pp][Aa][Cc][Ee])
character_literal      =#\\({character})?

single_string_char     =[^\\\"]
string_esc_seq         =\\[\"\\abfnrtv]
hex_esc_seq            =\\x({digit16})+
reg_string_char        ={single_string_char}|{string_esc_seq}
string_literal         =\"({reg_string_char})*\"

atoms                  =(#[TtFf])

forms                  ={coreforms}|{clrforms}|{coreforms2}|{coreforms3}
coreforms              ="define"|"lambda"|"set!"|"quote"|"if"|"cond"|"case"|"do"|"unless"|"when"|"let"|"let*"|"letrec"|"letrec*"|"library"|"assert"|"define-syntax"|"syntax-case"|"syntax-rules"
coreforms2             ="case-lambda"|"begin"|"or"|"and"|"letrec-syntax"|"let-syntax"|"unquote"|"quasiquote"|"unquote-splicing"|"let-values"|"define-record-type"|"syntax"|"import"|"delay"
coreforms3             ="unsyntax"|"unsyntax-splicing"|"quasisyntax"|"with-syntax"|"identifier-syntax"|"endianess"|"guard"|"define-enumeration"|"define-condition-type"|"record-constructor-descriptor"|"record-type-descriptor"|"let*-values"
clrforms               ="clr-static-event-add!"|"clr-static-event-remove!"|"clr-event-add!"|"clr-event-remove!"|"clr-clear-usings"|"clr-using"|"clr-reference"|"clr-is"|"clr-foreach"|"clr-cast"|"clr-call"|"clr-static-call"|"clr-field-get"|"clr-field-set!"|"clr-static-field-get"|"clr-static-field-set!"|"clr-prop-get"|"clr-prop-set!"|"clr-static-prop-get"|"clr-static-prop-set!"|"clr-indexer-get"|"clr-indexer-set!"|"clr-new"|"clr-new-array"
auxforms               ="export"|"rename"|"except"|"only"|"else"|"=>"|"mutable"|"immutable"|"fields"|"nongenerative"|"parent"|"protocol"|"sealed"|"opaque"|"parent-rtd"|"..."|"_"

procs                  = {baseprocs}|{baseprocs2}|{baseprocs3}|{baseprocs4}|{baseprocs5}|{baseprocs6}|{baseprocs7}|{baseprocs8}|{baseprocs9}|{baseprocs10}
baseprocs              ="car"|"cdr"|"eq?"|"eqv?"|"equal?"|"not"|"null?"|"pair?"|"cons"|"map"|"append"|"list"|"vector"|"list?"|"vector?"|"vector-ref"|"apply"|"error"|"cons*"|"call-with-values"|"values"
baseprocs2             ="call/cc"|"call-wit-current-continuation"|"memv"|"assv"|"memq"|"assq"|"assoc"|"member"|"void"|"boolean?"|"number?"|"char?"|"cadr"|"cddr"|"newline"|"display"|"read"|"write"
baseprocs3             ="+"|"-"|"*"|"/"|"length"|"vector-length"|"="|"vector-set!"|"<="|"<"|">"|">="|"integer->char"|"char->integer"|"for-each"|"char<=?"|"string->list"|"symbol->string"|"procedure?"
baseprocs4             ="string-append"|"string?"|"with-input-from-file"|"with-output-from-file"|"dynamic-wind"|"list->vector"|"make-vector"|"vector->list"|"string->symbol"|"gensym"|"symbol?"
baseprocs5             ="call-with-current-continuation"|"string-ci>?"|"string-ci>=?"|"string-ci<?"|"string-ci<=?"|"char-whitespace?"|"char-upper-case?"|"char-lower-case?"|"char-title-case?"|"char-fold-case?"
baseprocs6             ="char-upcase"|"char-downcase"|"string-upcase"|"string-titlecase"|"string-normalize-nfkd"|"string-normalize-nfkc"|"string-normalize-nfd"|"string-normalize-nfc"|"string-foldcase"|"string-downcase"|"string-ci=?"|"char-numeric?"|"char-general-category"|"char-titlecase"|"char-foldcase"|"char-ci>?"|"char-ci>=?"|"char-ci=?"|"char-ci<?"|"char-ci<=?"|"char-alphabetic?"|"make-variable-transformer"|"identifier?"|"generate-temporaries"|"free-identifier=?"|"syntax->datum"|"datum->syntax"|"bound-identifier=?"|"record-type-descriptor?"|"record-predicate"|"record-mutator"|"record-constructor"|"record-accessor"|"make-record-type-descriptor"|"make-record-constructor-descriptor"|"record?"|"record-type-uid"|"record-type-sealed?"|"record-type-parent"|"record-type-opaque?"|"record-type-name"|"record-type-generative?"|"record-type-field-names"|"record-rtd"|"record-field-mutable?"|"delete-file"|"file-exists?"|"vector-sort!"|"vector-sort"|"list-sort"|"symbol-hash"|"string-ci-hash"|"string-hash"|"equal-hash"|"hashtable-equivalence-function"|"make-hashtable"|"hashtable-hash-function"|"make-eqv-hashtable"|"make-eq-hashtable"|"hashtable?"|"hashtable-update!"|"hashtable-size"|"hashtable-set!"|"hashtable-ref"|"hashtable-mutable?"|"hashtable-keys"|"hashtable-entries"|"hashtable-delete!"|"hashtable-copy"|"hashtable-contains?"|"hashtable-clear!"|"write-char"|"with-output-to-file"|"read-char"|"peek-char"|"open-output-file"|"open-input-file"|"close-output-port"|"close-input-port"|"eof-object?"|"eof-object"|"current-error-port"|"current-output-port"|"current-input-port"|"output-port?"|"input-port?"|"utf-8-codec"|"utf-16-codec"|"transcoder-error-handling-mode"|"transcoder-eol-style"|"transcoder-codec"|"transcoded-port"|"textual-port?"|"string->bytevector"|"standard-output-port"|"standard-input-port"|"standard-error-port"|"set-port-position!"|"put-u8"|"put-string"|"put-datum"|"put-char"|"put-bytevector"|"port?"|"port-transcoder"|"port-position"|"port-has-set-port-position!?"|"port-has-port-position?"|"port-eof?"|"output-port-buffer-mode"|"open-string-output-port"|"open-string-input-port"|"open-file-output-port"|"open-file-input/output-port"|"open-file-input-port"|"open-bytevector-output-port"|"open-bytevector-input-port"|"native-transcoder"|"native-eol-style"|"make-transcoder"|"latin-1-codec"|"make-i/o-write-error"|"make-i/o-read-error"|"make-i/o-port-error"|"make-i/o-invalid-position-error"|"make-i/o-filename-error"|"make-i/o-file-protection-error"|"make-i/o-file-is-read-only-error"|"make-i/o-file-does-not-exist-error"|"make-i/o-file-already-exists-error"|"make-i/o-error"|"make-i/o-encoding-error"|"make-i/o-decoding-error"|"make-custom-textual-output-port"|"make-custom-textual-input/output-port"|"make-custom-textual-input-port"|"make-custom-binary-output-port"|"make-custom-binary-input/output-port"|"make-custom-binary-input-port"|"make-bytevector"|"lookahead-u8"|"lookahead-char"|"i/o-write-error?"|"i/o-read-error?"|"i/o-port-error?"|"i/o-invalid-position-error?"|"i/o-filename-error?"|"i/o-file-protection-error?"|"i/o-file-is-read-only-error?"|"i/o-file-does-not-exist-error?"|"i/o-file-already-exists-error?"|"i/o-error?"|"i/o-error-port"|"i/o-error-filename"|"i/o-encoding-error?"|"i/o-encoding-error-char"|"i/o-decoding-error?"|"get-u8"|"get-string-n!"|"get-string-n"|"get-string-all"|"get-line"|"get-datum"|"get-char"|"get-bytevector-some"|"get-bytevector-n!"|"get-bytevector-n"|"get-bytevector-all"|"flush-output-port"|"close-port"|"exit"|"command-line"|"remove"|"remv"|"remp"|"remq"|"partition"
baseprocs7             ="call-with-input-file"|"call-with-output-file"|"memp"|"exists"|"for-all"|"fold-right"|"fold-left"|"find"|"filter"|"assp"|"call-with-string-output-port"|"call-with-port"|"call-with-bytevector-output-port"|"bytevector->string"|"buffer-mode?"|"binary-port?"|"with-exception-handler"|"raise-continuable"|"raise"|"make-enumeration"|"enum-set=?"|"enum-set-universe"|"enum-set-union"|"enum-set-subset?"|"enum-set-projection"|"enum-set-member?"|"enum-set-intersection"|"enum-set-indexer"|"enum-set-difference"|"enum-set-constructor"|"enum-set-complement"|"enum-set->list"|"who-condition?"|"warning?"|"violation?"|"undefined-violation?"|"syntax-violation?"|"syntax-violation-subform"|"syntax-violation-form"|"syntax-violation"|"simple-conditions"|"serious-condition?"|"non-continuable-violation?"|"&non-continuable"|"message-condition?"|"make-who-condition"|"make-warning"|"make-violation"|"make-undefined-violation"|"make-syntax-violation"|"make-serious-condition"|"make-non-continuable-violation"|"make-message-condition"|"make-lexical-violation"|"make-irritants-condition"|"make-implementation-restriction-violation"|"make-error"|"make-assertion-violation"|"lexical-violation?"|"irritants-condition?"|"implementation-restriction-violation?"|"&implementation-restriction"|"error?"|"condition-who"|"condition-predicate"|"condition-message"|"condition-irritants"|"condition-accessor"|"condition"|"assertion-violation?"|"condition?"|"utf32->string"|"utf16->string"|"utf8->string"|"uint-list->bytevector"|"u8-list->bytevector"|"string->utf8"|"string->utf32"|"string->utf16"|"sint-list->bytevector"|"native-endianness"|"bytevector?"|"bytevector=?"|"bytevector-uint-set!"|"bytevector-uint-ref"|"bytevector-u8-set!"|"bytevector-u8-ref"|"bytevector-u64-set!"|"bytevector-u64-ref"|"bytevector-u64-native-set!"|"bytevector-u64-native-ref"|"bytevector-u32-set!"|"bytevector-u32-ref"|"bytevector-u32-native-set!"|"bytevector-u32-native-ref"|"bytevector-u16-set!"|"bytevector-u16-ref"|"bytevector-u16-native-set!"|"bytevector-u16-native-ref"|"bytevector-sint-set!"|"bytevector-sint-ref"|"bytevector-s8-set!"|"bytevector-s8-ref"|"bytevector-s64-set!"|"bytevector-s64-ref"|"bytevector-s64-native-set!"|"bytevector-s64-native-ref"|"bytevector-s32-set!"|"bytevector-s32-ref"|"bytevector-s32-native-set!"|"bytevector-s32-native-ref"|"bytevector-s16-set!"|"bytevector-s16-ref"|"bytevector-s16-native-set!"|"bytevector-s16-native-ref"|"bytevector-length"|"bytevector-ieee-single-set!"|"bytevector-ieee-single-ref"|"bytevector-ieee-single-native-set!"|"bytevector-ieee-single-native-ref"|"bytevector-ieee-double-set!"|"bytevector-ieee-double-ref"|"bytevector-ieee-double-native-set!"|"bytevector-ieee-double-native-ref"|"bytevector-fill!"|"bytevector-copy!"|"bytevector-copy"|"bytevector->uint-list"|"bytevector->u8-list"|"bytevector->sint-list"|"no-nans-violation?"|"no-infinities-violation?"|"make-no-nans-violation"|"make-no-infinities-violation"|"real->flonum"|"flzero?"|"fltruncate"|"fltan"|"flsqrt"|"flsin"|"flround"|"flpositive?"|"flonum?"|"flodd?"|"flnumerator"|"flnegative?"|"flnan?"|"flmod0"|"flmod"|"flmin"|"flmax"|"fllog"|"flinteger?"|"flinfinite?"|"flfloor"|"flfinite?"|"flexpt"|"flexp"|"fleven?"|"fldiv0-and-mod0"|"fldiv0"|"fldiv-and-mod"|"fldiv"|"fldenominator"|"flcos"|"flceiling"|"flatan"|"flasin"|"flacos"|"flabs"|"fl>?"|"fl>=?"|"fl=?"|"fl<?"|"fl<=?"|"fl/"|"fl-"|"fl+"|"fl*"|"fixnum->flonum"|"fxzero?"|"fxxor"|"fxrotate-bit-field"|"fxreverse-bit-field"|"fxpositive?"|"fxodd?"|"fxnot"|"fxnegative?"|"fxmod0"|"fxmod"|"fxmin"|"fxmax"
baseprocs10            ="fxlength"|"fxior"|"fxif"|"fxfirst-bit-set"|"fxeven?"|"fxdiv0-and-mod0"|"fxdiv0"|"fxdiv-and-mod"|"fxdiv"|"fxcopy-bit-field"|"fxcopy-bit"|"fxbit-set?"|"fxbit-field"|"fxbit-count"|"fxarithmetic-shift-right"|"fxarithmetic-shift-left"|"fxarithmetic-shift"|"fxand"|"fx>?"|"fx>=?"|"fx=?"|"fx<?"|"fx<=?"|"fx-/carry"|"fx-"|"fx+/carry"|"fx+"|"fx*/carry"|"fx*"|"greatest-fixnum"|"least-fixnum"|"fixnum-width"|"fixnum?"|"bitwise-rotate-bit-field"|"bitwise-reverse-bit-field"|"bitwise-length"|"bitwise-if"|"bitwise-first-bit-set"|"bitwise-copy-bit-field"|"bitwise-copy-bit"|"bitwise-bit-set?"|"bitwise-bit-field"|"bitwise-bit-count"|"bitwise-xor"|"bitwise-ior"|"bitwise-and"|"bitwise-not"|"bitwise-arithmetic-shift-right"|"bitwise-arithmetic-shift-left"|"bitwise-arithmetic-shift"|"zero?"|"vector-map"|"vector-for-each"|"vector-fill!"|"truncate"|"tan"|"symbol=?"|"substring"|"string>?"|"string>=?"|"string=?"|"string<?"|"string<=?"|"string-ref"|"string-length"|"string-for-each"|"string-copy"|"string->number"|"string"|"sqrt"|"sin"|"round"|"reverse"|"real?"|"real-valued?"|"real-part"|"rationalize"|"rational?"|"rational-valued?"|"positive?"|"odd?"|"numerator"|"number->string"|"negative?"|"nan?"|"min"|"max"|"make-string"|"make-rectangular"|"make-polar"|"magnitude"|"log"
baseprocs8             ="list-tail"|"list-ref"|"list->string"|"lcm"|"integer?"|"integer-valued?"|"infinite?"|"inexact?"|"inexact"|"imag-part"|"gcd"|"floor"|"finite?"|"expt"|"exp"|"exact?"|"exact-integer-sqrt"|"exact"|"even?"|"div0-and-mod0"|"mod0"|"div0"|"div-and-mod"|"mod"|"div"|"denominator"|"cos"|"complex?"|"char>?"|"char>=?"|"char=?"|"char<?"|"ceiling"|"cadaar"|"cddddr"|"cdddar"|"cddadr"|"cddaar"|"cdaddr"|"cdadar"|"cdaadr"|"cdaaar"|"cadddr"|"caddar"|"cadadr"|"cddaar"|"caaddr"|"caadar"|"caaadr"|"caaaar"|"cdddr"|"cddar"|"cdadr"|"cdaar"|"caddr"|"cadar"|"caadr"|"caaar"|"cdar"|"caar"|"boolean=?"|"atan"|"assertion-violation"|"asin"|"angle"|"acos"|"abs"|"scheme-report-environment"|"quotient"|"null-environment"|"remainder"|"modulo"|"inexact->exact"|"force"|"exact->inexact"|"eval"
baseprocs9             ="environment"|"set-cdr!"|"string-set!"|"exit"|"set-car!"|"string-fill!"|"command-line"|"make-variable-transformer"|"identifier?"|"generate-temporaries"|"free-identifier=?"|"syntax->datum"|"datum->syntax"|"bound-identifier=?"|"syntax-violation"|"delete-file"|"file-exists?"|"make-i/o-write-error"|"make-i/o-read-error"|"make-i/o-port-error"|"make-i/o-invalid-position-error"|"make-i/o-filename-error"|"make-i/o-file-protection-error"|"make-i/o-file-is-read-only-error"|"make-i/o-file-does-not-exist-error"|"make-i/o-file-already-exists-error"|"make-i/o-error"|"i/o-write-error?"|"i/o-read-error?"|"i/o-port-error?"|"i/o-invalid-position-error?"|"i/o-filename-error?"|"i/o-file-protection-error?"|"i/o-file-is-read-only-error?"|"i/o-file-does-not-exist-error?"|"i/o-file-already-exists-error?"|"i/o-error?"|"i/o-error-port"|"i/o-error-filename"|"vector-sort!"|"vector-sort"|"list-sort"
      
%state ML_COMMENT

%%

{white_space}+        { ; }
{new_line}            { return NewLine();}
                      
{comment_start}       { ENTER(ML_COMMENT); return Comment(); }                      
{line_comment}        { return Comment(); }

<ML_COMMENT>[^\n\|]+         { return Comment(); }
<ML_COMMENT>{comment_end}     { EXIT(); return Comment(); }
<ML_COMMENT>"|"               { return Comment(); }
 
{atoms}               { return Number(LITERAL); } 

{auxforms}            { return Other(SYMBOL); }
{forms}               { return Keyword(SYMBOL); }
{procs}               { return Type(SYMBOL); }


"("                   { return Operator(LBRACE); }                     
")"                   { return Operator(RBRACE); } 
"["                   { return Operator(LBRACK); }                     
"]"                   { return Operator(RBRACK); } 
"#("                  { return Operator(VECTORLBRACE); }
"#vu8("               { return Operator(BYTEVECTORLBRACE); }

"`"                   { return Operator(QUASIQUOTE); }
"'"                   { return Operator(QUOTE); }
",@"                  { return Operator(UNQUOTESPLICING); }
","                   { return Operator(UNQUOTE);}

"#`"                   { return Operator(QUASISYNTAX); }
"#'"                   { return Operator(SYNTAX); }
"#,@"                  { return Operator(UNSYNTAXSPLICING); }
"#,"                   { return Operator(UNSYNTAX);}



{character_literal}   { return Number(CHARACTER); }                      
{number}              { return Number(NUMBER); }
{string_literal}      { return String(STRING); }

{identifier}          { return Identifier(SYMBOL); }

"."                   { return Operator(DOT); }

.                     { return Error(); }

