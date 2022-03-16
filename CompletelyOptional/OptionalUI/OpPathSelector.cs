using BepInEx.Configuration;
using System;
using System.IO;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Allow users to choose relative path.
    /// </summary>
    public class OpPathSelector : UIconfig
    {
        /// <summary>
        /// Allow users to choose relative path.
        /// </summary>
        /// <param name="pos">LeftBottom position. This is a circular button with radius of 30 pxl.</param>
        /// <param name="cosmeticPath"></param>
        /// <param name="extension">Requested extension ('dir' for directory, default is *)</param>
        /// <exception cref="ElementFormatException">Thrown when defaultPath is Invalid</exception>
        public OpPathSelector(ConfigEntry<string> config, Vector2 pos, string extension = "*", string cosmeticPath = "") : base(config, pos, 30f, cosmeticPath)
        {
            this.extension = extension;
            if (!string.IsNullOrEmpty(cosmeticPath))
            {
                try { Path.GetFullPath(this.value); }
                catch (Exception e) { throw new ElementFormatException($"OpPathSelector (key: {key} has Invalid defaultPath: {defaultValue}", e); }
            }

            fixedRad = 30f;

            color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
            circles = new FSprite[5];
            circles[0] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"] };
            circles[1] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircle"] };
            circles[2] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["HoldButtonCircle"] };
            circles[3] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircle"] };
            circles[4] = new FSprite("Futile_White")
            { shader = menu.manager.rainWorld.Shaders["VectorCircleFadable"] };
            for (int i = 0; i < circles.Length; i++) { myContainer.AddChild(circles[i]); circles[i].SetPosition(55f, 55f); }

            throw new NotImplementedException("OpPathSelector might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        private readonly FSprite[] circles;

        /// <summary>
        /// Main Colour for Label and Button
        /// </summary>
        public Color color;

        /// <summary>
        /// Requested extension ('dir' for directory)
        /// </summary>
        public readonly string extension;

        /// <summary>
        /// Whether this selector is for Directory or File
        /// </summary>
        public bool isDir => extension.ToLower() == "dir";

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            Color c = bumpBehav.GetColor(color);
            // label.color = c;
            float r = rad; // + 8f * (bumpBehav.sizeBump + 0.5f * Mathf.Sin(bumpBehav.extraSizeBump * Mathf.PI)) * ((!held) ? 1f : (0.5f + 0.5f * Mathf.Sin(pulse * Mathf.PI * 2f))) + 0.5f;
            for (int i = 0; i < circles.Length; i++) { circles[i].scale = r / 8f; circles[i].SetPosition(rad, rad); }
            circles[0].color = new Color(0.0196078438f, 0f, Mathf.Lerp(0.3f, 0.6f, bumpBehav.col));
            circles[1].color = c;
            circles[1].alpha = 2f / r;
            circles[2].scale = (r + 10f) / 8f;
            circles[2].alpha = 0f; // Mathf.Clamp01(!isProgress ? filled : progress / 100f);
            circles[2].color = Color.Lerp(Color.white, color, 0.7f);
            circles[3].color = Color.Lerp(c, MenuColorEffect.MidToDark(c), 0.5f);
            circles[3].scale = (r + 15f) / 8f;
            circles[3].alpha = 2f / (r + 15f);
            float dim = 0.5f + 0.5f * Mathf.Sin(bumpBehav.sin / 30f * Mathf.PI * 2f);
            dim *= bumpBehav.sizeBump;
            if (greyedOut) { dim = 0f; }
            circles[4].scale = (r - 8f * bumpBehav.sizeBump) / 8f;
            circles[4].alpha = 2f / (r - 8f * bumpBehav.sizeBump);
            circles[4].color = new Color(0f, 0f, dim);
        }

        public override void Update()
        {
            base.Update();
            if (MouseOver && Input.GetMouseButtonDown(0))
            {
            }
        }

        /// <summary>
        /// FileInfo casting for the value
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when the extention is 'dir'</exception>
        public FileInfo valueFile
        {
            get
            {
                if (isDir) { throw new InvalidGetPropertyException(this, "valueFile"); }
                return new FileInfo(Path.GetFullPath(this.value));
            }
        }

        /// <summary>
        /// DirectoryInfo casting for the value
        /// </summary>
        /// <exception cref="InvalidGetPropertyException">Thrown when the extention is not 'dir'</exception>
        public DirectoryInfo valueDir
        {
            get
            {
                if (!isDir) { throw new InvalidGetPropertyException(this, "valueFile"); }
                return new DirectoryInfo(Path.GetFullPath(this.value));
            }
        }
    }
}