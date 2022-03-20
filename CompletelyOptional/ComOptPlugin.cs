﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using RWCustom;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion(CompletelyOptional.ComOptPlugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(CompletelyOptional.ComOptPlugin.PLUGIN_VERSION)]

namespace CompletelyOptional
{
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("RainWorld.exe")]
    public class ComOptPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.completelyoptional.plugin";
        public const string PLUGIN_NAME = "CompletelyOptional";
        public const string PLUGIN_VERSION = "2.0.0.0";

        public static readonly bool testing = true;

        #region AutoUpdate

        // Update URL - don't touch!
        // You can go to this in a browser (it's safe), but you might not understand the result.
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/2/0";

        // Version - increase this by 1 when you upload a new version of the mod.
        // The first upload should be with version 0, the next version 1, the next version 2, etc.
        public int version = 58;

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";

        public string keyN = "63oqj+JWQUUgPdZYGdMrZC1V8cNWBI6WYr04NxBKVPb1rrvDs8xRudZAuSdErhHyb9Qa6+ziG4GiXzfC8gElkSZ4uUfQgfomBgd4NACRCl+xHlIhyyYEFuexlj0pGK/OXKS8cX5zLqSXgGxnCfeqmHrs6pBvDrBnXHfWv1N6vOMBKmReRSioj2CBNJqYQxIY91Aoiyg+jf6AcJa/WOf9GmEr6OFcWQRkckf/GAcyq8EAV+BAQUOTfsZoYcFGqKGVmdFjMQokuuj0/Ut/zvd9SX/LwoIwCfZpfuGTBGM1+2+h9zd0US0eSG6en8QswahPukSKv2R+8uHfPBMCATzhgAoaf7xGM6zFwTgHlIODY4u8plB29OEOfelIYF0TSDVmuEPQFYXlNE2VsIUQLPuI4zjKxirEd2MQQUuek/P4X98SyPxmT53bLgoNPwfSXkUo4n2/b0gXgf7FCAU6XtFf1DZKwNo+3s6ESVidZUqYykTFRXQ5KLGHB6elC2oB/9J7m4AeysLDk+FU7AKnJ/VNwc6t+wq9d+8wolS6wBVFPbapprpJqRS5SYAdY+GpCSqO+IPIDZKYp1tQbx5J9egvr7zo5YiXDZmParFQk1Kc9ikLQgtIxk9D8qF4rYjcx96ZtMrDJM98vMuj9TdMzL5zqQRrfZpEAn7AYOlQs2uUP+E=";

        #endregion AutoUpdate

        /// <summary>
        /// Directory that all the data is saved
        /// </summary>
        public static DirectoryInfo directory;

        public static ComOptPlugin instance;

        public void Awake()
        {
            instance = this;

            directory = new DirectoryInfo(string.Concat(
                Custom.RootFolderDirectory(),
                "UserData",
                Path.DirectorySeparatorChar
                ));
            if (!directory.Exists) { directory.Create(); directory.Refresh(); }

            directory = new DirectoryInfo(string.Concat(
                Custom.RootFolderDirectory(),
                "UserData",
                Path.DirectorySeparatorChar,
                "ModData",
                Path.DirectorySeparatorChar
                ));
            if (!directory.Exists) { directory.Create(); directory.Refresh(); }

            OptionsMenuPatch.SubPatch();
            On.ProcessManager.ctor += ProcessManagerCtor;
            // ProgressData.SubPatch(); moved to post-initialization of OIs, makes no sense to hook before the OIs are even instantiated.

            #region ConfigEntries

            cfgPlayCustomSong = Config.Bind("Settings", "PlayCustomSong", true, "Whether to play custom song or not");
            dataStarredMods = Config.Bind("Data", "StarredMods", "", "List of starred mods"); // split by |

            #endregion ConfigEntries
        }

        internal ConfigEntry<bool> cfgPlayCustomSong;
        internal ConfigEntry<string> dataStarredMods;

        /// <summary>
        /// <see cref="RainWorld"/> instance
        /// </summary>
        public static RainWorld rw { get; internal set; }

        /// <summary>
        /// <see cref="ProcessManager"/> instance
        /// </summary>
        public static ProcessManager pm { get; internal set; }

        /// <summary>
        /// Catch <see cref="RainWorld"/> instance
        /// </summary>
        private static void ProcessManagerCtor(On.ProcessManager.orig_ctor orig, ProcessManager self, RainWorld rainWorld)
        {
            try
            {
                pm = self;
                rw = rainWorld;
            }
            finally
            {
                orig(self, rainWorld);
            }
        }

        #region curFramerate

        public static float curFramerate = 60.0f;

        private static readonly float[] dtHistory = new float[16];
        private static int dtHistoryMark = 0;

        public void Update()
        {
            dtHistory[dtHistoryMark] = Time.deltaTime;
            dtHistoryMark--; if (dtHistoryMark < 0) { dtHistoryMark = dtHistory.Length - 1; }
            float sum = 0;
            for (int h = 0; h < dtHistory.Length; h++) { sum += dtHistory[h]; }
            curFramerate = dtHistory.Length / sum;
        }

        #endregion curFramerate

        #region Logger

        private static ManualLogSource logSource => instance.Logger;

        internal static void LogMessage(object msg) => logSource.LogMessage(msg);

        internal static void LogInfo(object msg) => logSource.LogInfo(msg);

        internal static void LogWarning(object msg) => logSource.LogWarning(msg);

        internal static void LogError(object msg) => logSource.LogError(msg);

        #endregion Logger
    }
}