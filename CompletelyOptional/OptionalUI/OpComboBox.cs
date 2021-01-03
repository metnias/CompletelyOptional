using Menu;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CompletelyOptional;
using RWCustom;

namespace OptionalUI
{
    /// <summary>
    /// DropDown box that can also be searched, useful for selecting an item from a list
    /// </summary>
    public class OpComboBox : UIconfig
    {
        /// <summary>
        /// DropDown box that can be also be searched
        /// </summary>
        /// <param name="pos">LeftBottom Position of folded <see cref="OpComboBox"/></param>
        /// <param name="width">The box width of folded <see cref="OpComboBox"/>. The height is fixed to 24f.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="list">Will be sorted automatically by <see cref="ListItem.value"/>, then <see cref="ListItem.name"/></param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <exception cref="ElementFormatException">Thrown when list has no <see cref="ListItem"/>.</exception>
        public OpComboBox(Vector2 pos, float width, string key, List<ListItem> list, string defaultName = "") : base(pos, new Vector2(width, 24f), key, defaultName)
        {
            this._size = new Vector2(Mathf.Max(24f, size.x), 24f);
            this.description = InternalTranslator.Translate("Click to open the list, Double Click to search");
            if (IsResourceSelector) { return; }
            if (list is null || list.Count < 1) { throw new ElementFormatException(this, "The list must contain at least one ListItem", this.key); }
            list.Sort(ListItem.Comparer);
            this.itemList = list.ToArray();
            this.ResetIndex();
            this.Initialize(defaultName);
            // throw new NotImplementedException("OpComboBox will come to you, Soon(tm)! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        /// <summary>
        /// DropDown box that can be also be searched.
        /// </summary>
        /// <param name="pos">LeftBottom Position of folded <see cref="OpComboBox"/></param>
        /// <param name="width">The box width of folded <see cref="OpComboBox"/>. The height is fixed to 24f.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="array">The index will be <see cref="ListItem.value"/>, so the order will be kept</param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <exception cref="ElementFormatException">Thrown when array has no item.</exception>
        public OpComboBox(Vector2 pos, float width, string key, string[] array, string defaultName = "") : this(pos, width, key, ArrayToList(array), defaultName)
        { }

        private static List<ListItem> ArrayToList(string[] array)
        {
            List<ListItem> result = new List<ListItem>();
            for (int i = 0; i < array.Length; i++)
            { result.Add(new ListItem(array[i], i)); }
            return result;
        }

        internal void ResetIndex()
        {
            for (int i = 0; i < this.itemList.Length; i++)
            { this.itemList[i].index = i; }
            this.searchDelay = Mathf.FloorToInt(Custom.LerpMap(Mathf.Clamp(itemList.Length, 10, 90), 10, 90, 1, 20));
            this.searchDelay = FrameMultiply(this.searchDelay);
        }

        internal virtual void Initialize(string defaultName)
        {
            if (string.IsNullOrEmpty(defaultName)) { this.defaultValue = ""; this.ForceValue(""); }
            else
            {
                bool flag = false;
                for (int i = 0; i < this.itemList.Length; i++)
                { if (this.itemList[i].name == defaultName) { flag = true; break; } }
                if (flag) { this.defaultValue = defaultName; this.ForceValue(this.defaultValue); }
                else { this.defaultValue = ""; this.ForceValue(""); }
            }
            if (!_init) { return; }

            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

            this.rect = new DyeableRect(this.menu, this.owner, this.pos, this.size);
            this.lblText = new MenuLabel(this.menu, this.owner, this.value, new Vector2(this.pos.x - this.size.x / 2f + 12f, this.pos.y), this.size - new Vector2(12f, 0f), false);
            this.lblText.label.alignment = FLabelAlignment.Left;
            this.rectList = new DyeableRect(this.menu, this.owner, this.pos, this.size);
            this.rectScroll = new DyeableRect(this.menu, this.owner, this.pos, this.size);
            this.lblList = new MenuLabel[0];
            this.subObjects.Add(this.rect);
            this.subObjects.Add(this.lblText);
            this.subObjects.Add(this.rectList); this.rectList.Hide();
            this.subObjects.Add(this.rectScroll); this.rectScroll.Hide();
            this.sprArrow = new FSprite("Big_Menu_Arrow", true)
            { scale = 0.5f, rotation = 180f, anchorX = 0.5f, anchorY = 0.5f };
            this.myContainer.AddChild(this.sprArrow);
            this.sprArrow.SetPosition(this.size.x - 12f, this.size.y / 2f);

            this.bumpList = new BumpBehaviour(this) { held = false, MouseOver = false };
            this.bumpScroll = new BumpBehaviour(this) { held = false, MouseOver = false };
        }

        internal ListItem[] itemList;

        /// <summary>
        /// Grab the array of <see cref="ListItem"/> in <see cref="OpComboBox"/>
        /// </summary>
        public ListItem[] GetItemList() => itemList;

        private DyeableRect rect, rectList, rectScroll;
        private MenuLabel lblText;
        private MenuLabel[] lblList;
        private FSprite sprArrow;

        private bool IsResourceSelector => this is OpResourceSelector;

        public override string value
        {
            get => base.value;
            set
            {
                if (base.value != value)
                {
                    if (string.IsNullOrEmpty(value)) { base.value = ""; return; }
                    foreach (ListItem i in this.itemList)
                    { if (i.name == value) { base.value = value; return; } }
                }
            }
        }

        /// <summary>
        /// Edge Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorF = greyedOut ? this.bumpBehav.GetColor(this.colorFill) : this.colorFill;

            this.sprArrow.color = this.held ? DyeableRect.MidToDark(this.rect.color) : this.rect.color;
            this.lblText.label.color = this.sprArrow.color;

            if (!held) { this.lblText.text = this.value; return; }

            this.rectList.size.x = this.size.x;
            this.rectList.pos = new Vector2(this.pos.x, this.pos.y + (downward ? -this.rectList.size.y : this.size.y));
            this.rectList.addSize = new Vector2(4f, 4f) * bumpList.AddSize;
            this.rectList.color = bumpList.GetColor(this.colorEdge);
            this.rectList.colorF = this.colorFill;
            this.rectList.fillAlpha = bumpList.FillAlpha;

            for (int i = 0; i < this.lblList.Length; i++)
            {
                this.lblList[i].text = this.itemList[this.listTop + i].name;
                this.lblList[i].label.color = this.lblList[i].text == this.value ? this.sprArrow.color : this.rectList.color;
                if (i == listHover)
                {
                    this.lblList[i].label.color = Color.Lerp(this.lblList[i].label.color,
                        mouseDown || this.lblList[i].text == this.value ? DyeableRect.MidToDark(this.lblList[i].label.color) : Color.white, bumpList.Sin(this.lblList[i].text == this.value ? 60f : 10f));
                }
                this.lblList[i].pos = new Vector2(this.pos.x - this.size.x / 2f + 12f, this.pos.y - 25f - 20f * i + (downward ? 0f : this.size.y + this.rectList.size.y));
            }
            this.lblText.text = this.value;
            //lblList[0].text = $"MO:{(MouseOver ? "O" : "X")}, lsMO:{(bumpList.MouseOver ? "O" : "X")}, scMO:{(bumpScroll.MouseOver ? "O" : "X")}, MD:{(mouseDown ? "O" : "X")}"; // Test

            this.rectScroll.size.y = ScrollLen(itemList.Length);
            this.rectScroll.pos.x = this.rectList.pos.x + this.size.x - 20f;
            this.rectScroll.pos.y = Custom.LerpAndTick(this.rectScroll.pos.y, ScrollPos(itemList.Length), bumpScroll.held ? 0.6f : 0.2f, (bumpScroll.held ? 0.6f : 0.2f) * DTMultiply(dt));
            this.rectScroll.addSize = new Vector2(2f, 2f) * bumpScroll.AddSize;
            this.rectScroll.color = bumpScroll.GetColor(this.colorEdge);
            this.rectScroll.colorF = bumpScroll.held ? this.rectScroll.color : this.colorFill;
            this.rectScroll.fillAlpha = bumpScroll.held ? 1f : bumpScroll.FillAlpha;
        }

