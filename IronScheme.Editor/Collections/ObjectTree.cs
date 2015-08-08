#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Collections;

namespace IronScheme.Editor.Collections
{
	/// <summary>
	/// Summary description for StringTree.
	/// </summary>
  [Serializable]
	class ObjectTree
	{
		string key;
		object accept;
		ObjectTree child;
		ObjectTree next;

    [NonSerialized]
		BakeInfo bakeinfo = null;

		object Value
		{
			get {return accept;}
		}

		class BakeInfo
		{
			public readonly int childcount;
			public readonly int statecount;
			public readonly int acceptcount;

			public BakeInfo(int childcount, int statecount, int acceptcount)
			{
				this.childcount		= childcount;
				this.statecount		= statecount;
				this.acceptcount	= acceptcount;
			}
		}

		public ObjectTree() : this(null){}

		ObjectTree(string key)
		{ 
			this.key = key;
			accept = null; 
			child = null; 
			next = null;
		}

		bool IsRoot
		{
			get {return key == null;}
		}

		bool IsBaked
		{
			get {return bakeinfo != null;}
		}

		public bool IsAccepting
		{
			get {return accept != null;}
		}

		ObjectTree GetLastNext()
		{
			ObjectTree last = this;

      while (last.next != null)
      {
        last = last.next;
      }

			return last;
		}

		int ChildCount
		{
			get 
			{
				if (IsBaked)
				{
					return bakeinfo.childcount;
				}
				else
				{
					ObjectTree s = child;
					int count = 0;
					while (s != null)
					{
						count++;
						s = s.next;
					}
					return count;
				}
			}
		}

		ObjectTree MatchKey(string key)
		{
			ObjectTree currentnode = this;
			while (currentnode != null)
			{
        if (currentnode.key != null && key != null)
        {
          if (currentnode.key.ToLower() == key.ToLower())
          {
            return currentnode;
          }
        }
				currentnode = currentnode.next;
			}
			return null;
		}

		ObjectTree GetObjectTreeAt(string[] stack)
		{
      if (stack == null) 
      {
        return null;
      }

      if (stack.Length == 0) 
      {
        return this;
      }

			ObjectTree parent = this;
			int counter = 0;
			do
			{
				ObjectTree i = parent.MatchKey(stack[counter++]);
        if (null == i)
        {
          return null;
        }
        if (null == i.child && counter < stack.Length)
        {
          return null;
        }
        if (counter == stack.Length)
        {
          return i;
        }
				parent = i.child;
			}
			while (true);
		}

		bool AddStackToObjectTree(string[] stack, ObjectTree parent, 
			ObjectTree prev, int counter, object accepts)
		{
			do
			{  
				if (counter == stack.Length)
				{
					parent.accept = accepts;
					return true;
				}
				else 
				{
					ObjectTree pnode = new ObjectTree(stack[counter]);
 
          if (null != parent)
          {
            parent.child = pnode;
          }
					if (null != prev)
					{
						prev.GetLastNext().next = pnode;
						prev = null;
					}
					parent = pnode;
				}
			}
			while (counter++ < stack.Length);
			return true;
		}

		string[][] Match(string[] stack)
		{
			ArrayList arr = new ArrayList();
			string[] ostack = stack.Clone() as string[];
			
			MatchWild(ostack, ostack, arr);
			
			return arr.ToArray(typeof(string[])) as string[][];
		}

		void MatchWild(string[] stack, string[] stacktemp, ArrayList results)
		{
			ObjectTree pn = null;
			int i = 0;
			for (; i < stacktemp.Length; i++)
			{
        if (stacktemp[i] == null) 
        {
          break;
        }
			}

			string[] substack = new string[stacktemp.Length - i];

      for (int j = 0; j < substack.Length; j++)
      {
        substack[j] = stacktemp[i + j];
      }

			if (substack.Length > 0)
			{
				pn = GetObjectTreeAt(stacktemp);

        if (pn != null)
        {
          pn = pn.child;
        }
        else if (i == 0)
        {
          pn = this;
        }

				while (pn != null)
				{
					substack[0] = pn.key;
					//this one
					stack[i] = pn.key;

					pn.MatchWild(stack, substack, results);    
					
					pn = pn.next;
				}
				substack[0] = null;
			}
			else
			{
				pn = GetObjectTreeAt(stacktemp);
        if (pn != null)
        {
          if (pn.IsAccepting != false)
          {
            results.Add(stack);
          }
        }
			}
		}

		public bool Remove(string[] stack)
		{
			return Add(stack, null);
		}

		public bool Add(string[] stack, object accepts)
		{
			if (IsBaked)
			{
				throw new Exception("Cannot mould a baked tree");
			}

      if (stack.Length == 0) 
      {
        return false;
      }
         
			ObjectTree parent = this;

			int counter = 0;
			do
			{
				ObjectTree i = parent.MatchKey(stack[counter++]);
        if (null == i)
        {
          return AddStackToObjectTree(stack, null, parent, --counter, accepts);
        }
        if (null == i.child && counter < stack.Length)
        {
          return AddStackToObjectTree(stack, i, null, counter, accepts);
        }
        if (counter == stack.Length)
        {
          return AddStackToObjectTree(stack, i, null, counter, accepts);
        }
				parent = i.child;
			}
			while (true);
		}

		public object Accepts(string[] stack)
		{
			ObjectTree i = GetObjectTreeAt(stack);
			if (null != i)
			{
				return i.accept;
			}
			return null;
		}

