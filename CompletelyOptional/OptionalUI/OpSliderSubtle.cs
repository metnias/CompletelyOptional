using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpSliderSubtle : OpSlider, SelectableUIelement
    {
        /// <summary>
        /// SubtleSlider that let you input integer in small range
        /// </summary>
        /// <param name="pos">left-bottom corner coordinate (excluding extra length in the end of slider line)</param>
        /// <param name="key">unique keyword for this UIconfig</param>
        /// <param name="range">x = min, y = max</param>
        /// <param name="length">Length of this slider will be this (Min: 20 * range; The width is 30 pxl)</param>
        /// <param name="vertical">if true, the slider will go vertical and the length will be used as height</param>
        /// <param name="defaultValue">default integer value</param>
        /// <exception cref="ElementFormatException">Thrown when the range of this is less than 2 or more than 30</exception>
        public OpSliderSubtle(Vector2 pos, string key, IntVector2 range, int length, bool vertical = false, int defaultValue = 0) : base(pos, key, range, length, vertical, defaultValue)
        {
            int r = range.y - range.x + 1;
            if (r > 31) { throw new ElementFormatException(this, "The range of OpSliderSubtle should be lower than 31! Use normal OpSlider instead.", key); }
            else if (r < 2) { throw new ElementFormatException(this, "The range of OpSliderSubtle is less than 2! Check your \'range\' parameter.", key); }
            float l = Mathf.Max(32f, (float)length);
            this.mul = Mathf.Max(l / (r - 1), 8f);
            l = Mathf.Max(this.mul * (r - 1), l);
            this._size = this.vertical ? new Vector2(30f, l) : new Vector2(l, 30f);
            this.fixedSize = this._size;
            this._value = Custom.IntClamp(defaultValue, min, max).ToString();
            this.wheelTick = 1;
            this.defaultValue = this.value;

            if (!_init) { return; }

            this.Nobs = new FSprite[r];
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                this.Nobs[i] = new FSprite("pixel", true) { anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.Nobs[i]);
            }
            this.Circle = new FSprite("buttonCircleB", true) { anchorX = 0.5f, anchorY = 0.5f }; //"buttonCircleB"
            this.myContainer.AddChild(this.Circle);
            this.myContainer.sortZ = this.Nobs[0].sortZ + 1f;
        }

        protected internal override void Initialize()
        {
            base.Initialize();
            if (!_init) { return; }
        }

        private readonly FSprite[] Nobs;
        private readonly FSprite Circle;

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            float s = !disabled ? Mathf.Clamp(this.bumpBehav.AddSize, 0f, 1f) : 0f;
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
            if (this.vertical) { this.Circle.x = 15.01f; this.Circle.y = this.mul * (this.valueInt - this.min) + 0.01f; }
            else { this.Circle.y = 15.01f; this.Circle.x = this.mul * (this.valueInt - this.min) + 0.01f; }
            this.Circle.scale = 1.0f + s * 0.3f;
            this.Circle.color = this.bumpBehav.GetColor(this.colorEdge);
        }

        protected internal override void LineVisibility(Vector2 cutPos, Vector2 cutSize)
        {
            base.LineVisibility(cutPos, cutSize);
            this.lineSprites[0].isVisible = false;
            this.lineSprites[3].isVisible = false;
            for (int i = 0; i < this.Nobs.Length; i++)
            {
                if (this.valueInt - this.min == i) { this.Nobs[i].isVisible = false; }
                else { this.Nobs[i].isVisible = true; }
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            // if (this.isHidden) { return; }
        }

        public override void OnChange()
        {
            base.OnChange();
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Unload()
        {
            base.Unload();
        }
    }
}
