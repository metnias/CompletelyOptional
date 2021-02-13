using CompletelyOptional;
using Menu;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// static class for getting useful value regarding <see cref="MenuLabel"/> and <see cref="FLabel"/>
    /// </summary>
    public static class LabelTest
    {
        private static MenuLabel tester;
        private static MenuLabel testerB;

        /// <summary>
        /// Initializes this class; Do NOT call this on your own
        /// </summary>
        public static void Initialize(Menu.Menu menu)
        {
            tester = new MenuLabel(menu, menu.pages[0], "A", new Vector2(10000f, 10000f), new Vector2(10000f, 100f), false);
            tester.label.alpha = 0f; tester.label.RemoveFromContainer();
            testerB = new MenuLabel(menu, menu.pages[0], "B", new Vector2(10000f, 10500f), new Vector2(10000f, 300f), true);
            testerB.label.alpha = 0f; testerB.label.RemoveFromContainer();
            if (OptionScript.ComModExists)
            {
                float s = 1f / (int)OptionScript.ComMod.GetType().GetField("pMulti", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(OptionScript.ComMod);
                tester.label.scale = s;
                s = 1f / (int)OptionScript.ComMod.GetType().GetField("pdMulti", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(OptionScript.ComMod);
                testerB.label.scale = s;
            }

            if (!hasChecked)
            {
                hasChecked = true;
                _textHeight = tester.label.textRect.height; _textHeightB = testerB.label.textRect.height;
                tester.text = "A\nB"; testerB.text = "A\nB";
                _lineHeight = tester.label.textRect.height - _textHeight; _lineHeightB = testerB.label.textRect.height - _textHeightB;
                string meanTest = "ABCDEFGHIJKLMNOPQRSTUVWXYZ, abcdefghijklmnopqrstuvwxyz. abcdefghijklmnopqrstuvwxyz! abcdefghijklmnopqrstuvwxyz?";
                tester.text = meanTest; testerB.text = meanTest; //60473.68
                _charMean = tester.label.textRect.width / meanTest.Length;
                _charMeanB = testerB.label.textRect.width / meanTest.Length;
                _charLim = Mathf.FloorToInt(60000f / (_lineHeight * _charMean));
                _charLimB = Mathf.FloorToInt(60000f / (_lineHeightB * _charMeanB));
                _font = (string)typeof(FLabel).GetField("_fontName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tester.label);
                _fontB = (string)typeof(FLabel).GetField("_fontName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(testerB.label);
                Debug.Log($"CompletelyOptional) Label th: {_textHeight:0.0} thB: {_textHeightB:0.00} / lh: {_lineHeight:0.0} lhB: {_lineHeightB:0.0} / cm: {_charMean:0.0} cmB: {_charMeanB:0.0} / cl: {_charLim} clB: {_charLimB}");
            }
        }

        private static bool hasChecked = false;

        private static float _lineHeight = 15f, _lineHeightB = 30f;
        private static float _textHeight = 15f, _textHeightB = 30f;
        private static float _charMean = 6.4f, _charMeanB = 11.1f;
        private static int _charLim = 600, _charLimB = 175;
        private static string _font = "font", _fontB = "DisplayFont";

        /// <summary>
        /// LineHeight of current font
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Line Height in pxl</returns>
        public static float LineHeight(bool bigText) => !bigText ? _lineHeight : _lineHeightB;

        // public static float TextHeight(bool bigText) => !bigText ? _textHeight : _textHeightB;

        /// <summary>
        /// Mean pxl length for English characters with this font. Useful for optimizing autoWrap.
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Mean pxl length of single character for current font</returns>
        public static float CharMean(bool bigText) => !bigText ? _charMean : _charMeanB;

        /// <summary>
        /// Average text limit of singular <see cref="FLabel"/> that does not cause Futile Crash
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Absolute text length limit</returns>
        public static int CharLimit(bool bigText) => !bigText ? _charLim : _charLimB;

        /// <summary>
        /// Returns <see cref="FFont"/> fontName of <see cref="FLabel"/> for when you're directly using <see cref="FLabel"/> instead of <see cref="MenuLabel"/>.
        /// Use this instead of hard-coded font, to keep compatibility with Rain World 1.8
        /// </summary>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>fontName which can be used for <see cref="FLabel.FLabel(string, string, FTextParams)"/></returns>
        public static string GetFont(bool bigText) => !bigText ? _font : _fontB;

        /// <summary>
        /// Checks how long pixels would the text take.
        /// <para>This is very costy in performance, so do not use this too much. See also <see cref="CharMean(bool)"/></para>
        /// </summary>
        /// <param name="text">Text you want to test</param>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <returns>Text width in pxl</returns>
        public static float GetWidth(string text, bool bigText)
        {
            /*
            if (!hasChecked) { return CharMean(bigText) * text.Length; }
            MenuLabel l = !bigText ? tester : testerB;
            l.text = text;
            return l.label.textRect.width;*/

            FFont font = Futile.atlasManager.GetFontWithName(GetFont(bigText));
            float width = 0f;
            foreach (var item in font.GetQuadInfoForText(text, new FTextParams()))
            { width = Mathf.Max(width, item.bounds.width); }
            return width;
        }

        /// <summary>
        /// Text wrapper by <c>dual curly potato noodles</c>
        /// </summary>
        /// <param name="text">Text you want to wrap</param>
        /// <param name="bigText">Whether the font is big variant or not</param>
        /// <param name="width">Pixel width</param>
        /// <returns></returns>
        public static string WrapText(this string text, bool bigText, float width)
        {
            string ActualWrapText(string t)
            {
                StringBuilder r = new StringBuilder(t); // = t.Replace("\r\n", "\n")
                FFont font = Futile.atlasManager.GetFontWithName(GetFont(bigText));

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
                    void TrimWhitespace(int j)
                    {
                        int start = j;
                        while (j < t.Length && t[j] != '\n' && char.IsWhiteSpace(t[j]))
                        {
                            j++;
                        }
                        r.Remove(start, j - start);
                    }

                    var lineWidth = quads[i].rect.x + quads[i].rect.width - (quads[lastStartLineIndex].rect.x + quads[lastStartLineIndex].rect.width);

                    var curChar = r[i];
                    if (lineWidth > width)
                    {
                        lastStartLineIndex = lastWhitespaceIndex == -1 ? i + 1 : lastWhitespaceIndex + 1;
                        lastWhitespaceIndex = -1;

                        r.Insert(lastStartLineIndex, '\n');
                        quads.Insert(lastStartLineIndex + 1, quads[lastStartLineIndex]);

                        // Trim auto-linebreaks so there isn't extra space at the beginning of a line
                        TrimWhitespace(lastStartLineIndex);
                    }
                    else if (curChar == '\n')
                    {
                        lastWhitespaceIndex = -1;
                        lastStartLineIndex = i;
                    }
                    else if (char.IsWhiteSpace(curChar))
                    {
                        lastWhitespaceIndex = i;
                    }
                }
                return r.ToString();
            }

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                ret.Append(ActualWrapText(lines[i]));
                if (i < lines.Length - 1) { ret.Append('\n'); }
            }
            return ret.ToString();
        }
    }
}
