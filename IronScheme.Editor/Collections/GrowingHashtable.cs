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
using System.Runtime.InteropServices;

namespace Xacc.Collections
{
  /// <summary>
  /// Growing Hashtable.
  /// </summary>
  class GrowingHashtable : IDictionary
  {
    struct bucket
    {
      internal object key;
      internal object val;
    }

    int size;
    bucket[] buckets;
	
    static readonly uint[] primes = 	{ //11,17,23,29,
      37,47,59,71,89,107,131,163,197,239,293,353,431,521,631,761,919,
      1103,1327,1597,1931,2333,2801,3371,4049,4861,5839,7013,8419,10103,12143,14591,
      17519,21023,25229,30293,36353,43627,52361,62851,75431,90523, 108631, 130363,
      156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
      968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287,
      4999559, 5999471, 7199369 };

    /// <summary>
    /// Creates an instances of GrowingHashtable
    /// </summary>
    public GrowingHashtable()
    {
      size = 0;
      buckets = new bucket[0];
    }

    static uint Hash(object key)
    { 
      return unchecked( (uint) key.GetHashCode());
    }

    uint GetCap()
    {
      uint i = 0, l = 0;
      for (;i < size;i++)
      {
        l += primes[i];
      }
      return l;
    }

    int AddBucketSet()
    {
      bucket[] newbuckets = new bucket[GetCap() + primes[size]];
      Array.Copy(buckets, newbuckets, buckets.Length);
      buckets = newbuckets;
      return ++size;
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="M:System.Collections.Hashtable.Add(System.Object,System.Object)"]/*'/>
    public void Add(object key, object value)
    {
      for (uint i = 0, pos = 0, hash = Hash(key); i < size; i++)
      {
        pos += hash % primes[i];
        if (buckets[pos].key == null)
        {
          //MUST assign by ref
          buckets[pos].key = key;
          buckets[pos].val = value;
          return;
        }
      }
      /* too small */
      AddBucketSet();

      /* just do it again */
      Add(key, value);
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="M:System.Collections.Hashtable.Contains(System.Object)"]/*'/>
    public bool Contains(object key)
    {
      for (uint i = 0, pos = 0, hash = Hash(key); i < size;i++)
      {
        pos += hash % primes[i];
        if (key.Equals(buckets[pos].key))
        {
          return true;
        }
      }
      return false;
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.Hashtable.Item(System.Object)"]/*'/>
    public object this[object key]
    {
      get 
      {
        for (uint i = 0, pos = 0, hash = Hash(key); i < size; i++)
        {
          pos += hash % primes[i];
          if (key.Equals(buckets[pos].key))
          {
            return buckets[pos].val;
          }
        }
        return null;
      }
      set 
      {
        if (!Contains(key))
        {
          Add(key, value);
        }
        else
        {
          for (uint i = 0, pos = 0, hash = Hash(key); i < size; i++)
          {
            pos += hash % primes[i];
            if (key.Equals(buckets[pos].key))
            {
              buckets[pos].val = value;
            }
          }
        }
      }
    }

    #region IDictionary Members

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.Hashtable.IsReadOnly"]/*'/>
    public bool IsReadOnly
    {
      get	{return false;}
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="M:System.Collections.Hashtable.GetEnumerator()"]/*'/>
    public IDictionaryEnumerator GetEnumerator()
    {
      ArrayList vals = new ArrayList(size);

      foreach (bucket b in buckets)
      {
        if (b.key != null)
        {
          vals.Add(new DictionaryEntry(b.key, b.val));
        }
      }

      DictionaryEntry[] entries = vals.ToArray(typeof(DictionaryEntry)) as DictionaryEntry[];
			
      //check me!!!
      return entries.GetEnumerator() as IDictionaryEnumerator;
    }

    void IDictionary.Remove(object key)
    {
      // TODO:  Add GrowingHashtable.Remove implementation
    }

    void IDictionary.Clear()
    {
      // TODO:  Add GrowingHashtable.Clear implementation
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.Hashtable.Values"]/*'/>
    public ICollection Values
    {
      get	
      {
        ArrayList vals = new ArrayList(size);

        foreach (bucket b in buckets)
        {
          if (b.key != null)
          {
            vals.Add(b.val);
          }
        }
				
        return vals;
      }
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.Hashtable.Keys"]/*'/>
    public ICollection Keys
    {
      get
      {
        ArrayList vals = new ArrayList(size);

        foreach (bucket b in buckets)
        {
          if (b.key != null)
          {
            vals.Add(b.key);
          }
        }
				
        return vals;
      }
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.Hashtable.IsFixedSize"]/*'/>
    public bool IsFixedSize
    {
      get	{return false;}
    }

    #endregion

    #region ICollection Members

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.ICollection.IsSynchronized"]/*'/>
    public bool IsSynchronized
    {
      get	{	return false;	}
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.ICollection.Count"]/*'/>
    public int Count
    {
      get	{	return size;	}
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)"]/*'/>
    public void CopyTo(Array array, int index)
    {
      // TODO:  Add GrowingHashtable.CopyTo implementation
    }

    ///<include file='C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.xml' 
    ///	path='doc/members/member[@name="P:System.Collections.ICollection.SyncRoot"]/*'/>
    public object SyncRoot
    {
      get	{	return null;}
    }

    #endregion

    #region IEnumerable Members

    IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}
