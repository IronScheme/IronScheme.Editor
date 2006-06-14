%{
/*
 * Copyright (C) 2003  Lorenzo Bettini <bettini@gnu.org>
 * Copyright (C) 2005  Llewellyn Pritchard (leppie) <llewellyn@pritchard.org> 
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 */

#include "gram_HLSL.h"

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_HLSL.c"

keyword1  (do|while|in|for|sampler_state|discard|return|if|else|register|sampler|technique|pass|texture|compile|extern|shared|static|uniform|volatile)
keyword2	(const|row_major|col_major|vector|matrix|typedef|pixelshader|vertexshader|string|stateblock|stateblock_state|void|sampler1D|sampler2D|sampler3D|samplerCUBE)
keyword3  (texture1D|texture2D|texture3D|textureCUBE|pixelfragment|vertexfragment|inline|inout|out|NULL|LINEAR|struct|true|false) 

keyword   ({keyword1}|{keyword2}|{keyword3})

functions1  (mul|lerp|tex2D|tex1D|texCUBE|normalize|frac|dot|pow|saturate|reflect|sin|cos|tan|exp|min|max|cross|abs|acos|all|any|asin|atan|atan2)
functions2	(ceil|clamp|cosh|degrees|determinant|exp2|floor|fmod|frexp|isfinite|isinf|isnan|ldexp|lit|log|log2|log10|modf|noise|radians|round|rsqrt)
functions3  (sign|sincos|sinh|smoothstep|step|sqrt|tanh|transpose|distance|faceforward|length|refract|tex1Dproj|tex1Dbias|tex2Dproj|tex2Dbias)
functions4	(texRECTbias|tex3D|tex3Dproj|tex3Dbias|texCUBEproj|texCUBEbias|ddx|ddy|debug)

function   ({functions1}|{functions2}|{functions3}|{functions4})

range [1-4]
			
type1    (int({range}(x{range})?)?|float({range}(x{range})?)?|double({range}(x{range})?)?|half({range}(x{range})?)?|bool({range}(x{range})?)?)
type2   (COLOR[01]?|POSITION|TEXCOORD[0-7]|BINORMAL[0-9]|BLENDINDICES[0-9]|BLENDWEIGHT[0-9]|PSIZE[0-9]|TANGENT[0-9]TESSFACTOR[0-9]|POSITIONT|NORMAL|FOG|VFACE|DEPTH|DEPTH[0-9]|WORLDVIEW|VPOS)	
			
type  ({type1}|{type2})

operator [\~\!\%\^\*\(\)\-\+\=\[\]\|\\\:\;\,\.\/\?\&\<\>]

comment                "//"[^\n]*

comment_start          "/*"
comment_end            "*/"

white_space            [ \t]
new_line               \n

preprocessor           ^{white_space}*#{white_space}*

dec_digit              [0-9]
hex_digit              [0-9A-Fa-f]
int_suffix             [UuLl]|[Uu][Ll]|[Ll][Uu]
dec_literal            {dec_digit}+{int_suffix}?
hex_literal            0[xX]{hex_digit}+{int_suffix}?
integer_literal        {dec_literal}|{hex_literal}

real_suffix            [FfDdMm]
sign                   [+\-]
exponent_part          [eE]{sign}?{dec_digit}+
whole_real1            {dec_digit}+{exponent_part}{real_suffix}?
whole_real2            {dec_digit}+{real_suffix}
part_real              {dec_digit}*\.{dec_digit}+{exponent_part}?{real_suffix}?
real_literal           {whole_real1}|{whole_real2}|{part_real}

single_char            [^\\\']
simple_esc_seq         \\[\'\"\\0abfnrtv]
uni_esc_seq1           \\u{hex_digit}{4}
uni_esc_seq2           \\U{hex_digit}{8}
uni_esc_seq            {uni_esc_seq1}|{uni_esc_seq2}
hex_esc_seq            \\x{hex_digit}{1,4}
character              {single_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
character_literal      \'{character}\'

single_string_char     [^\\\"]
reg_string_char        {single_string_char}|{simple_esc_seq}|{hex_esc_seq}|{uni_esc_seq}
regular_string         \"{reg_string_char}*\"
quote_esc_seq          \"\"
string_literal         {regular_string}

letter_char            [A-Za-z]
ident_char             {dec_digit}|{letter_char}|"_"|"@"
identifier             ({letter_char}|"_"){ident_char}*
at_identifier          \@{identifier}
ws_identifier          {identifier}({white_space}+{identifier})*

rank_specifier         "["{white_space}*(","{white_space}*)*"]"

%x ML_COMMENT

%%

{preprocessor}[^\n].+ { RETURN4(PREPROC); }
{white_space}+        { ; }

{comment}             { RETURN4(COMMENT); }                      
{comment_start}       { ENTER(ML_COMMENT); RETURN4(COMMENT); }

<ML_COMMENT>
{
{new_line}            { RETURN4(NEWLINE); }
[^*\n]+               { RETURN4(COMMENT); }
"*"                   { RETURN4(COMMENT); }
{comment_end}         { EXIT(); RETURN4(COMMENT); }
}
                    
{keyword}             { RETURN3(KW, ID); } 
{type}                { RETURN3(TYPE, ID); }
{function}            { RETURN3(OTHER, ID);}
                      
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }

{operator}            { RETURN4(OP); }                     
{identifier}          { RETURN3(IDENTIFIER, ID); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }
%%

 