using Menu;
using Music;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// These code attach themselves to OptionsMenu.
    /// </summary>
    public static class OptionsMenuPatch
    {
#pragma warning disable IDE0060

        public static string UpdateInfoTextPatch(On.Menu.OptionsMenu.orig_UpdateInfoText orig, OptionsMenu menu)
        {
            //Code
            if (menu.selectedObject == enterConfig)
            { return InternalTranslator.Translate("Configure Settings for Rain World Mods"); }
            return orig.Invoke(menu);
        }

        public static void UpdatePatch(On.Menu.OptionsMenu.orig_Update orig, OptionsMenu menu)
        {
            if (mod)
            {
                //modmenu.Update();
                menu.manager.currentMainLoop = null;
                //menu.manager.soundLoader.ReleaseAllUnityAudio();
                menu.processActive = false;

                menu.manager.currentMainLoop = new ConfigMenu(menu.manager);
                modmenu = menu.manager.currentMainLoop as ConfigMenu;
                modmenu.vanillaMenu = menu;

                return;
            }
            else if (modmenu != null)
            {
                modmenu.ShutDownProcess();
                modmenu = null;
            }

            if (menu.manager.currentMainLoop != menu)
            {
                menu.ShutDownProcess();
                return;
            }
            if (!mod && enterConfig == null)
            { //ctor
                string t = InternalTranslator.Translate("MOD CONFIG");
                enterConfig = new SimpleButton(menu, menu.pages[0], t, "MOD CONFIG", new Vector2(340f, 50f), new Vector2(Mathf.Max(110f, t.Length * 9f + 15f), 30f));
                menu.pages[0].subObjects.Add(enterConfig);
                //menu.manager.musicPlayer.MenuRequestsSong(CompletelyOptional.ConfigManager.randomSong, 2f, 2f);
                menu.backButton.nextSelectable[2] = enterConfig;
                enterConfig.nextSelectable[1] = menu.soundSlider;
                enterConfig.nextSelectable[0] = menu.backButton;
                enterConfig.nextSelectable[2] = menu.creditsButton;
                menu.creditsButton.nextSelectable[0] = enterConfig;
            }

            orig.Invoke(menu);
            /*
            if (resolutionDirty)
            {
                resolutionDirty = false;
                Screen.SetResolution((int)menu.manager.rainWorld.options.ScreenSize.x, (int)menu.manager.rainWorld.options.ScreenSize.y, false);
                Screen.fullScreen = false;
                Screen.showCursor = true;
                menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
            }
            */
        }

        public static void SingalPatch(On.Menu.OptionsMenu.orig_Singal orig, OptionsMenu menu, MenuObject sender, string message)
        {
            if (message == "MOD CONFIG")
            {
                mod = true;
                menu.PlaySound(SoundID.MENU_Switch_Page_In);
                menu.manager.rainWorld.options.Save();
                //this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
                menu.manager.soundLoader.ReleaseAllUnityAudio();

                modmenu = new ConfigMenu(menu.manager);
                menu.manager.currentMainLoop = modmenu;
                modmenu.vanillaMenu = menu;
                modmenu.OpenMenu();
            }
            else
            {
                orig.Invoke(menu, sender, message);
            }
        }

        public static void ShutDownProcessPatch(On.Menu.OptionsMenu.orig_ShutDownProcess orig, OptionsMenu menu)
        {
            orig.Invoke(menu);

            if (OptionScript.ComModExists)
            {
                if (menu.CurrLang != InGameTranslator.LanguageID.Portuguese)
                {
                    OptionScript.curLang = (string)OptionScript.ComMod.GetType().GetField("customLang", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(OptionScript.ComMod);
                }
                else { OptionScript.curLang = "eng"; }
            }
            else
            {
                OptionScript.curLang = OptionScript.ID2Code[(int)menu.CurrLang];
            }
            InternalTranslator.LoadTranslation();

            string songid = "";
            if (menu.manager.musicPlayer != null)
            {
                songid = menu.manager.musicPlayer.song?.name.Substring(0, 5);
            }

            if (!mod)
            { //going back to main menu
                if (menu.manager.musicPlayer != null && songid != "RW_8 " && songid != "Title")
                {
                    Debug.Log(string.Concat("Shutdown Option Music :" + menu.manager.musicPlayer.song?.name));
                    menu.manager.musicPlayer.nextSong = new MenuOrSlideShowSong(menu.manager.musicPlayer, "RW_8 - Sundown", 0.8f, 2f)
                    {
                        playWhenReady = false
                    };
                }
            }
            if (enterConfig != null)
            {
                enterConfig.RemoveSprites();
                enterConfig = null;
            }
        }

        public static ConfigMenu modmenu;
        public static SimpleButton enterConfig;
        public static bool mod = false;
        public static ConfigMenu config;
        //public static Dictionary<string, int> tuch6A;
        //public static bool resolutionDirty;
    }
}
