%namespace Xacc.Languages.IronScheme

%{

public override string[] Extensions {get {return new string[]{"ss","scm","pp", "sch", "sls"}; }}
public override string Name {get {return "R6RS Scheme"; }}
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
    ll.Add(c.car.ToString());
    c = c.cdr as Cons;
  }
  return string.Join(" ", ll.ToArray());
}

static IEnumerable<ICodeElement> GetDefs(object defs)
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
        if (defname != null && (defname.StartsWith("define")))
        {
          object name = ((Cons)c.cdr).car;
          object defbody = ((Cons)c.cdr).cdr;
          string sname = null;
          if (name is Cons)
          {
            Identifier id = ((Cons)name).car as Identifier;
            sname = id.Name;
            Definition d = new Definition(sname, defname == "define" ? defbody : null);
            d.Location = id.Location;
            yield return d;
          }
          else
          {
            Identifier id = name as Identifier;
            if (id != null)
            {
              sname = id.Name;
              Definition d = new Definition(sname, defname == "define" ? defbody : null);
              d.Location = id.Location;
              yield return d;
            }
          }
        }
      }
    }
    
    if (body.car is Library || body.car is Module)
    {
      yield return body.car as ICodeElement;
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
    foreach (ICodeElement d in GetDefs(body))
    {
      Add(d);
    }
  }
  
  public override string Fullname
  {
    get {return Name;}
  }
}

[Serializable]
[Image("CodeDelegate.png")]
class Module : CodeType
{
  
  public Module(object name, object body)
  {
    Name = GetName(name);
    foreach (ICodeElement d in GetDefs(body))
    {
      Add(d);
    }
  }
  
  public override string Fullname
  {
    get {return Name;}
  }
}

[Serializable]
[Image("CodeValueType.png")]
class TopLevel : CodeType
{
  public TopLevel(object body)
  {
    Name = "toplevel";
    foreach (ICodeElement d in GetDefs(body))
    {
      Add(d);
    }
  }
  
  public bool AllLibraries()
  {
    foreach (ICodeElement e in this.Members)
    {
      if (!(e is Library))
      {
        return false;
      }
    }
    return true;
  }
}

[Serializable]
[Image("CodeMethod.png")]
class Definition : CodeMember
{
  public Definition(string name, object body)
  {
    Name = name;
/*    foreach (ICodeElement d in GetDefs(body))
    {
      Add(d);
    }*/
  }
}

class DefineLocation : CodeElement
{
  public DefineLocation(string name)
  {
    Name = name;
  }
}

class Identifier : CodeElement
{
  public Identifier(string name)
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
%token DEFINE DEFINESYNTAX DEFINERECORDTYPE DEFINEENUMERATION DEFINECONDITIONTYPE MODULE

%token LBRACE RBRACE LBRACK RBRACK QUOTE QUASIQUOTE UNQUOTE UNQUOTESPLICING VECTORLBRACE DOT BYTEVECTORLBRACE
%token UNSYNTAX SYNTAX UNSYNTAXSPLICING QUASISYNTAX IGNOREDATUM
%token SYMBOL LITERAL STRING NUMBER CHARACTER 

%type <List>  exprlist body
%type <Value> expr list library toplevel definesym module



%start file

%%



file 
    : libraries
    | toplevel                                { if (($1 as TopLevel).AllLibraries())
                                                {
                                                  foreach (ICodeElement lib in ($1 as TopLevel).Members)
                                                  {
                                                    CodeModel.Add(lib);
                                                  }
                                                }
                                                else CodeModel.Add( $1 as CodeElement); }
    ;
    
libraries
    : library                                  { CodeModel.Add( $1 as CodeElement); }
    | libraries library                        { CodeModel.Add( $2 as CodeElement); }
    ;    
    
library
    : LBRACE LIBRARY list 
        export
        import
        body
      RBRACE                                   { MakePair(@1,@7); try { $$ = new Library($3, $6); ((CodeElement)$$).Location = @3; } catch {} }
    ;   
    
toplevel
    : body                                    { $$ = new TopLevel($1);}
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
    
module
    : LBRACE MODULE list body RBRACE          { $$ = new Module($3, $4); ((CodeElement)$$).Location = @3; }
    ;
    
list
    : LBRACE exprlist RBRACE                  { MakePair(@1,@3); $$ = $2; @@ = @1 + @3; }
    | LBRACK exprlist RBRACK                  { MakePair(@1,@3); $$ = $2; @@ = @1 + @3; }
    | LBRACE exprlist expr DOT expr RBRACE    { MakePair(@1,@6); $$ = Append($2, new Cons($3,$5)); @@ = @1 + @6;  }
    | LBRACK exprlist expr DOT expr RBRACK    { MakePair(@1,@6); $$ = Append($2, new Cons($3,$5)); @@ = @1 + @6;  }
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
    | MODULE 
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
    | module                                
    | SYMBOL                                  { $$ = new Identifier($1.Value as string); ((CodeElement)$$).Location = @1; }
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

static void CountBraces(string text, ref int open, ref int close)
{
  for (int i = 0; i < text.Length; i++)
  {
    switch (text[i])
    {
      case '[': case '(': open++;  break;
      case ']': case ')': close++; break;
    }
  }
}

static bool BracesAreBalanced(string text)
{
  int open = 0, close = 0;
  CountBraces(text, ref open, ref close);
  return open == close;
}

static string[] balancedkeywords = { "" };

static bool IsBalancedKeyword(string text)
{
  switch (text)
  {
    case "let":
    case "letrec":
    case "let*":
    case "letrec*":
    case "lambda":
    case "case-lamba":
    case "define":
    case "set!":
    case "cond":
      return true;
    default:
      return false;
  }
}

public override int GetIndentation(string previousline, int tabsize)
{
  int i = previousline.LastIndexOf("(if ");
  if (i >= 0)
  {
    // need to count braces to determine
    var ii = i + 4;
    if (BracesAreBalanced(previousline.Substring(ii)))
    {
      return i + 4;
    }
  }
  i = previousline.LastIndexOf("(");
  if (i >= 0)
  {
    if (IsBalancedKeyword(previousline.Substring(i + 1)))
    {
      return i + tabsize;
    }
  }
  return 0;
}

public override bool SupportsNavigation
{
  get {return true; }
}

protected override bool UseProjectTreeForAutoComplete
{
  get {return true; }
}

public override string DefaultFileContent
{
  get {return "(import (rnrs))\n"; }
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












    