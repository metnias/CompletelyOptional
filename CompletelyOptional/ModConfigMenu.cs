using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            // UIContainer
            mContainer = new MenuContainer(this, this.pages[0]);
            this.pages[0].subObjects.Add(mContainer);
        }

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

        /// <summary>
        /// Whether to play sound or not
        /// </summary>
        public static bool mute { get; internal set; }
    }
}