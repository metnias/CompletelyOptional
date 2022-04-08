using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// Contains methods to interact with ConfigMachine in <see cref="ModConfigMenu"/>. See also <seealso cref="MachineConnector"/>.
    /// </summary>
    public static class ConfigConnector
    {
        /// <summary>
        /// Whether <see cref="ProcessManager.currentMainLoop"/> is currently <see cref="ModConfigMenu"/> or not.
        /// </summary>
        public static bool InConfigMenu => ModConfigMenu.instance != null;

        /// <summary>
        /// To display text at the bottom of the screen. Call this in <see cref="OptionInterface.Update"/> every frame for the duration you want, not just once. See also <seealso cref="ShowAlert"/>
        /// </summary>
        public static void ShowDescription(string text)
        { if (!InConfigMenu) { return; } ModConfigMenu.instance.ShowDescription(text); }

        /// <summary>
        /// To display flashing text at the top of the screen. Calling this once is enough to display alert for a set amount of time. See also <see cref="ShowDescription"/>.
        /// </summary>
        public static void ShowAlert(string text)
        { if (!InConfigMenu) { return; } ModConfigMenu.instance.ShowAlert(text); }

        public static void RequestReloadMenu()
        {
        }

        public static void RequestResetConfig()
        {
        }

        public static void SaveConfig(bool silent = false)
        {
            if (!silent) { ShowAlert("Saved Config of <ModID>".Replace("<ModID>", ConfigContainer.activeInterface.rwMod.ModID)); } // temp msg
        }

        public static bool ChangeActiveTab(int index, bool silent = false)
        {
            if (!InConfigMenu || index < 0) { return false; }
            if (index >= ConfigContainer.activeInterface.Tabs.Length) { return false; }
            ConfigContainer.ChangeActiveTab(index);
            if (!silent)
            {
                string name = ConfigContainer.activeInterface.Tabs[index].name;
                if (string.IsNullOrEmpty(name)) { name = "No. <TabIndex>".Replace("<TabIndex>", index.ToString()); }
                ShowAlert("Changed Active Tab to <TabName>".Replace("<TabName>", name));
            }
            return true;
        }

        public static bool ChangeActiveMod(string modIDquery, bool silent = false)
        {
            if (!InConfigMenu) { return false; }
            string candidateID = "";
            foreach (string id in ConfigContainer.OptItfID)
            { // some kinda approximate system here
                if (id.ToLower().Contains(modIDquery.ToLower())) { candidateID = id; break; }
            }
            if (string.IsNullOrEmpty(candidateID)) { return false; }
            ConfigContainer.ChangeActiveMod(ConfigContainer.FindItfIndex(candidateID));
            if (!silent)
            {
                ShowAlert("Changed Active Mod to <ModID>".Replace("<ModID>", ConfigContainer.activeInterface.rwMod.ModID));
            }
            return true;
        }

        public static void MuteMenu(bool mute) => ConfigContainer.mute = mute;

        public static void FocusNewElement(UIelement element)
        { if (!InConfigMenu) { return; } ConfigContainer.instance.FocusNewElement(element); }

        public static void FocusNewElementInDirection(IntVector2 direction)
        { if (!InConfigMenu) { return; } ConfigContainer.instance.FocusNewElementInDirection(direction); }
    }
}