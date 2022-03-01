using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// ConfigMachine's internal translator.
    /// </summary>
    public static class InternalTranslator
    {
        private static string[] data = null;

        private static string curLang => ModConfigMenu.curLang;
        private static Dictionary<string, string> converter;
        public const string allowedCodes = "eng, fre, ita, ger, spa, por, (kor, jap, rus, chi)";

        private static Dictionary<string, string> Code2Culture;

        public static CultureInfo GetCultureInfo()
        {
            if (Code2Culture == null)
            {
                Code2Culture = new Dictionary<string, string>()
                {
                    { "eng", "en-US" },
                    { "fre", "fr-FR" },
                    { "ita", "it-IT" },
                    { "ger", "de-DE" },
                    { "spa", "es-ES" },
                    { "por", "pt-PT" },
                    { "jap", "ja-JP" },
                    { "kor", "ko-KR" },
                    { "rus", "ru-RU" },
                    { "chi", "zh-CN" }
                };
            }
            if (Code2Culture.TryGetValue(curLang, out string c)) { return CultureInfo.CreateSpecificCulture(c); }
            return CultureInfo.CreateSpecificCulture("en-US");
        }

        public static string LangToCode(string lang)
        {
            if (lang.Length < 3)
            {
                Debug.LogError($"CompletelyOptional: Your Language Code({lang}) is wrong! Use one of these: {allowedCodes}");
                return "xxx";
            }
            return lang.Substring(0, 3).ToLower();
        }

        internal static void LoadTranslation()
        {
            Debug.Log($"CompletelyOptional Loading Translation for {curLang}");
            if (data == null) { ReadTXT(); }
            converter = new Dictionary<string, string>();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].StartsWith("//") || data[i].Length < 5) { continue; }
                //Debug.Log(data[i]);
                string[] langs = data[i].Split('|'); //Regex.Split(data[i], "/\|/");
                //if (langs.Length < 2) { continue; }
                for (int j = 1; j < langs.Length; j++)
                {
                    string[] piece = langs[j].Split('$'); //Regex.Split(langs[j], @"$");
                    if (piece.Length < 2)
                    {
                        Debug.LogError(string.Concat("CompletelyOptional: Specify Language for your translation in this format: \'lang$translation\'",
                            Environment.NewLine, "Allowed Language Codes are: ", allowedCodes)); continue;
                    }
                    if (curLang == LangToCode(piece[0]))
                    {
                        //Debug.Log(string.Concat(langs[0], "|", piece[1]));
                        if (converter.ContainsKey(langs[0])) { converter.Remove(langs[0]); }
                        converter.Add(langs[0], piece[1]);
                    }
                    else if (LangToCode(piece[0]) == "eng")
                    {
                        if (converter.ContainsKey(langs[0])) { continue; }
                        converter.Add(langs[0], piece[1]); //add default translation
                    }
                }
            }
        }

        internal static string Translate(string orig)
        {
            if (converter.TryGetValue(orig, out string res))
            { return res.Replace("\\n", Environment.NewLine); }
            else { return orig.Replace("\\n", Environment.NewLine); }
        }

        private static void ReadTXT()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "CompletelyOptional.Translations.txt";
            string result;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            { result = reader.ReadToEnd(); }

            data = Regex.Split(result, Environment.NewLine);
        }
    }
}