﻿using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    internal class InternalOI_Stats : InternalOI
    {
        public InternalOI_Stats() :
            base(new RainWorldMod("_Statistics_"), Reason.Statistics)
        {
        }

        // A function that turns off CM music
        // Save starred mods and remember last chosen mod

        public override void Initialize()
        {
            base.Initialize();

            // MOD CONFIG

            // turn off CM music
            // Reset starred mods
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
        }
    }
}