using OptionalUI;

namespace CompletelyOptional
{
    /// <summary>
    /// <see cref="MenuTab"/> for MenuTabWrapper, which sends <see cref="UIfocusable.Signal"/> to vanilla <see cref="UIelementWrapper.Singal"/>
    /// </summary>
    public class WrappedMenuTab : MenuTab
    {
        internal WrappedMenuTab(MenuTabWrapper wrapper)
        {
            this.wrapper = wrapper;
            this.wrapper.Container.AddChild(this.container);
        }

        public readonly MenuTabWrapper wrapper;

        protected internal override void Signal(UIfocusable trigger, string signal)
        {
            wrapper.Signal(trigger, signal);
        }
    }
}