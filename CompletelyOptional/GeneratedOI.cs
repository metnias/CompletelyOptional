using OptionalUI;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Partiality.Modloader;
using System.Collections.Generic;
using System;
using RWCustom;

namespace CompletelyOptional
{
    /// <summary>
    /// Procedurally generated OptionInterface
    /// </summary>
    public class GeneratedOI : OptionInterface
    {
        /// <summary>
        /// If you're using BepInEx.Configuration and not using LoadOI, this is called automatically.
        /// <para>Sections will be separated by OpTab.</para>
        /// </summary>
        public GeneratedOI(RainWorldMod mod, ConfigFile config) : base(mod)
        {
            mode = GenMode.BepInExConfig;
            bepConfigObj = config;
        }

        /// <summary>
        /// If you're using BepInEx.Configuration, GeneratedOI will be used automatically.
        /// This is for using default template, for when you want to use other function of OptionInterface(e.g. <see cref="OptionInterface.Translate(string)"/> but not wanting to set up Config Screen.
        /// </summary>
        /// <param name="plugin">BaseUnityPlugin instance</param>
        /// <param name="description">Description for your mod</param>
        public GeneratedOI(BaseUnityPlugin plugin, string description = "") : base(plugin)
        { mode = GenMode.ModderCall; modDescription = description; }

        /// <summary>
        /// This is for using default template, for when you want to use other function of OptionInterface(e.g. <see cref="OptionInterface.Translate(string)"/> but not wanting to set up Config Screen.
        /// </summary>
        /// <param name="mod">PartialityMod instance</param>
        /// <param name="description">Description for your mod</param>
        public GeneratedOI(PartialityMod mod, string description = "") : base(mod)
        { mode = GenMode.ModderCall; modDescription = description; }

        /// <summary>
        /// Keeps track on which ctor this OI used
        /// </summary>
        public readonly GenMode mode;
        /// <summary>
        /// BaseUnityPlugin.Config
        /// </summary>
        public ConfigFile bepConfig => bepConfigObj as ConfigFile;
        protected readonly object bepConfigObj;

        public string modDescription;

        /// <summary>
        /// Flags for which ctor this OI used
        /// </summary>
        public enum GenMode
        {
            BepInExConfig,
            ModderCall
        }

