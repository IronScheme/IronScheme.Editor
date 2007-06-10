%namespace Xacc.Languages.CSharp

%using System.Collections
%using System.Reflection


%{

public override string[] Extensions {get {return new string[]{"cs"}; }}
public override string Name {get {return "C#"; }}
protected override LexerBase GetLexer() { return new CSharpLexer(); } 

[Serializable]
class TypeRef : CodeTypeRef
{
  public TypeRef(string t) : base(t)
  {
  }
  
  public TypeRef(string t, bool isarr) : base(t, isarr)
  {
  }

  public TypeRef(Type t) : base(t)
  {
  }
  
  public TypeRef(CodeTypeRef r, bool isarray) : base(r, isarray)
  {
  }
  
  readonly static Hashtable typemap = new Hashtable();
  
  static TypeRef()
  {
    typemap.Add("Void", "void");
    typemap.Add("Int32", "int");
    typemap.Add("UInt32", "uint");
    typemap.Add("UInt16", "ushort");
    typemap.Add("Int16", "short");
    typemap.Add("Char", "char");
    typemap.Add("Object", "object");
    typemap.Add("String", "string");
    typemap.Add("Int64", "long");
    typemap.Add("UInt64", "ulong");
    typemap.Add("Byte", "byte");
    typemap.Add("SByte", "sbyte");
    typemap.Add("Boolean", "bool");
    typemap.Add("Single", "float");
    typemap.Add("Double", "double");
    typemap.Add("Decimal", "decimal");
    
  }
  
  protected override string MakeShort(string name)
  {
    if (typemap.ContainsKey(name))
    {
      name = typemap[name] as string;
    }
    return name;
  }
  
  public override string ToString()
  {
    return Name + (IsArray ? "[]" : "");
  }
}

%}


%union
{
  public ArrayList            list        {get {return value as ArrayList; } set {this.value = value;}}
  public CodeNamespace        ns          {get {return value as CodeNamespace; } set {this.value = value;}}
  public CodeElementList      elemlist    {get {return value as CodeElementList; } set {this.value = value;}}
  public CodeElement          elem        {get {return value as CodeElement; } set {this.value = value;}}
  public CodeTypeRef          typeref     {get {return value as CodeTypeRef; } set {this.value = value;}}
  public Object               primval     {get {return value; } set {this.value = value;}}
  public ParameterAttributes  paramattr   {get {return (ParameterAttributes)value; } set {this.value = value;}}
  
#if DEBUG
  public object Value { get { return value; } }
#endif

}

/* Special tokens to help disambiguate rank_specifiers */
%token RANK_SPECIFIER

/* C.1.4 Tokens */
%token <text> IDENTIFIER 
%token <text> INTEGER_LITERAL REAL_LITERAL CHARACTER_LITERAL STRING_LITERAL MLSTRING_LITERAL


/* C.1.7 KEYWORDS */ 
%token  ABSTRACT AS BASE BOOL BREAK
%token  BYTE CASE CATCH CHAR CHECKED
%token  CLASS CONST CONTINUE DECIMAL DEFAULT
%token  DELEGATE DO DOUBLE ELSE ENUM
%token  EVENT EXPLICIT EXTERN FALSE FINALLY
%token  FIXED FLOAT FOR FOREACH GOTO
%token  IF IMPLICIT IN INT INTERFACE
%token  INTERNAL IS LOCK LONG NAMESPACE
%token  NEW NULL_LITERAL OBJECT OPERATOR OUT
%token  OVERRIDE PARAMS PRIVATE PROTECTED PUBLIC
%token  READONLY REF RETURN SBYTE SEALED
%token  SHORT SIZEOF STACKALLOC STATIC KW_STRING
%token  STRUCT SWITCH THIS THROW TRUE
%token  TRY TYPEOF UINT ULONG UNCHECKED
%token  UNSAFE USHORT USING VIRTUAL VOID
%token  VOLATILE WHILE WHERE ARGLIST VAR ADD REMOVE

%token SELECT INTO ORDERBY FROM LET JOIN EQUALS ASCENDING DESCENDING GROUP ON BY

/* The ones that seem to be context sensitive */

/* Preprocessor Targets */
%token PPSTART PPDEFINE PPIF PPELSE PPENDIF PPREGION PPENDREGION PPID PPELIF
/* Accessor types */
%token GET SET
/* Event accessor declarations */
%token ADD REMOVE

/*** MULTI-CHARACTER OPERATORS ***/
%token PLUSEQ MINUSEQ STAREQ DIVEQ MODEQ QQ GTGTEQ GTGT
%token XOREQ  ANDEQ   OREQ LTLT GTGT LTLTEQ EQEQ NOTEQ
%token LEQ GEQ ANDAND OROR PLUSPLUS MINUSMINUS ARROW LAMBDA

%start compilation_unit  /* I think */

%type <elemlist> namespace_member_declarations namespace_body namespace_member_declarations_opt interface_body type_list type_list_opt
%type <elemlist> class_member_declarations_opt class_body class_member_declarations struct_body interface_member_declarations_opt
%type <elemlist> formal_parameter_list_opt formal_parameter_list struct_member_declarations_opt struct_member_declarations
%type <elemlist> interface_member_declarations enum_body enum_member_declarations_opt enum_member_declarations
%type <elem> constant_declaration field_declaration interface_indexer_declaration identifier_name
%type <elem> namespace_declaration namespace_member_declaration struct_member_declaration interface_member_declaration
%type <elem> enum_member_declaration constructor_declarator variable_declarator constant_declarator
%type <text> qualified_identifier qualifier namespace_name  type_qualified_identifier long_qualified_identifier
%type <elem> class_declaration struct_declaration interface_declaration enum_declaration delegate_declaration type_declaration
%type <elem> class_member_declaration method_declaration property_declaration type2
%type <elem> event_declaration indexer_declaration operator_declaration constructor_declaration destructor_declaration
%type <elem> formal_parameter fixed_parameter parameter_array method_header interface_method_declaration interface_property_declaration
%type <typeref> type return_type non_array_type simple_type primitive_type class_type numeric_type floating_point_type type_opt nullable_type
%type <typeref> integral_type array_type type_name non_null_type
%type <primval> literal mllit boolean_literal
%type <list> variable_declarators constant_declarators type_arg_list type_arg_list_opt
%type <paramattr> parameter_modifier_opt

%type <text> member_name gen_qualified_identifier norm_qualified_identifier gen_qualifier norm_qualifier member_name2 gen_qualified_identifier2

%nonassoc IFREDUCE
%nonassoc ELSE

%right SHIFT
%nonassoc '<' '>'
%left REDUCE



%%

/***** C.1.8 Literals *****/
literal
  : boolean_literal
  | INTEGER_LITERAL                                         { $$ = 0;/*int.Parse($1);*/ }
  | REAL_LITERAL                                            { $$ = 0f; /*float.Parse($1);*/ }
  | CHARACTER_LITERAL                                       { $$ = $1[0]; }
  | STRING_LITERAL                                          
  | NULL_LITERAL                                            { $$ = null; }
  | mllit
  ;

mllit
  : MLSTRING_LITERAL                                        
  | mllit MLSTRING_LITERAL                                  { $$ = $1 + $2; }
  ;
  
boolean_literal
  : TRUE                                                    { $$ = true; }
  | FALSE                                                   { $$ = false; }
  ;
