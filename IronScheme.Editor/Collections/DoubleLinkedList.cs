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
using System.Collections.Generic;
using System.Diagnostics;

namespace Xacc.Collections
{
  public sealed class FastDoubleLinkedList<T> : DoubleLinkedList<T> where T:class
  {
    
  }
	/// <summary>
	/// DoubleLinkedList.
	/// </summary>
	public class DoubleLinkedList<T> : ICollection<T>, ICloneable where T : class
	{
		/// <summary>
		/// Interface for position within the list
		/// </summary>
    public interface IPosition
		{
			/// <summary>
			/// Retrieves the data
			/// </summary>
      T Data { get;}

			/// <summary>
			/// The previous IPosition, or null, if it is the first position/sentinel
			/// </summary>
      IPosition Previous { get;}
			
			/// <summary>
			/// The next IPosition, or null, if it is the last position/sentinel
			/// </summary>
      IPosition Next { get;}
		}

    class Position : IPosition
		{
      internal T data;
      internal Position prev;
      internal Position next;

      public T Data { get { return data; } }
      public IPosition Previous { get { return prev; } }
      public IPosition Next { get { return next; } }

#if PROBE

      protected bool visited = false;
			public virtual bool Probe(Position caller)
			{
				if (visited)
				{
					Debug.Assert(caller.prev == this);
					visited = false;
					return prev.Probe(this);
				}
				else
				{
					Debug.Assert(caller.next == this);
					visited = true;
					return next.Probe(this);
				}
			}
#endif
		}

		#region Enumerators

		enum EnumerationFlags
		{
			Forward,
			Reverse,
			ForwardPosition,
			ReversePosition,
		}

    class Enumerator : IEnumerator<T>, ICloneable 
		{
      readonly DoubleLinkedList<T> dll;
			readonly int version;
      IPosition current;



      public Enumerator(DoubleLinkedList<T> dll)
			{
				this.dll = dll;
				this.version = dll.version;
				Reset();
			}

			public void Reset()
			{
				current = dll.startsentinel;
			}

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }
      void IDisposable.Dispose()
      {
      }


			public T Current
			{
				get	
				{	
					if (current == dll.startsentinel)
					{
						throw new InvalidOperationException("enumerator has not been initialized");
					}
					return current.Data;
				}
			}

			public bool MoveNext()
			{
				if (version != dll.version)
				{
					throw new InvalidOperationException("collection has been modified");
				}
				if (current.Next == dll.endsentinel)
				{
					return false;
				}
				else
				{
					current = current.Next;
					return true;
				}
			}
	
			public object Clone()
			{
				return MemberwiseClone();
			}
		}

    class ReverseEnumerator : IEnumerator<T>, ICloneable
		{
      readonly DoubleLinkedList<T> dll;
			readonly int version;
      IPosition current;

      public ReverseEnumerator(DoubleLinkedList<T> dll)
			{
				this.dll = dll;
				this.version = dll.version;
				Reset();
			}

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }
      void IDisposable.Dispose()
      {
      }

			public void Reset()
			{
				current = dll.endsentinel;
			}

      public T Current
			{
				get	
				{	
					if (current == dll.endsentinel)
					{
						throw new InvalidOperationException("enumerator has not been initialized");
					}
					return current.Data;
				}
			}

			public bool MoveNext()
			{
				if (version != dll.version)
				{
					throw new InvalidOperationException("collection has been modified");
				}
				if (current.Previous == dll.startsentinel)
				{
					return false;
				}
				else
				{
					current = current.Previous;
					return true;
				}
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

    class PostionEnumerator : IEnumerator<IPosition>, ICloneable
		{
      readonly DoubleLinkedList<T> dll;
			readonly int version;
      IPosition current;

      public PostionEnumerator(DoubleLinkedList<T> dll)
			{
				this.dll = dll;
				this.version = dll.version;
				Reset();
			}

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }
      void IDisposable.Dispose()
      {
      }

			public void Reset()
			{
				current = dll.startsentinel;
			}

      public IPosition Current
			{
				get	
				{	
					if (current == dll.startsentinel)
					{
						throw new InvalidOperationException("enumerator has not been initialized");
					}
					return current;
				}
			}

			public bool MoveNext()
			{
				if (version != dll.version)
				{
					throw new InvalidOperationException("collection has been modified");
				}
				if (current.Next == dll.endsentinel)
				{
					return false;
				}
				else
				{
					current = current.Next;
					return true;
				}
			}
	
			public object Clone()
			{
				return MemberwiseClone();
			}
		}

