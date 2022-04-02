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
        public InternalOI_Test() : base(new RainWorldMod("_TestDummy_")
        {
            author = "topicular",
            description = "Internal OI for testing new features of Config Machine",
            version = ComOptPlugin.PLUGIN_VERSION
        }, Reason.TestOI)
        {
        }

        public override bool Configurable()
        { return false; }

        public override void Initialize()
        {
            #region init

            base.Initialize();
            this.Tabs = new OpTab[10];
            this.Tabs[0] = new OpTab(this, "TEST0");
            this.Tabs[1] = new OpTab(this, "TEST1");
            this.Tabs[2] = new OpTab(this, "TESTSCROLL");
            this.Tabs[3] = new OpTab(this, "TEST3") { colorButton = Color.blue };
            this.Tabs[4] = new OpTab(this, "TEST4") { colorCanvas = Color.yellow };
            this.Tabs[5] = new OpTab(this, "TEST5asdfsdfkasjflksasjlk");
            this.Tabs[6] = new OpTab(this);
            this.Tabs[7] = new OpTab(this, "skfhaahsgeyioreiroqrqhrqklhklr");
            this.Tabs[8] = new OpTab(this, "TESTSERSETSE");
            this.Tabs[9] = new OpTab(this, "TEST");

            Tabs[0].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 0", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            Tabs[1].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 1", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            Tabs[3].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 3", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            Tabs[7].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 7", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            Tabs[8].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 8", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            Tabs[9].AddItems(new OpLabel(new Vector2(100f, 540f), new Vector2(400f, 50f), "ConfigMachine Internal Test 9", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, 540f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));

            #endregion init

            OpColorPicker cpk = new OpColorPicker(null, new Vector2(100f, 300f), "CCCCCC") { colorEdge = Color.yellow };
            Tabs[0].AddItems(cpk, new OpLabel(new Vector2(100f, 460f), new Vector2(150f, 20f), "OpColorPicker Test") { bumpBehav = cpk.bumpBehav, color = Color.yellow });

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
            OpScrollBox scb = new OpScrollBox(new Vector2(50f, 100f), new Vector2(240f, 150f), 400f);
            Tabs[0].AddItems(scb);
            scb.AddItems(new OpLabelLong(new Vector2(20f, 20f), new Vector2(120f, 200f)),
                new OpSlider(null, new Vector2(160f, 30f), new IntVector2(-20, 20), length: 180, true, 10));
            hold = new OpHoldButton(new Vector2(440f, 200f), new Vector2(120f, 24f), "_", "Hold");
            Tabs[0].AddItems(hold);

            OpCheckBox chk = new OpCheckBox(null, new Vector2(100f, 420f), true);
            Tabs[1].AddItems(chk, new OpLabel(new Vector2(100f, 470f), new Vector2(50f, 15f), "CheckBox") { bumpBehav = chk.bumpBehav });
            OpUpdown ud = new OpUpdown(null, new Vector2(20f, 220f), 150f, 10);
            ud.SetRange(-10000000, 10000000);
            Tabs[1].AddItems(ud);
            ud = new OpUpdown(null, new Vector2(220f, 220f), 150f, 2, 20.5f);
            ud.SetRange(-10000f, 10000f);
            Tabs[1].AddItems(ud);
            Tabs[1].AddItems(new OpTextBox(null, new Vector2(420f, 220f), 150f, 30));

            // For canvas-sized ScrollBox
            OpScrollBox sb = new OpScrollBox(Tabs[2], 2400f, false);
            // Use OpScrollBox.AddItems instead of OpTab.AddItems.
            sb.AddItems(new OpLabel(new Vector2(100f, sb.contentSize - 60f), new Vector2(400f, 50f), "ConfigMachine Internal Test", FLabelAlignment.Center, true),
                    new OpLabel(new Vector2(420f, sb.contentSize - 60f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            sb.AddItems(new OpImage(new Vector2(420f, 50f), t));
            sb.AddItems(new OpImage(new Vector2(420f, 120f), "gateSymbol0"));
            // OpColorPicker cpk0 = new OpColorPicker(new Vector2(100f, 100f), "_Red", "CCCCCC") { colorEdge = Color.red };
            // sb.AddItems(cpk0, new OpLabel(new Vector2(300f, 100f), new Vector2(100f, 20f), "Red Colorpicker") { bumpBehav = cpk0.bumpBehav, color = Color.red });
            // OpColorPicker cpk1 = new OpColorPicker(new Vector2(100f, 2000f), "_Blue", "AAAAAA") { colorEdge = Color.blue };
            // sb.AddItems(cpk1, new OpLabel(new Vector2(300f, 2000f), new Vector2(100f, 20f), "Blue Colorpicker") { bumpBehav = cpk1.bumpBehav, color = Color.blue });
            sb.AddItems(new OpSlider(null, new Vector2(100f, 1800f), new IntVector2(0, 100), 300, cosmeticValue: 50));
            sb.AddItems(new OpSlider(null, new Vector2(500f, 1700f), new IntVector2(0, 100), 300, true, cosmeticValue: 50));
            sb.AddItems(new OpSliderTick(null, new Vector2(100f, 1400f), new IntVector2(0, 20), 200, cosmeticValue: 10));
            sb.AddItems(new OpSliderTick(null, new Vector2(500f, 1300f), new IntVector2(0, 20), 200, true, cosmeticValue: 10));

            Tabs[3].AddItems(new OpKeyBinder(null, new Vector2(100f, 300f), new Vector2(200f, 30f), rwMod, false, cosmeticKey: "Enter"));

            sb = new OpScrollBox(Tabs[4], 1500f, false);
            sb.AddItems(new OpLabel(new Vector2(100f, sb.contentSize - 60f), new Vector2(400f, 50f), "ConfigMachine Internal Test 4", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, sb.contentSize - 60f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            sb.AddItems(new OpRect(new Vector2(25f, 1200f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(225f, 1200f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(425f, 1200f), new Vector2(150f, 150f)));
            sb.AddItems(new OpLabel(new Vector2(25f, 1200f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabel(new Vector2(225f, 1200f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabel(new Vector2(425f, 1200f), new Vector2(150f, 150f), bigText: true) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });
            sb.AddItems(new OpRect(new Vector2(25f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(225f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpRect(new Vector2(425f, 1000f), new Vector2(150f, 150f)));
            sb.AddItems(new OpLabel(new Vector2(25f, 1000f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabel(new Vector2(225f, 1000f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabel(new Vector2(425f, 1000f), new Vector2(150f, 150f)) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });
            sb.AddItems(new OpRect(new Vector2(25f, 650f), new Vector2(550f, 250f)));
            sb.AddItems(new OpRect(new Vector2(25f, 350f), new Vector2(550f, 250f)));
            sb.AddItems(new OpRect(new Vector2(25f, 50f), new Vector2(550f, 250f)));
            sb.AddItems(new OpLabelLong(new Vector2(25f, 650f), new Vector2(550f, 250f)) { verticalAlignment = OpLabel.LabelVAlignment.Top });
            sb.AddItems(new OpLabelLong(new Vector2(25f, 350f), new Vector2(550f, 250f)) { verticalAlignment = OpLabel.LabelVAlignment.Center });
            sb.AddItems(new OpLabelLong(new Vector2(25f, 50f), new Vector2(550f, 250f)) { verticalAlignment = OpLabel.LabelVAlignment.Bottom });

            sb = new OpScrollBox(Tabs[5], 1500f, false);
            sb.AddItems(new OpLabel(new Vector2(100f, sb.contentSize - 60f), new Vector2(400f, 50f), "ConfigMachine Internal Test 5", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, sb.contentSize - 60f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            OpComboBox cb = new OpComboBox(null, new Vector2(100f, 1000f), 150f, new List<ListItem>() {
                new ListItem("Slugcat"), new ListItem("Is"), new ListItem("HalfSlug"),
                new ListItem("And"), new ListItem("HalfBunny"), new ListItem("Creature") }, "Slugcat");
            sb.AddItems(cb, new OpLabel(100f, 1050f, "AutoSort") { bumpBehav = cb.bumpBehav });
            cb = new OpComboBox(null, new Vector2(350f, 50f), 150f, new List<ListItem>() {
                new ListItem("The0", 0), new ListItem("quick1", 1), new ListItem("brown2", 2),
                new ListItem("fox3", 3), new ListItem("jumps4", 4), new ListItem("over5", 5),
                new ListItem("the6", 6), new ListItem("lazy7", 7), new ListItem("dog8", 8)}, "brown2");
            sb.AddItems(cb, new OpLabel(350f, 1050f, "DefinedOrder") { bumpBehav = cb.bumpBehav });
            sb.AddItems(new OpResourceList(null, new Vector2(70f, 600f), 200f, typeof(CreatureTemplate.Type), OpListBox.GetLineCountFromHeight(300f)));
            // sb.AddItems(new OpResourceList(new Vector2(330f, 600f), 200f, "", typeof(AbstractPhysicalObject.AbstractObjectType), 10, downward: false));
            sb.AddItems(new OpListBox(null, new Vector2(330f, 600f), 200f, new List<ListItem>() {
                new ListItem("The Survivor", 0), new ListItem("The Monk", 1), new ListItem("The Hunter", 2),
                new ListItem("The Sporecat", 3), new ListItem("The Electric Cat", 4), new ListItem("The Programmer Cat", 5)}, OpListBox.GetLineCountFromHeight(200f), false));
            sb.AddItems(new OpLabel(330f, 780f, "Player 2 plays as", true));

            sb = new OpScrollBox(Tabs[6], 1000f, false);
            sb.AddItems(new OpLabel(new Vector2(100f, sb.contentSize - 60f), new Vector2(400f, 50f), "ConfigMachine Internal Test 6", FLabelAlignment.Center, true),
                                new OpLabel(new Vector2(420f, sb.contentSize - 60f), new Vector2(100f, 20f), ComOptPlugin.PLUGIN_VERSION, FLabelAlignment.Right));
            // Use OpScrollBox.AddItems instead of OpTab.AddItems.
            OpResourceSelector rs = new OpResourceSelector(null, new Vector2(100f, 700f), 200f, typeof(CreatureTemplate.Type));
            sb.AddItems(rs, new OpLabel(100f, 730f, "CreatureTemplate.Type") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 600f), 200f, OpResourceSelector.SpecialEnum.Decals);
            sb.AddItems(rs, new OpLabel(100f, 630f, "Decals") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 500f), 200f, OpResourceSelector.SpecialEnum.Illustrations);
            sb.AddItems(rs, new OpLabel(100f, 530f, "Illustrations") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 400f), 200f, OpResourceSelector.SpecialEnum.Palettes);
            sb.AddItems(rs, new OpLabel(100f, 430f, "Palettes") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 300f), 200f, OpResourceSelector.SpecialEnum.Regions);
            sb.AddItems(rs, new OpLabel(100f, 330f, "Regions") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 200f), 200f, OpResourceSelector.SpecialEnum.Shaders);
            sb.AddItems(rs, new OpLabel(100f, 230f, "Shaders") { bumpBehav = rs.bumpBehav });
            rs = new OpResourceSelector(null, new Vector2(100f, 100f), 200f, OpResourceSelector.SpecialEnum.Songs);
            sb.AddItems(rs, new OpLabel(100f, 130f, "Songs") { bumpBehav = rs.bumpBehav });

            sb = new OpScrollBox(Tabs[7], 1000f, true);
            sb.AddItems(new OpHoldButton(new Vector2(100f, 100f), "BREAK", "BREAK"));
            sb.AddItems(new OpSimpleButton(new Vector2(400f, 100f), new Vector2(80f, 30f), "PRESS", "PRESS"));
            sb.AddItems(new OpSimpleButton(new Vector2(400f, 160f), new Vector2(80f, 30f), "HOLD", "HOLD") { canHold = true });
            sb.AddItems(new OpCheckBox(null, 400f, 300f, false));
            OpRadioButtonGroup rbg = new OpRadioButtonGroup(null);
            sb.AddItems(rbg);
            rbg.SetButtons(new OpRadioButton[] { new OpRadioButton(100f, 300f), new OpRadioButton(150f, 300f), new OpRadioButton(200f, 300f) });
            sb.AddItems(new OpDragger(null, 600f, 50f) { min = 0, max = 99 });
        }

        private OpHoldButton hold;
        // private float perc = 0f;

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (signal == "BREAK") { throw new System.Exception("TEST BREAK"); }
        }

        public override void Update()
        {
            base.Update();
            // if (Random.value < 0.3f) { perc += Mathf.Pow(Random.value, 2f); }
            // if (perc > 100f) { perc = Random.value > 0.1f ? 100f : 0f; }
            // hold.SetProgress(perc);
        }
    }
}