/********** C.2 Syntactic grammar **********/

/***** C.2.1 Basic concepts *****/
namespace_name
  : qualified_identifier
  ;
  
type_name
  : qualified_identifier                         { $$ = new TypeRef($1); if (inblock > 0 && $1 == "var") OverrideToken(@1, TokenClass.Keyword); else OverrideToken(@1, TokenClass.Type); }
  ;
  
member_name
  : IDENTIFIER type_list_opt                    { $$ = $1 + ($2 == null ? "" : ("<" + CodeElement.Join($2) + ">")) ; @@ = @1; }
  ;
  
type_opt
  :
  | type
  ;  
  
type_list_opt
  : 
  | '<' type_list '>'                     {  MakePair(@1,@3);  $$ = $2; }
  | '<' type_list '<' type_list GTGT      {  $$ = $2; }
  ; 
  
type_list
  : type_opt                                 { $$ = new CodeElementList($1); } 
  | type_list ',' type_opt                  { $$ = $1; $$.Add($3); }
  ;
  
type_arg_list_opt
  :
  | '<' type_arg_list '>'                  {  MakePair(@1,@3);  $$ = $2; }
  ;  
  
type_arg_list
  : IDENTIFIER                             { OverrideToken(@1, TokenClass.Type); $$ = new ArrayList(); $$.Add($1); }
  | type_arg_list ',' IDENTIFIER           { OverrideToken(@3, TokenClass.Type); $$ = $1; $$.Add($3); }
  ;

/***** C.2.2 Types *****/

nullable_opt
  : 
  | '?'
  ;


type
  : non_array_type                              
  | array_type                                              { $$ = new TypeRef($1, true); }
  ;
  
nullable_type
  : type_name '?'                                          { $$ = $1; @@ = @1; }
  ;
  
non_null_type
  : simple_type
  | type_name                                             
  ;  
  
non_array_type
  : non_null_type
  | nullable_type
  ;
  
simple_type
  : primitive_type nullable_opt                            { $$ = $1; @@ = @1;}
  | class_type
 /* | pointer_type */
  ;
primitive_type
  : numeric_type                                            
  | BOOL                                                    { $$ = new TypeRef(typeof(bool)); }
  ;
numeric_type
  : integral_type
  | floating_point_type
  | DECIMAL                                                 { $$ = new TypeRef(typeof(decimal)); }
  ;
integral_type
  : SBYTE                                                   { $$ = new TypeRef(typeof(sbyte)); }
  | BYTE                                                    { $$ = new TypeRef(typeof(byte)); }
  | SHORT                                                   { $$ = new TypeRef(typeof(short)); }
  | USHORT                                                  { $$ = new TypeRef(typeof(ushort)); }
  | INT                                                     { $$ = new TypeRef(typeof(int)); }
  | UINT                                                    { $$ = new TypeRef(typeof(uint)); }
  | LONG                                                    { $$ = new TypeRef(typeof(long)); }
  | ULONG                                                   { $$ = new TypeRef(typeof(ulong)); }
  | CHAR                                                    { $$ = new TypeRef(typeof(char)); }
  ;
floating_point_type
  : FLOAT                                                   { $$ = new TypeRef(typeof(float)); }
  | DOUBLE                                                  { $$ = new TypeRef(typeof(double)); }
  ;
class_type
  : OBJECT                                                  { $$ = new TypeRef(typeof(object)); }
  | KW_STRING                                               { $$ = new TypeRef(typeof(string)); }
  ;
pointer_type
  : type '*'
  | VOID '*'
  ;
array_type
  : array_type rank_specifier                               { $$ = $1; @@ = @1;}
  | simple_type rank_specifier                              { $$ = $1; @@ = @1;}
  | qualified_identifier rank_specifier                     { $$ = new TypeRef($1,true); @@ = @1; OverrideToken(@1, TokenClass.Type);}
  ;
rank_specifiers_opt
  : /* Nothing */
  | rank_specifier rank_specifiers_opt
  ;
rank_specifier
  : RANK_SPECIFIER
  ;
/***** C.2.3 Variables *****/
variable_reference
  : expression
  ;
/***** C.2.4 Expressions *****/
argument_list
  : argument
  | argument_list ',' argument
  ;
argument
  : expression
  | REF variable_reference
  | OUT variable_reference
  ;
primary_expression
  : parenthesized_expression
  | primary_expression_no_parenthesis
  | error
  ;
primary_expression_no_parenthesis
  : literal
  | array_creation_expression
  | member_access
  | invocation_expression
  | element_access
  | this_access
  | base_access
  | anon_delegate_expression
  | anon_object_creation_expression
  | new_expression
  | default_expression
  | typeof_expression
  | sizeof_expression
  | checked_expression
  | unchecked_expression
  ;

anon_object_creation_expression
  : NEW anon_object_init
  ;

anon_object_init
  : '{' mem_dec_list_opt '}'                                      { MakePair(@1,@3); }
  | '{' mem_dec_list ',' '}'                                      { MakePair(@1,@4); }
  ;  
  
mem_dec_list_opt
  :
  | mem_dec_list
  ;
  
mem_dec_list
  : mem_dec
  | mem_dec_list ',' mem_dec
  ;
  
mem_dec
  : qualified_identifier
  | member_access
  | IDENTIFIER '=' expression
  ;
  
parenthesized_expression
  : '(' expression ')'                                              { MakePair(@1,@3); $$ = $2; @@ = @2;}
  ;
member_access
  : primary_expression '.' IDENTIFIER                               { /* instance class members */ }
  | primitive_type '.' IDENTIFIER                                   {   }
  | class_type '.' IDENTIFIER                                       {  /* static class members */ }
  ;

default_expression
  : DEFAULT '(' type ')'                                            { MakePair(@2,@4); }
  ;  
  
invocation_expression
  : primary_expression_no_parenthesis type_list_opt '(' argument_list_opt ')'     { MakePair(@3,@5); @@ = @1;}
  | qualified_identifier '(' argument_list_opt ')'                  { MakePair(@2,@4); @@ = @1; }
  ;
argument_list_opt
  : /* Nothing */
  | argument_list
  ;
element_access
  : primary_expression '[' expression_list ']'                      { MakePair(@2,@4);}
  | qualified_identifier '[' expression_list ']'                    { MakePair(@2,@4);}
  ;
expression_list_opt
  : /* Nothing */
  | expression_list
  ;
expression_list
  : expression
  | expression_list ',' expression
  ;
this_access
  : THIS
  ;
base_access
  : BASE '.' IDENTIFIER
  | BASE '[' expression_list ']'                                    { MakePair(@2,@4);}
  ;
post_increment_expression
  : postfix_expression PLUSPLUS
  ;
post_decrement_expression
  : postfix_expression MINUSMINUS
  ;
anon_delegate_expression
  : DELEGATE method_body
  | DELEGATE '(' formal_parameter_list_opt ')' method_body         { MakePair(@2,@4);}
  ;
new_expression
  : object_creation_expression
  ;
