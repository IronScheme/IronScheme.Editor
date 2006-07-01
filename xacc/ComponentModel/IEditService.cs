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
using Xacc.Controls;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Xacc.ComponentModel
{
  /// <summary>
  /// Support find dialog in document
  /// </summary>
  public interface IFind
  {
    /// <summary>
    /// Shows the find/replace dialog
    /// </summary>
    void FindReplace();
  }

  /// <summary>
  /// Support for scrolling a document
  /// </summary>
  public interface IScroll
  {
    /// <summary>
    /// Scrolls one page up
    /// </summary>
    void ScrollPageUp();

    /// <summary>
    /// Scrolls one page down
    /// </summary>
    void ScrollPageDown();

    /// <summary>
    /// Scroll up
    /// </summary>
    void ScrollUp();

    /// <summary>
    /// Scroll down
    /// </summary>
    void ScrollDown();
  }

  /// <summary>
  /// Support for navigating a document
  /// </summary>
  public interface INavigate
  {
    /// <summary>
    /// Navigate up
    /// </summary>
    void NavigateUp();

    /// <summary>
    /// Navigate down
    /// </summary>
    void NavigateDown();

    /// <summary>
    /// Navigate left
    /// </summary>
    void NavigateLeft();

    /// <summary>
    /// Navigate right
    /// </summary>
    void NavigateRight();

    /// <summary>
    /// Navigate home
    /// </summary>
    void NavigateHome();

    /// <summary>
    /// Navigate end
    /// </summary>
    void NavigateEnd();

    /// <summary>
    /// Navigate up one page
    /// </summary>
    void NavigatePageUp();

    /// <summary>
    /// Navigate down one page
    /// </summary>
    void NavigatePageDown();
  }

  /// <summary>
  /// Support for basic editing
  /// </summary>
  public interface IEdit
  {
    /// <summary>
    /// Undo's the last operation
    /// </summary>
    void Undo();

    /// <summary>
    /// Redo's the last operation
    /// </summary>
    void Redo();

    /// <summary>
    /// Cuts the current selection to the clipboard
    /// </summary>
    void Cut();

    /// <summary>
    /// Copys the current selection to the clipboard
    /// </summary>
    void Copy();

    /// <summary>
    /// Pastes the content of the clipboard
    /// </summary>
    void Paste();

    /// <summary>
    /// Delete selected content
    /// </summary>
    void DeleteSelected();

    /// <summary>
    /// Select all content
    /// </summary>
    void SelectAll();
  }


  /// <summary>
  /// Support for special clipboard actions
  /// </summary>
  public interface IEditSpecial
  {
    /// <summary>
    /// Copys selection to text format
    /// </summary>
    void CopyToText();

    /// <summary>
    /// Copys selection to HTML format
    /// </summary>
    void CopyToHtml();

    /// <summary>
    /// Copies selection to RTF format
    /// </summary>
    void CopyToRtf();
  }

  /// <summary>
  /// Support for advanced editing
  /// </summary>
  public interface IEditAdvanced
  {
    /// <summary>
    /// Comments the selection
    /// </summary>
    void CommentSelection();

    /// <summary>
    /// Uncomments the selection
    /// </summary>
    void UnCommentSelection();

    /// <summary>
    /// Increases the indent
    /// </summary>
    void IncreaseIndent();

    /// <summary>
    /// Decrease the indent
    /// </summary>
    void DecreaseIndent();

    /// <summary>
    /// Converts selection to uppercase
    /// </summary>
    void SelectionToLower();

    /// <summary>
    /// Converts selection to lower case
    /// </summary>
    void SelectionToUpper();
  }

  interface IEditService : IService
  {
  }

  [Menu("Edit")]
  sealed class EditService : ServiceBase, IEditService
  {
    readonly FileManager fm;
    FindDialog finddlg;

    public EditService()
    {
      fm = ServiceHost.File as FileManager;
    }

    [MenuItem("Find/Replace", Index = 0, State = ApplicationState.Buffer, Image = "Edit.Find.png", AllowToolBar = true)]
    void FindReplace()
    {
      //IFind
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        if (finddlg == null)
        {
          finddlg = new FindDialog(atb);
          ServiceHost.Window.MainForm.AddOwnedForm(finddlg);
        }
			
        finddlg.Show();
        finddlg.Location = atb.PointToScreen(new Point(atb.Width - finddlg.Width - atb.vscroll.Width, 0));
      }
    }

    [MenuItem("Undo", Index = 10, State = ApplicationState.Edit, Image = "Edit.Undo.png", AllowToolBar = true)]
    void Undo()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.Undo();
      }
    }

    [MenuItem("Redo", Index = 11, State = ApplicationState.Edit, Image = "Edit.Redo.png", AllowToolBar = true)]
    void Redo()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.Redo();
      }
    }

    [MenuItem("Cut", Index = 20, State = ApplicationState.Edit, Image = "Edit.Cut.png", AllowToolBar = true)]
    void Cut()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        try
        {
          atb.Cut();
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
          // something is breaking the clipboard
          Trace.WriteLine("Could not cut to clipboard, is VNC open? This is a known error.");
        }
      }
    }

    [MenuItem("Copy", Index = 21, State = ApplicationState.Edit, Image = "Edit.Copy.png", AllowToolBar = true)]
    void Copy()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        try
        {
          atb.Copy();
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
          // something is breaking the clipboard
          Trace.WriteLine("Could not copy to clipboard, is VNC open? This is a known error.");
        }
      }
    }

    [MenuItem("Paste", Index = 22, State = ApplicationState.Edit, Image = "Edit.Paste.png", AllowToolBar = true)]
    void Paste()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.Paste();
      }
    }

    [MenuItem("Delete", Index = 23, State = ApplicationState.Edit, Image = "Edit.Delete.png")]
    void Delete()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.DeleteSelected();
      }
    }

    [MenuItem("Select all", Index = 30, State = ApplicationState.Edit, Image = "Edit.SelectAll.png")]
    void SelectAll()
    {
      IEdit atb = fm.CurrentControl as IEdit;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.SelectAll();
      }
    }

    [MenuItem("Copy Special\\Copy to Text", Index = 40, State = ApplicationState.Buffer, Image = "Edit.Copy.png")]
    void CopyToText()
    {
      IEditSpecial atb = fm.CurrentControl as IEditSpecial;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.CopyToText();
      }
    }

    [MenuItem("Copy Special\\Copy to HTML", Index = 41, State = ApplicationState.Buffer, Image = "Edit.Copy.png")]
    void CopyToHtml()
    {
      IEditSpecial atb = fm.CurrentControl as IEditSpecial;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.CopyToHtml();
      }
    }

    [MenuItem("Copy Special\\Copy to RTF", Index = 42, State = ApplicationState.Buffer, Image = "Edit.Copy.png")]
    void CopyToRtf()
    {
      IEditSpecial atb = fm.CurrentControl as IEditSpecial;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.CopyToRtf();
      }
    }

    [MenuItem("Advanced\\Strip trailing space", Index = 43, State = ApplicationState.Buffer)]
    void StripSpace()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.StripTrailingWhiteSpace();
      }
    }

