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
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using IronScheme.Editor.Collections;

namespace IronScheme.Editor.ComponentModel
{


  class Cordbg : DebuggerBase
  {
    Process process;
    StreamWriter input;
    StreamReader output;
    Thread reader;

    protected override void Dispose(bool disposing)
    {
      Exit();
      reader.Abort();
      process.Dispose();
      base.Dispose (disposing);
    }

    public static bool DebuggerIsAvailable
    {
      get {return ServiceHost.Discovery.NetRuntimeSDK != null;}
    }

    ManualResetEvent dbgready = new ManualResetEvent(false);

		public Cordbg()
		{
			process = new Process();

      string cmd = ServiceHost.Discovery.NetRuntimeSDK;
      if (cmd == null)
      {
        return;
      }

			ProcessStartInfo psi = new ProcessStartInfo(cmd + @"\Bin\cordbg.exe");
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;

			process.StartInfo = psi;
			process.EnableRaisingEvents = true;

      reader = new Thread( new ThreadStart(ReaderLoop));
      reader.Name = "Cordbg reader";
			reader.Start();

			process.Start();

			input = process.StandardInput;
			output = process.StandardOutput;

      dbgready.WaitOne();
		}

    static string Join(ICollection col)
    {
      string output = string.Empty;

      if (col.Count > 0)
      {
        ArrayList l = new ArrayList(col);
        for (int i = 0; i < l.Count - 1; i++)
        {
          output += (l[i] == null ? string.Empty : (l[i].ToString() + " "));
        }

        output += ((l[l.Count - 1] == null) ? string.Empty : l[l.Count - 1].ToString());
      }

      return output;
    }




    volatile bool hasstarted = false;
    
    public bool IsReady
    {
      get {return hasstarted;}
    }



    #region CorDbg interaction

    static Hashtable illegalnames = new Hashtable();

    static Cordbg()
    {
      illegalnames.Add("new", true);
    }

    bool IsAllowed(string varname)
    {
      if (illegalnames.ContainsKey(varname))
      {
        return false;
      }
      return true;
    }

    volatile bool dontadd = false;