object_creation_expression
  : NEW type '(' argument_list_opt ')'  object_col_init_opt         { MakePair(@3,@5); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  | NEW type object_col_init_opt                                    { AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  | NEW error                                                       { AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  ;
  
object_col_init_opt
  :
  | object_col_init
  ;
  
object_col_init
  : object_init
  | col_init
  ; 
  
object_init
  : '{' mem_init_list_opt '}'                                       { MakePair(@1,@3);}
  | '{' mem_init_list ',' '}'                                       { MakePair(@1,@4);}
  ;
  
mem_init_list_opt
  :
  | mem_init_list
  ;  
  
mem_init_list
  : mem_init
  | mem_init_list ',' mem_init
  ;  
  
mem_init  
  : IDENTIFIER '=' init_value
  ;
  
init_value
  : expression
  | object_col_init
  ;  
  
col_init   
  : '{' elem_init_list '}'                                          { MakePair(@1,@3);}
  | '{' elem_init_list ',' '}'                                      { MakePair(@1,@4);}
  ;
  
elem_init_list
  : elem_init
  | elem_init_list ',' elem_init
  ;
  
elem_init
  : nonassign_expression
  | '{' expression_list '}'                                         { MakePair(@1,@3);}
  ;    
  
array_creation_expression
  : NEW non_array_type '[' expression_list ']'                      {  MakePair(@3,@5); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
    rank_specifiers_opt array_initializer_opt                       {  MakePair(@3,@5); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  | NEW array_type array_initializer                                {  AddAutoComplete(@1, typeof(CodeType),typeof(CodeNamespace)); }
  | NEW rank_specifier array_initializer
  ;
array_initializer_opt
  : /* Nothing */
  | array_initializer
  ;
typeof_expression
  : TYPEOF '(' type ')'                                             { MakePair(@2,@4); AddAutoComplete(@2, typeof(CodeType),typeof(CodeNamespace)); }
  | TYPEOF '(' VOID ')'                                             { MakePair(@2,@4);}
  ;
checked_expression
  : CHECKED '(' expression ')'                                      { MakePair(@2,@4);}
  ;
unchecked_expression
  : UNCHECKED '(' expression ')'                                    { MakePair(@2,@4);}
  ;
pointer_member_access
  : postfix_expression ARROW IDENTIFIER                             { /* instance class members */ }
  ;
addressof_expression
  : '&' unary_expression
  ;
sizeof_expression
  : SIZEOF '(' type ')'                                             { MakePair(@2,@4); AddAutoComplete(@2, typeof(CodeType), typeof(CodeNamespace));}
  ;
postfix_expression
  : primary_expression
  | qualified_identifier
  | post_increment_expression
  | post_decrement_expression
  | pointer_member_access
  ;
  
unary_expression_not_plusminus
  : postfix_expression 
  | '!' unary_expression
  | '~' unary_expression
  | cast_expression
  ;
pre_increment_expression
  : PLUSPLUS unary_expression
  ;
pre_decrement_expression
  : MINUSMINUS unary_expression
  ;
unary_expression
  : unary_expression_not_plusminus
  | '+' unary_expression
  | '-' unary_expression
  | '*' unary_expression
  | pre_increment_expression
  | pre_decrement_expression
  | addressof_expression
  ;
/* For cast_expression we really just want a (type) in the brackets,
 * but have to do some factoring to get rid of conflict with expressions.
 * The paremtnesised expression in the first three cases below should be 
 * semantically restricted to an identifier, optionally follwed by qualifiers
 */

cast_expression
  : '(' IDENTIFIER '?' ')' unary_expression_not_plusminus                           { OverrideToken(@2, TokenClass.Type); MakePair(@1,@4); }
  | '(' expression ')' unary_expression_not_plusminus                               { OverrideToken(@2, TokenClass.Type); MakePair(@1,@3); }
  | '(' multiplicative_expression '*' ')' unary_expression                          { MakePair(@1,@4);}
  | '(' qualified_identifier rank_specifier type_quals_opt ')' unary_expression     { OverrideToken(@2, TokenClass.Type); MakePair(@1,@5); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace));}
  | '(' primitive_type type_quals_opt ')' unary_expression                          { MakePair(@1,@4); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace));}
  | '(' class_type type_quals_opt ')' unary_expression                              { MakePair(@1,@4); AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace));}
  | '(' primitive_type '?' ')' unary_expression                                     { MakePair(@1,@4);}
  | '(' nullable_type ')' unary_expression                                          { MakePair(@1,@3);}
  | '(' VOID type_quals_opt ')' unary_expression                                    { MakePair(@1,@4);} 
  ;
type_quals_opt
  : /* Nothing */
  | type_quals
  ;
type_quals
  : type_qual
  | type_quals type_qual
  ;
type_qual 
  : rank_specifier 
  | '*'
  ;
multiplicative_expression
  : unary_expression
  | multiplicative_expression '*' unary_expression  
  | multiplicative_expression '/' unary_expression
  | multiplicative_expression '%' unary_expression
  ;
additive_expression
  : multiplicative_expression
  | additive_expression '+' multiplicative_expression
  | additive_expression '-' multiplicative_expression
  ;
shift_expression
  : additive_expression 
  | shift_expression LTLT additive_expression
  | shift_expression GTGT additive_expression 
  ;
 

relational_expression
  : shift_expression
  | relational_expression '<' shift_expression 
  | relational_expression '>' shift_expression
  | relational_expression LEQ shift_expression
  | relational_expression GEQ shift_expression
  | relational_expression IS non_null_type /* HACK */                                        
  | relational_expression AS type                                         
  ;
equality_expression
  : relational_expression
  | equality_expression EQEQ relational_expression
  | equality_expression NOTEQ relational_expression
  ;
and_expression
  : equality_expression
  | and_expression '&' equality_expression
  ;
exclusive_or_expression
  : and_expression
  | exclusive_or_expression '^' and_expression
  ;
inclusive_or_expression
  : exclusive_or_expression
  | inclusive_or_expression '|' exclusive_or_expression
  ;
conditional_and_expression
  : inclusive_or_expression
  | conditional_and_expression ANDAND inclusive_or_expression
  ;
conditional_or_expression
  : conditional_and_expression
  | conditional_or_expression OROR conditional_and_expression
  ;
conditional_expression
  : conditional_or_expression
  | IDENTIFIER '?' expression ':' expression                            { MakePair(@2,@4);}
  | conditional_or_expression '?' expression ':' expression             { MakePair(@2,@4);}
  | conditional_or_expression QQ expression 
  ;
assignment
  : unary_expression assignment_operator expression
  ;
assignment_operator
  : '=' | PLUSEQ | MINUSEQ | STAREQ | DIVEQ | MODEQ 
  | XOREQ | ANDEQ | OREQ | LTLTEQ | GTGTEQ
  ;
expression
  : assignment
  | nonassign_expression
  ;
  
nonassign_expression
  : conditional_expression 
  | lambda_expression
  | query_expression 
  ;
  
lambda_expression
  : '(' lambda_paramlist_opt ')' LAMBDA lambda_expression_body                      { MakePair(@1,@3);}
  | '(' IDENTIFIER ')' LAMBDA lambda_expression_body                                { MakePair(@1,@3);}
  | impltype_lambda_parameter LAMBDA lambda_expression_body                     
  | '(' IDENTIFIER ',' impltype_lambda_paramlist ')' LAMBDA lambda_expression_body  { MakePair(@1,@5);}
  ;
  
lambda_paramlist_opt
  :
  | lambda_paramlist
  ;
  
lambda_paramlist
  : expltype_lambda_paramlist
  ;  
 
