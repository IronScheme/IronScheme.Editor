using System;
using Xacc.Languages;
using System.Collections;
using System.Text;
using Xacc.ComponentModel;
using Xacc.CodeModel;
using System.Drawing;

namespace Xacc.Languages.CSLex
{
  [CLSCompliant(false)]
  public struct Yytoken : IToken
  {
    readonly TokenClass tokenclass;
    Location location;
    string yytext;

    public static readonly Yytoken EOF = new Yytoken(TokenClass.EOF);

    public Yytoken(TokenClass tokenclass) 
    {
      this.tokenclass = tokenclass;
      yytext = string.Empty;
      location = null;
    }

    public static implicit operator Yytoken(Color color)
    {
      TokenClass tokenclass = (TokenClass)((int)TokenClass.Custom | (0xffffff & color.ToArgb()));
      return new Yytoken(tokenclass);
    }

    public static implicit operator Yytoken(TokenClass tokenclass)
    {
      return new Yytoken(tokenclass);
    }

    public int Length
    {
      get { return yytext.Length; }
    }

    public string Text
    {
      get {return yytext;}
      set {yytext = value;}
    }

    public TokenClass Class
    {
      get { return tokenclass; }
    }

    public Location Location
    {
      get { return location; }
      set { location = value; }
    }

    public int Type
    {
      get { return -1;}
    }
  }



	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[CLSCompliant(false)]
	public abstract class Language<T> : Languages.Language where T: struct , IToken
	{
    protected abstract LexerBase Lexer {get;}
    //protected abstract Parser Parser {get;}

    readonly ArrayList tokens = new ArrayList();
    readonly Hashtable definedmap = new Hashtable();

    protected sealed override IToken[] Lex(string input, ref Stack state)
    {
      if (input.Length > 4092) 
      {
        input = input.Substring(0, 4092);
      }
      
      tokens.Clear();
      Lexer.Start(input);

      if (state != null)
      {
        Lexer.Stack = state.Clone() as Stack;
        Lexer.BEGIN((int)Lexer.Stack.Peek());
      }
      else
      {
        Lexer.Stack = new Stack();
        Lexer.ENTER(0);
      }

      IToken t;

      while ( (t = Lexer.yywrap()) != null )
      {
        tokens.Add(t);
      }
      
      if (Lexer.Stack.Count > 0)
      {
        Lexer.Stack.Pop();
      }

      Lexer.Stack.Push(Lexer.State);

      state = Lexer.Stack;

      return (IToken[])tokens.ToArray(typeof(IToken));
    }

    public virtual bool Parse()
    {
      return true;
    }

    protected override int yyparse(IEnumerator lines)
    {
      Lexer.lines = lines;
      return Parse() ? 0 : 1;
    }

    public abstract class LexerBase : gppg.IScanner<T>
    {
      Stack stack = new Stack();

      public Stack Stack
      {
        get { return stack; }
        set { stack = value; }
      }

      public void BEGIN(int newstate)
      {
        yybegin((int)newstate);
      }

      public void ENTER(int newstate)
      {
        stack.Push(newstate);
        BEGIN(newstate);
      }
      
      public void EXIT()
      {
        int c = stack.Count;
        if (c > 0)
        {
          stack.Pop();
          if (c > 1)
          {
            int i = (int)stack.Peek();
            BEGIN(i);
          }
        }
      }

      public IToken yywrap()
      {
        IToken t = lex();
        if (t == null || t.Equals(Yytoken.EOF))
        {
          return null;
        }
        t.Text = yytext();
        if (t.Length == 0)
        {
          return null;
        }
        t.Location = new Location(0, yychar, 0, yychar + yylength());

        return t;
      }

      internal IEnumerator lines;

      public override int yylex()
      {
        while (lines.MoveNext())
        {
          T t = (T)lines.Current;
          if ((int)t.Class >= -1)
          {
            yylval = t;
            System.Diagnostics.Debug.Assert(t.Type > 2);
            return t.Type;
          }
        }
        return eofToken;
      }