    class ReversePostionEnumerator : IEnumerator<IPosition>, ICloneable
		{
      readonly DoubleLinkedList<T> dll;
			readonly int version;
      IPosition current;

      public ReversePostionEnumerator(DoubleLinkedList<T> dll)
			{
				this.dll = dll;
				this.version = dll.version;
				Reset();
			}

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }
      void IDisposable.Dispose()
      {
      }

			public void Reset()
			{
				current = dll.endsentinel;
			}

      public IPosition Current
			{
				get	
				{	
					if (current == dll.endsentinel)
					{
						throw new InvalidOperationException("enumerator has not been initialized");
					}
					return current;
				}
			}

			public bool MoveNext()
			{
				if (version != dll.version)
				{
					throw new InvalidOperationException("collection has been modified");
				}
				if (current.Previous == dll.startsentinel)
				{
					return false;
				}
				else
				{
					current = current.Previous;
					return true;
				}
			}
	
			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		#endregion

    sealed class EndSentinel : Position
		{
#if PROBE
			public override bool Probe(Position caller)
			{
				Debug.Assert(caller.next == this);
				Debug.Assert(caller == prev);
				return prev.Probe(this);
			}
#endif
		}

    sealed class StartSentinel : Position
		{
#if PROBE
			public override bool Probe(Position caller)
			{
				if (visited)
				{
					Debug.Assert(caller.prev == this);
					visited = false;
					return true;
				}
				else
				{
					Debug.Assert(caller == null);
					visited = true;
					return next.Probe(this);
				}
			}
#endif
		}

#if PROBE
		internal void SendProbe()
		{
			Timers.HiPerfTimer hp = new Xacc.Timers.HiPerfTimer();
			hp.Start();
			Debug.Assert(startsentinel.Probe(null));
			hp.Stop();
//			Trace.WriteLine(string.Format("Probe returned successfully (hash: {1:X8} count: {0} time: {2:f0}ms)",
//				count, GetHashCode(), hp.Duration));
		}
#endif

    /// <summary>
    /// Gets a synchronized wrapper for a list
    /// </summary>
    /// <param name="list">the list to wrap</param>
    /// <returns>the synchronized list</returns>
		public static DoubleLinkedList<T> Syncronized(DoubleLinkedList<T> list)
		{
			return new SyncDoubleLinkList(list);
		}

		readonly Position startsentinel = new StartSentinel();
		readonly Position endsentinel		= new EndSentinel();

		readonly object synclock = new object();
    Type keytype;

		int count;	
		int version;

		EnumerationFlags flags = 0;

    readonly Dictionary<T, IPosition> reversemapping = new Dictionary<T, IPosition>();

		/// <summary>
		/// The position of the first element, or null, if list is empty
		/// </summary>
		public virtual IPosition First
		{
			get{return startsentinel.next != endsentinel ? startsentinel.next : null;}
		}

		/// <summary>
		/// The position of the last element, or null, if list is empty
		/// </summary>
		public virtual IPosition Last
		{
			get{return endsentinel.prev != startsentinel ? endsentinel.prev : null;}
		}


		/// <summary>
		/// Creates an empty list
		/// </summary>
		public DoubleLinkedList()
		{
			startsentinel.next = endsentinel;
			endsentinel.prev = startsentinel;

			count = 0;
			version = 0;
		}

		/// <summary>
		/// Creates a list from an ICollection
		/// </summary>
		/// <param name="icol">The collections elements to copy from</param>
		public DoubleLinkedList(ICollection<T> icol) : this()
		{
			foreach (T o in icol)
			{
				Add(o);
			}
		}

		/// <summary>
		/// Returns the position of an object
		/// </summary>
		/// <param name="obj">the object to lookup</param>
		/// <returns>the postion containing the object, or null, if not found</returns>
		public virtual IPosition PositionOf(T obj)
		{
			return reversemapping[obj];
		}

    void ICollection<T>.Add(T obj)
    {
      Add(obj);
    }

		/// <summary>
		/// Adds an object to the end of the list
		/// </summary>
		/// <param name="obj">the object to add</param>
		/// <returns>the position of the added object</returns>
    public virtual IPosition Add(T obj)
		{
			version++;
			count++;

      Position p = new Position();
			p.prev = endsentinel.prev;
			p.next = endsentinel;
			p.prev.next = p;
			endsentinel.prev = p;
			p.data = obj;

      Type t = obj.GetType();

      if (keytype == null)
      {
        keytype = t;
      }
      else if (keytype != t)
      {
        Debug.Fail("Invalid condition");
      }

			reversemapping.Add(obj, p);

			return p;
		}

