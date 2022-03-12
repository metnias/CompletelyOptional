﻿using BepInEx;
using OptionalUI;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Globalization;

namespace CompletelyOptional
{
    /// <summary>
    /// Contains all OptionInterfaces, and connects UIelements and Rain World Menu
    /// </summary>
    public class ConfigContainer : MenuObject
    {
        public ConfigContainer(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            // Initialize
            instance = this;
            myContainer = new FContainer();
            owner.Container.AddChild(myContainer);
            _soundFill = 0;
            holdElement = false;
            history = new Stack<ConfigHistory>();

            // Load OptionInterfaces
            LoadItfs();

            menuTab = new MenuTab();
            lastFocusedElement = menuTab.backButton;
            focusedElement = menuTab.backButton;

            activeItfIndex = 0;
            activeTabIndex = 0;
            activeTab = activeInterface.Tabs[activeTabIndex];

            menuTab.Activate();
            activeTab.Activate();

            // Update Once
            GrafUpdate(0f);
            Update();
        }

        public static ConfigContainer instance;

        private ModConfigMenu cfgMenu => this.menu as ModConfigMenu;
        internal static MenuTab menuTab;
        internal static OpTab activeTab;

        /// <summary>
        /// Currently active Tab's Index. To change this, use (smth about OI's interfacing method)
        /// </summary>
        public static int activeTabIndex { get; private set; }

        internal static OptionInterface activeInterface => OptItfs[activeItfIndex];
        internal int scrollInitDelay { get; private set; }
        internal int scrollDelay { get; private set; }

        internal static void ChangeActiveTab(int newIndex)
        {
            // Close
            activeTab?.Deactivate();
            // Open
            activeTabIndex = newIndex;
            activeTab = activeInterface.Tabs[activeTabIndex];
            activeTab.Activate();
        }

        internal void ChangeActiveMod(int newIndex)
        {
            // Close
            activeTab?.Deactivate();
            savedActiveTabIndex[activeItfIndex] = activeTabIndex;

            // Open
            activeItfIndex = newIndex;
            activeTabIndex = savedActiveTabIndex[activeItfIndex];
            activeTab = activeInterface.Tabs[activeTabIndex];
            activeTab.Activate();

            menuTab.modList.ScrollToShow(newIndex - 1);
        }

        #region ItfHandler

        /// <summary>
        /// Blacklisted mod from config menu.
        /// </summary>
        internal static string[] blackList = new string[]
        {
            "CompletelyOptional",
            "ConfigMachine",
            "ComOptPlugin",
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

        private void LoadItfs()
        {
            List<OptionInterface> listItf = new List<OptionInterface>();
            mute = true;

            // Load Plugins

            #region Load

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
                        oi = new InternalOI_Blank(plugin);
                    }
                    else if (method.Invoke(plugin, null) is OptionInterface itf)
                    {
                        oi = itf;
                        //Your code
                        ComOptPlugin.LogInfo($"Loaded OptionInterface from {oi.rwMod.ModID} (type: {oi.GetType()})");
                    }
                    else
                    {
                        oi = new InternalOI_Blank(plugin);
                        ComOptPlugin.LogInfo($"{oi.rwMod.ModID} did not return an OptionInterface in LoadOI.");
                    }
                }
                catch (Exception ex)
                {
                    oi = new InternalOI_Blank(plugin);
                    if (blackList.Contains(oi.rwMod.ModID) || !char.IsLetter(oi.rwMod.ModID[0]))
                    { continue; }

                    ComOptPlugin.LogWarning($"{oi.rwMod.ModID} threw an exception in LoadOI: {ex.Message}");
                }
                if (blackList.Contains(oi.rwMod.ModID) || !char.IsLetter(oi.rwMod.ModID[0])) { continue; }
                if (oi.rwMod.ModID == "SlugBase") { OptionInterface.SlugBaseExists = true; }

                if (oi is InternalOI && plugin.Config.Keys.Count > 0)
                {
                    // Use BepInEx Configuration
                    oi = new GeneratedOI(oi.rwMod, plugin.Config); // temp disable
                }

