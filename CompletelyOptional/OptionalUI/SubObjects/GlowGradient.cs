using CompletelyOptional;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Gradient glow effect used in various way. In default this is black.
    /// </summary>
    public class GlowGradient
    {
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
                anchorY = 1f,
                x = this.centerPos.x,
                y = this.centerPos.y
            };
            this.container.AddChild(this.sprite);
        }

        public GlowGradient(FContainer container, Vector2 pos, Vector2 size, float alpha = 0.5f) : this(container, pos - size / 2f, size.x / 2f, size.y / 2f, alpha)
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

        private readonly FSprite sprite;

        public float radH
        { get => _radH; set { if (_radH == value) { return; } _radH = value; OnChange(); } }

        public float radV
        { get => _radV; set { if (_radV == value) { return; } _radV = value; OnChange(); } }

        public Vector2 size
        { get => new Vector2(_radH, _radV) * 2f; set { _radH = size.x / 2f; _radV = size.y / 2f; OnChange(); } }

        public float alpha
        { get => _alpha; set { if (_alpha == value) { return; } _alpha = value; OnChange(); } }

        public Vector2 centerPos
        { get => _centerPos; set { if (_centerPos == value) { return; } _centerPos = value; OnChange(); } }

        public Vector2 pos
        { get => _centerPos - new Vector2(_radH, _radV); set { _centerPos = value + new Vector2(_radH, _radV); OnChange(); } }

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