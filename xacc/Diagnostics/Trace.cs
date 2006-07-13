#region License
 /*	  xacc                																											*
 	*		Copyright (C) 2003-2006  Llewellyn@Pritchard.org                          *
 	*																																							*
	*		This program is free software; you can redistribute it and/or modify			*
	*		it under the terms of the GNU Lesser General Public License as            *
  *   published by the Free Software Foundation; either version 2.1, or					*
	*		(at your option) any later version.																				*
	*																																							*
	*		This program is distributed in the hope that it will be useful,						*
	*		but WITHOUT ANY WARRANTY; without even the implied warranty of						*
	*		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the							*
	*		GNU Lesser General Public License for more details.												*
	*																																							*
	*		You should have received a copy of the GNU Lesser General Public License	*
	*		along with this program; if not, write to the Free Software								*
	*		Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
#endregion

using System;
using System.Collections;
using System.Diagnostics;

namespace Xacc.Diagnostics
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
          string.Format("xacc.ide: {0}", typeof(Trace).Assembly.GetName().Version),
          string.Format(".NET:     {0}", Environment.Version),
          string.Format("OS:       {0}", Environment.OSVersion)
        }) + Environment.NewLine;
      }
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
          string msg = string.Format("{0,-15}:{1,-60}:{2}", category, 
            sf.GetMethod(), string.Format(format, args).Replace("\n", "\\n").Replace("\t","\\t"));
          TRACE[pos++%TRACELENGTH] = msg;
          System.Diagnostics.Trace.WriteLine(msg);
        }
      }
      else
      {
        string msg = category + " : " + string.Format(format, args);
        TRACE[pos++%TRACELENGTH] = msg;
        System.Diagnostics.Trace.WriteLine(msg);
      }

    }
	}
}
