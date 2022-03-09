using OptionalUI;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special UI used internally in CM for switching tabs
    /// </summary>
    internal class ConfigTabController : UIelement, ICanBeFocused
    {
        public ConfigTabController(MenuTab tab) : base(new Vector2(520f, 120f) - UIelement._offset, new Vector2(40f, 600f))
        {
            tab.AddItems(this);
            tabButtons = new TabSelectButton[tabButtonLimit];
            topIndex = 0;
            scrollButtons = new TabScrollButton[2];
            scrollButtons[0] = new TabScrollButton(true, this);
            scrollButtons[1] = new TabScrollButton(false, this);

            rectCanvas = new DyeableRect(this.myContainer, new Vector2(543f, 105f) - UIelement._offset - this.pos, new Vector2(630f, 630f), true) { fillAlpha = 0.5f };
            // author version license: 908 30, 245 86
            rectPpty = new DyeableRect(this.myContainer, new Vector2(908f, 30f) - UIelement._offset - this.pos, new Vector2(245f, 86f), true) { hiddenSide = DyeableRect.HiddenSide.Top };

            OnChange();
        }

        // Todo: load saved tab no
        // display specification of mods
        // add effect for focused and selected

        private readonly DyeableRect rectCanvas, rectPpty;

        internal MenuTab menuTab => this.tab as MenuTab;

        internal TabSelectButton[] tabButtons;
        internal TabScrollButton[] scrollButtons;
        internal const int tabButtonLimit = 10;

        public bool GreyedOut => false;

        public bool CurrentlyFocusableMouse => ConfigContainer.activeInterface.Tabs.Length > 1;

        public bool CurrentlyFocusableNonMouse => ConfigContainer.activeInterface.Tabs.Length > 1;

        public Rect FocusRect =>
            new Rect(tabButtons[focusedIndex].pos.x, tabButtons[focusedIndex].pos.y,
                tabButtons[focusedIndex].size.x, tabButtons[focusedIndex].size.y);

        internal int index
        {
            get { return ConfigContainer.activeTabIndex; }
            set
            {
                if (ConfigContainer.activeTabIndex == value)
                {
                    PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                    return;
                }
                PlaySound(SoundID.MENU_MultipleChoice_Clicked);
                ConfigContainer.ChangeActiveTab(value);
                OnChange();
            }
        }

        public override void Reset()
        {
            base.Reset();
            OnChange();
            Update();
            GrafUpdate(0f);
        }

        /// <summary>
        /// Current activeInterface's <see cref="OpTab"/> count
        /// </summary>
        public static int TabCount => ConfigContainer.activeInterface.Tabs.Length;

        private int _tabCount = -1;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            rectCanvas.colorEdge = ConfigContainer.activeTab.colorCanvas;
            rectCanvas.colorFill = MenuColorEffect.MidToVeryDark(ConfigContainer.activeTab.colorCanvas);

            rectCanvas.GrafUpdate(timeStacker); rectPpty.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            rectCanvas.Update(); rectPpty.Update();

            if (TabCount != _tabCount) { OnChange(); }

            lastScrollBump = scrollBump;
            scrollBump = Mathf.Lerp(scrollBump, 0f, 0.2f);

            // Scroll to Focus
            if (focusedIndex < topIndex)
            {
                scrollBump = Mathf.Max(scrollBump - 2.5f * (topIndex - focusedIndex), -8f);
                topIndex = focusedIndex;
            }
            else if (focusedIndex >= topIndex + tabButtonLimit)
            {
                scrollBump = Mathf.Min(scrollBump + 2.5f * (topIndex - focusedIndex), 8f);
                topIndex = focusedIndex - tabButtonLimit - 1;
            }

            if (!MenuMouseMode && this.Focused())
            {
                if (!ConfigContainer.holdElement)
                {
                    // If this gets focus
                    // becomes hold automatically
                    ConfigContainer.holdElement = true;
                    focusedIndex = ConfigContainer.activeTabIndex;
                }
                else if (!CtlrInput.jmp)
                {
                    // X direction
                    if (CtlrInput.x != 0 && LastCtlrInput.x == 0)
                    {
                        ConfigContainer.holdElement = false;
                        menu.cfgContainer.FocusNewElementInDirection(new IntVector2(CtlrInput.x, 0));
                        return;
                    }
                    // Y direction
                    if (menu.input.y != 0 && menu.lastInput.y != menu.input.y)
                    { focusedIndex += System.Math.Sign(menu.input.y); }
                    if (menu.input.y != 0 && menu.lastInput.y == menu.input.y && menu.input.x == 0)
                    { this.scrollInitDelay++; }
                    else
                    { this.scrollInitDelay = 0; }
                    if (this.scrollInitDelay > ModConfigMenu.DASinit)
                    {
                        this.scrollDelay++;
                        if (this.scrollDelay > ModConfigMenu.DASdelay)
                        {
                            this.scrollDelay = 0;
                            if (menu.input.y != 0 && menu.lastInput.y == menu.input.y)
                            { focusedIndex += System.Math.Sign(menu.input.y); }
                        }
                    }
                    else { this.scrollDelay = 0; }

                    if (focusedIndex < 0)
                    {
                        focusedIndex = 0;
                        if (topIndex > 0)
                        { // Scroll Up
                            topIndex--;
                            PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                            scrollBump = Mathf.Max(scrollBump - 4f, -8f);
                        }
                        else
                        { // No more scroll
                            PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard);
                            scrollBump = Mathf.Max(scrollBump - 2f, -8f);
                        }
                    }
                    else if (focusedIndex > _tabCount)
                    { // Escape
                        ConfigContainer.holdElement = false;
                        menu.cfgContainer.FocusNewElementInDirection(new IntVector2(0, menu.input.y));
                        return;
                    }
                    else if (focusedIndex > tabButtonLimit)
                    {
                        if (topIndex < _tabCount - tabButtonLimit)
                        { // Scroll Down
                            topIndex++;
                            PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                            scrollBump = Mathf.Min(scrollBump + 4f, 8f);
                        }
                        else
                        { // Escape to Save Button
                            ConfigContainer.holdElement = false;
                            menu.cfgContainer.FocusNewElement(menuTab.saveButton);
                            return;
                        }
                    }
                    else
                    { // Switch to next button
                        PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                    }
                }
            }
        }

        /// <summary>
        /// bump value for scrolling effect
        /// </summary>
        internal float scrollBump, lastScrollBump;

        /// <summary>
        /// Required for DAS
        /// </summary>
        private int scrollDelay, scrollInitDelay;

        internal int topIndex, focusedIndex;

        public override void OnChange()
        { //Selected Tab/Tab number has changed
            base.OnChange();
            if (_tabCount != TabCount)
            {
                _tabCount = TabCount;
                topIndex = 0;
                focusedIndex = ConfigContainer.savedActiveTabIndex[ConfigContainer.activeItfIndex];
                if (_tabCount > tabButtonLimit)
                { scrollButtons[0].Show(); scrollButtons[1].Show(); }
                else
                { scrollButtons[0].Hide(); scrollButtons[1].Hide(); }
                Refresh();
            }
        }

        private void Refresh()
        {
            scrollBump = 0f; lastScrollBump = 0f;
            if (_tabCount < 2)
            { // No tab button
                foreach (TabSelectButton btn in tabButtons)
                { if (btn != null) { btn.Hide(); } }
                return;
            }

            for (int i = 0; i < tabButtonLimit; i++)
            {
                if (this.tabButtons[i] == null)
                {
                    if (i < _tabCount)
                    { this.tabButtons[i] = new TabSelectButton(i, this); }
                }
                else
                {
                    if (i >= _tabCount) { this.tabButtons[i].Hide(); }
                    else if (this.tabButtons[i].isInactive)
                    { this.tabButtons[i].Reset(); this.tabButtons[i].Show(); }
                }
            }
        }

        internal class TabSelectButton : UIelement
        {
            internal TabSelectButton(int index, ConfigTabController ctrler) : base(Vector2.one, Vector2.one)
            {
                this.buttonIndex = index;
                this.ctrl = ctrler;

                this.bumpBehav = new BumpBehaviour(this);

                this.Reset(); // repos & resize
                this.rect = new DyeableRect(myContainer, this.pos, this.size, true) { hiddenSide = DyeableRect.HiddenSide.Right };
                this.rectH = new DyeableRect(myContainer, this.pos, this.size, false) { hiddenSide = DyeableRect.HiddenSide.Right };
                this.label = OpLabel.CreateFLabel(this.name);
                this.label.alignment = FLabelAlignment.Left;
                this.myContainer.AddChild(this.label);

                this.ctrl.menuTab.AddItems(this);
            }

            internal bool mouseTop, click;
            internal float darken;
            public OpTab representingTab => ConfigContainer.activeInterface.Tabs[tabIndex];
            public string name => representingTab.name;
            public readonly ConfigTabController ctrl;
            public bool active => ctrl.index == tabIndex;

            /// <summary>
            /// Index of this button
            /// </summary>
            public readonly int buttonIndex;

            /// <summary>
            /// Index this Object is representing
            /// </summary>
            internal int tabIndex => buttonIndex + ctrl.topIndex;

            public readonly BumpBehaviour bumpBehav;

            public Color colorButton => representingTab.colorButton;
            public Color colorCanvas => representingTab.colorCanvas;

            /// <summary>
            /// Tab Label
            /// </summary>
            public FLabel label;

            /// <summary>
            /// Tab Boundary
            /// </summary>
            public DyeableRect rect;

            /// <summary>
            /// Tab Highlight
            /// </summary>
            public DyeableRect rectH;

            public override void Reset()
            {
                base.Reset();
                height = Mathf.Min(120f, 600f / Custom.IntClamp(ctrl._tabCount, 1, tabButtonLimit));
                this._pos = ctrl.pos + new Vector2(0f, height * (-tabIndex - 1) + 603f);
                this._size = new Vector2(30f, height - 6f);
                this.GrafUpdate(0f);
            }

            private float height;
            private string lastName;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

                if (!this.active && !this.MouseOver) { this.darken = Mathf.Max(0f, this.darken - 0.0333333351f); }
                else { this.darken = Mathf.Min(1f, this.darken + 0.1f); }

                this.label.rotation = -90f;
                string curName = string.IsNullOrEmpty(this.name) ? tabIndex.ToString() : this.name;
                if (lastName != curName)
                {
                    lastName = curName;
                    if (LabelTest.GetWidth(curName) > height) // Omit name too long
                    { curName = LabelTest.TrimText(curName, height, true); }
                    this.label.text = curName;
                }

                Color color = this.bumpBehav.GetColor(this.colorButton);
                this.label.color = Color.Lerp(MenuColorEffect.MidToDark(color), color, Mathf.Lerp(this.darken, 1f, 0.6f));
                this.label.y = Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker);
                color = Color.Lerp(MenuColorEffect.MidToDark(color), color, this.darken);
                this.rect.colorEdge = color;
                this.rect.colorFill = MenuColorEffect.MidToVeryDark(color);

                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos = this.pos + new Vector2(-this.rect.addSize.x * 0.5f, Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker));
                this.label.x = -addSize * 4f;

                this.rectH.colorEdge = color;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos = this.pos + new Vector2(-this.rectH.addSize.x * 0.5f, Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker));
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Update()
            {
                this.rect.Update(); this.rectH.Update();
                this.bumpBehav.Update();

                if (MenuMouseMode)
                {
                    if (MouseOver)
                    {
                        if (!mouseTop)
                        {
                            mouseTop = true;
                            ctrl.focusedIndex = this.tabIndex;
                            PlaySound(SoundID.MENU_Button_Select_Mouse);
                        }
                        if (string.IsNullOrEmpty(this.name))
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab No <TabIndex>").Replace("<TabIndex>", buttonIndex.ToString())); }
                        else
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab <TabName>").Replace("<TabName>", this.name)); }

                        if (Input.GetMouseButton(0)) { this.click = true; this.bumpBehav.held = true; }
                        else if (this.click)
                        {
                            this.click = false; this.bumpBehav.held = false;
                            ctrl.index = this.tabIndex;
                        }
                    }
                    else
                    {
                        mouseTop = false;
                        if (!Input.GetMouseButton(0)) { this.bumpBehav.held = false; this.click = false; }
                    }
                }
                else
                {
                    if (ctrl.focusedIndex == this.tabIndex)
                    {
                        // Show Description
                        if (string.IsNullOrEmpty(this.name))
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab No <TabIndex>").Replace("<TabIndex>", this.buttonIndex.ToString())); }
                        else
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab <TabName>").Replace("<TabName>", this.name)); }

                        // Press
                        if (CtlrInput.jmp)
                        {
                            if (!LastCtlrInput.jmp) { this.click = true; this.bumpBehav.held = true; }
                        }
                        else if (LastCtlrInput.jmp && this.click)
                        {
                            this.click = false; this.bumpBehav.held = false;
                            ctrl.index = this.tabIndex;
                        }
                    }
                    else { this.click = false; this.bumpBehav.held = false; }
                }
            }
        }

        internal class TabScrollButton : UIelement
        {
            public TabScrollButton(bool up, ConfigTabController ctrl) : base(Vector2.one, new Vector2(30f, 20f))
            {
                this.up = up;
                this.ctrl = ctrl;
                if (up) { this._pos = ctrl.pos + new Vector2(0f, 605f); }
                else { this._pos = ctrl.pos + new Vector2(0f, -25f); }
                bumpBehav = new BumpBehaviour(this);
                this.sprite = new FSprite("Big_Menu_Arrow")
                { rotation = up ? 0f : 180f, scale = 0.5f, x = 15f, y = 10f };
                this.myContainer.AddChild(this.sprite);
                this.ctrl.menuTab.AddItems(this);
            }

            internal readonly FSprite sprite;
            internal readonly bool up;
            private bool mouseTop = false, click = false;
            public readonly BumpBehaviour bumpBehav;
            public readonly ConfigTabController ctrl;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                if (Mathf.Abs(ctrl.scrollBump) > Mathf.Abs(ctrl.scrollBump))
                { bumpBehav.flash += 0.6f; }
                this.sprite.y = 10f + (up ? 1 : -1) * Mathf.Abs(Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker));
                this.sprite.color = bumpBehav.GetColor(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
            }

            public override void Update()
            {
                this.bumpBehav.Update();
                if (MenuMouseMode)
                {
                    if (MouseOver)
                    {
                        if (!mouseTop)
                        {
                            mouseTop = true;
                            PlaySound(SoundID.MENU_Button_Select_Mouse);
                        }

                        if (Input.GetMouseButton(0)) { this.click = true; this.bumpBehav.held = true; }
                        else if (this.click)
                        {
                            this.click = false; this.bumpBehav.held = false;
                            if (up)
                            {
                                if (ctrl.topIndex > 0)
                                { // Scroll Up
                                    ctrl.topIndex--;
                                    PlaySound(SoundID.MENU_Button_Select_Mouse);
                                    ctrl.scrollBump = Mathf.Max(ctrl.scrollBump - 4f, -8f);
                                }
                                else
                                { // No more scroll
                                    PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                                    ctrl.scrollBump = Mathf.Max(ctrl.scrollBump - 2f, -8f);
                                }
                            }
                            else
                            {
                                if (ctrl.topIndex < ctrl._tabCount - tabButtonLimit)
                                { // Scroll Down
                                    ctrl.topIndex++;
                                    PlaySound(SoundID.MENU_Button_Select_Mouse);
                                    ctrl.scrollBump = Mathf.Min(ctrl.scrollBump + 4f, 8f);
                                }
                                else
                                { // No more scroll
                                    PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                                    ctrl.scrollBump = Mathf.Min(ctrl.scrollBump + 2f, 8f);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        mouseTop = false;
                        if (!Input.GetMouseButton(0)) { this.bumpBehav.held = false; this.click = false; }
                    }
                }
            }
        }
    }
}