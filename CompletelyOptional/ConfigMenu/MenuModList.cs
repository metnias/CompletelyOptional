﻿using OptionalUI;
using UnityEngine;

namespace CompletelyOptional
{
    internal class MenuModList : UIelement, ICanBeFocused
    {
        public MenuModList(MenuTab tab) : base(new Vector2(208f, 40f) - UIelement._offset, new Vector2(250f, 684f))
        {
            tab.AddItems(this);
            rect = new DyeableRect(this.myContainer, new Vector2(-15f, -10f), new Vector2(280f, 705f));

            #region ScrollBox

            contentSize = Mathf.Max(this.size.y, 30f * ConfigContainer.OptItfs.Length);

            GameObject camObj = new GameObject("ComOptModList Camera");
            _cam = camObj.AddComponent<Camera>();

            camPos = new Vector2(-15000f, -10000f);
            childOffset = camPos - new Vector2(Mathf.Round(_offset.x), Mathf.Round(_offset.y));

            targetScrollOffset = -Mathf.Max(contentSize - size.y, 0f);
            scrollOffset = targetScrollOffset;
            UpdateCam();
            OnChange();
            this.GrafUpdate(0f);

            #endregion ScrollBox
        }

        internal MenuTab menuTab => this.tab as MenuTab;
        internal ConfigContainer cfgContainer => menu.cfgContainer;

        private readonly DyeableRect rect;

        public bool GreyedOut => false;

        public bool CurrentlyFocusableMouse => true;

        public bool CurrentlyFocusableNonMouse => true;

        public Rect FocusRect => throw new System.NotImplementedException();

        internal float buttonSlide = 40f, lastButtonSlide = 40f; // -60f (hidden) ~ 40f (slided out)

        internal float GetMySlide(int index, float timeStacker) => Mathf.Clamp(Mathf.Lerp(lastButtonSlide, buttonSlide, timeStacker) + index * 2.0f, 0f, 40f);

        #region ScrollBox

        private readonly float contentSize;
        private FTexture insideTexture;
        private RenderTexture _rt;
        private readonly Camera _cam;
        internal readonly Vector2 camPos, childOffset;
        public float scrollOffset { get; private set; }
        private float targetScrollOffset;

        private void UpdateCam()
        {
            // if (this.isInactive) { return; }
            _cam.aspect = size.x / size.y;
            _cam.orthographic = true;
            _cam.orthographicSize = size.y / 2f;
            _cam.nearClipPlane = 1f;
            _cam.farClipPlane = 100f;
            MoveCam();

            // Render before the main camera
            _cam.depth = -1000;

            // Aquire a render texture
            if ((_rt == null) || (_rt.width != size.x) || (_rt.height != size.y))
            {
                _rt?.Release();
                _rt = new RenderTexture((int)size.x, (int)size.y, 8)
                { filterMode = FilterMode.Point };

                _cam.targetTexture = _rt;

                #region UpdateTexture

                // Create an image to display the box's contents
                if (insideTexture == null)
                {
                    insideTexture = new FTexture(_rt, "modlist") { anchorX = 0f, anchorY = 0f, x = 0f, y = 0f };
                    this.myContainer.AddChild(insideTexture);
                }
                else { insideTexture.SetTexture(_rt); }

                #endregion UpdateTexture
            }
        }

        private void MoveCam()
        {
            Vector3 v = (Vector3)owner.ScreenPos + (Vector3)camPos + new Vector3(size.x / 2f, size.y / 2f, -50f) + Vector3.down * scrollOffset;
            _cam.gameObject.transform.position = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }

        #endregion ScrollBox

        // ModList:
        // ABC button, Mod button shows Name(Left) and Version(right)
        // Save first mod for each letter, and scroll to it
        // if name is too long, add ...
        // Tab: It will now have ^ and v instead of having 20 Limit
        // Also can have a tab that doesn't have button
        // PickUp: Focus/Select, Throw: Unfocus/Leave, Select: Control View
        // Display Unsaved change in button colour

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            rect.GrafUpdate(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            rect.Update();
        }

        /// <summary>
        /// Button for ModList
        /// </summary>
        internal class ModButton : OpSimpleButton
        {
            public ModButton(MenuModList list, int index) : base(Vector2.zero, new Vector2(250f, 24f), "Mod List")
            {
                this.list = list;
                this.index = index; // starts from 1; index 0 is for StatOI
                this._pos = list.camPos;
                this.labelVer = OpLabel.CreateFLabel("v1.0");
                this.myContainer.AddChild(this.labelVer);

                this.list.menuTab.AddItems(this);
                OnChange();
            }

            private readonly MenuModList list;

            public readonly int index;
            private readonly FLabel labelVer;

            public enum State
            {
                Unsupported,
                Idle,
                Selected
            }

            public override void OnChange()
            {
                base.OnChange();
                this.label.alignment = FLabelAlignment.Left;
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
            }

            public override void Update()
            {
                base.Update();
            }

            public override void Signal()
            {
            }

            public override bool CurrentlyFocusableMouse => false;
            public override bool CurrentlyFocusableNonMouse => false; // ModList will take the Focus

            protected internal override bool MouseOver => base.MouseOver;
        }

        internal class StatButton : OpSimpleImageButton
        {
            public StatButton(MenuModList list) : base(Vector2.zero, Vector2.one, "", "Menu_Symbol_Show_List")
            {
                this.list = list;
                this._size = new Vector2(20f, 20f);
                this.rect.size = this.size; this.rectH.size = this.size;

                this.list.menuTab.AddItems(this);
            }

            public readonly MenuModList list;

            public override void OnChange()
            {
                base.OnChange();
                this._size = new Vector2(20f, 20f);
                this.rect.size = this.size; this.rectH.size = this.size;
            }

            public override void Signal()
            {
            }
        }

        internal class AlphabetButton : OpSimpleButton
        {
            public AlphabetButton(MenuModList list, uint index) : base(Vector2.zero, Vector2.one, "", "")
            {
                this.list = list;
                this.index = index;

                this._size = new Vector2(20f, 20f);
                this.greyedOut = ConfigContainer.OptItfABC[index] < 0;

                this.list.menuTab.AddItems(this);
            }

            public readonly uint index;

            public readonly MenuModList list;

            public override bool CurrentlyFocusableMouse => !greyedOut;

            public override bool CurrentlyFocusableNonMouse => !greyedOut;

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);
                rect.Hide(); rectH.Hide();
            }

            public override void Signal()
            {
            }
        }
    }
}