        public override void Initialize()
        {
            base.Initialize();

            if (mode == GenMode.BepInExConfig)
            {
                ICollection<ConfigDefinition> keys = bepConfig.Keys;
                List<string> sections = new List<string>();
                foreach (ConfigDefinition k in keys)
                {
                    // Debug.Log($"{rwMod.ModID}) {k.Section}: {k.Key}");
                    if (!sections.Contains(k.Section)) { sections.Add(k.Section); }
                }
                sections.Sort();

                bool hasUnsupported = false, hasFirstScroll = false;
                OpScrollBox firstScroll = null;
                Tabs = new OpTab[Mathf.Min(20, sections.Count)];
                for (int t = 0; t < Tabs.Length; t++)
                {
                    Tabs[t] = new OpTab(sections[t]);
                    float h = t != 0 ? 50f : 150f;
                    List<ConfigDefinition> cds = new List<ConfigDefinition>();
                    foreach (ConfigDefinition k in keys) { if (k.Section == sections[t]) { cds.Add(k); } }
                    cds.Sort(CompareCDkey);
                    List<UIelement> elms = new List<UIelement>();
                    for (int e = 0; e < cds.Count; e++)
                    {
                        if (TryGetBase(bepConfig, cds[e], out ConfigEntryBase entryBase))
                        {
                            string desc = entryBase.Description.Description; // LoremIpsum.Generate(3, 4);
                            switch (entryBase.SettingType.Name.ToLower())
                            {
                                case "bool": // OpCheckBox
                                case "boolean":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<bool> eBool))
                                    {
                                        elms.Add(new OpCheckBox(new Vector2(30f, 600f - h - 40f), GenerateKey(cds[e]), (bool)eBool.DefaultValue)
                                        { description = GetFirstSentence(desc) });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(70f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(80f, 600f - h - 80f), new Vector2(500f, 45f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "byte": //OpSliderSubtle
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<byte> eByte))
                                    {
                                        elms.Add(new OpSliderSubtle(new Vector2(30f, 600f - h - 45f), GenerateKey(cds[e]), new IntVector2(0, 20), 240, false, Mathf.Clamp((byte)eByte.DefaultValue, 0, 20))
                                        { description = GetFirstSentence(desc) });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(80f, 600f - h - 90f), new Vector2(500f, 45f), desc)); }
                                        h += 90f;
                                    }
                                    else { continue; }
                                    break;
                                case "uint": //OpSlider
                                case "uint32":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<uint> eUint))
                                    {
                                        elms.Add(new OpSlider(new Vector2(30f, 600f - h - 45f), GenerateKey(cds[e]), new IntVector2(0, 100), 400, false, Mathf.Clamp(Convert.ToInt32((uint)eUint.DefaultValue), 0, 100))
                                        { description = GetFirstSentence(desc) });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(80f, 600f - h - 90f), new Vector2(500f, 45f), desc)); }
                                        h += 90f;
                                    }
                                    else { continue; }
                                    break;
                                case "int": //OpTextBox
                                case "int32":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<int> eInt))
                                    {
                                        elms.Add(new OpTextBox(new Vector2(30f, 600f - h - 45f), 110f, GenerateKey(cds[e]), ((int)eInt.DefaultValue).ToString(), OpTextBox.Accept.Int)
                                        { description = GetFirstSentence(desc), allowSpace = true });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(160f, 600f - h - 60f), new Vector2(420f, 45f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "float": //OpTextBox
                                case "single":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<float> eFloat))
                                    {
                                        elms.Add(new OpTextBox(new Vector2(30f, 600f - h - 45f), 110f, GenerateKey(cds[e]), ((float)eFloat.DefaultValue).ToString(), OpTextBox.Accept.Float)
                                        { description = GetFirstSentence(desc), allowSpace = true });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(160f, 600f - h - 60f), new Vector2(420f, 45f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "string": //OpTextBox or OpColorPicker
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<string> eString))
                                    {
                                        string defaultString = (string)eString.DefaultValue;
                                        if (OpColorPicker.IsStringHexColor(defaultString))
                                        { //OpColorPicker
                                            elms.Add(new OpColorPicker(new Vector2(30f, 600f - h - 170f), GenerateKey(cds[e]), defaultString));
                                            elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                            { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                            if (!string.IsNullOrEmpty(desc))
                                            { elms.Add(new OpLabelLong(new Vector2(200f, 600f - h - 170f), new Vector2(380f, 135f), desc)); }
                                            h += 170f;
                                        }
                                        else
                                        {
                                            elms.Add(new OpTextBox(new Vector2(30f, 600f - h - 45f), 110f, GenerateKey(cds[e]), defaultString, OpTextBox.Accept.StringASCII)
                                            { description = GetFirstSentence(desc) });
                                            elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                            { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                            if (!string.IsNullOrEmpty(desc))
                                            { elms.Add(new OpLabelLong(new Vector2(160f, 600f - h - 60f), new Vector2(420f, 45f), desc)); }
                                            h += 60f;
                                        }
                                    }
                                    else { continue; }
                                    break;
                                case "keycode": //OpKeyBinder
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<KeyCode> eKeyCode))
                                    {
                                        elms.Add(new OpKeyBinder(new Vector2(30f, 600f - h - 50f), new Vector2(150f, 30f), rwMod.ModID, GenerateKey(cds[e]), ((KeyCode)eKeyCode.DefaultValue).ToString(), false));
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(200f, 600f - h - 90f), new Vector2(380f, 75f), desc)); }
                                        h += 100f;
                                    }
                                    else { continue; }
                                    break;
                                default:
                                    // if type is enum => OpComboBox
                                    if (entryBase.SettingType.IsEnum)
                                    {
                                        elms.Add(new OpResourceSelector(new Vector2(30f, 600f - h - 45f), 120f, GenerateKey(cds[e]), entryBase.SettingType, (string)entryBase.DefaultValue));
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = GetFirstSentence(desc), bumpBehav = (elms[elms.Count - 1] as UIconfig).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(160f, 600f - h - 60f), new Vector2(420f, 45f), desc)); }
                                        h += 60f;
                                        break;
                                    }
                                    Debug.Log($"{rwMod.ModID} has unsupported ConfigEntry: {cds[e].Key}({entryBase.SettingType.Name})");
                                    hasUnsupported = true; continue; // Not supported
                            }
                            h += 20f; // between gap
                        }
                    }
                    if (h <= 600f)
                    {
                        if (t == 0) { AddBasicProfile(Tabs[0], rwMod); hasFirstScroll = false; }
                        Tabs[t].AddItems(elms.ToArray());
                    }
                    else
                    {
                        OpScrollBox box = new OpScrollBox(Tabs[t], h);
                        if (t == 0) { AddBasicProfile(box, rwMod); hasFirstScroll = true; firstScroll = box; }
                        foreach (UIelement elm in elms)
                        { elm.pos = new Vector2(elm.GetPos().x, elm.GetPos().y - 600f + h); }
                        box.AddItems(elms.ToArray());
                    }
                }
                if (hasUnsupported)
                {
                    string warn = InternalTranslator.Translate("This Plugin contains types of settings that are not supported by Config Machine:") + Environment.NewLine
                        + InternalTranslator.Translate("Go to [BepInEx]-[config] folder and use Notepad to edit those settings.");
                    if (hasFirstScroll)
                    {
                        firstScroll.AddItems(new OpLabel(new Vector2(50f, firstScroll.contentSize - 600f + 525f), new Vector2(500f, 30f), warn));
                    }
                    else
                    {
                        Tabs[0].AddItems(new OpLabel(new Vector2(50f, 525f), new Vector2(500f, 20f), warn));
                    }
                }
            }
            else
            {
                Tabs = new OpTab[1];
                Tabs[0] = new OpTab();
                AddBasicProfile(Tabs[0], rwMod);
                if (!string.IsNullOrEmpty(modDescription))
                { Tabs[0].AddItems(new OpLabelLong(new Vector2(50f, 200f), new Vector2(500f, 250f), modDescription, alignment: FLabelAlignment.Center)); }
            }
        }

        private string GenerateKey(ConfigDefinition def) => $"{rwMod.ModID}_{def.Section}_{def.Key}";

        private static bool TryGetBase(ConfigFile file, ConfigDefinition key, out ConfigEntryBase entryBase)
        {
            try { entryBase = file[key]; return true; }
            catch (Exception) { entryBase = null; }
            return false;
        }

        private static string GetFirstSentence(string text)
        { // Used to cut off long description that would ruin bottom text
            if (!text.Contains(".")) { return text; }
            return text.Split('.')[0];
        }

        private static int CompareCDkey(ConfigDefinition x, ConfigDefinition y)
        { return x.Key.CompareTo(y.Key); }

        /// <summary>
        /// Saves Config to BepInEx's cfg format. One of the bridges between Config Machine and BepInEx.Config.
        /// </summary>
        public bool SaveBepConfig(Dictionary<string, string> newConfig)
        {
            foreach (ConfigDefinition def in bepConfig.Keys)
            {
                if (newConfig.TryGetValue(GenerateKey(def), out string val))
                {
                    if (TryGetBase(bepConfig, def, out ConfigEntryBase entBase))
                    {
                        switch (entBase.SettingType.Name.ToLower())
                        {
                            case "bool":
                            case "boolean":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<bool> eBool))
                                { eBool.Value = val == "true" ? true : false; }
                                break;
                            case "byte":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<byte> eByte))
                                { eByte.Value = byte.Parse(val); }
                                break;
                            case "uint":
                            case "uint32":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<uint> eUint))
                                { eUint.Value = uint.Parse(val); }
                                break;
                            case "int":
                            case "int32":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<int> eInt))
                                { eInt.Value = int.Parse(val); }
                                break;
                            case "float":
                            case "single":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<float> eFloat))
                                { eFloat.Value = float.Parse(val); }
                                break;
                            case "string":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<string> eString))
                                { eString.Value = val; }
                                break;
                            case "keycode":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<KeyCode> eKeyCode))
                                { eKeyCode.Value = (KeyCode)Enum.Parse(typeof(KeyCode), val); }
                                break;
                            default:
                                if (entBase.SettingType.IsEnum)
                                { entBase.SetSerializedValue(val); break; }
                                continue;
                        }
                    }
                }
            }
            bepConfig.Save();
            return true;
        }

        /// <summary>
        /// Reloads Config from BepInEx's cfg format. One of the bridges between Config Machine and BepInEx.Config
        /// </summary>
        public bool LoadBepConfig()
        {
            bepConfig.Reload();
            return true;
        }

        /// <summary>
        /// Displays Config from BepInEx to UIconfigs. One of the bridges between Config Machine and BepInEx.Config
        /// </summary>
        public void ShowBepConfig()
        {
            bool m = ConfigMenu.mute;
            ConfigMenu.mute = true;
            foreach (ConfigDefinition def in bepConfig.Keys)
            {
                if (objectDictionary.TryGetValue(GenerateKey(def), out UIconfig obj))
                {
                    if (TryGetBase(bepConfig, def, out ConfigEntryBase entBase))
                    {
                        switch (entBase.SettingType.Name.ToLower())
                        {
                            case "bool":
                            case "boolean":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<bool> eBool))
                                { obj.value = eBool.Value ? "true" : "false"; }
                                break;
                            case "byte":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<byte> eByte))
                                { obj.value = eByte.Value.ToString(); }
                                break;
                            case "uint":
                            case "uint32":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<uint> eUint))
                                { obj.value = eUint.Value.ToString(); }
                                break;
                            case "int":
                            case "int32":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<int> eInt))
                                { obj.value = eInt.Value.ToString(); }
                                break;
                            case "float":
                            case "single":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<float> eFloat))
                                { obj.value = eFloat.Value.ToString(); }
                                break;
                            case "string":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<string> eString))
                                { obj.value = eString.Value; }
                                break;
                            case "keycode":
                                if (bepConfig.TryGetEntry(def, out ConfigEntry<KeyCode> eKeyCode))
                                { obj.value = eKeyCode.Value.ToString(); }
                                break;
                            default:
                                if (entBase.SettingType.IsEnum)
                                { obj.value = entBase.GetSerializedValue(); break; }
                                continue;
                        }
                    }
                }
            }
            ConfigMenu.mute = m;
        }

        /// <summary>
        /// Adds Basic Profile on top of OpTab
        /// </summary>
        /// <param name="tab"><see cref="OpTab"/> which will contain the profile</param>
        /// <param name="mod"><see cref="RainWorldMod"/> that has basic information of the mod</param>
        public static void AddBasicProfile(OpTab tab, RainWorldMod mod)
        {
            tab.AddItems(new OpLabel(new Vector2(100f, 550f), new Vector2(400f, 50f), mod.ModID, FLabelAlignment.Center, true));
            tab.AddItems(new OpLabel(new Vector2(50f, 500f), new Vector2(100f, 20f), InternalTranslator.Translate("Version: <ModVersion>").Replace("<ModVersion>", mod.Version), FLabelAlignment.Left));

            if (mod.author != RainWorldMod.authorNull)
            {
                tab.AddItems(new OpLabel(new Vector2(350f, 500f), new Vector2(200f, 20f), InternalTranslator.Translate("Author: <ModAuthor>").Replace("<ModAuthor>", mod.author), FLabelAlignment.Right) { autoWrap = true });
            }

            /*
            if (mod.coauthor != RainWorldMod.authorNull)
            {
                labelCoauthor = new OpLabel(new Vector2(100f, 420f), new Vector2(300f, 20f), string.Concat("Coautor: ", mod.coauthor));
                Tabs[0].AddItem(labelCoauthor);
                labelCoauthor.autoWrap = true;
            }
            if(mod.description != RainWorldMod.authorNull)
            {
                labelDesc = new OpLabel(new Vector2(80f, 350f), new Vector2(340f, 20f), mod.description, FLabelAlignment.Left);
                Tabs[0].AddItem(labelDesc);
                labelDesc.autoWrap = true;
            } */
        }

        /// <summary>
        /// Adds Basic Profile on top of <see cref="OpScrollBox"/>
        /// </summary>
        /// <param name="box"><see cref="OpScrollBox"/> which will contain the profile</param>
        /// <param name="mod"><see cref="RainWorldMod"/> that has basic information of the mod</param>
        public static void AddBasicProfile(OpScrollBox box, RainWorldMod mod)
        {
            box.AddItems(new OpLabel(new Vector2(100f, box.contentSize - 600f + 550f), new Vector2(400f, 50f), mod.ModID, FLabelAlignment.Center, true));
            box.AddItems(new OpLabel(new Vector2(50f, box.contentSize - 600f + 500f), new Vector2(100f, 20f), InternalTranslator.Translate("Version: <ModVersion>").Replace("<ModVersion>", mod.Version), FLabelAlignment.Left));

            if (mod.author != RainWorldMod.authorNull)
            {
                box.AddItems(new OpLabel(new Vector2(350f, box.contentSize - 600f + 500f), new Vector2(200f, 20f), InternalTranslator.Translate("Author: <ModAuthor>").Replace("<ModAuthor>", mod.author), FLabelAlignment.Right) { autoWrap = true });
            }
        }
    }
}
