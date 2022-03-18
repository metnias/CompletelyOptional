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
        #region Modders

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
        /// OpTab that contains UIelements for your config screen.
        /// Initialize this in <see cref="Initialize"/> like <c>this.Tabs = new OpTab[count];</c>
        /// </summary>
        public OpTab[] Tabs = null;

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

        /// <summary>
        /// Whether the mod is configurable or not.
        /// You can just replace this to <c>return true;</c> or <c>false</c> to save computing time.
        /// </summary>
        /// <returns>Whether the mod is configurable or not</returns>
        public virtual bool Configurable()
        {
            if (rwMod.type == RainWorldMod.Type.Dummy) { return false; }
            if (GrabConfigs().Count > 0) { return true; }
            else { return false; }
        }

        #endregion Modders

        #region Internal

        /// <summary>
        /// <see cref="RainWorldMod"/> that holds basic information of Partiality Mod or BepInEx Plugin
        /// </summary>
        public RainWorldMod rwMod { get; internal set; }

        protected DirectoryInfo directory => new DirectoryInfo(string.Concat(
                    ComOptPlugin.directory.FullName,
                    rwMod.ModID,
                    Path.DirectorySeparatorChar
                    ));

        /// <summary>
        /// BaseUnityPlugin.Config
        /// </summary>
        protected internal ConfigFile bepConfig => rwMod.plugin.Config;

        /// <summary>
        /// This ctor is only used by Config Machine internally.
        /// </summary>
        internal OptionInterface(RainWorldMod rwMod)
        {
            this.rwMod = rwMod;
            this.rawConfig = rawConfigDef;
        }

        /// <summary>
        /// This will be called by ConfigMachine manager automatically.
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
            if (!Configurable()) { return; }
            List<UIconfig> configs = GrabConfigs();
            foreach (UIconfig config in configs.ToArray())
            {
                config.value = config.GetStringValue(config.cfgEntry.BoxedValue);
            }
        }

        private List<UIconfig> GrabConfigs()
        {
            List<UIconfig> configs = new List<UIconfig>();
            if (Tabs == null) { return configs; }
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (Tabs[i] == null) { continue; }
                foreach (UIelement element in this.Tabs[i].items.ToArray())
                {
                    if (element.GetType().IsSubclassOf(typeof(UIconfig)))
                    {
                        if ((element as UIconfig).cosmetic) { continue; }
                        configs.Add(element as UIconfig);
                        //config.Add((element as UIconfig).key, (element as UIconfig).value);
                    }
                }
            }
            return configs;
        }

        internal void SaveConfig()
        {
            List<UIconfig> configs = GrabConfigs();
            foreach (UIconfig config in configs.ToArray())
            {
                switch (config.cfgEntry.SettingType.Name.ToLower())
                {
                    case "bool":
                    case "boolean":
                        (config.cfgEntry as ConfigEntry<bool>).Value = config.value == "true";
                        break;

                    case "byte":
                        (config.cfgEntry as ConfigEntry<byte>).Value = byte.Parse(config.value);
                        break;

                    case "uint":
                    case "uint32":
                        (config.cfgEntry as ConfigEntry<uint>).Value = uint.Parse(config.value);
                        break;

                    case "int":
                    case "int32":
                        (config.cfgEntry as ConfigEntry<int>).Value = int.Parse(config.value);
                        break;

                    case "float":
                    case "single":
                        (config.cfgEntry as ConfigEntry<float>).Value = float.Parse(config.value);
                        break;

                    case "string":
                        (config.cfgEntry as ConfigEntry<string>).Value = config.value;
                        break;

                    case "keycode":
                        (config.cfgEntry as ConfigEntry<KeyCode>).Value = (KeyCode)Enum.Parse(typeof(KeyCode), config.value);
                        break;

                    default:
                        config.cfgEntry.SetSerializedValue(config.value);
                        break;
                }
            }
            bepConfig.Save();
        }

        #endregion Internal

        #region Regacy

        /// <summary>
        /// Config Data. Their Key is the key you set when add <see cref="UIconfig"/> in <see cref="OpTab.AddItems(UIelement[])"/>.
        /// Value is the value of those configs, in string form.
        /// </summary>
        /// <remarks>As this is a <c>static</c>, add salt (e.g. ModID) to your keys to prevent collision with other mods.</remarks>
        private Dictionary<string, string> config;

        private string rawConfig;
        private const string rawConfigDef = "Unconfiguable";

        [Obsolete]
        internal bool RegacyLoadConfig()
        {
            config = new Dictionary<string, string>();
            rawConfig = rawConfigDef;
            if (!Configurable()) { return true; }
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

        [Obsolete]
        internal bool RegacySaveConfig(Dictionary<string, string> newConfig)
        {
            if (!Configurable()) { return true; }

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

        #endregion Regacy

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
    }
}