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
        /// <summary>
        /// Default Save Data of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultData
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Default Misc Save Data of this mod. If this isn't needed, just leave it be.
        /// </summary>
        public virtual string defaultMiscData
        {
            get { return string.Empty; }
        }

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

        private string[] _data;
        private string _miscdata;

        /// <summary>
        /// Save data tied to a specific slugcat
        /// Set this to whatever you want and call <see cref="SaveData"/> and <see cref="LoadData"/> when you need.
        /// </summary>
        public string data
        {
            get { return _data[slugcat]; }
            set { if (_data[slugcat] != value) { _data[slugcat] = value; DataOnChange(); } }
        }

        /// <summary>
        /// Save data shared across all slugcats on the same save-slot
        /// Set this to whatever you want and call <see cref="SaveData"/> and <see cref="LoadData"/> when you need.
        /// </summary>
        public string miscdata
        {
            get { return _miscdata; }
            set { if (_miscdata != value) { _miscdata = value; DataOnChange(); } }
        }

        /// <summary>
        /// Event when saved data is changed
        /// This is called when 1. <see cref="LoadData"/>, 2. Your mod changes data.
        /// </summary>
        public virtual void DataOnChange()
        {
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
            if (_data == null) GenerateDataArray(Mathf.Max(3, _slugcat)); // skipped menu/charselect :(
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = defaultData;
            }
            _miscdata = defaultMiscData;

            if (!directory.Exists) { Debug.Log("CompletelyOptional) Missing directory for " + this.rwMod.ModID); return; } // already set to default
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
                        Debug.Log($"{rwMod.ModID} data file has been tinkered!");
                        dataTinkered = true;
                    }
                    else
                    {
                        dataTinkered = false;
                    }
                    data = Crypto.DecryptString(data, string.Concat("OptionalData " + rwMod.ModID));
                }

                // data :
                string[] raw = Regex.Split(data, "<miscData>");
                if (raw.Length > 1)
                {
                    _miscdata = raw[1];
                    //Debug.Log("CM: Got misc data :" + raw[1]);
                }
                data = raw[0];
                //Debug.Log("CM: Got raw data :" + raw[0]);
                raw = Regex.Split(data, "<slugChar>");

                _data = new string[Math.Max(_data.Length, raw.Length)];
                for (int j = 0; j < raw.Length; j++)
                {
                    if (j == _slugcat && _data[j] != raw[j])
                    {
                        _data[j] = raw[j];
                        DataOnChange();
                    }
                    else _data[j] = raw[j];
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
            if (!directory.Exists) { directory.Create(); }

            string data = string.Empty;
            for (int i = 0; i < _data.Length; i++) { data += _data[i] + "<slugChar>"; };
            data += "<miscData>" + _miscdata;
            //Debug.Log("CM: Saving data :" + data);
            //if (string.IsNullOrEmpty(_data)) { return false; }
            try
            {
                string path = string.Concat(new object[] {
                directory.FullName,
                "data_",
                slot.ToString(),
                ".txt"
                });
                string enc = Crypto.EncryptString(data, string.Concat("OptionalData " + rwMod.ModID));
                string key = Custom.Md5Sum(enc);

                File.WriteAllText(path, key + enc);

                return true;
            }
            catch (Exception ex) { Debug.LogException(new SaveDataException(ex.ToString())); }

            return false;
        }

        /// <summary>
        /// Event that happens when the player starts a new save. Defaults to overiting data with defaultData.
        /// </summary>
        public virtual void OnNewSave()
        {
            data = defaultData;
        }

        /// <summary>
        /// Do not call this by your own.
        /// </summary>
        internal void BackgroundUpdate(int saveOrLoad)
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
        internal void GenerateDataArray(int slugcatLength)
        {
            _data = new string[slugcatLength];
            LoadData();
        }
    }
}
