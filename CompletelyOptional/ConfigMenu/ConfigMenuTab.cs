using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    /// <summary>
    /// Special kind of OpTab for ConfigMenu. You don't need this.
    /// </summary>
    internal class ConfigMenuTab : MenuTab
    {
        internal ConfigMenuTab() : base()
        {
            // 171 offset from left
            this.container.MoveToBack();
            backButton = new OpSimpleButton(new Vector2(490f, 50f), new Vector2(120f, 30f), backSignal, OptionalText.GetText(OptionalText.ID.ConfigMenuTab_Back));
            saveButton = new OpSimpleButton(new Vector2(630f, 50f), new Vector2(120f, 30f), saveSignal, OptionalText.GetText(OptionalText.ID.ConfigMenuTab_Apply));
            resetButton = new OpHoldButton(new Vector2(770f, 50f), new Vector2(120f, 30f), resetSignal, OptionalText.GetText(OptionalText.ID.ConfigMenuTab_Reset));
            this.AddItems(backButton, saveButton, resetButton);
            modList = new MenuModList(this);
            tabCtrler = new ConfigTabController(this);
        }

        internal MenuModList modList;

        internal ConfigTabController tabCtrler;

        internal OpSimpleButton backButton, saveButton;

        internal OpHoldButton resetButton;

        internal const string backSignal = "CANCEL", saveSignal = "APPLY", resetSignal = "RESET";

        protected internal override void Signal(UItrigger trigger, string signal)
        {
            switch (signal)
            {
                case backSignal:
                case saveSignal:
                case resetSignal:
                    ModConfigMenu.instance.Singal(null, signal);
                    break;
            }
        }
    }
}