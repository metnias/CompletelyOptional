using Partiality.Modloader;
using BepInEx;
using System.Reflection;
using System;
using System.Linq;

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
            this.description = authorNull;
            this.Version = mod.Version;
            try
            {
                Assembly assm = Assembly.GetAssembly(mod.GetType());
                if (assm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() is AssemblyDescriptionAttribute descAttr && !string.IsNullOrEmpty(descAttr.Description))
                {
                    description = descAttr.Description;
                }
            }
            catch (Exception) { }
        }

        public RainWorldMod(BaseUnityPlugin plugin)
        {
            this.type = Type.BepInExPlugin;
            this.mod = plugin;

            this.ModID = plugin.Info.Metadata.Name;
            this.author = authorNull;
            this.description = authorNull;
            this.Version = plugin.Info.Metadata.Version.ToString();
            try
            {
                Assembly assm = Assembly.GetAssembly(plugin.GetType());
                if (assm.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false).FirstOrDefault() is AssemblyTrademarkAttribute trademarkAttr && !string.IsNullOrEmpty(trademarkAttr.Trademark))
                {
                    author = trademarkAttr.Trademark;
                }
                else if (assm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).FirstOrDefault() is AssemblyCompanyAttribute companyAttr && !string.IsNullOrEmpty(companyAttr.Company))
                {
                    author = companyAttr.Company;
                }
                if (assm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() is AssemblyDescriptionAttribute descAttr && !string.IsNullOrEmpty(descAttr.Description))
                {
                    description = descAttr.Description;
                }
            }
            catch (Exception) { }
        }

        public readonly Type type;
        public string ModID, author, Version, description;
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
