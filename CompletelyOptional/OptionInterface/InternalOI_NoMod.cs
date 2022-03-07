using OptionalUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CompletelyOptional
{
    internal class InternalOI_NoMod : InternalOI
    {
        public InternalOI_NoMod() : base(new RainWorldMod("Instructions"), Reason.NoMod)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            TutoInit();
        }

        private void TutoInit()
        {
            labelID = new OpLabel(new Vector2(100f, 500f), new Vector2(400f, 50f), InternalTranslator.Translate("No Mod can be configured"), FLabelAlignment.Center, true);

            labelVersion = new OpLabel(new Vector2(100f, 440f), new Vector2(200f, 20f), "Config Machine (a.k.a. CompletelyOptional) by topicular", FLabelAlignment.Left);
            labelAuthor = new OpLabel(new Vector2(300f, 410f), new Vector2(200f, 20f), InternalTranslator.Translate("also shoutout to RW Discord for helping me out"), FLabelAlignment.Right);

            labelSluggo0 = new OpLabel(new Vector2(100f, 300f), new Vector2(400f, 20f), InternalTranslator.Translate("check out pinned tutorial in modding-support"));
            labelSluggo1 = new OpLabel(new Vector2(100f, 260f), new Vector2(400f, 20f), InternalTranslator.Translate("and create your own config screen!"));
            Tabs[0].AddItems(labelID, labelVersion, labelAuthor, labelSluggo0, labelSluggo1);
        }
    }
}