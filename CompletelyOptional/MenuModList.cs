using UnityEngine;

namespace OptionalUI
{
    internal class MenuModList : UIelement, ICanBeFocused
    {
        public MenuModList() : base(new Vector2(193f, 240f) - UIelement._offset, new Vector2(280f, 495f))
        {
        }

        private bool _focused = false;

        bool ICanBeFocused.Focused { get => _focused; set => _focused = value; }

        bool ICanBeFocused.GreyedOut => false;

        bool ICanBeFocused.CurrentlyFocusableMouse => true;

        bool ICanBeFocused.CurrentlyFocusableNonMouse => true;

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
        internal class ModButton : OpSimpleButton
        {
            public ModButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal, text)
            {
            }
        }

        internal class AlphabetButton : OpLabel
        {
            public AlphabetButton(Vector2 pos, Vector2 size, string text = "TEXT") : base(pos, size, text, FLabelAlignment.Center, false)
            {
            }
        }
    }
}