                // Initialize
                try
                {
                    oi.Initialize();
                }
                catch (Exception ex)
                {
                    oi = new InternalOI_Error(plugin, ex);
                    oi.Initialize();
                }

                listItf.Add(oi);
            }

            if (ComOptPlugin.testing)
            {
                OptionInterface t = new InternalOI_Test();
                try { t.Initialize(); }
                catch (Exception e) { t = new InternalOI_Error(t.rwMod, e); t.Initialize(); }
                listItf.Add(t);
            }

            #endregion Load

            #region Sort

            // Some code that about starred mods here so it would go first
            // Will think of how to save this list

            if (listItf.Count > 0)
            {
                listItf.Sort(CompareOIModID);
                OptItfs = new OptionInterface[listItf.Count + 1];
                OptItfs[0] = new InternalOI_Stats();
                OptItfs[0].Initialize();
                for (int k = 0; k < listItf.Count; k++)
                { OptItfs[k + 1] = listItf[k]; }
            }
            else
            { // No Mod
                OptItfs = new OptionInterface[] { new InternalOI_NoMod() };
                OptItfID = new string[] { OptItfs[0].rwMod.ModID };
                savedActiveTabIndex = new int[] { 0 };
                OptItfChanged = new bool[] { false };
                OptItfABC = new int[26];
                for (int j = 0; j < OptItfABC.Length; j++) { OptItfABC[j] = -1; }
                goto sorted;
            }

            #endregion Sort

            #region Regist

            OptItfID = new string[OptItfs.Length];
            savedActiveTabIndex = new int[OptItfs.Length];
            OptItfChanged = new bool[OptItfs.Length];
            OptItfABC = new int[26]; // ABC
            uint a = 97; //a - 1
            for (int i = 0; i < OptItfs.Length; i++)
            {
                // Save IDs
                OptItfID[i] = GenerateID(OptItfs[i].rwMod);
                string name = ListItem.GetRealName(OptItfs[i].rwMod.ModID);
                savedActiveTabIndex[i] = 0; OptItfChanged[i] = false;

                // Deactivate Tabs
                for (int t = 0; t < OptItfs[i].Tabs.Length; t++)
                { OptItfs[i].Tabs[t].Deactivate(); }

                // Save indexes of mods starting with ABC
                if (i == 0) { continue; } //Ignore InternalOI_Stats
                if (name[0] < a || !char.IsLetter(name[0])) { continue; }
                while (name[0] > a && a < 123)
                { OptItfABC[a - 97] = -1; a++; }
                if (name[0] == a) { OptItfABC[a - 97] = i; a++; continue; }
            }
            while (a < 123) { OptItfABC[a - 97] = -1; a++; }

        #endregion Regist

        sorted:

