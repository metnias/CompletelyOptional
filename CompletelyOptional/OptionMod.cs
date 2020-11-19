using Partiality.Modloader;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// PartialityMod part of CompletelyOptional Mod
    /// </summary>
    public class OptionMod : PartialityMod
    {
        public OptionMod()
        {
            ModID = "ConfigMachine";
            Version = "1443"; //Assembly.GetEntryAssembly().GetName().Version.ToString();
            author = "topicular";
            instance = this;
        }

        public const bool testing = false;
        public const bool soundTestEnabled = false;

        // Update URL - don't touch!
        // You can go to this in a browser (it's safe), but you might not understand the result.
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/2/0";

        // Version - increase this by 1 when you upload a new version of the mod.
        // The first upload should be with version 0, the next version 1, the next version 2, etc.
        public int version = 41;

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";

        public string keyN = "63oqj+JWQUUgPdZYGdMrZC1V8cNWBI6WYr04NxBKVPb1rrvDs8xRudZAuSdErhHyb9Qa6+ziG4GiXzfC8gElkSZ4uUfQgfomBgd4NACRCl+xHlIhyyYEFuexlj0pGK/OXKS8cX5zLqSXgGxnCfeqmHrs6pBvDrBnXHfWv1N6vOMBKmReRSioj2CBNJqYQxIY91Aoiyg+jf6AcJa/WOf9GmEr6OFcWQRkckf/GAcyq8EAV+BAQUOTfsZoYcFGqKGVmdFjMQokuuj0/Ut/zvd9SX/LwoIwCfZpfuGTBGM1+2+h9zd0US0eSG6en8QswahPukSKv2R+8uHfPBMCATzhgAoaf7xGM6zFwTgHlIODY4u8plB29OEOfelIYF0TSDVmuEPQFYXlNE2VsIUQLPuI4zjKxirEd2MQQUuek/P4X98SyPxmT53bLgoNPwfSXkUo4n2/b0gXgf7FCAU6XtFf1DZKwNo+3s6ESVidZUqYykTFRXQ5KLGHB6elC2oB/9J7m4AeysLDk+FU7AKnJ/VNwc6t+wq9d+8wolS6wBVFPbapprpJqRS5SYAdY+GpCSqO+IPIDZKYp1tQbx5J9egvr7zo5YiXDZmParFQk1Kc9ikLQgtIxk9D8qF4rYjcx96ZtMrDJM98vMuj9TdMzL5zqQRrfZpEAn7AYOlQs2uUP+E=";

        /// <summary>
        /// Directory that all the data/configs is saved
        /// </summary>
        public static DirectoryInfo directory;

        /// <summary>
        /// GameObject for CompletelyOptional MonoBehavior
        /// </summary>
        public static GameObject go;

        /// <summary>
        /// CompletelyOptional MonoBehavior
        /// </summary>
        public static OptionScript script;

        public static Dictionary<string, string> songNameDict;

        public static OptionMod instance;

        /// <summary>
        /// Create GameObject and remove remaining junk
        /// </summary>
        public override void OnEnable()
        {
            directory = new DirectoryInfo(string.Concat(
                Custom.RootFolderDirectory(),
                "ModConfigs",
                Path.DirectorySeparatorChar
                ));

            base.OnEnable();

            On.Menu.OptionsMenu.UpdateInfoText += new On.Menu.OptionsMenu.hook_UpdateInfoText(OptionsMenuPatch.UpdateInfoTextPatch);
            On.Menu.OptionsMenu.Update += new On.Menu.OptionsMenu.hook_Update(OptionsMenuPatch.UpdatePatch);
            On.Menu.OptionsMenu.Singal += new On.Menu.OptionsMenu.hook_Singal(OptionsMenuPatch.SingalPatch);
            On.Menu.OptionsMenu.ShutDownProcess += new On.Menu.OptionsMenu.hook_ShutDownProcess(OptionsMenuPatch.ShutDownProcessPatch);

            go = new GameObject("OptionController");
            script = go.AddComponent<OptionScript>();
            //OptionScript.mod = this;

            /*
            if (File.Exists(string.Concat(levelpath, "SoundTest.txt"))){
                RemoveMusicRoom();
            }*/
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        private static readonly string[] playlistMoody =
        {
            "NA_07 - Phasing", //16%
            "NA_11 - Reminiscence",
            "NA_18 - Glass Arcs",
            "NA_19 - Halcyon Memories",
            "NA_21 - New Terra",
        };

        private static readonly string[] playlistWork =
        {
            "NA_20 - Crystalline", //7%
            "NA_29 - Flutter",
            "NA_30 - Distance"
        };

        /// <summary>
        /// List of Random Song gets played in ConfigMenu,
        /// carefully chosen by me :P
        /// </summary>
        public static string randomSong
        {
            get
            {
                if (UnityEngine.Random.value < 0.8f)
                { return playlistMoody[UnityEngine.Random.Range(0, 4)]; }
                else
                { return playlistWork[UnityEngine.Random.Range(0, 2)]; }
            }
        }

        /// <summary>
        /// Path of Levels
        /// </summary>
        public static string levelpath = string.Concat(new object[] {
            Custom.RootFolderDirectory(),
            "Levels",
            Path.DirectorySeparatorChar
        });

        /*
        /// <summary>
        /// Creates SoundTest Room files
        /// </summary>
        public static void CopyMusicRoom()
        {
            string sspath = string.Concat(new object[] {
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                "SS",
                Path.DirectorySeparatorChar,
                "Rooms",
                Path.DirectorySeparatorChar
            });

            File.Copy(string.Concat(sspath, "SS_AI.txt"), string.Concat(levelpath, "SoundTest.txt"), true);
            File.Copy(string.Concat(sspath, "SS_AI_1.png"), string.Concat(levelpath, "SoundTest_1.png"), true);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "CompletelyOptional.SoundTest.MaxRoom_Settings.txt";
            string result;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            File.WriteAllText(string.Concat(levelpath, "SoundTest_Settings.txt"), result);
        }

        /// <summary>
        /// Removes SoundTest room files to prevent unwanted predicaments
        /// </summary>
        public static void RemoveMusicRoom()
        {
            File.Delete(string.Concat(levelpath, "SoundTest.txt"));
            File.Delete(string.Concat(levelpath, "SoundTest_1.png"));
            File.Delete(string.Concat(levelpath, "SoundTest_Settings.txt"));
        }
        */
    }
}
