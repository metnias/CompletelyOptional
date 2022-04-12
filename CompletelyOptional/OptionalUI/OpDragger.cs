using BepInEx.Configuration;
using CompletelyOptional;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Dragger to adjust int value easily.
    /// </summary>
    public class OpDragger : UIconfig, IValueInt
    {
        /// <summary>
        /// Dragger to adjust int value in a cramped space. The fixedSize is 24x24.
        /// </summary>
        /// <param name="pos">BottomLeft</param>
        /// <param name="cosmeticInt">default value for <see cref="UIconfig.cosmetic"/></param>
        public OpDragger(ConfigEntry<int> config, Vector2 pos, int cosmeticInt = 0) : base(config, pos, new Vector2(24f, 24f), cosmeticInt.ToString())
        {
            this.fixedSize = new Vector2(24f, 24f);

            this.rect = new DyeableRect(myContainer, Vector2.zero, this.size, true);
            this.label = FLabelCreate(!cosmetic ? config.DefaultValue.ToString() : cosmeticInt.ToString());
            FLabelPlaceAtCenter(this.label, 0f, 2f, 24f, 20f);
            this.myContainer.AddChild(this.label);

            this.min = 0; this._min = 0; this.max = 99; this._max = 99;

            this.useCT = false;
        }

        /// <summary>
        /// Dragger to adjust int value in a cramped space. The fixedSize is 24x24.
        /// </summary>
        /// <param name="posX">Left position.</param>
        /// <param name="posY">Bottom position.</param>
        /// <param name="cosmeticInt">default value</param>
        public OpDragger(ConfigEntry<int> config, float posX, float posY, int cosmeticInt = 0) : this(config, new Vector2(posX, posY), cosmeticInt)
        { }

        protected internal override string DisplayDescription()
        {
            if (!string.IsNullOrEmpty(description)) { return description; }
            if (MenuMouseMode) { return OptionalText.GetText(OptionalText.ID.OpDragger_MouseTuto); }
            return OptionalText.GetText(!held ? OptionalText.ID.OpDragger_NonMouseGrabTuto : OptionalText.ID.OpDragger_NonMouseUseTuto);
        }

        /// <summary>
        /// Minimum value. default = 0
        /// </summary>
        public int min;

        /// <summary>
        /// Maximum value. default = 99
        /// </summary>
        public int max;

        private int _min, _max;

        private string[] customText; private bool useCT;

        /// <summary>
        /// Set custom text to display for each index.
        /// </summary>
        /// <param name="newTexts">strings to display from min to max</param>
        public void SetCustomText(params string[] newTexts)
        {
            useCT = true;
            customText = new string[max - min + 1];
            for (int i = 0; i < customText.Length; i++) { customText[i] = (min + i).ToString(); }
            for (int j = 0; j < newTexts.Length; j++)
            {
                if (j >= customText.Length) { break; }
                if (string.IsNullOrEmpty(newTexts[j])) { continue; }
                customText[j] = newTexts[j];
            }
        }

        /// <summary>
        /// Boundary DyeableRect
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// FLabel
        /// </summary>
        public FLabel label;

        /// <summary>
        /// Text Colour. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorText = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (greyedOut)
            {
                this.label.color = this.bumpBehav.GetColor(this.colorText);
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rect.GrafUpdate(timeStacker);
                return;
            }

            this.rect.colorFill = this.colorFill;
            this.greyFade = Custom.LerpAndTick(this.greyFade, this.held ? 0f : 1f, 0.05f, 0.025f / frameMulti);

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;

            this.label.color = this.bumpBehav.GetColor(this.colorText);
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.GrafUpdate(timeStacker);
        }

        private float greyFade;

        private float savMouse; private int savValue;

        string IValueType.valueString { get => this.value; set => this.value = value; }

        public override void Update()
        {
            base.Update();
            if (greyedOut) { return; }
            this.rect.Update();
            if (_min != min || _max != max) { Change(); }

            if (MenuMouseMode)
            {
                if (this.held)
                {
                    if (Input.GetMouseButton(0))
                    {
                        int newVal = Custom.IntClamp(this.savValue + Mathf.FloorToInt((Input.mousePosition.y - this.savMouse) / 10f), this.min, this.max);
                        if (this.GetValueInt() != newVal)
                        { this.SetValueInt(newVal); PlaySound(SoundID.MENU_Scroll_Tick); }
                    }
                    else
                    { this.held = false; }
                }
                else if (!this.held && this.menu.manager.menuesMouseMode && this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    {
                        this.held = true;
                        this.savMouse = Input.mousePosition.y;
                        this.savValue = this.GetValueInt();
                        PlaySound(SoundID.MENU_First_Scroll_Tick);
                    }
                    else if (this.menu.mouseScrollWheelMovement != 0)
                    {
                        int num = this.GetValueInt() - (int)Mathf.Sign(this.menu.mouseScrollWheelMovement);
                        num = Custom.IntClamp(num, this.min, this.max);
                        if (num != this.GetValueInt())
                        {
                            this.bumpBehav.flash = 1f;
                            PlaySound(SoundID.MENU_Scroll_Tick);
                            this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
                            this.SetValueInt(num);
                        }
                    }
                }
            }
            else
            {
                if (this.held)
                {
                    if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
                    {
                        this.held = false;
                        lastVal = this.GetValueInt();
                        PlaySound(SoundID.MENU_Checkbox_Check);
                        return;
                    }
                    if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
                    {
                        this.held = false;
                        this.SetValueInt(lastVal);
                        PlaySound(SoundID.MENU_Checkbox_Uncheck);
                        return;
                    }

                    if (CtlrInput.y != 0)
                    {
                        int tick = bumpBehav.JoystickPressAxis(true);
                        if (tick != 0) { TryTick(true); }
                        else
                        {
                            tick = bumpBehav.JoystickHeldAxis(true, 3f);
                            if (tick != 0) { TryTick(false); }
                        }

                        void TryTick(bool first)
                        {
                            int newVal = Custom.IntClamp(this.GetValueInt() + tick, min, max);
                            if (newVal != this.GetValueInt())
                            { PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick); this.SetValueInt(newVal); }
                            else { PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None); }
                        }
                    }
                }
            }
        }

        private int lastVal;

        protected internal override void Change()
        {
            base.Change();
            if (MouseOver || held)
            {
                this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
                this.bumpBehav.flash = Mathf.Min(1f, this.bumpBehav.flash + 0.5f);
            }
            if (_min != min || _max != max)
            {
                if (min > _max)
                {
                    min = _min;
                    ComOptPlugin.LogError(string.Concat("You set new minimum of OpDragger(key: ", this.key, ") larger than its maximum! (", min.ToString(), ">", _max.ToString(), ") Disregarding new change."));
                }
                else if (max < _min)
                {
                    max = _max;
                    ComOptPlugin.LogError(string.Concat("You set new maximum of OpDragger(key: ", this.key, ") larger than its minimum! (", min.ToString(), ">", _max.ToString(), ") Disregarding new change."));
                }
                if (useCT)
                {
                    string[] temp = new string[max - min + 1];
                    for (int i = 0; i < temp.Length; i++) { temp[i] = (min + i).ToString(); }
                    for (int j = 0; j < customText.Length; j++)
                    {
                        if (j >= temp.Length) { break; }
                        if (int.TryParse(customText[j], out int k)) { if (k == _min + j) { continue; } }
                        temp[j] = customText[j];
                    }
                    customText = temp;
                }
                _min = min;
                _max = max;
            }

            if (useCT) { this.label.text = customText[this.GetValueInt() - this.min]; }
            else { this.label.text = value; }

            FLabelPlaceAtCenter(this.label, 0f, 2f, 24f, 20f);
        }
    }
}