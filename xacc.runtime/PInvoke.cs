//#define NO_PINVOKE

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Xacc.Runtime
{
	/// <summary>
	/// Summary description for PInvoke.
	/// </summary>
	public sealed class kernel32
	{
    const string FILENAME = "kernel32";
    kernel32(){}

#if !NO_PINVOKE
    [DllImport(FILENAME)]
    public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport(FILENAME)]
    public static extern bool QueryPerformanceFrequency(out long lpFrequency);

    
    [DllImport(FILENAME)]
    public extern static int GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath,int cchBuffer);

#else

    public static bool QueryPerformanceCounter(out long lpPerformanceCount)
    {
      lpPerformanceCount = DateTime.Now.Ticks;
      return true;
    }

    public static bool QueryPerformanceFrequency(out long lpFrequency)
    {
      lpFrequency = 10000000L;
      return true;
    }

    public static int GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath,int cchBuffer)
    {
      lpszLongPath.Append(lpszShortPath);
      return lpszShortPath.Length;
    }
#endif

	}

  public sealed class user32
  {
    const string FILENAME = "user32";
    user32(){}

#if !NO_PINVOKE

    [DllImport(FILENAME)]
    public static extern int MsgWaitForMultipleObjects(int nCount, int pHandles, bool fWaitAll, int dwMilliseconds, int dwWakeMask);
#else
    public static int MsgWaitForMultipleObjects(int nCount, int pHandles, bool fWaitAll, int dwMilliseconds, int dwWakeMask)
    {
      // have no clue????
      System.Threading.Thread.Sleep(0);
      System.Threading.Thread.Sleep(250);
      return 0;
    }
#endif
  }
}
