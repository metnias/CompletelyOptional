using CompletelyOptional;
using RWCustom;
using System;
using UnityEngine;
using BepInEx.Configuration;

namespace OptionalUI
{
    /// <summary>
    /// ColorPicker for Option Config.
    /// The fixedSize is 150x150, and output is in form of Hex(FFFFFF)
    /// </summary>
    public class OpColorPicker : UIconfig
    {
        /// <summary>
        /// ColorPicker for Option Config.
        /// The fixedSize is 150x150, and output is in form of Hex. (See also <seealso cref="MenuColorEffect.HexToColor"/>)
        /// </summary>
        /// <exception cref="ElementFormatException">Thrown when defaultHex is not a proper Hex code</exception>
        /// <param name="config"><see cref="ConfigEntry{T}"/> which this UIconfig is connected to. Its <see cref="ConfigEntryBase.SettingType"/> must be <see cref="bool"/>.</param>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="cosmeticHex">Default Hex for <see cref="UIconfig.cosmetic"/>. (See also <seealso cref="MenuColorEffect.ColorToHex"/>)</param>
        public OpColorPicker(ConfigEntry<string> config, Vector2 pos, string cosmeticHex = "FFFFFF") : base(config, pos, new Vector2(150f, 150f), cosmeticHex)
        {
            ctor = false; //to prevent OnChange from running before ready
            this.fixedSize = new Vector2(150f, 150f);
            if (!MenuColorEffect.IsStringHexColor(defaultValue))
            { throw new ElementFormatException(this, "OpColorPicker Error: DefaultValue is not a proper value.\nMust be in form of \'FFFFFF\'.", key); }

            this.PaletteHex = OpColorPicker.PaletteHexDefault;
            this.PaletteName = OpColorPicker.PaletteNameDefault;

            this.rect = new DyeableRect(myContainer, Vector2.zero, this.fixedSize.Value, true) { fillAlpha = 0.8f }; //Boundary Rectangle
            this._size = this.fixedSize.Value;
            this.focusGlow = new GlowGradient(myContainer, Vector2.zero, 8f, 8f, 0.5f) { color = colorText };
            this.focusGlow.Hide();

            r = 0; g = 0; b = 0;
            h = 0; s = 0; l = 0;
            this._value = "000000";

            //lblR/G/B: Displays R/G/B value
            lblB = FLabelCreate(r.ToString()); lblB.alignment = FLabelAlignment.Left;
            lblB.x = 124f; lblB.y = 40f; lblB.isVisible = false; myContainer.AddChild(lblB);
            lblG = FLabelCreate(r.ToString()); lblG.alignment = FLabelAlignment.Left;
            lblG.x = 124f; lblG.y = 80f; lblG.isVisible = false; myContainer.AddChild(lblG);
            lblR = FLabelCreate(r.ToString()); lblR.alignment = FLabelAlignment.Left;
            lblR.x = 124f; lblR.y = 120f; lblR.isVisible = false; myContainer.AddChild(lblR);
            //lblP: Displays Selected Palette Color Name
            lblP = FLabelCreate("X"); FLabelPlaceAtCenter(lblP, 15f, 88f, 120f, 20f);
            lblP.isVisible = false; lblP.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);
            myContainer.AddChild(lblP);

            //Hex Value Label
            lblHex = FLabelCreate(_value); lblHex.alignment = FLabelAlignment.Left;
            lblHex.x = 60f; lblHex.y = 15f; myContainer.AddChild(lblHex);
            //Top Menu RGB mode Button
            lblRGB = FLabelCreate("RGB"); FLabelPlaceAtCenter(lblRGB, 20f, 130f, 30f, 15f);
            myContainer.AddChild(lblRGB);
            //Top Menu HSL mode Button
            lblHSL = FLabelCreate("HSL"); FLabelPlaceAtCenter(lblHSL, 60f, 130f, 30f, 15f);
            myContainer.AddChild(lblHSL);
            //Top Menu Palette mode Button
            lblPLT = FLabelCreate("PLT"); FLabelPlaceAtCenter(lblPLT, 100f, 130f, 30f, 15f);
            myContainer.AddChild(lblPLT);

            //Calculate Texture for HSL Plane
            RecalculateTexture();

            //Add R/G/B Sliders.
            this.ftxr1 = new FTexture(ttre1, "cpk1" + key);
            this.myContainer.AddChild(this.ftxr1);
            this.ftxr2 = new FTexture(ttre2, "cpk2" + key);
            this.myContainer.AddChild(this.ftxr2);
            this.ftxr3 = new FTexture(ttre2, "cpk3" + key);
            this.myContainer.AddChild(this.ftxr3);
            this.ftxr1.SetPosition(new Vector2(60f, 80f));
            this.ftxr2.SetPosition(new Vector2(135f, 80f));
            this.ftxr3.SetPosition(new Vector2(60f, 40f));
            this.ftxr3.isVisible = false;

            //This displays current color.
            this.cdis0 = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                scaleX = 18f,
                scaleY = 12f,
                alpha = 1f,
                x = 135f,
                y = 15f
            };
            this.myContainer.AddChild(this.cdis0);

