using Menu;
using Music;
using OptionalUI;
using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Menu Screen for Mod Config
    /// </summary>
    public class ConfigMenu : Menu.Menu, SelectOneButton.SelectOneButtonOwner, CheckBox.IOwnCheckBox
    {
        public OptionsMenu vanillaMenu;

        public ConfigMenu(ProcessManager manager) : base(manager, ProcessManager.ProcessID.OptionsMenu)
        {
            Debug.Log("ConfigMenu ctor!");
            this.manager.currentMainLoop = this; //duplicate
            this.manager.upcomingProcess = null;

            this.pages.Add(new Page(this, null, "hub", 0));
            OptionScript.configMenu = this;
            OptionScript.tabs = new Dictionary<string, OpTab>();

            if (this.manager.musicPlayer == null)
            {
                this.manager.musicPlayer = new MusicPlayer(this.manager);
                this.manager.sideProcesses.Add(this.manager.musicPlayer);
            }
            this.manager.musicPlayer.MenuRequestsSong(GetRandomSong(), 1f, 2f);
            //this.manager.musicPlayer.song = new MenuOrSlideShowSong(this.manager.musicPlayer, randomSong, 1f, 2f);
            //this.manager.musicPlayer.song.playWhenReady = true;

            redUnlocked = (this.manager.rainWorld.progression.miscProgressionData.redUnlocked ||
                File.Exists(string.Concat(Custom.RootFolderDirectory(), "unlockred.txt")) ||
                this.manager.rainWorld.progression.miscProgressionData.redMeatEatTutorial > 2
                );

            opened = false;
            selectedModIndex = 0;
            menuTab = new MenuTab();

            List<string> allLevels = new List<string>();
            this.multiplayerUnlocks = new MultiplayerUnlocks(manager.rainWorld.progression, allLevels);
            currentInterface = null;

            OptionScript.soundFill = 0;
            freezeMenu = false;
            BoundKey = new Dictionary<string, string>();
            //Get Vanilla Keys
            for (int i = 0; i < OptionScript.rw.options.controls.Length; i++)
            {
                Options.ControlSetup setup = OptionScript.rw.options.controls[i];
                if (setup.preset == Options.ControlSetup.Preset.KeyboardSinglePlayer)
                {
                    for (int p = 0; p < setup.keyboardKeys.Length; p++)
                    {
                        if (!BoundKey.ContainsValue(setup.keyboardKeys[p].ToString()))
                        { BoundKey.Add(string.Concat("Vanilla_", i.ToString(), "_", p.ToString()), setup.keyboardKeys[p].ToString()); }
                    }
                }
                else
                {
                    for (int p = 0; p < setup.gamePadButtons.Length; p++)
                    {
                        string key = setup.gamePadButtons[p].ToString();
                        if (key.Length > 9 && int.TryParse(key.Substring(8, 1), out int _))
                        { }
                        else
                        { key = key.Substring(0, 8) + i.ToString() + key.Substring(8); }
                        if (!BoundKey.ContainsValue(key))
                        { BoundKey.Add(string.Concat("Vanilla_", i.ToString(), "_", p.ToString()), key); }
                    }
                }
            }

            LabelTest.Initialize(this);
        }

        /// <summary>
        /// List of Binded Key.
        /// Key: ...it's key. Value: ...this is also Key... of OpKeyBinder.
        /// </summary>
        public static Dictionary<string, string> BoundKey;

        /// <summary>
        /// Whether to freeze menu or not
        /// </summary>
        public static bool freezeMenu;

        public static OptionScript script;
        public static bool gamepad;

        private static readonly int[] bgList =
        {
            33, 34, 35, 42
        };

        private static readonly int[] bgListRed =
        {
            32, 33, 34, 35, 36, 38, 39, 40, 42,
            6, 9, 10, 18, 7
        };

        public static bool redUnlocked;

        public void Initialize() //UI
        {
            if (vanillaMenu != null)
            {
                vanillaMenu.ShutDownProcess();
                vanillaMenu = null;
            }

            instance = this;

            if (!redUnlocked)
            { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgList[Mathf.FloorToInt(UnityEngine.Random.value * (bgList.Length))])); }
            else
            { this.scene = new InteractiveMenuScene(this, this.pages[0], (MenuScene.SceneID)(bgListRed[Mathf.FloorToInt(UnityEngine.Random.value * (bgListRed.Length))])); }
            Debug.Log(string.Concat("Chosen Background : " + this.scene.sceneID.ToString()));
            this.pages[0].subObjects.Add(this.scene);

            this.fadeSprite.RemoveFromContainer();
            this.pages[0].Container.AddChild(this.fadeSprite); //reset fadeSprite

            this.darkSprite = new FSprite("pixel", true)
            {
                color = new Color(0f, 0f, 0f),
                anchorX = 0f,
                anchorY = 0f,
                scaleX = 1368f,
                scaleY = 770f,
                x = -1f,
                y = -1f,
                alpha = 0.6f
            };
            this.pages[0].Container.AddChild(this.darkSprite);

            modListBound = new OpRect(new Vector2(193f, 240f) - UIelement._offset, new Vector2(280f, 495f), 0.3f);
            //modCanvasBound = new OpRect(new Vector2(553f, 105f), new Vector2(630f, 630f), 0.4f);
            modCanvasBound = new OpRect(new Vector2(543f, 105f) - UIelement._offset, new Vector2(630f, 630f), 0.4f);
            //Base: new Vector2(468f, 120f);
            menuTab.AddItems(modListBound, modCanvasBound);
            this.pages[0].subObjects.Add(modListBound.rect);
            this.pages[0].subObjects.Add(modCanvasBound.rect);

            this.backButton = new SimpleButton(this, this.pages[0], InternalTranslator.Translate("BACK"), "CANCEL", new Vector2(450f, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.backButton);
            this.saveButton = new SimpleButton(this, this.pages[0], InternalTranslator.Translate("APPLY"), "APPLY", new Vector2(600f, 50f), new Vector2(110f, 30f));
            this.pages[0].subObjects.Add(this.saveButton);
            base.MutualHorizontalButtonBind(saveButton, backButton);
            this.resetButton = new HoldButton(this, this.pages[0], InternalTranslator.Translate("RESET CONFIG"), "RESET CONFIG", new Vector2(300f, 110f), 30f);
            this.pages[0].subObjects.Add(this.resetButton);
            this.alertLabel = new MenuLabel(this, this.pages[0], "", new Vector2(383f, 735f), new Vector2(600f, 30f), false);
            this.pages[0].subObjects.Add(this.alertLabel);
            this.alertLabelFade = 0f; this.lastAlertLabelFade = 0f; this.alertLabelSin = 0f;

            //Dark Box for ModList & Canvas
            //modlist x200 y400 w240 h600
            //canvas  x568 y120 w600 h600

            this.resetButton.nextSelectable[2] = this.backButton;
            this.backButton.nextSelectable[0] = this.resetButton;
            this.backButton.nextSelectable[2] = this.saveButton;
            this.saveButton.nextSelectable[0] = this.backButton;

            menuTab.lblInfoButton = new OpLabel(new Vector2(233f, 215f) - UIelement._offset, new Vector2(200f, 20f), "Config Machine " + MenuTab.GetCMVersion());
            menuTab.lblInfoButton.bumpBehav = new BumpBehaviour(menuTab.lblInfoButton);
            menuTab.AddItems(menuTab.lblInfoButton);
            this.pages[0].subObjects.Add(menuTab.lblInfoButton.label);

            List<OptionInterface> itfs = new List<OptionInterface>();

            // Initialize
            foreach (string id in OptionScript.loadedInterfaceDict.Keys)
            {
                OptionInterface itf = OptionScript.loadedInterfaceDict[id];
                Debug.Log(string.Concat("OptionInterface Initializing: " + id));

#pragma warning disable CS0612
                try { itf.Initialize(); }
                catch (Exception ex)
                { itf = new UnconfiguableOI(itf.mod, new GeneralInitializeException(ex)); goto replaced; }

                if (itf.Tabs == null || itf.Tabs.Length < 1)
                { itf = new UnconfiguableOI(itf.mod, new NoTabException(id)); goto replaced; }
                else if (itf.Tabs.Length > 20)
                { itf = new UnconfiguableOI(itf.mod, new TooManyTabsException()); goto replaced; }

                try { itf.LoadConfig(); itf.ShowConfig(); }
                catch (Exception ex)
                {
                    itf = new UnconfiguableOI(itf.mod, new LoadDataException(string.Concat("OILoad/ShowConfigError: ", id, " had a problem in Load/ShowConfig()\nAre you editing LoadConfig()/ShowConfig()? That could cause serious error.",
                           Environment.NewLine, ex
                           )));
                    goto replaced;
                }
#pragma warning restore CS0612

                itfs.Add(itf);
                continue;

            replaced:
                OptionScript.loadedInterfaceDict.Remove(id);
                OptionScript.loadedInterfaceDict.Add(id, itf);
                itf.Initialize();
                itfs.Add(itf);
            }

            // Remove Excess
            int priority = -1; UnconfiguableOI listItf = null; List<RainWorldMod> listIgnored = new List<RainWorldMod>();
            while (itfs.Count > 16 && priority < 0) // 1
            {
                Debug.Log(string.Concat("Mod Count: ", itfs.Count, "! Discarding priority ", priority));
                /* if (priority == -2)
                {
                    List<OptionInterface> temp2 = new List<OptionInterface>();
                    foreach (OptionInterface oi in itfs) { if (oi.GetPriority() > -2) { temp2.Add(oi); } }
                    itfs = temp2; priority++; continue;
                } */
                if (priority == -1)
                {
                    PartialityMod blankMod = new PartialityMod
                    {
                        ModID = InternalTranslator.Translate("Mod List"),
                        Version = "XXXX",
                        author = RainWorldMod.authorNull
                    };
                    listItf = new UnconfiguableOI(blankMod, UnconfiguableOI.Reason.TooManyMod);
                }
                List<OptionInterface> temp = new List<OptionInterface>();
                foreach (OptionInterface oi in itfs)
                {
                    if (oi.GetPriority() > priority) { temp.Add(oi); }
                    else { listIgnored.Add(oi.rwMod); }
                }
                itfs = temp;
                priority++;
            }

            // Sorting
            {
                List<OptionInterface>[] sortTemp = new List<OptionInterface>[4];
                for (int p = 0; p < 4; p++) { sortTemp[p] = new List<OptionInterface>(); }
                foreach (OptionInterface oi in itfs) { sortTemp[oi.GetPriority() + 1].Add(oi); }
                int cmCount = 0;
                for (int p = 0; p < 4; p++) { cmCount += sortTemp[p].Count; sortTemp[p].Sort(CompareOIModID); }

                itfs = new List<OptionInterface>();
                if (priority > -1)
                {
                    OptionScript.loadedInterfaceDict.Remove(InternalTranslator.Translate("Mod List"));
                    OptionScript.loadedInterfaceDict.Add(InternalTranslator.Translate("Mod List"), listItf);
                    listItf.SetIgnoredModList(listIgnored);
                    if (cmCount > 15) { listItf.SetConfiguableModList(sortTemp); }
                    else { listItf.SetConfiguableModList(null); }
                    listItf.Initialize();
                    itfs.Add(listItf);
                }
                for (int p = 3; p >= 0; p--)
                {
                    for (int i = 0; i < sortTemp[p].Count; i++)
                    { itfs.Add(sortTemp[p][i]); }
                }
            }

            // Add Buttons
            modButtons = new SelectOneButton[Math.Min(16, itfs.Count)];
            Dictionary<int, string> dictionary = new Dictionary<int, string>(itfs.Count);
            selectedModIndex = 0;
            for (int i = 0; i < itfs.Count; i++)
            {
                OptionInterface itf = itfs[i];
#pragma warning disable CS0162
                if (OptionMod.testing) { Debug.Log(string.Concat("Mod(" + i + ") : " + itf.rwMod.ModID)); }
#pragma warning restore CS0162
                for (int t = 0; t < itf.Tabs.Length; t++)
                {
                    OptionScript.tabs.Add(string.Concat(i.ToString("D3") + "_" + t.ToString("D2")), itf.Tabs[t]);
                    foreach (UIelement element in itf.Tabs[t].items)
                    {
                        foreach (MenuObject obj in element.subObjects) { this.pages[0].subObjects.Add(obj); }
                        this.pages[0].Container.AddChild(element.myContainer);
                    }
                    itf.Tabs[t].Hide();
                }
                dictionary.Add(i, itf.rwMod.ModID);

                if (i > 15) { continue; }
                modButtons[i] = new SelectOneButton(this, this.pages[0], itf.rwMod.ModID, "ModSelect", new Vector2(208f, 700f - 30f * i), new Vector2(250f, 24f), modButtons, i);
                this.pages[0].subObjects.Add(modButtons[i]);
            }
            modList = dictionary;
            scrollMode = false;
            if (itfs.Count > 16)
            {
                scrollMode = true;
                scrollTop = 1;
                modButtons[1].menuLabel.text = InternalTranslator.Translate("Scroll Up");
                modButtons[1].signalText = "ScrollUp";
                modButtons[15].menuLabel.text = InternalTranslator.Translate("Scroll Down");
                modButtons[15].signalText = "ScrollDown";
                ScrollButtons();
            }

            this.resetButton.nextSelectable[1] = this.modButtons[this.modButtons.Length - 1];
            this.backButton.nextSelectable[1] = this.modButtons[this.modButtons.Length - 1];
            this.saveButton.nextSelectable[1] = this.modButtons[this.modButtons.Length - 1];
            this.modButtons[this.modButtons.Length - 1].nextSelectable[3] = this.saveButton;
            if (this.modButtons.Length > 1)
            {
                for (int m = 0; m < this.modButtons.Length - 1; m++)
                {
                    this.modButtons[m].nextSelectable[3] = this.modButtons[m + 1];
                    this.modButtons[m + 1].nextSelectable[1] = this.modButtons[m];
                }
            }

            //Load Tab
            selectedTabIndex = 0;
            currentInterface = OptionScript.loadedInterfaceDict[modList[0]];
            currentTab = OptionScript.tabs[string.Concat(selectedModIndex.ToString("D3") + "_" + selectedTabIndex.ToString("D2"))];

            currentTab.Show();
            if (currentInterface.Configuable())
            {
                saveButton.buttonBehav.greyedOut = false;
                resetButton.buttonBehav.greyedOut = false;
            }
            else
            {
                saveButton.buttonBehav.greyedOut = true;
                resetButton.buttonBehav.greyedOut = true;
            }

            tabCtrler = new ConfigTabController(new Vector2(503f, 120f) - UIelement._offset, new Vector2(40f, 600f), menuTab, this);
            menuTab.AddItems(tabCtrler);
            foreach (MenuObject obj in tabCtrler.subObjects)
            { this.pages[0].subObjects.Add(obj); }

            this.selectedObject = this.backButton;

            OptionScript.configChanged = false;
        }

        public static bool scrollMode { get; private set; }
        public static int scrollTop;

        public static void ScrollButtons()
        {
            for (int i = 2; i < 15; i++)
            { instance.modButtons[i].menuLabel.text = instance.modList[scrollTop + i - 2]; }
        }

        public static ConfigMenu instance;

        /// <summary>
        /// Comparator for Sorting OptionInterfaces by ModID
        /// </summary>
        private static int CompareOIModID(OptionInterface x, OptionInterface y)
        { return ListItem.GetRealName(x.rwMod.ModID).CompareTo(ListItem.GetRealName(y.rwMod.ModID)); }

        public Dictionary<int, string> modList;
        public static int selectedModIndex;
        public static OptionInterface currentInterface;
        public static OpTab currentTab;
        public static int selectedTabIndex;
        public static FContainer tabContainer;
        public static MenuTab menuTab;
        public static ConfigTabController tabCtrler;
        public static string description, alert;

        public SelectOneButton[] modButtons;

        public SimpleButton backButton;
        public SimpleButton saveButton;
        public HoldButton resetButton;
        private MenuLabel alertLabel;
        private float alertLabelFade, lastAlertLabelFade, alertLabelSin;

        internal OpRect modListBound, modCanvasBound;

        private static string GetRandomSong() => OptionMod.randomSong;

        public FSprite fadeSprite, darkSprite;

        public static void ChangeSelectedTab()
        {
            currentTab.Hide();
            currentTab = OptionScript.tabs[string.Concat(selectedModIndex.ToString("D3") + "_" + selectedTabIndex.ToString("D2"))];
            currentTab.Show();
        }

        public void ChangeSelectedMod()
        {
            //Unload Current Ones
            if (MenuTab.logMode) { menuTab.HideChangeLog(); }
            currentInterface = OptionScript.loadedInterfaceDict[modList[selectedModIndex]];
            selectedTabIndex = 0;
            tabCtrler.OnChange();
            ChangeSelectedTab();
            if (currentInterface.Configuable())
            {
                saveButton.buttonBehav.greyedOut = false;
                resetButton.buttonBehav.greyedOut = false;
            }
            else
            {
                saveButton.buttonBehav.greyedOut = true;
                resetButton.buttonBehav.greyedOut = true;
            }
        }

        public static void SaveAllConfig()
        {
            foreach (KeyValuePair<string, OptionInterface> item in OptionScript.loadedInterfaceDict)
            {
                if (!OptionScript.loadedInterfaceDict[item.Key].Configuable()) { continue; }
                Dictionary<string, string> newConfig = OptionScript.loadedInterfaceDict[item.Key].GrabConfig();
                OptionScript.loadedInterfaceDict[item.Key].SaveConfig(newConfig);
            }
        }

        public static void SaveCurrentConfig()
        {
            if (!currentInterface.Configuable()) { return; }
            Dictionary<string, string> newConfig = currentInterface.GrabConfig();
            currentInterface.SaveConfig(newConfig);
        }

        private bool reset = false;

        /// <summary>
        /// Call this manually if you really need to. This is what Reset Config Button does.
        /// </summary>
        public static void ResetCurrentConfig()
        {
            instance.reset = true;
            instance.opened = false;
            instance.OpenMenu();
        }

        private void ResetCurrentConfigForReal()
        {
            reset = false;
            // if (!currentInterface.Configuable()) { return; }

            foreach (OpTab tab in currentInterface.Tabs)
            {
                foreach (UIelement element in tab.items)
                {
                    foreach (MenuObject obj in element.subObjects)
                    {
                        obj.RemoveSprites();
                        this.pages[0].subObjects.Remove(obj);
                    }
                    this.pages[0].Container.RemoveChild(element.myContainer);
                    element.myContainer.RemoveFromContainer();
                }
                tab.Unload();
            }
            int i = 0;
            do
            {
                string key = string.Concat(selectedModIndex.ToString("D3") + "_" + i.ToString("D2"));
                if (OptionScript.tabs.ContainsKey(key)) { OptionScript.tabs.Remove(key); }
                else { break; }
                i++;
            } while (i < 100);

            currentInterface.Initialize();
            for (i = 0; i < currentInterface.Tabs.Length; i++)
            {
                string key = string.Concat(selectedModIndex.ToString("D3") + "_" + i.ToString("D2"));
                OptionScript.tabs.Add(key, currentInterface.Tabs[i]);
                foreach (UIelement element in currentInterface.Tabs[i].items)
                {
                    foreach (RectangularMenuObject obj in element.subObjects)
                    { this.pages[0].subObjects.Add(obj); }
                    this.pages[0].Container.AddChild(element.myContainer);
                }
                currentInterface.Tabs[i].Hide();
            }

            selectedTabIndex = 0;
            tabCtrler.Reset();
            currentTab = currentInterface.Tabs[0];
            currentTab.Show();

            if (currentInterface.Configuable())
            {
                currentInterface.SaveConfig(currentInterface.GrabConfig());
                currentInterface.ConfigOnChange();
            }

            //OptionScript.configChanged = false;
        }

        private string lastDescription;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (this.alertLabel != null)
            {
                this.alertLabel.label.alpha = Mathf.Clamp01(Mathf.Lerp(this.lastAlertLabelFade, this.alertLabelFade, timeStacker));
                this.alertLabel.label.color = Color.Lerp(MenuRGB(MenuColors.White), MenuRGB(MenuColors.MediumGrey),
                    0.5f + 0.5f * Mathf.Sin((this.alertLabelSin + timeStacker) / 4f));
            }
        }

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
            if (!string.IsNullOrEmpty(alert))
            {
                this.alertLabelFade = 2f; this.lastAlertLabelFade = 2f;
                this.alertLabelSin = 0f;
                this.alertLabel.text = alert;
                alert = null;
            }
            this.lastAlertLabelFade = this.alertLabelFade;
            this.alertLabelFade = Mathf.Max(0f, this.alertLabelFade - 1f / Mathf.Lerp(1f, 100f, Mathf.Clamp01(this.alertLabelFade)));
            this.alertLabelSin += this.alertLabelFade;

            base.Update(); //keep buttons to be sane

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

            if (this.manager.musicPlayer != null)
            {
                if (this.manager.musicPlayer.song == null && this.manager.musicPlayer.nextSong == null)
                { this.manager.musicPlayer.MenuRequestsSong(GetRandomSong(), 1f, 2f); }
            }
        }

        public Dictionary<string, int> buttonList;

        public override void Singal(MenuObject sender, string message)
        {
            if (message != null)
            {
                if (buttonList == null)
                {
                    Dictionary<string, int> dictionary = new Dictionary<string, int>(4)
                    {
                        { "CANCEL", 0 },
                        { "APPLY", 1 },
                        { "RESET CONFIG", 2 },
                        { "SOUNDTEST", 3 }
                    };
                    buttonList = dictionary;
                }

                if (buttonList.TryGetValue(message, out int num))
                {
                    switch (num)
                    {
                        case 0:
                            OptionsMenuPatch.mod = false;
                            opened = false;
                            base.PlaySound(SoundID.MENU_Switch_Page_Out);
                            this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
                            break;

                        case 1:
                            if (OptionScript.configChanged)
                            {
                                base.PlaySound(SoundID.MENU_Switch_Page_In);
                                SaveCurrentConfig();
                                OptionScript.configChanged = false;
                                this.saveButton.menuLabel.text = InternalTranslator.Translate("SAVE ALL");
                            }
                            else
                            {
                                if (this.saveButton.menuLabel.text == InternalTranslator.Translate("SAVE ALL"))
                                {
                                    base.PlaySound(SoundID.MENU_Next_Slugcat);
                                    SaveAllConfig();
                                    //EXIT
                                    OptionsMenuPatch.mod = false;
                                    opened = false;
                                    this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.OptionsMenu);
                                }
                                else
                                {
                                    base.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                                }
                            }
                            break;

                        case 2:
                            ResetCurrentConfig();
                            base.PlaySound(SoundID.MENU_Switch_Page_In);
                            break;

                        case 3:
                            if (!redUnlocked) { base.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed); }
                            else
                            {
                                base.PlaySound(SoundID.MENU_Switch_Page_In);

                                Debug.Log("Enter SoundTest");

                                this.InitializeSitting();
                                this.manager.rainWorld.progression.ClearOutSaveStateFromMemory();
                                if (this.manager.arenaSitting.ReadyToStart)
                                {
                                    this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);//, 1.2f);
                                }
                                else
                                {
                                    this.manager.arenaSitting = null;
                                }
                            }
                            break;
                    }
                }
            }
        }

        public ArenaSetup GetArenaSetup
        {
            get
            {
                return this.manager.arenaSetup;
            }
        }

        public ArenaSetup.GameTypeSetup GetGameTypeSetup
        {
            get
            {
                return this.GetArenaSetup.GetOrInitiateGameTypeSetup(ArenaSetup.GameTypeID.Sandbox);
            }
        }

        private readonly MultiplayerUnlocks multiplayerUnlocks;

        private void InitializeSitting()
        {
            if (this.manager.arenaSetup == null)
            {
                ArenaSetup setup = new ArenaSetup
                {
                    currentGameType = ArenaSetup.GameTypeID.Sandbox
                };

                setup.gametypeSetups.Add(new ArenaSetup.GameTypeSetup());
                setup.gametypeSetups[setup.gametypeSetups.Count - 1].InitAsGameType(ArenaSetup.GameTypeID.Sandbox);

                this.manager.arenaSetup = setup;
            }

            this.manager.arenaSitting = new ArenaSitting(this.GetGameTypeSetup, this.multiplayerUnlocks)
            {
                levelPlaylist = new List<string>()
            };
            this.manager.arenaSitting.levelPlaylist.Add("SoundTest");
        }

        public void KillTabElements()
        {
            foreach (KeyValuePair<string, OpTab> item in OptionScript.tabs)
            {
                OpTab tab = OptionScript.tabs[item.Key];

                foreach (UIelement element in tab.items)
                {
                    foreach (MenuObject obj in element.subObjects)
                    {
                        obj.RemoveSprites();
                        //this.pages[0].subObjects.Remove(obj);
                    }
                    element.myContainer.RemoveAllChildren();
                    this.pages[0].Container.RemoveChild(element.myContainer);
                    element.myContainer.RemoveFromContainer();
                    element.Unload();
                }
                tab.Unload();
            }

            //this.pages[0].subObjects.Remove(modListBound.menuObj);
            //this.pages[0].subObjects.Remove(modCanvasBound.menuObj);
            menuTab.Unload();
        }

        public override void ShutDownProcess()
        {
            //foreach (UIelement element in menuTab.items)
            //{
            //foreach (MenuObject obj in element.subObjects)
            //{
            //this.pages[0].subObjects.Remove(obj);
            //}
            //this.pages[0].Container.RemoveChild(element.myContainer);
            //element.myContainer.RemoveFromContainer();
            //}
            KillTabElements();
            OptionScript.KillTabs();
            base.ShutDownProcess();
            this.darkSprite.RemoveFromContainer();
            currentTab = null;
            FTexture.GarbageCollect(false); // Already going to be called by vanilla
        }

        public override string UpdateInfoText()
        {
            if (this.selectedObject is SelectOneButton)
            {
                if ((this.selectedObject as SelectOneButton).signalText == "ModSelect")
                {
                    string id = (this.selectedObject as SelectOneButton).menuLabel.text;
                    string output;
                    bool fc = false;
                    if (OptionScript.loadedInterfaceDict.TryGetValue(id, out OptionInterface oi))
                    {
                        fc = oi.Configuable();
                        if (fc)
                        {
                            if (!string.IsNullOrEmpty(oi.rwMod.author) && oi.rwMod.author != RainWorldMod.authorNull)
                            { output = InternalTranslator.Translate("Configure <ModID> by <ModAuthor>").Replace("<ModAuthor>", oi.rwMod.author); }
                            else
                            { output = InternalTranslator.Translate("Configure <ModID>"); }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(oi.rwMod.author) && oi.rwMod.author != RainWorldMod.authorNull)
                            { output = InternalTranslator.Translate("Display <ModID> by <ModAuthor>").Replace("<ModAuthor>", oi.rwMod.author); }
                            else
                            { output = InternalTranslator.Translate("Display <ModID>"); }
                        }
                    }
                    else
                    {
                        if (fc) { output = InternalTranslator.Translate("Configure <ModID>"); }
                        else { output = InternalTranslator.Translate("Display <ModID>"); }
                    }
                    output = output.Replace("<ModID>", id);

                    return output;
                }
                else if ((this.selectedObject as SelectOneButton).signalText == "ScrollUp")
                { return InternalTranslator.Translate("Scroll Up"); }
                else if ((this.selectedObject as SelectOneButton).signalText == "ScrollDown")
                { return InternalTranslator.Translate("Scroll Down"); }
            }
            if (this.selectedObject is HoldButton)
            {
                if (modList.TryGetValue(selectedModIndex, out string id))
                { return InternalTranslator.Translate("Hold down to restore original config of <ModID>").Replace("<ModID>", id); }
                return InternalTranslator.Translate("Hold down to restore original config for this mod");
            }
            if (this.selectedObject == this.backButton)
            {
                if (OptionScript.configChanged)
                { return InternalTranslator.Translate("Return to vanilla option menu (WITHOUT saving!)"); }
                else
                { return InternalTranslator.Translate("Return to vanilla option menu"); }
            }
            if (this.selectedObject == this.saveButton)
            {
                if (this.saveButton.menuLabel.text == InternalTranslator.Translate("SAVE ALL"))
                { return InternalTranslator.Translate("Save all changes and exit"); }
                else
                {
                    if (modList.TryGetValue(selectedModIndex, out string id))
                    { return InternalTranslator.Translate("Save changed config of <ModID>").Replace("<ModID>", id); }
                    return InternalTranslator.Translate("Save changed config of this mod");
                }
            }

            return base.UpdateInfoText();
        }

        bool CheckBox.IOwnCheckBox.GetChecked(CheckBox box)
        {
            throw new NotImplementedException();
        }

        void CheckBox.IOwnCheckBox.SetChecked(CheckBox box, bool c)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> radioDictionary;

        private void InitRadioDict()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(2)
                    {
                        { "ModSelect", 0 },
                        { "SaveSlot", 1 },
                        { "ScrollUp", 2 },
                        { "ScrollDown", 3 }
                    };
            radioDictionary = dictionary;
        }

        public int GetCurrentlySelectedOfSeries(string series)
        {
            if (series != null)
            {
                if (radioDictionary == null) { InitRadioDict(); }
                if (radioDictionary.TryGetValue(series, out int num))
                {
                    switch (num)
                    {
                        case 0:
                            if (MenuTab.logMode) { return -1; }
                            if (scrollMode && selectedModIndex != 0) { return selectedModIndex - scrollTop + 2; }
                            return selectedModIndex;

                        default:
                            return 0;
                    }
                }
            }
            return -1;
        }

        public void SetCurrentlySelectedOfSeries(string series, int to)
        {
            if (series != null)
            {
                if (radioDictionary == null) { InitRadioDict(); }
                if (radioDictionary.TryGetValue(series, out int num))
                {
                    switch (num)
                    {
                        case 0:
                            //Change Selected Mod
                            if (scrollMode && to != 0) { to += scrollTop - 2; }
                            if (selectedModIndex != to || MenuTab.logMode)
                            {
                                selectedModIndex = to;
                                ChangeSelectedMod();
                            }
                            break;

                        case 2:
                            if (scrollTop < 2) { break; }
                            scrollTop--;
                            if (selectedModIndex != 0 && selectedModIndex > scrollTop + 12) { selectedModIndex--; ChangeSelectedMod(); }
                            ScrollButtons();
                            break;

                        case 3:
                            if (scrollTop >= modList.Count - 13) { break; }
                            scrollTop++;
                            if (selectedModIndex != 0 && selectedModIndex < scrollTop) { selectedModIndex++; ChangeSelectedMod(); }
                            ScrollButtons();
                            break;
                    }
                }
            }
        }

        public bool opened = false;
        private bool fadein = false;
        private int fadeoutFrame;

        public void OpenMenu()
        {
            if (!opened)
            { //init
                opened = true;
                fadeoutFrame = 9;
                fadein = false;
                if (this.fadeSprite != null)
                {
                    this.fadeSprite.RemoveFromContainer();
                }
                this.fadeSprite = new FSprite("Futile_White", true)
                {
                    color = new Color(0f, 0f, 0f),
                    x = this.manager.rainWorld.screenSize.x / 2f,
                    y = this.manager.rainWorld.screenSize.y / 2f,
                    alpha = 0f,
                    shader = this.manager.rainWorld.Shaders["EdgeFade"]
                };

                this.pages[0].Container.AddChild(this.fadeSprite);
                return;
            }
            float multiplier = Math.Min(1f, this.fadeoutFrame * 0.05f);
            this.fadeSprite.scaleX = (this.manager.rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, multiplier) + 2f) / 16f;
            this.fadeSprite.scaleY = (this.manager.rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, multiplier) + 2f) / 16f;
            this.fadeSprite.alpha = Mathf.InverseLerp(0f, 0.9f, multiplier);
        }
    }
}
