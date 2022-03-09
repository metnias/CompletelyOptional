using System;
using System.Collections.Generic;
using CoralBrain;
using HUD;
using Noise;
using RWCustom;
using UnityEngine;

namespace OptionalUI
{
    public class OpSlugcat : UIcreature
    {
        /// <summary>
        /// [DO NOT USE]
        /// Add Slugcat Preview. Size is fixed to 150x150.
        /// Using this is not recommended. This doesn't even sync w/ real Player.
        /// Need to be reworked, or removed.
        /// </summary>
        /// <param name="pos">LeftBottom Position</param>
        public OpSlugcat(Vector2 pos) : base(pos, CreatureTemplate.Type.Slugcat)
        {
            type = 0;

            //creature = FormatterServices.GetUninitializedObject(typeof(OptionalSlugcat)) as OptionalSlugcat;
            //(creature as OptionalSlugcat).Init();

            if (!init) { return; }

            creature = new OptionalSlugcat(absCreature, world);
            graphic = new OptionalSlugcatGraphics(creature);
            (graphic as OptionalSlugcatGraphics).cage = cage;
            (graphic as OptionalSlugcatGraphics).fence = fence;
            graphic.InitiateSprites(null, null);
            graphic.Reset();

            bodymode = OptionalSlugcat.BodyModeIndex.Default;
            animode = OptionalSlugcat.AnimationIndex.StandUp;

            doWiggle = false;

            this.description = "It is hungry, find food";
        }

        public OptionalSlugcat.BodyModeIndex bodymode
        {
            get { return (creature as OptionalSlugcat).bodyMode; }
            set { (creature as OptionalSlugcat).bodyMode = value; }
        }
        /// <summary>
        /// [Not Working!] Set Slugcat Animation.
        /// </summary>
        public OptionalSlugcat.AnimationIndex animode
        {
            get { return (creature as OptionalSlugcat).animation; }
            set { (creature as OptionalSlugcat).animation = value; }
        }
        /// <summary>
        /// [Not Working!] Make Slugcat Wiggle
        /// </summary>
        public bool doWiggle
        {
            get { return (creature as OptionalSlugcat).doWiggle; }
            set { (creature as OptionalSlugcat).doWiggle = value; }
        }

        /// <summary>
        /// Set Slugcat Type. 0: White, 1: Yellow, 2: Red
        /// </summary>
        public int type
        {
            get { return _type; }
            set
            {
                if (value < 0 || value > 2) { return; } //Invalid
                if (_type != value)
                {
                    _type = value;
                    OnChange();
                }
            }
        }
        public int _type = 0;

        public override void Unload()
        {
            OptionalSlugcatGraphics slugraph = (graphic as OptionalSlugcatGraphics);
            foreach (FSprite sprite in slugraph.sprites)
            {
                sprite.RemoveFromContainer();
            }
            slugraph.cage.RemoveFromContainer();
            slugraph.fence.RemoveFromContainer();

            base.Unload();
        }

        public override void OnChange()
        {
            if (!init) { return; }
            base.OnChange();

            OptionalSlugcat sluggo = this.creature as OptionalSlugcat;
            switch (_type)
            {
                default:
                case 0: sluggo.slugcatStats.name = SlugcatStats.Name.White; break;
                case 1: sluggo.slugcatStats.name = SlugcatStats.Name.Yellow; break;
                case 2: sluggo.slugcatStats.name = SlugcatStats.Name.Red; break;
            }

            //sluggo.graphicsModule.Reset();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            (creature as OptionalSlugcat).MouseOver = this.MouseOver;
            (graphic as OptionalSlugcatGraphics).MouseOver = this.MouseOver;

            if (this.MouseOver)
            {
                (creature as OptionalSlugcat).MousePos = this.MousePos;
                (graphic as OptionalSlugcatGraphics).MousePos = this.MousePos;
            }
        }

        public Vector2 MousePos
        {
            get
            {
                if (this.MouseOver)
                {
                    return new Vector2(this.menu.mousePosition.x - pos.x - 75f, this.menu.mousePosition.y - pos.y);
                }
                else
                {
                    return new Vector2(-1, -1);
                }
            }
        }

        public class OptionalSlugcat : Creature
        {
            public OptionalSlugcat(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
            {
                this.feetStuckPos = default(Vector2?);
                this.standing = true;
                this.animationFrame = 0;
                this.superLaunchJump = 0;
                this.directionBoosts = new float[4];
                this.slugcatStats = new SlugcatStats(0, false);
                float num = 0.7f * this.slugcatStats.bodyWeightFac;
                base.bodyChunks = new BodyChunk[2];
                base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 30f), 9f, num / 2f);
                base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 30f), 8f, num / 2f);
                this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[1];
                this.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 17f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
                this.bodyContactPoints = new Vector2[2];
                this.bodyContactPoints[0] = new Vector2(0f, 0f);
                this.bodyContactPoints[1] = new Vector2(0f, 0f);

                this.animation = AnimationIndex.None;
                this.bodyMode = BodyModeIndex.Default;
                base.airFriction = 0.999f;
                base.gravity = 0.9f;
                this.bounce = 0.1f;
                this.surfaceFriction = 0.5f;
                this.collisionLayer = 1;
                base.waterFriction = 0.96f;
                base.buoyancy = 0.95f;
                this.aerobicLevel = 1f;

                this.flipDirection = 1;
                if ((double)UnityEngine.Random.value < 0.5)
                {
                    this.flipDirection = -1;
                }
                //this.room = world.GetAbstractRoom(abstractCreature.pos.room).realizedRoom;
                this.sleepCounter = 0;
                this.grasps = new Grasp[2];

