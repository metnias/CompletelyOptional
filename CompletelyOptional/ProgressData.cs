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
            // On.PlayerProgression.Revert +=

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
            LoadOIsMisc();
        }

        // Called when there's no file to load
        internal static void PlayerProgression_InitiateProgression(On.PlayerProgression.orig_InitiateProgression orig, PlayerProgression self)
        {
            orig(self);
            InitiateOIsMisc();
        }

        internal static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
        {
            WipeOIsProgression(-1);
            orig(self);
        }

        // Called with saveAsDeathOrQuit=true from StoryGameSession; =false from loading Red's statistics
        internal static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
        {
            bool loadedFromStarve = self.currentSaveState == null && self.starvedSaveState != null && self.starvedSaveState.saveStateNumber == saveStateNumber;
            bool loadedFromMemory = loadedFromStarve || (self.currentSaveState != null && self.currentSaveState.saveStateNumber == saveStateNumber);

            SaveState saveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
            
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
            SaveOIsPers(saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        #endregion HOOKS

        private const bool doLog = true;

        private static void DebugLog(string text = "", [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (doLog) UnityEngine.Debug.Log(memberName + " : " + text);
        }

        internal static void LoadOIsMisc()
        {
            DebugLog();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.LoadMisc();
                    oi.InitSave();
                    oi.InitPers();
                }
            }
        }

        internal static void InitiateOIsMisc()
        {
            DebugLog();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.InitMisc();
                    oi.InitSave();
                    oi.InitPers();
                }
            }
        }


        internal static void WipeOIsProgression(int saveStateNumber)
        {
            DebugLog();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.WipeProgression(saveStateNumber); // Has a chance to clear/keep misc data ?
                }
            }
        }

        internal static void LoadOIsSave(SaveState saveState, bool loadedFromMemory, bool loadedFromStarve)
        {
            DebugLog();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    oi.LoadSave(saveState, loadedFromMemory, loadedFromStarve);
                }
            }
        }

        internal static void SaveOIsProgression(bool saveState, bool savePers, bool saveMisc)
        {
            DebugLog();
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
            DebugLog();
            foreach (OptionInterface oi in OptionScript.loadedInterfaces)
            {
                if (oi.hasProgData)
                {
                    if (saveAsIfPlayerDied || saveAsIfPlayerQuit)
                        oi.SaveSimulatedDeath(saveAsIfPlayerDied, saveAsIfPlayerQuit);
                    else
                        oi.SaveProgression(false, true, false);
                }
            }
        }
    }
}
