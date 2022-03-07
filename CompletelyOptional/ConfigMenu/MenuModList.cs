using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 40f) - UIelement._offset, new Vector2(250f, 684f))
        {
            tab.AddItems(this);

            // abc buttons
            abcButtons = new AlphabetButton[26];
            for (uint u = 0; u < abcButtons.Length; u++)
            { abcButtons[u] = new AlphabetButton(this, u); }

            // backpanel & stat button
            backSide = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 250f, color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black) };
            rightSides = new FSprite[2];
            rightSides[0] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f };
            rightSides[1] = new FSprite("pixel") { anchorX = 0f, anchorY = 0f, scaleX = 2f };
            myContainer.AddChild(backSide); myContainer.AddChild(rightSides[0]); myContainer.AddChild(rightSides[1]);

            statButton = new StatButton(this);

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
        private readonly FSprite[] rightSides;
        private readonly StatButton statButton;

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
            else if (element is StatButton)
            { // Switch Mod to InternalOI_Stats
            }
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton
        {
            public ModButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(250f, 24f), "Mod List")
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

        internal class StatButton : OpSimpleImageButton
        {
            public StatButton(MenuModList list) : base(Vector2.zero, Vector2.one, "", "Menu_Symbol_Show_List")
            {
                this.list = list;
                this.list.menuTab.AddItems(this);
                this._size = new Vector2(20f, 20f);
                this.rect.size = this.size; this.rectH.size = this.size;
            }

            public readonly MenuModList list;

            public override void OnChange()
            {
                base.OnChange();
                this._size = new Vector2(20f, 20f);
                this.rect.size = this.size; this.rectH.size = this.size;
            }

            public override void Signal()
            {
                list.Signal(this);
            }
        }

        internal class AlphabetButton : OpSimpleButton
        {
            public AlphabetButton(MenuModList list, uint index) : base(Vector2.zero, Vector2.one, "", "")
            {
                this.list = list;
                this.index = index;
                represent = (char)(index + 65); // upper A: 65
                this.text = represent.ToString();
                this.list.menuTab.AddItems(this);

                this._size = new Vector2(20f, 20f);
                this.greyedOut = ConfigContainer.OptItfABC[index] < 0;
                OnChange();
            }

            public readonly MenuModList list;

            public readonly uint index;
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