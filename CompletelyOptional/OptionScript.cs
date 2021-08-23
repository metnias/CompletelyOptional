using BepInEx;
using Menu;
using OptionalUI;
using Partiality.Modloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// MonoBehavior part of CompletelyOptional Mod
    /// </summary>
    public class OptionScript : MonoBehaviour
    {
        public OptionScript()
        {
            init = false;
        }

        internal static void ProcessManagerCtor(On.ProcessManager.orig_ctor orig, ProcessManager self, RainWorld rainWorld)
        {
            try
            {
                pm = self;
                rw = rainWorld;
                //if (rw == null) { return; }
                try
                {
                    Initialize();
                }
                catch (Exception ex) { Debug.LogError(ex); Debug.LogException(ex); }
                for (int i = 0; i < dtHistory.Length; i++) { dtHistory[i] = 0.016667f; }
                init = true;

                ConfigMenu.currentTab = null;
            }
            finally
            {
                orig(self, rainWorld);
            }
        }

        public static bool init = false;
        public static bool isOptionMenu = false;

        /// <summary>
        /// RainWorld Instance.
        /// </summary>
        public static RainWorld rw;

        /// <summary>
        /// ProcessManager Instance.
        /// </summary>
        public static ProcessManager pm;

        /// <summary>
        /// ConfigMenu Instance.
        /// </summary>
        public static ConfigMenu configMenu;

        /// <summary>
        /// Prevent Sound Engine from crashing by not letting sound when this is >100
        /// </summary>
        public static int soundFill;

        /// <summary>
        /// All loadedMods directly taken from Partiality ModManager.
        /// </summary>
        private static List<PartialityMod> loadedMods;

        /// <summary>
        /// Loaded Mod Dictionary.
        /// Key: ModID, Value: PartialityMod Instance
        /// </summary>
        private static Dictionary<string, PartialityMod> loadedModsDictionary;

        /// <summary>
        /// List of OptionInterface Instances
        /// </summary>
        public static List<OptionInterface> loadedInterfaces;

        /// <summary>
        /// Loaded OptionInterface Instances.
        /// Key: ModID, Value: OI Instance
        /// </summary>
        public static Dictionary<string, OptionInterface> loadedInterfaceDict;

        /// <summary>
        /// Rain World/ModConfigs.
        /// </summary>
        public static DirectoryInfo Directory => OptionMod.directory;

        /// <summary>
        /// Blacklisted mod from config menu.
        /// </summary>
        public static string[] blackList = new string[]
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

        /// <summary>
        /// Current SaveSlot.
        /// </summary>
        public static int Slot => rw.options.saveSlot;

        /// <summary>
        /// Currently Playing Slugcat.
        /// </summary>
        public static int Slugcat => rw.progression.PlayingAsSlugcat;

        /// <summary>
        /// Whether Config has changed in Config Menu or not
        /// </summary>
        public static bool configChanged;

        /// <summary>
        /// Current Language of the game, including ComMod ones
        /// </summary>
        public static string curLang;

        public static PartialityMod ComMod;
        public static bool ComModExists { get; private set; } = false;
        public static Dictionary<int, string> ID2Code;

        public static PartialityMod SlugBaseMod;
        public static bool SlugBaseExists { get; private set; } = false;

        private static bool ValidModIDCheck(string id) => !loadedModsDictionary.ContainsKey(id) && Regex.IsMatch(id, "^[^\\/?%*:|\"<>/.]+$");

        /// <summary>
        /// Runs right before MainMenu opens
        /// </summary>
        private static void Initialize()
        {
            //Application.RegisterLogCallback(new Application.LogCallback(TroubleShoot));

            ID2Code = new Dictionary<int, string>()
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
            if (ID2Code.TryGetValue(rw.options.language, out string code)) { curLang = code; }
            else { curLang = "eng"; }
            string curCurLang = curLang;
            InternalTranslator.LoadTranslation();

            #region PartialityMods
            loadedMods = Partiality.PartialityManager.Instance.modManager.loadedMods;
            loadedModsDictionary = new Dictionary<string, PartialityMod>();
            ComModExists = false;
            foreach (PartialityMod mod in loadedMods)
            {
                if (string.IsNullOrEmpty(mod.ModID)) { goto invaildID; }
                if (blackList.Contains<string>(mod.ModID)) //No Config for this :P
                {
                    if (mod.ModID == "CommunicationModule")
                    {
                        ComMod = mod; ComModExists = true;
                        if (rw.options.language != 5)
                        { curLang = (string)mod.GetType().GetField("customLang", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(mod); }
                        else { curLang = "eng"; }
                    }
                    continue;
                }
                if (mod.ModID.Substring(0, 1) == "_") { continue; } //Skip this mod from configuration
                if (mod.ModID == "SlugBase") { SlugBaseMod = mod; SlugBaseExists = true; }

                if (loadedModsDictionary.ContainsKey(mod.ModID))
                { Debug.LogError($"Duplicate ModID detected! (dupe ID: {mod.ModID})"); }
                else if (Regex.IsMatch(mod.ModID, "^[^\\/?%*:|\"<>/.]+$"))
                { loadedModsDictionary.Add(mod.ModID, mod); continue; }

            invaildID:
                string InvalidID = string.IsNullOrEmpty(mod.ModID) ? "No ModID" : mod.ModID;
                Debug.Log($"{InvalidID} does not support CompletelyOptional: Invalid Mod ID! Using Mod Type name instead.");
                mod.ModID = mod.GetType().Name;
                //Assembly.GetAssembly(this.mod.GetType());
                if (!ValidModIDCheck(mod.ModID))
                {
                    Debug.Log($"{mod.ModID} does not support CompletelyOptional either! Using Assembly Name instead.");
                    mod.ModID = Assembly.GetAssembly(mod.GetType()).GetName().Name;
                    if (!ValidModIDCheck(mod.ModID))
                    { Debug.Log($"{mod.ModID} does not support CompletelyOptional either! Abandoning {InvalidID}!"); continue; }
                }
                loadedModsDictionary.Add(mod.ModID, mod);
            }

            //Debug.Log(string.Concat("curLang: ", curLang));
            if (curCurLang != curLang) { InternalTranslator.LoadTranslation(); }

            isOptionMenu = false;
            loadedInterfaceDict = new Dictionary<string, OptionInterface>();
            loadedInterfaces = new List<OptionInterface>();

            //No Mods Installed!
            if (loadedModsDictionary.Count == 0 && !OptionMod.ReferencesBepInEx)
            {
                loadedModsDictionary = new Dictionary<string, PartialityMod>(1);
                PartialityMod blankMod = new PartialityMod
                {
                    ModID = "No Mods Installed",
                    Version = "XXXX",
                    author = RainWorldMod.authorNull
                };
                loadedModsDictionary.Add(blankMod.ModID, blankMod);

                UnconfiguableOI itf = new UnconfiguableOI(blankMod, UnconfiguableOI.Reason.NoMod);
                loadedInterfaceDict.Add(blankMod.ModID, itf);
                loadedInterfaces.Add(itf);

                return;
            }

            //Load Mod Interfaces!
            foreach (KeyValuePair<string, PartialityMod> item in loadedModsDictionary)
            {
                PartialityMod mod = loadedModsDictionary[item.Key];
                OptionInterface itf;

                try
                {
                    var method = mod.GetType().GetMethod("LoadOI");
                    if (method == null || method.GetParameters().Length > 0 || method.ContainsGenericParameters)
                    {
                        // Mod didn't attempt to interface with CompletelyOptional, don't bother logging it.
                        itf = new UnconfiguableOI(mod, UnconfiguableOI.Reason.NoInterface);
                    }
                    else if (method.Invoke(mod, null) is OptionInterface oi)
                    {
                        itf = oi;
                        //Your code
                        Debug.Log($"Loaded OptionInterface from {mod.ModID} (type: {itf.GetType()})");
                    }
                    else
                    {
                        itf = new UnconfiguableOI(mod, UnconfiguableOI.Reason.NoInterface);
                        Debug.Log($"{mod.ModID} did not return an OptionInterface in LoadOI.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"{mod.ModID} threw an exception in LoadOI: {ex.Message}");
                    itf = new UnconfiguableOI(mod, UnconfiguableOI.Reason.NoInterface);
                }

                if (itf.rwMod.mod == null)
                {
                    NullModException e = new NullModException(mod.ModID);
                    Debug.Log(e); Debug.LogException(e);
                    itf.rwMod = new RainWorldMod(mod);
                    // itf = new UnconfiguableOI(mod, e);
                }

                try
                {
                    itf.Initialize();
                }
                catch (Exception ex)
                { //try catch better
                    itf = new UnconfiguableOI(mod, new GeneralInitializeException(ex));
                    itf.Initialize();
                }

                if (itf.Tabs == null || itf.Tabs.Length < 1)
                {
                    itf = new UnconfiguableOI(itf.rwMod, new NoTabException(mod.ModID));
                    itf.Initialize();
                    //OptionScript.loadedInterfaceDict.Remove(mod.ModID);
                    //OptionScript.loadedInterfaceDict.Add(mod.ModID, itf);
                }
                else if (itf.Configuable())
                {
                    if (itf.LoadConfig())
                    {
                        Debug.Log($"CompletelyOptional) {itf.rwMod.ModID} config has been loaded.");
                    }
                    else
                    {
                        Debug.Log($"CompletelyOptional) {itf.rwMod.ModID} does not have config.json.");
                        //itf.Initialize();
                        try
                        {
                            itf.SaveConfig(itf.GrabConfig());
                            Debug.Log($"CompletelyOptional) {itf.rwMod.ModID} uses default config.");
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Saving Default Config Failed.");
                            Debug.Log(e);
                        }
                    }
                }

                loadedInterfaces.Add(itf);
                loadedInterfaceDict.Add(itf.rwMod.ModID, itf);
                //loadedModsDictionary[item.Key].GetType().GetMember("OI")
            }
            // Progression of loaded OIs
            ProgressData.SubPatch();
            if (OptionScript.rw.setup.loadProg) ProgressData.LoadOIsMisc();

            #endregion PartialityMods

            #region InternalTest
            if (OptionMod.testing)
            {
#pragma warning disable CS0162
                PartialityMod temp = new PartialityMod()
                {
                    author = "topicular",
                    ModID = "InternalTest",
                    Version = OptionMod.instance.Version
                };
                loadedModsDictionary.Add(temp.ModID, temp);
                OptionInterface itf = new InternalTestOI(temp);
                try { itf.Initialize(); }
                catch (Exception ex)
                {
                    itf = new UnconfiguableOI(temp, new GeneralInitializeException(ex));
                    itf.Initialize();
                }
                loadedInterfaces.Add(itf);
                loadedInterfaceDict.Add(temp.ModID, itf);
#pragma warning restore CS0162
            }
            #endregion InternalTest

            // dual's code
            if (OptionMod.ReferencesBepInEx)
            {
                LoadBaseUnityPlugins();
            }

            Debug.Log($"CompletelyOptional finished initializing {loadedInterfaceDict.Count} OIs.");
        }

        private static void LoadBaseUnityPlugins()
        {
            BaseUnityPlugin[] plugins = FindObjectsOfType<BaseUnityPlugin>();
            if (plugins.Length < 1) { return; }

            foreach (BaseUnityPlugin plugin in plugins)
            {
                OptionInterface itf;

                // Load ITF
                try
                {
                    var method = plugin.GetType().GetMethod("LoadOI");
                    if (method == null || method.GetParameters().Length > 0 || method.ContainsGenericParameters)
                    {
                        // Mod didn't attempt to interface with CompletelyOptional, don't bother logging it.
                        itf = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);
                    }
                    else if (method.Invoke(plugin, null) is OptionInterface oi)
                    {
                        itf = oi;
                        //Your code
                        Debug.Log($"Loaded OptionInterface from {itf.rwMod.ModID} (type: {itf.GetType()})");
                    }
                    else
                    {
                        itf = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);
                        Debug.Log($"{itf.rwMod.ModID} did not return an OptionInterface in LoadOI.");
                    }
                }
                catch (Exception ex)
                {
                    itf = new UnconfiguableOI(plugin, UnconfiguableOI.Reason.NoInterface);

                    if (blackList.Contains(itf.rwMod.ModID) || itf.rwMod.ModID.Substring(0, 1) == "_") 
                    { 
                        continue; 
                    }

                    Debug.Log($"{itf.rwMod.ModID} threw an exception in LoadOI: {ex.Message}");
                }

                if (itf is UnconfiguableOI && plugin.Config.Keys.Count > 0)
                {
                    // Use BepInEx Configuration
                    itf = new GeneratedOI(itf.rwMod, plugin.Config);
                }

                // Initialize the interface
                try
                {
                    itf.Initialize();
                }
                catch (Exception ex)
                { 
                    // Try-catch better
                    itf = new UnconfiguableOI(itf.rwMod, new GeneralInitializeException(ex));
                    itf.Initialize();
                }
                if (itf.Tabs == null || itf.Tabs.Length < 1)
                {
                    itf = new UnconfiguableOI(itf.rwMod, new NoTabException(itf.rwMod.ModID));
                    itf.Initialize();
                    // OptionScript.loadedInterfaceDict.Remove(mod.ModID);
                    // OptionScript.loadedInterfaceDict.Add(mod.ModID, itf);
                }
                else if (itf.Configuable())
                {
                    if (itf.LoadConfig())
                    {
                        Debug.Log($"{itf.rwMod.ModID} config has been loaded.");
                    }
                    else
                    {
                        Debug.Log($"{itf.rwMod.ModID} does not have config.txt.");
                        //itf.Initialize();
                        try
                        {
                            itf.SaveConfig(itf.GrabConfig());
                            Debug.Log($"{itf.rwMod.ModID} uses default config.");
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Saving Default Config Failed.");
                            Debug.Log(e);
                        }
                    }
                }

                loadedInterfaces.Add(itf);
                loadedInterfaceDict.Add(itf.rwMod.ModID, itf);
            }
        }

        /// <summary>
        /// List of Tabs in ConfigMenu
        /// </summary>
        public static Dictionary<string, OpTab> tabs;

        /// <summary>
        /// Unload All Tabs
        /// </summary>
        internal static void KillTabs()
        {
            foreach (KeyValuePair<string, OpTab> item in tabs)
            { tabs[item.Key].Unload(); }
        }

        /// <summary>
        /// Show Low-friendly TroubleShoot Messages
        /// </summary>
#pragma warning disable IDE0060

        public static void TroubleShoot(string logString, string stackTrace, LogType type)
#pragma warning restore IDE0060
        {
            if (type != LogType.Exception && type != LogType.Error) { return; }
            if (Regex.IsMatch(logString, "/FieldAccessException/"))
            {
            }
        }

        public static float curFramerate = 60.0f;
        private static float[] dtHistory = new float[16];
        private static int dtHistoryMark = 0;

        /// <summary>
        /// MonoBehavior Update
        /// </summary>
        public void Update()
        {
            if (!init) { return; }

            if (pm.currentMainLoop?.ID != ProcessManager.ProcessID.OptionsMenu)
            { goto BackgroundUpdate; }
            else if (!OptionsMenuPatch.mod)
            { return; }

            //Option is running
            ConfigMenu.script = this;
            isOptionMenu = true;

            #region curFramerate
            dtHistory[dtHistoryMark] = Time.deltaTime;
            dtHistoryMark--; if (dtHistoryMark < 0) { dtHistoryMark = dtHistory.Length - 1; }
            float sum = 0;
            for (int h = 0; h < dtHistory.Length; h++) { sum += dtHistory[h]; }
            curFramerate = dtHistory.Length / sum;
            #endregion curFramerate

            if (soundFill > 0) { soundFill--; }
            ConfigMenu.description = "";
            if (ConfigMenu.currentTab != null)
            {
                bool fade = ConfigMenu.instance.fadeSprite != null;
                ConfigMenu.menuTab.Update(Time.deltaTime);
                if (MenuTab.logMode)
                {
                    ConfigMenu.instance.saveButton.buttonBehav.greyedOut = true;
                    ConfigMenu.instance.resetButton.buttonBehav.greyedOut = true;
                    return;
                }
                ConfigMenu.instance.backButton.buttonBehav.greyedOut = fade;
                try
                {
                    if (!ConfigMenu.freezeMenu)
                    {
                        ConfigMenu.currentTab.Update(Time.deltaTime);
                        ConfigMenu.instance.saveButton.buttonBehav.greyedOut = fade || false;
                        for (int m = 0; m < ConfigMenu.instance.modButtons.Length; m++)
                        { ConfigMenu.instance.modButtons[m].buttonBehav.greyedOut = fade || false; }
                        ConfigMenu.tabCtrler.greyedOut = fade || false;
                    }
                    else
                    {
                        bool h = false;
                        foreach (UIelement element in ConfigMenu.currentTab.items)
                        {
                            if (element.GetType().IsSubclassOf(typeof(UIconfig)))
                            {
                                if ((element as UIconfig).held)
                                {
                                    h = true;
                                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                                    {
                                        if (Input.GetKey(KeyCode.V) && !string.IsNullOrEmpty(UniClipboard.GetText()))
                                        {
                                            string grab = UniClipboard.GetText();
                                            if ((element as UIconfig).CopyFromClipboard(grab))
                                            {
                                                (element as UIconfig).bumpBehav.flash = 1f;
                                                if ((element as UIconfig).cosmetic)
                                                { ConfigMenu.alert = InternalTranslator.Translate("Pasted <Text> from Clipboard").Replace("<Text>", grab); }
                                                else
                                                { ConfigMenu.alert = InternalTranslator.Translate("Pasted <Text> from Clipboard to <ObjectName>").Replace("<Text>", grab).Replace("<ObjectName>", (element as UIconfig).key); }
                                            }
                                        }
                                        else if (Input.GetKey(KeyCode.C))
                                        {
                                            string grab = (element as UIconfig).CopyToClipboard();
                                            if (!string.IsNullOrEmpty(grab))
                                            {
                                                (element as UIconfig).bumpBehav.flash = 1f;
                                                UniClipboard.SetText(grab);
                                                if ((element as UIconfig).cosmetic)
                                                { ConfigMenu.alert = InternalTranslator.Translate("Copied <Text> to Clipboard").Replace("<Text>", grab); }
                                                else
                                                { ConfigMenu.alert = InternalTranslator.Translate("Copied <Text> to Clipboard from <ObjectName>").Replace("<Text>", grab).Replace("<ObjectName>", (element as UIconfig).key); }
                                            }
                                        }
                                    }
                                    element.Update(Time.deltaTime);
                                    continue;
                                }
                            }
                            if (element.GetType().IsSubclassOf(typeof(UItrigger)))
                            { if ((element as UItrigger).held) { h = true; element.Update(Time.deltaTime); continue; } }
                        }
                        if (!h)
                        {
                            foreach (UIelement element in ConfigMenu.currentTab.items)
                            { element.Update(Time.deltaTime); }
                        }
                        ConfigMenu.instance.saveButton.buttonBehav.greyedOut = fade || h;
                        for (int m = 0; m < ConfigMenu.instance.modButtons.Length; m++)
                        { ConfigMenu.instance.modButtons[m].buttonBehav.greyedOut = fade || h; }
                        ConfigMenu.tabCtrler.greyedOut = h;
                    }
                    ConfigMenu.currentInterface.Update(Time.deltaTime);
                }
                catch (Exception ex)
                { //Update Error Handle!
                    RainWorldMod mod = ConfigMenu.currentInterface.rwMod;
                    List<Exception> unload = new List<Exception>();
                    ConfigMenu menu = (pm.currentMainLoop as ConfigMenu);
                    foreach (OpTab tab in ConfigMenu.currentInterface.Tabs)
                    {
                        try
                        {
                            tab.Hide();
                            tab.Unload();
                        }
                        catch (Exception ex0) { unload.Add(ex0); }
                    }
                    string fullLog = string.Concat(
                        mod.ModID, " had error in Update(Time.deltaTime)!", Environment.NewLine,
                        ex.ToString());
                    foreach (Exception ex0 in unload)
                    {
                        fullLog += Environment.NewLine + "TabUnloadError: " + ex0.ToString();
                    }

                    UnconfiguableOI newItf = new UnconfiguableOI(mod, new GenericUpdateException(fullLog));
                    loadedInterfaceDict.Remove(mod.ModID);
                    loadedInterfaceDict.Add(mod.ModID, newItf);

                    int index = 0;
                    foreach (KeyValuePair<int, string> item in menu.modList)
                    {
                        if (item.Value == mod.ModID) { index = item.Key; break; }
                    }
                    int i = 0;
                    do
                    {
                        string key = string.Concat(index.ToString("D3") + "_" + i.ToString("D2"));
                        if (tabs.ContainsKey(key)) { tabs.Remove(key); }
                        else
                        {
                            break;
                        }
                        i++;
                    } while (i < 100);

                    newItf.Initialize();
                    ConfigMenu.selectedTabIndex = 0;
                    tabs.Add(string.Concat(index.ToString("D3") + "_00"), newItf.Tabs[0]);

                    foreach (UIelement element in newItf.Tabs[0].items)
                    {
                        foreach (MenuObject obj in element.subObjects) { menu.pages[0].subObjects.Add(obj); }
                        menu.pages[0].Container.AddChild(element.myContainer);
                    }
                    newItf.Tabs[0].Show();

                    ConfigMenu.currentInterface = newItf;
                    ConfigMenu.currentTab = newItf.Tabs[0];
                    ConfigMenu.tabCtrler.Reset();

                    (pm.currentMainLoop as ConfigMenu).PlaySound(SoundID.MENU_Error_Ping);
                    (pm.currentMainLoop as ConfigMenu).opened = false;
                    (pm.currentMainLoop as ConfigMenu).OpenMenu();
                }
            }

            return;

        BackgroundUpdate:
            isOptionMenu = false;
            //Background running
            // if (pm.currentMainLoop?.ID == ProcessManager.ProcessID.IntroRoll) { return; }
            /*
            foreach (OptionInterface oi in loadedInterfaces)
            {
            }
            */
        }
    }
}
