using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;
using Menu;

namespace OptionalUI
{
    /// <summary>
    /// [DO NOT USE]
    /// Creature Preview.
    /// </summary>
    public class UIcreature : UIelement
    {
        /// <summary>
        /// Do not use this directly.
        /// Size is fixed to 150f, 150f.
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="type">Creature Type</param>
        public UIcreature(Vector2 pos, CreatureTemplate.Type type) : base(pos, new Vector2(150f, 150f))
        {
            _inBox = true;
            if (!init) { return; }

            this.rect = new DyeableRect(menu, owner, pos + offset, size, true);
            this.subObjects.Add(this.rect);
            //owner.subObjects.Add(this.menuObj);

            this._type = type;

            cage = new FContainer();
            this.myContainer.AddChild(cage);
            cage.SetPosition(new Vector2(75f, 20f));
            fence = new FContainer();
            this.myContainer.AddChild(fence);
            fence.SetPosition(new Vector2(75f, 20f));
            fence.MoveToFront();

            absTemplate = new OptionalCreatureTemplate(type);
            absCreature = new OptionalAbstractCreature(world, absTemplate);
            absCreature.state = new OptionalCreatureState(absCreature);
        }

        public DyeableRect rect;
        private CreatureTemplate.Type _type;
        public OptionalAbstractCreature absCreature;
        public OptionalCreatureTemplate absTemplate;
        public GraphicsModule graphic;
        public Creature creature;
        public static World world
        {
            get
            {
                return CompletelyOptional.OptionScript.optionalWorld;
            }
        }
        public FContainer cage;
        public FContainer fence;

        public bool inBox
        {
            get { return _inBox; }
            set
            {
                if (_inBox != value)
                {
                    _inBox = value;
                    OnChange();
                }
            }
        }
        private bool _inBox;

        public override void Update(float dt)
        {
            base.Update(dt);
            try
            {
                creature.Update(true);
                graphic.DrawSprites(null, null, dt, new Vector2(0f, 0f));
                graphic.Update();
            }
            catch (Exception ex) { Debug.LogError(ex); }
        }

        public override void OnChange()
        {
            if (!init) { return; }
            base.OnChange();

            if (_inBox)
            {
                if (this.rect == null)
                {
                    this.rect = new DyeableRect(menu, owner, pos - new Vector2(size.x * 0.5f, 0f), size, true);
                    this.subObjects.Add(rect);
                    owner.subObjects.Add(rect);
                }
            }
            else if (this.rect != null)
            {
                this.subObjects.Remove(rect);
                owner.subObjects.Remove(rect);
                this.rect = null;
            }
        }

        public override void Hide()
        {
            base.Hide();
            if (_inBox && this.rect != null)
            {
                foreach (FSprite sprite in (this.rect as RoundedRect).sprites)
                {
                    sprite.isVisible = false;
                }
            }
        }

        public override void Show()
        {
            base.Show();
            if (_inBox && this.rect != null)
            {
                foreach (FSprite sprite in (this.rect as RoundedRect).sprites)
                {
                    sprite.isVisible = true;
                }
            }
        }

        public override void Unload()
        {
            base.Unload();
            if (this.rect != null)
            {
                this.subObjects.Remove(rect);
            }

            this.myContainer.RemoveChild(cage);
            creature.Destroy();
        }

        public class OptionalAbstractCreature : AbstractCreature
        {
            public OptionalAbstractCreature(World world, CreatureTemplate creatureTemplate) : base(world, creatureTemplate, null, CompletelyOptional.OptionScript.optionalCoordinate, new EntityID())
            {
            }
        }

        public class OptionalCreatureTemplate : CreatureTemplate
        {
            public OptionalCreatureTemplate(Type type) : base(type, null, new List<TileTypeResistance>(0), new List<TileConnectionResistance>(0), new CreatureTemplate.Relationship())
            {
                this.AI = false;
                this.grasps = 0;
                this.meatPoints = 0;
                this.socialMemory = false;
            }
        }

        public class OptionalCreatureState : CreatureState
        {
            public OptionalCreatureState(AbstractCreature creature) : base(creature)
            {
            }
        }
    }
}