      protected static readonly Yytoken PREPROC = new Yytoken(TokenClass.Preprocessor);
      protected static readonly Yytoken WARNING = new Yytoken(TokenClass.Warning);
      protected static readonly Yytoken IGNORE = new Yytoken(TokenClass.Ignore);
      protected static readonly Yytoken DOCCOMMENT = new Yytoken(TokenClass.DocComment);
      protected static readonly Yytoken ERROR = new Yytoken(TokenClass.Error);
      protected static readonly Yytoken NEWLINE = new Yytoken(TokenClass.NewLine);
      protected static readonly Yytoken COMMENT = new Yytoken(TokenClass.Comment);
      protected static readonly Yytoken PLAIN = new Yytoken(TokenClass.Any);
      protected static readonly Yytoken IDENTIFIER = new Yytoken(TokenClass.Identifier);
      protected static readonly Yytoken TYPE = new Yytoken(TokenClass.Type);
      protected static readonly Yytoken KEYWORD = new Yytoken(TokenClass.Keyword);
      protected static readonly Yytoken PAIR = new Yytoken(TokenClass.Pair);
      protected static readonly Yytoken OPERATOR = new Yytoken(TokenClass.Operator);
      protected static readonly Yytoken NUMBER = new Yytoken(TokenClass.Number);
      protected static readonly Yytoken STRING = new Yytoken(TokenClass.String);
      protected static readonly Yytoken CHARACTER = new Yytoken(TokenClass.Character);
      protected static readonly Yytoken OTHER = new Yytoken(TokenClass.Other);

      protected int yychar;
      protected int yyline;

      public int State
      {
        get { return yy_lexical_state; }
      }

      public abstract IToken lex();

      public void Start(string input)
      {
        Reset();
        yy_reader = new System.IO.StringReader(input + "\n");
      }

      protected const int YY_BUFFER_SIZE = 512;
      protected const int YY_F = -1;
      protected const int YY_NO_STATE = -1;
      protected const int YY_NOT_ACCEPT = 0;
      protected const int YY_START = 1;
      protected const int YY_END = 2;
      protected const int YY_NO_ANCHOR = 4;

      // dont make these static like as in Flex, using inheritence
      protected int YY_BOL;
      protected int YY_EOF;

      protected System.IO.TextReader yy_reader;
      protected int yy_buffer_index;
      protected int yy_buffer_read;
      protected int yy_buffer_start;
      protected int yy_buffer_end;
      protected char[] yy_buffer = new char[YY_BUFFER_SIZE];
      protected bool yy_at_bol;
      protected int yy_lexical_state;

      public int eofToken;

      protected LexerBase()
      {
        Reset();
      }

      void Reset()
      {
        Array.Clear(yy_buffer, 0, YY_BUFFER_SIZE);
        yy_buffer_read = 0;
        yy_buffer_index = 0;
        yy_buffer_start = 0;
        yy_buffer_end = 0;
        yychar = 0;
        yyline = 0;
        yy_at_bol = true;
        yy_lexical_state = 0;
      }

      protected bool yy_eof_done = false;

      public void yybegin(int state)
      {
        yy_lexical_state = state;
      }

      protected int yy_advance()
      {
        int next_read;
        int i;
        int j;

        if (yy_buffer_index < yy_buffer_read)
        {
          return yy_buffer[yy_buffer_index++];
        }

        if (0 != yy_buffer_start)
        {
          i = yy_buffer_start;
          j = 0;
          while (i < yy_buffer_read)
          {
            yy_buffer[j] = yy_buffer[i];
            ++i;
            ++j;
          }
          yy_buffer_end = yy_buffer_end - yy_buffer_start;
          yy_buffer_start = 0;
          yy_buffer_read = j;
          yy_buffer_index = j;
          next_read = yy_reader.Read(yy_buffer,
            yy_buffer_read,
            yy_buffer.Length - yy_buffer_read);
          if (0 == next_read)
          {
            return YY_EOF;
          }
          yy_buffer_read = yy_buffer_read + next_read;
        }

        while (yy_buffer_index >= yy_buffer_read)
        {
          if (yy_buffer_index >= yy_buffer.Length)
          {
            yy_buffer = yy_double(yy_buffer);
          }
          next_read = yy_reader.Read(yy_buffer,
            yy_buffer_read,
            yy_buffer.Length - yy_buffer_read);
          if (0 == next_read)
          {
            return YY_EOF;
          }
          yy_buffer_read = yy_buffer_read + next_read;
        }
        return yy_buffer[yy_buffer_index++];
      }

