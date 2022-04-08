using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OptionalUI;
using Menu;
using UnityEngine;

namespace CompletelyOptional
{
    public class UIelementWrapper : RectangularMenuObject
    {
        public UIelementWrapper(MenuTabWrapper tabWrapper, UIelement element) : base(tabWrapper.menu, tabWrapper.owner, element.GetPos(), element.size)
        {
            this.tabWrapper = tabWrapper;
            this.tabWrapper.tab.AddItems(element);
            if (this.tabWrapper.wrappers.ContainsKey(element)) { throw new Exception(); }
            this.tabWrapper.wrappers.Add(element, this);
            glow = new GlowGradient(this.tabWrapper.glowContainer, this.pos, this.size, 0.5f)
            { color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey) };
        }

        private readonly GlowGradient glow;

        public readonly MenuTabWrapper tabWrapper;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(this, message);
        }
    }
}