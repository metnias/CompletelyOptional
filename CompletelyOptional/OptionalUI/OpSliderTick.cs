using BepInEx.Configuration;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpSliderTick : OpSlider
    {
        /// <summary>
        /// SubtleSlider that let you input integer in small range
        /// </summary>
        /// <param name="pos">left-bottom corner coordinate (excluding extra length in the end of slider line)</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="length">Length of this slider will be this (Min: 20 * range; The width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="cosmeticValue">default integer value</param>
        /// <exception cref="ElementFormatException">Thrown when the range of this is less than 2 or more than 30</exception>
        public OpSliderTick(ConfigEntry<int> config, Vector2 pos, IntVector2 range, int length, bool vertical = false, int cosmeticValue = 0) : base(config, pos, range, length, vertical, cosmeticValue)
        {
            int r = range.y - range.x + 1;
            if (r > 31) { throw new ElementFormatException(this, "The range of OpSliderSubtle should be lower than 31! Use normal OpSlider instead.", key); }
            else if (r < 2) { throw new ElementFormatException(this, "The range of OpSliderSubtle is less than 2! Check your \'range\' parameter.", key); }
            float l = Mathf.Max(32f, (float)length);
            this.mul = Mathf.Max(l / (r - 1), 8f);
            l = Mathf.Max(this.mul * (r - 1), l);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this.fixedSize = this._size;
            this._value = Custom.IntClamp(!cosmetic ? (int)config.DefaultValue : cosmeticValue, min, max).ToString();
            this.mousewheelTick = 1;
            this.defaultValue = this.value;

            this.Nobs = new FSprite[r];
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                this.Nobs[i] = new FSprite("pixel", true) { anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.Nobs[i]);
            }
            this.circle = new FSprite("buttonCircleB", true) { anchorX = 0.5f, anchorY = 0.5f }; //"buttonCircleB"
            this.myContainer.AddChild(this.circle);
            this.myContainer.sortZ = this.Nobs[0].sortZ + 1f;
        }

        /// <summary>
        /// For GeneratedOI; 0-20
        /// </summary>
        internal OpSliderTick(ConfigEntry<byte> config, Vector2 pos, int length, bool vertical = false) : base(config, pos, new IntVector2(0, 20), length, vertical)
        {
            int r = 21;
            if (r > 31) { throw new ElementFormatException(this, "The range of OpSliderSubtle should be lower than 31! Use normal OpSlider instead.", key); }
            else if (r < 2) { throw new ElementFormatException(this, "The range of OpSliderSubtle is less than 2! Check your \'range\' parameter.", key); }
            float l = Mathf.Max(32f, (float)length);
            this.mul = Mathf.Max(l / (r - 1), 8f);
            l = Mathf.Max(this.mul * (r - 1), l);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this.fixedSize = this._size;
            this._value = Custom.IntClamp((int)config.DefaultValue, min, max).ToString();
            this.mousewheelTick = 1;
            this.defaultValue = this.value;

            this.Nobs = new FSprite[r];
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                this.Nobs[i] = new FSprite("pixel", true) { anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.Nobs[i]);
            }
            this.circle = new FSprite("buttonCircleB", true) { anchorX = 0.5f, anchorY = 0.5f }; //"buttonCircleB"
            this.myContainer.AddChild(this.circle);
            this.myContainer.sortZ = this.Nobs[0].sortZ + 1f;
        }

        protected readonly FSprite[] Nobs;
        protected readonly FSprite circle;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            float s = !greyedOut ? Mathf.Clamp(this.bumpBehav.AddSize, 0f, 1f) : 0f;
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                if (this.vertical)
                {
                    this.Nobs[i].x = 15.01f; this.Nobs[i].y = this.mul * i + 0.01f;
                    this.Nobs[i].scaleX = 6f + s * 4f; this.Nobs[i].scaleY = 2f;
                }
                else
                {
                    this.Nobs[i].y = 15.01f; this.Nobs[i].x = this.mul * i + 0.01f;
                    this.Nobs[i].scaleY = 6f + s * 4f; this.Nobs[i].scaleX = 2f;
                }
                this.Nobs[i].color = this.lineSprites[0].color;
            }
            if (this.vertical) { this.circle.x = 15.01f; this.circle.y = this.mul * (this.GetValueInt() - this.min) + 0.01f; }
            else { this.circle.y = 15.01f; this.circle.x = this.mul * (this.GetValueInt() - this.min) + 0.01f; }
            this.circle.scale = 1.0f + s * 0.3f;
            this.circle.color = this.bumpBehav.GetColor(this.colorEdge);
        }

        protected override void LineVisibility(Vector2 cutPos, Vector2 cutSize)
        {
            base.LineVisibility(cutPos, cutSize);
            this.lineSprites[0].isVisible = false;
            this.lineSprites[3].isVisible = false;
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                if (this.GetValueInt() - this.min == i) { this.Nobs[i].isVisible = false; }
                else { this.Nobs[i].isVisible = true; }
            }
        }
    }
}