            ConfigContainer.mute = false;
        }

        /// <summary>
        /// Array of OptionInterface Instances
        /// </summary>
        internal static OptionInterface[] OptItfs;

        /// <summary>
        /// Loaded OptionInterface Instances' ModIDs
        /// </summary>
        internal static string[] OptItfID;

        internal static int FindItfIndex(UIelement element) => FindItfIndex(element.tab.owner);

        internal static int FindItfIndex(OptionInterface itf) => FindItfIndex(itf.rwMod);

        internal static int FindItfIndex(RainWorldMod rwMod) => Array.IndexOf(OptItfID, GenerateID(rwMod));

        /// <summary>
        /// ABC index of ModIDs
        /// </summary>
        internal static int[] OptItfABC;

        internal static int activeItfIndex { get; private set; }
        internal static int[] savedActiveTabIndex;
        internal static bool[] OptItfChanged { get; private set; }

        internal static string GenerateID(string ModID, string author)
        {
            ModID = ModID.Replace(' ', '_');
            if (string.IsNullOrEmpty(author)) { return ModID; }
            author = author.Replace(' ', '_');
            return $"{ModID}-{author}";
        }

        internal static string GenerateID(RainWorldMod rwMod) => GenerateID(rwMod.ModID, rwMod.author);

        private static readonly CompareInfo comInfo = CultureInfo.InvariantCulture.CompareInfo;

        /// <summary>
        /// Comparator for Sorting OptionInterfaces by ModID
        /// </summary>
        private static int CompareOIModID(OptionInterface x, OptionInterface y)
        {
            return
                comInfo.Compare(ListItem.GetRealName(GenerateID(x.rwMod.ModID, x.rwMod.author)),
                ListItem.GetRealName(GenerateID(y.rwMod.ModID, y.rwMod.author)),
                CompareOptions.StringSort);
        }

        #endregion ItfHandler

        #region FocusHandler

        private List<UIelement> focusables
        {
            get
            {
                List<UIelement> list = new List<UIelement>();
                foreach (UIelement item in menuTab.focusables)
                { if (!item.isInactive) { list.Add(item); } }
                if (activeTab != null)
                {
                    foreach (UIelement item in activeTab.focusables)
                    { if (!item.isInactive) { list.Add(item); } }
                }

                List<UIelement> res = new List<UIelement>();
                if (menu.manager.menuesMouseMode)
                {
                    foreach (UIelement item in list)
                    { if (!(item is OpScrollBox)) { res.Add(item); } }
                }
                else
                {
                    if (focusedElement == null || !focusedElement.inScrollBox)
                    {// Candidates are items that aren't in scrollBox
                        foreach (UIelement item in list)
                        { if (!item.inScrollBox) { res.Add(item); } }
                    }
                    else
                    {
                        foreach (UIelement item in list)
                        { if (item.inScrollBox && item.scrollBox == focusedElement.scrollBox) { res.Add(item); } }
                    }
                }
                return res;
            }
        }

        public static UIelement focusedElement { get; private set; }
        private static UIelement lastFocusedElement;
        internal static bool holdElement;

        /// <summary>
        /// Change <see cref="focusedElement"/>
        /// </summary>
        /// <param name="element"><see cref="ICanBeFocused"/> for new focus</param>
        internal void FocusNewElement(UIelement element)
        {
            if (element == null) { return; }
            if (!(element is ICanBeFocused))
            { ComOptPlugin.LogWarning($"{element.GetType()} is not ICanBeFocused. FocusNewElement ignored."); return; }
            if (element != focusedElement)
            {
                lastFocusedElement = focusedElement;
                focusedElement = element;
                PlaySound((focusedElement as ICanBeFocused).GreyedOut
                    ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                // Always play Gamepad sound even in Mouse mode, as this is called by either Gamepad or Modder
            }
        }

        /// <summary>
        /// Request moving focus to new relative direction from currently focused element
        /// </summary>
        /// <param name="direction">+x is Rightside, +y is Upside</param>
        internal void FocusNewElementInDirection(IntVector2 direction)
        {
            UIelement element = FocusCandidate(direction);
            FocusNewElement(element);
        }

        private UIelement FocusCandidate(IntVector2 direction)
        {
            if (focusedElement == null)
            { // current mod button
                return menuTab.modList.GetCurrentModButton();
            }
            if (!(focusedElement is ICanBeFocused)) { return lastFocusedElement; }
            UIelement result = lastFocusedElement;
            Vector2 curCenter = focusedElement.CenterPos();
            List<UIelement> candidates = this.focusables;
            float likelihood = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] is ICanBeFocused
                    && (candidates[i] as ICanBeFocused).CurrentlyFocusableNonMouse
                    && (candidates[i] as ICanBeFocused) != focusedElement)
                {
                    Vector2 cndCenter = candidates[i].CenterPos();
                    // if (direction.y == 0 || cndCenter.y < curCenter.y != direction.y < 0)
                    // {
                    //     bool flag = direction.x != 0 && cndCenter.x < curCenter.x == direction.x < 0;
                    // }
                    float dist = Vector2.Distance(curCenter, cndCenter);
                    Vector2 dir = Custom.DirVec(curCenter, cndCenter);
                    float angle = 0.5f - 0.5f * Vector2.Dot(dir, direction.ToVector2().normalized);
                    if (angle > 0.5f)
                    {
                        if (angle > 0.8f)
                        {
                            angle = 0.5f - 0.5f * Vector2.Dot(-dir, direction.ToVector2().normalized);
                            dist = Vector2.Distance(curCenter, cndCenter + direction.ToVector2() * ((direction.x == 0) ? 1800f : 2400f));
                        }
                        else { dist += 100000f; }
                    }
                    angle *= 50f;
                    float cndLikelihood = (1f + dist) * (1f + angle);
                    if (cndLikelihood < likelihood)
                    {
                        result = candidates[i];
                        likelihood = cndLikelihood;
                    }
                }
            }
            return result;
        }

        #endregion FocusHandler

        // Called by ModConfigMenu.GrafUpdate
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            menuTab.GrafUpdate(timeStacker);
            try { activeTab.GrafUpdate(timeStacker); }
            catch (Exception ex) { InterfaceUpdateError(true, ex); }
        }

        internal bool allowFocusMove;

        private bool lastPressZ;

        // Called by ModConfigMenu.Update
        public override void Update()
        {
            base.Update();
            _soundFill = _soundFill > 0 ? _soundFill - 1 : 0;
            if (menu.ForceNoMouseMode) { focusedElement = null; return; } // == FreezeMenuFunctions

            UIelement focusedElementBeforeUpdate = focusedElement;
            allowFocusMove = true;

            #region UIelement.Update

            try { activeInterface.Update(); }
            catch (Exception ex) { InterfaceUpdateError(false, ex); }
            if (holdElement)
            {
                try
                {
                    if (focusedElement != null && !(focusedElement as ICanBeFocused).GreyedOut) { focusedElement.Update(); }
                    else { holdElement = false; }
                }
                catch (Exception ex) { InterfaceUpdateError(true, ex); }
            }
            if (!holdElement)
            {
                menuTab.Update();
                try { activeTab.Update(); }
                catch (Exception ex) { InterfaceUpdateError(true, ex); }
            }

            #endregion UIelement.Update

            #region FocusManage

            allowFocusMove = allowFocusMove && focusedElementBeforeUpdate == focusedElement;

            if (!holdElement && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            { //Undo
                if (Input.GetKey(KeyCode.Z)) { if (!lastPressZ) { UndoConfigChange(); } lastPressZ = true; }
                else { lastPressZ = false; }
            }
            if (menu.manager.menuesMouseMode)
            { // Mouse Mode
                if (!holdElement)
                {
                    UIelement lastFocus = focusedElement;
                    focusedElement = null;
                    List<UIelement> list = this.focusables;
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((list[j] as ICanBeFocused).CurrentlyFocusableMouse && list[j].MouseOver)
                        {
                            focusedElement = list[j];
                            if (focusedElement != lastFocus)
                            {
                                PlaySound((focusedElement as ICanBeFocused).GreyedOut
                                    ? SoundID.MENU_Greyed_Out_Button_Select_Mouse : SoundID.MENU_Button_Select_Mouse);
                            }
                            break;
                        }
                    }
                }
                else // holdElement
                {
                    if (focusedElement is UIconfig && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    { // Copy & Paste
                        if (Input.GetKey(KeyCode.V) && !string.IsNullOrEmpty(UniClipboard.GetText()))
                        {
                            string grab = UniClipboard.GetText();
                            if ((focusedElement as UIconfig).CopyFromClipboard(grab))
                            {
                                (focusedElement as UIconfig).bumpBehav.flash = 1f;
                                if ((focusedElement as UIconfig).cosmetic)
                                { cfgMenu.ShowAlert(InternalTranslator.Translate("Pasted <Text> from Clipboard").Replace("<Text>", grab)); }
                                else
                                { cfgMenu.ShowAlert(InternalTranslator.Translate("Pasted <Text> from Clipboard to <ObjectName>").Replace("<Text>", grab).Replace("<ObjectName>", (focusedElement as UIconfig).key)); }
                            }
                        }
                        else if (Input.GetKey(KeyCode.C))
                        {
                            string grab = (focusedElement as UIconfig).CopyToClipboard();
                            if (!string.IsNullOrEmpty(grab))
                            {
                                (focusedElement as UIconfig).bumpBehav.flash = 1f;
                                UniClipboard.SetText(grab);
                                if ((focusedElement as UIconfig).cosmetic)
                                { cfgMenu.ShowAlert(InternalTranslator.Translate("Copied <Text> to Clipboard").Replace("<Text>", grab)); }
                                else
                                { cfgMenu.ShowAlert(InternalTranslator.Translate("Copied <Text> to Clipboard from <ObjectName>").Replace("<Text>", grab).Replace("<ObjectName>", (focusedElement as UIconfig).key)); }
                            }
                        }
                    }
                }
            }
            else
            { // Controller/Keyboard Mode
                if (!allowFocusMove) { this.scrollDelay = 0; } // Focus changed by OI.Update
                else if (!holdElement)
                {
                    if (menu.input.jmp && !menu.lastInput.jmp) // Hold Element
                    { holdElement = true; }
                    else // Switch Focus
                    {
                        if (menu.input.y != 0 && menu.lastInput.y != menu.input.y)
                        { this.FocusNewElementInDirection(new IntVector2(0, menu.input.y)); }
                        else if (menu.input.x != 0 && menu.lastInput.x != menu.input.x)
                        { this.FocusNewElementInDirection(new IntVector2(menu.input.x, 0)); }
                        if (menu.input.y != 0 && menu.lastInput.y == menu.input.y && menu.input.x == 0)
                        { this.scrollInitDelay++; }
                        else if (menu.input.x != 0 && menu.lastInput.x == menu.input.y && menu.input.y == 0)
                        { this.scrollInitDelay++; }
                        else
                        { this.scrollInitDelay = 0; }
                        if (this.scrollInitDelay > ModConfigMenu.DASinit)
                        {
                            this.scrollDelay++;
                            if (this.scrollDelay > ModConfigMenu.DASdelay)
                            {
                                this.scrollDelay = 0;
                                if (menu.input.y != 0 && menu.lastInput.y == menu.input.y)
                                { this.FocusNewElementInDirection(new IntVector2(0, menu.input.y)); }
                                else if (menu.input.x != 0 && menu.lastInput.x == menu.input.x)
                                { this.FocusNewElementInDirection(new IntVector2(menu.input.x, 0)); }
                            }
                        }
                        else { this.scrollDelay = 0; }
                    }
                }
                else
                {
                    if (menu.input.thrw && !menu.lastInput.thrw && focusedElement != null) // Move Upward
                    {
                        UIelement curFocusedElement = focusedElement;
                        if (focusedElement.inScrollBox) // Move to ScrollBox
                        {
                            focusedElement.scrollBox.lastFocusedElement = focusedElement;
                            focusedElement = focusedElement.scrollBox;
                        }
                        else if (!(focusedElement.tab is MenuTab)) // Move to TabController
                        {
                            if (activeInterface.Tabs.Length > 1) { focusedElement = menuTab.tabCtrler.GetCurrentTabButton(); }
                            else { focusedElement = menuTab.modList.GetCurrentModButton(); }
                        }
                        else if (focusedElement is ConfigTabController.TabSelectButton) // Move to Mod List
                        {
                            focusedElement = menuTab.modList.GetCurrentModButton();
                        }
                        else // Move to Save/Back button
                        {
                            focusedElement = OptItfChanged[activeItfIndex] ? menuTab.saveButton : menuTab.backButton;
                        }
                        bool moved;
                        if (curFocusedElement != focusedElement) { lastFocusedElement = curFocusedElement; moved = true; }
                        else { moved = false; }
                        if (moved)
                        {
                            PlaySound((focusedElement as ICanBeFocused).GreyedOut
                                ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                        }
                    }
                }
            }

            #endregion FocusManage
        }

        internal void InterfaceUpdateError(bool tab, Exception ex)
        {
            // Fade out
            // Change itf to Error version
            // Recreate List's mod button to error version
        }

        /// <summary>
        /// Notify config change to Menu
        /// </summary>
        /// <param name="config">Changed <see cref="UIconfig"/></param>
        /// <param name="oldValue">Original <see cref="UIconfig.value"/> before the change</param>
        /// <param name="value">New <see cref="UIconfig.value"/></param>
        internal void NotifyConfigChange(UIconfig config, string oldValue, string value)
        {
            OptItfChanged[FindItfIndex(config)] = true;
            if (history.Count > 0)
            {
                ConfigHistory last = history.Peek();
                if (last.config == config) //(last.config.key == config.key)
                {
                    oldValue = history.Pop().origValue;
                    if (oldValue == value) { return; } // User-reverted config; Remove history
                }
            }
            history.Push(new ConfigHistory() { config = config, origValue = oldValue });
            // ComOptPlugin.LogMessage($"ConfigChange[{history.Count}]) {history.Peek().config.GetType()}({history.Peek().config.key}): [{history.Peek().origValue}] ==> [{value}]");
        }

        internal void UndoConfigChange() // Ctrl + Z or Grab + Jump
        {
            // ComOptPlugin.LogMessage($"UndoConfig[{history.Count}]) {history.Peek().config.GetType()}({history.Peek().config.key}): [{history.Peek().origValue}] <== [{history.Peek().config.value}]");

            if (history.Count > 0)
            {
                ConfigHistory last = history.Pop();
                string curValue = last.config.value;
                last.config.ForceValue(last.origValue);
                last.config.OnChange();
                OptItfChanged[FindItfIndex(last.config)] = true;
                // Alert
                ModConfigMenu.instance.ShowAlert(
                    "Undo change of the value of [<UIconfigKey>] from [<CurrentValue>] to [<OriginalValue>]"
                    .Replace("<UIconfigKey>", last.config.key).Replace("<CurrentValue>", curValue).Replace("<OriginalValue>", last.config.value)
                    );
            }
        }

        /// <summary>
        /// History for undo
        /// </summary>
        private readonly Stack<ConfigHistory> history;

        private struct ConfigHistory
        {
            public UIconfig config;
            public string origValue;
        }

        #region Sound

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID)
        {
            if (_soundFilled || ModConfigMenu.instance.manager.rainWorld.options.soundEffectsVolume == 0f) { return; }
            _soundFill += GetSoundFill(soundID);
            ModConfigMenu.instance.PlaySound(soundID);
        }

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID, float, float, float)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID, float pan, float vol, float pitch)
        {
            if (_soundFilled || ModConfigMenu.instance.manager.rainWorld.options.soundEffectsVolume == 0f) { return; }
            _soundFill += GetSoundFill(soundID);
            ModConfigMenu.instance.PlaySound(soundID, pan, vol, pitch);
        }

        private static int GetSoundFill(SoundID soundID)
        {
            SoundLoader.SoundData soundData = ModConfigMenu.instance.manager.menuMic.GetSoundData(soundID, -1);
            AudioClip clip = ModConfigMenu.instance.manager.menuMic.soundLoader.GetAudioClip(soundData.audioClip);
            if (clip == null) { return 0; }
            return Mathf.CeilToInt(Mathf.Sqrt(clip.length * 60.0f)) + 1;
        }

        /// <summary>
        /// Add number (in proportion with sound effect's length) to this whenever you're playing soundeffect. See also <seealso cref="_soundFilled"/>
        /// </summary>
        private static int _soundFill;

        /// <summary>
        /// Whether the sound engine is full or not. See also <seealso cref="_soundFill"/>
        /// </summary>
        private static bool _soundFilled => _soundFill > UIelement.FrameMultiply(300) || mute;

        /// <summary>
        /// Whether to play sound or not
        /// </summary>
        public static bool mute { get; internal set; }

        #endregion Sound
    }
}