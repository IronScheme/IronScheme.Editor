// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Text;


namespace gpcc
{
  public class SemanticAction
  {
    private Production production;
    private int pos;
    private string commands;


    public SemanticAction(Production production, int pos, string commands)
    {
      this.production = production;
      this.pos = pos;
      this.commands = commands;
    }


    public void GenerateCode(CodeGenerator codeGenerator)
    {
      int i = 0;

      while (i < commands.Length)
      {
        switch (commands[i])
        {
          case '/':
            Output(i++);
            if (commands[i] == '/') // C++ style comments
            {
              while (i < commands.Length && commands[i] != '\n')
                Output(i++);
              if (i < commands.Length)
                Output(i++);
            }
            else if (commands[i] == '*') // C style comments
            {
              Output(i++);
              do
              {
                while (i < commands.Length && commands[i] != '*')
                  Output(i++);
                if (i < commands.Length)
                  Output(i++);
              } while (i < commands.Length && commands[i] != '/');
              if (i < commands.Length)
                Output(i++);
            }
            break;

          case '"':       // start of string literal
            Output(i++);
            while (i < commands.Length && commands[i] != '"')
            {
              if (commands[i] == '\\')
                Output(i++);
              if (i < commands.Length)
                Output(i++);
            }
            if (i < commands.Length)
              Output(i++);
            break;

          case '@':		// start of verbatin string literal
            Output(i++);
            if (i < commands.Length && commands[i] == '"')
            {
              Output(i++);
              while (i < commands.Length && commands[i] != '"')
                Output(i++);
              if (i < commands.Length)
                Output(i++);
              break;
            }
            else
            {
              if (commands[i] == '@')
              {
                i++;
                Console.Write("yyval.Location");
              }
              else if (char.IsDigit(commands[i]))
              {
                int num = commands[i] - '0';
                i++;
                if (char.IsDigit(commands[i]))
                {
                  num = num * 10 + commands[i] - '0';
                  i++;
                }
                Console.Write("value_stack.array[value_stack.top-{0}].Location", pos - num + 1);
              }
              else
                Console.Error.WriteLine("Unexpected '@'");
            }
            break;

          case '\'':      // start of char literal
            Output(i++);
            while (i < commands.Length && commands[i] != '\'')
            {
              if (commands[i] == '\\')
                Output(i++);
              if (i < commands.Length)
                Output(i++);
            }
            if (i < commands.Length)
              Output(i++);
            break;

          //case '#':       // #n placeholder
          //  i++;
          //  if (char.IsDigit(commands[i]))
          //  {
          //    int num = commands[i] - '0';
          //    i++;
          //    if (char.IsDigit(commands[i]))
          //    {
          //      num = num * 10 + commands[i] - '0';
          //      i++;
          //    }
          //    Console.Write("value_stack.array[value_stack.top-{0}]", pos - num + 1);
          //  }
          //  else
          //  {
          //    i--;
          //    goto default;
          //  }
          //  break;

          case '$':       // $$ or $n placeholder
            i++;
            string kind = null;
            if (commands[i] == '<') // $<kind>n
            {
              i++;
              StringBuilder builder = new StringBuilder();
              while (i < commands.Length && commands[i] != '>')
              {
                builder.Append(commands[i]);
                i++;
              }
              if (i < commands.Length)
              {
                i++;
                kind = builder.ToString();
              }
            }

            if (commands[i] == '$')
            {
              i++;
              if (kind == null)
                kind = production.lhs.kind;

              Console.Write("yyval");

              if (kind != null)
                Console.Write(".{0}", kind);
            }
            else if (char.IsDigit(commands[i]))
            {
              int num = commands[i] - '0';
              i++;
              if (char.IsDigit(commands[i]))
              {
                num = num * 10 + commands[i] - '0';
                i++;
              }
              if (kind == null)
                kind = production.rhs[num - 1].kind;

              Console.Write("value_stack.array[value_stack.top-{0}]", pos - num + 1);

              if (kind != null)
                Console.Write(".{0}", kind);
            }
            else
              Console.Error.WriteLine("Unexpected '$'");
            break;

          default:
            Output(i++);
            break;
        }
      }
      Console.WriteLine();
    }


    private void Output(int i)
    {
      if (commands[i] == '\n')
        Console.WriteLine();
      else
        Console.Write(commands[i]);
    }
  }
}