using Menu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalUI
{
    public class OpLabelLong : OpLabel
    {
        /// <summary>
        /// Special Label that creates multiple <see cref="MenuLabel"/>s to break Futile's character limits.
        /// </summary>
        /// <param name="pos">BottomLeft Position</param>
        /// <param name="size">Size of the Box</param>
        /// <param name="text">Text you want to display</param>
        /// <param name="autoWrap">Whether you want to wrap text automatically.</param>
        /// <param name="alignment">Alignment. Left/Center/Right. Left is default.</param>
        public OpLabelLong(Vector2 pos, Vector2 size, string text = "TEXT", bool autoWrap = true, FLabelAlignment alignment = FLabelAlignment.Left) : base(pos, size, text, alignment, false)
        {
            this.autoWrap = autoWrap;
            this.labels = new List<MenuLabel>();
            this.allowOverflow = true;
            if (!_init) { return; }
            OnChange();
        }

        /// <summary>
        /// if false, text will be cut if it exceeds the size.
        /// </summary>
        public bool allowOverflow;

        /// <summary>
        /// The List of <see cref="MenuLabel"/> for this
        /// </summary>
        public List<MenuLabel> labels;

        public override void OnChange()
        {
            if (this.labels == null || !_init) { return; }
            base.OnChange();
            float lineHeight = LabelTest.LineHeight(false, isVFont);
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
                splits[num] += a;
                lblTxt += a.Length + 1;
                sLines++;
            }
            while (this.labels.Count < splits.Count)
            {
                MenuLabel nl = new MenuLabel(this.menu, this.owner, isVFont ? "A" : "", this.pos, this.size, false)
                { text = "" };
                this.labels.Add(nl);
                this.subObjects.Add(nl);
            }
            num = 0; // linebreak sum
            float lh = lineHeight * GetLineCount();
            for (int b = 0; b < labels.Count; b++)
            {
                if (splits.Count <= b) { this.labels[b].text = string.Empty; continue; }
                int ln = 0; string[] s = splits[b].Split('\n');
                for (int i = 0; i < s.Length; i++) { if (!string.IsNullOrEmpty(s[i])) { ln++; } }
                switch (this._alignment)
                {
                    default:
                    case FLabelAlignment.Center:
                        this.labels[b].label.alignment = FLabelAlignment.Center;
                        this.labels[b].pos.x = this.pos.x;
                        break;

                    case FLabelAlignment.Left:
                        this.labels[b].label.alignment = FLabelAlignment.Left;
                        this.labels[b].pos.x = this.pos.x - this.size.x * 0.5f;
                        break;

                    case FLabelAlignment.Right:
                        this.labels[b].label.alignment = FLabelAlignment.Right;
                        this.labels[b].pos.x = this.pos.x + this.size.x * 0.5f;
                        break;
                }
                this.labels[b].size.x = this._size.x;
                this.labels[b].size.y = ln * lineHeight;
                num += ln;
                switch (this.verticalAlignment)
                {
                    default:
                    case LabelVAlignment.Top:
                        this.labels[b].pos.y = this.pos.y + this.size.y - num * lineHeight;
                        break;

                    case LabelVAlignment.Bottom:
                        this.labels[b].pos.y = this.pos.y + lh - num * lineHeight;
                        break;

                    case LabelVAlignment.Center:
                        this.labels[b].pos.y = this.pos.y + (this.size.y + lh) / 2f - num * lineHeight;
                        break;
                }
                this.labels[b].text = splits[b];
            }
        }

        public override void GrafUpdate(float dt)
        {
            base.GrafUpdate(dt);
            if (!_init || isHidden) { return; }
            Color c;
            if (this.bumpBehav == null) { c = this.color; }
            else { c = this.bumpBehav.GetColor(this.color); }
            foreach (MenuLabel l in labels) { l.label.color = c; }
        }

        public override void Hide()
        {
            base.Hide();
            foreach (MenuLabel l in labels) { l.label.isVisible = false; }
        }

        public override void Show()
        {
            base.Show();
            foreach (MenuLabel l in labels) { l.label.isVisible = true; }
        }
    }
}