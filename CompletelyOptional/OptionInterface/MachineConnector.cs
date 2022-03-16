using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using BepInEx;

namespace CompletelyOptional
{
    /// <summary>
    /// Contains methods to interact with ConfigMachine at any time. See also <seealso cref="ConfigConnector"/>.
    /// </summary>
    public static class MachineConnector
    {
        internal static Dictionary<string, OptionInterface> registeredOIs = new Dictionary<string, OptionInterface>();

        /// <summary>
        /// Register <see cref="OptionInterface"/> to ConfigMachine to
        /// </summary>
        /// <param name="oi"></param>
        public static void RegisterOI(OptionInterface oi)
        {
            if (registeredOIs.ContainsKey(ConfigContainer.GenerateID(oi.rwMod)))
            {
                ComOptPlugin.LogMessage("This mod has already registered OptionInterface. Pre-registered one will be discarded then.");
                registeredOIs.Remove(ConfigContainer.GenerateID(oi.rwMod));
            }
            registeredOIs.Add(ConfigContainer.GenerateID(oi.rwMod), oi);
        }

        /// <summary>
        /// See also <seealso cref="ConfigConnector.RequestResetConfig"/> for when the game is <seealso cref="ConfigConnector.InConfigMenu"/>.
        /// </summary>
        /// <param name="oi"></param>
        public static bool ResetConfig(OptionInterface oi)
        {
            if (registeredOIs.TryGetValue(ConfigContainer.GenerateID(oi.rwMod), out oi))
            {
                // Something something reset
                // ComOptPlugin.LogMessage("This mod has already registered OptionInterface. Pre-registered one will be discarded then.");
                return true;
            }
            return false;
        }
    }
}