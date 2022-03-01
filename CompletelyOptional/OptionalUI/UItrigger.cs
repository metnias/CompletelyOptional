using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Special kind of <see cref="UIelement"/> that can trigger <see cref="OptionInterface.Signal(UItrigger, string)"/>
    /// </summary>
    public abstract class UItrigger : UIelement, FocusableUIelement
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
        private bool _greyedOut = false;

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
        /// Either this is <see cref="FocusableUIelement.GreyedOut"/> or <see cref="UIelement.isInactive"/>.
        /// Prevents its interaction in <see cref="Update()"/>.
        /// </summary>
        public bool disabled => this._greyedOut || this.isInactive;

        bool FocusableUIelement.CurrentlyFocusableMouse => !this.disabled;

        bool FocusableUIelement.CurrentlyFocusableNonMouse => true;

        bool FocusableUIelement.Focused { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        bool FocusableUIelement.GreyedOut { get => _greyedOut; }

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
            this.bumpBehav.Update(timeStacker);
        }

        public override void Update()
        {
            base.Update();

            if (this.held && this.inScrollBox) { this.scrollBox.MarkDirty(0.5f); this.scrollBox.Update(); }
            if (showDesc && !this._greyedOut) { ConfigMenu.description = this.description; }
        }
    }
}