        private float ScrollPos(int listSize) => this.rectList.pos.y + 10f + (this.rectList.size.y - 20f - this.rectScroll.size.y) * ((listSize - lblList.Length) - this.listTop) / (listSize - lblList.Length);

        private float ScrollLen(int listSize) => (this.rectList.size.y - 40f) * Mathf.Clamp01((float)lblList.Length / listSize) + 20f;

        private bool mouseDown = false, searchMode = false, downward = true;
        private int dTimer = 0, searchDelay, searchIdle = 0, listTop, listHover = -1;
        private float scrollHeldPos;
        private BumpBehaviour bumpList, bumpScroll;

        /// <summary>
        /// Return the index for specified value. Checks for current value if the argument is empty.
        /// <para>returns -1 if there's no such matching item</para>
        /// </summary>
        /// <param name="checkName">Leave this empty for checking index for current value</param>
        /// <returns></returns>
        public int GetIndex(string checkName = "")
        {
            if (string.IsNullOrEmpty(checkName)) { checkName = this.value; }
            for (int i = 0; i < this.itemList.Length; i++)
            { if (this.itemList[i].name == checkName) { return this.itemList[i].index; } }
            return -1;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            this.searchMode = false; //temp
            if (dTimer > 0) { dTimer--; }
            this.bumpBehav.MouseOver = base.MouseOver && !this.MouseOverList();
            if (this.held)
            {
                this.bumpList.MouseOver = this.MouseOverList();
                this.bumpScroll.MouseOver = this.MousePos.x >= this.rectScroll.pos.x - this.pos.x && this.MousePos.x <= this.rectScroll.pos.x + this.rectScroll.size.x - this.pos.x;
                this.bumpScroll.MouseOver = this.bumpScroll.MouseOver && this.MousePos.y >= this.rectScroll.pos.y - this.pos.y && this.MousePos.y <= this.rectScroll.pos.y + this.rectScroll.size.y - this.pos.y;
                if (this.bumpScroll.held)
                { // Holding scrollbar
                    if (Input.GetMouseButton(0))
                    { // Hold Scroll and update listtop
                        int num = Mathf.RoundToInt((this.MousePos.y - this.rectList.pos.y + this.pos.y - 10f - scrollHeldPos) * (this.itemList.Length - this.lblList.Length) / (this.rectList.size.y - 20f - this.rectScroll.size.y));
                        num = Custom.IntClamp(this.itemList.Length - this.lblList.Length - num, 0, this.itemList.Length - this.lblList.Length);
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
                else if (this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    { // Click behaviour
                        if (!mouseDown)
                        {
                            if (this.bumpScroll.MouseOver)
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
                            if (dTimer > 0) { dTimer = 0; this.searchMode = true; return; }
                            else { dTimer = FrameMultiply(15); this.value = ""; this.menu.PlaySound(SoundID.MENU_Checkbox_Uncheck); goto close; }
                        }
                    }
                    else // MouseOver List
                    {
                        if (this.MousePos.x >= 10f && this.MousePos.x <= this.rectList.size.x - 30f)
                        {
                            if (this.downward) { listHover = Mathf.FloorToInt((this.MousePos.y + this.rectList.size.y + 10f) / 20f); }
                            else { listHover = Mathf.FloorToInt((this.MousePos.y - this.size.y + 10f) / 20f); }
                            if (listHover > this.lblList.Length || listHover <= 0) { listHover = -1; }
                            else { listHover = this.lblList.Length - listHover; }
                        }
                        else { listHover = -1; }

                        if (this.menu.mouseScrollWheelMovement != 0)
                        {
                            int num = listTop + (int)Mathf.Sign(this.menu.mouseScrollWheelMovement) * Mathf.CeilToInt(this.lblList.Length / 2f);
                            num = Custom.IntClamp(num, 0, this.itemList.Length - this.lblList.Length);
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
                                    this.searchMode = false;
                                }
                                else
                                { // Select one from here
                                    newVal = this.itemList[listTop + listHover].name;
                                }
                                if (newVal != this.value) { this.value = newVal; this.menu.PlaySound(SoundID.MENU_Checkbox_Check); goto close; }
                            }
                        }
                    }
                }
                else
                { // Mouse out
                    if (Input.GetMouseButton(0) && !mouseDown || mouseDown && !Input.GetMouseButton(0)) { this.menu.PlaySound(SoundID.MENU_Checkbox_Uncheck); goto close; }
                }
                this.bumpList.Update(dt);
                this.bumpScroll.Update(dt);
                // Update Scroll
                return;
            close:
                mouseDown = false;
                held = false;
                this.Unheld();
                return;
            }
            if (base.MouseOver)
            {
                if (Input.GetMouseButton(0)) { mouseDown = true; }
                else if (mouseDown)
                {
                    mouseDown = false;
                    if (dTimer > 0) { dTimer = 0; this.searchMode = true; }
                    else { dTimer = FrameMultiply(15); }
                    // clicked: Expand list
                    this.held = true; this.fixedSize = this.size;
                    // Check available space
                    float listHeight = 20f * (Mathf.Clamp(this.itemList.Length, 1, 5)) + 10f;
                    if (listHeight < this.GetPos().y) { downward = true; }
                    else if (100f < this.GetPos().y) { downward = true; listHeight = 100f; }
                    else
                    {
                        float upHeight = 600f; // OpTab height
                        if (this.inScrollBox)
                        { upHeight = this.scrollBox.horizontal ? this.scrollBox.size.y : this.scrollBox.contentSize; }
                        upHeight -= this.GetPos().y + this.size.y;
                        if (upHeight < this.GetPos().y) { downward = true; listHeight = Mathf.Floor(this.GetPos().y / 20f) * 20f - 10f; }
                        else { downward = false; listHeight = Mathf.Min(listHeight, Mathf.Clamp(Mathf.Floor(upHeight / 20f), 1, 5) * 20f + 10f); }
                    }
                    // downward = false; // upward test
                    // Set RectList size
                    this.rectList.size = new Vector2(this.size.x, listHeight);
                    this.rectList.pos = new Vector2(this.pos.x, this.pos.y + (downward ? -listHeight : this.size.y));
                    this.rectList.Show();
                    // Set LabelList
                    this.lblList = new MenuLabel[Mathf.FloorToInt(listHeight / 20f)];
                    if (downward)
                    {
                        this.listTop = GetIndex() + 1;
                        if (this.listTop > this.itemList.Length - lblList.Length)
                        { this.listTop = this.itemList.Length - lblList.Length; }
                    }
                    else
                    {
                        this.listTop = GetIndex() - lblList.Length;
                        if (this.listTop < 0) { this.listTop = 0; }
                    }
                    for (int i = 0; i < this.lblList.Length; i++)
                    {
                        this.lblList[i] = new MenuLabel(menu, menu.pages[0], this.itemList[this.listTop + i].name, new Vector2(-10000f, -10000f), new Vector2(this.size.x - 12f, 20f), false);
                        this.lblList[i].label.alignment = FLabelAlignment.Left;
                        this.subObjects.Add(this.lblList[i]);
                    }
                    // Set RectScroll
                    this.rectScroll.size = new Vector2(15f, ScrollLen(itemList.Length));
                    this.rectScroll.pos = new Vector2(this.rectList.pos.x + this.rectList.size.x - 20f, ScrollPos(itemList.Length));
                    this.rectScroll.Show();

                    this.sprArrow.rotation = downward ? 180f : 0f;
                    this.bumpBehav.flash = 1f;
                    this.bumpList.flash = 1f;
                    this.bumpList.held = false;
                    this.bumpScroll.held = false;

                    this.menu.PlaySound(SoundID.MENU_Checkbox_Check);
                }
                else if (this.menu.mouseScrollWheelMovement != 0)
                {
                    int idx = GetIndex();
                    int num = idx + (int)Mathf.Sign(this.menu.mouseScrollWheelMovement);
                    num = Custom.IntClamp(num, 0, this.itemList.Length - 1);
                    if (num != idx)
                    {
                        this.bumpBehav.flash = 1f;
                        if (!_soundFilled) { _soundFill += 6; this.menu.PlaySound(SoundID.MENU_Scroll_Tick); }
                        this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
                        this.value = this.itemList[num].name;
                    }
                }
            }
            else
            {
                if (!Input.GetMouseButton(0)) { mouseDown = false; }
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(24f, size.x), 24f);
            base.OnChange();
            if (!_init) { return; }
            this.rect.pos = this.pos; this.rect.size = this.size;
            this.lblText.pos = new Vector2(this.pos.x - this.size.x / 2f + 12f, this.pos.y);
            this.lblText.size = this.size - new Vector2(12f, 0f);
            //this.lblText.text = this.value;
            this.sprArrow.SetPosition(this.size.x - 12f, this.size.y / 2f);
        }

