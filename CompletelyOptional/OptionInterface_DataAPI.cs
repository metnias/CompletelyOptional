using CompletelyOptional;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OptionalUI
{
    public partial class OptionInterface
    {
        #region Converter

        /// <summary>
        /// Very basic <see cref="Dictionary{TKey, TValue}"/> to <see cref="string"/> converter for <see cref="OptionInterface.data"/>
        /// or <see cref="OptionInterface.saveData"/> etc. See also <seealso cref="StringToDictionary(string)"/> for reversal
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DictionaryToString(Dictionary<string, string> data)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in data)
            { builder.Append(pair.Key).Append(":").Append(pair.Value).Append(','); }
            string result = builder.ToString();
            result = result.TrimEnd(','); // remove the end comma
            return result;
        }

        /// <summary>
        /// Very basic decoder for string from <see cref="DictionaryToString(Dictionary{string, string})"/>.
        /// This does NOT have an exception handler for wrongly formatted string, by the way.
        /// </summary>
        /// <param name="file"><see cref="string"/> generated from <see cref="DictionaryToString(Dictionary{string, string})"/></param>
        public static Dictionary<string, string> StringToDictionary(string file)
        {
            var result = new Dictionary<string, string>();
            string value = File.ReadAllText(file);
            string[] items = value.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < items.Length; i++)
            {
                string[] item = items[i].Split(new char[':']);
                if (item.Length < 2) { continue; }

                if (result.ContainsKey(item[0])) { result[item[0]] = item[1]; }
                else { result.Add(item[0], item[1]); }
            }
            return result;
        }

        #endregion Converter

        #region customData

        /// <summary>
        /// Default <see cref="data"/> of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultData
        {
            get { return string.Empty; }
        }

        private string _data;

        /// <summary>
        /// Data tied to nothing. Stays even if the user changed Saveslot. Useful for keeping extra data for mod settings.
        /// This won't get saved or loaded automatically and you have to call it by yourself.
        /// Set this to whatever you want and call <see cref="SaveData"/> and <see cref="LoadData"/> when you need.
        /// Causes a call to <see cref="DataOnChange"/> when its value is changed.
        /// <para>See also <seealso cref="DictionaryToString(Dictionary{string, string})"/></para>
        /// </summary>
        public string data
        {
            get { return _data; }
            set { if (_data != value) { _data = value; DataOnChange(); } }
        }

        /// <summary>
        /// Event when either <see cref="data"/> is changed. Override it to add your own behavior.
        /// This is called when 1. You run <see cref="LoadData"/>, 2. Your mod changes <see cref="data"/>.
        /// </summary>
        public virtual void DataOnChange()
        {
        }

        /// <summary>
        /// Load your raw data from ConfigMachine Mod.
        /// Call this by your own.
        /// Check <see cref="progDataTinkered"/> to see if saved data is tinkered or not.
        /// </summary>
        public virtual void LoadData()
        {
            _data = defaultData;
            if (!directory.Exists) { DataOnChange(); return; }
            try
            {
                string data = string.Empty;
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name != "data.txt") { continue; }

                    //LoadData:
                    data = File.ReadAllText(file.FullName, Encoding.UTF8);
                    string key = data.Substring(0, 32);
                    data = data.Substring(32, data.Length - 32);
                    if (Custom.Md5Sum(data) != key)
                    {
                        Debug.Log($"CompletelyOptional) {rwMod.ModID} data file has been tinkered!");
                        dataTinkered = true;
                    }
                    else { dataTinkered = false; }
                    _data = Crypto.DecryptString(data, CryptoDataKey);
                    DataOnChange();
                    return;
                }
            }
            catch (Exception ex) { Debug.LogException(new LoadDataException(ex.ToString())); }
            DataOnChange();
        }

        /// <summary>
        /// If you want to see whether your <see cref="data"/> is tinkered or not.
        /// </summary>
        public bool dataTinkered { get; private set; } = false;

        private string CryptoDataKey => "OptionalData" + rwMod.ModID;

        /// <summary>
        /// Save your raw <see cref="data"/> in file. Call this by your own.
        /// <para>This is NOT <see cref="saveData"/> which is a part of Progression API</para>
        /// </summary>
        /// <returns>Whether it succeed or not</returns>
        public virtual bool SaveData()
        {
            if (!directory.Exists) { directory.Create(); }

            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "data.txt"
                });
                string enc = Crypto.EncryptString(_data ?? "", CryptoDataKey);
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

        #endregion customData

        #region progData

        // Progression API by Henpemaz
        // You can see this one's not by me cuz there's no bracket for one-line if statements
        // Data that is stored/retrieved mimicking the game's behavior
        // save -> SaveState
        // pers -> DeathPersistentData
        // misc -> Progression & MiscProgressionData
        // See https://media.discordapp.net/attachments/849525819855863809/877611189444698113/unknown.png

        /// <summary>
        /// Progression API: Whether the Progression Data subsystem is used by the mod or not.
        /// Set this to true to enable saving and loading <see cref="saveData"/>, <see cref="persData"/> and <see cref="miscData"/>
        /// </summary>
        public bool hasProgData = false;

        /// <summary>
        /// If you want to see whether your most recently loaded <see cref="saveData"/>, <see cref="persData"/> or <see cref="miscData"/> was tinkered or not.
        /// When this happens defaultData will be used instead of loading from the file.
        /// </summary>
        public bool progDataTinkered { get; private set; } = false;

        private string _saveData;
        private string _persData;
        private string _miscData;

        /// <summary>
        /// Progression API: saveData tied to a specific slugcat's playthrough. This gets reverted automatically if the Slugcat loses.
        /// Enable <see cref="hasProgData"/> to use this. Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// Typically saveData is only saved when the slugcat hibernates. Exceptionally, it's saved when the player uses a passage, and on Red's ascension/gameover
        /// <para>This is NOT <see cref="SaveData"/> method which is to be manually called for saving <seealso cref="data"/></para>
        /// <para>See also <seealso cref="persData"/>, <seealso cref="miscData"/>, and <seealso cref="DictionaryToString(Dictionary{string, string})"/></para>
        /// </summary>
        public string saveData
        {
            get
            {
                if (!hasProgData) throw new NoProgDataException(this);
                return _saveData;
            }
            set
            {
                if (!hasProgData) throw new NoProgDataException(this);
                if (_saveData != value) { _saveData = value; }
            }
        }

        /// <summary>
        /// Progression API: Savedata tied to a specific slugcat's playthrough, death-persistent.
        /// Enable <see cref="hasProgData"/> to use this. Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// Typically persData is saved when 1. going in-game (calls <see cref="SaveDeath"/>) 2. Surviving/dying/quitting/meeting an echo/ascending etc.
        /// <para>See also <seealso cref="saveData"/>, <seealso cref="miscData"/>, and <seealso cref="DictionaryToString(Dictionary{string, string})"/></para>
        /// </summary>
        public string persData
        {
            get
            {
                if (!hasProgData) throw new NoProgDataException(this);
                return _persData;
            }
            set
            {
                if (!hasProgData) throw new NoProgDataException(this);
                if (_persData != value) { _persData = value; }
            }
        }

        /// <summary>
        /// Progression API: Savedata shared across all slugcats on the same Saveslot.
        /// Enable <see cref="hasProgData"/> to use this. Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// Typically miscData is saved when saving/starving/dying/meeting an echo/ascending etc, but not when quitting the game to the menu.
        /// <para>See also <seealso cref="saveData"/>, <seealso cref="persData"/>, and <seealso cref="DictionaryToString(Dictionary{string, string})"/></para>
        /// </summary>
        public string miscData
        {
            get
            {
                if (!hasProgData) throw new NoProgDataException(this);
                return _miscData;
            }
            set
            {
                if (!hasProgData) throw new NoProgDataException(this);
                if (_miscData != value) { _miscData = value; }
            }
        }

        /// <summary>
        /// Progression API: Default <see cref="saveData"/> of this mod.
        /// </summary>
        public virtual string defaultSaveData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Progression API: Default <see cref="persData"/> of this mod.
        /// </summary>
        public virtual string defaultPersData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Progression API: Default <see cref="miscData"/> of this mod.
        /// </summary>
        public virtual string defaultMiscData
        {
            get { return string.Empty; }
        }

        #region ProgCRUD

        // CRUD//ILSW
        internal void InitSave()
        {
            saveData = defaultSaveData;
        }

        internal void LoadSave(int slugNumber)
        {
            if (slugNumber < 0) throw new InvalidSlugcatException(this);
            saveData = ReadProgressionFile("save", slugNumber, seed, defaultSaveData);
        }

        internal void SaveSave(int slugNumber)
        {
            if (slugNumber < 0) throw new InvalidSlugcatException(this);
            if (saveData != defaultSaveData)
                WriteProgressionFile("save", slugNumber, seed, saveData);
        }

        internal void WipeSave(int slugNumber)
        {
            if (slugNumber == -1) DeleteAllProgressionFiles("save");
            else DeleteProgressionFile("save", slugNumber);
            if (slugNumber == slugcat) InitSave();
        }

        internal void InitPers()
        {
            persData = defaultPersData;
        }

        internal void LoadPers(int slugNumber)
        {
            if (slugNumber < 0) throw new InvalidSlugcatException(this);
            persData = ReadProgressionFile("pers", slugNumber, seed, defaultPersData);
        }

        internal void SavePers(int slugNumber)
        {
            if (slugNumber < 0) throw new InvalidSlugcatException(this);
            if (persData != defaultPersData)
                WriteProgressionFile("pers", slugNumber, seed, persData);
        }

        internal void WipePers(int slugNumber)
        {
            if (slugNumber == -1) DeleteAllProgressionFiles("pers");
            else DeleteProgressionFile("pers", slugNumber);
            if (slugNumber == slugcat) InitPers();
        }

        internal void InitMisc()
        {
            miscData = defaultMiscData;
        }

        internal void LoadMisc()
        {
            miscData = ReadProgressionFile("misc", -1, -1, defaultMiscData);
        }

        internal void SaveMisc()
        {
            if (miscData != defaultMiscData)
                WriteProgressionFile("misc", -1, -1, miscData);
        }

        internal void WipeMisc()
        {
            DeleteProgressionFile("misc", -1);
            InitMisc();
        }

        #endregion ProgCRUD

        #region ProgIO
        // progdata1_White.txt
        // progpers1_White.txt
        // progmisc1.txt

        private string GetTargetFilename(string file, string slugName)
        {
            return directory.FullName + Path.DirectorySeparatorChar +
                $"prog{file}{slot}{(string.IsNullOrEmpty(slugName) ? string.Empty : "_" + slugName)}.txt";
        }

        private string ReadProgressionFile(string file, int slugNumber, int validSeed, string defaultData)
        {
            // some locals here have the same name as class stuff and caused me a headache at some point
            if (!directory.Exists) return defaultData;

            string slugName = GetSlugcatName(slugNumber);
            string targetFile = GetTargetFilename(file, slugName);

            if (!File.Exists(targetFile)) return defaultData;

            string data = File.ReadAllText(targetFile, Encoding.UTF8);
            string key = data.Substring(0, 32);
            data = data.Substring(32, data.Length - 32);
            if (Custom.Md5Sum(data) != key)
            {
                Debug.Log($"{rwMod.ModID} progData file has been tinkered!");
                progDataTinkered = true;
            }
            else
            {
                data = Crypto.DecryptString(data, CryptoProgDataKey(slugName));
                string[] seedsplit = Regex.Split(data, "<Seed>"); // expected: <Seed>####<Seed>data
                if (seedsplit.Length >= 3)
                {
                    if (int.TryParse(seedsplit[1], out int seed) && seed == validSeed)
                        return seedsplit[2];
                }
            }
            return defaultData;
        }

        private void WriteProgressionFile(string file, int slugNumber, int validSeed, string data)
        {
            if (!directory.Exists) { directory.Create(); }
            string slugName = GetSlugcatName(slugNumber);
            string targetFile = GetTargetFilename(file, slugName);
            data = $"<Seed>{validSeed}<Seed>{data}";

            string enc = Crypto.EncryptString(data, CryptoProgDataKey(slugName));
            string key = Custom.Md5Sum(enc);

            File.WriteAllText(targetFile, key + enc);
        }

        private void DeleteProgressionFile(string file, int slugNumber)
        {
            if (!directory.Exists) return;
            string slugName = GetSlugcatName(slugNumber);
            string targetFile = GetTargetFilename(file, slugName);
            if (!File.Exists(targetFile)) return;
            // no backups for now I suppose
            File.Delete(targetFile);
        }

        private void DeleteAllProgressionFiles(string file)
        {
            if (!directory.Exists) return;
            foreach (var f in directory.GetFiles($"prog{file}{slot}_*.txt"))
            {
                f.Delete();
            }
        }

        #endregion ProgIO

        /// <summary>
        /// Progression API: An event called internally whenever CM has loaded this OIs progression through one of it's hooks.
        /// This happens when loading, initializing, wiping etc. When this event happens, all OIs progrdata has been loaded/initialized.
        /// Called regardless of there being an actual value change in save/pers/misc data.
        /// </summary>
        protected internal virtual void ProgressionLoaded()
        { }

        /// <summary>
        /// Progression API: An event called internally imediatelly before CM would save progression through one of it's hooks.
        /// This even exists so that the OI can serialize any objects it holds in memory before saving.
        /// When this is called, other OIs might not have yet serialized theirs however.
        /// </summary>
        protected internal virtual void ProgressionPreSave()
        { }

        /// <summary>
        /// Progression API: An event that happens when loading into the game, starving, quitting outside of the grace period, and death. Saves death-persistent data of a simulated or real death/quit.
        /// Rain World by default creates a save "as if the player died" when it loads into the game to counter the app unexpectedly closing, so do 'revert' any modifications you wanted to save after calling base() on this method.
        /// This method defaults to saving <see cref="persData"/>, so do any modifications to that to simulate a death/quit, call base(), and then revert your data.
        /// </summary>
        protected internal virtual void SaveDeath(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            SavePers(slugcat);
        }

        // HOOKPOINTS
        internal void InitProgression() // Called when the slot file isn't found on the game's side.
        {
            // To match the game having a fresh start, wipe all ?
            WipeSave(-1);
            WipePers(-1);
            WipeMisc();
        }

        internal void LoadProgression() // Called on load, slot-switch or post-wipe
        {
            InitSave();
            InitPers();
            LoadMisc();
        }

        internal void SaveProgression(bool saveState, bool savePers, bool saveMisc)
        {
            if (saveState) SaveSave(slugcat);
            if (savePers) SavePers(slugcat);
            if (saveMisc) SaveMisc();
        }

        internal void WipeProgression(int saveStateNumber)
        {
            WipeSave(saveStateNumber);
            WipePers(saveStateNumber);
            if (saveStateNumber == -1)
                WipeMisc();
        }

        internal void LoadSaveState()
        {
            LoadSave(slugcat);
            LoadPers(slugcat);
        }

        /// <summary>
        /// Currently selected saveslot
        /// </summary>
        public int slot => OptionScript.Slot;

        /// <summary>
        /// Currently selected slugcat
        /// </summary>
        public int slugcat => OptionScript.Slugcat;

        /// <summary>
        /// Seed of currently loaded savestate
        /// Used for validating loaded progression
        /// </summary>
        public int seed => OptionScript.rw.progression.currentSaveState != null ? OptionScript.rw.progression.currentSaveState.seed : -1;

        /// <summary>
        /// Reads the death-persistent data of the specified slugcat directly from its file, without replacing <see cref="persData"/>
        /// </summary>
        public string GetProgDataOfSlugcat(string slugName)
        {
            int slugNumber = GetSlugcatOfName(slugName);
            return ReadProgressionFile("pers", slugNumber, GetSlugcatSeed(slugNumber, slot), defaultPersData);
        }

        /// <summary>
        /// Reads the death-persistent data of the specified slugcat directly from its file, without replacing <see cref="persData"/>
        /// </summary>
        public string GetProgDataOfSlugcat(int slugcatNumber) => GetProgDataOfSlugcat(GetSlugcatName(slugcatNumber));

        /// <summary>
        /// Helper for getting the text name for a slugcat, which is used for filenames for data
        /// </summary>
        public static string GetSlugcatName(int slugcat)
        {
            if (slugcat < 0) return null;
            if (slugcat < 3) { return ((SlugcatStats.Name)slugcat).ToString(); }
            if (OptionScript.SlugBaseExists && IsSlugBaseSlugcat(slugcat)) { return GetSlugBaseSlugcatName(slugcat); }
            else { return ((SlugcatStats.Name)slugcat).ToString(); }
        }

        /// <summary>
        /// Reverse helper for getting the slugcat of a given text name
        /// </summary>
        public static int GetSlugcatOfName(string name)
        {
            // I tried to keep the same order as GetSlugcatName but...
            if (string.IsNullOrEmpty(name)) return -1;
            switch (name.ToLower())
            {
                case "white": return 0;
                case "yellow": return 1;
                case "red": return 2;
                default:
                    if (OptionScript.SlugBaseExists && IsSlugBaseName(name)) { return GetSlugBaseSlugcatOfName(name); }
                    break;
            }
            return (int)Enum.Parse(typeof(SlugcatStats.Name), name, true);
        }

        internal static int GetSlugcatSeed(int slugcat, int slot)
        {
            // Load from currently loaded save if available and valid
            SaveState save = OptionScript.rw?.progression?.currentSaveState;
            if (save != null && save.saveStateNumber == slugcat)
            {
                return save.seed;
            }
            // Load from slugbase custom save file
            if (OptionScript.SlugBaseExists && IsSlugBaseSlugcat(slugcat))
            {
                return GetSlugBaseSeed(slugcat, slot);
            }
            // Load from vanilla save file
            if (OptionScript.rw.progression.IsThereASavedGame(slugcat))
            {
                string[] progLines = OptionScript.rw.progression.GetProgLines();
                if (progLines.Length != 0)
                {
                    for (int i = 0; i < progLines.Length; i++)
                    {
                        string[] data = Regex.Split(progLines[i], "<progDivB>");
                        if (data.Length == 2 && data[0] == "SAVE STATE" && int.Parse(data[1][21].ToString()) == slugcat)
                        {
                            List<SaveStateMiner.Target> query = new List<SaveStateMiner.Target>()
                        {
                            new SaveStateMiner.Target(">SEED", "<svB>", "<svA>", 20)
                        };
                            List<SaveStateMiner.Result> result = SaveStateMiner.Mine(OptionScript.rw, data[1], query);
                            if (result.Count == 0) break;
                            try
                            {
                                return int.Parse(result[0].data);
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        #region SlugBase

        private static bool IsSlugBaseName(string name) => SlugBase.PlayerManager.GetCustomPlayer(name) != null;

        private static bool IsSlugBaseSlugcat(int slugcat) => SlugBase.PlayerManager.GetCustomPlayer(slugcat) != null;

        private static int GetSlugBaseSlugcatOfName(string name) => SlugBase.PlayerManager.GetCustomPlayer(name)?.SlugcatIndex ?? -1;

        private static string GetSlugBaseSlugcatName(int slugcat) => SlugBase.PlayerManager.GetCustomPlayer(slugcat)?.Name;

        private static int GetSlugBaseSeed(int slugcat, int slot)
        {
            SlugBase.SlugBaseCharacter ply = SlugBase.PlayerManager.GetCustomPlayer(slugcat);
            if (ply == null || !SlugBase.SaveManager.HasCustomSaveData(ply.Name, slot)) return -1;
            string saveData = File.ReadAllText(SlugBase.SaveManager.GetSaveFilePath(ply.Name, slot));
            List<SaveStateMiner.Target> query = new List<SaveStateMiner.Target>()
                {
                    new SaveStateMiner.Target(">SEED", "<svB>", "<svA>", 20)
                };
            List<SaveStateMiner.Result> result = SaveStateMiner.Mine(OptionScript.rw, saveData, query);
            if (result.Count != 0)
            {
                try
                {
                    return int.Parse(result[0].data);
                }
                catch (Exception) { }
            }
            return -1;
        }

        #endregion SlugBase

        protected string CryptoProgDataKey(string slugName) => "OptionalProgData" + (string.IsNullOrEmpty(slugName) ? "Misc" : slugName) + rwMod.ModID;

        #endregion progData
    }
}
