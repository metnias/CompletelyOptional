using CompletelyOptional;
using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// To Interact with CompletelyOptional Mod
    /// Check if PartialityMod file has this class
    /// </summary>
    public class OptionInterface
    {
        /// <summary>
        /// Option Interface for Partiality Mod/Patch.
        /// Create <c>public static [YourOIclass] LoadOI()</c> in your <see cref="PartialityMod"/>.
        /// ConfigMachine will load your OI after IntroRoll.
        /// </summary>
        /// <remarks>Example:
        /// <code>
        /// public static MyOptionInterface LoadOI()
        /// {
        ///     return new MyOptionInterface(MyMod.instance);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="mod">Your Partiality mod.</param>
        public OptionInterface(PartialityMod mod)
        {
            this.mod = mod;
            this.rawConfig = "Unconfiguable";
            instance = this;
        }

        private static OptionInterface instance;

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
            _slot = slot;
            return false;
        }

        /// <summary>
        /// The less the value is, the more likely to ignored by ConfigMachine. The range is -1 ~ 2.
        /// </summary>
        public int GetPriority()
        {
            if (OptionScript.blackList.Contains(mod?.ModID)) { return -1; }
            if (this is UnconfiguableOI oi)
            {
                if (oi.reason != UnconfiguableOI.Reason.NoInterface) { return 2; }
                return -1;
            }
            else if (this is InternalTestOI) { return 2; }
            return this.Configuable() ? 1 : 0;
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
                OptionScript.soundFill = value;
            }
        }

        /// <summary>
        /// Whether the Sound Engine is full or not. See also <seealso cref="soundFill"/> for example.
        /// </summary>
        public static bool soundFilled => soundFill > 80;

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
        /// The <see cref="PartialityMod"/> using this <see cref="OptionInterface"/>.
        /// </summary>
        public PartialityMod mod;

        /// <summary>
        /// OpTab that contains UIelements for your config screen.
        /// Initialize this in <see cref="Initialize"/> like <c>this.Tabs = new OpTab[count];</c>
        /// </summary>
        public OpTab[] Tabs;

        /// <summary>
        /// Default Save Data of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultData
        {
            get { return string.Empty; }
        }

        private DirectoryInfo directory => new DirectoryInfo(string.Concat(
                    OptionMod.directory.FullName,
                    mod.ModID,
                    Path.DirectorySeparatorChar
                    ));

        /// <summary>
        /// Currently selected saveslot
        /// </summary>
        public static int slot { get { return OptionScript.Slot; } }

        private static int _slot;

        /// <summary>
        /// Currently selected slugcat
        /// </summary>
        public static int slugcat { get { return OptionScript.Slugcat; } }

        private static int _slugcat;

        /// <summary>
        /// Config Data. Their Key is the key you set when add <see cref="UIconfig"/> in <see cref="OpTab.AddItems(UIelement[])"/>.
        /// Value is the value of those configs, in string form.
        /// </summary>
        /// <remarks>As this is a <c>static</c>, add salt (e.g. ModID) to your keys to prevent collision with other mods.</remarks>
        public static Dictionary<string, string> config;

        private string rawConfig;

        /// <summary>
        /// This will be called by ConfigMachine manager. It's called automatically.
        /// </summary>
        public bool LoadConfig()
        {
            config = new Dictionary<string, string>();
            rawConfig = "Unconfiguable";
            if (!directory.Exists)
            { directory.Create(); return false; }

            string path = string.Concat(directory.FullName, "config.txt");
            if (File.Exists(path))
            {
                try
                {
                    string txt = File.ReadAllText(path, Encoding.UTF8);
                    string key = txt.Substring(0, 32);
                    txt = txt.Substring(32, txt.Length - 32);
                    if (Custom.Md5Sum(txt) != key)
                    {
                        Debug.Log($"{mod.ModID} config file has been tinkered! Load Default Config instead.");
                        return false;
                    }

                    this.rawConfig = Crypto.DecryptString(txt, string.Concat("OptionalConfig ", mod.ModID));
                }
                catch
                {
                    Debug.Log(new LoadDataException($"{mod.ModID} config file has been corrupted! Load Default Config instead."));
                    return false;
                }
            }
            else
            {
                //load default config from dictionary and save.
                return false;
            }

            //Set Initialized stuff to new value
            Dictionary<string, string> loadedConfig = new Dictionary<string, string>();

            //convert to dictionary
            //<CfgC>key<CfgB><CfgD>value<CfgA>
            string[] array = Regex.Split(this.rawConfig, "<CfgA>");

            for (int i = 0; i < array.Length; i++)
            { //<CfgC>key<CfgB><CfgD>value
                string key = string.Empty;
                string value = string.Empty;
                string[] array2 = Regex.Split(array[i], "<CfgB>");
                for (int j = 0; j < array2.Length; j++)
                { //<CfgC>key || <CfgD>value
                    if (array2[j].Length < 6) { continue; }
                    if (array2[j].Substring(0, 6) == "<CfgC>")
                    {
                        key = array2[j].Substring(6, array2[j].Length - 6);
                    }
                    else if (array2[j].Substring(0, 6) == "<CfgD>")
                    {
                        value = array2[j].Substring(6, array2[j].Length - 6);
                    }
                }
                if (string.IsNullOrEmpty(key))
                { //?!?!
                    continue;
                }
                loadedConfig.Add(key, value);
            }

            config = loadedConfig;

            try
            {
                ConfigOnChange();
            }
            catch (Exception e)
            {
                Debug.Log("CompletelyOptional: Lost backward compatibility in Config! Reset Config.");
                Debug.Log(new LoadDataException(e.ToString()));
                File.Delete(path);
                config = new Dictionary<string, string>();
                rawConfig = "Unconfiguable";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Displays loaded config to <see cref="UIconfig"/>s. It's called automatically.
        /// </summary>
        public void ShowConfig()
        {
            GrabObject();
            if (!(config?.Count > 0)) { return; } //Nothing Loaded.
            foreach (KeyValuePair<string, string> setting in config)
            {
                if (objectDictionary.TryGetValue(setting.Key, out UIconfig obj))
                {
                    obj.value = setting.Value;
                }
                else
                {
                    Debug.Log($"{mod.ModID} setting has been removed. (key: {setting.Key} / value: {setting.Value})");
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
        public Dictionary<string, string> GrabConfig()
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
        public bool SaveConfig(Dictionary<string, string> newConfig)
        {
            if (newConfig.Count < 1) { return false; } //Nothing to Save.
            config = newConfig;
            if (!directory.Exists)
            {
                directory.Create();
            }
            ConfigOnChange();

            this.rawConfig = string.Empty;
            //convert to raw data
            //<CfgC>key<CfgB><CfgD>value<CfgA>

            foreach (KeyValuePair<string, string> item in newConfig)
            {
                this.rawConfig = this.rawConfig + "<CfgC>" + item.Key + "<CfgB><CfgD>" + item.Value + "<CfgA>";
            }

            //Write in file
            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "config.txt"
                });
                string enc = Crypto.EncryptString(this.rawConfig, string.Concat("OptionalConfig " + mod.ModID));
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc, Encoding.UTF8);

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

        private string[] _data;

        /// <summary>
        /// Set this to whatever you want and call <see cref="SaveData"/> and <see cref="LoadData"/> when you need.
        /// </summary>
        public string data
        {
            get { return _data[slugcat]; }
            set { if (_data[slugcat] != value) { instance.DataOnChange(); _data[slugcat] = value; } }
        }

        /// <summary>
        /// Use <see cref="data"/> instead.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown when used.</exception>
        [Obsolete]
        public static string progData
        {
            get { throw new NotImplementedException("OptionInterface.progData is no longer used! Use OptionInterface.data instead."); }
        }

        /// <summary>
        /// Event when saved data is changed
        /// This is called when 1. <see cref="LoadData"/>, 2. Your mod changes data.
        /// </summary>
        public virtual void DataOnChange()
        {
        }

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
        /// Event that happens when selected SaveSlot has been changed.
        /// This automatically saves and loads data by default.
        /// </summary>
        public virtual void SlotOnChange()
        {
            SaveData();
            _slot = slot; _slugcat = slugcat;
            LoadData();
        }

        /// <summary>
        /// If this is true, data is automatically Saved/Loaded like vanilla game
        /// </summary>
        public bool progressData = false;

        public bool saveAsDeath
        {
            get { if (!progressData) { throw new Exception(); } return _saveAsDeath; }
            set { _saveAsDeath = value; }
        }

        public bool saveAsQuit
        {
            get { if (!progressData) { throw new Exception(); } return _saveAsQuit; }
            set { _saveAsQuit = value; }
        }

        private bool _saveAsDeath = false, _saveAsQuit = false;

        /// <summary>
        /// Load your raw data from ConfigMachine Mod.
        /// Call this by your own.
        /// Check <see cref="dataTinkered"/> to see if saved data is tinkered or not.
        /// </summary>
        /// <returns>Loaded Data</returns>
        public virtual void LoadData()
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = defaultData;
            }
            try
            {
                string data = string.Empty;
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name.Substring(file.Name.Length - 4) != ".txt") { continue; }

                    if (file.Name.Substring(0, 4) == "data")
                    {
                        if (slot.ToString() != file.Name.Substring(file.Name.Length - 5, 1)) { continue; }
                    }
                    else { continue; }

                    //LoadSlotData:
                    data = File.ReadAllText(file.FullName, Encoding.UTF8);
                    string key = data.Substring(0, 32);
                    data = data.Substring(32, data.Length - 32);
                    if (Custom.Md5Sum(data) != key)
                    {
                        Debug.Log($"{mod.ModID} data file has been tinkered!");
                        dataTinkered = true;
                    }
                    else
                    {
                        dataTinkered = false;
                    }
                    data = Crypto.DecryptString(data, string.Concat("OptionalData " + mod.ModID));
                }
                string[] raw = Regex.Split(data, "<slugChar>");
                _data = new string[Math.Max(_data.Length, raw.Length)];
                for (int j = 0; j < raw.Length; j++)
                {
                    _data[j] = raw[j];
                }
                return;
            }
            catch (Exception ex) { Debug.LogException(new LoadDataException(ex.ToString())); }
        }

        /// <summary>
        /// If you want to see whether your data is tinkered or not.
        /// </summary>
        public bool dataTinkered { get; private set; } = false;

        /// <summary>
        /// Save your raw data in file. bool is whether it succeed or not
        /// Call this by your own.
        /// </summary>
        public virtual bool SaveData()
        {
            string data = string.Empty;
            for (int i = 0; i < _data.Length; i++) { data += _data[i] + "<slugChar>"; };
            //if (string.IsNullOrEmpty(_data)) { return false; }
            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "data_",
                slot.ToString(),
                ".txt"
                });
                string enc = Crypto.EncryptString(data, string.Concat("OptionalData " + mod.ModID));
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
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
        /// Do not call this by your own.
        /// </summary>
        public void BackgroundUpdate(int saveOrLoad)
        {
            switch (saveOrLoad)
            {
                case 1: SaveData(); break;
                case 2: LoadData(); break;
            }
            if (_slot != slot) { SlotOnChange(); }
            else if (_slugcat != slugcat) { SlotOnChange(); }
        }

        /// <summary>
        /// Do not call this by your own.
        /// </summary>
        /// <param name="slugcatLength"></param>
        public void GenerateDataArray(int slugcatLength)
        {
            _data = new string[slugcatLength];
            LoadData();
        }

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

        #region translator

        /// <summary>
        /// File name of your mod's translation data in your mod's assembly, including '.txt'
        /// </summary>
        public string transFile;

        private string[] transData = null;

        private string curLang;
        private Dictionary<string, string> transConverter;

        /// <summary>
        /// Translate your string using embed txt in your mod
        /// </summary>
        /// <param name="orig">Original text</param>
        /// <returns>Translated text (returns original if translation isn't found.)</returns>
        public string Translate(string orig)
        {
            if (curLang != OptionScript.curLang)
            {
                curLang = OptionScript.curLang;
                LoadTranslation();
            }

            if (transConverter.TryGetValue(orig, out string res))
            { return res.Replace("\\n", Environment.NewLine); }
            else { return orig.Replace("\\n", Environment.NewLine); }
        }

        /// <summary>
        /// Returns <see cref="CultureInfo"/> according to Rain World Language Settings.
        /// </summary>
        /// <remarks>Example: <c>DateTime.ParseExact("20200123", "yyyyMMdd", null).ToString("D", GetCultureInfo());</c></remarks>
        public static CultureInfo GetCultureInfo() => InternalTranslator.GetCultureInfo();

        /// <summary>
        /// Returns 3-lettered
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageID() => OptionScript.curLang;

        private void LoadTranslation()
        {
            transConverter = new Dictionary<string, string>();

            if (transData == null) { if (!ReadTransTXT()) { return; }; }
            for (int i = 0; i < transData.Length; i++)
            {
                if (transData[i].StartsWith("//") || transData[i].Length < 5) { continue; }
                //Debug.Log(data[i]);
                string[] langs = transData[i].Split('|'); //Regex.Split(data[i], "/\|/");
                if (langs.Length < 2) { continue; }
                bool hasDefault = false;
                for (int j = 1; j < langs.Length; j++)
                {
                    string[] piece = langs[j].Split('$'); //Regex.Split(langs[j], @"$");
                    if (piece.Length < 2)
                    {
                        Debug.LogError($"CompletelyOptional: Specify Language for your translation in this format: \'lang$translation\'\n Allowed Language Codes are: {InternalTranslator.allowedCodes}"); continue;
                    }
                    if (curLang == InternalTranslator.LangToCode(piece[0]))
                    {
                        //string cvt = Regex.Replace(piece[1], "/\\en/i", Environment.NewLine);
                        if (transConverter.ContainsKey(langs[0]))
                        {
                            if (!hasDefault) { Debug.LogError($"Conflicting Key found: \'{langs[0]}\'"); }
                            transConverter.Remove(langs[0]);
                        }
                        transConverter.Add(langs[0], piece[1]);
                    }
                    else if (InternalTranslator.LangToCode(piece[0]) == "eng")
                    {
                        if (transConverter.ContainsKey(langs[0])) { continue; }
                        transConverter.Add(langs[0], piece[1]); //add default translation
                        hasDefault = true;
                    }
                }
            }
        }

        private void CheckTestTranslation()
        {
            if (string.IsNullOrEmpty(transFile) || !transFile.Contains('.')) { return; }
            string[] temp = transFile.Split('.');
            string test = string.Concat(directory.FullName, temp[temp.Length - 2], ".txt");

            if (File.Exists(test))
            {
                Debug.Log($"{this.mod.ModID} reloaded external translation: {test}");
                transData = Regex.Split(File.ReadAllText(test), Environment.NewLine);
                LoadTranslation();
            }
        }

        private bool ReadTransTXT()
        {
            if (this.mod == null)
            { Debug.LogError("CompletelyOptional: OptionInterface.mod is null!"); return false; }

            if (string.IsNullOrEmpty(transFile)) { return false; }

            var assembly = Assembly.GetAssembly(this.mod.GetType());
            string result;

            try
            {
                using (Stream stream = assembly.GetManifestResourceStream(transFile))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CompletelyOptional: Unable to find the translation txt named {transFile}");
                Debug.LogException(e);
                Debug.LogError("This is the list of resources in your mod's assembly. Pick one and set \'transFile\' in ctor!");

                string[] names = assembly.GetManifestResourceNames();
                foreach (string name in names) { Debug.LogError(name); }
                return false;
            }

            transData = Regex.Split(result, Environment.NewLine);
            return true;
        }

        #endregion translator

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
