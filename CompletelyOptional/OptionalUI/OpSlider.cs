using CompletelyOptional;
using Menu;
using RWCustom;
using System;
using UnityEngine;

//RangeSlider, SubtleSlider
namespace OptionalUI
{
    /// <summary>
    /// Slider that let you input integer more visually.
    /// </summary>
    public class OpSlider : UIconfig, SelectableUIelement
    {
        /// <summary>
        /// Slider that let you input integer more visually.
        /// </summary>
        /// <param name="pos">LeftBottom position. (excluding extra length in the end of slider line)</param>
        /// <param name="key">unique keyword for this UIconfig</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="multi">Length of this slider will be this value * range. minimum 1f. (The width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="defaultValue">default integer value</param>
        public OpSlider(Vector2 pos, string key, IntVector2 range, float multi = 1.0f, bool vertical = false, int defaultValue = 0) : base(pos, new Vector2(), key, defaultValue.ToString())
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse button and Drag up/down to adjust value" : "Hold your mouse button and Drag left/right to adjust value");
            this.min = range.x; this.max = range.y;
            int r = this.max - this.min + 1;
            this.wheelTick = r > 5 ? Math.Max(Mathf.CeilToInt(r / 12f), 4) : 1;
            this.mul = Mathf.Max(multi, 1.0f);
            this._size = this.vertical ? new Vector2(30f, (r - 1) * this.mul) : new Vector2((r - 1) * this.mul, 30f);
            this.fixedSize = this._size;
            this.ForceValue(Custom.IntClamp(defaultValue, min, max).ToString());
            if (!_init) { return; }
            Initialize();
        }

        /// <summary>
        /// Slider that let you input integer more visually.
        /// </summary>
        /// <param name="pos">left-bottom corner coordinate (excluding extra length in the end of slider line)</param>
        /// <param name="key">unique keyword for this UIconfig</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="length">Length of this slider will be this (minimum is the range; and the width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="defaultValue">default integer value</param>
        public OpSlider(Vector2 pos, string key, IntVector2 range, int length, bool vertical = false, int defaultValue = 0) : base(pos, new Vector2(), key, defaultValue.ToString())
        {
            this.vertical = vertical;
            this.description = InternalTranslator.Translate(this.vertical ?
                "Hold your mouse and Drag up/down to adjust value" : "Hold your mouse and Drag left/right to adjust value");
            this.min = range.x; this.max = range.y;
            int r = this.max - this.min + 1;
            this.wheelTick = r > 5 ? Math.Max(Mathf.CeilToInt(r / 12f), 4) : 1;
            float l = Mathf.Max((float)(r - 1), (float)length);
            this.mul = l / (float)(r - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this.ForceValue(Custom.IntClamp(defaultValue, min, max).ToString());
            this.defaultValue = this.value;
            if (!_init) { return; }
            Initialize();
        }

        private bool subtleSlider => this is OpSliderSubtle;

        /* private bool rangeSlider
        {
            get { return this is OpSliderRange; }
        }*/

        internal virtual void Initialize()
        {
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.colorLine = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

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
                if (!subtleSlider) this.rect = new DyeableRect(menu, owner, this.pos + new Vector2(4f, -8f), new Vector2(24f, 16f), true);
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
                if (!subtleSlider) this.rect = new DyeableRect(menu, owner, this.pos + new Vector2(-8f, 4f), new Vector2(16f, 24f), true);
                //if (rangeSlider) { this.lineSprites[4].scaleY = 2f; this.lineSprites[4].anchorX = 1f; }
            }
            if (!subtleSlider)
            {
                this.subObjects.Add(this.rect);
                this.label = new MenuLabel(menu, owner, this.value, this.rect.pos, new Vector2(40f, 20f), false);
                this.label.label.alpha = 0f;
                this.label.label.alignment = FLabelAlignment.Center;
                this.subObjects.Add(this.label);
                this.myContainer.sortZ = this.rect.sprites[0].sortZ + 1f;
            }
        }

        /// <summary>
        /// The amount that changes when you scroll your mousewheel over <see cref="OpSlider"/>
        /// </summary>
        public int wheelTick;

        internal readonly int min, max;
        internal readonly bool vertical;
        internal float mul;

        internal FSprite[] lineSprites;
        internal DyeableRect rect;
        internal MenuLabel label;

        bool SelectableUIelement.IsMouseOverMe { get { return !this.held && this.MouseOver; } }

        bool SelectableUIelement.CurrentlySelectableMouse { get { return !this.disabled; } }

        bool SelectableUIelement.CurrentlySelectableNonMouse { get { return !this.disabled; } }

