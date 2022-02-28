using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OptionalUI
{
    internal class MenuModList : UIelement, FocusableUIelement
    {
        public MenuModList() : base(new Vector2(193f, 240f) - UIelement._offset, new Vector2(280f, 495f))
        {
        }

        // ModList:
        // ABC button, Mod button shows Name(Left) and Version(right)
        // Save first mod for each letter, and scroll to it
        // if name is too long, add ...
        // Tab: It will now have ^ and v instead of having 20 Limit
        // Also can have a tab that doesn't have button
        // PickUp: Focus/Select, Throw: Unfocus/Leave, Select: Control View
        // Display Unsaved change in button colour

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton
        {
        }

        internal class AlphabetButton
        {
        }
    }
}