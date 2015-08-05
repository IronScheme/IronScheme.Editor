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
using IronScheme.Editor.ComponentModel;

namespace IronScheme.Editor.CodeModel
{
  /*
   * Location - Normal
   *          - Pair
   *          - Range
   * 
   */

  delegate void LocationCallback(IToken token);

  /// <summary>
  /// Defines a location in a specific file
  /// </summary>
  [Serializable]
  //[StructLayout(LayoutKind.Auto, Pack=2)]
  // possible shrinkage, use a indexed lookup table, although this could get big
  public sealed class Location : IComparable//, ISerializable
  {
    int start, end;

    internal LocationCallback callback;

    
    // Layout (x2) |------- 18 bit line ------|---- 12 bit col ----|-2 bit flags-|
    // lines about 600k max
    // col about 1024
    // end col 1024
    // line count big too
    // pack error and warning
    // filename is ok
    internal string filename;
    
    Location()
    {
    }

//    Location(SerializationInfo info, StreamingContext ctx)
//    {
//      
//    }
//
//    void ISerializable.GetObjectData(SerializationInfo information, StreamingContext context){}

    /// <summary>
    /// Creates an instance of Location
    /// </summary>
    /// <param name="col">the column</param>
    public Location(int col)
      : this(0, col)
    {
      
    }

    /// <summary>
    /// Creates an instance of Location
    /// </summary>
    /// <param name="line">the line number</param>
    /// <param name="col">the column</param>
    /// <param name="endline">the line count</param>
    /// <param name="endcol">the end column</param>
    public Location(int line, int col, int endline, int endcol)
      : this(line,col,endline,endcol, string.Empty)
    {
    }

    /// <summary>
    /// Creates an instance of Location
    /// </summary>
    /// <param name="line">the line number</param>
    /// <param name="col">the column</param>
    public Location(int line, int col)
      : this(line, col, 0, 0, string.Empty)
    {
    }

    /// <summary>
    /// Creates an instance of Location
    /// </summary>
    /// <param name="line">the line number</param>
    /// <param name="col">the column</param>
    /// <param name="endline">the line count</param>
    /// <param name="endcol">the end column</param>
    /// <param name="filename">the filename</param>
    public Location(int line, int col, int endline, int endcol, string filename)
    {
      LineNumber = line;
      Column = col;
      LineCount = endline;
      EndColumn = endcol;
      this.filename = filename;
    }

    const int LINEMAX = (1 << 18) - 1;
    const int LINEMASK= (LINEMAX << 14);
    const int COLMAX  = (1 << 12) - 1;
    const int COLMASK = (COLMAX << 2);
    const int TAGMAX  = (1 << 2) - 1;
    const int TAGMASK = (TAGMAX << 0);

    const int ERROR = 1;
    const int WARN = 1;
    const int DISABLED = 2;
    const int PAIRED = 2;

    /// <summary>
    /// Gets or sets the line number
    /// </summary>
    public int LineNumber
    {
      get {return (start >> 14) & LINEMAX ;}
      set 
      {
        start &= ~LINEMASK;
        start |= ((value << 14) & LINEMASK);
      }
    }

    /// <summary>
    /// Gets or sets the line count
    /// </summary>
    public int LineCount
    {
      get {return (end >> 14) & LINEMAX ;}
      set 
      {
        end &= ~LINEMASK;
        end |= ((value << 14) & LINEMASK);
      }
    }

    /// <summary>
    /// Gets or sets the end line number.
    /// </summary>
    /// <value>The end line number.</value>
    public int EndLineNumber
    {
      get { return LineNumber + LineCount - 1; }
      set { LineCount = EndLineNumber - LineNumber + 1; }
    }

    /// <summary>
    /// Gets or sets the start column
    /// </summary>
    public int Column
    {
      get { return (start >> 2) & COLMAX ; }
      set 
      {
        start &= ~COLMASK;
        start |= ((value << 2) & COLMASK);
      }
    }

    /// <summary>
    /// Gets or sets the end column
    /// </summary>
    public int EndColumn
    {
      get { return (end >> 2) & COLMAX; }
      set 
      {
        end &= ~COLMASK;
        end |= ((value << 2) & COLMASK);
      }
    }

    /// <summary>
    /// Gets the filename
    /// </summary>
    public string Filename
    {
      get {return filename;}
    }

    /// <summary>
    /// Gets or sets error condition
    /// </summary>
    public bool Error
    {
      get {return (start & ERROR) == ERROR;}
      set 
      {
        if (value)
        {
          start |= ERROR;
        }
        else
        {
          start &= ~ERROR;
        }
      }
    }

    /// <summary>
    /// Gets or sets warning condition
    /// </summary>
    public bool Warning
    {
      get {return (end & WARN) == WARN;}
      set 
      {
        if (value)
        {
          end |= WARN;
        }
        else
        {
          end &= ~WARN;
        }
      }
    }

