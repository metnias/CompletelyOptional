using Menu;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpSimpleButton : UItrigger, SelectableUIelement
    {
        /// <summary>
        /// Simple Rectangular Botton
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Minimum size is 24x24</param>
        /// <param name="signal">Keyword that gets sent to Signal</param>
        /// <param name="text">Text you want to have inside the button</param>
        public OpSimpleButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal)
        {
            this._size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
            if (!_init) { return; }
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(this.menu, this.owner, this.pos, this.size, true);
            this.subObjects.Add(this.rect);
            this.rectH = new DyeableRect(this.menu, this.owner, this.pos, this.size, false);
            this.subObjects.Add(this.rectH);
            if (!IsImageButton)
            {
                this.label = new MenuLabel(this.menu, this.owner, text, this.pos, this.size, false);
                this.subObjects.Add(this.label);
            }
        }

        /// <summary>
        /// Text inside the button
        /// </summary>
        public string text { get { return this.label.text; } set { this.label.text = value; OnChange(); } }

        /// <summary>
        /// The colour of Rectangle Edge and Text
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// The colour of Rectangle Fill
        /// </summary>
        public Color colorFill;

        protected internal readonly MenuLabel label;
        protected internal readonly DyeableRect rect, rectH;

        internal bool IsImageButton => this is OpSimpleImageButton;

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(24f, this.size.x), Mathf.Max(24f, this.size.y));
            base.OnChange();
            this.rect.pos = this.pos;
            this.rect.size = this.size;
            this.rectH.pos = this.pos;
            this.rectH.size = this.size;
            if (!IsImageButton)
            {
                this.label.pos = this.pos;
                this.label.size = this.size;
            }
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            if (greyedOut)
            {
                if (!IsImageButton) { this.label.label.color = this.bumpBehav.GetColor(this.colorEdge); }
                this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorF = this.bumpBehav.GetColor(this.colorFill);
                this.rectH.color = this.bumpBehav.GetColor(this.colorEdge);
                return;
            }

            if (!IsImageButton) { this.label.label.color = this.bumpBehav.GetColor(this.colorEdge); }

            this.rectH.color = this.bumpBehav.GetColor(this.colorEdge);
            this.rectH.addSize = new Vector2(-2f, -2f) * this.bumpBehav.AddSize;
            float highlight = this.MouseOver && !this.held ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * this.bumpBehav.AddSize : 0f;
            for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = highlight; }

            this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(6f, 6f) * this.bumpBehav.AddSize;
            this.rect.colorF = this.colorFill;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (disabled) { return; }

            if (this.MouseOver)
            {
                if (Input.GetMouseButton(0))
                { this.held = true; }
                else
                {
                    if (this.held)
                    {
                        this.held = false;
                        this.menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                        this.Signal();
                    }
                }
            }
            else if (!Input.GetMouseButton(0))
            {
                this.held = false;
            }
        }

        public override void Hide()
        {
            base.Hide();
            if (!_init) { return; }
            this.rect.Hide(); this.rectH.Hide();
            if (!IsImageButton) { this.label.label.isVisible = false; }
        }

        public override void Show()
        {
            base.Show();
            if (!_init) { return; }
            this.rect.Show(); this.rectH.Show();
            if (!IsImageButton) { this.label.label.isVisible = true; }
        }
    }
}
