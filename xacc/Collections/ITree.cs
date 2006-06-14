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