        /// <summary>
        /// Text and Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        /// <summary>
        /// Colour of Lines. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorLine;

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            float add = this.greyedOut ? 0f : this.bumpBehav.AddSize;
            Vector2 cutPos, cutSize;
            if (!vertical)
            {
                if (!subtleSlider)
                {
                    float p = this.mul * (float)(this.valueInt - this.min);
                    this.rect.pos.y = this.pos.y + 4f;
                    this.rect.pos.x = this.pos.x + p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.pos = new Vector2(this.rect.pos.x - 14f, this.rect.pos.y + 25f);
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (this.valueInt - this.min) * this.mul;
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
                if (!subtleSlider)
                {
                    float p = this.mul * (float)(this.valueInt - this.min);
                    this.rect.pos.x = this.pos.x + 4f;
                    this.rect.pos.y = this.pos.y + p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.pos = new Vector2(this.rect.pos.x - 10f, this.rect.pos.y + 18f);
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (this.valueInt - this.min) * this.mul;
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
                if (subtleSlider) return;
                this.rect.colorF = this.bumpBehav.GetColor(this.colorFill);
                this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
                this.label.label.color = this.bumpBehav.GetColor(this.colorEdge);
                if (this.MouseOver) { this.label.label.alpha = Mathf.Min(0.5f, this.label.label.alpha + 0.05f); }
                else { this.label.label.alpha = Mathf.Max(0f, this.label.label.alpha - 0.1f); }
                return;
            }

            if (!subtleSlider)
            {
                if (this.held || MouseOver)
                { this.label.label.alpha = Mathf.Min(this.label.label.alpha + 0.1f, 1f); }
                else { this.label.label.alpha = Mathf.Max(this.label.label.alpha - 0.15f, 0f); }

                this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
                if (this.held) { this.rect.colorF = this.bumpBehav.GetColor(this.colorEdge); }
                else { this.rect.colorF = this.bumpBehav.GetColor(this.colorFill); }

                this.rect.fillAlpha = this.held ? 1f : this.bumpBehav.FillAlpha;
            }

            Color color = this.bumpBehav.GetColor(this.colorLine);
            foreach (FSprite l in this.lineSprites) { l.color = color; }
            if (!subtleSlider) { this.label.label.color = Color.Lerp(this.rect.color, Color.white, 0.5f); }
        }

        internal virtual void LineVisibility(Vector2 cutPos, Vector2 cutSize)
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

        public override void Update(float dt)
        {
            base.Update(dt);
            if (disabled) { return; }

            if (this.held)
            {
                if (Input.GetMouseButton(0))
                {
                    float t = this.vertical ? this.MousePos.y : this.MousePos.x;
                    this.valueInt = Mathf.Clamp(Mathf.RoundToInt(t / this.mul) + this.min, this.min, this.max);
                }
                else
                {
                    this.held = false;
                }
            }
            else if (!this.held && this.menu.manager.menuesMouseMode && this.MouseOver)
            {
                if (Input.GetMouseButton(0))
                {
                    this.held = true;
                    menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                }
                else if (this.menu.mouseScrollWheelMovement != 0)
                {
                    int num = valueInt - (int)Mathf.Sign(this.menu.mouseScrollWheelMovement) * this.wheelTick;
                    num = Custom.IntClamp(num, this.min, this.max);
                    if (num != valueInt)
                    {
                        this.valueInt = num;
                    }
                }
            }
        }

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            if (MouseOver || held)
            {
                if (!_soundFilled)
                {
                    _soundFill += 5;
                    menu.PlaySound(SoundID.MENU_Scroll_Tick);
                }
                this.bumpBehav.flash = Mathf.Min(1f, this.bumpBehav.flash + 0.5f);
                this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
            }
            if (!subtleSlider) this.label.label.text = this.value;

            /*
            int r = this.max - this.min + 1;
            float l = Mathf.Max((float)(r - 1), (float)(!vertical ? this._size.x : this._size.y));
            this.mul = l / (r - 1);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            */
        }

        public override void Show()
        {
            base.Show();
            for (int i = 0; i < lineSprites.Length; i++) { lineSprites[i].isVisible = true; }
            if (!subtleSlider)
            {
                this.rect.Show();
                this.label.label.isVisible = true;
            }

            #region visibility

            float add = this.greyedOut ? 0f : this.bumpBehav.AddSize;
            Vector2 cutPos, cutSize;
            if (!vertical)
            {
                if (!subtleSlider)
                {
                    float p = this.mul * (float)(this.valueInt - this.min);
                    this.rect.pos.y = this.pos.y + 4f;
                    this.rect.pos.x = this.pos.x + p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.pos = new Vector2(this.rect.pos.x - 14f, this.rect.pos.y + 25f);
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (this.valueInt - this.min) * this.mul;
                    float s = Mathf.Clamp(add, 0f, 1f);
                    s = 1.0f + s * 0.3f;
                    cutPos = new Vector2(t, 15f);
                    cutSize = new Vector2(17f * s, 17f * s);
                    cutPos -= cutSize / 2f;
                }
            }
            else
            {
                if (!subtleSlider)
                {
                    float p = this.mul * (float)(this.valueInt - this.min);
                    this.rect.pos.x = this.pos.x + 4f;
                    this.rect.pos.y = this.pos.y + p - 8f;
                    this.rect.addSize = new Vector2(4f, 4f) * add;
                    this.label.pos = new Vector2(this.rect.pos.x - 10f, this.rect.pos.y + 18f);
                    cutPos = this.rect.pos - this.pos;
                    cutSize = this.rect.size;
                    cutPos -= this.rect.addSize / 2f;
                    cutSize += this.rect.addSize;
                }
                else
                {
                    float t = (this.valueInt - this.min) * this.mul;
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

        public override void Hide()
        {
            base.Hide();
            for (int i = 0; i < lineSprites.Length; i++) { lineSprites[i].isVisible = false; }
            if (!subtleSlider)
            {
                this.rect.Hide();
                this.label.label.isVisible = false;
            }
        }

        public override void Unload()
        {
            for (int i = 0; i < lineSprites.Length; i++) { lineSprites[i].RemoveFromContainer(); }
            base.Unload();
        }

        public override bool CopyFromClipboard(string value)
        {
            this.held = false;
            return base.CopyFromClipboard(value);
        }
    }
}
