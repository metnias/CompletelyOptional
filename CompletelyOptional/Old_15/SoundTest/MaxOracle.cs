using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class MaxOracle : Oracle
    {
        public MaxOracle(AbstractPhysicalObject abstractPhysicalObject, Room room) : base(abstractPhysicalObject, room)
        {
            this.room = room;
            this.ID = OracleID.DJ;
            base.bodyChunks = new BodyChunk[2];
            for (int i = 0; i < base.bodyChunks.Length; i++)
            {
                base.bodyChunks[i] = new BodyChunk(this, i, new Vector2(350f, 350f), 6f, 0.5f);
            }
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[1];
            this.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 9f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.mySwarmers = new List<OracleSwarmer>();
            base.airFriction = 0.99f;
            base.gravity = 0.0f;
            this.bounce = 0.1f;
            this.surfaceFriction = 0.17f;
            this.collisionLayer = 1;
            base.waterFriction = 0.92f;
            base.buoyancy = 0.95f;

            this.oracleBehavior = new MaxOracleBehavior(this);
            this.myScreen = new OracleProjectionScreen(room, this.oracleBehavior);
            room.AddObject(this.myScreen);
            this.marbles = new List<PebblesPearl>();
            //this.SetUpMarbles();
            room.gravity = 0f;
            for (int j = 0; j < room.updateList.Count; j++)
            {
                if (room.updateList[j] is AntiGravity)
                {
                    (room.updateList[j] as AntiGravity).active = false;
                    break;
                }
            }

            this.arm = new OracleArm(this);
        }

        public new bool Consious
        {
            get
            {
                return true;
            }
        }

        public override void InitiateGraphicsModule()
        {
            if (base.graphicsModule == null)
            {
                base.graphicsModule = new MaxGraphics(this);
            }
        }

        public override void Update(bool eu)
        {
            //base.Update(eu);
            for (int i = 0; i < this.bodyChunks.Length; i++)
            {
                this.bodyChunks[i].Update();
            }
            this.abstractPhysicalObject.pos.Tile = this.room.GetTilePosition(this.firstChunk.pos);
            for (int j = 0; j < this.bodyChunkConnections.Length; j++)
            {
                this.bodyChunkConnections[j].Update();
            }

            if (this.room.abstractRoom.index != this.abstractPhysicalObject.pos.room)
            {
                Debug.Log("ROOM MISMATCH FOR PHYSICAL OBJECT " + this.abstractPhysicalObject.type.ToString());
                if (this.abstractPhysicalObject is AbstractCreature)
                {
                    Debug.Log("critter name: " + (this.abstractPhysicalObject as AbstractCreature).creatureTemplate.name);
                }
                this.abstractPhysicalObject.pos.room = this.room.abstractRoom.index;
            }

            this.evenUpdate = eu;

            if (this.appendages != null)
            {
                for (int l = 0; l < this.appendages.Count; l++)
                {
                    this.appendages[l].Update();
                }
            }

            this.oracleBehavior.Update(eu);
            if (base.graphicsModule != null && !(base.graphicsModule as MaxGraphics).initiated)
            {
                (base.graphicsModule as MaxGraphics).initiated = true;
                for (int i = 0; i < 100; i++)
                {
                    (base.graphicsModule as MaxGraphics).Update();
                }
            }
            this.arm.Update();
            if (this.spasms > 0)
            {
                this.spasms--;
                base.firstChunk.vel += Custom.RNV() * UnityEngine.Random.value * 5f;
                base.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 5f;
            }
            if (this.stun > 0)
            {
                this.stun--;
            }
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
        }

        private void SetUpMarbles()
        {
            for (int i = 0; i < 6; i++)
            {
                this.CreateMarble(this, new Vector2(500f, 300f) + Custom.RNV() * 20f, 0, 35f, (i != 2 && i != 3) ? ((i != 5) ? 0 : 2) : 1);
            }
            for (int j = 0; j < 2; j++)
            {
                this.CreateMarble(this, new Vector2(500f, 300f) + Custom.RNV() * 20f, 1, 100f, (j != 1) ? 0 : 2);
            }
            this.CreateMarble(null, new Vector2(220f, 300f), 0, 0f, 1);
            Vector2 a = new Vector2(280f, 130f);
            Vector2 vector = Custom.DegToVec(-32.7346f);
            Vector2 a2 = Custom.PerpendicularVector(vector);
            for (int k = 0; k < 3; k++)
            {
                for (int l = 0; l < 5; l++)
                {
                    if (k != 2 || l != 2)
                    {
                        this.CreateMarble(null, a + a2 * (float)k * 17f + vector * (float)l * 17f, 0, 0f, ((k != 2 || l != 0) && (k != 1 || l != 3)) ? 1 : 2);
                    }
                }
            }
            this.CreateMarble(null, new Vector2(687f, 318f), 0, 0f, 1);
            this.CreateMarble(this.marbles[this.marbles.Count - 1], new Vector2(687f, 318f), 0, 18f, 0);
            this.CreateMarble(null, new Vector2(650f, 567f), 0, 0f, 2);
            this.CreateMarble(this.marbles[this.marbles.Count - 1], new Vector2(640f, 577f), 0, 38f, 1);
            this.CreateMarble(this.marbles[this.marbles.Count - 2], new Vector2(640f, 577f), 0, 38f, 2);
            this.CreateMarble(null, new Vector2(317f, 100f), 0, 0f, 2);
            this.CreateMarble(null, new Vector2(747f, 474f), 0, 0f, 0);
            this.CreateMarble(null, new Vector2(314f, 600f), 0, 0f, 2);
            this.CreateMarble(null, new Vector2(308f, 611f), 0, 0f, 2);
            this.CreateMarble(null, new Vector2(751f, 231f), 0, 0f, 1);
            this.CreateMarble(null, new Vector2(760f, 224f), 0, 0f, 1);
            this.CreateMarble(null, new Vector2(720f, 234f), 0, 0f, 0);
            this.CreateMarble(null, new Vector2(309f, 452f), 0, 0f, 0);
            this.CreateMarble(this.marbles[this.marbles.Count - 1], new Vector2(309f, 452f), 0, 42f, 1);
            this.marbles[this.marbles.Count - 1].orbitSpeed = 0.8f;
            this.CreateMarble(this.marbles[this.marbles.Count - 1], new Vector2(309f, 452f), 0, 12f, 0);
        }

        private void CreateMarble(PhysicalObject orbitObj, Vector2 ps, int circle, float dist, int color)
        {
            AbstractPhysicalObject abstractPhysicalObject = new PebblesPearl.AbstractPebblesPearl(this.room.world, null, this.room.GetWorldCoordinate(ps), this.room.game.GetNewID(), -1, -1, null, color, this.pearlCounter);
            this.pearlCounter++;
            this.room.abstractRoom.entities.Add(abstractPhysicalObject);
            PebblesPearl pebblesPearl = new PebblesPearl(abstractPhysicalObject, this.room.world);
            //pebblesPearl.oracle = this;
            pebblesPearl.firstChunk.HardSetPosition(ps);
            pebblesPearl.orbitObj = orbitObj;
            if (orbitObj == null)
            {
                pebblesPearl.hoverPos = new Vector2?(ps);
            }
            pebblesPearl.orbitCircle = circle;
            pebblesPearl.orbitDistance = dist;
            pebblesPearl.marbleColor = color;
            this.room.AddObject(pebblesPearl);
            this.marbles.Add(pebblesPearl);
        }

        private void SetUpSwarmers()
        {
            /*
            this.glowers = 5;

            Debug.Log("Moon spawning " + this.glowers + " swarmers");
            for (int i = 0; i < this.glowers; i++)
            {
                Vector2 vector = this.oracleBehavior.OracleGetToPos + new Vector2(0f, 100f) + Custom.RNV() * UnityEngine.Random.value * 50f;
                SLOracleSwarmer sloracleSwarmer = new SLOracleSwarmer(new AbstractPhysicalObject(this.room.world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, this.room.GetWorldCoordinate(vector), this.room.game.GetNewID()), this.room.world);
                this.room.abstractRoom.entities.Add(sloracleSwarmer.abstractPhysicalObject);
                sloracleSwarmer.firstChunk.HardSetPosition(vector);
                this.room.AddObject(sloracleSwarmer);
                this.mySwarmers.Add(sloracleSwarmer);

                sloracleSwarmer.hoverAtGrabablePos = false;
            }
            this.health = Mathf.InverseLerp(0f, 5f, (float)this.glowers);
            */
        }

        public void GlowerEaten()
        {
            if (this.glowers > 0)
            {
                this.health -= 1f / (float)this.glowers;
            }
            base.firstChunk.vel += Custom.DegToVec(-45f + UnityEngine.Random.value * 90f) * 4f;
            this.spasms = 44;
            this.stun = Math.Max(this.stun, 183);
        }

        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);
            //(this.oracleBehavior as SSOracleBehavior).NewAction(SSOracleBehavior.Action.ThrowOut_KillOnSight);
        }

        public override void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
        {
            base.GraphicsModuleUpdated(actuallyViewed, eu);
            /*if (this.oracleBehavior is MaxOracleBehavior && (this.oracleBehavior as MaxOracleBehavior).currSubBehavior is MaxOracleBehavior.SSOracleGetGreenNeuron)
            {
                ((this.oracleBehavior as MaxOracleBehavior).currSubBehavior as MaxOracleBehavior.SSOracleGetGreenNeuron).HoldingNeuronUpdate(eu);
            }*/
        }

        public MaxOracleBehavior oracleBehavior;
        public MaxOracle.OracleArm arm;
        public OracleID ID;
        public OracleProjectionScreen myScreen;
        public List<PebblesPearl> marbles;
        public StarMatrix starMatrix;
        public List<OracleSwarmer> mySwarmers;
        public float health = 1f;
        public int glowers;
        public int spasms;
        public int stun;
        private int pearlCounter;

        public enum OracleID
        {
            //SS,
            //SL,
            DJ
        }

        public new class OracleArm
        {
            public OracleArm(MaxOracle oracle)
            {
                this.oracle = oracle;

                this.baseMoveSoundLoop = new StaticSoundLoop(SoundID.SS_AI_Base_Move_LOOP, oracle.firstChunk.pos, oracle.room, 1f, 1f);

                this.cornerPositions = new Vector2[4];
                //this.cornerPositions[0] = oracle.room.MiddleOfTile(10, 31);
                //this.cornerPositions[1] = oracle.room.MiddleOfTile(38, 31);
                //this.cornerPositions[2] = oracle.room.MiddleOfTile(38, 3);
                //this.cornerPositions[3] = oracle.room.MiddleOfTile(10, 3);
                this.cornerPositions[0] = new Vector2(50, 610);
                this.cornerPositions[1] = new Vector2(610, 610);
                this.cornerPositions[2] = new Vector2(610, 50);
                this.cornerPositions[3] = new Vector2(50, 50);
                this.joints = new OracleArm.Joint[4];
                for (int j = 0; j < this.joints.Length; j++)
                {
                    this.joints[j] = new OracleArm.Joint(this, j);
                    if (j > 0)
                    {
                        this.joints[j].previous = this.joints[j - 1];
                        this.joints[j - 1].next = this.joints[j];
                    }
                }
                this.framePos = 10002.5f;
                this.lastFramePos = this.framePos;
            }

            public Vector2 BasePos(float timeStacker)
            {
                return this.OnFramePos(Mathf.Lerp(this.lastFramePos, this.framePos, timeStacker));
            }

            public void Update()
            {
                this.oracle.bodyChunks[1].vel *= 0.4f;
                this.oracle.bodyChunks[0].vel *= 0.4f;
                this.oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(this.oracle.oracleBehavior.OracleGetToPos - this.oracle.bodyChunks[0].pos, 100f) / 100f * 6.2f;
                this.oracle.bodyChunks[1].vel += Vector2.ClampMagnitude(this.oracle.oracleBehavior.OracleGetToPos - this.oracle.oracleBehavior.GetToDir * this.oracle.bodyChunkConnections[0].distance - this.oracle.bodyChunks[0].pos, 100f) / 100f * 3.2f;
                Vector2 baseGetToPos = this.oracle.oracleBehavior.BaseGetToPos;
                Vector2 vector = new Vector2(Mathf.Clamp(baseGetToPos.x, this.cornerPositions[0].x, this.cornerPositions[1].x), this.cornerPositions[0].y);
                float num = Vector2.Distance(vector, baseGetToPos);
                float num2 = Mathf.InverseLerp(this.cornerPositions[0].x, this.cornerPositions[1].x, baseGetToPos.x);
                for (int i = 1; i < 4; i++)
                {
                    Vector2 vector2;
                    if (i % 2 == 0)
                    {
                        vector2 = new Vector2(Mathf.Clamp(baseGetToPos.x, this.cornerPositions[0].x, this.cornerPositions[1].x), this.cornerPositions[i].y);
                    }
                    else
                    {
                        vector2 = new Vector2(this.cornerPositions[i].x, Mathf.Clamp(baseGetToPos.y, this.cornerPositions[2].y, this.cornerPositions[0].y));
                    }
                    float num3 = Vector2.Distance(vector2, baseGetToPos);
                    if (num3 < num)
                    {
                        vector = vector2;
                        num = num3;
                        if (i == 1)
                        {
                            num2 = (float)i + Mathf.InverseLerp(this.cornerPositions[0].y, this.cornerPositions[2].y, baseGetToPos.y);
                        }
                        else if (i == 2)
                        {
                            num2 = (float)i + Mathf.InverseLerp(this.cornerPositions[1].x, this.cornerPositions[0].x, baseGetToPos.x);
                        }
                        else if (i == 3)
                        {
                            num2 = (float)i + Mathf.InverseLerp(this.cornerPositions[2].y, this.cornerPositions[0].y, baseGetToPos.y);
                        }
                    }
                }
                this.baseMoving = (Vector2.Distance(this.BasePos(1f), vector) > ((!this.baseMoving) ? 350f : 50f) && this.oracle.oracleBehavior.consistentBasePosCounter > 30);
                this.lastFramePos = this.framePos;
                if (this.baseMoving)
                {
                    this.framePos = Mathf.MoveTowardsAngle(this.framePos * 90f, num2 * 90f, 1f) / 90f;
                    if (this.baseMoveSoundLoop != null)
                    {
                        this.baseMoveSoundLoop.volume = Mathf.Min(this.baseMoveSoundLoop.volume + 0.1f, 1f);
                        this.baseMoveSoundLoop.pitch = Mathf.Min(this.baseMoveSoundLoop.pitch + 0.025f, 1f);
                    }
                }
                else if (this.baseMoveSoundLoop != null)
                {
                    this.baseMoveSoundLoop.volume = Mathf.Max(this.baseMoveSoundLoop.volume - 0.1f, 0f);
                    this.baseMoveSoundLoop.pitch = Mathf.Max(this.baseMoveSoundLoop.pitch - 0.025f, 0.5f);
                }
                if (this.baseMoveSoundLoop != null)
                {
                    this.baseMoveSoundLoop.pos = this.BasePos(1f);
                    this.baseMoveSoundLoop.Update();
                }

                for (int j = 0; j < this.joints.Length; j++)
                {
                    this.joints[j].Update();
                }
            }

            public Vector2 BaseDir(float timeStacker)
            {
                float num = Mathf.Lerp(this.lastFramePos, this.framePos, timeStacker) % 4f;
                float num2 = 0.1f;
                if (num < num2)
                {
                    return Vector3.Slerp(new Vector2(1f, 0f), new Vector2(0f, -1f), 0.5f + Mathf.InverseLerp(0f, num2, num) * 0.5f);
                }
                if (num < 1f - num2)
                {
                    return new Vector2(0f, -1f);
                }
                if (num < 1f + num2)
                {
                    return Vector3.Slerp(new Vector2(0f, -1f), new Vector2(-1f, 0f), Mathf.InverseLerp(1f - num2, 1f + num2, num));
                }
                if (num < 2f - num2)
                {
                    return new Vector2(-1f, 0f);
                }
                if (num < 2f + num2)
                {
                    return Vector3.Slerp(new Vector2(-1f, 0f), new Vector2(0f, 1f), Mathf.InverseLerp(2f - num2, 2f + num2, num));
                }
                if (num < 3f - num2)
                {
                    return new Vector2(0f, 1f);
                }
                if (num < 3f + num2)
                {
                    return Vector3.Slerp(new Vector2(0f, 1f), new Vector2(1f, 0f), Mathf.InverseLerp(3f - num2, 3f + num2, num));
                }
                if (num < 4f - num2)
                {
                    return new Vector2(1f, 0f);
                }
                return Vector3.Slerp(new Vector2(1f, 0f), new Vector2(0f, -1f), Mathf.InverseLerp(4f - num2, 4f, num) * 0.5f);
            }

            public Vector2 OnFramePos(float timeStacker)
            {
                float num = Mathf.Lerp(this.lastFramePos, this.framePos, timeStacker) % 4f;
                float num2 = 0.1f;
                float num3 = Mathf.Abs(this.cornerPositions[0].x - this.cornerPositions[1].x) * num2;
                Vector2 a = default(Vector2);
                float ang;
                if (num < num2)
                {
                    a = new Vector2(this.cornerPositions[0].x + num3, this.cornerPositions[1].y - num3);
                    ang = -45f + Mathf.InverseLerp(0f, num2, num) * 45f;
                }
                else
                {
                    if (num < 1f - num2)
                    {
                        return Vector2.Lerp(this.cornerPositions[0], this.cornerPositions[1], Mathf.InverseLerp(0f, 1f, num));
                    }
                    if (num < 1f + num2)
                    {
                        a = new Vector2(this.cornerPositions[1].x - num3, this.cornerPositions[1].y - num3);
                        ang = Mathf.InverseLerp(1f - num2, 1f + num2, num) * 90f;
                    }
                    else
                    {
                        if (num < 2f - num2)
                        {
                            return Vector2.Lerp(this.cornerPositions[1], this.cornerPositions[2], Mathf.InverseLerp(1f, 2f, num));
                        }
                        if (num < 2f + num2)
                        {
                            a = new Vector2(this.cornerPositions[2].x - num3, this.cornerPositions[2].y + num3);
                            ang = 90f + Mathf.InverseLerp(2f - num2, 2f + num2, num) * 90f;
                        }
                        else
                        {
                            if (num < 3f - num2)
                            {
                                return Vector2.Lerp(this.cornerPositions[2], this.cornerPositions[3], Mathf.InverseLerp(2f, 3f, num));
                            }
                            if (num < 3f + num2)
                            {
                                a = new Vector2(this.cornerPositions[3].x + num3, this.cornerPositions[3].y + num3);
                                ang = 180f + Mathf.InverseLerp(3f - num2, 3f + num2, num) * 90f;
                            }
                            else
                            {
                                if (num < 4f - num2)
                                {
                                    return Vector2.Lerp(this.cornerPositions[3], this.cornerPositions[0], Mathf.InverseLerp(3f, 4f, num));
                                }
                                a = new Vector2(this.cornerPositions[0].x + num3, this.cornerPositions[0].y - num3);
                                ang = 270f + Mathf.InverseLerp(4f - num2, 4f, num) * 45f;
                            }
                        }
                    }
                }
                return a + Custom.DegToVec(ang) * num3;
            }

            public MaxOracle oracle;
            public OracleArm.Joint[] joints;
            public Vector2[] cornerPositions;
            public float lastFramePos;
            public float framePos;
            public bool baseMoving;
            public StaticSoundLoop baseMoveSoundLoop;

            public class Joint
            {
                public Joint(OracleArm arm, int index)
                {
                    this.arm = arm;
                    this.index = index;
                    this.currentInvKinFlip = ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                    switch (index)
                    {
                        case 0:
                            this.totalLength = 300f;
                            break;

                        case 1:
                            this.totalLength = 150f;
                            break;

                        case 2:
                            this.totalLength = 90f;
                            break;

                        case 3:
                            this.totalLength = 30f;
                            break;
                    }
                    this.pos = arm.BasePos(1f);
                    this.lastPos = this.pos;
                }

                public Vector2 ElbowPos(float timeStacker, Vector2 Tip)
                {
                    Vector2 vc = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
                    if (this.next != null)
                    {
                        return Custom.InverseKinematic(Tip, vc, this.totalLength * 0.333333343f, this.totalLength * 0.6666667f, (this.index % 2 != 0) ? -1f : 1f);
                    }
                    return Custom.InverseKinematic(Tip, vc, this.totalLength / 3f, this.totalLength / 3f, (this.index % 2 != 0) ? -1f : 1f);
                }

                public void Update()
                {
                    this.lastPos = this.pos;
                    this.pos += this.vel;
                    this.vel *= 0.8f;
                    if ((float)this.index == 0f)
                    {
                        this.pos = this.arm.BasePos(1f);
                    }
                    else if (this.index < this.arm.joints.Length - 1)
                    {
                        if (this.index == 1 && (this.arm.baseMoving || this.arm.oracle.room.GetTile(this.previous.ElbowPos(1f, this.pos)).Solid))
                        {
                            Vector2 vector = Custom.InverseKinematic(this.previous.pos, this.next.pos, this.previous.totalLength, this.totalLength, this.currentInvKinFlip);
                            Vector2 from = Custom.InverseKinematic(this.previous.pos, this.next.pos, this.previous.totalLength, this.totalLength, -this.currentInvKinFlip);
                            float num = (!this.arm.oracle.room.GetTile(vector).Solid) ? 0f : 10f;
                            num += ((!this.arm.oracle.room.GetTile(this.previous.ElbowPos(1f, Vector2.Lerp(vector, this.previous.pos, 0.2f))).Solid) ? 0f : 1f);
                            float num2 = (!this.arm.oracle.room.GetTile(from).Solid) ? 0f : 10f;
                            num2 += ((!this.arm.oracle.room.GetTile(this.previous.ElbowPos(1f, Vector2.Lerp(from, this.previous.pos, 0.2f))).Solid) ? 0f : 1f);
                            if (num > num2)
                            {
                                this.currentInvKinFlip *= -1f;
                                vector = Custom.InverseKinematic(this.previous.pos, this.next.pos, this.previous.totalLength, this.totalLength, this.currentInvKinFlip);
                            }
                            else if (num == 0f)
                            {
                                this.vel += Vector2.ClampMagnitude(vector - this.pos, 100f) / 100f * 1.8f;
                            }
                        }
                        SharedPhysics.TerrainCollisionData terrainCollisionData = new SharedPhysics.TerrainCollisionData(this.pos, this.lastPos, this.vel, 1f, new IntVector2(0, 0), true);
                        terrainCollisionData = SharedPhysics.VerticalCollision(this.arm.oracle.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(this.arm.oracle.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(this.arm.oracle.room, terrainCollisionData);
                        this.pos = terrainCollisionData.pos;
                        this.vel = terrainCollisionData.vel;
                    }
                    if (this.next != null)
                    {
                        Vector2 vector2 = Custom.DirVec(this.pos, this.next.pos);
                        float num3 = Vector2.Distance(this.pos, this.next.pos);
                        float num4 = 0.5f;
                        if (this.index == 0)
                        {
                            num4 = 0f;
                        }
                        else if (this.index == this.arm.joints.Length - 2)
                        {
                            num4 = 1f;
                        }
                        float num5 = -1f;
                        float num6 = 0.5f;
                        if (this.previous != null)
                        {
                            Vector2 lhs = Custom.DirVec(this.previous.pos, this.pos);
                            num6 = Custom.LerpMap(Vector2.Dot(lhs, vector2), -1f, 1f, 1f, 0.2f);
                        }
                        if (num3 > this.totalLength)
                        {
                            num5 = this.totalLength;
                        }
                        else if (num3 < this.totalLength * num6)
                        {
                            num5 = this.totalLength * num6;
                        }
                        if (num5 > 0f)
                        {
                            this.pos += vector2 * (num3 - num5) * num4;
                            this.vel += vector2 * (num3 - num5) * num4;
                            this.next.vel -= vector2 * (num3 - num5) * (1f - num4);
                        }
                    }
                    else
                    {
                        Vector2 a = this.arm.oracle.bodyChunks[1].pos;

                        a -= this.arm.oracle.oracleBehavior.GetToDir * this.totalLength / 2f;

                        a += Custom.DirVec(this.arm.oracle.bodyChunks[1].pos, this.pos) * this.totalLength / 2f;
                        this.vel += Vector2.ClampMagnitude(a - this.pos, 50f) / 50f * 1.2f;
                        this.pos += Vector2.ClampMagnitude(a - this.pos, 50f) / 50f * 1.2f;
                        Vector2 a2 = Custom.DirVec(this.pos, this.arm.oracle.bodyChunks[0].pos);
                        float num7 = Vector2.Distance(this.pos, this.arm.oracle.bodyChunks[0].pos);
                        this.pos += a2 * (num7 - this.totalLength);
                        this.vel += a2 * (num7 - this.totalLength);
                    }
                }

                public OracleArm arm;
                public OracleArm.Joint previous;
                public OracleArm.Joint next;
                public int index;
                public Vector2 pos;
                public Vector2 lastPos;
                public Vector2 vel;
                public float totalLength;
                public float currentInvKinFlip;
            }
        }
    }
}