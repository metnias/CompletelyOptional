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
            throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        public OpUpdown(Vector2 pos, float sizeX, string key, float defaultFloat, int decimalNum) : base(pos, sizeX, key, defaultFloat.ToString(), Accept.Float)
        {
            throw new NotImplementedException("OpUpdown might come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        public void SetRange(int min, int max)
        {
        }

        public void SetRange(float min, float max)
        {
        }

        //private DyeableRect rectUp, rectDown;
    }
}
