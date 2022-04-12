using CompletelyOptional;
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
                if (owner is UIfocusable fc) { return fc.greyedOut; }
                return false;
            }
            set { _greyedOut = value; }
        }

        public bool held
        {
            get
            {
                if (_held.HasValue) { return _held.Value; }
                if (owner is UIfocusable c) { return c.held; }
                return false;
            }
            set { _held = value; }
        }

        private bool? _greyedOut, _held;

        public bool Focused
        {
            get
            {
                if (!_focused.HasValue)
                {
                    if (this.owner is UIfocusable)
                    { return this.owner.MenuMouseMode ? this.owner.MouseOver : (this.owner as UIfocusable).Focused; }
                    return this.owner.MouseOver;
                }
                return _focused.Value;
            }
            set { _focused = value; }
        }

        private bool? _focused = null;

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
        /// This will be called automatically with <see cref="UIfocusable.Update"/>.
        /// <para>If you added this manually in your custom UIelement, add this in your <see cref="UIelement.Update"/></para>
        /// </summary>
        public void Update()
        {
            float dtMulti = 1f / UIelement.frameMulti;
            this.flash = Custom.LerpAndTick(this.flash, 0f, 0.03f, 0.16667f * dtMulti);
            if (Focused)
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
            //if (held)
            {
                if (owner.CtlrInput.y == 0 && owner.CtlrInput.x != 0 && owner.CtlrInput.x == owner.LastCtlrInput.x) { scrollInitCounter++; }
                else if (owner.CtlrInput.x == 0 && owner.CtlrInput.y != 0 && owner.CtlrInput.y == owner.LastCtlrInput.y) { scrollInitCounter++; }
                else { scrollInitCounter = 0; }
                if (scrollInitCounter > ModConfigMenu.DASinit) { scrollCounter++; }
                else { scrollCounter = 0; }
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

        #region Joystick

        private int scrollInitCounter = 0;
        private int scrollCounter = 0;

        /// <summary>
        /// Shortcut to check whether Joystick has been pressed to a certain direction (and not held down).
        /// See also <seealso cref="JoystickPressAxis"/>, <seealso cref="JoystickHeld"/>.
        /// </summary>
        public bool JoystickPress(IntVector2 direction)
        {
            if (direction.x != 0)
            {
                if (direction.x != owner.CtlrInput.x) { return false; }
                if (owner.CtlrInput.x == owner.LastCtlrInput.x) { return false; }
            }
            else { if (owner.CtlrInput.x != 0) { return false; } }
            if (direction.y != 0)
            {
                if (direction.y != owner.CtlrInput.y) { return false; }
                if (owner.CtlrInput.y == owner.LastCtlrInput.y) { return false; }
            }
            else { if (owner.CtlrInput.y != 0) { return false; } }
            return true;
        }

        /// <summary>
        /// Shortcut to check whether Joystick has been pressed by certain axis. Returns 0 when it's not initial press.
        /// See also <seealso cref="JoystickPress"/>, <seealso cref="JoystickHeldAxis"/>.
        /// </summary>
        /// <param name="vertical"></param>
        /// <returns></returns>
        public int JoystickPressAxis(bool vertical)
        {
            if (vertical)
            {
                if (owner.CtlrInput.y != owner.LastCtlrInput.y) { return owner.CtlrInput.y; }
                return 0;
            }
            if (owner.CtlrInput.x != owner.LastCtlrInput.x) { return owner.CtlrInput.x; }
            return 0;
        }

        /// <summary>
        /// Shortcut to check whether Joystick has been pressed to a certain direction for long time.
        /// See also <seealso cref="JoystickHeldAxis"/>, <seealso cref="JoystickPress"/>.
        /// </summary>
        /// <param name="speed">The higher the speed, the quicker this turns true. If this is 0f, this'll return the value after DASinit is passed.</param>
        public bool JoystickHeld(IntVector2 direction, float speed = 1.0f)
        {
            if (speed > 0f && scrollCounter < Mathf.CeilToInt(ModConfigMenu.DASdelay / speed)) { return false; }
            if (direction.x != 0) { if (owner.CtlrInput.x != direction.x) { return false; } }
            else { if (owner.CtlrInput.x != 0) { return false; } }
            if (direction.y != 0) { if (owner.CtlrInput.y != direction.y) { return false; } }
            else { if (owner.CtlrInput.y != 0) { return false; } }
            scrollCounter = 0;
            return true;
        }

        /// <summary>
        /// Shortcut to check whether Joystick has been held down by certain axis. Returns 0 when it's not the right timing.
        /// See also <seealso cref="JoystickHeld"/>, <seealso cref="JoystickPressAxis"/>.
        /// </summary>
        /// <param name="vertical"></param>
        /// <param name="speed">The higher the speed, the quicker this turns value. If this is 0f, this'll return the value after DASinit is passed.</param>
        /// <returns></returns>
        public int JoystickHeldAxis(bool vertical, float speed = 1.0f)
        {
            if (speed > 0f && scrollCounter < Mathf.CeilToInt(ModConfigMenu.DASdelay / speed)) { return 0; }
            scrollCounter = 0;
            return vertical ? owner.CtlrInput.y : owner.CtlrInput.x;
        }

        /// <summary>
        /// Shortcut to register a button press
        /// </summary>
        public bool ButtonPress(ButtonType type)
        {
            switch (type)
            {
                default:
                case ButtonType.Jmp: return owner.CtlrInput.jmp && !owner.LastCtlrInput.jmp;
                case ButtonType.Mp: return owner.CtlrInput.mp && !owner.LastCtlrInput.mp;
                case ButtonType.Pckp: return owner.CtlrInput.pckp && !owner.LastCtlrInput.pckp;
                case ButtonType.Thrw: return owner.CtlrInput.thrw && !owner.LastCtlrInput.thrw;
            }
        }

        public enum ButtonType
        {
            Jmp,
            Mp,
            Pckp,
            Thrw
        }

        #endregion Joystick
    }
}