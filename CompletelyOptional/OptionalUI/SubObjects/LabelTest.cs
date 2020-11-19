using CompletelyOptional;
using Menu;
using System.Reflection;
using UnityEngine;

namespace OptionalUI
{
    public static class LabelTest
    {
        private static MenuLabel tester;
        private static MenuLabel testerB;

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
                Debug.Log($"CompletelyOptionl: Label th: {_textHeight:0.0} thB: {_textHeightB:0.00} / lh: {_lineHeight:0.0} lhB: {_lineHeightB:0.0} / cm: {_charMean:0.0} cmB: {_charMeanB:0.0} / cl: {_charLim} clB: {_charLimB}");
            }
        }

        private static bool hasChecked = false;

        private static float _lineHeight = 15f, _lineHeightB = 30f;
        private static float _textHeight = 15f, _textHeightB = 30f;
        private static float _charMean = 6.4f, _charMeanB = 11.1f;
        private static int _charLim = 600, _charLimB = 175;

        public static float LineHeight(bool bigText) => !bigText ? _lineHeight : _lineHeightB;

        public static float TextHeight(bool bigText) => !bigText ? _textHeight : _textHeightB;

        public static float CharMean(bool bigText) => !bigText ? _charMean : _charMeanB;

        public static int CharLimit(bool bigText) => !bigText ? _charLim : _charLimB;

        public static float GetWidth(string text, bool bigText)
        {
            if (!hasChecked) { return CharMean(bigText) * text.Length; }
            MenuLabel l = !bigText ? tester : testerB;
            l.text = text;
            return l.label.textRect.width;
        }

        //public static void
    }
}
