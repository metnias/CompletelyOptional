using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using UnityEngine;
using OptionalUI;

namespace CompletelyOptional
{
    /// <summary>
    /// Allows using <see cref="UIelement"/>s in vanilla <see cref="Menu.Menu"/>
    /// </summary>
    public class MenuTabWrapper : RectangularMenuObject
    {
        public MenuTabWrapper(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, pos, size)
        {
            tab = new WrappedMenuTab(this);
        }

        public readonly WrappedMenuTab tab;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            tab.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            tab.Update();
        }

        /// <summary>
        /// As <see cref="UItrigger"/> is not <see cref="MenuObject"/>, this <see cref="MenuTabWrapper"/> will be sent instead.
        /// So use message to differenciate signals.
        /// </summary>
        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(this, message);
        }
    }
}