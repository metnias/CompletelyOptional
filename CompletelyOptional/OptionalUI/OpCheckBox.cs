using CompletelyOptional;
using RWCustom;
using UnityEngine;

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
        /// <param name="pos">LeftBottom of Position</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultBool">default bool</param>
        public OpCheckBox(Vector2 pos, string key, bool defaultBool = false) : base(pos, new Vector2(24f, 24f), key, "false")
        {
            this.fixedSize = new Vector2(24f, 24f);
            this._value = defaultBool ? "true" : "false";
            this.defaultValue = this.value;

            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(myContainer, this.pos, this.size, true);
            this.symbolSprite = new FSprite("Menu_Symbol_CheckBox", true);
            this.myContainer.AddChild(this.symbolSprite);
            this.symbolSprite.SetAnchor(0f, 0f);
            this.symbolSprite.SetPosition(2f, 2f);
            this.description = InternalTranslator.Translate("Click to Check/Uncheck");
        }

        /// <summary>
        /// Simple CheckBox which returns "true" of "false". The fixedSize is 24x24.
        /// </summary>
        /// <param name="posX">Left of Position</param>
        /// <param name="posY">Bottom of Position</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultBool">default bool</param>
        public OpCheckBox(float posX, float posY, string key, bool defaultBool = false) : this(new Vector2(posX, posY), key, defaultBool)
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
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        string IValueBool.valueString { get => this.value; set => this.value = value; }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            if (greyedOut)
            {
                if (this.GetValueBool()) { this.symbolSprite.alpha = 1f; }
                else { this.symbolSprite.alpha = 0f; }
                this.symbolSprite.color = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                return;
            }

            Color ce = this.bumpBehav.GetColor(this.colorEdge);
            if (this.MouseOver)
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
        }

        public override void Update()
        {
            base.Update();
            if (this.greyedOut) { return; }

            if (this.MouseOver)
            {
                if (Input.GetMouseButton(0))
                {
                    this.held = true;
                }
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
                {
                    this.held = false;
                }
            }
        }

        private float symbolHalfVisible;

        public override void OnChange()
        {
            base.OnChange();

            this.rect.pos = this.pos;
        }
    }
}