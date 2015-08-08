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
	/// Defines interface for tree structure
	/// </summary>
	interface ITree : ICollection
	{
    /// <summary>
    /// Adds an object to the tree
    /// </summary>
    /// <param name="value">the object value</param>
    /// <param name="key">the location</param>
		void Add(object value, Array key);

    /// <summary>
    /// Checks whether tree contains location
    /// </summary>
    /// <param name="key">the location</param>
    /// <returns></returns>
		bool Contains(Array key);

    /// <summary>
    /// Gets or sets the value at a specific location
    /// </summary>
		object this[Array key] {get;set;}
	}
}
