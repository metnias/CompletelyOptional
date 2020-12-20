using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Accessible <see cref="FContainer"/> in Config screen which you can add <see cref="FSprite"/>s on your own.
    /// <para>See <see cref="container"/>.</para>
    /// </summary>
    public class OpContainer : UIelement
    {
        /// <summary>
        /// Accessible <see cref="FContainer"/> in Config screen which you can add <see cref="FSprite"/>s on your own.
        /// <para>See <see cref="container"/>.</para>
        /// </summary>
        /// <param name="pos">Origin point (BottomLeft) of <see cref="container"/></param>
        public OpContainer(Vector2 pos) : base(pos, Vector2.one)
        {
            if (!_init) { this.myContainer = new FContainer(); } // prevents NullRef even when it's accessed in wrong time
        }

        /// <summary>
        /// Accessible <see cref="FContainer"/> which you can add <see cref="FSprite"/>s on your own
        /// </summary>
        public FContainer container => myContainer;
    }
}
