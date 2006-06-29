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
using Xacc.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

namespace Xacc.CodeModel
{
  #region CodeElementList

  /// <summary>
  /// Collection of CodeElements
  /// </summary>
  [Serializable]
  public class CodeElementList : CollectionBase
  {
    /// <summary>
    /// Creates an instance of CodeElementList
    /// </summary>
    public CodeElementList()
    {
    }

    /// <summary>
    /// Creates an instance of CodeElementList
    /// </summary>
    /// <param name="elem">first element to add</param>
    public CodeElementList(ICodeElement elem)
    {
      Add(elem);
    }

    /// <summary>
    /// Creates an instance of CodeElementList
    /// </summary>
    /// <param name="col">a collection of elements to add</param>
    public CodeElementList(ICollection col)
    {
      AddRange(col);
    }

    /// <summary>
    /// Adds an element to the list
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the index of the element</returns>
    public int Add(ICodeElement elem)
    {
      if (elem == null)
      {
        return -1;
      }

      return List.Add(elem);
    }

    /// <summary>
    /// Adds a collection of elements to the list
    /// </summary>
    /// <param name="elems">the collection to add</param>
    public void AddRange(ICollection elems)
    {
      if (elems == null)
      {
        return;
      }

      foreach (ICodeElement elem in elems)
      {
        Add(elem);
      }
    }

    /// <summary>
    /// Gets the element at the specified index
    /// </summary>
    public ICodeElement this[int index]
    {
      get {return List[index] as ICodeElement; }
    }

    /// <summary>
    /// Conversion from ArrayList to CodeElementList
    /// </summary>
    /// <param name="col">the arraylist to convert</param>
    /// <returns>a new instance of CodeElementList</returns>
    /// <remarks>Conversion marked implicit for coding convienience in yacc files</remarks>
    public static implicit operator CodeElementList(ArrayList col)
    {
      if (col == null)
      {
        return null;
      }

      CodeElementList cel = new CodeElementList();
      cel.AddRange(col);
      return cel;
    }

    /// <summary>
    /// Converts to array
    /// </summary>
    /// <param name="type">the type of the array</param>
    /// <returns>an array of type</returns>
    public Array ToArray(Type type)
    {
      return InnerList.ToArray(type);
    }
  }

  #endregion

  #region CodeElement

  /// <summary>
  /// Base interface for all code elements
  /// </summary>
	public interface ICodeElement
	{
    /// <summary>
    /// Gets or sets the name of the element
    /// </summary>
		string					Name									{get;set;}

    /// <summary>
    /// Gets the full name of the code element
    /// </summary>
    string					Fullname							{get;}

    /// <summary>
    /// Gets or sets the parent element
    /// </summary>
    [Obsolete("Not used")]
    ICodeElement		Parent								{get;set;}

    /// <summary>
    /// Gets or sets the location of the element
    /// </summary>
    Location        Location              {get;set;}

#if DEBUG
    string          Hash                  {get;}
#endif
    /// <summary>
    /// Gets or sets userdata
    /// </summary>
    object Tag    {get;set;}
	}

  /// <summary>
  /// Base class for all code elements
  /// </summary>
	[Image("Code.Local.png")]
	[Serializable]
  public abstract class CodeElement : ICodeElement
	{
    [NonSerialized]
		static int			counter = 1;

		string					name;
		Location        location;

    [NonSerialized]
    object tag = null;

#if DEBUG
    public string Hash
    {
      get {return string.Format("{0:X8}", GetHashCode()); }
    }
#endif

    /// <summary>
    /// Gets or sets userdata
    /// </summary>
    public object Tag    
    {
      get {return tag;}
      set {tag = value;}
    }

    /// <summary>
    /// Creates an instance of CodeElement
    /// </summary>
		protected CodeElement() : this(null)	{}

    /// <summary>
    /// Creates an instance of CodeElement
    /// </summary>
    /// <param name="name">name of the element</param>
    protected CodeElement(string name) : this (name, null){}

    /// <summary>
    /// Creates an instance of CodeElement
    /// </summary>
    /// <param name="name">name of the element</param>
    /// <param name="location">location of the element</param>
		protected CodeElement(string name, Location location)
		{
			if (name == null)
			{
				this.name = GetType().Name + string.Format("_{0:D2}", counter++);
			}
			else
			{
				this.name		= name;
			}
      this.location = location;
		}

    /// <summary>
    /// Gets the full name of the code element
    /// </summary>
    public virtual string Fullname
    {
      get {return Name; }
    }

    /// <summary>
    /// Gets or sets the parent element
    /// </summary>
    [Obsolete("Not used")]
		public ICodeElement Parent
		{
			get {return null;}
			set {;}
		}

    /// <summary>
    /// Gets or sets the location of the element
    /// </summary>
    public Location Location
    {
      get {return location;}
      set {location = value;}
    }

    /// <summary>
    /// Gets or sets the name of the element
    /// </summary>
		public string Name
		{
			get {return name;}
			set {name = value;}
		}

    /// <summary>
    /// Gets the string of the element
    /// </summary>
    /// <returns>name</returns>
		public override string ToString()
		{
			return Name;
		}

    /// <summary>
    /// Joins a collection of code elements into a string
    /// </summary>
    /// <param name="col">the collection of elements</param>
    /// <returns>the joined string</returns>
    protected static string Join(ICollection col)
    {
      return Join(col, ", ");
    }

    /// <summary>
    /// Joins a collection of code elements into a string
    /// </summary>
    /// <param name="col">the collection of elements</param>
    /// <param name="sep">the seperator to use</param>
    /// <returns>the joined string</returns>
    protected static string Join(ICollection col, string sep)
    {
      if (col == null)
      {
        return string.Empty;
      }
      ArrayList list = new ArrayList(col);
      string output = string.Empty;
      if (list.Count > 0)
      {
        for (int i = 0; i < list.Count - 1; i++)
        {
          output += (list[i].ToString() + sep);
        }
        output += list[list.Count - 1].ToString();
      }
      return output;
    }

    /// <summary>
    /// Joins a collection of code elements into a string
    /// </summary>
    /// <param name="list">the collection of elements</param>
    /// <param name="sep">the seperator to use</param>
    /// <returns>the joined string</returns>
    protected static string Join(ICodeElement[] list, string sep)
    {
      if (list == null)
      {
        return string.Empty;
      }
      string output = string.Empty;
      if (list.Length > 0)
      {
        for (int i = 0; i < list.Length - 1; i++)
        {
          output += (list[i].ToString() + sep);
        }
        output += list[list.Length - 1].ToString();
      }
      return output;
    }

    /// <summary>
    /// Joins a collection of code elements into a string
    /// </summary>
    /// <param name="list">the collection of elements</param>
    /// <returns>the joined string</returns>
    protected static string Join(ICodeElement[] list)
    {
      return Join(list, ", ");
    }
	}

  #endregion

  #region CodeContainerElement

  /// <summary>
  /// Base interface for code container elements
  /// </summary>
	public interface ICodeContainerElement	: ICodeElement
	{
    /// <summary>
    /// Gets the element with name in this container
    /// </summary>
		ICodeElement		this[string name]			{get;}

