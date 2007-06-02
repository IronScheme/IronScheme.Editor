// Gardens Point Parser Generator (Scanner Interface)
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Collections.Generic;
using System.Text;


namespace Xacc.Languages.gppg
{
  public abstract class IScanner<ValueType>
  {
    public ValueType yylval;
    public abstract int yylex();
    public abstract void yyerror(string format, params object[] args);
  }
}
