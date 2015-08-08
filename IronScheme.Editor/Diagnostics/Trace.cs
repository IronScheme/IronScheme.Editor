#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace IronScheme.Editor.Diagnostics
{
	class Trace
	{
    const int TRACELENGTH = 1024;
    static readonly string[] TRACE = new string[TRACELENGTH];
    static int pos = 0;
    public static bool debugmode = false;

    public static string SystemInfo
    {
      get 
      {
        return string.Join(Environment.NewLine, new string[] 
        {
          string.Format(".NET:     {0}", Environment.Version),
          string.Format("OS:       {0}", Environment.OSVersion),
          string.Format("Modules:  {0}", GetModules())
        }) + Environment.NewLine;
      }
    }

    static string GetModules()
    {
      List<string> mods = new List<string>();
      foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
      {
        AssemblyName assname = ass.GetName();
        mods.Add(string.Format("{0}({1}){2}", assname.Name, assname.Version, ass.GlobalAssemblyCache ? "*" : ""));
      }
      return string.Join(",", mods.ToArray());
    }

    public static string GetFullTrace()
    {
      pos %= TRACELENGTH;
      ArrayList alllines = new ArrayList();

      for (int i = pos; i < TRACELENGTH; i++)
      {
        if (TRACE[i] != null)
        {
          alllines.Add(TRACE[i]);
        }
      }

      for (int i = 0; i < pos; i++)
      {
        if (TRACE[i] != null)
        {
          alllines.Add(TRACE[i]);
        }
      }

      string ts = string.Join(Environment.NewLine, alllines.ToArray(typeof(string)) as string[]);
      return ts;
    }

    public static string GetTrace()
    {
      string sysinfo = SystemInfo;
      string ts = GetFullTrace();
      if (ts.Length > 1000 - sysinfo.Length)
      {
        ts = ts.Substring(ts.Length - (1000 - sysinfo.Length));
      }
      return sysinfo + Environment.NewLine + ts;
    }

    [Conditional("TRACE")]
    public static void WriteLine(string category, string format, params object[] args)
    {
      if (debugmode)
      {
        lock(TRACE)
        {
          // get the caller
          StackTrace st = new StackTrace(false);
          StackFrame sf = st.GetFrame(2);

          try
          {
            string msg = string.Format("{0,-15}:{1,-60}:{2}", category,
              sf.GetMethod(), string.Format(format, args).Replace("\n", "\\n").Replace("\t", "\\t"));
            TRACE[pos++ % TRACELENGTH] = msg;
            System.Diagnostics.Trace.WriteLine(msg);
          }
          catch
          {
            // get the caller
            string msg = string.Format("{0,-15}:{1,-60}:{2}", category,
              sf.GetMethod(), string.Format(format.Replace("{", "{{").Replace("}", "}}"), args).Replace("\n", "\\n").Replace("\t", "\\t"));
            TRACE[pos++ % TRACELENGTH] = msg;
            System.Diagnostics.Trace.WriteLine(msg);
          }
        }
      }
      else
      {
        string msg = category + " : " + string.Format(format, args);
        TRACE[pos++%TRACELENGTH] = msg;
        System.Diagnostics.Trace.WriteLine(msg);
      }

      //if (IronScheme.Editor.ComponentModel.ServiceHost.Initialized)
      //{
      //  IronScheme.Editor.ComponentModel.ServiceHost.StatusBar.StatusText = string.Format(format, args);
      //}

    }
	}
}
