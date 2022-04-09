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
            this._alignment = alignment;
            if (!this.IsLong)
            {
                this.label = new FLabel(LabelTest.GetFont(this._bigText, this.isVFont), _text)
                {
                    alignment = this._alignment,
                    color = this.color,
                    y = -10000f
                };
                this.myContainer.AddChild(this.label);
                Change();
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

            this.label.text = this._text;
            Change();
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
        /// Changing this will call <see cref="Change"/> automatically.
        /// See also <seealso cref="LabelVAlignment"/>.
        /// </summary>
        public LabelVAlignment verticalAlignment
        {
            get => _verticalAlignment;
            set { if (_verticalAlignment != value) { _verticalAlignment = value; Change(); } }
        }

        private LabelVAlignment _verticalAlignment;

        protected bool IsLong => this is OpLabelLong;

        /// <summary>
        /// Access <see cref="FLabel"/>
        /// </summary>
        public FLabel label;

        protected readonly bool _bigText;

        /// <summary>
        /// (default : false) If this is true, OpLabel will automatically make text in MultiLine.
        /// </summary>
        public bool autoWrap;

        //private int lineLength;
        public FLabelAlignment alignment
        {
            get => _alignment;
            set { if (_alignment != value) { _alignment = value; Change(); } }
        }

        protected FLabelAlignment _alignment;

        /// <summary>
        /// Colour of the text. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Set this to <see cref="UIfocusable.bumpBehav"/> to make its colour reactive.
        /// <para>Example:
        /// <c>OpCheckBox chk = new OpCheckBox(posChk, "_");
        /// Tabs[0].AddItems(chk, new OpLabel(posLbl, sizLbl, "Text") { bumpBehav = chk.bumpBehav });</c>
        /// </para>
        /// </summary>
        /// <remarks>Alternatively, you can initialize this on its own to make <see cref="OpLabel"/> reactive to Mouse.</remarks>
        public BumpBehaviour bumpBehav;

        protected readonly bool isVFont;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            if (this.IsLong) { return; }
            if (this.bumpBehav == null) { this.label.color = this.color; }
            else { this.label.color = this.bumpBehav.GetColor(this.color); }
        }

        protected internal override void Change()
        {
            this._size = new Vector2(Mathf.Max(this._size.x, 20f), Mathf.Max(this._size.y, 20f)); // Minimum size
            base.Change();
            if (this.bumpBehav?.owner == this) { this.bumpBehav.Update(); }

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

            switch (this._alignment)
            {
                default:
                case FLabelAlignment.Center:
                    this.label.alignment = FLabelAlignment.Center;
                    this.label.x = this.size.x / 2f;
                    break;

                case FLabelAlignment.Left:
                    this.label.alignment = FLabelAlignment.Left;
                    this.label.x = 0f;
                    break;

                case FLabelAlignment.Right:
                    this.label.alignment = FLabelAlignment.Right;
                    this.label.x = this.size.x;
                    break;
            }

            float lh = LabelTest.LineHeight(_bigText, isVFont) * GetLineCount();
            switch (this._verticalAlignment)
            {
                default:
                case LabelVAlignment.Center:
                    this.label.y = this.size.y / 2f;
                    break;

                case LabelVAlignment.Top:
                    this.label.y = this.size.y - lh / 2f;
                    break;

                case LabelVAlignment.Bottom:
                    this.label.y = lh / 2f;
                    break;
            }
        }

        /// <summary>
        /// text that will be displayed with this label. Do not edit <see cref="label"/> directly.
        /// See also <seealso cref="GetLineCount"/>
        /// </summary>
        public string text
        {
            get => _text;
            set { if (_text != value) { _text = value; Change(); } }
        }

        protected string _text;
        protected string _displayText;

        public int GetLineCount() => _displayText.Split('\n').Length;
    }
}