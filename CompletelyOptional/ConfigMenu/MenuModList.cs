using OptionalUI;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        // use Arena's LevelSelector way of doing
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

        private MenuTab menuTab => this.tab as MenuTab;
        private ConfigContainer cfgContainer => menu.cfgContainer;

        private float abcSlide = 40f, lastAbcSlide = 40f; // -60f (hidden) ~ 40f (slided out)

        private float GetMyAbcSlide(int index, float timeStacker) => Mathf.Clamp(Mathf.Lerp(lastAbcSlide, abcSlide, timeStacker) + index * 2.0f, 0f, 40f);

        private float floatScrollPos = 0f, floatScrollVel = 0f;

        private readonly ModButton[] modButtons;
        private readonly AlphabetButton[] abcButtons;
        private readonly FSprite backSide;
        private readonly FSprite[] sideLines;
        private readonly ListButton[] roleButtons;
        private int scrollPos = 0;
        private const int scrollVisible = 26;

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetIndex">targetIndex = itfIndex - 1</param>
        internal void ScrollToShow(int targetIndex)
        {
            if (scrollPos > targetIndex) { scrollPos = targetIndex; }
            else if (scrollPos + scrollVisible < targetIndex) { scrollPos = targetIndex - scrollVisible; }
            ClampScrollPos();
        }

        internal ModButton GetCurrentModButton() => modButtons[ConfigContainer.activeItfIndex - 1];

        // ModList:
        // ABC button, Mod button shows Name(Left) and Version(right)
        // Save first mod for each letter, and scroll to it
        // if name is too long, add ...
        // Tab: It will now have ^ and v instead of having 20 Limit
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
            bool listFocused = ConfigContainer.focusedElement is IAmPartOfModList;
            // abc buttons slide
            lastAbcSlide = abcSlide;
            abcSlide += 4f / UIelement.frameMulti * (listFocused ? 1f : -1f);
            abcSlide = Mathf.Clamp(abcSlide, -60f, 40f);
            // scrollPos
            floatScrollPos = Custom.LerpAndTick(this.floatScrollPos, scrollPos, 0.01f, 0.01f);
            floatScrollVel *= Custom.LerpMap(System.Math.Abs(scrollPos - floatScrollPos), 0.25f, 1.5f, 0.45f, 0.99f);
            floatScrollVel += Mathf.Clamp(scrollPos - floatScrollPos, -2.5f, 2.5f) / 2.5f * 0.15f;
            floatScrollVel = Mathf.Clamp(floatScrollVel, -1.2f, 1.2f);
            floatScrollPos += floatScrollVel;
        }

        private void Signal(UIelement element, int index = -1)
        {
            if (element is ModButton)
            { // Switch Mod
                cfgContainer.ChangeActiveMod(index);
            }
            else if (element is AlphabetButton)
            { // Scroll to Alphabet
                ScrollToShow(ConfigContainer.OptItfABC[index] - 1);
                ClampScrollPos();
                cfgContainer.FocusNewElement(modButtons[ConfigContainer.OptItfABC[index] - 1]);
            }
            else if (element is ListButton)
            {
                if (index == 0)
                { // Switch Mod to InternalOI_Stats
                    cfgContainer.ChangeActiveMod(0);
                }
                else
                { // Scroll Up(-1) or Down(+1)
                    scrollPos += index;
                    ClampScrollPos();
                }
            }
        }

        private void ClampScrollPos() =>
            scrollPos = Custom.IntClamp(scrollPos, 0, System.Math.Max(0, ConfigContainer.OptItfs.Length - scrollVisible + 1));

        private void Star(int index)
        {
            // Add/Remove ID to/from star list
            // ConfigContainer.OptItfID[index];
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton, IAmPartOfModList
        {
            public ModButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(250f, height), "Mod List")
            {
                this.list = list;
                this.index = index + 1; // starts from 1; index 0 is for StatOI
                this.list.menuTab.AddItems(this);
                this._pos = MyPos;

                // Get Type
                if (itf is InternalOI)
                {
                    if ((itf as InternalOI).reason == InternalOI.Reason.Error) { type = ItfType.Error; }
                    else { type = ItfType.Blank; }
                }
                else { type = itf.Configurable() ? ItfType.Configurable : ItfType.Inconfigurable; }
                greyedOut = type == ItfType.Blank;

                this.text = itf.rwMod.ModID;
                this.labelVer = OpLabel.CreateFLabel(itf.rwMod.Version);
                this.myContainer.AddChild(this.labelVer);

                this.list.menuTab.AddItems(this);
                OnChange();
            }

            private Vector2 MyPos => list.pos + new Vector2(0f, 650f - (index - list.floatScrollPos) * height);

            public const float height = 25f;

            private readonly MenuModList list;

            public readonly int index;
            private OptionInterface itf => ConfigContainer.OptItfs[index];
            private readonly FLabel labelVer;
            private float fade, lastFade; // 0f : visible, 1f: invisible

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
                UpdateColor();
                base.OnChange();
                this.label.alignment = FLabelAlignment.Left;
                this.labelVer.alignment = FLabelAlignment.Right;
                this.rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                if (lastFade >= 1f) { return; }
                base.GrafUpdate(timeStacker);
                this.label.alpha = Mathf.Pow(1f - Mathf.Lerp(lastFade, fade, timeStacker), 2f);
            }

            public override void Update()
            {
                this.lastFade = this.fade;
                this.fade = Mathf.Clamp01(Mathf.Max(index - list.floatScrollPos, list.floatScrollPos - index - MenuModList.scrollVisible));
                if (fade >= 1f) { return; }

                base.Update();
                this._pos = MyPos;

                if (this.Focused())
                {
                    if (MenuMouseMode)
                    {
                        if (Input.GetMouseButtonDown(1)) // Starred
                        { list.Star(index); }
                    }
                    else
                    {
                        if (menu.input.mp && !menu.lastInput.mp) // Starred
                        { list.Star(index); }
                    }
                }
            }

            private Color? setColor;

            public void SetColor(Color? color)
            { setColor = color; UpdateColor(); }

            public void UpdateColor()
            {
                if (setColor.HasValue) { colorEdge = setColor.Value; return; }
                if (greyedOut) { return; }
                if (type == ItfType.Error) { colorEdge = cError; return; }
                if (ConfigContainer.OptItfChanged[index + 1]) { colorEdge = cChange; return; }
                colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            }

            private static Color cError = new Color(0.8f, 0.1f, 0.2f);
            private static Color cChange = new Color(0.8f, 0.8f, 0.2f);

            public override void Signal()
            {
                list.Signal(this, index);
            }

            public override bool CurrentlyFocusableMouse => this.fade < 0.5f;
            public override bool CurrentlyFocusableNonMouse => this.fade < 0.5f;

            protected internal override bool MouseOver => base.MouseOver;
        }

        internal class ListButton : OpSimpleImageButton, IAmPartOfModList
        {
            public ListButton(MenuModList list, Role role) : base(Vector2.zero, new Vector2(24f, 24f), "", RoleSprite(role))
            {
                this.role = role;
                this.list = list;
                this.list.menuTab.AddItems(this);

                switch (this.role)
                {
                    case Role.Stat:
                        this._pos = new Vector2(462f, 700f); break;
                    case Role.ScrollUp:
                        this._pos = new Vector2(321f, 720f); break;
                    case Role.ScrollDown:
                        this._pos = new Vector2(321f, 26f); this.sprite.rotation = 180f; break;
                }
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

            public override void Update()
            {
                base.Update();
                switch (role)
                {
                    case Role.Stat:
                        greyedOut = ConfigContainer.activeItfIndex == 0;
                        break;

                    case Role.ScrollUp:
                        greyedOut = list.scrollPos == 0;
                        break;

                    case Role.ScrollDown:
                        greyedOut = list.scrollPos > list.modButtons.Length - MenuModList.scrollVisible + 1;
                        break;
                }
            }

            public override void Signal()
            {
                list.Signal(this, (int)role);
            }
        }

        internal class AlphabetButton : OpSimpleButton, IAmPartOfModList
        {
            public AlphabetButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(24f, 24f), "", "")
            {
                this.list = list;
                this.index = index;
                represent = (char)(index + 65); // upper A: 65
                this.text = represent.ToString();
                this.list.menuTab.AddItems(this);
                this._pos = new Vector2(440f, 150f + (25 - index) * 20f); // x: 480f

                this.unused = ConfigContainer.OptItfABC[index] < 0;
                OnChange();
            }

            public readonly MenuModList list;

            public readonly int index;
            public readonly char represent;

            public override bool CurrentlyFocusableMouse => !greyedOut && slideOut;

            public override bool CurrentlyFocusableNonMouse => !greyedOut && slideOut;

            private bool slideOut => list.GetMyAbcSlide(index, 0f) >= 40f;
            private readonly bool unused;

            public override void OnChange()
            {
                base.OnChange();
                rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                float mySlide = list.GetMyAbcSlide(index, timeStacker);
                this._pos.x = 440f + mySlide;
                if (slideOut) { this.rectH.Show(); }
                else { this.rectH.Hide(); }
                base.GrafUpdate(timeStacker);
                mySlide = Mathf.Clamp01(mySlide / 40f);
                this.label.alpha = Mathf.Pow(mySlide, 2f);
            }

            public override void Update()
            {
                greyedOut = unused || !slideOut;
                this.bumpBehav.greyedOut = unused;
                base.Update();
            }

            public override void Signal()
            {
                list.Signal(this, (int)index);
            }
        }

        private interface IAmPartOfModList
        { }
    }
}