    // FUN FUN FUN :p
    static Regex sourceparse = new Regex(@"^\d+:(?<source>.*)$", RegexOptions.Compiled);
    static Regex autoparse   = new Regex(@"(""[^""]*"")|([_a-zA-Z][\w\.]*\s*\()|(^\s*[_a-zA-Z][\w\.]*\s+(?<var>[_a-zA-Z]\w*))|(?<var>[_a-zA-Z][\w\.]*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    static Regex threadstate = new Regex(@"^Thread\s(?<tid>0x[\da-fA-F]+)\sCurrent\sState:(?<state>.+)$", RegexOptions.Compiled);
    static Regex threadmsg   = new Regex(@"^\[thread\s(?<tid>0x[\da-fA-F]+)\](?<msg>.*)$", RegexOptions.Compiled);
    static Regex proccreated = new Regex(@"^Process\s\d+/0x[\da-fA-F]+\screated\.$", RegexOptions.Compiled);
    static Regex varparse    = new Regex(@"^(?<var>[\w\$\.]+)=(\(0x[\da-fA-F]+\)\s)?(?<type>.+)$", RegexOptions.Compiled);
    static Regex bpparse     = new Regex(@"^#(?<id>\d+)\s+(?<file>.+)\s+(?<meth>[^\s]+)\s+((?<active>\[active\])|(?<unbound>\[unbound\]))$", RegexOptions.Compiled);
    static Regex bpparse2    = new Regex(@"^#(?<id>\d+)\s+(?<file>.+)\s+(?<unbound>\[unbound\])$", RegexOptions.Compiled);
    static Regex bpbound     = new Regex(@"^Breakpoint #(?<id>\d+) has bound to .+$", RegexOptions.Compiled);
    static Regex bpbreak     = new Regex(@"^break at #(?<id>\d+)	.+$", RegexOptions.Compiled);
    static Regex modeparse   = new Regex(@"^ (?<mode>\w+)=(?<v>(0|1))", RegexOptions.Compiled);
    static Regex exparse     = new Regex(@"Unhandled exception generated: \((?<ex>0x\w{8})\) <(?<type>[^>]+)>$", RegexOptions.Compiled);

    string lastvar = string.Empty;

    Set autos = new Set();

    //bool getmodes = false;
    volatile bool ignoreline = false;

    void ParseLine(string line)
    {
      Match m = null;

      if (ignoreline)
      {
        ignoreline = false;
        return;
      }

//      if (getmodes)
//      {
//        m = modeparse.Match(line);
//        if (m.Success)
//        {
//          bool on = m.Groups["v"].Value == "1";
//          string mode = m.Groups["mode"].Value;
//
//          Mode mm = (Mode) Enum.Parse(typeof(Mode), mode);
//          if (on)
//          {
//            systemmode |= mm;
//          }
//          else
//          {
//            systemmode &= ~mm;
//          }
//        }
//        return;
//      }

      if (line.StartsWith("Warning:"))
      {
        Trace.WriteLine("! " + line);
        return;
      }

      if (line == "Variable unavailable, or not valid" || line == "Error: Variable not found")
      {
        vars.Remove(lastvar);
        currentframe.RemoveAuto(lastvar);
        return;
      }

      if (line == "Failed to find method to match source line. Unable to set breakpoint.")
      {
        bp.SetBound(false);
        return;
      }

      if (line == "Process exited.")
      {
        OnDebugProcessExited();
        return;
      }

      if (line == "No local variables in scope.")
      {
        return; 
      }

      if (line == "Process not running.")
      {
        return;
      }

      m = threadstate.Match(line);
      if (m.Success)
      {
        return;
      }

      m = threadmsg.Match(line);
      if (m.Success)
      {
        return;
      }

      m = sourceparse.Match(line);
      if (m.Success)
      {
        othervars.Clear();

        autos = new Set();
        locals.Clear();
        frames.Clear();

        string src = m.Groups["source"].Value;

        foreach (Match mm in autoparse.Matches(src))
        {
          if (mm.Groups["var"].Success)
          {
            string var = mm.Groups["var"].Value;
            if (IsAllowed(var))
            {
              autos.Add(var);
            }
          }
        }
        return;
      }

      StackFrame sf = StackFrame.FromDebugger(line);
      if (sf != null)
      { 
        if (sf.IsCurrentFrame)
        {
          currentframe = sf;
          currentframe.SetAutos(autos);
        }
        frames.Add(sf);
        return;
      }

      m = proccreated.Match(line);
      if (m.Success)
      {
        return;
      }

      m = varparse.Match(line);
      if (m.Success)
      {
        string var = m.Groups["var"].Value;
        string type = m.Groups["type"].Value;

        if (exinfo == null)
        {

          if (var.StartsWith("$"))
          {
            othervars.Add(var);
          }
          else
          {
            if (!dontadd)
            {
              locals.Add(var);
            }
            else
            {
              autos.Add(var);
            }
          }
          Trace.WriteLine(string.Format("n: {0} v: {1}", var, type));
          vars[var] = type;
        }
        else
        {
          switch (var)
          {
            case "_message": exinfo.message = type; break;
          }
        }
        return;
      }

      m = bpparse.Match(line);
      if (m.Success)
      {
        bool active = m.Groups["active"].Success;
        bool unbound = m.Groups["unbound"].Success;

        string bpid = m.Groups["id"].Value;
        string file = m.Groups["file"].Value;
        string meth = m.Groups["meth"].Value;

        if (bp == null)
        {
          bp = new Breakpoint();
        }
        bp.id = Convert.ToInt32(bpid);
        bp.SetBound(active && !unbound);

        return;
      }

      m = bpparse2.Match(line);
      if (m.Success)
      {
        bool unbound = m.Groups["unbound"].Success;

        string bpid = m.Groups["id"].Value;
        string file = m.Groups["file"].Value;

        if (bp == null)
        {
          bp = new Breakpoint();
        }

        bp.id = Convert.ToInt32(bpid);
        bp.SetBound(!unbound);
        return;
      }

      m = bpbound.Match(line);

      if (m.Success)
      {
        int bpid = Convert.ToInt32( m.Groups["id"].Value);

        Breakpoint bp = bptracker[bpid] as Breakpoint;
        if (bp != null)
        {
          bp.SetBound(true);
        }
        return;
      }

      m = bpbreak.Match(line);

      if (m.Success)
      {
        int bpid = Convert.ToInt32( m.Groups["id"].Value);

        Breakpoint bp = bptracker[bpid] as Breakpoint;
        return;
      }

      m = exparse.Match(line);

      if (m.Success)
      {

        int addr = Convert.ToInt32( m.Groups["ex"].Value, 16);
        string extype = m.Groups["type"].Value;
        Trace.WriteLine(addr, extype);
        exinfo = new ExceptionInfo();
        exinfo.type = extype;
        return;
      }

      Trace.WriteLine(string.Format("# Line not parsed: {0}", line));
    }

    Breakpoint bp;
    

    readonly Hashtable bptracker = new Hashtable();
    
    ManualResetEvent mrev = new ManualResetEvent(false);

    void SendCommand(string format, params object[] args)
    {
      mrev.Reset();
      string cmd = string.Format(format, args);
      Trace.WriteLine("> " + cmd);
      input.WriteLine(cmd);

      if (cmd == "ex")
      {
        mrev.WaitOne(100, true);
        return;
      }

      mrev.WaitOne();
      bufferresetcnt = 0;
      if (exinfo != null)
      {
        lastexinfo = exinfo;
        exinfo = null;
      }
    }

    int bufferresetcnt = 0;

		void ReaderLoop()
		{
      int bindex = 0;
      char[] buffer = new char[512];

			while (true)
			{
				if (output != null)
				{
          if (bufferresetcnt >= 1024)
          {
            input.WriteLine("hack");
            bufferresetcnt = 0;
          }
          
					int c = output.Read();
          if (c < 0)
          {
            return; // exit <<EOF>>
          }

          if (bindex == 0 && c == ' ')
          {
            continue;
          }

          bufferresetcnt++;
          buffer[bindex++] = (char) c;

          if (c == '\n')
          {
            if (hasstarted)
            {
              string line = new string(buffer,0, bindex).Trim();
              if (line.Length > 0)
              {
                ParseLine(line);
              }
            }
            Array.Clear(buffer, 0, bindex);
            bindex = 0;
            continue;
          }

          if (bindex == 8)
          {
            if (new string(buffer,bindex - 8, 8) == "(cordbg)")
            {
              if (hasstarted)
              {
                mrev.Set();
              }
              else
              {
                hasstarted = true;
                dbgready.Set();
              }
              Array.Clear(buffer, 0, bindex);
              bindex = 0;
              continue;
            }
          }
				}

				Thread.Sleep(0);
			}
		}

    #endregion
	    
    #region Modes

    [Flags]
    public enum Mode
    {
      AppDomainLoads = 0x0001,//          Display appdomain and assembly load events
      ClassLoads = 0x0002,//              Display class load events
      DumpMemoryInBytes = 0x0004,//       Display memory contents as bytes or DWORDs
      EnhancedDiag = 0x0008,//            Display enhanced diagnostic information
      HexDisplay = 0x0010,//              Display numbers in hexadecimal or decimal
      ILNatPrint = 0x0020,//              Display offsets in IL or native-relative, or both
      ISAll = 0x0040,//                   Step through all interceptors
      ISClinit = 0x0080,//                Step through class initializers
      ISExceptF = 0x0100,//               Step through exception filters
      ISInt = 0x0200,//                   Step through user interceptors
      ISPolicy = 0x0400,//                Step through context policies
      ISSec = 0x0800,//                   Step through security interceptors
      JitOptimizations = 0x1000,//        JIT compilation generates debuggable code
      LoggingMessages = 0x2000,//         Display managed code log messages
      ModuleLoads = 0x4000,//             Display module load events
      SeparateConsole = 0x8000,//         Specify if debuggees get their own console
      ShowArgs = 0x00010000,//                Display method arguments in stack trace
      ShowModules = 0x00020000,//             Display module names in the stack trace
      ShowStaticsOnPrint = 0x00040000,//      Display static fields for objects
      ShowSuperClassOnPrint = 0x00080000,//   Display contents of super class for objects
      UnmanagedTrace = 0x00100000,//          Display unmanaged debug events
      USAll = 0x00200000,//                   Step through all unmapped stop locations
      USEpi = 0x00400000,//                   Step through method epilogs
      USPro = 0x00800000,//                   Step through method prologs
      USUnmanaged = 0x01000000,//             Step through unmanaged code
      Win32Debugger = 0x02000000,//           Specify Win32 debugger (UNSUPPORTED: use at your own risk)
      EmbeddedCLR = 0x04000000,//             Select the desktop or embedded CLR debugging
    }

    #endregion
    
    #region Commands
    
    /// <summary>
    /// Set or display breakpoints.
    /// </summary>
    public void Break()
    {
      SendCommand("b");
    }

    /// <summary>
    /// Set or display breakpoints.
    /// </summary>
    public override void Break(Breakpoint bp)
    {
      this.bp = bp;
      SendCommand("b {0}", bp);
      bptracker[bp.id] = bp;
      this.bp = null;
    }

    /// <summary>
    /// Continue the current process.
    /// </summary>
    public override void Continue()
    {
      SendCommand("cont");
    }

    /// <summary>
    /// Remove one or more breakpoints.
    /// </summary>
    /// <param name="bps"></param>
    public void DeleteBreakpoint(params Breakpoint[] bps)
    {
      if (bps.Length == 0)
      {
        SendCommand("del");
      }
      else
      {
        SendCommand("del {0}", Join(bps));
      }
    }

    /// <summary>
    /// Kill the current process and exit the debugger.
    /// </summary>
    public void Exit()
    {
      SendCommand("ex");
    }

    //Mode systemmode;
    const Mode OPERATINGMODE = (Mode) 1507344 | Mode.SeparateConsole;

    /// <summary>
    /// Display/modify various debugger modes.
    /// </summary>
    public void GetModes()
    {
//      getmodes = true;
//      SendCommand("m");
//      getmodes = false;
    }

    public override void SetDefaultModes()
    {
      Mode m = OPERATINGMODE;
      int p = 1;

      while (p <= 0x07FFFFFF)
      {
        SetMode((Mode)p, ((Mode)p & m) != 0);
        p <<= 1;
      }
    }

    /// <summary>
    /// Display/modify various debugger modes.
    /// </summary>
    public void SetMode(Mode m, bool enabled)
    {
      ignoreline = true;
      SendCommand("m {0} {1}", m, enabled ? "1" : "0");
    }

    /// <summary>
    /// Step into the next source line.
    /// </summary>
    public override void In()
    {
      SendCommand("i");
    }

    /// <summary>
    /// Step over the next source line.
    /// </summary>
    public override void Next()
    {
      SendCommand("n");
    }


    /// <summary>
    /// Print variables (locals, args, statics, etc.).
    /// </summary>
    public override void Print()
    {
      SendCommand("p");
    }

    /// <summary>
    /// Print variables (locals, args, statics, etc.).
    /// </summary>
    /// <param name="vars"></param>
    public override void Print(string[] vars)
    {
      dontadd = true;
      foreach (string var in vars)
      {
        lastvar = var;
        SendCommand("p {0}", var);
      }
      dontadd = false;
    }

    /// <summary>
    /// Remove one or more breakpoints.
    /// </summary>
    /// <param name="bp"></param>
    public override void RemoveBreakpoint(Breakpoint bp)
    {
      SendCommand("rem {0}", bp.id);
    }

    /// <summary>
    /// Step out of the current function.
    /// </summary>
    public override void Out()
    {
      SendCommand("o");
    }

    /// <summary>
    /// Start a process for debugging.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="args"></param>
    public override void Run(string filename, params string[] args)
    {
      if (args.Length == 0)
      {
        SendCommand("r {0}", filename);
      }
      else
      {
        SendCommand("r {0} {1}", filename, Join(args));
      }
    }

    /// <summary>
    /// Displays a stack trace for the current thread.
    /// </summary>
    public override void Where()
    {
      frames.Clear();
      SendCommand("w");
    }


#if DEBUG
    /// <summary>
    /// Display appdomains/assemblies/modules in the current process.
    /// </summary>
    public void AppDomainEnum()
    {
      AppDomainEnum(true);
    }

    /// <summary>
    /// Display appdomains/assemblies/modules in the current process.
    /// </summary>
    /// <param name="all"></param>
    public void AppDomainEnum(bool all)
    {
      SendCommand("ap " + (all ? "1" : "0"));
    }

    /// <summary>
    /// Associate a source file with a breakpoint or stack frame.
    /// </summary>
    /// <param name="filename"></param>
    public void AssociateSource(string filename)
    {
      SendCommand("as s " + System.IO.Path.GetFullPath(filename));
    }

    /// <summary>
    /// Associate a source file with a breakpoint or stack frame.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="bp"></param>
    public void AssociateSource(string filename, Breakpoint bp)
    {
      SendCommand("as b {0} {1}", bp.id, System.IO.Path.GetFullPath(filename));
    }

    /// <summary>
    /// Attach to a running process
    /// </summary>
    /// <param name="pid"></param>
    public void Attach(int pid)
    {
      SendCommand("a {0}", pid);
    }

    /// <summary>
    /// Stop on exception, thread, and/or load events.
    /// </summary>
    public void Catch()
    {
      SendCommand("ca");
    }

    /// <summary>
    /// Continue the current process.
    /// </summary>
    /// <param name="count"></param>
    public void Continue(int count)
    {
      SendCommand("cont {0}", count);
    }

    /// <summary>
    /// Detach from the current process.
    /// </summary>
    public void Detach()
    {
      SendCommand("de");
    }

    /// <summary>
    /// Display native or IL disassembled instructions.
    /// </summary>
    public void Disassemble()
    {
      Disassemble(5);
    }

    /// <summary>
    /// Display native or IL disassembled instructions.
    /// </summary>
    /// <param name="linecount"></param>
    public void Disassemble(int linecount)
    {
      SendCommand("dis {0}", linecount);
    }

    /// <summary>
    /// Display native or IL disassembled instructions.
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="linecount"></param>
    public void Disassemble(int delta, int linecount)
    {
      SendCommand("dis {0} {1}", delta, linecount);
    }

    /// <summary>
    /// Navigate down from the current stack frame pointer.
    /// </summary>
    public void Down()
    {
      Down(1);
    }

    /// <summary>
    /// Navigate down from the current stack frame pointer.
    /// </summary>
    /// <param name="count"></param>
    public void Down(int count)
    {
      SendCommand("d {0}", count);
    }

    /// <summary>
    /// Dump the contents of memory.
    /// </summary>
    /// <param name="address"></param>
    public void Dump(int address)
    {
      SendCommand("du 0x{0:X8}", address);
    }

    /// <summary>
    /// Dump the contents of memory.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="bytecount"></param>
    public void Dump(int address, int bytecount)
    {
      SendCommand("du 0x{0:X8} {1}", address, bytecount);
    }

    /// <summary>
    /// Function evaluation
    /// </summary>
    /// <param name="functionname"></param>
    /// <param name="args"></param>
    public void FunctionEval(string functionname, params string[] args)
    {
      SendCommand("f {0} {1}", functionname, Join(args));
    }

    /// <summary>
    /// Continue the current process.
    /// </summary>
    public void Go()
    {
      SendCommand("g");
    }

    /// <summary>
    /// Continue the current process.
    /// </summary>
    /// <param name="count"></param>
    public void Go(int count)
    {
      SendCommand("g {0}", count);
    }

    /// <summary>
    /// Display debugger command descriptions.
    /// </summary>
    public void Help()
    {
      SendCommand("h");
    }

    /// <summary>
    /// Ignore exception, thread, and/or load events.
    /// </summary>
    public void Ignore()
    {
      SendCommand("ig");
    }

    /// <summary>
    /// Step into the next source line.
    /// </summary>
    /// <param name="count"></param>
    public void In(int count)
    {
      SendCommand("i {0}", count);
    }

    /// <summary>
    /// Kill the current process.
    /// </summary>
    public void Kill()
    {
      SendCommand("k");
    }

    /// <summary>
    /// Display loaded modules, classes, or global functions.
    /// </summary>
    public void List()
    {
      SendCommand("l");
    }

    /// <summary>
    /// Step over the next source line.
    /// </summary>
    /// <param name="count"></param>
    public void Next(int count)
    {
      SendCommand("n {0}", count);
    }

    /// <summary>
    /// Set or display the source file search path.
    /// </summary>
    public void Path()
    {
      SendCommand("pa");
    }

    /// <summary>
    /// Set or display the source file search path.
    /// </summary>
    /// <param name="path"></param>
    public void Path(string path)
    {
      SendCommand("pa {0}", System.IO.Path.GetFullPath(path));
    }

    /// <summary>
    /// Display all managed processes running on the system.
    /// </summary>
    public void ProcessEnum()
    {
      SendCommand("pro");
    }

    /// <summary>
    /// Kill the current process and exit the debugger.
    /// </summary>
    public void Quit()
    {
      SendCommand("q");
    }

    /// <summary>
    /// Reload a source file for display.
    /// </summary>
    public void RefreshSource()
    {
      SendCommand("ref");
    }

    /// <summary>
    /// Reload a source file for display.
    /// </summary>
    /// <param name="sourcefile"></param>
    public void RefreshSource(string sourcefile)
    {
      SendCommand("ref {0}", sourcefile);
    }

    /// <summary>
    /// Display CPU registers for current thread.
    /// </summary>
    public void Registers()
    {
      SendCommand("reg");
    }

    /// <summary>
    /// Resume a thread.
    /// </summary>
    /// <param name="tid"></param>
    public void Resume(int tid)
    {
      SendCommand("re {0}", tid);
    }

    /// <summary>
    /// Resume a thread.
    /// </summary>
    /// <param name="tid"></param>
    public void ResumeAllOther(int tid)
    {
      SendCommand("re ~{0}", tid);
    }

    /// <summary>
    /// Start a process for debugging.
    /// </summary>
    public void Run()
    {
      SendCommand("r");
    }

    /// <summary>
    /// Modify the value of a variable (locals, statics, etc.).
    /// </summary>
    /// <param name="var"></param>
    /// <param name="value"></param>
    public void Set(string var, object value)
    {
      SendCommand("set {0} {1}", var, value);
    }

    /// <summary>
    /// Set the next statement to a new line.
    /// </summary>
    /// <param name="linenumber"></param>
    public void SetIP(int linenumber)
    {
      SendCommand("setip {0}", linenumber);
    }

    /// <summary>
    /// Display source code lines.
    /// </summary>
    public void Show()
    {
      Show(5);
    }

    /// <summary>
    /// Display source code lines.
    /// </summary>
    /// <param name="count"></param>
    public void Show(int count)
    {
      SendCommand("sh {0}", count);
    }

    /// <summary>
    /// Step into the next source line.
    /// </summary>
    public void Step()
    {
      Step(1);
    }

    /// <summary>
    /// Step into the next source line.
    /// </summary>
    /// <param name="count"></param>
    public void Step(int count)
    {
      SendCommand("s {0}", count);
    }

    /// <summary>
    /// Set or display breakpoints.
    /// </summary>
    public void Stop()
    {
      Break();
    }

    /// <summary>
    /// Suspend a thread.
    /// </summary>
    /// <param name="tid"></param>
    public void Suspend(int tid)
    {
      SendCommand("su {0}", tid);
    }

    /// <summary>
    /// Suspend a thread.
    /// </summary>
    /// <param name="tid"></param>
    public void SuspendAllOther(int tid)
    {
      SendCommand("su ~{0}", tid);
    }

    /// <summary>
    /// Set or display current threads.
    /// </summary>
    public void Threads()
    {
      SendCommand("t");
    }

    /// <summary>
    /// Set or display current threads.
    /// </summary>
    public void Threads(int tid)
    {
      SendCommand("t {0}", tid);
    }

    /// <summary>
    /// Navigate up from the current stack frame pointer.
    /// </summary>
    public void Up()
    {
      Up(1);
    }

    /// <summary>
    /// Navigate up from the current stack frame pointer.
    /// </summary>
    /// <param name="count"></param>
    public void Up(int count)
    {
      SendCommand("u {0}", count);
    }

    int prevwhere = 0;

    /// <summary>
    /// Displays a stack trace for the current thread.
    /// </summary>
    /// <param name="count"></param>
    public void Where(int count)
    {
      frames.Clear();
      prevwhere = count;
      SendCommand("w {0}", count);
    }
#endif

    #endregion

	}
}
