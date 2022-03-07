using BepInEx;
using OptionalUI;
using System;
using UnityEngine;

namespace CompletelyOptional
{
    internal class InternalOI_Error : InternalOI
    {
        public InternalOI_Error(BaseUnityPlugin plugin, Exception exception) : base(plugin, Reason.Error)
        {
            CtorInitError(exception);
        }

        public InternalOI_Error(RainWorldMod rwMod, Exception exception) : base(rwMod, Reason.Error)
        {
            CtorInitError(exception);
        }

        private void CtorInitError(Exception exception)
        {
            if (this.rwMod.ModID != null)
            { Debug.LogError($"CompletelyOptional: {this.rwMod.ModID} had issue in OptionInterface:"); }
            this.reason = Reason.Error;
            if (exception == null)
            { exception = new GeneralInitializeException("Unidentified Exception!"); }
            this.exception = exception.ToString();
            Debug.LogError(this.exception);
            //Debug.LogException(exception);
        }

        public override void Initialize()
        {
            base.Initialize();
            GeneratedOI.AddBasicProfile(Tabs[0], rwMod);

            blue = new OpRect(new Vector2(30f, 20f), new Vector2(540f, 420f)) { fillAlpha = 0.7f, colorFill = new Color(0.121568627f, 0.40392156862f, 0.69411764705f, 1f) };

            Color white = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White);
            oof = new OpLabel(new Vector2(100f, 380f), new Vector2(30f, 40f), ":(", FLabelAlignment.Left, true) { color = white };
            labelSluggo0 = new OpLabel(new Vector2(150f, 390f), new Vector2(300f, 20f), InternalTranslator.Translate("There was an issue initializing OptionInterface.")) { color = white };
            labelSluggo1 = new OpLabelLong(new Vector2(50f, 40f), new Vector2(500f, 320f), exception) { color = white, allowOverflow = false };
            labelVersion = new OpLabel(new Vector2(50f, 480f), new Vector2(100f, 20f), string.Concat(Environment.NewLine, "Config Machine ", ComOptPlugin.ver), FLabelAlignment.Left);

            Tabs[0].AddItems(blue, oof, labelSluggo0, labelSluggo1, labelVersion);
        }
    }
}