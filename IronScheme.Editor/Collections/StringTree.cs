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
	/// <summary>
	/// Summary description for StringTree.
	/// </summary>
  [Serializable]
	class StringTree
	{
		char key;
		object accept;
		internal StringTree child;
		StringTree next;

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

		public StringTree() : this((char)0){}

		StringTree(char key)
		{ 
			this.key = key;
			accept = null; 
			child = null; 
			next = null;
		}

		bool IsRoot
		{
			get {return key == '\0';}
		}

		bool IsBaked
		{
			get {return bakeinfo != null;}
		}

		public bool IsAccepting
		{
			get {return accept != null;}
		}

		StringTree GetLastNext()
		{
			StringTree last = this;

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
					StringTree s = child;
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

		StringTree MatchKey(char key)
		{
			StringTree currentnode = this;
			while (currentnode != null)
			{
        if (currentnode.key == key)
        {
          return currentnode;
        }
				currentnode = currentnode.next;
			}
			return null;
		}

		StringTree GetStringTreeAt(char[] stack)
		{
      if (stack == null) 
      {
        return null;
      }

      if (stack.Length == 0) 
      {
        return this;
      }

			StringTree parent = this;
			int counter = 0;
			do
			{
				StringTree i = parent.MatchKey(stack[counter++]);
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

		bool AddStackToStringTree(char[] stack, StringTree parent, 
			StringTree prev, int counter, object accepts)
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
					StringTree pnode = new StringTree(stack[counter]);
 
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

    public string[] Match(string stack)
    {
      return Match(stack.ToCharArray());
    }

		string[] Match(char[] stack)
		{
			ArrayList arr = new ArrayList();
			char[] ostack = stack.Clone() as char[];
			
			MatchWild(ostack, ostack, arr);
			
			return arr.ToArray(typeof(string)) as string[];
		}

		void MatchWild(char[] stack, char[] stacktemp, ArrayList results)
		{
			StringTree pn = null;
			int i = 0;
			for (; i < stacktemp.Length; i++)
			{
        if (stacktemp[i] == '?') 
        {
          break;
        }
			}

			char[] substack = new char[stacktemp.Length - i];

      for (int j = 0; j < substack.Length; j++)
      {
        substack[j] = stacktemp[i + j];
      }

			if (substack.Length > 0)
			{
				pn = GetStringTreeAt(stacktemp);

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
				substack[0] = '\0';
			}
			else
			{
				pn = GetStringTreeAt(stacktemp);
        if (pn != null)
        {
          if (pn.IsAccepting != false)
          {
            results.Add(stack);
          }
        }
			}
		}

    public bool Remove(string stack)
    {
      return Add(stack.ToCharArray(), null);
    }

		bool Remove(char[] stack)
		{
			return Add(stack, null);
		}

    public bool Add(string stack, object value)
    {
      return Add(stack.ToCharArray(), value);
    }

		bool Add(char[] stack, object accepts)
		{
			if (IsBaked)
			{
				throw new Exception("Cannot mould a baked tree");
			}

      if (stack.Length == 0) 
      {
        return false;
      }
         
			StringTree parent = this;

			int counter = 0;
			do
			{
				StringTree i = parent.MatchKey(stack[counter++]);
        if (null == i)
        {
          return AddStackToStringTree(stack, null, parent, --counter, accepts);
        }
        if (null == i.child && counter < stack.Length)
        {
          return AddStackToStringTree(stack, i, null, counter, accepts);
        }
        if (counter == stack.Length)
        {
          return AddStackToStringTree(stack, i, null, counter, accepts);
        }
				parent = i.child;
			}
			while (true);
		}

    public object this[string stack]
    {
      get { return Accepts(stack.ToCharArray()); }
    }

		object Accepts(char[] stack)
		{
			StringTree i = GetStringTreeAt(stack);
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
 
		bool MatchItem(char[] buffer, ref int index, int pos, int size, string[] results)
		{
			buffer[pos] = key;

			if (accept != null && index != 0)
			{
				buffer[pos + 1] = '\0';
				//o my lord! new string(char[]) doesnt stop at null char!!!!
				results[size - index--] = new string(buffer,0, pos + 1);
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

    public string[] AcceptStates(string stack)
    {
      return AcceptStates(stack.ToCharArray());
    }

		string[] AcceptStates(char[] stack)
		{
			StringTree pnode = GetStringTreeAt(stack);

      if (stack == null || stack.Length == 0)
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

			ArrayList allres = new ArrayList();

			while (pnode != null)
			{
				int index = pnode.AcceptStateCount;
				int size = index;

				char[] buffer = new char[pnode.Depth(2)];
				string[] results = new string[size];

				pnode.MatchItem(buffer, ref index, 0, size, results);

				allres.AddRange(results);

				pnode = pnode.next;
			}

			return allres.ToArray(typeof(string)) as string[];
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