    /// <summary>
    /// Gets a list of all the elements in this container
    /// </summary>
		ICodeElement[]	Elements							{get;}

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
		ICodeElement		Add(ICodeElement elem);

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    ICodeElement		Remove(ICodeElement elem);

    /// <summary>
    /// Adds a collection of elements to the container
    /// </summary>
    /// <param name="elems">the collection of elements to add</param>
    void		        AddRange(ICollection elems);

    /// <summary>
    /// Gets the element count
    /// </summary>
    int             ElementCount          {get;}
	}

  /// <summary>
  /// Base class for code container elements
  /// </summary>
  [Serializable]
	public abstract class CodeContainerElement : CodeElement, ICodeContainerElement
	{
    /// <summary>
    /// Internal use
    /// </summary>
		readonly internal Hashtable elements = new Hashtable();
    readonly static IComparer NAMECOMPARE = new ElementNameComparer();
    readonly static IComparer LOCATIONCOMPARE = new ElementLocationComparer();
    readonly static IComparer TYPECOMPARE = new ElementTypeComparer();

    /// <summary>
    /// Creates an instance of CodeContainerElement
    /// </summary>
    protected CodeContainerElement(){}

    /// <summary>
    /// Creates an instance of CodeContainerElement
    /// </summary>
    /// <param name="col">collection of code elements to add</param>
    protected CodeContainerElement(ICollection col)
    {
      AddRange(col);
    }

    /// <summary>
    /// Gets the element count
    /// </summary>
    public int ElementCount
    {
      get { return elements.Count; }
    }

		ICodeElement ICodeContainerElement.this[string name]
		{
			get {return elements[name] as ICodeElement;}
		}

    class ElementLocationComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        ICodeElement a = x as ICodeElement;
        ICodeElement b = y as ICodeElement;

        if (a == b)
        {
          return 0;
        }

