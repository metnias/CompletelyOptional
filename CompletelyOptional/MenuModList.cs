using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement, ICanBeFocused
    {
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 40f) - UIelement._offset, new Vector2(250f, 684f))
        {
            menuTab = tab; menuTab.AddItems(this);
            rect = new DyeableRect(this.myContainer, new Vector2(-15f, -10f), new Vector2(280f, 705f));
        }

        internal MenuTab menuTab;

        private readonly DyeableRect rect;

        private bool _focused = false;

        bool ICanBeFocused.Focused { get => _focused; set => _focused = value; }

        bool ICanBeFocused.GreyedOut => false;

        bool ICanBeFocused.CurrentlyFocusableMouse => true;

        bool ICanBeFocused.CurrentlyFocusableNonMouse => true;

        Rect ICanBeFocused.FocusRect => throw new System.NotImplementedException();

        // ModList:
        // ABC button, Mod button shows Name(Left) and Version(right)
        // Save first mod for each letter, and scroll to it
        // if name is too long, add ...
        // Tab: It will now have ^ and v instead of having 20 Limit
        // Also can have a tab that doesn't have button
        // PickUp: Focus/Select, Throw: Unfocus/Leave, Select: Control View
        // Display Unsaved change in button colour

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            rect.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            rect.Update();
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton
        {
            public ModButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal, text)
            {
            }

            public enum State
            {
                Unsupported,
                Idle,
                Selected
            }
        }

        internal class ListButton : OpSimpleImageButton
        {
            public ListButton(Vector2 pos, Vector2 size, string signal, string fAtlasElement) : base(pos, size, signal, fAtlasElement)
            {
            }

            // In Mod list mode, Add button for turn off custom music, and Remove Focus marker animation
        }

        internal class AlphabetButton : OpLabel
        {
            public AlphabetButton(Vector2 pos, Vector2 size, string text = "TEXT") : base(pos, size, text, FLabelAlignment.Center, false)
            {
            }
        }
    }
}