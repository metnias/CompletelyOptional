using OptionalUI;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        // use Arena's LevelSelector way of doing
        public MenuModList(ConfigMenuTab tab) : base(new Vector2(208f, 60f), new Vector2(250f, 650f))
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

        private ConfigMenuTab menuTab => this.tab as ConfigMenuTab;
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
            float targetPos = scrollPos;
            if (slider != null && slider.held) { targetPos = slider.floatPos; }
            floatScrollPos = Custom.LerpAndTick(this.floatScrollPos, targetPos, 0.01f, 0.01f);
            floatScrollVel *= Custom.LerpMap(System.Math.Abs(targetPos - floatScrollPos), 0.25f, 1.5f, 0.45f, 0.99f);
            floatScrollVel += Mathf.Clamp(targetPos - floatScrollPos, -2.5f, 2.5f) / 2.5f * 0.15f;
            floatScrollVel = Mathf.Clamp(floatScrollVel, -1.2f, 1.2f);
            floatScrollPos += floatScrollVel;
        }

        private void Signal(UIelement element, int index = -1)
        {
            if (element is ModButton)
            { // Switch Mod
                if (index != ConfigContainer.activeItfIndex)
                { ConfigContainer.ChangeActiveMod(index); }
                return;
            }
            if (element is AlphabetButton)
            { // Scroll to Alphabet
                int target = ConfigContainer.OptItfABC[index];
                if (target > scrollPos)
                { // Going down; try showing all of this alphabet
                    for (int i = index + 1; i <= ConfigContainer.OptItfABC.Length; i++)
                    {
                        if (i == ConfigContainer.OptItfABC.Length) { target += scrollVisible; break; } // Last alphabet
                        if (ConfigContainer.OptItfABC[i] > 0)
                        { target = Custom.IntClamp(ConfigContainer.OptItfABC[i] - 1, target, target + scrollVisible); break; }
                    }
                }
                else { target--; }
                ScrollToShow(target);
                ClampScrollPos();
                cfgContainer.FocusNewElement(modButtons[ConfigContainer.OptItfABC[index] - 1]);
                return;
            }
            if (element is ListButton)
            {
                if (index == 0)
                { // Switch Mod to InternalOI_Stats
                    ConfigContainer.ChangeActiveMod(0);
                }
                else
                { // Scroll Up(-1) or Down(+1)
                    scrollPos += index;
                    ClampScrollPos();
                }
                return;
            }
        }

        private void ClampScrollPos() =>
            scrollPos = Custom.IntClamp(scrollPos, 0, System.Math.Max(0, modButtons.Length - scrollVisible + 1));

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
                this._pos = new Vector2(list.pos.x, MyPos);
                glow = new GlowGradient(myContainer, Vector2.zero, this.size);

                // Get Type
                if (itf is InternalOI)
                {
                    if ((itf as InternalOI).reason == InternalOI.Reason.Error) { type = ItfType.Error; CreateIcon(IconContext.Error); }
                    else if ((itf as InternalOI).reason == InternalOI.Reason.TestOI) { type = ItfType.Inconfigurable; }
                    else { type = ItfType.Blank; this.mute = true; }
                }
                else { type = itf.Configurable() ? ItfType.Configurable : ItfType.Inconfigurable; }
                greyedOut = type == ItfType.Blank;

                switch (type)
                {
                    case ItfType.Configurable:
                        if (itf.rwMod.author != RainWorldMod.authorNull)
                        { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Configure).Replace("<ModID>", itf.rwMod.ModID).Replace("<ModAuthor>", itf.rwMod.author); }
                        else { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_ConfigureAnonymous).Replace("<ModID>", itf.rwMod.ModID); }
                        break;

                    case ItfType.Error:
                    case ItfType.Inconfigurable:
                        if (itf.rwMod.author != RainWorldMod.authorNull)
                        { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Display).Replace("<ModID>", itf.rwMod.ModID).Replace("<ModAuthor>", itf.rwMod.author); }
                        else { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_DisplayAnonymous).Replace("<ModID>", itf.rwMod.ModID); }
                        break;

                    default:
                    case ItfType.Blank:
                        if (itf.rwMod.author != RainWorldMod.authorNull)
                        { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Blank).Replace("<ModID>", itf.rwMod.ModID).Replace("<ModAuthor>", itf.rwMod.author); }
                        else { description = OptionalText.GetText(OptionalText.ID.MenuModList_ModButton_Blankest).Replace("<ModID>", itf.rwMod.ModID); }
                        break;
                }

                this.text = itf.rwMod.ModID;
                if (itf.rwMod.version != RainWorldMod.authorNull)
                {
                    this.labelVer = FLabelCreate(itf.rwMod.version);
                    this.myContainer.AddChild(this.labelVer);
                    this.labelVer.y = this.label.y;
                    this.labelVer.alignment = FLabelAlignment.Right;
                }

                this.list.menuTab.AddItems(this);
                Change();
                OnClick += Signal;
            }

            private float MyPos => list.pos.y + 650f - (index - list.floatScrollPos) * height;

            public const float height = 25f;

            private readonly MenuModList list;

            public readonly int index;
            private OptionInterface itf => ConfigContainer.OptItfs[index];
            private readonly FLabel labelVer;
            private float fade, lastFade; // 0f : visible, 1f: invisible

            public readonly ItfType type;

            private FSprite icon;
            private readonly GlowGradient glow;

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

            protected internal override void Change()
            {
                UpdateColor();
                base.Change();
                this.label.alignment = FLabelAlignment.Left;
                this.label.x = 10f;
                if (this.labelVer != null) { this.labelVer.x = this.size.x; }

                this.rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                if (lastFade >= 1f)
                {
                    this.label.alpha = 0f;
                    if (this.labelVer != null) { this.labelVer.alpha = 0f; }
                    if (this.icon != null) { this.icon.alpha = 0f; }
                    return;
                }
                base.GrafUpdate(timeStacker);
                this.rectH.Hide();
                if (greyedOut) { this.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey); }
                this.label.alpha = Mathf.Pow(1f - Mathf.Lerp(lastFade, fade, timeStacker), 2f);
                if (this.labelVer != null)
                { this.labelVer.alpha = this.label.alpha; this.labelVer.color = this.label.color; }
                if (this.icon != null)
                { this.icon.color = this.label.color; this.icon.alpha = this.label.alpha; }
                glow.color = label.color;
                glow.alpha = greyedOut ? 0.0f : rectH.sprites[0].alpha * label.alpha * 0.6f; // Use highlight
            }

            private static int scrollCounter = 0; private bool lastFocused = false;

            public override void Update()
            {
                this.lastFade = this.fade;
                this.fade = Mathf.Clamp01(Mathf.Max(list.floatScrollPos - index + 1, index - list.floatScrollPos - MenuModList.scrollVisible));
                if (ConfigContainer.focusedElement == this && fade >= 1f) { list.ScrollToShow(this.index); }
                if (fade >= 1f) { return; }

                this.pos = new Vector2(list.pos.x, MyPos);
                base.Update();

                if (MenuMouseMode)
                {
                    if (this.Focused())
                    {
                        if (Input.GetMouseButtonDown(1)) // Starred
                        { list.Star(index); }
                        else if (menu.mouseScrollWheelMovement != 0 && !list.roleButtons[1].isInactive)
                        {
                            list.Signal(list.roleButtons[menu.mouseScrollWheelMovement < 0 ? 1 : 2], System.Math.Sign(menu.mouseScrollWheelMovement) * 3);
                            list.PlaySound(SoundID.MENU_Scroll_Tick);
                        }
                    }
                }
                else
                {
                    if (this.Focused())
                    {
                        if (CtlrInput.mp && !LastCtlrInput.mp) // Starred
                        { list.Star(index); }

                        if (CtlrInput.y != 0)
                        {
                            if (CtlrInput.y != LastCtlrInput.y) { scrollCounter = 0; }
                            else { scrollCounter++; }
                        }

                        if (list.roleButtons[1].isInactive) { return; }

                        if (this.index == list.scrollPos + 1 && !list.roleButtons[1].greyedOut)
                        {
                            if (CtlrInput.y > 0)
                            {
                                ConfigContainer.instance.allowFocusMove = false;
                                if (scrollCounter == 0) { ScrollSignal(true, true); }
                                else if (scrollCounter > ModConfigMenu.DASinit && scrollCounter % (ModConfigMenu.DASdelay / 2) == 1)
                                { ScrollSignal(true, false); }
                            }
                        }
                        else if (this.index == list.scrollPos + scrollVisible && !list.roleButtons[2].greyedOut)
                        {
                            if (CtlrInput.y < 0)
                            {
                                ConfigContainer.instance.allowFocusMove = false;
                                if (scrollCounter == 0) { ScrollSignal(false, true); }
                                else if (scrollCounter > ModConfigMenu.DASinit && scrollCounter % (ModConfigMenu.DASdelay / 2) == 1)
                                { ScrollSignal(false, false); }
                            }
                        }
                    }
                    lastFocused = this.Focused();
                    // pressing right: jump to its alphabet button
                    // pressing left: jump to slider if it's available
                }

                void ScrollSignal(bool up, bool first)
                {
                    if (!lastFocused) { return; }
                    list.Signal(list.roleButtons[up ? 1 : 2], up ? -1 : 1);
                    list.PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
                    ModButton nextFocus = list.modButtons[Custom.IntClamp(index + (up ? -2 : 0), 0, list.modButtons.Length - 1)];
                    bool prevMute = nextFocus.mute; nextFocus.mute = true;
                    ConfigContainer.instance.FocusNewElement(nextFocus);
                    nextFocus.mute = prevMute;
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

            public void Signal(UIfocusable self)
            {
                list.Signal(this, index);
            }

            protected internal override bool CurrentlyFocusableMouse => this.fade < 0.5f;
            protected internal override bool CurrentlyFocusableNonMouse => this.fade < 0.5f;

            protected internal override bool MouseOver => base.MouseOver;
        }

        internal class ListButton : OpSimpleImageButton, IAmPartOfModList
        {
            public ListButton(MenuModList list, Role role) : base(Vector2.zero, new Vector2(24f, 24f), RoleSprite(role))
            {
                this.role = role;
                this.list = list;
                this.list.menuTab.AddItems(this);

                switch (this.role)
                {
                    case Role.Stat:
                        this._pos = new Vector2(466f, 700f); // x462f : centered
                        this.soundClick = SoundID.MENU_Switch_Arena_Gametype; this.greyedOut = true;
                        OnClick += Signal;
                        description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_Stat); break;
                    case Role.ScrollUp:
                        this._pos = new Vector2(321f, 720f);
                        this.soundClick = SoundID.None;
                        OnPressInit += Signal; OnPressHold += SignalHold;
                        description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_ScrollUp); break;
                    case Role.ScrollDown:
                        this._pos = new Vector2(321f, 26f); this.sprite.rotation = 180f;
                        this.soundClick = SoundID.None;
                        OnPressInit += Signal; OnPressHold += SignalHold;
                        description = OptionalText.GetText(OptionalText.ID.MenuModList_ListButton_ScrollDw); break;
                }
                Change();
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

            protected internal override bool CurrentlyFocusableNonMouse
            {
                get
                {
                    if (role == Role.Stat) { return true; }
                    return false;
                }
            }

            public override void Update()
            {
                base.Update();
                this.mute = !MenuMouseMode;
                switch (role)
                {
                    case Role.Stat:
                        greyedOut = ConfigContainer.activeItfIndex == 0;
                        break;

                    case Role.ScrollUp:
                        greyedOut = list.scrollPos == 0;
                        break;

                    case Role.ScrollDown:
                        greyedOut = list.scrollPos > list.modButtons.Length - MenuModList.scrollVisible;
                        break;
                }
            }

            public void Signal(UIfocusable self)
            {
                list.Signal(this, (int)role);
                if (role != Role.Stat) { PlaySound(SoundID.MENU_First_Scroll_Tick); }
            }

            public void SignalHold(UIfocusable self)
            {
                list.Signal(this, (int)role);
                PlaySound(SoundID.MENU_Scroll_Tick);
            }
        }

        internal class AlphabetButton : OpSimpleButton, IAmPartOfModList
        {
            public AlphabetButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(24f, 24f), "")
            {
                this.list = list;
                this.index = index;
                represent = (char)(index + 65); // upper A: 65
                this.text = represent.ToString();
                this.list.menuTab.AddItems(this);
                this._pos = new Vector2(450f, 150f + (25 - index) * 20f); // x: 480f
                this.soundClick = SoundID.MENU_First_Scroll_Tick;
                glow = new GlowGradient(myContainer, -0.5f * size, 2f * size);

                this.unused = ConfigContainer.OptItfABC[index] < 0;
                Change();
                OnClick += Signal;
            }

            private readonly GlowGradient glow;

            protected internal override string DisplayDescription()
            {
                return OptionalText.GetText(MenuMouseMode ? OptionalText.ID.MenuModList_AlphabetButton_MouseDesc : OptionalText.ID.MenuModList_AlphabetButton_NonMouseDesc).Replace("<Letter>", represent.ToString());
            }

            private readonly MenuModList list;

            public readonly int index;
            public readonly char represent;

            protected internal override bool CurrentlyFocusableMouse => !greyedOut && slideOut;

            protected internal override bool CurrentlyFocusableNonMouse => !greyedOut && slideOut;

            private bool slideOut => list.GetMyAbcSlide(index, 0f) >= 30f;
            private readonly bool unused;

            protected internal override void Change()
            {
                base.Change();
                this._size = new Vector2(18f, 18f);
                this.rectH.pos = new Vector2(-3f, -3f);
                FLabelPlaceAtCenter(this.label, -3f, -3f, 24f, 24f);
                rect.Hide();
            }

            public override void GrafUpdate(float timeStacker)
            {
                float mySlide = list.GetMyAbcSlide(index, timeStacker);
                this._pos.x = 450f + mySlide;
                base.GrafUpdate(timeStacker);
                this.rectH.Hide();
                mySlide = Mathf.Clamp01(mySlide / 30f);
                this.label.alpha = Mathf.Pow(mySlide, 2f);
                glow.color = label.color;
                glow.alpha = greyedOut || !slideOut ? 0.0f : rectH.sprites[0].alpha * label.alpha * 0.6f; // Use highlight
            }

            public override void Update()
            {
                greyedOut = unused || !slideOut;
                this.bumpBehav.greyedOut = unused;
                base.Update();
            }

            public void Signal(UIfocusable self)
            {
                list.Signal(this, (int)index);
            }
        }

        internal class ListSlider : OpSlider, IAmPartOfModList
        {
            public ListSlider(MenuModList list)
                : base(null, new Vector2(list.pos.x - 30f, list.pos.y), //458f, 60f
                      new IntVector2(0, System.Math.Max(0, ConfigContainer.OptItfs.Length - scrollVisible)),
                      650, true, ConfigContainer.OptItfs.Length - scrollVisible)
            {
                this.list = list;
                this.subtleCircle = new FSprite("Menu_Subtle_Slider_Nob")
                { anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.subtleCircle);
                this.list.menuTab.AddItems(this);

                description = OptionalText.GetText(OptionalText.ID.MenuModList_ListSlider_Desc);
            }

            private const float subSize = 10f;

            private readonly MenuModList list;
            private readonly FSprite subtleCircle;

            protected internal override bool CurrentlyFocusableMouse => base.CurrentlyFocusableMouse;
            protected internal override bool CurrentlyFocusableNonMouse => false;

            protected internal override bool MouseOver => Custom.DistLess(new Vector2(15f, this.subtleCircle.y), this.MousePos, subSize / 2f);

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);
                this.rect.Hide(); this.label.isVisible = false;
                this.lineSprites[0].isVisible = false; this.lineSprites[3].isVisible = false;
                this.subtleCircle.x = 15f; this.subtleCircle.y = this.mul * (float)(this.max - (MenuMouseMode && held ? floatPos : list.floatScrollPos));
                this.subtleCircle.scale = 10f / subSize;
                this.subtleCircle.color = this.rect.colorEdge;

                this.lineSprites[1].isVisible = true; this.lineSprites[1].y = -25f;
                this.lineSprites[2].isVisible = true;
                float cutPos = this.subtleCircle.y - (subSize / 2f);
                this.lineSprites[1].scaleY = 25f + cutPos;
                this.lineSprites[2].y = this.size.y + 25f;
                this.lineSprites[2].scaleY = this.size.y + 25f - (cutPos + subSize);
            }

            public override void Update()
            {
                base.Update();
                if (!this.held) { this.SetValueInt(this.max - list.scrollPos); }
                else if (this.MenuMouseMode)
                {
                    floatPos = this.max - Mathf.Clamp(this.MousePos.y / this.mul, 0f, this.max);
                    list.scrollPos = Mathf.RoundToInt(floatPos);
                    list.ClampScrollPos();
                }
            }

            public float floatPos;
        }

        internal interface IAmPartOfModList
        { }
    }
}