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
using System.Text;

namespace Xacc.Collections
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

