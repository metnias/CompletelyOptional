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
        /// Default <see cref="progMiscData"/> of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultProgMiscData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Currently selected saveslot
        /// </summary>
        public static int slot => OptionScript.Slot;

        private static int _slot;

        /// <summary>
        /// Currently selected slugcat
        /// </summary>
        public static int slugcat => OptionScript.Slugcat;

        private static int _slugcat;

        private string[] _progData;
        private string _progMiscData;

        /// <summary>
        /// Progression Savedata tied to a specific slugcat.
        /// Set this to whatever you want in game. Config Machine will then manage saving automatically.
        /// </summary>
        public string progData
        {
            get
            {
                hasProgData = true;
                if (slugcat > 0 && slugcat >= _progData.Length) { GenerateDataArray(slugcat); }
                return _progData[slugcat];
            }
            set { if (_progData[slugcat] != value) { hasProgData = true; _progData[slugcat] = value; ProgDataOnChange(); } }
        }

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
        /// Event that happens when selected SaveSlot has been changed.
        /// This automatically saves and loads <see cref="progData"/> and <see cref="progMiscData"/> by default.
        /// </summary>
        public virtual void SlotOnChange()
        {
            if (!hasProgData) { _slot = slot; _slugcat = slugcat; return; }
            SaveProgData();
            _slot = slot; _slugcat = slugcat;
            LoadProgData();
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

        /// <summary>
        /// Loads <see cref="progData"/> and <see cref="progMiscData"/>. This is called automatically.
        /// Check <see cref="progDataTinkered"/> to see if saved data is tinkered or not.
        /// </summary>
        internal void LoadProgData()
        {
            if (_progData == null) GenerateDataArray(Mathf.Max(3, _slugcat)); // skipped menu/charselect :(
            for (int i = 0; i < _progData.Length; i++)
            {
                _progData[i] = defaultData;
            }
            _progMiscData = defaultProgMiscData;

            if (!directory.Exists) { Debug.Log("CompletelyOptional) Missing directory for " + this.rwMod.ModID); return; } // already set to default
            try
            {
                string data = string.Empty;
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name.Length < 12) { continue; }
                    if (file.Name.Substring(file.Name.Length - 4) != ".txt") { continue; }

                    if (file.Name.Substring(0, 8) == "progData")
                    {
                        if (slot.ToString() != file.Name.Substring(file.Name.Length - 9, 1)) { continue; }
                    }
                    else { continue; }

                    //LoadSlotData:
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
                    }
                    data = Crypto.DecryptString(data, string.Concat("OptionalProgData" + rwMod.ModID));
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

                _progData = new string[Math.Max(_progData.Length, raw.Length)];
                for (int j = 0; j < raw.Length; j++)
                {
                    if (j == _slugcat && _progData[j] != raw[j])
                    {
                        _progData[j] = raw[j];
                        ProgDataOnChange();
                    }
                    else _progData[j] = raw[j];
                }
                return;
            }
            catch (Exception ex) { Debug.LogException(new LoadDataException(ex.ToString())); }
        }

        /// <summary>
        /// If you want to see whether your <see cref="progData"/> is tinkered or not.
        /// </summary>
        public bool progDataTinkered { get; private set; } = false;

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
                string enc = Crypto.EncryptString(data, string.Concat("OptionalProgData" + rwMod.ModID));
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

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
            if (_slot != slot) { SlotOnChange(); }
            else if (_slugcat != slugcat) { SlotOnChange(); }
        }

        /// <summary>
        /// Do not call this by your own.
        /// </summary>
        /// <param name="slugcatLength"></param>
        internal void GenerateDataArray(int slugcatLength)
        {
            _progData = new string[slugcatLength];
        }

        #endregion progData
    }
}
