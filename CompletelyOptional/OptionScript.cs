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
        public static List<PartialityMod> loadedMods;

        /// <summary>
        /// Loaded Mod Dictionary.
        /// Key: ModID, Value: PartialityMod Instance
        /// </summary>
        public static Dictionary<string, PartialityMod> loadedModsDictionary;

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
            "CommunicationModule"
        };

        /// <summary>
        /// Current SaveSlot.
        /// </summary>
        public static int Slot => pm.rainWorld.options.saveSlot;

        /// <summary>
        /// Currently Playing Slugcat.
        /// </summary>
        public static int Slugcat => pm.rainWorld.progression.PlayingAsSlugcat;

        /// <summary>
        /// Whether Config has changed in Config Menu or not
        /// </summary>
        public static bool configChanged;

        /// <summary>
        /// Current Language of the game, including ComMod ones
        /// </summary>
        public static string curLang;

        public static PartialityMod ComMod;
        public static bool ComModExists;
        public static Dictionary<int, string> ID2Code;

        private static bool ValidModIDCheck(string id) => !loadedModsDictionary.ContainsKey(id) && Regex.IsMatch(id, "^[^\\/?%*:|\"<>/.]+$");

        /// <summary>
        /// Runs right before MainMenu opens
        /// </summary>
        public static void Initialize()
        {
            //Application.RegisterLogCallback(new Application.LogCallback(TroubleShoot));

            loadedMods = Partiality.PartialityManager.Instance.modManager.loadedMods;
            loadedModsDictionary = new Dictionary<string, PartialityMod>();
            ComModExists = false;
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
            foreach (PartialityMod mod in loadedMods)
            {
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

                if (loadedModsDictionary.ContainsKey(mod.ModID))
                { Debug.LogError($"Duplicate ModID detected! (dupe ID: {mod.ModID})"); }
                else if (Regex.IsMatch(mod.ModID, "^[^\\/?%*:|\"<>/.]+$"))
                { loadedModsDictionary.Add(mod.ModID, mod); continue; }
                else { Debug.Log($"{mod.ModID} does not support CompletelyOptional: Invalid Mod ID! Using Mod Type name instead."); }

                string InvalidID = mod.ModID;
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
            InternalTranslator.LoadTranslation();

            loadedInterfaceDict = new Dictionary<string, OptionInterface>();
            loadedInterfaces = new List<OptionInterface>();

            //No Mods Installed!
            if (loadedModsDictionary.Count == 0)
            {
                loadedModsDictionary = new Dictionary<string, PartialityMod>(1);
                PartialityMod blankMod = new PartialityMod
                {
                    ModID = "No Mods Installed",
                    Version = "XXXX",
                    author = "NULL"
                };
                loadedModsDictionary.Add(blankMod.ModID, blankMod);

                UnconfiguableOI itf = new UnconfiguableOI(blankMod, UnconfiguableOI.Reason.NoMod);
                loadedInterfaceDict.Add(blankMod.ModID, itf);
                loadedInterfaces.Add(itf);

                return;
            }

            //Load Mod Interfaces!
            isOptionMenu = false;
            foreach (KeyValuePair<string, PartialityMod> item in loadedModsDictionary)
            {
                PartialityMod mod = loadedModsDictionary[item.Key];
                OptionInterface itf;

                try
                {
                    object obj = mod.GetType().GetMethod("LoadOI").Invoke(mod, new object[] { });
                    //Debug.Log(obj);
                    //itf = (OptionInterface)obj;
                    //itf = obj as OptionInterface;

                    if (obj.GetType().IsSubclassOf(typeof(OptionInterface)))
                    {
                        itf = obj as OptionInterface;
                        //Your code
                        Debug.Log($"Loaded OptionInterface from {mod.ModID} (type: {itf.GetType()})");
                    }
                    else
                    {
                        Debug.Log($"{mod.ModID} does not support CompletelyOptional: LoadOI returns what is not OptionInterface.");
                        itf = new UnconfiguableOI(mod, UnconfiguableOI.Reason.NoInterface);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"{mod.ModID} does not support CompletelyOptional: {ex.Message}");
                    itf = new UnconfiguableOI(mod, UnconfiguableOI.Reason.NoInterface);
                }

                if (itf.mod == null)
                { itf.mod = mod; Debug.Log(new NullModException(mod.ModID)); Debug.LogException(new NullModException(mod.ModID)); }
                else { itf.mod.ModID = mod.ModID; }

                try
                {
                    itf.Initialize();
                }
                catch (Exception ex)
                { //try catch better
                    itf = new UnconfiguableOI(itf.mod, new GeneralInitializeException(ex));
                    itf.Initialize();
                }

                if (itf.Tabs == null || itf.Tabs.Length < 1)
                {
                    itf = new UnconfiguableOI(itf.mod, new NoTabException(mod.ModID));
                    itf.Initialize();
                    //OptionScript.loadedInterfaceDict.Remove(mod.ModID);
                    //OptionScript.loadedInterfaceDict.Add(mod.ModID, itf);
                }
                else if (itf.Configuable())
                {
                    if (itf.LoadConfig())
                    {
                        Debug.Log($"CompletelyOptional: {mod.ModID} config has been loaded.");
                    }
                    else
                    {
                        Debug.Log($"CompletelyOptional: {mod.ModID} does not have config.txt.");
                        //itf.Initialize();
                        try
                        {
                            itf.SaveConfig(itf.GrabConfig());
                            Debug.Log($"CompletelyOptional: {mod.ModID} uses default config.");
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Saving Default Config Failed.");
                            Debug.Log(e);
                        }
                    }
                }

                loadedInterfaces.Add(itf);
                loadedInterfaceDict.Add(mod.ModID, itf);
                //loadedModsDictionary[item.Key].GetType().GetMember("OI")
            }
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
                    itf = new UnconfiguableOI(itf.mod, new GeneralInitializeException(ex));
                    itf.Initialize();
                }
                loadedInterfaces.Add(itf);
                loadedInterfaceDict.Add(temp.ModID, itf);
#pragma warning restore CS0162
            }
            Debug.Log($"CompletelyOptional: Finished Initializing {loadedInterfaceDict.Count} OIs");
        }

        /// <summary>
        /// List of Tabs in ConfigMenu
        /// </summary>
        public static Dictionary<string, OpTab> tabs;

        /// <summary>
        /// Unload All Tabs
        /// </summary>
        public static void KillTabs()
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

        /// <summary>
        /// MonoBehavior Update
        /// </summary>
        public void Update()
        {
            if (!init)
            {
                rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
                if (rw == null) { return; }
                pm = rw.processManager;
                if (pm.upcomingProcess == ProcessManager.ProcessID.MainMenu)
                {
                    try
                    {
                        Initialize();
                    }
                    catch (Exception ex) { Debug.LogError(ex); }
                    init = true;
                }
                ConfigMenu.currentTab = null;
                return;
            }
            if (pm == null)
            {
                rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
                if (rw == null) { return; }
                pm = rw.processManager;
                return;
            }

            if (pm.currentMainLoop?.ID != ProcessManager.ProcessID.OptionsMenu)
            { goto BackgroundUpdate; }
            else if (!OptionsMenuPatch.mod)
            { return; }

            //Option is running
            ConfigMenu.script = this;
            isOptionMenu = true;

            if (soundFill > 0) { soundFill--; }
            ConfigMenu.description = "";
            ConfigMenu.menuTab.Update(Time.deltaTime);
            if (MenuTab.logMode)
            {
                ConfigMenu.instance.saveButton.buttonBehav.greyedOut = true;
                ConfigMenu.instance.resetButton.buttonBehav.greyedOut = true;
                return;
            }
            bool fade = ConfigMenu.instance.fadeSprite != null;
            ConfigMenu.instance.backButton.buttonBehav.greyedOut = fade;
            if (ConfigMenu.currentTab != null)
            {
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
                            { if ((element as UIconfig).held) { h = true; element.Update(Time.deltaTime); continue; } }
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
                    PartialityMod mod = ConfigMenu.currentInterface.mod;
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

                    (pm.currentMainLoop as ConfigMenu).PlaySound(SoundID.MENU_Error_Ping);
                    (pm.currentMainLoop as ConfigMenu).opened = false;
                    (pm.currentMainLoop as ConfigMenu).OpenMenu();
                }
            }

            return;

        BackgroundUpdate:
            //Background running
            if (pm.currentMainLoop?.ID == ProcessManager.ProcessID.IntroRoll) { return; }
            /*
            foreach (OptionInterface oi in loadedInterfaces)
            {
            }
            */
        }
    }
}
