using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    public class OpLabelLong : OpLabel
    {
        /// <summary>
        /// Special Label that creates multiple <see cref="FLabel"/>s to break Futile's character limits.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size of the Box</param>
        /// <param name="text">Text you want to display</param>
        /// <param name="autoWrap">Whether you want to wrap text automatically.</param>
        /// <param name="alignment">Alignment. Left/Center/Right. Left is default.</param>
        public OpLabelLong(Vector2 pos, Vector2 size, string text = "TEXT", bool autoWrap = true, FLabelAlignment alignment = FLabelAlignment.Left)
            : base(pos, size, text, alignment, false)
        {
            this.autoWrap = autoWrap;
            this.labels = new List<FLabel>();
            this.allowOverflow = true;
            lineHeight = LabelTest.LineHeight(false, isVFont);
            OnChange();
        }

        /// <summary>
        /// if false, text will be cut if it exceeds the size.
        /// </summary>
        public bool allowOverflow;

        /// <summary>
        /// The List of <see cref="FLabel"/> for this
        /// </summary>
        public List<FLabel> labels;

        public override void OnChange()
        {
            base.OnChange();
            string[] lines = this._displayText.Replace(Environment.NewLine, "\n").Split(new char[] { '\n' });
            List<string> splits = new List<string>() { string.Empty };
            int num = 0, lblTxt = 0, lineMax = this.allowOverflow ? int.MaxValue : Mathf.FloorToInt(this.size.y / lineHeight);
            int sLines = 0;
            for (int l = 0; l < lines.Length; l++)
            {
                if (l > lineMax) { break; }
                string a = lines[l].Trim('\n');
                if (string.IsNullOrEmpty(a)) { a = " "; }
                if (lblTxt + a.Length > LabelTest.CharLimit(false, isVFont) || sLines > 8)
                { sLines = 0; lblTxt = 0; num++; splits.Add(string.Empty); }
                if (lblTxt > 0) { splits[num] += '\n'; }
                if (l == lineMax) { a = LabelTest.TrimText(a, this.size.x, true, false); }
                splits[num] += a;
                lblTxt += a.Length + 1;
                sLines++;
            }
            while (this.labels.Count < splits.Count)
            {
                FLabel nl = new FLabel(LabelTest.GetFont(this._bigText, this.isVFont), "")
                {
                    text = "",
                    alignment = this._alignment,
                    color = this.color,
                    y = -10000f
                };
                this.labels.Add(nl);
                this.myContainer.AddChild(nl);
            }

            num = 0; // linebreak sum
            float lh = lineHeight * GetLineCount();
            for (int b = 0; b < labels.Count; b++)
            {
                if (splits.Count <= b) { this.labels[b].text = string.Empty; continue; }
                this.labels[b].text = splits[b];
                int ln = 0; string[] s = splits[b].Split('\n');
                for (int i = 0; i < s.Length; i++) { if (!string.IsNullOrEmpty(s[i])) { ln++; } }
                switch (this._alignment)
                {
                    default:
                    case FLabelAlignment.Center:
                        this.labels[b].alignment = FLabelAlignment.Center;
                        this.labels[b].x = this.size.x / 2f;
                        break;

                    case FLabelAlignment.Left:
                        this.labels[b].alignment = FLabelAlignment.Left;
                        this.labels[b].x = 0f;
                        break;

                    case FLabelAlignment.Right:
                        this.labels[b].alignment = FLabelAlignment.Right;
                        this.labels[b].x = this.size.x;
                        break;
                }
                float sizeY = ln * lineHeight;
                num += ln;
                switch (this.verticalAlignment)
                { // Needs testing
                    default:
                    case LabelVAlignment.Top:
                        this.labels[b].y = this.size.y / 2f + sizeY - num * lineHeight;
                        break;

                    case LabelVAlignment.Bottom:
                        this.labels[b].y = this.size.y / 2f + lh - num * lineHeight;
                        break;

                    case LabelVAlignment.Center:
                        this.labels[b].y = this.size.y / 2f + (sizeY + lh) / 2f - num * lineHeight;
                        break;
                }
            }
        }

        private readonly float lineHeight; // lineHeight of current font

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            Color c;
            if (this.bumpBehav == null) { c = this.color; }
            else { c = this.bumpBehav.GetColor(this.color); }
            foreach (FLabel l in labels) { l.color = c; }
        }
    }
}