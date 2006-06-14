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

#include "gram_VBNET.h"

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_VBNET.c"

keyword1 (AddHandler|AddressOf|Alias|And|AndAlso|As|Boolean|ByRef|Byte|ByVal|Call|Case|Catch|CBool|CByte|CChar)
keyword2 (CDate|CDbl|CDec|Char|CInt|Class|CLng|CObj|Const|Continue|CSByte|CShort|CSng|CStr|CType|CUInt|CULng|CUShort|Date|Decimal)
keyword3 (Declare|Default|Delegate|Dim|DirectCast|Do|Double|Each|Else|ElseIf|End|EndIf|Enum|Erase|Error|Event|Exit|False|Finally|For)
keyword4 (Friend|Function|Get|GetType|Global|GoSub|GoTo|Handles|If|Implements|Imports|In|Inherits|Integer|Interface|Is)
keyword5 (IsNot|Let|Lib|Like|Long|Loop|Me|Mod|Module|MustInherit|MustOverride|MyBase|MyClass|Namespace|Narrowing|New)
keyword6 (Next|Not|Nothing|NotInheritable|NotOverridable|Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides)
keyword7 (ParamArray|Partial|Private|Property|Protected|Public|RaiseEvent|ReadOnly|ReDim|RemoveHandler|Resume|Return|SByte|Select|Set)
keyword8 (Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|Then|Throw|To|True|Try|TryCast|TypeOf|UInteger|ULong)
keyword9 (UShort|Using|Variant|Wend|When|While|Widening|With|WithEvents|WriteOnly|Xor)

keyword ({keyword1}|{keyword2}|{keyword3}|{keyword4}|{keyword5}|{keyword6}|{keyword7}|{keyword8}|{keyword9})	


operator [\~\!\%\^\*\(\)\-\+\=\[\]\|\\\:\;\,\.\/\?\&\<\>]

comment_start          "(*"
comment_end            "*)"

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

single_string_char     [^\"]
quote_esc_seq          \"\"
reg_string_char        {single_string_char}|{simple_esc_seq}|{quote_esc_seq}
regular_string         \"{reg_string_char}*\"
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

("'"|"REM")[^\n]*     { RETURN4(COMMENT); }
                      
{comment_start}       { ENTER(ML_COMMENT); RETURN4(COMMENT); }

<ML_COMMENT>
{
{new_line}            { RETURN4(NEWLINE); }
[^*\n]*               { RETURN4(COMMENT); }
"*"+[^*\)\n]*         { RETURN4(COMMENT); }
{comment_end}         { EXIT(); RETURN4(COMMENT); }
}

                    
{keyword}             { RETURN4(KW); } 
                      
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(CHARACTER); }
{string_literal}      { RETURN4(STRING); }

{operator}            { RETURN4(OP); }                     

{identifier}          { RETURN4(PLAIN); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }
%%

 