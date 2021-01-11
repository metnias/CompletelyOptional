using CompletelyOptional;
using Menu;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simple CheckBox.
    /// </summary>
    public class OpCheckBox : UIconfig, SelectableUIelement
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
            if (defaultBool) { this._value = "true"; }
            else { this._value = "false"; }
            this.defaultValue = this.value;

            if (!_init) { return; }
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(menu, owner, this.pos, this.size, true);
            this.subObjects.Add(rect);
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

        bool SelectableUIelement.IsMouseOverMe { get { return !this.held && this.MouseOver; } }

        bool SelectableUIelement.CurrentlySelectableMouse { get { return !this.disabled; } }

        bool SelectableUIelement.CurrentlySelectableNonMouse { get { return !this.disabled; } }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            if (greyedOut)
            {
                if (valueBool) { this.symbolSprite.alpha = 1f; }
                else { this.symbolSprite.alpha = 0f; }
                this.symbolSprite.color = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorF = this.bumpBehav.GetColor(this.colorFill);
                return;
            }

            Color ce = this.bumpBehav.GetColor(this.colorEdge);
            if (this.MouseOver)
            {
                this.symbolHalfVisible = Custom.LerpAndTick(this.symbolHalfVisible, 1f, 0.07f, 0.0166666675f * DTMultiply(dt));
                this.symbolSprite.color = Color.Lerp(DyeableRect.MidToDark(ce), ce, this.bumpBehav.Sin(10f));
            }
            else
            {
                this.symbolHalfVisible = 0f;
                this.symbolSprite.color = ce;
            }

            this.rect.color = ce;

            if (this.valueBool) { this.symbolSprite.alpha = 1f - this.symbolHalfVisible * 0.2f; }
            else { this.symbolSprite.alpha = this.symbolHalfVisible * 0.2f; }

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorF = this.colorFill;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (this.disabled) { return; }

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
                        this.valueBool = !this.valueBool;
                        menu.PlaySound(!this.valueBool ? SoundID.MENU_Checkbox_Check : SoundID.MENU_Checkbox_Uncheck);
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
            if (!_init) { return; }
            this.rect.pos = this.pos;
        }

        public override void Hide()
        {
            base.Hide();
            this.rect.Hide();
            this.symbolSprite.isVisible = false;
        }

        public override void Show()
        {
            base.Show();
            this.rect.Show();
            this.symbolSprite.isVisible = true;
        }

        public override void Unload()
        {
            base.Unload();
            this.symbolSprite.RemoveFromContainer();
        }
    }
}
