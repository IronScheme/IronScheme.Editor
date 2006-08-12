#region License
 /*	  xacc                																											*
 	*		Copyright (C) 2003-2006  Llewellyn@Pritchard.org                          *
 	*																																							*
	*		This program is free software; you can redistribute it and/or modify			*
	*		it under the terms of the GNU Lesser General Public License as            *
  *   published by the Free Software Foundation; either version 2.1, or					*
	*		(at your option) any later version.																				*
	*																																							*
	*		This program is distributed in the hope that it will be useful,						*
	*		but WITHOUT ANY WARRANTY; without even the implied warranty of						*
	*		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the							*
	*		GNU Lesser General Public License for more details.												*
	*																																							*
	*		You should have received a copy of the GNU Lesser General Public License	*
	*		along with this program; if not, write to the Free Software								*
	*		Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
#endregion

#region Includes
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;
using Xacc.Collections;
using Xacc.CodeModel;
using Xacc.Build;

using SR = System.Resources;
using TokenLine = Xacc.Controls.AdvancedTextBox.TextBuffer.TokenLine;
#endregion

namespace Xacc.Languages
{
  /// <summary>
  /// Base class for all languages
  /// </summary>
  [Image("File.Type.NA.png")]
  public abstract class Language
  {
    #region Fields

    ICodeFile codemodel;
    IToken lasttoken;
    readonly Collections.HashTree scopetree = new Xacc.Collections.HashTree();
    readonly Stack scopestack = new Stack();

    /// <summary>
    /// Internal use.
    /// </summary>
    protected IEnumerator lines;

    /// <summary>
    /// Internal use.
    /// </summary>
    protected int yyerrflag = 0;

    /// <summary>
    /// If errors are to be suppressed
    /// </summary>
    protected bool SuppressErrors = false;

    #endregion

    #region Properties

    internal ICollection Imports
    {
      get {return imports;}
    }

    internal IDictionary Aliases
    {
      get {return aliases;}
    }

    /// <summary>
    /// Checks if the language can be used for a file
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <returns>true if language can be used</returns>
    public virtual bool Match(string filename)
    {
      return false;
    }

    /// <summary>
    /// Checks if the language can be used for the firstline of a file
    /// </summary>
    /// <param name="firstline">the firstline of a file</param>
    /// <returns>true if language can be used</returns>
    public virtual bool MatchLine(string firstline)
    {
      return false;
    }

    /// <summary>
    /// Gets whether Language supports fold info
    /// </summary>
    public virtual bool HasFoldInfo
    {
      get {return false; }
    }

    /// <summary>
    /// Gets whether Language is read-only
    /// </summary>
    public virtual bool ReadOnly 
    {
      get {return false;}
    }

    /// <summary>
    /// Gets the last recorded token
    /// </summary>
    public IToken LastToken 
    {
      get {return (lines == null) ? lasttoken : lines.Current as IToken ;}
    }

    /// <summary>
    /// Defines the default file content for a newly created file
    /// </summary>
    public virtual string DefaultFileContent
    {
      get {return Environment.NewLine;}
    }

    /// <summary>
    /// Gets or sets the code model
    /// </summary>
    public ICodeFile CodeModel
    {
      get {return codemodel;}
      set {codemodel = value;}
    }

    /// <summary>
    /// Gets the default language
    /// </summary>
    public static Language Default
    {
      get {return ServiceHost.Language.Default;}
    }
	
    /// <summary>
    /// Gets the default extension for this language
    /// </summary>
    public string	DefaultExtension
    {
      get {return Extensions.Length > 0 ? Extensions[0] : null;}
    }
	
    /// <summary>
    /// Gets a list of extensions that this language supports
    /// </summary>
    public abstract string[]		Extensions			{get;}

    /// <summary>
    /// Gets the name of this language
    /// </summary>
    public abstract string			Name						{get;}

    #endregion

    #region Constructor

    /// <summary>
    /// Creates an instance of Language, and registers it
    /// </summary>
    protected Language() : this(true){}

    
    /// <summary>
    /// Creates an instance of Language
    /// </summary>
    /// <param name="autoreg">if language should be registered</param>
    protected Language(bool autoreg)
    {
      if (autoreg)
      {
        Register();
      }
    }

    /// <summary>
    /// Registers a language
    /// </summary>
    protected void Register()
    {
      ServiceHost.Language.Register(this);
    }

    #endregion

    #region Other goodies

    Hashtable parsedtypes = new Hashtable();

    protected bool IsType(object type)
    {
      if (type is string)
      {
        return parsedtypes.ContainsKey(type);
      }
      return false;
    }

    protected void OverrideToken(Location loc, TokenClass newclass)
    {
      loc.callback = delegate(IToken tok) 
      {
        if (tok.Class != TokenClass.Keyword)
        {
          tok.Class = newclass;
          if (newclass == TokenClass.Type)
          {
            parsedtypes[tok.Text] = loc;
          }
          else
          {
            if (parsedtypes.ContainsKey(tok.Text))
            {
              parsedtypes.Remove(tok.Text);
            }
          }
        }
      };

      if (cb != null)
      {
        cb.Invoke(loc);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    protected internal virtual string[] CommentLines(string[] lines)
    {
      return lines;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    protected internal virtual string[] UnCommentLines(string[] lines)
    {
      return lines;
    }

    #endregion

    #region Menu/Action support

    //list of action types, somehow add this to the menu
    internal ArrayList actions;

    #endregion

    #region yacc stuff

    /// <summary>
    /// Internal use
    /// </summary>
    /// <returns></returns>
    protected abstract int yyparse(IEnumerator lines);

    /// <summary>
    /// Resets the error state when parsing
    /// </summary>
    [Obsolete]
    protected void ResetError()
    {
      yyerrflag = 0;
    }

    /// <summary>
    /// Emits a warning message when parsing
    /// </summary>
    /// <param name="s">the message</param>
    /// <param name="loc">the location</param>
    protected void yywarn(string s, Location loc)
    {
      if (!SuppressErrors)
      {
        if (loc == null)
        {
          ServiceHost.Error.OutputErrors( this,  new ActionResult(0, s, CurrentFilename));
        }
        else
        {
          loc.Warning = true;
          if (cb != null)
          {
            cb.Invoke(loc);
          }
          ServiceHost.Error.OutputErrors( this, new ActionResult(s, loc));
        }
      }
    }

    /// <summary>
    /// Emits an error message when parsing
    /// </summary>
    /// <param name="s">the message</param>
    /// <param name="loc">the location</param>
    protected void yyerror(string s, Location loc)
    {
      if (!SuppressErrors)
      {
        if (loc == null)
        {
          ServiceHost.Error.OutputErrors( this,  new ActionResult(0, s, CurrentFilename));
        }
        else
        {
          loc.Error = true;
          if (cb != null)
          {
            cb.Invoke(loc);
          }
          ServiceHost.Error.OutputErrors( this,  new ActionResult(s, loc));
        }
      }
    }

    /// <summary>
    /// Emits a error message when parsing when an unexpected token was encountered
    /// </summary>
    /// <param name="v">the expected char</param>
    /// <param name="loc">the location</param>
    protected void yyexpect(char v, Location loc)
    {
      if (!SuppressErrors)
      {
        if (loc == null)
        {
          ServiceHost.Error.OutputErrors( this,  new ActionResult(0, string.Format("Expected '{0}'", v), CurrentFilename));
        }
        else
        {
          loc.Error = true;
          if (cb != null)
          {
            cb.Invoke(loc);
          }
          ServiceHost.Error.OutputErrors( this,  new ActionResult(string.Format("Expected '{0}'", v), loc));
        }
      }
    }

    /// <summary>
    /// Emits a error message when parsing when an unexpected token was encountered
    /// </summary>
    /// <param name="v">the expected string</param>
    /// <param name="loc">the location</param>
    protected void yyexpect(string v, Location loc)
    {
      if (!SuppressErrors)
      {
        if (loc == null)
        {
          ServiceHost.Error.OutputErrors( this,  new ActionResult(0, string.Format("Expected {0}", v), CurrentFilename));
        }
        else
        {
          loc.Error = true;
          if (cb != null)
          {
            cb.Invoke(loc);
          }
          ServiceHost.Error.OutputErrors(  this, new ActionResult(string.Format("Expected {0}", v), loc));
        }
      }
    }

    #endregion
   
    #region Parser events

    /// <summary>
    /// Fires before parsing begins
    /// </summary>
    /// <param name="filename">the file being parsed</param>
    protected virtual void Preparse(string filename)
    {
    }

    /// <summary>
    /// Fires when parsing has completed, regardless of the result
    /// </summary>
    protected virtual void Postparse()
    {
    }
    
    #endregion 
 
    #region CodeModel

    /// <summary>
    /// Makes a pair for brace matching
    /// </summary>
    /// <param name="a">left location</param>
    /// <param name="b">right location</param>
    protected void MakePair(Location a, Location b)
    {
      braces[a] = b;
      braces[b] = a;
    }

    /// <summary>
    /// Show autocomplete at a specified location
    /// </summary>
    /// <param name="a">the location</param>
    protected void ShowAutoComplete(Location a)
    {
    }

    /// <summary>
    /// Show member hint at specified location
    /// </summary>
    /// <param name="a">the location</param>
    /// <param name="hint">the hint</param>
    protected void ShowMemberHint(Location a, string hint)
    {
    }

    /// <summary>
    /// Adds a CodeElement to the scope
    /// </summary>
    /// <param name="elem">the element to add</param>
    protected void AddToScope(ICodeElement elem)
    {
      scopetree[scopestack.ToArray()] = elem;
    }

    /// <summary>
    /// Adds a list of CodeElements to the scope
    /// </summary>
    /// <param name="col">the collection to add</param>
    protected void AddToScope(ICollection col)
    {
      if (col == null)
      {
        return;
      }
      foreach (ICodeElement elem in col)
      {
        AddToScope(elem);
      }
    }

    /// <summary>
    /// Push the scope at location
    /// </summary>
    /// <param name="a">the location</param>
    protected void PushScope(Location a)
    {
      scopestack.Push(a);
    }

    /// <summary>
    /// Pops the scope at location
    /// </summary>
    /// <param name="a">the location</param>
    protected void PopScope(Location a)
    {
      scopestack.Pop();
    }

    #endregion

    #region Nested defs

    internal abstract class TokenEnumeratorBase : IEnumerator
    {
      readonly DoubleLinkedList<TokenLine> lines;
      DoubleLinkedList<TokenLine>.IPosition currentline;
      int tokenpos;
      TokenLine tl;
      IToken current;
      protected int line = 1;
      protected readonly float maxlines;
      internal readonly string filename;
      protected readonly Language lang;
      protected bool updatelocations = true;

      public TokenEnumeratorBase(DoubleLinkedList<TokenLine> lines, string filename, Language lang)
      {
        this.maxlines = lines.Count + 1;
        this.lang = lang;
        this.lines = lines;
        this.filename = filename;
        Reset();
      }

      public void Reset()
      {
        line = 1;
        currentline = lines.First;
        tokenpos = -1;
        if (currentline != null)
        {
          tl = currentline.Data;
        }
      }

      public object Current
      {
        get 
        {
          // should never be null, fix the bug
          if (current == null)
          {
            return null;
          }
#if DEBUG && FALSE
          IToken t = (IToken)current;
          Console.WriteLine("T: {0} C: {1} L: {2}", t.Type, t.Class, t.Location);
#endif
          return current;
        }
      }

      public bool MoveNext()
      {
      START: // fix to prevent recursion stack overflow, one could probably use for or while too, thanks to ahz
        if (tl == null)
        {
          return false;
        }
        tokenpos++;
        if (tl.Tokens == null || tl.Tokens.Length == tokenpos)
        {
          if (currentline.Next == null)
          {
            return false;
          }
          line++;
          tokenpos = -1;
          currentline = currentline.Next;
          tl = currentline.Data;
          if (tl == null)
          {
            return false;
          }
          goto START;
        }
        IToken t = tl.Tokens[tokenpos];
        
        if (!IsValid(t))
        {
          goto START;
        }

        if (updatelocations)
        {
          //this is from the lexer, will allways have Location not range
          Location loc = t.Location;

          //check existing error state and clear the line
          if (loc.Error || loc.Warning)// || loc.Paired)
          {
            if (lang.cb != null)
            {
              lang.cb.Invoke(loc);
            }
          }

          //reset error/warning state
          loc.Error = loc.Warning = false;

          loc.filename = filename;
          loc.LineNumber = line;
          //loc.Paired = false;
        }

        current = t;
        return true;
      }

      protected abstract bool IsValid(IToken token);
    }

    sealed class ParseTokenEnumerator : TokenEnumeratorBase
    {
      readonly IStatusBarService sb = ServiceHost.StatusBar;

      public ParseTokenEnumerator(DoubleLinkedList<TokenLine> lines, string filename, Language lang)
        : base (lines, filename, lang) 
      {
        updatelocations = true;
      }

      protected override bool IsValid(IToken token)
      {
        sb.Progress = line / maxlines;
        return token.Class >= 0 && token.Type >= 0 && !token.Location.Disabled;
      }
    }

    sealed class PreprocessorTokenEnumerator : TokenEnumeratorBase
    {
      public PreprocessorTokenEnumerator(DoubleLinkedList<TokenLine> lines, string filename, Language lang)
        : base (lines, filename, lang) { }

      protected override bool IsValid(IToken token)
      {
        bool t = token.Class == TokenClass.Preprocessor;
        if (!t)
        {
          if (token.Location.Disabled != lang.isdisabled)
          {
            lang.cb.Invoke(token.Location);
            token.Location.Disabled = lang.isdisabled;
          }
        }
        return t;
      }
    }

    #endregion

    #region Tracing

    class Trace
    {
      public static void WriteLine(string format, params object[] args)
      {
        Diagnostics.Trace.WriteLine("Language", format, args);
      }
    }

    #endregion

    #region Preprocessing

    bool isdisabled = false;
    readonly Hashtable defines = new Hashtable();
    readonly Stack condstack = new Stack();
    readonly Stack regionstack = new Stack();

    ArrayList pairings = null;

    internal void Preprocess(DoubleLinkedList<TokenLine> lines, string filename, IParserCallback cb, ArrayList pairings, params string[] defined)
    {
      this.pairings = new ArrayList();

      condstack.Clear();
      regionstack.Clear();
      defines.Clear();
      foreach (string d in defined)
      {
        if (d != null)
        {
          defines.Add(d,d);
        }
      }
      this.cb = cb;
      Preprocess( new PreprocessorTokenEnumerator(lines, filename, this));

      Set pp = new Set(pairings) & new Set(this.pairings);

      foreach (Pairing p in this.pairings)
      {
        if (pp.Contains(p))
        {
          // we have to replace it here, else references wont point correctly
          Pairing old = pp.Replace(p) as Pairing;
          p.hidden = old.hidden;
        }
        else
        {
          pp.Add(p);
        }
      }

      pairings.Clear();
      pairings.AddRange(pp);
      pairings.Sort();
      
      while (regionstack.Count > 0)
      {
        Region c = (Region) regionstack.Pop();
        c.start.Error = true;
        cb.Invoke(c.start);
        ServiceHost.Error.OutputErrors( new ActionResult("Region " + c.text + " not terminated",c.start));
      }

      while (condstack.Count > 0)
      {
        Conditional c = (Conditional) condstack.Pop();
        c.start.Error = true;
        cb.Invoke(c.start);
        ServiceHost.Error.OutputErrors( new ActionResult("Conditional " + c.text + " not terminated",c.start));
      }

      this.pairings.Clear();
      this.pairings = null;
    }

    /// <summary>
    /// Gets called when preprossing is required
    /// </summary>
    /// <param name="tokens">the preprocessor tokens</param>
    protected virtual void Preprocess(IEnumerator tokens)
    {

    }

    [Serializable]
    internal sealed class Conditional : Pairing
    {
      public Location alt;
      public bool disabled;
    }

    [Serializable]
    internal sealed class Region : Pairing
    {
    }

    [Serializable]
    internal abstract class Pairing : IComparable
    {
      public string text;
      public Location start;
      public Location end;
      public bool hidden = false;

      public override bool Equals(object obj)
      {
        Pairing b = obj as Pairing;
        if (b == null)
        {
          return false;
        }
        return b.text == text && b.start == start && b.end == end;
      }

#if DEBUG
      public int HashCode
      {
        get { return GetHashCode(); }
      }
#endif

      public override int GetHashCode()
      {
        return text.GetHashCode() ^ start.GetHashCode() ^ (end == null ? 0 : end.GetHashCode());
      }

      public bool IsInside(int line)
      {
        if (start == null)
        {
          return false;
        }
        if (start.LineNumber == line)
        {
          return line <= (end.LineNumber + end.LineCount);
        }
        else
        {
          if (end == null)
          {
            return line > start.LineNumber;
          }
          else
          {
            return line > start.LineNumber &&  line <= (end.LineNumber + end.LineCount);
          }
        }
      }

      int IComparable.CompareTo(object obj)
      {
        Pairing b = obj as Pairing;
        if (b == null)
        {
          return -1;
        }
        return start.LineNumber.CompareTo(b.start.LineNumber);
      }
    }


    /// <summary>
    /// Add a 'define' for conditional testing
    /// </summary>
    /// <param name="text">the 'define'</param>
    /// <param name="loc">the location</param>
    protected void AddDefine(string text, Location loc)
    {
      Trace.WriteLine("Defining: {0} @ {1}", text, loc);
      if (!defines.ContainsKey(text))
      {
        defines.Add(text, loc);
      }
      else
      {
        loc.Error = true;
        cb.Invoke(loc);
      }
    }

    bool IsConditionDisabled(Location loc)
    {
      bool res = false;
      foreach (Conditional c in condstack)
      {
        if (c.alt != null && loc > c.alt)
        {
          res |= !c.disabled;
        }
        else
        {
          res |= c.disabled;
        }
      }
      return res;
    }

    /// <summary>
    /// Gets called when a condtional needs to be evaluated
    /// </summary>
    /// <param name="text">the text to evaluate</param>
    /// <param name="defines"></param>
    /// <returns></returns>
    protected virtual bool EvalConditional(string text, Hashtable defines)
    {
      return !defines.ContainsKey(text);
    }

    /// <summary>
    /// Adds a conditional to the preprocessor
    /// </summary>
    /// <param name="text">the text to test</param>
    /// <param name="loc">the location</param>
    protected void AddConditional(string text, Location loc)
    {
      text = text.Trim();

      Conditional c = new Conditional();
      c.text = text;
      c.start = loc;
      condstack.Push(c);
      pairings.Add(c);
      c.disabled = EvalConditional(text, defines);
      
      isdisabled = IsConditionDisabled(loc);
    }

    /// <summary>
    /// Alternates the previously defined conditional
    /// </summary>
    /// <param name="loc">the location</param>
    protected void AltConditional(Location loc)
    {
      if (condstack.Count > 0)
      {
        Conditional c = (Conditional)condstack.Pop();
        c.alt = loc;
        isdisabled = IsConditionDisabled(loc) | !c.disabled;
        condstack.Push(c);
      }
    }

    /// <summary>
    /// Ends the current conditional
    /// </summary>
    /// <param name="loc">the location</param>
    protected void EndConditional(Location loc)
    {
      if (condstack.Count > 0)
      {
        Conditional c = condstack.Pop() as Conditional;
        c.end = loc;

        isdisabled = IsConditionDisabled(loc);
      }
    }

    /// <summary>
    /// Adds a region to the preprocessor
    /// </summary>
    /// <param name="text">the name of the region</param>
    /// <param name="loc">the location</param>
    protected void AddRegion(string text, Location loc)
    {
      text = text.Trim();
      Region c = new Region();
      c.text = text;
      c.start = loc;
      regionstack.Push(c);
      pairings.Add(c);
    }

    /// <summary>
    /// Ends the current region
    /// </summary>
    /// <param name="loc">the location</param>
    protected void EndRegion(Location loc)
    {
      if (regionstack.Count > 0)
      {
        Region c = regionstack.Pop() as Region;
        c.end = loc;
      }
    }

    #endregion

    #region Autocomplete

    readonly ArrayList imports = new ArrayList();

    /// <summary>
    /// Adds an import to the import list
    /// </summary>
    /// <param name="name">the name to import</param>
    protected void AddImport(string name)
    {
      imports.Add(name);
    }

    readonly Hashtable aliases = new Hashtable();

    /// <summary>
    /// Adds an alias to the file/type
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="value">the value</param>
    protected void AddAlias(string name, string value)
    {
      aliases[name] = value;
    }

    static readonly string[] defaultrefs = {};

    /// <summary>
    /// Gets the default file references
    /// </summary>
    protected virtual string[] DefaultReferences
    {
      get { return defaultrefs; }
    }

    Hashtable autolocations = new Hashtable();

    /// <summary>
    /// Adds AutoComplete info at the specified location
    /// </summary>
    /// <param name="loc">the location</param>
    /// <param name="ignoreimports">whether to ignore imports</param>
    /// <param name="filters">type filters</param>
    protected void AddAutoComplete(Location loc, bool ignoreimports, params Type[] filters)
    {
      autolocations[loc] = filters;
    }

    /// <summary>
    /// Adds AutoComplete info at the specified location
    /// </summary>
    /// <param name="loc">the location</param>
    /// <param name="filters">type filters</param>
    protected void AddAutoComplete(Location loc, params Type[] filters)
    {
      autolocations[loc] = filters;
    }
    
    internal static bool FilterType(Type[] filters, object o)
    {
      if (o == null)
      {
        return false;
      }

      if (filters.Length == 0)
      {
        return true;
      }

      Type ot = o.GetType();
      
      foreach (Type t in filters)
      {
        if (t.IsClass)
        {
          if (t == ot || ot.IsSubclassOf(t))
          {
            return true;
          }
        }
        if (t.IsInterface)
        {
          if (ot.IsSubclassOf(t))
          {
            return true;
          }
        }
      }
      return false;
    }

    static string[] Tokenize(string name, params string[] delimiters)
    {
      return Algorithms.XString.Tokenize(name, delimiters);
    }

    class DefaultImportComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        string[] a = (x as string).Split('.');
        string[] b = (y as string).Split('.');
        int al = a.Length;
        int bl = b.Length;
        if (al == bl)
        {
          return a[0].CompareTo(b[0]);
        }
        else
        {
          return al.CompareTo(bl);
        }
      }
    }

    static readonly IComparer DEFAULTIMPORTCOMPARER = new DefaultImportComparer();

    /// <summary>
    /// Gets the default import comparer
    /// </summary>
    protected virtual IComparer ImportComparer
    {
      get {return DEFAULTIMPORTCOMPARER;}
    }

    static string Join(Type[] t)
    {
      string[] s = new string[t.Length];
      for (int i = 0; i < s.Length; i++)
      {
        s[i] = t[i].Name;
      }
      return string.Join(", ", s);
    }

    ICodeElement[] Complete(string shint, params Type[] filters)
    {
      Set all = new Set();
      imports.Sort(ImportComparer);
  
      ////// HACK ////////

      if (filters.Length == 1 && filters[0] == typeof(CodeNamespace))
      {
        imports.Clear();
        imports.Add("");
      }

      ////// HACK ////////

      System.Diagnostics.Trace.WriteLine("'" + shint + "'", "Hint          ");
      //System.Diagnostics.Trace.WriteLine(Join(filters),     "Filters       ");                           
      System.Diagnostics.Trace.WriteLine(string.Join(", ", imports.ToArray(typeof(string)) as string[]), "Imports       ");


      foreach (string import in imports)
      {
        this.hint = shint;
        if (import.Length > 0)
        {
          this.hint = import + "." + shint;
        }

        string[] hint = Tokenize(this.hint, ".");
        object ao = tree.Accepts(hint);

        if (ao != null)
        {
          //if (FilterType(filters, ao))
          {
            ((ICodeElement)ao).Tag = this.hint;
            all.Add(ao);
          }
        }

        if (UseProjectTreeForAutoComplete)
        {
          ao = tree2.Accepts(hint);

          if (ao != null)
          {
            //if (FilterType(filters, ao))
          {
            all.Add(ao);
          }
          }
        }

        if (hint.Length == 0)
        {
          foreach (string[] s in tree.AcceptFirstStates(hint))
          {
            object val = tree.Accepts(s);
        
            if (val != null)
            //if (FilterType(filters, val))
            {
              ((ICodeElement)val).Tag = this.hint;
              all.Add(val);
            }
          }

          if (UseProjectTreeForAutoComplete)
          {
            foreach (string[] s in tree2.AcceptFirstStates(hint))
            {
              object val = tree2.Accepts(s);
        
              if (val != null)
                //if (FilterType(filters, val))
              {
                ((ICodeElement)val).Tag = this.hint;
                all.Add(val);
              }
            }
          }
        }
        else
        {
          foreach (string s in tree.CompleteFirstStates(hint))
          {
            string[] alls = new string[hint.Length];
            Array.Copy(hint, alls, hint.Length - 1);
            alls[alls.Length - 1] = s;
            string v = string.Join(string.Empty, alls);
            string[] vv = Tokenize(v, ".");
            object val = tree.Accepts(vv);
        
            if (val != null)
            //if (FilterType(filters, val))
            {
              ((ICodeElement)val).Tag = this.hint;
              all.Add(val);
            }
          }

          if (UseProjectTreeForAutoComplete)
          {
            foreach (string s in tree2.CompleteFirstStates(hint))
            {
              string[] alls = new string[hint.Length];
              Array.Copy(hint, alls, hint.Length - 1);
              alls[alls.Length - 1] = s;
              string v = string.Join(string.Empty, alls);
              string[] vv = Tokenize(v, ".");
              object val = tree2.Accepts(vv);
        
              if (val != null)
                //if (FilterType(filters, val))
              {
                ((ICodeElement)val).Tag = this.hint;
                all.Add(val);
              }
            }
          }
        }
      }

      this.hint = shint;

      return all.ToArray(typeof(ICodeElement)) as ICodeElement[];
    }

    string hint;
    Project proj;
    ObjectTree tree, tree2;

    /// <summary>
    /// Gets whether project tree is used for AutoComplete
    /// </summary>
    protected virtual bool UseProjectTreeForAutoComplete
    {
      get {return true;}
    }

    ICodeElement[] AutoComplete(IToken[] tokens, string line, int index)
    {
      //todo use scope as well
      string hint = string.Empty;
      Type[] hints = new Type[0];

      foreach (IToken token in tokens)
      {
        Location loc = token.Location;

        Type[] subhints = autolocations[loc] as Type[]; 
        if (subhints != null)
        {
          hints = subhints;
        }

        bool hack = false;

        if (loc.Column >= index)
        {
          if (loc.EndColumn == index + 1)
          {
            index++;
            hack = true;
          }
          else
          {
            break;
          }
        }

        if (index > line.Length)
        {
          break;
        }

        int len = index - loc.Column;

        if (len > token.Length)
        {
          len = token.Length;
        }
					
        string tok = line.Substring(loc.Column, len);
					
        if (tok == "." || token.Class == TokenClass.Identifier || token.Class == TokenClass.Type)
        {
          hint += tok;
        }
        else 
        {
          if (hack)
          {
            break;
          }
          hint = string.Empty;
        }

        if (hack)
        {
          break;
        }
      }

      hint = hint.Trim();

      this.filters = hints;
  
      return Complete(hint, hints);
    }

    /// <summary>
    /// Load the default file references
    /// </summary>
    /// <param name="proj">the project</param>
    /// <param name="filename">the filename</param>
    protected virtual void LoadDefaultReferences(Project proj, string filename)
    {
      ProgressDialog pd = new ProgressDialog("Loading AutoComplete information. This will only take a few seconds (really).");
      pd.StartPosition = FormStartPosition.CenterScreen;
      ServiceHost.Window.MainForm.AddOwnedForm(pd);
      pd.Show();

      Application.DoEvents();

      string[] vals = null;

      //CustomAction a = proj.GetAction(filename) as CustomAction;
      //if (a != null)
      //{
      //  Option o = a.GetOption("Reference");
      //  if (o != null)
      //  {
      //    vals = a.GetOptionValue(o) as string[];
      //  }
      //}

      if (vals == null)
      {
        vals = DefaultReferences;
      }
      else
      {
        string[] allrefs = new string[vals.Length + DefaultReferences.Length];
        Array.Copy(DefaultReferences, allrefs, DefaultReferences.Length);
        Array.Copy(vals, 0, allrefs, DefaultReferences.Length, vals.Length);
        vals = allrefs;
      }
        
      proj.LoadAssemblies(vals);

      pd.Close();
    }

    Type[] filters;
    readonly ICodeElement[] EMPTY = {};

    internal ICodeElement[] AutoComplete(IToken[] tokens, string line, int index, Project proj, string filename, 
      out string hint, out Type[] filters)
    {
      if (proj == null)
      {
        hint = string.Empty;
        filters = new Type[0];
        return EMPTY;
      }
      this.proj = proj;
      tree = proj.AutoCompleteTree;
      tree2 = proj.ProjectAutoCompleteTree;

      if (proj.References == null)
      {
        LoadDefaultReferences(proj, filename);
      }

      ICodeElement[] hints = AutoComplete(tokens, line, index);
      hint = this.hint;
      filters = this.filters;
      return hints;
    }

    #endregion

    #region Token related

    /// <summary>
    /// Gets the string name of a token
    /// </summary>
    /// <param name="type">the token type</param>
    /// <returns>the token name</returns>
    public virtual string GetTypeName(int type)
    {
      return ((TokenClass) type).ToString();
    }

    static readonly Hashtable typemapping = LanguageService.typemapping;

    /// <summary>
    /// Gets color info based on TokenClass
    /// </summary>
    /// <param name="t">the class</param>
    /// <returns>the corresponding colorinfo object</returns>
    public static ColorInfo GetColorInfo(TokenClass t)
    {
      return GetColorInfo((int)t);
    }

    /// <summary>
    /// Gets ColorInfo for a token type
    /// </summary>
    /// <param name="tokentype">the token type</param>
    /// <returns>the color info</returns>
    public static ColorInfo GetColorInfo(int tokentype)
    {
      TokenClass t = (TokenClass) tokentype;

      if ((t & TokenClass.Custom) != 0)
      {
        if (!typemapping.ContainsKey(t))
        {
          int v = tokentype & ~(int)TokenClass.Custom;
          ColorInfo ci = new ColorInfo(
          Color.FromKnownColor((KnownColor)(v >> 16 & 0xff)),
          Color.FromKnownColor((KnownColor)(v >> 8 & 0xff)),
          Color.FromKnownColor((KnownColor)( v & 0xff)),
          (FontStyle) (v >> 25 & 0xff ));

          typemapping.Add(t, ci);
        }
      }

      if (typemapping.ContainsKey(t))
      {
        return (ColorInfo)typemapping[t];
      }
      else
      {
        return (ColorInfo)typemapping[TokenClass.Any];
      }
    }

    #endregion
  
    #region Parser invoke

    internal Hashtable braces;

    string currfilename;

    /// <summary>
    /// Gets the current filename
    /// </summary>
    protected string CurrentFilename
    {
      get {return currfilename; }
    }

    /// <summary>
    /// Internal use
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    protected int Parse(IEnumerator lines)
    {
      string filename = currfilename = (lines as TokenEnumeratorBase).filename;
      codemodel = null;
      codemodel = new CodeFile(filename);
      ServiceHost.Error.ClearErrors(this);
      //scopetree.Clear();
      parsedtypes.Clear();
      scopestack.Clear();
      imports.Clear();
      imports.Add(string.Empty);
      aliases.Clear();
      braces.Clear();
      scopestack.Push( new Location(0));
      Preparse( filename );
      this.lines = lines;
      int res = yyparse(lines);
      lasttoken = LastToken;
      this.lines = null;
      Postparse();

      return res;
    }

    internal interface IParserCallback
    {
      void Invoke(Location loc);
    }

    /// <summary>
    /// Internal use
    /// </summary>
    internal IParserCallback cb;

    /// <summary>
    /// Internal use
    /// </summary>
    /// <param name="mlines"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    internal virtual int Parse(DoubleLinkedList<TokenLine> mlines, string filename)
    {
      return Parse(mlines, filename, null);
    }

    /// <summary>
    /// Internal use
    /// </summary>
    /// <param name="mlines"></param>
    /// <param name="filename"></param>
    /// <param name="cb"></param>
    /// <returns></returns>
    internal int Parse(DoubleLinkedList<TokenLine> mlines, string filename, IParserCallback cb)
    {
      this.cb = cb;
      return Parse(new ParseTokenEnumerator(mlines, filename, this));
    }

    #endregion

    #region Lexer invoke

    /// <summary>
    /// Lexes a single line of text
    /// </summary>
    /// <param name="line">the line to lex</param>
    /// <param name="state">the state at the end of the previous line</param>
    /// <returns>an array of tokens</returns>
    protected abstract IToken[] Lex(string line, ref Stack state);

    //readonly object LEXLOCK = new object();

    internal IToken[] Lex(string line, ref TokenLine state, DoubleLinkedList<TokenLine> mlines)
    {
      if (line == null)
      {
        return null;
      }
      //lock(LEXLOCK)
      {
        IToken[] tokens = null;

        TokenLine currentline = null, prevline = state;

        Stack ls = null;
        if (prevline == null) //first line
        {
          ls = null;
          if (mlines.First != null)
          {
            currentline = mlines.First.Data;
          }
        }
        else
        {
          //see if we have a line already
          if (prevline != mlines.Last.Data)
          {
            DoubleLinkedList<TokenLine>.IPosition pos = mlines.PositionOf(prevline);

            if (pos == null)
            {
              Debugger.Break();
            }

            pos = pos.Next;

            if (pos != null)
            {
              currentline = pos.Data;
            }
          }

          if (prevline.state != null)
          {
            ls = prevline.state;
          }
          else
          {
            ls = null;
          }
        }

        tokens = Lex(line, ref ls);

        if (currentline != null)
        {
          state = currentline;
          currentline.Tokens = tokens;
          currentline.state = ls;
        }

        return tokens;
      }
    }

    #endregion

    public override string ToString()
    {
      return string.Format("{0}", Name);
    }

	}
}
