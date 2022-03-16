using BepInEx;
using BepInEx.Configuration;
using CompletelyOptional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// To Interact with Config Machine, check if your project has this class
    /// </summary>
    public abstract partial class OptionInterface
    {
        /// <summary>
        /// Custom OptionInterface for BepInEx Plugin.
        /// Create <c>public static [YourOIclass] LoadOI()</c> in your <see cref="BaseUnityPlugin"/>.
        /// ConfigMachine will load your OI after IntroRoll.
        /// </summary>
        /// <remarks>Example:
        /// <code>
        /// public static MyOptionInterface LoadOI()
        /// {
        ///     return new MyOptionInterface(MyPlugin.instance);
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
        /// Whether the mod is configurable or not.
        /// You can just replace this to <c>return true;</c> or <c>false</c> to save computing time.
        /// </summary>
        /// <returns>Whether the mod is configurable or not</returns>
        public virtual bool Configurable()
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

        protected DirectoryInfo directory => new DirectoryInfo(string.Concat(
                    ComOptPlugin.directory.FullName,
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
        /// BaseUnityPlugin.Config
        /// </summary>
        public ConfigFile bepConfig => (rwMod.mod as BaseUnityPlugin).Config;

        /// <summary>
        /// This will be called by ConfigMachine manager automatically.
        /// If you called this directly, also call <see cref="ShowConfig"/> to apply them in UI.
        /// </summary>
        internal void LoadConfig()
        {
            config = new Dictionary<string, string>();
            rawConfig = rawConfigDef;
            if (!Configurable()) { return; }
            bepConfig.Reload();
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
                    ComOptPlugin.LogMessage($"{rwMod.ModID} setting has been removed. (key: {setting.Key} / value: {setting.Value})");
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
            if (!Configurable()) { return true; }
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
            catch (Exception ex) { ComOptPlugin.LogError(new SaveDataException(ex.ToString()).ToString()); }

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

        /// <summary>
        /// Write your UI overlay here.
        /// </summary>
        public virtual void Initialize()
        { //Also Reset Config (Initialize w/o LoadConfig), and call ConfigOnChange().
            this.Tabs = null;
            curLang = "";
            CheckTestTranslation();
        }

        /// <summary>
        /// Event that's called every frame. Do note that this is called right after <see cref="UIelement.Update"/>s in currently active <see cref="OpTab"/> are called.
        /// </summary>
        public virtual void Update()
        {
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
    }
}