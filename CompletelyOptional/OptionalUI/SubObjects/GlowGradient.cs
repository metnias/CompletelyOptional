using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Gradient glow effect used in various way. In default this is black.
    /// </summary>
    public class GlowGradient
    {
        /// <summary>
        /// Gradient glow effect used in various way. In default this is black.
        /// </summary>
        /// <param name="container"><see cref="FContainer"/> this will be added. It will call <see cref="FContainer.AddChild"/> on its own</param>
        /// <param name="centerPos">Center Position</param>
        /// <param name="radH">Horizontal Radius</param>
        /// <param name="radV">Vertical Radius</param>
        /// <param name="alpha">alpha</param>
        public GlowGradient(FContainer container, Vector2 centerPos, float radH, float radV, float alpha = 0.5f)
        {
            this.container = container;
            this._centerPos = centerPos;
            this._radH = radH; this._radV = radV; this._alpha = alpha;

            this.sprite = new FSprite("Futile_White", true)
            {
                scaleX = this.radH / 8f,
                scaleY = this.radV / 8f,
                shader = ModConfigMenu.instance.manager.rainWorld.Shaders["FlatLight"],
                color = this.color,
                alpha = this.alpha,
                anchorX = 0.5f,
                anchorY = 0.5f,
                x = this.centerPos.x,
                y = this.centerPos.y
            };
            this.container.AddChild(this.sprite);
        }

        /// <summary>
        /// Gradient glow effect used in various way. In default this is black.
        /// </summary>
        /// <param name="container"><see cref="FContainer"/> this will be added. It will call <see cref="FContainer.AddChild"/> on its own.</param>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size</param>
        /// <param name="alpha">alpha</param>
        public GlowGradient(FContainer container, Vector2 pos, Vector2 size, float alpha = 0.5f) : this(container, pos + size / 2f, size.x / 2f, size.y / 2f, alpha)
        { }

        public void OnChange()
        {
            this.sprite.scaleX = this.radH / 8f;
            this.sprite.scaleY = this.radV / 8f;
            this.sprite.color = this.color;
            this.sprite.x = this.centerPos.x;
            this.sprite.y = this.centerPos.y;
            this.sprite.alpha = alpha;
        }

        protected readonly FContainer container;

        /// <summary>
        /// The sprite of this glow.
        /// </summary>
        public readonly FSprite sprite;

        /// <summary>
        /// Horizontal radius
        /// </summary>
        public float radH
        { get => _radH; set { if (_radH == value) { return; } _radH = value; OnChange(); } }

        /// <summary>
        /// Vertical radius
        /// </summary>
        public float radV
        { get => _radV; set { if (_radV == value) { return; } _radV = value; OnChange(); } }

        /// <summary>
        /// Rectangular size bound to <see cref="radH"/> and <see cref="radV"/>
        /// </summary>
        public Vector2 size
        { get => new Vector2(_radH, _radV) * 2f; set { _radH = value.x / 2f; _radV = value.y / 2f; OnChange(); } }

        /// <summary>
        /// alpha of sprite
        /// </summary>
        public float alpha
        { get => _alpha; set { if (_alpha == value) { return; } _alpha = value; OnChange(); } }

        /// <summary>
        /// Center position
        /// </summary>
        public Vector2 centerPos
        { get => _centerPos; set { if (_centerPos == value) { return; } _centerPos = value; OnChange(); } }

        /// <summary>
        /// This sets <see cref="centerPos"/> indirectly using <see cref="radH"/> and <see cref="radV"/>. So set <see cref="size"/> or rads before for desired result.
        /// </summary>
        public Vector2 pos
        { get => _centerPos - new Vector2(_radH, _radV); set { centerPos = value + new Vector2(_radH, _radV); } }

        /// <summary>
        /// The colour of the sprite
        /// </summary>
        public Color color
        { get => _color; set { if (_color == value) { return; } _color = value; OnChange(); } }

        /// <summary>
        /// Hide this from the view
        /// </summary>
        public void Hide()
        { if (!isHidden) { isHidden = true; this.sprite.isVisible = false; } }

        /// <summary>
        /// Show this from the view
        /// </summary>
        public void Show()
        { if (isHidden) { isHidden = false; this.sprite.isVisible = true; } }

        /// <summary>
        /// Whether this is hidden or not. Use <see cref="Hide"/> and <see cref="Show"/> to manipulate.
        /// </summary>
        public bool isHidden { get; private set; } = false;

        private float _radH, _radV, _alpha;
        private Vector2 _centerPos;
        private Color _color = new Color(0.01f, 0.01f, 0.01f);
    }
}