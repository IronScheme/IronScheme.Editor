#region License
/* Copyright (c) 2003-2015 Llewellyn Pritchard
 * All rights reserved.
 * This source code is subject to terms and conditions of the BSD License.
 * See license.txt. */
#endregion

using System.Collections;
using System.ComponentModel;

namespace IronScheme.Editor.ComponentModel
{
  abstract class MenuDescriptor : TypeConverter
  {
    public sealed override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public abstract ICollection GetValues();

    public sealed override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      return new StandardValuesCollection(GetValues());
    }

  }
}
