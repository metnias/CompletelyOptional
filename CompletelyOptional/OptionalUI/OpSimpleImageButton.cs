using System;
using UnityEngine;

namespace OptionalUI
{
    public class OpSimpleImageButton : OpSimpleButton, FocusableUIelement
    {
        /// <summary>
        /// SimpleButton that uses <see cref="FAtlasElement"/> instead of text
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Minimum size is 24x24</param>
        /// <param name="signal">Keyword that gets sent to Signal</param>
        /// <param name="fAtlasElement">the name of FAtlasElement</param>
        /// <exception cref="ElementFormatException">Thrown when <paramref name="fAtlasElement"/> is Invalid</exception>
        public OpSimpleImageButton(Vector2 pos, Vector2 size, string signal, string fAtlasElement) : base(pos, size, signal, "")
        {
            if (!_init)
            {
                this.sprite = new FSprite("pixel", true);
                return;
            }

            FAtlasElement element;
            try
            { element = Futile.atlasManager.GetElementWithName(fAtlasElement); }
            catch (Exception ex)
            { throw new ElementFormatException(this, string.Concat("There is no such FAtlasElement called ", fAtlasElement, " : ", Environment.NewLine, ex.ToString())); }

            this.sprite = new FSprite(element.name, true);
            this.myContainer.AddChild(this.sprite);
            //this._size = element.sourceSize;
            this.sprite.SetAnchor(0.5f, 0.5f);
            this.sprite.SetPosition(this.size.x / 2f, this.size.y / 2f);

            this.isTexture = false;
        }

        /// <summary>
        /// SimpleButton that uses <see cref="Texture2D"/> instead of text
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Minimum size is 24x24</param>
        /// <param name="signal">Keyword that gets sent to Signal</param>
        /// <param name="image"><see cref="Texture2D"/> to display</param>
        /// <exception cref="ElementFormatException">Thrown when <paramref name="image"/> is null</exception>
        public OpSimpleImageButton(Vector2 pos, Vector2 size, string signal, Texture2D image) : base(pos, size, signal, "")
        {
            if (image == null) { throw new ElementFormatException(this, "There is no Texture2D for OpSimpleImageButton"); }
            if (!_init)
            {
                this.sprite = new FSprite("pixel", true);
                return;
            }

            this.sprite = new FTexture(image, "sib" + signal);
            this.sprite.SetAnchor(0.5f, 0.5f);
            this.myContainer.AddChild(this.sprite);
            this.sprite.SetPosition(this.size.x / 2f, this.size.y / 2f);
            //this._size = image.texelSize;

            this.isTexture = true;
        }

        /// <summary>
        /// <see cref="FSprite"/> of this button
        /// </summary>
        public FSprite sprite;

        private readonly bool isTexture;

        public override void Hide()
        {
            base.Hide();
            sprite.isVisible = false;
        }

        public override void Show()
        {
            base.Show();
            sprite.isVisible = true;
        }

        public override void OnChange()
        {
            base.OnChange();
            this.sprite.SetPosition(this.size.x / 2f, this.size.y / 2f);
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (!this.isTexture) { this.sprite.color = this.bumpBehav.GetColor(this.colorEdge); }
        }

        /// <summary>
        /// Swap Image to new one
        /// </summary>
        /// <param name="newImage">new image</param>
        /// <exception cref="InvalidActionException">Thrown when you called this with <see cref="FAtlasElement"/> version of the button</exception>
        public void ChangeImage(Texture2D newImage)
        {
            if (!_init) { return; }
            if (!isTexture) { throw new InvalidActionException(this, "You must construct this with Texture2D to use this function", signal); }
            if (newImage == null) { Debug.LogError("CompletelyOptional: newImage is null in OpSimpleImageButton.ChangeImage!"); return; }

            this._size = new Vector2(newImage.width, newImage.height);
            (this.sprite as FTexture).SetTexture(newImage);
            this.myContainer.AddChild(this.sprite);
        }

        public override void Unload()
        {
            base.Unload();
            if (isTexture) { (this.sprite as FTexture).Destroy(); }
        }
    }
}