            //This displays target color.
            this.cdis1 = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                scaleX = 12f,
                scaleY = 12f,
                alpha = 1f,
                x = 45f,
                y = 15f,
                isVisible = false
            };
            this.myContainer.AddChild(this.cdis1);

            ctor = true;
            this._value = "XXXXXX";
            this.value = defaultValue;

            //ComOptPlugin.LogInfo(string.Concat(key, ") dH: ", defaultHex, "/ v: ", this.value, "/ rgb: ", r, ",", g, ",", b));
        }

        /// <summary>
        /// DyeableRect : RoundedRect (but dyeable)
        /// </summary>
        private readonly DyeableRect rect;

        public override void Reset()
        {
            base.Reset();
            if (this.mode != PickerMode.HSL) { this.SwitchMode(PickerMode.HSL); }
        }

        private FCursor cursor;

        protected internal override string DisplayDescription()
        {
            if (!string.IsNullOrEmpty(description)) { return description; }
            if (typeMode)
            {
                return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseTypeTuto);
            }
            switch (mode)
            {
                case PickerMode.RGB:
                    return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseRGBTuto);

                case PickerMode.HSL:
                    return OptionalText.GetText(OptionalText.ID.OpColorPicker_MouseHSLTuto);

                case PickerMode.Palette:
                    return OptionalText.GetText(OptionalText.ID.OpColorPicker_MousePLTTuto);
            }
            return "";
        }

        protected internal override void Reactivate()
        {
            base.Reactivate();
            RecalculateTexture();
        }

        private int clickDelay = 0;

