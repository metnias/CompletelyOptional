using CompletelyOptional;
using UnityEngine;
using BepInEx.Configuration;

namespace OptionalUI
{
    /// <summary>
    /// Configuable Settings. Every configuable value is tied to <see cref="UIconfig"/> and <see cref="key"/>.
    /// <para>Saving and loading will be handled automatically when this is added to the <see cref="OpTab"/>.</para>
    /// </summary>
    /// <remarks>Adding '_' before key (or leaving it empty) makes this <see cref="cosmetic"/>, preventing it to be saved.</remarks>
    public abstract class UIconfig : UIelement, ICanBeFocused
    {
        // Codes for Modders who only uses provided elements

        #region Shallow

        /// <summary>
        /// Rectangular <see cref="UIconfig"/>.
        /// </summary>
        /// <param name="config"><see cref="ConfigEntry{T}"/> which this UIconfig is connected to. Set this to null to make this cosmetic.</param>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size</param>
        /// <param name="cosmeticValue">Default Value for <see cref="cosmetic"/> which doesn't use <see cref="ConfigEntry{T}"/>. Gets overriden by config.DefaultValue if it's available.</param>
        public UIconfig(ConfigEntryBase config, Vector2 pos, Vector2 size, object cosmeticValue = null) : base(pos, size)
        {
            if (config == null) { this.cosmetic = true; cfgEntry = null; }
            else { this.cosmetic = false; cfgEntry = config; }
            if (!this.cosmetic) { this.defaultValue = GetStringValue(config.DefaultValue); }
            else { this.defaultValue = GetStringValue(cosmeticValue); }
            this._value = this.defaultValue;
            this.bumpBehav = new BumpBehaviour(this);
        }

        internal string GetStringValue(object obj)
        {
            if (obj == null) { return ""; }
            switch (obj.GetType().Name.ToLower())
            {
                case "bool":
                case "boolean":
                    return (bool)obj ? "true" : "false";

                case "uint":
                case "uint32":
                    return ((uint)obj).ToString();

                case "byte":
                case "int":
                case "int32":
                    return ((int)obj).ToString();

                case "float":
                case "single":
                    return ((float)obj).ToString();

                case "string":
                    return (string)obj;

                case "keycode":
                    return ((KeyCode)obj).ToString();

                default:
                    return !cosmetic ? cfgEntry.GetSerializedValue() : obj.ToString();
            }
        }

        /// <summary>
        /// Circular <see cref="UIconfig"/>.
        /// </summary>
        /// <param name="config"><see cref="ConfigEntry{T}"/> which this UIconfig is connected to. Set this to null to make this cosmetic.</param>
        /// <param name="pos">BottomLeft Position (NOT center!)</param>
        /// <param name="rad">Radius</param>
        /// <param name="cosmeticValue">Default Value for <see cref="cosmetic"/> which doesn't use <see cref="ConfigEntry{T}"/>. Gets overriden by config.DefaultValue if it's available.</param>
        public UIconfig(ConfigEntryBase config, Vector2 pos, float rad, object cosmeticValue = null) : base(pos, rad)
        {
            if (config == null) { this.cosmetic = true; cfgEntry = null; }
            else { this.cosmetic = false; cfgEntry = config; }
            if (!this.cosmetic) { this.defaultValue = GetStringValue(config.DefaultValue); }
            else { this.defaultValue = GetStringValue(cosmeticValue); }
            this._value = this.defaultValue;
            this.bumpBehav = new BumpBehaviour(this);
        }

        /// <summary>
        /// <see cref="ConfigEntry{T}"/> which this <see cref="UIconfig"/> is connected to
        /// </summary>
        public readonly ConfigEntryBase cfgEntry;

        /// <summary>
        /// This is set in ctor.
        /// <para>It'll be the default <see cref="value"/> of this <see cref="UIconfig"/> when your mod is installed first/configs are reset.</para>
        /// </summary>
        public string defaultValue { get; protected set; }

