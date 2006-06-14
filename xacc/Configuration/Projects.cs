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
using System.Reflection;
using System.Xml.Serialization;

namespace Xacc.Configuration
{
  /// <summary>
  /// Internal use
  /// </summary>
  [XmlRoot("projectsbase", Namespace="xacc:build")]
	public abstract class Projects
	{
    /// <summary>
    /// Internal use
    /// </summary>
    protected internal Build.Project[] projects;

    /// <summary>
    /// Internal use
    /// </summary>
    [XmlIgnore]
    public static Type SerializerType;


	}
}