#if DEBUG
    [MenuItem("Break to line", Index = 1000, State = ApplicationState.Buffer)]
    void BreakLine()
    {
      AdvancedTextBox atb = ServiceHost.File[ServiceHost.File.Current] as AdvancedTextBox;
      AdvancedTextBox.TextBuffer.TokenLine tl = atb.Buffer.GetUserState(atb.Buffer.CurrentLine);
      AdvancedTextBox.TextBuffer.DrawInfo[] di = atb.Buffer.GetDrawCache(atb.Buffer.CurrentLine);
      IDebugService dbg = ServiceHost.Debug;
      System.Diagnostics.Debugger.Break();
      Console.WriteLine("{0}{1}", tl, di);
      atb.Buffer.SetDrawCache(atb.Buffer.CurrentLine, null);
      atb.Invalidate();
    }

    [MenuItem("Send probe", Index = 1001, State = ApplicationState.Buffer)]
    void SendProbe()
    {
      AdvancedTextBox atb = ServiceHost.File[ServiceHost.File.Current] as AdvancedTextBox;
      //System.Diagnostics.Debugger.Break();
      atb.Buffer.SendProbe();
    }

#endif

    #region Utility methods for buffer

    public void NavigateHome()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateHome();
      }
    }

    public void NavigateEnd()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateEnd();
      }
    }

    public void NavigateRight()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateRight();
      }
    }

    public void NavigateLeft()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateLeft();
      }
    }


    public void GotoFirstLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.GotoFirstLine();
      }
    }

    public void GotoLastLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.GotoLastLine();
      }
    }

    public void NavigatePageUp()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigatePageUp();
      }
    }

    public void NavigatePageDown()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigatePageDown();
      }
    }

    public void NavigateUp()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateUp();
      }
    }

    public void NavigateDown()
    {
      INavigate atb = fm.CurrentControl as INavigate;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.NavigateDown();
      }
    }

    public void ScrollToFirstLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.ScrollToFirstLine();
      }
    }

    public void ScrollToLastLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.ScrollToLastLine();
      }
    }

    public void ScrollPageUp()
    {
      IScroll atb = fm.CurrentControl as IScroll;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.ScrollPageUp();
      }
    }

    public void ScrollPageDown()
    {
      IScroll atb = fm.CurrentControl as IScroll;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.ScrollPageDown();
      }
    }

    public void ScrollUp()
    {
      IScroll atb = fm.CurrentControl as IScroll;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.ScrollUp();
      }
    }

    public void ScrollDown()
    {
      IScroll atb = fm.CurrentControl as IScroll;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.ScrollDown();
      }
    }

    public void GotoNextToken()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.GotoNextToken();
      }
    }

    public void GotoPreviousToken()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.GotoPreviousToken();
      }
    }


    
    public void RemoveBefore()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.RemoveBefore();
      }
    }

    public void RemoveAfter()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.RemoveAfter();
      }
    }

    public void InsertLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.InsertLine();
      }
    }
    
    public void SelectLine()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.SelectLine();
      }
    }

    public void ShowAutoComplete()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null && atb.Focused)
      {
        atb.ShowAutoComplete();
      }
    }

    public void HideAutoComplete()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.HideAutoComplete();
      }
    }

    public void AutoCompleteNextChoice()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.AutoCompleteNextChoice();
      }
    }

    public void AutoCompletePreviousChoice()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.AutoCompletePreviousChoice();
      }
    }

    public void AutoCompleteSelectChoice()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.AutoCompleteSelectChoice();
      }
    }

    public void AutoCompleteNextPage()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.AutoCompleteNextPage();
      }
    }

    public void AutoCompletePreviousPage()
    {
      AdvancedTextBox atb = fm.CurrentControl as AdvancedTextBox;
      if (atb != null)
      {
        atb.AutoCompletePreviousPage();
      }
    }

    [MenuItem("Advanced\\Comment selection", Index = 44, State = ApplicationState.Buffer)]
    public void CommentSelection()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.CommentSelection();
      }
    }

    [MenuItem("Advanced\\UnComment selection", Index = 45, State = ApplicationState.Buffer)]
    public void UnCommentSelection()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.UnCommentSelection();
      }
    }

    [MenuItem("Advanced\\Increase indent", Index = 46, State = ApplicationState.Buffer, Image="Edit.Indent.png")]
    public void IncreaseIndent()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.IncreaseIndent();
      }
    }

    [MenuItem("Advanced\\Decrease indent", Index = 47, State = ApplicationState.Buffer, Image="Edit.Unindent.png")]
    public void DecreaseIndent()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.DecreaseIndent();
      }
    }

    [MenuItem("Advanced\\Make lowercase", Index = 48, State = ApplicationState.Buffer, Image="Edit.ToLower.png")]
    public void SelectionToLower()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.SelectionToLower();
      }
    }

    [MenuItem("Advanced\\Make uppercase", Index = 49, State = ApplicationState.Buffer, Image="Edit.ToUpper.png")]
    public void SelectionToUpper()
    {
      IEditAdvanced atb = fm.CurrentControl as IEditAdvanced;
      if (atb != null && ((Control)atb).Focused)
      {
        atb.SelectionToUpper();
      }
    }

    #endregion

  }
}
