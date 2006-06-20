// Gardens Point Parser Generator (Scanner Interface)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Collections.Generic;
using System.Text;


namespace gppg
{
  public interface IScanner<ValueType>
  {
    ValueType yylval { get;set;}
    int yylex();
    void yyerror(string format, params object[] args);
  }
}
