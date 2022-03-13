using CompletelyOptional;
using Menu;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simple TextBox for general purpose.
    /// </summary>
    public class OpTextBox : UIconfig
    {
        /// <summary>
        /// Simple TextBox.
        /// </summary>
        /// <param name="pos">LeftBottom <see cref="UIelement.pos"/>.</param>
        /// <param name="sizeX">Horizontal size (min = 30 pxl). The height is fixed to 24 pxl.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultValue">Default string value</param>
        /// <param name="accept">Which type of text you want to <see cref="Accept"/></param>
        public OpTextBox(Vector2 pos, float sizeX, string key, string defaultValue = "TEXT", Accept accept = Accept.StringASCII) : base(pos, new Vector2(30f, 24f), key, defaultValue)
        {
            this._size = new Vector2(Mathf.Max(IsUpdown ? 40f : 30f, sizeX), IsUpdown ? 30f : 24f);
            this.description = InternalTranslator.Translate("Click and Type text");

            this.accept = accept;
            this._value = defaultValue;
            this._lastValue = defaultValue;
            this.defaultValue = this.value;
            this.maxLength = Mathf.FloorToInt((size.x - 20f) / 6f);
            if (this.accept == Accept.Float || this.accept == Accept.Int) { this.allowSpace = float.Parse(defaultValue) < 0f; }
            else { this.allowSpace = defaultValue.Contains(" "); }
            this.password = false;
            this.mouseDown = false;

            //this.keyboardOn = false;
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorText = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(myContainer, Vector2.zero, this.size, true) { fillAlpha = 0.5f };

            this.label = OpLabel.CreateFLabel(defaultValue);
            this.label.anchorX = 0f; this.label.anchorY = 1f;
            this.label.color = this.colorText;
            this.myContainer.AddChild(this.label);

            //this.label.label.SetPosition(new Vector2(5f, 3f) - (this._size * 0.5f));

            this.cursor = new FCursor();
            cursor.SetPosition(LabelTest.GetWidth(value) + LabelTest.CharMean(false), this.size.y * 0.5f - 7f);
            this.myContainer.AddChild(this.cursor);

            OnChange();
        }

        /// <summary>
        /// Whether this is <see cref="OpUpdown"/> or not
        /// </summary>
        public bool IsUpdown => this is OpUpdown;

        /// <summary>
        /// FLabel of TextBox.
        /// </summary>
        public readonly FLabel label;

        /// <summary>
        /// Boundary DyeableRect.
        /// </summary>
        public readonly DyeableRect rect;

        protected string _lastValue;
        protected readonly FCursor cursor;
        protected float cursorAlpha = 0f, lastCursorAlpha;

        /// <summary>
        /// Edge Colour of DyeableRect. Default is MediumGrey.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Text Colour, which affects Cursor Colour too. Default is MediumGrey.
        /// </summary>
        public Color colorText;

        /// <summary>
        /// Fill Colour of DyeableRect. Default is Black.
        /// </summary>
        public Color colorFill;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (greyedOut)
            {
                KeyboardOn = false;
                this.label.color = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(this.colorText));
                this.cursor.alpha = 0f;
                this.rect.colorEdge = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(this.colorEdge));
                this.rect.colorFill = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(this.colorFill));
                this.rect.GrafUpdate(timeStacker);
                return;
            }
            Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);

            //this.label.label.color = grey;
            //this.rect.color = grey;

            if (KeyboardOn) { this.col = Mathf.Min(1f, this.bumpBehav.col + 0.1f); }
            else { this.col = Mathf.Max(0f, this.bumpBehav.col - 0.0333333351f); }

            this.cursor.color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), this.colorText, this.bumpBehav.Sin());
            this.cursor.alpha = Mathf.Clamp01(Mathf.Lerp(lastCursorAlpha, cursorAlpha, timeStacker));
            this.cursor.x = IsUpdown ? this.size.x - (LabelTest.GetWidth(this.label.text) + LabelTest.CharMean(false) + 35f)
                : 10f + LabelTest.GetWidth(this.label.text) + LabelTest.CharMean(false);
            this.cursor.y = this.size.y * 0.5f - 7f;

            this.bumpBehav.col = this.col;
            this.rect.fillAlpha = Mathf.Lerp(0.5f, 0.8f, this.bumpBehav.col);
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.colorFill = this.colorFill;
            this.rect.GrafUpdate(timeStacker);
            this.label.color = Color.Lerp(this.colorText, white, Mathf.Clamp(this.bumpBehav.flash, 0f, 1f));
        }

        private float col;

        private int inputDelay;

        public override void Update()
        {
            this.rect.Update();
            this.col = this.bumpBehav.col;
            base.Update();
            this.bumpBehav.col = this.col;
            if (greyedOut) { return; }
            lastCursorAlpha = cursorAlpha;

            if (KeyboardOn)
            {
                cursorAlpha -= 0.05f / frameMulti;
                if (cursorAlpha < -0.5f) { cursorAlpha = 2f; }

                int lastInputDelay = inputDelay;
                foreach (char c in Input.inputString)
                {
                    // ComOptPlugin.LogInfo($"cA({cursorAlpha:F2}) input: {c}");
                    if (c == '\b')
                    {
                        inputDelay++;
                        if (inputDelay > 1 && (inputDelay <= ModConfigMenu.DASinit || inputDelay % ModConfigMenu.DASdelay != 1)) { break; }
                        cursorAlpha = 2.5f; this.bumpBehav.flash = 2.5f;
                        if (this.value.Length > 0)
                        {
                            this._value = this.value.Substring(0, this.value.Length - 1);
                            PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                            OnChange();
                        }
                        break;
                    }
                    else if ((c == '\n') || (c == '\r')) // enter/return
                    {
                        KeyboardOn = false;
                        PlaySound(SoundID.MENU_Checkbox_Check);
                        break;
                    }
                    else
                    {
                        inputDelay++;
                        if (inputDelay > 1 && (inputDelay <= ModConfigMenu.DASinit || inputDelay % ModConfigMenu.DASdelay != 1)) { break; }
                        cursorAlpha = 2.5f; this.bumpBehav.flash = 2.5f;
                        this.value += c;
                        break;
                    }
                }
                if (lastInputDelay == inputDelay) { inputDelay = 0; } // Key not pressed
            }
            else { cursorAlpha = 0f; inputDelay = 0; }
            if (Input.GetMouseButton(0) && !mouseDown)
            {
                if (this is OpUpdown ud && ud.mouseOverArrow) { return; }
                mouseDown = true;
                if (MouseOver && !KeyboardOn)
                { //Turn on
                    PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                    KeyboardOn = true;
                    this.cursor.isVisible = true;
                    this.cursorAlpha = 1f;
                    cursor.SetPosition(LabelTest.GetWidth(this.label.text) + LabelTest.CharMean(false), this.size.y * 0.5f - 7f);
                }
                else if (!MouseOver && KeyboardOn)
                { //Shutdown
                    KeyboardOn = false;
                    PlaySound(SoundID.MENU_Checkbox_Uncheck);
                }
            }
            else if (!Input.GetMouseButton(0))
            {
                mouseDown = false;
            }
        }

        protected bool mouseDown;

        protected bool KeyboardOn
        {
            get => this._keyboardOn;
            set
            {
                if (this._keyboardOn == value) { return; }
                this.held = value;
                if (this._keyboardOn && !value)
                {
                    this.cursor.isVisible = false;
                    /* if (this.accept == Accept.Float)
                    {
                        if (!float.TryParse(this.value, out _))
                        {
                            for (int i = this.value.Length - 1; i > 0; i--)
                            {
                                if (float.TryParse(this.value.Substring(0, i), out _))
                                {
                                    this._value = this.value.Substring(0, i);
                                    OnChange();
                                    PlaySound(SoundID.Mouse_Light_Switch_On);
                                    return;
                                }
                            }
                        }
                    } */
                }
                this._keyboardOn = value;
                OnChange();
            }
        }

        private bool _keyboardOn = false;

        /// <summary>
        /// If you want to hide what is written for whatever reason,
        /// even though Rain World is singleplayer game.
        /// </summary>
        public bool password;

        /// <summary>
        /// whether you allow space(for numbers, negative) or not.
        /// <para>Default is false, unless defaultValue is impossible with allowSpace is false then this is set to true instead.</para>
        /// </summary>
        public bool allowSpace;

        /// <summary>
        /// Which type of string this accepts. See also <see cref="allowSpace"/>
        /// </summary>
        public readonly Accept accept;

        /// <summary>
        /// maximum length. default is 100.
        /// </summary>
        public int maxLength
        {
            get { return _maxLength; }
            set
            {
                if (value < 2 || this._maxLength == value) { return; }
                _maxLength = value;
                if (this.value.Length > _maxLength)
                {
                    this.value = this.value.Substring(0, _maxLength);
                }
            }
        }

        private int _maxLength;

        public enum Accept
        {
            /// <summary>
            /// Can be parsed to <see cref="int"/>
            /// </summary>
            Int,

            /// <summary>
            /// Can be parsed to <see cref="float"/>
            /// </summary>
            Float,

            /// <summary>
            /// Only accepts english alphabet without spacing
            /// </summary>
            StringEng,

            /// <summary>
            /// Accepts all ASCII characters
            /// </summary>
            StringASCII
        }

        /// <summary>
        /// value in <see cref="int"/> form.
        /// </summary>
        protected int valueInt
        {
            get
            {
                switch (this.accept)
                {
                    default: if (int.TryParse(value, out int t)) { return t; } return 0;
                    case Accept.Int:
                        return int.Parse(value);

                    case Accept.Float:
                        if (float.TryParse(value, out float temp))
                        { return Mathf.FloorToInt(temp); }
                        return 0;
                }
            }
            set { this.value = value.ToString(); }
        }

        /// <summary>
        /// value in <see cref="float"/> form.
        /// </summary>
        protected float valueFloat
        {
            get
            {
                switch (this.accept)
                {
                    default: if (float.TryParse(value, out float t)) { return t; } return 0f;
                    case Accept.Int:
                        return (float)int.Parse(value);

                    case Accept.Float:
                        if (float.TryParse(value, out float temp)) { return temp; }
                        return 0f;
                }
            }
            set { this.value = value.ToString(); }
        }

        public override string value
        {
            get { return base.value; }
            set
            {
                if (base.value == value) { return; }
                _lastValue = base.value;
                this._value = value;

                if (this.accept == Accept.Int || this.accept == Accept.Float || !this.allowSpace)
                {
                    char[] temp0 = base.value.ToCharArray();
                    for (int t = 0; t < temp0.Length; t++)
                    {
                        if (!char.IsWhiteSpace(temp0[t])) { continue; }
                        this._value = _lastValue;
                        _lastValue = base.value;
                        return;
                    }
                }

                switch (this.accept)
                {
                    case Accept.Int:
                        // if (Regex.IsMatch(base.value, "^[0-9/-]+$")) { goto accepted; }
                        if (int.TryParse(base.value, out int _) || (this.allowSpace && base.value == "-")) { goto accepted; }
                        break;

                    case Accept.Float:
                        // if (Regex.IsMatch(base.value, "^[0-9/./-]+$")) { goto accepted; }
                        if (float.TryParse(base.value, out float _) || (this.allowSpace && base.value == "-")) { goto accepted; }
                        break;

                    default:
                    case Accept.StringEng:
                        if (Regex.IsMatch(base.value, "^[a-zA-Z/s]+$")) { goto accepted; }
                        break;

                    case Accept.StringASCII:
                        if (Regex.IsMatch(base.value, "^[\x20-\x7E/s]+$")) { goto accepted; }
                        break;
                }
                //revert
                this._value = _lastValue;
                _lastValue = base.value;
                return;

            accepted:
                if (KeyboardOn && Input.anyKey)
                {
                    PlaySound(SoundID.MENU_Checkbox_Uncheck);
                }
                if (this.tab != null) { ConfigContainer.instance.NotifyConfigChange(this, _lastValue, this._value); }

                OnChange();
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(IsUpdown ? 40f : 30f, this._size.x), IsUpdown ? 30f : 24f);
            base.OnChange();

            if (!this.password)
            {
                if (IsUpdown && !KeyboardOn)
                {
                    if ((this as OpUpdown).IsInt) { this.label.text = valueInt.ToString("N0"); }
                    else { this.label.text = valueFloat.ToString("N" + (this as OpUpdown).dNum); }
                }
                else { this.label.text = this.value; }
            }
            else
            {
                this.label.text = "";
                for (int n = 0; n < this.value.Length; n++)
                { this.label.text += "#"; }
            }

            this.rect.size = this.size;
            if (IsUpdown)
            {
                this.label.alignment = FLabelAlignment.Right;
                this.label.x = this.size.x - 30f; this.label.y = 23f;
            }
            else
            {
                this.label.x = 5f; this.label.y = 20f;
            }
        }

        protected internal override void Deactivate()
        {
            base.Deactivate();
            this.KeyboardOn = false;
        }

        protected internal override bool CopyFromClipboard(string value)
        {
            if (!KeyboardOn) { return false; }
            try { string old = this.value; this.value = value; return this.value != old; }
            catch { return false; }
        }

        protected internal override string CopyToClipboard()
        {
            cursorAlpha = 0f;
            this.cursor.alpha = 0f;
            return base.CopyToClipboard();
        }
    }
}