using CompletelyOptional;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using BepInEx;

namespace OptionalUI
{
    /// <summary>
    /// To Interact with Config Machine, check if your project has this class
    /// </summary>
    public partial class OptionInterface
    {
        /// <summary>
        /// OptionInterface for Partiality Mod.
        /// Create <c>public static [YourOIclass] LoadOI()</c> in your <see cref="PartialityMod"/>.
        /// ConfigMachine will load your OI after IntroRoll.
        /// </summary>
        /// <remarks>Example:
        /// <code>
        /// public static MyOptionInterface LoadOI()
        /// {
        ///     return new MyOptionInterface(mod: MyMod.instance);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="mod">Your Partiality mod</param>
        public OptionInterface(PartialityMod mod)
        {
#pragma warning disable CS0612
            if (mod != null) { this.mod = mod; this.rwMod = new RainWorldMod(mod); }
#pragma warning restore CS0612
            else { this.rwMod = new RainWorldMod(); }
            this.rawConfig = rawConfigDef;
        }

        /// <summary>
        /// Custom OptionInterface for BepInEx Plugin.
        /// Create <c>public static [YourOIclass] LoadOI()</c> in your <see cref="BaseUnityPlugin"/>.
        /// ConfigMachine will load your OI after IntroRoll.
        /// </summary>
        /// <remarks>Example:
        /// <code>
        /// public static MyOptionInterface LoadOI()
        /// {
        ///     return new MyOptionInterface(plugin: MyPlugin.instance);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="plugin">Your BepInEx plugin</param>
        public OptionInterface(BaseUnityPlugin plugin)
        {
            if (plugin != null) { this.rwMod = new RainWorldMod(plugin); }
            else { this.rwMod = new RainWorldMod(); }
            this.rawConfig = rawConfigDef;
        }

        /// <summary>
        /// This ctor is only used by Config Machine internally.
        /// </summary>
        internal OptionInterface(RainWorldMod rwMod)
        {
            this.rwMod = rwMod;
            this.rawConfig = rawConfigDef;
        }

        /// <summary>
        /// The less the value is, the more likely to get ignored by ConfigMachine. The range is -1 ~ 2.
        /// </summary>
        internal int GetPriority()
        {
            // if (OptionScript.blackList.Contains(rwMod.ModID)) { return -1; }
            if (this is UnconfiguableOI uoi)
            {
                if (uoi.reason == UnconfiguableOI.Reason.TooManyMod) { return (int)Priority.ModList; }
                return uoi.reason != UnconfiguableOI.Reason.NoInterface ? (int)Priority.Error : (int)Priority.NoInterface;
            }
            else if (this is GeneratedOI goi)
            {
                return goi.mode == GeneratedOI.GenMode.ModderCall ? (int)Priority.NoInterface : (int)Priority.Configuable;
            }
            else if (this is InternalTestOI) { return (int)Priority.InternalTest; }
            return this.Configuable() ? (int)Priority.Configuable : (int)Priority.Inconfiguable;
        }

        internal enum Priority : int
        {
            ModList = 3,
            InternalTest = 2,
            Error = 2,
            Configuable = 1,
            Inconfiguable = 0,
            NoInterface = -1
        }

        /// <summary>
        /// Is this ConfigScreen or not.
        /// If you are editing <see cref="Menu.MenuObject"/> in <see cref="UIelement"/>s,
        /// make sure that those codes don't run when this is false.
        /// Or it will throw <see cref="NullReferenceException"/>.
        /// </summary>
        public static bool IsConfigScreen
        {
            get { return OptionScript.init; }
        }

        /// <summary>
        /// How much the Sound Engine is full. This exists to prevent cramping Unity Sound thingly and causing sound artifacts. See also <seealso cref="soundFilled"/>
        /// </summary>
        /// <remarks>Example: <code>
        /// if (!soundFilled)
        /// {
        ///     soundFill += 5;
        ///     menu.PlaySound(SoundID.MENU_Scroll_Tick);
        /// }
        /// </code>
        /// </remarks>
        public static int soundFill
        {
            get
            {
                return OptionScript.soundFill;
            }
            set
            {
                if (OptionScript.soundFill < value)
                { OptionScript.soundFill += UIelement.FrameMultiply(value - OptionScript.soundFill); }
                else
                { OptionScript.soundFill = value; }
            }
        }

        /// <summary>
        /// Whether the Sound Engine is full or not. See also <seealso cref="soundFill"/> for example.
        /// </summary>
        public static bool soundFilled => soundFill > UIelement.FrameMultiply(80) || ConfigMenu.mute;

