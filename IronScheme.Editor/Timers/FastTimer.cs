#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Threading;
using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.Timers
{
  sealed class FastTimer : Disposable
	{
    readonly Thread loop;
    public event EventHandler Tick;
    bool enabled = false;
    int interval;
    bool trigger = false;

    static readonly long TICKSPERSECOND = new TimeSpan(0,0,1).Ticks;

		public FastTimer(int interval)
		{
      Interval = interval;
      loop = new Thread( new ThreadStart(StartLoop));
      loop.Name = "Timer";
      loop.IsBackground = true;
		}

    public void Trigger()
    {
      trigger = true;
    }

    public int Interval
    {
      get {return (int)(1000f/interval/TICKSPERSECOND);}
      set 
      {
        interval = (int)(value/1000f * TICKSPERSECOND);
      }
    }

    public bool Enabled
    {
      get {return enabled;}
      set 
      {
        if (value && !running)
        {  
          running = true;
          loop.Start();
        }
        enabled = value;
        reset = DateTime.Now.Ticks;
      }
    }

    long reset;
    volatile bool running = false;

    void StartLoop()
    {
      try
      {
        reset = DateTime.Now.Ticks;
        while (running)
        {
          if (enabled)
          {
            if (DateTime.Now.Ticks - reset > interval || trigger)
            {
              if (Tick != null)
              {
                Tick(this, EventArgs.Empty);
              }
              reset = DateTime.Now.Ticks;
              trigger = false;
            }
            Thread.Sleep(20);
          }
          else
          {
            Thread.Sleep(50);
            reset = DateTime.Now.Ticks;
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
    }

    protected override void Dispose(bool disposing)
    {
      running = false;
      enabled = false;
      //Thread.Sleep(50);
      //loop.Abort();
    }
  }
}
