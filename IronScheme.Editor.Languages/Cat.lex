using IronScheme.Editor.ComponentModel;
using System.Drawing;

using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class CatLanguage : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Cat"; } }
	  public override string[] Extensions {get { return new string[]{"cat"}; } }
	  protected override LexerBase GetLexer() { return new CatLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class CatLexer

%unicode

keyword   =(define)
preproc   =(macro)
type      =(int|bool|list|var|function|type|byte|char|string|dbl|bit|byte_block|istream|ostream)
func_core =(compose|cons|dec|dip|dup|eq|if|inc|pop|qv|to_list|uncons)
func_two  =(dispatch1|dispatch2|invoke1|invoke2|throw|try|type_of|app2|eval|y|and|nand|nor|not|or|eqz|eqf|neq|neqf|neqz|curry|curry2|rcompose|rcurry|bin_rec|for|for_each|repeat|rfor|while|whilen|whilene|whilenz|cat|count|count_while|drop|drop_while|empty|filter|first|flatten|fold|gen|head|last|map|mid|move_head|n|nil|nth|pair|rev|rmap|set_at|small|split|split_at|tail|take|take_while|unit|bury|dig|dup2|dupd|over|peek|poke|pop2|popd|swap|swap2|swapd|under)
func      ={func_core}|{func_two}

type_var  =("'"[a-z]{alpha_num}*)
stack_var =("'"[A-Z]{alpha_num}*)
esc_char  =(\\[^\n])
string_lit=(\"({internal_char})*\")
char_lit  =(\'({internal_char})\')
internal_char =({esc_char}|[^\n])
float_lit =({integer}\.{integer})
binary    =0b([01])+
hex_lit   =0[xX]({hex_digit})+
integer   =({number})+
hex_digit =[0-9a-fA-F]
number    =[0-9]
alpha_num =[a-zA-Z0-9_]
identifier=({alpha_num})+

operator =("-"|"~"|"!"|"%"|"^"|"*"|"("|")"|"+"|"["|"]"|"|"|"\\""|":"|";"|","|"."|"/"|"?"|"&"|"<"|">")

comment_start          ="/*"
comment_end            ="*"+"/"

line_comment           ="//".*

white_space            =[ \t]
new_line               =\n


%state ML_COMMENT

%%

({white_space})+      { ; }
                      
<YYINITIAL>{comment_start}       { ENTER(ML_COMMENT); return Comment(); }

<ML_COMMENT>[^\*\n]*               { return Comment(); }
<ML_COMMENT>"*"+[^\*/\n]*          { return Comment(); }
<ML_COMMENT>{comment_end}         { EXIT(); return Comment(); }

{line_comment}        { return Comment(); }

{preproc}              {return Preprocessor();}

{type}               { return Type(); } 
{type_var}               { return Type(); } 
{stack_var}             { return Other(); }
{func}                { return Keyword(); } 
{keyword}             { return Keyword(); } 

{binary}        { return Number(); }
{hex_lit}        { return Number(); }
{float_lit}     { return Number(); }
{integer}        { return Number(); }

{char_lit}   { return String(); }
{string_lit}      { return String(); }

{operator}            { return Operator(); }                     

{identifier}          { return Plain(); }

{new_line}            { return NewLine();}
.                     { return Plain(); }


