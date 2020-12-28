using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Configuable Settings. Every configuable value is tied to <see cref="UIconfig"/> and <see cref="key"/>.
    /// <para>Saving and loading will be handled automatically when this is added to the <see cref="OpTab"/>.</para>
    /// </summary>
    /// <remarks>Adding '_' before key (or leaving it empty) makes this <see cref="cosmetic"/>, preventing it to be saved.</remarks>
    public class UIconfig : UIelement
    {
        /// <summary>
        /// Rectangular <see cref="UIconfig"/>.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size</param>
        /// <param name="key">Key: this must be unique</param>
        /// <param name="defaultValue">Default Value</param>
        public UIconfig(Vector2 pos, Vector2 size, string key, string defaultValue = "") : base(pos, size)
        {
            if (string.IsNullOrEmpty(key)) { this.cosmetic = true; this.key = "_"; }
            else if (key.Substring(0, 1) == "_") { this.cosmetic = true; this.key = key; }
            else { this.cosmetic = false; this.key = key; }
            this._value = defaultValue;
            this.defaultValue = this._value;
            this.greyedOut = false;
            this._held = false;
            this.bumpBehav = new BumpBehaviour(this);
        }

        /// <summary>
        /// Circular <see cref="UIconfig"/>.
        /// </summary>
        /// <param name="pos">BottomLeft Position (NOT center!)</param>
        /// <param name="rad">Radius</param>
        /// <param name="key">Key: this must be unique</param>
        /// <param name="defaultValue">Default Value</param>
        public UIconfig(Vector2 pos, float rad, string key, string defaultValue = "") : base(pos, rad)
        {
            if (string.IsNullOrEmpty(key)) { this.cosmetic = true; this.key = "_"; }
            else if (key.Substring(0, 1) == "_") { this.cosmetic = true; this.key = key; }
            else { this.cosmetic = false; this.key = key; }
            this._value = defaultValue;
            this.defaultValue = this._value;
            this.greyedOut = false;
            this._held = false;
            this.bumpBehav = new BumpBehaviour(this);
        }

        /// <summary>
        /// This is set in ctor.
        /// <para>It'll be the default <see cref="value"/> of this <see cref="UIconfig"/> when your mod is installed first/configs are reset.</para>
        /// </summary>
        public string defaultValue { get; internal set; }

        public override void Reset()
        {
            base.Reset();
            this.value = this.defaultValue;
            this.held = false;
        }

        /// <summary>
        /// Set <see cref="key"/> to empty or start with '_' to make this <see cref="UIconfig"/> cosmetic and prevent saving
        /// </summary>
        public readonly bool cosmetic;

        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        /// <summary>
        /// Whether this is held or not.
        /// If this is true, other <see cref="UIelement"/> will be frozen.
        /// </summary>
        public bool held
        {
            get { return _held; }
            set
            {
                if (_held != value)
                {
                    _held = value;
                    ConfigMenu.freezeMenu = value;
                }
            }
        }

        private bool _held;

        /// <summary>
        /// Unique key for this <see cref="UIconfig"/>
        /// </summary>
        public readonly string key;

        /// <summary>
        /// If this is true, this <see cref="UIconfig"/> will be greyed out and can't be interacted.
        /// </summary>
        public bool greyedOut;

        /// <summary>
        /// Either this is <see cref="greyedOut"/> or <see cref="UIelement.isHidden"/>.
        /// Prevents its interaction in <see cref="Update(float)"/>.
        /// </summary>
        internal bool disabled => greyedOut || isHidden;

        /// <summary>
        /// If you want to change value directly without running <see cref="OnChange"/>.
        /// This is not recommended unless you know what you are doing.
        /// </summary>
        public void ForceValue(string newValue)
        {
            this._value = newValue;
        }

        private string _value;

        /// <summary>
        /// Value in <see cref="string"/> form, which is how it is saved. Changing this will call <see cref="OnChange"/> automatically.
        /// </summary>
        public virtual string value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    if (_init) { OnChange(); }
                }
            }
        }

        /// <summary>
        /// Value in <see cref="int"/> form
        /// </summary>
        public int valueInt
        {
            get { return int.TryParse(value, out int i) ? i : 0; }
            set { this.value = value.ToString(); }
        }

        /// <summary>
        /// Value in <see cref="float"/> form
        /// </summary>
        public float valueFloat
        {
            get { return float.TryParse(value, out float d) ? d : 0f; }
            set { this.value = value.ToString(); }
        }

        /// <summary>
        /// Value in <see cref="bool"/> form
        /// </summary>
        public bool valueBool
        {
            set { this.value = value ? "true" : "false"; }
            get
            {
                if (this.value == "true") { return true; }
                else { return false; }
            }
        }

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            if (!cosmetic)
            {
                OptionScript.configChanged = true;
                (menu as ConfigMenu).saveButton.menuLabel.text = InternalTranslator.Translate("APPLY");
            }
        }

        /// <summary>
        /// Separates Graphical update for code-visiblilty.
        /// </summary>
        /// <param name="dt">deltaTime</param>
        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
        }

        /// <summary>
        /// Update method that happens every frame.
        /// </summary>
        /// <param name="dt">deltaTime</param>
        public override void Update(float dt)
        {
            if (!_init) { return; }
            base.Update(dt);
            this.bumpBehav.Update(dt);
            if (this.held && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(dt); }
            if (showDesc && !this.greyedOut) { ConfigMenu.description = this.description; }
        }

        public virtual bool CopyFromClipboard(string value)
        {
            try { this.value = value; this.held = false; return this.value == value; }
            catch { return false; }
        }

        public virtual string CopyToClipboard()
        {
            this.held = false;
            return this.value;
        }

        public override void Hide()
        {
            this.held = false;
            base.Hide();
        }
    }
}
