using System;
using System.Collections.Generic;
using UnityEngine;
using CompletelyOptional;
using System.Linq;
using RWCustom;
using Menu;

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
    public class OpScrollBox : UIelement
    {
        private static readonly List<Camera> _cameras = new List<Camera>();

        /// <summary>
        /// The real position of the box's contents relative to the tab, a considerable distance off-screen.
        /// <para>This value is added to each child's pos automatically when they are added in <see cref="AddItems(UIelement[])"/>.</para>
        /// </summary>
        public Vector2 childOffset { get; private set; }

        /// <summary>
        /// A list of all items inside this scrollbox.
        /// </summary>
        public List<UIelement> children = new List<UIelement>();

        /// <summary>
        /// Whether or not the scrollbox scrolls horizontally.
        /// </summary>
        public readonly bool horizontal;

        /// <summary>
        /// The color of the border of the box behind this scrollbox.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// The color of the fill of the box behind this scrollbox.
        /// </summary>
        public Color colorFill;

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
        /// The height of the content inside this scrollbox.
        /// Used to calculate maximum scroll distance.
        /// </summary>
        public readonly float contentSize;

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
        /// Whether or not scrolling is currently locked. Use <see cref="Lock(bool)"/> and <see cref="Unlock"/> to manipulate.
        /// </summary>
        public bool ScrollLocked { get; private set; }

        /// <summary>
        /// Set to false to keep the mouse from interacting with this scrollbox's contents.
        /// </summary>
        public bool allowMouseOnContents = true;

        private bool _contentsDirty = true;
        private float _dirtyUntil = -1f;

        private readonly Camera _cam;
        private readonly int _camIndex;
        public readonly Vector2 camPos;
        public float scrollOffset { get; private set; }
        private float _scrollVel;
        private FTexture insideSprite;
        private RenderTexture _rt;
        private readonly DyeableRect _back, _slideBar;

        //private bool HasMouseFocus => ((_lockMouseFocus ?? MouseOver) && !_draggingSlider) && allowMouseOnContents;
        //private bool? _lockMouseFocus;
        private bool _draggingSlider = false;

        private float ScrollSize => horizontal ? size.x : size.y;

        /// <summary>
        /// Creates an empty scrollbox.
        /// </summary>
        /// <param name="pos">Bottom left position</param>
        /// <param name="size">Size of the visual box - for interior height see <see cref="contentSize"/></param>
        /// <param name="contentSize">The size of this box's contents (max 10000). Represents width if <see cref="horizontal"/> is true, height otherwise. See <see cref="contentSize"/></param>
        /// <param name="hasBack">Whether or not to create a box behind this scrollbox</param>
        /// <param name="hasSlideBar">Whether or not to create a slider at the right/bottom edge</param>
        public OpScrollBox(Vector2 pos, Vector2 size, float contentSize, bool horizontal = false, bool hasBack = true, bool hasSlideBar = true) : base(pos, size)
        {
            this._size.x = Mathf.Min(this._size.x, 800f); this._size.y = Mathf.Min(this._size.y, 800f);
            this.horizontal = horizontal;
            this.contentSize = Mathf.Clamp(contentSize, horizontal ? size.x : size.y, 10000f);
            this.isTab = false; this.hasScrolled = false;
            this.doesBackBump = true;

            this.bumpBehav = new BumpBehaviour(this);
            if (!_init) { return; }

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
            camPos = horizontal ? new Vector2(10000f, -10000f - 10300f * _camIndex) : new Vector2(10000f + 10300f * _camIndex, 10000f);

            childOffset = camPos - new Vector2(Mathf.Round(_offset.x), Mathf.Round(_offset.y));

            colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            colorFill = Color.black;
            if (hasBack)
            {
                _back = new DyeableRect(this.menu, this.owner, pos, size);
                subObjects.Add(_back);
                _back.color = colorEdge;
            }
            if (hasSlideBar)
            {
                scrollBumpBehav = new BumpBehaviour(this);
                _slideBar = new DyeableRect(this.menu, this.owner, pos, SliderSize);
                subObjects.Add(_slideBar);
                _slideBar.color = colorEdge;
                _slideBar.colorF = Color.Lerp(colorEdge, colorFill, 0.5f);
                _slideBar.fillAlpha = 0.5f;
            }

            this.ScrollToTop(true);
            UpdateCam();
            OnChange();
            this.GrafUpdate(0f);
        }

        /// <summary>
        /// Creates an empty scrollbox that fits <see cref="OpTab"/>. This constructor will call <see cref="OpTab.AddItems"/> automatically.
        /// </summary>
        /// <remarks>Example: <code>
        /// OpScrollBox sb = new OpScrollBox(Tabs[0], 2400f, false);
        /// sb.AddItems(new OpImage(new Vector2(420f, 1850f), "Futile_White"));
        /// </code></remarks>
        /// <param name="contentSize">The size of this box's contents (max 10000). Represents width if <see cref="horizontal"/> is true, height otherwise. See <see cref="contentSize"/></param>
        /// <param name="hasSlideBar">Whether or not to create a slider at the right/bottom edge</param>
        public OpScrollBox(OpTab tab, float contentSize, bool horizontal = false, bool hasSlideBar = true)
            : this(Vector2.zero, new Vector2(600f, 600f), contentSize, horizontal, false, hasSlideBar)
        {
            tab.AddItems(this); this.isTab = true;
            if (!_init) { return; }
            this._labelNotify = new MenuLabel(this.menu, this.owner,
                string.Concat(">>> ", InternalTranslator.Translate(hasSlideBar ? "Use Scroll Wheel or Scrollbar to see more" : "Use Scroll Wheel to see more"), " <<<"),
                new Vector2(200f, 0f) + UIelement._offset, new Vector2(200f, 20f), false);
            this.subObjects.Add(this._labelNotify);
        }

        private readonly bool isTab; private bool hasScrolled;
        private MenuLabel _labelNotify;

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
        /// Called after <see cref="Lock(bool)"/> to allow the user to scroll again.
        /// </summary>
        /// <seealso cref="Lock(bool)"/>
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

        private void UpdateSprite()
        {
            // Create an image to display the box's contents
            if (!_init) { return; }

            if (insideSprite == null)
            {
                insideSprite = new FTexture(_rt, "sb" + _camIndex) { anchorX = 0f, anchorY = 0f, x = 0f, y = 0f };
                this.myContainer.AddChild(insideSprite);
                return;
            }
            insideSprite.SetTexture(_rt);
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
                if (item.AddToScrollBox(this)) { this.tab.AddItems(item); children.Add(item); }
            }
        }

        private Vector2 SliderSize => horizontal ? new Vector2(Mathf.Max(Mathf.Min(size.x, size.x * size.x / contentSize), 20f), 15f) : new Vector2(15f, Mathf.Max(Mathf.Min(size.y, size.y * size.y / contentSize), 20f));
        private Vector2 SliderPos => horizontal ? new Vector2(-scrollOffset * size.x / contentSize, isTab ? -10f : 5f) : new Vector2((isTab ? 15f : 0f) + size.x - 20f, -scrollOffset * size.y / contentSize);

        private bool _firstUpdate = true;
        private float _dragOffset = 0f;
        private bool _lastMouseOver = false;
        private float _lastScrollOffset = 1f;

        public BumpBehaviour bumpBehav { get; private set; }
        private readonly BumpBehaviour scrollBumpBehav;
        private bool scrollMouseOver;

        private bool IsThereMouseOver()
        {
            foreach (UIelement e in this.tab.items)
            {
                if (e is UIconfig && !(e as UIconfig).disabled && e.MouseOver) { return true; }
            }
            return false;
        }

        public override void OnChange()
        {
            this._size.x = Mathf.Min(this._size.x, 800f); this._size.y = Mathf.Min(this._size.y, 800f);
            base.OnChange();
            UpdateCam();
        }

        public override void Update(float dt)
        {
            // Check redraw conditions
            if ((redrawFlags & RedrawEvents.Always) != 0)
                _contentsDirty = true;
            else if (((redrawFlags & RedrawEvents.OnHover) != 0) && (MouseOver || _lastMouseOver))
                // Continue animation for a small amount of time after the user moves the mouse away
                // ... so then elements such as buttons have time to complete their mouse-off animations
                MarkDirty(0.5f);
            else if (Time.unscaledTime <= _dirtyUntil)
                _contentsDirty = true;
            else if (((redrawFlags & RedrawEvents.OnKeypress) != 0) && Input.anyKey)
                MarkDirty(0.5f);
            _lastMouseOver = MouseOver;

            // Lock the mouse inside or outside the box if it was pressed down
            /*
            if (Input.GetMouseButtonDown(0))
                _lockMouseFocus = MouseOver;
            if (!Input.GetMouseButton(0))
                _lockMouseFocus = null; */

            bool hasMoved = false;

            // Do mouse drag scrolling
            scrollMouseOver = false;
            if (_draggingSlider)
            {
                if (ScrollLocked) { _draggingSlider = false; }
                else
                {
                    float scrollPerc = ((horizontal ? MousePos.x : MousePos.y) + _dragOffset) / ScrollSize;
                    scrollPerc *= -contentSize;
                    scrollOffset = scrollPerc;
                    targetScrollOffset = scrollOffset;
                    hasMoved = true;
                    if (!Input.GetMouseButton(0)) { _draggingSlider = false; }
                }
            }
            else if ((_slideBar != null) && !ScrollLocked && (MouseOver || isTab))
            {
                if (horizontal)
                {
                    if ((MousePos.y > SliderPos.y) && (MousePos.y < SliderPos.y + SliderSize.y))
                        if ((MousePos.x > SliderPos.x) && (MousePos.x < SliderPos.x + SliderSize.x))
                        {
                            scrollMouseOver = true;
                            if (Input.GetMouseButtonDown(0))
                            {
                                _dragOffset = SliderPos.x - MousePos.x;
                                _draggingSlider = true; this.hasScrolled = true;
                            }
                        }
                }
                else
                {
                    if ((MousePos.x > SliderPos.x) && (MousePos.x < SliderPos.x + SliderSize.x))
                        if ((MousePos.y > SliderPos.y) && (MousePos.y < SliderPos.y + SliderSize.y))
                        {
                            scrollMouseOver = true;
                            if (Input.GetMouseButtonDown(0))
                            {
                                _dragOffset = SliderPos.y - MousePos.y;
                                _draggingSlider = true; this.hasScrolled = true;
                            }
                        }
                }
            }

            // Do scroll wheel scrolling
            if (!ScrollLocked)
            {
                if (this.menu.mouseScrollWheelMovement != 0 && !IsThereMouseOver())
                {
                    if (MouseOver && !_draggingSlider)
                        targetScrollOffset -= (horizontal ? 40f : -40f) * Mathf.Sign(this.menu.mouseScrollWheelMovement);
                    if (targetScrollOffset != scrollOffset)
                    {
                        hasMoved = true; this.hasScrolled = true;
                        if (this.scrollBumpBehav != null)
                        {
                            this.scrollBumpBehav.flash = Mathf.Min(1f, this.scrollBumpBehav.flash + 0.2f);
                            this.scrollBumpBehav.sizeBump = Mathf.Min(2.5f, this.scrollBumpBehav.sizeBump + 0.3f);
                        }
                    }
                }
            }
            scrollOffset = Mathf.SmoothDamp(scrollOffset, targetScrollOffset, ref _scrollVel, 0.15f);
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

            base.Update(dt);

            // Redraw if this has scrolled
            if (scrollOffset != _lastScrollOffset)
            {
                _lastScrollOffset = scrollOffset;
                _contentsDirty = true;
            }

            _cam.enabled = _contentsDirty;
            _contentsDirty = false;
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (this.isHidden) { return; }
            this.bumpBehav.Update(dt);

            float sin = 1f;
            if (!this.hasScrolled && !this.ScrollLocked)
            {
                _notifySin += DTMultiply(dt);
                sin = 0.5f - 0.5f * Mathf.Sin(this._notifySin / 30f * 3.14159274f);
            }

            if (_back != null)
            {
                _back.colorF = this.colorFill;
                _back.color = doesBackBump ? this.bumpBehav.GetColor(this.colorEdge) : this.colorEdge;
                _back.fillAlpha = fillAlpha;
                _back.addSize = doesBackBump ? new Vector2(4f, 4f) * this.bumpBehav.AddSize : Vector2.zero;
                _back.addSize += new Vector2(2f, 2f);
                _back.pos = pos;
                _back.size = size;
            }
            if (_slideBar != null)
            {
                #region scrollBumpBehavUpdate

                this.scrollBumpBehav.flash = Custom.LerpAndTick(this.scrollBumpBehav.flash, 0f, 0.03f, 0.166666672f);
                if (this.scrollMouseOver)
                {
                    this.scrollBumpBehav.sizeBump = Custom.LerpAndTick(this.scrollBumpBehav.sizeBump, 1f, 0.1f, 0.1f);
                    this.scrollBumpBehav.sin += 1f;
                    if (!this.scrollBumpBehav.flashBool)
                    { this.scrollBumpBehav.flashBool = true; this.scrollBumpBehav.flash = 1f; }
                    this.scrollBumpBehav.col = Mathf.Min(1f, this.scrollBumpBehav.col + 0.1f);
                    this.scrollBumpBehav.extraSizeBump = Mathf.Min(1f, this.scrollBumpBehav.extraSizeBump + 0.1f);
                }
                else
                {
                    this.scrollBumpBehav.flashBool = false;
                    this.scrollBumpBehav.sizeBump = Custom.LerpAndTick(this.scrollBumpBehav.sizeBump, 0f, 0.1f, 0.05f);
                    this.scrollBumpBehav.col = Mathf.Max(0f, this.scrollBumpBehav.col - 0.0333333351f);
                    this.scrollBumpBehav.extraSizeBump = 0f;
                }
                this.scrollBumpBehav.greyedOut = this.ScrollLocked;

                #endregion scrollBumpBehavUpdate

                if (this._draggingSlider) { _slideBar.colorF = this.scrollBumpBehav.GetColor(this.colorEdge); _slideBar.fillAlpha = 1f; }
                else
                {
                    if (this.hasScrolled || this.ScrollLocked)
                    {
                        _slideBar.colorF = this.scrollBumpBehav.GetColor(this.colorFill);
                        _slideBar.fillAlpha = this.scrollBumpBehav.FillAlpha;
                    }
                    else
                    {
                        _slideBar.colorF = this.scrollBumpBehav.GetColor(this.colorEdge);
                        _slideBar.fillAlpha = 0.3f + 0.6f * sin;
                    }
                }
                _slideBar.color = this.scrollBumpBehav.GetColor(this.colorEdge);
                _slideBar.size = SliderSize;
                _slideBar.addSize = new Vector2(2f, 2f) * this.scrollBumpBehav.AddSize;
                _slideBar.pos = pos + SliderPos;
            }
            if (this._labelNotify != null)
            {
                if (this.ScrollLocked) { this._labelNotify.label.alpha = 0f; }
                else
                {
                    this._labelNotify.label.color = Color.Lerp(Color.white, this.scrollBumpBehav.GetColor(this.colorEdge), 0.5f);
                    if (!this.hasScrolled) { this._labelNotify.label.alpha = 0.5f + 0.5f * sin; }
                    else
                    {
                        this._labelNotify.label.alpha -= 0.03333f * DTMultiply(dt);
                        if (this._labelNotify.label.alpha < float.Epsilon)
                        {
                            this._labelNotify.RemoveSprites();
                            this.subObjects.Remove(this._labelNotify);
                            this._labelNotify = null;
                        }
                    }
                }
            }
        }

        private float _notifySin;

        public override void Hide()
        {
            base.Hide();
            _cam.gameObject.SetActive(false);
            if (_back != null) { this._back.Hide(); }
            if (_slideBar != null) { this._slideBar.Hide(); }
            if (_labelNotify != null) { this._labelNotify.label.isVisible = false; }
        }

        public override void Show()
        {
            base.Show();
            _cam.gameObject.SetActive(true);
            if (_back != null) { this._back.Show(); }
            if (_slideBar != null) { this._slideBar.Show(); }
            if (_labelNotify != null) { this._labelNotify.label.isVisible = true; }
        }

        public override void Unload()
        {
            base.Unload();
            if (_cam)
                UnityEngine.Object.Destroy(_cam.gameObject);
            if (insideSprite != null) { insideSprite.Destroy(); }
        }

        private void UpdateCam()
        {
            if (this.isHidden) { return; }
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
                UpdateSprite();
            }
        }

        private void MoveCam()
        {
            Vector3 v = (Vector3)owner.ScreenPos + (Vector3)camPos + new Vector3(size.x / 2f, size.y / 2f, -50f) + (horizontal ? Vector3.left : Vector3.down) * scrollOffset;
            _cam.gameObject.transform.position = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }
    }
}