		/// <summary>
		/// Clears the list
		/// </summary>
		public virtual void Clear()
		{
			startsentinel.next = endsentinel;
			endsentinel.prev = startsentinel;

			count = 0;
			version = 0;

			reversemapping.Clear();
		}

		/// <summary>
		/// Checks whether the list contains an object
		/// </summary>
		/// <param name="obj">the object to search for</param>
		/// <returns>true if found, else false</returns>
		public virtual bool Contains(T obj)
		{
			return reversemapping.ContainsKey(obj);
		}

		/// <summary>
		/// Returns the index of an object, or -1 if not found
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual int IndexOf(T obj)
		{
			int counter = 0;
			foreach (T o in this)
			{
				if (o == obj)
				{
					return counter;
				}
				counter++;
			}
			return -1;
		}

    /// <summary>
    /// Inserts a value after a IPosition
    /// </summary>
    /// <param name="before">the before pos</param>
    /// <param name="value">the value</param>
    public virtual void InsertAfter(IPosition before, T value)
    {
      Position newp = new Position();
      newp.data = value;

      (before.Next as Position).prev = newp;
      newp.next = (before.Next as Position);
      (before as Position).next = newp;
      newp.prev = (before as Position);

      count++;
      version++;

      Type t = value.GetType();

      if (keytype == null)
      {
        keytype = t;
      }
      else if (keytype != t)
      {
        Debug.Fail("Invalid condition");
      }
      reversemapping.Add(value, newp);
    }

    /// <summary>
    /// Inserts a value before a IPosition
    /// </summary>
    /// <param name="after">the after pos</param>
    /// <param name="value">the value</param>
    public virtual void InsertBefore(IPosition after, T value)
		{
			InsertAfter(after.Previous, value);
		}


		/// <summary>
		/// Inserts an object at a specified index
		/// </summary>
		/// <param name="index">the index to insert the object</param>
		/// <param name="obj">the object to insert</param>
		public virtual void Insert(int index, T obj)
		{
			if (index*2 > count)
			{
				flags = EnumerationFlags.ReversePosition;
				index = count - index - 1;
			}
			else
			{
				flags = EnumerationFlags.ForwardPosition;
			}
      foreach (Position pos in this as System.Collections.ICollection)
			{
				if (index-- == 0)
				{
					version++;
					count++;

          Position p = new Position();
					p.prev = pos;
					p.next = pos.next;
					p.prev.next = p;
					pos.prev = p;
					p.data = obj;
					
          Type t = obj.GetType();

          if (keytype == null)
          {
            keytype = t;
          }
          else if (keytype != t)
          {
            Debug.Fail("Invalid condition");
          }
          reversemapping.Add(obj, p);

					return;
				}
			}
		}

		/// <summary>
		/// Removes a object at a specified index
		/// </summary>
		/// <param name="index">the index to remove</param>
		public virtual void RemoveAt(int index)
		{
			if (index*2 > count)
			{
				flags = EnumerationFlags.ReversePosition;
				index = count - index - 1;
			}
			else
			{
				flags = EnumerationFlags.ForwardPosition;
			}
      foreach (IPosition pos in this as System.Collections.ICollection)
			{
				if (index-- == 0)
				{
					Remove(pos);													
					return;
				}
			}
		}

		/// <summary>
		/// Removes an object or position from the list
		/// </summary>
		/// <param name="position">the object or position to remove</param>
		/// <remarks>You can either pass the position or the object to remove</remarks>
    public virtual bool Remove(T position)
		{
			return Remove(reversemapping[position]) != null;
		}

		/// <summary>
		/// Returns false
		/// </summary>
		public virtual bool IsFixedSize
		{
			get {return false;}
		}

		/// <summary>
		/// Returns false
		/// </summary>
		public virtual bool IsReadOnly
		{
			get {return false;}
		}

    object Remove(IPosition pos)
		{
			if (pos == null)
			{
				return null;
			}
			version++;
			count--;

      Position p = pos as Position;

			p.prev.next = p.next;
			p.next.prev = p.prev;

  		reversemapping.Remove(p.data);

			return p.data;
		}

		/// <summary>
		/// Copies the objects from the list to an array
		/// </summary>
		/// <param name="dest">the destination array</param>
		/// <param name="index">the index to start copying at</param>
		public virtual void CopyTo(T[] dest, int index)
		{
			if (dest.GetLength(0) < index + Count)
			{
				throw new ArgumentOutOfRangeException("index", index, "Not enough space in destination array");
			}
			int counter = 0;
			foreach (T obj in this)
			{
				dest.SetValue(obj, counter++ + index);
			}
		}

