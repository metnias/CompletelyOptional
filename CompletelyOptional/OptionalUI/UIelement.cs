using CompletelyOptional;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// UIelement for <see cref="OpTab"/> Canvas (600x600)
    /// </summary>
    public abstract class UIelement
    {
        /// <summary>
        /// Rectangular UIelement.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size in pxl</param>
        public UIelement(Vector2 pos, Vector2 size)
        {
            this.isRectangular = true;
            this._pos = pos + _offset;
            this._size = size;
            this.menu = ModConfigMenu.instance;
            this.myContainer = new FContainer();
            this.myContainer.SetPosition(this.ScreenPos);
            this.myContainer.scaleX = 1f;
            this.myContainer.scaleY = 1f;
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
            this._pos = pos + _offset;
            this._rad = rad;
            this.menu = ModConfigMenu.instance;
            this.myContainer = new FContainer();
            this.myContainer.SetPosition(this.ScreenPos);
            this.myContainer.scaleX = 1f;
            this.myContainer.scaleY = 1f;
            this.inScrollBox = false;
            this.description = "";
            this.hidden = false;
        }

        /// <summary>
        /// If this is set, this element cannot change its <see cref="size"/>.
        /// </summary>
        protected internal Vector2? fixedSize;

        /// <summary>
        /// If this is set, this element cannot change its <see cref="rad"/>.
        /// </summary>
        protected internal float? fixedRad;

        /// <summary>
        /// This will be called by OpScrollBox automatically.
        /// </summary>
        internal bool AddToScrollBox(OpScrollBox scrollBox)
        {
            if (OpScrollBox.ChildBlacklist.Contains<Type>(this.GetType())) { Debug.LogError(this.GetType().Name + " instances may not be added to a scrollbox!"); return false; }
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
            this._pos -= this.scrollBox.childOffset;
            this.inScrollBox = false;
            this.scrollBox = null;
            this.OnChange();
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
        /// Resets <see cref="UIelement"/>.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Offset from BottomLeft of the screen.
        /// </summary>
        public static readonly Vector2 _offset = new Vector2(558.00f, 120.01f);

        /// <summary>
        /// Add number (in proportion with sound effect's length) to this whenever you're playing soundeffect. See also <seealso cref="_soundFilled"/>
        /// </summary>
        protected internal static int _soundFill
        {
            get { return OptionScript.soundFill; }
            set
            {
                if (OptionScript.soundFill < value)
                { OptionScript.soundFill += FrameMultiply(value - OptionScript.soundFill); }
                else
                { OptionScript.soundFill = value; }
            }
        }

        /// <summary>
        /// Whether the sound engine is full or not. See also <seealso cref="_soundFill"/>
        /// </summary>
        protected internal static bool _soundFilled => _soundFill > FrameMultiply(80);

        /// <summary>
        /// For grabbing LeftBottom Position of this element from LeftBottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>, without offset.
        /// </summary>
        /// <seealso cref="pos"/>
        public Vector2 GetPos()
        {
            if (inScrollBox)
            { return _pos - scrollBox.childOffset - _offset; }
            else
            { return _pos - _offset; }
        }

        /// <summary>
        /// For setting LeftBottom Position of this element from LeftBottom of <see cref="OpTab"/> or <see cref="OpScrollBox"/>.
        /// <para>For grabbing position without offset, use <see cref="GetPos"/>.</para>
        /// </summary>
        public Vector2 pos
        {
            get
            { return _pos; }
            set
            {
                if (inScrollBox)
                {
                    if (_pos != value + scrollBox.childOffset + _offset)
                    {
                        _pos = value + scrollBox.childOffset + _offset;
                        OnChange();
                    }
                }
                else if (_pos != value + _offset)
                {
                    _pos = value + _offset;
                    OnChange();
                }
            }
        }

        protected internal Vector2 _pos;

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

        protected internal Vector2 _size;

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
                if (fixedRad != null) { _rad = fixedRad.Value; OnChange(); }
                else if (_rad != value)
                {
                    _rad = Mathf.Max(value, 0f);
                    OnChange();
                }
            }
        }

        protected internal float _rad;

        /// <summary>
        /// Whether the element is Rectangular(true) or Circular(false)
        /// </summary>
        public readonly bool isRectangular;

        /// <summary>
        /// OpTab this element is belong to.
        /// </summary>
        protected internal OpTab tab;

        /// <summary>
        /// <see cref="Menu.Menu"/> instance this element is in.
        /// </summary>
        protected internal Menu.Menu menu;

        /// <summary>
        /// You can alternatively use <c>menu.pages[0]</c> for this
        /// </summary>
        protected internal Page owner => menu.pages[0];

        /// <summary>
        /// <see cref="FContainer"/> to add <see cref="FSprite"/>.
        /// </summary>
        protected internal FContainer myContainer;

        /// <summary>
        /// Do not use this. Instead, use <see cref="OpTab.AddItems(UIelement[])"/> and <see cref="OpTab.RemoveItems(UIelement[])"/>.
        /// </summary>
        /// <param name="newTab">new OpTab this item will be belong to</param>
        internal void SetTab(OpTab newTab)
        {
            if (this.tab != null && newTab != null) { this.tab.RemoveItems(this); }
            this.tab = newTab;
        }

        /// <summary>
        /// Whether this is Hidden manually by Modder.
        /// Use <see cref="Hide"/> and <see cref="Show"/> to manipulate this.
        /// <para>To check whether this is actually hidden/invisible, use <see cref="isHidden"/></para>
        /// </summary>
        public bool hidden { get; private set; }

        /// <summary>
        /// Whether this is actually Hidden. See also <seealso cref="hidden"/>
        /// </summary>
        public bool isHidden => hidden || this.tab?.isHidden == true || this.scrollBox?.isHidden == true;

        /// <summary>
        /// Called whenever this UIelement needs graphical change.
        /// RePosition and ReSize subObjects.
        /// </summary>
        public virtual void OnChange()
        {
            this.myContainer.SetPosition(this.ScreenPos);
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
        /// Whether mousecursor is over this element or not.
        /// </summary>
        public virtual bool MouseOver
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
        public Vector2 MousePos
        {
            get
            {
                Vector2 p = new Vector2(this.menu.mousePosition.x - this.ScreenPos.x, this.menu.mousePosition.y - this.ScreenPos.y);
                if (this.inScrollBox)
                { p += this.scrollBox.camPos - (this.scrollBox.horizontal ? Vector2.right : Vector2.up) * this.scrollBox.scrollOffset - this.scrollBox.pos; }
                return p;
            }
        }

        /// <summary>
        /// Infotext that will be shown at the bottom of the screen.
        /// </summary>
        public string description;

        /// <summary>
        /// Update method that happens every frame.
        /// </summary>
        public virtual void Update()
        {
            showDesc = !isHidden && this.MouseOver && !string.IsNullOrEmpty(this.description);
            // if (showDesc && !(this is UIconfig) && !(this is UItrigger))
            // { ConfigMenu.description = this.description; }
        }

        protected internal bool showDesc;

        /// <summary>
        /// Update for graphical detail.
        /// </summary>
        /// <param name="timeStacker">timeStacker</param>
        public virtual void GrafUpdate(float timeStacker)
        {
            this.myContainer.SetPosition(this.ScreenPos);
        }

        /// <summary>
        /// Called when exiting ConfigMenu.
        /// </summary>
        public virtual void Unload() // This should be protected internal, but i'm afraid that'd break compatibility
        {
            this.myContainer.RemoveAllChildren();
            this.myContainer.RemoveFromContainer();
        }

        /// <summary>
        /// Hide this element
        /// </summary>
        public virtual void Hide()
        {
            this.myContainer.isVisible = false;
            if (!this.tab.isHidden) { this.hidden = true; }
        }

        /// <summary>
        /// Show this element
        /// </summary>
        public virtual void Show()
        {
            this.myContainer.isVisible = true;
            this.hidden = false;
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
        /// Multiplies deltaTime for tick multiplier
        /// </summary>
        public static float DTMultiply(float deltaTime) => 60.0f * deltaTime;
    }
}