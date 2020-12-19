using System;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Image Display
    /// </summary>
    public class OpImage : UIelement
    {
        /// <summary>
        /// Simply displays <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="pos">LeftBottom Position in default</param>
        /// <param name="image">Image you want to display</param>
        /// <exception cref="ElementFormatException">Thrown when image is null</exception>
        public OpImage(Vector2 pos, Texture2D image) : base(pos, new Vector2(image.width, image.height))
        {
            if (image == null) { throw new ElementFormatException(this, "There is no Texture2D for OpImage"); }
            if (!_init)
            {
                this.sprite = new FSprite("pixel", true);
                return;
            }

            this.isTexture = true;

            this.sprite = new FTexture(image, "img");
            this.sprite.color = this._color;
            this.sprite.SetPosition(Vector2.zero);
            this.sprite.SetAnchor(this._anchor);
            this._size = new Vector2(image.width, image.height);
            this.myContainer.AddChild(this.sprite);
        }

        /// <summary>
        /// Show <see cref="FAtlasElement"/> in RainWorld.
        /// </summary>
        /// <param name="pos">LeftBottom Position</param>
        /// <param name="fAtlasElement">Name of <see cref="FAtlasElement"/> in RainWorld you want to display</param>
        /// <exception cref="ElementFormatException">Thrown when there is no <see cref="FAtlasElement"/> with the name</exception>
        public OpImage(Vector2 pos, string fAtlasElement) : base(pos, Vector2.zero)
        {
            if (!_init)
            {
                this.sprite = new FSprite("pixel", true);
                return;
            }

            this.isTexture = false;

            if (!Futile.atlasManager.DoesContainElementWithName(fAtlasElement))
            { throw new ElementFormatException(this, $"There is no such FAtlasElement called [{fAtlasElement}]"); }

            this.sprite = new FSprite(fAtlasElement, true);
            this.sprite.color = this._color;
            this.sprite.SetPosition(Vector2.zero);
            this.sprite.SetAnchor(this._anchor);
            this._size = this.sprite.element.sourceSize;
            this.myContainer.AddChild(this.sprite);
        }

        /// <summary>
        /// Swap <see cref="Texture2D"/> to new one. This must be initialized with <see cref="Texture2D"/> to use this. This will reset <see cref="color"/> to white.
        /// </summary>
        /// <exception cref="InvalidActionException">Thrown when you called this with <see cref="FAtlasElement"/> version of <see cref="OpImage"/></exception>
        /// <param name="newImage">New image to display</param>
        public void ChangeImage(Texture2D newImage)
        {
            if (!_init) { return; }
            if (!isTexture) { throw new InvalidActionException(this, "You must construct this with Texture2D to use this function"); }
            if (newImage == null) { Debug.LogError("CompletelyOptional: newImage is null in OpImage.ChangeImage!"); return; }

            this._color = Color.white;
            this._size = new Vector2(newImage.width * _scale.x, newImage.height * _scale.y);
            (this.sprite as FTexture).SetTexture(newImage);
            this.sprite.SetAnchor(this._anchor);
            this.sprite.alpha = this._alpha;
            this.sprite.color = Color.white;
            this.myContainer.AddChild(this.sprite);
        }

        /// <summary>
        /// Swap <see cref="FAtlasElement"/> to new one. This must be initialized with <see cref="FAtlasElement"/> to use this.
        /// </summary>
        /// <param name="newElement">New element name to display</param>
        /// <exception cref="InvalidActionException">Thrown when you called this with <see cref="Texture2D"/> version of <see cref="OpImage"/></exception>
        public void ChangeElement(string newElement)
        {
            if (!_init) { return; }
            if (isTexture) { throw new InvalidActionException(this, "You must construct this with a name of FAtlasElement to use this function"); }
            if (newElement == null) { Debug.LogError("CompletelyOptional: newElement is null in OpImage.ChangeElement!"); return; }
            myContainer.RemoveAllChildren();

            if (!Futile.atlasManager.DoesContainElementWithName(newElement))
            { Debug.LogError($"CompletelyOptional: There is no such FAtlasElement called [{newElement}]"); return; }
            this.sprite = new FSprite(newElement, true);
            this.sprite.color = this._color;
            this.myContainer.AddChild(this.sprite);
            this._size = new Vector2(sprite.element.sourceSize.x * _scale.x, sprite.element.sourceSize.y * _scale.y);
            this.sprite.SetAnchor(this._anchor);
            this.sprite.alpha = this._alpha;
        }

        /// <summary>
        /// AnchorX and Y for sprite.
        /// </summary>
        /// <remarks>
        /// In {0f, 0f}(default), pos will be the bottomleft of sprite. In {0.5f, 0.5f}, pos will be the center of sprite.
        /// </remarks>
        public Vector2 anchor
        {
            get { return _anchor; }
            set
            {
                if (_anchor != value)
                {
                    _anchor = value;
                    if (_init) { this.sprite.SetAnchor(_anchor); }
                }
            }
        }

        private Vector2 _anchor = new Vector2(0f, 0f);

        /// <summary>
        /// ScaleX and Y for sprite. {1f, 1f} in default.
        /// </summary>
        public Vector2 scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    if (_init) { OnChange(); }
                }
            }
        }
        private Vector2 _scale = new Vector2(1f, 1f);

        public FSprite sprite;
        private readonly bool isTexture;
        // private readonly int seed;
        // private string salt { get { return "img" + seed.ToString("D4"); } }

        /// <summary>
        /// Alpha of OpImage. 1f in default.
        /// </summary>
        public float alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    OnChange();
                }
            }
        }
        private float _alpha = 1f;

        /// <summary>
        /// Set new Colour.
        /// </summary>
        public Color color
        {
            get
            { return _color; }
            set
            {
                if (!isTexture && _color != value)
                {
                    _color = value;
                    OnChange();
                }
            }
        }
        private Color _color = Color.white;

        public override void OnChange()
        {
            base.OnChange();
            if (!_init) { return; }
            sprite.alpha = _alpha;
            sprite.color = _color;
            sprite.SetPosition(Vector2.zero);
            sprite.scaleX = _scale.x;
            sprite.scaleY = _scale.y;
            _size = new Vector2(sprite.element.sourceSize.x * _scale.x, sprite.element.sourceSize.y * _scale.y);
        }

        /*
        public override bool MouseOver
        {
            get
            {
            if(centered)
                return this.menu.mousePosition.x > pos.x && this.menu.mousePosition.y > pos.y && this.menu.mousePosition.x < pos.x + this.size.x && this.menu.mousePosition.y < pos.y + this.size.y;
            }
        }*/

        /* public override void Hide()
        {
            base.Hide();
            sprite.isVisible = false;
        }

        public override void Show()
        {
            base.Show();
            sprite.isVisible = true;
        } */

        public override void Unload()
        {
            base.Unload();
            if (isTexture)
            { //remove element
                (this.sprite as FTexture).Destroy();
                //Futile.atlasManager.UnloadAtlas(salt + "image");
            }
        }
    }
}
