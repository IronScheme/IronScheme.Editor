// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.IO;
using System.Text;


namespace gpcc
{
  public enum GrammarToken
  {
    Eof,
    Symbol,
    Literal,
    Action,
    Divider,
    Colon,
    SemiColon,
    EndOfSection,
    Union,
    Type,
    Token,
    Left,
    Right,
    NonAssoc,
    Prolog,
    Epilog,
    Kind,
    LeftCurly,
    RightCurly,
    Prec,
    Start,
    Namespace,
    Using,
    Visibility,
    ParserName,
    TokenName,
    ValueTypeName
  };


  class Scanner
  {
    public string yylval;

    private char next;
    private int pos;
    private string line;
    private int section;
    private string filename;
    private int linenr;
    private StreamReader reader;
    private StringBuilder builder;


    public Scanner(string path)
    {
      section = 1;
      filename = path;
      reader = new StreamReader(path);
      builder = new StringBuilder();
      pos = 0;
      linenr = 0;
      line = "";
      Advance();
    }


    public GrammarToken Next()
    {
      yylval = null;

      if (next == 0)
        return GrammarToken.Eof;

      if (section == 3)
      {
        builder.Length = 0;

        if (GPCG.LINES)
        {
          builder.AppendFormat("#line {0} \"{1}\"", linenr, filename);
          builder.AppendLine();
        }

        while (next != 0)
        {
          builder.Append(next);
          Advance();
        }
        yylval = builder.ToString();
        return GrammarToken.Epilog;
      }

      if (pos == 0 && line.StartsWith("%%"))
      {
        Advance();
        Advance();
        section++;
        return GrammarToken.EndOfSection;
      }

      switch (next)
      {
        case '/':					// Comments
          Advance();
          if (next == '/')		// C++ style comment
          {
            while (next != '\n')
            {
              Advance();
            }
            return Next();
          }
          else if (next == '*')	// C style comment
          {
            Advance();
            do
            {
              while (next != '*')
                Advance();
              Advance();
            }
            while (next != '/');
            Advance();

            return Next();
          }
          else
          {
            ReportError("unexpected / character, not in comment");
            return Next();
          }

        case '\'':		// Character literal
          Advance();
          bool backslash = (next == '\\');

          if (backslash)
            Advance();

          yylval = new string(Escape(backslash, next), 1);
          Advance();

          if (next != '\'')
            ReportError("Expected closing character quote");
          else
            Advance();
          return GrammarToken.Literal;

        case ' ':		// skip Whitespace
        case '\t':
        case '\n':
          Advance();
          return Next();

        case '%':		// %command of some kind
          Advance();
          if (next == '{')	// %{ Prolog %}
          {
            Advance();
            builder.Length = 0;

            if (GPCG.LINES)
            {
              builder.AppendFormat("#line {0} \"{1}\"", linenr, filename);
              builder.AppendLine();
            }

            do
            {
              while (next != '%')
              {
                builder.Append(next);
                Advance();
              }
              Advance();
            } while (next != '}');
            Advance();

            yylval = builder.ToString();
            return GrammarToken.Prolog;
          }
          else if (Char.IsLetter(next))
          {
            builder.Length = 0;
            while (Char.IsLetter(next))
            {
              builder.Append(next);
              Advance();
            }
            string keyword = builder.ToString();
            switch (keyword)
            {
              case "union":
                yylval = ScanUnion();
                return GrammarToken.Union;
              case "prec":
                return GrammarToken.Prec;
              case "token":
                return GrammarToken.Token;
              case "type":
                return GrammarToken.Type;
              case "nonassoc":
                return GrammarToken.NonAssoc;
              case "left":
                return GrammarToken.Left;
              case "right":
                return GrammarToken.Right;
              case "start":
                return GrammarToken.Start;
              case "namespace":
                return GrammarToken.Namespace;
              case "using":
                return GrammarToken.Using;
              case "visibility":
                return GrammarToken.Visibility;
              case "parsertype":
                return GrammarToken.ParserName;
              case "tokentype":
                return GrammarToken.TokenName;
              case "valuetype":
                return GrammarToken.ValueTypeName;
              default:
                ReportError("Unexpected keyword {0}", keyword);
                return Next();
            }
          }
          else
          {
            ReportError("Unexpected keyword {0}", next);
            return Next();
          }

        case '<':	// <id>
          {
            Advance();
            builder.Length = 0;
            while (next != '>' && next != 0)
            {
              builder.Append(next);
              Advance();
            }
            Advance();
            yylval = builder.ToString();
            return GrammarToken.Kind;
          }

        case '|':
          Advance();
          return GrammarToken.Divider;

        case ';':
          Advance();
          return GrammarToken.SemiColon;

        case ':':
          Advance();
          return GrammarToken.Colon;

        case '{':
          if (section == 1)
          {
            Advance();
            return GrammarToken.LeftCurly;
          }
          else // if (section == 2)
          {
            yylval = ScanCodeBlock();
            return GrammarToken.Action;
          }

        case '}':
          Advance();
          return GrammarToken.RightCurly;

        default:
          if (Char.IsLetter(next))
          {
            builder.Length = 0;
            while (Char.IsLetterOrDigit(next) || next == '_' || next == '.')
            {
              builder.Append(next);
              Advance();
            }
            yylval = builder.ToString();
            return GrammarToken.Symbol;
          }
          else
          {

            ReportError("Unexpected character '{0}'", next);
            Advance();
            return Next();
          }
      }
    }


