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

namespace Xacc.Collections
{
  /// <summary>
  /// An add only tree based on linked hashtables
  /// </summary>
  /// <remarks>This class is incomplete</remarks>
	class HashTree : ITree
	{
#if TESTME
		static void Main()
		{
			object val;
			HashTree h = new HashTree();

			h.Add("hello", 'h','e','l','l','o');
			h.Add("helpo", 'h','e','l','p','o');
			h.Add("bello", 'b','e','l','l','o');
			h.Add("hetlo", 'h','e','t','l','o');
			h.Add("hallo", 'h','a','l','l','o');
			h.Add("sello", 's','e','l','l','o');

			val = h['h','e','l','l','o'];
			val = h['h','e','l','p','o'];
			val = h['b','e','l','l','o'];
			val = h['h','e','t','l','o'];
			val = h['h','a','l','l','o'];
			val = h['s','e','l','l','o'];

			h['h','e','l','l','o'] = val;
			h['h','e','l','p','o'] = val;
			h['b','e','l','l','o'] = val;
			h['h','e','t','l','o'] = val;
			h['h','a','l','l','o'] = val;
			h['s','e','l','l','o'] = val;

			val = h['h','e','l','l','o'];
			val = h['h','e','l','p','o'];
			val = h['b','e','l','l','o'];
			val = h['h','e','t','l','o'];
			val = h['h','a','l','l','o'];
			val = h['s','e','l','l','o'];


		}
#endif

		readonly GrowingHashtable nodes = new GrowingHashtable();
		object value;
		int count = 0;

    /// <summary>
    /// Creates an instance of HashTree
    /// </summary>
		public HashTree()
		{

		}

		public ICollection Children
		{
			get {return nodes.Keys;}
		}

		public object ValueOf(object child)
		{
			return nodes[child];
		}

		public object Value
		{
			get {return value;}
		}

		int ChildCount
		{
			get {return nodes.Count;}
		}

		public HashTree GetSubHashTree(object key)
		{
			if (nodes.Contains(key))
			{
				return nodes[key] as HashTree;
			}
			return null;
		}

		public HashTree GetSubHashTree(Array key)
		{
			HashTree sub = this;
			foreach (object o in key)
			{
				if (sub.nodes.Contains(o))
				{
					sub = sub.nodes[o] as HashTree;
				}
				else
				{
					return null;
				}
			}
			return sub;
		}

		public bool IsInPath(Array key)
		{
			HashTree sub = this;
			foreach (object o in key)
			{
				if (sub.nodes.Contains(o))
				{
					sub = sub.nodes[o] as HashTree;
				}
				else
				{
					return false;
				}
			}
			return sub != null;
		}

		public void Add(object value, Array key)
		{
			HashTree cur = this;

			foreach (object o in key)
			{
				HashTree sub = cur.GetSubHashTree(o);
				if (sub == null)
				{
					cur.nodes.Add(o, (sub = new HashTree()));
				}
				cur = sub;
			}

			cur.value = value;
		}

		public bool Contains(Array key)
		{
			HashTree cur = this;

			foreach (object o in key)
			{
				HashTree sub = cur.GetSubHashTree(o);
				if (sub == null)
				{
					return false;
				}
				cur = sub;
			}

			if (cur != null && cur.value != null)
			{
				return true;
			}
			
			return false;
		}

		public object this[Array key]
		{
			get
			{
				HashTree sub = GetSubHashTree(key);
				return sub != null ? sub.value : null;
			}
			set
			{
				HashTree sub = GetSubHashTree(key);
				if (sub == null)
				{
					Add(value, key);
				}
				else
				{
					sub.value = value;
				}
			}
		}

		#region ICollection Members

		public bool IsSynchronized
		{
			get	{	return false;	}
		}

		public int Count
		{
			get	{ return count;}
		}

		public void CopyTo(Array array, int index)
		{
			// TODO:  Add StringHashTree.CopyTo implementation
		}

		public object SyncRoot
		{
			get	{	return null;}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			// TODO:  Add StringHashTree.System.Collections.IEnumerable.GetEnumerator implementation
			return null;
		}

		#endregion
	}
}
