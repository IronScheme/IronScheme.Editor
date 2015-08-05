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

namespace IronScheme.Editor.Collections
{
	class Set : IList
	{
		readonly Hashtable hset = new Hashtable();

		public Set()
		{
		}

		public Set(ICollection col)
		{
			AddRange(col);
		}

		public Set(params object[] values)
		{
			AddRange(values);
		}

		public void Add(object value) 
		{
			if (!Contains(value))
			{
				hset.Add(value, value);
			}
		}

		int IList.Add(object a)
		{
			Add(a);
			return Count - 1;
		}

		public void AddRange(ICollection values)
		{
			foreach (object o in values)
			{
				Add(o);
			}
		}

		public void AddRange(params object[] values)
		{
			foreach (object o in values)
			{
				Add(o);
			}
		}

    public object Replace(object o)
    {
      object old = hset[o];
      Remove(old);
      Add(o);
      return old;
    }

		public void CopyTo(Array array, int destindex)
		{
			ArrayList a = new ArrayList();
			foreach (object o in this)
			{
				a.Add(o);
			}
			a.CopyTo(array, destindex);
		}

		bool ICollection.IsSynchronized
		{
			get {return false;}
		}

		object ICollection.SyncRoot
		{
			get {return null;}
		}

		public void Clear()
		{
			hset.Clear();
		}

		bool IList.IsFixedSize
		{
			get {return false;}
		}

		bool IList.IsReadOnly
		{
			get {return false;}
		}

		public bool Contains(object value)
		{
			return hset.ContainsKey(value);
		}

		public Set Union(Set a)
		{
			return this | a;
		}

		public Set Intersect(Set a)
		{
			return this & a;
		}

		public Set Difference(Set a)
		{
			return this ^ a;
		}
		
		public static Set operator | (Set a, Set b)
		{
			Set u = new Set();
			foreach (object o in a)
			{
				u.Add(o);
			}
			foreach (object o in b)
			{
				u.Add(o);
			}
			return u;
		}

		public static Set operator ^ (Set a, Set b)
		{
			Set u = new Set();
			foreach (object o in a)
			{
				if (!b.Contains(o))
				{
					u.Add(o);
				}
			}
			foreach (object o in b)
			{
				if (!a.Contains(o))
				{
					u.Add(o);
				}
			}
			return u;
		}

		public static Set operator & (Set a, Set b)
		{
			Set u = new Set();
			foreach (object o in a)
			{
				if (b.Contains(o))
				{
					u.Add(o);
				}
			}
			return u;
		}

		public IEnumerator GetEnumerator()
		{
			return hset.Keys.GetEnumerator ();
		}

		int IList.IndexOf(object value)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int idx)
		{
			throw new NotSupportedException();
		}

		public int Count
		{
			get {return hset.Count;}
		}

		public void Remove(object value)
		{
			if (Contains(value))
			{
				hset.Remove(value);
			}
		}

		object IList.this[int idx]
		{
			get {throw new NotSupportedException();}
			set {throw new NotSupportedException();}
		}

		public bool this[params object[] obj]
		{
			get 
			{
				foreach (object o in obj)
				{
					if (!Contains(o))
					{
						return false;
					}
				}
				return true;
			}
			set 
			{
				if (value) 
				{
					AddRange(obj);
				}
				else
				{
					if (!this[obj])
					{
						RemoveRange(obj);
					}
				}
			}
		}

		public void RemoveRange(params object[] values)
		{
			foreach (object o in values)
			{
				Remove(o);
			}
		}

		public Array ToArray(Type itemtype)
		{
			ArrayList a = new ArrayList(this);
			return a.ToArray(itemtype);
		}

	}

}
