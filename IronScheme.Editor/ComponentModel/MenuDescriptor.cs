using System;
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
