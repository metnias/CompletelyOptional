using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompletelyOptional
{
    /// <summary>
    /// MenuTab for MenuTabWrapper, which sends Signal to vanilla Singal
    /// </summary>
    public class WrappedMenuTab : MenuTab
    {
        internal WrappedMenuTab(MenuTabWrapper wrapper)
        {
            this.wrapper = wrapper;
        }

        public readonly MenuTabWrapper wrapper;

        protected internal override void Signal(UItrigger trigger, string signal)
        {
            wrapper.Singal(null, signal);
        }
    }
}