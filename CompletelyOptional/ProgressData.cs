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
        }

        private static void LoadAndSaveOI(int saveOrLoad, bool force, bool saveAsDeath = false, bool saveAsQuit = false)
        {
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (force) { oi.BackgroundUpdate(saveOrLoad); }
                else if (oi.progressData)
                {
                    oi.BackgroundUpdate(saveOrLoad);
                    if (saveOrLoad == 1)
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
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                oi.GenerateDataArray(self.slugcatColorOrder.Length);
            }
            LoadAndSaveOI(0, true);
        }

        private static void SessionEndPatch(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig.Invoke(self, game, survived, newMalnourished);
            LoadAndSaveOI(2, false);
        }

        private static void LoadGamePatch(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig.Invoke(self, str, game);
            LoadAndSaveOI(2, false);
        }

        private static string SaveToStringPatch(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            string result = orig.Invoke(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            LoadAndSaveOI(1, false, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            return result;
        }
    }
}