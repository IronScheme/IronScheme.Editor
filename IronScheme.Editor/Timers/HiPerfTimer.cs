#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


//#define NO_PINVOKE

using System;
using System.Threading;
using IronScheme.Editor.Runtime;

namespace IronScheme.Editor.Timers
{
  [Obsolete("Use Stopwatch")]
  sealed class HiPerfTimer
	{
		long startTime, stopTime, freq;

		// Constructor
		public HiPerfTimer()
		{
			if (!kernel32.QueryPerformanceFrequency(out freq))
			{
				// high-performance counter not supported
				throw new NotSupportedException("no timer");
			}
		}

		// Start the timer
		public void Start()
		{
			// lets do the waiting threads there work
			Thread.Sleep(0);

			kernel32.QueryPerformanceCounter(out startTime);
		}

		// Stop the timer
		public void Stop()
		{
			kernel32.QueryPerformanceCounter(out stopTime);
		}

		// Returns the duration of the timer (in milliseconds)
		public double Duration
		{
			get
			{
				return (double)(stopTime - startTime) / freq * 1000F;
			}
		}

		public override string ToString()
		{
			return String.Format("{0,6:f1}ms", Duration);
		}
	}
}



