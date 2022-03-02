using CompletelyOptional;
using Menu;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simple TextBox for general purpose.
    /// </summary>
    public class OpTextBox : UIconfig, ICanBeFocused
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

            if (!_init) { return; }

            //this.keyboardOn = false;
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorText = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(menu, owner, this.pos, this.size, true) { fillAlpha = 0.5f };
            this.subObjects.Add(this.rect);

            this.label = new MenuLabel(menu, owner, defaultValue, this.pos + new Vector2(5f - this._size.x * 0.5f, IsUpdown ? 13f : 10f), new Vector2(this._size.x, 20f), false);
            this.label.label.alignment = FLabelAlignment.Left;
            this.label.label.SetAnchor(0f, 1f);
            this.label.label.color = this.colorText;
            //this.label.label.SetPosition(new Vector2(5f, 3f) - (this._size * 0.5f));
            this.subObjects.Add(this.label);

            this.cursor = new FCursor();
            cursor.SetPosition(LabelTest.GetWidth(value, false) + LabelTest.CharMean(false), this.size.y * 0.5f - 7f);
            this.myContainer.AddChild(this.cursor);
        }

        /// <summary>
        /// Whether this is <see cref="OpUpdown"/> or not
        /// </summary>
        public bool IsUpdown => this is OpUpdown;

        /// <summary>
        /// MenuLabel of TextBox.
        /// </summary>
        public MenuLabel label;

        /// <summary>
        /// Boundary DyeableRect.
        /// </summary>
        public DyeableRect rect;

        protected string _lastValue;
        private readonly FCursor cursor;
        private float cursorAlpha;

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

        bool ICanBeFocused.IsMouseOverMe { get { return !this.held && this.MouseOver; } }

        bool ICanBeFocused.CurrentlyFocusableMouse { get { return !this.disabled; } }

        bool ICanBeFocused.CurrentlyFocusableNonMouse { get { return !this.disabled; } }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (greyedOut)
            {
                KeyboardOn = false;
                this.label.label.color = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorText));
                this.cursor.alpha = 0f;
                this.rect.colorEdge = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorEdge));
                this.rect.colorFill = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorFill));
                return;
            }
            Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);

            //this.label.label.color = grey;
            //this.rect.color = grey;

            if (KeyboardOn) { this.col = Mathf.Min(1f, this.bumpBehav.col + 0.1f); }
            else { this.col = Mathf.Max(0f, this.bumpBehav.col - 0.0333333351f); }

            this.cursor.color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), this.colorText, this.bumpBehav.Sin());

            this.bumpBehav.col = this.col;
            this.rect.fillAlpha = Mathf.Lerp(0.5f, 0.8f, this.bumpBehav.col);
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.colorFill = this.colorFill;
            this.label.label.color = Color.Lerp(this.colorText, white, Mathf.Clamp(this.bumpBehav.flash, 0f, 1f));
        }

        private float col;

        public override void Update(float dt)
        {
            this.col = this.bumpBehav.col;
            base.Update(dt);
            this.bumpBehav.col = this.col;
            if (disabled) { return; }

            if (KeyboardOn)
            {
                cursor.alpha = Mathf.Clamp(cursorAlpha, 0f, 1f);
                cursorAlpha -= 0.05f;
                if (cursorAlpha < -0.5f) { cursorAlpha = 2f; }

                //input spotted! ->cursorAlpha = 2.5f;
                foreach (char c in Input.inputString)
                {
                    //Debug.Log(string.Concat("input: ", c));
                    if (c == '\b')
                    {
                        cursorAlpha = 2.5f; this.bumpBehav.flash = 2.5f;
                        if (this.value.Length > 0)
                        {
                            this._value = this.value.Substring(0, this.value.Length - 1);
                            if (!_soundFilled)
                            {
                                _soundFill += 12;
                                menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                            }
                            OnChange();
                        }
                        break;
                    }
                    else if ((c == '\n') || (c == '\r')) // enter/return
                    {
                        KeyboardOn = false;
                        menu.PlaySound(SoundID.MENU_Checkbox_Check);
                        break;
                    }
                    else
                    {
                        cursorAlpha = 2.5f;
                        this.bumpBehav.flash = 2.5f;
                        this.value += c;
                    }
                }
            }
            else { cursorAlpha = 0f; }
            if (Input.GetMouseButton(0) && !mouseDown)
            {
                if (this is OpUpdown ud && ud.mouseOverArrow) { return; }
                mouseDown = true;
                if (MouseOver && !KeyboardOn)
                { //Turn on
                    menu.PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                    KeyboardOn = true;
                    this.cursor.isVisible = true;
                    this.cursorAlpha = 1f;
                    cursor.SetPosition(LabelTest.GetWidth(this.label.text, false) + LabelTest.CharMean(false), this.size.y * 0.5f - 7f);
                }
                else if (!MouseOver && KeyboardOn)
                { //Shutdown
                    KeyboardOn = false;
                    menu.PlaySound(SoundID.MENU_Checkbox_Uncheck);
                }
            }
            else if (!Input.GetMouseButton(0))
            {
                mouseDown = false;
            }
        }

        protected internal bool mouseDown;

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
                                    menu.PlaySound(SoundID.Mouse_Light_Switch_On);
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
        public new int valueInt
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
        public new float valueFloat
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
                    if (!_soundFilled)
                    {
                        _soundFill += 8;
                        menu.PlaySound(SoundID.MENU_Checkbox_Uncheck);
                    }
                }

                OnChange();
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(IsUpdown ? 40f : 30f, this._size.x), IsUpdown ? 30f : 24f);
            base.OnChange();

            if (!this.password)
            {
                if (this is OpUpdown ud && !KeyboardOn)
                {
                    if (ud.IsInt) { this.label.text = valueInt.ToString("N0"); }
                    else { this.label.text = valueFloat.ToString("N" + ud.dNum); }
                }
                else { this.label.text = this.value; }
            }
            else
            {
                this.label.text = "";
                for (int n = 0; n < this.value.Length; n++)
                { this.label.text += "#"; }
            }

            this.rect.pos = this.pos;
            this.rect.size = this.size;
            this.label.pos = this.pos + new Vector2(5f - this.size.x * 0.5f, IsUpdown ? 13f : 10f);
            this.label.size = new Vector2(this._size.x, 20f);
            this.cursor.SetPosition(LabelTest.GetWidth(this.label.text, false) + LabelTest.CharMean(false), this.size.y * 0.5f - 7f);
        }

        public override void Hide()
        {
            base.Hide();
            this.KeyboardOn = false;
            this.label.label.isVisible = false;
            this.cursor.isVisible = false;
            this.rect.Hide();
        }

        public override void Show()
        {
            base.Show();

            this.label.label.isVisible = true;
            this.rect.Show();
        }

        public override void Unload()
        {
            base.Unload();
            //this.subObjects.Remove(this.label);
            //this.subObjects.Remove(this.rect);

            //this.myContainer.RemoveChild(this.cursor);
            this.cursor.RemoveFromContainer();
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