using CompletelyOptional;
using Menu;
using RWCustom;
using System;
using System.Linq;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// UIelement for <see cref="OpTab"/> Canvas (600x600)
    /// </summary>
    public abstract class UIelement
    {
        // Codes for Modders who only uses provided elements

        #region Shallow

        /// <summary>
        /// Rectangular UIelement.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size in pxl</param>
        public UIelement(Vector2 pos, Vector2 size)
        {
            this.isRectangular = true;
            this._pos = pos;
            this._size = size;
            this.menu = ModConfigMenu.instance;
            this.myContainer = new FContainer
            {
                x = this.ScreenPos.x + 0.01f,
                y = this.ScreenPos.y + 0.01f,
                scaleX = 1f,
                scaleY = 1f
            };
            this.inScrollBox = false;
            this.description = "";
            this.hidden = false;
        }

        /// <summary>
        /// Circular UIelement.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="rad">Radius in pxl</param>
        public UIelement(Vector2 pos, float rad)
        {
            this.isRectangular = false;
            this._pos = pos;
            this._rad = rad;
            this.menu = ModConfigMenu.instance;
            this.myContainer = new FContainer()
            {
                x = this.ScreenPos.x + 0.01f,
                y = this.ScreenPos.y + 0.01f,
                scaleX = 1f,
                scaleY = 1f
            };
            this.inScrollBox = false;
            this.description = "";
            this.hidden = false;
        }

        /// <summary>
        /// Resets <see cref="UIelement"/>.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Hide this element. Called manually by Modders
        /// </summary>
        public void Hide()
        {
            this.hidden = true;
            this.Deactivate();
        }

        /// <summary>
        /// Show this element.  Called manually by Modders
        /// </summary>
        public void Show()
        {
            this.hidden = false;
            this.Reactivate();
        }

        /// <summary>
        /// For grabbing LeftBottom Position of this element from LeftBottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>, without offset.
        /// See also <seealso cref="x"/> and <seealso cref="y"/>.
        /// </summary>
        public Vector2 GetPos()
        {
            if (inScrollBox)
            { return _pos - scrollBox.childOffset; }
            else
            { return _pos; }
        }

        /// <summary>
        /// For setting LeftBottom Position of this element from LeftBottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>, without offset.
        /// See also <seealso cref="x"/> and <seealso cref="y"/>.
        /// </summary>
        /// <param name="value"></param>
        public void SetPos(Vector2 value)
        {
            if (inScrollBox)
            {
                if (_pos != value + scrollBox.childOffset)
                {
                    _pos = value + scrollBox.childOffset + OpTab._offset;
                    OnChange();
                }
            }
            else if (_pos != value)
            {
                _pos = value;
                OnChange();
            }
        }

        /// <summary>
        /// For getting and setting Left Position of this element from the Left of <see cref="OpTab"/> or <see cref="OpScrollBox"/>, without offset.
        /// </summary>
        public float x { get => GetPos().x; set => SetPos(new Vector2(value, GetPos().y)); }

        /// <summary>
        /// For getting and setting Bottom Position of this element from the Bottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>, without offset.
        /// </summary>
        public float y { get => GetPos().y; set => SetPos(new Vector2(GetPos().x, value)); }

        /// <summary>
        /// Size of this element. Changing this will call <see cref="OnChange"/> automatically.
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when <see cref="isRectangular"/> is false</exception>
        public Vector2 size
        {
            get
            {
                if (!isRectangular) { throw new InvalidGetPropertyException(this, "size"); }
                return _size;
            }
            set
            {
                if (!isRectangular) { throw new InvalidActionException(this, "This Non-Rectangular item tried to set size which is Invalid!"); }
                if (fixedSize != null)
                {
                    _size = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
                    if (fixedSize.Value.x > 0f) { _size.x = fixedSize.Value.x; }
                    if (fixedSize.Value.y > 0f) { _size.y = fixedSize.Value.y; }
                }
                else if (_size != value)
                {
                    _size = new Vector2(Mathf.Max(value.x, 0f), Mathf.Max(value.y, 0f));
                }
                OnChange();
            }
        }

        /// <summary>
        /// Radius of the element. Changing this will call <see cref="OnChange"/> automatically.
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when <see cref="isRectangular"/> is true</exception>
        public float rad
        {
            get
            {
                if (isRectangular) { throw new InvalidGetPropertyException(this, "rad"); }
                return _rad;
            }
            set
            {
                if (isRectangular) { throw new InvalidActionException(this, "This Rectangular item tried to set rad which is Invalid!"); }
                if (fixedRad != null) { _rad = fixedRad.Value; OnChange(); }
                else if (_rad != value)
                {
                    _rad = Mathf.Max(value, 0f);
                    OnChange();
                }
            }
        }

        /// <summary>
        /// Infotext that will be shown at the bottom of the screen.
        /// </summary>
        public string description;

        /// <summary>
        /// Whether the element is Rectangular(true) or Circular(false)
        /// </summary>
        public readonly bool isRectangular;

        /// <summary>
        /// Whether the Modder has hide/inactivate this manually.
        /// Use <see cref="Hide"/> and <see cref="Show"/> to manipulate this.
        /// <para>To check whether this is actually inactive/invisible, use <see cref="isInactive"/></para>
        /// </summary>
        public bool hidden { get; private set; }

        /// <summary>
        /// Whether this is actually not Active/is hidden. See also <seealso cref="hidden"/>
        /// </summary>
        public bool isInactive => hidden || this.tab?.isInactive == true || this.scrollBox?.isInactive == true;

        /// <summary>
        /// Moves this UIelement to the backmost visually.
        /// </summary>
        public void MoveToBack() => this.myContainer.MoveToBack();

        /// <summary>
        /// Moves this UIelement to the frontmost visually.
        /// </summary>
        public void MoveToFront() => this.myContainer.MoveToFront();

        /// <summary>
        /// Moves this UIelement in front of referenced <see cref="UIelement"/> visually.
        /// </summary>
        /// <param name="reference">Target</param>
        public void MoveInFrontOfElement(UIelement reference) => this.myContainer.MoveInFrontOfOtherNode(reference.myContainer);

        /// <summary>
        /// Moves this UIelement behind referenced <see cref="UIelement"/> visually.
        /// </summary>
        /// <param name="reference">Target</param>
        public void MoveBehindElement(UIelement reference) => this.myContainer.MoveBehindOtherNode(reference.myContainer);

        #endregion Shallow

        // Codes for modders who makes custom UIelement

        #region Deep

        /// <summary>
        /// Called whenever this UIelement needs graphical change.
        /// RePosition and ReSize subObjects.
        /// </summary>
        public virtual void OnChange()
        {
        }

        /// <summary>
        /// Update method that happens every frame.
        /// </summary>
        public virtual void Update()
        {
            bool showDesc = !string.IsNullOrEmpty(this.description) && !isInactive
                 && (this.MouseOver || (this is ICanBeFocused && (this as ICanBeFocused).Focused()));
            if (showDesc) { menu.ShowDescription(this.description); }
        }

        /// <summary>
        /// Update for graphical detail.
        /// </summary>
        /// <param name="timeStacker">timeStacker</param>
        public virtual void GrafUpdate(float timeStacker)
        {
            this.myContainer.x = ScreenPos.x + 0.01f;
            this.myContainer.y = ScreenPos.y + 0.01f;
        }

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID) => ConfigContainer.PlaySound(soundID);

        /// <summary>
        /// Restricted <see cref="Menu.Menu.PlaySound(SoundID, float, float, float)"/> to prevent sound glitch
        /// </summary>
        public static void PlaySound(SoundID soundID, float pan, float vol, float pitch) => ConfigContainer.PlaySound(soundID, pan, vol, pitch);

        protected Vector2 _pos;
        protected Vector2 _size;
        protected float _rad;

        /// <summary>
        /// For setting LeftBottom Position of this element from LeftBottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>.
        /// <para>For grabbing position without offset, use <see cref="GetPos"/>.</para>
        /// </summary>
        protected internal Vector2 pos
        {
            get
            { return _pos; }
            set
            { if (_pos != value) { _pos = value; OnChange(); } }
        }

        /// <summary>
        /// If this is set, this element cannot change its <see cref="size"/>.
        /// </summary>
        protected Vector2? fixedSize;

        /// <summary>
        /// If this is set, this element cannot change its <see cref="rad"/>.
        /// </summary>
        protected float? fixedRad;

        /// <summary>
        /// OpTab this element is belong to.
        /// </summary>
        protected internal OpTab tab;

        /// <summary>
        /// <see cref="ModConfigMenu"/> instance this element is in.
        /// </summary>
        protected ModConfigMenu menu;

        /// <summary>
        /// You can alternatively use <c>menu.pages[0]</c> for this
        /// </summary>
        protected Page owner => menu.pages[0];

        /// <summary>
        /// <see cref="FContainer"/> to add <see cref="FSprite"/>.
        /// </summary>
        protected internal FContainer myContainer;

        /// <summary>
        /// Whether mousecursor is over this element or not.
        /// </summary>
        protected internal virtual bool MouseOver
        {
            get
            {
                if (this.isRectangular)
                {
                    return this.MousePos.x > 0f && this.MousePos.y > 0f && this.MousePos.x < this.size.x && this.MousePos.y < this.size.y;
                }
                else
                {
                    return Custom.DistLess(new Vector2(rad, rad), this.MousePos, rad);
                }
            }
        }

        /// <summary>
        /// Mouse Position on <see cref="UIelement"/>. The BottomLeft(<see cref="pos"/>) is (0f, 0f).
        /// </summary>
        protected internal Vector2 MousePos
        {
            get
            {
                Vector2 p = new Vector2(menu.mousePosition.x - ScreenPos.x, menu.mousePosition.y - ScreenPos.y);
                if (inScrollBox)
                { p += scrollBox.camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos; }
                else if (tab != null) { p -= new Vector2(tab.container.x, tab.container.y); }
                return p;
            }
        }

        /// <summary>
        /// Which <see cref="OpScrollBox"/> this element is in. See also <seealso cref="inScrollBox"/>
        /// </summary>
        protected internal OpScrollBox scrollBox { get; private set; }

        /// <summary>
        /// Whether this is inside <see cref="OpScrollBox"/>. See also <seealso cref="scrollBox"/>
        /// </summary>
        protected internal bool inScrollBox { get; private set; }

        /// <summary>
        /// Called when exiting ConfigMenu.
        /// </summary>
        protected internal virtual void Unload()
        {
            this.myContainer.RemoveAllChildren();
            this.myContainer.RemoveFromContainer();
        }

        protected internal Vector2 ScreenPos
        {
            get
            {
                if (this.owner == null) { return this._pos; }
                return owner.ScreenPos + this._pos;
            }
        }

        /// <summary>
        /// Set <see cref="FLabel"/>'s pos in the Center of the size
        /// </summary>
        /// <param name="label"><see cref="FLabel"/> to be placed</param>
        /// <param name="pos">Leftbottom position of arbitary rectangular, relative from <see cref="UIelement.pos"/></param>
        /// <param name="size">An arbitary rectangular for this label to be in its center</param>
        protected static void LabelPlaceAtCenter(FLabel label, Vector2 pos, Vector2 size)
        {
            label.x = pos.x + size.x / 2f;
            label.y = pos.y + size.y / 2f;
        }

        /// <summary>
        /// Frame multiplier for Many More Fixes' framerate unlock feature. See also <see cref="FrameMultiply(int)"/>
        /// </summary>
        public static float frameMulti => Mathf.Max(1.00f, ComOptPlugin.curFramerate / 60.0f);

        /// <summary>
        /// Multiplies frame count by <see cref="frameMulti"/> to accommodate with Many More Fixes' framerate unlock feature.
        /// </summary>
        public static int FrameMultiply(int origFrameCount) => Mathf.RoundToInt(origFrameCount * frameMulti);

        /// <summary>
        /// Whether the menu is using Mouse mode or Controller/Keyboard mode
        /// </summary>
        public bool MenuMouseMode => menu.manager.menuesMouseMode;

        /// <summary>
        /// User's <see cref="Player.InputPackage"/> for Controller/Keyboard support
        /// </summary>
        public Player.InputPackage CtlrInput => menu.input;

        /// <summary>
        /// User's <see cref="Player.InputPackage"/> in the last frame for Controller/Keyboard support
        /// </summary>
        public Player.InputPackage LastCtlrInput => menu.lastInput;

        #endregion Deep

        // Codes just for ConfigMachine

        #region Internal

        /// <summary>
        /// This will be called by OpScrollBox automatically.
        /// </summary>
        internal bool AddToScrollBox(OpScrollBox scrollBox)
        {
            if (OpScrollBox.ChildBlacklist.Contains<Type>(this.GetType())) { ComOptPlugin.LogError(this.GetType().Name + " instances may not be added to a scrollbox!"); return false; }
            if (this.inScrollBox) { ComOptPlugin.LogError("This item is already in an OpScrollBox! The later call is ignored."); return false; }
            this.inScrollBox = true;
            this.scrollBox = scrollBox;
            this._pos += this.scrollBox.childOffset;
            this.OnChange();
            return true;
        }

        internal void RemoveFromScrollBox()
        {
            if (!this.inScrollBox) { ComOptPlugin.LogError("This item is not in an OpScrollBox! This call will be ignored."); return; }
            if (this.scrollBox.lastFocusedElement == this)
            {
                this.scrollBox.lastFocusedElement = null;
                foreach (UIelement item in this.scrollBox.children)
                { if (item != this && item is ICanBeFocused) { this.scrollBox.lastFocusedElement = item; break; } }
            }
            this._pos -= this.scrollBox.childOffset;
            this.inScrollBox = false;
            this.scrollBox = null;
            this.OnChange();
        }

        /// <summary>
        /// Do not use this. Instead, use <see cref="OpTab.AddItems(UIelement[])"/> and <see cref="OpTab.RemoveItems(UIelement[])"/>.
        /// </summary>
        /// <param name="newTab">new OpTab this item will be belong to</param>
        internal void SetTab(OpTab newTab)
        {
            if (this.tab != null && newTab != null) { this.tab.RemoveItems(this); }
            this.tab = newTab;
        }

        internal Vector2 CenterPos()
        {
            if (this.isRectangular) { return this.ScreenPos + this.size / 2f; }
            return this.ScreenPos + (this.rad / 2f * Vector2.one);
        }

        /// <summary>
        /// Actually deactivate/hide this UIelement.
        /// </summary>
        protected internal virtual void Deactivate()
        {
            this.myContainer.isVisible = false;
        }

        /// <summary>
        /// Actually reactivate/unhide this UIelement.
        /// </summary>
        protected internal virtual void Reactivate()
        {
            if (!this.hidden) { this.myContainer.isVisible = true; }
        }

        #endregion Internal
    }
}