using CompletelyOptional;
using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// A box that contains other UI elements to be scrolled through vertically. See remarks for perfomance considerations.
    /// <para><see cref="OpScrollBox"/> is contribution of Slimed_Cubed.</para>
    /// </summary>
    /// <remarks>
    /// Before adding objects to an <see cref="OpScrollBox"/> it must first be added to a tab.
    /// <para>Each scrollbox owns a <see cref="Camera"/> that (normally) renders each frame. This could cause performance issues with many elements in many scrollboxes.
    /// Consider setting <see cref="redrawFlags"/> to <see cref="RedrawEvents.OnHover"/> to improve performance. Only applies if no child elements are animated unless the mouse is over them.
    /// If the contents are to be completely static, consider setting <see cref="redrawFlags"/> to <see cref="RedrawEvents.Never"/>.
    /// The contents will still be redrawn on scroll or when <see cref="MarkDirty()"/> is called.</para>
    /// <para>Adding instances of <see cref="OpScrollBox"/> as children will not function correctly.</para>
    /// </remarks>
    public class OpScrollBox : UIelement, ICanBeFocused
    {
        private static readonly List<Camera> _cameras = new List<Camera>();

        /// <summary>
        /// The real position of the box's contents relative to the tab, a considerable distance off-screen.
        /// <para>This value is added to each child's pos automatically when they are added in <see cref="AddItems(UIelement[])"/>.</para>
        /// </summary>
        protected internal readonly Vector2 childOffset;

        /// <summary>
        /// A list of all items inside this scrollbox.
        /// </summary>
        public List<UIelement> children { get; protected internal set; } = new List<UIelement>();

        /// <summary>
        /// Whether or not the scrollbox scrolls horizontally.
        /// </summary>
        public readonly bool horizontal;

        /// <summary>
        /// The color of the border of the box behind this scrollbox.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// The color of the fill of the box behind this scrollbox.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        /// <summary>
        /// The alpha of the fill of the box behind this scrollbox.
        /// </summary>
        public float fillAlpha = 0.3f;

        /// <summary>
        /// The visual offset, in pixels, of the contents of this scrollbox. Always equal to or less than zero.
        /// </summary>
        public float ScrollOffset
        {
            get => scrollOffset;
            set
            {
                _scrollVel = 0f;
                scrollOffset = targetScrollOffset = value;
            }
        }

        /// <summary>
        /// Returns the value of <see cref="ScrollOffset"/> at the topmost or rightmost position of the box.
        /// This value will always be negative or zero.
        /// </summary>
        public float MaxScroll => -Mathf.Max(contentSize - size.y, 0f);

        /// <summary>
        /// The target value of <see cref="ScrollOffset"/>. Change this to smoothly animate scrolling.
        /// </summary>
        public float targetScrollOffset;

        /// <summary>
        /// The height/width of the content inside this scrollbox.
        /// To change this, use <see cref="SetContentSize"/>.
        /// </summary>
        public float contentSize { get; private set; }

        /// <summary>
        /// Resize contentSize of OpScrollBox
        /// </summary>
        /// <param name="newSize">New contentSize (the max is 10000f)</param>
        /// <param name="sortToTop">Whether to sort children to top/left or bottom/right</param>
        /// <returns>Whether this call is valid or not</returns>
        public bool SetContentSize(float newSize, bool sortToTop = true)
        {
            float ns = Mathf.Clamp(newSize, horizontal ? size.x : size.y, MaxContentSize);
            if (Mathf.Approximately(contentSize, ns)) { return false; }
            if (sortToTop)
            {
                float ofs = ns - contentSize;
                foreach (UIelement item in children) // Move all stuff
                { item.SetPos(item.GetPos() + (horizontal ? new Vector2(ofs, 0f) : new Vector2(0f, ofs))); }
                contentSize = ns;
                targetScrollOffset += ofs;
                scrollOffset = targetScrollOffset;
            }
            else { contentSize = ns; }
            hasScrolled = false; // Add flashing to let user know there's a change
            return true;
        }

        /// <summary>
        /// Indicates when the contents of a scrollbox should be redrawn.
        /// </summary>
        /// <seealso cref="redrawFlags"/>
        [Flags]
        public enum RedrawEvents : short
        {
            /// <summary>Only redraws the scrollbox when scrolled. Use <see cref="MarkDirty()"/> to redraw manually.</summary>
            Never = 0x0,

            /// <summary>Redraws the scrollbox every frame.</summary>
            Always = 0x1,

            /// <summary>Redraws the scrollbox when the mouse is hovered over it. Will continue to redraw for 0.5 seconds afterwards.</summary>
            OnHover = 0x2,

            /// <summary>Redraws the scrollbox as long as a key is held down. Will continue to redraw for 0.5 seconds afterwards.</summary>
            OnKeypress = 0x4
        }

        /// <summary>
        /// Indicates when this scrollbox should be redrawn. Defaults to <see cref="RedrawEvents.Always"/>.
        /// </summary>
        /// <seealso cref="RedrawEvents"/>
        public RedrawEvents redrawFlags = RedrawEvents.Always;

        /// <summary>
        /// Whether or not scrolling is currently locked. Use <see cref="Lock"/> and <see cref="Unlock"/> to manipulate.
        /// </summary>
        public bool ScrollLocked { get; private set; }

        /// Set to false to keep the mouse from interacting with this scrollbox's contents.
        // public bool allowMouseOnContents = true;

        private bool _contentsDirty = true;
        private float _dirtyUntil = -1f;

        private readonly Camera _cam;
        private readonly int _camIndex;
        protected internal readonly Vector2 camPos;
        public float scrollOffset { get; protected set; }
        private float _scrollVel;
        protected FTexture insideTexture;
        protected RenderTexture _rt;
        protected readonly DyeableRect rectBack, rectSlidebar;

        //private bool HasMouseFocus => ((_lockMouseFocus ?? MouseOver) && !_draggingSlider) && allowMouseOnContents;
        //private bool? _lockMouseFocus;
        private bool _draggingSlider = false;

        protected float ScrollSize => horizontal ? size.x : size.y;

        private const float MaxContentSize = 10000f;

        /// <summary>
        /// Creates an empty scrollbox.
        /// </summary>
        /// <param name="pos">Bottom left position</param>
        /// <param name="size">Size of the visual box - for interior height see <see cref="contentSize"/></param>
        /// <param name="contentSize">The size of this box's contents (max 10000f). Represents width if <see cref="horizontal"/> is true, height otherwise. See <see cref="contentSize"/></param>
        /// <param name="hasBack">Whether or not to create a box behind this scrollbox</param>
        /// <param name="hasSlideBar">Whether or not to create a slider at the right/bottom edge</param>
        public OpScrollBox(Vector2 pos, Vector2 size, float contentSize, bool horizontal = false, bool hasBack = true, bool hasSlideBar = true) : base(pos, size)
        {
            this._size.x = Mathf.Min(this._size.x, 800f); this._size.y = Mathf.Min(this._size.y, 800f);
            this.horizontal = horizontal;
            this.contentSize = Mathf.Clamp(contentSize, horizontal ? size.x : size.y, MaxContentSize);
            this.isTab = false; this.hasScrolled = false;
            this.doesBackBump = true;

            this.bumpBehav = new BumpBehaviour(this);

            // Create a camera for this scrollbox
            GameObject camObj = new GameObject("OpScrollBox Camera " + _camIndex);
            _cam = camObj.AddComponent<Camera>();

            // Find a place in the camera list for it
            _camIndex = -1;
            for (int i = 0, top = _cameras.Count; i < top; i++)
            {
                if (_cameras[i] == null)
                {
                    _camIndex = i;
                    _cameras[i] = _cam;
                    break;
                }
            }
            if (_camIndex == -1)
            {
                _cameras.Add(_cam);
                _camIndex = _cameras.Count - 1;
            }

            // Camera position must be different for horizontal vs vertical scrollboxes, otherwise contents may overlap
            // Assumes that content width is below 10000 pxls.
            // 300 pixels are added as a safety, in case some elements are slightly off screen
            camPos = horizontal ? new Vector2(MaxContentSize, -MaxContentSize - (MaxContentSize + 300f) * _camIndex) : new Vector2(MaxContentSize + (MaxContentSize + 300f) * _camIndex, MaxContentSize);

            childOffset = camPos; // - new Vector2(Mathf.Round(_offset.x), Mathf.Round(_offset.y));

            if (hasBack)
            {
                rectBack = new DyeableRect(this.myContainer, Vector2.zero, size);
                rectBack.colorEdge = colorEdge;
            }
            if (hasSlideBar)
            {
                bumpSlidebar = new BumpBehaviour(this);
                rectSlidebar = new DyeableRect(this.myContainer, Vector2.zero, SliderSize);
                rectSlidebar.colorEdge = colorEdge;
                rectSlidebar.colorFill = Color.Lerp(colorEdge, colorFill, 0.5f);
                rectSlidebar.fillAlpha = 0.5f;
            }

            this.ScrollToTop(true);
            OnChange();
            this.GrafUpdate(0f);
        }

        /// <summary>
        /// Creates an empty scrollbox that fits <see cref="OpTab"/>. This constructor will call <see cref="OpTab.AddItems"/> automatically to add itself to its tab.
        /// </summary>
        /// <remarks>Example: <code>
        /// OpScrollBox sb = new OpScrollBox(Tabs[0], 2400f, false);
        /// sb.AddItems(new OpImage(new Vector2(420f, 1850f), "Futile_White"));
        /// </code></remarks>
        /// <param name="contentSize">The size of this box's contents (max 10000f). Represents width if <see cref="horizontal"/> is true, height otherwise. See <see cref="contentSize"/></param>
        /// <param name="hasSlideBar">Whether or not to create a slider at the right/bottom edge</param>
        public OpScrollBox(OpTab tab, float contentSize, bool horizontal = false, bool hasSlideBar = true)
            : this(Vector2.zero, new Vector2(600f, 600f), contentSize, horizontal, false, hasSlideBar)
        {
            tab.AddItems(this); this.isTab = true;

            this._labelNotify = FLabelCreate(string.Concat(">>> ", InternalTranslator.Translate(hasSlideBar ? "Use Scroll Wheel or Scrollbar to see more" : "Use Scroll Wheel to see more"), " <<<"));
            FLabelPlaceAtCenter(this._labelNotify, 200f, 0f, 200f, 20f);
            this.myContainer.AddChild(this._labelNotify);
        }

        private readonly bool isTab; private bool hasScrolled;
        private FLabel _labelNotify;

        /// <summary>
        /// if set to false, BackRectangular does not react with mouse over.
        /// </summary>
        public bool doesBackBump;

        /// <summary>
        /// Causes the contents of this scrollbox to be redrawn at the end of this frame.
        /// </summary>
        /// <seealso cref="MarkDirty(float)"/>
        public void MarkDirty()
        {
            _contentsDirty = true;
        }

        /// <summary>
        /// Redraws the scrollbox every frame for the given time.
        /// </summary>
        /// <param name="time">Length of time to redraw for, in seconds</param>
        /// <seealso cref="MarkDirty()"/>
        public void MarkDirty(float time)
        {
            MarkDirty();
            _dirtyUntil = Mathf.Max(_dirtyUntil, Time.unscaledTime + time);
        }

        /// <summary>
        /// Stops the user from scrolling this scrollbox. <see cref="targetScrollOffset"/> and <see cref="ScrollOffset"/> will function even if locked.
        /// </summary>
        /// <seealso cref="Unlock"/>
        /// <seealso cref="ScrollLocked"/>
        /// <param name="stopImmediately">If true the scrollbox stops immediately, otherwise it proceeds to <see cref="targetScrollOffset"/> before stopping.</param>
        public void Lock(bool stopImmediately)
        {
            ScrollLocked = true;
            if (stopImmediately)
            {
                targetScrollOffset = scrollOffset;
                _scrollVel = 0f;
            }
        }

        /// <summary>
        /// Called after <see cref="Lock"/> to allow the user to scroll again.
        /// </summary>
        /// <seealso cref="Lock"/>
        /// <seealso cref="ScrollLocked"/>
        public void Unlock()
        {
            ScrollLocked = false;
        }

        /// <summary>
        /// Immediately scrolls to the top of the box's content.
        /// If <see cref="horizontal"/> is <c>true</c>, it scrolls to the leftmost.
        /// </summary>
        /// <param name="immediate">If <c>true</c> the scrolling animation will be skipped</param>
        public void ScrollToTop(bool immediate = true)
        {
            targetScrollOffset = horizontal ? 0f : MaxScroll;
            if (immediate)
                scrollOffset = targetScrollOffset;
        }

        /// <summary>
        /// Immediately scrolls to the bottom of the box's content.
        /// If <see cref="horizontal"/> is <c>true</c>, it scrolls to the rightmost.
        /// </summary>
        /// <param name="immediate">If <c>true</c> the scrolling animation will be skipped</param>
        public void ScrollToBottom(bool immediate = true)
        {
            targetScrollOffset = horizontal ? MaxScroll : 0f;
            if (immediate)
                scrollOffset = targetScrollOffset;
        }

        public static void ScrollToChild(UIelement child)
        {
            if (!child.inScrollBox) { return; }
            OpScrollBox box = child.scrollBox;
            float gap = 10f;
            if (box.horizontal)
            { // lower left
                float target = child.pos.x;
                if (target < box.scrollOffset) { box.targetScrollOffset = target - gap; }
                else
                {
                    target += child is ICanBeFocused fChild ? fChild.FocusRect.width : child.size.x;
                    if (target > box.scrollOffset + box.size.x) { box.targetScrollOffset = target + gap; }
                }
            }
            else
            { // higher top
                float target = child.pos.y;
                if (target < box.scrollOffset) { box.targetScrollOffset = target - gap; }
                else
                {
                    target += child is ICanBeFocused fChild ? fChild.FocusRect.height : child.size.y;
                    if (target > box.scrollOffset + box.size.x) { box.targetScrollOffset = target + gap; }
                }
            }
        }

        /// <summary>
        /// UIelements that are forbidden to for <see cref="AddItems(UIelement[])"/>.
        /// </summary>
        public static readonly Type[] ChildBlacklist = new Type[]
        {
            typeof(OpScrollBox) //, typeof(OpSpriteEditor), typeof(OpPathSelector)
        };

        /// <summary>
        /// Adds a collection of items to the interior of this scrollbox.
        /// <para>The <see cref="OpScrollBox"/> instance must be added to a tab before calling this method.
        /// And for children, call <see cref="OpScrollBox.AddItems(UIelement[])"/> instead of <see cref="OpTab.AddItems(UIelement[])"/></para>
        /// </summary>
        /// <remarks>Example: <c>
        /// myScrollBox.AddItems(new OpLabel(50f, 50f, "Example"));
        /// </c></remarks>
        /// <exception cref="InvalidOperationException">This <see cref="OpScrollBox"/> was not added to an <see cref="OpTab"/> before adding children.</exception>
        /// <param name="items">Items to add</param>
        public void AddItems(params UIelement[] items)
        {
            if (this.tab == null) throw new InvalidOperationException("OpScrollBox must be added to an OpTab before items are added.");

            foreach (UIelement item in items)
            {
                if (item.AddToScrollBox(this))
                {
                    this.tab.AddItems(item);
                    children.Add(item);
                    if (lastFocusedElement == null && item is ICanBeFocused) { lastFocusedElement = item; }
                }
            }
        }

        protected internal UIelement lastFocusedElement = null;

        /// <summary>
        /// Remove UIelements from OpScrollbox they're in
        /// </summary>
        /// <param name="items">Items to remove from its <see cref="OpScrollBox"/></param>
        public static void RemoveItemsFromScrollBox(params UIelement[] items)
        {
            foreach (UIelement item in items)
            {
                if (!item.inScrollBox) { continue; }
                item.scrollBox.children.Remove(item);
                item.RemoveFromScrollBox();
            }
        }

        private Vector2 SliderSize => horizontal
            ? new Vector2(Mathf.Max(Mathf.Min(size.x, size.x * size.x / contentSize), 20f), 15f)
            : new Vector2(15f, Mathf.Max(Mathf.Min(size.y, size.y * size.y / contentSize), 20f));

        private Vector2 SliderPos => horizontal
            ? new Vector2(-scrollOffset * size.x / contentSize, isTab ? -10f : 5f)
            : new Vector2((isTab ? 15f : 0f) + size.x - 20f, -scrollOffset * size.y / contentSize);

        private bool _firstUpdate = true;
        private float _dragOffset = 0f;
        private bool _lastMouseOver = false, _lastMouseDown = false;
        private float _lastScrollOffset = 1f;

        public BumpBehaviour bumpBehav { get; private set; }

        bool ICanBeFocused.GreyedOut => false;

        Rect ICanBeFocused.FocusRect
        {
            get
            {
                Rect res = new Rect(this.ScreenPos.x, this.ScreenPos.y, this.size.x, this.size.y);
                if (tab != null) { res.x += tab.container.x; res.y += tab.container.y; }
                return res;
            }
        }

        bool ICanBeFocused.CurrentlyFocusableMouse => true;

        bool ICanBeFocused.CurrentlyFocusableNonMouse => true;

        private readonly BumpBehaviour bumpSlidebar;
        private bool scrollMouseOver;

        private bool IsThereChildMouseOver()
        {
            foreach (UIelement e in this.children)
            {
                if (e.isInactive) { continue; }
                if (e is ICanBeFocused && (e as ICanBeFocused).CurrentlyFocusableMouse && e.MouseOver) { return true; }
            }
            return false;
        }

        public override void OnChange()
        {
            this._size.x = Mathf.Min(this._size.x, 800f); this._size.y = Mathf.Min(this._size.y, 800f);
            base.OnChange();
            UpdateCam();
        }

        protected internal override bool MouseOver
        {
            get
            {
                if (!isTab) { return base.MouseOver; }
                return this.MousePos.x > -15f && this.MousePos.x < 615f && this.MousePos.y > -15f && this.MousePos.y < 615f;
            }
        }

        private bool _held = false;

        void ICanBeFocused.NonMouseHold()
        {
            ConfigContainer.instance.FocusNewElement(this);
            ConfigContainer.holdElement = true;
            _held = true;
        }

        public override void Update()
        {
            // Todo: scrollbox focus > uielement lastfocused

            rectBack?.Update(); rectSlidebar?.Update();
            this.bumpBehav.Update();

            #region CheckRedraw

            // Check redraw conditions
            if ((redrawFlags & RedrawEvents.Always) != 0)
            { _contentsDirty = true; }
            else if (((redrawFlags & RedrawEvents.OnHover) != 0) && (MouseOver || _lastMouseOver))
            {
                // Continue animation for a small amount of time after the user moves the mouse away
                // ... so then elements such as buttons have time to complete their mouse-off animations
                MarkDirty(0.5f);
            }
            else if (Time.unscaledTime <= _dirtyUntil)
            { _contentsDirty = true; }
            else if (((redrawFlags & RedrawEvents.OnKeypress) != 0) && Input.anyKey)
            { MarkDirty(0.5f); }
            #endregion CheckRedraw

            bool hasMoved = false;

            if (MenuMouseMode)
            {
                _lastMouseOver = MouseOver;

                // Lock the mouse inside or outside the box if it was pressed down
                /*
                if (Input.GetMouseButtonDown(0))
                    _lockMouseFocus = MouseOver;
                if (!Input.GetMouseButton(0))
                    _lockMouseFocus = null; */

                // Do mouse drag slider scrolling
                scrollMouseOver = false;
                if (_draggingSlider)
                {
                    if (ScrollLocked) { _draggingSlider = false; ConfigContainer.holdElement = false; }
                    else
                    {
                        float scrollPerc = ((horizontal ? MousePos.x : MousePos.y) + _dragOffset) / ScrollSize;
                        scrollPerc *= -contentSize;
                        scrollOffset = scrollPerc;
                        targetScrollOffset = scrollOffset;
                        hasMoved = true;
                        if (!Input.GetMouseButton(0)) { _draggingSlider = false; ConfigContainer.holdElement = false; }
                    }
                }
                else if ((rectSlidebar != null) && !ScrollLocked)
                {
                    if (MousePos.x > SliderPos.x && MousePos.x < SliderPos.x + SliderSize.x
                        && MousePos.y > SliderPos.y && MousePos.y < SliderPos.y + SliderSize.y)
                    {
                        scrollMouseOver = true;
                        if (Input.GetMouseButton(0) && !_lastMouseDown)
                        {
                            _dragOffset = horizontal ? SliderPos.x - MousePos.x : SliderPos.y - MousePos.y;
                            _draggingSlider = true; this.hasScrolled = true; ConfigContainer.holdElement = true;
                        }
                    }
                }
                _lastMouseDown = Input.GetMouseButton(0);

                // Do mouse scrollwheel scrolling
                if (!ScrollLocked)
                {
                    if (this.menu.mouseScrollWheelMovement != 0 && MouseOver)
                    {
                        if (!IsThereChildMouseOver() && !_draggingSlider)
                        { targetScrollOffset -= (horizontal ? 40f : -40f) * this.menu.mouseScrollWheelMovement; }
                        if (targetScrollOffset != scrollOffset)
                        {
                            hasMoved = true; this.hasScrolled = true;
                            PlaySound(SoundID.MENU_Scroll_Tick);
                            if (this.bumpSlidebar != null)
                            {
                                this.bumpSlidebar.flash = Mathf.Min(1f, this.bumpSlidebar.flash + 0.2f);
                                this.bumpSlidebar.sizeBump = Mathf.Min(2.5f, this.bumpSlidebar.sizeBump + 0.3f);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_held)
                {
                    if (CtlrInput.jmp && !LastCtlrInput.jmp)
                    { // Focused > (jmp) > !ScrollLocked ? holdElement : focus <- lastFocusedElement
                        if (lastFocusedElement != null)
                        {
                            if (!lastFocusedElement.isInactive && !(lastFocusedElement as ICanBeFocused).GreyedOut)
                            {
                                // Check if this is in view

                                // Use LastFocusedElement
                                this._held = false;
                                ConfigContainer.holdElement = false;
                                ConfigContainer.instance.FocusNewElement(lastFocusedElement);
                                goto skipNewFocus;
                            }
                        }
                        // Find New Focus
                        foreach (UIelement child in this.children)
                        {
                            if (!child.isInactive && child is ICanBeFocused candidate && !candidate.GreyedOut)
                            {
                            }
                        }
                        // if no candidate is found, just play dull sound
                    }
                    else if (!ScrollLocked)
                    {
                        if ((horizontal && CtlrInput.x != 0) || (!horizontal && CtlrInput.y != 0))
                        {
                            // Add input delay here
                            if (horizontal) { targetScrollOffset -= 40f * Mathf.Sign(CtlrInput.x); }
                            else { targetScrollOffset += 40f * Mathf.Sign(CtlrInput.y); }
                            if (targetScrollOffset != scrollOffset)
                            {
                                hasMoved = true; this.hasScrolled = true;
                                PlaySound(SoundID.MENU_Scroll_Tick);
                                if (this.bumpSlidebar != null)
                                {
                                    this.bumpSlidebar.flash = Mathf.Min(1f, this.bumpSlidebar.flash + 0.2f);
                                    this.bumpSlidebar.sizeBump = Mathf.Min(2.5f, this.bumpSlidebar.sizeBump + 0.3f);
                                }
                            }
                        }
                    }
                }
                else // if (ConfigContainer.focusedElement?.inScrollBox == true && ConfigContainer.focusedElement.scrollBox == this) // Child Focused
                {
                }
            }
        skipNewFocus:

            scrollOffset = Mathf.SmoothDamp(scrollOffset, targetScrollOffset, ref _scrollVel, 0.15f * frameMulti);
            // Snap the scrollbox to its target when it is within 0.5 of a pixel
            // This is to keep from rendering the box's contents each frame even when the difference in scroll is not visible
            if (Mathf.Abs(scrollOffset - targetScrollOffset) < 0.5f)
            {
                scrollOffset = targetScrollOffset;
                _scrollVel = 0f;
            }

            // Don't allow overscroll
            targetScrollOffset = Mathf.Clamp(targetScrollOffset, -Mathf.Max(contentSize - ScrollSize, 0f), 0f);
            scrollOffset = Mathf.Clamp(scrollOffset, -Mathf.Max(contentSize - ScrollSize, 0f), 0f);
            MoveCam();

            if (hasMoved || _firstUpdate)
            {
                _firstUpdate = false;
                this.OnChange();
            }

            base.Update();

            // Redraw if this has scrolled
            if (scrollOffset != _lastScrollOffset)
            {
                _lastScrollOffset = scrollOffset;
                _contentsDirty = true;
            }

            _cam.enabled = _contentsDirty;
            _contentsDirty = false;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            float sin = 1f;
            if (!this.hasScrolled && !this.ScrollLocked)
            {
                _notifySin += 1f; // Move to Update
                sin = 0.5f - 0.5f * Mathf.Sin(this._notifySin / 30f * 3.14159274f);
            }

            if (rectBack != null)
            {
                rectBack.GrafUpdate(timeStacker);
                rectBack.colorFill = this.colorFill;
                rectBack.colorEdge = doesBackBump ? this.bumpBehav.GetColor(this.colorEdge) : this.colorEdge;
                rectBack.fillAlpha = fillAlpha;
                rectBack.addSize = doesBackBump ? new Vector2(4f, 4f) * this.bumpBehav.AddSize : Vector2.zero;
                rectBack.addSize += new Vector2(2f, 2f);
                rectBack.size = size;
            }
            if (rectSlidebar != null)
            {
                this.bumpSlidebar.MouseOver = this.scrollMouseOver;
                this.bumpSlidebar.greyedOut = this.ScrollLocked;
                this.bumpSlidebar.Update();

                if (this._draggingSlider) { rectSlidebar.colorFill = this.bumpSlidebar.GetColor(this.colorEdge); rectSlidebar.fillAlpha = 1f; }
                else
                {
                    if (this.hasScrolled || this.ScrollLocked)
                    {
                        rectSlidebar.colorFill = this.bumpSlidebar.GetColor(this.colorFill);
                        rectSlidebar.fillAlpha = this.bumpSlidebar.FillAlpha;
                    }
                    else
                    {
                        rectSlidebar.colorFill = this.bumpSlidebar.GetColor(this.colorEdge);
                        rectSlidebar.fillAlpha = 0.3f + 0.6f * sin;
                    }
                }
                rectSlidebar.colorEdge = this.bumpSlidebar.GetColor(this.colorEdge);
                rectSlidebar.size = SliderSize;
                rectSlidebar.addSize = new Vector2(2f, 2f) * this.bumpSlidebar.AddSize;
                rectSlidebar.pos = SliderPos;
                rectSlidebar.GrafUpdate(timeStacker);
            }
            if (this._labelNotify != null)
            {
                if (this.ScrollLocked) { this._labelNotify.alpha = 0f; }
                else
                {
                    this._labelNotify.color = Color.Lerp(Color.white, this.bumpSlidebar.GetColor(this.colorEdge), 0.5f);
                    if (!this.hasScrolled) { this._labelNotify.alpha = 0.5f + 0.5f * sin; }
                    else
                    {
                        this._labelNotify.alpha -= 0.03333f / frameMulti; // move to Update
                        if (this._labelNotify.alpha < float.Epsilon)
                        {
                            this._labelNotify.isVisible = false;
                            this._labelNotify.RemoveFromContainer();
                            this._labelNotify = null;
                        }
                    }
                }
            }
        }

        private float _notifySin;

        protected internal override void Deactivate()
        {
            base.Deactivate();
            foreach (UIelement child in children) { child.Deactivate(); }
            _cam.gameObject.SetActive(false);
        }

        protected internal override void Reactivate()
        {
            base.Reactivate();
            foreach (UIelement child in children) { child.Reactivate(); }
            _cam.gameObject.SetActive(true);
        }

        protected internal override void Unload()
        {
            base.Unload();
            if (_cam) { UnityEngine.Object.Destroy(_cam.gameObject); }
            if (insideTexture != null) { insideTexture.Destroy(); }
        }

        private void UpdateCam()
        {
            if (this.isInactive) { return; }
            _cam.aspect = size.x / size.y;
            _cam.orthographic = true;
            _cam.orthographicSize = size.y / 2f;
            _cam.nearClipPlane = 1f;
            _cam.farClipPlane = 100f;
            MoveCam();

            // Render before the main camera
            _cam.depth = -1000;

            // Aquire a render texture
            if ((_rt == null) || (_rt.width != size.x) || (_rt.height != size.y))
            {
                _rt?.Release();
                _rt = new RenderTexture((int)size.x, (int)size.y, 8)
                { filterMode = FilterMode.Point };

                _cam.targetTexture = _rt;

                #region UpdateTexture

                // Create an image to display the box's contents
                if (insideTexture == null)
                {
                    insideTexture = new FTexture(_rt, "sb" + _camIndex) { anchorX = 0f, anchorY = 0f, x = 0f, y = 0f };
                    this.myContainer.AddChild(insideTexture);
                }
                else { insideTexture.SetTexture(_rt); }

                #endregion UpdateTexture
            }
        }

        private void MoveCam()
        {
            Vector3 v = (Vector3)owner.ScreenPos + (Vector3)camPos + new Vector3(size.x / 2f, size.y / 2f, -50f) + (horizontal ? Vector3.left : Vector3.down) * scrollOffset;
            if (this.tab != null) { v += (Vector3)tab.container.GetPosition(); }
            _cam.gameObject.transform.position = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }
    }
}