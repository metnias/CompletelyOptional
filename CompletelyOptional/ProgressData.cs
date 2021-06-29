using Menu;
using OptionalUI;

namespace CompletelyOptional
{
    public static class ProgressData
    {
        public static void SubPatch()
        {
            On.Menu.SlugcatSelectMenu.ctor += new On.Menu.SlugcatSelectMenu.hook_ctor(SlugMenuCtorPatch);
            On.DeathPersistentSaveData.SaveToString += new On.DeathPersistentSaveData.hook_SaveToString(SaveToStringPatch);
            On.SaveState.SessionEnded += new On.SaveState.hook_SessionEnded(SessionEndPatch);
            On.SaveState.LoadGame += new On.SaveState.hook_LoadGame(LoadGamePatch);
            //On.PlayerProgression.WipeAll hmmmmm
        }

        internal enum SaveAndLoad
        {
            Update = 0, // Call Slot Update
            Save = 1,
            Load = 2
        }

        private static void SaveAndLoadOIs(SaveAndLoad saveOrLoad, bool force, bool saveAsDeath = false, bool saveAsQuit = false)
        {
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.ProgSaveOrLoad(saveOrLoad);
                    if (!force && saveOrLoad == SaveAndLoad.Save)
                    {
                        oi.saveAsDeath = saveAsDeath;
                        oi.saveAsQuit = saveAsQuit;
                    }
                }
            }
        }

        private static void SlugMenuCtorPatch(On.Menu.SlugcatSelectMenu.orig_ctor orig, SlugcatSelectMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);
            SaveAndLoadOIs(SaveAndLoad.Update, true);
        }

        private static void SessionEndPatch(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig.Invoke(self, game, survived, newMalnourished);
            SaveAndLoadOIs(SaveAndLoad.Load, false);
        }

        private static void LoadGamePatch(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig.Invoke(self, str, game);
            if (str == string.Empty)
            {
                // Fresh start/restart
                foreach (OptionInterface oi in OptionScript.loadedInterfaces)
                {
                    if (oi.hasProgData) { oi.OnNewSave(false); }
                }
            }
            else
            {
                SaveAndLoadOIs(SaveAndLoad.Load, false);
            }
        }

        private static string SaveToStringPatch(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result = orig.Invoke(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            SaveAndLoadOIs(SaveAndLoad.Save, false, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            return result;
        }
    }
}
