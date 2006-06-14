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
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace Xacc.Configuration
{
  /// <summary>
  /// Xsd doesnt support multiple assemblies, so we just do it ourselves
  /// </summary>
	class Schema
	{
		Schema(){}

    static XmlSchema xb;

    public static XmlSchema ProjectSchema
    {
      get {return xb;}
    }

    public static void ExportSchema(string filename)
    {
      TextWriter w = File.CreateText(filename);
      
      XmlSchema xs = GetSchema(Projects.SerializerType);
      if (xs != null)
      {
        xb = xs;
        xs.Write(w);
      }
      
      w.Close();
    }

    static XmlSchema GetSchema(Type t)
    {
      XmlReflectionImporter xri = new XmlReflectionImporter();
      XmlTypeMapping xtm = xri.ImportTypeMapping(t);
      XmlSchemas schemas = new XmlSchemas();
      XmlSchemaExporter xse = new XmlSchemaExporter(schemas);
      xse.ExportTypeMapping(xtm);

      foreach (XmlSchema xs in schemas)
      {
        return xs;
      }
      return null;
    }
	}
}
