using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// Storing texts for translation
    /// </summary>
    public static class OptionalText
    {
        internal static void Initialize()
        {
            if (engText != null) { return; }
            engText = new string[Enum.GetNames(typeof(ID)).Length];

            engText[(int)ID.OpCheckBox_MouseTuto] = "Click to Check/Uncheck";
            engText[(int)ID.OpCheckBox_NonMouseTuto] = "Press JUMP to Check/Uncheck";
        }

        public enum ID : int
        {
            OpCheckBox_MouseTuto,
            OpCheckBox_NonMouseTuto
        }

        public static string[] engText;

        public static string GetText(ID id) => InternalTranslator.Translate(engText[(int)id]);
    }
}