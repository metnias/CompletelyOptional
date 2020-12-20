using Menu;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Circular Hold Button which can also be used as ProgressButton
    /// </summary>
    public class OpHoldButton : UItrigger, SelectableUIelement
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
            if (!_init) { return; }
            circles = new FSprite[5];
            circles[0] = new FSprite("Futile_White");
            circles[0].shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"];
            circles[1] = new FSprite("Futile_White");
            circles[1].shader = menu.manager.rainWorld.Shaders["VectorCircle"];
            circles[2] = new FSprite("Futile_White");
            circles[2].shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"];
            circles[3] = new FSprite("Futile_White");
            circles[3].shader = menu.manager.rainWorld.Shaders["VectorCircle"];
            circles[4] = new FSprite("Futile_White");
            circles[4].shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"];
            for (int i = 0; i < circles.Length; i++) { myContainer.AddChild(circles[i]); circles[i].SetPosition(55f, 55f); }
            label = new FLabel(LabelTest.GetFont(false), text) { alignment = FLabelAlignment.Center };
            label.SetPosition(new Vector2(55.01f, 55.01f));
            myContainer.AddChild(label);
        }

        private FLabel label;
        private MenuMicrophone.MenuSoundLoop soundLoop;
        private float filled = 0f, pulse;
        private readonly float fillTime;
        // private bool fillInstant => fillTime == 0f;
        private bool hasSignalled = false;
        private int releaseCounter;
        private readonly FSprite[] circles;
        /// <summary>
        /// Text to be displayed
        /// </summary>
        public string text { get { return _text; } set { if (_text != value) { _text = value; OnChange(); } } }
        private string _text;

        /// <summary>
        /// Main Colour for Label and Button
        /// </summary>
        public Color color;

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            label.SetPosition(new Vector2(rad + 0.01f, rad + 0.01f));
            if (!isProgress) { label.text = text; }
            else { label.text = progress.ToString("N" + Mathf.Clamp(progressDeci, 0, 4).ToString()) + "%"; }
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            Color c = bumpBehav.GetColor(color);
            label.color = c;
            float r = rad + 8f * (bumpBehav.sizeBump + 0.5f * Mathf.Sin(bumpBehav.extraSizeBump * Mathf.PI)) * ((!held) ? 1f : (0.5f + 0.5f * Mathf.Sin(pulse * Mathf.PI * 2f))) + 0.5f;
            for (int i = 0; i < circles.Length; i++) { circles[i].scale = r / 8f; circles[i].SetPosition(rad, rad); }
            circles[0].color = new Color(0.0196078438f, 0f, Mathf.Lerp(0.3f, 0.6f, bumpBehav.col));
            circles[1].color = c;
            circles[1].alpha = 2f / r;
            circles[2].scale = (r + 10f) / 8f;
            circles[2].alpha = Mathf.Clamp01(!isProgress ? filled : progress / 100f);
            circles[2].color = Color.Lerp(Color.white, color, 0.7f);
            circles[3].color = Color.Lerp(c, DyeableRect.MidToDark(c), 0.5f);
            circles[3].scale = (r + 15f) / 8f;
            circles[3].alpha = 2f / (r + 15f);
            float dim = 0.5f + 0.5f * Mathf.Sin(bumpBehav.sin / 30f * Mathf.PI * 2f);
            dim *= bumpBehav.sizeBump;
            if (greyedOut) { dim = 0f; }
            circles[4].scale = (r - 8f * bumpBehav.sizeBump) / 8f;
            circles[4].alpha = 2f / (r - 8f * bumpBehav.sizeBump);
            circles[4].color = new Color(0f, 0f, dim);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
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
                soundLoop.loopVolume = Mathf.Lerp(soundLoop.loopVolume, 1f, 0.85f);
                soundLoop.loopPitch = Mathf.Lerp(0.3f, 1.5f, filled) - 0.15f * Mathf.Sin(pulse * Mathf.PI * 2f);
                pulse += filled / 20f;
            }
            else
            {
                if (soundLoop != null)
                {
                    soundLoop.loopVolume = Mathf.Max(0f, soundLoop.loopVolume - 0.125f);
                    if (soundLoop.loopVolume <= 0f) { soundLoop.Destroy(); soundLoop = null; }
                }
                pulse = 0f;
            }
            bool lastHeld = held;
            if (held) { held = Input.GetMouseButton(0); }
            else { held = MouseOver && Input.GetMouseButton(0); }
            bumpBehav.sizeBump = !held ? 0f : 1f;
            if (held)
            {
                bumpBehav.sin = pulse;
                filled = Custom.LerpAndTick(filled, 1f, 0.007f, 1f / fillTime);
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
            if (lastHeld && !hasSignalled && !OptionInterface.soundFilled)
            {
                OptionInterface.soundFill += 20;
                menu.PlaySound(SoundID.MENU_Security_Button_Init);
            }
            if (hasSignalled)
            {
                releaseCounter++;
                if (releaseCounter > 30)
                {
                    filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f);
                    if (filled < 0.5f) { hasSignalled = false; }
                }
                else { filled = 1f; }
            }
            else { filled = Custom.LerpAndTick(filled, 0f, 0.04f, 0.025f); }
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

        public override void Unload()
        {
            base.Unload();
            if (soundLoop != null) { soundLoop.Destroy(); }
        }
    }
}
