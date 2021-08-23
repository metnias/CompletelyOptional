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
        /// </summary>
        public string data
        {
            get { return _data; }
            set { if (_data != value) { _data = value; DataOnChange(); } }
        }

        /// <summary>
        /// Event when either <see cref="data"/> is changed
        /// This is called when 1. <see cref="LoadData"/>, 2. Your mod changes <see cref="data"/>.
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
        // Progression API
        // Data that is stored/retrieved mimicking the game's behavior
        // save -> SaveState
        // pers -> DeathPersistentData
        // misc -> Progression & MiscProgressionData
        // See https://media.discordapp.net/attachments/849525819855863809/877611189444698113/unknown.png
        private class NoProgDataException : InvalidOperationException
        {
            public NoProgDataException(OptionInterface oi) : base($"OptionInterface {oi.rwMod.ModID} hasn't enabled hasProgData") { }
        }

        private class InvalidSlugcatException : ArgumentException
        {
            public InvalidSlugcatException(OptionInterface oi) : base($"OptionInterface {oi.rwMod.ModID} tried to use an invalid Slugcat number") { }
        }

        /// <summary>
        /// Progression API: Whether <see cref="saveData"/> or <see cref="miscData"/> is used by the mod or not
        /// </summary>
        internal protected bool hasProgData = false;

        /// <summary>
        /// If you want to see whether your <see cref="saveData"/> is tinkered or not.
        /// When this happens defaultData will be used instead of loading from the file.
        /// </summary>
        public bool progDataTinkered { get; private set; } = false;


        private string _saveData;
        private string _persData;
        private string _miscData;

        /// <summary>
        /// Progression API: Savedata tied to a specific slugcat's playthrough. This gets reverted automatically if the Slugcat loses.
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
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
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
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
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
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
            if(slugNumber == slugcat) InitSave();
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

        private string GetTargetFilename(string file, int slugNumber)
        {
            return directory.FullName + Path.DirectorySeparatorChar +
                $"prog{file}{slot}{(slugNumber >= 0 ? "_" + GetSlugcatName(slugNumber) : string.Empty)}.txt";
        }

        private string ReadProgressionFile(string file, int slugNumber, int validSeed, string defaultData)
        {
            if (!directory.Exists) return defaultData;

            string targetFile = GetTargetFilename(file, slugNumber);
            
            if(!File.Exists(targetFile)) return defaultData;

            data = File.ReadAllText(targetFile, Encoding.UTF8);
            string key = data.Substring(0, 32);
            data = data.Substring(32, data.Length - 32);
            if (Custom.Md5Sum(data) != key)
            {
                Debug.Log($"{rwMod.ModID} progData file has been tinkered!");
                progDataTinkered = true;
            }
            else
            {
                data = Crypto.DecryptString(data, CryptoProgDataKey(slugcat));
                string[] seedsplit = Regex.Split(data, "<Seed>"); // expected: <Seed>####<Seed>otherjunk
                if (seedsplit.Length > 1)
                {
                    if (int.TryParse(seedsplit[1], out int seed) && seed == validSeed)
                        return data;
                }
            }
            return defaultData;
        }

        private void WriteProgressionFile(string file, int slugNumber, int validSeed, string data)
        {
            if (!directory.Exists) { directory.Create(); }
            string targetFile = GetTargetFilename(file, slugNumber);
            data = $"<Seed>{validSeed}<Seed>{data}";

            string enc = Crypto.EncryptString(data, CryptoProgDataKey(slugNumber));
            string key = Custom.Md5Sum(enc);

            File.WriteAllText(targetFile, key + enc);
        }

        private void DeleteProgressionFile(string file, int slugNumber)
        {
            if (!directory.Exists) return;
            string targetFile = GetTargetFilename(file, slugNumber);
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
        /// An event called internally whenever CM has changed this OIs progression through one of it's hooks
        /// This happens when loading, initializing, wiping etc
        /// Called regardless of there being an actual value change in save/pers/misc data
        /// </summary>
        internal protected virtual void ProgressionChanged(bool saveAndPers, bool misc) { }

        internal protected virtual void ProgressionPreSave() { }


        // HOOKPOINTS
        internal void InitProgression() // Called when the slot file isn't found on the game's side.
        {
            // To match the game having a fresh start, wipe all ?
            ProgressionPreSave();
            WipeSave(-1);
            WipePers(-1);
            WipeMisc();
            ProgressionChanged(true, true);
        }

        internal void LoadProgression() // Called on load, slot-switch or post-wipe
        {
            InitSave();
            InitPers();
            LoadMisc();
            ProgressionChanged(true, true);
        }

        internal void SaveProgression(bool saveState, bool savePers, bool saveMisc)
        {
            ProgressionPreSave();
            if (saveState) SaveSave(slugcat);
            if (savePers) SavePers(slugcat);
            if (saveMisc) SaveMisc();
        }

        internal void WipeProgression(int saveStateNumber)
        {
            ProgressionPreSave();
            WipeSave(saveStateNumber);
            WipePers(saveStateNumber);
            if(saveStateNumber == -1)
                WipeMisc();
            ProgressionChanged(true, saveStateNumber == -1);
        }

        internal void LoadSave(SaveState saveState, bool loadedFromMemory, bool loadedFromStarve)
        {
            if (loadedFromMemory && seed == saveState.seed && slugcat == saveState.saveStateNumber) return; // We're good ? Not too sure when this happens

            seed = saveState.seed;
            slugcat = saveState.saveStateNumber;

            LoadSave(slugcat);
            LoadPers(slugcat);
            ProgressionChanged(true, false);
        }

        internal protected virtual void SaveSimulatedDeath(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            SavePers(slugcat);
        }







        /// <summary>
        /// Currently selected saveslot
        /// </summary>
        public int slot { get; private set; } = -1;

        /// <summary>
        /// Currently selected slugcat
        /// </summary>
        public int slugcat { get; private set; } = -1;

        /// <summary>
        /// Seed of currently loaded savestate
        /// Used for validating loaded progression
        /// </summary>
        public int seed { get; private set; } = -1;


        public string GetProgDataOfSlugcat(string name)
        {
#warning GetProgDataOfSlugcat not implemented
            throw new NotImplementedException("GetProgDataOfSlugcat not implemented");
        }

        public string GetProgDataOfSlugcat(int slugcatNumber) => GetProgDataOfSlugcat(GetSlugcatName(slugcatNumber));


        public static string GetSlugcatName(int slugcat)
        {
            if (slugcat < 3 && slugcat >= 0) { return ((SlugcatStats.Name)slugcat).ToString(); }
            if (OptionScript.SlugBaseExists && IsSlugBaseSlugcat(slugcat)) { return GetSlugBaseSlugcatName(slugcat); }
            else { return ((SlugcatStats.Name)Math.Max(0, slugcat)).ToString(); }
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

        private static bool IsSlugBaseSlugcat(int slugcat) => SlugBase.PlayerManager.GetCustomPlayer(slugcat) != null;
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

        private string CryptoProgDataKey(int slugcat) => "OptionalProgData" + (slugcat < 0 ? "Misc" : slugcat.ToString()) + rwMod.ModID;
        #endregion progData
    }
}
