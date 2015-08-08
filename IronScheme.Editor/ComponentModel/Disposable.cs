#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;

namespace IronScheme.Editor.ComponentModel
{
  /// <summary>
  /// Base class for disposable objects
  /// </summary>
  public abstract class Disposable : IDisposable
	{
    bool disposed = false;

    /// <summary>
    /// Fires when object is about to be disposed
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Fires when object has been disposed
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Disposes the object
    /// </summary>
    public void Dispose()
    {
      InvokeDispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~Disposable()
    {
      InvokeDispose(false);
    }

    /// <summary>
    /// Gets whether object has been disposed
    /// </summary>
    protected bool IsDisposed
    {
      get {return disposed;}
    }

    void InvokeDispose(bool disposing)
    {
      if (!disposed)
      {
        if (Disposing != null)
        {
          Disposing(this, EventArgs.Empty);
        }
        Dispose(disposing);
        disposed = true;
        if (Disposed != null)
        {
          Disposed(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Called when object is disposed
    /// </summary>
    /// <param name="disposing">true is Dispose() was called</param>
    protected virtual void Dispose(bool disposing)
    {
    }

  }


  public abstract class RemoteDisposable : MarshalByRefObject, IDisposable
  {
     bool disposed = false;

    /// <summary>
    /// Fires when object is about to be disposed
    /// </summary>
    public event EventHandler Disposing;

    /// <summary>
    /// Fires when object has been disposed
    /// </summary>
    public event EventHandler Disposed;

    /// <summary>
    /// Disposes the object
    /// </summary>
    public void Dispose()
    {
      InvokeDispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~RemoteDisposable()
    {
      InvokeDispose(false);
    }

    /// <summary>
    /// Gets whether object has been disposed
    /// </summary>
    protected bool IsDisposed
    {
      get {return disposed;}
    }

    void InvokeDispose(bool disposing)
    {
      if (!disposed)
      {
        if (Disposing != null)
        {
          Disposing(this, EventArgs.Empty);
        }
        Dispose(disposing);
        disposed = true;
        if (Disposed != null)
        {
          Disposed(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Called when object is disposed
    /// </summary>
    /// <param name="disposing">true is Dispose() was called</param>
    protected virtual void Dispose(bool disposing)
    {
    }

  }
}
