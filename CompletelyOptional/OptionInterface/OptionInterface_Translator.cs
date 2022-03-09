using CompletelyOptional;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    public partial class OptionInterface
    {
        /// <summary>
        /// File name of your mod's translation data in your mod's assembly, including '.txt'
        /// </summary>
        public string transFile;

        private string[] transData = null;

        private string curLang;
        private Dictionary<string, string> transConverter;

        /// <summary>
        /// Translate your string using embed txt in your mod
        /// </summary>
        /// <param name="orig">Original text</param>
        /// <returns>Translated text (returns original if translation isn't found.)</returns>
        public string Translate(string orig)
        {
            if (curLang != ModConfigMenu.curLang || transConverter == null)
            {
                // curLang = OptionScript.curLang;
                LoadTranslation();
            }

            if (transConverter.TryGetValue(orig, out string res))
            { return res.Replace("\\n", Environment.NewLine); }
            else { return orig.Replace("\\n", Environment.NewLine); }
        }

        /// <summary>
        /// Returns <see cref="CultureInfo"/> according to Rain World Language Settings.
        /// <seealso cref="GetLanguageID"/>
        /// </summary>
        /// <remarks>Example: <c>DateTime.ParseExact("20200123", "yyyyMMdd", null).ToString("D", GetCultureInfo());</c></remarks>
        public static CultureInfo GetCultureInfo() => InternalTranslator.GetCultureInfo();

        /// <summary>
        /// Returns first three letter of the language name in English. (e.g. Spanish -> spa)
        /// <seealso cref="GetCultureInfo"/>
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageID() => ModConfigMenu.curLang;

        private void LoadTranslation()
        {
            curLang = ModConfigMenu.curLang;
            transConverter = new Dictionary<string, string>();

            if (transData == null) { if (!ReadTransTXT()) { return; }; }
            for (int i = 0; i < transData.Length; i++)
            {
                if (transData[i].StartsWith("//") || transData[i].Length < 5) { continue; }
                string[] langs = transData[i].Split('|'); //Regex.Split(data[i], "/\|/");
                if (langs.Length < 2) { continue; }
                bool hasDefault = false;
                for (int j = 1; j < langs.Length; j++)
                {
                    string[] piece = langs[j].Split('$'); //Regex.Split(langs[j], @"$");
                    if (piece.Length < 2)
                    {
                        ComOptPlugin.LogError($"Specify Language for your translation in this format: \'lang$translation\'\n Allowed Language Codes are: {InternalTranslator.allowedCodes}"); continue;
                    }
                    if (curLang == InternalTranslator.LangToCode(piece[0]))
                    {
                        //string cvt = Regex.Replace(piece[1], "/\\en/i", Environment.NewLine);
                        if (transConverter.ContainsKey(langs[0]))
                        {
                            if (!hasDefault) { ComOptPlugin.LogError($"Conflicting Key found: \'{langs[0]}\'"); }
                            transConverter.Remove(langs[0]);
                        }
                        transConverter.Add(langs[0], piece[1]);
                        //ComOptPlugin.LogInfo($"{transConverter.Count}: {langs[0]}|{piece[1]}");
                    }
                    else if (InternalTranslator.LangToCode(piece[0]) == "eng")
                    {
                        if (transConverter.ContainsKey(langs[0])) { continue; }
                        transConverter.Add(langs[0], piece[1]); //add default translation
                        hasDefault = true;
                    }
                }
            }
        }

        private void CheckTestTranslation()
        {
            if (string.IsNullOrEmpty(transFile) || !transFile.Contains('.')) { return; }
            string[] temp = transFile.Split('.');
            string test = string.Concat(directory.FullName, temp[temp.Length - 2], ".txt");

            if (File.Exists(test))
            {
                ComOptPlugin.LogMessage($"{rwMod.ModID} reloaded external translation: {test}");
                string d = File.ReadAllText(test);
                if (d.Contains(Environment.NewLine)) { transData = Regex.Split(d, Environment.NewLine); }
                else { transData = Regex.Split(d, "\n"); }
                LoadTranslation();
            }
        }

        private bool ReadTransTXT()
        {
            if (string.IsNullOrEmpty(transFile)) { return false; }

            var assembly = Assembly.GetAssembly(rwMod.mod.GetType());
            string result;

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(transFile))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                ComOptPlugin.LogError($"Unable to find the translation txt named {transFile}");
                ComOptPlugin.LogError(e.ToString());
                ComOptPlugin.LogError("This is the list of resources in your mod's assembly. Pick one and set \'transFile\' in ctor!");

                string[] names = assembly.GetManifestResourceNames();
                foreach (string name in names) { ComOptPlugin.LogError(name); }
                return false;
            }

            if (result.Contains(Environment.NewLine)) { transData = Regex.Split(result, Environment.NewLine); }
            else { transData = Regex.Split(result, "\n"); }
            return true;
        }
    }
}