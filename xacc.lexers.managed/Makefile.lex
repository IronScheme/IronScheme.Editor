%{
/*  Lexical specification for Makefile */

#include "gram_Makefile.h"

%}

%option 8bit
%option noyywrap
%option nostdinit
%option never-interactive
%option outfile="gram_Makefile.c"

ws		                    [ ]+
comment                   "#".*   

identifier                [\._A-Za-z0-9]+
vardec                    {identifier}{ws}*"="
varstart                  "$"\(
varend                    \)
varref                    {varstart}{identifier}{varend}
ruledec                   ({identifier}|{varref}){ws}*":"

%x COMMAND
%x TARGET
%s RULE

%%

<COMMAND>
{
  {varref}                {RETURN4(PREPROC);}
  {identifier}            {RETURN4(PLAIN);}
  \n                      {EXIT(); RETURN4(NEWLINE); }
  [^\n ]                  {RETURN4(PLAIN);}
}

<TARGET>
{
  \t+                     {ENTER(COMMAND);}
  .                       {yyless(0); EXIT(); }
}

<RULE>
{
  {identifier}            {RETURN4(STRING);}
  \n                      {ENTER(TARGET); RETURN4(NEWLINE); }
}

{comment}                 {RETURN4(COMMENT);}
{ws}			                {;}

{vardec}                  {RETURN4(OTHER);}
{varref}                  {RETURN4(PREPROC);}
{ruledec}                 {ENTER(RULE); RETURN4(KW);}

{varstart}                {RETURN4(OP);}
{varend}                  {RETURN4(OP);}

\n                        {RETURN4(NEWLINE);}
.                         {RETURN4(PLAIN); }

%%
