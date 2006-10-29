#pragma warning disable 162
using Xacc.ComponentModel;
using System.Drawing;

using LexerBase = Xacc.Languages.CSLex.Language<Xacc.Languages.CSLex.Yytoken>.LexerBase;

namespace Xacc.Languages
{
  sealed class XmlLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "XML"; } }
	  public override string[] Extensions {get { return new string[]{"xml","html","xsl"}; } }
	  protected override LexerBase GetLexer() { return new XmlLexer(); } 
		  
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

mlstring_start            ="\""
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

{ws}			                {;}
\n                        {return NewLine();}

<YYINITIAL>{comment_start}           {ENTER(comment); return Comment();}

<comment>{comment_end}             {EXIT(); return Comment();}
<comment>[^-\n]+                   {return DocComment();}
<comment>\-                        {return DocComment(); }

<YYINITIAL>{pp_tag_start}            {ENTER(pp); return Keyword(); }

<pp>{pp_tag_end}              {EXIT(); return Keyword();}
<pp>[^\n%]+                   {return Custom(KnownColor.Black, KnownColor.Yellow); }
<pp>.                         {return Custom(KnownColor.Black, KnownColor.Yellow); }

<YYINITIAL>{cdatastart}              {ENTER(cdata); return Other(); }

<cdata>{cdataend}                {EXIT(); return Other();}
<cdata>[^\n\]]+                  {return Plain(); }
<cdata>.                         {return Plain(); }

<scriptstart>"="                       {return Operator();}
<scriptstart>{tag_end}                 {EXIT(); return Keyword();}
<scriptstart>{identifier}              {return Number();}
<scriptstart>{string}                  {return String();}
<scriptstart>{tag_mid_end}             {EXIT(); ENTER(script); return Keyword();}

<script>"</"({ws})*"script"({ws})*">" {EXIT(); return Keyword();}
<script>{tag_start}|[^ <\t\n]+    {return Plain();}

<YYINITIAL>{tag_mid_start}           {ENTER(endtag); return Keyword();}
<YYINITIAL>{tag_start}               {ENTER(starttag); return Keyword();}

<starttag>"script"                  {EXIT(); ENTER(scriptstart); return Keyword();}
<starttag>{identifier}              {EXIT(); ENTER(intag); return Keyword();}
<starttag>.                         {return Error(); }

<endtag>{tag_mid_end}             {EXIT(); return Keyword();}
<endtag>{identifier}              {return Keyword();}
<endtag>.                         {return Error(); }

<attr>{mlstring_start}          {ENTER(mlstr); return String();}
<attr>{string}                  {EXIT(); return String();}
<attr>[^ \t\n\"]+               {EXIT(); return String(); }

<intag>"="                       {ENTER(attr); return Operator();}
<intag>{tag_mid_end}             {EXIT(); return Keyword();}
<intag>{tag_end}                 {EXIT(); return Keyword();}
<intag>{identifier}              {return Number();}

<mlstr>{mlstring_end}            {EXIT(); EXIT(); return String();}
<mlstr>[^\"\n]+                  {return String();}

&[a-zA-Z]+\;              {return Other(); }
<YYINITIAL>{identifier}              {return Identifier();}

.                         {return Plain(); }

