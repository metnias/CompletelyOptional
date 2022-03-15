using CompletelyOptional;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        /// <param name="width">The box width of folded <see cref="OpComboBox"/>. The minimum width is 30f. The height is fixed to 24f.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="list">Will be sorted automatically by <see cref="ListItem.value"/>, then <see cref="ListItem.name"/></param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <exception cref="ElementFormatException">Thrown when list has no <see cref="ListItem"/>.</exception>
        public OpComboBox(Vector2 pos, float width, string key, List<ListItem> list, string defaultName = "") : base(pos, new Vector2(width, 24f), key, defaultName)
        {
            this._size = new Vector2(Mathf.Max(30f, size.x), 24f);
            this.fixedSize = new Vector2(-1f, 24f);
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
            this.searchDelay = Mathf.FloorToInt(Custom.LerpMap(Mathf.Clamp(itemList.Length, 10, 90), 10, 90, 10, 50));
            this.searchDelay = FrameMultiply(this.searchDelay);
        }

        internal virtual void Initialize(string defaultName)
        {
            if (string.IsNullOrEmpty(defaultName) && allowEmpty) { this.defaultValue = ""; this._value = ""; }
            else
            {
                bool flag = false;
                for (int i = 0; i < this.itemList.Length; i++)
                { if (this.itemList[i].name == defaultName) { flag = true; break; } }
                if (flag) { this.defaultValue = defaultName; this._value = this.defaultValue; }
                else if (!allowEmpty) { this.defaultValue = itemList[0].name; this._value = itemList[0].name; }
                else { this.defaultValue = ""; this._value = ""; }
            }

            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

            this.rect = new DyeableRect(this.myContainer, Vector2.zero, this.size);
            this.lblText = FLabelCreate(this.value); this.lblText.alignment = FLabelAlignment.Left;
            this.lblText.x = -this.size.x / 2f + 12f; this.lblText.y = this.size.y / 2f;
            this.myContainer.AddChild(this.lblText);
            if (IsListBox)
            {
                neverOpened = false;
                this.rectList = new DyeableRect(this.myContainer, Vector2.zero, this.size);
                this.rectScroll = new DyeableRect(this.myContainer, Vector2.zero, this.size);
            }
            else
            {
                neverOpened = true;
                this.sprArrow = new FSprite("Big_Menu_Arrow", true)
                { scale = 0.5f, rotation = 180f, anchorX = 0.5f, anchorY = 0.5f };
                this.myContainer.AddChild(this.sprArrow);
                this.sprArrow.SetPosition(this.size.x - 12f, this.size.y / 2f);
            }
            this.lblList = new FLabel[0];
            this.searchCursor = new FCursor();
            this.myContainer.AddChild(this.searchCursor);
            this.searchCursor.isVisible = false;

            this.bumpList = new BumpBehaviour(this) { held = false, MouseOver = false };
            this.bumpScroll = new BumpBehaviour(this) { held = false, MouseOver = false };

            GrafUpdate(0f);
        }

        protected ListItem[] itemList;
        protected List<ListItem> searchList;

        /// <summary>
        /// Grab the array of <see cref="ListItem"/> in <see cref="OpComboBox"/>
        /// </summary>
        public ListItem[] GetItemList() => itemList;

        private bool neverOpened;
        protected DyeableRect rect, rectList, rectScroll;
        protected FLabel lblText;
        protected FLabel[] lblList;
        private FSprite sprArrow;

        private bool IsResourceSelector => this is OpResourceSelector || this is OpResourceList;
        private bool IsListBox => this is OpListBox;

        public override void Reset()
        {
            if (held) { CloseList(); }
            base.Reset();
        }

        public override string value
        {
            get => base.value;
            set
            {
                if (base.value != value)
                {
                    if (string.IsNullOrEmpty(value)) { base.value = allowEmpty ? "" : itemList[0].name; return; }
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

        /// <summary>
        /// If true, clicking the box again will choose None(<c>""</c>). If false, the user must choose one option.
        /// </summary>
        public bool allowEmpty = false;

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.bumpBehav.greyedOut = this.greyedOut;
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorFill = greyedOut ? this.bumpBehav.GetColor(this.colorFill) : this.colorFill;
            this.rect.GrafUpdate(timeStacker);

            Color c = this.held ? MenuColorEffect.MidToDark(this.rect.colorEdge) : this.rect.colorEdge;
            if (!IsListBox)
            {
                this.sprArrow.color = c;
                this.lblText.color = this.searchMode ? this.rect.colorEdge : c;
            }
            this.lblText.color = this.searchMode ? this.rect.colorEdge : c;

            if (!IsListBox && !held) { this.lblText.text = string.IsNullOrEmpty(this.value) ? "------" : GetDisplayValue(); return; }

            this.rectList.size.x = this.size.x;
            this.rectList.pos = new Vector2(0f, (downward ? -this.rectList.size.y : this.size.y));
            if (IsListBox && downward) { this.rectList.pos.y += this.rectList.size.y; }
            this.rectList.addSize = new Vector2(4f, 4f) * bumpList.AddSize;
            this.rectList.colorEdge = bumpList.GetColor(this.colorEdge);
            this.rectList.colorFill = this.colorFill;
            this.rectList.fillAlpha = IsListBox ? bumpList.FillAlpha : Mathf.Lerp(0.5f, 0.7f, bumpList.col);
            this.rectList.GrafUpdate(timeStacker);

            for (int i = 0; i < this.lblList.Length; i++)
            {
                this.lblList[i].text = LabelTest.TrimText(this.searchMode ?
                    (this.searchList.Count > this.listTop + i ? this.searchList[this.listTop + i].displayName : "")
                    : this.itemList[this.listTop + i].displayName,
                    this.size.x - 24f, true);
                this.lblList[i].color = this.lblList[i].text == this.value ? c : this.rectList.colorEdge;
                if (i == listHover)
                {
                    this.lblList[i].color = Color.Lerp(this.lblList[i].color,
                        mouseDown || this.lblList[i].text == this.value ? MenuColorEffect.MidToDark(this.lblList[i].color) : Color.white, bumpList.Sin(this.lblList[i].text == GetDisplayValue() ? 60f : 10f));
                }
                this.lblList[i].x = 12f;
                this.lblList[i].y = -15f - 20f * i + (downward ? 0f : this.size.y + this.rectList.size.y);
                if (IsListBox && downward) { this.lblList[i].y += this.rectList.size.y; }
            }
            this.lblText.text = LabelTest.TrimText(this.searchMode ?
                this.searchQuery : (string.IsNullOrEmpty(this.value) ? "------" : GetDisplayValue()),
                this.size.x - 24f, true);
            //lblList[0].text = $"MO:{(MouseOver ? "O" : "X")}, lsMO:{(bumpList.MouseOver ? "O" : "X")}, scMO:{(bumpScroll.MouseOver ? "O" : "X")}, MD:{(mouseDown ? "O" : "X")}"; // Test
            if (this.searchMode)
            {
                this.searchCursor.x = Mathf.Min(this.size.x - 24f, 12f + LabelTest.GetWidth(this.searchQuery) + LabelTest.CharMean(false));
                this.searchCursor.y = this.size.y * 0.5f - 7f + (IsListBox && downward ? this.rectList.size.y : 0f);
                this.searchCursor.color = Color.Lerp(this.colorEdge, MenuColorEffect.MidToDark(this.colorEdge), this.bumpBehav.Sin(this.searchList.Count > 0 ? 10f : 30f));
                this.searchCursor.alpha = 0.4f + 0.6f * Mathf.Clamp01((float)this.searchIdle / this.searchDelay);
            }

            int listSize = this.searchMode ? this.searchList.Count : this.itemList.Length;
            if (listSize > this.lblList.Length)
            {
                this.rectScroll.Show();
                this.rectScroll.pos.x = this.rectList.pos.x + this.size.x - 20f;
                if (this.rectScroll.size.y != ScrollLen(listSize))
                {
                    this.rectScroll.size.y = ScrollLen(listSize);
                    this.rectScroll.pos.y = ScrollPos(listSize);
                }
                else
                { this.rectScroll.pos.y = Custom.LerpAndTick(this.rectScroll.pos.y, ScrollPos(listSize), bumpScroll.held ? 0.6f : 0.2f, (bumpScroll.held ? 0.6f : 0.2f) / frameMulti); }
                this.rectScroll.addSize = new Vector2(2f, 2f) * bumpScroll.AddSize;
                this.rectScroll.colorEdge = bumpScroll.GetColor(this.colorEdge);
                this.rectScroll.colorFill = bumpScroll.held ? this.rectScroll.colorEdge : this.colorFill;
                this.rectScroll.fillAlpha = bumpScroll.held ? 1f : bumpScroll.FillAlpha;
                this.rectScroll.GrafUpdate(timeStacker);
            }
            else { this.rectScroll.Hide(); }
        }

        /// <summary>
        /// How long the DropBox should be. Default is 5 items long.
        /// </summary>
        public int listHeight = 5;

        protected int _listHeight => Custom.IntClamp(listHeight, 1, itemList.Length);

        protected float ScrollPos(int listSize) => this.rectList.pos.y + 10f + (this.rectList.size.y - 20f - this.rectScroll.size.y) * ((listSize - lblList.Length) - this.listTop) / (listSize - lblList.Length);

        protected float ScrollLen(int listSize) => (this.rectList.size.y - 40f) * Mathf.Clamp01((float)lblList.Length / listSize) + 20f;

        protected bool mouseDown = false, searchMode = false;
        protected bool downward = true;
        protected int dTimer = 0, searchDelay, searchIdle = 0, listTop, listHover = -1;
        protected float scrollHeldPos;
        protected BumpBehaviour bumpList, bumpScroll;
        protected string searchQuery = "";
        protected FCursor searchCursor;

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

        private int inputDelay = 0;

        protected void SearchModeUpdate()
        {
            // Input
            char? c = OpTextBox.TypingAccept(ref inputDelay);
            if (c.HasValue)
            {
                if (c.Value == '\b')
                {
                    if (this.searchQuery.Length > 0)
                    {
                        this.searchQuery = (this.searchQuery.Substring(0, this.searchQuery.Length - 1));
                        this.searchIdle = -1;
                        PlaySound(SoundID.MENY_Already_Selected_MultipleChoice_Clicked);
                    }
                }
                else if (char.IsLetterOrDigit(c.Value) || c == ' ')
                {
                    this.bumpBehav.flash = 2.5f;
                    this.searchQuery += c;
                    this.searchIdle = -1;
                    PlaySound(SoundID.MENU_Checkbox_Uncheck);
                }
            }

            if (inputDelay == 0 && this.searchIdle < this.searchDelay)
            {
                this.searchIdle++;
                if (this.searchIdle == this.searchDelay) { RefreshSearchList(); }
            }
        }

        public override void Update()
        {
            base.Update();
            this.rect.Update();
            this.rectList?.Update(); this.rectScroll?.Update();

            // this.searchMode = false; //temp
            if (dTimer > 0) { dTimer--; }
            if (IsListBox) { return; }
            this.bumpBehav.MouseOver = base.MouseOver && !this.MouseOverList();
            if (this.held)
            {
                if (greyedOut) { goto close; }
                this.bumpList.MouseOver = this.MouseOverList();
                this.bumpScroll.MouseOver = this.MousePos.x >= this.rectScroll.pos.x - this.pos.x && this.MousePos.x <= this.rectScroll.pos.x + this.rectScroll.size.x - this.pos.x;
                this.bumpScroll.MouseOver = this.bumpScroll.MouseOver && this.MousePos.y >= this.rectScroll.pos.y - this.pos.y && this.MousePos.y <= this.rectScroll.pos.y + this.rectScroll.size.y - this.pos.y;
                if (this.searchMode && !this.bumpScroll.held)
                {
                    SearchModeUpdate();
                }
                int listSize = this.searchMode ? this.searchList.Count : this.itemList.Length;
                if (this.bumpScroll.held)
                { // Holding scrollbar
                    if (Input.GetMouseButton(0))
                    { // Hold Scroll and update listtop
                        int num = Mathf.RoundToInt((this.MousePos.y - this.rectList.pos.y + this.pos.y - 10f - scrollHeldPos) * (listSize - this.lblList.Length) / (this.rectList.size.y - 20f - this.rectScroll.size.y));
                        num = Custom.IntClamp(listSize - this.lblList.Length - num, 0, listSize - this.lblList.Length);
                        // ComOptPlugin.LogInfo($"{listTop} > {num}; {this.itemList.Length}, {this.lblList.Length}, {this.itemList.Length - this.lblList.Length}");
                        if (listTop != num)
                        {
                            PlaySound(SoundID.MENU_Scroll_Tick);
                            this.listTop = num;
                            this.bumpScroll.flash = Mathf.Min(1f, this.bumpScroll.flash + 0.2f);
                            this.bumpScroll.sizeBump = Mathf.Min(2.5f, this.bumpScroll.sizeBump + 0.3f);
                        }
                    }
                    else
                    { // Release Scrollbar
                        this.bumpScroll.held = false; this.mouseDown = false;
                        PlaySound(SoundID.MENU_Scroll_Tick);
                    }
                }
                else if (this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    { // Click behaviour
                        if (!mouseDown)
                        {
                            if (this.bumpScroll.MouseOver && listSize > this.lblList.Length)
                            {
                                scrollHeldPos = this.MousePos.y - this.rectScroll.pos.y + this.pos.y;
                                this.bumpScroll.held = true; PlaySound(SoundID.MENU_First_Scroll_Tick);
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
                            else { dTimer = FrameMultiply(15); if (allowEmpty) { this.value = ""; } PlaySound(SoundID.MENU_Checkbox_Uncheck); goto close; }
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
                            num = Custom.IntClamp(num, 0, listSize - this.lblList.Length);
                            // ComOptPlugin.LogInfo($"{listTop} > {num}; {this.itemList.Length}, {this.lblList.Length}, {this.itemList.Length - this.lblList.Length}");
                            if (listTop != num)
                            {
                                PlaySound(SoundID.MENU_Scroll_Tick);
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
                                if (newVal != this.value) { this.value = newVal; PlaySound(SoundID.MENU_Checkbox_Check); goto close; }
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
                            if (!string.IsNullOrEmpty(d)) { ModConfigMenu.instance.ShowDescription(d); }
                        }
                    }
                }
                else
                { // Mouse out
                    if (Input.GetMouseButton(0) && !mouseDown || mouseDown && !Input.GetMouseButton(0)) { PlaySound(SoundID.MENU_Checkbox_Uncheck); goto close; }
                }
                this.bumpList.Update();
                this.bumpScroll.Update();
                // Update Scroll
                return;
            close:
                mouseDown = false;
                held = false;
                this.CloseList();
                return;
            }
            if (isInactive) { return; }
            if (base.MouseOver)
            {
                if (Input.GetMouseButton(0)) { mouseDown = true; }
                else if (mouseDown)
                {
                    mouseDown = false;
                    if (dTimer > 0) { dTimer = 0; this.searchMode = true; EnterSearchMode(); }
                    else { dTimer = FrameMultiply(15); }
                    // clicked: Expand list
                    this.held = true; this.fixedSize = this.size;
                    OpenList();

                    PlaySound(SoundID.MENU_Checkbox_Check);
                }
                else if (this.menu.mouseScrollWheelMovement != 0)
                {
                    int idx = GetIndex();
                    int num = idx + (int)Mathf.Sign(this.menu.mouseScrollWheelMovement);
                    num = Custom.IntClamp(num, 0, this.itemList.Length - 1);
                    if (num != idx)
                    {
                        this.bumpBehav.flash = 1f;
                        PlaySound(SoundID.MENU_Scroll_Tick);
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
            base.OnChange();
            this._size.x = Mathf.Max(30f, this._size.x);

            this.rect.size = this.size;
            this.lblText.x = 12f;
            this.lblText.y = this.size.y / 2f;
            if (IsListBox && downward)
            {
                this.rect.pos.y = this.rectList.size.y;
                this.lblText.y += this.rectList.size.y;
            }
            if (!IsListBox) { this.sprArrow.SetPosition(this.size.x - 12f, this.size.y / 2f); }
        }

        protected internal override void Deactivate()
        {
            base.Deactivate();
            if (IsListBox)
            {
                this.searchMode = false;
                this.bumpScroll.held = false;
            }
            else { this.CloseList(); }
        }

        protected void OpenList()
        {
            // Check available space
            float listHeight = 20f * Mathf.Clamp(this.itemList.Length, 1, _listHeight) + 10f;
            if (!IsListBox)
            {
                if (listHeight < this.GetPos().y) { downward = true; }
                else if (100f < this.GetPos().y) { downward = true; listHeight = 100f; }
                else
                {
                    float upHeight = 600f; // OpTab height
                    if (this.inScrollBox)
                    { upHeight = this.scrollBox.horizontal ? this.scrollBox.size.y : this.scrollBox.contentSize; }
                    upHeight -= this.GetPos().y + this.size.y;
                    if (upHeight < this.GetPos().y) { downward = true; listHeight = Mathf.Floor(this.GetPos().y / 20f) * 20f - 10f; }
                    else { downward = false; listHeight = Mathf.Min(listHeight, Mathf.Clamp(Mathf.Floor(upHeight / 20f), 1, _listHeight) * 20f + 10f); }
                }
                // downward = false; //upward test
                // Initialize Rects
                if (neverOpened)
                {
                    this.rectList = new DyeableRect(this.myContainer, Vector2.zero, this.size);
                    this.rectScroll = new DyeableRect(this.myContainer, Vector2.zero, this.size);
                    neverOpened = false;
                }
                this.sprArrow.rotation = downward ? 180f : 0f;
            }
            // Set RectList size
            this.rectList.size = new Vector2(this.size.x, listHeight);
            this.rectList.pos = new Vector2(this.pos.x, this.pos.y + (downward ? -listHeight : this.size.y));
            this.rectList.Show();
            // Set LabelList
            this.lblList = new FLabel[Mathf.FloorToInt(listHeight / 20f)];
            if (downward)
            {
                this.listTop = GetIndex() + 1;
                if (this.listTop > this.itemList.Length - lblList.Length)
                { this.listTop = this.itemList.Length - lblList.Length; }
            }
            else
            { this.listTop = GetIndex() - lblList.Length; }
            if (listTop < 0) { listTop = 0; }
            for (int i = 0; i < this.lblList.Length; i++)
            {
                this.lblList[i] = FLabelCreate(this.itemList[this.listTop + i].EffectiveDisplayName);
                this.lblList[i].alignment = FLabelAlignment.Left;
                this.myContainer.AddChild(this.lblList[i]);
            }
            // Set RectScroll
            this.rectScroll.size = new Vector2(15f, ScrollLen(itemList.Length));
            this.rectScroll.pos = new Vector2(this.rectList.pos.x + this.rectList.size.x - 20f, ScrollPos(itemList.Length));
            if (lblList.Length < itemList.Length) { this.rectScroll.Show(); }

            this.bumpBehav.flash = 1f;
            this.bumpList.flash = 1f;
            this.bumpList.held = false;
            this.bumpScroll.held = false;
        }

        private void CloseList()
        {
            this.searchMode = false;
            this.searchCursor.isVisible = false;
            this.fixedSize = null;
            if (!neverOpened) { this.rectList.Hide(); this.rectScroll.Hide(); }
            for (int i = 0; i < this.lblList.Length; i++)
            { this.lblList[i].isVisible = false; this.lblList[i].RemoveFromContainer(); }
            this.lblList = new FLabel[0];
            this.bumpScroll.held = false;
        }

        protected bool MouseOverList()
        {
            if (!this.held && !IsListBox) { return false; }
            if (this.MousePos.x < 0f || this.MousePos.x > this.size.x) { return false; }
            if (this.downward)
            {
                if (IsListBox) { return this.MousePos.y >= 0f && this.MousePos.y <= this.rectList.size.y; }
                return this.MousePos.y >= -this.rectList.size.y && this.MousePos.y <= 0f;
            }
            else { return this.MousePos.y >= this.size.y && this.MousePos.y <= this.size.y + this.rectList.size.y; }
        }

        protected internal override bool MouseOver => base.MouseOver || this.MouseOverList();

        /// <summary>
        /// Add items to this <see cref="OpComboBox"/>
        /// </summary>
        /// <param name="sort">Whether re-sort the list or add newItems at the end</param>
        /// <param name="newItems">New <see cref="ListItem"/>s to be added</param>
        /// <exception cref="InvalidActionException">Thrown when you call this for <see cref="OpResourceSelector"/>.</exception>
        public void AddItems(bool sort = true, params ListItem[] newItems)
        {
            if (IsResourceSelector) { throw new InvalidActionException(this, "You cannot use AddItems for OpResourceSelector", this.key); }
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
            if (IsResourceSelector) { throw new InvalidActionException(this, "You cannot use RemoveItems for OpResourceSelector", this.key); }
            List<ListItem> temp = new List<ListItem>(this.itemList);
            foreach (string name in names)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i].name == name)
                    {
                        if (temp.Count == 1) { throw new InvalidActionException(this, "You cannot remove every items in OpComboBox", this.key); }
                        if (name == this.value)
                        { this._value = selectNext ? temp[i == 0 ? 1 : i - 1].name : ""; }
                        temp.RemoveAt(i); break;
                    }
                }
            }
            this.itemList = temp.ToArray();
            this.ResetIndex();
            this.OnChange();
        }

        protected internal override string CopyToClipboard()
        {
            if (!IsListBox) { this.CloseList(); }
            return base.CopyToClipboard();
        }

        protected internal override bool CopyFromClipboard(string value)
        {
            if (!this.searchMode)
            {
                this.searchMode = true;
                EnterSearchMode();
            }
            this.searchQuery = value;
            RefreshSearchList();
            return true;
        }

        protected void EnterSearchMode()
        {
            this.searchQuery = "";
            this.searchList = new List<ListItem>(this.itemList);
            this.searchList.Sort(ListItem.Comparer);
            this.searchCursor.isVisible = true;
            this.searchCursor.SetPosition(LabelTest.CharMean(false) * 1.5f, this.size.y * 0.5f - 7f + (IsListBox && downward ? this.rectList.size.y : 0f));
            this.searchIdle = 1000;
        }

        protected void RefreshSearchList()
        {
            int curTop = this.searchList.Count > 0 ? GetIndex(this.searchList[this.listTop].name) : 0;
            this.searchList.Clear();
            for (int i = 0; i < this.itemList.Length; i++)
            {
                if (ListItem.SearchMatch(this.searchQuery, this.itemList[i].displayName) || ListItem.SearchMatch(this.searchQuery, this.itemList[i].name))
                { this.searchList.Add(this.itemList[i]); }
            }
            this.searchList.Sort(ListItem.Comparer);
            for (int i = 1; i < this.searchList.Count; i++)
            {
                if (curTop > GetIndex(this.searchList[i].name))
                { this.listTop = Math.Max(0, i - 1); return; }
            }
            this.listTop = 0;
        }

        protected string GetDisplayValue()
        {
            foreach (var item in itemList)
            {
                if (item.name == value)
                { return item.EffectiveDisplayName; }
            }

            return value;
        }
    }
}