using BepInEx.Configuration;
using CompletelyOptional;
using OptionalUI.ValueTypes;
using RWCustom;
using System;
using UnityEngine;

//RangeSlider, SubtleSlider
namespace OptionalUI
{
    /// <summary>
    /// Slider that let you input integer more visually.
    /// </summary>
    public class OpSlider : UIconfig, IValueInt
    {
        /// <summary>
        /// Slider that let you input integer more visually.
        /// </summary>
        /// <param name="pos">LeftBottom position. (excluding extra length in the end of slider line)</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="multi">Length of this slider will be this value * range. minimum 1f. (The width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="cosmeticValue">default integer value</param>
        public OpSlider(ConfigEntry<int> config, Vector2 pos, IntVector2 range, float multi = 1.0f, bool vertical = false, int cosmeticValue = 0) : base(config, pos, new Vector2(), cosmeticValue)
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse button and Drag up/down to adjust value" : "Hold your mouse button and Drag left/right to adjust value");
            this.min = range.x; this.max = range.y;
            int span = this.span;
            this.mousewheelTick = span > 5 ? Math.Max(Mathf.CeilToInt(span / 12f), 4) : 1;
            this.mul = Mathf.Max(multi, 1.0f);
            this._size = this.vertical ? new Vector2(30f, (span - 1) * this.mul) : new Vector2((span - 1) * this.mul, 30f);
            this.fixedSize = this._size;
            this._value = Custom.IntClamp(!cosmetic ? (int)config.DefaultValue : cosmeticValue, min, max).ToString();
            this.defaultValue = this.value;

            Initialize();
        }

        /// <summary>
        /// Slider that let you input integer more visually.
        /// </summary>
        /// <param name="pos">left-bottom corner coordinate (excluding extra length in the end of slider line)</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="length">Length of this slider will be this (minimum is the range; and the width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="cosmeticValue">default integer value</param>
        public OpSlider(ConfigEntry<int> config, Vector2 pos, IntVector2 range, int length, bool vertical = false, int cosmeticValue = 0) : base(config, pos, new Vector2(), cosmeticValue)
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse button and Drag up/down to adjust value" : "Hold your mouse button and Drag left/right to adjust value");
            this.min = range.x; this.max = range.y;
            int span = this.span;
            this.mousewheelTick = span > 5 ? Math.Max(Mathf.CeilToInt(span / 12f), 4) : 1;
            float l = Mathf.Max((float)(span - 1), (float)length);
            this.mul = l / (float)(span - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this._value = Custom.IntClamp(!cosmetic ? (int)config.DefaultValue : cosmeticValue, min, max).ToString();
            this.defaultValue = this.value;

            Initialize();
        }

        /// <summary>
        /// For GeneratedOI, 0-100
        /// </summary>
        internal OpSlider(ConfigEntry<uint> config, Vector2 pos, int length, bool vertical = false) : base(config, pos, new Vector2())
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse button and Drag up/down to adjust value" : "Hold your mouse button and Drag left/right to adjust value");
            this.min = 0; this.max = 100;
            int span = this.span;
            this.mousewheelTick = span > 5 ? Math.Max(Mathf.CeilToInt(span / 12f), 4) : 1;
            float l = Mathf.Max((float)(span - 1), (float)length);
            this.mul = l / (float)(span - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this._value = Custom.IntClamp((int)config.DefaultValue, min, max).ToString();
            this.defaultValue = this.value;

            Initialize();
        }

        /// <summary>
        /// For GeneratedOI
        /// </summary>
        internal OpSlider(ConfigEntry<byte> config, Vector2 pos, IntVector2 range, int length, bool vertical = false) : base(config, pos, new Vector2())
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse button and Drag up/down to adjust value" : "Hold your mouse button and Drag left/right to adjust value");
            this.min = range.x; this.max = range.y;
            int span = this.span;
            this.mousewheelTick = span > 5 ? Math.Max(Mathf.CeilToInt(span / 12f), 4) : 1;
            float l = Mathf.Max((float)(span - 1), (float)length);
            this.mul = l / (float)(span - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this._value = Custom.IntClamp((int)config.DefaultValue, min, max).ToString();
            this.defaultValue = this.value;

            Initialize();
        }

        private bool tickSlider => this is OpSliderTick;

        /* private bool rangeSlider
        {
            get { return this is OpSliderRange; }
        }*/

