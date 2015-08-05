// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System.Collections.Generic;
using System.Text;


namespace gpcc
{
  public class State
  {
    private static int TotalStates = 0;

    public int num;
    public Symbol accessedBy = null;
    public List<ProductionItem> kernal_items = new List<ProductionItem>();
    public List<ProductionItem> all_items = new List<ProductionItem>();
    public Dictionary<Symbol, State> Goto = new Dictionary<Symbol, State>();
    public Set<Terminal> terminalTransitions = new Set<Terminal>();
    public Dictionary<NonTerminal, Transition> nonTerminalTransitions = new Dictionary<NonTerminal, Transition>();
    public Dictionary<Terminal, ParserAction> parseTable = new Dictionary<Terminal, ParserAction>();
    public Dictionary<Terminal, ParserAction> conflictTable = new Dictionary<Terminal, ParserAction>();


    public State(Production production)
    {
      num = TotalStates++;
      AddKernal(production, 0);
    }


    public State(List<ProductionItem> itemSet)
    {
      num = TotalStates++;
      kernal_items.AddRange(itemSet);
      all_items.AddRange(itemSet);
    }


    public void AddClosure()
    {
      foreach (ProductionItem item in kernal_items)
        AddClosure(item);
    }


    private void AddClosure(ProductionItem item)
    {
      if (item.pos < item.production.rhs.Count)
      {
        Symbol rhs = item.production.rhs[item.pos];
        if (rhs is NonTerminal)
          foreach (Production p in ((NonTerminal)rhs).productions)
            AddNonKernal(p);
      }
    }


    private void AddKernal(Production production, int pos)
    {
      ProductionItem item = new ProductionItem(production, pos);
      kernal_items.Add(item);
      all_items.Add(item);
    }


    private void AddNonKernal(Production production)
    {
      ProductionItem item = new ProductionItem(production, 0);

      if (!all_items.Contains(item))
      {
        all_items.Add(item);
        AddClosure(item);
      }
    }


    public void AddGoto(Symbol s, State next)
    {
      this.Goto[s] = next;

      if (s is Terminal)
        terminalTransitions.Add((Terminal)s);
      else
        nonTerminalTransitions.Add((NonTerminal)s, new Transition(this, (NonTerminal)s, next));
    }

    public string GetDebug()
    {
      StringBuilder builder = new StringBuilder();

      foreach (ProductionItem item in kernal_items)
      {
        builder.AppendFormat("{0}", item);
        builder.AppendLine();
      }

      return builder.ToString().TrimEnd('\n','\r');
    }


    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();

      builder.AppendFormat("State {0}", num);
      builder.AppendLine();
      builder.AppendLine();

      foreach (ProductionItem item in kernal_items)
      {
        builder.AppendFormat("    {0}", item);
        builder.AppendLine();
      }

      builder.AppendLine();

      foreach (KeyValuePair<Terminal, ParserAction> a in parseTable)
      {
        builder.AppendFormat("    {0,-20} {1}", a.Key, a.Value);
        builder.AppendLine();
      }

      builder.AppendLine();

      foreach (KeyValuePair<NonTerminal, Transition> n in nonTerminalTransitions)
      {
        builder.AppendFormat("    {0,-20} go to state {1}", n.Key, Goto[n.Key].num);
        builder.AppendLine();
      }

      builder.AppendLine();

      if (conflictTable.Count > 0)
      {
        builder.Append("    Conflicts:");
        builder.AppendLine();

        foreach (KeyValuePair<Terminal, ParserAction> a in conflictTable)
        {
          builder.AppendFormat("    {0,-20} {1}", a.Key, a.Value);
          builder.AppendLine();
        }

        builder.AppendLine();
      }

      return builder.ToString();
    }
  }
}