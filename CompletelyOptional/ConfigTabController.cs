using Menu;
using OptionalUI;
using System.Collections.Generic;
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
            this.menuTab = tab;
            this.cfgMenu = ModConfigMenu.instance;
            tabButtons = new SelectTab[10];
            omit = omit < 0f ? LabelTest.GetWidth("...", false) : omit;

            OnChange();
        }

        // Add Up/Down arrow

        internal MenuTab menuTab;
        internal ModConfigMenu cfgMenu;

        internal SelectTab[] tabButtons;

        private static float omit = -1f;

        bool ICanBeFocused.Focused { get => _focused; set => _focused = value; }
        bool ICanBeFocused.GreyedOut => false;

        bool ICanBeFocused.CurrentlyFocusableMouse => true;

        bool ICanBeFocused.CurrentlyFocusableNonMouse => true;

        private bool _focused;

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
        }

        public static int GetTabCount() => ConfigContainer.activeInterface.Tabs.Length;

        private int _tabCount = -1;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (MenuTab.logMode || mode == TabMode.NULL)
            {
                ConfigMenu.instance.modCanvasBound.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
                ConfigMenu.instance.modCanvasBound.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey);
            }
            else if (mode == TabMode.single)
            {
                ConfigMenu.instance.modCanvasBound.colorEdge = ConfigMenu.currentTab.colorCanvas;
                ConfigMenu.instance.modCanvasBound.colorFill = DyeableRect.MidToVeryDark(ConfigMenu.currentTab.colorCanvas);
            }
        }

        public override void Update()
        {
            base.Update();
            // If this gets focus
            // becomes hold automatically
            if ((this as ICanBeFocused).Focused)
            {
                foreach (SelectTab btn in this.tabButtons)
                {
                    btn.Update();
                    if (btn.active)
                    {
                        ConfigMenu.instance.modCanvasBound.colorEdge = selector.colorCanvas;
                        ConfigMenu.instance.modCanvasBound.colorFill = DyeableRect.MidToVeryDark(selector.colorCanvas);
                    }
                }
            }
        }

        public override void OnChange()
        { //Selected Tab has changed
            base.OnChange();
            if (_tabCount != GetTabCount())
            {
                Unload();
                _tabCount = GetTabCount();
                if (GetTabCount() == 1)
                {
                    mode = TabMode.single;
                }
                else if (GetTabCount() <= 12)
                {
                    mode = TabMode.tab;
                }
                else if (GetTabCount() <= 20)
                {
                    mode = TabMode.button;
                }
                else
                {
                    _index = 0;
                    throw new TooManyTabsException();
                }
                Initialize();
                _index = 0;
            }
            else if (_index == index) { return; }

            if (_index != index) { _index = index; }
        }

        public void Initialize()
        {
            this.tabButtons = new List<UIelement>();
            switch (this.mode)
            {
                default:
                case TabMode.single:
                    //Nothing to draw
                    break;

                case TabMode.tab:
                    for (int i = 0; i < _tabCount; i++)
                    {
                        SelectTab tab = new SelectTab(i, this);
                        this.tabButtons.Add(tab);
                        menu.pages[0].subObjects.Add(tab.rect);
                    }

                    break;

                case TabMode.button:
                    for (int i = 0; i < _tabCount; i++)
                    {
                        SelectButton btn = new SelectButton(i, this);
                        this.tabButtons.Add(btn);
                        menu.pages[0].subObjects.Add(btn.rect);
                    }
                    break;
            }
        }

        protected internal override void Deactivate()
        {
            base.Deactivate();
            foreach (UIelement element in this.tabButtons)
            { element.Deactivate(); }
        }

        protected internal override void Reactivate()
        {
            base.Reactivate();
            foreach (UIelement element in this.tabButtons)
            { element.Reactivate(); }
        }

        protected internal override void Unload()
        {
            base.Unload();
            switch (this.mode)
            {
                default:
                case TabMode.single:
                    //Nothing drawn
                    return;

                case TabMode.tab:
                    foreach (SelectTab tab in this.tabButtons)
                    {
                        menu.pages[0].subObjects.Remove(tab.rect);
                        menu.pages[0].subObjects.Remove(tab.rectH);
                        menu.pages[0].subObjects.Remove(tab.label);
                        tab.rect.RemoveSprites();
                        tab.rectH.RemoveSprites();
                        tab.label.RemoveSprites();
                    }
                    break;

                case TabMode.button:
                    foreach (SelectButton btn in this.tabButtons)
                    {
                        menu.pages[0].subObjects.Remove(btn.rect);
                        menu.pages[0].subObjects.Remove(btn.rectH);
                        menu.pages[0].subObjects.Remove(btn.label);
                        btn.rect.RemoveSprites();
                        btn.rectH.RemoveSprites();
                        btn.label.RemoveSprites();
                    }
                    break;
            }
            foreach (UIelement element in tabButtons)
            { element.Unload(); }
        }

        internal class SelectTab : UIelement
        {
            internal SelectTab(int index, ConfigTabController ctrler) : base(Vector2.zero, Vector2.zero)
            {
                this.index = index;
                this.ctrl = ctrler;

                this.bumpBehav = new BumpBehaviour(this);

                float height = Mathf.Min(120f, 600f / ctrler._tabCount);
                //ctrler._tabCount
                this._pos = ctrl.pos + new Vector2(0f, height * (-index - 1) + 603f); //ctrl.pos + new Vector2(17f, height * (ctrler._tabCount - index - 1) + 3f);
                //Debug.Log(string.Concat("Idx: ", index, " y: ", this._pos.y - ctrl.pos.y));
                this._size = new Vector2(30f, height - 6f);
                this.rect = new DyeableRect(myContainer, this.pos, this.size, true) { hiddenSide = DyeableRect.HiddenSide.Right };
                this.rectH = new DyeableRect(myContainer, this.pos, this.size, false) { hiddenSide = DyeableRect.HiddenSide.Right };
                this.label = OpLabel.CreateFLabel(this.name);
                this.label.alignment = FLabelAlignment.Left;
                this.myContainer.AddChild(this.label);
            }

            internal bool mouseTop, click;
            internal float darken;
            public OpTab representingTab => ConfigContainer.activeInterface.Tabs[index];
            public string name => representingTab.name;
            public readonly ConfigTabController ctrl;
            public bool active => ctrl.index == index;

            /// <summary>
            /// Index this Object is representing
            /// </summary>
            public int index;

            public readonly BumpBehaviour bumpBehav;

            public Color colorButton => representingTab.colorButton;
            public Color colorCanvas => representingTab.colorCanvas;
            public bool greyedOut => this.ctrl.greyedOut;

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

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.bumpBehav.greyedOut = this.greyedOut;
                this.bumpBehav.Update(timeStacker);
                this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

                if (!this.active && (greyedOut || !this.MouseOver)) { this.darken = Mathf.Max(0f, this.darken - 0.0333333351f); }
                else { this.darken = Mathf.Min(1f, this.darken + 0.1f); }

                this.label.label.rotation = -90f;
                this.label.text = string.IsNullOrEmpty(this.name) ? index.ToString() : this.name;

                if (greyedOut)
                {
                    Color cg = this.active ? this.colorButton : MenuColorEffect.Greyscale(this.colorButton);
                    cg = Color.Lerp(MenuColorEffect.MidToDark(cg), cg, this.darken);
                    this.rect.colorEdge = cg;
                    this.rect.colorFill = MenuColorEffect.MidToVeryDark(cg);
                    this.rectH.colorEdge = cg;
                    this.label.label.color = cg;
                    return;
                }

                Color color = this.bumpBehav.GetColor(this.colorButton);
                this.label.label.color = Color.Lerp(MenuColorEffect.MidToDark(color), color, Mathf.Lerp(this.darken, 1f, 0.6f));
                color = Color.Lerp(MenuColorEffect.MidToDark(color), color, this.darken);
                this.rect.colorEdge = color;
                this.rect.colorFill = MenuColorEffect.MidToVeryDark(color);

                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos = this.pos + new Vector2(-this.rect.addSize.x * 0.5f, 0f);
                this.label.pos.x = this.pos.x - addSize * 4f;

                this.rectH.colorEdge = color;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos = this.pos + new Vector2(-this.rectH.addSize.x * 0.5f, 0f);
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Update()
            {
                this.rect.Update(); this.rectH.Update();

                if (greyedOut || this.ctrl.hidden) { return; }

                if (MenuMouseMode)
                {
                    if (MouseOver)
                    {
                        if (!mouseTop)
                        {
                            mouseTop = true;
                            PlaySound(SoundID.MENU_Button_Select_Mouse);
                        }
                        if (string.IsNullOrEmpty(this.name))
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab No <TabIndex>").Replace("<TabIndex>", index.ToString())); }
                        else
                        { ModConfigMenu.instance.ShowDescription(InternalTranslator.Translate("Switch to Tab <TabName>").Replace("<TabName>", this.name)); }

                        if (Input.GetMouseButton(0)) { this.click = true; this.bumpBehav.held = true; }
                        else if (this.click)
                        {
                            this.click = false; this.bumpBehav.held = false;
                            ctrl.index = this.index;
                        }
                    }
                    else
                    {
                        mouseTop = false;
                        if (!Input.GetMouseButton(0)) { this.bumpBehav.held = false; this.click = false; }
                    }
                }
                else
                { // Controller
                }
            }
        }
    }
}