        public override void Reset()
        {
            base.Reset();
            this.value = this.defaultValue;
            this.held = false;
        }

        /// <summary>
        /// Set <see cref="key"/> to empty or start with '<c>_</c>' to make this <see cref="UIconfig"/> cosmetic and prevent saving
        /// </summary>
        public readonly bool cosmetic;

        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        /// <summary>
        /// Unique key for this <see cref="UIconfig"/>.
        /// </summary>
        public string key => cosmetic ? this.GetType().Name : cfgEntry.Definition.Key;

        /// <summary>
        /// If this is true, this <see cref="UIconfig"/> will be greyed out and can't be interacted.
        /// </summary>
        public bool greyedOut = false;

        /// <summary>
        /// If you want to change <see cref="value"/> directly without running <see cref="OnChange"/> and added to undo History.
        /// This is not recommended unless you know what you are doing.
        /// </summary>
        public void ForceValue(string newValue)
        {
            this._value = newValue;
        }

        /// <summary>
        /// Value in <see cref="string"/> form, which is how it is saved. Changing this will call <see cref="OnChange"/> automatically.
        /// When you're overriding this completely, make sure to call <see cref="ConfigContainer.NotifyConfigChange"/> in your override.
        /// See also <seealso cref="ForceValue"/>.
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
                    ConfigContainer.instance.allowFocusMove = false;
                    menu.cfgContainer.NotifyConfigChange(this, _value, value);
                    _value = value;
                    OnChange();
                }
            }
        }

        #endregion Shallow

        // Codes for modders who makes custom UIelement

        #region Deep

        /// <summary>
        /// Whether this is held or not.
        /// If this is true, other <see cref="UIelement"/> will be frozen.
        /// </summary>
        protected internal bool held
        {
            get { return _held; }
            set
            {
                if (_held != value)
                {
                    _held = value;
                    if (value) { menu.cfgContainer.FocusNewElement(this); }
                    else if (!this.Focused()) { return; }
                    ConfigContainer.holdElement = value;
                }
            }
        }

        /// <summary>
        /// Access to <see cref="UIconfig.value"/> without calling <see cref="OnChange"/>
        /// </summary>
        protected string _value;

        #region ICanBeFocused

        public virtual bool CurrentlyFocusableMouse => !this.greyedOut;

        public virtual bool CurrentlyFocusableNonMouse => true;

        public virtual bool GreyedOut => greyedOut;

        public virtual Rect FocusRect
        {
            get
            {
                Rect res = isRectangular ? new Rect(this.ScreenPos.x, this.ScreenPos.y, this.size.x, this.size.y)
                    : new Rect(this.ScreenPos.x, this.ScreenPos.y, this.rad * 2f, this.rad * 2f);
                if (tab != null) { res.x += tab.container.x; res.y += tab.container.y; }
                if (inScrollBox) { TrimFocusRectToScrollBox(ref res); }
                return res;
            }
        }

        #endregion ICanBeFocused

        public override void OnChange()
        {
            base.OnChange();
        }

        /// <summary>
        /// Update method that happens every frame.
        /// </summary>
        public override void Update()
        {
            base.Update();
            this.bumpBehav.Update();
            if (MenuMouseMode)
            { if (held && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(); } }
            else
            {
                if (held) { held = false; }
                if (this.Focused() && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(); }
            }
        }

        protected internal virtual bool CopyFromClipboard(string value)
        {
            try { this.value = value; this.held = false; return this.value == value; }
            catch { return false; }
        }

        protected internal virtual string CopyToClipboard()
        {
            this.held = false;
            return this.value;
        }

        protected internal override void Deactivate()
        {
            this.held = false;
            base.Deactivate();
        }

        #endregion Deep

        // Codes just for ConfigMachine

        #region Internal

        private bool _held = false;

        #endregion Internal
    }
}