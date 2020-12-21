using OptionalUI;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Partiality.Modloader;
using System.Collections.Generic;
using System;

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
            bepConfig = config;
        }

        /// <summary>
        /// If you're using BepInEx.Configuration, GeneratedOI will be used automatically.
        /// This is for more precise control over GeneratedOI.
        /// </summary>
        /// <param name="plugin"></param>
        public GeneratedOI(BaseUnityPlugin plugin) : base(plugin)
        { mode = GenMode.ModderCall; }

        /// <summary>
        /// PartialityMod version of GeneratedOI for lazy modders.
        /// </summary>
        /// <param name="mod"></param>
        public GeneratedOI(PartialityMod mod) : base(mod)
        { mode = GenMode.ModderCall; }

        /// <summary>
        ///
        /// </summary>
        public readonly GenMode mode;
        public readonly ConfigFile bepConfig;

        /// <summary>
        ///
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
                            string desc = entryBase.Description.Description;
                            switch (entryBase.SettingType.Name.ToLower())
                            {
                                case "bool": // OpCheckBox
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<bool> eBool))
                                    {
                                        elms.Add(new OpCheckBox(new Vector2(30f, 600f - h - 30f), GenerateKey(cds[e]), (bool)eBool.DefaultValue)
                                        { description = desc });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h - 15f), new Vector2(70f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = desc, bumpBehav = (elms[elms.Count - 1] as OpCheckBox).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(100f, 600f - h - 60f), new Vector2(480f, 60f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "byte":
                                case "uint":
                                case "int":
                                    break;
                                case "float":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<float> eFloat))
                                    {
                                        elms.Add(new OpTextBox(new Vector2(30f, 600f - h - 30f), 110f, GenerateKey(cds[e]), ((float)eFloat.DefaultValue).ToString(), OpTextBox.Accept.Float)
                                        { description = desc });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h), new Vector2(110f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = desc, bumpBehav = (elms[elms.Count - 1] as OpTextBox).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(150f, 600f - h - 60f), new Vector2(430f, 60f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "string":
                                    if (bepConfig.TryGetEntry(cds[e], out ConfigEntry<string> eString))
                                    {
                                        elms.Add(new OpTextBox(new Vector2(30f, 600f - h - 30f), 110f, GenerateKey(cds[e]), (string)eString.DefaultValue, OpTextBox.Accept.StringASCII)
                                        { description = desc });
                                        elms.Add(new OpLabel(new Vector2(20f, 600f - h), new Vector2(120f, 15f), cds[e].Key)
                                        { alignment = FLabelAlignment.Left, description = desc, bumpBehav = (elms[elms.Count - 1] as OpTextBox).bumpBehav });
                                        if (!string.IsNullOrEmpty(desc))
                                        { elms.Add(new OpLabelLong(new Vector2(160f, 600f - h - 60f), new Vector2(420f, 60f), desc)); }
                                        h += 60f;
                                    }
                                    else { continue; }
                                    break;
                                case "keycode":
                                case "color":
                                    break;
                                default: hasUnsupported = true; continue; // Not supported
                            }
                            h += 20f; // between gap
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
                        string warn = InternalTranslator.Translate("This plugin contains types of settings that are not supported by Config Machine:") + Environment.NewLine
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
            }
            else
            {
                Tabs = new OpTab[1];
                Tabs[0] = new OpTab();
                AddBasicProfile(Tabs[0], rwMod);
            }
        }

        private string GenerateKey(ConfigDefinition def) => $"{rwMod.ModID}_{def.Section}_{def.Key}";

        private static bool TryGetBase(ConfigFile file, ConfigDefinition key, out ConfigEntryBase entryBase)
        {
            try { entryBase = file[key]; return true; }
            catch (Exception) { entryBase = null; }
            return false;
        }

        private static int CompareCDkey(ConfigDefinition x, ConfigDefinition y)
        { return x.Key.CompareTo(y.Key); }

        public bool SaveBepConfig(Dictionary<string, string> newConfig)
        {
            bepConfig.Save();
            return true;
        }

        public bool LoadBepConfig()
        {
            bepConfig.Reload();
            return true;
        }

        public void ShowBepConfig()
        {
            foreach (ConfigDefinition def in bepConfig.Keys)
            {
                if (objectDictionary.TryGetValue(GenerateKey(def), out UIconfig obj))
                {
                    if (TryGetBase(bepConfig, def, out ConfigEntryBase entBase))
                    {
                        obj.value = (string)entBase.BoxedValue;
                    }
                }
            }
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
        /// Adds Basic Profile on top of OpScrollBox
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
    }
}
