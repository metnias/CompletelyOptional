using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Special kind of <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
    /// </summary>
    public abstract class UItrigger : UIelement, ICanBeFocused
    {
        #region Shallow

        /// <summary>
        /// Special kind of Rectangular <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size</param>
        /// <param name="signal">Non-exclusive signal key</param>
        public UItrigger(Vector2 pos, Vector2 size, string signal) : base(pos, size)
        {
            this.signal = signal;
            this._held = false;
            this.bumpBehav = new BumpBehaviour(this);
        }

        /// <summary>
        /// Special kind of Circular <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="rad">Radius</param>
        /// <param name="signal">Non-exclusive signal key</param>
        public UItrigger(Vector2 pos, float rad, string signal) : base(pos, rad)
        {
            this.signal = signal;
            this._held = false;
            this.bumpBehav = new BumpBehaviour(this);
        }

        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        /// <summary>
        /// Non-exclusive key for UItrigger
        /// </summary>
        public string signal;

        /// <summary>
        /// Whether this UItrigger is greyedOut or not
        /// </summary>
        public bool greyedOut = false;

        #endregion Shallow

        #region Deep

        public virtual bool CurrentlyFocusableMouse => !this.greyedOut;

        public virtual bool CurrentlyFocusableNonMouse => true;

        public virtual bool GreyedOut => greyedOut;

        public virtual Rect FocusRect
        {
            get
            {
                Rect res = new Rect(this.pos.x, this.pos.y, this.size.x, this.size.y);
                if (inScrollBox)
                {
                    Vector2 offset = scrollBox.camPos - (scrollBox.horizontal ? Vector2.right : Vector2.up) * scrollBox.scrollOffset - scrollBox.pos;
                    res.x += offset.x; res.y += offset.y;
                }
                return res;
            }
        }

        /// <summary>
        /// Whether this is held or not.
        /// If this is true, other <see cref="UIelement"/> will be frozen.
        /// </summary>
        protected internal bool held
        {
            get { return _held; }
            set
            {
                if (_held != value)
                {
                    _held = value;
                    if (value) { menu.cfgContainer.FocusNewElement(this); }
                    else if (menu.cfgContainer.focusedElement != this) { return; }
                    ConfigContainer.holdElement = value;
                }
            }
        }

        /// <summary>
        /// Calls <see cref="OptionInterface.Signal(UItrigger, string)"/>
        /// </summary>
        public virtual void Signal()
        {
            this.tab.owner.Signal(this, this.signal);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            this.bumpBehav.Update();
            if (this.held && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(); }
        }

        protected internal override void Deactivate()
        {
            this.held = false;
            base.Deactivate();
        }

        #endregion Deep

        #region Internal

        private bool _held;

        #endregion Internal
    }
}