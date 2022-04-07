using BepInEx;
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
    public class ConfigContainer : PositionedMenuObject
    {
        public ConfigContainer(Menu.Menu menu, MenuObject owner) : base(menu, owner, Vector2.zero)
        {
            // Initialize
            instance = this;
            myContainer = new FContainer();
            owner.Container.AddChild(myContainer);
            _soundFill = 0;
            holdElement = false;
            history = new Stack<ConfigHistory>();
            OpKeyBinder.InitBoundKey();

            // Load OptionInterfaces
            if (!cfgMenu.isReload) { LoadItfs(); }
            else { ReloadItfs(false); }

            menuTab = new MenuTab();
            lastFocusedElement = menuTab.backButton;
            focusedElement = menuTab.backButton;

            activeItfIndex = activeItfIndex >= OptItfs.Length ? 0 : activeItfIndex;
            activeTabIndex = savedActiveTabIndex[activeItfIndex];
            activeTab = activeInterface.Tabs[activeTabIndex];

            menuTab.Activate();
            activeTab.Activate();

            cursor = new FocusCursor(this);
            this.subObjects.Add(cursor);

            // Update Once
            GrafUpdate(0f);
            Update();
        }

        private FocusCursor cursor;
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

        internal static void ChangeActiveMod(int newIndex)
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

            foreach (KeyValuePair<string, PluginInfo> pair in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                BaseUnityPlugin plugin = pair.Value.Instance;
                RainWorldMod rwMod = new RainWorldMod(plugin);
                if (blackList.Contains(rwMod.ModID) || !char.IsLetter(rwMod.ModID[0])) { continue; }
                if (rwMod.ModID == "SlugBase") { OptionInterface.SlugBaseExists = true; }

                OptionInterface oi;

                // See if registered
                if (!MachineConnector.registeredOIs.TryGetValue(GenerateID(rwMod), out oi))
                {
                    if (plugin.Config.Keys.Count > 0) // Generate OI automatically
                    { oi = new GeneratedOI(plugin); }
                    else // Doesn't support CM
                    { oi = new InternalOI_Blank(plugin); }
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

                try { oi.LoadConfig(); oi.ShowConfig(); }
                catch (Exception ex)
                {
                    oi = new InternalOI_Error(plugin, new LoadDataException(string.Concat("OILoad/ShowConfigError: ", rwMod.ModID, " had a problem in Load/ShowConfig()\nAre you editing LoadConfig()/ShowConfig()? That could cause serious error.",
                           Environment.NewLine, ex
                           )));
                }

                listItf.Add(oi);
            }

            if (ComOptPlugin.testing)
            {
                OptionInterface t = new InternalOI_Test();
                try { t.Initialize(); }
                catch (Exception e) { t = new InternalOI_Error(t.rwMod, e); t.Initialize(); }
                listItf.Add(t);

                // dummy mods
                for (int i = 0; i < 50; i++)
                {
                    string name = (UnityEngine.Random.value < 0.15f ? "The " + LoremIpsum.Generate(1, 1, 1).Split(' ')[0]
                        : LoremIpsum.Generate(1, 1, 1).Split(' ')[0])
                        + (UnityEngine.Random.value < 0.3f ? " Mod" : "");
                    string ver = (UnityEngine.Random.value < 0.7f ? "0" : "1") + ".";
                    if (UnityEngine.Random.value < 0.7f)
                    {
                        ver += Mathf.FloorToInt(UnityEngine.Random.value * 10f) + ".";
                        ver += Mathf.FloorToInt(UnityEngine.Random.value * 10f) + ".";
                        ver += Mathf.FloorToInt(UnityEngine.Random.value * 10f);
                    }
                    else { ver = RainWorldMod.authorNull; }
                    listItf.Add(new InternalOI_Blank(new RainWorldMod(name + i) { version = ver }));
                    listItf[listItf.Count - 1].Initialize();
                }
            }

            #endregion Load

            #region Sort

            // Some code that about starred mods here so it would go first
            // Will think of how to save this list

            if (listItf.Count > 0)
            {
                listItf.Sort(CompareOIModID);
                if (OptItfs != null && OptItfs.Length == listItf.Count + 1)
                {
                    for (int i = 0; i < listItf.Count; i++)
                    {
                        if (OptItfID[i + 1] != GenerateID(listItf[i].rwMod))
                        { goto newMods; } // Mods has changed
                    }
                    // No mod changes; Refresh OptItfs anyway
                    OptItfs = new OptionInterface[listItf.Count + 1];
                    OptItfs[0] = new InternalOI_Stats();
                    OptItfs[0].Initialize();
                    for (int k = 0; k < listItf.Count; k++)
                    { OptItfs[k + 1] = listItf[k]; }
                    ReloadItfs(true); return;
                }
            newMods:
                OptItfs = new OptionInterface[listItf.Count + 1];
                OptItfs[0] = new InternalOI_Stats();
                OptItfs[0].Initialize();
                for (int k = 0; k < listItf.Count; k++)
                { OptItfs[k + 1] = listItf[k]; }
            }
            else
            { // No Mod
                if (OptItfs != null && OptItfs.Length == 1) { ReloadItfs(false); return; }
                OptItfs = new OptionInterface[] { new InternalOI_NoMod() };
                OptItfID = new string[] { OptItfs[0].rwMod.ModID };
                savedActiveTabIndex = new int[] { 0 };
                OptItfChanged = new bool[] { false };
                OptItfABC = new int[26];
                for (int j = 0; j < OptItfABC.Length; j++) { OptItfABC[j] = -1; }
                goto sorted;
            }

            #endregion Sort

            RegistItfs(false);

        sorted:

            mute = false;
        }

        private void ReloadItfs(bool noInit)
        {
            // ComOptPlugin.LogMessage($"Reload! isReload: {cfgMenu.isReload}; noInit: {noInit}");
            mute = true;

            if (!noInit)
            {
                for (int i = 0; i < OptItfs.Length; i++)
                {
                    try
                    {
                        OptItfs[i].Initialize();
                    }
                    catch (Exception ex)
                    {
                        OptItfs[i] = new InternalOI_Error(OptItfs[i].rwMod, ex);
                        OptItfs[i].Initialize();
                    }
                }
            }

            RegistItfs(true);

            mute = false;
        }

        private void RegistItfs(bool reload)
        {
            if (!reload)
            {
                OptItfID = new string[OptItfs.Length];
                savedActiveTabIndex = new int[OptItfs.Length];
                OptItfChanged = new bool[OptItfs.Length];
                OptItfABC = new int[26]; // ABC
            }
            uint a = 97; //a - 1
            for (int i = 0; i < OptItfs.Length; i++)
            {
                // Save IDs
                if (!reload)
                {
                    OptItfID[i] = GenerateID(OptItfs[i].rwMod);
                    savedActiveTabIndex[i] = 0;
                }
                else { savedActiveTabIndex[i] = savedActiveTabIndex[i] >= OptItfs[i].Tabs.Length ? 0 : savedActiveTabIndex[i]; }
                string name = !reload ? ListItem.GetRealName(OptItfs[i].rwMod.ModID) : "";
                OptItfChanged[i] = false;

                // Deactivate Tabs
                for (int t = 0; t < OptItfs[i].Tabs.Length; t++)
                { OptItfs[i].Tabs[t]?.Deactivate(); }

                // Save indexes of mods starting with ABC
                if (!reload)
                {
                    if (i == 0) { continue; } //Ignore InternalOI_Stats
                    if (name[0] < a || !char.IsLetter(name[0])) { continue; }
                    while (name[0] > a && a < 123)
                    { OptItfABC[a - 97] = -1; a++; }
                    if (name[0] == a) { OptItfABC[a - 97] = i; a++; continue; }
                }
            }
            if (!reload) { while (a < 123) { OptItfABC[a - 97] = -1; a++; } }
        }

        internal void ShutdownConfigContainer()
        {
            savedActiveTabIndex[activeItfIndex] = activeTabIndex;
            // Save InternalOI_Stats & StarredMods to ComOptPlugin

            #region UnloadItfs

            for (int i = 0; i < OptItfs.Length; i++)
            {
                if (OptItfs[i].Tabs == null) { continue; }
                for (int j = 0; j < OptItfs[i].Tabs.Length; j++)
                { OptItfs[i].Tabs[j]?.Unload(); }
            }
            menuTab.Unload();

            #endregion UnloadItfs

            halt = true;
            instance = null;
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

        internal static int FindItfIndex(string generatedID) => Array.IndexOf(OptItfID, generatedID);

        /// <summary>
        /// ABC index of ModIDs
        /// </summary>
        internal static int[] OptItfABC;

        internal static int activeItfIndex { get; private set; } = 0;
        internal static int[] savedActiveTabIndex;
        internal static bool[] OptItfChanged { get; private set; }

        internal static string GenerateID(string ModID, string author)
        {
            ModID = ModID.Replace(' ', '_');
            if (string.IsNullOrEmpty(author) || author == RainWorldMod.authorNull) { return ModID; }
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

        private List<UIelement> GetFocusables()
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
                { if (!(item is OpScrollBox) && (item as ICanBeFocused).CurrentlyFocusableMouse) { res.Add(item); } }
            }
            else
            {
                if (focusedElement == null || !focusedElement.inScrollBox)
                {// Candidates are items that aren't in scrollBox
                    foreach (UIelement item in list)
                    { if (!item.inScrollBox && (item as ICanBeFocused).CurrentlyFocusableNonMouse) { res.Add(item); } }
                }
                else
                {
                    foreach (UIelement item in list)
                    { if (item.inScrollBox && item.scrollBox == focusedElement.scrollBox && (item as ICanBeFocused).CurrentlyFocusableNonMouse) { res.Add(item); } }
                }
            }
            return res;
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
                if (element.inScrollBox) { OpScrollBox.ScrollToChild(element); }
                if (focusedElement.mute) { return; }
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
            List<UIelement> candidates = GetFocusables();
            float likelihood = float.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] is ICanBeFocused
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
            if (result.tab is MenuTab && (result == focusedElement || focusedElement.tab is MenuTab))
            { // Exception Matchmaking; jump into tab
                bool fromLeft;
                if (result is MenuTab.MenuButton || result is MenuTab.MenuHoldButton)
                {
                    if (direction.x == 0)
                    {
                        if (direction.y < 0) { fromLeft = false; goto jumpToActiveTab; }  // Move up into tab
                        else { return focusedElement; } // Ignore Down input
                    }
                    return result;
                }
                if (result.GetType() == focusedElement.GetType() &&
                    (focusedElement is ConfigTabController.TabSelectButton || focusedElement is MenuModList.IAmPartOfModList))
                {
                    if (direction.y == 0)
                    {
                        if (direction.x > 0) { fromLeft = true; goto jumpToActiveTab; } // Move right into tab
                        else { return focusedElement; } // Ignore Left input
                    }
                }
                return result;
            jumpToActiveTab:
                likelihood = float.MaxValue;
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (!(candidates[i].tab is MenuTab)
                        && (candidates[i] as ICanBeFocused) != focusedElement)
                    {
                        Vector2 cndCenter = candidates[i].CenterPos();
                        float cndLikelihood = fromLeft ? cndCenter.x : cndCenter.y; // Far Left or Far Bottom
                        if (cndLikelihood < likelihood)
                        {
                            result = candidates[i];
                            likelihood = cndLikelihood;
                        }
                    }
                }
            }
            else if (result.tab is MenuTab && !(focusedElement.tab is MenuTab))
            { // Postpone escaping
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (!(candidates[i].tab is MenuTab)
                        && (candidates[i] as ICanBeFocused) != focusedElement)
                    {
                        Vector2 cndCenter = candidates[i].CenterPos();
                        float cndLikelihood;
                        if (direction.x == 0)
                        { cndLikelihood = direction.y > 0 ? cndCenter.y - curCenter.y : curCenter.y - cndCenter.y; }
                        else
                        { cndLikelihood = direction.x > 0 ? cndCenter.x - curCenter.x : curCenter.x - cndCenter.x; }

                        if (cndLikelihood > 1f && cndLikelihood < likelihood)
                        {
                            result = candidates[i];
                            likelihood = cndLikelihood;
                        }
                    }
                }
            }
            return result;
        }

        #endregion FocusHandler

        /// <summary>
        /// Called by <see cref="ModConfigMenu.GrafUpdate"/>
        /// </summary>
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (halt) { return; }
            menuTab.GrafUpdate(timeStacker);
            try { activeTab?.GrafUpdate(timeStacker); }
            catch (Exception ex) { InterfaceUpdateError(true, ex); return; }
        }

        internal bool allowFocusMove;

        public static void ForceMenuMouseMode(bool? value) => instance.forceMouseMode = value;

        private bool? forceMouseMode = null;

        private bool lastPressZ;

        /// <summary>
        /// Called by <see cref="ModConfigMenu.Update"/>
        /// </summary>
        public override void Update()
        {
            bool lastMenuMouseMode = menu.manager.menuesMouseMode;
            base.Update();
            _soundFill = _soundFill > 0 ? _soundFill - 1 : 0;
            if (menu.ForceNoMouseMode || halt) { focusedElement = null; return; } // == FreezeMenuFunctions

            bool ctrlKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (ctrlKey) { forceMouseMode = true; }
            if (forceMouseMode.HasValue) { menu.manager.menuesMouseMode = forceMouseMode.Value; }
            forceMouseMode = null;
            if (lastMenuMouseMode != menu.manager.menuesMouseMode) { MouseModeChange(); }

            void MouseModeChange()
            { // MouseMode changed
                if (menu.manager.menuesMouseMode)
                { // NonMouse > Mouse
                }
                else
                { // Mouse > NonMouse
                    if (focusedElement == null)
                    {
                        UIelement result = lastFocusedElement;
                        Vector2 curCenter = menu.mousePosition - this.pos;
                        List<UIelement> candidates = GetFocusables();
                        float likelihood = float.MaxValue;
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            if (candidates[i].inScrollBox) { continue; }
                            Vector2 cndCenter = (candidates[i] as ICanBeFocused).FocusRect.center;
                            float dist = Vector2.Distance(curCenter, cndCenter);
                            float cndLikelihood = 1f + dist;
                            if (cndLikelihood < likelihood)
                            {
                                result = candidates[i];
                                likelihood = cndLikelihood;
                            }
                        }
                        focusedElement = result;
                    }
                }
                if (holdElement)
                {
                    (focusedElement as ICanBeFocused).NonMouseSetHeld(false);
                    holdElement = false;
                }
            }

            UIelement focusedElementBeforeUpdate = focusedElement;
            allowFocusMove = true;
            bool lastHoldElement = holdElement;

            #region UIelement.Update

            if (holdElement)
            {
                if (focusedElement == null) { holdElement = false; }
                else
                {
                    if (!(focusedElement.tab is MenuTab))
                    {
                        try
                        {
                            if (!(focusedElement as ICanBeFocused).GreyedOut) { focusedElement.Update(); }
                            else { holdElement = false; }
                        }
                        catch (Exception ex) { InterfaceUpdateError(true, ex); return; }
                    }
                }
                if (holdElement) { menuTab.Update(); }
            }
            if (!holdElement)
            {
                menuTab.Update();
                try { activeTab?.Update(); }
                catch (Exception ex) { InterfaceUpdateError(true, ex); return; }
            }
            try { activeInterface.Update(); }
            catch (Exception ex) { InterfaceUpdateError(false, ex); return; }

            #endregion UIelement.Update

            if (forceMouseMode.HasValue) { menu.manager.menuesMouseMode = forceMouseMode.Value; }
            if (lastMenuMouseMode != menu.manager.menuesMouseMode) { MouseModeChange(); }

            #region FocusManage

            allowFocusMove = allowFocusMove && focusedElementBeforeUpdate == focusedElement;

            if (menu.manager.menuesMouseMode)
            { // Mouse Mode
                if (!holdElement)
                {
                    UIelement lastFocus = focusedElement;
                    focusedElement = null;
                    List<UIelement> list = GetFocusables();
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((list[j] as ICanBeFocused).CurrentlyFocusableMouse && list[j].MouseOver)
                        {
                            focusedElement = list[j];
                            if (focusedElement != lastFocus && !focusedElement.mute)
                            {
                                PlaySound((focusedElement as ICanBeFocused).GreyedOut
                                    ? SoundID.MENU_Greyed_Out_Button_Select_Mouse : SoundID.MENU_Button_Select_Mouse);
                            }
                            break;
                        }
                    }
                    if (ctrlKey)
                    { //Undo
                        if (Input.GetKey(KeyCode.Z)) { if (!lastPressZ) { UndoConfigChange(); } lastPressZ = true; }
                        else { lastPressZ = false; }
                    }
                }
                else // holdElement
                {
                    if (focusedElement is UIconfig && ctrlKey)
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
                if (focusedElement == null)
                {
                    if (lastFocusedElement != null) { focusedElement = lastFocusedElement; }
                    else { focusedElement = menuTab.backButton; }
                }
                if (!allowFocusMove) { this.scrollInitDelay = 0; } // Focus changed by OI.Update
                else if (!holdElement && !lastHoldElement)
                {
                    if (menu.input.thrw && !menu.lastInput.thrw) // Move Focus Upward
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
                        if (curFocusedElement != focusedElement)
                        {
                            lastFocusedElement = curFocusedElement;
                            if (!focusedElement.mute)
                            {
                                PlaySound((focusedElement as ICanBeFocused).GreyedOut
                                    ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard);
                            }
                        }
                    }
                    else if (menu.input.jmp && !menu.lastInput.jmp && !(focusedElement as ICanBeFocused).GreyedOut) // Hold Element
                    {
                        (focusedElement as ICanBeFocused).NonMouseSetHeld(true);
                        if (focusedElement.inScrollBox) { OpScrollBox.ScrollToChild(focusedElement); }
                    }
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
                            this.scrollDelay += focusedElement is MenuModList.IAmPartOfModList ? 2 : 1;
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
            }

            #endregion FocusManage
        }

        private class FocusCursor : RectangularMenuObject
        {
            public FocusCursor(ConfigContainer cfg) : base(cfg.menu, cfg.menu.pages[0], Vector2.zero, Vector2.one)
            {
                this.cfg = cfg;
                this.sprites = new FSprite[4];
                for (int i = 0; i < this.sprites.Length; i++)
                {
                    this.sprites[i] = new FSprite("stickLeftA")
                    { rotation = 90f * i, anchorX = 1f, anchorY = 1f, scaleX = 1.25f, color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey) };
                    this.Container.AddChild(this.sprites[i]);
                }
            }

            public readonly ConfigContainer cfg;
            private FSprite[] sprites; // BtmL, TopL, TopR, BtmR

            public Rect? focusRect => (focusedElement as ICanBeFocused)?.FocusRect;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);

                for (int i = 0; i < this.sprites.Length; i++)
                {
                    this.sprites[i].element = Futile.atlasManager.GetElementWithName("stickLeft" + (!holdElement ? "A" : "B"));
                    this.sprites[i].alpha = 1.00f - Mathf.Lerp(lastFade, fade, timeStacker);
                }
                this.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) + 0.01f;
                this.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) + 0.01f;
                this.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) + 0.01f;
                this.sprites[1].y = Mathf.Lerp(lastPos.y + lastSize.y, pos.y + size.y, timeStacker) + 0.01f;
                this.sprites[2].x = Mathf.Lerp(lastPos.x + lastSize.x, pos.x + size.x, timeStacker) + 0.01f;
                this.sprites[2].y = Mathf.Lerp(lastPos.y + lastSize.y, pos.y + size.y, timeStacker) + 0.01f;
                this.sprites[3].x = Mathf.Lerp(lastPos.x + lastSize.x, pos.x + size.x, timeStacker) + 0.01f;
                this.sprites[3].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) + 0.01f;
            }

            private float fade = 1f, lastFade = 1f;

            public override void Update()
            {
                base.Update();

                lastFade = fade;
                if (cfg.menu.manager.menuesMouseMode)
                { fade = Custom.LerpAndTick(fade, 1f, 0.2f, 0.1f / UIelement.frameMulti); }
                else
                { fade = Custom.LerpAndTick(fade, 0f, 0.6f, 0.1f / UIelement.frameMulti); }
                if (this.focusRect.HasValue)
                {
                    Rect rect = this.focusRect.Value;
                    if (focusedElement.inScrollBox) { TrimFocusRectToScrollBox(focusedElement.scrollBox, ref rect); }
                    this.pos = Vector2.Lerp(this.pos, new Vector2(rect.x, rect.y), 0.6f / UIelement.frameMulti);
                    this.size = Vector2.Lerp(this.size, new Vector2(rect.width, rect.height), 0.6f / UIelement.frameMulti);
                }
            }

            private void TrimFocusRectToScrollBox(OpScrollBox scrollBox, ref Rect res)
            {
                Vector2 offset = scrollBox.camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos;
                res.x -= offset.x; res.y -= offset.y;
                // Clamp
                Rect scrollRect = ((ICanBeFocused)scrollBox).FocusRect;
                res.width = Mathf.Max(Mathf.Min(res.width, scrollRect.width, scrollRect.x + scrollRect.width - res.x, res.x + res.width - scrollRect.x), 0f);
                res.height = Mathf.Max(Mathf.Min(res.height, scrollRect.height, scrollRect.y + scrollRect.height - res.y, res.y + res.height - scrollRect.y), 0f);
                res.x = Mathf.Clamp(res.x, scrollRect.x, scrollRect.x + scrollRect.width);
                res.y = Mathf.Clamp(res.y, scrollRect.y, scrollRect.y + scrollRect.height);
            }
        }

        private bool halt = false;

        internal void InterfaceUpdateError(bool tab, Exception ex)
        {
            halt = true;

            // Unload current interface
            for (int i = 0; i < activeInterface.Tabs.Length; i++)
            {
                if (activeInterface.Tabs[i] != null)
                {
                    try { activeInterface.Tabs[i].Unload(); }
                    catch { continue; }
                }
            }
            activeTab = null;

            // Change itf to Error version
            OptItfs[activeItfIndex] = new InternalOI_Error(OptItfs[activeItfIndex].rwMod, ex);

            // Recreate List's mod button to error version

            // Fade out
            cfgMenu.PlaySound(SoundID.MENU_Error_Ping);
            ModConfigMenu.instance.manager.RequestMainProcessSwitch(EnumExt_ComOpt.ModConfigMenu, 0.05f);
        }

        /// <summary>
        /// Notify config change to Menu
        /// </summary>
        /// <param name="config">Changed <see cref="UIconfig"/></param>
        /// <param name="oldValue">Original <see cref="UIconfig.value"/> before the change</param>
        /// <param name="value">New <see cref="UIconfig.value"/></param>
        public void NotifyConfigChange(UIconfig config, string oldValue, string value)
        {
            if (config.cosmetic || config.tab == null) { return; }
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

        internal static void ResetCurrentConfig()
        {
            mute = true;
            foreach (UIconfig config in activeInterface.GrabConfigs().ToArray())
            { config.value = config.GetStringValue(config.cfgEntry.DefaultValue); }
            mute = false;
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