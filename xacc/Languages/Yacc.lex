#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;
using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class YaccLang : CSLex.Language<Yytoken>
  {
	  public override string Name {get {return "Yacc"; } }
	  public override string[] Extensions {get { return new string[]{"y"}; } }
	  LexerBase lexer = new YaccLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class YaccLexer

%{

int actstack =  0;
int markcnt = 0;

%}

%full

%state comment
%state code
%state actcode
%state unioncode

%%


[ \t]+            {;}


<code>"%}"                {EXIT(); return Keyword();}
<code>"class"|"enum"|"struct"|"interface"|"delegate"|"base"|"this"|"public"|"protected"|"private"   {return Keyword();}
<code>"internal"|"using"|"namespace"|"static"|"sealed"                                              {return Keyword();}
<code>"abstract"|"override"|"virtual"|"params"|"out"|"ref"|"get"|"set"|"value"                      {return Keyword();}


<unioncode>\{                  {return Operator();}
<unioncode>\}                  {EXIT(); return Operator();}
<unioncode>[a-zA-Z][_a-zA-Z]*  {return Identifier();}
<unioncode>.                   {return Plain();}


<actcode>\{                    {actstack++; return Plain();}
<actcode>\}                    {if (--actstack != 0){  EXIT(); return Operator();} else {return Plain();}}
<actcode>"$"("$"|[1-9][0-9]*)  {return Other(); }
<actcode>"@"("@"|[1-9][0-9]*)  {return Other(); }



<code,actcode>[0-9]+              {return Number();}
<code,actcode>"new"|"null"|"int"|"short"|"uint"|"ushort"|"byte"|"char"|"sbyte"|"ulong"|"float" {return Keyword();}
<code,actcode>"bool"|"object"|"long"|"double"|"decimal"|"return"|"break"|"goto"|"for"|"foreach" {return Keyword();}
<code,actcode>"while"|"do"|"in"|"string"|"new"|"void"|"continue"|"default"|"switch"|"case"|"if"|"else"  {return Keyword();}
<code,actcode>"//".*             {return Comment();}
<code,actcode>"/*"                {ENTER(comment); return Comment();}
<code,actcode>\"[^\"]*\"           {return String();}
<code,actcode>\'[^']+\'           {return String();}
<code,actcode>[a-zA-Z][_a-zA-Z]*  {return Plain();}
<code,actcode>.                   {return Plain();}

<comment>[^*\n]*           { return Comment(); }
<comment>"*"+[^*/\n]*      { return Comment(); }
<comment>"*"+"/"           { EXIT(); return Comment(); }

[0-9]+            {return Number();}
"error"           {return Keyword();}
"%left"           {return Keyword();}
"%right"          {return Keyword();}
"%token"          {return Keyword();}
"%prec"           {return Keyword();}
"%namespace"      {return Keyword();}
"%start"          {return Keyword();}
"%union"          {ENTER(unioncode);return Keyword();}
"%nonassoc"       {return Keyword();}
"%{"              {ENTER(code); return Keyword();}
"%%"              { if (markcnt++ != 0) ENTER(code); else markcnt = 0; return Keyword();}
[a-z][_a-z]*      {return Identifier();}
[A-Z][_A-Z]*      {return Identifier();}
\'[^']+\'         {return String();}
<YYINITIAL>"/*"              {ENTER(comment); return Comment();}
\|                {return Operator();}
\{                {actstack = 0; ENTER(actcode);return Operator();}
";"               {return Operator();}
":"               {return Operator();}
"<"               {return Operator();}
">"               {return Operator();}

\n                {return NewLine();}
.                 {return Plain();}