		internal void Bake()
		{
			if (next != null)
			{
				next.Bake();
			}
			if (child != null)
			{
				child.Bake();
			}

			int cc = ChildCount;
			int tc = TotalStateCount;
			int ac = AcceptStateCount;

			bakeinfo = new BakeInfo(cc,tc,ac);
		}

		public int AcceptStateCount
		{
			get 
			{
				if (IsBaked)
				{
					return bakeinfo.acceptcount;
				}
				else
				{
					int size = 0;
					if (child != null)
					{
						size += child.AcceptStateCount;
						if (child.next != null)
						{
							size += child.next.AcceptStateCount;
						}
					}
					if (IsAccepting)
					{
						size++;
					}
					return size;
				}
			}
		}

		public int TotalStateCount
		{
			get 
			{
				if (IsBaked)
				{
					return bakeinfo.statecount;
				}
				else
				{
					int size = 0;
          if (next != null)
          {
            size += next.TotalStateCount;
          }
          if (child != null)
          {
            size += child.TotalStateCount;
          }
					size++;
					return size;
				}
			}
		}
 
		bool MatchItem(string[] buffer, ref int index, int pos, int size, string[][] results)
		{
			buffer[pos] = key;

			if (accept != null && index != 0)
			{
        string[] newbuf = new string[pos + 1];
        Array.Copy(buffer, newbuf, pos + 1);
				results[size - index--] = newbuf;
			}

      if (index >= 0 & child != null)
      {
        if (child.MatchItem(buffer, ref index, pos + 1, size, results))
        {
          return true;
        }
      }

      if (index >= 0 & next != null)
      {
        if (next.MatchItem(buffer, ref index, pos, size, results))
        {
          return true;
        }
      }
        
			return false;
		}

    public string[] CompleteFirstStates(string[] stack)
    {
      string[] substack = new string[stack.Length - 1];
      Array.Copy(stack, substack, substack.Length);
      ObjectTree pnode = GetObjectTreeAt(substack);

      if (substack.Length == 0)
      {
        pnode = this;
      }
      else
      {
        if (pnode == null || pnode.child == null)
        {
          return new string[0];
        }
        else
        {
          pnode = pnode.child;
        }
      }

      Set allres = new Set();

      while (pnode != null)
      {
        string r = pnode.key;

        if (r != null && r.Length > 0 && r.ToLower().StartsWith(stack[stack.Length - 1].ToLower()))
        {
          allres.Add(r);
        }

        pnode = pnode.next;
      }

      return allres.ToArray(typeof(string)) as string[];
    }

    public string[][] CompleteStates(string[] stack)
    {
      string[] substack = new string[stack.Length - 1];
      Array.Copy(stack, substack, substack.Length);
      ObjectTree pnode = GetObjectTreeAt(substack);

      if (substack.Length == 0)
      {
        pnode = this;
      }
      else
      {
        if (pnode == null || pnode.child == null)
        {
          return new string[0][];
        }
        else
        {
          pnode = pnode.child;
        }
      }

      Set allres = new Set();

      while (pnode != null)
      {
        int index = pnode.AcceptStateCount;
        int size = index;

        string[] buffer = new string[pnode.Depth(2)];
        string[][] results = new string[size][];

        pnode.MatchItem(buffer, ref index, 0, size, results);
        
        foreach (string[] r in results)
        {
          if (r.Length > 0 && r[0].ToLower().StartsWith(stack[stack.Length - 1].ToLower()))
          {
            allres.Add(r);
          }
        }

        pnode = pnode.next;
      }

      return allres.ToArray(typeof(string[])) as string[][];
    }

    public string[][] AcceptFirstStates(string[] stack)
    {
      ObjectTree pnode = GetObjectTreeAt(stack);

      if (stack == null || stack.Length == 0)
      {
        pnode = this;
      }
      else
      {
        if (pnode == null || pnode.child == null)
        {
          return new string[0][];
        }
        else
        {
          pnode = pnode.child;
        }
      }

      ArrayList allres = new ArrayList();

      while (pnode != null)
      {
        string r = pnode.key;
        string[] result = new string[stack.Length + 1];
        Array.Copy(stack, result, stack.Length);
        result[stack.Length] = r;
        allres.Add(result);
        pnode = pnode.next;
      }

      return allres.ToArray(typeof(string[])) as string[][];
    }


		public string[][] AcceptStates(string[] stack)
		{
			ObjectTree pnode = GetObjectTreeAt(stack);

      if (stack == null || stack.Length == 0)
      {
        pnode = this;
      }
      else
      {
        if (pnode == null || pnode.child == null)
        {
          return new string[0][];
        }
        else
        {
          pnode = pnode.child;
        }
      }

			ArrayList allres = new ArrayList();

			while (pnode != null)
			{
				int index = pnode.AcceptStateCount;
				int size = index;

				string[] buffer = new string[pnode.Depth(2)];
				string[][] results = new string[size][];

				pnode.MatchItem(buffer, ref index, 0, size, results);

				allres.AddRange(results);

				pnode = pnode.next;
			}

			return allres.ToArray(typeof(string[])) as string[][];
		}

		int Depth(int depth)
		{
			int c = 0, n = 0;
      if (child != null)
      {
        c = child.Depth(depth + 1);
      }
      if (next != null)
      {
        n = next.Depth(depth);
      }
      if (c > depth)
      {
        depth = c;
      }
      if (n > depth)
      {
        depth = n;
      }

			return depth;
		}
	}
}
