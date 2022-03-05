using BepInEx;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    internal class InternalOI_Error : InternalOI
    {
        public InternalOI_Error(BaseUnityPlugin plugin, Exception exception) : base(plugin, exception)
        {
        }

        public InternalOI_Error(RainWorldMod rwMod, Exception exception) : base(rwMod, exception)
        {
        }
    }
}