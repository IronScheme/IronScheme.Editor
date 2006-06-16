using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class ILLanguage : CSLex.Language
  {
	  public override string Name {get {return "MSIL"; } }
	  public override string[] Extensions {get { return new string[]{"il"}; } }
	  LexerBase lexer = new ILLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class ILLexer
%full

single_line_comment    ="//".*

white_space            =[ \t]
new_line               =\n

preprocessor           =^({white_space})*#({white_space})*

attr                   =\[({white_space})*(assembly|return)({white_space})*:

dec_digit              =[0-9]
hex_digit              =[0-9A-Fa-f]
int_suffix             =[UuLl]|[Uu][Ll]|[Ll][Uu]
dec_literal            =({dec_digit})+({int_suffix})?
hex_literal            =0[xX]({hex_digit})+({int_suffix})?
integer_literal        ={dec_literal}|({hex_digit}{hex_digit})|{hex_literal}

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
string_literal         ={regular_string}

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"_"|"$")
identifier             =(({letter_char}|"_")({ident_char})*)
at_identifier          ='{identifier}'
ws_identifier          ={identifier}(({white_space})+{identifier})*

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

t1 =(".zeroinit"|".vtfixup"|".vtentry"|".vtable"|".ver"|".try"|".subsystem"|".size"|".set"|".removeon"|".publickeytoken"|".publickey"|".property"|".permissionset"|".permission")
t2 =(".param"|".pack"|".override"|".other"|".namespace"|".mresource"|".module"|".method"|".maxstack"|".locals"|".locale"|".line"|".language"|".imagebase"|".hash"|".get")
t3 =(".fire"|".file"|".field"|".export"|".event"|".entrypoint"|".emitbyte"|".data"|".custom"|".ctor"|".corflags"|".class"|".cctor"|".assembly"|".addon")

type =({t1}|{t2}|{t3})


k1=("nativeint"|"uint32"|"uint64"|"uint16"|"with"|"winapi"|"void"|"virtual"|"variant"|"vararg"|"valuetype"|"value"|"userdefined"|"unsigned"|"unmanagedexp"|"unmanaged")
k2=("unicode"|"typedref"|"true"|"to"|"tls"|"thiscall"|"tbstr"|"sysstring"|"syschar"|"synchronized"|"struct"|"string"|"streamed_object"|"stream"|"stored_object")
k3=("storage"|"stdcall"|"static"|"specialname"|"serializable"|"sequential"|"sealed"|"safearray"|"runtime"|"rtspecialname"|"request"|"reqsecobj"|"reqrefuse"|"reqOPERATORt")
k4=("reqmin"|"record"|"public"|"privatescOPERATORe"|"private"|"preservesig"|"prejitgrant"|"prejitdeny"|"pinvokeimpl"|"pinned"|"permitonly"|"out"|"OPERATORtil")
k5=("OPERATORt"|"objectref"|"object"|"nullref"|"null"|"notserialized"|"nOPERATORrocess"|"noncaslinkdemand"|"noncasinheritance"|"noncasdemand"|"nometadata"|"nomangle")
k6=("nomachine"|"noinlining"|"noappdomain"|"newslot"|"nested"|"native"|"modreq"|"modOPERATORt"|"method"|"marshal"|"managed"|"lpwstr"|"lptstr"|"lpstruct"|"lpstr")
k7=("literal"|"linkcheck"|"lasterr"|"iunknown"|"implements"|"internalcall"|"interface"|"int8"|"int64"|"int32"|"int16"|"instance"|"initonly"|"init"|"inheritcheck")
k8=("in"|"il"|"import"|"idispatch"|"hresult"|"hidebysig"|"handler"|"fromunmanaged"|"forwardref"|"float64"|"float32"|"fixed"|"finally"|"final"|"filter"|"filetime")
k9=("field"|"fault"|"fastcall"|"famorassem"|"family"|"famandassem"|"false"|"extern"|"extends"|"explicit"|"error"|"enum"|"deny"|"demand"|"default"|"decimal"|"date")
k10=("constrained."|"custom"|"currency"|"clsid"|"class"|"cil"|"char"|"cf"|"cdecl"|"catch"|"carray"|"callmostderived"|"byvalstr"|"bytearray"|"bstr"|"bool"|"blob_object")
k11=("blob"|"beforefieldinit"|"autochar"|"auto"|"at"|"assert"|"assembly"|"as"|"array"|"any"|"ansi"|"alignment"|"algorithm"|"abstract")

keyword =({k1}|{k2}|{k3}|{k4}|{k5}|{k6}|{k7}|{k8}|{k9}|{k10}|{k11})

i1=("xor"|"volatile."|"unbox"|"unaligned."|"throw"|"tail."|"switch"|"sub.ovf.un"|"sub.ovf"|"sub"|"stsfld"|"stobj"|"stloc.s"|"stloc.3"|"stloc.2"|"stloc.1"|"stloc.0")
i2=("stloc"|"stind.ref"|"stind.r8"|"stind.r4"|"stind.i8"|"stind.i4"|"stind.i2"|"stind.i1"|"stind.i"|"stfld"|"stelem.ref"|"stelem.r8"|"stelem.r4"|"stelem.i8"|"stelem.i4")
i3=("stelem.i2"|"stelem.i1"|"stelem.i"|"starg.s"|"starg"|"sizeof"|"shr.un"|"shr"|"shl"|"rethrow"|"ret"|"rem.un"|"rem"|"refanyval"|"refanytype"|"prefixref"|"prefix7")
i4=("prefix6"|"prefix5"|"prefix4"|"prefix3"|"prefix2"|"prefix1"|"pOPERATOR"|"pop"|"or"|"not"|"nop"|"nOPERATOR"|"newobj"|"newarr"|"neg"|"mul.ovf.un"|"mul.ovf"|"mul")
i5=("mkrefany"|"localloc"|"leave.s"|"leave"|"ldvirtftn"|"ldtoken"|"ldstr"|"ldsflda"|"ldsfld"|"ldobj"|"ldnull"|"ldloca.s"|"ldloca"|"ldloc.s"|"ldloc.3"|"ldloc.2")
i6=("ldloc.1"|"ldloc.0"|"ldloc"|"ldlen"|"ldind.u8"|"ldind.u4"|"ldind.u2"|"ldind.u1"|"ldind.ref"|"ldind.r8"|"ldind.r4"|"ldind.i8"|"ldind.i4"|"ldind.i2"|"ldind.i1")
i7=("ldind.i"|"ldftn"|"ldflda"|"ldfld"|"ldelema"|"ldelem.u8"|"ldelem.u4"|"ldelem.u2"|"ldelem.u1"|"ldelem.ref"|"ldelem.r8"|"ldelem.r4"|"ldelem.i8"|"ldelem.i4")
i8=("ldelem.i2"|"ldelem.i1"|"ldelem.i"|"ldc.r8"|"ldc.r4"|"ldc.i8"|"ldc.i4.s"|"ldc.i4.m1"|"ldc.i4.M1"|"ldc.i4.8"|"ldc.i4.7"|"ldc.i4.6"|"ldc.i4.5"|"ldc.i4.4"|"ldc.i4.3")
i9=("ldc.i4.2"|"ldc.i4.1"|"ldc.i4.0"|"ldc.i4"|"ldarga.s"|"ldarga"|"ldarg.s"|"ldarg.3"|"ldarg.2"|"ldarg.1"|"ldarg.0"|"ldarg"|"jmp"|"isinst"|"initobj"|"initblk"|"illegal")
i10=("endmac"|"endfinally"|"endfilter"|"endfault"|"dup"|"div.un"|"div"|"cpobj"|"cpblk"|"conv.u8"|"conv.u4"|"conv.u2"|"conv.u1"|"conv.u"|"conv.r8"|"conv.r4"|"conv.r.un")
i11=("conv.ovf.u8.un"|"conv.ovf.u8"|"conv.ovf.u4.un"|"conv.ovf.u4"|"conv.ovf.u2.un"|"conv.ovf.u2"|"conv.ovf.u1.un"|"conv.ovf.u1"|"conv.ovf.u.un"|"conv.ovf.u")
i12=("conv.ovf.i8.un"|"conv.ovf.i8"|"conv.ovf.i4.un"|"conv.ovf.i4"|"conv.ovf.i2.un"|"conv.ovf.i2"|"conv.ovf.i1.un"|"conv.ovf.i1"|"conv.ovf.i.un"|"conv.ovf.i")
i13=("conv.i8"|"conv.i4"|"conv.i2"|"conv.i1"|"conv.i"|"clt.un"|"clt"|"ckfinite"|"cgt.un"|"cgt"|"ceq"|"castclass"|"callvirt"|"calli"|"call"|"brzero.s"|"brzero")
i14=("brtrue.s"|"brtrue"|"brnull.s"|"brnull"|"brinst.s"|"brinst"|"brfalse.s"|"brfalse"|"break"|"br"|"br.s"|"box"|"bne.un.s"|"bne.un"|"blt.un.s"|"blt.un"|"blt.s")
i15=("blt"|"ble.un.s"|"ble.un"|"ble.s"|"ble"|"bgt.un.s"|"bgt.un"|"bgt.s"|"bgt"|"bge.un.s"|"bge.un"|"bge.s"|"bge"|"beq.s"|"beq"|"arglist"|"and"|"add.ovf.un"|"add.ovf"|"add")
    
inst =({i1}|{i2}|{i3}|{i4}|{i5}|{i6}|{i7}|{i8}|{i9}|{i10}|{i11}|{i12}|{i13}|{i14}|{i15})


%state IN_COMMENT

%%

{white_space}+    { ; /* ignore */ }

                     
"/*"              { ENTER(IN_COMMENT); return COMMENT; }


<IN_COMMENT>[^*\n]*           { return COMMENT; }
<IN_COMMENT>"*"+[^*/\n]*      { return COMMENT; }
<IN_COMMENT>"*"+"/"           { EXIT(); return COMMENT; }

{single_line_comment} { return COMMENT; }

"IL_"{hex_digit}{hex_digit}{hex_digit}{hex_digit}":"?  { return DOCCOMMENT; }

{type}                  {return TYPE;}                    
{keyword}               {return KEYWORD;}
{inst}                  {return OTHER;}
                      
{integer_literal}     {  return NUMBER; }
{real_literal}        {  return NUMBER; }
{character_literal}   {  return CHARACTER; }
{string_literal}      {  return STRING; }

                      
","   {  return OPERATOR; }
"["   {  return OPERATOR; }
"]"   {  return OPERATOR; }
"&"   {  return OPERATOR; }

{rank_specifier}     {  return OPERATOR; }



{identifier}             {  return PLAIN; }
{at_identifier}          {  return PLAIN; }

\n                       {  return NEWLINE;}
.                        {  return PLAIN; }


 
