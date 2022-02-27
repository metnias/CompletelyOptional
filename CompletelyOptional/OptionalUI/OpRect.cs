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
            this.alpha = alpha;
            if (!_init) { return; }

            this.rect = new DyeableRect(menu, owner, this.pos, size, true);
            this.subObjects.Add(this.rect);

            colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            colorFill = Color.black;
            this.rect.color = colorEdge;
            this.rect.colorF = colorFill;

            this.doesBump = false;
        }

        /// <summary>
        /// <see cref="DyeableRect"/> of this <see cref="UIelement"/>.
        /// </summary>
        public DyeableRect rect;

        /// <summary>
        /// If you want this Rect to react with MouseOver, set this true.
        /// </summary>
        public bool doesBump;

        /// <summary>
        /// Edge Colour of <see cref="DyeableRect"/>. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Colour of <see cref="DyeableRect"/>. Default is Black.
        /// </summary>
        public Color colorFill;

        /// <summary>
        /// fillAlpha of <see cref="DyeableRect"/>. (Ignored when <see cref="doesBump"/> is true)
        /// </summary>
        public float alpha;

        /// <summary>
        /// When <see cref="doesBump"/> is true, this gets used.
        /// </summary>
        public BumpBehaviour bumpBehav { get; private set; }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            this.rect.colorF = this.colorFill;

            if (!doesBump)
            {
                this.rect.fillAlpha = this.alpha;
                this.rect.color = this.colorEdge;
                return;
            }
            if (this.bumpBehav == null) { this.bumpBehav = new BumpBehaviour(this); }
            this.bumpBehav.Update(dt);

            this.rect.fillAlpha = this.bumpBehav.FillAlpha;
            this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
            this.rect.color = this.bumpBehav.GetColor(this.colorEdge);
        }

        public override void Update(float dt)
        {
            if (!_init) { return; }
            base.Update(dt);
        }

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            this.rect.pos = this.pos;
            this.rect.size = this.size;
        }

        public override void Unload()
        {
            base.Unload();
            //this.subObjects.Remove(this.rect);
        }

        public override void Hide()
        {
            base.Hide();
            this.rect.Hide();
        }

        public override void Show()
        {
            base.Show();
            this.rect.Show();
        }
    }
}