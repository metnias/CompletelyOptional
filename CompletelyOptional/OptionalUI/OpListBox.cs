using CompletelyOptional;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    public class OpListBox : OpComboBox
    {
        /// <summary>
        /// ListBox that also can be searched
        /// </summary>
        /// <param name="pos">LeftBottom Position of this <see cref="UIelement"/></param>
        /// <param name="width">The width of this element. The height is calculated by lineCount.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="list">Will be sorted automatically by <see cref="ListItem.value"/>, then <see cref="ListItem.name"/></param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <param name="lineCount"><see cref="OpComboBox.listHeight"/> for its height. Use <see cref="GetLineCountFromHeight(float)"/> to calculate this from pixel height.</param>
        /// <param name="downward">whether this goes downward or not</param>
        /// <exception cref="ElementFormatException">Thrown when list has no <see cref="ListItem"/>.</exception>
        public OpListBox(Vector2 pos, float width, string key, List<ListItem> list, int lineCount = 5, bool downward = true, string defaultName = "") : base(pos, width, key, list, defaultName)
        {
            this.description = InternalTranslator.Translate("Scroll the list, Double Click to search");
            this.listHeight = lineCount;
            this.downward = downward;
            if (_init && !(this is OpResourceList)) { this.OpenList(); }
        }

        /// <summary>
        /// ListBox that also can be searched
        /// </summary>
        /// <param name="pos">LeftBottom Position of this <see cref="UIelement"/></param>
        /// <param name="width">The width of this element. The height is calculated by lineCount.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="array">The index will be <see cref="ListItem.value"/>, so the order will be kept</param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <param name="lineCount"><see cref="OpComboBox.listHeight"/> for its height. Use <see cref="GetLineCountFromHeight(float)"/> to calculate this from pixel height.</param>
        /// <param name="downward">whether this goes downward or not</param>
        /// <exception cref="ElementFormatException">Thrown when list has no <see cref="ListItem"/>.</exception>
        public OpListBox(Vector2 pos, float width, string key, string[] array, int lineCount = 5, bool downward = true, string defaultName = "") : base(pos, width, key, array, defaultName)
        {
            this.description = InternalTranslator.Translate("Scroll the list, Double Click to search");
            this.listHeight = lineCount;
            this.downward = downward;
            if (_init && !(this is OpResourceList)) { this.OpenList(); }
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (disabled) { return; }

            this.bumpList.MouseOver = this.MouseOverList();
            bool xOver = this.MousePos.x >= this.rectScroll.pos.x - this.pos.x && this.MousePos.x <= this.rectScroll.pos.x + this.rectScroll.size.x - this.pos.x;
            this.bumpScroll.MouseOver = xOver && this.MousePos.y >= this.rectScroll.pos.y - this.pos.y && this.MousePos.y <= this.rectScroll.pos.y + this.rectScroll.size.y - this.pos.y;
            xOver = this.MousePos.x >= 0f && this.MousePos.x <= this.size.x;
            this.bumpBehav.MouseOver = !downward ? base.MouseOver && !this.MouseOverList() : xOver && this.MousePos.y >= this.rectList.size.y && this.MousePos.y <= this.rectList.size.y + this.size.y;

            if (this.searchMode && !this.bumpScroll.held)
            {
                // Input
                foreach (char c in Input.inputString)
                {
                    if (c == '\b')
                    {
                        if (this.searchQuery.Length > 0)
                        {
                            this.searchQuery = (this.searchQuery.Substring(0, this.searchQuery.Length - 1));
                            this.searchIdle = -1;
                            if (!_soundFilled) { _soundFill += 12; menu.PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked); }
                        }
                        break;
                    }
                    else if ((c == '\n') || (c == '\r')) // enter/return
                    { continue; }
                    else
                    {
                        this.bumpBehav.flash = 2.5f;
                        this.searchQuery += c;
                        this.searchIdle = -1;
                        if (!_soundFilled) { _soundFill += 12; menu.PlaySound(SoundID.MENU_Checkbox_Uncheck); }
                    }
                }
                if (this.searchIdle < this.searchDelay)
                {
                    this.searchIdle++;
                    if (this.searchIdle == this.searchDelay) { RefreshSearchList(); }
                }
            }
            int listSize = this.searchMode ? this.searchList.Count : this.itemList.Length;
            if (this.bumpScroll.held)
            { // Holding scrollbar
                if (Input.GetMouseButton(0))
                { // Hold Scroll and update listtop
                    int num = Mathf.RoundToInt((this.MousePos.y - this.rectList.pos.y + this.pos.y - 10f - scrollHeldPos) * (listSize - this.lblList.Length) / (this.rectList.size.y - 20f - this.rectScroll.size.y));
                    num = Custom.IntClamp(listSize - this.lblList.Length - num, 0, listSize - this.lblList.Length);
                    // Debug.Log($"{listTop} > {num}; {this.itemList.Length}, {this.lblList.Length}, {this.itemList.Length - this.lblList.Length}");
                    if (listTop != num)
                    {
                        if (!_soundFilled) { _soundFill += 6; this.menu.PlaySound(SoundID.MENU_Scroll_Tick); }
                        this.listTop = num;
                        this.bumpScroll.flash = Mathf.Min(1f, this.bumpScroll.flash + 0.2f);
                        this.bumpScroll.sizeBump = Mathf.Min(2.5f, this.bumpScroll.sizeBump + 0.3f);
                    }
                }
                else
                { // Release Scrollbar
                    this.bumpScroll.held = false; this.mouseDown = false;
                    this.menu.PlaySound(SoundID.MENU_Scroll_Tick);
                }
            }
            else if ((!downward && this.MouseOver) ||
                (xOver && this.MousePos.y >= 0f && this.MousePos.y <= this.rectList.size.y + this.size.y))
            {
                if (Input.GetMouseButton(0))
                { // Click behaviour
                    if (!mouseDown)
                    {
                        if (this.bumpScroll.MouseOver && listSize > this.lblList.Length)
                        {
                            scrollHeldPos = this.MousePos.y - this.rectScroll.pos.y + this.pos.y;
                            this.bumpScroll.held = true; this.menu.PlaySound(SoundID.MENU_First_Scroll_Tick);
                        }
                        else { mouseDown = true; }
                    }
                }
                if (!this.MouseOverList()) // MouseOver Mainbox
                {
                    if (!Input.GetMouseButton(0) && mouseDown)
                    {
                        mouseDown = false;
                        if (dTimer > 0) { dTimer = 0; this.searchMode = true; EnterSearchMode(); return; }
                        else { dTimer = FrameMultiply(15); if (allowEmpty) { this.value = ""; } this.menu.PlaySound(SoundID.MENU_Checkbox_Uncheck); }
                    }
                }
                else // MouseOver List
                {
                    if (this.MousePos.x >= 10f && this.MousePos.x <= this.rectList.size.x - 30f)
                    {
                        if (this.downward) { listHover = Mathf.FloorToInt((this.MousePos.y + 10f) / 20f); }
                        else { listHover = Mathf.FloorToInt((this.MousePos.y - this.size.y + 10f) / 20f); }
                        if (listHover > this.lblList.Length || listHover <= 0) { listHover = -1; }
                        else { listHover = this.lblList.Length - listHover; }
                    }
                    else { listHover = -1; }

                    if (this.menu.mouseScrollWheelMovement != 0)
                    {
                        int num = listTop + (int)Mathf.Sign(this.menu.mouseScrollWheelMovement) * Mathf.CeilToInt(this.lblList.Length / 2f);
                        num = Custom.IntClamp(num, 0, listSize - this.lblList.Length);
                        // Debug.Log($"{listTop} > {num}; {this.itemList.Length}, {this.lblList.Length}, {this.itemList.Length - this.lblList.Length}");
                        if (listTop != num)
                        {
                            if (!_soundFilled) { _soundFill += 6; this.menu.PlaySound(SoundID.MENU_Scroll_Tick); }
                            this.listTop = num;
                            this.bumpScroll.flash = Mathf.Min(1f, this.bumpScroll.flash + 0.2f);
                            this.bumpScroll.sizeBump = Mathf.Min(2.5f, this.bumpScroll.sizeBump + 0.3f);
                        }
                    }
                    else if (!Input.GetMouseButton(0) && mouseDown)
                    {
                        mouseDown = false;
                        // Click list to choose
                        if (listHover >= 0)
                        {
                            string newVal = this.value;
                            if (this.searchMode)
                            {
                                if (listTop + listHover < this.searchList.Count)
                                { newVal = this.searchList[listTop + listHover].name; }
                                // this.searchMode = false;
                            }
                            else if (listTop + listHover < this.itemList.Length)
                            { // Select one from here
                                newVal = this.itemList[listTop + listHover].name;
                            }
                            if (newVal != this.value) { this.value = newVal; this.menu.PlaySound(SoundID.MENU_Checkbox_Check); goto exit; }
                        }
                    }
                    if (listHover >= 0)
                    {
                        string d = "";
                        if (this.searchMode)
                        {
                            if (listTop + listHover < this.searchList.Count)
                            { d = this.searchList[listTop + listHover].desc; }
                        }
                        else if (listTop + listHover < this.itemList.Length) { d = this.itemList[listTop + listHover].desc; }
                        if (!string.IsNullOrEmpty(d)) { ConfigMenu.description = d; }
                    }
                }
            }
            else
            { // Mouse out
                if (Input.GetMouseButton(0) && !mouseDown || mouseDown && !Input.GetMouseButton(0)) { goto exit; }
            }
            this.held = this.bumpScroll.held || this.searchMode;
            this.bumpList.Update(dt);
            this.bumpScroll.Update(dt);
            return;
        exit:
            this.held = false;
            this.searchMode = false;
            this.searchCursor.isVisible = false;
            this.bumpScroll.held = false;
        }

        /// <summary>
        /// Calculate lineCount from total pixel height. The resulting height of <see cref="OpListBox"/> will always be equal or smaller than original height.
        /// </summary>
        /// <param name="height">pixel height, which is usually <see cref="UIelement.size"/>.y</param>
        public static int GetLineCountFromHeight(float height) => Math.Max(1, Mathf.FloorToInt((height - 34f) / 20f));

        public override void Show()
        {
            base.Show();

            this.rectList.Show(); this.rectScroll.Show();
            for (int i = 0; i < this.lblList.Length; i++)
            { this.lblList[i].label.isVisible = true; }
        }
    }
}