        protected internal virtual void Initialize()
        {
            this.lineSprites = new FSprite[4];
            for (int i = 0; i < this.lineSprites.Length; i++)
            {
                this.lineSprites[i] = new FSprite("pixel", true);
                this.myContainer.AddChild(this.lineSprites[i]);
            }
            if (this.vertical)
            {
                this.lineSprites[0].scaleY = 2f;
                this.lineSprites[0].scaleX = 6f;
                this.lineSprites[1].scaleX = 2f;
                this.lineSprites[1].anchorY = 0f;
                this.lineSprites[2].scaleX = 2f;
                this.lineSprites[2].anchorY = 1f;
                this.lineSprites[3].scaleY = 2f;
                this.lineSprites[3].scaleX = 6f;
                if (!tickSlider) { this.rect = new DyeableRect(myContainer, new Vector2(4f, -8f), new Vector2(24f, 16f), true); }
                //if (rangeSlider) { this.lineSprites[4].scaleX = 2f; this.lineSprites[4].anchorY = 1f; }
            }
            else
            {
                this.lineSprites[0].scaleX = 2f;
                this.lineSprites[0].scaleY = 6f;
                this.lineSprites[1].scaleY = 2f;
                this.lineSprites[1].anchorX = 0f;
                this.lineSprites[2].scaleY = 2f;
                this.lineSprites[2].anchorX = 1f;
                this.lineSprites[3].scaleX = 2f;
                this.lineSprites[3].scaleY = 6f;
                if (!tickSlider) { this.rect = new DyeableRect(myContainer, new Vector2(-8f, 4f), new Vector2(16f, 24f), true); }
                //if (rangeSlider) { this.lineSprites[4].scaleY = 2f; this.lineSprites[4].anchorX = 1f; }
            }
            if (!tickSlider)
            {
                this.label = FLabelCreate(this.value);
                this.label.alpha = 0f;
                this.myContainer.AddChild(this.label);
                FLabelPlaceAtCenter(this.label, this.rect.pos, new Vector2(40f, 20f));
            }
        }

        /// <summary>
        /// The amount that changes when you scroll your mousewheel over <see cref="OpSlider"/>
        /// </summary>
        public int mousewheelTick;

        protected readonly int min, max;
        public int span => max - min + 1;
        public readonly bool vertical;
        protected float mul;

        protected FSprite[] lineSprites;
        public DyeableRect rect { get; protected set; }
        public FLabel label { get; protected set; }

        string IValueType.valueString { get => this.value; set => this.value = value; }

        /// <summary>
        /// Text and Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        /// <summary>
        /// Colour of Lines. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorLine = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            float add = this.greyedOut ? 0f : this.bumpBehav.AddSize;
            Vector2 cutPos, cutSize;
            int valueInt = this.GetValueInt();
            if (!vertical)
            {
                if (!tickSlider)
                {
                    float p = this.mul * (float)(valueInt - this.min);
                    if (MenuMouseMode && this.held) { p = Mathf.Clamp(MousePos.x, 0f, size.x); }
                    this.rect.pos.y = 4f;
                    this.rect.pos.x = p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    FLabelPlaceAtCenter(this.label, this.rect.pos.x - 14f, this.rect.pos.y + 25f, 40f, 20f);
                    cutPos = this.rect.pos - this.rect.addSize / 2f + Vector2.one;
                    cutSize = this.rect.size + this.rect.addSize - Vector2.one;
                }
                else
                {
                    float t = (valueInt - this.min) * this.mul;
                    float s = Mathf.Clamp(add, 0f, 1f);
                    s = 1.0f + s * 0.3f;
                    cutPos = new Vector2(t, 15f);
                    cutSize = new Vector2(17f * s, 17f * s);
                    cutPos -= cutSize / 2f;
                }

                this.lineSprites[1].x = 0f;
                this.lineSprites[1].y = 15f;
                this.lineSprites[1].scaleX = cutPos.x;
                this.lineSprites[2].x = this.size.x;
                this.lineSprites[2].y = 15f;
                this.lineSprites[2].scaleX = this.size.x - (cutPos.x + cutSize.x);
                this.lineSprites[0].x = 0f;
                this.lineSprites[0].y = 15f;
                this.lineSprites[0].scaleY = 10f + 6f * add;
                this.lineSprites[3].x = this.size.x; // + this.ExtraLengthAtEnd;
                this.lineSprites[3].y = 15f;
                this.lineSprites[3].scaleY = 10f + 6f * add;

                this.LineVisibility(cutPos, cutSize);
            }
            else
            {
                if (!tickSlider)
                {
                    float p = this.mul * (float)(valueInt - this.min);
                    if (MenuMouseMode && this.held) { p = Mathf.Clamp(MousePos.y, 0f, size.y); }
                    this.rect.pos.x = 4f;
                    this.rect.pos.y = p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    FLabelPlaceAtCenter(this.label, this.rect.pos.x - 10f, this.rect.pos.y + 18f, 40f, 20f);
                    cutPos = this.rect.pos - this.rect.addSize / 2f + Vector2.one;
                    cutSize = this.rect.size + this.rect.addSize - Vector2.one;
                }
                else
                {
                    float t = (valueInt - this.min) * this.mul;
                    float s = Mathf.Clamp(add, 0f, 1f);
                    s = 1.0f + s * 0.3f;
                    cutPos = new Vector2(15f, t);
                    cutSize = new Vector2(17f * s, 17f * s);
                    cutPos -= cutSize / 2f;
                }

                this.lineSprites[1].y = 0f;
                this.lineSprites[1].x = 15f;
                this.lineSprites[1].scaleY = cutPos.y;
                this.lineSprites[2].y = this.size.y;
                this.lineSprites[2].x = 15f;
                this.lineSprites[2].scaleY = this.size.y - (cutPos.y + cutSize.y);
                this.lineSprites[0].y = 0f;
                this.lineSprites[0].x = 15f;
                this.lineSprites[0].scaleX = 10f + 6f * add;
                this.lineSprites[3].y = this.size.y;
                this.lineSprites[3].x = 15f;
                this.lineSprites[3].scaleX = 10f + 6f * add;

                this.LineVisibility(cutPos, cutSize);
            }