#pragma warning disable IDE0044
        private FLabel lblHex, lblRGB, lblHSL, lblPLT;
        private FTexture ftxr1, ftxr2, ftxr3;
        private Texture2D ttre1, ttre2, ttre3;
        private FSprite cdis0, cdis1;
        private FSprite sprPltCover; // For Covering Palette

        /// <summary>
        /// Output in Color type.
        /// </summary>
        public Color valueColor
        {
            get { return MenuColorEffect.HexToColor(value); }
            set { this.value = MenuColorEffect.ColorToHex(value); }
        }

        private FLabel lblR, lblG, lblB, lblP;
        protected bool ctor = false;

        private bool greyTrigger = false;

        private void GreyOut()
        {
            if (greyTrigger)
            { //Greyout
                Color ctxt = MenuColorEffect.Greyscale(MenuColorEffect.MidToDark(colorText));
                lblHex.color = ctxt; lblRGB.color = ctxt; lblHSL.color = ctxt; lblPLT.color = ctxt;
                this.rect.colorEdge = ctxt;

                RecalculateTexture();
                this.cdis1.isVisible = false;
                if (mode == 0)
                { //RGB
                    lblR.color = ctxt; lblG.color = ctxt; lblB.color = ctxt;
                    //Texture Refresh
                    TextureGreyscale(ref this.ttre1);
                    TextureGreyscale(ref this.ttre2);
                    TextureGreyscale(ref this.ttre3);

                    //Texture Reload
                    cdis0.color = new Color(r / 100f, g / 100f, b / 100f);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr3.SetTexture(ttre3);
                    this.ftxr3.SetPosition(new Vector2(60f, 40f));
                    this.ftxr2.SetPosition(new Vector2(60f, 80f));
                    this.ftxr1.SetPosition(new Vector2(60f, 120f));
                }
                else if (mode == PickerMode.HSL)
                {
                    TextureGreyscale(ref this.ttre1);
                    TextureGreyscale(ref this.ttre2);

                    cdis0.color = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr1.SetPosition(new Vector2(60f, 80f));
                    this.ftxr2.SetPosition(new Vector2(135f, 80f));
                    lblHex.text = "#" + value.ToString();
                }
                else
                {
                    this.lblP.isVisible = false;
                    this.sprPltCover.isVisible = false;

                    TextureGreyscale(ref this.ttre1);

                    cdis0.color = PaletteColor(pi);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr1.SetPosition(new Vector2(75f, 80f));
                }
            }
            else
            { //Revert
                RecalculateTexture();
                Change();
            }
        }

        private static void TextureGreyscale(ref Texture2D texture)
        {
            for (int u = 0; u < texture.width; u++)
            {
                for (int v = 0; v < texture.height; v++)
                {
                    Color c = texture.GetPixel(u, v);
                    texture.SetPixel(u, v, MenuColorEffect.Greyscale(c));
                }
            }
            texture.Apply();
        }

        /// <summary>
        /// Edge Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Text Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorText = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        public override void GrafUpdate(float timeStacker)
        { //Visual polish.
            if (greyedOut)
            {
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rect.GrafUpdate(timeStacker);
                return;
            }
            Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);

            lblRGB.color = colorText; lblHSL.color = colorText; lblPLT.color = colorText;
            lblR.color = colorText; lblG.color = colorText; lblB.color = colorText;
            lblP.color = white;

            if (this.MouseOver)
            {
                if (this.MousePos.y > 135f)
                { //mod settings
                    FLabel pick; bool flagBump = true;
                    if (this.MousePos.x > 20f && this.MousePos.x < 50f)
                    { pick = lblRGB; curFocus = MiniFocus.ModeRGB; }
                    else if (this.MousePos.x > 60f && this.MousePos.x < 90f)
                    { pick = lblHSL; curFocus = MiniFocus.ModeHSL; }
                    else if (this.MousePos.x > 100f && this.MousePos.x < 130f)
                    { pick = lblPLT; curFocus = MiniFocus.ModePLT; }
                    else { pick = null; flagBump = false; }

                    if (flagBump)
                    {
                        if (Input.GetMouseButton(0))
                        { pick.color = MenuColorEffect.MidToDark(colorText); }
                        else
                        { pick.color = Color.Lerp(colorText, white, this.bumpBehav.Sin(10f)); }
                    }
                }
                else
                {
                    if (mode == PickerMode.RGB)
                    {
                        //Just change display values
                        if (Input.GetMouseButton(0))
                        {
                            if (this.MousePos.x >= 10f && this.MousePos.y > 30f && this.MousePos.x <= 110f && this.MousePos.y < 130f)
                            {
                                if (this.MousePos.y < 50f) { lblB.color = Color.Lerp(colorText, white, this.bumpBehav.Sin(10f)); curFocus = MiniFocus.RGB_Blue; }
                                else if (this.MousePos.y > 70f && this.MousePos.y < 90f) { lblG.color = Color.Lerp(colorText, white, this.bumpBehav.Sin(10f)); curFocus = MiniFocus.RGB_Green; }
                                else if (this.MousePos.y > 110f) { lblR.color = Color.Lerp(colorText, white, this.bumpBehav.Sin(10f)); curFocus = MiniFocus.RGB_Red; }
                            }
                        }
                    }
                }
            }

            if (this.typeMode)
            {
                this.lblHex.color = Color.Lerp(white, colorText, this.bumpBehav.Sin());
                this.cursor.color = Color.Lerp(white, colorText, this.bumpBehav.Sin());
            }
            else
            {
                if (this.MouseOverHex())
                { this.lblHex.color = Input.GetMouseButton(0) ? MenuColorEffect.MidToDark(colorText) : Color.Lerp(white, colorText, this.bumpBehav.Sin(10f)); }
                else { this.lblHex.color = colorText; }
            }

            this.rect.fillAlpha = Mathf.Lerp(0.6f, 0.8f, this.bumpBehav.col);
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.GrafUpdate(timeStacker);

            #region focusGlow

            if (curFocus == MiniFocus.None) { focusGlow.Hide(); return; }
            focusGlow.Show();
            focusGlow.color = colorText;
            switch (curFocus)
            {
                #region Modes

                case MiniFocus.ModeRGB:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(15f, 125f);
                    focusGlow.alpha = this.bumpBehav.Sin(10f) * 0.5f + 0.2f;
                    break;

                case MiniFocus.ModeHSL:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(55f, 125f);
                    focusGlow.alpha = this.bumpBehav.Sin(10f) * 0.5f + 0.2f;
                    break;

                case MiniFocus.ModePLT:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(95f, 125f);
                    focusGlow.alpha = this.bumpBehav.Sin(10f) * 0.5f + 0.2f;
                    break;

                #endregion Modes

                #region RGB

                case MiniFocus.RGB_Red:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(110f, 105f);
                    focusGlow.alpha = this.bumpBehav.Sin() * 0.4f + 0.1f;
                    break;

                case MiniFocus.RGB_Green:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(110f, 65f);
                    focusGlow.alpha = this.bumpBehav.Sin() * 0.4f + 0.1f;
                    break;

                case MiniFocus.RGB_Blue:
                    focusGlow.size = new Vector2(40f, 25f);
                    focusGlow.pos = new Vector2(110f, 25f);
                    focusGlow.alpha = this.bumpBehav.Sin() * 0.4f + 0.1f;
                    break;

                #endregion RGB

                case MiniFocus.HEX:
                    focusGlow.size = new Vector2(60f, 25f);
                    focusGlow.pos = new Vector2(60f, 5f);
                    focusGlow.alpha = 0.5f - this.bumpBehav.Sin(typeMode ? 1f : 10f) * 0.4f;
                    break;

                default:
                    focusGlow.Hide(); break;
            }

            #endregion focusGlow
        }

        private bool mouseDown = false;
        private bool typeMode = false; private bool typed;
        private string typeHex;

        private static readonly string[] acceptKeys = new string[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8",
            "9", "a", "b", "c", "d", "e", "f"
        };

        private void SwitchMode(PickerMode newmod)
        {
            //Unload current mod
            this.ftxr1.isVisible = false;
            this.ftxr2.alpha = 1f;
            this.ftxr2.isVisible = false;
            this.ftxr3.isVisible = false;
            lblR.isVisible = false;
            lblG.isVisible = false;
            lblB.isVisible = false;
            lblP.isVisible = false;
            if (this.mode == PickerMode.Palette)
            {
                this.myContainer.RemoveChild(this.sprPltCover);
                this.sprPltCover.RemoveFromContainer();
                this.sprPltCover = null;
            }

            ctor = false;
            string temp = base.value;
            value = "000000";
            mode = newmod;
            value = temp;
            mode = newmod;

            RecalculateTexture();
            //load new mod
            switch (mode)
            {
                case PickerMode.RGB:
                    lblR.isVisible = true;
                    lblG.isVisible = true;
                    lblB.isVisible = true;

                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr3.SetTexture(ttre3);
                    this.ftxr1.isVisible = true;
                    this.ftxr2.isVisible = true;
                    this.ftxr3.isVisible = true;
                    this.ftxr3.SetPosition(new Vector2(60f, 40f));
                    this.ftxr2.SetPosition(new Vector2(60f, 80f));
                    this.ftxr1.SetPosition(new Vector2(60f, 120f));
                    break;

                case PickerMode.HSL:
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr1.isVisible = true;
                    this.ftxr2.isVisible = true;
                    this.ftxr1.SetPosition(new Vector2(60f, 80f));
                    this.ftxr2.SetPosition(new Vector2(135f, 80f));

                    break;

                case PickerMode.Palette:
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
                    //ComOptPlugin.LogInfo(string.Concat("value " + _value + "swapped to " + PaletteHex[pi] + " (pi: " + pi.ToString() + "/ min: " + min.ToString() + ")"));
                    value = this.PaletteHex[pi];
                    mode = PickerMode.Palette;

                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr1.isVisible = true;
                    this.ftxr1.SetPosition(new Vector2(75f, 80f));
                    this.sprPltCover = new FSprite("pixel", true)
                    {
                        color = new Color(0f, 0f, 0f),
                        scaleX = 120f,
                        scaleY = 48f,
                        alpha = 0.5f,
                        isVisible = false
                    };
                    this.sprPltCover.SetPosition(new Vector2(75f, 56f));
                    this.myContainer.AddChild(this.sprPltCover);
                    this.cdis1.isVisible = false;
                    this.lblP.isVisible = false;
                    break;
            }

            ctor = true;
            Change();
        }

        protected bool MouseOverHex()
        {
            if (this.MousePos.y < 3f || this.MousePos.y > 27f) { return false; }
            if (this.MousePos.x < 55f || this.MousePos.x > 120f) { return false; }
            return true; // new Vector2(25f, 5f), new Vector2(80f, 20f)
        }

        public override void Update()
        {
            base.Update();
            this.rect.Update();
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

            if (this.typeMode) // Type
            {
                curFocus = MiniFocus.HEX;
                this.held = true; ConfigContainer.ForceMenuMouseMode(true);
                if (!typed && Input.anyKey)
                {
                    typed = true;
                    for (int n = 0; n < acceptKeys.Length; n++)
                    {
                        if (Input.GetKey(acceptKeys[n]))
                        {
                            typeHex += acceptKeys[n].Substring(0, 1).ToUpper();
                            lblHex.text = "#" + typeHex;
                            this.cursor.x = 70f + LabelTest.GetWidth(typeHex); // this.cursor.y = 5f;
                            PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                            break;
                        }
                    }
                }
                else if (typed && !Input.anyKey)
                { typed = false; }
                if (typeHex.Length >= 6)
                {
                    if (mode == PickerMode.Palette) { this.SwitchMode(PickerMode.RGB); }
                    value = typeHex;
                    this.typeMode = false;
                    this.held = false;
                    this.myContainer.RemoveChild(this.cursor);
                    this.cursor = null;
                    PlaySound(SoundID.MENU_Player_Unjoin_Game);
                }
                else if (Input.GetMouseButton(0) && !this.MouseOverHex())
                {
                    if (mode == PickerMode.Palette) { this.SwitchMode(PickerMode.RGB); }
                    lblHex.text = "#" + value;
                    this.typeMode = false;
                    this.held = false;
                    this.myContainer.RemoveChild(this.cursor);
                    this.cursor = null;
                    PlaySound(SoundID.MENU_Player_Unjoin_Game);
                }
                return;
            }

            if (MenuMouseMode)
            { MouseModeUpdate(); }
            else
            { NonMouseModeUpdate(); }
        }

        private void MouseModeUpdate()
        {
            curFocus = MiniFocus.None;
            if (this.clickDelay > 0) { clickDelay--; }
            if (this.MouseOver)
            {
                if (!isDirty) { PlaySound(SoundID.MENU_Button_Select_Mouse); isDirty = true; }
                if (this.MousePos.y > 135f)
                { //mode switch
                    if (Input.GetMouseButton(0))
                    {
                        if (!typed)
                        {
                            int newmod = -1;
                            if (this.MousePos.x > 20f && this.MousePos.x < 50f) { newmod = 0; }
                            else if (this.MousePos.x > 60f && this.MousePos.x < 90f) { newmod = 1; }
                            else if (this.MousePos.x > 100f && this.MousePos.x < 130f) { newmod = 2; }
                            if (newmod >= 0)
                            {
                                if (mode != (PickerMode)newmod) //Mod is changed!
                                {
                                    PlaySound(SoundID.MENU_MultipleChoice_Clicked);
                                    this.SwitchMode((PickerMode)newmod);
                                }
                                else
                                { //Clicked already chosen mod
                                    PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                                }
                            }
                        }
                        typed = true;
                    }
                    else { typed = false; }

                    return;
                }
                else
                {
                    switch (mode)
                    {
                        case PickerMode.RGB:
                            //Just change display values
                            lblR.text = r.ToString();
                            lblG.text = g.ToString();
                            lblB.text = b.ToString();

                            int dr = r; int dg = g; int db = b;
                            if (this.MousePos.x >= 10f && this.MousePos.y > 30f && this.MousePos.x <= 110f && this.MousePos.y < 130f)
                            {
                                if (this.MousePos.y < 50f) { db = Mathf.RoundToInt(this.MousePos.x - 10f); lblB.text = db.ToString(); cdis1.isVisible = true; curFocus = MiniFocus.RGB_Blue; }
                                else if (this.MousePos.y > 70f && this.MousePos.y < 90f) { dg = Mathf.RoundToInt(this.MousePos.x - 10f); lblG.text = dg.ToString(); cdis1.isVisible = true; curFocus = MiniFocus.RGB_Green; }
                                else if (this.MousePos.y > 110f) { dr = Mathf.RoundToInt(this.MousePos.x - 10f); lblR.text = dr.ToString(); cdis1.isVisible = true; curFocus = MiniFocus.RGB_Red; }

                                //Not Calculating Texture now
                            }
                            cdis1.color = new Color(dr / 100f, dg / 100f, db / 100f);

                            //ComOptPlugin.LogInfo(dr.ToString() + " " + dg.ToString() + " " + db.ToString() + " ");
                            if (Input.GetMouseButton(0))
                            {
                                if (!mouseDown)
                                {
                                    this.held = true;
                                    mouseDown = true;
                                    PlaySound(SoundID.MENU_First_Scroll_Tick);
                                }
                                else
                                {
                                    if (r != dr || g != dg || b != db)
                                    {
                                        r = dr;
                                        g = dg;
                                        b = db;
                                        PlaySound(SoundID.MENU_Scroll_Tick);
                                        this.value = string.Concat(Mathf.RoundToInt(r * 255f / 100f).ToString("X2"),
                                            Mathf.RoundToInt(g * 255f / 100f).ToString("X2"),
                                            Mathf.RoundToInt(b * 255f / 100f).ToString("X2"));
                                    }
                                }
                            }
                            else { mouseDown = false; this.held = false; }

                            break;

                        case PickerMode.HSL:
                            if (this.MousePos.x > 130f && this.MousePos.x < 140f && this.MousePos.y >= 30f && this.MousePos.y <= 130f)
                            { //Lightness
                                cdis1.color = Custom.HSL2RGB(h / 100f, s / 100f, (this.MousePos.y - 30f) / 100f);
                                cdis1.isVisible = true;
                                curFocus = MiniFocus.HSL_Lightness;

                                if (Input.GetMouseButton(0))
                                {
                                    int lastL = l;
                                    l = Mathf.RoundToInt(this.MousePos.y - 30f);

                                    if (!mouseDown)
                                    {
                                        this.held = true;
                                        mouseDown = true;
                                        PlaySound(SoundID.MENU_First_Scroll_Tick);
                                    }
                                    else
                                    {
                                        if (l != lastL)
                                        {
                                            PlaySound(SoundID.MENU_Scroll_Tick);
                                        }
                                    }
                                    Change();
                                }
                                else { mouseDown = false; this.held = false; }
                            }
                            else if (this.MousePos.x <= 110f && this.MousePos.x >= 10f && this.MousePos.y >= 30f && this.MousePos.y <= 130f)
                            { //Hue&Satuation
                                cdis1.color = Custom.HSL2RGB((this.MousePos.x - 10f) / 100f, (this.MousePos.y - 30f) / 100f, l / 100f);
                                cdis1.isVisible = true;
                                curFocus = MiniFocus.HSL_Hue;

                                if (Input.GetMouseButton(0))
                                {
                                    int lastH = h; int lastS = s;
                                    h = Mathf.RoundToInt(this.MousePos.x - 10f);
                                    s = Mathf.RoundToInt(this.MousePos.y - 30f);

                                    if (!mouseDown)
                                    {
                                        this.held = true;
                                        mouseDown = true;
                                        PlaySound(SoundID.MENU_First_Scroll_Tick);
                                    }
                                    else
                                    {
                                        if (lastH != h || lastS != s)
                                        {
                                            PlaySound(SoundID.MENU_Scroll_Tick);
                                        }
                                    }
                                    Change();
                                }
                                else { mouseDown = false; this.held = false; }
                            }

                            break;

                        case PickerMode.Palette:
                            if (this.MousePos.x <= 135f && this.MousePos.x >= 15f && this.MousePos.y >= 32f && this.MousePos.y <= 128f)
                            {
                                lblP.isVisible = true;
                                sprPltCover.isVisible = true;
                                curFocus = MiniFocus.PLT_Selector;
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
                                            PlaySound(SoundID.Mouse_Scurry);
                                        }
                                        this._value = this.PaletteHex[_i];
                                        mode = PickerMode.Palette;
                                        Change();
                                    }
                                    else { this.mouseDown = false; this.held = false; }
                                }
                                else { lblP.text = ""; }

                                FLabelPlaceAtCenter(lblP, 15f, this.MousePos.y < 80f ? 88f : 52f, 120f, 20f);
                                sprPltCover.x = 75f; sprPltCover.y = this.MousePos.y < 80f ? 104f : 56f;
                                sprPltCover.MoveToFront();
                                lblP.MoveToFront();
                            }
                            else
                            {
                                cdis1.isVisible = false;
                                lblP.isVisible = false;
                                sprPltCover.isVisible = false;
                            }
                            break;
                    }
                }

                if (this.MouseOverHex())
                {
                    curFocus = MiniFocus.HEX;
                    if (Input.GetMouseButton(0))
                    {
                        if (!typed)
                        {
                            clickDelay += FrameMultiply(60);
                            if (clickDelay > FrameMultiply(100))
                            {
                                this.typeMode = true;
                                this.clickDelay = 0;
                                this.typed = true;
                                this.typeHex = "";
                                this.lblHex.text = "#";
                                PlaySound(SoundID.MENU_Player_Join_Game);
                                this.cursor = new FCursor() { x = 70f, y = 5f };
                                this.myContainer.AddChild(this.cursor);
                            }
                        }
                        typed = true;
                    }
                    else { typed = false; }
                }
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
                    switch (mode)
                    {
                        case PickerMode.RGB:
                            lblR.text = r.ToString();
                            lblG.text = g.ToString();
                            lblB.text = b.ToString();
                            break;

                        case PickerMode.Palette:
                            lblP.isVisible = false;
                            sprPltCover.isVisible = false;
                            break;
                    }

                    isDirty = false;
                }
            }
        }

        private bool isDirty;

        private GlowGradient focusGlow;

        private string lastVal;

        private void NonMouseModeUpdate()
        {
            clickDelay = 20;
            if (!held) { lastVal = _value; return; }
            if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Throw))
            { this.held = false; this.value = lastVal; return; }
            if (curFocus < 0)
            {
                if (bumpBehav.JoystickPress(-1, 0) && curFocus != MiniFocus.ModeRGB)
                { curFocus = (MiniFocus)((int)curFocus - 1); PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); return; }
                if (bumpBehav.JoystickPress(1, 0) && curFocus != MiniFocus.ModePLT)
                { curFocus = (MiniFocus)((int)curFocus + 1); PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); return; }
                if (bumpBehav.JoystickPress(0, -1))
                {
                    switch (mode)
                    {
                        case PickerMode.RGB: curFocus = MiniFocus.RGB_Red; break;
                        default:
                        case PickerMode.HSL: curFocus = MiniFocus.HSL_Hue; break;
                        case PickerMode.Palette: curFocus = MiniFocus.PLT_Selector; break;
                    }
                    PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                    return;
                }
                if (bumpBehav.JoystickPress(0, 1)) { PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard); return; }
                if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
                {
                    PickerMode newMode;
                    switch (curFocus)
                    {
                        case MiniFocus.ModeRGB: newMode = PickerMode.RGB; break;
                        default:
                        case MiniFocus.ModeHSL: newMode = PickerMode.HSL; break;
                        case MiniFocus.ModePLT: newMode = PickerMode.Palette; break;
                    }
                    if (newMode == mode) { PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked); }
                    else { SwitchMode(newMode); PlaySound(SoundID.MENU_MultipleChoice_Clicked); }
                    return;
                }
                return;
            }
            switch (mode)
            {
                case PickerMode.RGB:
                    lblR.text = r.ToString();
                    lblG.text = g.ToString();
                    lblB.text = b.ToString();

                    int tick = bumpBehav.JoystickPressAxis(false);
                    if (tick != 0) { RGBTick(true); }
                    else
                    {
                        tick = bumpBehav.JoystickHeldAxis(false, 3f);
                        if (tick != 0) { RGBTick(false); }
                    }

                    void RGBTick(bool first)
                    {
                        switch (curFocus)
                        {
                            case MiniFocus.RGB_Red:
                                int dr = Custom.IntClamp(r + tick, 0, 100);
                                if (dr == r) { PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None); return; }
                                r = dr;
                                break;

                            case MiniFocus.RGB_Green:
                                int dg = Custom.IntClamp(g + tick, 0, 100);
                                if (dg == g) { PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None); return; }
                                g = dg; break;

                            case MiniFocus.RGB_Blue:
                                int db = Custom.IntClamp(b + tick, 0, 100);
                                if (db == b) { PlaySound(first ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.None); return; }
                                b = db;
                                break;
                        }
                        PlaySound(first ? SoundID.MENU_First_Scroll_Tick : SoundID.MENU_Scroll_Tick);
                        this.value = string.Concat(Mathf.RoundToInt(r * 255f / 100f).ToString("X2"),
                                    Mathf.RoundToInt(g * 255f / 100f).ToString("X2"),
                                    Mathf.RoundToInt(b * 255f / 100f).ToString("X2"));
                    }

                    switch (curFocus) // Switch Focus
                    {
                        case MiniFocus.RGB_Red:
                            if (bumpBehav.JoystickPress(0, 1))
                            { curFocus = MiniFocus.ModeRGB; PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); break; }
                            if (bumpBehav.JoystickPress(0, -1))
                            { curFocus = MiniFocus.RGB_Green; PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); break; }
                            break;

                        case MiniFocus.RGB_Green:
                            if (bumpBehav.JoystickPress(0, 1))
                            { curFocus = MiniFocus.RGB_Red; PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); break; }
                            if (bumpBehav.JoystickPress(0, -1))
                            { curFocus = MiniFocus.RGB_Blue; PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); break; }
                            break;

                        case MiniFocus.RGB_Blue:
                            if (bumpBehav.JoystickPress(0, 1))
                            { curFocus = MiniFocus.RGB_Green; PlaySound(SoundID.MENU_Button_Select_Gamepad_Or_Keyboard); break; }
                            if (bumpBehav.JoystickPress(0, -1))
                            { PlaySound(SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard); break; }
                            break;
                    }
                    if (bumpBehav.ButtonPress(BumpBehaviour.ButtonType.Jump))
                    { lastVal = this._value; this.held = false; return; }
                    break;

                case PickerMode.HSL:
                    break;

                case PickerMode.Palette:
                    break;
            }
        }

        protected internal override void NonMouseSetHeld(bool newHeld)
        {
            base.NonMouseSetHeld(newHeld);
            if (newHeld)
            {
                switch (mode)
                {
                    case PickerMode.RGB: curFocus = MiniFocus.RGB_Red; break;
                    case PickerMode.HSL: curFocus = MiniFocus.HSL_Hue; break;
                    case PickerMode.Palette: curFocus = MiniFocus.PLT_Selector; break;
                }
            }
        }

        private MiniFocus curFocus;

        private enum MiniFocus : int
        {
            None = -99,
            ModeRGB = -3,
            ModeHSL = -2,
            ModePLT = -1,
            RGB_Red = 1,
            RGB_Green = 2,
            RGB_Blue = 3,
            HSL_Hue = 11,
            HSL_Saturation = 12,
            HSL_Lightness = 13,
            PLT_Selector = 21,
            HEX = 31
        }

        /// <summary>
        /// Red in RGB (0 ~ 100)
        /// </summary>
        private int r = 0; //0 ~ 100

        /// <summary>
        /// Green in RGB (0 ~ 100)
        /// </summary>
        private int g = 0;

        /// <summary>
        /// Blue in RGB (0 ~ 100)
        /// </summary>
        private int b = 0;

        /// <summary>
        /// Hue in HSL (0 ~ 100)
        /// </summary>
        private int h = 0; //0 ~ 100

        /// <summary>
        /// Saturation in HSL (0 ~ 100)
        /// </summary>
        private int s = 0; //0 ~ 100

        /// <summary>
        /// Lightness in HSL (0 ~ 100)
        /// </summary>
        private int l = 0; //0 ~ 100

        /// <summary>
        /// Palette Index
        /// </summary>
        private int pi = 0;

        /// <summary>
        /// In form of Hex ("FFFFFF")
        /// </summary>
        public override string value
        {
            get
            {
                if (mode == PickerMode.HSL)
                {
                    Color c = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                    r = Mathf.RoundToInt(c.r * 100f);
                    g = Mathf.RoundToInt(c.g * 100f);
                    b = Mathf.RoundToInt(c.b * 100f);
                    string newVal = string.Concat(Mathf.RoundToInt(r * 255f / 100f).ToString("X2"),
                        Mathf.RoundToInt(g * 255f / 100f).ToString("X2"),
                        Mathf.RoundToInt(b * 255f / 100f).ToString("X2"));
                    if (this._value != newVal)
                    {
                        ConfigContainer.instance.NotifyConfigChange(this, this._value, newVal);
                        this._value = newVal;
                    }
                }
                else if (mode == PickerMode.Palette) //palette
                {
                    string newVal = this.PaletteHex[pi];
                    if (this._value != newVal)
                    {
                        ConfigContainer.instance.NotifyConfigChange(this, this._value, newVal);
                        this._value = newVal;
                    }
                    return this.PaletteHex[pi];
                }

                //RGBtoHEX

                return base.value;
            }
            set
            {
                if (base.value == value) { return; }
                if (!MenuColorEffect.IsStringHexColor(value)) { return; }

                r = Mathf.RoundToInt(Convert.ToInt32(value.Substring(0, 2), 16) / 255f * 100f);
                g = Mathf.RoundToInt(Convert.ToInt32(value.Substring(2, 2), 16) / 255f * 100f);
                b = Mathf.RoundToInt(Convert.ToInt32(value.Substring(4, 2), 16) / 255f * 100f);

                RXColorHSL _hsl = RXColor.HSLFromColor(new Color(r / 100f, g / 100f, b / 100f));
                h = Mathf.RoundToInt(_hsl.h * 100f);
                s = Mathf.RoundToInt(_hsl.s * 100f);
                l = Mathf.RoundToInt(_hsl.l * 100f);

                base.value = value;

                Change();
                if (greyedOut) { GreyOut(); }
            }
        }

        //private float LastCol { get => lastCol; set => lastCol = value; }

        protected internal override void Change()
        {
            base.Change();
            if (!ctor) { return; }

            RecalculateTexture();

            switch (mode)
            {
                case PickerMode.RGB:
                    lblR.text = r.ToString();
                    lblG.text = g.ToString();
                    lblB.text = b.ToString();

                    cdis0.color = new Color(r / 100f, g / 100f, b / 100f);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr3.SetTexture(ttre3);

                    this.ftxr3.SetPosition(new Vector2(60f, 40f));
                    this.ftxr2.SetPosition(new Vector2(60f, 80f));
                    this.ftxr1.SetPosition(new Vector2(60f, 120f));
                    lblHex.text = "#" + value;
                    break;

                case PickerMode.HSL:
                    cdis0.color = Custom.HSL2RGB(h / 100f, s / 100f, l / 100f);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr2.SetTexture(ttre2);
                    this.ftxr1.SetPosition(new Vector2(60f, 80f));
                    this.ftxr2.SetPosition(new Vector2(135f, 80f));
                    lblHex.text = "#" + value;
                    break;

                case PickerMode.Palette:
                    cdis0.color = this.PaletteColor(pi);
                    this.ftxr1.SetTexture(ttre1);
                    this.ftxr1.SetPosition(new Vector2(75f, 80f));
                    lblHex.text = "#" + this.PaletteHex[pi];
                    break;
            }
        }

        private void RecalculateTexture()
        {
            if (mode == PickerMode.RGB)
            { //RGB
                ttre1 = new Texture2D(101, 20);
                ttre2 = new Texture2D(101, 20);
                ttre3 = new Texture2D(101, 20);

                // Draw colours
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
                // Draw cursor
                Color cr = new Color(1f - r / 100f, 1f - g / 100f, 1f - b / 100f);
                for (int u = Math.Max(0, r - 4); u <= Math.Min(100, r + 4); u++)
                {
                    int h = 5 - Math.Abs(r - u);
                    for (int v = 0; v < h; v++) { ttre1.SetPixel(u, v, cr); }
                    for (int v = 20 - h; v < 20; v++) { ttre1.SetPixel(u, v, cr); }
                }
                for (int u = Math.Max(0, g - 4); u <= Math.Min(100, g + 4); u++)
                {
                    int h = 5 - Math.Abs(g - u);
                    for (int v = 0; v < h; v++) { ttre2.SetPixel(u, v, cr); }
                    for (int v = 20 - h; v < 20; v++) { ttre2.SetPixel(u, v, cr); }
                }
                for (int u = Math.Max(0, b - 4); u <= Math.Min(100, b + 4); u++)
                {
                    int h = 5 - Math.Abs(b - u);
                    for (int v = 0; v < h; v++) { ttre3.SetPixel(u, v, cr); }
                    for (int v = 20 - h; v < 20; v++) { ttre3.SetPixel(u, v, cr); }
                }

                ttre1.Apply();
                ttre2.Apply();
                ttre3.Apply();
            }
            else if (mode == PickerMode.HSL)
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

        /// <summary>
        /// 0: RGB, 1: HSL, 2: Palette
        /// </summary>
        private PickerMode mode = PickerMode.HSL;

        private enum PickerMode
        { RGB = 0, HSL = 1, Palette = 2 }

        protected internal override void Unload()
        {
            base.Unload();

            this.myContainer.RemoveChild(this.cdis0);
            this.myContainer.RemoveChild(this.cdis1);
            this.cdis0.RemoveFromContainer();
            this.cdis1.RemoveFromContainer();

            if (this.mode == PickerMode.Palette)
            {
                this.myContainer.RemoveChild(this.sprPltCover);
                this.sprPltCover.RemoveFromContainer();
            }
            this.ftxr1.Destroy();
            this.ftxr2.Destroy();
            this.ftxr3.Destroy();

            mode = 0;
        }

        // 40x10 RGBtab 40x10 HSLtab 40x10 Palette Tab
        // 100x100 <== Color Picking 100x10 <== Vertical Bar(Brightness/Satuation)
        // Below: Values(RGB/HSL/Name) and Hex value.
        // Palette : 8x8 Boxes

        // Default Palette contains 128 + 4 Slugcat colors

        private Color PaletteColor(int index)
        {
            string hex = this.PaletteHex[index];
            float _r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
            float _g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
            float _b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;

            return new Color(_r, _g, _b);
        }

        /// <summary>
        /// Edit this and <see cref="PaletteName"/> to change the Palette of this <see cref="OpColorPicker"/>
        /// <para>See also <seealso cref="MenuColorEffect.ColorToHex"/></para>
        /// </summary>
        public string[] PaletteHex;

        /// <summary>
        /// Edit this and <see cref="PaletteHex"/> to change the Palette of this <see cref="OpColorPicker"/>
        /// </summary>
        public string[] PaletteName;

        /// <summary>
        /// HTML Color Palette from
        /// <c>https://www.w3schools.com/colors/colors_names.asp</c>
        /// </summary>
        private static readonly string[] PaletteHexDefault =
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
        private static readonly string[] PaletteNameDefault =
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
            "The Nightcat",
            "5P",
            "LTTM",
            "Spearmaster",
            "Saint",
            "Gourmand",
            "Rivulet",
            "Artificer"
        };

        protected internal override string CopyToClipboard()
        {
            this.typeHex = this.value; this.lblHex.text = "#" + this.typeHex;
            return this.value;
        }

        protected internal override bool CopyFromClipboard(string value)
        {
            value = value.Trim().TrimStart('#');
            if (MenuColorEffect.IsStringHexColor(value))
            { this.typeHex = value.Substring(0, 6).ToUpper(); this.lblHex.text = "#" + this.typeHex; return true; }
            return false;
        }
    }
}