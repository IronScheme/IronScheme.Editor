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

#region Includes
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using Xacc.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using Xacc.Controls;
using System.Drawing.Text;
using Xacc.Collections;
using Xacc.Runtime;

using SR = System.Resources;
#endregion

using System.Runtime.InteropServices;
using Xacc.Build;

namespace Xacc.ComponentModel
{
	/// <summary>
	/// Provides service for debugging
	/// </summary>
	[Name("Debug service")]
	public interface IDebugService : IService
	{
    /// <summary>
    /// Start debugging a project
    /// </summary>
    /// <param name="prj">the project to debug</param>
    void Start(Project prj);

    /// <summary>
    /// Start debugging the current project
    /// </summary>
    void Start();

    /// <summary>
    /// Exit the debugger
    /// </summary>
    void Exit();

    /// <summary>
    /// Checks if debugger is active
    /// </summary>
    bool IsActive {get;}
    
    /// <summary>
    /// Step to next line
    /// </summary>
    void Step();

    /// <summary>
    /// Step into function
    /// </summary>
    void StepInto();

    /// <summary>
    /// Step out of function
    /// </summary>
    void StepOut();

    /// <summary>
    /// Continue from break state
    /// </summary>
    void Continue();

    /// <summary>
    /// Inserts a breakpoint
    /// </summary>
    /// <param name="filename">the filename</param>
    /// <param name="linenr">the line number</param>
    /// <returns>a reference to the newly created breakpoint</returns>
    /// <remarks>Breakpoints are persisted in the project file</remarks>
    Breakpoint InsertBreakpoint(string filename, int linenr);

    /// <summary>
    /// Removes a breakpoint
    /// </summary>
    /// <param name="bp">the breakpoint to remove</param>
    /// <remarks>Breakpoints are persisted in the project file</remarks>
    void RemoveBreakpoint(Breakpoint bp);
	}

  [Menu("Debug")]
	sealed class DebugService : ServiceBase, IDebugService
	{
    ListView localsview, autosview, thisview, callstackview;
    Project proj;

    class ListViewEx : ListView
    {
      public ListViewEx()
      {
        View = View.Details;
        Dock = DockStyle.Fill;
        GridLines = true;
        FullRowSelect = true;
        Font = SystemInformation.MenuFont;
      }
    }

    [MenuItem("View\\Locals", Index = 0)]
    bool ViewLocals
    {
      get 
      { 
        if (localsview == null)
        {
          return false;
        }
        IDockContent dc = localsview.Parent as IDockContent;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ViewLocals)
        {
          (localsview.Parent as IDockContent).Activate();
        }
        else
        {
          (localsview.Parent as IDockContent).Hide();
        }
      }
    }

    [MenuItem("View\\Autos", Index = 0)]
    bool ViewAutos
    {
      get 
      { 
        if (autosview == null)
        {
          return false;
        }
        IDockContent dc = autosview.Parent as IDockContent;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ViewAutos)
        {
          (autosview.Parent as IDockContent).Activate();
        }
        else
        {
          (autosview.Parent as IDockContent).Hide();
        }
      }
    }

    [MenuItem("View\\This", Index = 0)]
    bool ViewThis
    {
      get 
      { 
        if (thisview == null)
        {
          return false;
        }
        IDockContent dc = thisview.Parent as IDockContent;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ViewThis)
        {
          (thisview.Parent as IDockContent).Activate();
        }
        else
        {
          (thisview.Parent as IDockContent).Hide();
        }
      }
    }

    [MenuItem("View\\Callstack", Index = 0)]
    bool ViewCallstack
    {
      get 
      { 
        if (callstackview == null)
        {
          return false;
        }
        IDockContent dc = callstackview.Parent as IDockContent;
        return dc.DockState != DockState.Hidden;
      }
      set 
      { 
        if (!ViewCallstack)
        {
          (callstackview.Parent as IDockContent).Activate();
        }
        else
        {
          (callstackview.Parent as IDockContent).Hide();
        }
      }
    }


    public bool IsActive 
    {
      get { return dbg != null;}
    }

