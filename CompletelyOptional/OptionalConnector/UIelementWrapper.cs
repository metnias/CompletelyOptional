using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using UnityEngine;
using OptionalUI;

namespace CompletelyOptional
{
    public class UIelementWrapper : PositionedMenuObject
    {
        public UIelementWrapper(Menu.Menu menu, MenuObject owner, Vector2 pos, UIelement element) : base(menu, owner, pos)
        {
        }
    }
}