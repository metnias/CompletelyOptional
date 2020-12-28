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
            this.author = string.IsNullOrEmpty(mod.author) ? authorNull : mod.author;
            this.Version = mod.Version;
        }

        public RainWorldMod(BaseUnityPlugin plugin)
        {
            this.type = Type.BepInExPlugin;
            this.mod = plugin;

            Assembly assm = Assembly.GetAssembly(plugin.GetType());
            this.ModID = assm.GetName().Name;
            this.author = authorNull;
            bool foundAthr = false;
            this.Version = "XXXX";
            bool foundVer = false;
            try
            {
                object[] attrs = assm.GetCustomAttributes(false);
                foreach (object attr in attrs)
                {
                    if (!foundVer)
                    {
                        if (attr.GetType() == typeof(AssemblyVersionAttribute))
                        { 
                            if (!string.IsNullOrEmpty(((AssemblyVersionAttribute)attr).Version)) 
                            { 
                                this.Version = ((AssemblyVersionAttribute)attr).Version; 
                            } 
                            foundVer = true; 
                        }
                        else if (attr.GetType() == typeof(AssemblyFileVersionAttribute))
                        { 
                            if (!string.IsNullOrEmpty(((AssemblyFileVersionAttribute)attr).Version)) 
                            { 
                                this.Version = ((AssemblyFileVersionAttribute)attr).Version; 
                            } 
                        }
                    }
                    if (attr.GetType() == typeof(AssemblyProductAttribute))
                    {
                        if (!string.IsNullOrEmpty(((AssemblyProductAttribute)attr).Product))
                        {
                            this.ModID = ((AssemblyProductAttribute)attr).Product;
                        }
                    }
                    else if (foundAthr)
                    {
                        continue;
                    }
                    else if (attr.GetType() == typeof(AssemblyTrademarkAttribute))
                    {
                        if (!string.IsNullOrEmpty(((AssemblyTrademarkAttribute)attr).Trademark))
                        {
                            this.author = ((AssemblyTrademarkAttribute)attr).Trademark;
                        }
                        foundAthr = true;
                    }
                    else if (attr.GetType() == typeof(AssemblyCompanyAttribute))
                    {
                        if (!string.IsNullOrEmpty(((AssemblyCompanyAttribute)attr).Company))
                        {
                            this.author = ((AssemblyCompanyAttribute)attr).Company;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        public readonly Type type;
        public string ModID, author, Version;
        public readonly object mod;
        // public PartialityMod PartialityMod => mod as PartialityMod;
        public const string authorNull = "NULL";

        public enum Type
        {
            PartialityMod,
            BepInExPlugin
        }
    }
}
