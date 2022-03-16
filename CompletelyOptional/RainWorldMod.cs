using BepInEx;
using System;
using System.Linq;
using System.Reflection;

namespace OptionalUI
{
    public struct RainWorldMod
    {
        /// <summary>
        /// For making dummy RainWorldMod
        /// </summary>
        internal RainWorldMod(string id)
        {
            this.type = Type.Dummy;
            this.mod = null;

            this.ModID = id;
            this.author = authorNull;
            this.description = authorNull;
            this.version = authorNull;
            this.license = authorNull;
        }

        public RainWorldMod(BaseUnityPlugin plugin)
        {
            this.type = Type.BepInExPlugin;
            this.mod = plugin;

            this.ModID = plugin.Info.Metadata.Name.Trim();
            this.author = authorNull;
            this.description = authorNull;
            this.version = plugin.Info.Metadata.Version.ToString().Trim();
            this.license = authorNull;
            try
            {
                Assembly assm = Assembly.GetAssembly(plugin.GetType());
                if (assm.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false).FirstOrDefault() is AssemblyTrademarkAttribute trademarkAttr && !string.IsNullOrEmpty(trademarkAttr.Trademark))
                {
                    author = trademarkAttr.Trademark.Trim();
                }
                else if (assm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).FirstOrDefault() is AssemblyCompanyAttribute companyAttr && !string.IsNullOrEmpty(companyAttr.Company))
                {
                    author = companyAttr.Company.Trim();
                }
                if (assm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).FirstOrDefault() is AssemblyDescriptionAttribute descAttr && !string.IsNullOrEmpty(descAttr.Description))
                {
                    description = descAttr.Description.Trim();
                }
                if (assm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).FirstOrDefault() is AssemblyCopyrightAttribute descCpyr && !string.IsNullOrEmpty(descCpyr.Copyright))
                {
                    license = descCpyr.Copyright.Trim();
                }
            }
            catch (Exception) { }
        }

        public readonly Type type;
        public readonly string ModID, author, version, description, license;
        public readonly object mod;

        public BaseUnityPlugin plugin => mod as BaseUnityPlugin;
        public const string authorNull = "_NULL_";

        public enum Type : int
        {
            Dummy = -1,
            BepInExPlugin
        }
    }
}