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
<CODEBLOCK,YYINITIAL>{COMMENT_START}	{ENTER(MLCOMMENT); return Comment();}

<MLCOMMENT>[^ \t\n\*]+		            {return Comment();}
<MLCOMMENT>{COMMENT_END}	            {EXIT(); return Comment();}
<MLCOMMENT>\*		            					{return Comment();}

<YYINITIAL>{DIRECTSTART}              {BEGIN(DIRECTIVES); return Keyword(); }

<DIRECTIVES>{DIRECTIVE}               {return Keyword();}
<DIRECTIVES>{ASSIGN}                  {ENTER(RESECT); return Operator();}
<DIRECTIVES>{IDENTIFIER}              {return Identifier(); }
<DIRECTIVES>,                  				{return Operator();}
<DIRECTIVES>{DIRECTSTART}             {BEGIN(RULES); return Keyword(); }
<DIRECTIVES>{INITSTART}               {ENTER(CODEBLOCK); return Keyword(); }
<DIRECTIVES>{CODESTART}               {ENTER(YYINITIAL); return Keyword(); }

<RESECT>\n                            {EXIT(); return NewLine();}
<RESECT, RULES>{OPERATOR}             {return Operator();}

<RESECT,RULES,CODEBLOCK,YYINITIAL>{STRING}    {return String();}

<RESECT,RULES>{MACROREF}  						{ return Other(); }

<RULES>{STATEREF}         						{return Type();}
<RULES>"{"                						{ ENTER(CODEBLOCK); return Operator(); }
<RULES>"}"                						{ EXIT(); return Operator(); }

<RESECT RULES>{ESCCHAR}   						{return Character();}
<RESECT,RULES>{CHAR}   								{return String();}

<CODEBLOCK>{INITEND}      						{EXIT(); return Keyword();}
<CODEBLOCK>{CSEXPR}       						{return Keyword();}
<YYINITIAL>{CODEEND}      						{EXIT(); return Keyword();}

<CODEBLOCK>"Type"       						  	{return Type();}
<CODEBLOCK>"String"       							{return String();}
<CODEBLOCK>"Comment"       						{return Comment();}
<CODEBLOCK>"Keyword"       						{return Keyword();}
<CODEBLOCK>"Character"       					{return Character();}
<CODEBLOCK>"DocComment"       				{return DocComment();}
<CODEBLOCK>"Number"       						{return Number();}
<CODEBLOCK>"String"       						{return String();}
<CODEBLOCK>"Operator"       					{return Operator();}
<CODEBLOCK>"Other"       						  {return Other();}
<CODEBLOCK>"Error"       						  {return Error();}
<CODEBLOCK>"Preprocessor"						  {return Preprocessor();}

<CODEBLOCK>"BEGIN"       					    {return Operator();}
<CODEBLOCK>"ENTER"       						  {return Operator();}
<CODEBLOCK>"EXIT"       						  {return Operator();}

<YYINITIAL>{CSALL}  									{ return Keyword(); }
<YYINITIAL>{CSPP}  									  { return Preprocessor(); }

<CODEBLOCK,YYINITIAL>{CSNUMBER}  			{ return Number(); }
<CODEBLOCK,YYINITIAL>{CSOPERATOR}  		{ return Operator(); }
<CODEBLOCK,YYINITIAL>"{"           		{ ENTER(State); return Operator(); }
<CODEBLOCK,YYINITIAL>"}"           		{ EXIT(); return Operator(); }

<CODEBLOCK,YYINITIAL>{CSIDENTIFIER}  	{ return Identifier(); }

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }
