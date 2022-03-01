using Menu;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simple Label.
    /// </summary>
    public class OpLabel : UIelement
    {
        /// <summary>
        /// Simple Label that displays text. See also <seealso cref="OpLabel(float, float, string, bool)"/> for lazier version.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size of the Box</param>
        /// <param name="text">Text you want to display; max length 600. See also <seealso cref="OpLabelLong"/><para>Leaving this as TEXT will call <see cref="LoremIpsum"/>.</para></param>
        /// <param name="alignment">Alignment. Left/Center/Right. Center is default.</param>
        /// <param name="bigText">Whether this uses bigFont or not</param>
        public OpLabel(Vector2 pos, Vector2 size, string text = "TEXT", FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false) : base(pos, size)
        {
            this._size = new Vector2(Mathf.Max(size.x, 20f), Mathf.Max(size.y, 20f));
            this._bigText = bigText;
            this.autoWrap = false;
            if (text == "TEXT")
            { //Lorem Ipsum
                int lc = this.IsLong ? Math.Max(1, Mathf.FloorToInt(_size.y * 0.65f / LabelTest.LineHeight(_bigText))) : 1;
                int charLim = Mathf.FloorToInt(_size.x / LabelTest.CharMean(_bigText)) * lc;
                int sentLim = charLim / LoremIpsum.meanCharPerSentence;
                if (this.IsLong)
                {
                    int pn = UnityEngine.Random.Range(1, Mathf.CeilToInt(lc / 1.5f));
                    text = LoremIpsum.Generate(Math.Max(1, (sentLim - 2) / pn), Math.Max(2, sentLim / pn), pn);
                    //charLim = Mathf.FloorToInt(charLim * 0.65f);
                }
                else { text = LoremIpsum.Generate(Math.Max(1, sentLim - 2), Math.Max(1, sentLim)); }
                text = text.Length > charLim ? text.Substring(0, charLim - 2).TrimEnd() + "." : text;
            }
            if (this.IsLong)
            { this._text = text; _verticalAlignment = LabelVAlignment.Top; }
            else
            { this._text = text; _verticalAlignment = LabelVAlignment.Center; }
            //this.lineLength = Mathf.FloorToInt((size.x - 10f) / 6f);
            string cleanText = Regex.Replace(_text, @"\s+", "");
            this.isVFont = cleanText.Length > 0 && !LabelTest.HasNonASCIIChars(cleanText);
            if (!_init) { return; }
            this.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this._alignment = alignment;
            if (!this.IsLong)
            {
                this.label = new MenuLabel(menu, owner, _text, this.pos, this.size, this._bigText);
                this.label.label.color = this.color;
                this.subObjects.Add(this.label);
                OnChange();
            }
        }

        /// <summary>
        /// Lazier version of Label. Auto-calculates the size (assuming text is single line), and aligned to Left.
        /// <para>Use <seealso cref="OpLabel(Vector2, Vector2, string, FLabelAlignment, bool)"/> for more complicated setup.</para>
        /// </summary>
        /// <param name="posX">Leftmost Position</param>
        /// <param name="posY">Bottom Position</param>
        /// <param name="text">Max length is limited to 200 for this constructor.</param>
        /// <param name="bigText">Whether this uses bigFont or not</param>
        public OpLabel(float posX, float posY, string text = "TEXT", bool bigText = false)
            : this(new Vector2(posX, posY), new Vector2(100f, 20f), text, FLabelAlignment.Left, bigText)
        {
            this._text = text.Length < LabelTest.CharLimit(_bigText) / 3 ? text : text.Substring(0, LabelTest.CharLimit(_bigText) / 3);
            this._size = new Vector2((this._bigText ? 10f : 7f) * text.Length + 10f, this._bigText ? 30f : 20f);
            if (!_init) { return; }
            this.label.size = this._size;
            this.label.text = this._text;
            OnChange();
        }

        /// <summary>
        /// Vertical Alignment of the text inside the box defined with <see cref="UIelement.size"/>. Will not work correctly with overflowed text.
        /// See also <seealso cref="verticalAlignment"/>.
        /// </summary>
        public enum LabelVAlignment
        {
            /// <summary>
            /// The text fills from the top (<see cref="UIelement.pos"/>.y + <see cref="UIelement.size"/>.y)
            /// </summary>
            Top,

            /// <summary>
            /// The text is vertically aligned to the center
            /// </summary>
            Center,

            /// <summary>
            /// The text fills from the bottom (<see cref="UIelement.pos"/>.y)
            /// </summary>
            Bottom
        }

        /// <summary>
        /// Vertical Alignment of the text inside the box defined with <see cref="UIelement.size"/>. Will not work correctly with overflowed text.
        /// Changing this will call <see cref="OnChange"/> automatically.
        /// See also <seealso cref="LabelVAlignment"/>.
        /// </summary>
        public LabelVAlignment verticalAlignment
        {
            get { return _verticalAlignment; }
            set { if (_verticalAlignment != value) { _verticalAlignment = value; if (_init) { OnChange(); } } }
        }

        private LabelVAlignment _verticalAlignment;

        protected internal bool IsLong => this is OpLabelLong;

        /// <summary>
        /// Access MenuLabel. Be aware: when IsConfigScreen is false, accessing this will throw NullRefException.
        /// </summary>
        public MenuLabel label;

        protected internal readonly bool _bigText;

        /// <summary>
        /// (default : false) If this is true, OpLabel will automatically make text in MultiLine.
        /// </summary>
        public bool autoWrap;

        //private int lineLength;
        public FLabelAlignment alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                if (_alignment != value)
                {
                    _alignment = value;
                    OnChange();
                }
            }
        }

        protected internal FLabelAlignment _alignment;

        /// <summary>
        /// Colour of the text. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color color;

        /// <summary>
        /// Set this to <see cref="UIconfig.bumpBehav"/> or <see cref="UItrigger.bumpBehav"/> to make its colour reactive.
        /// <para>Example:
        /// <c>OpCheckBox chk = new OpCheckBox(posChk, "_");
        /// Tabs[0].AddItems(chk, new OpLabel(posLbl, sizLbl, "Text") { bumpBehav = chk.bumpBehav });</c>
        /// </para>
        /// </summary>
        /// <remarks>Alternatively, you can initialize this on its own to make <see cref="OpLabel"/> reactive to Mouse.</remarks>
        public BumpBehaviour bumpBehav;

        protected readonly bool isVFont;

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (!_init || isInactive) { return; }
            if (this.bumpBehav?.owner == this) { this.bumpBehav.Update(dt); }
            if (this.IsLong) { return; }
            if (this.bumpBehav == null) { this.label.label.color = this.color; }
            else { this.label.label.color = this.bumpBehav.GetColor(this.color); }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(this._size.x, 20f), Mathf.Max(this._size.y, 20f));
            base.OnChange();
            if (!_init) { return; }

            if (string.IsNullOrEmpty(this._text)) { this._displayText = ""; goto displaySkip; }
            if (!this.autoWrap)
            {
                if (this.IsLong) { this._displayText = _text; }
                else { this._displayText = _text.Length < LabelTest.CharLimit(_bigText, isVFont) ? _text : _text.Substring(0, LabelTest.CharLimit(_bigText, isVFont)); }
            }
            else
            {
                string ml;
                if (this.IsLong) { ml = _text; }
                else { ml = _text.Length < LabelTest.CharLimit(_bigText, isVFont) ? _text : _text.Substring(0, LabelTest.CharLimit(_bigText, isVFont)); }

                this._displayText = LabelTest.WrapText(ml, _bigText, _size.x, isVFont);
            }
        displaySkip:
            if (this.IsLong) { return; }
            if (this.GetLineCount() > 10)
            {
                int IndexOfOccurence(int occurence)
                {
                    int i = 1;
                    int index = 0;

                    while (i <= occurence && (index = _displayText.IndexOf('\n', index + 1)) != -1)
                    {
                        if (i == occurence)
                            return index;
                        i++;
                    }
                    return -1;
                }
                int sp = IndexOfOccurence(10);
                if (sp > 0) { _displayText = _displayText.Substring(0, sp); }
            }
            this.label.text = this._displayText;
            this.label.size = this.size;
            switch (this._alignment)
            {
                default:
                case FLabelAlignment.Center:
                    this.label.label.alignment = FLabelAlignment.Center;
                    this.label.pos = this.pos;
                    break;

                case FLabelAlignment.Left:
                    this.label.label.alignment = FLabelAlignment.Left;
                    this.label.pos = this.pos - new Vector2(this.size.x * 0.5f, 0f);
                    break;

                case FLabelAlignment.Right:
                    this.label.label.alignment = FLabelAlignment.Right;
                    this.label.pos = this.pos + new Vector2(this.size.x * 0.5f, 0f);
                    break;
            }

            float lh = LabelTest.LineHeight(_bigText, isVFont) * GetLineCount();
            switch (this._verticalAlignment)
            {
                default:
                case LabelVAlignment.Center:
                    break;

                case LabelVAlignment.Top:
                    this.label.size.y = lh;
                    this.label.pos.y = this.pos.y + this.size.y - lh;
                    break;

                case LabelVAlignment.Bottom:
                    this.label.size.y = lh;
                    this.label.pos.y = this.pos.y + lh;
                    break;
            }
        }

        /// <summary>
        /// text that will be displayed with this label. Do not edit <see cref="label"/> directly.
        /// See also <seealso cref="GetLineCount"/>
        /// </summary>
        public string text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnChange();
                }
            }
        }

        protected internal string _text;
        protected internal string _displayText;

        public int GetLineCount() => _displayText.Split('\n').Length;

        public override void Hide()
        {
            base.Hide();
            if (!this.IsLong) { this.label.label.isVisible = false; }
        }

        public override void Show()
        {
            base.Show();
            if (!this.IsLong) { this.label.label.isVisible = true; }
        }

        public override void Unload()
        {
            base.Unload();
        }
    }
}