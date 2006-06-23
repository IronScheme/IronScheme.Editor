// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Collections.Generic;


namespace gpcc
{
  public class Grammar
  {
    public List<Production> productions = new List<Production>();
    public string unionType;
    public int NumActions = 0;
    public string prologCode;	// before first %%
    public string epilogCode;	// after last %%
    public NonTerminal startSymbol;
    public Production rootProduction;
    public Dictionary<string, NonTerminal> nonTerminals = new Dictionary<string, NonTerminal>();
    public Dictionary<string, Terminal> terminals = new Dictionary<string, Terminal>();

    public string Namespace;
    public List<string> use = new List<string>();
    public string Visibility = "public";
    public string ParserName = "Parser";
    public string TokenName = "Tokens";
    public string ValueTypeName = "ValueType";


    public Grammar()
    {
      LookupTerminal(GrammarToken.Symbol, "error");
      LookupTerminal(GrammarToken.Symbol, "EOF");
    }


    public Terminal LookupTerminal(GrammarToken token, string name)
    {
      if (!terminals.ContainsKey(name))
        terminals[name] = new Terminal(token == GrammarToken.Symbol, name);

      return terminals[name];
    }


    public NonTerminal LookupNonTerminal(string name)
    {
      if (!nonTerminals.ContainsKey(name))
        nonTerminals[name] = new NonTerminal(name);

      return nonTerminals[name];
    }


    public void AddProduction(Production production)
    {
      productions.Add(production);
      production.num = productions.Count;
    }


    public void CreateSpecialProduction(NonTerminal root)
    {
      rootProduction = new Production(LookupNonTerminal("$accept"));
      AddProduction(rootProduction);
      rootProduction.rhs.Add(root);
      rootProduction.rhs.Add(LookupTerminal(GrammarToken.Symbol, "EOF"));
    }
  }
}







