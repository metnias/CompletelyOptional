using RWCustom;
using System;
using UnityEngine;

namespace OptionalUI
{
    public class OpUpdown : OpTextBox
    {
        /// <summary>
        /// Numeric Updown (Spinner)
        /// </summary>
        /// <param name="pos">LeftBottom <see cref="UIelement.pos"/>.</param>
        /// <param name="sizeX">Horizontal size (min = 40 pxl). The height is fixed to 30 pxl.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultInt">Default int value</param>
        public OpUpdown(Vector2 pos, float sizeX, string key, int defaultInt) : base(pos, sizeX, key, defaultInt.ToString(), Accept.Int)
        {
            if (!_init) { return; }
            this.Initialize();
            // throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        /// <summary>
        /// Numeric Updown (Spinner)
        /// </summary>
        /// <param name="pos">LeftBottom <see cref="UIelement.pos"/>.</param>
        /// <param name="sizeX">Horizontal size (min = 40 pxl). The height is fixed to 30 pxl.</param>
        /// <param name="key">Unique <see cref="UIconfig.key"/></param>
        /// <param name="defaultFloat">Defalut float value</param>
        /// <param name="decimalNum">The number of decimal numbers after decimal point. 0 - 9. (e.g. decimalNum 1 => XX.X)</param>
        public OpUpdown(Vector2 pos, float sizeX, string key, float defaultFloat, byte decimalNum = 1) : base(pos, sizeX, key, defaultFloat.ToString(), Accept.Float)
        {
            dNum = Math.Min(decimalNum, (byte)9);
            if (!_init) { return; }
            this.Initialize();
            // throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        private void Initialize()
        {
            // rectUp = new DyeableRect(menu, owner, this.pos + new Vector2(this.size.x - 40f, 3f), new Vector2(16f, 24f), true);
            // rectDown = new DyeableRect(menu, owner, this.pos + new Vector2(this.size.x - 20f, 3f), new Vector2(16f, 24f), true);
            // subObjects.Add(rectUp); subObjects.Add(rectDown);

            arrUp = new FSprite("Big_Menu_Arrow", true)
            { scale = 0.5f, rotation = 0f, anchorX = 0.5f, anchorY = 0.5f, x = this.size.x - 15f, y = 20f, color = this.colorText };
            arrDown = new FSprite("Big_Menu_Arrow", true)
            { scale = 0.5f, rotation = 180f, anchorX = 0.5f, anchorY = 0.5f, x = this.size.x - 15f, y = 10f, color = this.colorText };
            myContainer.AddChild(arrUp); myContainer.AddChild(arrDown);

            bumpUp = new BumpBehaviour(this); bumpDown = new BumpBehaviour(this);
            OnChange();
        }

        //private DyeableRect rectUp, rectDown;
        private FSprite arrUp, arrDown;
        private int iMin = int.MinValue, iMax = int.MaxValue;
        private float fMin = float.MinValue, fMax = float.MaxValue;
        protected internal readonly byte dNum;
        private BumpBehaviour bumpUp, bumpDown;

        public override void OnChange()
        {
            if (!KeyboardOn) { ClampValue(); }
            base.OnChange();
            // rectUp.pos = this.pos + new Vector2(this.size.x - 40f, 3f);
            // rectDown.pos = this.pos + new Vector2(this.size.x - 20f, 3f);
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            arrUp.color = this.colorText; arrDown.color = this.colorText;
        }

        public override void Update(float dt)
        {
            if (!this.disabled)
            {
                bumpUp.greyedOut = this.greyedOut; bumpDown.greyedOut = this.greyedOut;
                bumpUp.MouseOver = false; bumpDown.MouseOver = false;
                if (this.MousePos.x > this.size.x - 25f && this.MousePos.x < this.size.x - 5f)
                {
                    if (this.MousePos.y > 5f)
                    {
                        if (this.MousePos.y < 15f) { bumpDown.MouseOver = true; }
                        else if (this.MousePos.y < 25f) { bumpUp.MouseOver = true; }
                    }
                }
            }
            else
            {
                bumpUp.held = false; bumpDown.held = false;
                bumpUp.greyedOut = true; bumpDown.greyedOut = true;
                bumpUp.MouseOver = false; bumpDown.MouseOver = false;
            }
            base.Update(dt);
            bumpUp.Update(dt); bumpDown.Update(dt);
            if (this.disabled) { return; }
        }

        /// <summary>
        /// Sets range for <see cref="UIconfig.valueInt"/>. Use for <see cref="int"/> version of <see cref="OpUpdown"/>
        /// </summary>
        /// <param name="intMin">Minimum value (default: <see cref="int.MinValue"/>)</param>
        /// <param name="intMax">Maximum value (default: <see cref="int.MaxValue"/>)</param>
        public void SetRange(int intMin, int intMax)
        {
            if (!IsInt) { SetRange(floatMin: intMin, floatMax: intMax); return; }
            iMin = intMin; iMax = Math.Max(iMin, intMax);
            ClampValue();
        }

        /// <summary>
        /// Sets range for <see cref="UIconfig.valueFloat"/>. Use for <see cref="float"/> version of <see cref="OpUpdown"/>
        /// </summary>
        /// <param name="floatMin">Minimum value (default: <see cref="float.MinValue"/>)</param>
        /// <param name="floatMax">Maximum value (default: <see cref="float.MaxValue"/>)</param>
        public void SetRange(float floatMin, float floatMax)
        {
            if (IsInt) { SetRange(intMin: Mathf.CeilToInt(floatMin), intMax: Mathf.FloorToInt(floatMax)); return; }
            fMin = floatMin; fMax = Mathf.Max(fMin, floatMax);
            ClampValue();
        }

        /// <summary>
        /// Whether this is initialized with int ctor or float ctor
        /// </summary>
        public bool IsInt => this.accept == Accept.Int;

        public override void Show()
        {
            base.Show();
            // rectUp.Show(); rectDown.Show();
        }

        public override void Hide()
        {
            base.Hide();
            // rectUp.Hide(); rectDown.Hide();
        }

        private void ClampValue()
        {
            if (IsInt)
            { this.valueInt = Custom.IntClamp(this.valueInt, iMin, iMax); }
            else
            { this.value = Mathf.Clamp(this.valueFloat, fMin, fMax).ToString("F" + dNum.ToString()); }
        }
    }
}
