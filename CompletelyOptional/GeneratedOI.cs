using OptionalUI;
using BepInEx;
using UnityEngine;
using Partiality.Modloader;

namespace CompletelyOptional
{
    /// <summary>
    /// Procedurally generated OptionInterface
    /// </summary>
    public class GeneratedOI : OptionInterface
    {
        /// <summary>
        /// If you're using BepInEx.Configuration, this is called automatically.
        /// <para>Category will be separated by OpTab.</para>
        /// </summary>
        /// <param name="plugin"></param>
        public GeneratedOI(BaseUnityPlugin plugin) : base(plugin)
        {
        }

        /// <summary>
        /// PartialityMod version of GeneratedOI for lazy modders.
        /// </summary>
        /// <param name="mod"></param>
        public GeneratedOI(PartialityMod mod) : base(mod)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab();
            AddBasicProfile(Tabs[0], rwMod);
        }

        /// <summary>
        /// Adds Basic Profile on top of OpTab
        /// </summary>
        /// <param name="tab"><see cref="OpTab"/> which will contain the profile</param>
        /// <param name="mod"><see cref="RainWorldMod"/> that has basic information of the mod</param>
        public static void AddBasicProfile(OpTab tab, RainWorldMod mod)
        {
            tab.AddItems(new OpLabel(new Vector2(100f, 550f), new Vector2(400f, 50f), mod.ModID, FLabelAlignment.Center, true));
            tab.AddItems(new OpLabel(new Vector2(50f, 500f), new Vector2(100f, 20f), InternalTranslator.Translate("Version: <ModVersion>").Replace("<ModVersion>", mod.Version), FLabelAlignment.Left));

            if (mod.author != RainWorldMod.authorNull)
            {
                tab.AddItems(new OpLabel(new Vector2(350f, 500f), new Vector2(200f, 20f), InternalTranslator.Translate("Author: <ModAuthor>").Replace("<ModAuthor>", mod.author), FLabelAlignment.Right) { autoWrap = true });
            }

            /*
            if (mod.coauthor != RainWorldMod.authorNull)
            {
                labelCoauthor = new OpLabel(new Vector2(100f, 420f), new Vector2(300f, 20f), string.Concat("Coautor: ", mod.coauthor));
                Tabs[0].AddItem(labelCoauthor);
                labelCoauthor.autoWrap = true;
            }
            if(mod.description != RainWorldMod.authorNull)
            {
                labelDesc = new OpLabel(new Vector2(80f, 350f), new Vector2(340f, 20f), mod.description, FLabelAlignment.Left);
                Tabs[0].AddItem(labelDesc);
                labelDesc.autoWrap = true;
            } */
        }
    }
}
