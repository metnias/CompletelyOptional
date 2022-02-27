using Menu;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// A special <see cref="RoundedRect"/> that can be coloured easily.
    /// If you are making custom <see cref="UIelement"/> that has this, Add this to <see cref="UIelement.subObjects"/>.
    /// </summary>
    public class DyeableRect : RoundedRect
    {
        /// <summary>
        /// A special <see cref="RoundedRect"/> that can be coloured easily.
        /// If you are making custom <see cref="UIelement"/> that has this, Add this to <see cref="UIelement.subObjects"/>.
        /// </summary>
        /// <remarks>Example for custom <see cref="UIelement"/> constructor: <code>
        /// this.rect = new DyeableRect(menu, owner, this.pos, this.size, true);
        /// this.subObjects.Add(this.rect);
        /// </code></remarks>
        public DyeableRect(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, bool filled = true) : base(menu, owner, pos, size, filled)
        {
            this.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this.colorF = Color.black;
            this.filled = filled;
            this.tab = false;
            if (filled)
            {
                tabInvisible = new List<FSprite>
                {
                    this.sprites[2],
                    this.sprites[6],
                    this.sprites[7],
                    this.sprites[11],
                    this.sprites[15],
                    this.sprites[16]
                };
            }
            else
            {
                tabInvisible = new List<FSprite>
                {
                    this.sprites[2], //3
                    this.sprites[6],
                    this.sprites[7]
                };
            }
        }

        private const float midDark = 0.4435f;
        private const float midVDark = 0.2217f;

        public static Color MidToDark(Color mid) => new Color(mid.r * midDark, mid.g * midDark, mid.b * midDark, mid.a);

        public static Color MidToVeryDark(Color mid) => new Color(mid.r * midVDark, mid.g * midVDark, mid.b * midVDark, mid.a);

        public static Color Grayscale(Color orig) => new Color(orig.grayscale, orig.grayscale, orig.grayscale, orig.a);

        private int SideSprite(int side) => ((!this.filled) ? 0 : 9) + side;

        private int CornerSprite(int corner) => ((!this.filled) ? 0 : 9) + 4 + corner;

        private int FillSideSprite(int side) => side;

        private int FillCornerSprite(int corner) => 4 + corner;

        private const int MainFillSprite = 8;

        //public int[] visibleIndex = { 0, 1, 3, 4, 5, 8, 9, 10, 12, 13, 14 };
        private readonly List<FSprite> tabInvisible;

        /// <summary>
        /// Edge Color of this Rect. Default is <see cref="Menu.Menu.MenuColors.MediumGrey"/>.
        /// </summary>
        public Color color;

        /// <summary>
        /// Fill Color of this Rect. Default is <see cref="Menu.Menu.MenuColors.Black"/>.
        /// </summary>
        public Color colorF;

        private readonly bool filled;

        /// <summary>
        /// whether you cut right side or not.
        /// </summary>
        public bool tab;

        /// <summary>
        /// Called automatically.
        /// </summary>
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (tab) { foreach (FSprite edge in tabInvisible) { edge.isVisible = false; } }
            for (int i = 0; i < 4; i++)
            {
                this.sprites[this.SideSprite(i)].color = this.color;
                this.sprites[this.CornerSprite(i)].color = this.color;
            }
            if (this.filled)
            {
                this.sprites[MainFillSprite].color = this.colorF;
                for (int i = 0; i < 4; i++)
                {
                    this.sprites[this.FillSideSprite(i)].color = this.colorF;
                    this.sprites[this.FillCornerSprite(i)].color = this.colorF;
                }
            }
        }

        /// <summary>
        /// Useful for hiding this in <see cref="UIelement.Hide"/>
        /// </summary>
        public void Hide()
        {
            foreach (FSprite s in this.sprites) { s.isVisible = false; }
        }

        /// <summary>
        /// Usefull for unhiding this in <see cref="UIelement.Show"/>
        /// </summary>
        public void Show()
        {
            foreach (FSprite s in this.sprites) { s.isVisible = true; }
            if (tab) { foreach (FSprite edge in tabInvisible) { edge.isVisible = false; } }
        }
    }

    /*  public interface iHaveDyeRect
    {
        /// <summary>
        /// DyeableRect instance
        /// </summary>
        DyeableRect rect { get; set; }
        /// <summary>
        /// Edge Color of DyeableRect
        /// </summary>
        Color colorEdge { get; set; }
        /// <summary>
        /// Fill Color of DyeableRect
        /// </summary>
        Color colorFill { get; set; }
    }*/
}