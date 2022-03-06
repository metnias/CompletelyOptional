using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement
    {
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 40f) - UIelement._offset, new Vector2(250f, 684f))
        {
            tab.AddItems(this);
            rect = new DyeableRect(this.myContainer, new Vector2(-15f, -10f), new Vector2(280f, 705f));
        }

        // Change of plan: use Arena's LevelSelector way of doing instead

        internal MenuTab menuTab => this.tab as MenuTab;
        internal ConfigContainer cfgContainer => menu.cfgContainer;

        private readonly DyeableRect rect;

        internal float abcSlide = 40f, lastAbcSlide = 40f; // -60f (hidden) ~ 40f (slided out)

        internal float GetMyAbcSlide(int index, float timeStacker) => Mathf.Clamp(Mathf.Lerp(lastAbcSlide, abcSlide, timeStacker) + index * 2.0f, 0f, 40f);

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
            rect.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            rect.Update();
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton
        {
            public ModButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(250f, 24f), "Mod List")
            {
                this.list = list;
                this.index = index; // starts from 1; index 0 is for StatOI
                //this._pos = list.camPos;
                this.labelVer = OpLabel.CreateFLabel("v1.0");
                this.myContainer.AddChild(this.labelVer);

                this.list.menuTab.AddItems(this);
                OnChange();
            }

            private readonly MenuModList list;

            public readonly int index;
            private readonly FLabel labelVer;
            private float fade, lastFade;

            public override void OnChange()
            {
                base.OnChange();
                this.label.alignment = FLabelAlignment.Left;
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }

            public override void Update()
            {
                this.lastFade = this.fade;
                base.Update();
            }

            public override void Signal()
            {
            }

            public override bool CurrentlyFocusableMouse => false;
            public override bool CurrentlyFocusableNonMouse => true;

            protected internal override bool MouseOver => base.MouseOver;
        }

        internal class StatButton : OpSimpleImageButton
        {
            public StatButton(MenuModList list) : base(Vector2.zero, Vector2.one, "", "Menu_Symbol_Show_List")
            {
                this.list = list;
                this._size = new Vector2(20f, 20f);
                this.rect.size = this.size; this.rectH.size = this.size;

                this.list.menuTab.AddItems(this);
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

                this._size = new Vector2(20f, 20f);
                this.greyedOut = ConfigContainer.OptItfABC[index] < 0;

                this.list.menuTab.AddItems(this);
            }

            public readonly MenuModList list;

            public readonly uint index;
            public readonly char represent;

            public override bool CurrentlyFocusableMouse => !greyedOut;

            public override bool CurrentlyFocusableNonMouse => !greyedOut;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                rect.Hide(); rectH.Hide();
            }

            public override void Signal()
            {
            }
        }
    }
}