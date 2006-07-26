using System;
using System.Collections.Generic;
using System.Text;
using Xacc.ComponentModel;

namespace Xacc.Languages
{
  public struct AntlrToken : antlr.IToken
  {
    readonly IToken token;
    public AntlrToken(IToken token)
    {
      this.token = token;
    }

    public int Type
    {
      get { return token.Type; }
      set { token.Type = value;}
    }

    public int getColumn()
    {
      return token.Location.Column;
    }

    public string getFilename()
    {
      return token.Location.Filename;
    }

    public int getLine()
    {
      return token.Location.LineNumber;
    }

    public string getText()
    {
      return token.Text;
    }

    public void setColumn(int c)
    {      
    }

    public void setFilename(string name)
    {      
    }

    public void setLine(int l)
    {      
    }

    public void setText(string t)
    {      
    }
  }
}