expltype_lambda_paramlist 
  : expltype_lambda_parameter
  | expltype_lambda_paramlist ',' expltype_lambda_parameter
  ;
  
  
impltype_lambda_paramlist
  : impltype_lambda_parameter
  | impltype_lambda_paramlist ',' impltype_lambda_parameter
  ;
  
expltype_lambda_parameter
  : type IDENTIFIER
  ;
 
impltype_lambda_parameter
  : IDENTIFIER
  ;

lambda_expression_body
  : expression
  | block
  ;
 

query_expression
  : start_query from_clause query_body end_query
  ;
  
from_clause
  : FROM type_opt IDENTIFIER IN expression
  ;
  
start_query
  : {inquery++;} 
  ;  
  
end_query
  : {if (inquery > 0) inquery--; }
  ;  
  
query_body
  : query_body_clauses_opt select_or_group_clause query_continuation_opt
  ;
  
query_body_clauses_opt
  : 
  | query_body_clauses
  ;  
  
query_body_clauses
  : query_body_clause
  | query_body_clauses query_body_clause
  ;
  
query_body_clause
  : from_clause
  | let_clause
  | where_clause
  | join_clause
  | join_into_clause
  | orderby_clause
  ;
  
let_clause
  : LET IDENTIFIER '=' expression
  ;
  
where_clause
  : WHERE boolean_expression
  ;
  
join_clause
  : JOIN type_opt IDENTIFIER IN expression ON expression EQUALS expression 
  ;
  
join_into_clause
  : JOIN type_opt IDENTIFIER IN expression ON expression EQUALS expression INTO IDENTIFIER
  ;
  
orderby_clause
  : ORDERBY orderings
  ;
  
orderings
  : ordering
  | orderings ',' ordering
  ;
  
ordering
  : expression ordering_direction_opt
  ;
  
ordering_direction_opt
  :
  | ordering_direction
  ;  
  
ordering_direction
  : ASCENDING
  | DESCENDING
  ;
  
select_or_group_clause
  : select_clause
  | group_clause
  ;
  
select_clause
  : SELECT expression
  ;
  
group_clause
  : GROUP expression BY expression
  ;

query_continuation_opt
  : 
  | query_continuation
  ;
  
query_continuation
  : INTO IDENTIFIER query_body 
  ;

  
constant_expression
  : expression
  ;
boolean_expression
  : expression
  ;
/***** C.2.5 Statements *****/
statement
  : labeled_statement
  | declaration_statement
  | embedded_statement
  ;
embedded_statement
  : block
  | empty_statement
  | expression_statement
  | selection_statement
  | iteration_statement
  | jump_statement
  | try_statement
  | checked_statement
  | unchecked_statement
  | lock_statement
  | using_statement
  | unsafe_statement
  | fixed_statement
  ;
block
  : block_start '{' statement_list_opt '}' block_end                  { MakePair(@2,Pin(@4));}
  ;

block_start
  : { inblock++;}
  ;
  
block_end
  : { if (inblock > 0) inblock--;}
  ;    
  
statement_list_opt
  : /* Nothing */
  | statement_list
  ;

statement_list
  : statement
  | statement_list statement
  ;
empty_statement
  : ';'
  ;
labeled_statement
  : IDENTIFIER ':' statement                             { OverrideToken(@1, TokenClass.Other); }
  ;
declaration_statement
  : local_variable_declaration ';'                                  {Pin(@2);}
  | local_constant_declaration ';'                                  {Pin(@2);}
  ;
local_variable_declaration
  : type variable_declarators
  | FROM variable_declarators                                    { OverrideToken(@1, TokenClass.Type); }
  ;
variable_declarators
  : variable_declarator                                        { $$ = new ArrayList(); $$.Add($1); }
  | variable_declarators ',' variable_declarator             { $$ = $1;  $$.Add($3); }
  ;
variable_declarator
  : IDENTIFIER                                                 { $$ = new CodeField($1); $$.Location = @1; }
  | IDENTIFIER '=' variable_initializer                        { $$ = new CodeField($1); $$.Location = @1; }
  ;
variable_initializer
  : expression
  | array_initializer
  | stackalloc_initializer
  ;
stackalloc_initializer
  : STACKALLOC type  '[' expression ']'                      { MakePair(@3,@5);}
  ; 
local_constant_declaration
  : CONST type constant_declarators                          
  ;
constant_declarators
  : constant_declarator                                        { $$ = new ArrayList(); $$.Add($1); }
  | constant_declarators ',' constant_declarator             { $$ = $1; $$.Add($3); }
  ;
constant_declarator
  : IDENTIFIER '=' constant_expression                         { $$ = new CodeField($1); $$.Location = @1;}
  ;
expression_statement
  : statement_expression ';'                                   {Pin(@2);}
  ;
statement_expression
  : invocation_expression
  | object_creation_expression
  | assignment
  | post_increment_expression
  | post_decrement_expression
  | pre_increment_expression
  | pre_decrement_expression
  ;
selection_statement
  : if_statement
  | switch_statement
  ;
if_statement
  : IF '(' boolean_expression ')' embedded_statement %prec IFREDUCE   { MakePair(@2,@4);}
  | IF '(' boolean_expression ')' embedded_statement 
    ELSE embedded_statement                                   { MakePair(@2,@4);}
  ;
switch_statement
  : SWITCH '(' expression ')' switch_block                    { MakePair(@2,@4);}
  ;
switch_block
  : '{' switch_sections_opt '}'                               { MakePair(@1,Pin(@3));}
  ;
switch_sections_opt
  : /* Nothing */
  | switch_sections
  ;
switch_sections
  : switch_section
  | switch_sections switch_section
  ;
switch_section
  : switch_labels statement_list
  ;
switch_labels
  : switch_label
  | switch_labels switch_label
  ;
switch_label
  : CASE constant_expression ':'
  | DEFAULT ':'
  ;
iteration_statement
  : while_statement
  | do_statement
  | for_statement
  | foreach_statement
  ;
unsafe_statement
  : UNSAFE block
  ;
while_statement
  : WHILE '(' boolean_expression ')' embedded_statement               { MakePair(@2,@4);}
  ;
do_statement
  : DO embedded_statement WHILE '(' boolean_expression ')' ';'        { MakePair(@4,@6);}
  ;
for_statement
  : FOR '(' for_initializer_opt ';' 
    for_condition_opt ';' for_iterator_opt ')'                        { MakePair(@2,@8);}
    embedded_statement
  ;
for_initializer_opt
  : /* Nothing */
  | for_initializer
  ;
for_condition_opt
  : /* Nothing */
  | for_condition
  ;
for_iterator_opt
  : /* Nothing */
  | for_iterator
  ;
for_initializer
  : local_variable_declaration
  | statement_expression_list
  ;
for_condition
  : boolean_expression
  ;
for_iterator
  : statement_expression_list
  ;
statement_expression_list
  : statement_expression
  | statement_expression_list ',' statement_expression
  ;
foreach_statement
  : FOREACH '(' type IDENTIFIER IN expression ')' embedded_statement  { MakePair(@2,@7); AddAutoComplete(@2, typeof(CodeType), typeof(CodeNamespace));}
  ;
jump_statement
  : break_statement
  | continue_statement
  | goto_statement
  | return_statement
  | throw_statement
  ;
break_statement
  : BREAK ';'                                                   {Pin(@2);}
  ;
