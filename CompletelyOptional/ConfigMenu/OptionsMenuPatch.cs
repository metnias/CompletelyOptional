using Menu;
using Music;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// These code attach themselves to OptionsMenu and ProcessManager for transitioning with ModConfigMenu
    /// </summary>
    internal static class OptionsMenuPatch
    {
        internal static void SubPatch()
        {
            On.Menu.OptionsMenu.ctor += CtorPatch;
            On.Menu.OptionsMenu.UpdateInfoText += UpdateInfoTextPatch;
            On.Menu.OptionsMenu.Singal += SingalPatch;
            On.Menu.OptionsMenu.ShutDownProcess += ShutDownProcessPatch;
            On.ProcessManager.SwitchMainProcess += SwitchMainProcessPatch;
        }

        /// <summary>
        /// Add MOD CONFIG Button
        /// </summary>
        private static void CtorPatch(On.Menu.OptionsMenu.orig_ctor orig, OptionsMenu menu, ProcessManager manager)
        {
            orig(menu, manager);

            string t = InternalTranslator.Translate("MOD CONFIG");
            enterConfig = new SimpleButton(menu, menu.pages[0], t, "MOD CONFIG", new Vector2(340f, 50f), new Vector2(Mathf.Max(110f, t.Length * 9f + 15f), 30f));
            menu.pages[0].subObjects.Add(enterConfig);
            menu.backButton.nextSelectable[2] = enterConfig;
            enterConfig.nextSelectable[1] = menu.soundSlider;
            enterConfig.nextSelectable[0] = menu.backButton;
            enterConfig.nextSelectable[2] = menu.creditsButton;
            menu.creditsButton.nextSelectable[0] = enterConfig;
        }

        /// <summary>
        /// Display InfoText for MOD CONFIG Button
        /// </summary>
        private static string UpdateInfoTextPatch(On.Menu.OptionsMenu.orig_UpdateInfoText orig, OptionsMenu menu)
        {
            //Code
            if (menu.selectedObject == enterConfig)
            { return InternalTranslator.Translate("Configure Settings for Rain World Mods"); }
            return orig(menu);
        }

        /// <summary>
        /// Add function to MOD CONFIG Button
        /// </summary>
        private static void SingalPatch(On.Menu.OptionsMenu.orig_Singal orig, OptionsMenu menu, MenuObject sender, string message)
        {
            if (message == "MOD CONFIG")
            {
                menu.PlaySound(SoundID.MENU_Switch_Page_In);
                menu.manager.RequestMainProcessSwitch(EnumExt_ComOpt.ModConfigMenu);
                menu.manager.rainWorld.options.Save();
                return;
            }
            orig(menu, sender, message);
        }

        /// <summary>
        /// Shutdown Mod Config music and remove the residue button
        /// </summary>
        private static void ShutDownProcessPatch(On.Menu.OptionsMenu.orig_ShutDownProcess orig, OptionsMenu menu)
        {
            orig(menu);

            if (menu.manager.musicPlayer != null) //Shutdown ModConfig Music
            {
                string songid = menu.manager.musicPlayer.song?.name.Substring(0, 5).ToUpper();
                if (songid != "RW_8 " && songid != "TITLE")
                {
                    ComOptPlugin.LogMessage($"Shutdown ModConfig Music :{menu.manager.musicPlayer.song?.name}");
                    menu.manager.musicPlayer.nextSong = new MenuOrSlideShowSong(menu.manager.musicPlayer, "RW_8 - Sundown", 0.8f, 2f)
                    { playWhenReady = false };
                }
            }
            if (enterConfig != null) // Remove MOD CONFIG Button
            {
                enterConfig.RemoveSprites();
                enterConfig = null;
            }
        }

        /// <summary>
        /// Transitioning to ModConfigMenu
        /// </summary>
        private static void SwitchMainProcessPatch(On.ProcessManager.orig_SwitchMainProcess orig, ProcessManager pm, ProcessManager.ProcessID ID)
        {
            if (ID == EnumExt_ComOpt.ModConfigMenu) // Switch to ModConfigMenu
            {
                MainLoopProcess oldMenu = pm.currentMainLoop;
                orig(pm, ID);
                pm.currentMainLoop = new ModConfigMenu(pm);
                oldMenu.CommunicateWithUpcomingProcess(pm.currentMainLoop);
                return;
            }
            orig(pm, ID);
        }

        private static SimpleButton enterConfig;
    }
}