using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using UnityEngine;
using RWCustom;

namespace CompletelyOptional
{
    public class SoundTestPlayerUI : RectangularMenuObject
    {
        public SoundTestPlayerUI(Menu.Menu menu, MenuObject owner, SoundTestOwner testOwner) : base(menu, owner, new Vector2(-1000f, -1000f), Vector2.zero)
        {
            this.lastPos = new Vector2(-1000f, -1000f);
            this.testOwner = testOwner;
            testOwner.ui = this;

            this.infoLabel = new MenuLabel(menu, this, "PlayerUI", new Vector2(this.size.x / 2f - 100f, 0f), new Vector2(200f, 20f), true);
            this.subObjects.Add(this.infoLabel);

            this.slugButton = new SlugButton(menu, this);
            this.subObjects.Add(this.slugButton);
        }

        public SoundTestOwner testOwner;
        public float visFac;
        public float lastVisFac;
        public MenuLabel infoLabel;
        public bool currentlyVisible = true;

        public SlugButton slugButton;

        public override void Update()
        {
            this.lastVisFac = this.visFac;
            base.Update();
            this.visFac = Custom.LerpAndTick(this.visFac, (!this.currentlyVisible) ? 0f : 1f, 0.03f, 0.05f);
            this.pos.x = this.testOwner.room.game.cameras[0].sSize.x * 0.5f - this.size.x * 0.5f + Menu.Menu.HorizontalMoveToGetCentered(this.menu.manager) - 300f;
            this.pos.y = 84f; //Mathf.Lerp(-40f - this.size.y, 5f, Custom.SCurve(Mathf.Pow(this.visFac, 0.6f), 0.75f));
            this.infoLabel.pos = new Vector2(200f, 300f);
            this.slugButton.pos = new Vector2(300f, 300f);
        }

        public class SlugButton : CircularMenuObject
        {
            public SlugButton(Menu.Menu menu, MenuObject owner) : base(menu, owner, new Vector2(300f, 300f), 50f)
            {
                //this.behavior = this.oracle.oracleBehavior;
                //this.graphics = this.oracle.graphicsModule as MaxGraphics;
            }

            public override void Update()
            {
                if (OptionScript.game != null && OptionScript.oracle != null)
                {
                    if (this.oracle != null) { goto initSkip; }
                    this.oracle = OptionScript.oracle;
                    this.room = this.oracle.room;
                    this.behavior = this.oracle.oracleBehavior;
                    this.graphics = this.oracle.graphicsModule as MaxGraphics;
                }
                else
                {
                    return;
                }

            initSkip:
                base.Update();
                if (this.MouseOver)
                {
                    Vector2 vector = (Vector2)Input.mousePosition + oracle.room.game.cameras[0].pos;
                    this.graphics.objectLooker.LookAtPoint(vector, 1f);
                    this.room.AddObject(new MouseSpark(vector, Custom.RNV() * UnityEngine.Random.value * 5f, 50f, new HSLColor(UnityEngine.Random.value, 1f, 0.5f).rgb));
                }
            }

            public MaxOracle oracle;
            public MaxOracleBehavior behavior;
            public MaxGraphics graphics;
            public Room room;
        }
    }
}