continue_statement
  : CONTINUE ';'                                                {Pin(@2);}
  ;
goto_statement
  : GOTO IDENTIFIER ';'                                         { OverrideToken(@2, TokenClass.Other); Pin(@3);}
  | GOTO CASE constant_expression ';'                           {Pin(@4);}
  | GOTO DEFAULT ';'                                            {Pin(@3);}
  ;
return_statement
  : RETURN expression_opt ';'                                    {Pin(@3);}
  ;
expression_opt
  : /* Nothing */
  | expression
  ;
throw_statement
  : THROW expression_opt ';'                                    {Pin(@3);}
  ;
try_statement
  : TRY block catch_clauses
  | TRY block finally_clause
  | TRY block catch_clauses finally_clause
  ;
catch_clauses
  : catch_clause
  | catch_clauses catch_clause
  ;
catch_clause
  : CATCH '(' class_type identifier_opt ')' block                 { MakePair(@2,@5); AddAutoComplete(@2, typeof(CodeType), typeof(CodeNamespace));}
  | CATCH '(' type_name identifier_opt ')' block                  { MakePair(@2,@5); AddAutoComplete(@2, typeof(CodeType), typeof(CodeNamespace));}
  | CATCH block
  ;
identifier_opt
  : /* Nothing */
  | IDENTIFIER
  ;
finally_clause
  : FINALLY block
  ;
checked_statement
  : CHECKED block
  ;
unchecked_statement
  : UNCHECKED block
  ;
lock_statement
  : LOCK '(' expression ')' embedded_statement                    { MakePair(@2,@4);}
  ;
using_statement
  : USING '(' resource_acquisition ')' embedded_statement         { MakePair(@2,@4);}
  ;
resource_acquisition
  : local_variable_declaration
  | expression
  ;
fixed_statement
  : FIXED '('  type fixed_pointer_declarators ')'                 { MakePair(@2,@4);}
    embedded_statement
  ;
fixed_pointer_declarators
  : fixed_pointer_declarator
  | fixed_pointer_declarators ',' fixed_pointer_declarator
  ;
fixed_pointer_declarator
  : IDENTIFIER '=' expression
  ;
compilation_unit
  : using_directives_opt attributes_opt                             { ; }
  | using_directives_opt namespace_member_declarations              { CodeModel.AddRange($2); }
  ;
using_directives_opt
  : /* Nothing */
  | using_directives
  ;
attributes_opt
  : /* Nothing */
  | attributes
  ;
namespace_member_declarations_opt
  : /* Nothing */                                                   
  | namespace_member_declarations
  ;
namespace_declaration
  : attributes_opt NAMESPACE 
    qualified_identifier namespace_body comma_opt                   {CodeNamespace ns = new CodeNamespace($3); 
                                                                     ns.AddRange($4); $$ = ns; $$.Location = @3;}
  ;
comma_opt
  : /* Nothing */
  | ';'                                                              {Pin(@2);}
  ;
  
qualified_identifier
  : gen_qualified_identifier                                        
  ;
  
qualifier
  : norm_qualifier
  ;  
  

norm_qualified_identifier
  : IDENTIFIER                                                  
  | norm_qualifier IDENTIFIER                                       { $$ = $1 + $2;}
  ;

norm_qualifier
  : IDENTIFIER '.'                                                  { $$ = $1 + "."; }
  | norm_qualifier IDENTIFIER '.'                                        { $$ = $1 + $2 + "."; }
  ;
  
long_qualified_identifier
  : gen_qualifier member_name '.'                                   { $$ = $1 + $2 + "."; if ($2.EndsWith(">") || IsKnownType($2)) OverrideToken(@2, TokenClass.Type);  }
  ;  
  
 
gen_qualified_identifier
  : member_name                                                  
  | gen_qualifier member_name                                       { $$ = $1 + $2;}
  ;

gen_qualifier
  : member_name '.'                                                  { $$ = $1 + ".";  if ($1.EndsWith(">") || IsKnownType($1)) OverrideToken(@1, TokenClass.Type); }
  | long_qualified_identifier                                        
  ;
  
namespace_body
  : '{' using_directives_opt namespace_member_declarations_opt '}'  { $$ = $3 ; { MakePair(@1,@4);}}
  ;
  
using_directives
  : using_directive
  | using_directives using_directive
  ;
  
using_directive
  : using_alias_directive
  | using_namespace_directive
  ;
  
using_alias_directive
  : USING IDENTIFIER '=' qualified_identifier ';'                   {
                                                                      AddAutoComplete(@3,typeof(CodeType), typeof(CodeNamespace)); 
                                                                      AddAlias($2, $4);
                                                                      OverrideToken(@2, TokenClass.Type);
                                                                    }
  ;
using_namespace_directive
  : USING namespace_name ';'                                        {
                                                                      AddAutoComplete(@1, true, typeof(CodeNamespace)); 
                                                                      AddImport($2);  Pin(@3);
                                                                    }
  | USING error                                                     { AddAutoComplete(@1, true, typeof(CodeNamespace));}
  ;
namespace_member_declarations
  : namespace_member_declaration                                    { $$ = new CodeElementList($1);}
  | namespace_member_declarations namespace_member_declaration      { $$ = $1; $$.Add($2); }
  ;
namespace_member_declaration
  : namespace_declaration
  | type_declaration
  | error 
  ;
type_declaration
  : class_declaration
  | struct_declaration
  | interface_declaration
  | enum_declaration
  | delegate_declaration
  ;

/***** Modifiers *****/
/* This now replaces:
 * class_modifier, constant_modifier, field_modifier, method_modifier, 
 * property_modifier, event_modifier, indexer_modifier, operator_modifier, 
 * constructor_modifier, struct_modifier, interface_modifier, 
 * enum_modifier, delegate_modifier
 */
modifiers_opt
  : /* Nothing */
  | modifiers
  ;
modifiers
  : modifier
  | modifiers modifier
  ;
modifier
  : ABSTRACT
  | EXTERN
  | INTERNAL
  | NEW
  | OVERRIDE
  | PRIVATE
  | PROTECTED
  | PUBLIC
  | READONLY
  | SEALED
  | STATIC
  | UNSAFE
  | VIRTUAL
  | VOLATILE
  ;
  
in_gen_clause
  : { ingenclause = true; }
  ;
  
gen_clause_list
  : gen_clause
  | gen_clause_list gen_clause
  ;

gen_clause_opt
  :                                                             { ingenclause = false; }
  | gen_clause_list                                             { ingenclause = false; }
  ;
  
gen_clause
  :  WHERE IDENTIFIER gen_class_base                            { OverrideToken(@2, TokenClass.Type); }
  ;
  
gen_class_type
  : STRUCT
  | CLASS
  | NEW '(' ')'
  | OBJECT
  | type_name
  ;
  
gen_class_base
  : ':' gen_type_list 
  ;
  
gen_type_list
  : gen_class_type
  | gen_type_list ',' gen_class_type 
  ;
  
/***** C.2.6 Classes *****/
class_declaration
  : attributes_opt modifiers_opt CLASS IDENTIFIER type_arg_list_opt in_gen_clause
    class_base_opt gen_clause_opt class_body comma_opt                       { CodeRefType ct = new CodeRefType($4); 
                                                                if ($5 != null) ct.GenericArguments = $5.ToArray(typeof(string)) as string[];
                                                                ct.AddRange($9); $$ = ct; $$.Location = @4;
                                                                OverrideToken(@4, TokenClass.Type); }
  ;