        /// <summary>
        /// Whether the mod is configurable or not.
        /// You can just replace this to <c>return true;</c> or <c>false</c> to save computing time.
        /// </summary>
        /// <returns>Whether the mod is configurable or not</returns>
        public virtual bool Configuable()
        {
            //if (!OptionScript.isOptionMenu) { return false; }
            Dictionary<string, string> temp = GrabConfig();
            if (temp.Count > 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// <see cref="RainWorldMod"/> that holds basic information of Partiality Mod or BepInEx Plugin
        /// </summary>
        public RainWorldMod rwMod { get; internal set; }

        /// <summary>
        /// OpTab that contains UIelements for your config screen.
        /// Initialize this in <see cref="Initialize"/> like <c>this.Tabs = new OpTab[count];</c>
        /// </summary>
        public OpTab[] Tabs;

        private protected DirectoryInfo directory => new DirectoryInfo(string.Concat(
                    OptionMod.directory.FullName,
                    rwMod.ModID,
                    Path.DirectorySeparatorChar
                    ));

        /// <summary>
        /// Config Data. Their Key is the key you set when add <see cref="UIconfig"/> in <see cref="OpTab.AddItems(UIelement[])"/>.
        /// Value is the value of those configs, in string form.
        /// </summary>
        /// <remarks>As this is a <c>static</c>, add salt (e.g. ModID) to your keys to prevent collision with other mods.</remarks>
        public static Dictionary<string, string> config;

        private string rawConfig;
        private const string rawConfigDef = "Unconfiguable";

        /// <summary>
        /// This will be called by ConfigMachine manager automatically.
        /// If you called this directly, also call <see cref="ShowConfig"/> to apply them in UI.
        /// </summary>
        internal bool LoadConfig()
        {
            config = new Dictionary<string, string>();
            rawConfig = rawConfigDef;
            if (!Configuable()) { return true; }
            if (this is GeneratedOI && (this as GeneratedOI).mode == GeneratedOI.GenMode.BepInExConfig)
            { return (this as GeneratedOI).LoadBepConfig(); }
            if (!directory.Exists) { return false; } //directory.Create();

            string path = string.Concat(directory.FullName, "config.json");
            if (File.Exists(path)) { this.rawConfig = File.ReadAllText(path, Encoding.UTF8); }
            else { return false; }

            Dictionary<string, object> loadedConfig = MiniJsonExtensions.dictionaryFromJson(this.rawConfig);
            foreach (KeyValuePair<string, object> item in loadedConfig)
            { config.Add(item.Key, item.Value.ToString()); }

            try
            { ConfigOnChange(); }
            catch (Exception e)
            {
                Debug.Log("CompletelyOptional) Lost backward compatibility in Config! Reset Config.");
                Debug.Log(new LoadDataException(e.ToString()));
                File.Delete(path);
                config = new Dictionary<string, string>();
                rawConfig = rawConfigDef;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Displays loaded config to <see cref="UIconfig"/>s. It's called automatically.
        /// </summary>
        internal void ShowConfig()
        {
            GrabObject();
            if (this is GeneratedOI && (this as GeneratedOI).mode == GeneratedOI.GenMode.BepInExConfig)
            { (this as GeneratedOI).ShowBepConfig(); return; }
            if (!(config?.Count > 0)) { return; } //Nothing Loaded.
            foreach (KeyValuePair<string, string> setting in config)
            {
                if (objectDictionary.TryGetValue(setting.Key, out UIconfig obj))
                {
                    obj.value = setting.Value;
                }
                else
                {
                    Debug.Log($"{rwMod.ModID} setting has been removed. (key: {setting.Key} / value: {setting.Value})");
                }
            }
        }

        private void GrabObject()
        {
            Dictionary<string, UIconfig> displayedConfig = new Dictionary<string, UIconfig>();
            for (int i = 0; i < this.Tabs.Length; i++)
            {
                Dictionary<string, UIconfig> tabDictionary = this.Tabs[i].GetTabObject();
                if (tabDictionary.Count < 1) { continue; }

                foreach (KeyValuePair<string, UIconfig> item in tabDictionary)
                {
                    if (item.Value.cosmetic) { continue; }
                    if (displayedConfig.ContainsKey(item.Key)) { throw new DupelicateKeyException(string.Empty, item.Key); }
                    displayedConfig.Add(item.Key, item.Value);
                }
            }
            this.objectDictionary = displayedConfig;
        }

        /// <summary>
        /// Grabbing config from config menu. It's called automatically.
        /// </summary>
        internal Dictionary<string, string> GrabConfig()
        {
            GrabObject();
            if (this.objectDictionary.Count < 1) { return new Dictionary<string, string>(0); }
            Dictionary<string, string> displayedConfig = new Dictionary<string, string>();
            foreach (KeyValuePair<string, UIconfig> setting in this.objectDictionary)
            {
                if (setting.Value.cosmetic) { continue; } //this won't happen
                displayedConfig.Add(setting.Key, setting.Value.value);
            }
            return displayedConfig;
        }

        /// <summary>
        /// Saving Config. It's called automatically.
        /// </summary>
        internal bool SaveConfig(Dictionary<string, string> newConfig)
        {
            if (!Configuable()) { return true; }
            if (this is GeneratedOI && (this as GeneratedOI).mode == GeneratedOI.GenMode.BepInExConfig)
            { return (this as GeneratedOI).SaveBepConfig(newConfig); } // Saving config is handled by BepInEx

            if (newConfig.Count < 1) { return false; } //Nothing to Save.
            config = newConfig;
            if (!directory.Exists) { directory.Create(); }
            ConfigOnChange();

            //Write in file
            try
            {
                string path = string.Concat(directory.FullName, "config.json");
                Dictionary<string, object> temp = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string> item in newConfig)
                {
                    temp.Add(item.Key, item.Value);
                }
                rawConfig = MiniJsonExtensions.toJson(temp);
                // Formatting
                rawConfig = rawConfig.Replace("{", "{\n    ");
                rawConfig = rawConfig.Replace("}", "\n}");
                rawConfig = rawConfig.Replace("\",\"", "\",\n    \"");
                rawConfig = rawConfig.Replace("\":\"", "\" : \"");

                File.WriteAllText(path, this.rawConfig, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

        /// <summary>
        /// Dictionary that contains configuable objects.
        /// Use <see cref="config"/> instead.
        /// </summary>
        public Dictionary<string, UIconfig> objectDictionary;

        /// <summary>
        /// Event that happens when Config is loaded from file/changed by config menu.
        /// Put your configuable var in here.
        /// </summary>
        public virtual void ConfigOnChange()
        {
            if (isOptionMenu)
            {
                foreach (OpTab tab in Tabs)
                {
                    foreach (UIelement item in tab.items)
                    {
                        if (item is UIconfig && (item as UIconfig).cosmetic)
                        {
                            item.Reset();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If true, <see cref="Initialize"/> is in Mod Config Menu; if false, we're in IntroRoll and loading Configs.
        /// Do not edit graphical details of <see cref="UIelement"/>s when this is false.
        /// </summary>
        /// <remarks>Example: <c>if (isOptionMenu) { myLabel.label.alpha = 0.5f; }</c></remarks>
        public static bool isOptionMenu => OptionScript.isOptionMenu;

        /// <summary>
        /// Write your UI overlay here. See also <seealso cref="isOptionMenu"/>.
        /// </summary>
        public virtual void Initialize()
        { //Also Reset Config (Initialize w/o LoadConfig), and call ConfigOnChange().
            this.Tabs = null;
			curLang = "";
            CheckTestTranslation();
        }

        /// <summary>
        /// Event that's called every frame. Do not call this by your own.
        /// </summary>
        /// <param name="dt">deltaTime</param>
        public virtual void Update(float dt)
        {
            foreach (OpTab tab in this.Tabs) { tab.owner = this; }
        }

        /// <summary>
        /// Method that's called from <see cref="UItrigger"/>s.
        /// Override this and write your own events.
        /// </summary>
        /// <param name="trigger"><see cref="UItrigger"/> instance.</param>
        /// <param name="signal"><see cref="UItrigger.signal"/> value.</param>
        public virtual void Signal(UItrigger trigger, string signal)
        {
        }

        #region Obsolete

        /// <summary>
        /// Returns whether ConfigMachine is loaded or not.
        /// </summary>
        /// <remarks>But if it didn't, your dependent mod also won't load so this never gets unused.</remarks>
        [Obsolete]
        public static bool ConfigModExist()
        {
            List<PartialityMod> loadedMods = Partiality.PartialityManager.Instance.modManager.loadedMods;
            foreach (PartialityMod mod in loadedMods)
            {
                if (mod.ModID == OptionMod.instance.ModID) { return true; }
            }
            //slot = OptionScript.Slot;
            return false;
        }

        /// <summary>
        /// The <see cref="PartialityMod"/> using this <see cref="OptionInterface"/>. Unused after v1.5; check <see cref="rwMod"/>
        /// </summary>
        [Obsolete]
        public PartialityMod mod;

        /// <summary>
        /// Use <see cref="OpColorPicker.HexToColor(string)"/> instead.
        /// </summary>
        [Obsolete]
        public static Color HexToColor(string hex)
        {
            return OpColorPicker.HexToColor(hex);
        }

        /// <summary>
        /// Use <see cref="OpColorPicker.ColorToHex(Color)"/> instead.
        /// </summary>
        [Obsolete]
        public static string ColorToHex(Color color)
        {
            return OpColorPicker.ColorToHex(color);
        }

        #endregion Obsolete
    }
}
