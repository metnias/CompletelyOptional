using OptionalUI;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        // use Arena's LevelSelector way of doing
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 60f), new Vector2(250f, 650f))
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
            { sideLines[2] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f }; } // Leftside
            else
            { slider = new ListSlider(this); }
            foreach (FSprite line in sideLines) { line.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey); }

            for (int i = 0; i < sideLines.Length; i++) { myContainer.AddChild(sideLines[i]); }

            roleButtons = new ListButton[3];
            roleButtons[0] = new ListButton(this, ListButton.Role.Stat);
            roleButtons[1] = new ListButton(this, ListButton.Role.ScrollUp);
            roleButtons[2] = new ListButton(this, ListButton.Role.ScrollDown);
            if (scrollFit) { roleButtons[1].Hide(); roleButtons[2].Hide(); }

            // mod buttons
            modButtons = new ModButton[ConfigContainer.OptItfs.Length - 1];
            for (int i = 0; i < modButtons.Length; i++)
            { modButtons[i] = new ModButton(this, i); }

            // modless
            if (modButtons.Length < 1)
            {
                roleButtons[0].Hide();
                for (int i = 0; i < abcButtons.Length; i++)
                { abcButtons[i].Hide(); }
            }
        }

        private MenuTab menuTab => this.tab as MenuTab;
        private ConfigContainer cfgContainer => menu.cfgContainer;

        private float abcSlide = -60f, lastAbcSlide = -60f; // -60f (hidden) ~ 40f (slided out)

        private float GetMyAbcSlide(int index, float timeStacker) => Mathf.Clamp(Mathf.Lerp(lastAbcSlide, abcSlide, timeStacker) + (abcButtons.Length - index) * 2.0f, 0f, 30f);

        private float floatScrollPos = 0f, floatScrollVel = 0f;

        private readonly ModButton[] modButtons;
        private readonly AlphabetButton[] abcButtons;
        private readonly FSprite backSide;
        private readonly FSprite[] sideLines;
        private readonly ListButton[] roleButtons;
        private readonly ListSlider slider;
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
            float listBtnAddSize = roleButtons[0].bumpBehav.AddSize;
            sideLines[0].x = 265f; sideLines[0].y = -25f;
            sideLines[1].x = 265f; sideLines[1].y = 663.5f + listBtnAddSize;
            sideLines[0].scaleY = 665.5f - listBtnAddSize;
            sideLines[1].scaleY = 12.5f - listBtnAddSize;
            if (slider == null)
            {
                sideLines[2].x = -15f; sideLines[2].y = -25f;
                sideLines[2].scaleY = 700f;
            }
        }

        protected internal override bool MouseOver => base.MouseOver
            || (this.MousePos.x > -15f && this.MousePos.x < 295f && this.MousePos.y > 140f && this.MousePos.y < 700f);

        public override void Update()
        {
            base.Update();
            bool listFocused = ConfigContainer.focusedElement is IAmPartOfModList || (MenuMouseMode && MouseOver);
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
                if (index != ConfigContainer.activeItfIndex)
                { PlaySound(SoundID.MENU_MultipleChoice_Clicked); cfgContainer.ChangeActiveMod(index); }
                else { PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked); }
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
                this.mute = true;

                // Get Type
                if (itf is InternalOI)
                {
                    if ((itf as InternalOI).reason == InternalOI.Reason.Error) { type = ItfType.Error; CreateIcon(IconContext.Error); }
                    else if ((itf as InternalOI).reason == InternalOI.Reason.TestOI) { type = ItfType.Inconfigurable; }
                    else { type = ItfType.Blank; }
                }
                else { type = itf.Configurable() ? ItfType.Configurable : ItfType.Inconfigurable; }
                greyedOut = type == ItfType.Blank;

                this.text = itf.rwMod.ModID;
                if (itf.rwMod.Version != RainWorldMod.authorNull)
                {
                    this.labelVer = CreateFLabel(itf.rwMod.Version);
                    this.myContainer.AddChild(this.labelVer);
                    this.labelVer.y = this.label.y;
                    this.labelVer.alignment = FLabelAlignment.Right;
                }

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

            private FSprite icon;

            /// <summary>
            /// Star: Multiplayer_Star x 0.8
            /// Error: Sandbox_SmallQuestionmark
            /// UnsavedChange: Menu_Symbol_Clear_All
            /// SavedChange: Menu_Symbol_CheckBox > fade
            /// </summary>
            /// <param name="context">0: star, 1: Error, 2: UnsavedChange, 3: SavedChange</param>
            private void CreateIcon(IconContext context)
            {
                if (icon != null) { icon.isVisible = false; icon.RemoveFromContainer(); icon = null; }
                string name = "";
                switch (context)
                {
                    case IconContext.Star: name = "Multiplayer_Star"; break;
                    case IconContext.Error: name = "Sandbox_SmallQuestionmark"; break;
                    case IconContext.UnsavedChange: name = "keyArrowA"; break;
                    case IconContext.SavedChange: name = "keyArrowB"; break;
                }
                icon = new FSprite(name) { anchorX = 0.5f, anchorY = 0.5f, x = 1.01f, y = this.size.y / 2f, color = this.colorEdge };
                switch (context)
                {
                    case IconContext.Star: icon.scale = 0.5f; break;
                    case IconContext.Error: icon.scale = 0.8f; break;
                    case IconContext.UnsavedChange:
                    case IconContext.SavedChange:
                        icon.rotation = 90f; icon.x = 0.01f; break;
                }
                this.myContainer.AddChild(icon);
            }

            private enum IconContext
            {
                Star, Error, UnsavedChange, SavedChange
            }

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
                this.label.x = 10f;
                if (this.labelVer != null) { this.labelVer.x = this.size.x; }

                this.rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                if (lastFade >= 1f) { return; }
                base.GrafUpdate(timeStacker);
                if (greyedOut) { this.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey); }
                this.label.alpha = Mathf.Pow(1f - Mathf.Lerp(lastFade, fade, timeStacker), 2f);
                if (this.labelVer != null)
                { this.labelVer.alpha = this.label.alpha; this.labelVer.color = this.label.color; }
                if (this.icon != null)
                { this.icon.color = this.label.color; this.icon.alpha = this.label.alpha; }
            }

            public override void Update()
            {
                this.lastFade = this.fade;
                this.fade = Mathf.Clamp01(Mathf.Max(index - list.floatScrollPos, list.floatScrollPos - index - MenuModList.scrollVisible));
                // if (fade >= 1f) { return; }

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
                        // pressing right: jump to its alphabet button
                        // pressing left: jump to slider if it's available
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
                        this._pos = new Vector2(466f, 700f); // x462f : centered
                        this.soundClick = SoundID.MENU_Switch_Arena_Gametype; break;
                    case Role.ScrollUp:
                        this._pos = new Vector2(321f, 720f);
                        this.soundClick = SoundID.MENU_First_Scroll_Tick; this.canHold = true; break;
                    case Role.ScrollDown:
                        this._pos = new Vector2(321f, 26f); this.sprite.rotation = 180f;
                        this.soundClick = SoundID.MENU_First_Scroll_Tick; this.canHold = true; break;
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

            private readonly MenuModList list;

            private static string RoleSprite(Role role)
            {
                switch (role)
                {
                    case Role.Stat:
                        return "Menu_InfoI"; //"Menu_Symbol_Show_List";

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
                ConfigContainer.instance.allowFocusMove = false;
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
                this._pos = new Vector2(450f, 150f + (25 - index) * 20f); // x: 480f
                this.soundClick = SoundID.MENU_First_Scroll_Tick;

                this.unused = ConfigContainer.OptItfABC[index] < 0;
                OnChange();
            }

            private readonly MenuModList list;

            public readonly int index;
            public readonly char represent;

            public override bool CurrentlyFocusableMouse => !greyedOut && slideOut;

            public override bool CurrentlyFocusableNonMouse => !greyedOut && slideOut;

            private bool slideOut => list.GetMyAbcSlide(index, 0f) >= 30f;
            private readonly bool unused;

            public override void OnChange()
            {
                base.OnChange();
                this._size = new Vector2(18f, 18f);
                this.rectH.pos = new Vector2(-3f, -3f);
                LabelPlaceAtCenter(this.label, -3f, -3f, 24f, 24f);
                rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                float mySlide = list.GetMyAbcSlide(index, timeStacker);
                this._pos.x = 450f + mySlide;
                if (slideOut) { this.rectH.Show(); }
                else { this.rectH.Hide(); }
                base.GrafUpdate(timeStacker);
                mySlide = Mathf.Clamp01(mySlide / 30f);
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
                ConfigContainer.instance.allowFocusMove = false;
                list.Signal(this, (int)index);
            }
        }

        internal class ListSlider : OpSlider, IAmPartOfModList
        {
            public ListSlider(MenuModList list)
                : base(new Vector2(458f, 60f), "_ModList_",
                      new IntVector2(0, System.Math.Max(0, ConfigContainer.OptItfs.Length - scrollVisible + 1)),
                      650, true, 0)
            {
                this.list = list;
                this.subtleCircle = new FSprite("Menu_Subtle_Slider_Nob")
                { anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.subtleCircle);
            }

            private const float subSize = 10f;

            private readonly MenuModList list;
            private readonly FSprite subtleCircle;

            public override bool CurrentlyFocusableMouse => base.CurrentlyFocusableMouse;
            public override bool CurrentlyFocusableNonMouse => false;

            protected internal override bool MouseOver => Custom.DistLess(new Vector2(0f, this.subtleCircle.y), this.MousePos, subSize / 2f);

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);
                this.rect.Hide();
                this.lineSprites[0].isVisible = false; this.lineSprites[3].isVisible = false;
                this.subtleCircle.x = 0f; this.subtleCircle.y = this.mul * (float)(list.floatScrollPos - this.min);
                this.subtleCircle.scale = 10f / subSize;
                this.subtleCircle.color = this.rect.colorEdge;

                this.lineSprites[1].isVisible = true; this.lineSprites[1].y = -25f;
                this.lineSprites[2].isVisible = true;
                float cutPos = this.subtleCircle.y - subSize / 2f;
                this.lineSprites[1].scaleY = 25f + cutPos;
                this.lineSprites[2].y = cutPos + subSize;
                this.lineSprites[2].scaleY = 700f - (cutPos + subSize);
            }

            public override void Update()
            {
                base.Update();

                if (!this.held) { this.SetValueInt(list.scrollPos); }
            }

            public override void OnChange()
            {
                base.OnChange();
                if (this.held)
                {
                    list.scrollPos = this.GetValueInt();
                    list.ClampScrollPos();
                }
            }
        }

        private interface IAmPartOfModList
        { }
    }
}