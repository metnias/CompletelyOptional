using Menu;
using OptionalUI;
using System;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public class CallerMemberNameAttribute : Attribute
    {
    }
}

namespace CompletelyOptional
{
    public static class ProgressData
    {
        private static bool _patched;
        internal static void SubPatch()
        {
            if (!_patched)
            { // Only run these hooks once
                _patched = true;
            } else return;

            // General progression
            // load or start fresh
            On.PlayerProgression.LoadProgression += PlayerProgression_LoadProgression;
            On.PlayerProgression.InitiateProgression += PlayerProgression_InitiateProgression;

            // Savestate instantiation/fetch
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState;
            On.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame; // lil bugfix

            // Reverts and clears
            On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
            On.PlayerProgression.WipeSaveState += PlayerProgression_WipeSaveState;
            // On.PlayerProgression.Revert += // reverts temp map data, not sure if relevant since it's not saving it and it'll be loading things from disk again

            // Saving
            On.PlayerProgression.SaveDeathPersistentDataOfCurrentState += PlayerProgression_SaveDeathPersistentDataOfCurrentState;
            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;

            // First call to progression ctor happens before we hook our hooks. OI initialization must call LoadOIsMisc();
        }

        #region HOOKS

        // HOOKS
        // General behavior: When saving/loading, save/load game first; when wiping, wipe mod first.
        // this way, the game's current state is always avaiable for mods to read.

        // Called trying to load a file
        internal static void PlayerProgression_LoadProgression(On.PlayerProgression.orig_LoadProgression orig, PlayerProgression self)
        {
            orig(self);
            LoadOIsProgression();
        }

        // Called when there's no file to load
        internal static void PlayerProgression_InitiateProgression(On.PlayerProgression.orig_InitiateProgression orig, PlayerProgression self)
        {
            orig(self);
            InitiateOIsProgression();
        }

        internal static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
        {
            WipeOIsProgression(-1);
            orig(self);
        }

        private static bool getOrInitSavePersLock = false;
        // Called with saveAsDeathOrQuit=true from StoryGameSession; =false from loading Red's statistics
        internal static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            bool loadedFromStarve = self.currentSaveState == null && self.starvedSaveState != null && self.starvedSaveState.saveStateNumber == saveStateNumber;
            bool loadedFromMemory = loadedFromStarve || (self.currentSaveState != null && self.currentSaveState.saveStateNumber == saveStateNumber);

            getOrInitSavePersLock = true;
            SaveState saveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
            getOrInitSavePersLock = false;
            LoadOIsSave(saveState, loadedFromMemory, loadedFromStarve);
            if (saveAsDeathOrQuit) SaveOIsPers(true, true);

            return saveState;
        }

        internal static void SlugcatSelectMenu_StartGame(On.Menu.SlugcatSelectMenu.orig_StartGame orig, SlugcatSelectMenu self, int storyGameCharacter)
        {
            orig(self, storyGameCharacter);
            // Bugfix to prevent crazy inconsistency that could happen if played restarted a save they just starved on
            // (vanilla would call 'Wipe' and still load the starve which is clearly a bug)
            
            if(self.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.New)
                self.manager.rainWorld.progression.starvedSaveState = null;
        }

        internal static void PlayerProgression_WipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, int saveStateNumber)
        {
            WipeOIsProgression(saveStateNumber);
            orig(self, saveStateNumber);
        }

        internal static void PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {
            orig(self, saveCurrentState, saveMaps, saveMiscProg);
            SaveOIsProgression(saveCurrentState, saveCurrentState, saveMiscProg);
        }

        internal static void PlayerProgression_SaveDeathPersistentDataOfCurrentState(On.PlayerProgression.orig_SaveDeathPersistentDataOfCurrentState orig, PlayerProgression self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
            if (getOrInitSavePersLock) return;
            SaveOIsPers(saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        #endregion HOOKS

        private static bool doLog = false;

        private static void LogMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (doLog) UnityEngine.Debug.Log(memberName);
        }

        internal static void RunPreSave()
        {
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.ProgressionPreSave();
                }
            }
        }

        internal static void RunPostLoaded()
        {
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.ProgressionLoaded();
                }
            }
        }

        internal static void LoadOIsProgression()
        {
            LogMethodName();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.LoadProgression();
                }
            }
            RunPostLoaded();
        }

        internal static void InitiateOIsProgression()
        {
            LogMethodName();

            RunPreSave();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.InitProgression();
                }
            }
            RunPostLoaded();
        }


        internal static void WipeOIsProgression(int saveStateNumber)
        {
            LogMethodName();
            RunPreSave();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.WipeProgression(saveStateNumber); // Todo add a chance to clear/keep misc data ?
                }
            }
            RunPostLoaded();
        }

        internal static void LoadOIsSave(SaveState saveState, bool loadedFromMemory, bool loadedFromStarve)
        {
            LogMethodName();
            if (loadedFromMemory) return; // We're good ? Not too sure when this happens
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.LoadSaveState();
                }
            }
            RunPostLoaded();
        }

        internal static void SaveOIsProgression(bool saveState, bool savePers, bool saveMisc)
        {
            LogMethodName();
            RunPreSave();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.SaveProgression(saveState, savePers, saveMisc);
                }
            }
        }

        internal static void SaveOIsPers(bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            LogMethodName();
            if (!(saveAsIfPlayerDied || saveAsIfPlayerQuit))
            {
                RunPreSave();
            }
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
                        oi.SaveDeath(saveAsIfPlayerDied, saveAsIfPlayerQuit);
                    else
                        oi.SaveProgression(false, true, false);
                }
            }
        }
    }
}
