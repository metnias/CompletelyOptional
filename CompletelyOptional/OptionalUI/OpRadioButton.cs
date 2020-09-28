using CompletelyOptional;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpRadioButton : UIelement, SelectableUIelement
    {
        /// <summary>
        /// This returns value in "true" of "false", although this is NOT a <see cref="UIconfig"/> thus this value won't be saved.
        /// <para>Initialize <see cref="OpRadioButtonGroup"/> first before these, then bind these to the Group using <see cref="OpRadioButtonGroup.SetButtons(OpRadioButton[])"/></para>
        /// </summary>
        /// <param name="pos">LeftBottom of the Button (The size is fixed to 24x24)</param>
        public OpRadioButton(Vector2 pos) : base(pos, new Vector2(24f, 24f))
        {
            this._value = "false";
            this.fixedSize = new Vector2(24f, 24f);
            if (!_init) { return; }
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(menu, owner, this.pos, this.size, true);
            this.subObjects.Add(rect);
            this.symbolSprite = new FSprite("Menu_Symbol_Clear_All", true);
            this.myContainer.AddChild(this.symbolSprite);
            this.symbolSprite.SetAnchor(0f, 0f);
            this.symbolSprite.SetPosition(2f, 2f);

            //this.group = group;
            this.index = -1;
            this.greyedOut = false;
            this.bumpBehav = new BumpBehaviour(this);

            this.click = false;

            this.description = InternalTranslator.Translate("Click to choose this option");
        }

        /// <summary>
        /// Lazier version of <see cref="OpRadioButton"/>. See also <seealso cref="OpRadioButtonGroup.SetButtons(OpRadioButton[])"/> for an example.
        /// <para>Initialize <see cref="OpRadioButtonGroup"/> first before these, then bind these to the Group using <see cref="OpRadioButtonGroup.SetButtons(OpRadioButton[])"/></para>
        /// </summary>
        /// <param name="posX">Left of the Button (The size is fixed to 24x24)</param>
        /// <param name="posY">Bottom of the Button</param>
        public OpRadioButton(float posX, float posY) : this(new Vector2(posX, posY)) { }

        //Use Circle from food.
        private readonly FSprite symbolSprite;

        private bool click;
        private readonly DyeableRect rect;

        /// <summary>
        /// Whether this button is greyedOut or not
        /// </summary>
        public bool greyedOut;

        /// <summary>
        /// Symbol and Edge Colour of DyeableRect. Default is MediumGrey.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of DyeableRect. Default is Black.
        /// </summary>
        public Color colorFill;

        bool SelectableUIelement.IsMouseOverMe { get { return this.MouseOver; } }

        bool SelectableUIelement.CurrentlySelectableMouse { get { return !this.greyedOut && !this.isHidden; } }

        bool SelectableUIelement.CurrentlySelectableNonMouse { get { return !this.greyedOut && !this.isHidden; } }

        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (this.isHidden) { return; }

            this.bumpBehav.greyedOut = this.greyedOut;
            this.bumpBehav.Update();

            if (greyedOut)
            {
                if (valueBool) { this.symbolSprite.alpha = 1f; }
                else { this.symbolSprite.alpha = 0f; }
                this.symbolSprite.color = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorEdge));
                this.rect.color = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorEdge));
                this.rect.colorF = DyeableRect.Grayscale(DyeableRect.MidToDark(this.colorFill));
                return;
            }

            Color ce = this.bumpBehav.GetColor(this.colorEdge);
            this.symbolSprite.color = ce;
            if (this.MouseOver)
            {
                this.symbolHalfVisible = Custom.LerpAndTick(this.symbolHalfVisible, 1f, 0.07f, 0.0166666675f);
                if (!this.valueBool) { this.symbolSprite.color = Color.Lerp(DyeableRect.MidToDark(ce), ce, this.bumpBehav.Sin(10f)); }
            }
            else
            { this.symbolHalfVisible = 0f; }

            this.rect.color = this.bumpBehav.GetColor(this.colorEdge);

            if (this.valueBool) { this.symbolSprite.alpha = 1f; }
            else { this.symbolSprite.alpha = this.symbolHalfVisible * 0.2f; }

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorF = this.colorFill;
        }

        /// <summary>
        /// size: mouseOver : big / pressed/leave : smol
        /// symbolColor : mouseOver : flash(not selected: 0.5flash) / not: 1/0
        /// </summary>
        /// <param name="dt">deltaTime</param>
        public override void Update(float dt)
        {
            base.Update(dt);
            this.bumpBehav.greyedOut = greyedOut;
            if (greyedOut || isHidden) { return; }

            if (this.MouseOver)
            {
                if (Input.GetMouseButton(0))
                {
                    this.click = true;
                    this.group.held = true;
                    this.bumpBehav.held = true;
                }
                else
                {
                    if (this.click)
                    {
                        this.group.held = false;
                        this.bumpBehav.held = false;
                        this.click = false;
                        this.menu.PlaySound(!this.valueBool ? SoundID.MENU_MultipleChoice_Clicked : SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                        this.value = "true";
                    }
                }
            }
            else if (!Input.GetMouseButton(0))
            {
                this.group.held = false;
                this.bumpBehav.held = false;
                this.click = false;
            }
        }

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            this.rect.pos = this.pos;
        }

        private float symbolHalfVisible;

        /// <summary>
        /// OpRadioButtonGroup this button is belong to.
        /// This will be automatically set when you SetButtons for Group.
        /// </summary>
        public OpRadioButtonGroup group;

        /// <summary>
        /// Index of this button.
        /// </summary>
        public int index;

        public string _value;

        /// <summary>
        /// OpRadioButton is not UIconfig, so this value will not be saved.
        /// (OpRadioButtonGroup is the one gets saved instead)
        /// </summary>
        public virtual string value
        {
            get
            {
                return _value;
            }
            set
            {
                if (this._value != value)
                {
                    this._value = value;
                    group.Switch(index);
                    OnChange();
                }
            }
        }

        /// <summary>
        /// Access value in bool form.
        /// </summary>
        public bool valueBool
        {
            set
            {
                if (value)
                {
                    this.value = "true";
                }
                else
                {
                    this.value = "false";
                }
            }
            get
            {
                if (this._value == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
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
            //this.subObjects.Remove(this.rect);
            this.symbolSprite.RemoveFromContainer();
        }
    }
}