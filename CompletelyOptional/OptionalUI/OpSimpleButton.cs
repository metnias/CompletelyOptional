using UnityEngine;

namespace OptionalUI
{
    public class OpSimpleButton : UItrigger
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

            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.rect = new DyeableRect(this.myContainer, this.pos, this.size, true);
            this.rectH = new DyeableRect(this.myContainer, this.pos, this.size, false);
            if (!IsImageButton)
            {
                this.label = OpLabel.CreateFLabel(text);
                this.myContainer.AddChild(this.label);
                LabelPlaceAtCenter(this.label, Vector2.zero, this.size);
            }
        }

        /// <summary>
        /// Click sound to play. In default this is <see cref="SoundID.MENU_Button_Standard_Button_Pressed"/>.
        /// </summary>
        public SoundID clickSound = SoundID.MENU_Button_Standard_Button_Pressed;

        /// <summary>
        /// Text inside the button
        /// </summary>
        public string text
        {
            get { return _text; }
            set
            {
                if (_text == value || IsImageButton) { return; }
                _text = value;
                this.label.text = LabelTest.TrimText(_text, this.size.x, true); OnChange();
            }
        }

        private string _text;

        /// <summary>
        /// The colour of Rectangle Edge and Text
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// The colour of Rectangle Fill
        /// </summary>
        public Color colorFill;

        protected readonly FLabel label;
        protected readonly DyeableRect rect, rectH;

        internal bool IsImageButton => this is OpSimpleImageButton;

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(24f, this.size.x), Mathf.Max(24f, this.size.y)); // Min Size
            base.OnChange();
            if (!IsImageButton) { LabelPlaceAtCenter(this.label, Vector2.zero, this.size); }
            this.rect.pos = this.pos;
            this.rect.size = this.size;
            this.rectH.pos = this.pos;
            this.rectH.size = this.size;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

            if (!IsImageButton)
            {
                this.label.color = this.bumpBehav.GetColor(this.colorEdge);
                LabelPlaceAtCenter(this.label, Vector2.zero, this.size);
            }
            if (greyedOut)
            {
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rectH.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                return;
            }

            this.rectH.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rectH.addSize = new Vector2(-2f, -2f) * this.bumpBehav.AddSize;
            float highlight = this.MouseOver && !this.held ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * this.bumpBehav.AddSize : 0f;
            for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = highlight; }

            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(6f, 6f) * this.bumpBehav.AddSize;
            this.rect.colorFill = this.colorFill;
        }

        public override void Update()
        {
            base.Update();
            this.rect.Update(); this.rectH.Update();
            if (greyedOut) { return; }

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
                            PlaySound(clickSound);
                            this.Signal();
                        }
                    }
                }
                else if (!Input.GetMouseButton(0))
                {
                    this.held = false;
                }
            }
            else
            {
                if (this.Focused())
                {
                    if (this.held)
                    {
                        if (!CtlrInput.jmp)
                        {
                            this.held = false;
                            PlaySound(clickSound);
                            this.Signal();
                        }
                    }
                    if (CtlrInput.jmp && !LastCtlrInput.jmp) { this.held = true; }
                }
            }
        }
    }
}