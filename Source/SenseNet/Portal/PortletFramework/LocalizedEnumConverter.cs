using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class LocalizedEnumConverter : System.ComponentModel.EnumConverter
    {
        public LocalizedEnumConverter(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo.GetCustomAttributes(typeof(LocalizedStringValueAttribute), false) as LocalizedStringValueAttribute[];

            if (attribs.Length > 0)
                return attribs[0].StringValue;

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}
