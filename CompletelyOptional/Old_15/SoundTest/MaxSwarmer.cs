using System.Collections.Generic;
using CoralBrain;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class MaxSwarmer : OracleSwarmer, IOwnProjectedCircles
    {
        public MaxSwarmer(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            this.travelDirection = Custom.RNV();
            this.currentBehavior = new MaxSwarmer.Behavior(this);
            this.color = this.currentBehavior.color;
            this.stuckList = new List<Vector2>();
        }

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);
            if (newRoom.game.SeededRandom(this.abstractPhysicalObject.ID.RandomSeed) < 0.005882353f && newRoom.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SuperStructureProjector) > 0f)
            {
                //newRoom.AddObject(new ProjectedCircle(newRoom, this, 0, 0f));
            }
            this.system = null;
            int num = 0;
            while (num < newRoom.updateList.Count && this.system == null)
            {
                if (newRoom.updateList[num] is CoralNeuronSystem)
                {
                    this.system = (newRoom.updateList[num] as CoralNeuronSystem);
                }
                num++;
            }
            this.stuckList.Clear();
            this.stuckListCounter = 10;
        }

        public override void Update(bool eu)
        {
            if (this.system != null && this.system.Frozen)
            {
                return;
            }
            base.Update(eu);
            if (!this.room.readyForAI || this.room.gravity * this.affectedByGravity > 0.5f)
            {
                return;
            }
            this.direction = this.travelDirection;
            switch (this.mode)
            {
                case MaxSwarmer.MovementMode.Swarm:
                    this.SwarmBehavior();
                    if (this.onlySwarm > 0)
                    {
                        this.onlySwarm--;
                    }
                    else if (this.currentBehavior.suckle && UnityEngine.Random.value < 0.1f && this.system != null && this.system.mycelia.Count > 0)
                    {
                        Mycelium mycelium = this.system.mycelia[UnityEngine.Random.Range(0, this.system.mycelia.Count)];
                        if (Custom.DistLess(base.firstChunk.pos, mycelium.Tip, 400f) && this.room.VisualContact(base.firstChunk.pos, mycelium.Tip))
                        {
                            bool flag = false;
                            int num = 0;
                            while (num < this.otherSwarmers.Count && !flag)
                            {
                                if ((this.otherSwarmers[num] as MaxSwarmer).suckleMyc == mycelium)
                                {
                                    flag = true;
                                }
                                num++;
                            }
                            if (!flag)
                            {
                                this.mode = MaxSwarmer.MovementMode.SuckleMycelia;
                                this.suckleMyc = mycelium;
                                this.attachedToMyc = false;
                            }
                        }
                    }
                    else if (this.room.aimap.getAItile(base.firstChunk.pos).terrainProximity < 7)
                    {
                        if (this.stuckListCounter > 0)
                        {
                            this.stuckListCounter--;
                        }
                        else
                        {
                            this.stuckList.Insert(0, base.firstChunk.pos);
                            if (this.stuckList.Count > 10)
                            {
                                this.stuckList.RemoveAt(this.stuckList.Count - 1);
                            }
                            this.stuckListCounter = 80;
                        }
                        if (UnityEngine.Random.value < 0.025f && this.stuckList.Count > 1 && Custom.DistLess(base.firstChunk.pos, this.stuckList[this.stuckList.Count - 1], 200f))
                        {
                            List<int> list = new List<int>();
                            for (int i = 0; i < this.room.abstractRoom.connections.Length; i++)
                            {
                                if (this.room.aimap.ExitDistanceForCreature(this.room.GetTilePosition(base.firstChunk.pos), i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)) > 0)
                                {
                                    list.Add(i);
                                }
                            }
                            if (list.Count > 0)
                            {
                                this.mode = MaxSwarmer.MovementMode.FollowDijkstra;
                                this.dijkstra = list[UnityEngine.Random.Range(0, list.Count)];
                            }
                        }
                    }
                    break;

                case MaxSwarmer.MovementMode.SuckleMycelia:
                    if (this.suckleMyc == null)
                    {
                        this.mode = MaxSwarmer.MovementMode.Swarm;
                    }
                    else if (this.attachedToMyc)
                    {
                        this.direction = Custom.DirVec(base.firstChunk.pos, this.suckleMyc.Tip);
                        float num2 = Vector2.Distance(base.firstChunk.pos, this.suckleMyc.Tip);
                        base.firstChunk.vel -= (2f - num2) * this.direction * 0.15f;
                        base.firstChunk.pos -= (2f - num2) * this.direction * 0.15f;
                        this.suckleMyc.points[this.suckleMyc.points.GetLength(0) - 1, 0] += (2f - num2) * this.direction * 0.35f;
                        this.suckleMyc.points[this.suckleMyc.points.GetLength(0) - 1, 2] += (2f - num2) * this.direction * 0.35f;
                        this.travelDirection = new Vector2(0f, 0f);
                        if (UnityEngine.Random.value < 0.05f)
                        {
                            this.room.AddObject(new NeuronSpark((base.firstChunk.pos + this.suckleMyc.Tip) / 2f));
                        }
                        if (UnityEngine.Random.value < 0.0125f)
                        {
                            this.suckleMyc = null;
                            this.onlySwarm = UnityEngine.Random.Range(40, 400);
                        }
                    }
                    else
                    {
                        this.travelDirection = Custom.DirVec(base.firstChunk.pos, this.suckleMyc.Tip);
                        if (Custom.DistLess(base.firstChunk.pos, this.suckleMyc.Tip, 5f))
                        {
                            this.attachedToMyc = true;
                        }
                        else if (UnityEngine.Random.value < 0.05f && !this.room.VisualContact(base.firstChunk.pos, this.suckleMyc.Tip))
                        {
                            this.suckleMyc = null;
                        }
                    }
                    this.color = Vector2.Lerp(this.color, this.currentBehavior.color, 0.05f);
                    break;

                case MaxSwarmer.MovementMode.FollowDijkstra:
                    {
                        IntVector2 tilePosition = this.room.GetTilePosition(base.firstChunk.pos);
                        int num3 = -1;
                        int num4 = int.MaxValue;
                        for (int j = 0; j < 4; j++)
                        {
                            if (!this.room.GetTile(tilePosition + Custom.fourDirections[j]).Solid)
                            {
                                int num5 = this.room.aimap.ExitDistanceForCreature(tilePosition + Custom.fourDirections[j], this.dijkstra, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
                                if (num5 > 0 && num5 < num4)
                                {
                                    num3 = j;
                                    num4 = num5;
                                }
                            }
                        }
                        if (num3 > -1)
                        {
                            this.travelDirection += Custom.fourDirections[num3].ToVector2().normalized * 1.4f + Custom.RNV() * UnityEngine.Random.value * 0.5f;
                        }
                        else
                        {
                            this.mode = MaxSwarmer.MovementMode.Swarm;
                        }
                        this.travelDirection.Normalize();
                        int num6 = this.room.aimap.ExitDistanceForCreature(tilePosition, this.dijkstra, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
                        if ((UnityEngine.Random.value < 0.025f && num6 < 34) || num6 < 12 || this.dijkstra < 0 || UnityEngine.Random.value < 0.0025f || (this.room.aimap.getAItile(base.firstChunk.pos).terrainProximity >= 7 && UnityEngine.Random.value < 0.0166666675f))
                        {
                            this.mode = MaxSwarmer.MovementMode.Swarm;
                        }
                        break;
                    }
            }
            base.firstChunk.vel += this.travelDirection * 0.8f * (1f - this.room.gravity * this.affectedByGravity);
            base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 0.2f, 3f, 1f, 0.9f);
            if (this.currentBehavior.Dead)
            {
                Vector2 vector = this.currentBehavior.color;
                this.currentBehavior = new MaxSwarmer.Behavior(this);
                if (UnityEngine.Random.value > 0.25f)
                {
                    this.currentBehavior.color = vector;
                }
            }
            else if (this.currentBehavior.leader == this)
            {
                this.currentBehavior.life -= this.currentBehavior.deathSpeed;
            }
            if (this.abstractPhysicalObject.destroyOnAbstraction && this.grabbedBy.Count > 0)
            {
                this.abstractPhysicalObject.destroyOnAbstraction = false;
            }
        }

        private void SwarmBehavior()
        {
            /*
            Vector2 a = default(Vector2);
            float num = 0f;
            float num2 = this.currentBehavior.torque;
            Vector2 a2 = new Vector2(0f, 0f);
            float num3 = 0f;
            float num4 = this.currentBehavior.revolveSpeed;
            float num5 = 0f;
            int num6 = 0;
            int num7 = -1;
            for (int i = this.listBreakPoint; i < this.otherSwarmers.Count; i++)
            {
                if (this.otherSwarmers[i].slatedForDeletetion)
                {
                    this.otherSwarmers.RemoveAt(i);
                    num7 = i;
                    break;
                }
                if (Custom.DistLess(base.firstChunk.pos, this.otherSwarmers[i].firstChunk.pos, 400f) && (this.otherSwarmers[i] as SSOracleSwarmer).mode != SSOracleSwarmer.MovementMode.SuckleMycelia)
                {
                    float num8 = Mathf.InverseLerp(400f, 0f, Vector2.Distance(base.firstChunk.pos, this.otherSwarmers[i].firstChunk.pos));
                    a += this.otherSwarmers[i].firstChunk.pos * num8;
                    num2 += (this.otherSwarmers[i] as MaxSwarmer).torque * num8;
                    num4 += (this.otherSwarmers[i] as MaxSwarmer).revolveSpeed * num8;
                    num5 += (this.otherSwarmers[i].rotation - Mathf.Floor(this.otherSwarmers[i].rotation)) * num8;
                    num += num8;
                    a2 += (this.otherSwarmers[i] as MaxSwarmer).color * Mathf.InverseLerp(0.9f, 1f, num8);
                    num3 += Mathf.InverseLerp(0.9f, 1f, num8);
                    this.travelDirection += (this.otherSwarmers[i].firstChunk.pos + (this.otherSwarmers[i] as MaxSwarmer).travelDirection * this.currentBehavior.aimInFront * num8 - base.firstChunk.pos).normalized * num8 * 0.01f;
                    this.travelDirection += (base.firstChunk.pos - this.otherSwarmers[i].firstChunk.pos).normalized * Mathf.InverseLerp(this.currentBehavior.idealDistance, 0f, Vector2.Distance(base.firstChunk.pos, this.otherSwarmers[i].firstChunk.pos)) * 0.1f;
                    if (this.currentBehavior.Dominance < (this.otherSwarmers[i] as SSOracleSwarmer).currentBehavior.Dominance * Mathf.Pow(num8, 4f))
                    {
                        this.currentBehavior = (this.otherSwarmers[i] as MaxSwarmer).currentBehavior;
                    }
                    num6++;
                    if (num6 > 30)
                    {
                        num7 = i;
                        break;
                    }
                }
            }
            this.listBreakPoint = num7 + 1;
            this.travelDirection += Custom.RNV() * 0.5f * this.currentBehavior.randomVibrations;
            if (num > 0f)
            {
                this.travelDirection += Custom.PerpendicularVector(base.firstChunk.pos, a / num) * this.torque;
                num5 /= num;
                num5 += Mathf.Floor(this.rotation);
                if (Mathf.Abs(this.rotation - num5) < 0.4f)
                {
                    this.rotation = Mathf.Lerp(this.rotation, num5, 0.05f);
                }
            }
            this.torque = Mathf.Lerp(this.torque, num2 / (1f + num), 0.1f);
            this.revolveSpeed = Mathf.Lerp(this.revolveSpeed, num4 / (1f + num), 0.2f);
            if (num3 > 0f)
            {
                this.color = Vector2.Lerp(this.color, a2 / num3, 0.4f);
            }
            this.color = Vector2.Lerp(this.color, this.currentBehavior.color, 0.05f);
            if (this.room.aimap.getAItile(base.firstChunk.pos).terrainProximity < 5)
            {
                IntVector2 tilePosition = this.room.GetTilePosition(base.firstChunk.pos);
                Vector2 a3 = new Vector2(0f, 0f);
                for (int j = 0; j < 4; j++)
                {
                    if (!this.room.GetTile(tilePosition + Custom.fourDirections[j]).Solid && !this.room.aimap.getAItile(tilePosition + Custom.fourDirections[j]).narrowSpace)
                    {
                        float num9 = 0f;
                        for (int k = 0; k < 4; k++)
                        {
                            num9 += (float)this.room.aimap.getAItile(tilePosition + Custom.fourDirections[j] + Custom.fourDirections[k]).terrainProximity;
                        }
                        a3 += Custom.fourDirections[j].ToVector2() * num9;
                    }
                }
                this.travelDirection = Vector2.Lerp(this.travelDirection, a3.normalized * 2f, 0.5f * Mathf.Pow(Mathf.InverseLerp(5f, 1f, (float)this.room.aimap.getAItile(base.firstChunk.pos).terrainProximity), 0.25f));
            }
            this.travelDirection.Normalize();
            */
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Color color;
            if (!this.dark)
            {
                color = Custom.HSL2RGB((this.color.x >= 0.5f) ? Custom.LerpMap(this.color.x, 0.5f, 1f, 0.6666667f, 0.997222245f) : Custom.LerpMap(this.color.x, 0f, 0.5f, 0.444444448f, 0.6666667f), 1f, 0.5f + 0.5f * this.color.y);
                sLeaser.sprites[4].color = Custom.HSL2RGB((this.color.x >= 0.5f) ? Custom.LerpMap(this.color.x, 0.5f, 1f, 0.6666667f, 0.997222245f) : Custom.LerpMap(this.color.x, 0f, 0.5f, 0.444444448f, 0.6666667f), 1f - this.color.y, Mathf.Lerp(0.8f + 0.2f * Mathf.InverseLerp(0.4f, 0.1f, this.color.x), 0.35f, Mathf.Pow(this.color.y, 2f)));
            }
            else
            {
                color = Custom.HSL2RGB((this.color.x > 0.5f) ? Custom.LerpMap(this.color.x, 0.5f, 1f, 0.6666667f, 0.997222245f) : 0.6666667f, 1f, Mathf.Lerp(0.1f, 0.5f, this.color.y));
                sLeaser.sprites[4].color = Custom.HSL2RGB((this.color.x > 0.5f) ? Custom.LerpMap(this.color.x, 0.5f, 1f, 0.6666667f, 0.997222245f) : 0.6666667f, 1f, Mathf.Lerp(0.75f, 0.9f, this.color.y));
                sLeaser.sprites[0].isVisible = false;
            }
            for (int i = 0; i < 4; i++)
            {
                sLeaser.sprites[i].color = color;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public Vector2 CircleCenter(int index, float timeStacker)
        {
            return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
        }

        public override bool Edible
        {
            get
            {
                return false;
            }
        }

        public CoralNeuronSystem system;
        public Vector2 travelDirection;
        public MaxSwarmer.Behavior currentBehavior;
        private float torque;
        private int listBreakPoint;
        public Vector2 color;
        public List<Vector2> stuckList;
        public int stuckListCounter;
        public MaxSwarmer.MovementMode mode;
        public Mycelium suckleMyc;
        public bool attachedToMyc;
        public int onlySwarm;
        public int dijkstra;
        public bool dark;

        public enum MovementMode
        {
            Swarm,
            SuckleMycelia,
            FollowDijkstra
        }

        public class Behavior
        {
            public Behavior(MaxSwarmer leader)
            {
                this.leader = leader;
                this.dom = UnityEngine.Random.value;
                this.idealDistance = Mathf.Lerp(10f, 300f, UnityEngine.Random.value * UnityEngine.Random.value);
                this.life = 1f;
                this.deathSpeed = 1f / Mathf.Lerp(40f, 220f, UnityEngine.Random.value);
                this.color = new Vector2((float)UnityEngine.Random.Range(0, 3) / 2f, (UnityEngine.Random.value >= 0.75f) ? 1f : 0f);
                this.aimInFront = Mathf.Lerp(40f, 300f, UnityEngine.Random.value);
                this.torque = ((UnityEngine.Random.value >= 0.5f) ? Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) : 0f);
                this.randomVibrations = UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value;
                this.revolveSpeed = ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f) / Mathf.Lerp(15f, 65f, UnityEngine.Random.value);
                this.suckle = (UnityEngine.Random.value < 0.166666672f);
            }

            public float Dominance
            {
                get
                {
                    return (!this.Dead) ? (this.dom * Mathf.Pow(this.life, 0.25f)) : -1f;
                }
            }

            public bool Dead
            {
                get
                {
                    return this.life <= 0f || this.leader.slatedForDeletetion || this.leader.currentBehavior != this;
                }
            }

            private float dom;
            public float idealDistance;
            public float aimInFront;
            public float torque;
            public float randomVibrations;
            public float revolveSpeed;
            public float life;
            public float deathSpeed;
            public MaxSwarmer leader;
            public Vector2 color;
            public bool suckle;
        }
    }
}