    private void Advance()
    {
      if (pos + 1 < line.Length)
        next = line[++pos];
      else
      {
        if (reader.EndOfStream)
          next = (char)0;
        else
        {
          line = reader.ReadLine() + "\n";
          //Console.WriteLine("\"{0}\"", line);
          //line = line + "\n";
          linenr++;
          pos = 0;
          next = line[pos];
        }
      }
    }


    private string ScanCodeBlock()
    {
      builder.Length = 0;

      if (GPCG.LINES)
      {
        builder.AppendFormat("#line {0} \"{1}\"\n", linenr, filename);
        builder.Append("\t\t\t");
      }

      builder.Append(next);
      Advance();
      int nest = 1;

      while (true)
      {
        switch (next)
        {
          case '{':
            nest++;
            builder.Append(next);
            Advance();
            break;
          case '}':
            builder.Append(next);
            Advance();
            if (--nest == 0)
              return builder.ToString();
            break;
          case '/':
            builder.Append(next);
            Advance();
            if (next == '/')			// C++ style comment
            {
              while (next != 0 && next != '\n')
              {
                builder.Append(next);
                Advance();
              }
            }
            else if (next == '*')		// C style comment
            {
              builder.Append(next);
              Advance();
              do
              {
                while (next != 0 && next != '*')
                {
                  builder.Append(next);
                  Advance();
                }
                if (next != 0)
                {
                  builder.Append(next);
                  Advance();
                }
              } while (next != 0 && next != '/');
              if (next != 0)
              {
                builder.Append(next);
                Advance();
              }
            }
            else
            {
              builder.Append(next);
              Advance();
            }
            break;
          case '"':		// string literal
            builder.Append(next);
            Advance();
            while (next != 0 && next != '"')
            {
              if (next == '\\')
              {
                builder.Append(next);
                Advance();
              }
              if (next != 0)
              {
                builder.Append(next);
                Advance();
              }
            }

            if (next != 0)
            {
              builder.Append(next);
              Advance();
            }
            break;
          case '\'':		// character literal
            builder.Append(next);
            Advance();
            while (next != 0 && next != '\'')
            {
              if (next == '\\')
              {
                builder.Append(next);
                Advance();
              }
              if (next != 0)
              {
                builder.Append(next);
                Advance();
              }
            }

            if (next != 0)
            {
              builder.Append(next);
              Advance();
            }
            break;
          case '@':
            builder.Append(next);
            Advance();
            if (next == '"')	// verbatim string literal
            {
              builder.Append(next);
              Advance();
              while (next != 0 && next != '"')
              {
                builder.Append(next);
                Advance();
              }

              if (next != 0)
              {
                builder.Append(next);
                Advance();
              }
              break;
            }
            break;
          default:
            builder.Append(next);
            Advance();
            break;
        }
      }
    }


    private string ScanUnion()
    {
      while (next != '{')
        Advance();

      string union = ScanCodeBlock();

      union = union.Substring(0, union.Length - 1) + @"
#line default
internal int type;
internal TokenClass tclass;
Location loc;
internal string text;

public Location Location {get {return loc;} set {loc = value;} }
public int Type {get {return type;}}
public TokenClass Class {get {return tclass;}}
public string Text {get {return text;} set {text = value;}}
public int Length {get {return Text.Length;}}

public static implicit operator ValueType(Xacc.Languages.CSLex.Yytoken y)
{
  ValueType t = new ValueType();
  t.type = -1;
  t.tclass = y.Class;
  return t;
}

public static readonly ValueType EOF = new ValueType();
";

      return union + "}";
    }


    private char Escape(bool backslash, char ch)
    {
      if (!backslash)
        return ch;
      else
        switch (ch)
        {
          case 'a':
            return '\a';
          case 'b':
            return '\b';
          case 'f':
            return '\f';
          case 'n':
            return '\n';
          case 'r':
            return '\r';
          case 't':
            return '\t';
          case 'v':
            return '\v';
          case '0':
            return '\0';
          default:
            ReportError("Unexpected escape character '\\{0}'", ch);
            return ch;
        }
    }


    public class ParseException : Exception
    {
      public int line, column;

      public ParseException(int line, int column, string message)
        : base(message)
      {
        this.line = line;
        this.column = column;
      }
    }

    public void ReportError(string format, params object[] args)
    {
      throw new ParseException(linenr, pos, string.Format(format, args));
    }
  }
}








