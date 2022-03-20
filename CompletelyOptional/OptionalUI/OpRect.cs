using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Simply Rounded Rectangle
    /// </summary>
    public class OpRect : UIelement
    {
        /// <summary>
        /// Create <see cref="DyeableRect"/> for Decoration
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="size">Size</param>
        /// <param name="alpha">0f ~ 1f (default: 0.3f)</param>
        public OpRect(Vector2 pos, Vector2 size, float alpha = 0.3f) : base(pos, size)
        {
            this.fillAlpha = alpha;
            this.doesBump = false;

            this.rect = new DyeableRect(this.myContainer, Vector2.zero, size, true)
            {
                colorEdge = this.colorEdge,
                colorFill = this.colorFill,
                fillAlpha = this.fillAlpha
            };
        }

        /// <summary>
        /// <see cref="DyeableRect"/> of this <see cref="UIelement"/>.
        /// </summary>
        public readonly DyeableRect rect;

        /// <summary>
        /// If you want this Rect to react with MouseOver, set this true.
        /// </summary>
        public bool doesBump;

        /// <summary>
        /// Edge Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is Black.
        /// </summary>
        public Color colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);

        /// <summary>
        /// fillAlpha of <see cref="DyeableRect"/>. (Ignored when <see cref="doesBump"/> is true)
        /// </summary>
        public float fillAlpha;

        /// <summary>
        /// When <see cref="doesBump"/> is true, this gets used.
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.rect.colorFill = this.colorFill;

            if (!doesBump)
            {
                this.rect.fillAlpha = this.fillAlpha;
                this.rect.colorEdge = this.colorEdge;

                this.rect.GrafUpdate(timeStacker);
                return;
            }
            if (this.bumpBehav == null) { this.bumpBehav = new BumpBehaviour(this); }

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);

            this.rect.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            this.rect.Update();
            this.bumpBehav?.Update();
        }

        public override void OnChange()
        {
            base.OnChange();
            this.rect.size = this.size;
        }
    }
}