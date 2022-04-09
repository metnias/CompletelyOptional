using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using UnityEngine;
using OptionalUI;

// Todo: Add FocusCandidate for this too...
// ...or just use UIelementWrapper

namespace CompletelyOptional
{
    /// <summary>
    /// Allows using <see cref="UIelement"/>s in vanilla <see cref="Menu.Menu"/> with <see cref="UIelementWrapper"/>s.
    /// </summary>
    public class MenuTabWrapper : PositionedMenuObject
    {
        public MenuTabWrapper(Menu.Menu menu, MenuObject owner) : base(menu, owner, Vector2.zero)
        {
            myContainer = new FContainer();
            owner.Container.AddChild(myContainer);
            glowContainer = new FContainer();
            myContainer.AddChild(glowContainer);
            wrappers = new Dictionary<UIelement, UIelementWrapper>();

            tab = new WrappedMenuTab(this);
        }

        internal readonly FContainer glowContainer;

        internal readonly WrappedMenuTab tab;

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

        internal Dictionary<UIelement, UIelementWrapper> wrappers;

        /// <summary>
        /// As <see cref="UIfocusable"/> is not <see cref="MenuObject"/>, this <see cref="MenuTabWrapper"/> will be sent instead.
        /// So use message to differenciate signals.
        /// </summary>
        internal void Signal(UIfocusable trigger, string signal)
        {
            wrappers[trigger].Singal(null, signal);
        }
    }
}