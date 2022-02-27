using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Special kind of <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
    /// </summary>
    public abstract class UItrigger : UIelement, SelectableUIelement
    {
        /// <summary>
        /// Special kind of Rectangular <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
        /// </summary>
        /// <remarks>If using <see cref="UItrigger"/> causes crash, check if you are running <see cref="OptionInterface.Update(float)"/></remarks>
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
        /// <remarks>If using <see cref="UItrigger"/> causes crash, check if you are running <see cref="OptionInterface.Update(float)"/></remarks>
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
        /// Non-exclusive key for UItrigger
        /// </summary>
        public string signal;

        /// <summary>
        /// Whether this UItrigger is greyedOut or not
        /// </summary>
        public bool greyedOut = false;

        /// <summary>
        /// Whether this is held or not.
        /// If this is true, other <see cref="UIelement"/> will be frozen.
        /// </summary>
        public bool held
        {
            get { return _held; }
            set
            {
                if (_held != value)
                {
                    _held = value;
                    ConfigMenu.freezeMenu = value;
                }
            }
        }

        private bool _held;

        /// <summary>
        /// Mimics <see cref="Menu.ButtonBehavior"/> of vanilla Rain World UIs
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        /// <summary>
        /// Either this is <see cref="greyedOut"/> or <see cref="UIelement.isHidden"/>.
        /// Prevents its interaction in <see cref="Update(float)"/>.
        /// </summary>
        public bool disabled => this.greyedOut || this.isHidden;

        bool SelectableUIelement.IsMouseOverMe { get { return this.MouseOver; } }

        bool SelectableUIelement.CurrentlySelectableMouse { get { return !this.disabled; } }

        bool SelectableUIelement.CurrentlySelectableNonMouse { get { return !this.disabled; } }

        /// <summary>
        /// Calls <see cref="OptionInterface.Signal(UItrigger, string)"/>
        /// </summary>
        public virtual void Signal()
        {
            if (_init) { this.tab.owner.Signal(this, this.signal); }
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
        }

        public override void Update(float dt)
        {
            if (!_init) { return; }
            base.Update(dt);
            this.bumpBehav.Update(dt);

            if (this.held && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(dt); }
            if (showDesc && !this.greyedOut) { ConfigMenu.description = this.description; }
        }
    }
}