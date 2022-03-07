using OptionalUI;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Internal OI for testing new features of Config Machine
    /// </summary>
    internal class InternalOI_Test : InternalOI
    {
        public InternalOI_Test() : base(new RainWorldMod("TestDummy")
        {
            author = "topicular",
            description = "Internal OI for testing new features of Config Machine",
            Version = ComOptPlugin.ver
        }, Reason.TestOI)
        {
        }

        public override bool Configurable()
        { return false; }

        public override void Initialize()
        {
            #region init

            base.Initialize();
            this.Tabs = new OpTab[3];
            this.Tabs[0] = new OpTab(this, "TEST0");
            this.Tabs[1] = new OpTab(this, "TEST1");
            this.Tabs[2] = new OpTab(this, "TESTSCROLL");

            Tabs[0].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f), new Vector2(100f, 20f), ComOptPlugin.ver, FLabelAlignment.Right));
            Tabs[1].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f), new Vector2(100f, 20f), ComOptPlugin.ver, FLabelAlignment.Right));

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
            OpScrollBox scb = new OpScrollBox(new Vector2(100f, 100f), new Vector2(240f, 150f), 400f);
            Tabs[0].AddItems(scb);
            scb.AddItems(new OpLabelLong(new Vector2(20f, 20f), new Vector2(120f, 200f)),
                new OpSlider(new Vector2(160f, 30f), "_", new IntVector2(-20, 20), length: 180, true, 10));

            OpCheckBox chk = new OpCheckBox(new Vector2(100f, 420f), "_", true);
            Tabs[1].AddItems(chk, new OpLabel(new Vector2(100f, 470f), new Vector2(50f, 15f), "CheckBox") { bumpBehav = chk.bumpBehav });
            OpUpdown ud = new OpUpdown(new Vector2(20f, 220f), 150f, "_", 10);
            ud.SetRange(-10000000, 10000000);
            Tabs[1].AddItems(ud);
            ud = new OpUpdown(new Vector2(220f, 220f), 150f, "_", 20.5f, 2);
            ud.SetRange(-10000f, 10000f);
            Tabs[1].AddItems(ud);
            Tabs[1].AddItems(new OpTextBox(new Vector2(420f, 220f), 150f, "_", "30", OpTextBox.Accept.Int));

            // For canvas-sized ScrollBox
            OpScrollBox sb = new OpScrollBox(Tabs[2], 2400f, false);
            // Use OpScrollBox.AddItems instead of OpTab.AddItems.
            sb.AddItems(new OpLabel(new Vector2(100f, 540f + 1800f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, 510f + 1800f), new Vector2(100f, 20f), ComOptPlugin.ver, FLabelAlignment.Right));
            sb.AddItems(new OpImage(new Vector2(420f, 50f), t));
            //sb.AddItems(new OpImage(new Vector2(420f, 120f), "gateSymbol0"));
            //Tabs[2].AddItems(new OpImage(new Vector2(420f, 120f), "gateSymbol0"));
            OpColorPicker cpk0 = new OpColorPicker(new Vector2(100f, 100f), "_TEST", "CCCCCC") { colorEdge = Color.red };
            sb.AddItems(cpk0, new OpLabel(new Vector2(300f, 100f), new Vector2(100f, 20f), "Red Colorpicker") { bumpBehav = cpk0.bumpBehav, color = Color.red });
            OpColorPicker cpk1 = new OpColorPicker(new Vector2(100f, 2000f), "_TEST", "AAAAAA") { colorEdge = Color.blue };
            sb.AddItems(cpk1, new OpLabel(new Vector2(300f, 2000f), new Vector2(100f, 20f), "Blue Colorpicker") { bumpBehav = cpk1.bumpBehav, color = Color.blue });
            sb.AddItems(new OpSlider(new Vector2(100f, 1800f), "", new IntVector2(0, 100), 300));
            sb.AddItems(new OpSlider(new Vector2(500f, 1700f), "", new IntVector2(0, 100), 300, true));
            sb.AddItems(new OpSliderTick(new Vector2(100f, 1400f), "", new IntVector2(0, 20), 200));
            sb.AddItems(new OpSliderTick(new Vector2(500f, 1300f), "", new IntVector2(0, 20), 200, true));

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
            sb.AddItems(new OpResourceList(new Vector2(70f, 600f), 200f, "", typeof(CreatureTemplate.Type), OpListBox.GetLineCountFromHeight(300f)));
            // sb.AddItems(new OpResourceList(new Vector2(330f, 600f), 200f, "", typeof(AbstractPhysicalObject.AbstractObjectType), 10, downward: false));
            sb.AddItems(new OpListBox(new Vector2(330f, 600f), 200f, "", new List<ListItem>() {
                new ListItem("The Survivor", 0), new ListItem("The Monk", 1), new ListItem("The Hunter", 2),
                new ListItem("The Sporecat", 3), new ListItem("The Electric Cat", 4), new ListItem("The Programmer Cat", 5)}, OpListBox.GetLineCountFromHeight(200f)));
            sb.AddItems(new OpLabel(330f, 780f, "Player 2 plays as", true));
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
    }
}