		/// <summary>
		/// Return false
		/// </summary>
		public virtual bool IsSynchronized
		{
			get {return false;}
		}

    /// <summary>
    /// The syncroot
    /// </summary>
		public virtual object SyncRoot
		{
			get{return synclock;}
		}

		/// <summary>
		/// Returns the number of objects in the list
		/// </summary>
		public virtual int Count
		{
			get {return count;}
		}

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      System.Collections.IEnumerator rator = null;
      switch (flags)
      {
        case EnumerationFlags.Forward:
          rator = new Enumerator(this);
          break;
        case EnumerationFlags.Reverse:
          rator = new ReverseEnumerator(this);
          break;
        case EnumerationFlags.ForwardPosition:
          rator = new PostionEnumerator(this);
          break;
        case EnumerationFlags.ReversePosition:
          rator = new ReversePostionEnumerator(this);
          break;
        default:
          rator = new Enumerator(this);
          break;
      }
      flags = 0;
      return rator;
    }

		/// <summary>
		/// Returns an enumerator for the list
		/// </summary>
		/// <returns>the enumerator</returns>
		public virtual IEnumerator<T> GetEnumerator()
		{
			IEnumerator<T> rator = null;
			switch (flags)
			{
				case EnumerationFlags.Forward:
          rator = new Enumerator(this);
					break;
				case EnumerationFlags.Reverse:
          rator = new ReverseEnumerator(this);
					break;
				default:
          rator = new Enumerator(this);
					break;
			}
			flags = 0;
			return rator;
		}

		/// <summary>
		/// Creates a deep copy of the list
		/// </summary>
		/// <returns></returns>
		public virtual object Clone()
		{
      return new DoubleLinkedList<T>(this); 
		}


    sealed class SyncDoubleLinkList : DoubleLinkedList<T>
		{
      readonly DoubleLinkedList<T> list;

      public SyncDoubleLinkList(DoubleLinkedList<T> list)
			{
				if (list.IsSynchronized)
				{
					throw new ArgumentException("list already synced", "list");
				}
				this.list = list;
			}

      public override IPosition Add(T obj)
			{
				lock(SyncRoot){	return list.Add (obj);}
			}

			public override void Clear()
			{
				lock(SyncRoot){list.Clear ();}
			}

			public override object Clone()
			{
				lock(SyncRoot){	return list.Clone ();	}
			}

			public override bool Contains(T obj)
			{
				lock(SyncRoot){	return list.Contains (obj);	}
			}

			public override void CopyTo(T[] dest, int index)
			{
				lock(SyncRoot){	list.CopyTo (dest, index);}
			}

			public override int Count
			{
				get	{lock(SyncRoot){return list.Count;}}
			}

      public override IPosition First
			{
				get {lock(SyncRoot){	return list.First;}}
			}

      public override IEnumerator<T> GetEnumerator()
			{
				lock(SyncRoot){return list.GetEnumerator ();}
			}

			public override int IndexOf(T obj)
			{
				lock(SyncRoot){return list.IndexOf (obj);}
			}

			public override void Insert(int index, T obj)
			{
				lock(SyncRoot){list.Insert (index, obj);}
			}

      public override void InsertAfter(IPosition before, T value)
			{
				lock(SyncRoot){list.InsertAfter (before, value);}
			}

      public override void InsertBefore(IPosition after, T value)
			{
				lock(SyncRoot){list.InsertBefore (after, value);}
			}

			public override bool IsFixedSize
			{
				get	{	lock(SyncRoot){return list.IsFixedSize;}}
			}

			public override bool IsReadOnly
			{
				get	{	lock(SyncRoot){return list.IsReadOnly;}	}
			}

			public override bool IsSynchronized
			{
				get	{	return true;}
			}

      public override IPosition Last
			{
				get	{	lock(SyncRoot){return list.Last;}	}
			}

      public override IPosition PositionOf(T obj)
			{
				lock(SyncRoot){return list.PositionOf (obj);}
			}

			public override bool Remove(T position)
			{
				lock(SyncRoot){return list.Remove (position);}
			}

			public override void RemoveAt(int index)
			{
				lock(SyncRoot){list.RemoveAt (index);}
			}

			public override object SyncRoot
			{
				get	{return list.SyncRoot;}
			}
		}
	}
}
