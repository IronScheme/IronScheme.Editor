#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class CSLexLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "CS Lex"; } }
	  public override string[] Extensions {get { return new string[]{"lex"}; } }
	  LexerBase lexer = new CSLexLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CSLexLexer

%state DIRECTIVES RULES
%state RESECT, CODEBLOCK
%state MLCOMMENT

DIRECTSTART								="%%"
CODESTART                 ="%{"
CODEEND                   ="%}"
INITSTART                 ="%init{"
INITEND                   ="%init}"
DIRECTIVE                 ="%"("class"|"state"|"unicode"|full|public|ignorecase)
WS		                    =[ \t]+
OPT_WS                    =[ \t]*
STRING                    =(\"[^\"\n]*\")
IDENTIFIER                =([a-zA-Z_][a-zA-Z_0-9]*)
MACROREF                  ="{"{IDENTIFIER}"}"
STATEREF                  ="<"{OPT_WS}{IDENTIFIER}((({OPT_WS},{OPT_WS})|({WS})){IDENTIFIER})*{OPT_WS}">"
ASSIGN                    ="="
ESCCHAR										=(\\[\\\"\[\]\)\(\|\*\+\-\?\.'a-zA-Z])
CHAR                      =[^\\\n]|([0-9]-[0-9])|([a-z]-[a-z])|([A-Z]-[A-Z])
OPERATOR									="[^"|[\)\(\[\]\|\+\*\-\.\?]
CSTYPES                   ="int"|"uint"|"string"|"char"|"bool"|"object"|"short"|"ushort"
CSTYPES2									="byte"|"sbyte"|"long"|"ulong"|"decimal"|"float"|"double"
CSEXPRKW                  ="break"|"return"|"continue"|"while"|"else"|"if"|"for"|"goto"|"do"|"foreach"|"lock"
CSEXPRKW2									="in"|"as"|"is"|"try"|"catch"|"finally"|"null"|"new"|"switch"|"default"|"throw"
CSKWOTHER                 ="using"|"class"|"public"|"private"|"protected"|"internal"|"sealed"|"override"
CSKWOTHER2								="void"|"params"|"struct"|"enum"|"delegate"|"event"|"interface"|"abstract"
CSKWOTHER3								="virtual"|"namespace"|"get"|"set"|"static"
CSEXPR                    ={CSTYPES}|{CSTYPES2}|{CSEXPRKW}|{CSEXPRKW2}
CSALL                     ={CSEXPR}|{CSKWOTHER}|{CSKWOTHER2}|{CSKWOTHER3}
CSIDENTIFIER              =[a-zA-Z_@][a-zA-Z0-9_]*
CSOPERATOR								=[\)\(\[\]\|\+\*\-%&^=;:\?<>\.,]
CSNUMBER									=[0-9]+(\.[0-9]+)?
CSPP                      =^{OPT_WS}#.*

COMMENT_START          		="/*"
COMMENT_END            		="*"+"/"
LINE_COMMENT           		=("//"[^/\n]*)|"//"

%%

<CODEBLOCK,YYINITIAL>{LINE_COMMENT}		{return (Yytoken)TokenClass.Comment;}
<CODEBLOCK,YYINITIAL>{COMMENT_START}	{ENTER(MLCOMMENT); return COMMENT;}

<MLCOMMENT>[^ \t\n\*]+		            {return COMMENT;}
<MLCOMMENT>{COMMENT_END}	            {EXIT(); return COMMENT;}
<MLCOMMENT>\*		            					{return COMMENT;}

<YYINITIAL>{DIRECTSTART}              {BEGIN(DIRECTIVES); return KEYWORD; }

<DIRECTIVES>{DIRECTIVE}               {return KEYWORD;}
<DIRECTIVES>{ASSIGN}                  {ENTER(RESECT); return OPERATOR;}
<DIRECTIVES>{IDENTIFIER}              {return IDENTIFIER; }
<DIRECTIVES>,                  				{return OPERATOR;}
<DIRECTIVES>{DIRECTSTART}             {BEGIN(RULES); return KEYWORD; }
<DIRECTIVES>{INITSTART}               {ENTER(CODEBLOCK); return KEYWORD; }
<DIRECTIVES>{CODESTART}               {ENTER(YYINITIAL); return KEYWORD; }

<RESECT>\n                            {EXIT(); return NEWLINE;}
<RESECT, RULES>{OPERATOR}             {return OPERATOR;}

<RESECT,RULES,CODEBLOCK,YYINITIAL>{STRING}    {return STRING;}

<RESECT,RULES>{MACROREF}  						{
                                        return (Yytoken)Color.DeepPink;
                                      }

<RULES>{STATEREF}         						{return TYPE;}
<RULES>"{"                						{ ENTER(CODEBLOCK); return OPERATOR; }
<RULES>"}"                						{ EXIT(); return OPERATOR; }

<RESECT RULES>{ESCCHAR}   						{return CHARACTER;}
<RESECT,RULES>{CHAR}   								{return STRING;}

<CODEBLOCK>{INITEND}      						{EXIT(); return KEYWORD;}
<CODEBLOCK>{CSEXPR}       						{return KEYWORD;}
<YYINITIAL>{CODEEND}      						{EXIT(); return KEYWORD;}

<CODEBLOCK>TYPE       						  	{return TYPE;}
<CODEBLOCK>STRING       							{return STRING;}
<CODEBLOCK>"COMMENT"       						{return COMMENT;}
<CODEBLOCK>"KEYWORD"       						{return KEYWORD;}
<CODEBLOCK>"CHARACTER"       					{return CHARACTER;}
<CODEBLOCK>"DOCCOMMENT"       				{return DOCCOMMENT;}
<CODEBLOCK>"NUMBER"       						{return NUMBER;}
<CODEBLOCK>"STRING"       						{return STRING;}
<CODEBLOCK>"OPERATOR"       					{return OPERATOR;}
<CODEBLOCK>"OTHER"       						  {return OTHER;}
<CODEBLOCK>"ERROR"       						  {return ERROR;}
<CODEBLOCK>"PREPROC"     						  {return PREPROC;}

<CODEBLOCK>"BEGIN"       					    {return OPERATOR;}
<CODEBLOCK>"ENTER"       						  {return OPERATOR;}
<CODEBLOCK>"EXIT"       						  {return OPERATOR;}

<YYINITIAL>{CSALL}  									{ return KEYWORD; }
<YYINITIAL>{CSPP}  									  { return PREPROC; }

<CODEBLOCK,YYINITIAL>{CSNUMBER}  			{ return NUMBER; }
<CODEBLOCK,YYINITIAL>{CSOPERATOR}  		{ return OPERATOR; }
<CODEBLOCK,YYINITIAL>"{"           		{ ENTER(State); return OPERATOR; }
<CODEBLOCK,YYINITIAL>"}"           		{ EXIT(); return OPERATOR; }

<CODEBLOCK,YYINITIAL>{CSIDENTIFIER}  	{ return IDENTIFIER; }

{WS}			                            {;}
\n                                    {return NEWLINE;}
.                                     {return ERROR; }
