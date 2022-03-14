using OptionalUI;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special UI used internally in CM for switching tabs
    /// </summary>
    internal class ConfigTabController : UIelement
    {
        public ConfigTabController(MenuTab tab) : base(new Vector2(520f, 120f), new Vector2(40f, 600f))
        {
            tab.AddItems(this);
            tabButtons = new TabSelectButton[tabButtonLimit];
            topIndex = 0;
            scrollButtons = new TabScrollButton[2];
            scrollButtons[0] = new TabScrollButton(true, this);
            scrollButtons[1] = new TabScrollButton(false, this);

            rectCanvas = new DyeableRect(this.myContainer, new Vector2(543f, 105f) - this._pos, new Vector2(630f, 630f), true) { fillAlpha = 0.5f };
            // author version license: 908 30, 245 86
            rectPpty = new DyeableRect(this.myContainer, new Vector2(908f, 30f) - this._pos, new Vector2(245f, 83f), true) { hiddenSide = DyeableRect.HiddenSide.Top };

            OnChange();
        }

        // Todo: load saved tab no
        // display specification of mods
        // add effect for focused and selected

        private readonly DyeableRect rectCanvas, rectPpty;

        internal MenuTab menuTab => this.tab as MenuTab;

        internal TabSelectButton[] tabButtons;
        internal TabScrollButton[] scrollButtons;
        internal const int tabButtonLimit = 8;

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

        internal TabSelectButton GetCurrentTabButton() => tabButtons[ConfigContainer.activeTabIndex - topIndex];

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
            scrollBump = Custom.LerpAndTick(scrollBump, 0f, 0.1f, 0.1f / frameMulti);
        }

        internal void ScrollToFocus(int focusedIndex)
        {
            if (focusedIndex < topIndex)
            {
                //scrollBump = Mathf.Max(scrollBump - 2.5f * (topIndex - focusedIndex), -12f);
                topIndex = focusedIndex;
            }
            else if (focusedIndex >= topIndex + tabButtonLimit)
            {
                //scrollBump = Mathf.Min(scrollBump + 2.5f * (topIndex - focusedIndex), 12f);
                topIndex = focusedIndex - tabButtonLimit + 1;
            }
        }

        /// <summary>
        /// bump value for scrolling effect
        /// </summary>
        internal float scrollBump, lastScrollBump;

        internal int topIndex;

        public override void OnChange()
        { //Selected Tab/Tab number has changed
            base.OnChange();
            if (_tabCount != TabCount)
            {
                _tabCount = TabCount;
                topIndex = 0;
                scrollBump = 0f; lastScrollBump = 0f;

                if (_tabCount > tabButtonLimit)
                { scrollButtons[0].Show(); scrollButtons[1].Show(); }
                else
                { scrollButtons[0].Hide(); scrollButtons[1].Hide(); }
                ScrollToFocus(ConfigContainer.activeTabIndex);
                Refresh();
            }
        }

        private void Refresh()
        {
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
                    else if (this.tabButtons[i].isInactive) { this.tabButtons[i].Show(); }
                    if (!this.tabButtons[i].isInactive) { this.tabButtons[i].Reset(); }
                }
            }
        }

        private void Signal(UItrigger trigger, int index = -1)
        {
            if (trigger is TabSelectButton)
            {
                if (index == ConfigContainer.activeTabIndex) { PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked); return; }
                PlaySound(SoundID.MENU_MultipleChoice_Clicked);
                ConfigContainer.ChangeActiveTab(index); return;
            }
            if (trigger is TabScrollButton)
            {
                if (index < 0)
                {
                    if (topIndex > 0)
                    { // Scroll Up
                        topIndex--; Refresh();
                        PlaySound(index < -1 ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick);
                        scrollBump = Mathf.Max(scrollBump - (index < -1 ? 4f : 6f), -12f);
                    }
                    else
                    { // No more scroll
                        PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                        scrollBump = Mathf.Max(scrollBump - 3f, -12f);
                    }
                }
                else
                {
                    if (topIndex < _tabCount - tabButtonLimit)
                    { // Scroll Down
                        topIndex++; Refresh();
                        PlaySound(index < -1 ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick);
                        scrollBump = Mathf.Min(scrollBump + (index > 1 ? 4f : 6f), 12f);
                    }
                    else
                    { // No more scroll
                        PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Mouse);
                        scrollBump = Mathf.Min(scrollBump + 3f, 12f);
                        return;
                    }
                }
            }
        }

        internal class TabSelectButton : OpSimpleButton
        {
            internal TabSelectButton(int index, ConfigTabController ctrler) : base(Vector2.one, Vector2.one, "", "")
            {
                this.buttonIndex = index;
                this.ctrl = ctrler;

                this.Reset(); // repos & resize
                this.rect.hiddenSide = DyeableRect.HiddenSide.Right;
                this.rectH.hiddenSide = DyeableRect.HiddenSide.Right;
                this.label.alignment = FLabelAlignment.Left;
                this.label.rotation = -90f;
                this.mute = true;
                this.myContainer.AddChild(this.label);

                this.ctrl.menuTab.AddItems(this);
            }

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

            public Color colorButton => representingTab.colorButton;
            public Color colorCanvas => representingTab.colorCanvas;

            private bool isTop => this.buttonIndex == 0;
            private bool isBottom => this.buttonIndex == ConfigTabController.tabButtonLimit - 1;

            public override void Reset()
            {
                base.Reset();

                if (string.IsNullOrEmpty(this.name))
                { this.description = InternalTranslator.Translate("Switch to Tab No <TabIndex>").Replace("<TabIndex>", buttonIndex.ToString()); }
                else
                { this.description = InternalTranslator.Translate("Switch to Tab <TabName>").Replace("<TabName>", this.name); }

                height = Mathf.Min(120f, 600f / Custom.IntClamp(ctrl._tabCount, 1, tabButtonLimit));
                this._pos = ctrl.pos + new Vector2(0f, height * (-buttonIndex - 1) + 603f);
                this._size = new Vector2(30f, height - 6f);

                string curName = string.IsNullOrEmpty(this.name) ? tabIndex.ToString() : this.name;
                if (lastName != curName)
                {
                    lastName = curName;
                    if (LabelTest.GetWidth(curName) > height - 16f) // Omit name too long
                    { curName = LabelTest.TrimText(curName, height - 16f, true); }
                    this.label.text = curName;
                }
                this.OnChange();
                this.label.alignment = FLabelAlignment.Left;
                this.GrafUpdate(0f);
            }

            private float height;
            private string lastName;

            public override void GrafUpdate(float timeStacker)
            {
                colorEdge = this.colorButton;
                colorFill = MenuColorEffect.MidToVeryDark(colorButton);

                base.GrafUpdate(timeStacker);

                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                float bump = Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker);
                this.label.x = -addSize * 4f + 15f; this.label.y = bump + 6f;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos.x = -this.rect.addSize.x * 0.5f; this.rect.pos.y = bump;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos.x = -this.rectH.addSize.x * 0.5f; this.rectH.pos.y = bump;
                this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Update()
            {
                base.Update();

                if (!this.active && !this.MouseOver) { this.darken = Mathf.Max(0f, this.darken - 0.0333333351f / frameMulti); }
                else { this.darken = Mathf.Min(1f, this.darken + 0.1f / frameMulti); }

                if (!MenuMouseMode && this.Focused())
                {
                    if (this.isTop && this.CtlrInput.y < 0)
                    {
                        if (this.CtlrInput.y != this.LastCtlrInput.y)
                        { ctrl.Signal(ctrl.scrollButtons[0], -1); }
                        else if (ConfigContainer.instance.scrollDelay >= ModConfigMenu.DASdelay)
                        { ctrl.Signal(ctrl.scrollButtons[0], -2); }
                    }
                    if (this.isBottom && this.CtlrInput.y > 0)
                    {
                        if (this.CtlrInput.y != this.LastCtlrInput.y)
                        { ctrl.Signal(ctrl.scrollButtons[0], 1); }
                        else if (ConfigContainer.instance.scrollDelay >= ModConfigMenu.DASdelay)
                        { ctrl.Signal(ctrl.scrollButtons[0], 2); }
                    }
                }
            }

            public override void Signal()
            {
                ConfigContainer.instance.allowFocusMove = false;
                ctrl.Signal(this, this.tabIndex);
            }
        }

        internal class TabScrollButton : OpSimpleImageButton
        {
            public TabScrollButton(bool up, ConfigTabController ctrl) : base(Vector2.one, Vector2.one, "", "Big_Menu_Arrow")
            {
                this._size = new Vector2(30f, 20f);
                this.up = up;
                this.ctrl = ctrl;
                if (up) { this._pos = ctrl.pos + new Vector2(0f, 600f); }
                else { this._pos = ctrl.pos + new Vector2(0f, -20f); }

                this.sprite.rotation = up ? 0f : 180f;
                this.sprite.scale = 0.5f;
                this.sprite.x = 15f;
                this.mute = true;
                this.canHold = true;
                this.rect.Hide(); this.rectH.Hide();
                this.ctrl.menuTab.AddItems(this);
            }

            internal readonly bool up;
            private readonly ConfigTabController ctrl;

            public override void OnChange()
            {
                base.OnChange();
                this._size = new Vector2(30f, 20f);
            }

            public override bool CurrentlyFocusableNonMouse => false;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.rect.Hide(); this.rectH.Hide();
                if (Mathf.Abs(ctrl.scrollBump) > Mathf.Abs(ctrl.lastScrollBump))
                { bumpBehav.flash += 0.6f; }
                this.sprite.y = 10f + (up ? 1 : -1) * Mathf.Abs(Mathf.Lerp(ctrl.lastScrollBump, ctrl.scrollBump, timeStacker));
                this.sprite.color = bumpBehav.GetColor(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
            }

            public override void Update()
            {
                this.greyedOut = up ? ctrl.topIndex == 0 : ctrl.topIndex >= ctrl._tabCount - ConfigTabController.tabButtonLimit;
                base.Update();
            }

            public override void Signal()
            {
                ConfigContainer.instance.allowFocusMove = false;
                ctrl.Signal(this, (up ? -1 : 1) * (heldCounter > ModConfigMenu.DASinit ? 2 : 1));
            }
        }
    }
}