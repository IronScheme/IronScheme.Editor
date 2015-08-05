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

//#define NO_PINVOKE

using System;
using System.Threading;
using IronScheme.Editor.Runtime;

namespace IronScheme.Editor.Timers
{
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



