using System;
using Menu;
using UnityEngine;
using RWCustom;
using CompletelyOptional;

namespace OptionalUI
{
    /// <summary>
    /// ColorPicker for Option Config.
    /// Size is fixed to 150x150, and output is in form of Hex(FFFFFF)
    /// </summary>
    public class OpColorPicker : UIconfig
    {
        /// <summary>
        /// ColorPicker for Option Config.
        /// Size is fixed to 150x150, and output is in form of Hex(FFFFFF)
        /// You can access value as Color via valueColor variable.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="key">Key</param>
        /// <param name="defaultHex">default Hex.</param>
        public OpColorPicker(Vector2 pos, string key, string defaultHex = "FFFFFF") : base(pos, new Vector2(150f, 150f), key, defaultHex)
        {
            ctor = false; //to prevent OnChange from running before ready
            Vector2 fixedSize = new Vector2(150f, 150f);
            mod = 0;
            if (!_init)
            { //If this is called in main menu, just load the value, not ui.
                this.ForceValue("XXXXXX");
                try
                { this.value = defaultHex; }
                catch
                {
                    Debug.LogError(string.Concat("DefaultHex is in incorrect format! key: ", key, " defaultHex: ", defaultHex));
                    this.value = "FF00FF";
                }
                this.defaultValue = this.value;

                //Debug.Log(string.Concat(key, ") dH: ", defaultHex, " v: ", this.value, " /rgb: ", r, ",", g, ",", b));
                return;
            }

            this.PaletteHex = OpColorPicker.PaletteHexDefault;
            this.PaletteName = OpColorPicker.PaletteNameDefault;
            this.clickDelay = 0;

            this.rect = new DyeableRect(menu, owner, this.pos, fixedSize, true) { fillAlpha = 0.8f }; //Boundary Rectangle
            this.size = fixedSize;
            this.subObjects.Add(rect);

            //Set Seed, so multiple ColorPicker texture in FAtlasManager at the same time to be possible
            do { seed = Mathf.FloorToInt(UnityEngine.Random.value * 1000); }
            while (Futile.atlasManager.DoesContainAtlas(salt + "ele1"));
            r = 0; g = 0; b = 0;
            h = 0; s = 0; l = 0;
            this.ForceValue("000000");

            Color grey = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            //lblR/G/B: Displays R/G/B value
            lblB = new MenuLabel(menu, owner, r.ToString(), pos + _offset + new Vector2(124f, 30f), new Vector2(20f, 20f), false);
            lblG = new MenuLabel(menu, owner, g.ToString(), pos + _offset + new Vector2(124f, 70f), new Vector2(20f, 20f), false);
            lblR = new MenuLabel(menu, owner, b.ToString(), pos + _offset + new Vector2(124f, 110f), new Vector2(20f, 20f), false);
            this.subObjects.Add(lblR);
            this.subObjects.Add(lblG);
            this.subObjects.Add(lblB);
            lblR.label.color = grey;
            lblG.label.color = grey;
            lblB.label.color = grey;
            //lblP: Displays Selected Palette Color Name
            lblP = new MenuLabel(menu, owner, "", pos + _offset + new Vector2(10f, 5f), new Vector2(60f, 20f), false);
            this.subObjects.Add(lblP);
            lblP.label.isVisible = false;
            lblP.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);

            //Hex Value Label
            lblHex = new MenuLabel(menu, owner, value.ToString(), pos + _offset + new Vector2(50f, 5f), new Vector2(60f, 20f), false);
            this.subObjects.Add(lblHex);
            lblHex.label.alignment = FLabelAlignment.Left;
            //Top Menu RGB mode Button
            lblRGB = new MenuLabel(menu, owner, "RGB", pos + _offset + new Vector2(20f, 130f), new Vector2(30f, 15f), false);
            this.subObjects.Add(lblRGB);
            lblRGB.label.color = grey;
            //Top Menu HSL mode Button
            lblHSL = new MenuLabel(menu, owner, "HSL", pos + _offset + new Vector2(60f, 130f), new Vector2(30f, 15f), false);
            this.subObjects.Add(lblHSL);
            lblHSL.label.color = grey;
            //Top Menu Palette mode Button
            lblPLT = new MenuLabel(menu, owner, "PLT", pos + _offset + new Vector2(100f, 130f), new Vector2(30f, 15f), false);
            this.subObjects.Add(lblPLT);
            lblPLT.label.color = grey;

            //Calculate Texture for RGB Slider
            RecalculateTexture();

            //Load Texture2D to Futile
            Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
            Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
            Futile.atlasManager.LoadAtlasFromTexture(salt + "ele3", ttre3);

            //Add R/G/B Sliders.
            this.rect1 = new FSprite(salt + "ele1", true);
            this.myContainer.AddChild(this.rect1);
            this.rect2 = new FSprite(salt + "ele2", true);
            this.myContainer.AddChild(this.rect2);
            this.rect3 = new FSprite(salt + "ele3", true);
            this.myContainer.AddChild(this.rect3);
            this.rect3.SetPosition(new Vector2(60f, 40f));
            this.rect2.SetPosition(new Vector2(60f, 80f));
            this.rect1.SetPosition(new Vector2(60f, 120f));

            //This displays current color.
            this.cdis0 = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                scaleX = 18f,
                scaleY = 12f,
                alpha = 1f
            };
            this.cdis0.SetPosition(135f, 15f);
            this.myContainer.AddChild(this.cdis0);

