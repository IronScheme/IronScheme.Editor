
using Xacc.ComponentModel;

#if STANDALONE
[assembly:PluginProvider(typeof(PluginLoader))]

sealed class PluginLoader : AssemblyPluginProvider
{
  public override void LoadAll(IPluginManagerService svc)
  {
    new Xacc.Languages.#NAME#Lang();
  }
}
#endif

namespace Xacc.Languages
{

  sealed class #NAME#Lang : CSLex.Language
  {
	  public override string Name {get {return "#LONGNAME#"; } }
	  public override string[] Extensions {get { return new string[]{#EXTS#}; } }
	  LexerBase lexer = new #NAME#Lexer();
	  protected override LexerBase Lexer
	  {
		  get {return lexer;}
	  }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class #NAME#Lexer

#UNICODE#
#FULL#
#IGNORECASE#

WS		                    =[ \t]+
KEYWORD                   =#KEYWORDS#
PREPROC                   =#PREPROCS#
NUMBER                    =#NUMBERS#
STRING                    =#STRINGS#
CHARACTER                 =#CHARACTERS#
TYPE                      =#TYPES#
OPERATOR                  =#OPERATOR#
LINE_COMMENT              =#LINE_COMMENT#[^\n]*
COMMENT_START             =#COMMENT_START#
COMMENT_END               =#COMMENT_END#
IDENTIFIER                =#IDENTIFIER#

%state ML_COMMENT

%%

<YYINITIAL>{KEYWORD}                  {return KEYWORD;}
<YYINITIAL>{PREPROC}                  {return PREPROC;}
<YYINITIAL>{STRING}                   {return STRING;}
<YYINITIAL>{CHARACTER}                {return CHARACTER;}
<YYINITIAL>{NUMBER}                   {return NUMBER;}
<YYINITIAL>{OPERATOR}                 {return OPERATOR;}
<YYINITIAL>{TYPE}                     {return TYPE;}
<YYINITIAL>{IDENTIFIER}               {return IDENTIFIER;}
<YYINITIAL>{LINE_COMMENT}             {return COMMENT;}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return COMMENT;}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return COMMENT;}
<ML_COMMENT>[^ \t\n\#COMMENT_END_FIRST#]+               {return COMMENT;}
<ML_COMMENT>"#COMMENT_END_FIRST#"                       {return COMMENT;}

{WS}			                            {;}
\n                                    {return NEWLINE;}
.                                     {return #MODE#; }