                this.wallSlideCounter = 0;
                this.canWallJump = 0;
                this.jumpChunkCounter = 0;
                this.jumpChunkCounter = 0;
                this.noGrabCounter = 0;
                this.waterJumpDelay = 0;
                this.forceFeetToHorizontalBeamTile = 0;
                this.canJump = 0;
                this.slowMovementStun = 0;
                this.backwardsCounter = 0;
                this.landingDelay = 0;
                this.jumpStun = 0;
                this.doWiggle = false;
                this.wiggleX = 8;
                this.wiggleY = -8;
            }
            public Player.InputPackage[] input { get; private set; }
            public int flipDirection { get; set; }
            public int lastFlipDirection { get; private set; }

            public Vector2 MousePos;
            public bool MouseOver;

            public bool doWiggle;
            private int wiggleX;
            private int wiggleY;

            public float Wiggle
            {
                get
                {
                    return this.wiggle * Mathf.Pow(Mathf.InverseLerp(2f, 12f, Vector2.Distance(base.bodyChunks[0].lastPos, base.bodyChunks[0].pos) + Vector2.Distance(base.bodyChunks[1].lastPos, base.bodyChunks[1].pos)), 0.5f);
                }
            }

            public SlugcatStats slugcatStats;

            public bool Malnourished
            {
                get
                {
                    return false;
                }
            }

            public float Sneak
            {
                get
                {
                    return Mathf.Lerp(Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, this.privSneak), 0.5f), 0.5f, base.Submersion);
                }
            }

            public PlayerState playerState;

            public void AerobicIncrease(float f)
            {
                this.aerobicLevel = Mathf.Min(1f, this.aerobicLevel + f / 9f);
            }

            public int animationFrame { get; protected set; }

            public override float VisibilityBonus
            {
                get
                {
                    return this.slugcatStats.generalVisibilityBonus - this.slugcatStats.visualStealthInSneakMode * this.Sneak;
                }
            }

            public override void InitiateGraphicsModule()
            {
                if (base.graphicsModule == null)
                {
                    base.graphicsModule = new OptionalSlugcatGraphics(this);
                }
            }

            public override void RemoveGraphicsModule()
            {
            }

            public override void Update(bool eu)
            {
                //this.MovementUpdate(eu);

                //SlugcatStats.Name name = this.slugcatStats.name;

                if (this.poleSkipPenalty > 0)
                {
                    this.poleSkipPenalty--;
                }
                if (this.shootUpCounter > 0)
                {
                    this.noGrabCounter = Math.Max(this.noGrabCounter, 2);
                    this.shootUpCounter--;
                    if (!this.input[0].jmp || this.input[0].y < 1 || base.mainBodyChunk.pos.y < base.mainBodyChunk.lastPos.y)
                    {
                        this.shootUpCounter = 0;
                    }
                }

                if (this.handOnExternalFoodSource != null && (!base.Consious || !Custom.DistLess(base.mainBodyChunk.pos, this.handOnExternalFoodSource.Value, 45f) || this.eatExternalFoodSourceCounter < 1))
                {
                    this.handOnExternalFoodSource = default(Vector2?);
                }

                if (!this.standing)
                {
                    this.privSneak = Mathf.Min(this.privSneak + 0.1f, 1f);
                }
                else
                {
                    this.privSneak = Mathf.Max(this.privSneak - 0.04f, 0f);
                }
                base.bodyChunks[0].loudness = 1.5f * (1f - this.Sneak) * this.slugcatStats.loudnessFac;
                base.bodyChunks[1].loudness = 0.7f * (1f - this.Sneak) * this.slugcatStats.loudnessFac;

                this.surfaceFriction = ((!base.Consious) ? 0.3f : 0.5f);
                /*
                if (base.bodyChunks[0].ContactPoint.y != 0 && base.bodyChunks[0].ContactPoint.y == -base.bodyChunks[1].ContactPoint.y)
                {
                    ComOptPlugin.LogInfo("WALLSTUCK");
                    base.bodyChunks[1].HardSetPosition(base.bodyChunks[0].pos + Custom.DirVec(base.bodyChunks[0].pos, base.bodyChunks[1].pos) * 2f);
                }*/
                if (this.dontGrabStuff > 0)
                {
                    this.dontGrabStuff--;
                }
                for (int l = 0; l < 4; l++)
                {
                    if (this.directionBoosts[l] > 0f)
                    {
                        this.directionBoosts[l] = Mathf.Min(this.directionBoosts[l] + 0.025f, 1f);
                    }
                }

                if (this.wantToJump > 0)
                {
                    this.wantToJump--;
                }

                if (base.bodyChunks[0].ContactPoint.x != 0 && this.input[0].x == base.bodyChunks[0].ContactPoint.x && base.bodyChunks[0].vel.y < 0f)
                {
                    BodyChunk bodyChunk3 = base.bodyChunks[0];
                    bodyChunk3.vel.y = bodyChunk3.vel.y * Mathf.Clamp(1f - this.surfaceFriction * ((base.bodyChunks[0].pos.y <= base.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f);
                }
                if (base.bodyChunks[1].ContactPoint.x != 0 && this.input[0].x == base.bodyChunks[1].ContactPoint.x && base.bodyChunks[1].vel.y < 0f)
                {
                    BodyChunk bodyChunk4 = base.bodyChunks[1];
                    bodyChunk4.vel.y = bodyChunk4.vel.y * Mathf.Clamp(1f - this.surfaceFriction * ((base.bodyChunks[0].pos.y <= base.bodyChunks[1].pos.y) ? 0.5f : 2f), 0f, 1f);
                }

                if (base.bodyChunks[0].pos.x == base.bodyChunks[1].pos.x && base.bodyChunks[0].vel.x == 0f && base.bodyChunks[1].vel.x == 0f && base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y)
                {
                    base.bodyChunks[0].vel.x = UnityEngine.Random.Range(-1f, 1f);
                    base.bodyChunks[1].vel.x = -base.bodyChunks[0].vel.x;
                }
                this.touchedNoInputCounter = 1000;

                //base.Update(eu);

                for (int i = 0; i < this.bodyChunks.Length; i++)
                {
                    //this.bodyChunks[i].Update();
                    /*
                    ComOptPlugin.LogInfo(string.Concat(
                        "index ", i.ToString(), " pos: ",
                        this.bodyChunks[i].pos.x.ToString("N2"), ", ",
                        this.bodyChunks[i].pos.y.ToString("N2"), " / vel: ",
                        this.bodyChunks[i].vel.x.ToString("N2"), ", ",
                        this.bodyChunks[i].vel.y.ToString("N2")
                    ));
                    */

                    this.bodyChunks[i] = VirtualBCUpdate(this.bodyChunks[i], i);
                }

                //this.abstractPhysicalObject.pos.Tile = this.room.GetTilePosition(this.firstChunk.pos);
                for (int j = 0; j < this.bodyChunkConnections.Length; j++)
                {
                    this.bodyChunkConnections[j].Update();
                }

                //base.Update(eu);
                this.evenUpdate = eu;

                //

                base.GoThroughFloors = false;

                if (base.stun < 1 && !base.dead && this.enteringShortCut == null && !base.inShortcut)
                {
                    this.MovementUpdate(eu);
                }

                bool flag2 = false;
                if (doWiggle && !this.lastWiggleJump)
                {
                    this.wiggle += 0.025f;
                    this.lastWiggleJump = true;
                }

                IntVector2 intVector = this.wiggleDirectionCounters;
                if (doWiggle && this.wiggleX != this.lastWiggleDir.x)
                {
                    flag2 = true;
                    if (intVector.y > 0)
                    {
                        this.wiggle += 0.0333333351f;
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y - 1;
                    }
                    this.lastWiggleDir.x = this.wiggleX;
                    this.lastWiggleJump = false;
                    if (this.wiggleDirectionCounters.x < 5)
                    {
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x + 1;
                    }
                }
                else { this.wiggleX = this.lastWiggleDir.x < 0 ? 8 : -8; }
                if (doWiggle && this.wiggleY != this.lastWiggleDir.y)
                {
                    flag2 = true;
                    if (intVector.x > 0)
                    {
                        this.wiggle += 0.0333333351f;
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x - 1;
                    }
                    this.lastWiggleDir.y = this.wiggleY;
                    this.lastWiggleJump = false;
                    if (this.wiggleDirectionCounters.y < 5)
                    {
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y + 1;
                    }
                }
                else { this.wiggleY = this.lastWiggleDir.y < 0 ? 8 : -8; }
                if (flag2)
                {
                    this.noWiggleCounter = 0;
                }
                else
                {
                    this.noWiggleCounter++;
                }
                this.wiggle -= Custom.LerpMap((float)this.noWiggleCounter, 5f, 35f, 0f, 0.0333333351f);
                if (this.noWiggleCounter > 20)
                {
                    if (this.wiggleDirectionCounters.x > 0)
                    {
                        this.wiggleDirectionCounters.x = this.wiggleDirectionCounters.x - 1;
                    }
                    if (this.wiggleDirectionCounters.y > 0)
                    {
                        this.wiggleDirectionCounters.y = this.wiggleDirectionCounters.y - 1;
                    }
                }
                this.wiggle = Mathf.Clamp(this.wiggle, 0f, 1f);
            }

            public BodyChunk VirtualBCUpdate(BodyChunk bodyChunk, int index)
            {
                if (float.IsNaN(bodyChunk.vel.y))
                {
                    ComOptPlugin.LogMessage("VELY IS NAN");
                    bodyChunk.vel.y = 0f;
                }
                if (float.IsNaN(bodyChunk.vel.x))
                {
                    ComOptPlugin.LogMessage("VELX IS NAN");
                    bodyChunk.vel.x = 0f;
                }
                bodyChunk.vel.y = bodyChunk.vel.y - 1f;

                bodyChunk.vel *= bodyChunk.owner.airFriction;
                bodyChunk.lastLastPos = bodyChunk.lastPos;
                bodyChunk.lastPos = bodyChunk.pos;
                if (bodyChunk.setPos != null)
                {
                    bodyChunk.pos = bodyChunk.setPos.Value;
                    bodyChunk.setPos = default(Vector2?);
                }
                else
                {
                    bodyChunk.pos += bodyChunk.vel;
                }

                //bodyChunk.onSlope = 0;
                //bodyChunk.slopeRad = bodyChunk.TerrainRad;
                //bodyChunk.lastContactPoint = bodyChunk.contactPoint;

                float terrainRad = Mathf.Max(bodyChunk.rad * bodyChunk.terrainSqueeze, 1f); ;

                if (bodyChunk.collideWithTerrain)
                {
                    //bodyChunk.CheckVerticalCollision();

                    this.bodyContactPoints[index].y = 0;

                    if (bodyChunk.vel.y > 0f) //flying upward
                    {
                        if (bodyChunk.pos.y > 120f)
                        {
                            bodyChunk.pos.y = 120f - terrainRad;
                            this.bodyContactPoints[index].y = 1;
                            bodyChunk.vel.y = -Mathf.Abs(bodyChunk.vel.y) * this.bounce;
                            if (Mathf.Abs(bodyChunk.vel.y) < 1f + 9f * (1f - this.bounce))
                            {
                                bodyChunk.vel.y = 0f;
                            }
                            bodyChunk.vel.x = bodyChunk.vel.x * Mathf.Clamp(bodyChunk.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                    }
                    else if (bodyChunk.vel.y < 0f)
                    {
                        if (bodyChunk.pos.y < 30f)
                        {
                            bodyChunk.pos.y = 21f + terrainRad;
                            this.bodyContactPoints[index].y = -1;
                            bodyChunk.vel.y = Mathf.Abs(bodyChunk.vel.y) * this.bounce;
                            if (bodyChunk.vel.y < 1f || bodyChunk.vel.y < 1f + 9f * (1f - this.bounce))
                            {
                                bodyChunk.vel.y = 0f;
                            }
                            bodyChunk.vel.x = bodyChunk.vel.x * Mathf.Clamp(this.surfaceFriction * 2f, 0f, 1f);
                        }
                    }

                    //bodyChunk.CheckHorizontalCollision();
                }
                else
                {
                    this.bodyContactPoints[index].x = 0;
                    this.bodyContactPoints[index].y = 0;
                }

                bodyChunk.splashStop = 0;
                return bodyChunk;
            }

            public Vector2[] bodyContactPoints;

            public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
            {
                if (this.spearOnBack != null)
                {
                    this.spearOnBack.GraphicsModuleUpdated(actuallyViewed, eu);
                }
                for (int i = 0; i < 2; i++)
                {
                    if (base.grasps[i] != null)
                    {
                        if (actuallyViewed)
                        {
                            base.grasps[i].grabbed.firstChunk.vel = (base.graphicsModule as OptionalSlugcatGraphics).hands[i].vel;
                            base.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (base.graphicsModule as OptionalSlugcatGraphics).hands[i].pos);
                            if (base.grasps[i].grabbed is Weapon)
                            {
                                Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[i].grabbed.bodyChunks[0].pos) * ((i != 0) ? 1f : -1f);
                                if (base.grasps[i].grabbed is Spear)
                                {
                                    vector = Vector3.Slerp(vector, Custom.DegToVec((80f + Mathf.Cos((float)(this.animationFrame + ((!this.leftFoot) ? 3 : 9)) / 12f * 2f * 3.14159274f) * 4f * (base.graphicsModule as PlayerGraphics).spearDir) * (base.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((base.graphicsModule as PlayerGraphics).spearDir));
                                }
                                (base.grasps[i].grabbed as Weapon).setRotation = new Vector2?(vector);
                                (base.grasps[i].grabbed as Weapon).rotationSpeed = 0f;
                            }
                        }
                        else
                        {
                            base.grasps[i].grabbed.firstChunk.pos = base.bodyChunks[0].pos;
                            base.grasps[i].grabbed.firstChunk.vel = base.mainBodyChunk.vel;
                        }
                    }
                }
            }

            public void Blink(int blink)
            {
                if (base.graphicsModule == null)
                {
                    return;
                }
                if ((base.graphicsModule as OptionalSlugcatGraphics).blink < blink)
                {
                    (base.graphicsModule as OptionalSlugcatGraphics).blink = blink;
                }
            }

            public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
            {
            }

            public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
            {
            }

            public override void Die()
            {
                base.Die();
            }

            public override void Stun(int st)
            {
            }

            public void UpdateAnimation()
            {
                Vector2 vector = new Vector2(0f, 30f);//this.room.MiddleOfTile(base.bodyChunks[0].pos)

                switch (this.animation)
                {
                    case AnimationIndex.StandUp:
                        if (this.standing)
                        {
                            BodyChunk bodyChunk6 = base.bodyChunks[0];
                            bodyChunk6.vel.x = bodyChunk6.vel.x * 0.7f;
                            this.bodyMode = BodyModeIndex.Stand;
                            if (base.bodyChunks[0].pos.y > base.bodyChunks[1].pos.y + 3f)
                            {
                                this.animation = AnimationIndex.None;
                            }
                        }
                        else
                        {
                            this.animation = AnimationIndex.DownOnFours;
                        }
                        break;

                    case AnimationIndex.DownOnFours:
                        if (!this.standing)
                        {
                            BodyChunk bodyChunk7 = base.bodyChunks[0];
                            bodyChunk7.vel.y = bodyChunk7.vel.y - 2f;
                            BodyChunk bodyChunk8 = base.bodyChunks[0];
                            bodyChunk8.vel.x = bodyChunk8.vel.x + (float)this.flipDirection;
                            BodyChunk bodyChunk9 = base.bodyChunks[1];
                            bodyChunk9.vel.x = bodyChunk9.vel.x - (float)this.flipDirection;
                            if (base.bodyChunks[0].pos.y < base.bodyChunks[1].pos.y || base.bodyChunks[0].ContactPoint.y == -1)
                            {
                                this.animation = AnimationIndex.None;
                            }
                        }
                        else
                        {
                            this.animation = AnimationIndex.StandUp;
                        }
                        break;

                    case AnimationIndex.HangUnderVerticalBeam:
                        this.bodyMode = BodyModeIndex.ClimbingOnBeam;
                        this.standing = false;

                        base.bodyChunks[0].pos.x = Mathf.Lerp(base.bodyChunks[0].pos.x, vector.x, 0.5f);
                        base.bodyChunks[0].pos.y = Mathf.Max(base.bodyChunks[0].pos.y, vector.y + 5f + base.bodyChunks[0].vel.y);
                        BodyChunk bodyChunk36 = base.bodyChunks[0];
                        bodyChunk36.vel.x = bodyChunk36.vel.x * 0f;
                        BodyChunk bodyChunk37 = base.bodyChunks[0];
                        bodyChunk37.vel.y = bodyChunk37.vel.y * 0.5f;
                        BodyChunk bodyChunk38 = base.bodyChunks[1];
                        bodyChunk38.vel.x = bodyChunk38.vel.x + (base.bodyChunks[0].pos.x < 0 ? 1f : -1f); //Autowiggle
                        if (this.input[0].y > 0)
                        {
                            BodyChunk bodyChunk39 = base.bodyChunks[0];
                            bodyChunk39.vel.y = bodyChunk39.vel.y + 2.5f;
                        }

                        break;

                    case AnimationIndex.BeamTip:
                        {
                            this.bodyMode = BodyModeIndex.ClimbingOnBeam;
                            this.standing = true;
                            this.canJump = 5;
                            base.bodyChunks[1].vel *= 0.5f;
                            base.bodyChunks[1].pos = (base.bodyChunks[1].pos + vector) / 2f;
                            BodyChunk bodyChunk45 = base.bodyChunks[0];
                            bodyChunk45.vel.y = bodyChunk45.vel.y + 1.5f;
                            BodyChunk bodyChunk46 = base.bodyChunks[0];
                            bodyChunk46.vel.y = bodyChunk46.vel.y + (float)this.input[0].y * 0.1f;
                            BodyChunk bodyChunk47 = base.bodyChunks[0];
                            bodyChunk47.vel.x = bodyChunk47.vel.x + (float)this.input[0].x * 0.1f;
                            break;
                        }
                    case AnimationIndex.Dead:
                        this.bodyMode = BodyModeIndex.Dead;
                        break;
                }
            }

            public void SlugcatGrab(PhysicalObject obj, int graspUsed)
            {
                int chunkGrabbed = 0;
                this.switchHandsCounter = 0;
                this.wantToPickUp = 0;
                this.noPickUpOnRelease = 20;
                this.Grab(obj, graspUsed, chunkGrabbed, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, !(obj is Cicada) && !(obj is JetFish));
            }

            private bool CanIPickThisUp(PhysicalObject obj)
            {
                return true;
            }

            private ObjectGrabability Grabability(PhysicalObject obj)
            {
                return ObjectGrabability.OneHand;
            }

            public int FreeHand()
            {
                for (int i = 0; i < base.grasps.Length; i++)
                {
                    if (base.grasps[i] == null)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public void MovementUpdate(bool eu)
            {
                this.whiplashJump = false;

                int num = 0;
                if (this.jumpStun != 0)
                {
                    num = this.jumpStun / Mathf.Abs(this.jumpStun);
                }
                this.lastFlipDirection = this.flipDirection;
                if (num != this.flipDirection && num != 0)
                {
                    this.flipDirection = num;
                }
                int num2 = 0;
                if (this.bodyMode != BodyModeIndex.ClimbingOnBeam)
                {
                    num2++;
                }

                if (this.feetStuckPos != null)
                {
                    this.feetStuckPos = new Vector2?(this.feetStuckPos.Value + new Vector2((base.bodyChunks[1].pos.x - this.feetStuckPos.Value.x) * (1f - this.surfaceFriction), 0f));
                    base.bodyChunks[1].pos = this.feetStuckPos.Value;
                }
                this.goIntoCorridorClimb = 0;
                this.canJump = 5;
                //this.bodyMode = BodyModeIndex.Stand;

                this.jumpBoost = 0f;

                //this.animation = AnimationIndex.None;
                BodyChunk bodyChunk3 = base.bodyChunks[1];
                bodyChunk3.vel.y = bodyChunk3.vel.y + 2f;
                BodyChunk bodyChunk4 = base.bodyChunks[0];
                bodyChunk4.vel.x = bodyChunk4.vel.x - (base.bodyChunks[0].pos.x - base.bodyChunks[1].pos.x) * 0.25f;
                BodyChunk bodyChunk5 = base.bodyChunks[0];
                bodyChunk5.vel.y = bodyChunk5.vel.y - 1f;

                this.dynamicRunSpeed[0] = 3.6f;
                this.dynamicRunSpeed[1] = 3.6f;
                float num5 = 2.4f;
                this.UpdateAnimation();
                this.crawlTurnDelay = 0;

                //this.UpdateBodyMode();

                this.bodyChunkConnections[0].distance = 17f;

                this.bodyChunkConnections[0].type = PhysicalObject.BodyChunkConnection.Type.Normal;

                if (this.dynamicRunSpeed[0] > 0f || this.dynamicRunSpeed[1] > 0f)
                {
                    if (this.slowMovementStun > 0)
                    {
                        this.dynamicRunSpeed[0] *= 0.5f + 0.5f * Mathf.InverseLerp(10f, 0f, (float)this.slowMovementStun);
                        this.dynamicRunSpeed[1] *= 0.5f + 0.5f * Mathf.InverseLerp(10f, 0f, (float)this.slowMovementStun);
                        num5 *= 0.4f + 0.6f * Mathf.InverseLerp(10f, 0f, (float)this.slowMovementStun);
                    }
                    for (int num11 = 0; num11 < 2; num11++)
                    {
                        if (num < 0)
                        {
                            float num12 = num5 * this.surfaceFriction;
                            if (base.bodyChunks[num11].vel.x - num12 < -this.dynamicRunSpeed[num11])
                            {
                                num12 = this.dynamicRunSpeed[num11] + base.bodyChunks[num11].vel.x;
                            }
                            if (num12 > 0f)
                            {
                                BodyChunk bodyChunk11 = base.bodyChunks[num11];
                                bodyChunk11.vel.x = bodyChunk11.vel.x - num12;
                            }
                        }
                        else if (num > 0)
                        {
                            float num13 = num5 * this.surfaceFriction;
                            if (base.bodyChunks[num11].vel.x + num13 > this.dynamicRunSpeed[num11])
                            {
                                num13 = this.dynamicRunSpeed[num11] - base.bodyChunks[num11].vel.x;
                            }
                            if (num13 > 0f)
                            {
                                BodyChunk bodyChunk12 = base.bodyChunks[num11];
                                bodyChunk12.vel.x = bodyChunk12.vel.x + num13;
                            }
                        }
                        if (base.bodyChunks[0].ContactPoint.y != 0 || base.bodyChunks[1].ContactPoint.y != 0)
                        {
                            float num14 = 0f;
                            if (this.input[0].x != 0)
                            {
                                num14 = Mathf.Clamp(base.bodyChunks[num11].vel.x, -this.dynamicRunSpeed[num11], this.dynamicRunSpeed[num11]);
                            }
                            BodyChunk bodyChunk13 = base.bodyChunks[num11];
                            bodyChunk13.vel.x = bodyChunk13.vel.x + (num14 - base.bodyChunks[num11].vel.x) * Mathf.Pow(this.surfaceFriction, 1.5f);
                        }
                    }
                }

                if (this.simulateHoldJumpButton > 0)
                {
                    this.simulateHoldJumpButton--;
                }

                if (this.canJump > 0 && this.wantToJump > 0)
                {
                    this.canJump = 0;
                    this.wantToJump = 0;
                    this.Jump();
                }
                //this.GrabUpdate(eu);
            }

            public void Jump()
            {
                this.feetStuckPos = default(Vector2?);
                this.AerobicIncrease(1f);
                AnimationIndex animationIndex = this.animation;
                this.animation = AnimationIndex.None;
                if (this.standing)
                {
                    base.bodyChunks[0].vel.y = 4f;
                    base.bodyChunks[1].vel.y = 3f;
                    this.jumpBoost = 8f;
                }
            }

            public int slowMovementStun;
            private int lastStun;
            private int goIntoCorridorClimb;
            private bool corridorDrop;
            private float[] dynamicRunSpeed = new float[2];
            public int jumpStun;
            public float jumpBoost;
            private int simulateHoldJumpButton;
            public int superLaunchJump;
            public int wallSlideCounter;
            private int killSuperLaunchJumpCounter;
            public int touchedNoInputCounter;
            public int forceSleepCounter;
            public int eatExternalFoodSourceCounter;
            public int dontEatExternalFoodSourceCounter;
            public Vector2? handOnExternalFoodSource;
            public int shootUpCounter;
            public int consistentDownDiagonal;
            public BodyChunk jumpChunk;
            public int jumpChunkCounter;
            private float wiggle;
            private int noWiggleCounter;
            public int wantToJump;
            public int canJump;
            public int wantToGrab;
            public int canWallJump;
            private int noGrabCounter;
            private int poleSkipPenalty;
            private int wantToPickUp;
            private int wantToThrow;
            private int dontGrabStuff;
            private int waterJumpDelay;
            public float sleepCurlUp;
            public int sleepCounter;
            public bool sleepWhenStill;
            private float privSneak;
            public int forceFeetToHorizontalBeamTile;
            private PhysicalObject pickUpCandidate;
            private float[] directionBoosts;
            public bool glowing;
            public float aerobicLevel;
            public bool standing;
            private Vector2? feetStuckPos;
            private int backwardsCounter;
            private int landingDelay;
            private int crawlTurnDelay;
            public float curcuitJumpMeter;
            private IntVector2 lastWiggleDir;
            private IntVector2 wiggleDirectionCounters;
            private bool lastWiggleJump;
            public IntVector2 zeroGPoleGrabDir;
            public AnimationIndex animation;
            private int ledgeGrabCounter;
            private bool straightUpOnHorizontalBeam;
            private Vector2 upOnHorizontalBeamPos;
            public bool whiplashJump;
            public BodyModeIndex bodyMode;
            public bool leftFoot;
            public int lowerBodyFramesOnGround;
            public int lowerBodyFramesOffGround;
            public int upperBodyFramesOnGround;
            public int upperBodyFramesOffGround;
            public IntVector2? dropGrabTile;
            public int switchHandsCounter;
            public int noPickUpOnRelease;
            public float switchHandsProcess;
            private bool WANTTOSTAND;
            public SpearOnBack spearOnBack;

            public enum AnimationIndex
            {
                None,
                //CrawlTurn,
                StandUp,
                DownOnFours,
                //LedgeCrawl,
                //LedgeGrab,
                //HangFromBeam,
                //GetUpOnBeam,
                //StandOnBeam,
                //ClimbOnBeam,
                //GetUpToBeamTip,
                HangUnderVerticalBeam,
                BeamTip,
                //CorridorTurn,
                //SurfaceSwim,
                //DeepSwim,
                //Roll,
                //Flip,
                //RocketJump,
                //BellySlide,
                //AntlerClimb,
                //GrapplingSwing,
                //ZeroGSwim,
                //ZeroGPoleGrab,
                //VineGrab,
                Dead
            }

            public enum BodyModeIndex
            {
                Default,
                //Crawl,
                Stand,
                //CorridorClimb,
                //ClimbIntoShortCut,
                //WallClimb,
                ClimbingOnBeam,
                //Swimming,
                //ZeroG,
                //Stunned,
                Dead
            }

            private enum ObjectGrabability
            {
                //CantGrab,
                OneHand//,
                //BigOneHand,
                //TwoHands,
                //Drag
            }

            public class AbstractOnBackStick : AbstractPhysicalObject.AbstractObjectStick
            {
                public AbstractOnBackStick(AbstractPhysicalObject player, AbstractPhysicalObject spear) : base(player, spear)
                {
                }

                public AbstractPhysicalObject Player
                {
                    get
                    {
                        return this.A;
                    }
                    set
                    {
                        this.A = value;
                    }
                }

                public AbstractPhysicalObject Spear
                {
                    get
                    {
                        return this.B;
                    }
                    set
                    {
                        this.B = value;
                    }
                }
            }

            public class SpearOnBack
            {
                public SpearOnBack(OptionalSlugcat owner)
                {
                    this.owner = owner;
                }

                public bool HasASpear
                {
                    get
                    {
                        return this.spear != null;
                    }
                }

                public void Update(bool eu)
                {
                    GraphicsModuleUpdated(true, eu);
                }

                public void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
                {
                    if (this.spear == null)
                    {
                        return;
                    }
                    if (this.spear.slatedForDeletetion || this.spear.grabbedBy.Count > 0)
                    {
                        if (this.abstractStick != null)
                        {
                            this.abstractStick.Deactivate();
                        }
                        this.spear = null;
                        return;
                    }
                    Vector2 vector = this.owner.mainBodyChunk.pos;
                    Vector2 vector2 = this.owner.bodyChunks[1].pos;
                    if (this.owner.graphicsModule != null)
                    {
                        vector = Vector2.Lerp((this.owner.graphicsModule as OptionalSlugcatGraphics).drawPositions[0, 0], (this.owner.graphicsModule as OptionalSlugcatGraphics).head.pos, 0.2f);
                        vector2 = (this.owner.graphicsModule as OptionalSlugcatGraphics).drawPositions[1, 0];
                    }
                    Vector2 vector3 = Custom.DirVec(vector2, vector);
                    if (this.owner.Consious)
                    {
                        if (this.owner.bodyMode == BodyModeIndex.Default && this.owner.animation == AnimationIndex.None && this.owner.standing && this.owner.bodyChunks[1].pos.y < this.owner.bodyChunks[0].pos.y - 6f)
                        {
                            this.flip = Custom.LerpAndTick(this.flip, (float)this.owner.input[0].x * 0.3f, 0.05f, 0.02f);
                        }
                        else
                        {
                            this.flip = Custom.LerpAndTick(this.flip, (float)this.owner.flipDirection * Mathf.Abs(vector3.x), 0.15f, 0.166666672f);
                        }
                        if (this.counter > 12 && !this.interactionLocked && this.owner.input[0].x != 0 && this.owner.standing)
                        {
                            float num = 0f;
                            for (int i = 0; i < this.owner.grasps.Length; i++)
                            {
                                if (this.owner.grasps[i] == null)
                                {
                                    num = ((i != 0) ? 1f : -1f);
                                    break;
                                }
                            }
                            this.spear.setRotation = new Vector2?(Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector2, vector) + Custom.LerpMap((float)this.counter, 12f, 20f, 0f, 360f * num)));
                        }
                        else
                        {
                            this.spear.setRotation = new Vector2?((vector3 - Custom.PerpendicularVector(vector3) * 0.9f * (1f - Mathf.Abs(this.flip))).normalized);
                        }
                        this.spear.ChangeOverlap(vector3.y < -0.1f && this.owner.bodyMode != BodyModeIndex.ClimbingOnBeam);
                    }
                    else
                    {
                        this.flip = Custom.LerpAndTick(this.flip, 0f, 0.15f, 0.142857149f);
                        this.spear.setRotation = new Vector2?(vector3 - Custom.PerpendicularVector(vector3) * 0.9f);
                        this.spear.ChangeOverlap(false);
                    }
                    this.spear.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(vector2, vector, 0.6f) - Custom.PerpendicularVector(vector2, vector) * 7.5f * this.flip);
                    this.spear.firstChunk.vel = this.owner.mainBodyChunk.vel;
                    this.spear.rotationSpeed = 0f;
                }

                public OptionalSlugcat owner;
                public Spear spear;
                public bool increment;
                public int counter;
                public float flip;
                public bool interactionLocked;
                public AbstractOnBackStick abstractStick;
            }
        }

        public class OptionalSlugcatGraphics : GraphicsModule
        {
            public OptionalSlugcatGraphics(PhysicalObject ow) : base(ow, false)
            {
                this.player = (ow as OptionalSlugcat);
                List<BodyPart> list = new List<BodyPart>();
                this.airborneCounter = 0f;
                this.tail = new TailSegment[4];
                this.tail[0] = new TailSegment(this, 6f, 4f, null, 0.85f, 1f, 1f, true);
                this.tail[1] = new TailSegment(this, 4f, 7f, this.tail[0], 0.85f, 1f, 0.5f, true);
                this.tail[2] = new TailSegment(this, 2.5f, 7f, this.tail[1], 0.85f, 1f, 0.5f, true);
                this.tail[3] = new TailSegment(this, 1f, 7f, this.tail[2], 0.85f, 1f, 0.5f, true);
                for (int i = 0; i < this.tail.Length; i++)
                {
                    list.Add(this.tail[i]);
                }
                this.hands = new SlugcatHand[2];
                for (int j = 0; j < 2; j++)
                {
                    this.hands[j] = new SlugcatHand(this, base.owner.bodyChunks[0], j, 3f, 0.8f, 1f);
                    list.Add(this.hands[j]);
                }
                this.head = new GenericBodyPart(this, 4f, 0.8f, 0.99f, base.owner.bodyChunks[0]);
                list.Add(this.head);
                this.legs = new GenericBodyPart(this, 1f, 0.8f, 0.99f, base.owner.bodyChunks[1]);
                list.Add(this.legs);
                this.legsDirection = new Vector2(0f, -1f);
                this.drawPositions = new Vector2[base.owner.bodyChunks.Length, 2];
                this.disbalanceAmount = 0f;
                this.balanceCounter = 0f;
                for (int k = 0; k < base.owner.bodyChunks.Length; k++)
                {
                    this.drawPositions[k, 0] = base.owner.bodyChunks[k].pos;
                    this.drawPositions[k, 1] = base.owner.bodyChunks[k].lastPos;
                }
                this.lookDirection = new Vector2(0f, 0f);
                this.objectLooker = new PlayerObjectLooker(this);
                if (this.player.slugcatStats.name == SlugcatStats.Name.Red && this.player.abstractCreature.world.game.IsStorySession)
                {
                    this.markBaseAlpha = Mathf.Pow(Mathf.InverseLerp(4f, 14f, (float)this.player.abstractCreature.world.game.GetStorySession.saveState.cycleNumber), 3.5f);
                }
                this.bodyParts = list.ToArray();

                this.spearDir = 0f;
                this.markAlpha = 0f;
                this.lastMarkAlpha = 0f;
            }

            public Vector2 MousePos;
            public bool MouseOver;

            public FContainer cage;
            public FContainer fence;

            public override bool ShouldBeCulled
            {
                get
                {
                    return false;
                }
            }

            public bool rub;

            public override void Update()
            {
                base.Update();
                this.lastBreath = this.breath;
                if (this.player.bodyMode != OptionalSlugcat.BodyModeIndex.Dead)
                {
                    this.breath += 1f / Mathf.Lerp(60f, 15f, Mathf.Pow(this.player.aerobicLevel, 1.5f));
                }
                else
                {
                    this.objectLooker.LookAtNothing();
                    this.blink = 10;
                }

                if (this.spearDir < 0f)
                {
                    this.spearDir = Mathf.Min(this.spearDir + 0.05f, 0f);
                }
                else if (this.spearDir > 0f)
                {
                    this.spearDir = Mathf.Max(this.spearDir - 0.05f, 0f);
                }

                float num = 0f;

                for (int i = 0; i < base.owner.bodyChunks.Length; i++)
                {
                    this.drawPositions[i, 1] = this.drawPositions[i, 0];
                }
                this.drawPositions[0, 0] = base.owner.bodyChunks[0].pos;
                this.drawPositions[1, 0] = base.owner.bodyChunks[1].pos;
                //int num2 = 0;
                bool flag = false;
                float num3 = 1f;
                switch (this.player.bodyMode)
                {
                    case OptionalSlugcat.BodyModeIndex.Default:
                        num3 = 0f;
                        break;

                    case OptionalSlugcat.BodyModeIndex.Stand:
                        this.drawPositions[0, 0].x += (float)this.player.flipDirection * 6f * Mathf.Clamp(Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
                        this.drawPositions[0, 0].y += Mathf.Cos((float)this.player.animationFrame / 6f * 2f * 3.14159274f) * 2f;
                        this.drawPositions[1, 0].x -= (float)this.player.flipDirection * (1.5f - (float)this.player.animationFrame / 6f);
                        this.drawPositions[1, 0].y += 2f + Mathf.Sin((float)this.player.animationFrame / 6f * 2f * 3.14159274f) * 4f;
                        flag = (Mathf.Abs(base.owner.bodyChunks[0].vel.x) > 2f && Mathf.Abs(base.owner.bodyChunks[1].vel.x) > 2f);
                        num3 = 1f - Mathf.Clamp((Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 1f) * 0.5f, 0f, 1f);
                        break;
                }

                if (this.player.bodyMode == OptionalSlugcat.BodyModeIndex.ClimbingOnBeam && this.player.animation == OptionalSlugcat.AnimationIndex.BeamTip)
                {
                    if (Mathf.Abs(base.owner.bodyChunks[0].vel.x) > 2f)
                    {
                        this.disbalanceAmount += ((this.player.animation != OptionalSlugcat.AnimationIndex.BeamTip) ? 3f : 17f);
                    }
                    else
                    {
                        this.disbalanceAmount -= 1f;
                    }
                    this.disbalanceAmount = Mathf.Clamp(this.disbalanceAmount, 0f, 120f);
                    this.balanceCounter += 1f + this.disbalanceAmount / 40f * (1f + UnityEngine.Random.value);
                    if (this.balanceCounter > 300f)
                    {
                        this.balanceCounter -= 300f;
                    }
                    float num8 = Mathf.Sin(this.balanceCounter / 300f * 3.14159274f * 2f) / (Mathf.Abs(base.owner.bodyChunks[1].vel.x) + 1f);
                    this.drawPositions[0, 0].x += num8 * (this.disbalanceAmount + 20f) * 0.08f;
                    this.drawPositions[0, 0].y += num8 * this.disbalanceAmount * 0.02f;
                    TailSegment tailSegment5 = this.tail[0];
                    tailSegment5.vel.x = tailSegment5.vel.x + num8 * (this.disbalanceAmount + 20f) * 0.1f;
                    TailSegment tailSegment6 = this.tail[1];
                    tailSegment6.vel.x = tailSegment6.vel.x + num8 * (this.disbalanceAmount + 20f) * 0.04f;
                }
                if (this.player.bodyMode != OptionalSlugcat.BodyModeIndex.Dead && this.player.standing && num > 0.5f)
                {
                    this.drawPositions[0, 0] += Custom.DirVec(this.objectLooker.mostInterestingLookPoint, this.player.bodyChunks[0].pos) * 3.4f * Mathf.InverseLerp(0.5f, 1f, num);
                    this.head.vel += Custom.DirVec(this.objectLooker.mostInterestingLookPoint, this.head.pos) * 1.4f * Mathf.InverseLerp(0.5f, 1f, num);
                }

                if (num > 0f)
                {
                    this.tail[0].vel += Custom.DirVec(this.objectLooker.mostInterestingLookPoint, this.drawPositions[1, 0]) * 5f * num;
                    this.tail[1].vel += Custom.DirVec(this.objectLooker.mostInterestingLookPoint, this.drawPositions[1, 0]) * 3f * num;
                    this.player.aerobicLevel = Mathf.Max(this.player.aerobicLevel, Mathf.InverseLerp(0.5f, 1f, num) * 0.9f);
                }
                Vector2 vector2 = base.owner.bodyChunks[0].pos;
                if (flag)
                {
                    vector2 = base.owner.bodyChunks[1].pos;
                    vector2.y -= 4f;
                    vector2.x += (float)this.player.flipDirection * 16f * Mathf.Clamp(Mathf.Abs(base.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
                }

                Vector2 pos = base.owner.bodyChunks[1].pos;
                float num9 = 28f;
                this.tail[0].connectedPoint = new Vector2?(this.drawPositions[1, 0]);
                Vector2 vector; //Middle of tile

                for (int k = 0; k < this.tail.Length; k++)
                {
                    //this.tail[k].Update();
                    this.tail[k].lastPos = this.tail[k].pos;
                    this.tail[k].pos += this.tail[k].vel;
                    this.tail[k].vel *= 0.999f;
                    this.tail[k].stretched = 1f;

                    if (this.tail[k].connectedSegment != null)
                    {
                        if (!Custom.DistLess(this.tail[k].pos, this.tail[k].connectedSegment.pos, this.tail[k].connectionRad))
                        {
                            float numf = Vector2.Distance(this.tail[k].pos, this.tail[k].connectedSegment.pos);
                            Vector2 a = Custom.DirVec(this.tail[k].pos, this.tail[k].connectedSegment.pos);
                            this.tail[k].pos -= (this.tail[k].connectionRad - numf) * a * (1f - this.tail[k].affectPrevious);
                            this.tail[k].vel -= (this.tail[k].connectionRad - numf) * a * (1f - this.tail[k].affectPrevious);
                            if (this.tail[k].pullInPreviousPosition)
                            {
                                this.tail[k].connectedSegment.pos += (this.tail[k].connectionRad - numf) * a * this.tail[k].affectPrevious;
                            }
                            this.tail[k].connectedSegment.vel += (this.tail[k].connectionRad - numf) * a * this.tail[k].affectPrevious;
                            this.tail[k].stretched = Mathf.Clamp((this.tail[k].connectionRad / (numf * 0.5f) + 2f) / 3f, 0.2f, 1f);
                        }
                    }
                    else if (this.tail[k].connectedPoint != null && !Custom.DistLess(this.tail[k].pos, this.tail[k].connectedPoint.Value, this.tail[k].connectionRad))
                    {
                        float numf2 = Vector2.Distance(this.tail[k].pos, this.tail[k].connectedPoint.Value);
                        Vector2 a2 = Custom.DirVec(this.tail[k].pos, this.tail[k].connectedPoint.Value);
                        this.tail[k].pos -= (this.tail[k].connectionRad - numf2) * a2 * 1f;
                        this.tail[k].vel -= (this.tail[k].connectionRad - numf2) * a2 * 1f;
                        this.tail[k].stretched = Mathf.Clamp((this.tail[k].connectionRad / (numf2 * 0.5f) + 2f) / 3f, 0.2f, 1f);
                    }

                    //if(k > 0)
                    //{
                    vector = new Vector2(this.tail[k].pos.x, 20f); //20f * Mathf.RoundToInt((this.tail[k].pos.y - 10f) / 20f) - 10f
                    this.tail[k] = PushOutOfVirtualTerrain(this.tail[k], vector) as TailSegment;
                    //}

                    /*
                    float numt = 0f;
                    float num2t = 0f;
                    float d = 10f; // 10f
                    vector = new Vector2(vector.x, vector.y + d);
                    this.tail[k].terrainContact = false;
                    if (this.tail[k].pos.y >= vector.y - d && this.tail[k].pos.y <= vector.y + d)
                    { //inside of block? in y
                        if (this.tail[k].lastPos.x < vector.x)
                        {
                            if (this.tail[k].pos.x > vector.x - d - this.tail[k].rad)
                            {
                                numt = vector.x - d - this.tail[k].rad;
                            }
                        }
                        else if (this.tail[k].pos.x < vector.x + d + this.tail[k].rad)
                        {
                            numt = vector.x + d + this.tail[k].rad;
                        }
                    }
                    if (this.tail[k].pos.x >= vector.x - d && this.tail[k].pos.x <= vector.x + d)
                    {
                        if (this.tail[k].pos.y < vector.y + d + this.tail[k].rad)
                        {
                            num2t = vector.y + d + this.tail[k].rad;
                        }
                    }
                    if (Mathf.Abs(this.tail[k].pos.x - numt) < Mathf.Abs(this.tail[k].pos.y - num2t) && numt != 0f)
                    {
                        this.tail[k].pos.x = numt;
                        this.tail[k].vel.x = numt - this.tail[k].pos.x;
                        this.tail[k].vel.y = this.tail[k].vel.y * 0.999f;
                        this.tail[k].terrainContact = true;
                    }
                    else if (num2t != 0f)
                    {
                        this.tail[k].pos.y = num2t;
                        this.tail[k].vel.y = num2t - this.tail[k].pos.y;
                        this.tail[k].vel.x = this.tail[k].vel.x * 0.999f;
                        this.tail[k].terrainContact = true;
                    }
                    else
                    { //x is in block
                        Vector2 vector2t = new Vector2(Mathf.Clamp(this.tail[k].pos.x, vector.x - d, vector.x + d), Mathf.Clamp(this.tail[k].pos.y, vector.y - d, vector.y + d));
                        //dis b/w this.tail[k] and center
                        if (Custom.DistLess(this.tail[k].pos, vector2, this.tail[k].rad))
                        {
                            float num3t = Vector2.Distance(this.tail[k].pos, vector2t);
                            Vector2 a = Custom.DirVec(this.tail[k].pos, vector2t);
                            this.tail[k].vel *= 0.999f;
                            this.tail[k].pos -= (this.tail[k].rad - num3t) * a;
                            this.tail[k].vel -= (this.tail[k].rad - num3t) * a;
                            this.tail[k].terrainContact = true;
                        }
                    }
                    */

                    this.tail[k].vel *= Mathf.Lerp(0.75f, 0.95f, num3);
                    TailSegment tailSegment7 = this.tail[k];
                    tailSegment7.vel.y = tailSegment7.vel.y - Mathf.Lerp(0.1f, 0.5f, num3);
                    num3 = (num3 * 10f + 1f) / 11f;
                    if (!Custom.DistLess(this.tail[k].pos, base.owner.bodyChunks[1].pos, 9f * (float)(k + 1)))
                    {
                        this.tail[k].pos = base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, this.tail[k].pos) * 9f * (float)(k + 1);
                    }
                    this.tail[k].vel += Custom.DirVec(vector2, this.tail[k].pos) * num9 / Vector2.Distance(vector2, this.tail[k].pos);
                    num9 *= 0.5f;
                    vector2 = pos;
                    pos = this.tail[k].pos;

                    //ComOptPlugin.LogInfo(string.Concat("Tail ", k," pos: ", this.tail[k].pos.ToString("N2"), " / vel: ", this.tail[k].vel.ToString("N2")));
                }

                this.blink--;
                if (this.blink < -UnityEngine.Random.Range(2, 1800))
                {
                    this.blink = UnityEngine.Random.Range(3, UnityEngine.Random.Range(3, 10));
                }

                if (UnityEngine.Random.value < 0.1f)
                {
                    this.objectLooker.Update();
                }
                if (UnityEngine.Random.value < 0.0025f)
                {
                    this.objectLooker.LookAtNothing();
                }

                this.lastLookDir = this.lookDirection;
                if (this.player.Consious && this.objectLooker.looking)
                {
                    this.lookDirection = Custom.DirVec(this.head.pos, this.objectLooker.mostInterestingLookPoint);
                }
                else
                {
                    this.lookDirection *= 0f;
                }

                if (num > 0.86f)
                {
                    this.blink = 5;
                    this.lookDirection *= -1f;
                }

                if (this.player.standing)
                {
                    this.head.vel -= this.lookDirection * 0.5f;
                    this.drawPositions[0, 0] -= this.lookDirection * 2f;
                }
                else
                {
                    this.head.vel += this.lookDirection;
                }
                Vector2 b = Custom.DirVec(this.drawPositions[1, 0], this.drawPositions[0, 0]) * 3f;
                //this.head.Update();
                vector = new Vector2(this.head.pos.x, 20f * Mathf.RoundToInt((this.head.pos.y - 10f) / 20f) + 10f);
                this.head.lastPos = this.head.pos;
                this.head.pos += this.head.vel;
                this.head.vel *= 0.999f;
                this.head = PushOutOfVirtualTerrain(this.head, vector) as GenericBodyPart;

                this.head.ConnectToPoint(Vector2.Lerp(this.drawPositions[0, 0], this.drawPositions[1, 0], 0.2f) + b, 3f, false, 0.2f, base.owner.bodyChunks[0].vel, 0.7f, 0.1f);
                //this.legs.Update();
                vector = new Vector2(this.legs.pos.x, 20f);
                this.legs.lastPos = this.legs.pos;
                this.legs.pos += this.legs.vel;
                this.legs.vel *= 0.999f;
                this.legs = PushOutOfVirtualTerrain(this.legs, vector) as GenericBodyPart;

                if (base.owner.bodyChunks[1].ContactPoint.y == -1)
                {
                    this.legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(this.legsDirection.x * 8f, 1f), 5f, false, 0.25f, new Vector2(base.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
                    this.legsDirection.x = this.legsDirection.x - (float)base.owner.bodyChunks[1].onSlope;
                    this.legsDirection.y = this.legsDirection.y - 1f;
                }
                else if (this.player.animation == OptionalSlugcat.AnimationIndex.BeamTip)
                {
                    this.legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(0f, -8f), 0f, false, 0.25f, new Vector2(0f, -10f), 0.5f, 0.1f);
                    this.legsDirection += Custom.DirVec(this.drawPositions[0, 0], vector) + new Vector2(0f, -10f); //vector : mid of tile
                }
                else
                {
                    this.legs.ConnectToPoint(base.owner.bodyChunks[1].pos + new Vector2(this.legsDirection.x * 8f, -2f), 4f, false, 0.25f, new Vector2(base.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
                    this.legsDirection += base.owner.bodyChunks[1].vel * 0.01f;
                    this.legsDirection.y = this.legsDirection.y - 0.05f;
                }
                this.legsDirection.Normalize();

                rub = false;
                if (this.player.Consious)
                {
                    //Edit this to reach MousePointer
                    if (this.MouseOver)
                    {
                        Vector2 coord = new Vector2(this.MousePos.x - this.head.pos.x, this.MousePos.y - this.head.pos.y + 18f);
                        this.objectLooker.LookAtPoint(coord, 1f);
                        int num19 = -1;
                        for (int n = 0; n < 2; n++)
                        {
                            if (this.player.grasps[n] == null && this.hands[1 - n].reachedSnapPosition)
                            {
                                num19 = n;
                            }
                        }
                        float dist = new Vector2(this.MousePos.x, this.MousePos.y - 70f).magnitude;

                        if (dist < 10f && Input.GetMouseButton(0))
                        {
                            this.objectLooker.LookAtNothing();
                            rub = true;
                            goto NoTuch;
                        }

                        if (dist < 30f && num19 > -1)
                        {
                            this.hands[num19].reachingForObject = true;
                            this.hands[num19].absoluteHuntPos = this.objectLooker.mostInterestingLookPoint;
                            if (num == 0f)
                            {
                                this.drawPositions[0, 0] += Custom.DirVec(this.drawPositions[0, 0], this.objectLooker.mostInterestingLookPoint) * 5f;
                                this.head.vel += Custom.DirVec(this.drawPositions[0, 0], this.objectLooker.mostInterestingLookPoint) * 2f;
                            }
                        }
                    }
                    else
                    {
                        //this.objectLooker.LookAtPoint(this.head.pos + new Vector2(0f, 0f), 1f);
                        this.objectLooker.LookAtNothing();
                    }
                }
            NoTuch:

                for (int num20 = 0; num20 < 2; num20++)
                {
                    //this.hands[num20].Update();
                    this.hands[num20] = VirtualHandUpdate(this.hands[num20], vector);
                }
                if (this.player.sleepCurlUp > 0f)
                {
                    float num21 = Mathf.Sign(this.player.bodyChunks[0].pos.x - this.player.bodyChunks[1].pos.x);
                    Vector2 vector3 = (this.player.bodyChunks[0].pos + this.player.bodyChunks[1].pos) / 2f;
                    this.drawPositions[0, 0] = Vector2.Lerp(this.drawPositions[0, 0], vector3, this.player.sleepCurlUp * 0.2f);
                    this.drawPositions[1, 0] = Vector2.Lerp(this.drawPositions[1, 0], vector3, this.player.sleepCurlUp * 0.2f);
                    this.drawPositions[0, 0].y += 2f * this.player.sleepCurlUp;
                    this.drawPositions[1, 0].y += 2f * this.player.sleepCurlUp;
                    this.drawPositions[1, 0].x -= 3f * num21 * this.player.sleepCurlUp;
                    for (int num22 = 0; num22 < this.tail.Length; num22++)
                    {
                        float num23 = (float)num22 / (float)(this.tail.Length - 1);
                        this.tail[num22].vel *= 1f - 0.2f * this.player.sleepCurlUp;
                        this.tail[num22].pos = Vector2.Lerp(this.tail[num22].pos, this.drawPositions[1, 0] + new Vector2((Mathf.Sin(num23 * 3.14159274f) * 25f - num23 * 10f) * -num21, Mathf.Lerp(5f, -15f, num23)), 0.1f * this.player.sleepCurlUp);
                    }
                    this.head.vel *= 1f - 0.4f * this.player.sleepCurlUp;
                    this.head.pos = Vector2.Lerp(this.head.pos, vector3 + new Vector2(num21 * 5f, -3f), 0.5f * this.player.sleepCurlUp);
                    if (this.player.sleepCurlUp == 1f || UnityEngine.Random.value < 0.0333333351f)
                    {
                        this.blink = Math.Max(2, this.blink);
                    }
                    for (int num24 = 0; num24 < 2; num24++)
                    {
                        this.hands[num24].absoluteHuntPos = vector3 + new Vector2(num21 * 10f, -20f);
                    }
                }
            }

            public SlugcatHand VirtualHandUpdate(SlugcatHand hand, Vector2 vector)
            {
                hand.lastPos = hand.pos;

                if (hand.retract && hand.mode != Limb.Mode.Retracted)
                {
                    hand.mode = Limb.Mode.HuntAbsolutePosition;
                    hand.absoluteHuntPos = hand.connection.pos;
                    if (Custom.DistLess(hand.absoluteHuntPos, hand.pos, hand.huntSpeed))
                    {
                        hand.mode = Limb.Mode.Retracted;
                    }
                }
                if (hand.mode == Limb.Mode.HuntRelativePosition)
                {
                    hand.absoluteHuntPos = hand.connection.pos + Custom.RotateAroundOrigo(hand.relativeHuntPos, Custom.AimFromOneVectorToAnother(hand.connection.rotationChunk.pos, hand.connection.pos));
                }
                switch (hand.mode)
                {
                    case Limb.Mode.HuntRelativePosition:
                    case Limb.Mode.HuntAbsolutePosition:
                        if (Custom.DistLess(hand.absoluteHuntPos, hand.pos, hand.huntSpeed))
                        {
                            hand.vel = hand.absoluteHuntPos - hand.pos;
                            hand.reachedSnapPosition = true;
                        }
                        else
                        {
                            hand.vel = Vector2.Lerp(hand.vel, Custom.DirVec(hand.pos, hand.absoluteHuntPos) * hand.huntSpeed, hand.quickness);
                            hand.reachedSnapPosition = false;
                        }
                        break;

                    case Limb.Mode.Retracted:
                        hand.vel = hand.connection.vel;
                        hand.pos = hand.connection.pos;
                        hand.reachedSnapPosition = true;
                        break;

                    case Limb.Mode.Dangle:
                        hand.reachedSnapPosition = false;
                        break;
                }
                hand.quickness = 7f;
                hand.huntSpeed = 0.5f;
                if (hand.mode != Limb.Mode.Retracted)
                {
                    hand.pos += hand.vel;
                    if (hand.mode == Limb.Mode.HuntRelativePosition)
                    {
                        hand.pos += hand.connection.vel;
                    }
                    hand.vel *= 0.999f;
                    if (hand.pushOutOfTerrain)
                    {
                        hand = PushOutOfVirtualTerrain(hand, vector) as SlugcatHand;
                    }
                }

                hand.ConnectToPoint(hand.connection.pos, 20f, false, 0f, hand.connection.vel, 0f, 0f);

                bool flag;
                if (hand.reachingForObject)
                {
                    hand.mode = Limb.Mode.HuntAbsolutePosition;
                    flag = false;
                    hand.reachingForObject = false;
                }
                else
                {
                    flag = true;
                    OptionalSlugcat.AnimationIndex animation;
                    switch ((this.owner as OptionalSlugcat).bodyMode)
                    {
                        case OptionalSlugcat.BodyModeIndex.Default:
                            break;

                        case OptionalSlugcat.BodyModeIndex.ClimbingOnBeam:
                            flag = false;
                            switch ((this.owner as OptionalSlugcat).animation)
                            {
                                case OptionalSlugcat.AnimationIndex.BeamTip:
                                    {
                                        hand.mode = Limb.Mode.HuntRelativePosition;
                                        float num = Mathf.Sin(6.28318548f * (this as OptionalSlugcatGraphics).balanceCounter / 300f);
                                        hand.relativeHuntPos.x = -20f + 40f * (float)hand.limbNumber;
                                        hand.relativeHuntPos.y = -4f - 6f * num * ((hand.limbNumber != 0) ? 1f : -1f);
                                        //hand.FindGrip(this.owner.owner.room, hand.connection.pos, this.connection.pos, 100f, this.connection.pos + new Vector2(-10f + 20f * (float)this.limbNumber, (this.limbNumber == 0 != ((this.owner.owner as Player).flipDirection == -1)) ? -5f : 0f), (this.limbNumber != 0) ? -1 : 1, -1, false);
                                        //Nothing to Grip.

                                        if (hand.mode == Limb.Mode.HuntAbsolutePosition)
                                        {
                                            if (hand.pos.y > this.owner.bodyChunks[0].pos.y + 5f)
                                            {
                                                this.head.vel.x = this.head.vel.x + (2f - 4f * (float)hand.limbNumber);
                                            }
                                            if (Mathf.Abs(this.owner.bodyChunks[0].pos.x - this.owner.bodyChunks[1].pos.x) < 10f)
                                            {
                                                this.disbalanceAmount -= 8f;
                                            }
                                        }
                                        else if (this.disbalanceAmount < 40f)
                                        {
                                            float num2 = (40f - this.disbalanceAmount) / 40f;
                                            hand.relativeHuntPos.y = hand.relativeHuntPos.y * (1f - num2);
                                            hand.relativeHuntPos.y = hand.relativeHuntPos.y - num2 * 15f;
                                            hand.relativeHuntPos.x = hand.relativeHuntPos.x * (1f - num2);
                                            if (num2 >= 1f)
                                            {
                                                flag = true;
                                            }
                                        }
                                        hand.huntSpeed = 5f;
                                        hand.quickness = 0.2f;
                                        break;
                                    }
                                case OptionalSlugcat.AnimationIndex.HangUnderVerticalBeam:
                                    hand.mode = Limb.Mode.HuntAbsolutePosition;
                                    hand.absoluteHuntPos = vector + new Vector2((hand.limbNumber != 0) ? 0.5f : -0.5f, (hand.limbNumber != 0) ? 25f : 20f);
                                    hand.huntSpeed = 10f;
                                    hand.quickness = 1f;
                                    break;
                            }
                            break;
                    }
                    animation = (this.owner as OptionalSlugcat).animation;
                    switch (animation)
                    {
                        case OptionalSlugcat.AnimationIndex.DownOnFours:
                            flag = false;
                            hand.mode = Limb.Mode.HuntAbsolutePosition;
                            hand.absoluteHuntPos = hand.connection.pos;
                            hand.absoluteHuntPos.x = hand.absoluteHuntPos.x + (-6f + (float)(12 * hand.limbNumber) + hand.connection.vel.normalized.x * 20f);
                            hand.absoluteHuntPos.y = hand.absoluteHuntPos.y + Mathf.Abs(hand.connection.vel.normalized.y) * -20f;
                            break;

                        default:
                            break;
                    }
                }

                if (flag)
                {
                    if ((this.owner as OptionalSlugcat).grasps?[hand.limbNumber] != null)
                    {
                        hand.mode = Limb.Mode.HuntRelativePosition;
                        hand.relativeHuntPos.x = -20f + 40f * (float)hand.limbNumber;
                        hand.relativeHuntPos.y = -12f;
                        //hand.relativeHuntPos.x = hand.relativeHuntPos.x * (1f - Mathf.Sin((this.owner as OptionalSlugcat).switchHandsProcess * 3.14159274f));
                        if (this.spearDir != 0f && (this.owner as OptionalSlugcat).bodyMode == OptionalSlugcat.BodyModeIndex.Stand)
                        {
                            Vector2 to = Custom.DegToVec(180f + ((hand.limbNumber != 0) ? 1f : -1f) * 8f) * 12f;
                            to.y += Mathf.Sin((float)(this.owner as OptionalSlugcat).animationFrame / 6f * 2f * 3.14159274f) * 2f;
                            hand.relativeHuntPos = Vector2.Lerp(hand.relativeHuntPos, to, Mathf.Abs(this.spearDir));

                            if ((this.owner as OptionalSlugcat).grasps[hand.limbNumber].grabbed is Weapon)
                            {
                                ((this.owner as OptionalSlugcat).grasps[hand.limbNumber].grabbed as Weapon).ChangeOverlap((this.spearDir > -0.4f && hand.limbNumber == 0) || (this.spearDir < 0.4f && hand.limbNumber == 1));
                            }
                        }
                        flag = false;
                    }
                }

                if (flag && hand.mode != Limb.Mode.Retracted)
                {
                    hand.retractCounter++;
                    if ((float)hand.retractCounter > 5f)
                    {
                        hand.mode = Limb.Mode.HuntAbsolutePosition;
                        hand.pos = Vector2.Lerp(hand.pos, this.owner.bodyChunks[0].pos, Mathf.Clamp(((float)hand.retractCounter - 5f) * 0.05f, 0f, 1f));
                        if (Custom.DistLess(hand.pos, this.owner.bodyChunks[0].pos, 2f) && hand.reachedSnapPosition)
                        {
                            hand.mode = Limb.Mode.Retracted;
                        }
                        hand.absoluteHuntPos = this.owner.bodyChunks[0].pos;
                        hand.huntSpeed = 1f + (float)hand.retractCounter * 0.2f;
                        hand.quickness = 1f;
                    }
                }
                else
                {
                    hand.retractCounter -= 10;
                    if (hand.retractCounter < 0)
                    {
                        hand.retractCounter = 0;
                    }
                }

                return hand;
            }

            public BodyPart PushOutOfVirtualTerrain(BodyPart part, Vector2 vector)
            {
                float numt = 0f;
                float num2t = 0f;
                float d = 10f; // 10f
                vector = new Vector2(vector.x, vector.y + d);
                part.terrainContact = false;
                if (part.pos.y >= vector.y - d && part.pos.y <= vector.y + d)
                { //inside of block? in y
                    if (part.lastPos.x < vector.x)
                    {
                        if (part.pos.x > vector.x - d - part.rad)
                        {
                            numt = vector.x - d - part.rad;
                        }
                    }
                    else if (part.pos.x < vector.x + d + part.rad)
                    {
                        numt = vector.x + d + part.rad;
                    }
                }
                if (part.pos.x >= vector.x - d && part.pos.x <= vector.x + d)
                {
                    if (part.pos.y < vector.y + d + part.rad)
                    {
                        num2t = vector.y + d + part.rad;
                    }
                }
                if (Mathf.Abs(part.pos.x - numt) < Mathf.Abs(part.pos.y - num2t) && numt != 0f)
                {
                    part.pos.x = numt;
                    part.vel.x = numt - part.pos.x;
                    part.vel.y = part.vel.y * 0.999f;
                    part.terrainContact = true;
                }
                else if (num2t != 0f)
                {
                    part.pos.y = num2t;
                    part.vel.y = num2t - part.pos.y;
                    part.vel.x = part.vel.x * 0.999f;
                    part.terrainContact = true;
                }
                else
                { //x is in block
                    Vector2 vector2 = new Vector2(Mathf.Clamp(part.pos.x, vector.x - d, vector.x + d), Mathf.Clamp(part.pos.y, vector.y - d, vector.y + d));
                    //dis b/w part and center
                    if (Custom.DistLess(part.pos, vector2, part.rad))
                    {
                        float num3t = Vector2.Distance(part.pos, vector2);
                        Vector2 a = Custom.DirVec(part.pos, vector2);
                        part.vel *= 0.999f;
                        part.pos -= (part.rad - num3t) * a;
                        part.vel -= (part.rad - num3t) * a;
                        part.terrainContact = true;
                    }
                }
                return part;
            }

            public override void Reset()
            {
                for (int i = 0; i < base.owner.bodyChunks.Length; i++)
                {
                    this.drawPositions[i, 0] = base.owner.bodyChunks[i].pos;
                    this.drawPositions[i, 1] = base.owner.bodyChunks[i].pos;
                }
                for (int j = 0; j < this.tail.Length; j++)
                {
                    this.tail[j].Reset(base.owner.bodyChunks[1].pos);
                }
                this.head.Reset(base.owner.bodyChunks[0].pos);
                this.legs.Reset(base.owner.bodyChunks[1].pos);
                this.hands[0].Reset(base.owner.bodyChunks[0].pos);
                this.hands[1].Reset(base.owner.bodyChunks[0].pos);
            }

            public void NudgeDrawPosition(int drawPos, Vector2 nudge)
            {
                this.drawPositions[drawPos, 0] += nudge;
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                this.sprites = new FSprite[12];
                this.sprites[0] = new FSprite("BodyA", true);
                this.sprites[0].anchorY = 0.7894737f;
                this.sprites[1] = new FSprite("HipsA", true);
                TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(4, 5, 6),
                new TriangleMesh.Triangle(5, 6, 7),
                new TriangleMesh.Triangle(8, 9, 10),
                new TriangleMesh.Triangle(9, 10, 11),
                new TriangleMesh.Triangle(12, 13, 14),
                new TriangleMesh.Triangle(2, 3, 4),
                new TriangleMesh.Triangle(3, 4, 5),
                new TriangleMesh.Triangle(6, 7, 8),
                new TriangleMesh.Triangle(7, 8, 9),
                new TriangleMesh.Triangle(10, 11, 12),
                new TriangleMesh.Triangle(11, 12, 13)
                };
                TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
                this.sprites[2] = triangleMesh;
                this.sprites[3] = new FSprite("HeadA0", true);
                this.sprites[4] = new FSprite("LegsA0", true);
                this.sprites[4].anchorY = 0.25f;
                this.sprites[5] = new FSprite("PlayerArm0", true);
                this.sprites[5].anchorX = 0.9f;
                this.sprites[5].scaleY = -1f;
                this.sprites[6] = new FSprite("PlayerArm0", true);
                this.sprites[6].anchorX = 0.9f;
                this.sprites[7] = new FSprite("OnTopOfTerrainHand", true);
                this.sprites[8] = new FSprite("OnTopOfTerrainHand", true);
                this.sprites[8].scaleX = -1f;
                this.sprites[9] = new FSprite("FaceA0", true);
                this.sprites[11] = new FSprite("pixel", true);
                this.sprites[11].scale = 5f;
                this.sprites[10] = new FSprite("Futile_White", true);
                this.sprites[10].shader = FShader.CreateShader("FlatLight", Shader.Find("Futile/FlatLight"));
                this.AddToContainer(null, null, cage);
                //base.InitiateSprites(sLeaser, rCam);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                //timeStacker *= 0.2f;
                float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(this.lastBreath, this.breath, timeStacker) * 3.14159274f * 2f);

                Vector2 vector = Vector2.Lerp(this.drawPositions[0, 1], this.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(this.drawPositions[1, 1], this.drawPositions[1, 0], timeStacker);
                Vector2 vector3 = Vector2.Lerp(this.head.lastPos, this.head.pos, timeStacker);
                if (this.player.aerobicLevel > 0.5f)
                {
                    vector += Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(0.5f, 1f, this.player.aerobicLevel) * 0.5f;
                    vector3 -= Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, this.player.aerobicLevel), 1.5f) * 0.75f;
                }
                float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));
                this.sprites[0].x = vector.x - camPos.x;
                this.sprites[0].y = vector.y - camPos.y - this.player.sleepCurlUp * 4f + Mathf.Lerp(0.5f, 1f, this.player.aerobicLevel) * num * (1f - num2);
                this.sprites[0].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
                this.sprites[0].scaleX = 1f + Mathf.Lerp(Mathf.Lerp(-0.05f, 0.05f, num) * num2, 0.15f, this.player.sleepCurlUp);
                this.sprites[1].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
                this.sprites[1].y = (vector2.y * 2f + vector.y) / 3f - camPos.y - this.player.sleepCurlUp * 3f;
                this.sprites[1].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(this.tail[0].lastPos, this.tail[0].pos, timeStacker));
                this.sprites[1].scaleY = 1f + this.player.sleepCurlUp * 0.2f;
                this.sprites[1].scaleX = 1f + this.player.sleepCurlUp * 0.2f + 0.05f * num;
                Vector2 vector4 = (vector2 * 3f + vector) / 4f;
                float d2 = 6f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vector5 = Vector2.Lerp(this.tail[i].lastPos, this.tail[i].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 a = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;
                    if (i == 0)
                    {
                        d3 = 0f;
                    }
                    (this.sprites[2] as TriangleMesh).MoveVertice(i * 4, vector4 - a * d2 + normalized * d3 - camPos);
                    (this.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + a * d2 + normalized * d3 - camPos);
                    if (i < 3)
                    {
                        (this.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - a * this.tail[i].StretchedRad - normalized * d3 - camPos);
                        (this.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + a * this.tail[i].StretchedRad - normalized * d3 - camPos);
                    }
                    else
                    {
                        (this.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos);
                    }
                    d2 = this.tail[i].StretchedRad;
                    vector4 = vector5;
                }
                float num3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
                int num4 = Mathf.RoundToInt(Mathf.Abs(num3 / 360f * 34f));
                if (this.player.sleepCurlUp > 0f)
                {
                    num4 = 7;
                    num4 = Custom.IntClamp((int)Mathf.Lerp((float)num4, 4f, this.player.sleepCurlUp), 0, 8);
                }
                Vector2 a2 = Vector2.Lerp(this.lastLookDir, this.lookDirection, timeStacker) * 3f * (1f - this.player.sleepCurlUp);
                if (this.player.sleepCurlUp > 0f)
                {
                    this.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + ((this.blink <= 0) ? "A" : "B") + Custom.IntClamp((int)Mathf.Lerp((float)num4, 1f, this.player.sleepCurlUp), 0, 8));
                    this.sprites[9].scaleX = Mathf.Sign(vector.x - vector2.x);
                    this.sprites[9].rotation = num3 * (1f - this.player.sleepCurlUp);

                    num3 = Mathf.Lerp(num3, 45f * Mathf.Sign(vector.x - vector2.x), this.player.sleepCurlUp);
                    vector3.y += 1f * this.player.sleepCurlUp;
                    vector3.x += Mathf.Sign(vector.x - vector2.x) * 2f * this.player.sleepCurlUp;
                    a2.y -= 2f * this.player.sleepCurlUp;
                    a2.x -= 4f * Mathf.Sign(vector.x - vector2.x) * this.player.sleepCurlUp;
                }
                else if (this.player.bodyMode != OptionalSlugcat.BodyModeIndex.Dead)
                {
                    Vector2 p = vector3 - vector2;
                    p.x *= 1f - a2.magnitude / 3f;
                    p = p.normalized;
                    this.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + ((this.blink <= 0) ? "A" : "B") + Mathf.RoundToInt(Mathf.Abs(Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), p) / 22.5f)));
                    if (rub) { this.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned"); }
                    if (Mathf.Abs(a2.x) < 0.1f)
                    {
                        this.sprites[9].scaleX = ((num3 >= 0f) ? 1f : -1f);
                    }
                    else
                    {
                        this.sprites[9].scaleX = Mathf.Sign(a2.x);
                    }
                    this.sprites[9].rotation = 0f;
                }
                else
                {
                    a2 *= 0f;
                    num4 = 0;
                    this.sprites[9].element = Futile.atlasManager.GetElementWithName("Face" + ((!this.player.dead) ? "Stunned" : "Dead"));
                    this.sprites[9].rotation = num3;
                }
                this.sprites[3].x = vector3.x - camPos.x;
                this.sprites[3].y = vector3.y - camPos.y;
                this.sprites[3].rotation = num3;
                this.sprites[3].scaleX = ((num3 >= 0f) ? 1f : -1f);
                this.sprites[3].element = Futile.atlasManager.GetElementWithName("HeadA" + num4);
                this.sprites[9].x = vector3.x + a2.x - camPos.x;
                this.sprites[9].y = vector3.y + a2.y - 2f - camPos.y;
                Vector2 vector6 = Vector2.Lerp(this.legs.lastPos, this.legs.pos, timeStacker);
                this.sprites[4].x = vector6.x - camPos.x;
                this.sprites[4].y = vector6.y - camPos.y;
                this.sprites[4].rotation = Custom.AimFromOneVectorToAnother(this.legsDirection, new Vector2(0f, 0f));
                this.sprites[4].isVisible = true;
                string elementName = "LegsAAir0";
                switch (this.player.bodyMode)
                {
                    case OptionalSlugcat.BodyModeIndex.Default:
                        break;

                    case OptionalSlugcat.BodyModeIndex.Stand:
                        elementName = "LegsA" + this.player.animationFrame;
                        this.sprites[4].scaleX = (float)((this.player.flipDirection <= 0) ? -1 : 1);
                        break;

                    case OptionalSlugcat.BodyModeIndex.ClimbingOnBeam:
                        if (this.player.animation == OptionalSlugcat.AnimationIndex.BeamTip)
                        {
                            elementName = "LegsAPole";
                        }
                        break;
                }
                this.sprites[4].element = Futile.atlasManager.GetElementWithName(elementName);
                for (int j = 0; j < 2; j++)
                {
                    Vector2 vector7 = Vector2.Lerp(this.hands[j].lastPos, this.hands[j].pos, timeStacker);
                    if (this.hands[j].mode != Limb.Mode.Retracted)
                    {
                        this.sprites[5 + j].x = vector7.x - camPos.x;
                        this.sprites[5 + j].y = vector7.y - camPos.y;
                        float num6 = 4.5f / ((float)this.hands[j].retractCounter + 1f);
                        if (this.player.animation == OptionalSlugcat.AnimationIndex.BeamTip && this.disbalanceAmount <= 40f && this.hands[j].mode == Limb.Mode.HuntRelativePosition)
                        {
                            num6 *= this.disbalanceAmount / 40f;
                        }
                        num6 *= Mathf.Abs(Mathf.Cos(Custom.AimFromOneVectorToAnother(vector2, vector) / 360f * 3.14159274f * 2f));
                        Vector2 vector8 = vector + Custom.RotateAroundOrigo(new Vector2((-1f + 2f * (float)j) * num6, -3.5f), Custom.AimFromOneVectorToAnother(vector2, vector));
                        this.sprites[5 + j].element = Futile.atlasManager.GetElementWithName("PlayerArm" + Mathf.RoundToInt(Mathf.Clamp(Vector2.Distance(vector7, vector8) / 2f, 0f, 12f)));
                        this.sprites[5 + j].rotation = Custom.AimFromOneVectorToAnother(vector7, vector8) + 90f;
                        OptionalSlugcat.BodyModeIndex bodyMode = this.player.bodyMode;

                        this.sprites[5 + j].scaleY = Mathf.Sign(Custom.DistanceToLine(vector7, vector, vector2));

                        if (this.player.animation == OptionalSlugcat.AnimationIndex.HangUnderVerticalBeam)
                        {
                            this.sprites[5 + j].scaleY = ((j != 0) ? -1f : 1f);
                        }
                    }
                    this.sprites[5 + j].isVisible = (this.hands[j].mode != Limb.Mode.Retracted);
                    this.sprites[7 + j].isVisible = false;
                }
                Vector2 vector9 = vector3 + Custom.DirVec(vector2, vector3) * 30f + new Vector2(0f, 30f);
                this.sprites[11].x = vector9.x - camPos.x;
                this.sprites[11].y = vector9.y - camPos.y;
                this.sprites[11].alpha = Mathf.Lerp(this.lastMarkAlpha, this.markAlpha, timeStacker);
                this.sprites[10].x = vector9.x - camPos.x;
                this.sprites[10].y = vector9.y - camPos.y;
                this.sprites[10].alpha = 0.2f * Mathf.Lerp(this.lastMarkAlpha, this.markAlpha, timeStacker);
                this.sprites[10].scale = 1f + Mathf.Lerp(this.lastMarkAlpha, this.markAlpha, timeStacker);

                this.sprites[9].color = new Color(0.01f, 0f, 0f);
                //base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                if (this.sprites[0].isVisible == this.culled)
                {
                    for (int j = 0; j < this.sprites.Length; j++)
                    {
                        this.sprites[j].isVisible = !this.culled;
                    }
                }
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                Color color = PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter);
                Color color2 = palette.blackColor;

                if (this.player.playerState.slugcatCharacter == 3)
                {
                    color2 = Color.Lerp(new Color(1f, 1f, 1f), color, 0.3f);
                    color = Color.Lerp(palette.blackColor, Custom.HSL2RGB(0.63055557f, 0.54f, 0.5f), Mathf.Lerp(0.08f, 0.04f, palette.darkness));
                }
                for (int i = 0; i < this.sprites.Length; i++)
                {
                    this.sprites[i].color = color;
                }
                this.sprites[11].color = Color.Lerp(PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter), Color.white, 0.3f);
                this.sprites[10].color = PlayerGraphics.SlugcatColor(this.player.playerState.slugcatCharacter);
                this.sprites[9].color = color2;
            }

            public static Color SlugcatColor(int i)
            {
                return PlayerGraphics.SlugcatColor(i);
            }

            public FSprite[] sprites;

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser = null, RoomCamera rCam = null, FContainer newContatiner = null)
            {
                if (newContatiner == null)
                {
                    newContatiner = this.cage;
                }
                for (int i = 0; i < this.sprites.Length; i++)
                {
                    if ((i > 6 && i < 9) || i > 9)
                    {
                        cage.AddChild(this.sprites[i]);
                    }
                    else
                    {
                        fence.AddChild(this.sprites[i]);
                    }
                }
            }

            public void LookAtPoint(Vector2 point, float interest)
            {
                this.objectLooker.LookAtPoint(point, interest);
            }

            public void LookAtNothing()
            {
                this.objectLooker.LookAtNothing();
            }

            public TailSegment[] tail;
            public GenericBodyPart head;
            private GenericBodyPart legs;
            public Vector2[,] drawPositions;
            private Vector2 legsDirection;
            public float disbalanceAmount;
            public float balanceCounter;
            public SlugcatHand[] hands;
            private OptionalSlugcat player;
            public float airborneCounter;
            public Vector2 lookDirection;
            public Vector2 lastLookDir;
            public int blink;
            public float lastMarkAlpha;
            public float markAlpha;
            private PlayerObjectLooker objectLooker;
            public LightSource lightSource;
            public float spearDir;
            public float markBaseAlpha = 1f;
            public float breath;
            public float lastBreath;

            private class PlayerObjectLooker
            {
                public PlayerObjectLooker(OptionalSlugcatGraphics owner)
                {
                    this.owner = owner;
                }

                public bool looking
                {
                    get
                    {
                        return this.currentMostInteresting != null || this.lookAtPoint != null;
                    }
                }

                public Vector2 mostInterestingLookPoint
                {
                    get
                    {
                        if (this.lookAtPoint != null)
                        {
                            return this.lookAtPoint.Value;
                        }
                        if (this.currentMostInteresting == null)
                        {
                            return this.owner.head.pos + this.owner.lookDirection * 100f;
                        }
                        if (this.currentMostInteresting is Creature)
                        {
                            return (this.currentMostInteresting as Creature).DangerPos;
                        }
                        return this.currentMostInteresting.firstChunk.pos;
                    }
                }

                public void Update()
                {
                    this.timeLookingAtThis++;

                    float num = (this.timeLookingAtThis >= 8) ? -0.5f : 0.5f;

                    if (this.lookAtPoint != null)
                    {
                        num = this.lookAtPointInterest + (this.timeLookingAtThis / 8) * ((this.timeLookingAtThis >= 8) ? -0.5f : 0.5f);
                    }
                    /*
                    foreach (UpdatableAndDeletable updatableAndDeletable in this.owner.player.room.updateList)
                    {
                        if (updatableAndDeletable != this.owner.player && updatableAndDeletable is PhysicalObject && this.HowInterestingIsThisObject(updatableAndDeletable as PhysicalObject) > num)
                        {
                            this.timeLookingAtThis = 0;
                            this.currentMostInteresting = (updatableAndDeletable as PhysicalObject);
                            num = this.HowInterestingIsThisObject(updatableAndDeletable as PhysicalObject);
                        }
                    }*/
                    if (num < 0.2f && UnityEngine.Random.value < 0.5f) //0.5f
                    {
                        this.LookAtNothing();
                    }
                }

                public void LookAtNothing()
                {
                    this.currentMostInteresting = null;
                    this.lookAtPoint = null;
                    this.timeLookingAtThis = 0;
                }

                public void LookAtPoint(Vector2 point, float interest)
                {
                    this.lookAtPointInterest = interest;
                    this.lookAtPoint = new Vector2?(point);
                    this.timeLookingAtThis = 0;
                }

                public OptionalSlugcatGraphics owner;
                public PhysicalObject currentMostInteresting;
                public int timeLookingAtThis;
                private Vector2? lookAtPoint;
                private float lookAtPointInterest;
            }
        }
    }
}