class_base_opt
  : /* Nothing */                                              
  | class_base
  ;
class_base
  : ':' class_type                                            { AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  | ':' interface_type_list                                   { AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  | ':' class_type ',' interface_type_list                    { AddAutoComplete(@1, typeof(CodeType), typeof(CodeNamespace)); }
  ;
interface_type_list
  : type_name                                               
  | interface_type_list ',' type_name                        
  ;
class_body
  : '{' class_member_declarations_opt '}'                     { $$ = $2; MakePair(@1,Pin(@3));}
  ;
class_member_declarations_opt
  : /* Nothing */                                             { $$ = new CodeElementList(); }
  | class_member_declarations
  ;
class_member_declarations                                      
  : class_member_declaration                                  { $$ = new CodeElementList($1); }
  | class_member_declarations class_member_declaration        { $$ = $1; $$.Add($2); }
  ;
class_member_declaration
  : constant_declaration                                      
  | field_declaration
  | method_declaration
  | property_declaration
  | event_declaration
  | indexer_declaration
  | operator_declaration
  | constructor_declaration
  | destructor_declaration
  | type_declaration
  | error                                     
  ;
constant_declaration
  : attributes_opt modifiers_opt CONST 
    type constant_declarators ';'                             { 
                                                                CodeElementList cel = new CodeElementList();
                                                                foreach (CodeField cf in $5)
                                                                {
                                                                  cf.Type = $4;
                                                                  cel.Add( cf ); 
                                                                }
                                                                $$ = new CodeComplexMember(cel);
                                                              }
  ;
field_declaration
  : attributes_opt modifiers_opt 
    type variable_declarators ';'                             { 
                                                                
                                                                CodeElementList cel = new CodeElementList();
                                                                foreach (CodeField cf in $4)
                                                                {
                                                                  cf.Type = $3;
                                                                  cel.Add( cf ); 
                                                                }
                                                                $$ = new CodeComplexMember(cel);
                                                              }
  ;
method_declaration
  : method_header in_gen_clause gen_clause_opt  method_body                 { $$ = $1; ingenclause = false;}
  ;
/* Inline return_type to avoid conflict with field_declaration */
method_header
  : attributes_opt modifiers_opt type 
    qualified_identifier '(' formal_parameter_list_opt ')'    { CodeMethod cm = new CodeMethod($4,$3,$6); $$ = cm;  $$.Location = @4;  MakePair(@5,@7); 
                                                                                  
    }
  | attributes_opt modifiers_opt VOID qualified_identifier 
    '(' formal_parameter_list_opt ')'                         { CodeMethod cm = new CodeMethod($4, new TypeRef(typeof(void)), $6);  $$ = cm;
                                                                $$.Location = @4;   MakePair(@5,@7);
                                                                } 
  ;
formal_parameter_list_opt
  : /* Nothing */                                             
  | formal_parameter_list arglist_opt                         { $$ = $1; }
  | ARGLIST
  ;

arglist_opt
  :
  | ',' ARGLIST
  ;
  
return_type
  : type                                                     
  | VOID                                                      { $$ = new TypeRef(typeof(void)); }
  ;
method_body
  : block
  | ';'
  ;
formal_parameter_list
  : formal_parameter                                          { $$ = new CodeElementList($1); }
  | formal_parameter_list ',' formal_parameter              { $$ = $1; $$.Add($3); }
  ;
  /* this is wrong */
formal_parameter
  : fixed_parameter
  | THIS fixed_parameter
  | parameter_array
  ;
fixed_parameter
  : attributes_opt parameter_modifier_opt type IDENTIFIER     { $$ = new CodeParameter($4,$3,$2);}
  ;
parameter_modifier_opt
  : /* Nothing */                                             { $$ = ParameterAttributes.None; }
  | REF                                                       { $$ = (ParameterAttributes.Out | ParameterAttributes.In); } 
  | OUT                                                       { $$ = ParameterAttributes.Out; }
  ;
parameter_array
  : attributes_opt PARAMS type IDENTIFIER                     { $$ = new CodeParameter($4,$3); OverrideToken(@3, TokenClass.Type); }
  ;
  
property_declaration                
  : attributes_opt modifiers_opt type qualified_identifier  
    '{' accessor_declarations '}'                             { $$ = new CodeProperty($4,$3); $$.Location = @4; MakePair(@5,Pin(@7));}
  ;
accessor_declarations
  : inproperty get_accessor_declaration set_accessor_declaration_opt outproperty
  | inproperty set_accessor_declaration get_accessor_declaration_opt outproperty
  ;
  
inproperty
  :                                                           { inproperty = true; } 
  ;  
  
outproperty
  :                                                           { inproperty = false; } 
  ; 
  
inset
  :                                                           { inset = true; }
  ;
     
outset
  :                                                           { inset = false; }
  ;
  
  
set_accessor_declaration_opt
  : /* Nothing */
  | set_accessor_declaration
  ;
get_accessor_declaration_opt
  : /* Nothing */
  | get_accessor_declaration
  ;
get_accessor_declaration
  : attributes_opt modifiers_opt GET outproperty
    accessor_body inproperty
  ;
set_accessor_declaration
  : attributes_opt modifiers_opt SET outproperty inset
    accessor_body outset inproperty
  ;
accessor_body
  : block
  | ';'
  ;
event_declaration
  : attributes_opt modifiers_opt EVENT type variable_declarators ';' { 
                                                                
                                                                CodeElementList cel = new CodeElementList();
                                                                foreach (string s in $5)
                                                                {
                                                                  CodeField cf = new CodeField(s,$4);
                                                                  cf.Location = @4;
                                                                  cel.Add( cf ); 
                                                                }
                                                                $$ = new CodeComplexMember(cel);
                                                              }
  | attributes_opt modifiers_opt EVENT type qualified_identifier 
    '{' event_accessor_declarations '}'                         { 
                                                                 
                                                                  MakePair(@6,Pin(@8));
                                                                  CodeField cf = new CodeField($5,$4);
                                                                  cf.Location = @4;
                                                                $$ = cf;  }
  ;
  
event_start
  : {inevent = true;}
  ;

event_end
  : {inevent = false;}
  ;    
  
event_accessor_declarations
  : event_start event_add_accessor_declaration event_remove_accessor_declaration event_end
  | event_start event_remove_accessor_declaration event_add_accessor_declaration event_end
  ;
  
event_add_accessor_declaration
  : attributes_opt ADD
    block 
  ;
event_remove_accessor_declaration
  : attributes_opt REMOVE
    block 
  ;
  
indexer_declaration
  : attributes_opt modifiers_opt indexer_declarator 
    '{' accessor_declarations '}'                                 { /*$$ = new CodeProperty("Item", null);*/ MakePair(@4,Pin(@6));}
  ;
indexer_declarator
  : type THIS '[' formal_parameter_list ']'                         {   MakePair(@3,@5);}
  | type qualified_this '[' formal_parameter_list ']'             {  MakePair(@3,@5);}
  ;
qualified_this
  : gen_qualifier THIS
  ;
/* Widen operator_declaration to make modifiers optional */
operator_declaration
  : attributes_opt modifiers_opt operator_declarator operator_body
  ;
operator_declarator
  : overloadable_operator_declarator
  | conversion_operator_declarator
  ;
overloadable_operator_declarator
  : type OPERATOR overloadable_operator 
    '(' attributes_opt type IDENTIFIER ')'                                       { MakePair(@4,@8); }
  | type OPERATOR overloadable_operator 
    '(' attributes_opt type IDENTIFIER ',' attributes_opt type IDENTIFIER ')'                   { MakePair(@4,@12); }
  ;
overloadable_operator
  : '+' | '-' 
  | '!' | '~' | PLUSPLUS | MINUSMINUS | TRUE | FALSE
  | '*' | '/' | '%' | '&' | '|' | '^' 
  | LTLT | GTGT | EQEQ | NOTEQ | '>' | '<' | GEQ | LEQ
  ;
conversion_operator_declarator
  : IMPLICIT OPERATOR type '(' type IDENTIFIER ')'                {   MakePair(@4,@7);}
  | EXPLICIT OPERATOR type '(' type IDENTIFIER ')'                { MakePair(@4,@7);}
  ;
constructor_declaration
  : attributes_opt modifiers_opt 
    constructor_declarator constructor_body                   { $$ = $3;  }
  ;
constructor_declarator
  : IDENTIFIER '(' formal_parameter_list_opt ')' 
    constructor_initializer_opt                               { $$ = new CodeConstructor($1, $3); $$.Location = @1;
                                                                 MakePair(@2,@4);  OverrideToken(@1, TokenClass.Type);}
  ;
constructor_initializer_opt
  : /* Nothing */
  | constructor_initializer
  ;
constructor_initializer
  : ':' BASE '(' argument_list_opt ')'                        { MakePair(@3,@5);}
  | ':' THIS '(' argument_list_opt ')'                        { MakePair(@3,@5);}
  ;
destructor_declaration
  : attributes_opt modifiers_opt '~' IDENTIFIER '(' ')' block {  OverrideToken(@4, TokenClass.Type); $$ = new CodeDestructor($4); $$.Location = @4;}
  ;
operator_body
  : block
  | ';'
  ;
constructor_body /*** Added by JP - same as method_body ***/
  : block
  | ';'
  ;

/***** C.2.7 Structs *****/
struct_declaration
  : attributes_opt modifiers_opt STRUCT IDENTIFIER type_arg_list_opt in_gen_clause 
    struct_interfaces_opt gen_clause_opt struct_body comma_opt               { CodeValueType cv = new CodeValueType($4);  
                                                                if ($5 != null) cv.GenericArguments = $5.ToArray(typeof(string)) as string[];
                                                                cv.AddRange($9); $$ = cv; $$.Location = @4;
                                                                OverrideToken(@4, TokenClass.Type);}
  ;
struct_interfaces_opt
  : /* Nothing */                                             
  | struct_interfaces
  ;
struct_interfaces
  : ':' interface_type_list
  ;
struct_body
  : '{' struct_member_declarations_opt '}'                    { $$ = $2; MakePair(@1,Pin(@3));}
  ;
struct_member_declarations_opt
  : /* Nothing */                                             
  | struct_member_declarations
  ;
struct_member_declarations
  : struct_member_declaration                                 { $$ = new CodeElementList($1); }
  | struct_member_declarations struct_member_declaration      { $$ = $1; $$.Add($2); }
  ;
struct_member_declaration
  : constant_declaration
  | field_declaration
  | method_declaration
  | property_declaration
  | event_declaration
  | indexer_declaration
  | operator_declaration
  | constructor_declaration
  | type_declaration
  ;

/***** C.2.8 Arrays *****/
array_initializer
  : '{' variable_initializer_list_opt '}'                     { MakePair(@1,@3);}
  | '{' variable_initializer_list ',' '}'                     { MakePair(@1,@4);}
  ;
variable_initializer_list_opt
  : /* Nothing */
  | variable_initializer_list
  ;
variable_initializer_list
  : variable_initializer
  | variable_initializer_list ',' variable_initializer
  ;

/***** C.2.9 Interfaces *****/
interface_declaration
  : attributes_opt modifiers_opt INTERFACE IDENTIFIER type_arg_list_opt in_gen_clause 
    interface_base_opt gen_clause_opt interface_body comma_opt                 { CodeInterface ci = new CodeInterface($4); 
                                                                  if ($5 != null) ci.GenericArguments = $5.ToArray(typeof(string)) as string[];
                                                                  ci.AddRange($9); $$ = ci; $$.Location = @4;
                                                                  OverrideToken(@4, TokenClass.Type);}
  ;
interface_base_opt
  : /* Nothing */
  | interface_base
  ;
interface_base
  : ':' interface_type_list
  ;
interface_body                                                 
  : '{' interface_member_declarations_opt '}'                   { $$ = $2; MakePair(@1,Pin(@3));}
  ;
interface_member_declarations_opt
  : /* Nothing */                                               
  | interface_member_declarations
  ;
interface_member_declarations 
  : interface_member_declaration                                { $$ = new CodeElementList($1); }
  | interface_member_declarations interface_member_declaration  { $$ = $1; $$.Add($2); }
  ;
interface_member_declaration
  : interface_method_declaration
  | interface_property_declaration
  | interface_event_declaration
  | interface_indexer_declaration
  ;
/* inline return_type to avoid conflict with interface_property_declaration */
interface_method_declaration
  : attributes_opt new_opt type member_name 
    '(' formal_parameter_list_opt ')' gen_clause_opt interface_empty_body      { $$ = new CodeMethod($4,$3,$6); $$.Location = @4;
                                                                  MakePair(@5,@7);  OverrideToken(@3, TokenClass.Type);}
  | attributes_opt new_opt VOID member_name 
    '(' formal_parameter_list_opt ')' gen_clause_opt interface_empty_body      { $$ = new CodeMethod($4, new TypeRef(typeof(void)), $6); 
                                                                  $$.Location = @4; MakePair(@5,@7); }
  ;
new_opt
  : /* Nothing */
  | NEW
  ;
interface_property_declaration
  : attributes_opt new_opt type member_name 
    '{' interface_accessors '}'                                  { $$ = new CodeProperty($4,$3); $$.Location = @4; 
                                                                  MakePair(@5,@7);  OverrideToken(@3, TokenClass.Type);}
  ;
interface_indexer_declaration
  : attributes_opt new_opt type THIS 
    '[' formal_parameter_list ']' 
    '{' interface_accessors '}'                                  { MakePair(@5,@7);  MakePair(@8,@10);
                                                                   $$ = new CodeProperty("Item", $3); $$.Location = @4;
                                                                   
                                                                 }
  ;

interface_accessors
  : inproperty attributes_opt GET interface_empty_body outproperty
  | inproperty attributes_opt SET interface_empty_body outproperty
  | inproperty attributes_opt GET interface_empty_body attributes_opt SET interface_empty_body outproperty
  | inproperty attributes_opt SET interface_empty_body attributes_opt GET interface_empty_body outproperty
  ;
interface_event_declaration
  : attributes_opt new_opt EVENT type member_name interface_empty_body    
  ;

interface_empty_body
  : ';'
  ;

/***** C.2.10 Enums *****/
enum_declaration
  : attributes_opt modifiers_opt ENUM IDENTIFIER 
    enum_base_opt enum_body comma_opt                 { CodeEnum ce = new CodeEnum($4); 
                                                        ce.AddRange($6); $$ = ce; $$.Location = @4;
                                                        OverrideToken(@4, TokenClass.Type);}
  ;
enum_base_opt
  : /* Nothing */
  | enum_base
  ;
enum_base
  : ':' integral_type
  ;
enum_body
  : '{' enum_member_declarations_opt '}'              { $$ = $2; MakePair(@1,Pin(@3));} 
  | '{' enum_member_declarations ',' '}'            { $$ = $2; MakePair(@1,Pin(@4)); }
  ;
enum_member_declarations_opt
  : /* Nothing */                                     
  | enum_member_declarations
  ;
enum_member_declarations
  : enum_member_declaration                           { $$ = new CodeElementList($1); }
  | enum_member_declarations ',' 
    enum_member_declaration                           { $$ = $1; $$.Add($3); }
  ;
enum_member_declaration
  : attributes_opt IDENTIFIER                         { $$ = new CodeField($2, new TypeRef(typeof(int))); $$.Location = @2;}
  | attributes_opt IDENTIFIER '=' constant_expression { $$ = new CodeField($2, new TypeRef(typeof(int))); $$.Location = @2;}
  ;

/***** C.2.11 Delegates *****/
delegate_declaration        
  : attributes_opt modifiers_opt DELEGATE return_type member_name 
    '(' formal_parameter_list_opt ')' in_gen_clause gen_clause_opt ';'               { $$ = new CodeDelegate($5,$4,$7); $$.Location = @5;
                                                          MakePair(@6,@8); OverrideToken(@5, TokenClass.Type);
                                                         }
  ;

/***** C.2.12 Attributes *****/
attributes
  : attribute_sections
  ;
attribute_sections
  : attribute_section
  | attribute_sections attribute_section
  ;
attribute_section
  : '[' attribute_list ']'                            { MakePair(@1,@3);}
  | '[' attribute_list ',' ']'                        { MakePair(@1,@4);}
  ;

attribute_list
  : attribute
  | attribute_list ',' attribute
  ;
attribute
  : attribute_name attribute_arguments_opt
  ;
attribute_arguments_opt
  : /* Nothing */
  | attribute_arguments
  ;
attribute_name
  : type_name                                         
  ;
attribute_arguments
  : '(' expression_list_opt ')'                       { MakePair(@1,@3);}
  ;

%%

string[] defaultrefs = {"mscorlib.dll", "System.dll", "System.Xml.dll", "System.Drawing.dll", "System.Windows.Forms.dll"};

protected override string[] DefaultReferences
{
  get { return defaultrefs; }
}

protected override void AfterPinRestore()
{
  inset = false;
  inproperty = false;
  inquery = 0;
  inblock = 0;
  inevent = false;
  ingenclause = false;
}

public override bool HasFoldInfo
{
  get {return true; }
}

public override bool SupportsNavigation
{
  get {return true; }
}



bool inset = false;
bool inproperty = false;
int inblock = 0;
int inquery = 0;
bool inevent = false;
bool _ingenclause = false;
bool ingenclause 
{
  get {return _ingenclause; }
  set { _ingenclause = value; }
}

protected override int yylex()
{
  int t = base.yylex();
  
  if (yylval.Type == CSharpLexer.FROM)
  {
    if(yypeek() == CSharpLexer.IDENTIFIER || yypeekval.Class == TokenClass.Keyword)
    {
      OverrideToken(yylval.Location, TokenClass.Keyword);
      return CSharpLexer.FROM;
    }
    else
    {
      return CSharpLexer.IDENTIFIER;
    }
  }
  
  
  if (yylval.Type == CSharpLexer.WHERE)
  {
    if (ingenclause || inquery > 0)
    {
      OverrideToken(yylval.Location, TokenClass.Keyword);
      return CSharpLexer.WHERE;
    }
    else
    {
      return CSharpLexer.IDENTIFIER;
    }
  }
  
  if (inevent)
  {
    if (yylval.Type == CSharpLexer.IDENTIFIER)
    {
      if (yylval.Text == "add")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.ADD;
      }
      if (yylval.Text == "remove")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.REMOVE;
      }
    }
  }
  
  if (inproperty)
  {
    if (yylval.Type == CSharpLexer.IDENTIFIER)
    {
      if (yylval.Text == "get")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.GET;
      }
      if (yylval.Text == "set")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.SET;
      }

    }
  }
  
  if (inset)
  {
    if (yylval.Type == CSharpLexer.IDENTIFIER)
    {
      if (yylval.Text == "value")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return t;
      }
    }
  }
  
  if (inquery > 0)
  {
    if (yylval.Type == CSharpLexer.IDENTIFIER)
    {
      if (yylval.Text == "select")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.SELECT;
      }
      if (yylval.Text == "into")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.INTO;
      }
      if (yylval.Text == "join")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.JOIN;
      }
      if (yylval.Text == "group")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.GROUP;
      }
      if (yylval.Text == "let")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.LET;
      }
      if (yylval.Text == "on")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.ON;
      }
      if (yylval.Text == "equals")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.EQUALS;
      }
      if (yylval.Text == "ascending")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.ASCENDING;
      }
      if (yylval.Text == "descending")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.DESCENDING;
      }
      if (yylval.Text == "orderby")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.ORDERBY;
      }
      if (yylval.Text == "by")
      {
        OverrideToken(yylval.Location, TokenClass.Keyword);
        return CSharpLexer.BY;
      }      
    }
  }
  
  return t;
  
}

