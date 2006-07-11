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

    //http://www.codeproject.com/useritems/ViewStateCompression.asp

    public static byte[] Compress(byte[] buffer)
    {
#if BZ2
      return Bzip2Utility.Compress(buffer);
#else
#if SHARPZIPLIB
      return blah;
#else
      using (MemoryStream output = new MemoryStream())
      {
        using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true))
        {
          gzip.Write(buffer, 0, buffer.Length);
        }
        return output.ToArray();
      }
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
      using (MemoryStream input = new MemoryStream(buffer))
      {
        using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress, true))
        {
          using (MemoryStream output = new MemoryStream())
          {
            byte[] buff = new byte[buffer.Length * 2];
            int read = -1;
            
            while ((read = gzip.Read(buff, 0, buff.Length)) > 0)
            {
              output.Write(buff, 0, read);
            }
            return output.ToArray();
          }
        }
      }
      
#endif
#endif
    }
	}
}
