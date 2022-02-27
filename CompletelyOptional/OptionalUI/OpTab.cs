using CompletelyOptional;
using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    public class OpTab
    {
        /// <summary>
        /// Tab. 600 pxl * 600 pxl.
        /// </summary>
        public OpTab(string name = "")
        {
            menu = false;
            this.items = new List<UIelement>();
            this.isHidden = true;
            this.name = name;
            this.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorCanvas = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        }

        public List<SelectableUIelement> selectables;

        /// <summary>
        /// Do NOT mess with this on your own.
        /// </summary>
        public OptionInterface owner;

        /// <summary>
        /// Colour of Tab Button. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color color;

        /// <summary>
        /// Colour of the Canvas. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color colorCanvas;

        /// <summary>
        /// Do NOT use this.
        /// </summary>
        public bool menu;

        /// <summary>
        /// Name of this that will display on the bottom.
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Do NOT mess with this on your own.
        /// </summary>
        public bool isHidden;

#pragma warning disable CA1822 // Mark members as static

        /// <summary>
        /// Use <see cref="OptionInterface.IsConfigScreen"/> instead.
        /// </summary>
        [Obsolete]
        public bool init => CompletelyOptional.OptionScript.isOptionMenu;

#pragma warning restore CA1822 // Mark members as static

        public List<UIelement> items;

        /// <summary>
        /// Update for OpTab. Automatically called. Don't call this by yourself.
        /// </summary>
        /// <param name="dt">deltaTime</param>
        public void Update(float dt)
        {
            if (this.isHidden || !CompletelyOptional.OptionScript.init) { return; }

            foreach (UIelement item in this.items)
            { item.Update(dt); }
        }

        #region ItemManager

        /// <summary>
        /// Obsolete! Use <see cref="AddItems(UIelement[])"/> instead.
        /// </summary>
        [Obsolete]
        public void AddItem(UIelement item)
        {
            this._AddItem(item);
        }

        private void _AddItem(UIelement item)
        {
            if (this.items.Contains(item)) { return; }
            if (item.tab != null && item.tab != this) { RemoveItemsFromTab(item); }
            this.items.Add(item);
            item.SetTab(this);
            if (OptionScript.isOptionMenu && ConfigMenu.currentTab == this)
            {
                foreach (MenuObject obj in item.subObjects) { OptionScript.configMenu.pages[0].subObjects.Add(obj); }
                OptionScript.configMenu.pages[0].Container.AddChild(item.myContainer);
            }
        }

        /// <summary>
        /// Add <see cref="UIelement"/> to this Tab.
        /// If the item is already in other tab, this will call <see cref="RemoveItemsFromTab(UIelement[])"/> automatically
        /// </summary>
        public void AddItems(params UIelement[] items)
        {
            foreach (UIelement item in items) { this._AddItem(item); }
        }

        /// <summary>
        /// Obsolete! Use RemoveItems instead.
        /// </summary>
        [Obsolete]
        public void RemoveItem(UIelement item)
        { _RemoveItem(item); }

        /// <summary>
        /// Remove <see cref="UIelement"/>  in this Tab.
        /// This will also remove them from <see cref="OpScrollBox"/> if <see cref="UIelement.inScrollBox"/>.
        /// See also <seealso cref="RemoveItemsFromTab(UIelement[])"/>
        /// </summary>
        public void RemoveItems(params UIelement[] items)
        {
            foreach (UIelement item in items) { this._RemoveItem(item); }
        }

        /// <summary>
        /// Remove <see cref="UIelement"/> from its Tab.
        /// This will also remove them from <see cref="OpScrollBox"/> if <see cref="UIelement.inScrollBox"/>.
        /// </summary>
        public static void RemoveItemsFromTab(params UIelement[] items)
        {
            foreach (UIelement item in items) { item.tab._RemoveItem(item); }
        }

        private void _RemoveItem(UIelement item)
        {
            if (item.inScrollBox) { item.RemoveFromScrollBox(); }
            while (this.items.Contains(item))
            { this.items.Remove(item); }
            item.SetTab(null);
        }

        /// <summary>
        /// Call <see cref="UIelement.Show"/> in bulk
        /// </summary>
        public static void ShowItems(params UIelement[] items)
        { foreach (UIelement item in items) { item.Show(); } }

        /// <summary>
        /// Call <see cref="UIelement.Hide"/> in bulk
        /// </summary>
        public static void HideItems(params UIelement[] items)
        { foreach (UIelement item in items) { item.Hide(); } }

        /// <summary>
        /// Destroy UIelement from runtime. Generally using this is NOT recommended.
        /// Using this for <see cref="UIconfig"/> should be avoided.
        /// </summary>
        public static void DestroyItems(params UIelement[] items)
        {
            foreach (UIelement item in items)
            {
                item.Hide();
                item.tab.RemoveItems(item);
                item.Unload();
            }
        }

        /// <summary>
        /// Set <see cref="UIconfig.greyedOut"/> and <see cref="UItrigger.greyedOut"/> in bulk.
        /// This will ignore if the item is neither of those
        /// </summary>
        /// <param name="greyedOut">New greyedOut value</param>
        public static void SetGreyedOutItems(bool greyedOut, params UIelement[] items)
        {
            foreach (UIelement item in items)
            {
                if (item is UIconfig) { (item as UIconfig).greyedOut = greyedOut; }
                else if (item is UItrigger) { (item as UItrigger).greyedOut = greyedOut; }
            }
        }

        #endregion ItemManager

        /// <summary>
        /// Hide this tab. Automatically called. Do NOT call this by yourself.
        /// </summary>
        public void Hide()
        {
            this.isHidden = true;
            foreach (UIelement element in this.items)
            { element.Hide(); }
        }

        /// <summary>
        /// Show this tab. Automatically called. Do NOT call this by yourself.
        /// </summary>
        public void Show()
        {
            this.isHidden = false;
            foreach (UIelement element in this.items)
            { if (!element.hidden) { element.Show(); } }
        }

        /// <summary>
        /// Called by Config Machine. You don't need to care about this.
        /// </summary>
        public Dictionary<string, string> GetTabDictionary()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            foreach (UIelement element in this.items)
            {
                if (element.GetType().IsSubclassOf(typeof(UIconfig)))
                {
                    if ((element as UIconfig).cosmetic) { continue; }
                    if (config.ContainsKey((element as UIconfig).key))
                    {
                        throw new DupelicateKeyException(this.name, (element as UIconfig).key);
                    }
                    config.Add((element as UIconfig).key, (element as UIconfig).value);
                }
            }

            return config;
        }

        /// <summary>
        /// Called by Config Machine. You don't need to care about this.
        /// </summary>
        public Dictionary<string, UIconfig> GetTabObject()
        {
            Dictionary<string, UIconfig> config = new Dictionary<string, UIconfig>();

            foreach (UIelement element in this.items)
            {
                if (element.GetType().IsSubclassOf(typeof(UIconfig)))
                {
                    if ((element as UIconfig).cosmetic) { continue; }
                    if (config.ContainsKey((element as UIconfig).key))
                    {
                        throw new DupelicateKeyException(this.name, (element as UIconfig).key);
                    }
                    config.Add((element as UIconfig).key, (element as UIconfig));
                }
            }

            return config;
        }

        public void Unload()
        {
            foreach (UIelement item in this.items)
            { item.Unload(); }
        }
    }
}