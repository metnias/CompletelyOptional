using CompletelyOptional;
using System;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Features:
    /// Four times undos
    /// 16 Palettes (First two are fixed to Clear and White)
    ///
    /// </summary>
    public class OpSpriteEditor : UIconfig
    {
        /// <summary>
        /// Config Machine's internal Sprite Editor with minimal functionality
        /// </summary>
        /// <param name="pos">Bottom Left Position</param>
        /// <param name="size">Minimum is Texture size + 10 pxl for each dimension</param>
        /// <param name="key"></param>
        /// <param name="defaultTexture"></param>
        public OpSpriteEditor(Vector2 pos, Vector2 size, string key, Texture2D defaultTexture) : base(pos, size, key, Texture2DToString(defaultTexture))
        {
            this._size = new Vector2(Mathf.Max(size.x, defaultTexture.width + 10f), Mathf.Max(size.y, defaultTexture.height + 10f));
            this.colorEdge = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            this._descCustom = "";

            this.rect = new DyeableRect(myContainer, this.pos, this.size, true);

            this.preview = new FTexture(defaultTexture, "edtp" + key);
            this.myContainer.AddChild(this.preview);
            this.preview.SetPosition(this.size.x / 2f, this.size.y / 2f);
            this.myContainer.MoveToFront();

            this.clickDelay = 0; this.clicked = false;

            throw new NotImplementedException("OpSpriteEditor will come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        private readonly DyeableRect rect;
        private readonly FTexture preview;

        /// <summary>
        /// Used for Colour of Button and Editor Bound Rectangular
        /// </summary>
        public Color colorEdge;

        /// <summary>
        /// Custom Description will be shown for Preview Mode
        /// </summary>
        public new string description
        {
            get
            {
                if (!held) { return string.IsNullOrEmpty(_descCustom) ? "Double Click to enter Editor mode" : _descCustom; }
                return _descContext;
            }
            set { _descCustom = value; }
        }

        private string _descCustom, _descContext;

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);

            if (!this.held)
            {
                if (this.MouseOver && !this.greyedOut)
                { ModConfigMenu.instance.ShowDescription(this.description); }

                this.rect.colorEdge = this.bumpBehav.GetColor(this.colorEdge);
                this.rect.colorFill = MenuColorEffect.MidToVeryDark(this.colorEdge);
                this.rect.addSize = new Vector2(4f, 4f) * this.bumpBehav.AddSize;
                this.rect.fillAlpha = this.bumpBehav.FillAlpha;
                return;
            }
            ModConfigMenu.instance.ShowDescription(this.description);

            this.rect.colorEdge = this.colorEdge;
            this.rect.colorFill = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black);
            this.rect.addSize = Vector2.zero;
            this.rect.fillAlpha = 1f;
        }

        private int clickDelay; private bool clicked;

        public override void Update()
        {
            base.Update();
            if (this.greyedOut)
            { if (this.held) { this.SwitchToPreview(); } return; }
            clickDelay = clickDelay > 0 ? clickDelay - 1 : 0;
            if (!this.held)
            {
                if (this.MouseOver)
                {
                    if (Input.GetMouseButton(0))
                    {
                        if (!clicked)
                        {
                            clicked = true;
                            if (clickDelay == 0) { clickDelay = 20; }
                            else { clickDelay = 0; this.SwitchToEditor(); }
                        }
                    }
                    else { clicked = false; }
                }
                return;
            }

            if (this.EditorMousePos.x > 0f && this.EditorMousePos.y > 0f && this.EditorMousePos.x < editorSize.x && this.EditorMousePos.y < editorSize.y)
            {
            }
            else
            {
                this._descContext = "Double Click outside to Save and Close";
                if (Input.GetMouseButton(0))
                {
                    if (!clicked)
                    {
                        clicked = true;
                        if (clickDelay == 0) { clickDelay = 20; }
                        else { clickDelay = 0; this.SwitchToPreview(); }
                    }
                }
                else { clicked = false; }
            }
        }

        private void SwitchToEditor()
        {
            this.held = true;
            this._descContext = "";

            this.rect.pos = editorOffset;
            this.rect.size = editorSize;
            foreach (FSprite spr in this.rect.sprites) { spr.MoveToFront(); }
            this.myContainer.MoveToFront();
        }

        private void SwitchToPreview()
        {
            this.held = false;
            this.OnChange();
        }

        private static readonly Vector2 editorOffset = new Vector2(50f, 50f);
        private static readonly Vector2 editorSize = new Vector2(500f, 500f);

        private Vector2 EditorMousePos => this.MousePos - this._pos + editorOffset;

        public override void OnChange()
        {
            base.OnChange();

            Texture2D curTexture = this.valueTexture;

            this._size = new Vector2(Mathf.Max(size.x, curTexture.width + 10f), Mathf.Max(size.y, curTexture.height + 10f));
            if (!this.held)
            {
                this.rect.size = this._size;
                this.rect.pos = this._pos;
                this.preview.SetPosition(this.size.x / 2f, this.size.y / 2f);
                this.preview.SetTexture(curTexture);
            }
        }

        public override void Reset()
        {
            base.Reset();
            this.SwitchToPreview();
        }

        protected internal override void Unload()
        {
            base.Unload();
            this.preview.Destroy();
        }

        /// <summary>
        /// Converts Base64 <see cref="string"/> to <see cref="Texture2D"/>
        /// </summary>
        /// <remarks>
        /// See <seealso cref="Texture2DToString(Texture2D)"/> for reverse.
        /// </remarks>
        /// <param name="data">Base64String</param>
        /// <returns>Texture2D</returns>
        public static Texture2D StringToTexture2D(string data)
        {
            byte[] bytes = System.Convert.FromBase64String(data);
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);
            return tex;
        }

        /// <summary>
        /// Converts <see cref="Texture2D"/> to Base64 <see cref="string"/>
        /// </summary>
        /// <remarks>
        /// See <seealso cref="StringToTexture2D(string)"/> for reverse.
        /// <param name="image">Texture2D to be converted</param>
        /// <returns>Base64String</returns>
        /// </remarks>
        public static string Texture2DToString(Texture2D image)
        {
            byte[] bytes = image.EncodeToPNG();
            return System.Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// value of this in <see cref="Texture2D"/> format.
        /// </summary>
        /// <remarks>
        /// See also <seealso cref="StringToTexture2D(string)"/> and
        /// <seealso cref="Texture2DToString(Texture2D)"/>.
        /// </remarks>
        public Texture2D valueTexture
        {
            get { return StringToTexture2D(this.value); }
            set
            {
                string temp = Texture2DToString(value);
                if (this.value != temp) { this.value = temp; }
            }
        }
    }
}