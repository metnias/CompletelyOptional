using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OptionalUI
{
    public class OpSimpleHoldButton : OpSimpleButton
    {
        public OpSimpleHoldButton(Vector2 pos, Vector2 size, string signal, string text = "") : base(pos, size, signal, text)
        {
        }
    }
}