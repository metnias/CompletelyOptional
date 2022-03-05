using BepInEx;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// This just disables button in Mod List.
    /// </summary>
    internal class InternalOI_Blank : InternalOI
    {
        public InternalOI_Blank(BaseUnityPlugin plugin) : base(plugin, Reason.NoInterface)
        {
        }

        public InternalOI_Blank(RainWorldMod rwMod) : base(rwMod, Reason.NoInterface)
        {
        }
    }
}