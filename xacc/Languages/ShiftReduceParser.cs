// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)

//#define TRACEPARSER

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace gppg
{
  [CLSCompliant(false)]
  public abstract class ShiftReduceParser<ValueType> : Xacc.Languages.CSLex.Language<ValueType> where ValueType : struct, Xacc.ComponentModel.IToken
  {
    public IScanner<ValueType> scanner;

    protected ValueType yyval;

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
    
    public override bool Parse()
    {
      if (scanner == null)
      {
        scanner = Lexer;
      }
      Xacc.ComponentModel.ServiceHost.Error.ClearErrors(Lexer);
      Initialize();	// allow derived classes to instantiate rules, states and nonTerminals

      Lexer.eofToken = this.eofToken;

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
            next = scanner.yylex();
          }

          WriteLine("Next token is {0}", TerminalToString(next));

          if (current_state.parser_table.ContainsKey(next))
            action = current_state.parser_table[next];
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

          if (action == -1)	// accept
            return true;
        }
        else if (action == 0)   // error
          if (!ErrorRecovery())
            return false;
      }
    }


    protected void Shift(int state_nr)
    {
      Write("Shifting token {0}, ", TerminalToString(next));
      current_state = states[state_nr];

      value_stack.Push(scanner.yylval);
      state_stack.Push(current_state);

      if (recovering)
      {
        if (next != errToken)
          tokensSinceLastError++;

        if (tokensSinceLastError > 5)
          recovering = false;
      }

      if (next != errToken)
        next = 0;
    }

    int rhslen;

    protected void Reduce(int rule_nr)
    {
      DisplayRule(rule_nr);

      Rule rule = rules[rule_nr];
      rhslen = rule.rhs.Length;

      //System.Diagnostics.Trace.Write(string.Format("[{1,3}]{0,-15}:", SymbolToString(rule.lhs), rule.lhs));

      //if (rule.rhs.Length == 0)
      //{
      //  System.Diagnostics.Trace.WriteLine(" /* empty */");
      //}
      //else
      //{
      //  System.Diagnostics.Trace.WriteLine("");
      //  int c = 0;

      //  foreach (int i in rule.rhs)
      //  {
      //    System.Diagnostics.Trace.WriteLine(string.Format("[{2,3}]  {0,-12} = {1}", SymbolToString(i), S(++c) ?? "<null>", i));
      //  }
      //}

      //System.Diagnostics.Trace.WriteLine("");

      if (rhslen == 1)
      {
        yyval = value_stack.array[value_stack.top - rhslen];
      }
      else
      {
        yyval = new ValueType();
      }
      if (rhslen > 1)
      {
        yyval.Location = value_stack.array[value_stack.top - rhslen].Location
          + value_stack.array[value_stack.top - 1].Location;
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

      for (int i = 0; i < rhslen; i++)
      {
        state_stack.Pop();
        value_stack.Pop();
      }

      DisplayStack();

      current_state = state_stack.Top();

      if (current_state.Goto.ContainsKey(rule.lhs))
      {
        current_state = states[current_state.Goto[rule.lhs]];
      }
      // isnt this an error on the 'else'?
      state_stack.Push(current_state);
      value_stack.Push(yyval);
    }

#if DEBUG
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
        int l = value_stack.top - 1;
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
        int l = state_stack.top - 1;
        int[] values = new int[l];

        for (int i = 0; i < l; i++)
        {
          values[i] = state_stack.array[i].num;
        }
        
        return values;
      }
    }

#endif


    protected abstract void DoAction(int action_nr);


    public bool ErrorRecovery()
    {
      if (!recovering) // if not recovering from previous error
        ReportError();

      recovering = true;
      tokensSinceLastError = 0;

      if (!FindErrorRecoveryState())
        return false;

      ShiftErrorToken();

      return DiscardInvalidTokens();
    }


    public void ReportError()
    {
      StringBuilder errorMsg = new StringBuilder();
      errorMsg.AppendFormat("syntax error, unexpected {0}", TerminalToString(next));

      if (current_state.parser_table.Count < 7)
      {
        bool first = true;
        foreach (int terminal in current_state.parser_table.Keys)
        {
          if (first)
            errorMsg.Append(", expecting ");
          else
            errorMsg.Append(", or ");

          errorMsg.Append(TerminalToString(terminal));
          first = false;
        }
      }

      scanner.yyerror(errorMsg.ToString());
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
        if (current_state.parser_table != null &&
          current_state.parser_table.ContainsKey(errToken) &&
          current_state.parser_table[errToken] > 0) // shift
          return true;

        WriteLine("Error: popping state {0}", state_stack.Top().num);

        state_stack.Pop();
        value_stack.Pop();

        DisplayStack();

        if (state_stack.IsEmpty())
        {
          Write("Aborting: didn't find a state that accepts error token");
          return false;
        }
        else
          current_state = state_stack.Top();
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
            next = scanner.yylex();
          }

          WriteLine("Next token is {0}", TerminalToString(next));

          if (next == errToken)
            return false;

          if (current_state.parser_table.ContainsKey(next))
            action = current_state.parser_table[next];

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