using BepInEx;
using OptionalUI;

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

        internal InternalOI_Blank(RainWorldMod rwMod) : base(rwMod, Reason.NoInterface)
        {
        }
    }
}