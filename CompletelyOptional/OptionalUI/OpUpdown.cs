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
        /// <param name="pos"></param>
        /// <param name="sizeX"></param>
        /// <param name="key"></param>
        /// <param name="defaultInt"></param>
        public OpUpdown(Vector2 pos, float sizeX, string key, int defaultInt) : base(pos, sizeX, key, defaultInt.ToString(), Accept.Int)
        {
            if (!_init) { return; }
            throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        public OpUpdown(Vector2 pos, float sizeX, string key, float defaultFloat, int decimalNum) : base(pos, sizeX, key, defaultFloat.ToString(), Accept.Float)
        {
            if (!_init) { return; }
            throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        private void Initialize()
        {
            rectUp = new DyeableRect(menu, owner, this.pos, this.size, true);
            rectDown = new DyeableRect(menu, owner, this.pos, this.size, true);
            subObjects.Add(rectUp); subObjects.Add(rectDown);

            arrUp = new FSprite("Big_Menu_Arrow", true)
            { scale = 0.5f, rotation = 0f, anchorX = 0.5f, anchorY = 0.5f };
            arrDown = new FSprite("Big_Menu_Arrow", true)
            { scale = 0.5f, rotation = 180f, anchorX = 0.5f, anchorY = 0.5f };
            myContainer.AddChild(arrUp); myContainer.AddChild(arrDown);
        }

        public void SetRange(int min, int max)
        {
            iMin = min; iMax = max;
            this.valueInt = Custom.IntClamp(this.valueInt, iMin, iMax);
        }

        public void SetRange(float min, float max)
        {
            fMin = min; fMax = max;
            this.valueFloat = Mathf.Clamp(this.valueInt, fMin, fMax);
        }

        private DyeableRect rectUp, rectDown;
        private FSprite arrUp, arrDown;
        private int iMin, iMax;
        private float fMin, fMax;

        public bool isInt => this.accept == Accept.Int;
    }
}
