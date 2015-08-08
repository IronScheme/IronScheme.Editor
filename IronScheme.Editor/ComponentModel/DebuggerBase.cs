#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Collections;
using System.Text.RegularExpressions;
using IronScheme.Editor.Collections;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Defines breakpoints for debugger
  /// </summary>
  [Serializable]
  public class Breakpoint
  {
    [NonSerialized]
    internal int id = -1;
    [NonSerialized]
    internal bool bound = true;
    [NonSerialized]
    internal EventHandler boundchanged;

    internal bool enabled = true;

    internal void SetEnabled(bool v)
    {
      if (enabled != v)
      {
        enabled = v;
        if (boundchanged != null)
        {
          boundchanged(this, EventArgs.Empty);
        }
      }
    }

    internal void SetBound(bool v)
    {
      if (bound != v)
      {
        bound = v;
        if (boundchanged != null)
        {
          boundchanged(this, EventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// line of breakpoint
    /// </summary>
    public int linenr = -1;

    /// <summary>
    /// filename of breakpoint
    /// </summary>
    public string filename = null;

    /// <summary>
    /// Gets the string representation of the breakpoint
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
      return string.Format("{0}:{1}", filename, linenr);
    }

  }

  /// <summary>
  /// Defines exception info for the debugger
  /// </summary>
  public class ExceptionInfo
  {
    /// <summary>
    /// The message
    /// </summary>
    public string message;

    /// <summary>
    /// The type
    /// </summary>
    public string type;
  }


  abstract class DebuggerBase : Disposable
  {
    public event EventHandler DebugProcessExited;
    protected ExceptionInfo exinfo;
    protected ArrayList frames = new ArrayList();
    internal ExceptionInfo lastexinfo;

    protected void OnDebugProcessExited()
    {
      if (DebugProcessExited != null)
      {
        DebugProcessExited(this, EventArgs.Empty);
      }
    }

    public StackFrame[] CallStack
    {
      get { return frames.ToArray(typeof(StackFrame)) as StackFrame[]; }
    }

    public int FrameCount
    {
      get { return frames.Count; }
    }

    protected StackFrame currentframe;

    public StackFrame CurrentFrame
    {
      get { return currentframe; }
    }

    public StackFrame TopFrame
    {
      get { return frames.Count == 0 ? null : (StackFrame)frames[0]; }
    }

    protected Set locals = new Set();
    protected Set othervars = new Set();
    protected Hashtable vars = new Hashtable();

    public string this[string var]
    {
      get
      {
        object r = vars[var];
        if (r == null)
        {
          return "<null>";
        }
        return r.ToString();
      }
    }

    public string[] Autos
    {
      get { return currentframe.Autos; }
    }

    public string[] Locals
    {
      get { return locals.ToArray(typeof(string)) as string[]; }
    }

    public string[] OtherVariables
    {
      get { return othervars.ToArray(typeof(string)) as string[]; }
    }

    #region Helper classes



    public class StackFrame
    {
      bool current;
      int id;
      int linenr;
      string filename;
      string module;
      string type;
      string method;
      int iloffset = 0;

      Set autos, autosprev;

      internal void SetAutos(Set s)
      {
        autosprev = autos;
        autos = s;

        if (autosprev != null && autosprev != autos)
        {
          foreach (object o in autos)
          {
            if (autosprev.Contains(o))
            {
              autosprev.Remove(o);
            }
          }
        }
      }

      public string[] Autos
      {
        get
        {
          if (autosprev == null)
          {
            return autos.ToArray(typeof(string)) as string[];
          }
          else
          {
            return (autos | autosprev).ToArray(typeof(string)) as string[];
          }
        }
      }

      internal void RemoveAuto(string name)
      {
        if (autosprev != null)
        {
          autosprev.Remove(name);
        }
        autos.Remove(name);
      }

      public int Id
      {
        get { return id; }
      }

      public bool IsCurrentFrame
      {
        get { return current; }
      }

      public int LineNumber
      {
        get { return linenr - 1; }
      }

      public int ILOffset
      {
        get { return iloffset; }
      }

      public string Filename
      {
        get { return filename; }
      }

      public string Method
      {
        get { return method; }
      }

      public string Module
      {
        get { return module; }
      }

      public string Type
      {
        get { return type; }
      }
      //@"0)* foo.Blah::Main +0065[native] +0007[IL] in d:\dev\XACC\xacc-ide\bin\Debug\test.cs:50"

      static Regex stackframe = new Regex(@"
^(?<id>\d+)\)(?<current>\*)?\s+
((?<module>\w+)!)?(?<type>[\w\.]+)::
(?<method>[\w\.]+)(\s(?<iloffset>(\+|\-)[\da-fA-F]{4}(\[(IL|native)\])?))+
\s
((\[(?<nosrc>[^\]]+)\])
|(in\s(?<filename>.*?):(?<linenr>\d+)))$"
        , RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

      StackFrame() { }

      static readonly Hashtable frames = new Hashtable();

      public static StackFrame FromDebugger(string line)
      {
        Match m = stackframe.Match(line);
        if (m.Success)
        {
          bool current = m.Groups["current"].Success;
          string module = m.Groups["module"].Value;
          string method = m.Groups["method"].Value;
          string type = m.Groups["type"].Value;
          string filename = string.Empty;
          string linenr = "0";

          if (m.Groups["nosrc"].Success)
          {
            filename = m.Groups["nosrc"].Value;
          }
          else
          {
            filename = m.Groups["filename"].Value;
            linenr = m.Groups["linenr"].Value;
          }

          string iloffset = m.Groups["iloffset"].Value;
          string id = m.Groups["id"].Value;

          if (id.Length == 0)
          {

          }

          string key = module + type + method;

          StackFrame sf = frames[key] as StackFrame;

          if (sf == null)
          {
            sf = new StackFrame();
            frames[key] = sf;
          }


          sf.filename = filename;
          sf.linenr = Convert.ToInt32(linenr);
          sf.id = Convert.ToInt32(id);
          //sf.iloffset = Convert.ToInt32(iloffset, 16);
          sf.module = module;
          sf.method = method;
          sf.type = type;
          sf.current = current;

          return sf;
        }
        return null;
      }
    }

    #endregion  

  
    public abstract void Break(Breakpoint bp);
    public abstract void RemoveBreakpoint(Breakpoint bp);
    public abstract void Next();
    public abstract void In();
    public abstract void Out();
    public abstract void Where();
    public abstract void SetDefaultModes();
    public abstract void Continue();

    public abstract void Print();
    public abstract void Print(string[] args);

    public abstract void Run(string filename, params string[] args);
  }
}
