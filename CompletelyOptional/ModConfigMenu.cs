using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using Menu;
using Music;
using OptionalUI;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class ModConfigMenu : Menu.Menu
    {
        public ModConfigMenu(ProcessManager manager) : base(manager, EnumExt_ComOpt.ModConfigMenu)
        {
            ComOptPlugin.LogInfo("ModConfigMenu ctor called");

            // Initialize
            instance = this;
            InternalTranslator.LoadTranslation();
            LabelTest.Initialize(this);
            redUnlocked = (this.manager.rainWorld.progression.miscProgressionData.redUnlocked ||
                File.Exists(string.Concat(Custom.RootFolderDirectory(), "unlockred.txt")) ||
                this.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial > 2
                );

            this.pages.Add(new Page(this, null, "hub", 0));

            // Play song
            if (this.manager.musicPlayer == null && this.manager.rainWorld.options.musicVolume > 0f)
            {
                this.manager.musicPlayer = new MusicPlayer(this.manager);
                this.manager.sideProcesses.Add(this.manager.musicPlayer);
            }
            this.manager.musicPlayer.MenuRequestsSong(randomSong, 1f, 2f);
            this.mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;

            // Show Background
            if (!ComOptPlugin.testing)
            {
                if (!redUnlocked)
                { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgList[Mathf.FloorToInt(UnityEngine.Random.value * (bgList.Length))])); }
                else
                { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgListRed[Mathf.FloorToInt(UnityEngine.Random.value * (bgListRed.Length))])); }
                ComOptPlugin.LogInfo($"Chosen Background : {this.scene.sceneID}");
                this.pages[0].subObjects.Add(this.scene);
            }
            else
            {
                this.scene = new SlideShowMenuScene(this, this.pages[0], MenuScene.SceneID.Intro_4_Walking);
                this.pages[0].subObjects.Add(this.scene);
            }

            // darkSprite
            this.darkSprite = new FSprite("pixel", true)
            {
                color = new Color(0.01f, 0.01f, 0.01f),
                anchorX = 0f,
                anchorY = 0f,
                scaleX = 1368f,
                scaleY = 770f,
                x = -1f,
                y = -1f,
                alpha = 0.85f
            };
            this.pages[0].Container.AddChild(this.darkSprite);

            // Load OptionInterfaces
            if (!_loadedOIs) { LoadOIs(); }

            // UIContainer
            mContainer = new MenuContainer(this, this.pages[0]);
            this.pages[0].subObjects.Add(mContainer);
        }

        /// <summary>
        /// Blacklisted mod from config menu.
        /// </summary>
        internal static string[] blackList = new string[]
        {
            "CompletelyOptional",
            "ConfigMachine",
            //"RustyMachine",
            "PolishedMachine",
            //"Enum Extender",
            "ComMod",
            "CommunicationModule",
            "BepInEx-Partiality-Wrapper",
            "BepInEx.Partiality.Wrapper",
            "PartialityWrapper",
            "Partiality Wrapper",
            "LogFix",
            "Log Fix"
        };

        private void LoadOIs()
        {
            loadedInterfaces = new List<OptionInterface>();
            loadedInterfaceDict = new Dictionary<string, OptionInterface>();

            // Load Plugins
            BaseUnityPlugin[] plugins = UnityEngine.Object.FindObjectsOfType<BaseUnityPlugin>();
            foreach (BaseUnityPlugin plugin in plugins)
            {
                OptionInterface oi;

                // Load OI
                try
                {
                    var method = plugin.GetType().GetMethod("LoadOI");
                    if (method == null || method.GetParameters().Length > 0 || method.ContainsGenericParameters)
                    {
                        // Mod didn't attempt to interface with CompletelyOptional, don't bother logging it.
                        oi = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);
                    }
                    else if (method.Invoke(plugin, null) is OptionInterface itf)
                    {
                        oi = itf;
                        //Your code
                        ComOptPlugin.LogInfo($"Loaded OptionInterface from {oi.rwMod.ModID} (type: {oi.GetType()})");
                    }
                    else
                    {
                        oi = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);
                        ComOptPlugin.LogInfo($"{oi.rwMod.ModID} did not return an OptionInterface in LoadOI.");
                    }
                }
                catch (Exception ex)
                {
                    oi = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);

                    if (blackList.Contains(oi.rwMod.ModID) || oi.rwMod.ModID.Substring(0, 1) == "_")
                    { continue; }

                    ComOptPlugin.LogWarning($"{oi.rwMod.ModID} threw an exception in LoadOI: {ex.Message}");
                }

                if (oi is UnconfiguableOI && plugin.Config.Keys.Count > 0)
                {
                    // Use BepInEx Configuration
                    oi = new GeneratedOI(oi.rwMod, plugin.Config);
                }

                loadedInterfaces.Add(oi);
                loadedInterfaceDict.Add(oi.rwMod.ModID, oi);
            }
            _loadedOIs = true;
        }

        private static bool _loadedOIs = false;

        /// <summary>
        /// List of OptionInterface Instances
        /// </summary>
        internal static List<OptionInterface> loadedInterfaces;

        /// <summary>
        /// Loaded OptionInterface Instances.
        /// Key: ModID, Value: OI Instance
        /// </summary>
        internal static Dictionary<string, OptionInterface> loadedInterfaceDict;

        public static ModConfigMenu instance;

        /// <summary>
        /// Screensized Dark Screen for fading
        /// </summary>
        private FSprite darkSprite;

        /// <summary>
        /// Container that connects and stores UIelements for Mod Configs
        /// </summary>
        internal MenuContainer mContainer;

        public override string UpdateInfoText()
        {
            return base.UpdateInfoText();
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
        }

        #region Aesthetics

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
        /// List of carefully chosen song gets played in ModConfigMenu
        /// </summary>
        private static string randomSong
        {
            get
            {
                if (UnityEngine.Random.value < 0.8f)
                { return playlistMoody[UnityEngine.Random.Range(0, 4)]; }
                else
                { return playlistWork[UnityEngine.Random.Range(0, 2)]; }
            }
        }

        private static readonly int[] bgList = { 33, 34, 35, 42 };

        private static readonly int[] bgListRed = { 32, 33, 34, 35, 36, 38, 39, 40, 42, 6, 9, 10, 18, 7 };

        /// <summary>
        /// Whether Hunter is unlocked at this point. Used to determine whether to use spoiler background or not
        /// </summary>
        public static bool redUnlocked { get; private set; }

        #endregion Aesthetics
    }
}