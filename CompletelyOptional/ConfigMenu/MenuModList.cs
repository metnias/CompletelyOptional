using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 60f) - UIelement._offset, new Vector2(250f, 650f))
        {
            // position of 26th button, size of mod list
            // actual pos: 193 35; size: 280 700
            tab.AddItems(this);

            // abc buttons
            abcButtons = new AlphabetButton[26];
            for (int i = 0; i < abcButtons.Length; i++)
            { abcButtons[i] = new AlphabetButton(this, i); }

            // backpanel & stat button
            backSide = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 250f, color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black), alpha = 0.4f };
            myContainer.AddChild(backSide);
            bool scrollFit = ConfigContainer.OptItfs.Length - 1 < scrollVisible;
            sideLines = new FSprite[scrollFit ? 3 : 2];
            sideLines[0] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f }; // Right Btm
            sideLines[1] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f }; // Right Top
            if (scrollFit)
            {
                sideLines[2] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f }; // Leftside
            }
            else
            { // Add subtle slider
            }

            for (int i = 0; i < sideLines.Length; i++) { myContainer.AddChild(sideLines[i]); }

            roleButtons = new ListButton[3];
            roleButtons[0] = new ListButton(this, ListButton.Role.Stat);
            roleButtons[1] = new ListButton(this, ListButton.Role.ScrollUp);
            roleButtons[2] = new ListButton(this, ListButton.Role.ScrollDown);
            if (scrollFit) { roleButtons[1].Hide(); roleButtons[2].Hide(); }

            // mod buttons
            modButtons = new ModButton[ConfigContainer.OptItfs.Length];
            for (int i = 0; i < modButtons.Length; i++)
            { modButtons[i] = new ModButton(this, i); }
        }

        // Change of plan: use Arena's LevelSelector way of doing instead

        private MenuTab menuTab => this.tab as MenuTab;
        private ConfigContainer cfgContainer => menu.cfgContainer;

        private float abcSlide = 40f, lastAbcSlide = 40f; // -60f (hidden) ~ 40f (slided out)

        private float GetMyAbcSlide(int index, float timeStacker) => Mathf.Clamp(Mathf.Lerp(lastAbcSlide, abcSlide, timeStacker) + index * 2.0f, 0f, 40f);

        private readonly ModButton[] modButtons;
        private readonly AlphabetButton[] abcButtons;
        private readonly FSprite backSide;
        private readonly FSprite[] sideLines;
        private readonly ListButton[] roleButtons;
        private int scrollPos; private const int scrollVisible = 26;

        // ModList:
        // ABC button, Mod button shows Name(Left) and Version(right)
        // Save first mod for each letter, and scroll to it
        // if name is too long, add ...
        // Tab: It will now have ^ and v instead of having 20 Limit
        // Also can have a tab that doesn't have button
        // PickUp: Focus/Select, Throw: Unfocus/Leave, Select: Control View
        // Display Unsaved change in button colour

        public override void OnChange()
        {
            base.OnChange();
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            sideLines[0].x = 250f;
            sideLines[0].y = 0f;
        }

        public override void Update()
        {
            base.Update();
            lastAbcSlide = abcSlide;
        }

        private void Signal(UIelement element, int index = -1)
        {
            if (element is ModButton)
            { // Switch Mod
                ConfigContainer.ChangeActiveMod(index);
            }
            else if (element is AlphabetButton)
            { // Scroll to Alphabet
            }
            else if (element is ListButton)
            {
                if (index == 0)
                { // Switch Mod to InternalOI_Stats
                    ConfigContainer.ChangeActiveMod(0);
                }
                else
                { // Scroll Up(-1) or Down(+1)
                }
            }
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton
        {
            public ModButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(250f, 25f), "Mod List")
            {
                this.list = list;
                this.index = index + 1; // starts from 1; index 0 is for StatOI
                this.list.menuTab.AddItems(this);
                //this._pos = list.camPos;

                // Get Type
                if (itf is InternalOI)
                {
                    if ((itf as InternalOI).reason == InternalOI.Reason.Error) { type = ItfType.Error; }
                    else { type = ItfType.Blank; }
                }
                else { type = itf.Configurable() ? ItfType.Configurable : ItfType.Inconfigurable; }

                this.text = itf.rwMod.ModID;
                this.labelVer = OpLabel.CreateFLabel(itf.rwMod.Version);
                this.myContainer.AddChild(this.labelVer);

                this.list.menuTab.AddItems(this);
                OnChange();
            }

            private readonly MenuModList list;

            public readonly int index;
            private OptionInterface itf => ConfigContainer.OptItfs[index];
            private readonly FLabel labelVer;
            private float fade, lastFade;

            public readonly ItfType type;

            public enum ItfType
            {
                Blank,
                Error,
                Inconfigurable,
                Configurable
            }

            public override void OnChange()
            {
                base.OnChange();
                this.label.alignment = FLabelAlignment.Left;
                this.labelVer.alignment = FLabelAlignment.Right;
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                Mathf.Lerp(lastFade, fade, timeStacker);
                list.GetMyAbcSlide(index, timeStacker);
                //ConfigContainer.OptItfChanged[index];
            }

            public override void Update()
            {
                this.lastFade = this.fade;
                base.Update();
            }

            public override void Signal()
            {
                list.Signal(this, index);
            }

            public override bool CurrentlyFocusableMouse => this.fade > 0.5f;
            public override bool CurrentlyFocusableNonMouse => this.fade > 0.5f;

            protected internal override bool MouseOver => base.MouseOver;
        }

        internal class ListButton : OpSimpleImageButton
        {
            public ListButton(MenuModList list, Role role) : base(Vector2.zero, new Vector2(24f, 24f), "", RoleSprite(role))
            {
                this.role = role;
                this.list = list;
                this.list.menuTab.AddItems(this);
                // 462 700, 321 720up/26down
                this._pos = this.list.pos - new Vector2(250f, 600f);
                OnChange();
            }

            public enum Role : int
            {
                Stat = 0,
                ScrollUp = -1,
                ScrollDown = 1
            }

            public readonly Role role;

            public readonly MenuModList list;

            private static string RoleSprite(Role role)
            {
                switch (role)
                {
                    case Role.Stat:
                        return "Menu_Symbol_Show_List";

                    case Role.ScrollUp:
                    case Role.ScrollDown:
                        return "Menu_Symbol_Arrow";
                }
                return "pixel";
            }

            public override void OnChange()
            {
                base.OnChange();
            }

            public override void Signal()
            {
                list.Signal(this, (int)role);
            }
        }

        internal class AlphabetButton : OpSimpleButton
        {
            public AlphabetButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(24f, 24f), "", "")
            {
                this.list = list;
                this.index = index;
                represent = (char)(index + 65); // upper A: 65
                this.text = represent.ToString();
                this.list.menuTab.AddItems(this);
                //480 150 + 20i

                this.greyedOut = ConfigContainer.OptItfABC[index] < 0;
                OnChange();
            }

            public readonly MenuModList list;

            public readonly int index;
            public readonly char represent;

            public override bool CurrentlyFocusableMouse => !greyedOut;

            public override bool CurrentlyFocusableNonMouse => !greyedOut;

            public override void OnChange()
            {
                base.OnChange();
                rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }

            public override void Signal()
            {
                list.Signal(this, (int)index);
            }
        }
    }
}