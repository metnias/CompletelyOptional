using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// A group of FSprite in the shape of RoundedRect that can be coloured easily.
    /// </summary>
    public class DyeableRect
    {
        /// <summary>
        /// A group of FSprite in the shape of RoundedRect that can be coloured easily.
        /// <para>Add <see cref="Update"/> and <see cref="GrafUpdate(float)"/>
        /// in your custom <see cref="UIelement.Update"/> and <see cref="UIelement.GrafUpdate(float)"/> when using this.</para>
        /// </summary>
        /// <remarks>Example for custom <see cref="UIelement"/> constructor: <code>
        /// this.rect = new DyeableRect(myContainer, this.pos, this.size, true);
        /// </code></remarks>
        public DyeableRect(FContainer container, Vector2 pos, Vector2 size, bool filled = true)
        {
            this.container = new FContainer();
            container.AddChild(this.container);
            this.pos = pos;
            this.size = size;
            this.container.x = this.pos.x; this.container.y = this.pos.y;
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorFill = Color.black;
            this.filled = filled;
            //this.tab = false;

            // Init Sprite
            this.sprites = new FSprite[(!filled) ? 8 : 17];
            for (int i = 0; i < 4; i++)
            {
                this.sprites[this.SideSprite(i)] = new FSprite("pixel", true);
                this.sprites[this.SideSprite(i)].scaleY = 2f;
                this.sprites[this.SideSprite(i)].scaleX = 2f;
                this.sprites[this.CornerSprite(i)] = new FSprite("UIroundedCorner", true);
                if (filled)
                {
                    this.sprites[this.FillSideSprite(i)] = new FSprite("pixel", true);
                    this.sprites[this.FillSideSprite(i)].scaleY = 6f;
                    this.sprites[this.FillSideSprite(i)].scaleX = 6f;
                    this.sprites[this.FillCornerSprite(i)] = new FSprite("UIroundedCornerInside", true);
                }
            }
            this.sprites[this.SideSprite(0)].anchorY = 0f;
            this.sprites[this.SideSprite(2)].anchorY = 0f;
            this.sprites[this.SideSprite(1)].anchorX = 0f;
            this.sprites[this.SideSprite(3)].anchorX = 0f;
            this.sprites[this.CornerSprite(0)].scaleY = -1f;
            this.sprites[this.CornerSprite(2)].scaleX = -1f;
            this.sprites[this.CornerSprite(3)].scaleY = -1f;
            this.sprites[this.CornerSprite(3)].scaleX = -1f;
            if (filled)
            {
                this.sprites[MainFillSprite] = new FSprite("pixel", true);
                this.sprites[MainFillSprite].anchorY = 0f;
                this.sprites[MainFillSprite].anchorX = 0f;
                this.sprites[this.FillSideSprite(0)].anchorY = 0f;
                this.sprites[this.FillSideSprite(2)].anchorY = 0f;
                this.sprites[this.FillSideSprite(1)].anchorX = 0f;
                this.sprites[this.FillSideSprite(3)].anchorX = 0f;
                this.sprites[this.FillCornerSprite(0)].scaleY = -1f;
                this.sprites[this.FillCornerSprite(2)].scaleX = -1f;
                this.sprites[this.FillCornerSprite(3)].scaleY = -1f;
                this.sprites[this.FillCornerSprite(3)].scaleX = -1f;
                for (int j = 0; j < 9; j++)
                {
                    this.sprites[j].color = new Color(0f, 0f, 0f);
                    this.sprites[j].alpha = 0.75f;
                }
            }
            for (int k = 0; k < this.sprites.Length; k++)
            {
                this.container.AddChild(this.sprites[k]);
            }
        }

        /// <summary>
        /// <see cref="FSprite"/>s of this rect
        /// </summary>
        public readonly FSprite[] sprites;

        protected readonly FContainer container;

        /// <param name="side">Left, Top, Right, Bottom</param>
        /// <returns></returns>
        protected int SideSprite(int side) => ((!this.filled) ? 0 : 9) + side;

        /// <param name="corner">BottomLeft, TopLeft, TopRight, BottomRight</param>
        /// <returns></returns>
        protected int CornerSprite(int corner) => ((!this.filled) ? 0 : 9) + 4 + corner;

        /// <param name="side">Left, Top, Right, Bottom</param>
        /// <returns></returns>
        protected internal int FillSideSprite(int side) => side;

        /// <param name="corner">BottomLeft, TopLeft, TopRight, BottomRight</param>
        /// <returns></returns>
        protected internal int FillCornerSprite(int corner) => 4 + corner;

        protected internal const int MainFillSprite = 8;

        /// <summary>
        /// Edge Color of this Rect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Fill Color of this Rect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorFill;

        /// <summary>
        /// Relative pos from <see cref="FContainer"/> origin this is in
        /// </summary>
        public Vector2 pos;

        /// <summary>
        /// The size of rectangular
        /// </summary>
        public Vector2 size;

        private readonly bool filled;

        /// <summary>
        /// Extra size for bump effect
        /// </summary>
        public Vector2 addSize;

        private Vector2 lastAddSize;

        /// <summary>
        /// whether you cut right side or not.
        /// </summary>
        public HiddenSide hiddenSide;

        public enum HiddenSide
        {
            None,
            Left,
            Right,
            Top,
            Bottom
        }

        /// <summary>
        /// Alpha of backdrop fill
        /// </summary>
        public float fillAlpha;

        private float lastFillAlpha;

        /// <summary>
        /// Call this in <see cref="UIelement.Update()"/>
        /// </summary>
        public void Update()
        {
            if (hidden) { return; }
            lastFillAlpha = fillAlpha;
            lastAddSize = addSize;
            this.container.x = this.pos.x; this.container.y = this.pos.y;
        }

        private int[] sideSprites()
        {
            switch (hiddenSide)
            {
                case HiddenSide.Left:
                    return filled ? new int[] { 0, 4, 5, 9, 13, 14 } : new int[] { 0, 4, 5 };

                case HiddenSide.Right:
                    return filled ? new int[] { 2, 6, 7, 11, 15, 16 } : new int[] { 2, 6, 7 };

                case HiddenSide.Top:
                    return filled ? new int[] { 1, 5, 6, 10, 14, 15 } : new int[] { 1, 5, 6 };

                case HiddenSide.Bottom:
                    return filled ? new int[] { 3, 4, 7, 12, 13, 16 } : new int[] { 3, 4, 7 };
            }
            return new int[0];
        }

        /// <summary>
        /// Call this in <see cref="UIelement.GrafUpdate(float)"/>
        /// </summary>
        public void GrafUpdate(float timeStacker)
        {
            if (hidden) { return; }
            this.container.x = this.pos.x; this.container.y = this.pos.y;

            #region RoundedRect

            Vector2 drawPos = -Vector2.Lerp(this.lastAddSize, this.addSize, timeStacker) / 2f;
            Vector2 drawSize = size + Vector2.Lerp(this.lastAddSize, this.addSize, timeStacker);
            drawPos.x = Mathf.Floor(drawPos.x) + 0.41f;
            drawPos.y = Mathf.Floor(drawPos.y) + 0.41f;
            this.sprites[this.SideSprite(0)].x = drawPos.x + 1f; // Left
            this.sprites[this.SideSprite(0)].y = drawPos.y + 7f;
            this.sprites[this.SideSprite(0)].scaleY = drawSize.y - 14f;
            this.sprites[this.SideSprite(1)].x = drawPos.x + 7f; // Top
            this.sprites[this.SideSprite(1)].y = drawPos.y + drawSize.y - 1f;
            this.sprites[this.SideSprite(1)].scaleX = drawSize.x - 14f;
            this.sprites[this.SideSprite(2)].x = drawPos.x + drawSize.x - 1f; // Right
            this.sprites[this.SideSprite(2)].y = drawPos.y + 7f;
            this.sprites[this.SideSprite(2)].scaleY = drawSize.y - 14f;
            this.sprites[this.SideSprite(3)].x = drawPos.x + 7f; // Bottom
            this.sprites[this.SideSprite(3)].y = drawPos.y + 1f;
            this.sprites[this.SideSprite(3)].scaleX = drawSize.x - 14f;
            this.sprites[this.CornerSprite(0)].x = drawPos.x + 3.5f; // BottomLeft
            this.sprites[this.CornerSprite(0)].y = drawPos.y + 3.5f;
            this.sprites[this.CornerSprite(1)].x = drawPos.x + 3.5f; // TopLeft
            this.sprites[this.CornerSprite(1)].y = drawPos.y + drawSize.y - 3.5f;
            this.sprites[this.CornerSprite(2)].x = drawPos.x + drawSize.x - 3.5f; // TopRight
            this.sprites[this.CornerSprite(2)].y = drawPos.y + drawSize.y - 3.5f;
            this.sprites[this.CornerSprite(3)].x = drawPos.x + drawSize.x - 3.5f; // BottomRight
            this.sprites[this.CornerSprite(3)].y = drawPos.y + 3.5f;
            Color color = new Color(1f, 1f, 1f);
            for (int i = 0; i < 4; i++)
            {
                this.sprites[this.SideSprite(i)].color = color;
                this.sprites[this.CornerSprite(i)].color = color;
            }
            if (this.filled)
            {
                this.sprites[this.FillSideSprite(0)].x = drawPos.x + 4f;
                this.sprites[this.FillSideSprite(0)].y = drawPos.y + 7f;
                this.sprites[this.FillSideSprite(0)].scaleY = drawSize.y - 14f;
                this.sprites[this.FillSideSprite(1)].x = drawPos.x + 7f;
                this.sprites[this.FillSideSprite(1)].y = drawPos.y + drawSize.y - 4f;
                this.sprites[this.FillSideSprite(1)].scaleX = drawSize.x - 14f;
                this.sprites[this.FillSideSprite(2)].x = drawPos.x + drawSize.x - 4f;
                this.sprites[this.FillSideSprite(2)].y = drawPos.y + 7f;
                this.sprites[this.FillSideSprite(2)].scaleY = drawSize.y - 14f;
                this.sprites[this.FillSideSprite(3)].x = drawPos.x + 7f;
                this.sprites[this.FillSideSprite(3)].y = drawPos.y + 4f;
                this.sprites[this.FillSideSprite(3)].scaleX = drawSize.x - 14f;
                this.sprites[this.FillCornerSprite(0)].x = drawPos.x + 3.5f;
                this.sprites[this.FillCornerSprite(0)].y = drawPos.y + 3.5f;
                this.sprites[this.FillCornerSprite(1)].x = drawPos.x + 3.5f;
                this.sprites[this.FillCornerSprite(1)].y = drawPos.y + drawSize.y - 3.5f;
                this.sprites[this.FillCornerSprite(2)].x = drawPos.x + drawSize.x - 3.5f;
                this.sprites[this.FillCornerSprite(2)].y = drawPos.y + drawSize.y - 3.5f;
                this.sprites[this.FillCornerSprite(3)].x = drawPos.x + drawSize.x - 3.5f;
                this.sprites[this.FillCornerSprite(3)].y = drawPos.y + 3.5f;
                this.sprites[MainFillSprite].x = drawPos.x + 7f;
                this.sprites[MainFillSprite].y = drawPos.y + 7f;
                this.sprites[MainFillSprite].scaleX = drawSize.x - 14f;
                this.sprites[MainFillSprite].scaleY = drawSize.y - 14f;
                for (int j = 0; j < 9; j++)
                {
                    this.sprites[j].alpha = Mathf.Lerp(this.lastFillAlpha, this.fillAlpha, timeStacker);
                }
            }

            #endregion RoundedRect

            if (hiddenSide != HiddenSide.None)
            {
                int[] hide = sideSprites();
                for (int i = 0; i < hide.Length; i++)
                { this.sprites[hide[i]].isVisible = false; }
            }

            #region Dye

            for (int i = 0; i < 4; i++)
            {
                this.sprites[this.SideSprite(i)].color = this.colorEdge;
                this.sprites[this.CornerSprite(i)].color = this.colorEdge;
            }
            if (this.filled)
            {
                this.sprites[MainFillSprite].color = this.colorFill;
                for (int i = 0; i < 4; i++)
                {
                    this.sprites[this.FillSideSprite(i)].color = this.colorFill;
                    this.sprites[this.FillCornerSprite(i)].color = this.colorFill;
                }
            }

            #endregion Dye
        }

        public bool hidden { get; private set; } = false;

        /// <summary>
        /// Useful for hiding this alone. See also <seealso cref="Show()"/> for the counterpart.
        /// <para>If its <see cref="container"/> is <see cref="UIelement.myContainer"/>,
        /// there's no need to do add this in <see cref="UIelement.Deactivate"/> to hide.</para>
        /// </summary>
        public void Hide()
        {
            if (hidden) { return; }
            this.container.isVisible = false;
            hidden = true;
        }

        /// <summary>
        /// Used for unhide this. See also <seealso cref="Hide()"/>.
        /// </summary>
        public void Show()
        {
            if (!hidden) { return; }
            this.container.isVisible = true;
            hidden = false;
        }
    }
}