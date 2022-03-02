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
            isInactive = false;
            modList = new MenuModList();
            tabCtrler = new ConfigTabController(this);
            backButton = new MenuButton(new Vector2(450f, 50f), new Vector2(110f, 30f), backSignal, InternalTranslator.Translate("BACK"));
            saveButton = new MenuButton(new Vector2(450f, 50f), new Vector2(110f, 30f), saveSignal, InternalTranslator.Translate("APPLY"));
            resetButton = new MenuHoldButton(new Vector2(450f, 50f), new Vector2(110f, 30f), resetSignal, InternalTranslator.Translate("RESET CONFIG"));

            this.AddItems(modList, tabCtrler, backButton, saveButton, resetButton);
        }

        public new void Update()
        {
            foreach (UIelement item in this.items) { item.Update(); }
        }

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
            public MenuButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal, text)
            {
            }

            public override void Signal()
            {
                (this.tab as MenuTab).Signal(this.signal);
            }
        }

        internal class MenuHoldButton : OpSimpleHoldButton
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