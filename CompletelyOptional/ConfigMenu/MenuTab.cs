using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special kind of OpTab for ConfigMenu. You don't need this.
    /// </summary>
    internal class MenuTab : OpTab
    {
        internal MenuTab() : base(null, "")
        {
            // 171 offset from left
            this.container.x = 0f; this.container.y = 0f;
            this.container.MoveToBack();
            isInactive = false;
            backButton = new MenuButton(new Vector2(490f, 50f), new Vector2(120f, 30f), backSignal, InternalTranslator.Translate("BACK"));
            saveButton = new MenuButton(new Vector2(630f, 50f), new Vector2(120f, 30f), saveSignal, InternalTranslator.Translate("APPLY"));
            resetButton = new MenuHoldButton(new Vector2(770f, 50f), new Vector2(120f, 30f), resetSignal, InternalTranslator.Translate("RESET CONFIG"));
            this.AddItems(backButton, saveButton, resetButton);
            modList = new MenuModList(this);
            tabCtrler = new ConfigTabController(this);
        }

        /* internal new void Update()
        {
            foreach (UIelement item in this.items) { if (!item.isInactive) { item.Update(); } }
        } */

        internal MenuModList modList;

        internal ConfigTabController tabCtrler;

        internal MenuButton backButton, saveButton;

        internal MenuHoldButton resetButton;

        internal const string backSignal = "CANCEL", saveSignal = "APPLY", resetSignal = "RESET";

        internal void Signal(string signalText)
        {
            switch (signalText)
            {
                case backSignal:
                case saveSignal:
                case resetSignal:
                    ModConfigMenu.instance.Singal(null, signalText);
                    break;
            }
        }

        internal class MenuButton : OpSimpleButton
        {
            public MenuButton(Vector2 pos, Vector2 size, string signal, string text) : base(pos, size, signal, text)
            {
            }

            public override void Signal()
            {
                (this.tab as MenuTab).Signal(this.signal);
            }
        }

        internal class MenuHoldButton : OpHoldButton
        {
            public MenuHoldButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal, text)
            {
            }

            public override void Signal()
            {
                (this.tab as MenuTab).Signal(this.signal);
            }
        }
    }
}