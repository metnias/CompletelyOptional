using Partiality.Modloader;

namespace OptionalUI
{
    public struct RainWorldMod
    {
        public RainWorldMod(PartialityMod mod)
        {
            this.type = Type.PartialityMod;
            this.mod = mod;
            this.ModID = mod.ModID;
            this.author = mod.author;
            this.Version = mod.Version;
        }

        public readonly Type type;
        public string ModID, author, Version;
        private readonly object mod;
        public PartialityMod PartialityMod => mod as PartialityMod;

        public enum Type
        {
            PartialityMod,
            BepInExPlugin
        }
    }
}
