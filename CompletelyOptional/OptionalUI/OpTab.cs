using CompletelyOptional;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    public class OpTab
    {
        /// <summary>
        /// Tab. 600 pxl * 600 pxl.
        /// </summary>
        public OpTab(OptionInterface owner, string name = "")
        {
            this.container = new FContainer();
            ConfigContainer.instance.Container.AddChild(this.container);
            this.container.isVisible = false;
            this.isInactive = true;
            this.owner = owner;
            this.items = new List<UIelement>();
            this.focusables = new List<UIelement>();
            this.name = name;
            this.colorButton = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorCanvas = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
        }

        public readonly OptionInterface owner;

        /// <summary>
        /// Colour of Tab Button. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color colorButton;

        /// <summary>
        /// Colour of the Canvas. <see cref="Menu.Menu.MenuColors.MediumGrey"/> in default.
        /// </summary>
        public Color colorCanvas;

        /// <summary>
        /// Name displayed on tab button and description
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Whether this tab is inactive or active. Controlled by <see cref="CompletelyOptional.ConfigTabController"/>.
        /// </summary>
        public bool isInactive { get; internal set; }

        #region ItemManager

        public List<UIelement> items;
        public List<UIelement> focusables;

        private void _AddItem(UIelement element)
        {
            if (this.items.Contains(element)) { return; }
            if (element.tab != null && element.tab != this) { RemoveItemsFromTab(element); }
            this.items.Add(element);
            this.container.AddChild(element.myContainer);
            if (element is ICanBeFocused) { this.focusables.Add(element); }
            element.SetTab(this);
        }

        /// <summary>
        /// Add <see cref="UIelement"/> to this Tab.
        /// If the item is already in other tab, this will call <see cref="RemoveItemsFromTab(UIelement[])"/> automatically
        /// </summary>
        public void AddItems(params UIelement[] elements)
        {
            foreach (UIelement item in elements) { this._AddItem(item); }
        }

        /// <summary>
        /// Remove <see cref="UIelement"/>  in this Tab.
        /// This will also remove them from <see cref="OpScrollBox"/> if <see cref="UIelement.inScrollBox"/>.
        /// See also <seealso cref="RemoveItemsFromTab(UIelement[])"/>
        /// </summary>
        public void RemoveItems(params UIelement[] items)
        {
            foreach (UIelement item in items) { this._RemoveItem(item); }
        }

        private void _RemoveItem(UIelement item)
        {
            if (item.inScrollBox) { item.RemoveFromScrollBox(); }
            while (this.items.Contains(item))
            { this.items.Remove(item); }
            this.container.RemoveChild(item.myContainer);
            while (this.focusables.Contains(item)) { this.focusables.Remove(item); }
            item.SetTab(null);
            if (ConfigContainer.focusedElement == item)
            { ConfigContainer.instance.FocusNewElementInDirection(new RWCustom.IntVector2(-1, 0)); }
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
        /// Remove <see cref="UIelement"/> from its Tab.
        /// This will also remove them from <see cref="OpScrollBox"/> if <see cref="UIelement.inScrollBox"/>.
        /// </summary>
        public static void RemoveItemsFromTab(params UIelement[] items)
        {
            foreach (UIelement item in items) { item.tab._RemoveItem(item); }
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

        #region Internal

        internal readonly FContainer container;

        /// <summary>
        /// Graphical Update of OpTab called by <see cref="CompletelyOptional.ConfigContainer.GrafUpdate"/>. Calls <see cref="UIelement.GrafUpdate"/>.
        /// </summary>
        internal void GrafUpdate(float timeStacker)
        {
            foreach (UIelement item in this.items)
            { if (!item.isInactive) { item.GrafUpdate(timeStacker); } }
        }

        /// <summary>
        /// Update for OpTab called by <see cref="CompletelyOptional.ConfigContainer.Update"/>. Calls <see cref="UIelement.Update"/>.
        /// </summary>
        internal void Update()
        {
            foreach (UIelement item in this.items) { if (!item.isInactive) { item.Update(); } }
        }

        internal void Deactivate()
        {
            this.isInactive = true;
            this.container.isVisible = false;
            foreach (UIelement element in this.items)
            { element.Deactivate(); }
        }

        internal void Activate()
        {
            this.isInactive = false;
            this.container.isVisible = true;
            foreach (UIelement element in this.items)
            { element.Reactivate(); }
        }

        /// <summary>
        /// Called by Config Machine.
        /// </summary>
        internal Dictionary<string, string> GetTabDictionary()
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

        internal Dictionary<string, UIconfig> GetTabObject()
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

        internal void Unload()
        {
            foreach (UIelement item in this.items)
            { item.Unload(); }
            this.container.RemoveAllChildren();
            this.container.RemoveFromContainer();
        }

        #endregion Internal
    }
}