        if (a.Location != null)
        {
          return a.Location.CompareTo(b.Location);
        }
        if (b.Location != null)
        {
          return -b.Location.CompareTo(a.Location);
        }
        return 0;
      }
    }

    class ElementNameComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        ICodeElement a = x as ICodeElement;
        ICodeElement b = y as ICodeElement;

        return a.Name.CompareTo(b.Name);
      }
    }

    class ElementTypeComparer : IComparer
    {
      public int Compare(object x, object y)
      {
        Type a = x as Type;
        Type b = y as Type;

        bool ac = typeof(ICodeContainerElement).IsAssignableFrom(a);
        bool bc = typeof(ICodeContainerElement).IsAssignableFrom(b);

        if ((ac && bc) | (!ac && !bc))
        {
          return a.Name.CompareTo(b.Name);
        }
        else 
        {
          if (ac)
          {
            return -1;
          }
          if (bc)
          {
            return 1;
          }
          return 0;
        }
      }
    }

    /// <summary>
    /// Gets an array of code elements based on type
    /// </summary>
    /// <param name="filter">the type to filter</param>
    /// <returns>the array of elements</returns>
    protected Array FilterElements(Type filter)
    {
      Hashtable tree = new Hashtable();
      
      foreach (ICodeElement ce in elements.Values)
      {
        ICodeOpaqueElement op = ce as ICodeOpaqueElement;
        if (op != null)
        {
          CodeContainerElement cce = op as CodeContainerElement;

          Array a = cce.FilterElements(filter);

          foreach (ICodeElement sce in a)
          {
            Type cetype = sce.GetType();

            foreach (Type itype in cetype.GetInterfaces())
            {
              if (itype == filter)
              {
                ArrayList inner = null;
                if (tree.ContainsKey(cetype))
                {
                  inner = tree[cetype] as ArrayList;
                }
                else
                {
                  tree[cetype] = ( inner = new ArrayList());
                }
                inner.Add(sce);
                break;
              }
            }
          }
        }
        else
        {
          Type cetype = ce.GetType();

          foreach (Type itype in cetype.GetInterfaces())
          {
            if (itype == filter)
            {
              //m.Add(ce);
              ArrayList inner = null;
              if (tree.ContainsKey(cetype))
              {
                inner = tree[cetype] as ArrayList;
              }
              else
              {
                tree[cetype] = ( inner = new ArrayList());
              }
              inner.Add(ce);
              break;
            }
          }
        }
      }

      ArrayList n = new ArrayList();

      ArrayList m = new ArrayList(tree.Keys);

      bool typecompare = false;

      if (typecompare)
      {
        m.Sort(TYPECOMPARE);

        foreach (Type t in m)
        {
          ArrayList a = tree[t] as ArrayList;
          a.Sort(NAMECOMPARE);
          n.AddRange(a);
        }
      }
      else
      {
        foreach (Type t in m)
        {
          ArrayList a = tree[t] as ArrayList;
          n.AddRange(a);
        }

        n.Sort(LOCATIONCOMPARE);
      }

			return n.ToArray(filter);
		}

		ICodeElement[] ICodeContainerElement.Elements
		{
			get	{	return FilterElements(typeof(ICodeElement)) as ICodeElement[];}
		}

		ICodeElement ICodeContainerElement.Add(ICodeElement elem)
		{
      if (elem == null)
      {
        return null;
      }
			elements[elem.Name] = elem;
			return elem;
		}

    ICodeElement ICodeContainerElement.Remove(ICodeElement elem)
    {
      if (elem == null)
      {
        return null;
      }
      elements.Remove(elem.Fullname);
      return elem;
    }

    /// <summary>
    /// Adds a collection of elements to the container
    /// </summary>
    /// <param name="elements">the collection of elements to add</param>
    public void AddRange(ICollection elements)
    {
      if (elements == null)
      {
        return;
      }

      foreach (ICodeElement elem in elements)
      {
        Add(elem);
      }
    }

    /// <summary>
    /// Adds a collection of elements to the container
    /// </summary>
    /// <param name="elements">the collection of elements to add</param>
    public void AddRange(ICodeElement[] elements)
    {
      AddRange((ICollection) elements);
    }

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    protected virtual ICodeElement Add(ICodeElement elem)
    {
      return ((ICodeContainerElement)this).Add(elem);
    }

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    protected virtual ICodeElement Remove(ICodeElement elem)
    {
      return ((ICodeContainerElement)this).Remove(elem);
    }

    /// <summary>
    /// Cant remember why I needed this....
    /// </summary>
    /// <param name="elem">the element to add</param>
    public void AddSpecial(ICodeElement elem)
    {
      ((ICodeContainerElement)this).Add(elem);
    }
	}

  #endregion

  #region ICodeOpaqueElement

  /// <summary>
  /// Defines an opaque container element
  /// </summary>
  public interface ICodeOpaqueElement : ICodeContainerElement
  {
  }

  #endregion

  #region CodeFile

  /// <summary>
  /// Defines an opaque container for namespaces
  /// </summary>
  public interface ICodeFile              : ICodeOpaqueElement, ICodeModule
  {
  }

  /// <summary>
  /// Defines an opaque container for namespaces
  /// </summary>
  [Serializable]
  public class CodeFile									: CodeModule, ICodeFile
  {
    /// <summary>
    /// Creates an instance of CodeFile
    /// </summary>
    /// <param name="name">the name of the file</param>
    public CodeFile(string name) : base(name)
    {
    }
  }

  #endregion

  #region CodeModule

	// more specific

  /// <summary>
  /// Defines an basic container for namespaces
  /// </summary>
	public interface ICodeModule						: ICodeContainerElement
	{
    /// <summary>
    /// Gets a namespace
    /// </summary>
		new ICodeNamespace		this[string name]		{get;}

    /// <summary>
    /// Gets all namespaces
    /// </summary>
				ICodeNamespace[]	Namespaces			    {get;}

    /// <summary>
    /// Add a namespace
    /// </summary>
    /// <param name="cns">the namespace to add</param>
    /// <returns>the newly added namespace</returns>
		ICodeNamespace Add(ICodeNamespace cns);

    /// <summary>
    /// Adds a code file to the module
    /// </summary>
    /// <param name="cf">the code file</param>
    /// <returns>the newly added code file</returns>
    ICodeFile Add(ICodeFile cf);
	}

  /// <summary>
  /// Defines an basic container for namespaces
  /// </summary>
  [Image("CodeModule.png")]
  [Serializable]
	public class CodeModule									: CodeContainerElement, ICodeModule
	{
    /// <summary>
    /// Creates an instance of CodeModule
    /// </summary>
    /// <param name="name">the name</param>
		public CodeModule(string name)
		{
			Name = name;
      Add(string.Empty);
		}

    /// <summary>
    /// Gets a namespace
    /// </summary>
		public ICodeNamespace this[string name]
		{
			get
			{
        string[] tokens = name.Split('.');
        ICodeNamespace cns = (this as ICodeContainerElement)[tokens[0]] as ICodeNamespace;
        int i = 1;
        while (cns != null && i < tokens.Length)
        {
          cns = (cns as ICodeContainerElement)[tokens[i++]] as ICodeNamespace;
        }
        return cns;
			}
		}

    /// <summary>
    /// Gets all namespaces
    /// </summary>
		public ICodeNamespace[] Namespaces
		{
			get
			{
				return FilterElements(typeof(ICodeNamespace)) as ICodeNamespace[];
			}
		}

    /// <summary>
    /// Add a namespace
    /// </summary>
    /// <param name="namespacename">the name</param>
    /// <returns>the newly created namespace</returns>
		public ICodeNamespace Add(string namespacename)
		{
      ICodeNamespace cns = this[namespacename];
      if (cns == null)
      {
        cns = Add( new CodeNamespace(namespacename));
      }
      return cns;
		}

    /// <summary>
    /// Adds a code file to the module
    /// </summary>
    /// <param name="cf">the code file</param>
    /// <returns>the newly added code file</returns>
    public ICodeFile Add(ICodeFile cf)
    {
      Add((ICodeElement)cf);
      return cf;
    }

    /// <summary>
    /// Add a namespace
    /// </summary>
    /// <param name="cns">the namespace to add</param>
    /// <returns>the newly added namespace</returns>
		public ICodeNamespace Add(ICodeNamespace cns)
		{
      ICodeNamespace sn = cns;
      ICodeNamespace cn = this[cns.Fullname];
      if (cn == null)
      {
        while (sn.Namespace != null && sn.Namespace != sn)
        {
          sn = sn.Namespace;
          Add(sn);
        }

        base.Add(cns);

        return cns;
      }
      if (cn == sn)
      {
        return cn;
      }
      //merge

      cn.AddRange( (cns as ICodeContainerElement).Elements);
			
			return cn;
		}

    /// <summary>
    /// Add a code element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    protected override ICodeElement Add(ICodeElement elem)
    {
      if (Runtime.Compiler.CLRRuntime == Runtime.CLR.Microsoft)
      {
        if (elem is ICodeNamespace)
        {
          return base.Add (elem);
        }
        else if (elem is ICodeType)
        {
          return this[string.Empty].Add(elem);
        }
        else if (elem is ICodeOpaqueElement)
        {
          foreach (ICodeElement ele in (elem as ICodeContainerElement).Elements)
          {
            Add(ele);
          }
          return elem;
        }
      
        base.Add(elem);
      }
      return elem;
    }

    /// <summary>
    /// Add a code type
    /// </summary>
    /// <param name="obj">the obj to add</param>
    /// <returns>the added object</returns>
    public ICodeType Add(ICodeType obj)
    {
      return Add((ICodeElement)obj) as ICodeType; 
    }
	}

  #endregion

  #region CodeMember

  /// <summary>
  /// Base interface for CodeMembers
  /// </summary>
	public interface ICodeMember						: ICodeElement
	{
    /// <summary>
    /// Gets or sets the enclosing type
    /// </summary>
		ICodeTypeRef			EnclosingType				{get;set;}					
	}

  /// <summary>
  /// Base class for CodeMembers
  /// </summary>
  [Image("Code.Member.png")]
  [Serializable]
  public abstract class CodeMember : CodeElement, ICodeMember
  {
    ICodeTypeRef enclosingtype = null;

    /// <summary>
    /// Gets or sets the enclosing type
    /// </summary>
    public ICodeTypeRef EnclosingType
    {
      get {return enclosingtype; }
      set {enclosingtype = value;}
    }

    /// <summary>
    /// Gets the full name of the code element
    /// </summary>
    public override string Fullname
    {
      get {return enclosingtype.Fullname + "." + base.Fullname;}
    }

  }

  /// <summary>
  /// Defines a complex container
  /// </summary>
  public interface ICodeComplexMember						: ICodeMember, ICodeContainerElement
  {
  }


  /// <summary>
  /// Defines a complex container
  /// </summary>
  [Serializable]
  public class CodeComplexMember : CodeContainerElement, ICodeComplexMember
  {
    /// <summary>
    /// Creates an instances of CodeComplexMember and add members to it
    /// </summary>
    /// <param name="mems">the collection of members to add</param>
    public CodeComplexMember(ICollection mems) : base(mems)
    {
    }

    ICodeTypeRef enclosingtype = null;

    /// <summary>
    /// Gets or sets the enclosing type
    /// </summary>
    public ICodeTypeRef EnclosingType
    {
      get {return enclosingtype; }
      set 
      {
        enclosingtype = value;
        foreach (ICodeMember cm in elements.Values)
        {
          cm.EnclosingType = value;
        }
      }
    }
  }



  #endregion

  #region CodeNamespace

 
  /// <summary>
  /// Interface for CodeNamespaces
  /// </summary>
	public interface ICodeNamespace					: ICodeContainerElement
	{
    /// <summary>
    /// Gets a type defined within this namespace
    /// </summary>
		new ICodeType			this[string name]		{get;}

    /// <summary>
    /// Gets all the types defined in this namespace
    /// </summary>
				ICodeType[]		Types								{get;}

    /// <summary>
    /// Gets the namespaces contained within this namespace
    /// </summary>
    ICodeNamespace[]		Namespaces					{get;}

    /// <summary>
    /// Gets or sets the parent namespace of this namespace
    /// </summary>
    ICodeNamespace		Namespace						{get;set;}

    /// <summary>
    /// Add a type to this namespace
    /// </summary>
    /// <param name="type">the type to add</param>
    /// <returns>the added type</returns>
		ICodeType Add(ICodeType type);

    /// <summary>
    /// Adds a namespace to the namespace
    /// </summary>
    /// <param name="ns">the namespace to add</param>
    /// <returns>the newly added namespace</returns>
    ICodeNamespace Add(ICodeNamespace ns);
	}

  /// <summary>
  /// Represents a code namespace
  /// </summary>
  [Image("CodeNamespace.png")]
  [Serializable]
	public class CodeNamespace							: CodeContainerElement, ICodeNamespace
	{
    ICodeNamespace ns;

    internal CodeNamespace()
    {
      Name = string.Empty;
      ns = this;
    }
    /// <summary>
    /// Creates an instance of CodeNamespace
    /// </summary>
    /// <param name="name">the namespace name</param>
		public CodeNamespace(string name) : this()
		{
      string[] tokens = name.Split('.');
			Name = tokens[tokens.Length - 1];
      if (tokens.Length > 1)
      {
        ns = new CodeNamespace(name.Replace("." + Name, string.Empty));
        ns.Add(this);
      }
      else
      {
        ns = new CodeNamespace();
      }
		}

    /// <summary>
    /// Gets the namespaces contained within this namespace
    /// </summary>
    public ICodeNamespace[]		Namespaces					
    {
      get {return FilterElements(typeof(ICodeNamespace)) as ICodeNamespace[];}
    }

    /// <summary>
    /// Adds a namespace to the namespace
    /// </summary>
    /// <param name="ns">the namespace to add</param>
    /// <returns>the newly added namespace</returns>
    public ICodeNamespace Add(ICodeNamespace ns)
    {
      Add((ICodeElement)ns);
      return ns;
    }

    /// <summary>
    /// Gets or sets the parent namespace of this namespace
    /// </summary>
    public ICodeNamespace	Namespace
    {
      get { return ns; }
      set { ns = value; }
    }

    /// <summary>
    /// Gets a type defined within this namespace
    /// </summary>
    public ICodeType this[string name]
		{
			get
			{
				return (this as ICodeContainerElement)[name] as ICodeType;
			}
		}

    /// <summary>
    /// Gets the full name of the object
    /// </summary>
    public override string Fullname
    {
      get
      {
        if (ns == null || Name == string.Empty)
        {
          return string.Empty;
        }
        string pname = ns.Fullname;
        if (pname.Length > 0)
        {
          return pname + "." + Name;
        }
        else
        {
          return Name;
        }
      }
    }


    /// <summary>
    /// Gets all the types defined in this namespace
    /// </summary>
		public ICodeType[] Types
		{
			get
			{
				return FilterElements(typeof(ICodeType)) as ICodeType[];
			}
		}

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    protected override ICodeElement Add(ICodeElement elem)
    {
      ICodeType ct = elem as ICodeType;
      ICodeElement res = base.Add (elem);
      if (ct != null)
      {
        ct.Namespace = this;
      }
      return res;
    }

    /// <summary>
    /// Add a type to this namespace
    /// </summary>
    /// <param name="type">the type to add</param>
    /// <returns>the added type</returns>
		public ICodeType Add(ICodeType type)
		{
			Add((ICodeElement)type);
			return type;
		}

    /// <summary>
    /// Removes a type from the namespace
    /// </summary>
    /// <param name="type">the type to remove</param>
    /// <returns>the removed type</returns>
    public ICodeType Remove(ICodeType type)
    {
      Remove((ICodeElement)type);
      return type;
    }
	}

  #endregion

  #region CodeType
	
  /// <summary>
  /// Base interface for code types
  /// </summary>
	public interface ICodeType							: ICodeMember
	{
    /// <summary>
    /// Gets or sets the container namespace
    /// </summary>
    ICodeNamespace		Namespace						{get;set;}

    /// <summary>
    /// Gets a member defined in this type
    /// </summary>
		ICodeMember		this[string name]		{get;}

    /// <summary>
    /// Gets all the members defined in this type
    /// </summary>
		ICodeMember[]	Members							{get;}

    /// <summary>
    /// Add a member to this type
    /// </summary>
    /// <param name="member">the member to add</param>
    /// <returns>the added member</returns>
		ICodeMember Add(ICodeMember member);
	}

  /// <summary>
  /// Base class for code types
  /// </summary>
	[Image("Code.Type.png")]
  [Serializable]
	public abstract class CodeType					: CodeContainerElement, ICodeType
	{
		ICodeTypeRef		enclosingtype = null;

    CodeNamespace ns = new CodeNamespace();

    /// <summary>
    /// Gets or sets the container namespace
    /// </summary>
    public CodeNamespace Namespace
    {
      get	{return ns;}
      set 
      {
        if (ns != value)
        {
          if (ns[Name] == this)
          {
            ns.Remove(this);
          }
          if (value[Name] != this)
          {
            value.Add(this);
          }
          else
          {
            ns = value;
          }
        }
      }
    }

    ICodeNamespace ICodeType.Namespace
    {
      get {return Namespace;}
      set {Namespace = value as CodeNamespace;}
    }

    /// <summary>
    /// Gets the full name of this object
    /// </summary>
    public override string Fullname
    {
      get 
      {
        if (ns.Fullname.Length == 0)
        {
          return base.Fullname;
        }
        else
        {
          return ns.Fullname + "." + base.Fullname;
        }
      }
    }

    /// <summary>
    /// Gets a member defined in this type
    /// </summary>
		public ICodeMember this[string name]
		{
			get
			{
				return (this as ICodeContainerElement)[name] as ICodeMember;
			}
		}

    /// <summary>
    /// Gets or sets the enclosing type
    /// </summary>
		public ICodeTypeRef EnclosingType
		{
			get {return enclosingtype;}
      set {enclosingtype = value;}
		}

    /// <summary>
    /// Gets all the members defined in this type
    /// </summary>
		public ICodeMember[] Members
		{
			get
			{
				return FilterElements(typeof(ICodeMember)) as ICodeMember[];
			}
		}

    /// <summary>
    /// Adds an element to the container
    /// </summary>
    /// <param name="elem">the element to add</param>
    /// <returns>the added element</returns>
    protected override ICodeElement Add(ICodeElement elem)
    {
      ICodeMember cm = elem as ICodeMember;
      if (cm != null)
      {
        cm.EnclosingType = new CodeTypeRef(this);
      }
      ICodeComplexMember cce = elem as ICodeComplexMember;
      if (cce != null)
      {
        foreach (ICodeMember ce in cce.Elements)
        {
          Add(ce);
        }
        return elem;
      }
      else
      {
        return base.Add(elem);
      }
    }

    /// <summary>
    /// Add a member to this type
    /// </summary>
    /// <param name="member">the member to add</param>
    /// <returns>the added member</returns>
		public ICodeMember Add(ICodeMember member)
		{
			Add((ICodeElement)member);
			return member;
		}


    /// <summary>
    /// Helper conversion
    /// </summary>
    /// <param name="type">the type</param>
    /// <returns>a type reference</returns>
    public static implicit operator CodeTypeRef(CodeType type)
    {
      return new CodeTypeRef(type);
    }
	}

  #endregion

  #region CodeEnum

  /// <summary>
  /// Defines a code enum
  /// </summary>
	public interface ICodeEnum							: ICodeType
	{
    /// <summary>
    /// Gets the underlying type
    /// </summary>
		ICodeTypeRef				UnderlyingType		{get;}

    /// <summary>
    /// Gets all the fields defines in this enum
    /// </summary>
		ICodeField[]				Fields						{get;}
	}

  /// <summary>
  /// Defines a code enum
  /// </summary>
  [Image("CodeEnum.png")]
  [Serializable]
  public class CodeEnum : CodeType, ICodeEnum
  {
    ICodeTypeRef underlyingtype;

    /// <summary>
    /// Creates an instance of CodeEnum
    /// </summary>
    /// <param name="name">the name of the enum</param>
    public CodeEnum(string name) : this(name, typeof(int))
    {
    }

    /// <summary>
    /// Creates an instance of CodeEnum
    /// </summary>
    /// <param name="name">the name of the enum</param>
    /// <param name="underlyingtype">the underlying type of the enum</param>
    public CodeEnum(string name, CodeTypeRef underlyingtype)
    {
      Name = name;
      this.underlyingtype = underlyingtype;

      if (underlyingtype == null)
      {
        this.underlyingtype = CodeTypeRef.Undefined;
      }
    }

    /// <summary>
    /// Gets the underlying type
    /// </summary>
    public ICodeTypeRef UnderlyingType
    {
      get {return underlyingtype;}
    }

    /// <summary>
    /// Gets all the fields defines in this enum
    /// </summary>
    public ICodeField[] Fields
    {
      get 
      {
        return FilterElements(typeof(ICodeField)) as ICodeField[];
      }
    }
  }

  #endregion

  #region CodeInterface

  /// <summary>
  /// Defines a code interface
  /// </summary>
	public interface ICodeInterface					: ICodeType
	{
    /// <summary>
    /// Gets the base types of the interface
    /// </summary>
		ICodeTypeRef[]			BaseTypes					{get;}

    /// <summary>
    /// Gets a list of properties in this interface
    /// </summary>
		ICodeProperty[]			Properties				{get;}

    /// <summary>
    /// Gets a list of methods in this interface
    /// </summary>
		ICodeMethod[]				Methods						{get;}
	}

  /// <summary>
  /// Defines a code interface
  /// </summary>
  [Image("CodeInterface.png")]
  [Serializable]
  public class CodeInterface : CodeType, ICodeInterface
  {
    ICodeTypeRef[] basetypes;

    /// <summary>
    /// Creates an instance of CodeInterface
    /// </summary>
    /// <param name="name">the name of the interface</param>
    /// <param name="basetypes">the base types of the interface</param>
    public CodeInterface(string name, params CodeTypeRef[] basetypes)
    {
      Name = name;
      this.basetypes = basetypes;
    }

    /// <summary>
    /// Gets or sets the base types of the interface
    /// </summary>
    public ICodeTypeRef[] BaseTypes
    {
      get { return basetypes;}
      set {basetypes = value;}
    }

    /// <summary>
    /// Gets a list of properties in this interface
    /// </summary>
    public ICodeProperty[] Properties
    {
      get
      {
        return FilterElements(typeof(ICodeProperty)) as ICodeProperty[];
      }
    }

    /// <summary>
    /// Gets a list of methods in this interface
    /// </summary>
    public ICodeMethod[] Methods
    {
      get
      {
        return FilterElements(typeof(ICodeMethod)) as ICodeMethod[];
      }
    }
  }

  #endregion

  #region CodeDelegate

  /// <summary>
  /// Defines a code delegate
  /// </summary>
	public interface ICodeDelegate					: ICodeType
	{
    /// <summary>
    /// Gets the return type of the delegate
    /// </summary>
		ICodeTypeRef				ReturnType				{get;}

    /// <summary>
    /// Gets the parameters of the delegate
    /// </summary>
		ICodeParameter[]		Parameters				{get;}
	}

  /// <summary>
  /// Defines a code delegate
  /// </summary>
  [Image("CodeDelegate.png")]
  [Serializable]
  public class CodeDelegate : CodeType, ICodeDelegate
  {
    ICodeTypeRef returntype;
    ICodeParameter[] parameters;

    /// <summary>
    /// Creates an instance of CodeDelegate
    /// </summary>
    /// <param name="name">the name of the delegate</param>
    /// <param name="returntype">the return type of the delegate</param>
    /// <param name="parameters">the parameters of the delegate</param>
    public CodeDelegate(string name, CodeTypeRef returntype, ICollection parameters)
    {
      Name = name;
      this.returntype = returntype;
      this.parameters = new ArrayList(parameters).ToArray(typeof(ICodeParameter))
        as ICodeParameter[];

      if (returntype == null)
      {
        this.returntype = CodeTypeRef.Undefined;
      }
    }

    /// <summary>
    /// Creates an instance of CodeDelegate
    /// </summary>
    /// <param name="name">the name of the delegate</param>
    /// <param name="parameters">the parameters of the delegate</param>
    public CodeDelegate(string name, ICollection parameters) :
    this(name, typeof(void), parameters)
    {
    }

    /// <summary>
    /// Gets the return type of the delegate
    /// </summary>
    public ICodeTypeRef ReturnType
    {
      get {return returntype;}
    }

    /// <summary>
    /// Gets the parameters of the delegate
    /// </summary>
    public ICodeParameter[] Parameters
    {
      get {return parameters;}
    }

    /// <summary>
    /// Gets the string of the element
    /// </summary>
    /// <returns>name</returns>
    public override string ToString()
    {
      return string.Format("{0} {1}({2})", returntype, Name, Join(parameters));
    }
  }

  #endregion

  #region CodeParameter

  /// <summary>
  /// Defines a code parameter
  /// </summary>
	public interface ICodeParameter					: ICodeElement
	{
    /// <summary>
    /// Gets the type of the parameter
    /// </summary>
		ICodeTypeRef				Type							{get;}

    /// <summary>
    /// Gets the attributes of the parameter
    /// </summary>
    ParameterAttributes Attributes        {get;}
	}

  /// <summary>
  /// Defines a code parameter
  /// </summary>
  [Serializable]
  public class CodeParameter : CodeElement, ICodeParameter
  {
    ICodeTypeRef type = null;
    ParameterAttributes attr;

    /// <summary>
    /// Creates an instance of CodeParameter
    /// </summary>
    /// <param name="name">the name of the parameter</param>
    /// <param name="type">the type of the parameter</param>
    public CodeParameter(string name, CodeTypeRef type)
      : this(name, type, ParameterAttributes.None)
    {
    }

    /// <summary>
    /// Creates an instance of CodeParameter
    /// </summary>
    /// <param name="name">the name of the parameter</param>
    /// <param name="type">the type of the parameter</param>
    /// <param name="attr">the attributes of the parameter</param>
    public CodeParameter(string name, CodeTypeRef type, ParameterAttributes attr)
    {
      this.attr = attr;
      Name = name;
      this.type = type;
      if (type == null)
      {
        this.type = CodeTypeRef.Undefined;
      }
    }

    /// <summary>
    /// Gets the attributes of the parameter
    /// </summary>
    public ParameterAttributes Attributes        
    {
      get {return attr;}
    }

    /// <summary>
    /// Gets the type of the parameter
    /// </summary>
    public ICodeTypeRef Type
    {
      get {return type;}
    }

    /// <summary>
    /// Gets the string of the element
    /// </summary>
    /// <returns>name</returns>
    public override string ToString()
    {
      string at = string.Empty;

      if (attr == (ParameterAttributes.Out | ParameterAttributes.In))
      {
        at = "ref ";
      }
      if (attr == ParameterAttributes.Out)
      {
        at = "out ";
      }

      return  at + type + " " + Name;
    }

  }

  #endregion

  #region CodeRefType
  
  /// <summary>
  /// Defines a code reference type
  /// </summary>
	public interface ICodeRefType						: ICodeType
	{
    /// <summary>
    /// Gets or sets the basetype for this type
    /// </summary>
		ICodeTypeRef			  BaseType					{get;set;}

    /// <summary>
    /// Gets or sets the implemented interfaces for this type
    /// </summary>
    ICodeTypeRef[]		  Interfaces    		{get;set;}

    /// <summary>
    /// Gets a list of contructors for this type
    /// </summary>
		ICodeConstructor[]	Constructors			{get;}

    /// <summary>
    /// Gets a list of properties in this type
    /// </summary>
		ICodeProperty[]			Properties				{get;}

    /// <summary>
    /// Gets a list of methods in this type
    /// </summary>
		ICodeMethod[]				Methods						{get;}

    /// <summary>
    /// Gets a list of fields in this type
    /// </summary>
		ICodeField[]				Fields						{get;}

    /// <summary>
    /// Gets a list of delegates in this type
    /// </summary>
		ICodeDelegate[]			Delegates					{get;}

    /// <summary>
    /// Gets a list of nested types in this type
    /// </summary>
		ICodeType[]					NestedTypes				{get;}
	}

  /// <summary>
  /// Defines a code reference type
  /// </summary>
  [Image("CodeRefType.png")]
  [Serializable]
  public class CodeRefType : CodeType, ICodeRefType
  {
    ICodeTypeRef basetype = null;
    ICodeTypeRef[] interfaces = null;

    /// <summary>
    /// Creates an instance of CodeRefType
    /// </summary>
    /// <param name="name">the name of the type</param>
    public CodeRefType(string name) :
      this(name, null)
    {
    }

    /// <summary>
    /// Creates an instance of CodeRefType
    /// </summary>
    /// <param name="name">the name of the type</param>
    /// <param name="basetype">the base type</param>
    /// <param name="interfaces">implemented interfaces</param>
    public CodeRefType(string name, CodeTypeRef basetype, params CodeTypeRef[] interfaces)
    {
      Name = name;
      this.basetype = basetype == null ? typeof(object) : basetype;
      this.interfaces = interfaces;
    }

    /// <summary>
    /// Gets or sets the basetype for this type
    /// </summary>
    public ICodeTypeRef BaseType
    {
      get {return basetype;}
      set {basetype = value;}
    }

    /// <summary>
    /// Gets or sets the implemented interfaces for this type
    /// </summary>
    public ICodeTypeRef[] Interfaces
    {
      get { return interfaces;}
      set {interfaces = value;}
    }

    /// <summary>
    /// Gets a list of contructors for this type
    /// </summary>
    public ICodeConstructor[] Constructors
    {
      get
      {
        return FilterElements(typeof(ICodeConstructor)) as ICodeConstructor[];;
      }
    }

    /// <summary>
    /// Gets a list of properties in this type
    /// </summary>
    public ICodeProperty[] Properties
    {
      get
      {
        return FilterElements(typeof(ICodeProperty)) as ICodeProperty[];
      }
    }

    /// <summary>
    /// Gets a list of methods in this type
    /// </summary>
    public ICodeMethod[] Methods
    {
      get
      {
        return FilterElements(typeof(ICodeMethod)) as ICodeMethod[];
      }
    }

    /// <summary>
    /// Gets a list of fields in this type
    /// </summary>
    public ICodeField[] Fields
    {
      get
      {
        return FilterElements(typeof(ICodeField)) as ICodeField[];
      }
    }

    /// <summary>
    /// Gets a list of delegates in this type
    /// </summary>
    public ICodeDelegate[] Delegates
    {
      get
      {
        return FilterElements(typeof(ICodeDelegate)) as ICodeDelegate[];
      }
    }

    /// <summary>
    /// Gets a list of nested types in this type
    /// </summary>
    public ICodeType[] NestedTypes
    {
      get
      {
        return FilterElements(typeof(ICodeType)) as ICodeType[];
      }
    }
  }

  #endregion

  #region CodeValueType

  /// <summary>
  /// Defines a code value type
  /// </summary>
  public interface ICodeValueType					: ICodeType
  {
    /// <summary>
    /// Gets or sets the implemented interfaces for this type
    /// </summary>
    ICodeTypeRef[]		  Interfaces    		{get;}

    /// <summary>
    /// Gets a list of contructors for this type
    /// </summary>
    ICodeConstructor[]	Constructors			{get;}

    /// <summary>
    /// Gets a list of properties in this type
    /// </summary>
    ICodeProperty[]			Properties				{get;}

    /// <summary>
    /// Gets a list of methods in this type
    /// </summary>
    ICodeMethod[]				Methods						{get;}

    /// <summary>
    /// Gets a list of fields in this type
    /// </summary>
    ICodeField[]				Fields						{get;}

    /// <summary>
    /// Gets a list of delegates in this type
    /// </summary>
    ICodeDelegate[]			Delegates					{get;}

    /// <summary>
    /// Gets a list of nested types in this type
    /// </summary>
    ICodeType[]					NestedTypes				{get;}
  }

  /// <summary>
  /// Defines a code value type
  /// </summary>
  [Image("CodeValueType.png")]
  [Serializable]
  public class CodeValueType : CodeType, ICodeValueType
  {
    ICodeTypeRef[] interfaces;

    /// <summary>
    /// Creates an instance of CodeValueType
    /// </summary>
    /// <param name="name">the name of the type</param>
    /// <param name="interfaces">a list of implemented interfaces</param>
    public CodeValueType(string name, params CodeTypeRef[] interfaces)
    {
      Name = name;
      this.interfaces = interfaces;
    }

    /// <summary>
    /// Gets or sets the implemented interfaces for this type
    /// </summary>
    public ICodeTypeRef[] Interfaces
    {
      get { return interfaces;}
      set {interfaces = value;}
    }

    /// <summary>
    /// Gets a list of contructors for this type
    /// </summary>
    public ICodeConstructor[] Constructors
    {
      get
      {
        return FilterElements(typeof(ICodeConstructor)) as ICodeConstructor[];;
      }
    }

    /// <summary>
    /// Gets a list of properties in this type
    /// </summary>
    public ICodeProperty[] Properties
    {
      get
      {
        return FilterElements(typeof(ICodeProperty)) as ICodeProperty[];
      }
    }

    /// <summary>
    /// Gets a list of methods in this type
    /// </summary>
    public ICodeMethod[] Methods
    {
      get
      {
        return FilterElements(typeof(ICodeMethod)) as ICodeMethod[];
      }
    }

    /// <summary>
    /// Gets a list of fields in this type
    /// </summary>
    public ICodeField[] Fields
    {
      get
      {
        return FilterElements(typeof(ICodeField)) as ICodeField[];
      }
    }

    /// <summary>
    /// Gets a list of delegates in this type
    /// </summary>
    public ICodeDelegate[] Delegates
    {
      get
      {
        return FilterElements(typeof(ICodeDelegate)) as ICodeDelegate[];
      }
    }

    /// <summary>
    /// Gets a list of nested types in this type
    /// </summary>
    public ICodeType[] NestedTypes
    {
      get
      {
        return FilterElements(typeof(ICodeType)) as ICodeType[];
      }
    }
  }

  #endregion

  #region CodeField

  /// <summary>
  /// Defines a code field
  /// </summary>
	public interface ICodeField							: ICodeMember
	{
    /// <summary>
    /// Gets the type of the field
    /// </summary>
		ICodeTypeRef				Type							{get;}
	}

  /// <summary>
  /// Defines a code field
  /// </summary>
  [Image("CodeField.png")]
  [Serializable]
  public class CodeField : CodeMember, ICodeField
  {
    ICodeTypeRef type = null;

    /// <summary>
    /// Creates an instance of CodeField
    /// </summary>
    /// <param name="name">the name of the field</param>
    /// <param name="type">the type of the field</param>
    public CodeField(string name, CodeTypeRef type)
    {
      Name = name;
      if (type == null)
      {
        this.type = CodeTypeRef.Undefined;
      }
      else
      {
        this.type = type;
      }
    }

    /// <summary>
    /// Gets the type of the field
    /// </summary>
    public ICodeTypeRef Type
    {
      get {return type;}
    }

    /// <summary>
    /// Gets the string representation of this object
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
    {
      return Name + " : " + type;
    }

  }

  #endregion

  #region CodeEvent

  /// <summary>
  /// Defines a code event
  /// </summary>
	public interface ICodeEvent							: ICodeMember
	{
    /// <summary>
    /// Gets the event type
    /// </summary>
		ICodeTypeRef				HandlerType						{get;}
	}

  /// <summary>
  /// Defines a code event
  /// </summary>
  [Image("CodeEvent.png")]
  [Serializable]
  public class CodeEvent : CodeMember, ICodeEvent
  {
    ICodeTypeRef handlertype = null;

    /// <summary>
    /// Creates an instance of CodeEvent
    /// </summary>
    /// <param name="name">the name of the event</param>
    /// <param name="handlertype">the type of the handler</param>
    public CodeEvent(string name, CodeTypeRef handlertype)
    {
      Name = name;
      this.handlertype = handlertype;
      if (handlertype == null)
      {
        this.handlertype = CodeTypeRef.Undefined;
      }
    }

    /// <summary>
    /// Gets the event type
    /// </summary>
    public ICodeTypeRef HandlerType
    {
      get {return handlertype;}
    }

    /// <summary>
    /// Gets the string representation of this object
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
    {
      return handlertype + " " + Name;
    }

  }

  #endregion

  #region CodeTypeRef

  /// <summary>
  /// Defines a reference to a code type
  /// </summary>
	public interface ICodeTypeRef						: ICodeElement
	{
    /// <summary>
    /// Gets the namespace of this type reference
    /// </summary>
    string Namespace {get;}

    /// <summary>
    /// Gets whether this type is an array
    /// </summary>
    bool IsArray {get;}
	}

  /// <summary>
  /// Defines a reference to a code type
  /// </summary>
  [Serializable]
  public class CodeTypeRef : CodeElement, ICodeTypeRef
  {
    string ns;
    bool isarray = false;

    /// <summary>
    /// An undefined type
    /// </summary>
    public static readonly CodeTypeRef Undefined = new CodeTypeRef("'undefined type'");

    /// <summary>
    /// Gets whether this type is an array
    /// </summary>
    public bool IsArray
    {
      get {return isarray;}
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="type">the type</param>
    public CodeTypeRef(CodeTypeRef type) :
      this (type, false)
    {
    }

    /// <summary>
    /// Makes the type name short
    /// </summary>
    /// <param name="name">the long type name</param>
    /// <returns>the short type name</returns>
    protected virtual string MakeShort(string name) { return name; }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="elemtype">the element type</param>
    /// <param name="isarray">if an array</param>
    public CodeTypeRef(CodeTypeRef elemtype, bool isarray) 
    {
      if (elemtype == null)
      {
        elemtype = CodeTypeRef.Undefined;
      }
      Name = MakeShort(elemtype.Name);
      ns = elemtype.Namespace;
      this.isarray = isarray;
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="type">the element type</param>
    public CodeTypeRef(CodeType type) 
      : this(type.Name, type.Namespace.Name, false)
    {
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="type">the element type</param>
    public CodeTypeRef(Type type) 
      : this(type.Name, type.Namespace, type.IsArray)
    {
      
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="type">the element type</param>
    /// <param name="isarray">if an array</param>
    public CodeTypeRef(Type type, bool isarray) 
      : this(type.Name, type.Namespace, isarray)
    {
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="isarray">if an array</param>
    public CodeTypeRef(string name, bool isarray) 
      : this(name, string.Empty, isarray)
    {
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="name">the name</param>
    public CodeTypeRef(string name) 
      : this(name, string.Empty, false)
    {
    }

    /// <summary>
    /// Creates an instance of CodeTypeRef
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="ns">the namespace</param>
    /// <param name="isarray">if an array</param>
    public CodeTypeRef(string name, string ns, bool isarray)
    {
      int i = name.LastIndexOf('.');
      
      if (i > -1)
      {
        Name = MakeShort(name.Substring(i + 1));
        ns = name.Substring(0, i - 1);
      }
      else
      {
        Name = MakeShort(name);
        this.ns = ns;
      }
      this.isarray = isarray;
    }

    /// <summary>
    /// Gets the namespace of this type reference
    /// </summary>
    public string Namespace
    {
      get {return ns;}
    }

    /// <summary>
    /// Helper conversion
    /// </summary>
    /// <param name="type">the type</param>
    /// <returns>a code reference to the type</returns>
    public static implicit operator CodeTypeRef(Type type)
    {
      return new CodeTypeRef(type);
    }
  }

  #endregion

  #region CodeMethod

  /// <summary>
  /// Defines a code method
  /// </summary>
  public interface ICodeMethod						: ICodeMember
  {
    /// <summary>
    /// Gets the return type of this method
    /// </summary>
    ICodeTypeRef				ReturnType				{get;}

    /// <summary>
    /// Gets the parameters of this method
    /// </summary>
    ICodeParameter[]		Parameters				{get;}

    /// <summary>
    /// Gets the locals of this method
    /// </summary>
    CodeElementList 		Locals						{get;}

    /// <summary>
    /// Gets the statements of this method
    /// </summary>
    CodeElementList			Statements				{get;}
  }

  /// <summary>
  /// Defines a code method
  /// </summary>
  [Image("CodeMethod.png")]
  [Serializable]
  public class CodeMethod : CodeMember, ICodeMethod
  {
    ICodeTypeRef returntype;
    ICodeParameter[] parameters;
    CodeElementList locals = new CodeElementList();

    //NOTE: no serialization on statement level and higher
    [NonSerialized] 
    CodeElementList statements = new CodeElementList();

    /// <summary>
    /// Creates an instance of CodeMethod
    /// </summary>
    /// <param name="name">the name of the method</param>
    /// <param name="returntype">the return type</param>
    /// <param name="parameters">the parameters of this method</param>
    public CodeMethod(string name, CodeTypeRef returntype, ICollection parameters)
    {
      Name = name;
      this.returntype = returntype;
      
      this.parameters = parameters == null ? new ICodeParameter[0] : new ArrayList(parameters).ToArray(typeof(ICodeParameter)) 
        as ICodeParameter[];
    }

    /// <summary>
    /// Creates an instance of CodeMethod
    /// </summary>
    /// <param name="name">the name of the method</param>
    /// <param name="parameters">the parameters of this method</param>
    public CodeMethod(string name, ICollection parameters) :
      this(name, typeof(void), parameters)
    {
    }

    /// <summary>
    /// Gets the return type of this method
    /// </summary>
    public ICodeTypeRef ReturnType
    {
      get { return returntype;}
    }

    /// <summary>
    /// Gets the parameters of this method
    /// </summary>
    public ICodeParameter[] Parameters
    {
      get {return parameters;}
    }

    /// <summary>
    /// Gets the locals of this method
    /// </summary>
    public CodeElementList Locals
    {
      get {return locals; }
    }

    /// <summary>
    /// Gets the statements of this method
    /// </summary>
    public CodeElementList Statements
    {
      get {return statements; }
    }

    /// <summary>
    /// Gets the string representation of this object
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
    {
      if (returntype == null)
      {
        return string.Format("{0}({1})", Name, Join(parameters));
      }
      else
      {
        return string.Format("{1}({2}) : {0}", returntype, Name, Join(parameters));
      }
    }

  }

  #endregion

  #region CodeStatement

  /// <summary>
  /// Defines a code statement
  /// </summary>
	public interface ICodeStatement					: ICodeElement
	{

	}
//
//  public abstract class CodeStatement : CodeElement, ICodeStatement
//  {
//  }
//
//  public class CodeAssignStatement : CodeStatement
//  {
//  }
//
//  public class CodeExpressionStatement : CodeStatement
//  {
//  }
//
//  public class CodeBlockStatement : CodeStatement
//  {
//  }
//
//  public class CodeConditionalStatement : CodeStatement
//  {
//  }
//
//  public class CodeIterationStatement : CodeStatement
//  {
//  }



  #endregion

  #region CodeExpression

  /// <summary>
  /// Defines a code expression
  /// </summary>
	public interface ICodeExpression				: ICodeElement
	{

	}
//
//  public abstract class CodeExpression : CodeElement, ICodeExpression
//  {
//  }
//
//  public class CodeBinaryExpression : CodeExpression
//  {
//  }
//
//
//  public class CodeUnaryExpression : CodeExpression
//  {
//  }
//
//  public class CodeVariableReferenceExpression : CodeExpression
//  {
//  }
//
//  public class CodeMethodInvokeExpression : CodeExpression
//  {
//  }
//
//  public class CodeConstantExpression : CodeExpression
//  {
//  }

  #endregion

 //////////////  //////////  //////////     /////////// 
     ////       ///      /// ///      ///  ///       ///
     ////       ///      /// ///       /// ///       ///
     ////       ///      /// ///       /// ///       ///
     ////       ///      /// ///      ///  ///       ///
     ////        //////////  //////////     /////////// 
  
   
  /// <summary>
  /// Defines a code property
  /// </summary>
  public interface ICodeProperty					: ICodeMember
  {
    /// <summary>
    /// Gets the property type
    /// </summary>
    ICodeTypeRef        PropertyType      {get;}

    /// <summary>
    /// Gets the getter method
    /// </summary>
    ICodeMethod					Getter						{get;}

    /// <summary>
    /// Gets the setter method
    /// </summary>
    ICodeMethod					Setter						{get;}
  }

  /// <summary>
  /// Defines a code property
  /// </summary>
  [Image("CodeProperty.png")]
  [Serializable]
  public class CodeProperty : CodeMember, ICodeProperty
  {
    ICodeTypeRef proptype;

    /// <summary>
    /// Creates an instance of CodeProperty
    /// </summary>
    /// <param name="name">the name</param>
    /// <param name="proptype">the property type</param>
    public CodeProperty(string name, ICodeTypeRef proptype)
    {
      Name = name;
      this.proptype = proptype;
    }

    /// <summary>
    /// Gets the property type
    /// </summary>
    public ICodeTypeRef PropertyType
    {
      get {return proptype;}
    }

    /// <summary>
    /// Gets the getter method
    /// </summary>
    public ICodeMethod Getter
    {
      get
      {
        // TODO:  Add COdeProperty.Getter getter implementation
        return null;
      }
    }

    /// <summary>
    /// Gets the setter method
    /// </summary>
    public ICodeMethod Setter
    {
      get
      {
        // TODO:  Add COdeProperty.Setter getter implementation
        return null;
      }
    }

    /// <summary>
    /// Gets the string representation of this object
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
    {
      return proptype + " " + Name;
    }
  }


  /// <summary>
  /// Defines a code constructor
  /// </summary>
  public interface ICodeConstructor				: ICodeMethod
  {
  }

  /// <summary>
  /// Defines a code constructor
  /// </summary>
  [Serializable]
  public class CodeConstructor : CodeMethod, ICodeConstructor
  {
    /// <summary>
    /// Creates an instance of CodeConstructor
    /// </summary>
    /// <param name="name">the name of the type</param>
    /// <param name="parameters">the parameters</param>
    public CodeConstructor(string name, ICollection parameters) 
      : base(name, null, parameters)
    {
    }
 
  }

  /// <summary>
  /// Defines a code constructor
  /// </summary>
  public interface ICodeDestructor				: ICodeMethod
  {
  }

  /// <summary>
  /// Defines a code destructor
  /// </summary>
  [Serializable]
  public class CodeDestructor : CodeMethod, ICodeDestructor
  {
    /// <summary>
    /// Creates an instance of CodeDestructor
    /// </summary>
    /// <param name="name">the name of the type</param>
    public CodeDestructor(string name) 
      : base(name, null, new CodeElementList())
    {
    }

    /// <summary>
    /// Gets the string representation of this object
    /// </summary>
    /// <returns>the string value</returns>
    public override string ToString()
    {
      return "~" + base.ToString ();
    }
  }

}
