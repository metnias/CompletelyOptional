using CompletelyOptional;
using RWCustom;
using System;
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

            if (string.IsNullOrEmpty(_data))
            { Debug.LogException(new SaveDataException($"CompletelyOptional) {rwMod.ModID} data has nothing to be saved!")); return false; }
            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "data.txt"
                });
                string enc = Crypto.EncryptString(_data, CryptoDataKey);
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

        #endregion customData

        #region progData

        /// <summary>
        /// Default <see cref="progData"/> of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultProgData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Default <see cref="progPersData"/> of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultProgPersData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Default <see cref="progMiscData"/> of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultProgMiscData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Currently selected saveslot
        /// </summary>
        public int slot { get; private set; } = -1;

        /// <summary>
        /// Currently selected slugcat
        /// </summary>
        public int slugcat { get; private set; } = -1;

        private string _progData;
        private string _progPersData;
        private string _progMiscData;

        /// <summary>
        /// Progression Savedata tied to a specific slugcat. This gets reverted automatically if the Slugcat loses.
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// </summary>
        public string progData
        {
            get
            {
                if (!hasProgData)
                { hasProgData = true; slot = OptionScript.Slot; slugcat = OptionScript.Slugcat; LoadProgData(); }
                else if (slot != OptionScript.Slot || slugcat != OptionScript.Slugcat)
                { SlotOnChange(); }
                return _progData;
            }
            set
            {
                if (slot != OptionScript.Slot || slugcat != OptionScript.Slugcat)
                { SlotOnChange(); }
                if (_progData != value) { hasProgData = true; _progData = value; ProgDataOnChange(); }
            }
        }

        /// <summary>
        /// Progression Savedata tied to a specific slugcat.
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// </summary>
        public string progPersData
        {
            get
            {
                if (!hasProgData)
                { hasProgData = true; slot = OptionScript.Slot; slugcat = OptionScript.Slugcat; LoadProgData(); }
                else if (slot != OptionScript.Slot || slugcat != OptionScript.Slugcat)
                { SlotOnChange(); }
                return _progPersData;
            }
            set
            {
                if (slot != OptionScript.Slot || slugcat != OptionScript.Slugcat)
                { SlotOnChange(); }
                if (_progPersData != value) { hasProgData = true; _progPersData = value; ProgDataOnChange(); }
            }
        }

        public string GetProgDataOfSlugcat(string name)
        {
            return "";
        }

        public string GetProgDataOfSlugcat(int slugcatNumber) => GetProgDataOfSlugcat(GetSlugcatName(slugcatNumber));

        /// <summary>
        /// Progression Savedata shared across all slugcats on the same Saveslot.
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// </summary>
        public string progMiscData
        {
            get { hasProgData = true; return _progMiscData; }
            set { if (_progMiscData != value) { hasProgData = true; _progMiscData = value; ProgDataOnChange(); } }
        }

        /// <summary>
        /// Event when either <see cref="progData"/> or <see cref="progMiscData"/> is changed.
        /// This is called when 1. <see cref="LoadProgData"/>, 2. Your mod changes either <see cref="progData"/> or <see cref="progMiscData"/>.
        /// </summary>
        public virtual void ProgDataOnChange()
        {
        }

        /// <summary>
        /// Event that happens when either selected SaveSlot or Slugcat has been changed.
        /// This automatically saves and loads <see cref="progData"/> and <see cref="progMiscData"/> by default.
        /// </summary>
        public virtual void SlotOnChange()
        {
            // if (!hasProgData) { slot = OptionScript.Slot; slugcat = OptionScript.Slugcat; return; }
            // SaveProgData();
            slot = OptionScript.Slot; slugcat = OptionScript.Slugcat;
            if (hasProgData) LoadProgData();
        }

        /// <summary>
        /// Whether <see cref="progData"/> or <see cref="progMiscData"/> is used by the mod or not
        /// </summary>
        internal bool hasProgData = false;

        /// <summary>
        /// Whether the current <see cref="progData"/> is Saved as Death.
        /// </summary>
        public bool saveAsDeath { get; internal set; } = false;

        /// <summary>
        /// Whether the current <see cref="progData"/> is Saved as Quit.
        /// </summary>
        public bool saveAsQuit { get; internal set; } = false;

        public static string GetSlugcatName(int slugcat)
        {
            if (slugcat < 3 && slugcat >= 0) { return ((SlugcatStats.Name)slugcat).ToString(); }
            if (OptionScript.SlugBaseExists) { return GetSlugBaseSlugcatName(slugcat); }
            else { return ((SlugcatStats.Name)Math.Max(0, slugcat)).ToString(); }
        }

#if !STABLE

        internal static int GetSlugcatSeed(int slugcat)
        {
            return 0;
        }