//    [MenuItem("Break", Index = 3, State = 2, Shortcut = Shortcut.CtrlShiftC)]
//    public void Break()
//    {
//      dbg.Break();
//    }

    internal EventHandler bpboundchange;
    Breakpoint tempbp;

    void BpBoundChange(object sender, EventArgs e)
    {
      Breakpoint bp = sender as Breakpoint;

      if (bp != null)
      {
        tempbp = bp;
      }
      else
      {
        bp = tempbp;
      }

      if (InvokeRequired)
      {
        BeginInvoke(bpboundchange, new object[] { sender, e});
        return;
      }

      AdvancedTextBox atb = ServiceHost.File[bp.filename] as AdvancedTextBox;
      if (atb != null)
      {
        atb.Invalidate();
      }
    }

    public Breakpoint InsertBreakpoint(string filename, int linenr)
    {
      Project proj = ServiceHost.Project.Current;
      Breakpoint bp = proj.GetBreakpoint(filename, linenr);
      linenr++;

      if (bp == null)
      {
        bp = new Breakpoint();

        bp.boundchanged = bpboundchange;
        bp.filename = filename;
        bp.linenr = linenr;

        if (dbg == null)
        {
          proj.SetBreakpoint(bp);
        }
        else 
        {
          dbg.Break(bp);
          proj.SetBreakpoint(bp);
        }
      }

      return bp;
    }

    public void RemoveBreakpoint(Breakpoint bp)
    {
      if (dbg != null)
      {
        dbg.RemoveBreakpoint(bp);
      }

      ServiceHost.Project.Current.RemoveBreakpoint(bp);
    }

    public void Start(Project prj)
    {
      this.proj = prj;
      int checkcount = prj.Actions.Length;
      foreach (string a in prj.Actions)
      {
        //checkcount--;
        //ProcessAction pa = a as ProcessAction;
        //if (pa != null)
        //{
        //  Option o = pa.OutputOption;
        //  if (o == null)
        //  {
        //    if (checkcount == 0)
        //    {
        //      MessageBox.Show(ServiceHost.Window.MainForm, "Project does not have an output option.",
        //        "Error", 0, MessageBoxIcon.Error);
        //      return;
        //    }
        //  }
        //  else
        //  {
        //    string outfile = pa.GetOptionValue(o) as string;

        //    if (outfile == null || outfile == string.Empty)
        //    {
        //      MessageBox.Show(ServiceHost.Window.MainForm, "No output specified.\nPlease specify an output file in the project properties.",
        //        "Error", 0, MessageBoxIcon.Error);
        //      return;
        //    }

        //    outfile = prj.RootDirectory + Path.DirectorySeparatorChar + outfile;

        //    if (Path.GetExtension(outfile) == ".exe")
        //    {
        //      bool rebuild = false;

        //      if (File.Exists(outfile))
        //      {
        //        DateTime build = File.GetLastWriteTime(outfile);
        //        foreach (string file in prj.Sources)
        //        {
        //          if (File.Exists(file)) //little bug i need to sort
        //          {
        //            if (File.GetLastWriteTime(file) > build || ServiceHost.File.IsDirty(file))
        //            {
        //              rebuild = true;
        //              break;
        //            }
        //          }
        //        }
        //      }
        //      else
        //      {
        //        rebuild = true;
        //      }

        //      if (rebuild && !prj.Build())
        //      {
        //        MessageBox.Show(ServiceHost.Window.MainForm, string.Format("Build Failed: Unable to debug: {0}",
        //          outfile), "Error", 0, MessageBoxIcon.Error);
        //        return;
        //      }

        //      try
        //      {
        //        Start(outfile);
        //      }
        //      catch (Exception ex)
        //      {
        //        MessageBox.Show(ServiceHost.Window.MainForm, string.Format("Error debugging: {0}\nError: {1}",
        //          outfile, ex.GetBaseException().Message), "Error", 0, MessageBoxIcon.Error);
        //      }

        //      return;						
        //    }
        //  }
        //}
      }
    }

    [MenuItem("Start", Index = 5, State = ApplicationState.Project, Image = "Debug.Start.png", AllowToolBar = true)]
    public void Start()
    {
      Project prj = ServiceHost.Project.StartupProject;
      if (prj == null)
      {
        MessageBox.Show(ServiceHost.Window.MainForm, "No startup project has been selected", "Error", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
      else
      {
        Start(prj);
      }
    }
	
    public DebugService()
		{
      bpboundchange = new EventHandler(BpBoundChange);
      localsview = new ListViewEx();
      autosview = new ListViewEx();
      thisview = new ListViewEx();
      callstackview = new ListViewEx();


        InitVariableView(localsview, autosview, thisview);
        InitCallstackView(callstackview);

        //      ServiceHost.Project.Opened += new EventHandler(ProjectEvent);
        //      ServiceHost.Project.Closed += new EventHandler(ProjectEvent);
        //
        //      ServiceHost.File.Opened +=new FileManagerEventHandler(FileEvent);
        //      ServiceHost.File.Closed +=new FileManagerEventHandler(FileEvent);
      if (SettingsService.idemode)
      {

        IWindowService ws = ServiceHost.Window;

        IDockContent tbp = Runtime.DockFactory.Content();
        tbp.Text = "Locals";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        tbp.Controls.Add(localsview);
        tbp.Show(ws.Document, DockState.DockBottom);
        tbp.Hide();
        tbp.HideOnClose = true;

        tbp = Runtime.DockFactory.Content();
        tbp.Text = "Autos";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        autosview.Tag = tbp;
        tbp.Controls.Add(autosview);
        tbp.Show(ws.Document, DockState.DockBottom);
        tbp.Hide();
        tbp.HideOnClose = true;

        tbp = Runtime.DockFactory.Content();
        tbp.Text = "This";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        thisview.Tag = tbp;
        tbp.Controls.Add(thisview);
        tbp.Show(ws.Document, DockState.DockBottom);
        tbp.Hide();
        tbp.HideOnClose = true;

        tbp = Runtime.DockFactory.Content();
        tbp.Text = "Callstack";
        tbp.Icon = ServiceHost.ImageListProvider.GetIcon("console.png");
        callstackview.Tag = tbp;
        tbp.Controls.Add(callstackview);
        tbp.Show(ws.Document, DockState.DockBottom);
        tbp.Hide();
        tbp.HideOnClose = true;
      }
		}

    void InitCallstackView(ListView lv)
    {
      lv.Columns.Add("ID", 50, 0);
      lv.Columns.Add("Type", 200, 0);
      lv.Columns.Add("Method", 200, 0);
      lv.Columns.Add("Filename", 300, 0);
      lv.Columns.Add("Line", 50, 0);
    }

    void InitVariableView(params ListView[] lvs)
    {
      foreach (ListView lv in lvs)
      {
        lv.Columns.Add("Name", 200, 0);
        lv.Columns.Add("Value", 350, 0);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (dbg != null)
      {
        dbg.Dispose();
      }
      base.Dispose (disposing);
    }


    Cordbg dbg = null;
    string filename;
    ArrayList menus = new ArrayList();

    public bool Start(string filename)
    {
      if (!Cordbg.DebuggerIsAvailable)
      {
        return false;
      }
      //startdebug.Text = "Continue";
      this.filename = filename;
      dbg = new Cordbg();
      dbg.DebugProcessExited+=new EventHandler(dbg_DebugProcessExited);

      ServiceHost.State |= ApplicationState.Debug;
      ServiceHost.Error.ClearErrors(this);
      Console.WriteLine("Debugger started.");
      DebugReady();

      //get back focus
      ServiceHost.Window.MainForm.Activate();
      
      return true;
    }

    void Precommand()
    {
      ServiceHost.State &= ~ApplicationState.Break;
    }


    [MenuItem("Step", Index = 10, State = ApplicationState.DebugBreak, Image = "Debug.Step.png", AllowToolBar = true)]
    public void Step()
    {
      Precommand();
      dbg.Next();
      CommandCompleted();
    }

    [MenuItem("Step Into", Index = 11, State = ApplicationState.DebugBreak, Image = "Debug.StepInto.png", AllowToolBar = true)]
    public void StepInto()
    {
      Precommand();
      dbg.In();
      CommandCompleted();
    }

    [MenuItem("Toggle Breakpoint", Index = 20, State = ApplicationState.ProjectBuffer, Image = "Debug.ToggleBP.png", AllowToolBar = true)]
    void ToggleBP()
    {
      Project proj = ServiceHost.Project.Current;
      IFileManagerService fm = ServiceHost.File;
      AdvancedTextBox atb = fm[fm.Current] as AdvancedTextBox;
      if (atb != null)
      {
        int cline = atb.Buffer.CurrentLine;
        Breakpoint bp = proj.GetBreakpoint(fm.Current,cline);
        
        if (bp != null)
        {
          RemoveBreakpoint(bp);
          proj.RemoveBreakpoint(bp);
        }
        else
        {
          bp = InsertBreakpoint(fm.Current, cline);
          if (bp.bound)
          {
            proj.SetBreakpoint(bp);
          }
        }

        atb.Invalidate();
      }
    }

    [MenuItem("Toggle all Breakpoint", Index = 22, State = ApplicationState.ProjectBuffer, Image = "Debug.ToggleAllBP.png")]
    void ToggleAllBP()
    {
      Project proj = ServiceHost.Project.Current;
      IFileManagerService fm = ServiceHost.File;
      AdvancedTextBox atb = fm[fm.Current] as AdvancedTextBox;
      if (atb != null)
      {
        int cline = atb.Buffer.CurrentLine;

        Hashtable bpsmap = proj.GetBreakpoints(fm.Current);
        if (bpsmap != null)
        {
          ArrayList bps = new ArrayList(bpsmap.Values);
        
          foreach (Breakpoint bp in bps)
          {
            RemoveBreakpoint(bp);
            proj.RemoveBreakpoint(bp);
          }

          atb.Invalidate();
        }
      }
    }

    [MenuItem("Toggle Breakpoint state", Index = 21, State = ApplicationState.ProjectBuffer, Image = "Debug.ToggleBPState.png")]
    void ToggleBPState()
    {
      Project proj = ServiceHost.Project.Current;
      IFileManagerService fm = ServiceHost.File;
      AdvancedTextBox atb = fm[fm.Current] as AdvancedTextBox;
      if (atb != null)
      {
        int cline = atb.Buffer.CurrentLine;
        Breakpoint bp = proj.GetBreakpoint(fm.Current,cline);
        
        if (bp != null)
        {
          bp.SetEnabled(!bp.enabled);
        }
      }
    }

    bool allbp = false;

    [MenuItem("Toggle all Breakpoint state", Index = 23, State = ApplicationState.ProjectBuffer, Image = "Debug.ToggleAllBPState.png")]
    void ToggleAllBPState()
    {
      Project proj = ServiceHost.Project.Current;
      IFileManagerService fm = ServiceHost.File;
      AdvancedTextBox atb = fm[fm.Current] as AdvancedTextBox;
      if (atb != null)
      {
        int cline = atb.Buffer.CurrentLine;
        Hashtable bpsmap = proj.GetBreakpoints(fm.Current);
        if (bpsmap != null)
        {
          ArrayList bps = new ArrayList(bpsmap.Values);
        
          foreach (Breakpoint bp in bps)
          {
            bp.enabled = allbp;
          }

          atb.Invalidate();

          allbp = !allbp;
        }
      }
    }

    [MenuItem("Step Out", Index = 12, State = ApplicationState.DebugBreak, Image = "Debug.StepOut.png", AllowToolBar = true)]
    public void StepOut()
    {
      Precommand();
      dbg.Out();
      CommandCompleted();
    }

    void UpdateCallstackView(Cordbg.StackFrame[] frames)
    {
      callstackview.Items.Clear();

      foreach (Cordbg.StackFrame sf in frames)
      {
        ListViewItem lvi = new ListViewItem( new string[] 
        {
          sf.Id.ToString(),
          sf.Type,
          sf.Method,
          sf.Filename,
          (sf.LineNumber + 1).ToString()
        });

        callstackview.Items.Add(lvi);
        lvi.Font = SystemInformation.MenuFont;
        if (sf.IsCurrentFrame)
        {
          lvi.Selected = true;
        }
      }
    }

    void UpdateVariableView(ListView lv, params string[] values)
    {
      lv.Items.Clear();

      Array.Sort(values);

      foreach (string var in values)
      {
        ListViewItem lvi = new ListViewItem( new string[]
          {
            var,
            dbg[var]
          });
        lvi.Font = SystemInformation.MenuFont;

        lv.Items.Add(lvi);
      }
    }

    void UpdateViews()
    {
      UpdateVariableView(localsview, dbg.Locals);
      UpdateVariableView(autosview, dbg.Autos);
      UpdateVariableView(thisview, "this");
      UpdateCallstackView(dbg.CallStack);

      ServiceHost.State |= ApplicationState.Break;
    }

    string lastsrcfile = null;
    int lastlinenr;

    void GotoFilePosition()
    {
      if (lastsrcfile != null)
      {
        AdvancedTextBox atb2 = ServiceHost.File[lastsrcfile] as AdvancedTextBox;
        if (atb2 != null)
        {
          atb2.debugline = -1;
          atb2.debugexcept = false;
        }
      }

      lastlinenr = dbg.TopFrame.LineNumber;
      lastsrcfile = dbg.TopFrame.Filename;
      AdvancedTextBox atb = proj.OpenFile(lastsrcfile) as AdvancedTextBox;
      if (atb != null)
      {
        atb.debugline = lastlinenr;
        atb.debugexcept = dbg.lastexinfo != null;
        atb.MoveIntoView(lastlinenr);
        ServiceHost.File.BringToFront(atb);
      }
    }

    static string[] thiss = {"this"};
    bool checkforbp = true;

    void CommandCompleted()
    {
      dbg.Where();

      if (dbg.TopFrame != null)
      {
        if (dbg.TopFrame.LineNumber >= 0)
        {
          if (dbg.lastexinfo != null)
          {
            dbg.Print();
            dbg.Print(dbg.Autos);
            dbg.Print(thiss);

            GotoFilePosition();
            UpdateViews();
            ServiceHost.Error.OutputErrors( new ActionResult(ActionResultType.Error, lastlinenr + 1, 
              string.Format("An exception of type '{0}' has occurred. Message: {1}", dbg.lastexinfo.type, dbg.lastexinfo.message)
              , lastsrcfile));
          }
          else
          if (checkforbp)
          {
            Breakpoint bp = proj.GetBreakpoint(dbg.TopFrame.Filename, dbg.TopFrame.LineNumber);
            if (bp != null && bp.enabled)
            {
              dbg.Print();
              dbg.Print(dbg.Autos);
              dbg.Print(thiss);

              GotoFilePosition();
              UpdateViews();
              checkforbp = false;
              
            }
            else
            {
              Continue();
            }
          }
          else
          {
            dbg.Print();
            dbg.Print(dbg.Autos);
            dbg.Print(thiss);

            GotoFilePosition();
            UpdateViews();
          }
        }
        else
        {
          StepOut();
        }
      }
    }

    void DebugReady()
    {
      dbg.SetDefaultModes();

      dbg.Run(filename);
      //dbg.GetModes();
      foreach (Breakpoint bp in proj.GetAllBreakpoints())
      {
        if (bp.enabled)
        {
          bp.SetBound(false);
          dbg.Break(bp);
        }
      }

      CommandCompleted();
    }

    [MenuItem("Continue", Index = 6, State = ApplicationState.DebugBreak, Image = "Debug.Continue.png", AllowToolBar = true)]
    public void Continue()
    {
      checkforbp = true;
      dbg.Continue();
      CommandCompleted();
      checkforbp = false;
    }

    [MenuItem("Exit", Index = 7, State = ApplicationState.Debug, Image = "Debug.Exit.png", AllowToolBar = true)]
    public void Exit()
    {
      ProcessExit();
    }

    void ProcessExit()
    {
      if (dbg != null)
      {
        dbg.Dispose();
        dbg = null;

        ServiceHost.State &= ~ApplicationState.DebugBreak;

        checkforbp = true;

        Breakpoint[] bps = null;

        bps = proj.GetAllBreakpoints();

        foreach (Breakpoint bp in bps)
        {
          bp.bound = true;
        }

        proj = null;

        localsview.Items.Clear();
        autosview.Items.Clear();
        thisview.Items.Clear();
        callstackview.Items.Clear();

        if (lastsrcfile != null)
        {
          AdvancedTextBox atb = ServiceHost.File[lastsrcfile] as AdvancedTextBox;
          if (atb != null)
          {
            atb.debugexcept = false;
            atb.debugline = -1;
            ServiceHost.File.BringToFront(atb);
          }
        }

        Console.WriteLine("Debugger exited.");
      }
    }

    delegate void VOIDVOID();

    void dbg_DebugProcessExited(object sender, EventArgs e)
    {
      try
      {
        BeginInvoke( new VOIDVOID(ProcessExit), new object[0]);
      }
      catch (InvalidOperationException)
      {
        //happens on app exit, no harm done
      }
    }
  }
}
