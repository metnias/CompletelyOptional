using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}