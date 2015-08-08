#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion


using System;
using System.Collections;
using System.Text;

namespace IronScheme.Editor.Collections
{
	class TrackingStack : CollectionBase
	{
		struct Holder
		{
			internal int level;
			internal object data;
			internal bool up;
			internal bool single;

			public Holder(int level, object data)
			{
				this.level = level;
				this.data = data;
				up = true;
				single = false;
			}
		}
		
		int level = 0, pos = 0;

		public TrackingStack()
		{
		}

		protected override void OnClear()
		{
			base.OnClear ();
			level = 0;
			pos = 0;
		}

		/// <summary>
		/// Adds an object to the stack
		/// </summary>
		/// <param name="o">the object to add</param>
		/// <returns>a tracking id</returns>
		public int Enter(object o)
		{
			Holder h = new Holder( ++level, o);
			int r = pos;
			if (Count > pos)
				List[pos] = h;
			else
				List.Add( h );
			pos++;
			return r;
		}

		/// <summary>
		/// Adds an object and accepts it immediately, iow cannot be tracked
		/// </summary>
		/// <param name="o">the object to add</param>
		public void EnterAccept(object o)
		{
			Holder h = new Holder(level + 1, o);
			h.single = true;
			if (Count > pos)
				List[pos] = h;
			else
				List.Add( h );
			pos++;
		}

		/// <summary>
		/// Rejects an object, removes everything after and including itself
		/// </summary>
		/// <param name="token">the tracking id</param>
		public void Reject(int token)
		{
			level = ((Holder) List[token]).level - 1;
			InnerList.RemoveRange(token, Count - token);
			pos = token;
		}

		/// <summary>
		/// Accepts a previously entered object
		/// </summary>
		/// <param name="token">the tracking id</param>
		public void Accept(int token)
		{
			Holder h = (Holder)List[token];
			h.up = false;
			if (Count > pos)
				List[pos] = h;
			else
				List.Add(h);
			pos++;
			level--;			
		}

		public object this[int index]
		{
			get {return ((Holder)List[index]).data;}
		}

		class Enumerator :IEnumerator
		{
			TrackingStack s;
			int index = -1;
	
			public Enumerator(TrackingStack s)
			{
				this.s = s;
			}
	
			public void Reset()
			{
				index = -1;
			}

			public object Current
			{
				get
				{
					return s[index];
				}
			}

			public bool MoveNext()
			{
				if (++index >= s.Count)
					return false;
				return true;
			}

		}

		public new IEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
	}
}

