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
            this._greyedOut = null; this._held = false;
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
                if (_greyedOut.HasValue) { return _greyedOut.Value; }
                if (owner is ICanBeFocused fc) { return fc.GreyedOut; }
                return false;
            }
            set { _greyedOut = value; }
        }

        public bool held
        {
            get
            {
                if (_held.HasValue) { return _held.Value; }
                if (owner is UIconfig c) { return c.held; }
                if (owner is UItrigger t) { return t.held; }
                return false;
            }
            set { _held = value; }
        }

        private bool? _greyedOut, _held;

        public bool MouseOver
        {
            get { if (!_mouseOver.HasValue) { return this.owner.MouseOver; } return _mouseOver.Value; }
            set { _mouseOver = value; }
        }

        private bool? _mouseOver = null;

        /// <summary>
        /// Grab Reactive Color with BumpBehav
        /// </summary>
        /// <param name="orig">Original Color</param>
        /// <returns>Reactive Color</returns>
        public Color GetColor(Color orig)
        {
            if (greyedOut) { return MenuColorEffect.Greyscale(MenuColorEffect.MidToVeryDark(orig)); }

            return Color.Lerp(orig, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Mathf.Max(Mathf.Min(this.col, this.held ? 0.5f : 0.8f) / 2f, Mathf.Clamp01(this.flash)));
        }

        /// <summary>
        /// This will be called automatically with <see cref="UIconfig.Update"/> or <see cref="UItrigger.Update"/>.
        /// <para>If you added this manually in your custom UIelement, add this in your <see cref="UIelement.Update"/></para>
        /// </summary>
        public void Update()
        {
            float dtMulti = 1f / UIelement.frameMulti;
            this.flash = Custom.LerpAndTick(this.flash, 0f, 0.03f, 0.16667f * dtMulti);
            if (MouseOver)
            {
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 1f, 0.1f, 0.1f * dtMulti);
                this.sin += 1f * dtMulti;
                if (!this.flashBool)
                { this.flashBool = true; this.flash = 1f; }
                if (!this.greyedOut)
                {
                    this.col = Mathf.Min(1f, this.col + 0.1f * dtMulti);
                    this.extraSizeBump = Mathf.Min(1f, this.extraSizeBump + 0.1f * dtMulti);
                }
            }
            else
            {
                this.flashBool = false;
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 0f, 0.1f, 0.05f * dtMulti);
                this.col = Mathf.Max(0f, this.col - 0.03333f * dtMulti);
                this.extraSizeBump = 0f;
            }
        }

        /// <summary>
        /// Multiplay this with <c>new Vector2(4f, 4f)</c> and apply to <see cref="DyeableRect"/>.addSize
        /// </summary>
        public float AddSize => this.held || (this.owner.MouseOver && Input.GetMouseButton(0)) ? 0.5f * this.sizeBump : this.sizeBump + 0.5f * Mathf.Sin(this.extraSizeBump * 3.1416f);

        /// <summary>
        /// Apply this to <see cref="DyeableRect"/>.fillAlpha
        /// </summary>
        public float FillAlpha => Mathf.Lerp(0.3f, 0.6f, this.col);

        /// <summary>
        /// Use this with <see cref="Color.Lerp(Color, Color, float)"/> to make colour blinking.
        /// </summary>
        /// <param name="period">Default is 30f. Recommend 10f for fast blinking.</param>
        public float Sin(float period = 30f) => 0.5f - 0.5f * Mathf.Sin(this.sin / period * 3.1416f);
    }
}