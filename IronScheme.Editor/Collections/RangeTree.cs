#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IronScheme.Editor.Collections
{
#if TESTS
  class Duration : IRange<DateTime>
  {
    DateTime start, finish;

    public Duration(DateTime starttime, DateTime finishtime)
    {
      start = starttime;
      finish = finishtime;
    }


    public DateTime Start
    {
      get { return start; }
    }

    public DateTime End
    {
      get { return finish; }
    }

    public override string ToString()
    {
      return string.Format("{0} - {1}", start, finish);
    }



  }

  class Log
  {
    static readonly RangedTree<DateTime, string> events = new RangedTree<DateTime, string>();

    public static void Event(DateTime starttime, DateTime finishtime, string desc)
    {
      Console.WriteLine("Completed: {0} Duration: {1}ms", desc, (finishtime - starttime).TotalMilliseconds);
      events.Add(new Duration(starttime, finishtime), desc);
    }

    static void LogEvent(Duration duration, string desc)
    {
      events.Add(duration, desc);
    }
  }

  class Loader
  {
    readonly static Random RANDOM = new Random();

    string name;

    public Loader(string name)
    {
      this.name = name;
    }

    public void Load()
    {
      DateTime start = DateTime.Now;
      Thread.Sleep(RANDOM.Next(0, 100));
      foreach (Loader child in children)
      {
        child.Load();
      }
      Thread.Sleep(RANDOM.Next(100, 1000));
      DateTime finish = DateTime.Now;

      Log.Event(start, finish, name);
    }

    readonly List<Loader> children = new List<Loader>();

    public List<Loader> Children
    {
      get { return children; }
    } 

  }

  class Test
  {
    static void Main(string[] args)
    {
      Loader root = new Loader("root");
      Loader child = new Loader("childroot1");
      root.Children.Add(child);
      root.Children.Add(new Loader("childroot2"));
      root.Children.Add(new Loader("childroot3"));

      child.Children.Add(new Loader("child1"));
      child.Children.Add(new Loader("child2"));


      root.Load();


      Console.WriteLine();


    }
  }



  class Foo : IRange<int>
  {
    int start, end;

    //static void Main(string[] args)
    //{
    //  RangedTree<int, object> tree = new RangedTree<int, object>();

    //  List<Foo> foos = new List<Foo>();



    //  foos.Add(new Foo(0, 100));
    //  foos.Add(new Foo(1, 100));
    //  //foos.Add(new Foo(0, 99));
    //  foos.Add(new Foo(2, 21));
    //  foos.Add(new Foo(3, 10));
    //  foos.Add(new Foo(4, 5));
    //  foos.Add(new Foo(6, 10));
    //  foos.Add(new Foo(11, 20));
    //  foos.Add(new Foo(12, 19));
    //  foos.Add(new Foo(13, 18));
    //  foos.Add(new Foo(23, 31));
    //  foos.Add(new Foo(24, 26));
    //  foos.Add(new Foo(25, 25));
    //  foos.Add(new Foo(63, 65));
    //  foos.Add(new Foo(72, 83));
    //  foos.Add(new Foo(74, 79));
    //  foos.Add(new Foo(84, 90));
    //  foos.Add(new Foo(91, 97));

    //  List<Foo> foos2 = new List<Foo>(foos); 


    //  Random r = new Random();
    //  int c = 100;
      
    //  while (c > 0)
    //  {
    //    tree.Clear();
    //    foos = new List<Foo>(foos2);

    //    //add test
    //    while (foos.Count > 0)
    //    {
    //      int i = r.Next(0, foos.Count - 1);
    //      IRange<int> ir = foos[i];
    //      foos.RemoveAt(i);
    //      tree.Add(ir, ir.ToString());
    //    }

    //    Debug.Assert(tree.Count == foos2.Count);

    //    int j = 0;

    //    // iterator test
    //    foreach (KeyValuePair<IRange<int>, object> kvp in tree)
    //    {

    //      foos.Add(kvp.Key as Foo);
    //      //Console.WriteLine("K: {0} V: {1}", kvp.Key, kvp.Value);
    //      Debug.Assert(foos2[j] == kvp.Key);
    //      j++;
    //    }

    //    // remove test
    //    while (foos.Count > 0)
    //    {
    //      int i = r.Next(0, foos.Count - 1);
    //      IRange<int> ir = foos[i];
    //      foos.RemoveAt(i);
    //      bool res = tree.Remove(ir);
    //      Debug.Assert(res);
    //    }

    //    Debug.Assert(tree.Count == 0);


    //    c--;
    //  }

    //  Console.WriteLine();

    //}

    public Foo(int start, int end)
    {
      this.start = start;
      this.end = end;
    }

    public int Start
    {
      get { return start; }
    }

    public int End
    {
      get { return end; }
    }

    public override string ToString()
    {
      return string.Format("{0}..{1}", Start, End);
    }

  }
#endif
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IRange<T> where T : IComparable<T>
  {
    T Start { get; }
    T End { get; }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="K"></typeparam>
  /// <typeparam name="V"></typeparam>
  public class RangedTree<K, V> : IDictionary<IRange<K>, V> where K : IComparable<K>
  {
    #region Fields

    readonly Node root;
    int count = 0;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RangedTree&lt;K, V&gt;"/> class.
    /// </summary>
    /// <param name="rootkey">The rootkey.</param>
    /// <param name="rootvalue">The rootvalue.</param>
    public RangedTree(IRange<K> rootkey, V rootvalue)
    {
      root = new Node(rootkey, rootvalue);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RangedTree&lt;K, V&gt;"/> class.
    /// </summary>
    public RangedTree() : this(new InfiniteRange(), default(V))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangedTree&lt;K, V&gt;"/> class.
    /// </summary>
    /// <param name="items">The items.</param>
    public RangedTree(IEnumerable<KeyValuePair<IRange<K>,V>> items) : this()
    {
      ICollection<KeyValuePair<IRange<K>, V>> c = (ICollection<KeyValuePair<IRange<K>, V>>)this;
      foreach (KeyValuePair<IRange<K>, V> kvp in items)
      {
        c.Add(kvp);
      }
    }

    #endregion

    #region Range Compare

    sealed class InfiniteRange : IRange<K>
    {
      public K Start
      {
        get { throw new Exception("The method or operation is not implemented, INTENTIONALLY."); }
      }

      public K End
      {
        get { throw new Exception("The method or operation is not implemented, INTENTIONALLY."); }
      }
    }

    enum Comparison
    {
      Same,
      After,
      Before,
      Nested,
      Encapsulate,
      IntersectsAbove,
      IntersectsBelow,
      Invalid,
      Empty
    }

    static bool IsValidRange(IRange<K> key)
    {
      return key.Start.Equals(key.End) || key.Start.CompareTo(key.End) < 0;
    }

    static Comparison Compare(IRange<K> a, IRange<K> b)
    {
      // fast check
      if (a == b)
      {
        return Comparison.Same;
      }

      if (!IsValidRange(a) || !IsValidRange(b))
      {
        return Comparison.Invalid;
      }


      int abStart = a.Start.CompareTo(b.Start);
      int abEnd = a.End.CompareTo(b.End);
      /*
       * Truth table
       * ===========
       * 
       * [abStart][abEnd]
       *     0       0                 Same
       *     0      -1                 Nested
       *     0       1                 Encapsulated
       *     
       *    -1       0                 Encapsulated
       *     1       0                 Nested
       *     
       *    -1       1                 Encapsulated
       *    -1      -1  aEndbStart < 0 Before
       *    -1      -1  aEndbStart > 0 IntersectsBelow
       *     
       *     1      -1                 Nested
       *     1       1  aStartbEnd > 0 After
       *     1       1  aStartbEnd < 0 IntersectsAbove
       * 
       */

      if (abStart == 0)
      {
        if (abEnd == 0)
        {
          return Comparison.Same;
        }
        else if (abEnd < 0)
        {
          return Comparison.Nested;
        }
        else
        {
          return Comparison.Encapsulate;
        }
      }
      else if (abEnd == 0)
      {
        if (abStart < 0)
        {
          return Comparison.Encapsulate;
        }
        else
        {
          return Comparison.Nested;
        }
      }
      else
      {
        if (abStart < 0)
        {
          if (abEnd > 0)
          {
            return Comparison.Encapsulate;
          }
          else
          {
            if (a.End.CompareTo(b.Start) <= 0)
            {
              return Comparison.Before;
            }
            else
            {
              return Comparison.IntersectsBelow;
            }
          }
        }
        else
        {
          if (abEnd < 0)
          {
            return Comparison.Nested;
          }
          else
          {
            if (a.Start.CompareTo(b.End) >= 0)
            {
              return Comparison.After;
            }
            else
            {
              return Comparison.IntersectsAbove;
            }
          }
        }
      }
    }

    #endregion

    public INavigator RootNavigator
    {
      get { return root; }
    }

    public INavigator GetNavigator(IRange<K> key)
    {
      return root.GetChildNode(key);
    }

    #region INavigator

    /// <summary>
    /// 
    /// </summary>
    public interface INavigator
    {
      /// <summary>
      /// Gets the key.
      /// </summary>
      /// <value>The key.</value>
      IRange<K> Key { get;}
      /// <summary>
      /// Gets or sets the value.
      /// </summary>
      /// <value>The value.</value>
      V Value { get;set;}

      /// <summary>
      /// Gets the next.
      /// </summary>
      /// <value>The next.</value>
      INavigator Next { get;}
      /// <summary>
      /// Gets the previous.
      /// </summary>
      /// <value>The previous.</value>
      INavigator Previous { get;}
      /// <summary>
      /// Gets the parent.
      /// </summary>
      /// <value>The parent.</value>
      INavigator Parent { get;}
      /// <summary>
      /// Gets the root.
      /// </summary>
      /// <value>The root.</value>
      INavigator Root { get;}

      /// <summary>
      /// Gets the first child.
      /// </summary>
      /// <value>The first child.</value>
      INavigator FirstChild { get;}

      /// <summary>
      /// Gets the last child.
      /// </summary>
      /// <value>The last child.</value>
      INavigator LastChild { get;}

      /// <summary>
      /// The depth of this node.
      /// </summary>
      /// <value>the depth of the node, 0 if root</value>
      int Depth { get;}

      /// <summary>
      /// Gets a value indicating whether this instance is root.
      /// </summary>
      /// <value><c>true</c> if this instance is root; otherwise, <c>false</c>.</value>
      bool IsRoot { get;}
      /// <summary>
      /// Gets a value indicating whether this instance has children.
      /// </summary>
      /// <value>
      /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
      /// </value>
      bool HasChildren { get;}

      /// <summary>
      /// Gets the siblings.
      /// </summary>
      /// <value>The siblings.</value>
      IEnumerable<INavigator> Siblings { get;}
      /// <summary>
      /// Gets the children.
      /// </summary>
      /// <value>The children.</value>
      IEnumerable<INavigator> Children { get;}
      /// <summary>
      /// Gets all nodes.
      /// </summary>
      /// <value>All nodes.</value>
      IEnumerable<INavigator> AllNodes { get;}

      INavigator this[IRange<K> key] { get;}
    }

    #endregion

    #region Node

    sealed class Node : INavigator
    {
      List<Node> nodes = new List<Node>();
      internal readonly IRange<K> key;
      internal V value; // not readonly, DUH!

      Node parent = null;

      public INavigator Parent
      {
        get { return parent; }
      }

      public override string ToString()
      {
        if (key is InfiniteRange)
        {
          return "Infinity";
        }
        return string.Format("K: {0} V: {1}", key , value);
      }

      public Node(IRange<K> key, V value)
      {
        this.key = key;
        this.value = value;
      }

      public void Clear()
      {
        nodes.Clear();
      }

      public Node GetChildNode(IRange<K> key)
      {
        Comparison result;
        int i = BinarySearch(key, out result);

        switch (result)
        {
          case Comparison.Same:
            return nodes[i];

          case Comparison.Nested:
            return nodes[i].GetChildNode(key);

          case Comparison.IntersectsAbove:
          case Comparison.IntersectsBelow:
            throw new ArgumentException("invalid key");
        }

        return null;
      }

      int BinarySearch(IRange<K> key, out Comparison result)
      {
        int min = 0, max = nodes.Count - 1;
        int mid = -1;
        result = Comparison.Empty;

        while (min <= max)
        {
          mid = (max - min)/2 + min;
          Node midnode = nodes[mid];

          result = Compare(key, midnode.key);

          switch (result)
          {
            case Comparison.Same:
            case Comparison.Nested:
            case Comparison.Encapsulate:
            case Comparison.IntersectsBelow:
            case Comparison.IntersectsAbove:
              return mid;
            
            case Comparison.Before:
              if (max == min)
              {
                return min;
              }
              max = mid - 1;
              continue;
            
            case Comparison.After:
              if (max == min)
              {
                return max;
              }
              min = mid + 1;
              continue;
          }
        }
        
        return mid;
      }

      bool IsValidChild(Node child)
      {
        Comparison result = Compare(key, child.key);
        return result == Comparison.Encapsulate;
      }

      public IEnumerable<IRange<K>> Keys
      {
        get
        {
          foreach (Node n in AllNodes)
          {
            yield return n.key;
          }
        }
      }

      public IEnumerable<V> Values
      {
        get
        {
          foreach (Node n in AllNodes)
          {
            yield return n.value;
          }
        }
      }

      public bool Contains(IRange<K> key)
      {

        Comparison result;
        int i = BinarySearch(key, out result);

        switch (result)
        {
          case Comparison.Same:
            return true;
          case Comparison.Nested:
            Node subnode = nodes[i];
            return subnode.Contains(key);
        }

        return false;
      }

      public INavigator Add(Node child)
      {
        Comparison result;
        int i = BinarySearch(child.key, out result);

        switch (result)
        {
          case Comparison.Same:
            throw new ArgumentException("already contains this child");

          case Comparison.Before:
            child.parent = this;
            nodes.Insert(i, child);
            return child;

          case Comparison.After:
            child.parent = this;
            nodes.Insert(i + 1, child);
            return child;

          // this is the tricky one: here we need to see how many nodes are actually encapsulated
          case Comparison.Encapsulate:
            Node nn = nodes[i];
            child.parent = this;
            nodes[i] = child;
            child.Add(nn);

            // now handle siblings encapsulated too
            // this is done ugly as it is faster that using siblings or next/previous
            List<int> removals = new List<int>();

            // what goes up...
            for (int j = 1; j + i < nodes.Count; j++)
            {
              Node nnn = nodes[i + j];
              Comparison r = Compare(child.key, nnn.key);
              if (r == Comparison.Encapsulate)
              {
                child.Add(nnn);
                removals.Add(i + j);
              }
              else
              {
                break;
              }
            }

            // must come down...
            for (int j = 1; i - j >= 0 ; j++)
            {
              Node nnn = nodes[i - j];
              Comparison r = Compare(child.key, nnn.key);
              if (r == Comparison.Encapsulate)
              {
                child.Add(nnn);
                removals.Add(i - j);
              }
              else
              {
                break;
              }
            }

            // finally get rid of them
            foreach (int r in removals)
            {
              nodes[r] = null;
            }

            List<Node> newnodes = new List<Node>();

            foreach (Node node in nodes)
            {
              if (node != null)
              {
                newnodes.Add(node);
              }
            }

            nodes = newnodes;
            return child;
          
          case Comparison.Nested:
            Node n = nodes[i];
            return n.Add(child);
          
          case Comparison.Empty: // used as empty
            child.parent = this;
            nodes.Add(child);
            return child;

          case Comparison.IntersectsAbove:
          case Comparison.IntersectsBelow:
            throw new ArgumentException("invalid key");
        }
        return null;
      }

      public bool Remove(IRange<K> key)
      {
        Comparison result;
        int i = BinarySearch(key, out result);

        switch (result)
        {
          case Comparison.Same:
            Node rn = nodes[i];
            nodes.RemoveAt(i);
            foreach (Node cn in rn.nodes)
            {
              Add(cn);
            }
            return true;            

          case Comparison.Nested:
            Node n = nodes[i];
            return n.Remove(key);
        }

        return false;
      }

      #region INavigator Members

      public IEnumerable<INavigator> AllNodes
      {
        get
        {
          foreach (Node n in this.nodes)
          {
            yield return n;
            foreach (INavigator sn in n.AllNodes)
            {
              yield return sn;
            }
          }
        }
      }

      public INavigator Previous
      {
        get
        {
          if (parent != null)
          {
            Comparison result;
            int i = parent.BinarySearch(key, out result);
            if (result == Comparison.Same)
            {
              if (i > 0)
              {
                return parent.nodes[i - 1];
              }
            }
          }
          return null;
        }
      }

      public INavigator Next
      {
        get
        {
          if (parent != null)
          {
            Comparison result;
            int i = parent.BinarySearch(key, out result);
            if (result == Comparison.Same)
            {
              if (i < parent.nodes.Count - 2)
              {
                return parent.nodes[i + 1];
              }
            }
          }
          return null;
        }
      }

      public IEnumerable<INavigator> Siblings
      {
        get
        {
          if (parent != null)
          {
            foreach (Node n in parent.nodes)
            {
              if (n != this)
              {
                yield return n;
              }
            }
          }
        }
      }

      public IEnumerable<INavigator> Children
      {
        get
        {
          foreach (Node n in nodes)
          {
            yield return n;
          }
        }
      }

      public IRange<K> Key
      {
        get { return key; }
      }

      public V Value
      {
        get { return value; }
        set { this.value = value; }
      }

      public INavigator Root
      {
        get 
        {
          if (IsRoot)
          {
            return this;
          }
          return parent.Root;
        }
      }

      public bool IsRoot
      {
        get { return parent == null; }
      }

      public bool HasChildren
      {
        get { return nodes.Count > 0; }
      }

      public INavigator this[IRange<K> key]
      {
        get { return GetChildNode(key); }
      }

      public int Depth
      {
        get
        {
          int i = 0;
          INavigator n = this;
          while (!n.IsRoot)
          {
            n = n.Parent;
            i++;
          }
          return i;
        }
      }


      public INavigator FirstChild
      {
        get { return HasChildren ? nodes[0] : null; }
      }

      public INavigator LastChild
      {
        get { return HasChildren ? nodes[nodes.Count - 1] : null; }
      }

      #endregion
    }

    #endregion

    #region IDictionary

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(IRange<K> key, V value)
    {
      root.Add(new Node(key, value));
      count++;
    }

    public INavigator AddSpecial(IRange<K> key, V value)
    {
      INavigator n = root.Add(new Node(key, value));
      count++;
      return n;
    }

    /// <summary>
    /// Determines whether the specified key contains key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// 	<c>true</c> if the specified key contains key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsKey(IRange<K> key)
    {
      if (!IsValidRange(key))
      {
        throw new ArgumentException("key is not valid");
      }

      Comparison result = Compare(key, root.key);
      if (result == Comparison.Same)
      {
        return true;
      }
      else
      {
        return root.Contains(key);
      }
    }

    /// <summary>
    /// Gets the keys.
    /// </summary>
    /// <value>The keys.</value>
    public ICollection<IRange<K>> Keys
    {
      get { return new List<IRange<K>>(root.Keys); }
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    public bool Remove(IRange<K> key)
    {
      bool result = root.Remove(key);
      if (result)
      {
        count--;
      }
      return result;
    }

    /// <summary>
    /// Tries the get value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public bool TryGetValue(IRange<K> key, out V value)
    {
      Node n = root.GetChildNode(key);
      if (n == null)
      {
        value = default(V);
        return false;
      }
      value = n.value;
      return true;
    }

    /// <summary>
    /// Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public ICollection<V> Values
    {
      get { return new List<V>(root.Values); }
    }

    /// <summary>
    /// Gets or sets the <see cref="V"/> with the specified key.
    /// </summary>
    /// <value></value>
    public V this[IRange<K> key]
    {
      get
      {
        Node n = root.GetChildNode(key);
        if (n == null)
        {
          throw new KeyNotFoundException();
        }
        return n.value;
      }
      set
      {
        Node n = root.GetChildNode(key);
        if (n == null)
        {
          throw new KeyNotFoundException();
        }
        n.value = value;
      }
    }

    #endregion

    #region ICollection

    /// <summary>
    /// Adds the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    void ICollection<KeyValuePair<IRange<K>, V>>.Add(KeyValuePair<IRange<K>, V> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
      root.Clear();
      count = 0;
    }

    /// <summary>
    /// Determines whether [contains] [the specified item].
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
    /// </returns>
    bool ICollection<KeyValuePair<IRange<K>, V>>.Contains(KeyValuePair<IRange<K>, V> item)
    {
      return ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<IRange<K>, V>[] array, int arrayIndex)
    {
      foreach (KeyValuePair<IRange<K>, V> kvp in this)
      {
        array[arrayIndex++] = kvp;
      }
    }

    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get { return count; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadOnly
    {
      get { return false; }
    }

    bool ICollection<KeyValuePair<IRange<K>, V>>.Remove(KeyValuePair<IRange<K>, V> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<IRange<K>, V>> GetEnumerator()
    {
      foreach (Node n in root.AllNodes)
      {
        yield return new KeyValuePair<IRange<K>, V>(n.key, n.value);
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


  }


}
