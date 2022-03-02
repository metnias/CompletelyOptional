using BepInEx;
using Menu;
using Music;
using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            description = ""; lastDescription = "";
            Dictionary<int, string> ID2Code = new Dictionary<int, string>()
            {
                { 0 , "eng" },
                { 1 , "fre" },
                { 2 , "ita" },
                { 3 , "ger" },
                { 4 , "spa" },
                { 5 , "por" },
                { 6 , "jap" },
                { 7 , "kor" }
            };
            if (ID2Code.TryGetValue(this.manager.rainWorld.options.language, out string code)) { curLang = code; }
            else { curLang = "eng"; }
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
            cfgContainer = new ConfigContainer(this, this.pages[0]);
            this.pages[0].subObjects.Add(cfgContainer);
        }

        /// <summary>
        /// Current Language of the game, including ComMod ones
        /// </summary>
        internal static string curLang;

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

        private string description, lastDescription;

        /// <summary>
        /// Send Menu description text to display
        /// </summary>
        public void ShowDescription(string description) => this.description = description;

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
        internal ConfigContainer cfgContainer;

        public override void Update()
        {
            if (!string.IsNullOrEmpty(description) && string.IsNullOrEmpty(this.UpdateInfoText()))
            {
                this.infoLabelFade = 1f;
                this.infoLabel.text = description;
                if (lastDescription != description)
                {
                    this.infolabelDirty = false;
                    this.infoLabelSin = 0f;
                }
            }
            lastDescription = description;
            /* //SoonTM
            if (!string.IsNullOrEmpty(alert))
            {
                this.alertLabelFade = 2f; this.lastAlertLabelFade = 2f;
                this.alertLabelSin = 0f;
                this.alertLabel.text = alert;
                alert = null;
            }
            this.lastAlertLabelFade = this.alertLabelFade;
            this.alertLabelFade = Mathf.Max(0f, this.alertLabelFade - 1f / Mathf.Lerp(1f, 100f, Mathf.Clamp01(this.alertLabelFade)));
            this.alertLabelSin += this.alertLabelFade; */

            base.Update(); //keep buttons to be sane

            /*
            if (fadein && this.scene != null && (int)this.scene.sceneID < 12 && (int)this.scene.sceneID > 6)
            { //clamp offset
                this.scene.camPos = new Vector2(this.scene.camPos.x * 0.7f, this.scene.camPos.y * 0.7f);
            }

            if (this.fadeSprite != null)
            {
                if (!fadein)
                {
                    this.fadeoutFrame++;
                    if (this.fadeoutFrame > 40)
                    {
                        //if not loaded yet ==> this.fadeoutFrame = 30; return;
                        this.fadeoutFrame = 20;
                        this.fadein = true;
                        if (currentInterface == null) { this.Initialize(); }
                        if (reset) { ResetCurrentConfigForReal(); }
                    }
                }
                else
                {
                    this.fadeoutFrame--;
                    if (this.fadeoutFrame < 1)
                    {
                        this.fadeoutFrame = 0;
                        this.fadeSprite.RemoveFromContainer();
                        this.fadeSprite = null;
                        return;
                    }
                }
                this.OpenMenu();
                return;
            }
            else
            {
                if (OptionScript.configChanged && this.backButton != null)
                { this.backButton.menuLabel.text = InternalTranslator.Translate("CANCEL"); }
                else
                { this.backButton.menuLabel.text = InternalTranslator.Translate("BACK"); }
            }
            */

            // Play new song
            if (this.manager.musicPlayer != null)
            {
                if (this.manager.musicPlayer.song == null && this.manager.musicPlayer.nextSong == null)
                { this.manager.musicPlayer.MenuRequestsSong(randomSong, 1f, 2f); }
            }
        }

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