            if (this.greyedOut)
            {
                foreach (FSprite s in this.lineSprites) { s.color = this.bumpBehav.GetColor(this.colorLine); }
                if (tickSlider) return;
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.label.color = this.bumpBehav.GetColor(this.colorEdge);
                if (this.Focused() || this.MouseOver) { this.label.alpha = Mathf.Min(0.5f, this.label.alpha + 0.05f); }
                else { this.label.alpha = Mathf.Max(0f, this.label.alpha - 0.1f); }
                return;
            }

            if (!tickSlider)
            {
                if (this.held || this.Focused() || this.MouseOver)
                { this.label.alpha = Mathf.Min(this.label.alpha + 0.1f, 1f); }
                else { this.label.alpha = Mathf.Max(this.label.alpha - 0.15f, 0f); }

                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                if (this.held) { this.rect.colorFill = this.bumpBehav.GetColor(this.colorEdge); }
                else { this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill); }

                this.rect.fillAlpha = this.held ? 1f : this.bumpBehav.FillAlpha;
                this.rect.GrafUpdate(timeStacker);
            }

            Color color = this.bumpBehav.GetColor(this.colorLine);
            foreach (FSprite l in this.lineSprites) { l.color = color; }
            if (!tickSlider) { this.label.color = Color.Lerp(this.rect.colorEdge, Color.white, 0.5f); }
        }

        protected virtual void LineVisibility(Vector2 cutPos, Vector2 cutSize)
        {
            if (!vertical)
            {
                this.lineSprites[1].isVisible = cutPos.x > 0f;
                this.lineSprites[2].isVisible = this.size.x > cutPos.x + cutSize.x;
                this.lineSprites[0].isVisible = cutPos.x > 0f;
                this.lineSprites[3].isVisible = this.size.x > cutPos.x + cutSize.x;
            }
            else
            {
                this.lineSprites[1].isVisible = cutPos.y > 0f;
                this.lineSprites[2].isVisible = this.size.y > cutPos.y + cutSize.y;
                this.lineSprites[0].isVisible = cutPos.y > 0f;
                this.lineSprites[3].isVisible = this.size.y > cutPos.y + cutSize.y;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!tickSlider) { this.rect.Update(); }
            if (greyedOut) { return; }

            if (MenuMouseMode)
            {
                if (this.held)
                {
                    if (Input.GetMouseButton(0))
                    {
                        float t = this.vertical ? this.MousePos.y : this.MousePos.x;
                        this.SetValueInt(Custom.IntClamp(Mathf.RoundToInt(t / this.mul) + this.min, this.min, this.max));
                    }
                    else
                    {
                        this.held = false;
                    }
                }
                else if (!this.held && this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    {
                        this.held = true;
                        PlaySound(SoundID.MENU_First_Scroll_Tick);
                    }
                    else if (this.menu.mouseScrollWheelMovement != 0)
                    {
                        int num = this.GetValueInt() - (int)Mathf.Sign(this.menu.mouseScrollWheelMovement) * this.mousewheelTick;
                        num = Custom.IntClamp(num, this.min, this.max);
                        if (num != this.GetValueInt()) { this.SetValueInt(num); }
                    }
                }
            }
            else
            {
                if (this.Focused())
                {
                    if (CtlrInput.thrw && !LastCtlrInput.thrw)
                    {
                        this.held = false;
                        PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                        return;
                    }
                    int newVal = this.GetValueInt();
                    bool tick = false;
                    if (vertical)
                    {
                        if (CtlrInput.y != 0 && LastCtlrInput.y != CtlrInput.y)
                        {
                            newVal = Custom.IntClamp(newVal + CtlrInput.y, min, max); tick = true;
                        }
                        if (CtlrInput.y != 0 && LastCtlrInput.y == CtlrInput.y && CtlrInput.x == 0)
                        { this.scrollInitDelay++; }
                        else
                        { this.scrollInitDelay = 0; }
                        if (this.scrollInitDelay > ModConfigMenu.DASinit)
                        {
                            this.scrollDelay++;
                            if (this.scrollDelay > ModConfigMenu.DASdelay)
                            {
                                this.scrollDelay = 0;
                                if (CtlrInput.y != 0 && LastCtlrInput.y == CtlrInput.y)
                                { newVal = Custom.IntClamp(newVal + CtlrInput.y, min, max); tick = true; }
                            }
                        }
                        else { this.scrollDelay = 0; }
                    }
                    else
                    {
                        if (CtlrInput.x != 0 && LastCtlrInput.x != CtlrInput.x)
                        { newVal = Custom.IntClamp(newVal + CtlrInput.x, min, max); tick = true; }
                        if (CtlrInput.x != 0 && LastCtlrInput.x == CtlrInput.y && CtlrInput.y == 0)
                        { this.scrollInitDelay++; }
                        else
                        { this.scrollInitDelay = 0; }
                        if (this.scrollInitDelay > ModConfigMenu.DASinit)
                        {
                            this.scrollDelay++;
                            if (this.scrollDelay > ModConfigMenu.DASdelay)
                            {
                                this.scrollDelay = 0;
                                if (CtlrInput.x != 0 && LastCtlrInput.x == CtlrInput.x)
                                { newVal = Custom.IntClamp(newVal + CtlrInput.x, min, max); tick = true; }
                            }
                        }
                        else { this.scrollDelay = 0; }
                    }
                    if (tick)
                    {
                        if (newVal != this.GetValueInt())
                        { PlaySound(scrollInitDelay > 1 ? SoundID.MENU_Scroll_Tick : SoundID.MENU_First_Scroll_Tick); this.SetValueInt(newVal); }
                        else { PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard); }
                    }
                }
            }
        }

        private float scrollInitDelay, scrollDelay;

        public override void OnChange()
        {
            base.OnChange();

            if (MouseOver || held)
            {
                PlaySound(SoundID.MENU_Scroll_Tick);
                this.bumpBehav.flash = Mathf.Min(1f, this.bumpBehav.flash + 0.5f);
                this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
            }
            if (!tickSlider) this.label.text = this.value;

            /*
            int r = this.span;
            float l = Mathf.Max((float)(r - 1), (float)(!vertical ? this._size.x : this._size.y));
            this.mul = l / (r - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            */
        }

        protected internal override void Reactivate()
        {
            base.Reactivate();

            #region visibility

            float add = this.greyedOut ? 0f : this.bumpBehav.AddSize;
            Vector2 cutPos, cutSize;
            int valueInt = this.GetValueInt();
            if (!vertical)
            {
                if (!tickSlider)
                {
                    float p = this.mul * (float)(valueInt - this.min);
                    this.rect.pos.y = 4f;
                    this.rect.pos.x = p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.x = this.rect.pos.x - 14f;
                    this.label.y = this.rect.pos.y + 25f;
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (valueInt - this.min) * this.mul;
                    float s = Mathf.Clamp(add, 0f, 1f);
                    s = 1.0f + s * 0.3f;
                    cutPos = new Vector2(t, 15f);
                    cutSize = new Vector2(17f * s, 17f * s);
                    cutPos -= cutSize / 2f;
                }
            }
            else
            {
                if (!tickSlider)
                {
                    float p = this.mul * (float)(valueInt - this.min);
                    this.rect.pos.x = 4f;
                    this.rect.pos.y = p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.x = this.rect.pos.x - 10f;
                    this.label.y = this.rect.pos.y + 18f;
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (valueInt - this.min) * this.mul;
                    float s = Mathf.Clamp(add, 0f, 1f);
                    s = 1.0f + s * 0.3f;
                    cutPos = new Vector2(15f, t);
                    cutSize = new Vector2(17f * s, 17f * s);
                    cutPos -= cutSize / 2f;
                }
            }

            this.LineVisibility(cutPos, cutSize);

            #endregion visibility
        }

        protected internal override bool CopyFromClipboard(string value)
        {
            this.held = false;
            return base.CopyFromClipboard(value);
        }
    }
}