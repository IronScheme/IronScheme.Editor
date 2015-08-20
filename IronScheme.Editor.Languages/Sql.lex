using IronScheme.Editor.ComponentModel;


using LexerBase = IronScheme.Editor.Languages.CSLex.Language<IronScheme.Editor.Languages.CSLex.Yytoken>.LexerBase;

namespace IronScheme.Editor.Languages
{
  sealed class SqlLang : CSLex.Language<CSLex.Yytoken>
  {
	  public override string Name {get {return "Sql"; } }
	  public override string[] Extensions {get { return new string[]{"sql"}; } }
	  protected override LexerBase GetLexer() { return new SqlLexer(); }
  }
}
//NOTE: comments are not allowed except in code blocks
%%

%class SqlLexer
%full
%ignorecase

dec_digit              =[0-9]
hex_digit              =[0-9A-Fa-f]
int_suffix             =[UuLl]|[Uu][Ll]|[Ll][Uu]
dec_literal            =({dec_digit})+({int_suffix})?
hex_literal            =0[xX]({hex_digit})+({int_suffix})?
integer_literal        ={dec_literal}|{hex_literal}

real_suffix            =[FfDdMm]
sign                   =[-\+]
exponent_part          =[eE]({sign})?({dec_digit})+
whole_real1            =({dec_digit})+{exponent_part}({real_suffix})?
whole_real2            =({dec_digit})+{real_suffix}
part_real              =({dec_digit})*\.({dec_digit})+({exponent_part})?({real_suffix})?
real_literal           ={whole_real1}|{whole_real2}|{part_real}

WS		                    =[ \t]+
KEYWORD                   =("DELETE"|"IN"|"UNIQUE"|"NONCLUSTERED"|"FOR"|"ROWGUIDCOL"|"PRIMARY"|"DEFAULT"|"DATABASE"|"OFF"|"LOG"|"open"|"order by"|"deallocate"|"fetch"|"goto"|"while"|"NOT"|"IF"|"IS"|"COLLATE"|"ADD"|"ALL"|"NOCHECK"|"GROUP BY"|"UNION"|"CHECK"|"INDEX"|"CONSTRAINT"|"CLUSTERED"|"identity"|"null"|"exec"|"use"|"drop"|"exists"|"view"|"primary key"|"references"|"not null"|"foreign key"|"SELECT"|"INSERT"|"INTO"|"VALUES"|"FROM"|"THEN"|"BEGIN"|"END"|"AS"|"AND"|"ON"|"OR"|"WHERE"|"ORDERBY"|"GROUPBY"|"CASE"|"INNER"|"OUTER"|"CROSS"|"JOIN"|"LEFT"|"RIGHT"|"RETURN"|"GO"|"DECLARE"|"SET"|"ELSE"|"WHEN"|"CREATE"|"ALTER"|"TABLE")
BRACE_KEYWORD             =\[{KEYWORD}\]
NUMBER                    ={integer_literal}|{real_literal}
STRING                    =\"([^\"\n])*\"
MLSTRINGSTART             =N?"'"
TYPE                      =("DATETIME"|"CHAR"|"VARCHAR"|"NVARCHAR"|"decimal"|"date"|"integer"|"real"|"numeric"|"smallint"|"bigint"|"tinyint"|"bit"|"image"|"money"|"int"|"sysname"|"sql_variant"|"uniqueidentifier")
BRACE_TYPE                =\[{TYPE}\]
FUNCTION                  ="newid"|"ISNULL"|"SUM"|"DATEADD"|"CONVERT"|"LTRIM"|"RTRIM"|"CHARINDEX"|"UPPER"|"LOWER"|"MAX"|"MIN"|"REPLICATE"|"LEN"|"GETDATE"|"object_id"|"raiserror"|"quotename"|"OBJECTPROPERTY"|"power"
OPERATOR                  ="("|")"|"="|"<"|">"|"."|"+"|"/"|"-"|"*"|","|";"|"&"|"|"|"^"
LINE_COMMENT              =--[^\n]*
COMMENT_START             ="/*"
COMMENT_END               ="*/"
IDENTIFIER                =[@#]*[a-zA-Z][_$a-zA-Z0-9]*
BRACE_IDENTIFIER          =\[[^\]]+\]
LABEL                     =[a-zA-Z][_$a-zA-Z0-9]*({WS})?:

%state ML_COMMENT
%state ML_STRING

%%

<YYINITIAL>{KEYWORD}                  {return Keyword();}
<YYINITIAL>{BRACE_KEYWORD}            {return Keyword();}
<YYINITIAL>{FUNCTION}                 {return Keyword();}
<YYINITIAL>{STRING}                   {return String();}
<YYINITIAL>{NUMBER}                   {return Number();}
<YYINITIAL>{OPERATOR}                 {return Operator();}
<YYINITIAL>{LABEL}                    {return Other();}
<YYINITIAL>{TYPE}                     {return Type();}
<YYINITIAL>{IDENTIFIER}               {return Identifier();}
<YYINITIAL>{BRACE_IDENTIFIER}         {return Identifier();}
<YYINITIAL>{LINE_COMMENT}             {return Comment();}
<YYINITIAL>{COMMENT_START}            {ENTER(ML_COMMENT); return Comment();}
<YYINITIAL>{MLSTRINGSTART}            {ENTER(ML_STRING); return String();}

<ML_COMMENT>{COMMENT_END}             {EXIT(); return Comment();}
<ML_COMMENT>[^ \t\n\*]+               {return Comment();}
<ML_COMMENT>"*"                       {return Comment();}

<ML_STRING>"'"                        {EXIT(); return String(); }
<ML_STRING>[^ \t\n\\']+               {return String();}
<ML_STRING>\\[']                      {return String();}

{WS}			                            {;}
\n                                    {return NewLine();}
.                                     {return Error(); }



 