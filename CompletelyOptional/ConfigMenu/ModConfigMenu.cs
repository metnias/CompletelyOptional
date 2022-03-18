using Menu;
using Music;
using OptionalUI;
using RWCustom;
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

            // Update rw pm
            ComOptPlugin.pm = this.manager;
            ComOptPlugin.rw = this.manager.rainWorld;

            // Initialize
            instance = this;
            description = ""; lastDescription = "";
            if (!isReload)
            {
                redUnlocked = (this.manager.rainWorld.progression.miscProgressionData.redUnlocked ||
                    File.Exists(string.Concat(Custom.RootFolderDirectory(), "unlockred.txt")) ||
                    this.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial > 2
                    );
            }

            this.pages.Add(new Page(this, null, "hub", 0));
            LabelTest.Initialize(this);

            // Play song
            if (!isReload)
            {
                if (this.manager.musicPlayer == null && this.manager.rainWorld.options.musicVolume > 0f)
                {
                    this.manager.musicPlayer = new MusicPlayer(this.manager);
                    this.manager.sideProcesses.Add(this.manager.musicPlayer);
                }
                this.manager.musicPlayer.MenuRequestsSong(randomSong, 1f, 2f);
                this.mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
            }

            // Show Background
            if (!ComOptPlugin.testing)
            {
                if (isReload)
                { this.scene = new InteractiveMenuScene(this, this.pages[0], reloadScene); }
                else
                {
                    if (!redUnlocked)
                    { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgList[Mathf.FloorToInt(UnityEngine.Random.value * (bgList.Length))])); }
                    else
                    { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgListRed[Mathf.FloorToInt(UnityEngine.Random.value * (bgListRed.Length))])); }
                    ComOptPlugin.LogInfo($"Chosen Background : {this.scene.sceneID}");
                }
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

            // AlertLabel
            alertLabel = new MenuLabel(this, this.pages[0], "", new Vector2(383f, 735f), new Vector2(600f, 30f), false);
            this.pages[0].subObjects.Add(this.alertLabel);

            // UIContainer
            cfgContainer = new ConfigContainer(this, this.pages[0]);
            this.pages[0].subObjects.Add(cfgContainer);
        }

        private const int _DASinit = 20, _DASdelay = 6;

        /// <summary>
        /// Delayed Auto Shift initial delay frame (20)
        /// </summary>
        public static int DASinit => UIelement.FrameMultiply(_DASinit);

        /// <summary>
        /// Delayed Auto Shift delay frame after initial one (6)
        /// </summary>
        public static int DASdelay => UIelement.FrameMultiply(_DASdelay);

        /// <summary>
        /// Current Language of the game, including ComMod ones
        /// </summary>
        internal static string curLang;

        private string description, lastDescription;

        /// <summary>
        /// Send Menu description text to display
        /// </summary>
        internal void ShowDescription(string text) => this.description = text;

        #region Alert

        /// <summary>
        /// Send Menu alert text to display
        /// </summary>
        internal void ShowAlert(string text) => this.alertText = text;

        private readonly MenuLabel alertLabel;
        private string alertText = "";
        private float alertLabelFade = 0f, lastAlertLabelFade = 0f, alertLabelSin = 0f;

        #endregion Alert

        public static ModConfigMenu instance;

        /// <summary>
        /// Screensized Dark Screen for fading
        /// </summary>
        private readonly FSprite darkSprite;

        /// <summary>
        /// Container that connects and stores UIelements for Mod Configs
        /// </summary>
        internal ConfigContainer cfgContainer;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            cfgContainer.GrafUpdate(timeStacker);

            alertLabel.label.alpha = Custom.SCurve(Mathf.Clamp01(Mathf.Lerp(lastAlertLabelFade, alertLabelFade, timeStacker)), 0.3f);
            if (lastAlertLabelFade > 0f)
            {
                alertLabel.label.color = Color.Lerp(MenuRGB(MenuColors.White), MenuRGB(MenuColors.MediumGrey),
                        0.5f + 0.5f * Mathf.Sin((this.alertLabelSin + timeStacker) / 4f));
            }
        }

        public override void Update()
        {
            lastDescription = description;
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
            this.lastInfoLabelFade = this.infoLabelFade;

            if (!string.IsNullOrEmpty(alertText))
            {
                this.alertLabelFade = 2f; this.lastAlertLabelFade = 2f;
                this.alertLabelSin = 0f;
                this.alertLabel.text = alertText;
                alertText = null;
            }
            this.lastAlertLabelFade = this.alertLabelFade;
            this.alertLabelFade = Mathf.Max(0f, this.alertLabelFade - 1f / Mathf.Lerp(1f, 100f, Mathf.Clamp01(this.alertLabelFade)));
            this.alertLabelSin += this.alertLabelFade;

            base.Update(); //keep buttons to be sane

            cfgContainer.Update();

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

        internal bool isReload = false;
        private MenuScene.SceneID reloadScene;

        public override void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
        {
            base.CommunicateWithUpcomingProcess(nextProcess);
            if (nextProcess is ModConfigMenu cm)
            {
                cm.isReload = true;
                reloadScene = this.scene.sceneID;
            }
        }

        public override string UpdateInfoText()
        {
            return base.UpdateInfoText();
        }

        public override void ShutDownProcess()
        {
            cfgContainer.ShutdownConfigContainer();
            base.ShutDownProcess();
            instance = null;
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            switch (message)
            {
                case MenuTab.backSignal:
                    PlaySound(SoundID.MENU_Switch_Page_Out);
                    this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
                    break;

                case MenuTab.saveSignal:
                    if (ConfigContainer.OptItfChanged[ConfigContainer.activeItfIndex]) { ConfigContainer.activeInterface.SaveConfig(); }
                    else if (ConfigContainer.OptItfChanged.Any(x => x))
                    {
                    }
                    break;

                case MenuTab.resetSignal:
                    break;
            }
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