protected internal override string[] CommentLines(string[] lines)
{
  string[] newlines = new string[lines.Length];
  for (int i = 0; i < lines.Length; i++)
  {
    if (lines[i].StartsWith("//"))
    {
      newlines[i] = lines[i];
    }
    else
    {
      newlines[i] = "//" + lines[i];
    }
  }
  return newlines;
}

protected internal override string[] UnCommentLines(string[] lines)
{
  string[] newlines = new string[lines.Length];
  for (int i = 0; i < lines.Length; i++)
  {
    if (lines[i].StartsWith("//"))
    {
      newlines[i] = lines[i].Substring(2);
    }
    else
    {
      newlines[i] = lines[i];
    }
  }
  return newlines;
}

protected override void Preprocess(IEnumerator tokens)
{
  while (tokens.MoveNext())
  {
    ValueType t = (ValueType)tokens.Current;
    
    switch (t.text.Trim())
    {
      case "#":
        break;
      case "define":
        if (tokens.MoveNext())
        {
          t = (ValueType)tokens.Current;
          AddDefine(t.text, t.Location);
        }
        break;
      case "if":
        if (tokens.MoveNext())
        {
          t = (ValueType)tokens.Current;
          AddConditional(t.text, t.Location);
        }
        break;
      case "else":
        AltConditional(t.Location);
        break;
      case "endif":
        EndConditional(t.Location);
        break;
      case "region":
        if (tokens.MoveNext())
        {
          // BIG HACK!!!
          if (t.Location.LineNumber < ((ValueType)tokens.Current).Location.LineNumber)
          {
            AddRegion("#region", t.Location);
          }
          else
          {
            t = (ValueType)tokens.Current;
            AddRegion(t.text, t.Location);
          }
        }
        break;
      case "endregion":
        EndRegion(t.Location);
        break;
      default:
        break;
      
    }
  }
}


    



