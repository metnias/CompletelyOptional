using OptionalUI;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Default OI that's called when your mod does not support CompletelyOptional.
    /// Also shows the error in your OI.
    /// </summary>
    public class UnconfiguableOI : OptionInterface
    {
        public UnconfiguableOI(PartialityMod mod, Reason type) : base(mod)
        {
            this.reason = type;
        }

        public UnconfiguableOI(PartialityMod mod, Exception exception) : base(mod)
        {
            Debug.LogError(string.Concat("CompletelyOptional: ", this.mod.ModID, " had issue in OptionInterface:"));
            this.reason = Reason.InitError;
            this.exception = exception.ToString();
            Debug.LogException(exception);
        }

        public Reason reason;
        public string exception;

        /// <summary>
        /// Set the list of ignored mod
        /// </summary>
        public void SetIgnoredModList(List<PartialityMod> mods)
        {
            mods.Sort(CompareModID);
            this.ignoredMods = mods.ToArray();
        }

        private PartialityMod[] ignoredMods;
        private PartialityMod[] configuableMods;

        public void SetConfiguableModList(List<OptionInterface>[] ois)
        {
            if (ois is null) { configuableMods = new PartialityMod[0]; return; }
            List<PartialityMod> mods = new List<PartialityMod>();
            for (int p = 3; p >= 1; p--)
            {
                for (int i = 0; i < ois[p].Count; i++)
                { mods.Add(ois[p][i].mod); }
            }
            //mods.Sort(CompareModID);
            configuableMods = mods.ToArray();
        }

        /// <summary>
        /// Comparator for Sorting OptionInterfaces by ModID
        /// </summary>
        private static int CompareModID(PartialityMod x, PartialityMod y)
        { return x.ModID.CompareTo(y.ModID); }

        public enum Reason
        {
            NoInterface,
            InitError,
            NoMod,
            TooManyMod
        }

        public override bool Configuable()
        {
            return false;
        }

        public override bool SaveData()
        {
            return false;
        }

        public override void LoadData()
        {
            return;
        }

#pragma warning disable CA1822 // Mark members as static

        public new bool LoadConfig() => true;

#pragma warning disable IDE0060

        public new bool SaveConfig(Dictionary<string, string> newConfig) => true;

#pragma warning restore IDE0060
#pragma warning restore CA1822

        public OpRect testRect;

        public override void Initialize()
        {
            if (this.reason == Reason.TooManyMod)
            {
                ListMods();
                return;
            }

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab();

            if (this.reason == Reason.NoMod)
            {
                TutoInit();
                return;
            }

            //Futile.atlasManager.LogAllElementNames();

            labelID = new OpLabel(new Vector2(100f, 550f), new Vector2(400f, 50f), mod.ModID, FLabelAlignment.Center, true);
            labelVersion = new OpLabel(new Vector2(50f, 500f), new Vector2(100f, 20f), InternalTranslator.Translate("Version: <ModVersion>").Replace("<ModVersion>", mod.Version), FLabelAlignment.Left);
            Tabs[0].AddItems(labelID, labelVersion);
            if (mod.author != "NULL")
            {
                labelAuthor = new OpLabel(new Vector2(350f, 500f), new Vector2(200f, 20f), InternalTranslator.Translate("Author: <ModAuthor>").Replace("<ModAuthor>", mod.author), FLabelAlignment.Right);
                Tabs[0].AddItems(labelAuthor);
                labelAuthor.autoWrap = true;
            }

            /*
            if (mod.coauthor != "NULL")
            {
                labelCoauthor = new OpLabel(new Vector2(100f, 420f), new Vector2(300f, 20f), string.Concat("Coautor: ", mod.coauthor));
                Tabs[0].AddItem(labelCoauthor);
                labelCoauthor.autoWrap = true;
            }
            if(mod.description != "NULL")
            {
                labelDesc = new OpLabel(new Vector2(80f, 350f), new Vector2(340f, 20f), mod.description, FLabelAlignment.Left);
                Tabs[0].AddItem(labelDesc);
                labelDesc.autoWrap = true;
            }*/

            switch (this.reason)
            {
                case Reason.NoInterface:
                    labelSluggo0 = new OpLabel(new Vector2(100f, 350f), new Vector2(400f, 20f), InternalTranslator.Translate("This Partiality Mod cannot be configured."));
                    Tabs[0].AddItems(labelSluggo0);

                    break;

                case Reason.InitError:
                    blue = new OpRect(new Vector2(30f, 20f), new Vector2(540f, 420f)) { alpha = 0.7f, colorFill = new Color(0.121568627f, 0.40392156862f, 0.69411764705f, 1f) };

                    Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);
                    oof = new OpLabel(new Vector2(100f, 380f), new Vector2(30f, 40f), ":(", FLabelAlignment.Left, true) { color = white };
                    labelSluggo0 = new OpLabel(new Vector2(150f, 390f), new Vector2(300f, 20f), InternalTranslator.Translate("There was an issue initializing OptionInterface.")) { color = white };
                    labelSluggo1 = new OpLabelLong(new Vector2(50f, 40f), new Vector2(500f, 320f), exception) { color = white, allowOverflow = false };

                    Tabs[0].AddItems(blue, oof, labelSluggo0, labelSluggo1);

                    labelVersion.text += string.Concat(Environment.NewLine, "Config Machine ", MenuTab.GetCMVersion());
                    break;
            }
        }

        private void TutoInit()
        {
            labelID = new OpLabel(new Vector2(100f, 500f), new Vector2(400f, 50f), InternalTranslator.Translate("No Mod can be configured"), FLabelAlignment.Center, true);

            labelVersion = new OpLabel(new Vector2(100f, 440f), new Vector2(200f, 20f), "Config Machine (a.k.a. CompletelyOptional) by topicular", FLabelAlignment.Left);
            labelAuthor = new OpLabel(new Vector2(300f, 410f), new Vector2(200f, 20f), InternalTranslator.Translate("also shoutout to RW Discord for helping me out"), FLabelAlignment.Right);

            labelSluggo0 = new OpLabel(new Vector2(100f, 300f), new Vector2(400f, 20f), InternalTranslator.Translate("check out pinned tutorial in modding-support"));
            labelSluggo1 = new OpLabel(new Vector2(100f, 260f), new Vector2(400f, 20f), InternalTranslator.Translate("and create your own config screen!"));
            Tabs[0].AddItems(labelID, labelVersion, labelAuthor, labelSluggo0, labelSluggo1);
        }

        private void ListMods()
        {
            const int modInPage = 20;
            int cListNum = Mathf.CeilToInt(configuableMods.Length / (float)modInPage);
            int iListNum = Mathf.CeilToInt(ignoredMods.Length / (float)modInPage);

            string modNumDecimal = "D" + (configuableMods.Length + ignoredMods.Length).ToString().Length.ToString();
            Tabs = new OpTab[cListNum + iListNum];
            for (int t = 0; t < cListNum; t++)
            {
                string name = InternalTranslator.Translate("Configurable Mods") + " " + ((t * modInPage) + 1).ToString(modNumDecimal) + " ~ "
                    + (Math.Min((t + 1) * modInPage, configuableMods.Length)).ToString(modNumDecimal);
                Tabs[t] = new OpTab(name);
                Tabs[t].AddItems(new OpLabel(new Vector2(100f, 500f), new Vector2(400f, 50f), InternalTranslator.Translate("Configurable Mod List"), FLabelAlignment.Center, true));
            }
            for (int t = cListNum; t < cListNum + iListNum; t++)
            {
                string name = InternalTranslator.Translate("Other Mods") + " " + (1 + configuableMods.Length + ((t - cListNum) * modInPage)).ToString(modNumDecimal) + " ~ "
                    + (configuableMods.Length + Math.Min((t - cListNum + 1) * modInPage, ignoredMods.Length)).ToString(modNumDecimal);
                Tabs[t] = new OpTab(name);
                Tabs[t].AddItems(new OpLabel(new Vector2(100f, 500f), new Vector2(400f, 50f), InternalTranslator.Translate("Other Mod List"), FLabelAlignment.Center, true));
            }

            for (int i = 0; i < configuableMods.Length; i++)
            {
                string name = configuableMods[i].ModID + " (" + configuableMods[i].Version + ")";
                string ath = "";
                if (!string.IsNullOrEmpty(configuableMods[i].author) && configuableMods[i].author != "NULL") { ath = configuableMods[i].author; }
                Vector2 pos = default;
                pos.x = i % 2 == 0 ? 40f : 320f;
                pos.y = 405f - Mathf.FloorToInt((i % modInPage) / 2f) * 40f;
                string desc = string.IsNullOrEmpty(ath) ? InternalTranslator.Translate("Configure <ModID>")
                    : InternalTranslator.Translate("Configure <ModID> by <ModAuthor>").Replace("<ModAuthor>", ath);
                Tabs[Mathf.FloorToInt(i / (float)modInPage)].AddItems(new OpSimpleButton(pos, new Vector2(240f, 30f), configuableMods[i].ModID, name)
                { description = desc.Replace("<ModID>", configuableMods[i].ModID) });
            }

            for (int i = 0; i < ignoredMods.Length; i++)
            {
                string name = ignoredMods[i].ModID + " (" + ignoredMods[i].Version + ")";
                string desc = " ";
                if (!string.IsNullOrEmpty(ignoredMods[i].author) && ignoredMods[i].author != "NULL")
                { desc = InternalTranslator.Translate("<ModID> by <ModAuthor>").Replace("<ModAuthor>", ignoredMods[i].author).Replace("<ModID>", ignoredMods[i].ModID); }
                Vector2 pos = default;
                pos.x = i % 2 == 0 ? 50f : 330f;
                pos.y = 410f - Mathf.FloorToInt((i % modInPage) / 2f) * 40f;
                Tabs[cListNum + Mathf.FloorToInt(i / (float)modInPage)].AddItems(new OpRect(new Vector2(pos.x - 10f, pos.y - 5f), new Vector2(240f, 30f))
                { description = desc, doesBump = true },
                new OpLabel(pos, new Vector2(220f, 20f), name));
            }
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (reason != Reason.TooManyMod) { return; }

            foreach (KeyValuePair<int, string> mod in ConfigMenu.instance.modList)
            { if (mod.Value == signal) { ConfigMenu.selectedModIndex = mod.Key; break; } }

            if (ConfigMenu.selectedModIndex < ConfigMenu.scrollTop) { ConfigMenu.scrollTop = ConfigMenu.selectedModIndex; ConfigMenu.ScrollButtons(); }
            else if (ConfigMenu.selectedModIndex > ConfigMenu.scrollTop + 12) { ConfigMenu.scrollTop = ConfigMenu.selectedModIndex - 12; ConfigMenu.ScrollButtons(); }

            ConfigMenu.instance.ChangeSelectedMod();
        }

        public OpLabel labelID, labelVersion, labelAuthor, labelCoauthor;
        public OpLabel labelDesc, labelSluggo0, labelSluggo1;
        public OpRect blue;
        public OpLabel oof;

        public override void Update(float dt)
        {
            base.Update(dt);
        }
    }
}
