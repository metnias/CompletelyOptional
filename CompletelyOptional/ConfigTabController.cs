using Menu;
using OptionalUI;
using RWCustom;
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
            tabButtons = new TabSelectButton[10];
            omit = omit < 0f ? LabelTest.GetWidth("...", false) : omit;

            rectCanvas = new DyeableRect(this.myContainer, new Vector2(543f, 105f) - UIelement._offset - this.pos, new Vector2(630f, 630f), true) { fillAlpha = 0.5f };
            // author version license: 908 30, 245 86
            rectPpty = new DyeableRect(this.myContainer, new Vector2(908f, 30f) - UIelement._offset - this.pos, new Vector2(245f, 86f), true) { hiddenSide = DyeableRect.HiddenSide.Top };

            OnChange();
        }

        private readonly DyeableRect rectCanvas, rectPpty;

        // Add Up/Down arrow

        internal MenuTab menuTab;

        internal TabSelectButton[] tabButtons;

        private static float omit = -1f;

        private bool _focused;

        bool ICanBeFocused.Focused { get => _focused; set => _focused = value; }
        bool ICanBeFocused.GreyedOut => false;

        bool ICanBeFocused.CurrentlyFocusableMouse => ConfigContainer.activeInterface.Tabs.Length > 1;

        bool ICanBeFocused.CurrentlyFocusableNonMouse => ConfigContainer.activeInterface.Tabs.Length > 1;

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

        public static int TabCount => ConfigContainer.activeInterface.Tabs.Length;

        private static int tabButtonCount => Custom.IntClamp(TabCount, 1, 10);

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

            // If this gets focus
            // becomes hold automatically
        }

        public override void OnChange()
        { //Selected Tab/Tab number has changed
            base.OnChange();
            if (_tabCount != TabCount)
            {
                _tabCount = TabCount;
                Initialize();
            }
        }

        public void Initialize()
        {
            for (int i = 0; i < 10; i++)
            {
                this.tabButtons[i] = new TabSelectButton(i, this);
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
            foreach (TabSelectButton btn in tabButtons) { btn.Unload(); }
        }

        internal class TabSelectButton : UIelement
        {
            internal TabSelectButton(int index, ConfigTabController ctrler) : base(Vector2.zero, Vector2.zero)
            {
                this.buttonIndex = index;
                this.tabIndex = index;
                this.ctrl = ctrler;
                this.ctrl.menuTab.AddItems(this);

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
            internal int tabIndex;

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

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                this.bumpBehav.Update(timeStacker);
                this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

                if (!this.active && !this.MouseOver) { this.darken = Mathf.Max(0f, this.darken - 0.0333333351f); }
                else { this.darken = Mathf.Min(1f, this.darken + 0.1f); }

                this.label.rotation = -90f;
                this.label.text = string.IsNullOrEmpty(this.name) ? tabIndex.ToString() : this.name;

                Color color = this.bumpBehav.GetColor(this.colorButton);
                this.label.color = Color.Lerp(MenuColorEffect.MidToDark(color), color, Mathf.Lerp(this.darken, 1f, 0.6f));
                color = Color.Lerp(MenuColorEffect.MidToDark(color), color, this.darken);
                this.rect.colorEdge = color;
                this.rect.colorFill = MenuColorEffect.MidToVeryDark(color);

                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos = this.pos + new Vector2(-this.rect.addSize.x * 0.5f, 0f);
                this.label.x = -addSize * 4f;

                this.rectH.colorEdge = color;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos = this.pos + new Vector2(-this.rectH.addSize.x * 0.5f, 0f);
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Update()
            {
                this.rect.Update(); this.rectH.Update();

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
                { // Controller
                }
            }
        }
    }
}