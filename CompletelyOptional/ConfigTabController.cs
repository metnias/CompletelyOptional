using Menu;
using OptionalUI;
using RWCustom;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special UI used internally in CM for switching tabs
    /// </summary>
    public class ConfigTabController : UIelement
    {
        public ConfigTabController(Vector2 pos, Vector2 size, MenuTab tab, ConfigMenu menu) : base(pos, size)
        {
            this.menuTab = tab;
            this.cfgMenu = menu;
            this.mode = TabMode.NULL;
            subElements = new List<UIelement>();
            this.greyedOut = false;

            OnChange();
        }

        public MenuTab menuTab;
        public ConfigMenu cfgMenu;

        public List<UIelement> subElements;

        public bool greyedOut;

        public int index
        {
            get { return ConfigMenu.selectedTabIndex; }
            set
            {
                if (_index == value)
                {
                    menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                    return;
                }
                menu.PlaySound(SoundID.MENU_MultipleChoice_Clicked);
                ConfigMenu.selectedTabIndex = value;
                ConfigMenu.ChangeSelectedTab();
                OnChange();
            }
        }

        public override void Reset()
        {
            base.Reset();
            _index = 0;
            OnChange();
            Update(0f);
        }

        private int _index;

        public static int GetTabCount() => ConfigMenu.currentInterface.Tabs.Length;

        private int _tabCount = -1;

        private enum TabMode
        {
            single, //==1
            tab, //>1, <=12
            button, //>12
            NULL
        }

        private TabMode mode;

        public override void Update(float dt)
        {
            base.Update(dt);
            foreach (UIelement element in this.subElements)
            {
                element.Update(dt);
                if (element is Selector selector && selector.active)
                {
                    ConfigMenu.instance.modCanvasBound.colorEdge = selector.color;
                    ConfigMenu.instance.modCanvasBound.colorFill = DyeableRect.MidToVeryDark(selector.color);
                }
            }
            if (MenuTab.logMode || mode == TabMode.NULL)
            {
                ConfigMenu.instance.modCanvasBound.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
                ConfigMenu.instance.modCanvasBound.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey);
            }
            else if (mode == TabMode.single)
            {
                ConfigMenu.instance.modCanvasBound.colorEdge = ConfigMenu.currentTab.color;
                ConfigMenu.instance.modCanvasBound.colorFill = DyeableRect.MidToVeryDark(ConfigMenu.currentTab.color);
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
            this.subElements = new List<UIelement>();
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
                        this.subElements.Add(tab);
                        menu.pages[0].subObjects.Add(tab.rect);
                    }

                    break;

                case TabMode.button:
                    for (int i = 0; i < _tabCount; i++)
                    {
                        SelectButton btn = new SelectButton(i, this);
                        this.subElements.Add(btn);
                        menu.pages[0].subObjects.Add(btn.rect);
                    }
                    break;
            }
        }

        public override void Hide()
        {
            base.Hide();
            foreach (UIelement element in this.subElements)
            { element.Hide(); }
        }

        public override void Show()
        {
            base.Show();
            foreach (UIelement element in this.subElements)
            { element.Show(); }
        }

        public override void Unload()
        {
            base.Unload();
            switch (this.mode)
            {
                default:
                case TabMode.single:
                    //Nothing drawn
                    return;

                case TabMode.tab:
                    foreach (SelectTab tab in this.subElements)
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
                    foreach (SelectButton btn in this.subElements)
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
            foreach (UIelement element in subElements)
            { element.Unload(); }
        }

        public class Selector : UIelement
        {
            public Selector(int index, ConfigTabController ctrler) : base(new Vector2(), new Vector2())
            {
                this.index = index;
                this.ctrl = ctrler;

                this.bumpBehav = new BumpBehaviour(this);
            }

            public readonly ConfigTabController ctrl;
            public bool active => ctrl.index == index;

            /// <summary>
            /// Index this Object is presenting
            /// </summary>
            public readonly int index;

            public readonly BumpBehaviour bumpBehav;

            public Color color => ConfigMenu.currentInterface.Tabs[this.index].color;
            public bool greyedOut => this.ctrl.greyedOut;

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);
                this.bumpBehav.greyedOut = this.greyedOut;
                this.bumpBehav.Update(dt);

                if (!this.active && (greyedOut || !this.MouseOver)) { this.darken = Mathf.Max(0f, this.darken - 0.0333333351f); }
                else { this.darken = Mathf.Min(1f, this.darken + 0.1f); }
            }

            internal bool mouseTop, click;
            internal float darken;
            public string name => ConfigMenu.currentInterface.Tabs[index].name;

            public override void Update(float dt)
            {
                foreach (MenuObject obj in this.subObjects)
                {
                    obj.Update();
                    if (!this.ctrl.hidden) { obj.GrafUpdate(dt); }
                }
                this.GrafUpdate(dt);

                if (greyedOut || this.ctrl.hidden) { return; }
                if (MouseOver)
                {
                    if (!mouseTop)
                    {
                        mouseTop = true;
                        ctrl.cfgMenu.PlaySound(SoundID.MENU_Button_Select_Mouse);
                    }
                    if (string.IsNullOrEmpty(this.name))
                    { ConfigMenu.description = InternalTranslator.Translate("Switch to Tab No <TabIndex>").Replace("<TabIndex>", index.ToString()); }
                    else
                    { ConfigMenu.description = InternalTranslator.Translate("Switch to Tab <TabName>").Replace("<TabName>", this.name); }

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
        }

        public class SelectTab : Selector
        {
            public SelectTab(int index, ConfigTabController ctrler) : base(index, ctrler)
            {
                float height = Mathf.Min(120f, 600f / ctrler._tabCount);
                //ctrler._tabCount
                this._pos = ctrl.pos + new Vector2(17f, height * (-index - 1) + 603f); //ctrl.pos + new Vector2(17f, height * (ctrler._tabCount - index - 1) + 3f);
                //Debug.Log(string.Concat("Idx: ", index, " y: ", this._pos.y - ctrl.pos.y));
                this._size = new Vector2(30f, height - 6f);
                this.rect = new DyeableRect(menu, owner, this.pos, this.size, true) { tab = true };
                this.rectH = new DyeableRect(menu, owner, this.pos, this.size, false) { tab = true };
                this.label = new MenuLabel(menu, owner, this.name, this.pos, this.size, false);

                this.subObjects.Add(this.rect);
                this.subObjects.Add(this.rectH);
                this.subObjects.Add(this.label);
            }

            /// <summary>
            /// Tab Label
            /// </summary>
            public MenuLabel label;

            /// <summary>
            /// Tab Boundary
            /// </summary>
            public DyeableRect rect;

            /// <summary>
            /// Tab Highlight
            /// </summary>
            public DyeableRect rectH;

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);

                this.label.label.rotation = -90f;
                this.label.text = string.IsNullOrEmpty(this.name) ? index.ToString() : this.name;

                if (greyedOut)
                {
                    Color cg = this.active ? this.color : DyeableRect.Grayscale(this.color);
                    cg = Color.Lerp(DyeableRect.MidToDark(cg), cg, this.darken);
                    this.rect.color = cg;
                    this.rect.colorF = DyeableRect.MidToVeryDark(cg);
                    this.rectH.color = cg;
                    this.label.label.color = cg;
                    return;
                }

                Color color = this.bumpBehav.GetColor(this.color);
                this.label.label.color = Color.Lerp(DyeableRect.MidToDark(color), color, Mathf.Lerp(this.darken, 1f, 0.6f));
                color = Color.Lerp(DyeableRect.MidToDark(color), color, this.darken);
                this.rect.color = color;
                this.rect.colorF = DyeableRect.MidToVeryDark(color);

                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos = this.pos + new Vector2(-this.rect.addSize.x * 0.5f, 0f);
                this.label.pos.x = this.pos.x - addSize * 4f;

                this.rectH.color = color;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos = this.pos + new Vector2(-this.rectH.addSize.x * 0.5f, 0f);
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Hide()
            {
                this.rect.Hide(); this.rectH.Hide(); this.label.label.isVisible = false;
            }

            public override void Show()
            {
                this.rect.Show(); this.rectH.Show(); this.label.label.isVisible = true;
            }

            public override void Unload()
            { base.Unload(); }
        }

        /// <summary>
        /// When the number of Tab is more than 12. Only first 2 character of <see cref="OpTab.name"/> will be displayed.
        /// </summary>
        public class SelectButton : Selector
        {
            public SelectButton(int index, ConfigTabController ctrler) : base(index, ctrler)
            {
                this._pos = ctrl.pos + new Vector2(18f, 30f * (19 - index) + 3f);
                this.size = new Vector2(30f, 24f);
                this.rect = new DyeableRect(menu, owner, this.pos, this.size, true) { tab = true };
                this.rectH = new DyeableRect(menu, owner, this.pos, this.size, false) { tab = true };
                this.label = new MenuLabel(menu, owner, this.name, this.pos, this.size, false);
                this.subObjects.Add(this.rect);
                this.subObjects.Add(this.rectH);
                this.subObjects.Add(this.label);
            }

            public MenuLabel label;
            public DyeableRect rect, rectH;

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);

                this.label.text = string.IsNullOrEmpty(this.name) ? index.ToString() : (this.name.Length > 2 ? this.name.Substring(0, 2) : this.name);

                if (greyedOut)
                {
                    Color cg = this.active ? this.color : DyeableRect.Grayscale(this.color);
                    cg = Color.Lerp(DyeableRect.MidToDark(cg), cg, this.darken);
                    this.rect.color = cg;
                    this.rect.colorF = DyeableRect.MidToVeryDark(cg);
                    this.rectH.color = cg;
                    this.label.label.color = cg;
                    return;
                }

                Color color = this.bumpBehav.GetColor(this.color);
                this.label.label.color = Color.Lerp(DyeableRect.MidToDark(color), color, Mathf.Lerp(this.darken, 1f, 0.6f));
                color = Color.Lerp(DyeableRect.MidToDark(color), color, this.darken);
                this.rect.color = color;
                this.rect.colorF = DyeableRect.MidToVeryDark(color);

                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                float addSize = this.active ? 1f : this.bumpBehav.AddSize;
                this.rect.addSize = new Vector2(8f, 4f) * addSize;
                this.rect.pos = this.pos + new Vector2(-this.rect.addSize.x * 0.5f, 0f);
                this.label.pos.x = this.pos.x - addSize * 4f;

                this.rectH.color = color;
                this.rectH.addSize = new Vector2(4f, -4f) * addSize;
                this.rectH.pos = this.pos + new Vector2(-this.rectH.addSize.x * 0.5f, 0f);
                float highlight = this.MouseOver ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * addSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = active ? 1f : highlight; }
            }

            public override void Hide()
            {
                this.rect.Hide(); this.rectH.Hide(); this.label.label.isVisible = false;
            }

            public override void Show()
            {
                this.rect.Show(); this.rectH.Show(); this.label.label.isVisible = true;
            }

            public override void Unload()
            {
                base.Unload();
            }
        }
    }
}