    /// <summary>
    /// Gets or sets error condition
    /// </summary>
    public bool Disabled
    {
      get {return (start & DISABLED) == DISABLED;}
      set 
      {
        if (value)
        {
          start |= DISABLED;
        }
        else
        {
          start &= ~DISABLED;
        }
      }
    }

    /// <summary>
    /// Gets or sets warning condition
    /// </summary>
    public bool Paired
    {
      get {return (end & PAIRED) == PAIRED;}
      set 
      {
        if (value)
        {
          end |= PAIRED;
        }
        else
        {
          end &= ~PAIRED;
        }
      }
    }

    /// <summary>
    /// Tests equality of an object
    /// </summary>
    /// <param name="obj">the object to test</param>
    /// <returns>true if the sanm</returns>
    public override bool Equals(object obj)
    {
      Location b = obj as Location;
      if (b != null)
      {
        return b.start == start && b.end == end && b.filename == filename;
      }
      return false;
    }

    /// <summary>
    /// Gets the hashcode
    /// </summary>
    /// <returns>the hashcode</returns>
    public override int GetHashCode()
    {
      return start ^ end;
    }

    /// <summary>
    /// Tests not equality of two Locations
    /// </summary>
    /// <param name="a">first location</param>
    /// <param name="b">second location</param>
    /// <returns>true if not equal</returns>
    public static bool operator != (Location a, Location b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Tests equality of two Locations
    /// </summary>
    /// <param name="a">first location</param>
    /// <param name="b">second location</param>
    /// <returns>true if equal</returns>
    public static bool operator == (Location a, Location b)
    {
      if (object.ReferenceEquals(a,b))
      {
        return true;
      }
      if (object.ReferenceEquals(a,null))
      {
        return object.ReferenceEquals(b,null);
      }
      return a.Equals(b);
    }

    /// <summary>
    /// Determines whether the specified a is in.
    /// </summary>
    /// <param name="a">A.</param>
    /// <returns>
    /// 	<c>true</c> if the specified a is in; otherwise, <c>false</c>.
    /// </returns>
    public bool IsIn(Location a)
    {
      if (a == this)
      {
        return true;
      }
      return a.LineNumber <= LineNumber &&
        a.EndLineNumber >= EndLineNumber &&
        a.Column <= Column &&
        a.EndColumn >= EndColumn;
    }

    /// <summary>
    /// Compares the positions of two Locations
    /// </summary>
    /// <param name="a">first location</param>
    /// <param name="b">second location</param>
    /// <returns>true if smaller</returns>
    public static bool operator <(Location a, Location b)
    {
      if (a == b)
      {
        return false;
      }
      if (a.LineNumber == b.LineNumber)
      {
        return a.Column < b.Column;
      }
      else
      {
        return a.LineNumber < b.LineNumber;
      }
    }

    /// <summary>
    /// Compares the positions of two Locations
    /// </summary>
    /// <param name="a">first location</param>
    /// <param name="b">second location</param>
    /// <returns>true if larger</returns>
    public static bool operator >(Location a, Location b)
    {
      if (a == b)
      {
        return false;
      }
      if (a.LineNumber == b.LineNumber)
      {
        return a.Column > b.Column;
      }
      else
      {
        return a.LineNumber > b.LineNumber;
      }
    }

    /// <summary>
    /// Creates a range
    /// </summary>
    /// <param name="a">start</param>
    /// <param name="b">end</param>
    /// <returns>the new location</returns>
    public static Location operator +(Location a, Location b)
    {
      if (a == null && b == null)
      {
        return null;
      }
      else if (a == null)
      {
        return b.MemberwiseClone() as Location;
      }
      else if (b == null)
      {
        return a.MemberwiseClone() as Location;
      }
      else if (a == b)
      {
        return a;
      }
      int al = a.LineNumber;
      Location c = new Location(al, a.Column, b.LineNumber - al + b.LineCount, b.EndColumn);
      c.filename = a.filename ?? b.filename;
      c.Error = a.Error | b.Error;
      c.Warning = a.Warning | b.Warning;
      return c;
    }

#if DEBUG
    public int HashCode
    {
      get { return GetHashCode(); }
    }
#endif

    /// <summary>
    /// Get location in string form
    /// </summary>
    /// <returns>value</returns>
    public override string ToString()
    {
      return string.Format("{0}:({1}:{2},{3}:{4})", System.IO.Path.GetFileName(filename), 
        LineNumber, Column, LineNumber + LineCount, EndColumn);
    }
    #region IComparable Members

    int IComparable.CompareTo(object obj)
    {
      Location b = obj as Location;
      if (b == null)
      {
        return -1;
      }
      if (this < b)
      {
        return -1;
      }
      if (this > b)
      {
        return 1;
      }
      return 0;
    }

    internal int CompareTo(Location a)
    {
      return ((IComparable)this).CompareTo(a);
    }

    #endregion
  }

}
