using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class HLSLLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "HLSL"; } }
	  public override string[] Extensions {get { return new string[]{"fx"}; } }
	  LexerBase lexer = new CssLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class HLSLLexer

keyword1  =(do|while|in|for|sampler_state|discard|return|if|else|register|sampler|technique|pass|texture|compile|extern|shared|static|uniform|volatile)
keyword2	=(const|row_major|col_major|vector|matrix|typedef|pixelshader|vertexshader|string|stateblock|stateblock_state|void|sampler1D|sampler2D|sampler3D|samplerCUBE)
keyword3  =(texture1D|texture2D|texture3D|textureCUBE|pixelfragment|vertexfragment|inline|inout|out|NULL|LINEAR|struct|true|false) 

keyword   =({keyword1}|{keyword2}|{keyword3})

functions1  =(mul|lerp|tex2D|tex1D|texCUBE|normalize|frac|dot|pow|saturate|reflect|sin|cos|tan|exp|min|max|cross|abs|acos|all|any|asin|atan|atan2)
functions2	=(ceil|clamp|cosh|degrees|determinant|exp2|floor|fmod|frexp|isfinite|isinf|isnan|ldexp|lit|log|log2|log10|modf|noise|radians|round|rsqrt)
functions3  =(sign|sincos|sinh|smoothstep|step|sqrt|tanh|transpose|distance|faceforward|length|refract|tex1Dproj|tex1Dbias|tex2Dproj|tex2Dbias)
functions4	=(texRECTbias|tex3D|tex3Dproj|tex3Dbias|texCUBEproj|texCUBEbias|ddx|ddy|debug)

function   =({functions1}|{functions2}|{functions3}|{functions4})

range =[1-4]
			
type1    =(int({range}(x{range})?)?|float({range}(x{range})?)?|double({range}(x{range})?)?|half({range}(x{range})?)?|bool({range}(x{range})?)?)
type2   =(COLOR[01]?|POSITION|TEXCOORD[0-7]|BINORMAL[0-9]|BLENDINDICES[0-9]|BLENDWEIGHT[0-9]|PSIZE[0-9]|TANGENT[0-9]TESSFACTOR[0-9]|POSITIONT|NORMAL|FOG|VFACE|DEPTH|DEPTH[0-9]|WORLDVIEW|VPOS)	
			
type  =({type1}|{type2})

operator =("-"|"~"|"!"|"%"|"^"|"*"|"("|")"|"+"|"["|"]"|"|"|"\\""|":"|";"|","|"."|"/"|"?"|"&"|"<"|">")

comment                ="//"[^\n]*

comment_start          ="/*"
comment_end            ="*/"

white_space            =[ \t]
new_line               =\n

preprocessor           =^{white_space}*#{white_space}*

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
hex_esc_seq            =\\x({hex_digit})?({hex_digit})?({hex_digit})?{hex_digit}
character              ={single_char}|{simple_esc_seq}|{hex_esc_seq}|
character_literal      ='({character})'

single_string_char     =[^\\\"\n]
string_esc_seq         =\\[\"\\abfnrtv]
reg_string_char        ={single_string_char}|{string_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         =\"({reg_string_char})*\"
error_string           =\"({reg_string_char})*
string_literal         ={regular_string}

letter_char            =[A-Za-z]
ident_char             =({dec_digit}|{letter_char}|"_"|"@")
identifier             =({letter_char}|"_")({ident_char})*
at_identifier          =\@{identifier}
ws_identifier          =({identifier}(({white_space})+{identifier})*)

rank_specifier         ="["({white_space})*(","({white_space})*)*"]"

%state ML_COMMENT

%%

<YYINITIAL>{preprocessor}[^\n].+ { return PREPROC; }
({white_space})+      { ; }

{comment}             { return COMMENT; }                      
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return COMMENT; }


<ML_COMMENT>[^*\n]+               { return COMMENT; }
<ML_COMMENT>"*"                   { return COMMENT; }
<ML_COMMENT>{comment_end}         { EXIT(); return COMMENT; }
                    
{keyword}             { return KEYWORD; } 
{type}                { return TYPE; }
{function}            { return OTHER;}
                      
{integer_literal}     { return NUMBER; }
{real_literal}        { return NUMBER; }

{operator}            { return OPERATOR; }                     
{identifier}          { return IDENTIFIER; }

{new_line}            { return NEWLINE;}
.                     { return PLAIN; }



 