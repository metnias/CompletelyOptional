using Partiality.Modloader;
using BepInEx;
using System.Reflection;
using System;

namespace OptionalUI
{
    public struct RainWorldMod
    {
        public RainWorldMod(PartialityMod mod)
        {
            this.type = Type.PartialityMod;
            this.mod = mod;
            this.ModID = mod.ModID;
            this.author = mod.author;
            this.Version = mod.Version;
        }

        public RainWorldMod(BaseUnityPlugin plugin)
        {
            this.type = Type.BepInExPlugin;
            this.mod = plugin;

            Assembly assm = Assembly.GetAssembly(plugin.GetType());
            this.ModID = assm.GetName().Name;
            this.author = "NULL";
            bool foundAthr = false;
            this.Version = "XXXX";
            bool foundVer = false;
            try
            {
                object[] attrs = assm.GetCustomAttributes(false);
                foreach (object attr in attrs)
                {
                    if (!foundVer && attr.GetType() == typeof(AssemblyVersionAttribute)) { this.Version = ((AssemblyVersionAttribute)attr).Version; foundVer = true; }
                    else if (!foundVer && attr.GetType() == typeof(AssemblyFileVersionAttribute)) { this.Version = ((AssemblyFileVersionAttribute)attr).Version; }
                    else if (attr.GetType() == typeof(AssemblyProductAttribute)) { this.ModID = ((AssemblyProductAttribute)attr).Product; }
                    else if (foundAthr) { continue; }
                    else if (attr.GetType() == typeof(AssemblyTrademarkAttribute)) { this.ModID = ((AssemblyTrademarkAttribute)attr).Trademark; foundAthr = true; }
                    else if (attr.GetType() == typeof(AssemblyCompanyAttribute)) { this.ModID = ((AssemblyCompanyAttribute)attr).Company; }
                }
            }
            catch (Exception) { }
        }

        public readonly Type type;
        public string ModID, author, Version;
        public readonly object mod;
        // public PartialityMod PartialityMod => mod as PartialityMod;

        public enum Type
        {
            PartialityMod,
            BepInExPlugin
        }
    }
}
