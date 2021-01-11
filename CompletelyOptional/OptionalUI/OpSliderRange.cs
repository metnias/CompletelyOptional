using RWCustom;
using System;
using UnityEngine;

namespace OptionalUI
{
    public class OpSliderRange : OpSlider
    {
        public OpSliderRange(Vector2 pos, string key, IntVector2 range, float multi = 1, bool vertical = false, int defaultValue = 0) : base(pos, key, range, multi, vertical, defaultValue)
        {
            throw new NotImplementedException("OpSliderRange may come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        public OpSliderRange(Vector2 pos, string key, IntVector2 range, int length, bool vertical = false, int defaultValue = 0) : base(pos, key, range, length, vertical, defaultValue)
        {
            throw new NotImplementedException("OpSliderRange may come to you, in future! If you're seeing this error as an user, download the latest ConfigMachine.");
        }

        protected internal override void Initialize()
        {
            base.Initialize();
        }
    }
}
