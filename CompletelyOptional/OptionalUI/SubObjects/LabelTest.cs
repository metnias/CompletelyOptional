using Menu;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// static class for getting useful value regarding <see cref="FLabel"/>
    /// </summary>
    public static class LabelTest
    {
        private static MenuLabel tester;
        private static MenuLabel testerB;

        internal static void Initialize(Menu.Menu menu)
        {
            if (tester != null) { tester.RemoveSprites(); tester = null; }
            if (testerB != null) { testerB.RemoveSprites(); testerB = null; }
            tester = new MenuLabel(menu, menu.pages[0], "", new Vector2(10000f, 10000f), new Vector2(10000f, 100f), false);
            tester.label.alpha = 0f; tester.label.RemoveFromContainer();
            testerB = new MenuLabel(menu, menu.pages[0], "", new Vector2(10000f, 10500f), new Vector2(10000f, 300f), true);
            testerB.label.alpha = 0f; testerB.label.RemoveFromContainer();

            if (!hasChecked)
            {
                hasChecked = true;
                tester.text = "A"; testerB.text = "A";
                _textHeight = tester.label.textRect.height; _textHeightB = testerB.label.textRect.height;
                tester.text = "A\nB"; testerB.text = "A\nB";
                _lineHeight = tester.label.textRect.height - _textHeight; _lineHeightB = testerB.label.textRect.height - _textHeightB;
                //_textHeight *= s; _lineHeight *= s; _textHeightB *= sd; _lineHeightB *= sd;
                string meanTest = "ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?";
                tester.text = meanTest; testerB.text = meanTest; //60473.68
                _charMean = tester.label.textRect.width / meanTest.Length;
                _charMeanB = testerB.label.textRect.width / meanTest.Length;
                _charLim = Mathf.FloorToInt(60000f / (_lineHeight * _charMean));
                _charLimB = Mathf.FloorToInt(60000f / (_lineHeightB * _charMeanB));
                _font = (string)typeof(FLabel).GetField("_fontName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tester.label);
                _fontB = (string)typeof(FLabel).GetField("_fontName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(testerB.label);
                CompletelyOptional.ComOptPlugin.LogInfo($"LabelTest Initialized) th: {_textHeight:0.0} thB: {_textHeightB:0.00} / lh: {_lineHeight:0.0} lhB: {_lineHeightB:0.0} / cm: {_charMean:0.0} cmB: {_charMeanB:0.0} / cl: {_charLim} clB: {_charLimB}");
            }
        }

        private static bool hasChecked = false;

        private static float _lineHeight = 15f, _lineHeightB = 30f;
        private static float _textHeight = 15f, _textHeightB = 30f;
        private static float _charMean = 6.4f, _charMeanB = 11.1f;
        private static int _charLim = 600, _charLimB = 175;
        private static string _font = "font", _fontB = "DisplayFont";

        private const float _VlineHeight = 15f, _VlineHeightB = 30f;
        private const float _VtextHeight = 15f, _VtextHeightB = 30f;
        private const float _VcharMean = 6.4f, _VcharMeanB = 11.1f;
        private const int _VcharLim = 600, _VcharLimB = 175;
        private const string _Vfont = "font", _VfontB = "DisplayFont";

        /// <summary>
        /// Create <see cref="FLabel"/> with a font automatically selected, and centered.
        /// </summary>
        /// <param name="text">Initial text to be inserted</param>
        /// <param name="bigText">Whether to use big font or not</param>
        /// <returns></returns>
        public static FLabel CreateFLabel(string text, bool bigText = false) => OpLabel.CreateFLabel(text, bigText);

        /// <summary>
        /// LineHeight of current font
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Line Height in pxl</returns>
        public static float LineHeight(bool bigText, bool ascii = false) => !ascii ? (!bigText ? _lineHeight : _lineHeightB) : (!bigText ? _VlineHeight : _VlineHeightB);

        // public static float TextHeight(bool bigText) => !bigText ? _textHeight : _textHeightB;

        /// <summary>
        /// Mean pxl length for English characters with this font. Useful for optimizing autoWrap.
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Mean pxl length of single character for current font</returns>
        public static float CharMean(bool bigText, bool ascii = false) => !ascii ? (!bigText ? _charMean : _charMeanB) : (!bigText ? _VcharMean : _charMeanB);

        /// <summary>
        /// Average text limit of singular <see cref="FLabel"/> that does not cause Futile Crash
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Absolute text length limit</returns>
        public static int CharLimit(bool bigText, bool ascii = false) => !ascii ? (!bigText ? _charLim : _charLimB) : (!bigText ? _VcharLim : _VcharLimB);

        /// <summary>
        /// Returns <see cref="FFont"/> fontName of <see cref="FLabel"/> for when you're directly using <see cref="FLabel"/> instead of <see cref="MenuLabel"/>.
        /// Use this instead of hard-coded font, to keep compatibility with Rain World 1.8
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>fontName which can be used for <see cref="FLabel.FLabel(string, string, FTextParams)"/></returns>
        public static string GetFont(bool bigText, bool ascii = false) => !ascii ? (!bigText ? _font : _fontB) : (!bigText ? _Vfont : _VfontB);

        /// <summary>
        /// Returns whether the text has non ASCII characters or not
        /// </summary>
        public static bool HasNonASCIIChars(string text) => (System.Text.Encoding.UTF8.GetByteCount(text) != text.Length);

        /// <summary>
        /// Checks how long pixels would the text take.
        /// <para>This is very costy in performance, so do not use this too much. See also <see cref="CharMean"/></para>
        /// </summary>
        /// <param name="text">Text you want to test</param>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Text width in pxl</returns>
        public static float GetWidth(string text, bool bigText = false)
        {
            /*
            if (!hasChecked) { return CharMean(bigText) * text.Length; }
            MenuLabel l = !bigText ? tester : testerB;
            l.text = text;
            return l.label.textRect.width;*/

            FFont font = Futile.atlasManager.GetFontWithName(GetFont(bigText, !HasNonASCIIChars(text)));
            float width = 0f;
            foreach (var item in font.GetQuadInfoForText(text, new FTextParams()))
            { width = Mathf.Max(width, item.bounds.width); }
            return width;
        }

        /// <summary>
        /// Trim Text to width. This won't trim if the text is shorter than wanted width.
        /// </summary>
        /// <param name="text">Singlelined text you want to trim</param>
        /// <param name="width">Wanted width</param>
        /// <param name="addDots">Whether to add ... at the end of the text when trimmed</param>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns></returns>
        public static string TrimText(string text, float width, bool addDots = false, bool bigText = false)
        {
            float dotsWidth = 0f;
            if (addDots)
            { // Get dotsWidth
                dotsWidth = omit[(bigText ? 1 : 0) + (HasNonASCIIChars(text) ? 2 : 0)];
                if (dotsWidth < 0f)
                {
                    dotsWidth = !HasNonASCIIChars(text) ? GetWidth(omitDots, bigText) : (GetWidth("à" + omitDots, bigText) - GetWidth("à", bigText));
                    omit[(bigText ? 1 : 0) + (HasNonASCIIChars(text) ? 2 : 0)] = dotsWidth;
                }
            }
            float curWidth = GetWidth(text, bigText);
            if (curWidth < width) { return text; }

            // Trim
            if (addDots) { width -= dotsWidth; }
            if (curWidth > width * 2f)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (GetWidth(text.Substring(0, i), bigText) > width) { return text.Substring(0, i - 1) + (addDots ? omitDots : ""); }
                }
            }
            else
            {
                for (int i = text.Length - 1; i > 0; i--)
                { if (GetWidth(text.Substring(0, i), bigText) < width) { return text.Substring(0, i) + (addDots ? omitDots : ""); } }
            }
            return omitDots;
        }

        private const string omitDots = "...";
        private static readonly float[] omit = new float[] { -1f, -1f, -1f, -1f };

        /// <summary>
        /// Text wrapper by <c>dual curly potato noodles</c>
        /// </summary>
        /// <param name="text">Text you want to wrap</param>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <param name="width">Pixel width</param>
        /// <returns></returns>
        public static string WrapText(this string text, bool bigText, float width, bool ascii = false)
        {
            FFont font = Futile.atlasManager.GetFontWithName(GetFont(bigText, ascii));
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                ret.Append(WrapLine(lines[i]));
                if (i < lines.Length - 1) { ret.Append('\n'); }
            }
            return ret.ToString();

            string WrapLine(string t)
            {
                StringBuilder r = new StringBuilder(t);

                List<FLetterQuad> quads = new List<FLetterQuad>();
                foreach (var item in font.GetQuadInfoForText(t, new FTextParams()))
                {
                    quads.AddRange(item.quads);
                    quads.Add(new FLetterQuad() { charInfo = new FCharInfo() }); // Placeholders for newlines
                }

                int lastStartLineIndex = 0;
                int lastWhitespaceIndex = -1;
                for (int i = 0; i < t.Length; i++)
                {
                    void TrimWhitespace()
                    {
                        while (i < t.Length - 1 && t[i] != '\n' && char.IsWhiteSpace(t[i]))
                        {
                            i++;
                        }
                    }

                    var lineWidth = quads[i].rect.x + quads[i].rect.width - (quads[lastStartLineIndex].rect.x + quads[lastStartLineIndex].rect.width);

                    var curChar = r[i];
                    if (curChar == '\n')
                    {
                        TrimWhitespace(); // Trim line-ending whitespace
                        lastWhitespaceIndex = -1;
                        lastStartLineIndex = i;
                        continue;
                    }

                    if (lineWidth < 0.01f)
                    {
                        TrimWhitespace(); // Trim line-starting whitespace
                    }
                    else if (char.IsWhiteSpace(curChar))
                    {
                        lastWhitespaceIndex = i;
                    }

                    if (lineWidth > width)
                    {
                        if (lastWhitespaceIndex == -1)
                        {
                            r.Insert(i + 1, '\n');
                        }
                        else
                        {
                            r.Insert(lastWhitespaceIndex + 1, '\n');
                            i = lastWhitespaceIndex;
                        }
                        continue;
                    }
                }
                return r.ToString();
            }
        }
    }
}