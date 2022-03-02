using System.Reflection;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Special kind of <see cref="FSprite"/> that can use raw <see cref="Texture2D"/>
    /// </summary>
    public class FTexture : FSprite
    {
        /// <summary>
        /// Special kind of <see cref="FSprite"/> that accepts raw <see cref="Texture2D"/>
        /// </summary>
        /// <param name="texture"><see cref="Texture2D"/> to display</param>
        /// <param name="salt">Additional salt (No need to be unique)</param>
        public FTexture(Texture2D texture, string salt = "")
        {
            this._salt = salt;
            this.even = false;
            do { _seed = Mathf.FloorToInt(UnityEngine.Random.value * (string.IsNullOrEmpty(salt) ? 100000 : 1000)); }
            while (Futile.atlasManager.DoesContainAtlas(this.salt));

            _AddTexture2DToFManager(texture, this.salt);

            base._facetTypeQuad = true;
            base._localVertices = new Vector2[4];

            base.Init(FFacetType.Quad, Futile.atlasManager.GetElementWithName(this.salt), 1);
            base._isAlphaDirty = true;
            base.UpdateLocalVertices();
        }

        /// <summary>
        /// Special kind of <see cref="FSprite"/> that accepts raw <see cref="Texture"/>
        /// </summary>
        /// <param name="texture"><see cref="Texture"/> to display</param>
        /// <param name="salt">Additional salt (No need to be unique)</param>
        public FTexture(Texture texture, string salt = "")
        {
            this._salt = salt;
            this.even = false;
            do { _seed = Mathf.FloorToInt(UnityEngine.Random.value * (string.IsNullOrEmpty(salt) ? 100000 : 1000)); }
            while (Futile.atlasManager.DoesContainAtlas(this.salt));

            _AddTextureToFManager(texture, this.salt);

            base._facetTypeQuad = true;
            base._localVertices = new Vector2[4];

            base.Init(FFacetType.Quad, Futile.atlasManager.GetAtlasWithName(this.salt).elements[0], 1);
            base._isAlphaDirty = true;
            base.UpdateLocalVertices();
        }

        internal static uint garbage = 0;

        private readonly string _salt;
        private readonly int _seed;

        /// <summary>
        /// Randomly generated salt to prevent duplicate <see cref="FAtlasElement"/> name
        /// </summary>
        public string salt => _salt + _seed.ToString("D5") + (even ? "A" : "B");

        private bool even;

        private static void _AddTexture2DToFManager(Texture2D texture, string name)
        {
            if (Futile.atlasManager.DoesContainAtlas(name)) { RemoveTextureFromFManager(name); }
            Futile.atlasManager.LoadAtlasFromTexture(name, texture);
            garbage += (uint)(texture.width * texture.height / 100);
            if (garbage > 1000000) { GarbageCollect(); }
        }

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static void _AddTextureToFManager(Texture texture, string name)
        {
            if (Futile.atlasManager.DoesContainAtlas(name)) { RemoveTextureFromFManager(name); }
            FAtlas atlas = Futile.atlasManager.LoadAtlasFromTexture(name, texture);
            typeof(FAtlas).GetField("_texture", flags).SetValue(atlas, texture);
            typeof(FAtlas).GetField("_textureSize", flags).SetValue(atlas, new Vector2(texture.width, texture.height));
            atlas.elements[0].sourceSize = new Vector2(texture.width * Futile.resourceScaleInverse, texture.height * Futile.resourceScaleInverse);
            atlas.elements[0].sourcePixelSize = new Vector2(texture.width, texture.height);
            atlas.elements[0].sourceRect = new Rect(0f, 0f, texture.width * Futile.resourceScaleInverse, texture.height * Futile.resourceScaleInverse);
            garbage += (uint)(texture.width * texture.height / 100);
            if (garbage > 1000000) { GarbageCollect(); }
        }

        public static void GarbageCollect(bool actual = true)
        {
            if (actual)
            {
                CompletelyOptional.ComOptPlugin.LogMessage("FTexture called GarbageCollect");
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
            garbage = 0;
        }

        private static void RemoveTextureFromFManager(string name)
        { Futile.atlasManager.UnloadAtlas(name); }

        /// <summary>
        /// Add Texture2D to <see cref="FAtlasManager"/>, and returns <see cref="FAtlasElement.name"/> so it can be used in <see cref="FAtlasManager.GetElementWithName(string)"/>
        /// </summary>
        /// <param name="texture"><see cref="Texture2D"/> to be added</param>
        /// <param name="salt">Set this to reduce iteration to get an unique <see cref="FAtlasElement.name"/></param>
        /// <returns>FElement name</returns>
        public static string AddTexture2DToFManager(Texture2D texture, string salt)
        {
            int s; string name;
            do
            {
                s = Mathf.FloorToInt(UnityEngine.Random.value * (string.IsNullOrEmpty(salt) ? 100000 : 1000));
                name = salt + s.ToString("D5");
            }
            while (Futile.atlasManager.DoesContainAtlas(name));
            _AddTexture2DToFManager(texture, salt);
            return name;
        }

        /// <summary>
        /// Change <see cref="Texture2D"/> to display
        /// </summary>
        /// <param name="newTexture">New <see cref="Texture2D"/></param>
        public void SetTexture(Texture2D newTexture)
        {
            even = !even;
            _AddTexture2DToFManager(newTexture, this.salt);
            base.SetElementByName(this.salt);
        }

        /// <summary>
        /// Change <see cref="Texture"/> to display
        /// </summary>
        /// <param name="newTexture">New <see cref="Texture"/></param>
        public void SetTexture(Texture newTexture)
        {
            even = !even;
            _AddTextureToFManager(newTexture, this.salt);
            base.element = Futile.atlasManager.GetAtlasWithName(this.salt).elements[0];
        }

        /// <summary>
        /// Call this when you finished using <see cref="FTexture"/> to clear its texture from <see cref="FAtlasManager"/>
        /// </summary>
        public void Destroy()
        {
            this.RemoveFromContainer();
            RemoveTextureFromFManager(this.salt);
            even = !even;
            if (Futile.atlasManager.DoesContainAtlas(this.salt)) { RemoveTextureFromFManager(this.salt); }
        }
    }
}