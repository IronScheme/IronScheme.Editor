%namespace LSharp


%{

public override string[] Extensions {get {return new string[]{"ls","lsp"}; }}
public override string Name {get {return "LSharp"; }}
LexerBase lexer = new LSharpLexer();
protected override LexerBase Lexer {get {return lexer; }}

[Serializable]
[Image("bullet.png")]
class List : CodeContainerElement
{
  public List(CodeElementList l)
  {
    AddRange(l);
  }
  
  public override string ToString()
  {
    return "(...)";
  }
}


[Serializable]
[Image("CodeMethod.png")]
class Function : CodeElement
{
  string value;
  public Function(string value)
  {
    this.value = value;
  }
  
  public override string ToString()
  {
    return value;
  }
}

[Serializable]
[Image("CodeField.png")]
class Identifier : CodeElement
{
  string value;
  public Identifier(string value)
  {
    this.value = value;
  }
  
  public override string ToString()
  {
    return value;
  }
}

[Serializable]
[Image("CodeProperty.png")]
class Keyword : CodeElement
{
  string value;
  public Keyword(string value)
  {
    this.value = value;
  }
  
  public override string ToString()
  {
    return value;
  }
}



[Serializable]
[Image("File.Type.NA.png")]
class File : CodeContainerElement
{
  public File(string filename, CodeElementList l)
  {
    Name = filename;
    AddRange(l);
  }
}

[Serializable]
[Image("CodeEnum.png")]
class Literal : CodeElement
{
  string value;
  public Literal(string value)
  {
    this.value = value;
  }
  
  public override string ToString()
  {
    return value;
  }
}

%} 

%union
{

  public CodeElementList  list {get {return value as CodeElementList; } set {this.value = value;}}
  public CodeElement      elem {get {return value as CodeElement; } set {this.value = value;}}
  
#if DEBUG
  public object Value { get { return value; } }
#endif

}

%token LBRACE RBRACE DEFMACRO DEFUN ADD APPEND APPLY ASSOC CAAR CAAAR CAADR CADAR CADDR CADR CAR
%token CDAAR CDAR CDDAR CDDDR CDDR CDR CONS COPYLIST DIV ENV EQ EQL EVAL EVALSTRING SPLICE UNQUOTE ARGREST 
%token EXITFN FIRST GT GTE LT LTE INSPECT IS LENGTH LIST LOAD LOGAND LOGOR LOGXOR MACROEXPAND MAP MEMBER
%token MUL NCONC NEW NOT NEQ NTH PR PRL READ READSTRING REFERENCE REVERSE REST SUB THROW TYPEOF USING
%token AND BACKQUOTE CALL COND DEC DO EACH FN FOR IF INC LET MACRO OR QUOTE SETF THE TO TRACE TRY WHEN WHILE WITH

%token <text> IDENTIFIER LITERAL STRING INTEGER

%type <list> lists exprlist
%type <elem> list listcontent expr literal

%start file

%%

file 
    : lists                         { CodeModel.Add( new File(CurrentFilename, $1)); }
    ;

lists 
    :                               { $$ = new CodeElementList(); }
    | lists expr                    { $$ = $1; $$.Add($2); }
    ;
    
list
    : LBRACE specialform RBRACE     { MakePair(@1,@3); }
    | LBRACE macros RBRACE          { MakePair(@1,@3); }
    | LBRACE functions RBRACE       { MakePair(@1,@3); }
    | LBRACE IDENTIFIER exprlist RBRACE { MakePair(@1,@4); OverrideToken(@2, IsType($2) ? TokenClass.Type : TokenClass.Identifier); }
    | LBRACE literal exprlist RBRACE    { MakePair(@1,@4); }
    | LBRACE list exprlist RBRACE       { MakePair(@1,@4); }
    | LBRACE specexpr exprlist RBRACE   { MakePair(@1,@4); }
    ;
    
macros
    : DEFUN IDENTIFIER args exprlist    { OverrideToken(@2, TokenClass.Type); }
    | DEFMACRO IDENTIFIER args exprlist { OverrideToken(@2, TokenClass.Type); }
    ;
    
