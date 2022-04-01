using CompletelyOptional;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;
using BepInEx.Configuration;

namespace OptionalUI
{
    /// <summary>
    /// Simple CheckBox.
    /// </summary>
    public class OpCheckBox : UIconfig, IValueBool
    {
        /// <summary>
        /// Simple CheckBox which returns "true" of "false". The fixedSize is 24x24.
        /// </summary>
        /// <param name="config"><see cref="ConfigEntry{T}"/> which this UIconfig is connected to.</param>
        /// <param name="pos">LeftBottom of Position</param>
        /// <param name="cosmeticBool">default bool for <see cref="UIconfig.cosmetic"/>.</param>
        public OpCheckBox(ConfigEntry<bool> config, Vector2 pos, bool cosmeticBool = false) : base(config, pos, new Vector2(24f, 24f), cosmeticBool)
        {
            this.fixedSize = new Vector2(24f, 24f);

            this.rect = new DyeableRect(myContainer, Vector2.zero, this.size, true);
            this.symbolSprite = new FSprite("Menu_Symbol_CheckBox", true)
            { anchorX = 0.5f, anchorY = 0.5f, x = 12f, y = 12f };
            this.myContainer.AddChild(this.symbolSprite);
            this.description = InternalTranslator.Translate("Click to Check/Uncheck");
        }

        /// <summary>
        /// Simple CheckBox which returns "true" of "false". The fixedSize is 24x24.
        /// </summary>
        /// <param name="config"><see cref="ConfigEntry{T}"/> which this UIconfig is connected to. Its <see cref="ConfigEntryBase.SettingType"/> must be <see cref="bool"/>.</param>
        /// <param name="posX">Left of Position</param>
        /// <param name="posY">Bottom of Position</param>
        /// <param name="defaultBool">default bool</param>
        public OpCheckBox(ConfigEntry<bool> config, float posX, float posY, bool defaultBool = false) : this(config, new Vector2(posX, posY), defaultBool)
        { }

        /// <summary>
        /// <see cref="DyeableRect"/> for the boundary.
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// <see cref="FSprite"/> for Check Symbol.
        /// </summary>
        public FSprite symbolSprite;

        /// <summary>
        /// Symbol and Edge Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        string IValueType.valueString { get => this.value; set => this.value = value; }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            if (greyedOut)
            {
                if (this.GetValueBool())
                {
                    this.symbolSprite.alpha = 1f;
                    this.symbolSprite.color = this.bumpBehav.GetColor(this.colorEdge);
                }
                else { this.symbolSprite.alpha = 0f; }
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rect.GrafUpdate(timeStacker);
                return;
            }

            Color ce = this.bumpBehav.GetColor(this.colorEdge);
            if (this.Focused() || this.MouseOver)
            {
                this.symbolHalfVisible = Custom.LerpAndTick(this.symbolHalfVisible, 1f, 0.07f, 0.0166666675f / frameMulti);
                this.symbolSprite.color = Color.Lerp(MenuColorEffect.MidToDark(ce), ce, this.bumpBehav.Sin(10f));
            }
            else
            {
                this.symbolHalfVisible = 0f;
                this.symbolSprite.color = ce;
            }

            this.rect.colorEdge = ce;

            if (this.GetValueBool()) { this.symbolSprite.alpha = 1f - this.symbolHalfVisible * 0.2f; }
            else { this.symbolSprite.alpha = this.symbolHalfVisible * 0.2f; }

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorFill = this.colorFill;
            this.rect.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            if (this.greyedOut) { return; }
            this.rect.Update();

            if (MenuMouseMode)
            {
                if (this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    { this.held = true; }
                    else
                    {
                        if (this.held)
                        {
                            this.held = false;
                            this.SetValueBool(!this.GetValueBool());
                            PlaySound(!this.GetValueBool() ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
                        }
                    }
                }
                else if (this.held)
                {
                    if (!Input.GetMouseButton(0))
                    { this.held = false; }
                }
            }
            else
            {
                if (this.held)
                {
                    menu.ShowAlert($"HELD! jmp {CtlrInput.jmp}");
                    if (!CtlrInput.jmp)
                    {
                        this.held = false;
                        this.SetValueBool(!this.GetValueBool());
                        PlaySound(!this.GetValueBool() ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
                    }
                }
            }
        }

        private float symbolHalfVisible;
    }
}