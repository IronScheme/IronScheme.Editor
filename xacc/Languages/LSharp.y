%namespace LSharp


%{

public override string[] Extensions {get {return new string[]{"ls"}; }}
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

  public CodeElementList  list {get {return (CodeElementList)value; } set {this.value = value;}}
  public CodeElement      elem {get {return (CodeElement)value; } set {this.value = value;}}
}

%token LBRACE RBRACE
%token <text> KEYWORD FUNCTION IDENTIFIER LITERAL

%type <list> listcontents
%type <elem> list listcontent

%start file

%%

file 
    : listcontents                  { CodeModel.Add( new File(CurrentFilename, $1)); }
    ;
    
list
    : LBRACE listcontents RBRACE    { 
                                      AddAutoComplete(@1, typeof(Function), typeof(Keyword));
                                      MakePair(@1,@3); 
                                      $$ = new List($2);
                                    }
     ;
    
listcontents 
    : /* empty */                   { $$ = new CodeElementList(); }
    | listcontents listcontent      { $$.Add($2); }
    ;    

listcontent
    : FUNCTION                      { $$ = new Function($1);}
    | KEYWORD                       {
                                      if ($1 == "new")
                                      {
                                        AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace));
                                      } 
                                      $$ = new Keyword($1);
                                    }
    | IDENTIFIER                    { $$ = new Identifier($1);}
    | LITERAL                       { $$ = new Literal($1);}
    | list                          
    | error                         { $$ = null; }
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












    