%namespace Xacc.Languages.IronScheme

%{

public override string[] Extensions {get {return new string[]{"ss","scm","pp", "sch"}; }}
public override string Name {get {return "IronScheme"; }}
protected override LexerBase GetLexer() { return new IronSchemeLexer(); } 


%} 

%union
{
  
#if DEBUG
  public object Value { get { return value; } }
#endif
}

%token LBRACE RBRACE LBRACK RBRACK QUOTE QUASIQUOTE UNQUOTE UNQUOTESPLICING VECTORLBRACE DOT BYTEVECTORLBRACE
%token UNSYNTAX SYNTAX UNSYNTAXSPLICING QUASISYNTAX IGNOREDATUM
%token SYMBOL LITERAL STRING NUMBER CHARACTER 

%start file

%%

file 
    : exprlist                                    
    ;
    
list
    : LBRACE exprlist RBRACE                      { MakePair(@1,@3); }
    | LBRACK exprlist RBRACK                      { MakePair(@1,@3); }
    | LBRACE exprlist expr DOT expr RBRACE        { MakePair(@1,@6); }
    | specexpr expr                               
    ;

exprlist
    :                                             
    |  exprlist expr                              
    ;       
    
expr
    : list                                        
    | SYMBOL                                      
    | STRING                                      
    | NUMBER                                      
    | LITERAL                                     
    | CHARACTER                                   
    | VECTORLBRACE exprlist RBRACE                { MakePair(@1,@3); }
    | BYTEVECTORLBRACE exprlist RBRACE            { MakePair(@1,@3); }
    | IGNOREDATUM expr                            { ; }
    ; 

specexpr
    : QUOTE                                       
    | UNQUOTESPLICING                             
    | QUASIQUOTE                                  
    | UNQUOTE                                     
    | SYNTAX                                      
    | UNSYNTAXSPLICING                            
    | QUASISYNTAX                                 
    | UNSYNTAX                                    
    
    ;    


    
    

%%


protected override bool UseProjectTreeForAutoComplete
{
  get {return false; }
}

protected internal override string[] CommentLines(string[] lines)
{
  string[] newlines = new string[lines.Length];
  for (int i = 0; i < lines.Length; i++)
  {
    if (lines[i].StartsWith(";"))
    {
      newlines[i] = lines[i];
    }
    else
    {
      newlines[i] = ";" + lines[i];
    }
  }
  return newlines;
}

protected internal override string[] UnCommentLines(string[] lines)
{
  string[] newlines = new string[lines.Length];
  for (int i = 0; i < lines.Length; i++)
  {
    if (lines[i].StartsWith(";"))
    {
      newlines[i] = lines[i].Substring(1);
    }
    else
    {
      newlines[i] = lines[i];
    }
  }
  return newlines;
}












    