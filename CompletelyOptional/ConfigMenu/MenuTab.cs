using OptionalUI;

namespace CompletelyOptional
{
    /// <summary>
    /// Special kind of OpTab for Menu. See also <seealso cref="MenuTabWrapper"/>
    /// </summary>
    public class MenuTab : OpTab
    {
        internal MenuTab() : base(null, "")
        {
            // 171 offset from left
            this.container.x -= _offset.x; this.container.y -= _offset.y; this.container.isVisible = true;
            isInactive = false;
        }

        protected internal FContainer myContainer => container;

        protected internal override void Signal(UItrigger trigger, string signal)
        {
        }
    }
}