#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System.Collections;

namespace IronScheme.Editor.Collections
{
  /// <summary>
  /// 
  /// </summary>
  public class UndoRedoStack
  {
    // this should be enough for the time being in kilobytes
    const int MAXLEVEL = 4096*1024;
    int level = 0, redolevel = 0;
    int size = 0;
    ArrayList stack = new ArrayList(128);
    readonly IHasUndo buffer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    public UndoRedoStack(IHasUndo buffer)
    {
      this.buffer = buffer;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
      stack.Clear();
      size = level = redolevel = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanUndo
    {
      get {return level > 0;}
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsEmpty
    {
      get {return level == 0;}
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsFull
    {
      get {return size >= MAXLEVEL;}
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanRedo
    {
      get	{return redolevel > level;}
    }

    /// <summary>
    /// 
    /// </summary>
    public int RedoLevels
    {
      get {return redolevel;}
    }

    /// <summary>
    /// 
    /// </summary>
    public int CurrentLevel
    {
      get {return level;}
      set 
      {
        level = value;
      }
    }

    int SizeTo(int index)
    {
      int size = 0;
      for (int i = 0; i < index; i++)
      {
        size += (stack[i] as Operation).Size;
      }
      return size;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="op"></param>
    public void Push(Operation op)
    {
      if (CanRedo)
      {
        stack.RemoveRange(level, redolevel - level);
      }

      if (IsFull)
      {
        int pivot = stack.Count/10;
        size -= SizeTo(pivot);

        stack.RemoveRange(0, pivot);
        level -= pivot;
      }

      op.buffer = buffer;
      level++;
      stack.Add(op);
      size += op.Size;

      // once another object is pushed, the redolevel gets reset
      redolevel = level;
    }

    /// <summary>
    /// 
    /// </summary>
    public Operation Top
    {
      get 
      {
        if (IsEmpty)
        {
          return null;
        }
        return stack[level - 1] as Operation;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Operation Pop()
    {
      Operation op = Top;
      level--;
      size -= op.Size;
      return op;
    }
  }

  /// <summary>
  /// Interface for getting state
  /// </summary>
  public interface IHasUndo
  {
    /// <summary>
    /// Gets the current state
    /// </summary>
    /// <returns>the current state</returns>
    object GetUndoState();

    /// <summary>
    /// Restores the previously saved state
    /// </summary>
    /// <param name="state">the state toe restore</param>
    void RestoreUndoState(object state);
  }

  /// <summary>
  /// 
  /// </summary>
  public abstract class Operation
  {
    object before, after;

    internal IHasUndo buffer;

    internal void CallUndo()
    {
      buffer.RestoreUndoState(after);
      Undo();
      buffer.RestoreUndoState(before);
    }

    internal void CallRedo()
    {
      buffer.RestoreUndoState(before);
      Redo();
      buffer.RestoreUndoState(after);
    }

    /// <summary>
    /// 
    /// </summary>
    protected abstract void Undo();

    /// <summary>
    /// 
    /// </summary>
    protected abstract void Redo();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    protected Operation(object before, object after)
    {
      this.before = before;
      this.after = after;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual int Size
    {
      get {return 1;}
    }
  }
}
