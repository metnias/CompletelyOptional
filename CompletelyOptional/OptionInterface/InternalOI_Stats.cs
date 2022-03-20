using OptionalUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CompletelyOptional
{
    internal class InternalOI_Stats : InternalOI
    {
        public InternalOI_Stats() :
            base(new RainWorldMod("_Statistics_"), Reason.Statistics)
        {
        }

        private void LoadIllustration()
        {
            string[] files = new string[] { imgTitle, imgTitleS };
            foreach (string file in files)
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { wrapMode = TextureWrapMode.Clamp };
                Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CompletelyOptional.Resources." + file + ".png");
                byte[] streamData = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(streamData, 0, (int)manifestResourceStream.Length);
                texture.LoadImage(streamData);
                HeavyTexturesCache.LoadAndCacheAtlasFromTexture(file, texture);
            }
        }

        private const string imgTitle = "ConfigTitle", imgTitleS = "ConfigShadow";

        // A function that turns off CM music
        // Save starred mods and remember last chosen mod

        public override void Initialize()
        {
            LoadIllustration();
            base.Initialize();

            // MOD CONFIG
            OpImage title = new OpImage(new Vector2(10f, 450f), imgTitleS);
            Tabs[0].AddItems(title);
            title = new OpImage(new Vector2(10f, 450f), imgTitle);
            title.sprite.shader = ComOptPlugin.rw.Shaders["MenuText"];
            Tabs[0].AddItems(title);

            // turn off CM music
            // Reset starred mods
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
        }
    }
}