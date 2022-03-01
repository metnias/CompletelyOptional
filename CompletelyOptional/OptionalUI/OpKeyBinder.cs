using CompletelyOptional;
using Menu;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simple Key Binder
    /// </summary>
    public class OpKeyBinder : UIconfig, FocusableUIelement
    {
        /// <summary>
        /// Simple Key Binder. Value is the <see cref="KeyCode"/> in string form.
        /// </summary>
        /// <param name="pos">LeftBottom Position of this UI</param>
        /// <param name="size">Size; minimum size is 30x30.</param>
        /// <param name="modID">Your ModID</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultKey">Default <see cref="KeyCode"/> name. Set to empty to bind to <see cref="KeyCode.None"/> in default.</param>
        /// <param name="collisionCheck">Whether you will check the key is colliding with other <see cref="OpKeyBinder"/> or not</param>
        /// <param name="ctrlerNo">Which Controller this <see cref="OpKeyBinder"/> can bind</param>
        /// <exception cref="ElementFormatException">Thrown when defaultKey is null or empty</exception>
        public OpKeyBinder(Vector2 pos, Vector2 size, string modID, string key, string defaultKey, bool collisionCheck = true, BindController ctrlerNo = BindController.AnyController) : base(pos, size, key, defaultKey)
        {
            // if (string.IsNullOrEmpty(defaultKey)) { throw new ElementFormatException(this, "OpKeyBinderNull: defaultKey for this keyBinder is null.", key); }
            if (string.IsNullOrEmpty(defaultKey)) { this._value = none; }
            this.controlKey = !this.cosmetic ? string.Concat(modID, "_", key) : "_";
            this.modID = modID;
            this._size = new Vector2(Mathf.Max(30f, size.x), Mathf.Max(30f, size.y));
            this.collisionCheck = !this.cosmetic && collisionCheck;
            this.bind = ctrlerNo;
            this.defaultValue = this.value;

            if (!_init) { return; }
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.Initalize(defaultKey);
        }

        private static readonly string none = (KeyCode.None).ToString();

        private void Initalize(string defaultKey)
        {
            if (this.collisionCheck && BoundKey.ContainsValue(defaultKey) && defaultKey != none)
            { //defaultKey Conflict!
                //string anotherControlKey;
                foreach (KeyValuePair<string, string> item in BoundKey)
                {
                    if (item.Value == defaultKey)
                    {
                        string anotherMod = Regex.Split(item.Key, "_")[0];

                        if (modID != anotherMod)
                        {
                            Debug.LogError(string.Concat("More than two mods are using same defaultKey!", Environment.NewLine,
                            "Conflicting Control: ", item.Key, " & ", controlKey, " (duplicate defalutKey: ", item.Value, ")"
                            ));

                            this._desError = string.Concat("Conflicting defaultKey with Mod ", anotherMod);
                            break;
                        }
                        throw new ElementFormatException(this, "You are using duplicated defaultKey for OpKeyBinders!", key);
                    }
                }
            }
            else { this._desError = ""; }
            if (this.collisionCheck)
            {
                BoundKey.Add(this.controlKey, defaultKey);
            }
            this._description = "";

            this.rect = new DyeableRect(menu, owner, this.pos, this.size, true);
            this.subObjects.Add(this.rect);
            this.rect.fillAlpha = 0.3f;

            this.label = new MenuLabel(menu, owner, defaultKey, this.pos, this.size, true);
            this.subObjects.Add(this.label);

            this.sprite = new FSprite("GamepadIcon", true) { anchorX = 0f, anchorY = 0.5f, scale = 0.333f };
            this.myContainer.AddChild(sprite);

            this.OnChange();
        }

        /// <summary>
        /// Whether you will check the key is colliding with other KeyBinder or not
        /// </summary>
        protected bool collisionCheck;

        bool FocusableUIelement.IsMouseOverMe { get { return !this.held && this.MouseOver; } }

        bool FocusableUIelement.CurrentlyFocusableMouse { get { return !this.disabled; } }

        bool FocusableUIelement.CurrentlyFocusableNonMouse { get { return !this.disabled; } }

        private static Dictionary<string, string> BoundKey
        {
            get { return ConfigMenu.BoundKey; }
            set { ConfigMenu.BoundKey = value; }
        }

        private readonly string controlKey; private readonly string modID;

        //clicked ==> input key (Mouse out ==> reset)
        /// <summary>
        /// Boundary DyeableRect
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// Text and Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        /// <summary>
        /// Get <see cref="BindController"/> of vanilla with player, useful for <see cref="SetController(BindController)"/>
        /// </summary>
        /// <remarks>Example: <c>kbSetting.SetController(OpKeyBinder.GetControllerForPlayer(1));</c></remarks>
        /// <exception cref="ElementFormatException">Thrown when Player number is not 1 ~ 4</exception>
        /// <param name="player">Start from Player [1 to 4]</param>
        /// <returns></returns>
        public static BindController GetControllerForPlayer(int player)
        {
            if (player < 1 || player > 4) { throw new ElementFormatException(string.Concat("OpKeyBinder.GetControllerForPlayer threw error: Player number must be 1 ~ 4.")); }
            return (BindController)OptionScript.rw.options.controls[player - 1].gamePadNumber;
        }

        /// <summary>
        /// Abandoned because this only created confusion. Use <see cref="GetControllerForPlayer(int)"/>.
        /// </summary>
        [Obsolete]
        public int player;

        /// <summary>
        /// If you want to convert <see cref="string"/> to <see cref="KeyCode"/> but you are too lazy to code that by yourself.
        /// </summary>
        /// <remarks>Example: <c>if (Input.GetKey(StringToKeyCode(OptionInterface.config[MyModKey])))</c></remarks>
        /// <param name="str">KeyCode in string</param>
        /// <returns>KeyCode</returns>
        public static KeyCode StringToKeyCode(string str)
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), str);
        }

        /// <summary>
        /// This displays Key
        /// </summary>
        public MenuLabel label;

        private FSprite sprite;

        public enum BindController
        {
            /// <summary>
            /// Accept input from any controller connected
            /// </summary>
            AnyController = 0,

            /// <summary>
            /// Accept input from the first controller (See Vanilla config menu for reference)
            /// </summary>
            Controller1 = 1,

            /// <summary>
            /// Accept input from the second controller (See Vanilla config menu for reference)
            /// </summary>
            Controller2 = 2,

            /// <summary>
            /// Accept input from the third controller (See Vanilla config menu for reference)
            /// </summary>
            Controller3 = 3,

            /// <summary>
            /// Accept input from the fourth controller (See Vanilla config menu for reference)
            /// </summary>
            Controller4 = 4
        };

        private BindController bind;

        /// <summary>
        /// Set new Controller number for this OpKeyBinder. See also <seealso cref="GetControllerForPlayer(int)"/>
        /// </summary>
        /// <param name="controller">New <see cref="BindController"/></param>
        public void SetController(BindController controller)
        {
            // if (this.bind == controller) { return; }
            string newValue = ChangeBind(this.value, this.bind, controller);
            this.bind = controller;
            this.value = newValue;
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.sprite.color = this.bumpBehav.GetColor(this.colorEdge);

            if (greyedOut)
            {
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                if (string.IsNullOrEmpty(this._desError))
                { this.label.label.color = this.bumpBehav.GetColor(this.colorEdge); }
                else
                { this.label.label.color = new Color(0.5f, 0f, 0f); }
                return;
            }

            this.rect.colorFill = this.colorFill;
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;

            Color myColor = this.bumpBehav.GetColor(string.IsNullOrEmpty(this._desError) ? this.colorEdge : Color.red);
            if (this.MouseOver)
            {
                ConfigMenu.description = this.GetDescription();
                myColor = Color.Lerp(myColor, Color.white, this.bumpBehav.Sin());
            }

            this.label.label.color = myColor;
        }

        private bool anyKeyDown; private bool lastAnyKeyDown;

        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(_desError))
            { return _desError; }
            if (!string.IsNullOrEmpty(_description))
            { return _description; }
            if (IsJoystick(this.value)) { return InternalTranslator.Translate("Click this and Press any button to bind (Ctrl + No to set controller number)"); }
            else { return InternalTranslator.Translate("Click this and Press any key/button to bind (Esc to unbind)"); }
        }

        public void SetDescription(string value)
        { _description = value; }

        private string _description;
        private string _desError;

        /// <summary>
        /// Use <see cref="SetDescription(string)"/> and <see cref="GetDescription"/> instead.
        /// </summary>
        [Obsolete]
        public new string description
        {
            get { return GetDescription(); }
            set { SetDescription(value); }
        }

        public override string value
        {
            get { return base.value; }
            set
            {
                if (base.value != value)
                {
                    if (!this.collisionCheck || !BoundKey.ContainsValue(value) || value == none)
                    { this._desError = ""; }
                    else
                    {
                        foreach (KeyValuePair<string, string> item in BoundKey)
                        {
                            if (item.Value == value)
                            {
                                if (item.Key == this.controlKey) { break; }
                                string anotherMod = Regex.Split(item.Key, "_")[0];
                                if (anotherMod != this.modID)
                                {
                                    if (anotherMod == "Vanilla")
                                    { this._desError = InternalTranslator.Translate("Conflicting button with Vanilla Options"); }
                                    else
                                    { this._desError = InternalTranslator.Translate("Conflicting button with Mod named <AnotherModID>").Replace("<AnotherModID>", anotherMod); }
                                }
                                else
                                { this._desError = InternalTranslator.Translate("<ConflictButton> button is already in use").Replace("<ConflictButton>", value); }
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(this._desError))
                    {
                        this._value = value;
                        menu.PlaySound(SoundID.MENU_Button_Successfully_Assigned);
                        if (this.collisionCheck)
                        {
                            BoundKey.Remove(controlKey);
                            BoundKey.Add(controlKey, value);
                        }
                    }
                    else
                    { menu.PlaySound(SoundID.MENU_Error_Ping); }
                    OnChange();
                }
            }
        }

        private static bool IsJoystick(string keyCode) => keyCode.Length > 8 && keyCode.ToLower().Substring(0, 8) == "joystick";

        private static string ChangeBind(string oldKey, BindController oldBind, BindController newBind)
        {
            if (!IsJoystick(oldKey)) { return oldKey; }
            string btn = oldKey.Substring(oldBind != BindController.AnyController ? 9 : 8);
            int b = (int)newBind;
            return "Joystick" + (newBind != BindController.AnyController ? b.ToString() : "") + btn;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (disabled) { return; }

            if (this.MouseOver && Input.GetMouseButton(0))
            {
                this.held = true;
            }

            this.lastAnyKeyDown = this.anyKeyDown;
            this.anyKeyDown = Input.anyKey;

            if (this.held)
            {
                if (IsJoystick(this.value) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                {
                    bool flag = false; BindController newBind = BindController.AnyController;
                    if (Input.GetKey("`") || Input.GetKey("0") || Input.GetKey(KeyCode.Escape)) { flag = true; newBind = BindController.AnyController; }
                    else if (Input.GetKey("1")) { flag = true; newBind = BindController.Controller1; }
                    else if (Input.GetKey("2")) { flag = true; newBind = BindController.Controller2; }
                    else if (Input.GetKey("3")) { flag = true; newBind = BindController.Controller3; }
                    else if (Input.GetKey("4")) { flag = true; newBind = BindController.Controller4; }
                    if (!flag) { return; }
                    if (this.bind == newBind)
                    { menu.PlaySound(SoundID.MENU_Error_Ping); this.held = false; return; }
                    this.SetController(newBind);
                    this.held = false;
                    return;
                }
                if (!this.lastAnyKeyDown && this.anyKeyDown)
                {
                    if (Input.GetKey(KeyCode.Escape))
                    {
                        this.value = none; this.held = false; return;
                    }
                    foreach (object obj in Enum.GetValues(typeof(KeyCode)))
                    {
                        KeyCode keyCode = (KeyCode)((int)obj);
                        if (Input.GetKey(keyCode))
                        {
                            string text = keyCode.ToString();
                            if (text.Length > 4 && text.Substring(0, 5) == "Mouse")
                            {
                                if (!this.MouseOver)
                                {
                                    menu.PlaySound(SoundID.MENU_Error_Ping);
                                    this.held = false;
                                }
                                return;
                            }
                            if (this.value != text)
                            {
                                if (IsJoystick(text))
                                {
                                    if (this.bind != BindController.AnyController)
                                    {
                                        int b = (int)this.bind;
                                        text = text.Substring(0, 8) + b.ToString() + text.Substring(8);
                                    }
                                }

                                this.value = text;
                                this.held = false;
                            }
                            break;
                        }
                    }
                }
            }
            else if (!this.held && this.MouseOver && Input.GetMouseButton(0))
            {
                this.held = true;
                menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                this.label.label.text = "?";
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(30f, this.size.x), Mathf.Max(30f, this.size.y));
            base.OnChange();
            if (!_init) { return; }
            this.sprite.isVisible = IsJoystick(this.value);
            if (IsJoystick(this.value))
            {
                this.sprite.SetPosition(5f, this.size.y / 2f);
                this.label.text = this.value.Replace("Joystick", "");
                this.label.pos = new Vector2(this.pos.x + 20f, this.pos.y);
                this.label.size = new Vector2(this.size.x - 20f, this.size.y);
            }
            else
            {
                this.label.text = this.value;
                this.label.pos = this.pos;
                this.label.size = this.size;
            }
            this.rect.pos = this.pos;
            this.rect.size = this.size;
        }

        public override void Hide()
        {
            base.Hide();
            this.rect.Hide();
            this.label.label.isVisible = false;
            this.sprite.isVisible = false;
        }

        public override void Show()
        {
            base.Show();
            this.rect.Show();
            this.label.label.isVisible = true;
            this.sprite.isVisible = IsJoystick(this.value);
        }

        public override void Unload()
        {
            base.Unload();
            BoundKey.Remove(this.controlKey);
        }
    }
}