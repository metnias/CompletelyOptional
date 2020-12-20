using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CompletelyOptional;

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
        /// <param name="size">The box size of folded <see cref="OpComboBox"/>. Minimum is 24 pxl for each dimension.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="list">Will be sorted automatically by <see cref="ListItem.value"/>, then <see cref="ListItem.name"/></param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <exception cref="ElementFormatException">Thrown when list has no <see cref="ListItem"/>.</exception>
        public OpComboBox(Vector2 pos, Vector2 size, string key, List<ListItem> list, string defaultName = "") : base(pos, size, key, defaultName)
        {
            this._size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
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
        /// <param name="size">The box size of folded <see cref="OpComboBox"/>. Minimum is 24 pxl for each dimension.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="array">The index will be <see cref="ListItem.value"/>, so the order will be kept</param>
        /// <param name="defaultName">Set to empty to have no default selection</param>
        /// <exception cref="ElementFormatException">Thrown when array has no item.</exception>
        public OpComboBox(Vector2 pos, Vector2 size, string key, string[] array, string defaultName = "") : this(pos, size, key, ArrayToList(array), defaultName)
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
        }

        internal virtual void Initialize(string defaultName)
        {
            if (string.IsNullOrEmpty(defaultName)) { this.defaultValue = ""; this.ForceValue(""); }
            else
            {
                bool flag = false;
                for (int i = 0; i < this.itemList.Length; i++)
                { if (this.itemList[i].name == defaultName) { flag = true; } }
                if (flag) { this.defaultValue = defaultName; this.ForceValue(this.defaultValue); }
            }
            if (!_init) { return; }

            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

            this.rect = new DyeableRect(this.menu, this.owner, this.pos, this.size);
            this.lblText = new MenuLabel(this.menu, this.owner, this.value, this.pos, this.size, false);
            this.rectList = new DyeableRect(this.menu, this.owner, this.pos, this.size);
            this.subObjects.Add(this.rect);
            this.subObjects.Add(this.lblText);
            this.subObjects.Add(this.rectList); this.rectList.Hide();
        }

        internal ListItem[] itemList;

        /// <summary>
        /// Grab the array of <see cref="ListItem"/> in <see cref="OpComboBox"/>
        /// </summary>
        public ListItem[] GetItemList() => itemList;

        private DyeableRect rect, rectList;
        private MenuLabel lblText;

        private bool IsResourceSelector => this is OpResourceSelector;

        public override string value
        {
            get => base.value;
            set
            {
                if (base.value != value)
                {
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
        }

        private bool mouseDown;
        private int test = -1, dTimer;

        public override void Update(float dt)
        {
            base.Update(dt);

            if (this.held)
            {
                if (dTimer > 0)
                {
                    dTimer--;
                    //double click behaviour: mouseover & !getmousebutton & mousedown => dbclick
                }

                return;
            }
            if (this.MouseOver)
            {
                if (Input.GetMouseButton(0)) { mouseDown = true; }
                else if (mouseDown)
                {
                    mouseDown = false;
                    dTimer = 20;
                    this.held = true;
                    //clicked
                    test = test < this.itemList.Length - 1 ? test + 1 : 0;
                    this.value = this.itemList[test].name;
                    this.lblText.text = this.itemList[test].name;
                }
            }
            else
            {
                if (!Input.GetMouseButton(0)) { mouseDown = false; }
            }
        }

        public override void OnChange()
        {
            this._size = new Vector2(Mathf.Max(24f, size.x), Mathf.Max(24f, size.y));
            base.OnChange();
            if (!_init) { return; }
            this.rect.pos = this.pos; this.rect.size = this.size;
            this.lblText.pos = this.pos; this.lblText.size = this.size;
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
            this.lblText.label.isVisible = false;
        }

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
