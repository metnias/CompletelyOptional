using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    internal class InternalOI_NoMod : InternalOI
    {
        public InternalOI_NoMod() : base(null, Reason.NoMod)
        {
        }
    }
}