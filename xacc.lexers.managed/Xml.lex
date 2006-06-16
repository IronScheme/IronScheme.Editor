using Xacc.ComponentModel;
using System.Drawing;

namespace Xacc.Languages
{
  sealed class XmlLanguage : CSLex.Language
  {
	  public override string Name {get {return "XML"; } }
	  public override string[] Extensions {get { return new string[]{"xml","html","xsl"}; } }
	  LexerBase lexer = new XmlLexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
	  
	  public override bool MatchLine(string startline)
    {
      return startline.StartsWith("<");
    }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class XmlLexer
%unicode

ws		                    =[ \t]+
comment_start             ="<!--"
comment_end               ="-->"

cdatastart                ="<![CDATA["
cdataend                  ="]]>"

pp_tag_start              ="<%"
pp_tag_end                ="%>"

tag_mid_start             ="</"
tag_start                 ="<!"|"<?"|"<"
tag_mid_end               =">"
tag_end                   ="?>"|"/>"

mlstring_start            ="\""[^\n\"]*\n
mlstring_end              ="\""
string                    =("\""[^\n\"]*"\"")|("'"[^']*"'")

identifier                =([_a-zA-Z#\.][-#\.:_a-zA-Z0-9]*)

nl		    =\n

%state comment
%state intag
%state starttag
%state endtag
%state cdata
%state script
%state scriptstart
%state pp
%state mlstr
%state attr

%%

<YYINITIAL>{comment_start}           {ENTER(comment); return COMMENT;}

<comment>{comment_end}             {EXIT(); return COMMENT;}
<comment>[^-\n]+                   {return DOCCOMMENT;}
<comment>.                         {return DOCCOMMENT; }

<YYINITIAL>{pp_tag_start}            {ENTER(pp); return KEYWORD; }

<pp>{pp_tag_end}              {EXIT(); return KEYWORD;}
<pp>[^\n%]+                   {return PLAIN; }
<pp>.                         {return PLAIN; }

<YYINITIAL>{cdatastart}              {ENTER(cdata); return OTHER; }

<cdata>{cdataend}                {EXIT(); return OTHER;}
<cdata>[^\n\]]+                  {return PLAIN; }
<cdata>.                         {return PLAIN; }

<scriptstart>"="                       {return OPERATOR;}
<scriptstart>{tag_end}                 {EXIT(); return KEYWORD;}
<scriptstart>{identifier}              {return NUMBER;}
<scriptstart>{string}                  {return STRING;}
<scriptstart>{tag_mid_end}             {EXIT(); ENTER(script); return KEYWORD;}

<script>"</"({ws})*"script"({ws})*">" {EXIT(); return KEYWORD;}
<script>{tag_start}|[^ <\t\n]+    {return PLAIN;}

<YYINITIAL>{tag_mid_start}           {ENTER(endtag); return KEYWORD;}
<YYINITIAL>{tag_start}               {ENTER(starttag); return KEYWORD;}

<starttag>"script"                  {EXIT(); ENTER(scriptstart); return KEYWORD;}
<starttag>{identifier}              {EXIT(); ENTER(intag); return KEYWORD;}
<starttag>.                         {return ERROR; }

<endtag>{tag_mid_end}             {EXIT(); return KEYWORD;}
<endtag>{identifier}              {return KEYWORD;}
<endtag>.                         {return ERROR; }

<attr>{mlstring_start}          {ENTER(mlstr); return STRING;}
<attr>{string}                  {EXIT(); return STRING;}
<attr>[^ \t\n\"]+               {EXIT(); return STRING; }

<intag>"="                       {ENTER(attr); return OPERATOR;}
<intag>{tag_mid_end}             {EXIT(); return KEYWORD;}
<intag>{tag_end}                 {EXIT(); return KEYWORD;}
<intag>{identifier}              {return NUMBER;}

<mlstr>{mlstring_end}            {EXIT(); EXIT(); return STRING;}
<mlstr>[^\"\n]+                  {return STRING;}

<YYINITIAL>&[a-zA-Z]+\;              {return OTHER; }
<YYINITIAL>{identifier}              {return PLAIN;}

{ws}			                {;}
\n                        {return NEWLINE;}

<YYINITIAL>.                         {return PLAIN; }

