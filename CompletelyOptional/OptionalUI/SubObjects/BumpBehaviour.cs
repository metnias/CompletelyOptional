using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
    /// </summary>
    public class BumpBehaviour
    {
        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        /// <param name="owner"><see cref="UIelement"/> owner</param>
        public BumpBehaviour(UIelement owner)
        {
            this.owner = owner;
            this._greyedOut = false; this._held = false;
        }

        /// <summary>
        /// <see cref="UIelement"/> owner
        /// </summary>
        public readonly UIelement owner;

        public float sizeBump, extraSizeBump;
        public float col, flash, sin;
        public bool flashBool;

        public bool greyedOut
        {
            get
            {
                if (owner is UIconfig c) { return c.greyedOut; }
                if (owner is UItrigger t) { return t.greyedOut; }
                return _greyedOut;
            }
            set { _greyedOut = value; }
        }

        public bool held
        {
            get
            {
                if (owner is UIconfig c) { return c.held; }
                if (owner is UItrigger t) { return t.held; }
                return _held;
            }
            set { _held = value; }
        }

        private bool _greyedOut, _held;

        /// <summary>
        /// Grab Reactive Color with BumpBehav
        /// </summary>
        /// <param name="orig">Original Color</param>
        /// <returns>Reactive Color</returns>
        public Color GetColor(Color orig)
        {
            if (greyedOut) { return DyeableRect.Grayscale(DyeableRect.MidToVeryDark(orig)); }

            return Color.Lerp(orig, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Mathf.Max(Mathf.Min(this.col, this.held ? 0.5f : 0.8f) / 2f, Mathf.Clamp01(this.flash)));
        }

        /// <summary>
        /// This will be called automatically with <see cref="UIconfig.Update(float)"/> or <see cref="UItrigger.Update(float)"/>
        /// </summary>
        public void Update()
        {
            this.flash = Custom.LerpAndTick(this.flash, 0f, 0.03f, 0.166666672f);
            if (this.owner.MouseOver)
            {
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 1f, 0.1f, 0.1f);
                this.sin += 1f;
                if (!this.flashBool)
                { this.flashBool = true; this.flash = 1f; }
                if (!this.greyedOut)
                {
                    this.col = Mathf.Min(1f, this.col + 0.1f);
                    this.extraSizeBump = Mathf.Min(1f, this.extraSizeBump + 0.1f);
                }
            }
            else
            {
                this.flashBool = false;
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 0f, 0.1f, 0.05f);
                this.col = Mathf.Max(0f, this.col - 0.0333333351f);
                this.extraSizeBump = 0f;
            }
        }

        /// <summary>
        /// Multiplay this with <c>new Vector2(4f, 4f)</c> and apply to <see cref="DyeableRect"/>.addSize
        /// </summary>
        public float AddSize => this.held || (this.owner.MouseOver && Input.GetMouseButton(0)) ? 0.5f * this.sizeBump : this.sizeBump + 0.5f * Mathf.Sin(this.extraSizeBump * 3.14159274f);

        /// <summary>
        /// Apply this to <see cref="DyeableRect"/>.fillAlpha
        /// </summary>
        public float FillAlpha => Mathf.Lerp(0.3f, 0.6f, this.col);

        /// <summary>
        /// Use this with <see cref="Color.Lerp(Color, Color, float)"/> to make colour blinking.
        /// </summary>
        /// <param name="period">Default is 30f. Recommend 10f for fast blinking.</param>
        public float Sin(float period = 30f) => 0.5f - 0.5f * Mathf.Sin(this.sin / period * 3.14159274f);
    }
}
