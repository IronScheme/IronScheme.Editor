%{
/*  Lexical specification for Yacc/Xacc */

#include "gram_Yacc.h"

static int actstack =  0;
static int markcnt = 0;

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Yacc.c"

%x comment
%x code
%x actcode
%x unioncode

%%

[0-9]+            {RETURN1(NUM);}
"error"           {RETURN2(KW,ERROR);}
"%left"           {RETURN3(KW,LEFT);}
"%right"          {RETURN3(KW,RIGHT);}
"%token"          {RETURN3(KW,TOKEN);}
"%prec"           {RETURN3(KW,PREC);}
"%type"           {RETURN3(KW,TYPE);}
"%start"          {RETURN3(KW,START);}
"%union"          {ENTER(unioncode);RETURN3(KW,UNION);}
"%nonassoc"       {RETURN3(KW,NONASSOC);}
"%name"           {RETURN3(KW,PARSERNAME);}
"%ext"            {RETURN3(KW,PARSEREXT);}
"%{"              {ENTER(code); RETURN4(KW);}
"%%"              { if (markcnt++) ENTER(code); else markcnt = 0; RETURN3(KW,MARK);}
[a-z][_a-z]*      {RETURN1(IDENTIFIER);}
[A-Z][_A-Z]*      {RETURN3(CHARACTER,IDENTIFIER);}
\"[^"]*\"         {RETURN3(STRING,IDENTIFIER);}
\'[^']+\'         {RETURN3(STRING,IDENTIFIER);}
"/*"              {ENTER(comment); yymore();}
\|                {RETURN3(OP,'|');}
\{                {actstack = 0; ENTER(actcode);RETURN3(OP,'{');}
";"               {RETURN3(OP,';');}
":"               {RETURN3(OP,':');}
"<"               {RETURN3(OP,'<');}
">"               {RETURN3(OP,'>');}
[ \t]+            {;}

<code>
{
"%}"                {EXIT(); RETURN4(KW);}
"class"|"enum"|"struct"|"interface"|"delegate"|"base"|"this"|"public"|"protected"|"private"   {RETURN4(KW);}
"internal"|"using"|"namespace"|"static"|"sealed"                                              {RETURN4(KW);}
"abstract"|"override"|"virtual"|"params"|"out"|"ref"|"get"|"set"|"value"                      {RETURN4(KW);}
}

<unioncode>
{
\{                  {RETURN3(OP,'{');}
\}                  {EXIT(); RETURN3(OP,'}');}
[a-zA-Z][_a-zA-Z]*  {RETURN1(IDENTIFIER);}
[ \t]+              {;}
\n                  {RETURN4(NEWLINE);}
.                   {RETURN0();}
}

<actcode>
{
\{                    {actstack++; RETURN4(PLAIN);}
\}                    {if (--actstack){  EXIT(); RETURN3(OP,'}'); } else {RETURN4(PLAIN);}}
"$"("$"|[1-9][0-9]*)  {RETURN4(OTHER); }
"@"("@"|[1-9][0-9]*)  {RETURN4(OTHER); }
}

<code,actcode>
{
[0-9]+              {RETURN4(NUMBER);}
"new"|"null"|"int"|"short"|"uint"|"ushort"|"byte"|"char"|"sbyte"|"ulong"|"float" {RETURN4(KW);}
"bool"|"object"|"long"|"double"|"decimal"|"return"|"break"|"goto"|"for"|"foreach" {RETURN4(KW);}
"while"|"do"|"in"|"string"|"void"|"continue"|"default"|"switch"|"case"|"if"|"else"  {RETURN4(KW);}
"//".*             {RETURN4(COMMENT);}
"/*"                {ENTER(comment); RETURN4(COMMENT);}
\"[^"]*\"           {RETURN4(STRING);}
\'[^']+\'           {RETURN4(STRING);}
[a-zA-Z][_a-zA-Z]*  {RETURN4(PLAIN);}
[ \t]+              {;}
\n                  {RETURN4(NEWLINE);}
.                   {RETURN4(PLAIN);}
}

<comment>
{
\n                { RETURN4(NEWLINE);}
[^*\n]*           { RETURN4(COMMENT); }
"*"+[^*/\n]*      { RETURN4(COMMENT); }
"*"+"/"           { EXIT(); RETURN4(COMMENT); }
}

\n                {RETURN4(NEWLINE);}
.                 {RETURN0();}

%%
