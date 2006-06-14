//#define BZ2
//#define SHARPZIPLIB // not done

using System;
using System.IO;
using System.IO.Compression;

#if BZ2
using Xacc.IO.Compression;
#endif

namespace Xacc.Runtime
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public sealed class Compression
	{
    Compression(){}

    public static byte[] Compress(byte[] buffer)
    {
#if BZ2
      return Bzip2Utility.Compress(buffer);
#else
#if SHARPZIPLIB
      return blah;
#else
      return buffer;
#endif
#endif
    }

    public static byte[] Decompress(byte[] buffer)
    {
#if BZ2
      return Bzip2Utility.Decompress(buffer);
#else
#if SHARPZIPLIB
      return blah;
#else
      return buffer;
#endif
#endif
    }
	}
}
