using OptionalUI;

namespace CompletelyOptional
{
    /// <summary>
    /// <see cref="MenuTab"/> for MenuTabWrapper
    /// </summary>
    public class WrappedMenuTab : MenuTab
    {
        internal WrappedMenuTab(MenuTabWrapper wrapper)
        {
            this.wrapper = wrapper;
            this.wrapper.Container.AddChild(this.container);
        }

        public readonly MenuTabWrapper wrapper;
    }
}