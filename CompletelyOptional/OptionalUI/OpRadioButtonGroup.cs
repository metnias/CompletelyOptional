using Menu;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    /// <summary>
    /// Ties a number of <see cref="OpRadioButton"/> together, so only one of them can be activated. See remarks for detail.
    /// </summary>
    /// <remarks>
    /// Initialize this then <see cref="OpTab.AddItems(UIelement[])"/> to make it functional.
    /// Then Initialize <see cref="OpRadioButton"/> instances, then use <see cref="OpRadioButtonGroup.SetButtons(OpRadioButton[])"/> to bind them.
    /// </remarks>
    public class OpRadioButtonGroup : UIconfig
    {
        /// <summary>
        /// Ties a number of <see cref="OpRadioButton"/> together, so only one of them can be activated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        public OpRadioButtonGroup(string key, int defaultValue = 0) : base(Vector2.zero, new Vector2(1f, 1f), key, defaultValue.ToString())
        {
            this._value = defaultValue.ToString();
            this.defaultValue = this.value;
            if (!_init) { return; }
            this._greyedOut = false;

            this.subObjects = new List<PositionedMenuObject>(0);
        }

        /// <summary>
        /// Bind <see cref="OpRadioButton"/> to this group.
        /// <para>This will call <see cref="OpTab.AddItems(UIelement[])"/> or <see cref="OpScrollBox.AddItems(UIelement[])"/> for <see cref="OpRadioButton"/>s automatically.</para>
        /// </summary>
        /// <remarks>Example (See also <seealso cref="OpRadioButton(float, float)"/>): <code>
        /// OpRadioButtonGroup group = new OpRadioButtonGroup("MyModChoice");
        /// Tabs[0].AddItems(group);
        /// group.SetButtons(new OpRadioButton[] { new OpRadioButton(300f, 450f), new OpRadioButton(350f, 450f) });
        /// </code></remarks>
        /// <param name="buttons">Array of OpRadioButton</param>
        public virtual void SetButtons(OpRadioButton[] buttons)
        {
            this.buttons = buttons;

            if (this.inScrollBox) { this.scrollBox.AddItems(this.buttons); }
            else if (this.tab != null) { this.tab.AddItems(this.buttons); }

            for (int i = 0; i < buttons.Length; i++)
            {
                this.buttons[i].group = this;
                this.buttons[i].index = i;
                if (i == base.valueInt) { this.buttons[i]._value = "true"; }
                else { this.buttons[i]._value = "false"; }
                this.buttons[i].OnChange();
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (this.held) { foreach (OpRadioButton b in this.buttons) { b.Update(dt); } }
        }

        /// <summary>
        /// Whether this RadioButtonGroup is greyedOut or not
        /// Changing this overrides original <see cref="OpRadioButton.greyedOut"/>.
        /// </summary>
        public new bool greyedOut
        {
            get { return _greyedOut; }
            set
            {
                _greyedOut = value;
                for (int i = 0; i < this.buttons.Length; i++)
                { this.buttons[i].greyedOut = value; }
            }
        }

        internal bool _greyedOut;

        /// <summary>
        /// Set all <see cref="OpRadioButton.colorEdge"/> to new one
        /// </summary>
        /// <param name="newColor">New Edge and Symbol Color</param>
        public void SetColorEdge(Color newColor)
        {
            foreach (OpRadioButton button in this.buttons)
            { button.colorEdge = newColor; }
        }

        /// <summary>
        /// Set all <see cref="OpRadioButton.colorFill"/> to new one
        /// </summary>
        /// <param name="newColor">New Fill Color</param>
        public void SetColorFill(Color newColor)
        {
            foreach (OpRadioButton button in this.buttons)
            { button.colorFill = newColor; }
        }

        public OpRadioButton[] buttons;

        public new int valueInt
        {
            get
            {
                return int.Parse(this.value);
            }
            set
            {
                if (base.value == value.ToString()) { return; }
                if (value >= this.buttons.Length) { return; }
                this._value = value.ToString();
                if (_init) { Switch(value); }
            }
        }

        public override string value
        {
            get => base.value;
            set
            {
                if (base.value == value) { return; }
                if (int.TryParse(value, out int v))
                {
                    if (v >= this.buttons.Length) { return; }
                    this._value = value;
                    if (_init) { Switch(v); }
                }
            }
        }

        protected internal virtual void Switch(int index)
        {
            for (int i = 0; i < this.buttons.Length; i++)
            {
                this.buttons[i]._value = "false";
                this.buttons[i].OnChange();
            }
            this.buttons[index]._value = "true";
            this.buttons[index].OnChange();
            base.value = index.ToString();
            this.OnChange();
        }

        public override void OnChange()
        {
            if (!_init) { return; }
            base.OnChange();
        }

        /// <summary>
        /// Calls <see cref="OpRadioButton.Hide"/> for its children
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            for (int i = 0; i < this.buttons.Length; i++)
            { this.buttons[i].Hide(); }
        }

        /// <summary>
        /// Calls <see cref="OpRadioButton.Show"/> for its children
        /// </summary>
        public override void Show()
        {
            base.Show();
            for (int i = 0; i < this.buttons.Length; i++)
            { this.buttons[i].Show(); }
        }
    }
}