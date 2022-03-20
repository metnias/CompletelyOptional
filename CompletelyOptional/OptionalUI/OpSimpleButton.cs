using CompletelyOptional;
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

            this.rect = new DyeableRect(this.myContainer, Vector2.zero, this.size, true);
            this.rectH = new DyeableRect(this.myContainer, Vector2.zero, this.size, false);
            if (!IsImageButton)
            {
                this.label = FLabelCreate(text);
                this.myContainer.AddChild(this.label);
                FLabelPlaceAtCenter(this.label, Vector2.zero, this.size);
            }
        }

        /// <summary>
        /// Whether this triggers <see cref="UItrigger.Signal"/> when it's kept pressed or not. See also <seealso cref="soundHold"/>.
        /// </summary>
        public bool canHold = false;

        /// <summary>
        /// How long this is held.
        /// </summary>
        protected int heldCounter = 0;

        /// <summary>
        /// A sound to play when this is held and released (<see cref="canHold"/> is false), or pressed initially (<see cref="canHold"/> is true). In default this is <see cref="SoundID.MENU_Button_Standard_Button_Pressed"/>.
        /// </summary>
        public SoundID soundClick = SoundID.MENU_Button_Standard_Button_Pressed;

        /// <summary>
        /// A sound to play when this is held for long to trigger <see cref="UItrigger.Signal"/>. See also <seealso cref="canHold"/>, <seealso cref="soundClick"/>.
        /// </summary>
        public SoundID soundHold = SoundID.MENU_Scroll_Tick;

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
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// The colour of Rectangle Fill
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        protected readonly FLabel label;
        protected readonly DyeableRect rect, rectH;

        internal bool IsImageButton => this is OpSimpleImageButton;

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(24f, this.size.x), Mathf.Max(24f, this.size.y)); // Min Size
            base.OnChange();
            if (!IsImageButton) { FLabelPlaceAtCenter(this.label, Vector2.zero, this.size); }
            this.rect.size = this.size;
            this.rectH.size = this.size;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

            if (!IsImageButton)
            {
                this.label.color = this.bumpBehav.GetColor(this.colorEdge);
            }
            if (greyedOut)
            {
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rectH.Hide();
                return;
            }
            this.rectH.Show();
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
                    {
                        if (!this.held && canHold)
                        {
                            PlaySound(soundClick);
                            this.Signal();
                        }
                        this.held = true; heldCounter++;
                    }
                    else
                    {
                        if (this.held)
                        {
                            this.held = false;
                            if (!canHold)
                            {
                                PlaySound(soundClick);
                                this.Signal();
                            }
                            heldCounter = 0;
                        }
                    }
                }
                else if (!Input.GetMouseButton(0))
                {
                    this.held = false;
                    heldCounter = 0;
                }
            }
            else
            {
                if (this.Focused())
                {
                    if (CtlrInput.jmp)
                    {
                        this.held = true; heldCounter++;
                        if (canHold && !LastCtlrInput.jmp)
                        {
                            PlaySound(soundClick);
                            this.Signal();
                        }
                    }
                    else
                    {
                        if (this.held)
                        {
                            this.held = false;
                            if (!canHold)
                            {
                                PlaySound(soundClick);
                                this.Signal();
                            }
                            heldCounter = 0;
                        }
                    }
                }
            }
            if (canHold && heldCounter > ModConfigMenu.DASinit && heldCounter % ModConfigMenu.DASdelay == 1)
            {
                PlaySound(soundHold);
                this.Signal();
                this.bumpBehav.sin = 0.5f;
            }
        }
    }
}