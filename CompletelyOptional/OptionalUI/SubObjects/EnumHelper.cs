using System;
using System.ComponentModel;

namespace OptionalUI
{
    /// <summary>
    /// by PJB3005
    /// </summary>
    internal static class EnumHelper
    {
        public static string GetEnumDesc(Enum value)
        {
            if (value == null) { return null; }
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) { return null; }

            var attribs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attribs?.Length > 0)
                return attribs[0].Description;

            return null;
        }
    }
}
