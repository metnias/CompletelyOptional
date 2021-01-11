using Menu;
using OptionalUI;
using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Internal OI for testing new features of Config Machine
    /// </summary>
    public class InternalTestOI : OptionInterface
    {
        public InternalTestOI(PartialityMod mod) : base(mod)
        {
        }

        public override bool Configuable()
        { return false; }

        public override void Initialize()
        {
            #region init

            base.Initialize();
            this.Tabs = new OpTab[3];
            this.Tabs[0] = new OpTab("TEST0");
            this.Tabs[1] = new OpTab("TEST1") { color = Color.cyan };
            this.Tabs[2] = new OpTab("TESTSCROLL");

            string v = string.Concat(Environment.NewLine, "ConfigMachine Version", ": ");
            v += OptionMod.instance.Version.Substring(0, 1);
            for (int i = 1; i < OptionMod.instance.Version.Length; i++) { v += string.Concat(".", OptionMod.instance.Version.Substring(i, 1)); }

            Tabs[0].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f), new Vector2(100f, 20f), v, FLabelAlignment.Right));
            Tabs[1].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f), new Vector2(100f, 20f), v, FLabelAlignment.Right));

            #endregion init

            //OpColorPicker cpk = new OpColorPicker(new Vector2(100f, 100f), "_TEST", "CCCCCC") { colorEdge = Color.yellow };
            //Tabs[0].AddItems(cpk, new OpLabel(new Vector2(100f, 260f), new Vector2(150f, 20f), "OpColorPicker Test") { bumpBehav = cpk.bumpBehav });

            Texture2D t = new Texture2D(64, 64);
            Color[] n = new Color[64 * 64];
            int y = 0;
            while (y < 64)
            {
                int x = 0;
                while (x < 64)
                {
                    float s = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    //float s = UnityEngine.Random.value;
                    n[y * 64 + x] = s > 0.5f ? Color.white : Color.clear;
                    x++;
                }
                y++;
            }
            t.SetPixels(n); t.Apply();

            //Tabs[0].AddItems(new OpSpriteEditor(new Vector2(300f, 100f), new Vector2(100f, 100f), "_TEST", t));
            Tabs[0].AddItems(new OpSimpleButton(new Vector2(400f, 200f), new Vector2(100f, 24f), "singal", "SimpleButton"));

            Tabs[1].AddItems(new OpRect(new Vector2(0f, 0f), new Vector2(600f, 600f), 0f));

            OpCheckBox chk = new OpCheckBox(new Vector2(100f, 420f), "_", true);
            Tabs[1].AddItems(chk, new OpLabel(new Vector2(100f, 470f), new Vector2(50f, 15f), "CheckBox") { bumpBehav = chk.bumpBehav });

            OpRadioButtonGroup group = new OpRadioButtonGroup("_");
            Tabs[1].AddItems(group);
            group.SetButtons(new OpRadioButton[] { new OpRadioButton(300f, 450f), new OpRadioButton(350f, 450f) });

            // For canvas-sized ScrollBox
            OpScrollBox sb = new OpScrollBox(Tabs[2], 2400f, false);
            // Use OpScrollBox.AddItems instead of OpTab.AddItems.
            sb.AddItems(new OpLabel(new Vector2(100f, 540f + 1800f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f + 1800f), new Vector2(100f, 20f), v, FLabelAlignment.Right));
            sb.AddItems(new OpImage(new Vector2(420f, 50f), t));
            //sb.AddItems(new OpImage(new Vector2(420f, 120f), "gateSymbol0"));
            //Tabs[2].AddItems(new OpImage(new Vector2(420f, 120f), "gateSymbol0"));
            OpColorPicker cpk0 = new OpColorPicker(new Vector2(100f, 100f), "_TEST", "CCCCCC") { colorEdge = Color.red };
            sb.AddItems(cpk0, new OpLabel(new Vector2(300f, 100f), new Vector2(100f, 20f), "Red Colorpicker") { bumpBehav = cpk0.bumpBehav, color = Color.red });
            OpColorPicker cpk1 = new OpColorPicker(new Vector2(100f, 2000f), "_TEST", "AAAAAA") { colorEdge = Color.blue };
            sb.AddItems(cpk1, new OpLabel(new Vector2(300f, 2000f), new Vector2(100f, 20f), "Blue Colorpicker") { bumpBehav = cpk1.bumpBehav, color = Color.blue });
            sb.AddItems(new OpSlider(new Vector2(100f, 1800f), "", new IntVector2(0, 100), 300));
            sb.AddItems(new OpSlider(new Vector2(500f, 1700f), "", new IntVector2(0, 100), 300, true));
            sb.AddItems(new OpSliderSubtle(new Vector2(100f, 1400f), "", new IntVector2(0, 20), 200));
            sb.AddItems(new OpSliderSubtle(new Vector2(500f, 1300f), "", new IntVector2(0, 20), 200, true));

            /*
            sb.AddItems(new OpRect(new Vector2(25f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(225f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(425f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpLabel(new Vector2(25f, 1000f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabel(new Vector2(225f, 1000f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabel(new Vector2(425f, 1000f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });
            sb.AddItems(new OpRect(new Vector2(25f, 800f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(225f, 800f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(425f, 800f), new Vector2(150f, 150f)));
            sb.AddItems(new OpLabel(new Vector2(25f, 800f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabel(new Vector2(225f, 800f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabel(new Vector2(425f, 800f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });
            sb.AddItems(new OpRect(new Vector2(25f, 600f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(225f, 600f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(425f, 600f), new Vector2(150f, 150f)));
            sb.AddItems(new OpLabelLong(new Vector2(25f, 600f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabelLong(new Vector2(225f, 600f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabelLong(new Vector2(425f, 600f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });
            */

            OpComboBox cb = new OpComboBox(new Vector2(100f, 1000f), 150f, "", new List<ListItem>() {
                new ListItem("Slugcat"), new ListItem("Is"), new ListItem("HalfSlug"),
                new ListItem("And"), new ListItem("HalfBunny"), new ListItem("Creature") }, "Slugcat");
            sb.AddItems(cb, new OpLabel(100f, 1050f, "AutoSort") { bumpBehav = cb.bumpBehav });
            cb = new OpComboBox(new Vector2(350f, 1000f), 150f, "", new List<ListItem>() {
                new ListItem("The0", 0), new ListItem("quick1", 1), new ListItem("brown2", 2),
                new ListItem("fox3", 3), new ListItem("jumps4", 4), new ListItem("over5", 5),
                new ListItem("the6", 6), new ListItem("lazy7", 7), new ListItem("dog8", 8)}, "brown2");
            sb.AddItems(cb, new OpLabel(350f, 1050f, "DefinedOrder") { bumpBehav = cb.bumpBehav });
            sb.AddItems(new OpResourceSelector(new Vector2(200f, 900f), 200f, "", typeof(CreatureTemplate.Type)));

            sb.AddItems(new OpHoldButton(new Vector2(400f, 400f), "", "Button", 50f));
            prg = new OpHoldButton(new Vector2(200f, 400f), "", "Button") { color = new Color(1f, 0.5f, 1f) };
            sb.AddItems(prg);

            // Use OpScrollBox.AddItems instead of OpTab.AddItems.
            /*
            OpResourceSelector rs = new OpResourceSelector(new Vector2(100f, 1600f), new Vector2(200f, 30f), "", typeof(CreatureTemplate.Type));
            sb.AddItems(rs, new OpLabel(100f, 1630f, "CreatureTemplate.Type") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1500f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Decals);
            sb.AddItems(rs, new OpLabel(100f, 1530f, "Decals") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1400f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Illustrations);
            sb.AddItems(rs, new OpLabel(100f, 1430f, "Illustrations") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1300f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Palettes);
            sb.AddItems(rs, new OpLabel(100f, 1330f, "Palettes") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1200f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Regions);
            sb.AddItems(rs, new OpLabel(100f, 1230f, "Regions") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1100f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Shaders);
            sb.AddItems(rs, new OpLabel(100f, 1130f, "Shaders") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(new Vector2(100f, 1000f), new Vector2(200f, 30f), "", OpResourceSelector.SpecialEnum.Songs);
            sb.AddItems(rs, new OpLabel(100f, 1030f, "Songs") { bumpBehav = rs.bumpBehav });*/
        }

        private OpHoldButton prg;

        public override void Update(float dt)
        {
            base.Update(dt);
            prg.SetProgress(prg.progress >= 100f ? 0f : prg.progress + 0.3f + UnityEngine.Random.value * 0.1f);
            prg.GrafUpdate(dt);
        }
    }
}
