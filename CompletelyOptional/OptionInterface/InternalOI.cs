using BepInEx;
using OptionalUI;

namespace CompletelyOptional
{
    /// <summary>
    /// Default OI that's called when your mod does not support CompletelyOptional.
    /// Also shows the error in your OI.
    /// </summary>
    internal abstract class InternalOI : OptionInterface
    {
        public InternalOI(BaseUnityPlugin plugin, Reason type) : base(plugin)
        { this.reason = type; }

        public InternalOI(RainWorldMod rwMod, Reason type) : base(rwMod)
        { this.reason = type; }

        public Reason reason;

        public enum Reason
        {
            /// <summary>
            /// The mod has no OI.
            /// </summary>
            NoInterface,

            /// <summary>
            /// Error has occured.
            /// </summary>
            Error,

            /// <summary>
            /// There is no mod installed.
            /// </summary>
            NoMod,

            /// <summary>
            /// Statistics screen
            /// </summary>
            Statistics,

            /// <summary>
            /// Internal Test OI for UIelement testing
            /// </summary>
            TestOI
        }

        public override bool Configurable()
        {
            return false;
        }

        public override void Initialize()
        {
            Tabs = new OpTab[1];
            Tabs[0] = new OpTab(this);
        }

        protected OpLabel labelID, labelVersion, labelAuthor; //labelCoauthor labelDesc;
        protected OpLabel labelSluggo0, labelSluggo1;
    }
}