            //This displays cursor color.
            this.cdis1 = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                scaleX = 12f,
                scaleY = 12f,
                alpha = 1f,
                isVisible = false
            };
            this.cdis1.SetPosition(70f, 15f);
            this.myContainer.AddChild(this.cdis1);

            this._description = "";
            this.inputMode = false;
            ctor = true;
            try
            {
                //Now doing this will do the job.
                this.ForceValue("XXXXXX");
                this.value = defaultHex;
                this.defaultValue = this.value;
            }
            catch
            {
                //Throw Error Screen.
                throw new ElementFormatException(this, "OpColorPicker Error: DefaultHex is not a proper value.\nMust be in form of \'FFFFFF\'.", key);
            }
            //Debug.Log(string.Concat(key, ") dH: ", defaultHex, "/ v: ", this.value, "/ rgb: ", r, ",", g, ",", b));
        }
        private readonly int seed;
        private string salt => "cpk" + seed.ToString("D3");

        /// <summary>
        /// DyeableRect : RoundedRect (but dyeable)
        /// </summary>
        public DyeableRect rect;
        public override void Reset()
        {
            base.Reset();
            this.mod = 0;
        }

        /// <summary>
        /// If you want to convert hex from config dictionary to Color but you are too lazy to code that by yourself.
        /// </summary>
        /// <param name="hex">Hex ("FFFFFF")</param>
        /// <returns>Color</returns>
        public static Color HexToColor(string hex)
        {
            if (hex == "000000") { return Color.black; }
            return new Color(
                    Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                    Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                    1f
                    );
        }

        /// <summary>
        /// Converts color to hex; useful to set default for colorpickers.
        /// </summary>
        /// <param name="color">original color</param>
        /// <returns>hex value</returns>
        public static string ColorToHex(Color color)
        {
            return string.Concat(Mathf.RoundToInt(color.r * 255).ToString("X2"),
                    Mathf.RoundToInt(color.g * 255).ToString("X2"),
                    Mathf.RoundToInt(color.b * 255).ToString("X2"));
        }

        public new string description
        {
            get
            {
                if (!string.IsNullOrEmpty(_description)) { return _description; }
                if (inputMode)
                {
                    return "Type Hex Code for desired Color";
                }
                switch (mod)
                {
                    case 0:
                        return "Select Color with RGB value";

                    case 1:
                        return "Select Color with HSL square";

                    case 2:
                        return "Select Color with Palette";
                }
                return "";
            }
            set { _description = value; }
        }
        private string _description;

        /// <summary>
        /// When the Tab this obj is in is hidden
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            foreach (FSprite sprite in this.rect.sprites)
            {
                sprite.isVisible = false;
            }
            switch (mod)
            {
                case 0:
                    lblR.label.isVisible = false;
                    lblG.label.isVisible = false;
                    lblB.label.isVisible = false;

                    this.rect1.isVisible = false;
                    this.rect2.isVisible = false;
                    this.rect3.isVisible = false;
                    break;

                case 1:
                    this.rect1.isVisible = false;
                    this.rect2.isVisible = false;
                    break;

                case 2:
                    lblP.label.isVisible = false;
                    this.rect1.isVisible = false;
                    this.rect2.isVisible = false;
                    break;
            }

            lblHex.label.isVisible = false;
            lblRGB.label.isVisible = false;
            lblHSL.label.isVisible = false;
            lblPLT.label.isVisible = false;

            this.cdis0.isVisible = false;
            this.cdis1.isVisible = false;
        }
        /// <summary>
        /// when the tab this obj is in is revealed
        /// </summary>
        public override void Show()
        {
            base.Show();
            foreach (FSprite sprite in this.rect.sprites)
            {
                sprite.isVisible = true;
            }

            RecalculateTexture();
            switch (mod)
            {
                case 0:
                    lblR.label.isVisible = true;
                    lblG.label.isVisible = true;
                    lblB.label.isVisible = true;

                    this.rect1.isVisible = true;
                    this.rect2.isVisible = true;
                    this.rect3.isVisible = true;
                    break;

                case 1:
                    this.rect1.isVisible = true;
                    this.rect2.isVisible = true;
                    break;

                case 2:
                    lblP.label.isVisible = true;
                    this.rect1.isVisible = true;
                    this.rect2.isVisible = true;
                    break;
            }

            lblHex.label.isVisible = true;
            lblRGB.label.isVisible = true;
            lblHSL.label.isVisible = true;
            lblPLT.label.isVisible = true;

            this.cdis0.isVisible = true;
            if (this.cdis1.color != this.cdis0.color)
            {
                this.cdis1.isVisible = true;
            }
        }

        private int clickDelay;

        public MenuLabel lblHex;
        public MenuLabel lblRGB;
        public MenuLabel lblHSL;
        public MenuLabel lblPLT;
        public FSprite rect1;
        public FSprite rect2;
        public FSprite rect3;
        public Texture2D ttre1;
        public Texture2D ttre2;
        public Texture2D ttre3;
        public FSprite cdis0;
        public FSprite cdis1;
        /// <summary>
        /// Output in Color type.
        /// </summary>
        public Color valueColor
        {
            get
            {
                return HexToColor(value);
            }
            set
            {
                this.value = ColorToHex(value);
            }
        }

        public MenuLabel lblR;
        public MenuLabel lblG;
        public MenuLabel lblB;
        /// <summary>
        /// Displays Name of Palette color
        /// </summary>
        public MenuLabel lblP;
        public bool ctor = false;

        private bool greyTrigger = false;

        private void GreyOut()
        {
            if (greyTrigger)
            { //Greyout
                Color ctxt = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
                lblHex.label.color = ctxt;
                lblRGB.label.color = ctxt;
                lblHSL.label.color = ctxt;
                lblPLT.label.color = ctxt;

                this.rect.color = ctxt;

                RecalculateTexture();
                this.cdis1.isVisible = false;
                if (mod == 0)
                { //RGB
                    lblR.label.color = ctxt;
                    lblG.label.color = ctxt;
                    lblB.label.color = ctxt;
                    //Texture Unload
                    this.myContainer.RemoveChild(this.rect1);
                    this.myContainer.RemoveChild(this.rect2);
                    this.myContainer.RemoveChild(this.rect3);
                    this.rect1.RemoveFromContainer();
                    this.rect2.RemoveFromContainer();
                    this.rect3.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                    Futile.atlasManager.UnloadAtlas(salt + "ele2");
                    Futile.atlasManager.UnloadAtlas(salt + "ele3");

                    this.ttre1 = Grayscale(this.ttre1);
                    this.ttre2 = Grayscale(this.ttre2);
                    this.ttre3 = Grayscale(this.ttre3);
                    this.ttre1.Apply();
                    this.ttre2.Apply();
                    this.ttre3.Apply();

                    //Texture Reload
                    cdis0.color = new Color(r / 100f, g / 100f, b / 100f);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele3", ttre3);
                    this.rect1 = new FSprite(salt + "ele1", true);
                    this.myContainer.AddChild(this.rect1);
                    this.rect2 = new FSprite(salt + "ele2", true);
                    this.myContainer.AddChild(this.rect2);
                    this.rect3 = new FSprite(salt + "ele3", true);
                    this.myContainer.AddChild(this.rect3);
                    this.rect3.SetPosition(new Vector2(60f, 40f));
                    this.rect2.SetPosition(new Vector2(60f, 80f));
                    this.rect1.SetPosition(new Vector2(60f, 120f));
                }
                else if (mod == 1)
                {
                    this.myContainer.RemoveChild(this.rect1);
                    this.myContainer.RemoveChild(this.rect2);
                    this.rect1.RemoveFromContainer();
                    this.rect2.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                    Futile.atlasManager.UnloadAtlas(salt + "ele2");

                    this.ttre1 = Grayscale(this.ttre1);
                    this.ttre2 = Grayscale(this.ttre2);
                    this.ttre1.Apply();
                    this.ttre2.Apply();

                    cdis0.color = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                    this.rect1 = new FSprite(salt + "ele1", true);
                    this.myContainer.AddChild(this.rect1);
                    this.rect2 = new FSprite(salt + "ele2", true);
                    this.myContainer.AddChild(this.rect2);
                    this.rect1.SetPosition(new Vector2(60f, 80f));
                    this.rect2.SetPosition(new Vector2(135f, 80f));
                    lblHex.text = value.ToString();
                }
                else
                {
                    this.lblP.label.isVisible = false;
                    this.rect2.isVisible = false;
                    this.myContainer.RemoveChild(this.rect1);
                    this.rect1.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");

                    this.ttre1 = Grayscale(this.ttre1);
                    this.ttre1.Apply();

                    cdis0.color = PaletteColor(pi);
                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                    this.rect1 = new FSprite(salt + "ele1", true);
                    this.myContainer.AddChild(this.rect1);
                    this.rect1.SetPosition(new Vector2(75f, 80f));
                }
                //Debug.Log(this.rect1.element.name);
            }
            else
            { //Revert
                Color ctxt = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
                lblHex.label.color = ctxt;
                lblRGB.label.color = ctxt;
                lblHSL.label.color = ctxt;
                lblPLT.label.color = ctxt;

                lblR.label.color = ctxt;
                lblG.label.color = ctxt;
                lblB.label.color = ctxt;
                lblP.label.color = ctxt;

                this.rect.color = ctxt;

                RecalculateTexture();
                OnChange();
            }
        }

        private Texture2D Grayscale(Texture2D texture)
        {
            for (int u = 0; u < texture.width; u++)
            {
                for (int v = 0; v < texture.height; v++)
                {
                    Color c = texture.GetPixel(u, v);
                    float f = c.grayscale;
                    c = new Color(f, f, f, 1f);
                    texture.SetPixel(u, v, c);
                }
            }
            return texture;
        }

        private float col; // private float lastCol;
        private float sizeBump; private float extraSizeBump;
        public override void GrafUpdate(float dt)
        { //Visual polish.
            Color darkgrey = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
            if (greyedOut)
            {
                this.rect.color = darkgrey;
                return;
            }
            Color grey = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);

            lblRGB.label.color = grey;
            lblHSL.label.color = grey;
            lblPLT.label.color = grey;
            lblR.label.color = grey;
            lblG.label.color = grey;
            lblB.label.color = grey;
            lblP.label.color = white;
            //this.LastCol = this.col;
            if (this.MouseOver)
            {
                ConfigMenu.description = this.description;

                this.col = Mathf.Min(1f, this.col + 0.1f);
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 1f, 0.1f, 0.1f);
                this.extraSizeBump = Mathf.Min(1f, this.extraSizeBump + 0.1f);

                if (this.MousePos.y > 135f)
                { //mod settings
                    FLabel pick; bool flagBump = true;
                    if (this.MousePos.x > 20f && this.MousePos.x < 50f)
                    {
                        pick = lblRGB.label;
                    }
                    else if (this.MousePos.x > 60f && this.MousePos.x < 90f)
                    {
                        pick = lblHSL.label;
                    }
                    else if (this.MousePos.x > 100f && this.MousePos.x < 130f)
                    {
                        pick = lblPLT.label;
                    }
                    else { pick = null; flagBump = false; }

                    if (flagBump)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            pick.color = darkgrey;
                        }
                        else
                        {
                            pick.color = white;
                        }
                    }
                }
                else
                {
                    if (mod == 0)
                    {
                        //Just change display values
                        if (Input.GetMouseButton(0))
                        {
                            if (this.MousePos.x >= 10f && this.MousePos.y > 30f && this.MousePos.x <= 110f && this.MousePos.y < 130f)
                            {
                                if (this.MousePos.y < 50f) { lblB.label.color = white; }
                                else if (this.MousePos.y > 70f && this.MousePos.y < 90f) { lblG.label.color = white; }
                                else if (this.MousePos.y > 110f) { lblR.label.color = white; }
                            }
                        }
                    }
                }
            }
            else
            {
                this.col = Mathf.Max(0f, this.col - 0.0333333351f);
                this.sizeBump = Custom.LerpAndTick(this.sizeBump, 0f, 0.1f, 0.05f);
                this.extraSizeBump = 0f;
            }
            //RectBump:

            this.rect.fillAlpha = Mathf.Lerp(0.6f, 0.8f, this.col);
            this.rect.addSize = new Vector2(4f, 4f) * (this.sizeBump + 0.5f * Mathf.Sin(this.extraSizeBump * 3.14159274f)) * ((this.MouseOver) ? 1f : 0f);
            Color edge = Color.Lerp(grey, white, this.col);
            this.rect.color = edge;
        }

        private bool mouseDown;
        private bool inputMode; private bool input;
        private string inputHex;

        private static readonly string[] acceptKeys = new string[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8",
            "9", "a", "b", "c", "d", "e", "f"
        };

        public override void Update(float dt)
        {
            base.Update(dt);
            if (hidden) { return; }
            this.cdis1.isVisible = false;
            this.cdis1.color = this.cdis0.color;

            if (greyedOut)
            {
                if (greyTrigger) { return; }
                greyTrigger = true;
                GreyOut();
                return;
            }
            else if (greyTrigger)
            {
                greyTrigger = false;
                GreyOut();
            }

            if (this.clickDelay > 0) { clickDelay--; }
            if (this.inputMode)
            {
                this.held = true;
                if (!input && Input.anyKey)
                {
                    input = true;
                    for (int n = 0; n < acceptKeys.Length; n++)
                    {
                        if (Input.GetKey(acceptKeys[n]))
                        {
                            inputHex += acceptKeys[n].Substring(0, 1).ToUpper();
                            lblHex.text = inputHex;
                            if (!_soundFilled && inputHex.Length < 6)
                            {
                                _soundFill += 12;
                                menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                            }
                            break;
                        }
                    }
                }
                else if (input && !Input.anyKey)
                { input = false; }
                if (inputHex.Length >= 6)
                {
                    mod = 0;
                    value = inputHex;
                    this.inputMode = false;
                    this.held = false;
                    menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
                }
                else if (Input.GetMouseButton(0) && !this.MouseOver)
                {
                    mod = 0;
                    lblHex.text = value;
                    this.inputMode = false;
                    this.held = false;
                    menu.PlaySound(SoundID.MENU_Player_Unjoin_Game);
                }
                return;
            }

            if (this.MouseOver)
            {
                bool button = false;

                if (!isDirty) { menu.PlaySound(SoundID.MENU_Button_Select_Mouse); isDirty = true; }
                if (this.MousePos.y > 135f)
                { //mod settings
                    if (Input.GetMouseButtonDown(0))
                    {
                        int newmod = -1;
                        if (this.MousePos.x > 20f && this.MousePos.x < 50f)
                        {
                            newmod = 0;
                        }
                        else if (this.MousePos.x > 60f && this.MousePos.x < 90f)
                        {
                            newmod = 1;
                        }
                        else if (this.MousePos.x > 100f && this.MousePos.x < 130f)
                        {
                            newmod = 2;
                        }

                        if (newmod != -1 && mod != newmod) //Mod is changed!
                        {
                            //button = true;
                            //Unload current mod
                            switch (mod)
                            {
                                case 0:
                                    lblR.label.isVisible = false;
                                    lblG.label.isVisible = false;
                                    lblB.label.isVisible = false;

                                    this.myContainer.RemoveChild(this.rect1);
                                    this.myContainer.RemoveChild(this.rect2);
                                    this.myContainer.RemoveChild(this.rect3);
                                    this.rect1.RemoveFromContainer();
                                    this.rect2.RemoveFromContainer();
                                    this.rect3.RemoveFromContainer();
                                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                                    Futile.atlasManager.UnloadAtlas(salt + "ele2");
                                    Futile.atlasManager.UnloadAtlas(salt + "ele3");
                                    break;

                                case 1:
                                    this.myContainer.RemoveChild(this.rect1);
                                    this.myContainer.RemoveChild(this.rect2);
                                    this.rect1.RemoveFromContainer();
                                    this.rect2.RemoveFromContainer();
                                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                                    Futile.atlasManager.UnloadAtlas(salt + "ele2");
                                    break;

                                case 2:
                                    lblP.label.isVisible = false;
                                    this.rect2.alpha = 1f;
                                    this.myContainer.RemoveChild(this.rect1);
                                    this.myContainer.RemoveChild(this.rect2);
                                    this.rect1.RemoveFromContainer();
                                    this.rect2.RemoveFromContainer();
                                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                                    break;
                            }

                            ctor = false;
                            string temp = base.value;
                            value = "000000";
                            mod = newmod;
                            value = temp;
                            mod = newmod;
                            menu.PlaySound(SoundID.MENU_MultipleChoice_Clicked);

                            RecalculateTexture();
                            //load new mod
                            switch (mod)
                            {
                                case 0:
                                    lblR.label.isVisible = true;
                                    lblG.label.isVisible = true;
                                    lblB.label.isVisible = true;

                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele3", ttre3);
                                    this.rect1 = new FSprite(salt + "ele1", true);
                                    this.myContainer.AddChild(this.rect1);
                                    this.rect2 = new FSprite(salt + "ele2", true);
                                    this.myContainer.AddChild(this.rect2);
                                    this.rect3 = new FSprite(salt + "ele3", true);
                                    this.myContainer.AddChild(this.rect3);
                                    this.rect3.SetPosition(new Vector2(60f, 40f));
                                    this.rect2.SetPosition(new Vector2(60f, 80f));
                                    this.rect1.SetPosition(new Vector2(60f, 120f));
                                    break;

                                case 1:
                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                                    this.rect1 = new FSprite(salt + "ele1", true);
                                    this.myContainer.AddChild(this.rect1);
                                    this.rect2 = new FSprite(salt + "ele2", true);
                                    this.myContainer.AddChild(this.rect2);
                                    this.rect1.SetPosition(new Vector2(60f, 80f));
                                    this.rect2.SetPosition(new Vector2(135f, 80f));

                                    break;

                                case 2:
                                    int min = 4000; pi = 0;
                                    int valr = Convert.ToInt32(temp.Substring(0, 2), 16);
                                    int valg = Convert.ToInt32(temp.Substring(2, 2), 16);
                                    int valb = Convert.ToInt32(temp.Substring(4, 2), 16);
                                    for (int k = 0; k < this.PaletteHex.Length; k++)
                                    {
                                        int dr = Math.Abs(valr - Convert.ToInt32(PaletteHex[k].Substring(0, 2), 16));
                                        int dg = Math.Abs(valg - Convert.ToInt32(PaletteHex[k].Substring(2, 2), 16));
                                        int db = Math.Abs(valb - Convert.ToInt32(PaletteHex[k].Substring(4, 2), 16));
                                        if (min > (dr + dg + db))
                                        {
                                            min = dr + dg + db;
                                            pi = k;
                                        }
                                    }
                                    //Debug.Log(string.Concat("value " + _value + "swapped to " + PaletteHex[pi] + " (pi: " + pi.ToString() + "/ min: " + min.ToString() + ")"));
                                    value = this.PaletteHex[pi];
                                    mod = 2;

                                    Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                                    this.rect1 = new FSprite(salt + "ele1", true);
                                    this.rect1.SetPosition(new Vector2(75f, 80f));
                                    this.myContainer.AddChild(this.rect1);
                                    this.rect2 = new FSprite("pixel", true)
                                    {
                                        color = new Color(0f, 0f, 0f),
                                        scaleX = 120f,
                                        scaleY = 48f,
                                        alpha = 0.5f,
                                        isVisible = false
                                    };
                                    this.rect2.SetPosition(new Vector2(75f, 56f));
                                    this.myContainer.AddChild(this.rect2);
                                    this.cdis1.isVisible = false;
                                    this.lblP.pos = pos + new Vector2(10f, 30f);
                                    this.lblP.pos = pos + new Vector2(15f, 52f);
                                    this.lblP.label.isVisible = false;
                                    break;
                            }

                            ctor = true;
                            OnChange();
                        }
                        else
                        { //Clicked already chosen mod
                            menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                        }
                    }
                    return;
                }
                else
                {
                    switch (mod)
                    {
                        case 0:
                            //Just change display values
                            lblR.text = r.ToString();
                            lblG.text = g.ToString();
                            lblB.text = b.ToString();

                            int dr = r; int dg = g; int db = b;
                            if (this.MousePos.x >= 10f && this.MousePos.y > 30f && this.MousePos.x <= 110f && this.MousePos.y < 130f)
                            {
                                if (this.MousePos.y < 50f) { db = Mathf.RoundToInt(this.MousePos.x - 10f); lblB.text = db.ToString(); cdis1.isVisible = true; }
                                else if (this.MousePos.y > 70f && this.MousePos.y < 90f) { dg = Mathf.RoundToInt(this.MousePos.x - 10f); lblG.text = dg.ToString(); cdis1.isVisible = true; }
                                else if (this.MousePos.y > 110f) { dr = Mathf.RoundToInt(this.MousePos.x - 10f); lblR.text = dr.ToString(); cdis1.isVisible = true; }

                                //Not Calculating Texture now
                            }
                            cdis1.color = new Color(dr / 100f, dg / 100f, db / 100f);

                            //Debug.Log(dr.ToString() + " " + dg.ToString() + " " + db.ToString() + " ");
                            if (Input.GetMouseButton(0))
                            {
                                if (!mouseDown)
                                {
                                    this.held = true;
                                    mouseDown = true;
                                    menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                                }
                                else
                                {
                                    if (r != dr || g != dg || b != db)
                                    {
                                        r = dr;
                                        g = dg;
                                        b = db;
                                        if (!_soundFilled)
                                        {
                                            menu.PlaySound(SoundID.MENU_Scroll_Tick);
                                            _soundFill += 4;
                                        }
                                        button = true;
                                        this.ForceValue(string.Concat(Mathf.RoundToInt(r * 255f / 100f).ToString("X2"),
                                            Mathf.RoundToInt(g * 255f / 100f).ToString("X2"),
                                            Mathf.RoundToInt(b * 255f / 100f).ToString("X2")));
                                    }
                                }
                                OnChange();
                            }
                            else { mouseDown = false; this.held = false; }

                            break;

                        case 1:
                            if (this.MousePos.x > 130f && this.MousePos.x < 140f && this.MousePos.y >= 30f && this.MousePos.y <= 130f)
                            { //Lightness
                                cdis1.color = Custom.HSL2RGB(h / 100f, s / 100f, (this.MousePos.y - 30f) / 100f);
                                cdis1.isVisible = true;

                                if (Input.GetMouseButton(0))
                                {
                                    int lastL = l;
                                    l = Mathf.RoundToInt(this.MousePos.y - 30f);

                                    if (!mouseDown)
                                    {
                                        this.held = true;
                                        mouseDown = true;
                                        menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                                    }
                                    else
                                    {
                                        if (l != lastL)
                                        {
                                            button = true;
                                            if (!_soundFilled)
                                            {
                                                menu.PlaySound(SoundID.MENU_Scroll_Tick);
                                                _soundFill += 4;
                                            }
                                        }
                                    }
                                    OnChange();
                                }
                                else { mouseDown = false; this.held = false; }
                            }
                            else if (this.MousePos.x <= 110f && this.MousePos.x >= 10f && this.MousePos.y >= 30f && this.MousePos.y <= 130f)
                            { //Hue&Satuation
                                cdis1.color = Custom.HSL2RGB((this.MousePos.x - 10f) / 100f, (this.MousePos.y - 30f) / 100f, l / 100f);
                                cdis1.isVisible = true;

                                if (Input.GetMouseButton(0))
                                {
                                    int lastH = h; int lastS = s;
                                    h = Mathf.RoundToInt(this.MousePos.x - 10f);
                                    s = Mathf.RoundToInt(this.MousePos.y - 30f);

                                    if (!mouseDown)
                                    {
                                        this.held = true;
                                        mouseDown = true;
                                        menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                                    }
                                    else
                                    {
                                        if (lastH != h || lastS != s)
                                        {
                                            button = true;
                                            if (!_soundFilled)
                                            {
                                                menu.PlaySound(SoundID.MENU_Scroll_Tick);
                                                _soundFill += 4;
                                            }
                                        }
                                    }
                                    OnChange();
                                }
                                else { mouseDown = false; this.held = false; }
                            }

                            break;

                        case 2:
                            if (this.MousePos.x <= 135f && this.MousePos.x >= 15f && this.MousePos.y >= 32f && this.MousePos.y <= 128f)
                            {
                                lblP.label.isVisible = true;
                                rect2.isVisible = true;
                                int _i = Mathf.FloorToInt((128f - this.MousePos.y) / 8f) * 15 + Mathf.FloorToInt((this.MousePos.x - 15f) / 8f);

                                if (_i < this.PaletteHex.Length)
                                {
                                    lblP.text = this.PaletteName[_i];
                                    cdis1.color = this.PaletteColor(_i);
                                    cdis1.isVisible = true;

                                    if (Input.GetMouseButton(0))
                                    {
                                        this.mouseDown = true; this.held = true;
                                        if (pi != _i)
                                        {
                                            pi = _i;
                                            button = true;
                                            if (!_soundFilled)
                                            {
                                                menu.PlaySound(SoundID.Mouse_Scurry);
                                                _soundFill += 8;
                                            }
                                        }
                                        this.ForceValue(this.PaletteHex[_i]);
                                        mod = 2;
                                        OnChange();
                                    }
                                    else { this.mouseDown = false; this.held = false; }
                                }
                                else
                                {
                                    lblP.text = "";
                                }

                                lblP.size = new Vector2(120f, 20f);
                                if (this.MousePos.y < 80f)
                                {
                                    lblP.pos = pos + new Vector2(15f, 88f);
                                    rect2.SetPosition(new Vector2(75f, 104f));
                                }
                                else
                                {
                                    lblP.pos = pos + new Vector2(15f, 52f);
                                    rect2.SetPosition(new Vector2(75f, 56f));
                                }
                                rect2.MoveToFront();
                                lblP.label.MoveToFront();
                            }
                            else
                            {
                                cdis1.isVisible = false;
                                lblP.label.isVisible = false;
                                rect2.isVisible = false;
                            }

                            break;
                    }
                }

                if (!button && Input.GetMouseButton(0) && !input)
                {
                    clickDelay += 60;
                    input = true;
                    if (clickDelay > 100)
                    {
                        this.inputMode = true;
                        this.clickDelay = 0;
                        this.input = false;
                        this.inputHex = "";
                        this.lblHex.text = "";
                        menu.PlaySound(SoundID.MENU_Player_Join_Game);
                    }
                }
                else if (input && !Input.GetMouseButton(0)) { input = false; }
            }
            else
            {
                if (this.held)
                {
                    if (!Input.GetMouseButton(0)) this.held = false;
                }
                else if (isDirty)
                { //return the values back to current setting
                    cdis1.isVisible = false;
                    switch (mod)
                    {
                        case 0:
                            lblR.text = r.ToString();
                            lblG.text = g.ToString();
                            lblB.text = b.ToString();
                            break;

                        case 2:
                            lblP.label.isVisible = false;
                            rect2.isVisible = false;
                            break;
                    }

                    isDirty = false;
                }
            }
        }
        private bool isDirty;

        //public UnityEngine.UI.Image img1;

        /// <summary>
        /// Red in RGB (0 ~ 100)
        /// </summary>
        public int r = 0; //0 ~ 100
        /// <summary>
        /// Green in RGB (0 ~ 100)
        /// </summary>
        public int g = 0;
        /// <summary>
        /// Blue in RGB (0 ~ 100)
        /// </summary>
        public int b = 0;

        /// <summary>
        /// Hue in HSL (0 ~ 100)
        /// </summary>
        public int h = 0; //0 ~ 100
        /// <summary>
        /// Saturation in HSL (0 ~ 100)
        /// </summary>
        public int s = 0; //0 ~ 100
        /// <summary>
        /// Lightness in HSL (0 ~ 100)
        /// </summary>
        public int l = 0; //0 ~ 100

        /// <summary>
        /// Palette Index
        /// </summary>
        public int pi = 0;

        /// <summary>
        /// In form of Hex ("FFFFFF")
        /// </summary>
        public override string value
        {
            get
            {
                if (mod == 1)
                {
                    Color c = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                    r = Mathf.RoundToInt(c.r * 100f);
                    g = Mathf.RoundToInt(c.g * 100f);
                    b = Mathf.RoundToInt(c.b * 100f);
                    this.ForceValue(string.Concat(Mathf.RoundToInt(r * 255f / 100f).ToString("X2"),
                        Mathf.RoundToInt(g * 255f / 100f).ToString("X2"),
                        Mathf.RoundToInt(b * 255f / 100f).ToString("X2")));
                }
                else if (mod == 2) //palette
                {
                    this.ForceValue(this.PaletteHex[pi]);
                    return this.PaletteHex[pi];
                }

                //RGBtoHEX

                return base.value;
            }
            set
            {
                if (base.value == value) { return; }
                this.ForceValue(value);

                r = Mathf.RoundToInt(Convert.ToInt32(value.Substring(0, 2), 16) / 255f * 100f);
                g = Mathf.RoundToInt(Convert.ToInt32(value.Substring(2, 2), 16) / 255f * 100f);
                b = Mathf.RoundToInt(Convert.ToInt32(value.Substring(4, 2), 16) / 255f * 100f);

                //Vector3 _hsl = FromRGB(r / 100f, g / 100f, b / 100f);
                RXColorHSL _hsl = RXColor.HSLFromColor(new Color(r / 100f, g / 100f, b / 100f));

                //Debug.Log(_value + " / " + _hsl.x.ToString()+ "/" + _hsl.y.ToString()+ "/" + _hsl.z.ToString());

                h = Mathf.FloorToInt(_hsl.h * 100f);
                s = Mathf.FloorToInt(_hsl.s * 100f);
                l = Mathf.FloorToInt(_hsl.l * 100f);

                // if (mod == 2) { mod = 0; }

                OnChange();
                if (_init && greyedOut)
                {
                    GreyOut();
                }
            }
        }

        //private float LastCol { get => lastCol; set => lastCol = value; }

        public override void OnChange()
        {
            this._size = new Vector2(150f, 150f);
            base.OnChange();
            if (!ctor || !_init) { return; }

            this.rect.pos = this.pos;
            this.lblB.pos = this.pos + new Vector2(124f, 30f);
            this.lblG.pos = this.pos + new Vector2(124f, 70f);
            this.lblR.pos = this.pos + new Vector2(124f, 110f);
            this.lblP.pos = this.pos + new Vector2(10f, 5f);
            this.lblHex.pos = this.pos + new Vector2(50f, 5f);
            this.lblRGB.pos = this.pos + new Vector2(20f, 130f);
            this.lblHSL.pos = this.pos + new Vector2(60f, 130f);
            this.lblPLT.pos = this.pos + new Vector2(100f, 130f);

            RecalculateTexture();

            if (mod == 0)
            { //RGB
                lblR.text = r.ToString();
                lblG.text = g.ToString();
                lblB.text = b.ToString();
                //Texture Unload
                this.myContainer.RemoveChild(this.rect1);
                this.myContainer.RemoveChild(this.rect2);
                this.myContainer.RemoveChild(this.rect3);
                this.rect1.RemoveFromContainer();
                this.rect2.RemoveFromContainer();
                this.rect3.RemoveFromContainer();
                Futile.atlasManager.UnloadAtlas(salt + "ele1");
                Futile.atlasManager.UnloadAtlas(salt + "ele2");
                Futile.atlasManager.UnloadAtlas(salt + "ele3");
                //Texture Reload

                cdis0.color = new Color(r / 100f, g / 100f, b / 100f);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele3", ttre3);
                this.rect1 = new FSprite(salt + "ele1", true);
                this.myContainer.AddChild(this.rect1);
                this.rect2 = new FSprite(salt + "ele2", true);
                this.myContainer.AddChild(this.rect2);
                this.rect3 = new FSprite(salt + "ele3", true);
                this.myContainer.AddChild(this.rect3);
                this.rect3.SetPosition(new Vector2(60f, 40f));
                this.rect2.SetPosition(new Vector2(60f, 80f));
                this.rect1.SetPosition(new Vector2(60f, 120f));
                lblHex.text = value.ToString();
            }
            else if (mod == 1)
            {
                this.myContainer.RemoveChild(this.rect1);
                this.myContainer.RemoveChild(this.rect2);
                this.rect1.RemoveFromContainer();
                this.rect2.RemoveFromContainer();
                Futile.atlasManager.UnloadAtlas(salt + "ele1");
                Futile.atlasManager.UnloadAtlas(salt + "ele2");

                cdis0.color = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele2", ttre2);
                this.rect1 = new FSprite(salt + "ele1", true);
                this.myContainer.AddChild(this.rect1);
                this.rect2 = new FSprite(salt + "ele2", true);
                this.myContainer.AddChild(this.rect2);
                this.rect1.SetPosition(new Vector2(60f, 80f));
                this.rect2.SetPosition(new Vector2(135f, 80f));
                lblHex.text = value.ToString();
            }
            else
            {
                this.myContainer.RemoveChild(this.rect1);
                this.rect1.RemoveFromContainer();
                Futile.atlasManager.UnloadAtlas(salt + "ele1");

                cdis0.color = this.PaletteColor(pi);
                Futile.atlasManager.LoadAtlasFromTexture(salt + "ele1", ttre1);
                this.rect1 = new FSprite(salt + "ele1", true);
                this.myContainer.AddChild(this.rect1);
                this.rect1.SetPosition(new Vector2(75f, 80f));
                lblHex.text = this.PaletteHex[pi].ToString();
            }
        }

        public void RecalculateTexture()
        {
            if (mod == 0)
            { //RGB
                ttre1 = new Texture2D(101, 20);
                ttre2 = new Texture2D(101, 20);
                ttre3 = new Texture2D(101, 20);
                for (int u = 0; u <= 100; u++)
                {
                    for (int v = 0; v < 20; v++)
                    {
                        Color c = new Color(u / 100f, g / 100f, b / 100f);
                        ttre1.SetPixel(u, v, c);
                        c = new Color(r / 100f, u / 100f, b / 100f);
                        ttre2.SetPixel(u, v, c);
                        c = new Color(r / 100f, g / 100f, u / 100f);
                        ttre3.SetPixel(u, v, c);
                    }
                }
                ttre1.Apply();
                ttre2.Apply();
                ttre3.Apply();
            }
            else if (mod == 1)
            { //HSL
                ttre1 = new Texture2D(100, 101);
                ttre2 = new Texture2D(10, 101);
                for (int v = 0; v <= 100; v++)
                {
                    Color c;
                    for (int u = 0; u < 100; u++)
                    {
                        c = Custom.HSL2RGB(u / 100f, v / 100f, l / 100f);
                        ttre1.SetPixel(u, v, c);
                    }
                    c = Custom.HSL2RGB(h / 100f, s / 100f, v / 100f);
                    for (int u = 0; u < 10; u++)
                    {
                        ttre2.SetPixel(u, v, c);
                    }
                }
                ttre2.SetPixel(0, 50, new Color(1f, 1f, 1f));
                ttre2.SetPixel(2, 50, new Color(1f, 1f, 1f));
                ttre2.SetPixel(8, 50, new Color(1f, 1f, 1f));
                ttre2.SetPixel(9, 50, new Color(1f, 1f, 1f));

                ttre1.Apply();
                ttre2.Apply();
            }
            else
            {   //Palette
                ttre1 = new Texture2D(120, 96);
                for (int y = 0; y < 12; y++)
                {
                    for (int x = 0; x < 15; x++)
                    {
                        int _i = y * 15 + x;
                        for (int u = 0; u < 8; u++)
                        {
                            for (int v = 0; v < 8; v++)
                            {
                                if (u == 7 || v == 7)
                                {
                                    ttre1.SetPixel(x * 8 + u, 96 - (y * 8 + v), new Color(0f, 0f, 0f));
                                    continue;
                                }
                                if (_i < this.PaletteHex.Length)
                                {
                                    ttre1.SetPixel(x * 8 + u, 96 - (y * 8 + v), PaletteColor(_i));
                                }
                                else
                                {
                                    ttre1.SetPixel(x * 8 + u, 96 - (y * 8 + v), new Color(0f, 0f, 0f));
                                }
                            }
                        }
                    }
                }
                ttre1.Apply();
            }
        }

        /*
        /// <summary>
        /// Output: Vector3(H, S, L) (0f ~ 1f)
        /// https://www.rapidtables.com/convert/color/rgb-to-hsl.html
        /// </summary>
        /// <param name="R">R value (0f ~ 1f)</param>
        /// <param name="G">G value (0f ~ 1f)</param>
        /// <param name="B">B value (0f ~ 1f)</param>
        /// <returns></returns>
        public static Vector3 FromRGB(float R, float G, float B)
        {
            float _Min = Mathf.Min(R, G, B);
            float _Max = Mathf.Max(R, G, B);
            float _Delta = _Max - _Min;
            //RGB : float (0f ~ 1f)
            float H = 0f;
            float S = 0f;
            float L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                S = (float)(_Delta / (1.0f - Mathf.Abs(_Max + _Min - 1.0f)));

                if (R == _Max)
                {
                    H = (G - B) / _Delta;
                }
                else if (G == _Max)
                {
                    H = 2.0f + (B - R) / _Delta;
                }
                else if (B == _Max)
                {
                    H = 4.0f + (R - G) / _Delta;
                }
            }
            //Debug.Log(string.Concat("Hue: " + (H * 60f).ToString() + "deg"));
            H = (H % 6.0f) / 6.0f;

            return new Vector3(H, S, L);
        }*/

        /// <summary>
        /// 0: RGB, 1: HSL, 2: Palette
        /// </summary>
        public int mod = 0;

        public override void Unload()
        {
            base.Unload();
            /*
            this.subObjects.Remove(rect);
            this.subObjects.Remove(lblR);
            this.subObjects.Remove(lblG);
            this.subObjects.Remove(lblB);
            this.subObjects.Remove(lblP);
            this.subObjects.Remove(lblHex);
            this.subObjects.Remove(lblRGB);
            this.subObjects.Remove(lblHSL);
            this.subObjects.Remove(lblPLT);*/
            this.myContainer.RemoveChild(this.cdis0);
            this.myContainer.RemoveChild(this.cdis0);
            this.cdis0.RemoveFromContainer();
            this.cdis1.RemoveFromContainer();

            switch (mod)
            {
                case 0:
                    this.myContainer.RemoveChild(this.rect1);
                    this.myContainer.RemoveChild(this.rect2);
                    this.myContainer.RemoveChild(this.rect3);
                    this.rect1.RemoveFromContainer();
                    this.rect2.RemoveFromContainer();
                    this.rect3.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                    Futile.atlasManager.UnloadAtlas(salt + "ele2");
                    Futile.atlasManager.UnloadAtlas(salt + "ele3");
                    break;

                case 1:
                    this.myContainer.RemoveChild(this.rect1);
                    this.myContainer.RemoveChild(this.rect2);
                    this.rect1.RemoveFromContainer();
                    this.rect2.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                    Futile.atlasManager.UnloadAtlas(salt + "ele2");
                    break;

                case 2:
                    this.myContainer.RemoveChild(this.rect1);
                    this.myContainer.RemoveChild(this.rect2);
                    this.rect1.RemoveFromContainer();
                    this.rect2.RemoveFromContainer();
                    Futile.atlasManager.UnloadAtlas(salt + "ele1");
                    break;
            }

            mod = 0;
        }

        // 40x10 RGBtab 40x10 HSLtab 40x10 Palette Tab
        // 100x100 <== Color Picking 100x10 <== Vertical Bar(Brightness/Satuation)
        // Below: Values(RGB/HSL/Name) and Hex value.
        // Palette : 8x8 Boxes

        // Default Palette contains 128 + 4 Slugcat colors
        /// <summary>
        /// Palette Color
        /// </summary>
        /// <param name="index">Color Index</param>
        /// <returns></returns>
        public Color PaletteColor(int index)
        {
            string hex = this.PaletteHex[index];
            float _r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
            float _g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
            float _b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;

            return new Color(_r, _g, _b);
        }
        public string[] PaletteHex;
        public string[] PaletteName;

        /// <summary>
        /// HTML Color Palette from
        /// https://www.w3schools.com/colors/colors_names.asp
        /// </summary>
        public static readonly string[] PaletteHexDefault =
        {
            "F0F8FF",
            "FAEBD7",
            "00FFFF",
            "7FFFD4",
            "F0FFFF",
            "F5F5DC",
            "FFE4C4",
            "000000",
            "FFEBCD",
            "0000FF",
            "8A2BE2",
            "A52A2A",
            "DEB887",
            "5F9EA0",
            "7FFF00",
            "D2691E",
            "FF7F50",
            "6495ED",
            "FFF8DC",
            "DC143C",
            "00FFFF",
            "00008B",
            "008B8B",
            "B8860B",
            "A9A9A9",
            "006400",
            "BDB76B",
            "8B008B",
            "556B2F",
            "FF8C00",
            "9932CC",
            "8B0000",
            "E9967A",
            "8FBC8F",
            "483D8B",
            "2F4F4F",
            "00CED1",
            "9400D3",
            "FF1493",
            "00BFFF",
            "696969",
            "1E90FF",
            "B22222",
            "FFFAF0",
            "228B22",
            "FF00FF",
            "DCDCDC",
            "F8F8FF",
            "FFD700",
            "DAA520",
            "808080",
            "008000",
            "ADFF2F",
            "F0FFF0",
            "FF69B4",
            "CD5C5C",
            "4B0082",
            "FFFFF0",
            "F0E68C",
            "E6E6FA",
            "FFF0F5",
            "7CFC00",
            "FFFACD",
            "ADD8E6",
            "F08080",
            "E0FFFF",
            "FAFAD2",
            "D3D3D3",
            "D3D3D3",
            "90EE90",
            "FFB6C1",
            "FFA07A",
            "20B2AA",
            "87CEFA",
            "778899",
            "778899",
            "B0C4DE",
            "FFFFE0",
            "00FF00",
            "32CD32",
            "FAF0E6",
            "FF00FF",
            "800000",
            "66CDAA",
            "0000CD",
            "BA55D3",
            "9370DB",
            "3CB371",
            "7B68EE",
            "00FA9A",
            "48D1CC",
            "C71585",
            "191970",
            "F5FFFA",
            "FFE4E1",
            "FFE4B5",
            "FFDEAD",
            "000080",
            "FDF5E6",
            "808000",
            "6B8E23",
            "FFA500",
            "FF4500",
            "DA70D6",
            "EEE8AA",
            "98FB98",
            "AFEEEE",
            "DB7093",
            "FFEFD5",
            "FFDAB9",
            "CD853F",
            "FFC0CB",
            "DDA0DD",
            "B0E0E6",
            "800080",
            "663399",
            "FF0000",
            "BC8F8F",
            "4169E1",
            "8B4513",
            "FA8072",
            "F4A460",
            "2E8B57",
            "FFF5EE",
            "A0522D",
            "C0C0C0",
            "87CEEB",
            "6A5ACD",
            "708090",
            "FFFAFA",
            "00FF7F",
            "4682B4",
            "D2B48C",
            "008080",
            "D8BFD8",
            "FF6347",
            "40E0D0",
            "EE82EE",
            "F5DEB3",
            "FFFFFF",
            "F5F5F5",
            "FFFF00",
            "9ACD32",
            "FFFFFF",
            "FFFF73",
            "FF7373",
            "010101",
            "FF66CB",
            "1B4557",
            "4F2E69",
            "ABF257",
            "F0C296",
            "91CCF0",
            "70243D"
        };
        /// <summary>
        /// Name of Palette Colors
        /// </summary>
        public static readonly string[] PaletteNameDefault =
        {
            "AliceBlue",
            "AntiqueWhite",
            "Aqua",
            "Aquamarine",
            "Azure",
            "Beige",
            "Bisque",
            "Black",
            "BlanchedAlmond",
            "Blue",
            "BlueViolet",
            "Brown",
            "BurlyWood",
            "CadetBlue",
            "Chartreuse",
            "Chocolate",
            "Coral",
            "CornflowerBlue",
            "Cornsilk",
            "Crimson",
            "Cyan",
            "DarkBlue",
            "DarkCyan",
            "DarkGoldenRod",
            "DarkGray",
            "DarkGreen",
            "DarkKhaki",
            "DarkMagenta",
            "DarkOliveGreen",
            "DarkOrange",
            "DarkOrchid",
            "DarkRed",
            "DarkSalmon",
            "DarkSeaGreen",
            "DarkSlateBlue",
            "DarkSlateGray",
            "DarkTurquoise",
            "DarkViolet",
            "DeepPink",
            "DeepSkyBlue",
            "DimGray",
            "DodgerBlue",
            "FireBrick",
            "FloralWhite",
            "ForestGreen",
            "Fuchsia",
            "Gainsboro",
            "GhostWhite",
            "Gold",
            "GoldenRod",
            "Gray",
            "Green",
            "GreenYellow",
            "HoneyDew",
            "HotPink",
            "IndianRed",
            "Indigo",
            "Ivory",
            "Khaki",
            "Lavender",
            "LavenderBlush",
            "LawnGreen",
            "LemonChiffon",
            "LightBlue",
            "LightCoral",
            "LightCyan",
            "LightGoldenRodYellow",
            "LightGray",
            "LightGrey",
            "LightGreen",
            "LightPink",
            "LightSalmon",
            "LightSeaGreen",
            "LightSkyBlue",
            "LightSlateGray",
            "LightSlateGrey",
            "LightSteelBlue",
            "LightYellow",
            "Lime",
            "LimeGreen",
            "Linen",
            "Magenta",
            "Maroon",
            "MediumAquaMarine",
            "MediumBlue",
            "MediumOrchid",
            "MediumPurple",
            "MediumSeaGreen",
            "MediumSlateBlue",
            "MediumSpringGreen",
            "MediumTurquoise",
            "MediumVioletRed",
            "MidnightBlue",
            "MintCream",
            "MistyRose",
            "Moccasin",
            "NavajoWhite",
            "Navy",
            "OldLace",
            "Olive",
            "OliveDrab",
            "Orange",
            "OrangeRed",
            "Orchid",
            "PaleGoldenRod",
            "PaleGreen",
            "PaleTurquoise",
            "PaleVioletRed",
            "PapayaWhip",
            "PeachPuff",
            "Peru",
            "Pink",
            "Plum",
            "PowderBlue",
            "Purple",
            "RebeccaPurple",
            "Red",
            "RosyBrown",
            "RoyalBlue",
            "SaddleBrown",
            "Salmon",
            "SandyBrown",
            "SeaGreen",
            "SeaShell",
            "Sienna",
            "Silver",
            "SkyBlue",
            "SlateBlue",
            "SlateGray",
            "Snow",
            "SpringGreen",
            "SteelBlue",
            "Tan",
            "Teal",
            "Thistle",
            "Tomato",
            "Turquoise",
            "Violet",
            "Wheat",
            "White",
            "WhiteSmoke",
            "Yellow",
            "YellowGreen",
            "The Survivor",
            "The Monk",
            "The Hunter",
            "The Watcher",
            "5P",
            "LTTM",
            "Spearmaster",
            "Saint",
            "Gourmand",
            "Rivulet",
            "Artificer"
        };
    }
}