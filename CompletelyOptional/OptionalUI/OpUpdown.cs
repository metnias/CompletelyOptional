using CompletelyOptional;
using OptionalUI.ValueTypes;
using RWCustom;
using System;
using UnityEngine;

namespace OptionalUI
{
    public class OpUpdown : OpTextBox, IValueInt
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

            this.Initialize();
            // throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        private void Initialize()
        {
            // rectUp = new DyeableRect(menu, owner, this.pos + new Vector2(this.size.x - 40f, 3f), new Vector2(16f, 24f), true);
            // rectDown = new DyeableRect(menu, owner, this.pos + new Vector2(this.size.x - 20f, 3f), new Vector2(16f, 24f), true);
            // subObjects.Add(rectUp); subObjects.Add(rectDown);

            this.description = InternalTranslator.Translate("Click and Type numbers, Use Arrows or Scrollwheel to adjust");

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
        private float fMin = -1000000, fMax = 1000000;
        protected internal readonly byte dNum;
        private BumpBehaviour bumpUp, bumpDown;
        protected internal bool mouseOverArrow;
        private int arrowCounter = 0, bumpCount, bumpDeci;
        private Vector2 scrollHeldPos; private float scrollHeldTickPos;

        public override void OnChange()
        {
            if (!KeyboardOn) { ClampValue(); }
            base.OnChange();
            // rectUp.pos = this.pos + new Vector2(this.size.x - 40f, 3f);
            // rectDown.pos = this.pos + new Vector2(this.size.x - 20f, 3f);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            arrUp.color = bumpUp.GetColor(this.colorText);
            arrDown.color = bumpDown.GetColor(this.colorText);
        }

        public override void Update()
        {
            if (greyedOut)
            {
                mouseOverArrow = false;
                bumpUp.held = false; bumpDown.held = false;
                bumpUp.greyedOut = true; bumpDown.greyedOut = true;
                bumpUp.MouseOver = false; bumpDown.MouseOver = false;
            }
            else
            {
                mouseOverArrow = false;
                bumpUp.greyedOut = this.greyedOut; bumpDown.greyedOut = this.greyedOut;
                bumpUp.MouseOver = false; bumpDown.MouseOver = false;
                if (this.MousePos.x > this.size.x - 25f && this.MousePos.x < this.size.x - 5f)
                {
                    if (this.MousePos.y > 5f)
                    {
                        if (this.MousePos.y < 15f) { bumpDown.MouseOver = true; mouseOverArrow = true; }
                        else if (this.MousePos.y < 25f) { bumpUp.MouseOver = true; mouseOverArrow = true; }
                    }
                }
            }
            base.Update();
            bumpUp.Update(); bumpDown.Update();
            if (this.greyedOut) { return; }
            byte bumpFlag = 200; // 200: no bump; 0: bump fail; 1: bump up; 2: bump down
            if (held && !KeyboardOn)
            {
                mouseDown = Input.GetMouseButton(arrowCounter > 0 ? 0 : 2);
                if (mouseDown)
                {
                    if (arrowCounter > 0) // arrow holding mode
                    {
                        arrowCounter--;
                        if (arrowCounter < 1)
                        {
                            arrowCounter = FrameMultiply(8);
                            bumpFlag = (byte)(TryBump(bumpUp.held) ? (bumpUp.held ? 1 : 2) : 0);
                            if (bumpCount >= 10) { bumpCount = 1; bumpDeci++; }
                        }
                    }
                    else // scrollwheel holding mode
                    {
                        if (Mathf.Abs(MousePos.y - scrollHeldTickPos) > 10f)
                        {
                            bool up = MousePos.y - scrollHeldTickPos > 0f;
                            scrollHeldTickPos = MousePos.y;
                            int deci = Custom.IntClamp(Mathf.FloorToInt((Mathf.Abs(MousePos.x - scrollHeldPos.x) - 50f) / 30f), 0, 9);
                            if (IsInt)
                            {
                                int old = valueInt;
                                valueInt += (int)Mathf.Pow(10, deci) * (up ? 1 : -1);
                                // ComOptPlugin.LogInfo($"deci[{deci}]: {old} > {valueInt} ({(int)Mathf.Pow(10, deci) })");
                                if (old != valueInt) { bumpFlag = (byte)(up ? 1 : 2); }
                                else { bumpFlag = 0; }
                            }
                            else
                            {
                                float old = valueFloat;
                                valueFloat += Mathf.Pow(10, deci - dNum) * (up ? 1 : -1);
                                // ComOptPlugin.LogInfo($"deci[{deci - dNum}]: {old} > {valueFloat} ({Mathf.Pow(10, deci - dNum) })");
                                if (old != valueFloat) { bumpFlag = (byte)(up ? 1 : 2); }
                                else { bumpFlag = 0; }
                            }
                        }
                    }
                }
                else
                {
                    //exit
                    held = false;
                    arrowCounter = 0;
                    bumpUp.held = false; bumpDown.held = false;
                }
            }
            else if (this.MouseOver && this.menu.mouseScrollWheelMovement != 0) // scroll mode
            {
                bumpDeci = IsInt ? 0 : -dNum;
                bumpFlag = (byte)(TryBump(this.menu.mouseScrollWheelMovement < 0f) ? (this.menu.mouseScrollWheelMovement < 0f ? 1 : 2) : 0);
            }
            if (this.MouseOver && !mouseDown)
            {
                if (mouseOverArrow && Input.GetMouseButton(0))
                {
                    mouseDown = true; this.held = true;
                    if (bumpUp.MouseOver) { bumpUp.held = true; bumpUp.flash = 2f; }
                    else { bumpDown.held = true; bumpDown.flash = 2f; }
                    arrowCounter = FrameMultiply(24);
                    bumpCount = 0;
                    bumpDeci = IsInt ? 0 : -dNum;
                    bumpFlag = (byte)(TryBump(bumpUp.held) ? (bumpUp.held ? 1 : 2) : 0);
                }
                else if (Input.GetMouseButton(2))
                {
                    mouseDown = true; this.held = true;
                    scrollHeldPos = MousePos;
                    scrollHeldTickPos = scrollHeldPos.y;
                    arrowCounter = -1;
                    PlaySound(SoundID.MENU_First_Scroll_Tick);
                }
            }

            #region BumpFlagReaction

            if (bumpFlag < 200)
            {
                switch (bumpFlag)
                {
                    case 0: // bump fail
                        PlaySound(arrowCounter > 0 ? SoundID.MENU_Checkbox_Uncheck : SoundID.MENU_Scroll_Tick, 0f, 1f, 0.7f);
                        break;

                    case 1: // bump up
                    case 2: // bump down
                        this.bumpBehav.flash = 1f;
                        PlaySound(arrowCounter > 0 ? SoundID.MENU_Checkbox_Uncheck : SoundID.MENU_Scroll_Tick);
                        if (bumpFlag == 1) { bumpUp.flash += 0.7f; }
                        else { bumpDown.flash += 0.7f; }
                        break;
                }
                this.bumpBehav.sizeBump = Mathf.Min(2.5f, this.bumpBehav.sizeBump + 1f);
            }

            #endregion BumpFlagReaction
        }

        private bool TryBump(bool plus)
        {
            if (IsInt)
            {
                int old = this.valueInt;
                this.valueInt += (plus ? 1 : -1) * Mathf.RoundToInt(Mathf.Pow(10f, bumpDeci));
                if (old != this.valueInt) { bumpCount++; return true; }
                return false;
            }
            else
            {
                float old = this.valueFloat;
                this.valueFloat += (plus ? 1f : -1f) * Mathf.Pow(10f, bumpDeci);
                if (old != this.valueFloat) { bumpCount++; return true; }
                return false;
            }
        }

        /// <summary>
        /// Sets range for <see cref="UIconfig.value"/>. Use for <see cref="int"/> version of <see cref="OpUpdown"/>
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
        /// Sets range for <see cref="UIconfig.value"/>. Use for <see cref="float"/> version of <see cref="OpUpdown"/>
        /// </summary>
        /// <param name="floatMin">Minimum value (default: -1,000,000)</param>
        /// <param name="floatMax">Maximum value (default: 1,000,000)</param>
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

        #region IValueType

        string IValueType.valueString { get => this.value; set => this.value = value; }

        public void SetValueFloat(float value) => this.valueFloat = value;

        public float GetValueFloat() => this.valueFloat;

        public void SetValueInt(int value) => this.valueInt = value;

        public int GetValueInt() => this.valueInt;

        #endregion IValueType

        private void ClampValue()
        {
            if (IsInt)
            { this.valueInt = Custom.IntClamp(this.valueInt, iMin, iMax); }
            else
            { this.value = Mathf.Clamp(this.valueFloat, fMin, fMax).ToString("F" + dNum.ToString()); }
        }
    }
}