#endif

        #region SlugBase

        private static string GetSlugBaseSlugcatName(int slugcat) => SlugBase.PlayerManager.GetCustomPlayer(slugcat).Name;

        #endregion SlugBase

        // progData1_White.txt
        // progPersData1_White.txt
        // progMiscData1.txt

        /// <summary>
        /// Loads <see cref="progData"/>. This is called automatically.
        /// Check <see cref="progDataTinkered"/> to see if saved data is tinkered or not.
        /// </summary>
        internal void LoadProgData()
        { // check also seed match for progdata
            _progData = defaultProgData;
            if (!directory.Exists) { Debug.Log("CompletelyOptional) Missing directory for " + this.rwMod.ModID); directory.Create(); return; }
            try
            {
                string data = string.Empty, name = GetSlugcatName(slugcat);
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name.Length < 12 + name.Length) { continue; }
                    if (file.Name.Substring(file.Name.Length - 4) != ".txt") { continue; }

                    if (file.Name.Substring(0, 8) == "progData")
                    {
                        if (slot.ToString() != file.Name.Substring(file.Name.Length - 9, 1)) { continue; }
                        if (!file.Name.Substring(10).StartsWith(name)) { continue; }
                    }
                    else { continue; }

                    //LoadProgData:
                    data = File.ReadAllText(file.FullName, Encoding.UTF8);
                    string key = data.Substring(0, 32);
                    data = data.Substring(32, data.Length - 32);
                    if (Custom.Md5Sum(data) != key)
                    {
                        Debug.Log($"{rwMod.ModID} progData file has been tinkered!");
                        progDataTinkered = true;
                    }
                    else
                    {
                        progDataTinkered = false;
                        data = Crypto.DecryptString(data, CryptoProgDataKey(slugcat));
                        string[] seedsplit = Regex.Split(data, "<Seed>");
                    }
                }

                // data :
                string[] raw = Regex.Split(data, "<miscData>");
                if (raw.Length > 1)
                {
                    _progMiscData = raw[1];
                    //Debug.Log("CM: Got misc data :" + raw[1]);
                }
                data = raw[0];
                //Debug.Log("CM: Got raw data :" + raw[0]);
                raw = Regex.Split(data, "<slugChar>");

#if !STABLE
                /*
                _progData = new string[Math.Max(_progData.Length, raw.Length)];
                for (int j = 0; j < raw.Length; j++)
                {
                    if (j == slugcat && _progData[j] != raw[j])
                    {
                        _progData[j] = raw[j];
                        ProgDataOnChange();
                    }
                    else _progData[j] = raw[j];
                }
                */
#endif
                return;
            }
            catch (Exception ex)
            {
                Debug.LogException(new LoadDataException(ex.ToString()));
            }
        }

        /// <summary>
        /// If you want to see whether your <see cref="progData"/> is tinkered or not.
        /// </summary>
        public bool progDataTinkered { get; private set; } = false;

        private static int GetSeedOfSlugcat(int slugcat)
        {
            return 0;
        }

        /// <summary>
        /// Saves <see cref="progData"/> and <see cref="progMiscData"/>. This is called automatically.
        /// </summary>
        /// <returns>Whether the saving is succeeded or not</returns>
        internal bool SaveProgData()
        {
            if (!directory.Exists) { directory.Create(); }

            string data = string.Empty;
            for (int i = 0; i < _progData.Length; i++) { data += _progData[i] + "<slugChar>"; };
            data += "<miscData>" + _progMiscData;
            //Debug.Log("CM: Saving data :" + data);
            //if (string.IsNullOrEmpty(_data)) { return false; }
            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "progData_",
                slot.ToString(),
                ".txt"
                });
                string enc = Crypto.EncryptString(data, CryptoProgDataKey(9));
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

        private string CryptoProgDataKey(int slugcat) => "OptionalProgData" + (slugcat < 0 ? "Misc" : slugcat.ToString()) + rwMod.ModID;

        /// <summary>
        /// Event that happens when the player starts a new save, resets the save, or resumed the game with this mod freshly installed(<paramref name="resume"/>).
        /// Default is to overwrite <see cref="progData"/> with <see cref="defaultProgData"/>.
        /// </summary>
        /// <param name="resume">Whether this is resuming the game with the mod freshly installed(<c>true</c>), or starting a new save/resetting save(<c>false</c>).</param>
        public virtual void OnNewSave(bool resume)
        {
            progData = defaultProgData;
        }

        /// <summary>
        /// Do not call this by your own.
        /// </summary>
        internal void ProgSaveOrLoad(ProgressData.SaveAndLoad saveOrLoad)
        {
            switch (saveOrLoad)
            {
                case ProgressData.SaveAndLoad.Save: SaveProgData(); break;
                case ProgressData.SaveAndLoad.Load: LoadProgData(); break;
            }
            if (slot != OptionScript.Slot || slugcat != OptionScript.Slugcat) { SlotOnChange(); }
        }

        #endregion progData
    }
}
