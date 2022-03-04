using System;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Useful <see cref="Color"/> effect for making Rain World styled Menu UI
    /// </summary>
    public static class MenuColorEffect
    {
        private const float midDark = 0.4435f;
        private const float midVDark = 0.2217f;

        /// <summary>
        /// Dim the color relative to <see cref="Menu.Menu.MenuColors.MediumGrey"/> and <see cref="Menu.Menu.MenuColors.DarkGrey"/>.
        /// </summary>
        public static Color MidToDark(Color mid) => new Color(mid.r * midDark, mid.g * midDark, mid.b * midDark, mid.a);

        /// <summary>
        /// Dim the color relative to <see cref="Menu.Menu.MenuColors.MediumGrey"/> and <see cref="Menu.Menu.MenuColors.DarkGrey"/>.
        /// </summary>
        public static Color MidToVeryDark(Color mid) => new Color(mid.r * midVDark, mid.g * midVDark, mid.b * midVDark, mid.a);

        private const float greyR = 0.9924f, greyG = 0.9630f, greyB = 1.0452f;

        /// <summary>
        /// Convert <see cref="Color"/> to tinted grayscale, like Rain World's Greys
        /// </summary>
        public static Color Greyscale(Color orig) => new Color(orig.grayscale * greyR, orig.grayscale * greyG, orig.grayscale * greyB, orig.a);

        /// <summary>
        /// Converts HEX <see cref="string"/> to <see cref="Color"/>.
        /// Useful for grabbing <see cref="Color"/> from <see cref="OptionInterface.config"/>.
        /// See also <seealso cref="RXColor.GetColorFromHex(uint)"/> if the format is <see cref="uint"/>.
        /// </summary>
        /// <remarks>
        /// Example: <c>myConfig = OpColorPicker.HexToColor(OptionInterface.config[MyModSetting]);</c>
        /// </remarks>
        /// <param name="hex">HEX ("FFFFFF")</param>
        /// <returns>Color (Alpha is always 1f)</returns>
        /// <exception cref="FormatException">Thrown when input is not correct Color Hex. Use <see cref="IsStringHexColor(string)"/> for checking</exception>
        public static Color HexToColor(string hex)
        {
            if (hex == "000000") { return new Color(0.01f, 0.01f, 0.01f, 1f); }
            try
            {
                float a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) / 255f : 1f;
                return new Color(
                        Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                        a
                        );
            }
            catch { throw new FormatException($"Given input [{hex}] is not correct form of HEX Color"); }
        }

        /// <summary>
        /// Checks if the string is valid HEX
        /// </summary>
        public static bool IsStringHexColor(string test)
        {
            if (test.Length != 6 && test.Length != 8) { return false; }
            try { HexToColor(test); return true; }
            catch { return false; }
        }

        /// <summary>
        /// Converts <see cref="Color"/> to HEX <see cref="string"/>.
        /// Useful to set defaultHex in ctor.
        /// </summary>
        /// <remarks>
        /// Example: <c>new OpColorPicker(cpkPos, "MyModSetting", OpColorPicker.ColorToHex(new Color(0.5f, 0.5f, 0.5f)));</c>
        /// </remarks>
        /// <param name="color">Original Color</param>
        /// <returns>Color Hex string</returns>
        public static string ColorToHex(Color color)
        {
            return string.Concat(Mathf.RoundToInt(color.r * 255).ToString("X2"),
                    Mathf.RoundToInt(color.g * 255).ToString("X2"),
                    Mathf.RoundToInt(color.b * 255).ToString("X2"));
        }

        /// <summary>
        /// Converts HSL to RGB, in case you didn't know <see cref="RXColor"/> library.
        /// </summary>
        public static Color HSLtoRGB(RXColorHSL hslColor) => RXColor.ColorFromHSL(hslColor);

        /// <summary>
        /// Converts HSL to RGB, in case you didn't know <see cref="RXColor"/> library.
        /// </summary>
        public static Color HSLtoRGB(float hue, float saturation, float luminosity) => RXColor.ColorFromHSL(hue, saturation, luminosity);

        /// <summary>
        /// Converts RGB to HSL, in case you didn't know <see cref="RXColor"/> library.
        /// </summary>
        public static RXColorHSL ColorToHSL(Color color) => RXColor.HSLFromColor(color);
    }
}