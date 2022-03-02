using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpHoldButton : UItrigger, FocusableUIelement
    {
        /// <summary>
        /// Circular Hold Button which can also be used as ProgressButton
        /// </summary>
        /// <param name="pos">BottomLeft <see cref="UIelement.pos"/>; <see cref="UIelement.fixedRad"/> is 55f (110f in diameter)</param>
        /// <param name="signal"><see cref="UItrigger.signal"/></param>
        /// <param name="fillTime">How long do you need to hold to call Signal (set to 0f for instant)</param>
        /// <param name="displayText">Text to be displayed (overriden when it's ProgressButton mode)</param>
        public OpHoldButton(Vector2 pos, string signal, string displayText, float fillTime = 80f) : base(pos, 55f, signal)
        {
            this.fillTime = Mathf.Max(0f, fillTime);
            fixedRad = 55f;
            _text = displayText;
            color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

            circles = new FSprite[5];
            circles[0] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"] };
            circles[1] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircle"] };
            circles[2] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"] };
            circles[3] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircle"] };
            circles[4] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"] };
            for (int i = 0; i < circles.Length; i++) { myContainer.AddChild(circles[i]); circles[i].SetPosition(55f, 55f); }
            label = new FLabel(LabelTest.GetFont(false), text) { alignment = FLabelAlignment.Center };
            myContainer.AddChild(label);
            label.SetPosition(new Vector2(55.01f, 55.01f));
        }

        /// <summary>
        /// Rectangular Hold Button that looks similar with <see cref="OpSimpleButton"/>
        /// </summary>
        /// <param name="pos">BottomLeft <see cref="UIelement.pos"/>; <see cref="UIelement.fixedRad"/> is 55f (110f in diameter)</param>
        /// <param name="size">The size of this button. Minimum size is 24x24</param>
        /// <param name="signal"><see cref="UItrigger.signal"/></param>
        /// <param name="fillTime">How long do you need to hold to call Signal (set to 0f for instant)</param>
        /// <param name="displayText">Text to be displayed (overriden when it's ProgressButton mode)</param>
        public OpHoldButton(Vector2 pos, Vector2 size, string signal, string displayText, float fillTime = 80f) : base(pos, size, signal)
        {
            this.fillTime = Mathf.Max(0f, fillTime);
            this._size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
            _text = displayText;
            this.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

            this.rect = new DyeableRect(this.myContainer, this.pos, this.size, true);
            this.rectH = new DyeableRect(this.myContainer, this.pos, this.size, false);
            this.rectF = new FSprite("pixel", true)
            {
                anchorX = 0f,
                anchorY = 0f,
                x = rect.sprites[DyeableRect.MainFillSprite].x,
                y = rect.sprites[DyeableRect.MainFillSprite].y,
                scaleX = 9f,
                scaleY = rect.sprites[DyeableRect.MainFillSprite].scaleY,
                color = this.color
            };
            this.label = new FLabel(LabelTest.GetFont(false, !LabelTest.HasNonASCIIChars(text)), text);
            this.myContainer.AddChild(this.label);
            this.myContainer.AddChild(this.rectF);
        }

        private readonly FLabel label;
        private MenuMicrophone.MenuSoundLoop soundLoop;
        private float filled = 0f, pulse;
        private readonly float fillTime;

        // private bool fillInstant => fillTime == 0f;
        private bool hasSignalled = false;

        private int releaseCounter;
        private readonly FSprite[] circles;
        private readonly DyeableRect rect, rectH;
        private readonly FSprite rectF;

        /// <summary>
        /// Text to be displayed
        /// </summary>
        public string text
        { get { return _text; } set { if (_text != value) { _text = value; OnChange(); } } }

        private string _text;

        /// <summary>
        /// Main Colour for Label and Button
        /// </summary>
        public Color color;

        public override void OnChange()
        {
            base.OnChange();
            if (!isRectangular) { SetLabelPos(label, Vector2.zero, Vector2.one * rad * 2f); }
            else
            {
                this._size = new Vector2(Mathf.Max(24f, this.size.x), Mathf.Max(24f, this.size.y)); // Min Size
                SetLabelPos(label, Vector2.zero, this.size);
                this.rect.pos = this.pos;
                this.rect.size = this.size;
                this.rectH.pos = this.pos;
                this.rectH.size = this.size;
            }
            if (!isProgress) { label.text = text; }
            else { label.text = progress.ToString("N" + Custom.IntClamp(progressDeci, 0, 4).ToString()) + "%"; }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            float fill = Mathf.Clamp01(!isProgress ? filled : progress / 100f);
            Color c = bumpBehav.GetColor(color);
            label.color = c;
            if (!isRectangular)
            {
                // Identical to vanilla HoldButton
                float r = rad + 8f * (bumpBehav.sizeBump + 0.5f * Mathf.Sin(bumpBehav.extraSizeBump * Mathf.PI)) * ((!held) ? 1f : (0.5f + 0.5f * Mathf.Sin(pulse * Mathf.PI * 2f))) + 0.5f;
                for (int i = 0; i < circles.Length; i++) { circles[i].scale = r / 8f; circles[i].SetPosition(rad, rad); }
                circles[0].color = new Color(0.0196078438f, 0f, Mathf.Lerp(0.3f, 0.6f, bumpBehav.col));
                circles[1].color = c;
                circles[1].alpha = 2f / r;
                circles[2].scale = (r + 10f) / 8f;
                circles[2].alpha = fill;
                circles[2].color = Color.Lerp(Color.white, color, 0.7f);
                circles[3].color = Color.Lerp(c, MenuColorEffect.MidToDark(c), 0.5f);
                circles[3].scale = (r + 15f) / 8f;
                circles[3].alpha = 2f / (r + 15f);
                float dim = 0.5f + 0.5f * Mathf.Sin(bumpBehav.sin / 30f * Mathf.PI * 2f);
                dim *= bumpBehav.sizeBump;
                if (greyedOut) { dim = 0f; }
                circles[4].scale = (r - 8f * bumpBehav.sizeBump) / 8f;
                circles[4].alpha = 2f / (r - 8f * bumpBehav.sizeBump);
                circles[4].color = new Color(0f, 0f, dim);
            }
            else
            {
                // Similar to OpSimpleButton
                if (greyedOut)
                {
                    this.rect.colorEdge = this.bumpBehav.GetColor(this.color);
                    this.rect.colorFill = this.bumpBehav.GetColor(MenuColorEffect.MidToDark(this.color));
                    this.rectH.colorEdge = this.bumpBehav.GetColor(this.color);
                    this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);
                    return;
                }
                this.rectH.colorEdge = this.bumpBehav.GetColor(this.color);
                this.rectH.addSize = new Vector2(-2f, -2f) * this.bumpBehav.AddSize;
                float highlight = this.MouseOver && !this.held ? (0.5f + 0.5f * this.bumpBehav.Sin(10f)) * this.bumpBehav.AddSize : 0f;
                for (int j = 0; j < 8; j++) { this.rectH.sprites[j].alpha = highlight; }

                this.rect.colorEdge = this.bumpBehav.GetColor(this.color);
                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                this.rect.addSize = new Vector2(6f, 6f) * this.bumpBehav.AddSize;
                this.rect.colorFill = MenuColorEffect.MidToDark(this.color);
                this.rect.GrafUpdate(timeStacker); this.rectH.GrafUpdate(timeStacker);

                if (fill > 0f)
                {
                    // Fill rect's sprite from left to right with edge colour
                    // and change label text's color from edge to very dark
                    for (int i = 0; i < ((fill == 1f) ? 4 : 2); i++)
                    {
                        this.rect.sprites[this.rect.FillCornerSprite(i)].alpha = 1f;
                        this.rect.sprites[this.rect.FillCornerSprite(i)].color = this.rect.colorEdge;
                    }
                    this.rectF.x = pos.x + 7f;
                    this.rectF.y = pos.y;
                    this.rectF.scaleX = (size.x - 14f) * fill;
                    this.rectF.scaleY = size.y;
                    this.rectF.color = this.rect.colorEdge;
                    this.rect.sprites[DyeableRect.MainFillSprite].x = (size.x - 14f) * fill + 7f;
                    this.rect.sprites[DyeableRect.MainFillSprite].scaleX = (size.x - 14f) * (1f - fill);
                    this.rect.sprites[this.rect.FillSideSprite(1)].x = (size.x - 14f) * fill + 7f;
                    this.rect.sprites[this.rect.FillSideSprite(1)].scaleX = (size.x - 14f) * (1f - fill);
                    this.rect.sprites[this.rect.FillSideSprite(3)].x = (size.x - 14f) * fill + 7f;
                    this.rect.sprites[this.rect.FillSideSprite(3)].scaleX = (size.x - 14f) * (1f - fill);
                    label.color = Color.Lerp(c, MenuColorEffect.MidToVeryDark(c), fill);
                }
                else { this.rectF.scaleX = 0f; }
            }
        }

        public override void Update()
        {
            base.Update();
            if (isRectangular) { this.rect.Update(); this.rectH.Update(); }
            if (disabled || isProgress)
            {
                held = false;
                filled = 0f;
                pulse = 0f;
                bumpBehav.sizeBump = disabled ? 0f : 1f;
                bumpBehav.sin = 0f;
                return;
            }

            if (held)
            {
                if (soundLoop == null) { soundLoop = menu.PlayLoop(SoundID.MENU_Security_Button_LOOP, 0f, 0f, 1f, false); }
                soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f * frameMulti);
                soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, filled) - 0.15f * Mathf.Sin(pulse * Mathf.PI * 2f);
                pulse += frameMulti * filled / 20f;
            }
            else
            {
                if (soundLoop != null)
                {
                    soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f * frameMulti);
                    if (soundLoop.loopVolume <= 0f) { soundLoop.Destroy(); soundLoop = null; }
                }
                pulse = 0f;
            }
            bool lastHeld = held;
            if (MenuMouseMode)
            {
                if (held) { held = Input.GetMouseButton(0); }
                else { held = MouseOver && Input.GetMouseButton(0); }
            }
            else
            {
                held = CtlrInput.jmp;
            }
            bumpBehav.sizeBump = !held ? 0f : 1f;
            if (held)
            {
                bumpBehav.sin = pulse;
                filled = Custom.LerpAndTick(filled, 1f, 0.007f, frameMulti / fillTime);
                if (filled >= 1f && !hasSignalled)
                {
                    Signal();
                    hasSignalled = true;
                    menu.PlaySound(SoundID.MENU_Security_Button_Init);
                }
                releaseCounter = 0;
                return;
            }
            // Release
            if (lastHeld && !hasSignalled)
            {
                PlaySound(SoundID.MENU_Security_Button_Init);
            }
            if (hasSignalled)
            {
                releaseCounter++;
                if (releaseCounter > 30)
                {
                    filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f * frameMulti);
                    if (filled < 0.5f) { hasSignalled = false; }
                }
                else { filled = 1f; }
            }
            else { filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f * frameMulti); }
        }

        #region ProgressButton

        private bool isProgress = false;

        /// <summary>
        /// Progress of this ProgressButton in percentage (0f - 100f). See also <see cref="SetProgress(float)"/>
        /// </summary>
        public float progress { get; private set; }

        /// <summary>
        /// The number of numbers after decimal points. Maximum is 4.
        /// <para>Example: 0 => '50%', 1 => '50.0%', 2 => '50.00%'</para>
        /// </summary>
        public byte progressDeci = 2;

        /// <summary>
        /// Turn this button into ProgressButton mode, disabling interaction.
        /// </summary>
        /// <param name="percentage">0f - 100f to set progress (negative number to disable ProgressButtom mode)</param>
        public void SetProgress(float percentage)
        {
            if (percentage < 0f) { isProgress = false; progress = 0f; return; }
            isProgress = true;
            progress = Mathf.Clamp(percentage, 0f, 100f);
            OnChange();
        }

        #endregion ProgressButton

        protected internal override void Unload()
        {
            base.Unload();
            if (soundLoop != null) { soundLoop.Destroy(); }
        }
    }
}