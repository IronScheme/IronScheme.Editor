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
using System.Threading;
using Xacc.ComponentModel;

namespace Xacc.Timers
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
      loop.Abort();
    }
  }
}
