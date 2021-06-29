using System;
using System.ComponentModel;

namespace OptionalUI
{
    internal static class EnumHelper
    {
        public static string GetEnumDesc(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attribs?.Length > 0)
                return attribs[0].Description;

            return null;
        }
    }
}
