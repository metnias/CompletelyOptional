﻿using BepInEx;
using CompletelyOptional;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Contains all UIelement, connects UIelements and Rain World Menu
    /// </summary>
    public class ConfigContainer : Menu.MenuObject
    {
        public ConfigContainer(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            // Initialize
            _soundFill = 0;
            holdElement = false;
            history = new Stack<ConfigHistory>();

            // Load OptionInterfaces
            if (!_loadedOIs) { LoadItfs(); }

            menuTab = new MenuTab();
            activeItfIndex = 0;
            activeTabIndex = 0;
            activeTab = activeInterface.Tabs[activeTabIndex];

            lastFocusedElement = menuTab.backButton;
            focusedElement = menuTab.backButton;
        }

        internal static MenuTab menuTab;
        internal static OpTab activeTab;
        public static int activeTabIndex { get; private set; }
        internal static OptionInterface activeInterface => OptItfs[activeItfIndex];
        private int scrollInitDelay;
        private int scrollDelay;

        internal static void ChangeActiveTab(int newIndex)
        {
            if (activeTab == null) { activeTab.Deactivate(); }
            activeTabIndex = newIndex;
            activeTab = activeInterface.Tabs[activeTabIndex];
            activeTab.Activate();
        }

        internal static bool holdElement;

        #region ItfHandler

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

        internal void LoadItfs()
        {
            List<OptionInterface> listItf = new List<OptionInterface>();
            ConfigContainer.mute = true;

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

                listItf.Add(oi);
            }

            #endregion Load

            #region Sort

            // Some code that about starred mods here so it would go first
            // Will think of how to save this list

            listItf.Sort(CompareOIModID);
            OptItfs = listItf.ToArray();
            OptItfID = new string[OptItfs.Length];
            OptItfABC = new int[26];
            uint a = 97; //a
            for (int i = 0; i < OptItfs.Length; i++)
            {
                // Save IDs
                OptItfID[i] = GenerateID(OptItfs[i].rwMod);
                string name = ListItem.GetRealName(OptItfs[i].rwMod.ModID);

                // Deactivate Tabs
                for (int t = 0; t < OptItfs[i].Tabs.Length; t++)
                { OptItfs[i].Tabs[t].Deactivate(); }

                // Save indexes of mods starting with ABC
                if (name[0] < (char)a) { continue; }
                if (name[0] == (char)a) { OptItfABC[a - 97] = i; a++; continue; }
                while (name[0] > (char)a && a < 123)
                { OptItfABC[a - 97] = -1; a++; }
            }

            #endregion Sort

            ConfigContainer.mute = false;
            _loadedOIs = true;
        }

        private static bool _loadedOIs = false;

        /// <summary>
        /// Array of OptionInterface Instances
        /// </summary>
        internal static OptionInterface[] OptItfs;

        /// <summary>
        /// Loaded OptionInterface Instances' ModIDs
        /// </summary>
        internal static string[] OptItfID;

        internal static int[] OptItfABC;
        internal static int activeItfIndex { get; private set; }

        internal static string GenerateID(string ModID, string author)
        {
            ModID = ModID.Replace(' ', '_');
            if (string.IsNullOrEmpty(author)) { return ModID; }
            author = author.Replace(' ', '_');
            return $"{ModID}-{author}";
        }

        internal static string GenerateID(RainWorldMod rwMod) => GenerateID(rwMod.ModID, rwMod.author);

        /// <summary>
        /// Comparator for Sorting OptionInterfaces by ModID
        /// </summary>
        private static int CompareOIModID(OptionInterface x, OptionInterface y)
        {
            return ListItem.GetRealName(GenerateID(x.rwMod.ModID, x.rwMod.author))
                  .CompareTo(ListItem.GetRealName(GenerateID(y.rwMod.ModID, y.rwMod.author)));
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
                    if (this.focusedElement == null || !this.focusedElement.inScrollBox)
                    {// Candidates are items that aren't in scrollBox
                        foreach (UIelement item in list)
                        { if (!item.inScrollBox) { res.Add(item); } }
                    }
                    else
                    {
                        foreach (UIelement item in list)
                        { if (item.inScrollBox && item.scrollBox == this.focusedElement.scrollBox) { res.Add(item); } }
                    }
                }
                return res;
            }
        }

        public UIelement focusedElement { get; private set; }
        private UIelement lastFocusedElement;

        /// <summary>
        /// Change <see cref="focusedElement"/>
        /// </summary>
        /// <param name="element"><see cref="ICanBeFocused"/> for new focus</param>
        internal void FocusNewElement(UIelement element)
        {
            if (element == null) { return; }
            if (!(element is ICanBeFocused))
            { ComOptPlugin.LogWarning($"{element.GetType()} is not ICanBeFocused. FocusNewElement ignored."); return; }
            if (element != this.focusedElement)
            {
                this.lastFocusedElement = this.focusedElement;
                this.focusedElement = element;
                PlaySound((this.focusedElement as ICanBeFocused).GreyedOut
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
            UIelement element = this.FocusCandidate(direction);
            this.FocusNewElement(element);
        }

        private UIelement FocusCandidate(IntVector2 direction)
        {
            if (this.focusedElement == null)
            { // current mod button
                return menuTab.modList;
            }
            if (!(this.focusedElement is ICanBeFocused))
            {
                return this.lastFocusedElement;
            }
            UIelement result = this.lastFocusedElement;
            Vector2 curCenter = this.focusedElement.CenterPos();
            List<UIelement> candidates = this.focusables;
            float likelihood = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] is ICanBeFocused
                    && (candidates[i] as ICanBeFocused).CurrentlyFocusableNonMouse
                    && (candidates[i] as ICanBeFocused) != this.focusedElement)
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
            activeTab.GrafUpdate(timeStacker);
        }

        // Called by ModConfigMenu.Update
        public override void Update()
        {
            base.Update();
            _soundFill = _soundFill > 0 ? _soundFill - 1 : 0;
            if (menu.ForceNoMouseMode) { this.focusedElement = null; return; } // == FreezeMenuFunctions
            if (menu.manager.menuesMouseMode)
            { // Mouse Mode
                if (!holdElement)
                {
                    UIelement lastFocus = this.focusedElement;
                    this.focusedElement = null;
                    List<UIelement> list = this.focusables;
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((list[j] as ICanBeFocused).CurrentlyFocusableMouse && list[j].MouseOver)
                        {
                            this.focusedElement = list[j];
                            if (this.focusedElement != lastFocus)
                            {
                                PlaySound((this.focusedElement as ICanBeFocused).GreyedOut
                                    ? SoundID.MENU_Greyed_Out_Button_Select_Mouse : SoundID.MENU_Button_Select_Mouse);
                            }
                            break;
                        }
                    }
                }
            }
            else
            { // Controller/Keyboard Mode
                if (!holdElement)
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
                        bool moved = false;
                        if (focusedElement.inScrollBox) // Move to ScrollBox
                        {
                            focusedElement = focusedElement.scrollBox;
                        }
                        else if (!(focusedElement.tab is MenuTab)) // Move to TabController
                        {
                            if (activeInterface.Tabs.Length > 1) { focusedElement = menuTab.tabCtrler; }
                            else { focusedElement = menuTab.modList; }
                        }
                        else if (focusedElement is ConfigTabController) // Move to Mod List
                        {
                            focusedElement = menuTab.modList;
                        }
                        else // Move to Save button
                        {
                            focusedElement = menuTab.saveButton;
                        }
                        if (moved)
                        {
                            PlaySound((focusedElement as ICanBeFocused).GreyedOut
                                ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                        }
                    }
                    if (menu.input.mp && !menu.lastInput.mp && focusedElement is MenuModList) // Starred
                    {
                    }
                }
            }
            activeInterface.Update();
            if (holdElement)
            {
                if (this.focusedElement != null && !(this.focusedElement as ICanBeFocused).GreyedOut) { this.focusedElement.Update(); }
                else { holdElement = false; }
            }
            if (!holdElement)
            {
                menuTab.Update();
                activeTab.Update();
            }
        }

        /// <summary>
        /// Notify config change to Menu
        /// </summary>
        /// <param name="config">Changed <see cref="UIconfig"/></param>
        /// <param name="oldValue">Original <see cref="UIconfig.value"/> before the change</param>
        /// <param name="value">New <see cref="UIconfig.value"/></param>
        internal void NotifyConfigChange(UIconfig config, string oldValue, string value)
        {
            if (history.Count > 0)
            {
                ConfigHistory last = history.Peek();
                if (last.config.key == config.key)
                {
                    oldValue = history.Pop().origValue;
                    if (oldValue == value) { return; } // User-reverted config; Remove history
                }
            }
            history.Push(new ConfigHistory() { config = config, origValue = oldValue });
            // configChanged = true; == history.Count > 0
            // cfgContainer.menuTab.saveButton.text = InternalTranslator.Translate("APPLY");
        }

        /// <summary>
        /// History for undo
        /// </summary>
        private Stack<ConfigHistory> history;

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
            return Mathf.CeilToInt(clip.length * 60.0f) + 1;
        }

        /// <summary>
        /// Add number (in proportion with sound effect's length) to this whenever you're playing soundeffect. See also <seealso cref="_soundFilled"/>
        /// </summary>
        private static int _soundFill;

        /// <summary>
        /// Whether the sound engine is full or not. See also <seealso cref="_soundFill"/>
        /// </summary>
        private static bool _soundFilled => _soundFill > UIelement.FrameMultiply(80) || mute;

        /// <summary>
        /// Whether to play sound or not
        /// </summary>
        public static bool mute { get; internal set; }

        #endregion Sound
    }
}