        public override void Show()
        {
            base.Show();
            this.rect.Show();
            this.lblText.label.isVisible = true;
        }

        public override void Hide()
        {
            base.Hide();
            this.rect.Hide();
            this.Unheld();
        }

        private void Unheld()
        {
            this.fixedSize = null;
            this.rectList.Hide();
            for (int i = 0; i < this.lblList.Length; i++)
            { this.lblList[i].RemoveSprites(); this.subObjects.Remove(this.lblList[i]); }
            this.lblList = new MenuLabel[0];
            this.rectScroll.Hide();
            this.bumpScroll.held = false;
        }

        private bool MouseOverList()
        {
            if (!this.held) { return false; }
            if (this.MousePos.x < 0f || this.MousePos.x > this.size.x) { return false; }
            if (this.downward) { return this.MousePos.y >= -this.rectList.size.y && this.MousePos.y <= 0f; }
            else { return this.MousePos.y >= this.size.y && this.MousePos.y <= this.size.y + this.rectList.size.y; }
        }

        public override bool MouseOver => base.MouseOver || this.MouseOverList();

        /// <summary>
        /// Add items to this <see cref="OpComboBox"/>
        /// </summary>
        /// <param name="sort">Whether re-sort the list or add newItems at the end</param>
        /// <param name="newItems">New <see cref="ListItem"/>s to be added</param>
        /// <exception cref="InvalidActionException">Thrown when you call this for <see cref="OpResourceSelector"/>.</exception>
        public void AddItems(bool sort = true, params ListItem[] newItems)
        {
            if (IsResourceSelector) { throw new InvalidActionException(this, "You cannot use AddItems for OpEnumSelector", this.key); }
            List<ListItem> temp = new List<ListItem>(this.itemList);
            temp.AddRange(newItems);
            if (sort) { temp.Sort(ListItem.Comparer); }
            this.itemList = temp.ToArray();
            this.ResetIndex();
            this.OnChange();
        }

        /// <summary>
        /// Remove items with the said names
        /// </summary>
        /// <param name="selectNext">If true, when selected item is removed the value will be the next one. If false, the value will become null.</param>
        /// <param name="names">Names of items to be removed</param>
        /// <exception cref="InvalidActionException">Thrown when this is <see cref="OpResourceSelector"/>, or you removed every items.</exception>
        public void RemoveItems(bool selectNext = true, params string[] names)
        {
            if (IsResourceSelector) { throw new InvalidActionException(this, "You cannot use RemoveItems for OpEnumSelector", this.key); }
            List<ListItem> temp = new List<ListItem>(this.itemList);
            foreach (string name in names)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].name == name)
                    {
                        if (temp.Count == 1) { throw new InvalidActionException(this, "You cannot remove every items in OpComboBox", this.key); }
                        if (name == this.value)
                        { this.ForceValue(selectNext ? temp[i == 0 ? 1 : i - 1].name : ""); }
                        temp.RemoveAt(i); break;
                    }
                }
            }
            this.itemList = temp.ToArray();
            this.ResetIndex();
            this.OnChange();
        }
    }
}
