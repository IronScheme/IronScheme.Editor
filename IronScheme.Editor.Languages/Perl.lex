%{
/*
 * Copyright (C) 2004  Lorenzo Bettini <bettini@gnu.org>
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

#include "gram_Perl.h"

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Perl.c"

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

white_space            [ \t]
new_line               \n

keyword1 (chomp|chop|chr|crypt|hex|index|lc|lcfirst|length|oct|ord|pack|q|qq|reverse|rindex|sprintf|substr|tr|uc|ucfirst|m|s|qw)
keyword2 (abs|atan2|cos|exp|hex|int|log|oct|rand|sin|sqrt|srand|my|local|our)
keyword3 (delete|each|exists|keys|values|pack|read|syscall|sysread|syswrite|unpack|vec)
keyword4 (undef|unless|return|length|grep|sort|caller|continue|dump|eval|exit|goto|last|next|redo|sub|wantarray)
keyword5 (pop|push|shift|splice|unshift|split|switch|join|defined|foreach|last)
keyword6 (chop|chomp|bless|dbmclose|dbmopen|ref|tie|tied|untie|while|next|map)
keyword7 (eq|die|cmp|lc|uc|and|do|if|else|elsif|for|use|require|package|import)
keyword8 (chdir|chmod|chown|chroot|fcntl|glob|ioctl|link|lstat|mkdir|open|opendir|readlink|rename|rmdir|stat|symlink|umask|unlink|utime)
keyword9 (binmode|close|closedir|dbmclose|dbmopen|die|eof|fileno|flock|format|getc|print|printf|read|readdir|rewinddir|seek|seekdir|select|syscall|sysread|sysseek|syswrite|tell|telldir|truncate|warn|write)
keyword10 (alarm|exec|fork|getpgrp|getppid|get­priority|kill|pipe|qx|setpgrp|setpriority|sleep|system|times|wait|waitpid)
keyword ({keyword1}|{keyword2}|{keyword3}|{keyword4}|{keyword5}|{keyword6}|{keyword7}|{keyword8}|{keyword9}|{keyword10})

other   (($[0-9a-zA-Z_]*)|(@[0-9a-zA-Z_]*)|(%[0-9a-zA-Z_]*))

operator [~!%\^\*\(\)-+=\[\]|\\:;,\./?&<>]

line_comment          "#"[^\n]*

%%

{white_space}+        { ; }
                      
{line_comment}        { RETURN4(COMMENT); }
                    
{keyword}             { RETURN4(KW); } 

{other}               { RETURN4(OTHER); }
                      
{integer_literal}     { RETURN4(NUMBER); }
{real_literal}        { RETURN4(NUMBER); }
{character_literal}   { RETURN4(CHARACTER); }
{string_literal}      { RETURN4(STRING); }

{operator}            { RETURN4(OP); }                     

{identifier}          { RETURN4(PLAIN); }

{new_line}            { RETURN4(NEWLINE);}
.                     { RETURN4(PLAIN); }
%%

