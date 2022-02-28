using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using OptionalUI;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Contains all UIelement, connects UIelements and Rain World Menu
    /// </summary>
    public class MenuContainer : Menu.MenuObject
    {
        public MenuContainer(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            menuTab = new MenuTab();
        }

        internal MenuTab menuTab;
        internal OpTab activeTab;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            menuTab.GrafUpdate(timeStacker);
            if (activeTab != null) { activeTab.GrafUpdate(timeStacker); }
        }

        public override void Update()
        {
            base.Update();
            menuTab.Update();
            if (activeTab != null) { activeTab.Update(); }
        }

        private UIelement focusedElement;

        internal void PlaySound(SoundID soundID, int soundFill) => menu.PlaySound(soundID);

        internal void PlaySound(SoundID soundID, int soundFill, float pan, float vol, float pitch) => menu.PlaySound(soundID, pan, vol, pitch);

        private void FocusNewElement(IntVector2 direction)
        {
            UIelement element = this.FocusCandidate(direction);
            if (element != null && element != this.focusedElement)
            {
                this.focusedElement = element;
                this.PlaySound((!(this.focusedElement is UIconfig) || (this.focusedElement as UIconfig).greyedOut) //WIP
                    ? SoundID.MENU_Greyed_Out_Button_Select_Gamepad_Or_Keyboard : SoundID.MENU_Button_Select_Gamepad_Or_Keyboard, 10);
            }
        }

        private UIelement FocusCandidate(IntVector2 direction)
        {
            if (this.focusedElement == null)
            { // current mod button
                return this.activeTab.focusables[0];
            }
            if (!(this.focusedElement is FocusableUIelement))
            {
                return this.pages[this.currentPage].lastSelectedObject; //lastSelectedObject
            }
            MenuObject result = this.pages[this.currentPage].lastSelectedObject;
            Vector2 vector = this.CenterPositionOfObject(this.selectedObject as PositionedMenuObject);
            float num = float.MaxValue;
            for (int i = 0; i < this.pages[this.currentPage].selectables.Count; i++)
            {
                if (this.pages[this.currentPage].selectables[i] is PositionedMenuObject && this.pages[this.currentPage].selectables[i].CurrentlySelectableNonMouse && this.pages[this.currentPage].selectables[i] != this.selectedObject)
                {
                    Vector2 vector2 = this.CenterPositionOfElement(this.pages[this.currentPage].selectables[i] as PositionedMenuObject);
                    if (direction.y == 0 || vector2.y < vector.y != direction.y < 0)
                    {
                        bool flag = direction.x != 0 && vector2.x < vector.x == direction.x < 0;
                    }
                    float num2 = Vector2.Distance(vector, vector2);
                    Vector2 vector3 = Custom.DirVec(vector, vector2);
                    float num3 = 0.5f - 0.5f * Vector2.Dot(vector3, direction.ToVector2().normalized);
                    if (num3 > 0.5f)
                    {
                        if (num3 > 0.8f)
                        {
                            num3 = 0.5f - 0.5f * Vector2.Dot(-vector3, direction.ToVector2().normalized);
                            num2 = Vector2.Distance(vector, vector2 + direction.ToVector2() * ((direction.x == 0) ? 1800f : 2400f));
                        }
                        else
                        {
                            num2 += 10000f;
                        }
                    }
                    num3 *= 50f;
                    float num4 = (1f + num2) * (1f + num3);
                    if (num4 < num)
                    {
                        result = (this.pages[this.currentPage].selectables[i] as MenuObject);
                        num = num4;
                    }
                }
            }
            return result;
        }

        private Vector2 CenterPositionOfElement(UIelement obj)
        {
            if (obj.isRectangular) { return obj.ScreenPos + obj.size / 2f; }
            return obj.ScreenPos + (obj.rad / 2f * Vector2.one);
        }

        // Mouse mode / Controller or Keyboard mode
    }
}