functions
    : ADD exprlist
    | APPEND exprlist
    | APPLY expr exprlist
    | ASSOC expr expr
    | CAAAR expr
    | CAADR expr
    | CAAR expr
    | CADAR expr
    | CADDR expr
    | CADR expr
    | CAR expr
    | CDAAR expr
    | CDAR expr
    | CDDAR expr
    | CDDDR expr
    | CDDR expr
    | CDR expr
    | CONS expr expr
    | COPYLIST expr
    | DIV exprlist
    | ENV
    | EQ exprlist
    | EQL exprlist
    | EVAL expr
    | EVALSTRING expr
    | EXITFN expropt
    | FIRST expr
    | GT expr expr exprlist
    | GTE expr expr exprlist
    | LTE expr expr exprlist
    | LT expr expr exprlist
    | INSPECT expr
    | IS IDENTIFIER expr
    | LENGTH expr
    | LIST exprlist
    | LOAD STRING
    | LOGAND exprlist
    | LOGOR exprlist
    | LOGXOR exprlist
    | MACROEXPAND expr exprlist
    | MAP expr expr
    | MEMBER expr expr
    | MUL exprlist
    | NCONC exprlist
    | NEW IDENTIFIER exprlist
    | NOT expr
    | NEQ exprlist
    | NTH INTEGER expr
    | PR exprlist
    | PRL exprlist
    | READ expr expropt
    | READSTRING expr
    | REFERENCE stringlist
    | REVERSE expr
    | REST expr
    | SUB exprlist
    | THROW expr
    | TYPEOF IDENTIFIER
    | USING stringlist
    ;
    
stringlist
    :                               {;}
    | stringlist STRING             {;}
    ;    
        
    
    
specialform
    : AND exprlist
    | CALL IDENTIFIER expr exprlist
    | COND condexprlist expropt
    | DEC expr
    | DO exprlist
    | EACH IDENTIFIER expr exprlist
    | FN args exprlist
    | FOR expr expr expr exprlist
    | IF expr expr expropt
    | INC expr
    | LET IDENTIFIER expr exprlist
    | MACRO args exprlist
    | OR exprlist
    | SETF setvaluexpr
    | THE IDENTIFIER expr
    | TO IDENTIFIER expr exprlist
    | TRACE IDENTIFIER exprlist
    | TRY expr expr expropt
    | WHEN expr exprlist
    | WHILE expr exprlist
    | WITH setvaluexpr expr
    ; 
    
setvaluexpr
    :                               {;}
    | setvaluexpr IDENTIFIER expr   {;}
    | setvaluexpr UNQUOTE expr expr {;}
    ;
    
args
    : LBRACE arglist argtail RBRACE { MakePair(@1,@4); }
    ; 
    
argtail
    :
    | ARGREST IDENTIFIER            {;}
    ;
    
arglist
    :                               {;}
    | arglist IDENTIFIER            {;}
    ;       

condexprlist
    :                               {;}
    | condexprlist expr expr        {;}
    ;
    
exprlist
    :                               { $$ = new CodeElementList(); }
    | exprlist expr                 { $$= $1; $$.Add($2);}
    ;       
    
expropt
    :                               {;}
    | expr                          {;}
    ;
    
specexpr
    : QUOTE expr                    {;}
    | BACKQUOTE expr                {;}
    | SPLICE expr                   {;}
    | UNQUOTE expr                  {;}    
    ;
    
expr
    : listcontent                   {;}
    | list                          {;}
    | specexpr                      {;}
    | error                         {;}
    ; 

listcontent
    : IDENTIFIER                    { $$ = new Identifier($1); }
    | literal                       {;}
    ; 
    
literal
    : STRING                        { $$ = new Literal($1);}
    | INTEGER                       { $$ = new Literal($1);}
    | LITERAL                       { $$ = new Literal($1);}
    ;      
    
    
    

%%

void CreateFunctions(ICodeNamespace env, params string[] names)
{
  foreach (string name in names)
  {
    Function f = new Function(name.Trim());
    f.Name = name.Trim();
    env.Add(f);
  }
}

void CreateForms(ICodeNamespace env, params string[] names)
{
  foreach (string name in names)
  {
    Keyword f = new Keyword(name.Trim());
    f.Name = name.Trim();
    env.Add(f);
  }
}

readonly static string[] functions  = LSharp.TopLoop.Environment.GetSymbols(typeof(LSharp.Function));
readonly static string[] forms      = LSharp.TopLoop.Environment.GetSymbols(typeof(LSharp.SpecialForm));

protected override void LoadDefaultReferences(Project proj, string filename)
{
  //make dummy autocomplete info
  CodeModule lsharp = new CodeModule("LSharp");
  ICodeNamespace env = new CodeNamespace("");

  CreateFunctions(env, functions);
  
  Function f = new Function("|");
  f.Name = "|";
  env.Add(f);
  
  CreateForms(env, forms);
  
  lsharp.Add(env);
  
  proj.AddReferencesAndGenerateTree(lsharp);
  proj.LoadAssemblies("mscorlib.dll");
}

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












    