      protected void yy_move_end()
      {
        if (yy_buffer_end > yy_buffer_start &&
          '\n' == yy_buffer[yy_buffer_end - 1])
          yy_buffer_end--;
        if (yy_buffer_end > yy_buffer_start &&
          '\r' == yy_buffer[yy_buffer_end - 1])
          yy_buffer_end--;
      }

      protected bool yy_last_was_cr = false;

      protected void yy_mark_start()
      {
        yychar = yychar
          + yy_buffer_index - yy_buffer_start;
        yy_buffer_start = yy_buffer_index;
      }

      protected void yy_mark_end()
      {
        yy_buffer_end = yy_buffer_index;
      }

      protected void yy_to_mark()
      {
        yy_buffer_index = yy_buffer_end;
        yy_at_bol = (yy_buffer_end > yy_buffer_start) &&
          ('\r' == yy_buffer[yy_buffer_end - 1] ||
          '\n' == yy_buffer[yy_buffer_end - 1] ||
          2028/*LS*/ == yy_buffer[yy_buffer_end - 1] ||
          2029/*PS*/ == yy_buffer[yy_buffer_end - 1]);
      }

      protected char YYCHAR
      {
        get { return yy_buffer[yy_buffer_start]; }
      }

      protected string yytext()
      {
        return (new string(yy_buffer,
          yy_buffer_start,
          yy_buffer_end - yy_buffer_start));
      }

      protected int yylength()
      {
        return yy_buffer_end - yy_buffer_start;
      }

      protected char[] yy_double(char[] buf)
      {
        int i;
        char[] newbuf;
        newbuf = new char[2 * buf.Length];
        for (i = 0 ; i < buf.Length ; ++i)
        {
          newbuf[i] = buf[i];
        }
        return newbuf;
      }

      protected const int YY_E_INTERNAL = 0;
      protected const int YY_E_MATCH = 1;
      protected string[] yy_error_string = {
                                           "Error: Internal error.\n",
                                           "Error: Unmatched input.\n"
                                         };

      protected void yy_error(int code, bool fatal)
      {
        System.Console.Write(yy_error_string[code]);
        System.Console.Out.Flush();
        if (fatal)
        {
          throw new System.Exception("Fatal Error.\n");
        }
      }

      protected static int[][] unpackFromString(int size1, int size2, string st)
      {
        int colonIndex = -1;
        string lengthString;
        int sequenceLength = 0;
        int sequenceInteger = 0;

        int commaIndex;
        string workString;

        int[][] res = new int[size1][];

        for (int i = 0 ; i < size1 ; i++)
        {
          res[i] = new int[size2];

          for (int j = 0 ; j < size2 ; j++)
          {
            if (sequenceLength != 0)
            {
              res[i][j] = sequenceInteger;
              sequenceLength--;
              continue;
            }
            commaIndex = st.IndexOf(',');
            workString = (commaIndex == -1) ? st :
              st.Substring(0, commaIndex);
            st = st.Substring(commaIndex + 1);
            colonIndex = workString.IndexOf(':');
            if (colonIndex == -1)
            {
              res[i][j] = System.Int32.Parse(workString);
              continue;
            }
            lengthString =
              workString.Substring(colonIndex + 1);
            sequenceLength = System.Int32.Parse(lengthString);
            workString = workString.Substring(0, colonIndex);
            sequenceInteger = System.Int32.Parse(workString);
            res[i][j] = sequenceInteger;
            sequenceLength--;
          }
        }
        return res;
      }

      public override void yyerror(string format, params object[] args)
      {
        yylval.Location.Error = true;
        ServiceHost.Error.OutputErrors(this, new Xacc.Build.ActionResult(string.Format(format, args), yylval.Location));
      }
    }


	}
}
