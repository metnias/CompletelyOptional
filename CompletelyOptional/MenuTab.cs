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
            isInactive = false;
            modList = new MenuModList();
            tabCtrler = new ConfigTabController(this);
            backButton = new MenuButton(new Vector2(490f, 50f), new Vector2(120f, 30f), backSignal, InternalTranslator.Translate("BACK"));
            saveButton = new MenuButton(new Vector2(630f, 50f), new Vector2(120f, 30f), saveSignal, InternalTranslator.Translate("APPLY"));
            resetButton = new MenuHoldButton(new Vector2(770f, 50f), new Vector2(120f, 30f), resetSignal, InternalTranslator.Translate("RESET CONFIG"));

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
            public MenuButton(Vector2 pos, Vector2 size, string signal, string id, string ver = "") : base(pos, size, signal, id)
            {
                this.ver = ver;
                labelVer = new FLabel(LabelTest.GetFont(false, false), ver);
                this.myContainer.AddChild(labelVer);
                SetLabelsPos();
            }

            private void SetLabelsPos()
            {
                this.label.alignment = FLabelAlignment.Left;
                this.label.x = 9f; this.label.y = this.size.y / 2f;
                this.labelVer.alignment = FLabelAlignment.Right;
                this.labelVer.x = this.size.x - 9f; this.labelVer.y = this.size.y / 2f;
            }

            private readonly FLabel labelVer;
            internal string ver;

            public override void OnChange()
            {
                base.OnChange();
                label.text = this.text;
                labelVer.text = this.ver;
                SetLabelsPos();
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