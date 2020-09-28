using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// <see cref="FContainer"/> that acts like <see cref="FSprite"/>
    /// </summary>
    public class FCursor : FContainer
    {
        public FCursor()
        {
            this.sprites = new FSprite[3];
            for (int i = 0; i < 3; i++)
            {
                this.sprites[i] = new FSprite("pixel", true)
                {
                    color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey), 0.5f),
                    scaleX = 2f,
                    scaleY = 2f
                };
                this.sprites[i].SetAnchor(0f, 0f);
                this.AddChild(sprites[i]);
            }
            this.sprites[0].scaleX = 7f;
            this.sprites[0].SetPosition(0f, 8f);
            this.sprites[1].scaleY = 5f;
            this.sprites[1].SetPosition(6f, 5f);
            this.sprites[2].SetPosition(5f, 4f);
        }

        /// <summary>
        /// Colour of this sprite
        /// </summary>
        public Color color
        {
            get { return this.sprites[0].color; }
            set
            {
                for (int i = 0; i < 3; i++)
                { this.sprites[i].color = value; }
            }
        }

        public FSprite[] sprites;
    }
}