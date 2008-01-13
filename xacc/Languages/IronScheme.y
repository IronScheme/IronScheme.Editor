%namespace Xacc.Languages.IronScheme

%{

public override string[] Extensions {get {return new string[]{"ss","scm","pp", "sch", "sls"}; }}
public override string Name {get {return "IronScheme"; }}
protected override LexerBase GetLexer() { return new IronSchemeLexer(); } 

internal class Cons : CodeElement
{
  public object car;
  public object cdr;
  
  public Cons(object car)
  {
    this.car = car;
  }
  
  public Cons(object car, object cdr)
  {
    this.car = car;
    this.cdr = cdr;
  }
  
  static string WriteFormat(object o)
  {
    if (o == null)
    {
      return "()";
    }
    else return o.ToString();
  }
  
  public override string ToString()
  {
    List<string> v = new List<string>();
    Cons s = this;

    while (s != null)
    {
      v.Add(WriteFormat(s.car));
      if (s.cdr != null && !(s.cdr is Cons))
      {
        v.Add(".");
        v.Add(WriteFormat(s.cdr));
        break;
      }
      s = s.cdr as Cons;
    }
    return string.Format("({0})", string.Join(" ", v.ToArray()));
  }
}

static readonly object Ignore = new object();

static Cons Last(Cons c)
{
  while (c.cdr != null)
  {
    c = c.cdr as Cons;
  }
  return c;
}

static Cons Append(Cons c, Cons t)
{
  if (c == null || c.car == Ignore)
  {
    return t;
  }
  if (t == null || t.car == Ignore)
  {
    return c;
  }
  Last(c).cdr = t;
  return c;
}

static string GetName(object list)
{
  List<string> ll = new List<string>();
  Cons c = list as Cons;
  while (c != null)
  {
    ll.Add(c.car as string);
    c = c.cdr as Cons;
  }
  return string.Join(" ", ll.ToArray());
}

static IEnumerable<Definition> GetDefs(object defs)
{
  Cons body = defs as Cons;
  
  while (body != null)
  {
    Cons c = body.car as Cons;
    if (c != null)
    {
      if (c.car is DefineLocation)
      {
      string defname = c.car.ToString();
      if (defname != null && defname.StartsWith("define"))
      {
        object name = ((Cons)c.cdr).car;
        string sname = null;
        if (name is Cons)
        {
          sname = ((Cons)name).car.ToString();
        }
        else
        {
          sname = name.ToString();
        }
        Definition d = new Definition(sname);
        d.Location = ((CodeElement)c.car).Location;
        
        yield return d;
      }
      }
    }
    body = body.cdr as Cons;
  }
  yield break;
}

[Serializable]
[Image("CodeRefType.png")]
class Library : CodeType
{
  
  public Library(object name, object body)
  {
    Name = GetName(name);
    foreach (Definition d in GetDefs(body))
    {
      Add(d);
    }
  }
}

[Serializable]
[Image("CodeValueType.png")]
class TopLevel : CodeType
{
  public TopLevel(object body)
  {
    Name = "toplevel";
    foreach (Definition d in GetDefs(body))
    {
      Add(d);
    }
  }
}

[Serializable]
[Image("CodeMethod.png")]
class Definition : CodeMember
{
  public Definition(string name)
  {
    Name = name;
  }
}

class DefineLocation : CodeElement
{
  public DefineLocation(string name)
  {
    Name = name;
  }
}



%} 

%union
{
  internal Parser.Cons List {get {return value as Parser.Cons; } set {this.value = value;}}
  public Object Value {get {return value; } set {this.value = value;}}
}

%token LIBRARY IMPORT EXPORT
%token DEFINE DEFINESYNTAX DEFINERECORDTYPE DEFINEENUMERATION DEFINECONDITIONTYPE

%token LBRACE RBRACE LBRACK RBRACK QUOTE QUASIQUOTE UNQUOTE UNQUOTESPLICING VECTORLBRACE DOT BYTEVECTORLBRACE
%token UNSYNTAX SYNTAX UNSYNTAXSPLICING QUASISYNTAX IGNOREDATUM
%token SYMBOL LITERAL STRING NUMBER CHARACTER 

%type <List>  exprlist body
%type <Value> expr list library toplevel definesym



%start file

%%



file 
    : library                                 { CodeModel.Add( $1 as CodeElement); }
    | toplevel                                { CodeModel.Add( $1 as CodeElement); }
    ;
    
library
    : LBRACE LIBRARY list 
        export
        import
        body
      RBRACE                                      { MakePair(@1,@7); try { $$ = new Library($3, $6); ((CodeElement)$$).Location = @3; } catch {} }
    ;   
    
toplevel
    : import body                             { $$ = new TopLevel($2);}
    ;   

export
    : list
    ;

import
    : list
    ;  
    
body
    : exprlist
    ;    
    
list
    : LBRACE exprlist RBRACE                  { MakePair(@1,@3); $$ = $2; @@ = @1 + @3; }
    | LBRACK exprlist RBRACK                  { MakePair(@1,@3); $$ = $2; @@ = @1 + @3; }
    | LBRACE exprlist expr DOT expr RBRACE    { MakePair(@1,@6); $$ = Append($2, new Cons($3,$5)); @@ = @1 + @6;  }
    | specexpr expr                               
    ;
    
   
exprlist
    :                                             { $$ = null; }
    | exprlist expr                               { $$ = Append($1,new Cons($2)); }
    ;
    
libsym
    : IMPORT
    | EXPORT
    | LIBRARY 
    ;
    
definesym
    : DEFINE
    | DEFINESYNTAX
    | DEFINECONDITIONTYPE
    | DEFINEENUMERATION
    | DEFINERECORDTYPE
    ;    
    
expr
    : list                                        
    | library                                     
    | SYMBOL                                     
    | libsym
    | definesym                               { $$ = new DefineLocation($1 as string); ((CodeElement)$$).Location = @1; }
    | STRING                                      
    | NUMBER                                      
    | LITERAL                                     
    | CHARACTER                                   
    | VECTORLBRACE exprlist RBRACE            { MakePair(@1,@3); }
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

public override ICodeElement[] GetIdentifiers(string line, int lci, IToken[] tokens, out string hint)
{
  return IronSchemeIdentifiers.GetR6RSIds(line, lci, tokens, out hint);
}

public override bool SupportsNavigation
{
  get {return true; }
}

protected override bool UseProjectTreeForAutoComplete
{
  get {return true; }
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












    