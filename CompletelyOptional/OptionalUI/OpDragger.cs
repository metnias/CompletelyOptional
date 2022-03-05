using CompletelyOptional;
using OptionalUI.ValueTypes;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Dragger to adjust int value easily.
    /// </summary>
    public class OpDragger : UIconfig, IValueInt
    {
        /// <summary>
        /// Dragger to adjust int value in a cramped space. The fixedSize is 24x24.
        /// </summary>
        /// <param name="pos">BottomLeft</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultInt">default value</param>
        public OpDragger(Vector2 pos, string key, int defaultInt = 0) : base(pos, new Vector2(24f, 24f), key, defaultInt.ToString())
        {
            this.fixedSize = new Vector2(24f, 24f);

            this.colorText = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;

            this.rect = new DyeableRect(myContainer, this.pos, this.size, true);
            this.label = OpLabel.CreateFLabel(defaultInt.ToString());
            PlaceLabelAtCenter(this.label, new Vector2(0f, 2f), new Vector2(24f, 20f));

            this.min = 0; this._min = 0; this.max = 99; this._max = 99;
            this.description = InternalTranslator.Translate("Hold your mouse button and Drag up/down to adjust value");

            this.useCT = false;
        }

        /// <summary>
        /// Dragger to adjust int value in a cramped space. The fixedSize is 24x24.
        /// </summary>
        /// <param name="posX">Left position.</param>
        /// <param name="posY">Bottom position.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultInt">default value</param>
        public OpDragger(float posX, float posY, string key, int defaultInt = 0) : this(new Vector2(posX, posY), key, defaultInt)
        { }

        /// <summary>
        /// Minimum value. default = 0
        /// </summary>
        public int min;

        /// <summary>
        /// Maximum value. default = 99
        /// </summary>
        public int max;

        private int _min, _max;

        private string[] customText; private bool useCT;

        /// <summary>
        /// Set custom text to display for each index.
        /// </summary>
        /// <param name="newTexts">strings to display from min to max</param>
        public void SetCustomText(params string[] newTexts)
        {
            useCT = true;
            customText = new string[max - min + 1];
            for (int i = 0; i < customText.Length; i++) { customText[i] = (min + i).ToString(); }
            for (int j = 0; j < newTexts.Length; j++)
            {
                if (j >= customText.Length) { break; }
                if (string.IsNullOrEmpty(newTexts[j])) { continue; }
                customText[j] = newTexts[j];
            }
        }

        /// <summary>
        /// Boundary DyeableRect
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// FLabel
        /// </summary>
        public FLabel label;

        /// <summary>
        /// Text Colour. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorText;

        /// <summary>
        /// Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (greyedOut)
            {
                this.label.color = this.bumpBehav.GetColor(this.colorText);
                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                return;
            }

            this.rect.colorFill = this.colorFill;
            this.greyFade = Custom.LerpAndTick(this.greyFade, (!CompletelyOptional.ConfigMenu.freezeMenu || this.held) ? 0f : 1f, 0.05f, 0.025f / frameMulti);

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;

            this.label.color = this.bumpBehav.GetColor(this.colorText);
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
        }

        private float greyFade;

        private float savMouse; private int savValue;

        string IValueType.valueString { get => this.value; set => this.value = value; }

        public override void Update()
        {
            base.Update();
            if (greyedOut) { return; }
            if (_min != min || _max != max) { OnChange(); }

            if (this.held)
            {
                if (Input.GetMouseButton(0))
                { this.SetValueInt(Custom.IntClamp(this.savValue + Mathf.FloorToInt((Input.mousePosition.y - this.savMouse) / 10f), this.min, this.max)); }
                else
                { this.held = false; }
            }
            else if (!this.held && this.menu.manager.menuesMouseMode && this.MouseOver)
            {
                if (Input.GetMouseButton(0))
                {
                    this.held = true;
                    this.savMouse = Input.mousePosition.y;
                    this.savValue = this.GetValueInt();
                    PlaySound(SoundID.MENU_First_Scroll_Tick);
                }
                else if (this.menu.mouseScrollWheelMovement != 0)
                {
                    int num = this.GetValueInt() - (int)Mathf.Sign(this.menu.mouseScrollWheelMovement);
                    num = Custom.IntClamp(num, this.min, this.max);
                    if (num != this.GetValueInt())
                    {
                        this.bumpBehav.flash = 1f;
                        PlaySound(SoundID.MENU_Scroll_Tick);
                        this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
                        this.SetValueInt(num);
                    }
                }
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(24f, 24f);
            base.OnChange();
            if (MouseOver || held)
            {
                PlaySound(SoundID.MENU_Scroll_Tick);
                this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
                this.bumpBehav.flash = Mathf.Min(1f, this.bumpBehav.flash + 0.5f);
            }
            if (_min != min || _max != max)
            {
                if (min > _max)
                {
                    min = _min;
                    Debug.LogError(string.Concat("You set new minimum of OpDragger(key: ", this.key, ") larger than its maximum! (", min.ToString(), ">", _max.ToString(), ") Disregarding new change."));
                }
                else if (max < _min)
                {
                    max = _max;
                    Debug.LogError(string.Concat("You set new maximum of OpDragger(key: ", this.key, ") larger than its minimum! (", min.ToString(), ">", _max.ToString(), ") Disregarding new change."));
                }
                if (useCT)
                {
                    string[] temp = new string[max - min + 1];
                    for (int i = 0; i < temp.Length; i++) { temp[i] = (min + i).ToString(); }
                    for (int j = 0; j < customText.Length; j++)
                    {
                        if (j >= temp.Length) { break; }
                        if (int.TryParse(customText[j], out int k)) { if (k == _min + j) { continue; } }
                        temp[j] = customText[j];
                    }
                    customText = temp;
                }
                _min = min;
                _max = max;
            }

            if (useCT) { this.label.text = customText[this.GetValueInt() - this.min]; }
            else { this.label.text = value; }

            this.rect.pos = this.pos;
            PlaceLabelAtCenter(this.label, new Vector2(0f, 2f), new Vector2(24f, 20f));
        }
    }
}