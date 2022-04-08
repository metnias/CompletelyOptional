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
            tab = new MenuTab();
        }

        public readonly MenuTab tab;
    }
}