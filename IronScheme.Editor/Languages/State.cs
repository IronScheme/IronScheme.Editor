// Gardens Point Parser Generator (Runtime component)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Xacc.Languages.gppg
{
  [DebuggerDisplay("{num}")]
  public class State
  {
    public int num;
    public Dictionary<int, int> parser_table;  // Terminal -> ParseAction
    public Dictionary<int, int> conflict_table = new Dictionary<int,int>(3);  // Terminal -> ParseAction
    public Dictionary<int, int> Goto;          // NonTerminal -> State;
    public int defaultAction = 0;			   // ParseAction


    public State(int[] actions, int[] gotos)
      : this(actions)
    {
      Goto = new Dictionary<int, int>(gotos.Length/2);
      for (int i = 0 ; i < gotos.Length ; i += 2)
        Goto.Add(gotos[i], gotos[i + 1]);
    }

    public State(int[] actions, int[] gotos, int[] conflicts)
      : this(actions, gotos)
    {
      for (int i = 0; i < conflicts.Length; i += 2)
        conflict_table.Add(conflicts[i], conflicts[i + 1]);
    }

    public State(int[] actions)
    {
      parser_table = new Dictionary<int, int>(actions.Length/2);
      for (int i = 0 ; i < actions.Length ; i += 2)
        parser_table.Add(actions[i], actions[i + 1]);
    }

    public State(int defaultAction)
    {
      this.defaultAction = defaultAction;
    }

    public State(int defaultAction, int[] gotos)
      : this(defaultAction)
    {
      Goto = new Dictionary<int, int>(gotos.Length/2);
      for (int i = 0 ; i < gotos.Length ; i += 2)
        Goto.Add(gotos[i], gotos[i + 1]);
    }

    public State(int defaultAction, int[] gotos, int[] conflicts)
      : this(defaultAction, gotos)
    {
      for (int i = 0; i < conflicts.Length; i += 2)
        conflict_table.Add(conflicts[i], conflicts[i + 1]);
    }
#if DEBUG
    public string DebugInfo
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Default = {{{0}}}, ", defaultAction);
        sb.Append("Actions = {");
        foreach (int i in parser_table.Values)
        {
          sb.AppendFormat("{0}:{1}, ", i, parser_table[i]);
        }
        if (parser_table.Count > 0)
        {
          sb.Length -= 2;
        }
        sb.Append("}, ");

        sb.Append("Gotos = {");
        foreach (int i in Goto.Values)
        {
          sb.AppendFormat("{0}:{1}, ", i, Goto[i]);
        }
        if (Goto.Count > 0)
        {
          sb.Length -= 2;
        }
        sb.Append("}, ");

        sb.Append("Conflicts = {");
        foreach (int i in conflict_table.Values)
        {
          sb.AppendFormat("{0}:{1}, ", i, conflict_table[i]);
        }
        if (conflict_table.Count > 0)
        {
          sb.Length -= 2;
        }

        sb.Append("}");

        return sb.ToString();
      }
    }
#endif
  }
}