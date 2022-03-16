using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using BepInEx;

namespace CompletelyOptional
{
    public static class MachineConnector
    {
        internal static Dictionary<string, OptionInterface> registeredOIs = new Dictionary<string, OptionInterface>();

        /// <summary>
        /// Register
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
    }
}