using Menu;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special kind of OpTab for ConfigMenu. You don't need this.
    /// </summary>
    public class MenuTab : OpTab
    {
        public MenuTab() : base("")
        {
            isHidden = false;
            _initLog = false; logMode = false;
            _lblInfoDesc = InternalTranslator.Translate("Click to Display Config Machine ChangeLog");
            _sbDesc = InternalTranslator.Translate("Display <ModID>");
        }

        private static ConfigTabController TabCtrler => ConfigMenu.tabCtrler;
        public OpLabel lblInfoButton;
        private readonly string _lblInfoDesc, _sbDesc;
        protected bool _initLog; private bool _pressInfoBtn;
        public static bool logMode { get; protected set; }

        public new void Update(float dt)
        {
            foreach (UIelement item in this.items) { item.Update(dt); }

            if (!logMode)
            {
                if (this.lblInfoButton == null) { return; }
                if (this.lblInfoButton.MouseOver)
                {
                    ConfigMenu.description = _lblInfoDesc;
                    if (Input.GetMouseButton(0))
                    { this._pressInfoBtn = true; }
                    else
                    {
                        if (this._pressInfoBtn)
                        {
                            this._pressInfoBtn = false;
                            this.lblInfoButton.menu.PlaySound(SoundID.MENU_Regions_Switch_Region);
                            this.ShowChangeLog();
                        }
                    }
                }
                else if (!Input.GetMouseButton(0)) { this._pressInfoBtn = false; }
            }
            else
            {
                if (curVer == 0) { sbLeft.Hide(); }
                else { sbLeft.Show(); sbLeft.description = _sbDesc.Replace("<ModID>", vers[curVer - 1]); }
                if (curVer == vers.Length - 1) { sbRight.Hide(); }
                else { sbRight.Show(); sbRight.description = _sbDesc.Replace("<ModID>", vers[curVer + 1]); }
            }
        }

        private static string[] rawLog;
        private OpScrollBox box;
        private OpLabel lblTitle, lblVer, lblTrans;
        private ScrollButton sbRight, sbLeft;
        private string[] vers;
        private int curVer;

        private static string GetTranslator()
        {
            string name = InternalTranslator.Translate("Translator Name");
            if (name == "Translator Name") { return string.Empty; }
            return InternalTranslator.Translate("Translated to English by <TranslatorName>").Replace("<TranslatorName>", name);
        }

        public static string GetCMVersion()
        {
            string v = OptionMod.instance.Version.Substring(0, 1);
            for (int i = 1; i < OptionMod.instance.Version.Length; i++) { v += string.Concat(".", OptionMod.instance.Version.Substring(i, 1)); }
            return InternalTranslator.Translate("Version: <ModVersion>").Replace("<ModVersion>", v);
        }

        private static string ConvertDate(string yyyymmdd) => DateTime.ParseExact(yyyymmdd, "yyyyMMdd", null).ToString("D", InternalTranslator.GetCultureInfo());

        private void InitChangeLog()
        {
            List<UIelement> newElems = new List<UIelement>();

            this.lblTitle = new OpLabel(new Vector2(200f, 560f), new Vector2(200f, 30f), InternalTranslator.Translate("Config Machine ChangeLog"), bigText: true);
            this.lblVer = new OpLabel(new Vector2(50f, 540f), new Vector2(200f, 20f), GetCMVersion(), FLabelAlignment.Left);
            this.lblTrans = new OpLabel(new Vector2(350f, 540f), new Vector2(200f, 20f), GetTranslator(), FLabelAlignment.Right);
            this.AddItems(this.lblTitle, this.lblVer, this.lblTrans);
            newElems.Add(this.lblTitle); newElems.Add(this.lblVer); newElems.Add(this.lblTrans);

            if (rawLog == null) { ReadTXT(); }
            int len = Math.Min(rawLog.Length - 1, 16);
            this.box = new OpScrollBox(new Vector2(20f, 10f), new Vector2(560f, 520f), len * 560f, true, true, false)
            { redrawFlags = OpScrollBox.RedrawEvents.OnHover, doesBackBump = false };
            this.AddItems(this.box); this.box.Lock(true);
            newElems.Add(this.box);
            this.vers = new string[len];
            for (int i = 0; i < len; i++)
            {
                string[] split = Regex.Split(rawLog[i + 1], "#!#");
                this.vers[i] = split[0];
                OpLabel ver = new OpLabel(new Vector2(130f + i * 560f, 470f), new Vector2(300f, 30f), split[0], bigText: true);
                OpLabelLong log = new OpLabelLong(new Vector2(20f + i * 560f, 20f), new Vector2(520f, 420f), split[2].Trim()) { allowOverflow = false };
                if (!string.IsNullOrEmpty(split[1]))
                { ver.description = ConvertDate(split[1].Trim()); ver.bumpBehav = new BumpBehaviour(ver); }
                this.box.AddItems(ver, log);
                newElems.Add(ver); newElems.Add(log);
            }

            sbRight = new ScrollButton(new Vector2(460f, 470f), true);
            sbLeft = new ScrollButton(new Vector2(90f, 470f), false);
            this.AddItems(sbRight, sbLeft); sbLeft.Hide(); curVer = 0;
            newElems.Add(sbRight); newElems.Add(sbLeft);

            foreach (UIelement element in newElems)
            {
                foreach (MenuObject obj in element.subObjects) { OptionScript.configMenu.pages[0].subObjects.Add(obj); }
                OptionScript.configMenu.pages[0].Container.AddChild(element.myContainer);
            }

            _initLog = true;
        }

        public void Signal(string signalText)
        {
            curVer += signalText == "Right" ? 1 : -1;
            this.box.targetScrollOffset = curVer * -560f;
        }

        private static void ReadTXT()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "CompletelyOptional.Changelog.txt";
            string result;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            { result = reader.ReadToEnd(); }

            rawLog = Regex.Split(result, "###");
        }

        public void ShowChangeLog()
        {
            TabCtrler.Hide(); lblInfoButton.Hide(); ConfigMenu.currentTab.Hide();
            if (!_initLog) { InitChangeLog(); }
            lblTitle.Show(); lblVer.Show(); lblTrans.Show();
            box.Show(); box.MarkDirty();
            logMode = true;
        }

        public void HideChangeLog()
        {
            TabCtrler.Show(); lblInfoButton.Show(); ConfigMenu.currentTab.Show();
            lblTitle.Hide(); lblVer.Hide(); lblTrans.Hide();
            box.Hide(); curVer = 0; box.ScrollToTop(true);
            sbLeft.Hide(); sbRight.Hide();
            logMode = false;
        }

        public new void Unload()
        {
            foreach (UIelement item in this.items)
            { item.Unload(); }
            TabCtrler.Unload();
        }

        private class ScrollButton : OpSimpleImageButton
        {
            public ScrollButton(Vector2 pos, bool right) : base(pos, new Vector2(50f, 50f), right ? "Right" : "Left", "Big_Menu_Arrow")
            {
                this.sprite.rotation = right ? 90f : -90f;
            }

            public override void Signal()
            {
                (this.tab as MenuTab).Signal(this.signal);
            }

            public override void GrafUpdate(float dt)
            {
                base.GrafUpdate(dt);
                this.myContainer.SetPosition(this.pos);
            }
        }
    }
}