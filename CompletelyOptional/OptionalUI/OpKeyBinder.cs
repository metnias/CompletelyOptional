using BepInEx.Configuration;
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
    public class OpKeyBinder : UIconfig
    {
        /// <summary>
        /// Simple Key Binder. Value is the <see cref="KeyCode"/> in string form.
        /// </summary>
        /// <param name="pos">LeftBottom Position of this UI</param>
        /// <param name="size">Size; minimum size is 30x30.</param>
        /// <param name="rwMod">Your <see cref="RainWorldMod"/>. Use <see cref="OptionInterface.rwMod"/>.</param>
        /// <param name="cosmeticKey">Default <see cref="KeyCode"/> name. Set to empty to bind to <see cref="KeyCode.None"/> in default.</param>
        /// <param name="collisionCheck">Whether you will check the key is colliding with other <see cref="OpKeyBinder"/> or not</param>
        /// <param name="ctrlerNo">Which Controller this <see cref="OpKeyBinder"/> can bind</param>
        /// <exception cref="ElementFormatException">Thrown when defaultKey is null or empty</exception>
        public OpKeyBinder(ConfigEntry<KeyCode> config, Vector2 pos, Vector2 size, RainWorldMod rwMod, bool collisionCheck = true, BindController ctrlerNo = BindController.AnyController, string cosmeticKey = "") : base(config, pos, size, cosmeticKey)
        {
            if (string.IsNullOrEmpty(defaultValue)) { this._value = none; }
            this.modID = ConfigContainer.GenerateID(rwMod);
            this.controlKey = this.cosmetic ? "_" : "" + string.Concat(modID, "-", key);
            this._size = new Vector2(Mathf.Max(30f, size.x), Mathf.Max(30f, size.y));
            this.collisionCheck = !this.cosmetic && collisionCheck;
            this.bind = ctrlerNo;
            this.defaultValue = this.value;

            this.Initalize(defaultValue);
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
                        string anotherMod = Regex.Split(item.Key, "-")[0] + "-" + Regex.Split(item.Key, "-")[1];

                        if (modID != anotherMod)
                        {
                            ComOptPlugin.LogError(string.Concat("More than two mods are using same defaultKey!", Environment.NewLine,
                            "Conflicting Control: ", item.Key, " & ", controlKey, " (duplicate defalutKey: ", item.Value, ")"
                            ));

                            this._desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictOtherModDefault).Replace("<ModID>", anotherMod);
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

            this.rect = new DyeableRect(myContainer, Vector2.zero, this.size, true);
            this.rect.fillAlpha = 0.3f;

            this.label = FLabelCreate(defaultKey, true);
            FLabelPlaceAtCenter(this.label, Vector2.zero, this.size);
            this.myContainer.AddChild(this.label);

            this.sprite = new FSprite("GamepadIcon", true) { anchorX = 0f, anchorY = 0.5f, scale = 0.333f };
            this.myContainer.AddChild(sprite);

            this.Change();
        }

        protected internal override string DisplayDescription()
        {
            if (!string.IsNullOrEmpty(_desError)) { return _desError; }
            if (!string.IsNullOrEmpty(description)) { return description; }
            if (MenuMouseMode)
            {
                if (!held) return OptionalText.GetText(OptionalText.ID.OpKeyBinder_MouseSelectTuto);
                return OptionalText.GetText(!IsJoystick(value) ? OptionalText.ID.OpKeyBinder_MouseBindTuto : OptionalText.ID.OpKeyBinder_MouseJoystickBindTuto);
            }
            if (!held) return OptionalText.GetText(OptionalText.ID.OpKeyBinder_NonMouseSelectTuto);
            return OptionalText.GetText(!IsJoystick(value) ? OptionalText.ID.OpKeyBinder_NonMouseBindTuto : OptionalText.ID.OpKeyBinder_NonMouseJoystickBindTuto);
        }

        /// <summary>
        /// Whether you will check the key is colliding with other KeyBinder or not
        /// </summary>
        protected bool collisionCheck;

        internal static Dictionary<string, string> BoundKey;

        internal static void InitBoundKey()
        {
            // Load Illust
            MenuIllustration icon = new MenuIllustration(ModConfigMenu.instance, ConfigContainer.instance, string.Empty, "GamepadIcon", Vector2.zero, true, true);
            ConfigContainer.instance.subObjects.Add(icon);
            icon.sprite.isVisible = false;

            BoundKey = new Dictionary<string, string>();
            //Get Vanilla Keys
            for (int i = 0; i < ComOptPlugin.rw.options.controls.Length; i++)
            {
                Options.ControlSetup setup = ComOptPlugin.rw.options.controls[i];
                if (setup.preset == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    for (int p = 0; p < setup.keyboardKeys.Length; p++)
                    {
                        if (!BoundKey.ContainsValue(setup.keyboardKeys[p].ToString()))
                        { BoundKey.Add(string.Concat("Vanilla_", i.ToString(), "_", p.ToString()), setup.keyboardKeys[p].ToString()); }
                    }
                }
                else
                {
                    for (int p = 0; p < setup.gamePadButtons.Length; p++)
                    {
                        string key = setup.gamePadButtons[p].ToString();
                        if (key.Length > 9 && int.TryParse(key.Substring(8, 1), out int _))
                        { }
                        else
                        { key = key.Substring(0, 8) + i.ToString() + key.Substring(8); }
                        if (!BoundKey.ContainsValue(key))
                        { BoundKey.Add(string.Concat("Vanilla_", i.ToString(), "_", p.ToString()), key); }
                    }
                }
            }
        }

        private readonly string controlKey, modID;

        //clicked ==> input key (Mouse out ==> reset)
        /// <summary>
        /// Boundary DyeableRect
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// Text and Edge Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of DyeableRect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        /// <summary>
        /// Get <see cref="BindController"/> of vanilla with player, useful for <see cref="SetController(BindController)"/>
        /// </summary>
        /// <remarks>Example: <c>kbSetting.SetController(OpKeyBinder.GetControllerForPlayer(1));</c></remarks>
        /// <exception cref="ElementFormatException">Thrown when Player number is not 1 ~ 4</exception>
        /// <param name="player">Start from Player [1 to 4]</param>
        /// <returns></returns>
        public static BindController GetControllerForPlayer(int player)
        {
            if (player < 1 || player > 4) { throw new ElementFormatException("OpKeyBinder.GetControllerForPlayer threw error: Player number must be 1 ~ 4."); }
            return (BindController)ComOptPlugin.rw.options.controls[player - 1].gamePadNumber;
        }

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
        /// FLabel that displays <see cref="value"/>(Key)
        /// </summary>
        public FLabel label { get; private set; }

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

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.sprite.color = this.bumpBehav.GetColor(this.colorEdge);

            if (greyedOut)
            {
                this.rect.colorFill = this.bumpBehav.GetColor(this.colorFill);
                this.rect.GrafUpdate(timeStacker);
                if (string.IsNullOrEmpty(this._desError))
                { this.label.color = this.bumpBehav.GetColor(this.colorEdge); }
                else
                { this.label.color = new Color(0.5f, 0f, 0f); }
                return;
            }

            this.rect.colorFill = this.colorFill;
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.GrafUpdate(timeStacker);

            Color myColor = this.bumpBehav.GetColor(string.IsNullOrEmpty(this._desError) ? this.colorEdge : Color.red);
            if (this.Focused() || this.MouseOver)
            {
                myColor = Color.Lerp(myColor, Color.white, this.bumpBehav.Sin());
            }

            this.label.color = myColor;
        }

        private bool anyKeyDown; private bool lastAnyKeyDown;

        private string _desError;

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
                                    { this._desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictVanilla); }
                                    else
                                    { this._desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictOtherMod).Replace("<AnotherModID>", anotherMod); }
                                }
                                else
                                { this._desError = OptionalText.GetText(OptionalText.ID.OpKeyBinder_ErrorConflictCurrMod).Replace("<ConflictButton>", value); }
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
                    Change();
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

        protected internal override void NonMouseSetHeld(bool newHeld)
        {
            base.NonMouseSetHeld(newHeld);
            if (newHeld)
            {
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                this.label.text = "?";
            }
        }

        public override void Update()
        {
            base.Update();
            this.rect.Update();
            if (greyedOut) { return; }

            if (this.MouseOver && Input.GetMouseButton(0))
            { this.held = true; }

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
                    { PlaySound(SoundID.MENU_Error_Ping); this.held = false; return; }
                    this.SetController(newBind);
                    this.held = false;
                    FocusMoveDisallow(this);
                    return;
                }
                if (!this.lastAnyKeyDown && this.anyKeyDown)
                {
                    if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.JoystickButton6))
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
                                    PlaySound(SoundID.MENU_Error_Ping);
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
                PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                this.label.text = "?";
            }
        }

        protected internal override void Change()
        {
            this._size = new Vector2(Mathf.Max(30f, this.size.x), Mathf.Max(30f, this.size.y));
            base.Change();

            this.sprite.isVisible = IsJoystick(this.value);
            if (IsJoystick(this.value))
            {
                this.sprite.SetPosition(5f, this.size.y / 2f);
                this.label.text = this.value.Replace("Joystick", "");
                FLabelPlaceAtCenter(this.label, new Vector2(20f, 0), this.size - new Vector2(20f, 0));
            }
            else
            {
                this.label.text = this.value;
                FLabelPlaceAtCenter(this.label, Vector2.zero, this.size);
            }
            this.rect.size = this.size;
        }

        protected internal override void Unload()
        {
            base.Unload();
            BoundKey.Remove(this.controlKey);
        }
    }
}