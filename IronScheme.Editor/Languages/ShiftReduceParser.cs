// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)

//#define TRACEPARSER

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace Xacc.Languages.gppg
{
  [CLSCompliant(false)]
  public abstract class ShiftReduceParser<ValueType> : Xacc.Languages.CSLex.Language<ValueType> where ValueType : struct, Xacc.ComponentModel.IToken
  {
    public IScanner<ValueType> scanner;

    protected ValueType yyval;
    protected ValueType yylval;
    protected ValueType yypeekval;

    private int next;
    private State current_state;

    private bool recovering;
    private int tokensSinceLastError;

    private ParserStack<State> state_stack = new ParserStack<State>(8);
    protected ParserStack<ValueType> value_stack = new ParserStack<ValueType>(8);

    protected string[] nonTerminals;
    protected State[] states;
    protected Rule[] rules;

    protected int errToken;
    protected int eofToken;

    protected abstract void Initialize();

    [Conditional("TRACEPARSER")]
    void WriteLine(string format, params object[] args)
    {
      System.Diagnostics.Trace.WriteLine(string.Format(format, args));
    }

    [Conditional("TRACEPARSER")]
    void Write(string format, params object[] args)
    {
      System.Diagnostics.Trace.Write(string.Format(format, args));
    }

    protected int yypeek()
    {
      if (tokenstream.Count <= (tokenpos + 1))
      {
        int next = scanner.yylex();
        tokenstream.Add(yypeekval = scanner.yylval);
        return next;
      }
      return (yypeekval = tokenstream[tokenpos + 1]).Type;     
    }

    protected virtual int yylex()
    {
      lexcount++;

      if (tokenstream.Count <= tokenpos)
      {
        int next = scanner.yylex();
        yylval = scanner.yylval;
        tokenstream.Add(yylval);
        tokenpos++;
        return next;
      }
      scanner.yylval = yylval = tokenstream[tokenpos++];
      return yylval.Type;
    }

    int tokenpos = 0;

    readonly List<ValueType> tokenstream = new List<ValueType>();
    readonly Dictionary<int, int> reducehandles = new Dictionary<int, int>();

    readonly Stack<ParserState> parserstates = new Stack<ParserState>();

    readonly Stack<State> reducestack = new Stack<State>();

    class ParserState
    {
      public ParserStack<State> state_stack;
      public ParserStack<ValueType> value_stack;
      public int tokenpos;

      public ParserState(ShiftReduceParser<ValueType> parser)
      {
        tokenpos = parser.tokenpos - 1;
        state_stack = parser.state_stack.Clone();
        value_stack = parser.value_stack.Clone();
      }

      public void Restore(ShiftReduceParser<ValueType> parser)
      {
        parser.tokenpos = tokenpos;
        parser.state_stack = state_stack;
        parser.value_stack = value_stack;

        parser.PopTill(parser.tokenstream[tokenpos].Location);

        parser.next = 0;
        parser.current_state = parser.state_stack.Top();
      }
    }

    int saves = 0;
    int restores = 0;
    int pinrescue = 0;
    int lexcount = 0;
    
    public override bool Parse()
    {
      if (scanner == null)
      {
        scanner = lexer;
      }
      tokenstream.Clear();
      reducehandles.Clear();
      parserstates.Clear();
      state_stack.Clear();
      value_stack.Clear();
      reducestack.Clear();
      tokenpos = 0;
      saves = 0;
      restores = 0;
      pinrescue = 0;
      lexcount = 0;
      lastpin = null;
      

      Xacc.ComponentModel.ServiceHost.Error.ClearErrors(lexer);
      if (states == null)
      {
        Initialize();	// allow derived classes to instantiate rules, states and nonTerminals
      }

      lexer.eofToken = this.eofToken;

      next = 0;
      current_state = states[0];

      state_stack.Push(current_state);
      value_stack.Push(yyval);

      while (true)
      {
        WriteLine("Entering state {0} ", current_state.num);
        int action = current_state.defaultAction;

        if (current_state.parser_table != null)
        {
          if (next == 0)
          {
            Write("Reading a token: ");
            next = yylex();
          }

          WriteLine("Next token is {0}", TerminalToString(next));
          int tryaction;

          //handle reduce   



          //else
          if (!reducehandles.ContainsKey(tokenpos) && 
            current_state.parser_table.TryGetValue(next, out tryaction))
          {
            //
            action = tryaction;

            if (current_state.conflict_table.TryGetValue(next, out tryaction))
            {
              //action = tryaction;
              ParserState ps = new ParserState(this);
              reducehandles[tokenpos] = action;
              parserstates.Push(ps);
              reducestack.Push(current_state);

              saves++;
            }
          }
          else if (current_state.conflict_table.TryGetValue(next, out tryaction))
          {
            action = tryaction;
            reducehandles.Remove(tokenpos);
          }
          else if (next == eofToken)
          {
            ReportError();
            return false;
          }
        }

        if (action > 0)         // shift
        {
          Shift(action);
        }
        else if (action < 0)   // reduce
        {
          Reduce(-action);

          if (next == 0 && reducestack.Count > 0 && lastreducestate == reducestack.Peek())
          {
            parserstates.Pop();
            reducestack.Pop();
          }

          if (action == -1)	// accept
            return true;
        }
        else if (action == 0)   // error
        {
          if (CanRestore(parserstates))
          {
            reducestack.Pop();
            ParserState ps = parserstates.Pop();
            ps.Restore(this);

            restores++;
          }
          else
          if (!ErrorRecovery())
          {
            return false;
          }
        }
      }
    }

    Xacc.CodeModel.Location lastpin = null;

    private bool CanRestore(Stack<ShiftReduceParser<ValueType>.ParserState> parserstates)
    {
      if (parserstates.Count > 0)
      {
        ParserState s = parserstates.Peek();
        Xacc.CodeModel.Location l = tokenstream[s.tokenpos].Location;
        if (lastpin == null)
        {
          return true;
        }
        else
        {
          if (l > lastpin)
          {
            return true;
          }
          else
          {
            parserstates.Clear();
            reducestack.Clear();
            AfterPinRestore();
            pinrescue++;
            return false;
          }
        }

      }
      return false;
    }

    protected Xacc.CodeModel.Location Pin(Xacc.CodeModel.Location loc)
    {
      return (lastpin = loc);
    }


    protected void Shift(int state_nr)
    {
      Write("Shifting token {0}, ", TerminalToString(next));
      current_state = states[state_nr];

      value_stack.Push(yylval);
      state_stack.Push(current_state);

      if (recovering)
      {
        if (next != errToken)
        {
          tokensSinceLastError++;
        }

        if (tokensSinceLastError > 5)
        {
          recovering = false;
        }
      }

      if (next != errToken)
        next = 0;
    }

    int rhslen;

    State lastreducestate;

    protected void Reduce(int rule_nr)
    {
      DisplayRule(rule_nr);

      Rule rule = rules[rule_nr];
      rhslen = rule.rhs.Length;

      if (rhslen == 1)
      {
        yyval = value_stack.array[value_stack.top - rhslen];
      }
      else
      {
        yyval = new ValueType();
      }


#if !DEBUG
      try
      {
        DoAction(rule_nr);
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex, "Parser action");
      }
#else
      DoAction(rule_nr);
#endif

      if (rhslen > 1 && yyval.Location == null)
      {
        yyval.Location = value_stack.array[value_stack.top - rhslen].Location
          + value_stack.array[value_stack.top - 1].Location;
      }
#if DEBUG && false
      if (yyval.Location != null)
      {
        Xacc.Controls.AdvancedTextBox atb = Xacc.ComponentModel.ServiceHost.File.CurrentControl as Xacc.Controls.AdvancedTextBox;
        if (atb != null)
        {
          atb.ParseLocation = yyval.Location;
        }
      }
#endif

      for (int i = 0; i < rhslen; i++)
      {
        state_stack.Pop();
        value_stack.Pop();
      }

      DisplayStack();

      lastreducestate = current_state = state_stack.Top();

      int trylhs;
      if (current_state.Goto.TryGetValue(rule.lhs, out trylhs))
      {
        current_state = states[trylhs];
      }

      // isnt this an error on the 'else'?
      state_stack.Push(current_state);
      value_stack.Push(yyval);
    }

    #region Debug
#if DEBUG

    public string Next {get {return TerminalToString(next); } }

    protected string[] stringstates, stringrules;

    public object SS { get { return yyval.Value; } }
    public object S(int i) { return i > rhslen ? null : value_stack.array[value_stack.top - rhslen - 1 + i].Value; }
    public object S1 { get { return S(1); } }
    public object S2 { get { return S(2); } }
    public object S3 { get { return S(3); } }
    public object S4 { get { return S(4); } }
    public object S5 { get { return S(5); } }
    public object S6 { get { return S(6); } }
    public object S7 { get { return S(7); } }
    public object S8 { get { return S(8); } }
    public object S9 { get { return S(9); } }

    public object LL { get { return yyval.Location; } }
    public object L(int i) { return i > rhslen ? null : value_stack.array[value_stack.top - rhslen - 1 + i].Location; }
    public object L1 { get { return L(1); } }
    public object L2 { get { return L(2); } }
    public object L3 { get { return L(3); } }
    public object L4 { get { return L(4); } }
    public object L5 { get { return L(5); } }
    public object L6 { get { return L(6); } }
    public object L7 { get { return L(7); } }
    public object L8 { get { return L(8); } }
    public object L9 { get { return L(9); } }

    public object[] ValueStack
    {
      get 
      {
        int l = value_stack.top;
        object[] values = new object[l];

        for (int i = 0; i < l; i++)
        {
          values[i] = value_stack.array[i].Value;
        }
        
        return values;
      }
    }

    public int[] StateStack
    {
      get
      {
        int l = state_stack.top;
        int[] values = new int[l];

        for (int i = 0; i < l; i++)
        {
          values[i] = state_stack.array[i].num;
        }
        
        return values;
      }
    }

    public string[] StringStateStack
    {
      get
      {
        int l = state_stack.top;
        string[] values = new string[l];

        for (int i = 0; i < l; i++)
        {
          values[i] = stringstates[state_stack.array[i].num];
        }

        return values;
      }
    }

#endif
    #endregion

    protected abstract void DoAction(int action_nr);
    
    public bool ErrorRecovery()
    {
      if (!recovering) // if not recovering from previous error
      {
        ReportError();
      }

      recovering = true;
      tokensSinceLastError = 0;

      if (!FindErrorRecoveryState())
      {
        return false;
      }

      ShiftErrorToken();

      return DiscardInvalidTokens();
    }

    protected virtual bool SuppressAllErrors { get { return false; } }

    public void ReportError()
    {
      StringBuilder errorMsg = new StringBuilder();
      errorMsg.AppendFormat("syntax error, unexpected {0}", TerminalToString(next));

      if (current_state.parser_table.Count < 20)
      {
        bool first = true;
        foreach (int terminal in current_state.parser_table.Keys)
        {
          if (first)
          {
            errorMsg.Append(", expecting ");
          }
          else
          {
            errorMsg.Append(", or ");
          }

          errorMsg.Append(TerminalToString(terminal));
          first = false;
        }
      }

      if (SuppressErrors || SuppressAllErrors)
      {
        Trace.WriteLine(errorMsg + " @ " + yylval.Location, "Parser         ");
      }
      else
      {
        Trace.WriteLine(errorMsg + " @ " + yylval.Location, "Parser         ");
        scanner.yyerror(errorMsg.ToString());
      }
    }


    public void ShiftErrorToken()
    {
      int old_next = next;
      next = errToken;

      Shift(current_state.parser_table[next]);

      WriteLine("Entering state {0} ", current_state.num);

      next = old_next;
    }


    public bool FindErrorRecoveryState()
    {
      while (true)    // pop states until one found that accepts error token
      {
        int tryaction;
        if (current_state.parser_table != null &&
          current_state.parser_table.TryGetValue(errToken, out tryaction) &&
          tryaction > 0) // shift
        {
          return true;
        }

        if (!state_stack.IsEmpty())
        {
          WriteLine("Error: popping state {0}", state_stack.Top().num);

          state_stack.Pop();
          value_stack.Pop();

          DisplayStack();
        }

        if (state_stack.IsEmpty())
        {

          Write("Aborting: didn't find a state that accepts error token");
          return false;
        }
        else
        {


          current_state = state_stack.Top();
        }
      }
    }


    public bool DiscardInvalidTokens()
    {

      int action = current_state.defaultAction;

      if (current_state.parser_table != null)
      {
        // Discard tokens until find one that works ...
        while (true)
        {
          if (next == 0)
          {
            Write("Reading a token: ");
            next = yylex();
          }

          WriteLine("Next token is {0}", TerminalToString(next));

          if (next == errToken)
            return false;

          if (next == eofToken)
          {
            return true;
          }

          int tryaction;
          if (current_state.parser_table.TryGetValue(next, out tryaction))
          {
            action = tryaction;
          }

          if (action != 0)
            return true;
          else
          {
            WriteLine("Error: Discarding {0}", TerminalToString(next));
            next = 0;
          }
        }
      }
      else
      {
        next = 0;
        return true;
      }
    }


    protected void yyerrok()
    {
      recovering = false;
    }


    protected void AddState(int statenr, State state)
    {
      states[statenr] = state;
      state.num = statenr;
    }

    [Conditional("TRACEPARSER")]
    private void DisplayStack()
    {
      Write("State now");
      for (int i = 0 ; i < state_stack.top ; i++)
        Write(" {0}", state_stack.array[i].num);
      WriteLine("");
    }

    [Conditional("TRACEPARSER")]
    private void DisplayRule(int rule_nr)
    {
      Write("Reducing stack by rule {0}, ", rule_nr);
      DisplayProduction(rules[rule_nr]);
    }

    [Conditional("TRACEPARSER")]
    private void DisplayProduction(Rule rule)
    {
      if (rule.rhs.Length == 0)
        Write("/* empty */ ");
      else
        foreach (int symbol in rule.rhs)
          Write("{0} ", SymbolToString(symbol));

      WriteLine("-> {0}", SymbolToString(rule.lhs));
    }


    protected abstract string TerminalToString(int terminal);


    private string SymbolToString(int symbol)
    {
      if (symbol < 0)
        return nonTerminals[-symbol];
      else
        return TerminalToString(symbol);
    }


    protected string CharToString(char ch)
    {
      switch (ch)
      {
        case '\a':
          return @"'\a'";
        case '\b':
          return @"'\b'";
        case '\f':
          return @"'\f'";
        case '\n':
          return @"'\n'";
        case '\r':
          return @"'\r'";
        case '\t':
          return @"'\t'";
        case '\v':
          return @"'\v'";
        case '\0':
          return @"'\0'";
        default:
          return string.Format("'{0}'", ch);
      }
    }
  }
}