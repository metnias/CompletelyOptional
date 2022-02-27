using System;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class MaxGraphics : GraphicsModule
    {
        public MaxGraphics(PhysicalObject ow) : base(ow, false)
        {
            int seed = UnityEngine.Random.seed;
            UnityEngine.Random.seed = 56;
            this.totalSprites = 0;
            this.armJointGraphics = new ArmJointGraphics[this.oracle.arm.joints.Length];
            for (int i = 0; i < this.oracle.arm.joints.Length; i++)
            {
                this.armJointGraphics[i] = new ArmJointGraphics(this, this.oracle.arm.joints[i], this.totalSprites);
                this.totalSprites += this.armJointGraphics[i].totalSprites;
            }

            this.firstUmbilicalSprite = this.totalSprites;
            this.umbCord = new UbilicalCord(this, this.totalSprites);
            this.totalSprites += this.umbCord.totalSprites;

            this.tail = new TailSegment[4];
            this.tail[0] = new TailSegment(this, 6f, 4f, null, 0.85f, 1f, 1f, true);
            this.tail[1] = new TailSegment(this, 4f, 7f, this.tail[0], 0.85f, 1f, 0.5f, true);
            this.tail[2] = new TailSegment(this, 2.5f, 7f, this.tail[1], 0.85f, 1f, 0.5f, true);
            this.tail[3] = new TailSegment(this, 1f, 7f, this.tail[2], 0.85f, 1f, 0.5f, true);
            this.firstTailSprite = this.totalSprites;
            this.totalSprites++;

            this.firstBodyChunkSprite = this.totalSprites;
            this.totalSprites += 2;
            this.firstFootSprite = this.totalSprites;
            this.totalSprites += 4;

            this.halo = new Halo(this, this.totalSprites);
            this.totalSprites += this.halo.totalSprites;
            this.gown = new Gown(this);
            this.robeSprite = this.totalSprites;
            this.totalSprites++;

            this.firstHandSprite = this.totalSprites;
            this.totalSprites += 4;
            //this.head = new GenericBodyPart(this, 5f, 0.5f, 0.995f, this.oracle.firstChunk);
            this.head = new GenericBodyPart(this, 4f, 0.8f, 0.99f, base.owner.bodyChunks[0]);

            this.firstHeadSprite = this.totalSprites;
            this.totalSprites += 10;
            this.fadeSprite = this.totalSprites;
            this.totalSprites++;

            this.killSprite = this.totalSprites;
            this.totalSprites++;

            this.hands = new GenericBodyPart[2];
            for (int j = 0; j < 2; j++)
            {
                this.hands[j] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, this.oracle.firstChunk);
            }
            this.feet = new GenericBodyPart[2];
            for (int k = 0; k < 2; k++)
            {
                this.feet[k] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, this.oracle.firstChunk);
            }

            this.knees = new Vector2[2, 2];
            for (int l = 0; l < 2; l++)
            {
                for (int m = 0; m < 2; m++)
                {
                    this.knees[l, m] = this.oracle.firstChunk.pos;
                }
            }
            this.firstArmBaseSprite = this.totalSprites;
            this.armBase = new ArmBase(this, this.firstArmBaseSprite);
            this.totalSprites += this.armBase.totalSprites;
            this.voiceFreqSamples = new float[64];
            UnityEngine.Random.seed = seed;

            this._faceSprite = this.totalSprites;
            this.totalSprites++;

            this.blink = 10;

            this.mute = false;
            this.objectLooker = new MaxGraphics.MaxObjectLooker(this);

            Debug.Log(this.totalSprites); //original : 394
        }
        public TailSegment[] tail;
        private int firstTailSprite;
        public int TailSprite
        {
            get
            {
                return this.firstTailSprite;
            }
        }
        private int _faceSprite;
        public int FaceSprite
        {
            get
            {
                return _faceSprite;
            }
        }
        public int blink;

        public bool mute;
        public MaxGraphics.MaxObjectLooker objectLooker;

        //public Vector2[,] drawPositions;

        public MaxOracle oracle
        {
            get
            {
                return base.owner as MaxOracle;
            }
        }

        public int FootSprite(int side, int part)
        {
            return this.firstFootSprite + side * 2 + part;
        }

        public int HandSprite(int side, int part)
        {
            return this.firstHandSprite + side * 2 + part;
        }

        public int PhoneSprite(int side, int part)
        {
            if (side == 0)
            {
                return this.firstHeadSprite + part;
            }
            return this.firstHeadSprite + 7 + part;
        }

        public int HeadSprite
        {
            get
            {
                return this.firstHeadSprite + 3;
            }
        }

        public int ChinSprite
        {
            get
            {
                return this.firstHeadSprite + 4;
            }
        }

        public int EyeSprite(int e)
        {
            return this.firstHeadSprite + 5 + e;
        }

        public int MoonThirdEyeSprite
        {
            get
            {
                return this.firstHeadSprite + 11;
            }
        }

        private Vector2 RelativeLookDir(float timeStacker)
        {
            return Custom.RotateAroundOrigo(Vector2.Lerp(this.lastLookDir, this.lookDir, timeStacker), -Custom.AimFromOneVectorToAnother(Vector2.Lerp(this.oracle.bodyChunks[1].lastPos, this.oracle.bodyChunks[1].pos, timeStacker), Vector2.Lerp(this.oracle.firstChunk.lastPos, this.oracle.firstChunk.pos, timeStacker)));
        }

        public override void Update()
        {
            base.Update();

            this.breathe += 1f / Mathf.Lerp(10f, 60f, this.oracle.health);
            this.lastBreatheFac = this.breathFac;
            this.breathFac = Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(this.breathe * 3.14159274f * 2f), 1f, Mathf.Pow(this.oracle.health, 2f));
            if (this.gown != null)
            {
                this.gown.Update();
            }
            if (this.halo != null)
            {
                this.halo.Update();
            }
            this.armBase.Update();

            //this.lastLookDir = this.lookDir;

            //this.lookDir = Vector2.ClampMagnitude(this.oracle.oracleBehavior.lookPoint - this.oracle.firstChunk.pos, 100f) / 100f;
            //this.lookDir = Vector2.ClampMagnitude(this.lookDir + this.randomTalkVector * this.averageVoice * 0.3f, 1f);

            Vector2 b = Custom.DirVec(this.oracle.bodyChunks[1].pos, this.oracle.bodyChunks[0].pos) * 3f;
            this.head.Update();
            //this.head.ConnectToPoint(this.oracle.firstChunk.pos + Custom.DirVec(this.oracle.bodyChunks[1].pos, this.oracle.firstChunk.pos) * 6f, 8f, true, 0f, this.oracle.firstChunk.vel, 0.5f, 0.01f);
            this.head.ConnectToPoint(Vector2.Lerp(this.oracle.bodyChunks[0].pos, this.oracle.bodyChunks[1].pos, 0.2f) + b, 3f, false, 0.2f, base.owner.firstChunk.vel, 0.7f, 0.1f);

            this.head.vel += Custom.DirVec(this.oracle.bodyChunks[1].pos, this.oracle.firstChunk.pos) * this.breathFac;
            this.head.vel += this.lookDir * 0.5f * this.breathFac;

            for (int i = 0; i < 2; i++)
            {
                this.feet[i].Update();
                this.feet[i].ConnectToPoint(this.oracle.bodyChunks[1].pos, 10f, false, 0f, this.oracle.bodyChunks[1].vel, 0.3f, 0.01f);

                this.feet[i].vel += Custom.DirVec(this.oracle.firstChunk.pos, this.oracle.bodyChunks[1].pos) * 0.3f;
                this.feet[i].vel += Custom.PerpendicularVector(Custom.DirVec(this.oracle.firstChunk.pos, this.oracle.bodyChunks[1].pos)) * 0.15f * ((i != 0) ? 1f : -1f);
                this.hands[i].Update();
                this.hands[i].ConnectToPoint(this.oracle.firstChunk.pos, 15f, false, 0f, this.oracle.firstChunk.vel, 0.3f, 0.01f);
                GenericBodyPart genericBodyPart3 = this.hands[i];
                genericBodyPart3.vel.y = genericBodyPart3.vel.y - 0.5f;
                this.hands[i].vel += Custom.DirVec(this.oracle.firstChunk.pos, this.oracle.bodyChunks[1].pos) * 0.3f;
                this.hands[i].vel += Custom.PerpendicularVector(Custom.DirVec(this.oracle.firstChunk.pos, this.oracle.bodyChunks[1].pos)) * 0.3f * ((i != 0) ? 1f : -1f);
                this.knees[i, 1] = this.knees[i, 0];

                this.hands[i].vel += this.randomTalkVector * this.averageVoice * 0.8f;
                /*
                if (i == 0 && (this.oracle.oracleBehavior as MaxOracleBehavior).HandTowardsPlayer())
                {
                    this.hands[0].vel += Custom.DirVec(this.hands[0].pos, this.oracle.oracleBehavior.player.mainBodyChunk.pos) * 3f;
                }*/
                this.knees[i, 0] = (this.feet[i].pos + this.oracle.bodyChunks[1].pos) / 2f + Custom.PerpendicularVector(Custom.DirVec(this.oracle.firstChunk.pos, this.oracle.bodyChunks[1].pos)) * 4f * ((i != 0) ? 1f : -1f);
            }
            for (int j = 0; j < this.armJointGraphics.Length; j++)
            {
                this.armJointGraphics[j].Update();
            }
            if (this.umbCord != null)
            {
                this.umbCord.Update();
            }
            else if (this.discUmbCord != null)
            {
                this.discUmbCord.Update();
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

            this.lastLookDir = this.lookDir;
            if (this.objectLooker.looking)
            {
                this.lookDir = Custom.DirVec(this.head.pos, this.objectLooker.mostInterestingLookPoint);
            }
            else
            {
                this.lookDir *= 0f;
            }

            Vector2 vector2 = base.owner.bodyChunks[0].pos;
            this.tail[0].connectedPoint = new Vector2?(base.owner.bodyChunks[1].pos);
            float num3 = 1f;
            float num9 = 28f;
            Vector2 pos = base.owner.bodyChunks[1].pos;

            for (int k = 0; k < this.tail.Length; k++)
            {
                this.tail[k].Update();
                this.tail[k].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - base.owner.bodyChunks[1].submersion));
                TailSegment tailSegment7 = this.tail[k];
                tailSegment7.vel.y = tailSegment7.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - base.owner.bodyChunks[1].submersion) * base.owner.room.gravity;
                num3 = (num3 * 10f + 1f) / 11f;
                if (!Custom.DistLess(this.tail[k].pos, base.owner.bodyChunks[1].pos, 9f * (float)(k + 1)))
                {
                    this.tail[k].pos = base.owner.bodyChunks[1].pos + Custom.DirVec(base.owner.bodyChunks[1].pos, this.tail[k].pos) * 9f * (float)(k + 1);
                }
                this.tail[k].vel += Custom.DirVec(vector2, this.tail[k].pos) * num9 / Vector2.Distance(vector2, this.tail[k].pos);
                num9 *= 0.5f;
                vector2 = pos;
                pos = this.tail[k].pos;
            }

            /*
            if (this.oracle.oracleBehavior.voice != null && this.oracle.oracleBehavior.voice.currentAudioSource != null && this.oracle.oracleBehavior.voice.currentAudioSource.isPlaying)
            {
                this.oracle.oracleBehavior.voice.currentAudioSource.GetSpectrumData(this.voiceFreqSamples, 0, FFTWindow.BlackmanHarris);
                this.averageVoice = 0f;
                for (int k = 0; k < this.voiceFreqSamples.Length; k++)
                {
                    this.averageVoice += this.voiceFreqSamples[k];
                }
                this.averageVoice /= (float)this.voiceFreqSamples.Length;
                this.averageVoice = Mathf.InverseLerp(0f, 0.00014f, this.averageVoice);
                if (this.averageVoice > 0.7f && UnityEngine.Random.value < this.averageVoice / 14f)
                {
                    this.randomTalkVector = Custom.RNV();
                }
            }
            else
            {
                this.randomTalkVector *= 0.9f;
                if (this.averageVoice > 0f)
                {
                    for (int l = 0; l < this.voiceFreqSamples.Length; l++)
                    {
                        this.voiceFreqSamples[l] = 0f;
                    }
                    this.averageVoice = 0f;
                }
            }*/
            this.lastEyesOpen = this.eyesOpen;
            this.eyesOpen = ((!this.oracle.oracleBehavior.EyesClosed) ? 1f : 0f);

            if (this.lightsource == null)
            {
                this.lightsource = new LightSource(this.oracle.firstChunk.pos, false, Custom.HSL2RGB(0.1f, 1f, 0.5f), this.oracle);
                this.lightsource.affectedByPaletteDarkness = 0f;
                this.oracle.room.AddObject(this.lightsource);
            }
            else
            {
                this.lightsource.setAlpha = new float?((this.oracle.oracleBehavior as MaxOracleBehavior).working);
                this.lightsource.setRad = new float?(400f);
                this.lightsource.setPos = new Vector2?(this.oracle.firstChunk.pos);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[this.totalSprites];
            for (int i = 0; i < base.owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[this.firstBodyChunkSprite + i] = new FSprite("Circle20", true);
                sLeaser.sprites[this.firstBodyChunkSprite + i].scale = base.owner.bodyChunks[i].rad / 10f;
                sLeaser.sprites[this.firstBodyChunkSprite + i].color = new Color(1f, (i != 0) ? 0f : 0.5f, (i != 0) ? 0f : 0.5f);
            }

            for (int j = 0; j < this.armJointGraphics.Length; j++)
            {
                this.armJointGraphics[j].InitiateSprites(sLeaser, rCam);
            }

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
            sLeaser.sprites[this.TailSprite] = triangleMesh;
            sLeaser.sprites[this.TailSprite].color = new Color(1f, 1f, 1f);

            if (this.gown != null)
            {
                this.gown.InitiateSprite(this.robeSprite, sLeaser, rCam);
            }
            if (this.halo != null)
            {
                this.halo.InitiateSprites(sLeaser, rCam);
            }
            this.armBase.InitiateSprites(sLeaser, rCam);

            //sLeaser.sprites[this.HeadSprite] = new FSprite("Circle20", true);
            sLeaser.sprites[this.HeadSprite] = new FSprite("HeadA0", true);
            sLeaser.sprites[this.HeadSprite].anchorY = 0.25f;

            sLeaser.sprites[this.ChinSprite] = new FSprite("Circle20", true);
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[this.EyeSprite(k)] = new FSprite("pixel", true);

                sLeaser.sprites[this.PhoneSprite(k, 0)] = new FSprite("Circle20", true);
                sLeaser.sprites[this.PhoneSprite(k, 1)] = new FSprite("Circle20", true);
                sLeaser.sprites[this.PhoneSprite(k, 2)] = new FSprite("LizardScaleA1", true);
                sLeaser.sprites[this.PhoneSprite(k, 2)].anchorY = 0f;
                sLeaser.sprites[this.PhoneSprite(k, 2)].scaleY = 0.8f;
                sLeaser.sprites[this.PhoneSprite(k, 2)].scaleX = ((k != 0) ? 1f : -1f) * 0.75f;
                sLeaser.sprites[this.HandSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[this.HandSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
                sLeaser.sprites[this.FootSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[this.FootSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
            }

            if (this.umbCord != null)
            {
                this.umbCord.InitiateSprites(sLeaser, rCam);
            }
            else if (this.discUmbCord != null)
            {
                this.discUmbCord.InitiateSprites(sLeaser, rCam);
            }
            //sLeaser.sprites[this.HeadSprite].scaleX = this.head.rad / 9f;
            //sLeaser.sprites[this.HeadSprite].scaleY = this.head.rad / 11f;
            sLeaser.sprites[this.ChinSprite].scale = this.head.rad / 15f;
            sLeaser.sprites[this.fadeSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[this.fadeSprite].scale = 12.5f;
            sLeaser.sprites[this.fadeSprite].color = new Color(0f, 0f, 0f);
            sLeaser.sprites[this.fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[this.fadeSprite].alpha = 0.5f;

            sLeaser.sprites[this.killSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[this.killSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];

            sLeaser.sprites[this.FaceSprite] = new FSprite("FaceA0", true);

            this.AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            sLeaser.RemoveAllSpritesFromContainer();
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Midground");
            }
            for (int i = this.firstArmBaseSprite; i < this.firstArmBaseSprite + this.armBase.totalSprites; i++)
            {
                rCam.ReturnFContainer((i >= this.firstArmBaseSprite + 6 && i != this.firstArmBaseSprite + 8) ? "Shortcuts" : "Midground").AddChild(sLeaser.sprites[i]);
            }
            if (this.halo == null)
            {
                for (int j = 0; j < this.totalSprites; j++) //firstArmBaseSprite
                {
                    newContatiner.AddChild(sLeaser.sprites[j]);
                }
            }
            else
            {
                for (int k = 0; k < this.halo.firstSprite; k++)
                {
                    newContatiner.AddChild(sLeaser.sprites[k]);
                }
                FContainer fcontainer = rCam.ReturnFContainer("BackgroundShortcuts");
                for (int l = this.halo.firstSprite; l < this.halo.firstSprite + this.halo.totalSprites; l++)
                {
                    fcontainer.AddChild(sLeaser.sprites[l]);
                }
                for (int m = this.halo.firstSprite + this.halo.totalSprites; m < this.totalSprites; m++) //this.firstArmBaseSprite
                {
                    if (m != this.fadeSprite && m != this.killSprite)
                    {
                        newContatiner.AddChild(sLeaser.sprites[m]);
                    }
                }
            }
            rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[this.fadeSprite]);
            rCam.ReturnFContainer("Shortcuts").AddChild(sLeaser.sprites[this.killSprite]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(base.owner.firstChunk.lastPos, base.owner.firstChunk.pos, timeStacker);
            Vector2 vector2 = Custom.DirVec(Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker), vector);
            Vector2 a = Custom.PerpendicularVector(vector2);
            Vector2 a2 = Vector2.Lerp(this.lastLookDir, this.lookDir, timeStacker);
            Vector2 vector3 = Vector2.Lerp(this.head.lastPos, this.head.pos, timeStacker);
            for (int i = 0; i < base.owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[this.firstBodyChunkSprite + i].x = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.x, base.owner.bodyChunks[i].pos.x, timeStacker) - camPos.x;
                sLeaser.sprites[this.firstBodyChunkSprite + i].y = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.y, base.owner.bodyChunks[i].pos.y, timeStacker) - camPos.y;
            }
            sLeaser.sprites[this.firstBodyChunkSprite].rotation = Custom.AimFromOneVectorToAnother(vector, vector3) - Mathf.Lerp(14f, 0f, Mathf.Lerp(this.lastBreatheFac, this.breathFac, timeStacker));
            sLeaser.sprites[this.firstBodyChunkSprite + 1].rotation = Custom.VecToDeg(vector2);
            for (int j = 0; j < this.armJointGraphics.Length; j++)
            {
                this.armJointGraphics[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            if ((this.oracle.oracleBehavior as MaxOracleBehavior).killFac > 0f)
            {
                sLeaser.sprites[this.killSprite].isVisible = true;
                //sLeaser.sprites[this.killSprite].x = Mathf.Lerp(this.oracle.oracleBehavior.player.mainBodyChunk.lastPos.x, this.oracle.oracleBehavior.player.mainBodyChunk.pos.x, timeStacker) - camPos.x;
                //sLeaser.sprites[this.killSprite].y = Mathf.Lerp(this.oracle.oracleBehavior.player.mainBodyChunk.lastPos.y, this.oracle.oracleBehavior.player.mainBodyChunk.pos.y, timeStacker) - camPos.y;
                float f = Mathf.Lerp((this.oracle.oracleBehavior as MaxOracleBehavior).lastKillFac, (this.oracle.oracleBehavior as MaxOracleBehavior).killFac, timeStacker);
                sLeaser.sprites[this.killSprite].scale = Mathf.Lerp(200f, 2f, Mathf.Pow(f, 0.5f));
                sLeaser.sprites[this.killSprite].alpha = Mathf.Pow(f, 3f);
            }
            else
            {
                sLeaser.sprites[this.killSprite].isVisible = false;
            }

            sLeaser.sprites[this.fadeSprite].x = vector3.x - camPos.x;
            sLeaser.sprites[this.fadeSprite].y = vector3.y - camPos.y;

            Vector2 vectort4 = (Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker) * 3f + vector) / 4f;
            float dt2 = 6f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 vectort5 = Vector2.Lerp(this.tail[i].lastPos, this.tail[i].pos, timeStacker);
                Vector2 normalized = (vectort5 - vectort4).normalized;
                Vector2 at = Custom.PerpendicularVector(normalized);
                float dt3 = Vector2.Distance(vectort5, vectort4) / 5f;
                if (i == 0)
                {
                    dt3 = 0f;
                }
                (sLeaser.sprites[this.TailSprite] as TriangleMesh).MoveVertice(i * 4, vectort4 - at * dt2 + normalized * dt3 - camPos);
                (sLeaser.sprites[this.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vectort4 + at * dt2 + normalized * dt3 - camPos);
                if (i < 3)
                {
                    (sLeaser.sprites[this.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vectort5 - at * this.tail[i].StretchedRad - normalized * dt3 - camPos);
                    (sLeaser.sprites[this.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vectort5 + at * this.tail[i].StretchedRad - normalized * dt3 - camPos);
                }
                else
                {
                    (sLeaser.sprites[this.TailSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vectort5 - camPos);
                }
                dt2 = this.tail[i].StretchedRad;
                vectort4 = vectort5;
            }

            if (this.gown != null)
            {
                this.gown.DrawSprite(this.robeSprite, sLeaser, rCam, timeStacker, camPos);
            }
            if (this.halo != null)
            {
                this.halo.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            this.armBase.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 vector4 = Custom.DirVec(vector3, vector);
            Vector2 a3 = Custom.PerpendicularVector(vector4);

            float numh3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
            int numh4 = Mathf.RoundToInt(Mathf.Abs(numh3 / 360f * 34f));
            sLeaser.sprites[this.HeadSprite].x = vector3.x - camPos.x;
            sLeaser.sprites[this.HeadSprite].y = vector3.y - camPos.y;
            sLeaser.sprites[this.HeadSprite].rotation = numh3;
            sLeaser.sprites[this.HeadSprite].scaleX = ((numh3 >= 0f) ? 1f : -1f);
            sLeaser.sprites[this.HeadSprite].element = Futile.atlasManager.GetElementWithName("HeadA" + numh4);

            Vector2 vector5 = this.RelativeLookDir(timeStacker);
            Vector2 a4 = Vector2.Lerp(vector3, vector, 0.15f);
            a4 += a3 * vector5.x * 2f;
            sLeaser.sprites[this.ChinSprite].x = a4.x - camPos.x;
            sLeaser.sprites[this.ChinSprite].y = a4.y - camPos.y;
            float num = Mathf.Lerp(this.lastEyesOpen, this.eyesOpen, timeStacker);
            for (int k = 0; k < 2; k++)
            {
                float num2 = (k != 0) ? 1f : -1f;
                Vector2 vector6 = vector3 + a3 * Mathf.Clamp(vector5.x * 3f + 2.5f * num2, -5f, 5f) + vector4 * (1f - vector5.y * 3f);
                //sLeaser.sprites[this.EyeSprite(k)].rotation = Custom.VecToDeg(vector4);
                //sLeaser.sprites[this.EyeSprite(k)].scaleX = 1f + ((k != 0) ? Mathf.InverseLerp(1f, 0.5f, vector5.x) : Mathf.InverseLerp(-1f, -0.5f, vector5.x)) + (1f - num);
                //sLeaser.sprites[this.EyeSprite(k)].scaleY = Mathf.Lerp(1f, 2f, num);
                //sLeaser.sprites[this.EyeSprite(k)].x = vector6.x - camPos.x;
                //sLeaser.sprites[this.EyeSprite(k)].y = vector6.y - camPos.y;
                //sLeaser.sprites[this.EyeSprite(k)].alpha = 0.5f + 0.5f * num;
                int side = (k < 1 != vector5.x < 0f) ? 1 : 0;
                Vector2 a5 = vector3 + a3 * Mathf.Clamp(Mathf.Lerp(7f, 5f, Mathf.Abs(vector5.x)) * num2, -11f, 11f);
                for (int l = 0; l < 2; l++)
                {
                    sLeaser.sprites[this.PhoneSprite(side, l)].rotation = Custom.VecToDeg(vector4);
                    sLeaser.sprites[this.PhoneSprite(side, l)].scaleY = 5.5f * ((l != 0) ? 0.8f : 1f) / 20f;
                    sLeaser.sprites[this.PhoneSprite(side, l)].scaleX = Mathf.Lerp(3.5f, 5f, Mathf.Abs(vector5.x)) * ((l != 0) ? 0.8f : 1f) / 20f;
                    sLeaser.sprites[this.PhoneSprite(side, l)].x = a5.x - camPos.x;
                    sLeaser.sprites[this.PhoneSprite(side, l)].y = a5.y - camPos.y;
                }
                sLeaser.sprites[this.PhoneSprite(side, 2)].x = a5.x - camPos.x;
                sLeaser.sprites[this.PhoneSprite(side, 2)].y = a5.y - camPos.y;
                sLeaser.sprites[this.PhoneSprite(side, 2)].rotation = Custom.AimFromOneVectorToAnother(vector, a5 - vector4 * 40f - a2 * 10f);
                Vector2 vector7 = Vector2.Lerp(this.hands[k].lastPos, this.hands[k].pos, timeStacker);
                Vector2 vector8 = vector + a * 4f * ((k != 1) ? 1f : -1f);

                Vector2 cB = vector7 + Custom.DirVec(vector7, vector8) * 3f + vector2;
                Vector2 cA = vector8 + a * 5f * ((k != 1) ? 1f : -1f);
                sLeaser.sprites[this.HandSprite(k, 0)].x = vector7.x - camPos.x;
                sLeaser.sprites[this.HandSprite(k, 0)].y = vector7.y - camPos.y;
                Vector2 vector9 = vector8 - a * 2f * ((k != 1) ? 1f : -1f);
                float num3 = 4f;
                for (int m = 0; m < 7; m++)
                {
                    float f2 = (float)m / 6f;
                    Vector2 vector10 = Custom.Bezier(vector8, cA, vector7, cB, f2);
                    Vector2 vector11 = Custom.DirVec(vector9, vector10);
                    Vector2 a6 = Custom.PerpendicularVector(vector11) * ((k != 0) ? 1f : -1f);
                    float d = Vector2.Distance(vector9, vector10);
                    (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4, vector10 - vector11 * d * 0.3f - a6 * num3 - camPos);
                    (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 1, vector10 - vector11 * d * 0.3f + a6 * num3 - camPos);
                    (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 2, vector10 - a6 * num3 - camPos);
                    (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).MoveVertice(m * 4 + 3, vector10 + a6 * num3 - camPos);
                    vector9 = vector10;
                }
                vector7 = Vector2.Lerp(this.feet[k].lastPos, this.feet[k].pos, timeStacker);
                vector8 = Vector2.Lerp(this.oracle.bodyChunks[1].lastPos, this.oracle.bodyChunks[1].pos, timeStacker);
                Vector2 to = Vector2.Lerp(this.knees[k, 1], this.knees[k, 0], timeStacker);
                cB = Vector2.Lerp(vector7, to, 0.9f);
                cA = Vector2.Lerp(vector8, to, 0.9f);
                sLeaser.sprites[this.FootSprite(k, 0)].x = vector7.x - camPos.x;
                sLeaser.sprites[this.FootSprite(k, 0)].y = vector7.y - camPos.y;
                vector9 = vector8 - a * 2f * ((k != 1) ? 1f : -1f);
                float num4 = 4f;
                for (int n = 0; n < 7; n++)
                {
                    float f3 = (float)n / 6f;
                    num3 = 2f;
                    Vector2 vector12 = Custom.Bezier(vector8, cA, vector7, cB, f3);
                    Vector2 vector13 = Custom.DirVec(vector9, vector12);
                    Vector2 a7 = Custom.PerpendicularVector(vector13) * ((k != 0) ? 1f : -1f);
                    float d2 = Vector2.Distance(vector9, vector12);
                    (sLeaser.sprites[this.FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4, vector12 - vector13 * d2 * 0.3f - a7 * (num4 + num3) * 0.5f - camPos);
                    (sLeaser.sprites[this.FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 1, vector12 - vector13 * d2 * 0.3f + a7 * (num4 + num3) * 0.5f - camPos);
                    (sLeaser.sprites[this.FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 2, vector12 - a7 * num3 - camPos);
                    (sLeaser.sprites[this.FootSprite(k, 1)] as TriangleMesh).MoveVertice(n * 4 + 3, vector12 + a7 * num3 - camPos);
                    vector9 = vector12;
                    num4 = num3;
                }
            }

            if (this.umbCord != null)
            {
                this.umbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            else if (this.discUmbCord != null)
            {
                this.discUmbCord.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            string dbg = string.Empty;
            for (int t = 0; t < 4; t++)
            {
                dbg += t.ToString("X1") + ":" + (sLeaser.sprites[this.TailSprite] as TriangleMesh).vertices[t] + "/";
            }
            //Debug.Log(dbg);

            Vector2 p = vector3 - vector2;
            float numf3 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector2, vector, 0.5f), vector3);
            Vector2 af2 = Vector2.Lerp(this.lastLookDir, this.lookDir, timeStacker) * 3f;
            p.x *= 1f - af2.magnitude / 3f;
            p = p.normalized;
            sLeaser.sprites[FaceSprite].element = Futile.atlasManager.GetElementWithName("Face" + ((this.blink <= 0) ? "A" : "B") + Mathf.RoundToInt(Mathf.Abs(Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), p) / 22.5f)));
            if (Mathf.Abs(af2.x) < 0.1f)
            {
                sLeaser.sprites[FaceSprite].scaleX = ((numf3 >= 0f) ? 1f : -1f);
            }
            else
            {
                sLeaser.sprites[FaceSprite].scaleX = Mathf.Sign(af2.x);
            }
            sLeaser.sprites[FaceSprite].rotation = 0f;
            sLeaser.sprites[FaceSprite].x = vector3.x + af2.x - camPos.x;
            sLeaser.sprites[FaceSprite].y = vector3.y + af2.y - 2f - camPos.y;

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            this.SLArmBaseColA = new Color(1f, 1f, 1f); //new Color(0.521568656f, 0.521568656f, 0.5137255f);
            this.SLArmHighLightColA = new Color(1f, 1f, 1f); //new Color(0.5686275f, 0.5686275f, 0.549019635f);
            this.SLArmBaseColB = palette.texture.GetPixel(5, 1);
            this.SLArmHighLightColB = palette.texture.GetPixel(5, 2);
            for (int i = 0; i < this.armJointGraphics.Length; i++)
            {
                this.armJointGraphics[i].ApplyPalette(sLeaser, rCam, palette);
            }
            Color color;

            color = new Color(1f, 1f, 1f);

            for (int j = 0; j < base.owner.bodyChunks.Length; j++)
            {
                sLeaser.sprites[this.firstBodyChunkSprite + j].color = color;
            }
            sLeaser.sprites[this.HeadSprite].color = color;
            sLeaser.sprites[this.ChinSprite].color = color;

            sLeaser.sprites[this.TailSprite].color = color;

            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[this.PhoneSprite(k, 0)].color = this.armJointGraphics[0].BaseColor(default(Vector2));
                sLeaser.sprites[this.PhoneSprite(k, 1)].color = this.armJointGraphics[0].HighLightColor(default(Vector2));
                sLeaser.sprites[this.PhoneSprite(k, 2)].color = this.armJointGraphics[0].HighLightColor(default(Vector2));
                sLeaser.sprites[this.HandSprite(k, 0)].color = color;
                sLeaser.sprites[this.EyeSprite(k)].alpha = 0f;
                if (this.gown != null)
                {
                    for (int l = 0; l < 7; l++)
                    {
                        (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = this.gown.Color(0.4f);
                        (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = this.gown.Color(0f);
                        (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = this.gown.Color(0.4f);
                        (sLeaser.sprites[this.HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = this.gown.Color(0f);
                    }
                }
                else
                {
                    sLeaser.sprites[this.HandSprite(k, 1)].color = color;
                }
                sLeaser.sprites[this.FootSprite(k, 0)].color = color;
                sLeaser.sprites[this.FootSprite(k, 1)].color = color;
            }
            if (this.umbCord != null)
            {
                this.umbCord.ApplyPalette(sLeaser, rCam, palette);
                sLeaser.sprites[this.firstUmbilicalSprite].color = palette.blackColor;
            }
            else if (this.discUmbCord != null)
            {
                this.discUmbCord.ApplyPalette(sLeaser, rCam, palette);
            }
            this.armBase.ApplyPalette(sLeaser, rCam, palette);

            sLeaser.sprites[FaceSprite].color = new Color(0.01f, 0f, 0f);
            sLeaser.sprites[FaceSprite].alpha = 1f;
        }

        public int totalSprites;
        public int firstBodyChunkSprite;
        public int firstFootSprite;
        public int firstHandSprite;
        public int firstUmbilicalSprite;
        public int firstArmBaseSprite;

        public ArmJointGraphics[] armJointGraphics;
        public Gown gown;
        public Halo halo;
        public UbilicalCord umbCord;
        public DisconnectedUbilicalCord discUmbCord;
        public ArmBase armBase;
        public int fadeSprite;
        public int killSprite;
        public GenericBodyPart head;
        public GenericBodyPart[] hands;
        public GenericBodyPart[] feet;
        public Vector2[,] knees;
        public int robeSprite;
        public int firstHeadSprite;
        private Vector2 lookDir;
        private Vector2 lastLookDir;
        public float eyesOpen;
        public float lastEyesOpen;
        public float breathe;
        public float[] voiceFreqSamples;
        public float averageVoice;
        public Vector2 randomTalkVector;
        public LightSource lightsource;
        private Color SLArmBaseColA;
        private Color SLArmBaseColB;
        private Color SLArmHighLightColA;
        private Color SLArmHighLightColB;
        private float breathFac;
        private float lastBreatheFac;
        public bool initiated;

        public class MaxObjectLooker
        {
            public MaxObjectLooker(MaxGraphics owner)
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
                        return this.owner.head.pos + this.owner.lookDir * 100f;
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
                if (this.currentMostInteresting != null && this.currentMostInteresting.slatedForDeletetion)
                {
                    this.currentMostInteresting = null;
                }
                float num = this.HowInterestingIsThisObject(this.currentMostInteresting) + ((this.timeLookingAtThis >= 8) ? -0.5f : 0.5f);
                if (this.lookAtPoint != null)
                {
                    num = this.lookAtPointInterest + ((this.timeLookingAtThis >= 8) ? -0.5f : 0.5f);
                }
                foreach (UpdatableAndDeletable updatableAndDeletable in this.owner.oracle.room.updateList)
                {
                    if (updatableAndDeletable != this.owner.oracle && updatableAndDeletable is PhysicalObject && this.HowInterestingIsThisObject(updatableAndDeletable as PhysicalObject) > num)
                    {
                        this.timeLookingAtThis = 0;
                        this.currentMostInteresting = (updatableAndDeletable as PhysicalObject);
                        num = this.HowInterestingIsThisObject(updatableAndDeletable as PhysicalObject);
                    }
                }
                if (num < 0.2f && UnityEngine.Random.value < 0.5f)
                {
                    this.LookAtNothing();
                }
            }

            public void LookAtNothing()
            {
                this.currentMostInteresting = null;
                this.timeLookingAtThis = 0;
            }

            public void LookAtObject(PhysicalObject obj)
            {
                this.currentMostInteresting = obj;
                this.timeLookingAtThis = 0;
            }

            public void LookAtPoint(Vector2 point, float interest)
            {
                this.lookAtPointInterest = interest;
                this.lookAtPoint = new Vector2?(point);
                this.timeLookingAtThis = 0;
            }

            private float HowInterestingIsThisObject(PhysicalObject obj)
            {
                if (obj == null)
                {
                    return 0f;
                }
                float num = 1f;
                if (Custom.DistLess(this.owner.oracle.bodyChunks[0].pos, obj.bodyChunks[0].pos, 400f) && this.owner.oracle.room.VisualContact(this.owner.oracle.bodyChunks[0].pos, obj.bodyChunks[0].pos))
                {
                    num *= Mathf.Lerp(obj.bodyChunks[0].vel.magnitude + 1f, 2f, 0.5f);
                }
                return num / Mathf.Lerp(Mathf.Pow(Vector2.Distance(this.owner.oracle.bodyChunks[0].pos, obj.bodyChunks[0].pos), 1.5f), 1f, 0.995f);
            }

            public MaxGraphics owner;
            public PhysicalObject currentMostInteresting;
            public int timeLookingAtThis;
            private Vector2? lookAtPoint;
            private float lookAtPointInterest;
        }

        public class ArmJointGraphics
        {
            public ArmJointGraphics(MaxGraphics owner, MaxOracle.OracleArm.Joint myJoint, int firstSprite)
            {
                this.owner = owner;
                this.myJoint = myJoint;
                this.firstSprite = firstSprite;
                this.totalSprites = 15;
                this.meshSegs = (int)(myJoint.totalLength / 10f);
                this.smallArmLength = myJoint.totalLength / 4f;
                this.cogRotat = UnityEngine.Random.value;
                this.lastCogRotat = this.cogRotat;
                this.armJointSound = new StaticSoundLoop(SoundID.SS_AI_Arm_Joint_LOOP, myJoint.pos, myJoint.arm.oracle.room, 1f, Custom.LerpMap((float)myJoint.index, 0f, 3f, 0.5f, 1.5f));
            }

            public int CogSprite(int cog)
            {
                return this.firstSprite + cog;
            }

            public int MetalPartSprite(int part)
            {
                return this.firstSprite + 3 + part;
            }

            public int SegmentSprite(int segment, int highLight)
            {
                return this.firstSprite + 3 + 6 + segment * 2 + highLight;
            }

            public int JointSprite(int joint, int part)
            {
                if (joint == 0)
                {
                    return this.firstSprite + 3 + 10 + part;
                }
                return this.firstSprite + 3 + 4 + part;
            }

            public void Update()
            {
                Vector2 pos;
                if (this.myJoint.next != null)
                {
                    pos = this.myJoint.next.pos;
                }
                else
                {
                    pos = this.owner.oracle.bodyChunks[1].pos;
                }
                float num = Vector2.Distance(this.myJoint.pos, pos);
                float num2 = (this.lastAC - num) / this.myJoint.totalLength;
                this.lastCogRotat = this.cogRotat;
                this.cogRotat += num2 * ((this.myJoint.index % 2 != 0) ? 1f : -1f) * 2f;
                this.lastAC = num;
                if (this.owner.mute)
                {
                    this.armJointSound.volume = 0f;
                }
                else
                {
                    this.armJointSound.Update();
                    this.armJointSound.volume = Mathf.InverseLerp(0.0001f, 0.0003f, Mathf.Abs(num2)) * Custom.LerpMap((float)this.myJoint.index, 0f, 3f, 1f, 0.15f, 0.5f);
                    this.armJointSound.pitch = Mathf.Clamp(1f - num2 * 100f, 0.5f, 1.5f) * Custom.LerpMap((float)this.myJoint.index, 0f, 3f, 0.5f, 1.5f);
                    this.armJointSound.pos = this.myJoint.pos;
                }
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[this.MetalPartSprite(0)] = new FSprite("MirosLegSmallPart", true);
                sLeaser.sprites[this.MetalPartSprite(0)].anchorY = 1f;
                sLeaser.sprites[this.MetalPartSprite(1)] = new FSprite("pixel", true);
                sLeaser.sprites[this.MetalPartSprite(2)] = new FSprite("pixel", true);
                sLeaser.sprites[this.MetalPartSprite(3)] = new FSprite("deerEyeB", true);
                sLeaser.sprites[this.MetalPartSprite(0)].scaleX = ((this.myJoint.index != 0) ? ((this.myJoint.index != 1) ? 0.8f : 1f) : 1.5f) * ((this.myJoint.index % 2 != 0) ? 1f : -1f);
                sLeaser.sprites[this.MetalPartSprite(0)].scaleY = ((this.myJoint.index != 0) ? ((this.myJoint.index != 1) ? 0.5f : 1f) : 2f);
                sLeaser.sprites[this.MetalPartSprite(1)].scaleX = ((this.myJoint.index != 0) ? (((double)this.myJoint.index != 1.5) ? 1f : 1f) : 2f);
                sLeaser.sprites[this.MetalPartSprite(2)].scaleX = ((this.myJoint.index != 0) ? (((double)this.myJoint.index != 1.5) ? 1f : 1f) : 2f);
                sLeaser.sprites[this.MetalPartSprite(2)].anchorY = 0f;
                sLeaser.sprites[this.MetalPartSprite(1)].anchorY = 0f;
                sLeaser.sprites[this.MetalPartSprite(1)].scaleY = this.smallArmLength;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[this.SegmentSprite(i, j)] = TriangleMesh.MakeLongMesh(this.meshSegs, false, false);
                        sLeaser.sprites[this.JointSprite(i, 0)] = new FSprite("Circle20", true);
                        sLeaser.sprites[this.JointSprite(i, 1)] = new FSprite("deerEyeB", true);
                    }
                }
                sLeaser.sprites[this.JointSprite(0, 0)].scale = (7f - (float)this.myJoint.index * 0.5f) / 10f;
                sLeaser.sprites[this.JointSprite(1, 0)].scale = (6f - (float)this.myJoint.index * 1.5f) / 10f;
                for (int k = 0; k < 3; k++)
                {
                    sLeaser.sprites[this.CogSprite(k)] = new FSprite("pixel", true);
                    sLeaser.sprites[this.CogSprite(k)].scaleY = 18f - (float)this.myJoint.index * 2f;
                    sLeaser.sprites[this.CogSprite(k)].scaleX = 5f - (float)this.myJoint.index * 0.5f;
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = Vector2.Lerp(this.myJoint.lastPos, this.myJoint.pos, timeStacker);
                Vector2 vector2;
                if (this.myJoint.next != null)
                {
                    vector2 = Vector2.Lerp(this.myJoint.next.lastPos, this.myJoint.next.pos, timeStacker);
                }
                else
                {
                    vector2 = Vector2.Lerp(this.owner.oracle.bodyChunks[1].lastPos, this.owner.oracle.bodyChunks[1].pos, timeStacker);
                }
                Vector2 vector3 = this.myJoint.ElbowPos(timeStacker, vector2);
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[this.JointSprite(0, i)].x = vector.x + Custom.DirVec(vector, vector3).x * ((this.myJoint.index != 0) ? 2f : 12f) - camPos.x;
                    sLeaser.sprites[this.JointSprite(0, i)].y = vector.y + Custom.DirVec(vector, vector3).y * ((this.myJoint.index != 0) ? 2f : 12f) - camPos.y;
                    sLeaser.sprites[this.JointSprite(1, i)].x = vector3.x - camPos.x;
                    sLeaser.sprites[this.JointSprite(1, i)].y = vector3.y - camPos.y;
                }
                sLeaser.sprites[this.MetalPartSprite(0)].x = vector3.x - camPos.x;
                sLeaser.sprites[this.MetalPartSprite(0)].y = vector3.y - camPos.y;
                sLeaser.sprites[this.MetalPartSprite(1)].x = vector3.x - camPos.x;
                sLeaser.sprites[this.MetalPartSprite(1)].y = vector3.y - camPos.y;
                sLeaser.sprites[this.MetalPartSprite(0)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(vector, vector2, 0.2f), vector3);
                sLeaser.sprites[this.MetalPartSprite(1)].rotation = Custom.AimFromOneVectorToAnother(vector3, Vector2.Lerp(vector, vector2, 0.8f));
                Vector2 vector4 = vector3 + Custom.DirVec(vector3, Vector2.Lerp(vector, vector2, 0.8f)) * this.smallArmLength;
                sLeaser.sprites[this.MetalPartSprite(2)].x = vector2.x - camPos.x;
                sLeaser.sprites[this.MetalPartSprite(2)].y = vector2.y - camPos.y;
                sLeaser.sprites[this.MetalPartSprite(2)].rotation = Custom.AimFromOneVectorToAnother(vector2, vector4);
                sLeaser.sprites[this.MetalPartSprite(2)].scaleY = Vector2.Distance(vector4, vector2) + 10f;
                sLeaser.sprites[this.MetalPartSprite(3)].x = vector4.x - camPos.x;
                sLeaser.sprites[this.MetalPartSprite(3)].y = vector4.y - camPos.y;
                float num = Mathf.Lerp(this.lastCogRotat, this.cogRotat, timeStacker);
                for (int j = 0; j < 3; j++)
                {
                    sLeaser.sprites[this.CogSprite(j)].x = vector.x + Custom.DirVec(vector, vector3).x * ((this.myJoint.index != 0) ? 2f : 12f) - camPos.x;
                    sLeaser.sprites[this.CogSprite(j)].y = vector.y + Custom.DirVec(vector, vector3).y * ((this.myJoint.index != 0) ? 2f : 12f) - camPos.y;
                    sLeaser.sprites[this.CogSprite(j)].rotation = Custom.AimFromOneVectorToAnother(vector, vector3) + 60f * (float)j + num * 360f;
                }
                Vector2 vector5 = Custom.DirVec(vector, vector3);
                Vector2 vector6 = Custom.DirVec(vector3, vector2);
                Vector2 vector7 = Vector3.Slerp(vector5, vector6, 0.5f);
                vector += vector5 * Mathf.Lerp(this.myJoint.totalLength * 0.05f, 4f, 0.5f);
                vector2 -= vector6 * Mathf.Lerp(this.myJoint.totalLength * 0.025f, 4f, 0.5f);
                Vector2 cA = vector + vector5 * Vector2.Distance(vector, vector3) * 0.2f;
                Vector2 cB = vector3 - vector7 * Vector2.Distance(vector, vector3) * 0.2f;
                Vector2 cA2 = vector3 + vector7 * Vector2.Distance(vector3, vector2) * 0.2f;
                Vector2 cB2 = vector2 - vector6 * Vector2.Distance(vector3, vector2) * 0.2f;
                for (int k = 0; k < 2; k++)
                {
                    Vector2 a = (k != 0) ? vector3 : vector;
                    Vector2 v = (k != 0) ? vector7 : vector5;
                    float num2 = 0.5f;
                    float num3 = 5f;
                    if (this.myJoint.index == 0)
                    {
                        num3 = 7f;
                    }
                    else if (this.myJoint.index == 2)
                    {
                        num3 = 4f;
                    }
                    else if (this.myJoint.index == 3)
                    {
                        num3 = 3f;
                    }
                    Vector2 vector8 = (k != 0) ? Custom.Bezier(vector3, cA2, vector2, cB2, 0.5f) : Custom.Bezier(vector, cA, vector3, cB, 0.5f);
                    for (int l = 1; l <= this.meshSegs; l++)
                    {
                        float num4 = (float)l / (float)(this.meshSegs - 1);
                        float num5 = 0.6f + 0.5f * (1f - Mathf.Sin(num4 * 3.14159274f)) + 0.3f * Mathf.Max(Mathf.Sin(Mathf.InverseLerp(0f, 0.3f, num4) * 3.14159274f), Mathf.Sin(Mathf.InverseLerp(0.7f, 1f, num4) * 3.14159274f));
                        if (num4 == 1f)
                        {
                            num5 = 0.5f;
                        }
                        Vector2 vector9 = (k != 0) ? Custom.Bezier(vector3, cA2, vector2, cB2, num4) : Custom.Bezier(vector, cA, vector3, cB, num4);
                        Vector2 vector10;
                        if (num4 < 1f)
                        {
                            if (k == 0)
                            {
                                vector10 = Custom.DirVec(vector9, Custom.Bezier(vector, cA, vector3, cB, (float)(l + 1) / (float)(this.meshSegs - 1)));
                            }
                            else
                            {
                                vector10 = Custom.DirVec(vector9, Custom.Bezier(vector3, cA2, vector2, cB2, (float)(l + 1) / (float)(this.meshSegs - 1)));
                            }
                        }
                        else
                        {
                            vector10 = ((k != 0) ? vector6 : vector7);
                        }
                        float num6 = 0f;
                        if ((k == 0 && num4 > 0.75f) || (k == 1 && num4 < 0.25f))
                        {
                            num5 *= 0.5f;
                            num6 = num5 * num3 * ((this.myJoint.index % 2 != 0) ? -1f : 1f);
                        }
                        (sLeaser.sprites[this.SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 4, (a + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 + num3 * (num5 + num2) * 0.5f) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 3, (a + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 - num3 * (num5 + num2) * 0.5f) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 2, vector9 + Custom.PerpendicularVector(vector10) * (num6 + num3 * num5) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 0)] as TriangleMesh).MoveVertice(l * 4 - 1, vector9 + Custom.PerpendicularVector(vector10) * (num6 - num3 * num5) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 4, (a + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 + num3 * (num5 + num2) * 0.5f * 0.5f) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 3, (a + vector9) / 2f + Custom.PerpendicularVector(v) * (num6 - num3 * (num5 + num2) * 0.5f * 0.5f) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 2, vector9 + Custom.PerpendicularVector(vector10) * (num6 + num3 * num5 * 0.5f) - camPos);
                        (sLeaser.sprites[this.SegmentSprite(k, 1)] as TriangleMesh).MoveVertice(l * 4 - 1, vector9 + Custom.PerpendicularVector(vector10) * (num6 - num3 * num5 * 0.5f) - camPos);
                        a = vector9;
                        v = vector10;
                        num2 = num5;
                    }
                }
            }

            public Color BaseColor(Vector2 ps)
            {
                Color a = Custom.HSL2RGB(0.725f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.12f - 0.10f * this.owner.oracle.room.Darkness(this.myJoint.pos), Mathf.Pow(1f, 0.45f)));
                //Color a = Custom.HSL2RGB(0.025f, Mathf.Lerp(0.4f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.05f, 0.7f - 0.5f * this.owner.oracle.room.Darkness(this.myJoint.pos), Mathf.Pow(1f, 0.45f)));
                return Color.Lerp(a, new Color(0f, 0f, 0.1f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.5f);
            }

            public Color HighLightColor(Vector2 ps)
            {
                Color a = Custom.HSL2RGB(0.725f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.30f - 0.25f * this.owner.oracle.room.Darkness(this.myJoint.pos), Mathf.Pow(1f, 0.25f)));
                //Color a = Custom.HSL2RGB(0.025f, Mathf.Lerp(0.5f, 0.1f, Mathf.Pow(1f, 0.5f)), Mathf.Lerp(0.15f, 0.85f - 0.65f * this.owner.oracle.room.Darkness(this.myJoint.pos), Mathf.Pow(1f, 0.45f)));
                return Color.Lerp(a, new Color(0f, 0f, 0.15f), Mathf.Pow(Mathf.InverseLerp(0.45f, -0.05f, 1f), 0.9f) * 0.4f);
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                this.metalColor = Color.Lerp(palette.blackColor, palette.texture.GetPixel(5, 5), 0.12f);

                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[this.SegmentSprite(i, 0)].color = this.BaseColor(default(Vector2));
                    sLeaser.sprites[this.SegmentSprite(i, 1)].color = this.HighLightColor(default(Vector2));
                    sLeaser.sprites[this.JointSprite(i, 0)].color = this.metalColor;
                    sLeaser.sprites[this.JointSprite(i, 1)].color = Color.Lerp(this.metalColor, this.HighLightColor(default(Vector2)), 0.5f);
                }
                for (int j = 0; j < 3; j++)
                {
                    sLeaser.sprites[this.CogSprite(j)].color = this.metalColor;
                }
                for (int k = 0; k < 4; k++)
                {
                    sLeaser.sprites[this.MetalPartSprite(k)].color = this.metalColor;
                }
            }

            private MaxGraphics owner;
            public MaxOracle.OracleArm.Joint myJoint;
            public int firstSprite;
            public int totalSprites;
            public int meshSegs;
            private float smallArmLength;
            private float cogRotat;
            private float lastCogRotat;
            private float lastAC;
            public Color metalColor;
            public StaticSoundLoop armJointSound;
        }

        public class Gown
        {
            public Gown(MaxGraphics owner)
            {
                this.owner = owner;
                this.clothPoints = new Vector2[this.divs, this.divs, 3];
            }

            public void Update()
            {
                Vector2 pos = this.owner.oracle.firstChunk.pos;
                Vector2 vector = Custom.DirVec(this.owner.oracle.bodyChunks[1].pos, this.owner.oracle.firstChunk.pos);
                Vector2 perp = Custom.PerpendicularVector(vector);
                for (int i = 0; i < this.divs; i++)
                {
                    for (int j = 0; j < this.divs; j++)
                    {
                        float num = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)i);
                        float t = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)j);
                        this.clothPoints[i, j, 1] = this.clothPoints[i, j, 0];
                        this.clothPoints[i, j, 0] += this.clothPoints[i, j, 2];
                        this.clothPoints[i, j, 2] *= 0.999f;
                        this.clothPoints[i, j, 2].y -= 0.9f * this.owner.owner.room.gravity;
                        Vector2 vector2 = this.IdealPosForPoint(i, j, pos, vector, perp);
                        this.clothPoints[i, j, 2] = new Vector3(this.clothPoints[i, j, 2].x, this.clothPoints[i, j, 2].y, 0f) + Vector3.Slerp(-vector, Custom.DirVec(this.owner.oracle.bodyChunks[1].pos, vector2), t) * 0.02f;
                        float num2 = Vector2.Distance(this.clothPoints[i, j, 0], vector2);
                        float num3 = Mathf.Lerp(0f, 9f, t);
                        Vector2 a = Custom.DirVec(this.clothPoints[i, j, 0], vector2);
                        if (num2 > num3)
                        {
                            this.clothPoints[i, j, 0] -= (num3 - num2) * a;
                            this.clothPoints[i, j, 2] -= (num3 - num2) * a;
                        }
                        for (int k = 0; k < 4; k++)
                        {
                            IntVector2 intVector = new IntVector2(i, j) + Custom.fourDirections[k];
                            if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < this.divs && intVector.y < this.divs)
                            {
                                num2 = Vector2.Distance(this.clothPoints[i, j, 0], this.clothPoints[intVector.x, intVector.y, 0]);
                                a = Custom.DirVec(this.clothPoints[i, j, 0], this.clothPoints[intVector.x, intVector.y, 0]);
                                float num4 = Vector2.Distance(vector2, this.IdealPosForPoint(intVector.x, intVector.y, pos, vector, perp));
                                this.clothPoints[i, j, 2] -= (num4 - num2) * a * 0.05f;
                                this.clothPoints[intVector.x, intVector.y, 2] += (num4 - num2) * a * 0.05f;
                            }
                        }
                    }
                }
            }

            private Vector2 IdealPosForPoint(int x, int y, Vector2 bodyPos, Vector2 dir, Vector2 perp)
            {
                float num = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)x);
                float t = Mathf.InverseLerp(0f, (float)(this.divs - 1), (float)y);
                Vector2 a = bodyPos + Mathf.Lerp(-1f, 1f, num) * perp * Mathf.Lerp(5f, 11f, t);
                return a + dir * Mathf.Lerp(5f, -18f, t) * (1f + Mathf.Sin(3.14159274f * num) * 0.35f * Mathf.Lerp(-1f, 1f, t));
            }

            public Color Color(float f)
            {
                return Custom.HSL2RGB(Mathf.Lerp(0.65f, 0.80f, Mathf.Pow(f, 2f)), Mathf.Lerp(0.9f, 0.6f, f), Mathf.Lerp(0.6f, 0.1f, Mathf.Pow(f, 2f)));
                //return Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.02f, Mathf.Pow(f, 2f)), Mathf.Lerp(1f, 0.8f, f), 0.5f);
            }

            public void InitiateSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("Futile_White", this.divs - 1);
                for (int i = 0; i < this.divs; i++)
                {
                    for (int j = 0; j < this.divs; j++)
                    {
                        (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j * this.divs + i] = this.Color((float)i / (float)(this.divs - 1));
                    }
                }
            }

            public void DrawSprite(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                for (int i = 0; i < this.divs; i++)
                {
                    for (int j = 0; j < this.divs; j++)
                    {
                        (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * this.divs + j, Vector2.Lerp(this.clothPoints[i, j, 1], this.clothPoints[i, j, 0], timeStacker) - camPos);
                    }
                }
            }

            private MaxGraphics owner;
            private int divs = 11;
            public Vector2[,,] clothPoints;
        }

        public class Halo
        { // Work with Music
            public Halo(MaxGraphics owner, int firstSprite)
            {
                this.owner = owner;
                this.firstSprite = firstSprite;
                this.totalSprites = 2;
                this.connections = new Halo.Connection[20];
                this.totalSprites += this.connections.Length;
                for (int i = 0; i < this.connections.Length; i++)
                {
                    this.connections[i] = new Halo.Connection(this, new Vector2(owner.owner.room.PixelWidth / 2f, owner.owner.room.PixelHeight / 2f) + Custom.RNV() * Mathf.Lerp(300f, 500f, UnityEngine.Random.value));
                }
                this.connectionsFireChance = Mathf.Pow(UnityEngine.Random.value, 3f);
                this.firstBitSprite = firstSprite + this.totalSprites;
                this.bits = new Halo.MemoryBit[3][];
                this.bits[0] = new Halo.MemoryBit[10];
                this.bits[1] = new Halo.MemoryBit[30];
                this.bits[2] = new Halo.MemoryBit[60];
                for (int j = 0; j < this.bits.Length; j++)
                {
                    for (int k = 0; k < this.bits[j].Length; k++)
                    {
                        this.bits[j][k] = new Halo.MemoryBit(this, new IntVector2(j, k));
                    }
                }
                this.totalSprites += 100;
                this.ringRotations = new float[10, 5];
                this.expand = 1f;
                this.getToExpand = 1f;
            }

            public void Update()
            {
                for (int i = 0; i < this.connections.Length; i++)
                {
                    this.connections[i].lastLightUp = this.connections[i].lightUp;
                    this.connections[i].lightUp *= 0.9f;
                    if (this.owner.mute)
                    {
                        if (UnityEngine.Random.value < this.connectionsFireChance / 120f)
                        {
                            this.connections[i].lightUp = 1f;
                        }
                    }
                    else
                    {
                        if (UnityEngine.Random.value < this.connectionsFireChance / 40f)
                        {
                            this.connections[i].lightUp = 1f;
                            this.owner.owner.room.PlaySound(SoundID.SS_AI_Halo_Connection_Light_Up, 0f, 1f, 1f);
                        }
                    }
                }
                if (UnityEngine.Random.value < 0.0166666675f)
                {
                    this.connectionsFireChance = Mathf.Pow(UnityEngine.Random.value, 3f);
                }
                for (int j = 0; j < this.ringRotations.GetLength(0); j++)
                {
                    this.ringRotations[j, 1] = this.ringRotations[j, 0];
                    if (this.ringRotations[j, 0] != this.ringRotations[j, 3])
                    {
                        this.ringRotations[j, 4] += 1f / Mathf.Lerp(20f, Mathf.Abs(this.ringRotations[j, 2] - this.ringRotations[j, 3]), 0.5f);
                        this.ringRotations[j, 0] = Mathf.Lerp(this.ringRotations[j, 2], this.ringRotations[j, 3], Custom.SCurve(this.ringRotations[j, 4], 0.5f));
                        if (this.ringRotations[j, 4] > 1f)
                        {
                            this.ringRotations[j, 4] = 0f;
                            this.ringRotations[j, 2] = this.ringRotations[j, 3];
                            this.ringRotations[j, 0] = this.ringRotations[j, 3];
                        }
                    }
                    else if (UnityEngine.Random.value < 0.0333333351f)
                    {
                        this.ringRotations[j, 3] = this.ringRotations[j, 0] + ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f) * Mathf.Lerp(15f, 150f, UnityEngine.Random.value);
                    }
                }
                for (int k = 0; k < this.bits.Length; k++)
                {
                    for (int l = 0; l < this.bits[k].Length; l++)
                    {
                        this.bits[k][l].Update();
                    }
                }
                if (UnityEngine.Random.value < 0.0166666675f)
                {
                    int num = UnityEngine.Random.Range(0, this.bits.Length);
                    for (int m = 0; m < this.bits[num].Length; m++)
                    {
                        this.bits[num][m].SetToMax();
                    }
                }
                this.lastExpand = this.expand;
                this.lastPush = this.push;
                this.lastWhite = this.white;
                this.expand = Custom.LerpAndTick(this.expand, this.getToExpand, 0.05f, 0.0125f);
                this.push = Custom.LerpAndTick(this.push, this.getToPush, 0.02f, 0.025f);
                this.white = Custom.LerpAndTick(this.white, this.getToWhite, 0.07f, 0.0227272734f);
                bool flag = false;
                if (UnityEngine.Random.value < 0.00625f)
                {
                    if (UnityEngine.Random.value < 0.125f)
                    {
                        flag = (this.getToWhite < 1f);
                        this.getToWhite = 1f;
                    }
                    else
                    {
                        this.getToWhite = 0f;
                    }
                }
                if (UnityEngine.Random.value < 0.00625f || flag)
                {
                    this.getToExpand = ((UnityEngine.Random.value >= 0.5f || flag) ? Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f)) : 1f);
                }
                if (UnityEngine.Random.value < 0.00625f || flag)
                {
                    this.getToPush = ((UnityEngine.Random.value >= 0.5f || flag) ? ((float)(-1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6)))) : 0f);
                }
            }

            public void ChangeAllRadi()
            {
                this.getToExpand = Mathf.Lerp(0.8f, 2f, Mathf.Pow(UnityEngine.Random.value, 1.5f));
                this.getToPush = (float)(-1 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 6)));
            }

            private float Radius(float ring, float timeStacker)
            {
                return (3f + ring + Mathf.Lerp(this.lastPush, this.push, timeStacker) - 0.5f * this.owner.averageVoice) * Mathf.Lerp(this.lastExpand, this.expand, timeStacker) * 10f;
            }

            private float Rotation(int ring, float timeStacker)
            {
                return Mathf.Lerp(this.ringRotations[ring, 1], this.ringRotations[ring, 0], timeStacker);
            }

            public Vector2 Center(float timeStacker)
            {
                Vector2 vector = Vector2.Lerp(this.owner.head.lastPos, this.owner.head.pos, timeStacker);
                return vector + Custom.DirVec(Vector2.Lerp(this.owner.owner.firstChunk.lastPos, this.owner.owner.firstChunk.pos, timeStacker), vector) * 20f;
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[this.firstSprite + i] = new FSprite("Futile_White", true);
                    sLeaser.sprites[this.firstSprite + i].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
                    sLeaser.sprites[this.firstSprite + i].color = new Color(0f, 0f, 0f);
                }
                for (int j = 0; j < this.connections.Length; j++)
                {
                    sLeaser.sprites[this.firstSprite + 2 + j] = TriangleMesh.MakeLongMesh(20, false, false);
                    sLeaser.sprites[this.firstSprite + 2 + j].color = new Color(0f, 0f, 0f);
                }
                for (int k = 0; k < 100; k++)
                {
                    sLeaser.sprites[this.firstBitSprite + k] = new FSprite("pixel", true);
                    sLeaser.sprites[this.firstBitSprite + k].scaleX = 4f;
                    sLeaser.sprites[this.firstBitSprite + k].color = new Color(0f, 0f, 0f);
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = this.Center(timeStacker);
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[this.firstSprite + i].x = vector.x - camPos.x;
                    sLeaser.sprites[this.firstSprite + i].y = vector.y - camPos.y;
                    sLeaser.sprites[this.firstSprite + i].scale = this.Radius((float)i, timeStacker) / 8f;
                }
                sLeaser.sprites[this.firstSprite].alpha = Mathf.Lerp(3f / this.Radius(0f, timeStacker), 1f, Mathf.Lerp(this.lastWhite, this.white, timeStacker));
                sLeaser.sprites[this.firstSprite + 1].alpha = 3f / this.Radius(1f, timeStacker);
                for (int j = 0; j < this.connections.Length; j++)
                {
                    if (this.connections[j].lastLightUp > 0.05f || this.connections[j].lightUp > 0.05f)
                    {
                        Vector2 vector2 = this.connections[j].stuckAt;
                        float d = 2f * Mathf.Lerp(this.connections[j].lastLightUp, this.connections[j].lightUp, timeStacker);
                        for (int k = 0; k < 20; k++)
                        {
                            float f = (float)k / 19f;
                            Vector2 a = Custom.DirVec(vector, this.connections[j].stuckAt);
                            Vector2 vector3 = Custom.Bezier(this.connections[j].stuckAt, this.connections[j].handle, vector + a * this.Radius(2f, timeStacker), vector + a * 400f, f);
                            Vector2 vector4 = Custom.DirVec(vector2, vector3);
                            Vector2 a2 = Custom.PerpendicularVector(vector4);
                            float d2 = Vector2.Distance(vector2, vector3);
                            (sLeaser.sprites[this.firstSprite + 2 + j] as TriangleMesh).MoveVertice(k * 4, vector3 - vector4 * d2 * 0.3f - a2 * d - camPos);
                            (sLeaser.sprites[this.firstSprite + 2 + j] as TriangleMesh).MoveVertice(k * 4 + 1, vector3 - vector4 * d2 * 0.3f + a2 * d - camPos);
                            (sLeaser.sprites[this.firstSprite + 2 + j] as TriangleMesh).MoveVertice(k * 4 + 2, vector3 - a2 * d - camPos);
                            (sLeaser.sprites[this.firstSprite + 2 + j] as TriangleMesh).MoveVertice(k * 4 + 3, vector3 + a2 * d - camPos);
                            vector2 = vector3;
                        }
                    }
                }
                int num = this.firstBitSprite;
                for (int l = 0; l < this.bits.Length; l++)
                {
                    for (int m = 0; m < this.bits[l].Length; m++)
                    {
                        float num2 = (float)m / (float)this.bits[l].Length * 360f + this.Rotation(l, timeStacker);
                        Vector2 vector5 = vector + Custom.DegToVec(num2) * this.Radius((float)l + 0.5f, timeStacker);
                        sLeaser.sprites[num].scaleY = 8f * this.bits[l][m].Fill(timeStacker);
                        sLeaser.sprites[num].x = vector5.x - camPos.x;
                        sLeaser.sprites[num].y = vector5.y - camPos.y;
                        sLeaser.sprites[num].rotation = num2;
                        num++;
                    }
                }
            }

            private MaxGraphics owner;
            public int firstSprite;
            public int totalSprites;
            private int firstBitSprite;
            public Halo.Connection[] connections;
            public float connectionsFireChance;
            public Halo.MemoryBit[][] bits;
            public float[,] ringRotations;
            public float expand;
            public float lastExpand;
            public float getToExpand;
            public float push;
            public float lastPush;
            public float getToPush;
            public float white;
            public float lastWhite;
            public float getToWhite;

            public class Connection
            {
                public Connection(Halo halo, Vector2 stuckAt)
                {
                    this.halo = halo;
                    Vector2 to = stuckAt;
                    to.x = Mathf.Clamp(to.x, halo.owner.oracle.arm.cornerPositions[0].x, halo.owner.oracle.arm.cornerPositions[1].x);
                    to.y = Mathf.Clamp(to.y, halo.owner.oracle.arm.cornerPositions[2].y, halo.owner.oracle.arm.cornerPositions[1].y);
                    this.stuckAt = Vector2.Lerp(stuckAt, to, 0.5f);
                    this.handle = stuckAt + Custom.RNV() * Mathf.Lerp(400f, 700f, UnityEngine.Random.value);
                }

                public Halo halo;
                public Vector2 stuckAt;
                public Vector2 handle;
                public float lightUp;
                public float lastLightUp;
            }

            public class MemoryBit
            {
                public MemoryBit(Halo halo, IntVector2 position)
                {
                    this.halo = halo;
                    this.position = position;
                    this.filled = UnityEngine.Random.value;
                    this.lastFilled = this.filled;
                    this.getToFilled = this.filled;
                    this.fillSpeed = 0f;
                }

                public float Fill(float timeStacker)
                {
                    if (this.blinkCounter % 4 > 1 && this.filled == this.getToFilled)
                    {
                        return 0f;
                    }
                    return Mathf.Lerp(this.lastFilled, this.filled, timeStacker);
                }

                public void SetToMax()
                {
                    this.getToFilled = 1f;
                    this.fillSpeed = Mathf.Lerp(this.fillSpeed, 0.25f, 0.25f);
                    this.blinkCounter = 20;
                }

                public void Update()
                {
                    this.lastFilled = this.filled;
                    if (this.filled != this.getToFilled)
                    {
                        this.filled = Custom.LerpAndTick(this.filled, this.getToFilled, 0.03f, this.fillSpeed);
                    }
                    else if (this.blinkCounter > 0)
                    {
                        this.blinkCounter--;
                    }
                    else if (UnityEngine.Random.value < 0.0166666675f)
                    {
                        this.getToFilled = UnityEngine.Random.value;
                        this.fillSpeed = 1f / Mathf.Lerp(2f, 80f, UnityEngine.Random.value);
                    }
                }

                public Halo halo;
                public IntVector2 position;
                private float filled;
                private float lastFilled;
                private float getToFilled;
                private float fillSpeed;
                public int blinkCounter;
            }
        }

        public class UbilicalCord
        {
            public UbilicalCord(MaxGraphics owner, int firstSprite)
            {
                this.owner = owner;
                this.firstSprite = firstSprite;
                this.totalSprites = 1;
                this.coord = new Vector2[80, 3];
                for (int i = 0; i < this.coord.GetLength(0); i++)
                {
                    this.coord[i, 0] = owner.owner.firstChunk.pos;
                    this.coord[i, 1] = this.coord[i, 0];
                }
                this.totalSprites += this.coord.GetLength(0) * 2;
                this.smallCords = new Vector2[14, 20, 3];
                this.smallCordsLengths = new float[this.smallCords.GetLength(0)];
                this.smallCordsHeadDirs = new Vector2[this.smallCords.GetLength(0)];
                this.smallCoordColors = new int[this.smallCords.GetLength(0)];
                for (int j = 0; j < this.smallCords.GetLength(0); j++)
                {
                    this.smallCordsLengths[j] = ((UnityEngine.Random.value >= 0.5f) ? Mathf.Lerp(50f, 200f, Mathf.Pow(UnityEngine.Random.value, 1.5f)) : (50f + UnityEngine.Random.value * 15f));
                    this.smallCoordColors[j] = UnityEngine.Random.Range(0, 3);
                    this.smallCordsHeadDirs[j] = Custom.RNV() * UnityEngine.Random.value;
                    for (int k = 0; k < this.smallCords.GetLength(1); k++)
                    {
                        this.coord[k, 0] = owner.owner.firstChunk.pos;
                        this.coord[k, 1] = this.coord[k, 0];
                    }
                }
                this.totalSprites += this.smallCords.GetLength(0);
            }

            public int SegmentSprite(int seg, int part)
            {
                return this.firstSprite + 1 + seg * 2 + part;
            }

            public int SmallCordSprite(int c)
            {
                return this.firstSprite + 1 + this.coord.GetLength(0) * 2 + c;
            }

            public void Update()
            {
                for (int i = 0; i < this.coord.GetLength(0); i++)
                {
                    float value = (float)i / (float)(this.coord.GetLength(0) - 1);
                    this.coord[i, 1] = this.coord[i, 0];
                    this.coord[i, 0] += this.coord[i, 2];
                    this.coord[i, 2] *= 0.995f;
                    this.coord[i, 2].y += Mathf.InverseLerp(0.2f, 0f, value);
                    this.coord[i, 2].y -= this.owner.owner.room.gravity * 0.9f;
                    SharedPhysics.TerrainCollisionData terrainCollisionData = new SharedPhysics.TerrainCollisionData(this.coord[i, 0], this.coord[i, 1], this.coord[i, 2], 5f, new IntVector2(0, 0), true);
                    terrainCollisionData = SharedPhysics.VerticalCollision(this.owner.owner.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.HorizontalCollision(this.owner.owner.room, terrainCollisionData);
                    terrainCollisionData = SharedPhysics.SlopesVertically(this.owner.owner.room, terrainCollisionData);
                    this.coord[i, 0] = terrainCollisionData.pos;
                    this.coord[i, 2] = terrainCollisionData.vel;
                }
                this.SetStuckSegments();
                for (int j = 1; j < this.coord.GetLength(0); j++)
                {
                    Vector2 a = Custom.DirVec(this.coord[j, 0], this.coord[j - 1, 0]);
                    float num = Vector2.Distance(this.coord[j, 0], this.coord[j - 1, 0]);
                    this.coord[j, 0] -= (10f - num) * a * 0.5f;
                    this.coord[j, 2] -= (10f - num) * a * 0.5f;
                    this.coord[j - 1, 0] += (10f - num) * a * 0.5f;
                    this.coord[j - 1, 2] += (10f - num) * a * 0.5f;
                }
                this.SetStuckSegments();
                for (int k = 0; k < this.coord.GetLength(0) - 1; k++)
                {
                    Vector2 a2 = Custom.DirVec(this.coord[k, 0], this.coord[k + 1, 0]);
                    float num2 = Vector2.Distance(this.coord[k, 0], this.coord[k + 1, 0]);
                    this.coord[k, 0] -= (10f - num2) * a2 * 0.5f;
                    this.coord[k, 2] -= (10f - num2) * a2 * 0.5f;
                    this.coord[k + 1, 0] += (10f - num2) * a2 * 0.5f;
                    this.coord[k + 1, 2] += (10f - num2) * a2 * 0.5f;
                }
                this.SetStuckSegments();
                float num3 = 0.5f;
                for (int l = 2; l < 4; l++)
                {
                    for (int m = l; m < this.coord.GetLength(0) - l; m++)
                    {
                        this.coord[m, 2] += Custom.DirVec(this.coord[m - l, 0], this.coord[m, 0]) * num3;
                        this.coord[m - l, 2] -= Custom.DirVec(this.coord[m - l, 0], this.coord[m, 0]) * num3;
                        this.coord[m, 2] += Custom.DirVec(this.coord[m + l, 0], this.coord[m, 0]) * num3;
                        this.coord[m + l, 2] -= Custom.DirVec(this.coord[m + l, 0], this.coord[m, 0]) * num3;
                    }
                    num3 *= 0.75f;
                }
                if (!Custom.DistLess(this.coord[this.coord.GetLength(0) - 1, 0], this.owner.owner.firstChunk.pos, 80f))
                {
                    Vector2 a3 = Custom.DirVec(this.coord[this.coord.GetLength(0) - 1, 0], this.owner.owner.firstChunk.pos);
                    float num4 = Vector2.Distance(this.coord[this.coord.GetLength(0) - 1, 0], this.owner.owner.firstChunk.pos);
                    this.coord[this.coord.GetLength(0) - 1, 0] -= (80f - num4) * a3 * 0.25f;
                    this.coord[this.coord.GetLength(0) - 1, 2] -= (80f - num4) * a3 * 0.5f;
                }
                for (int n = 0; n < this.smallCords.GetLength(0); n++)
                {
                    for (int num5 = 0; num5 < this.smallCords.GetLength(1); num5++)
                    {
                        this.smallCords[n, num5, 1] = this.smallCords[n, num5, 0];
                        this.smallCords[n, num5, 0] += this.smallCords[n, num5, 2];
                        this.smallCords[n, num5, 2] *= Custom.LerpMap(this.smallCords[n, num5, 2].magnitude, 2f, 6f, 0.999f, 0.9f);
                        this.smallCords[n, num5, 2].y -= this.owner.owner.room.gravity * 0.9f;
                    }
                    float num6 = this.smallCordsLengths[n] / (float)this.smallCords.GetLength(1);
                    for (int num7 = 1; num7 < this.smallCords.GetLength(1); num7++)
                    {
                        Vector2 a4 = Custom.DirVec(this.smallCords[n, num7, 0], this.smallCords[n, num7 - 1, 0]);
                        float num8 = Vector2.Distance(this.smallCords[n, num7, 0], this.smallCords[n, num7 - 1, 0]);
                        this.smallCords[n, num7, 0] -= (num6 - num8) * a4 * 0.5f;
                        this.smallCords[n, num7, 2] -= (num6 - num8) * a4 * 0.5f;
                        this.smallCords[n, num7 - 1, 0] += (num6 - num8) * a4 * 0.5f;
                        this.smallCords[n, num7 - 1, 2] += (num6 - num8) * a4 * 0.5f;
                    }
                    for (int num9 = 0; num9 < this.smallCords.GetLength(1) - 1; num9++)
                    {
                        Vector2 a5 = Custom.DirVec(this.smallCords[n, num9, 0], this.smallCords[n, num9 + 1, 0]);
                        float num10 = Vector2.Distance(this.smallCords[n, num9, 0], this.smallCords[n, num9 + 1, 0]);
                        this.smallCords[n, num9, 0] -= (num6 - num10) * a5 * 0.5f;
                        this.smallCords[n, num9, 2] -= (num6 - num10) * a5 * 0.5f;
                        this.smallCords[n, num9 + 1, 0] += (num6 - num10) * a5 * 0.5f;
                        this.smallCords[n, num9 + 1, 2] += (num6 - num10) * a5 * 0.5f;
                    }
                    this.smallCords[n, 0, 0] = this.coord[this.coord.GetLength(0) - 1, 0];
                    this.smallCords[n, 0, 2] *= 0f;
                    this.smallCords[n, 1, 2] += Custom.DirVec(this.coord[this.coord.GetLength(0) - 2, 0], this.coord[this.coord.GetLength(0) - 1, 0]) * 5f;
                    this.smallCords[n, 2, 2] += Custom.DirVec(this.coord[this.coord.GetLength(0) - 2, 0], this.coord[this.coord.GetLength(0) - 1, 0]) * 3f;
                    this.smallCords[n, 3, 2] += Custom.DirVec(this.coord[this.coord.GetLength(0) - 2, 0], this.coord[this.coord.GetLength(0) - 1, 0]) * 1.5f;
                    this.smallCords[n, this.smallCords.GetLength(1) - 1, 0] = this.owner.head.pos;
                    this.smallCords[n, this.smallCords.GetLength(1) - 1, 2] *= 0f;
                    this.smallCords[n, this.smallCords.GetLength(1) - 2, 2] -= (this.owner.lookDir + this.smallCordsHeadDirs[n]) * 2f;
                    this.smallCords[n, this.smallCords.GetLength(1) - 3, 2] -= this.owner.lookDir + this.smallCordsHeadDirs[n];
                }
            }

            private void SetStuckSegments()
            {
                this.coord[0, 0] = this.owner.owner.room.MiddleOfTile(24, 2);
                this.coord[0, 2] *= 0f;
                Vector2 pos = this.owner.armJointGraphics[1].myJoint.pos;
                Vector2 vector = this.owner.armJointGraphics[1].myJoint.ElbowPos(1f, this.owner.armJointGraphics[2].myJoint.pos);
                for (int i = -1; i < 2; i++)
                {
                    float num = (i != 0) ? 0.5f : 1f;
                    this.coord[this.coord.GetLength(0) - 20 + i, 0] = Vector2.Lerp(this.coord[this.coord.GetLength(0) - 20 + i, 0], Vector2.Lerp(pos, vector, 0.4f + 0.07f * (float)i) + Custom.PerpendicularVector(pos, vector) * 8f, num);
                    this.coord[this.coord.GetLength(0) - 20 + i, 2] *= 1f - num;
                }
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[this.firstSprite] = TriangleMesh.MakeLongMesh(this.coord.GetLength(0), false, false);
                for (int i = 0; i < this.coord.GetLength(0); i++)
                {
                    sLeaser.sprites[this.SegmentSprite(i, 0)] = new FSprite("CentipedeSegment", true);
                    sLeaser.sprites[this.SegmentSprite(i, 1)] = new FSprite("CentipedeSegment", true);
                    sLeaser.sprites[this.SegmentSprite(i, 0)].scaleX = 0.5f;
                    sLeaser.sprites[this.SegmentSprite(i, 0)].scaleY = 0.3f;
                    sLeaser.sprites[this.SegmentSprite(i, 1)].scaleX = 0.4f;
                    sLeaser.sprites[this.SegmentSprite(i, 1)].scaleY = 0.15f;
                }
                for (int j = 0; j < this.smallCords.GetLength(0); j++)
                {
                    sLeaser.sprites[this.SmallCordSprite(j)] = TriangleMesh.MakeLongMesh(this.smallCords.GetLength(1), false, false);
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = this.coord[0, 0];
                Vector2 vector2 = new Vector2(0f, 0f);
                float d = 1.2f;
                for (int i = 0; i < this.coord.GetLength(0); i++)
                {
                    Vector2 vector3 = Vector2.Lerp(this.coord[i, 1], this.coord[i, 0], timeStacker);
                    Vector2 vector4 = Custom.DirVec(vector, vector3);
                    Vector2 a = Custom.PerpendicularVector(vector4);
                    float d2 = Vector2.Distance(vector, vector3);
                    (sLeaser.sprites[this.firstSprite] as TriangleMesh).MoveVertice(i * 4, vector3 - vector4 * d2 * 0.5f - a * d - camPos);
                    (sLeaser.sprites[this.firstSprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector3 - vector4 * d2 * 0.5f + a * d - camPos);
                    (sLeaser.sprites[this.firstSprite] as TriangleMesh).MoveVertice(i * 4 + 2, vector3 - a * d - camPos);
                    (sLeaser.sprites[this.firstSprite] as TriangleMesh).MoveVertice(i * 4 + 3, vector3 + a * d - camPos);
                    Vector2 b = vector4;
                    if (i < this.coord.GetLength(0) - 1)
                    {
                        b = Custom.DirVec(vector3, Vector2.Lerp(this.coord[i + 1, 1], this.coord[i + 1, 0], timeStacker));
                    }
                    sLeaser.sprites[this.SegmentSprite(i, 0)].x = vector3.x - camPos.x;
                    sLeaser.sprites[this.SegmentSprite(i, 0)].y = vector3.y - camPos.y;
                    sLeaser.sprites[this.SegmentSprite(i, 0)].rotation = Custom.VecToDeg((vector4 + b).normalized) + 90f;
                    sLeaser.sprites[this.SegmentSprite(i, 1)].x = vector3.x - camPos.x;
                    sLeaser.sprites[this.SegmentSprite(i, 1)].y = vector3.y - camPos.y;
                    sLeaser.sprites[this.SegmentSprite(i, 1)].rotation = Custom.VecToDeg((vector4 + b).normalized) + 90f;
                    vector = vector3;
                    vector2 = vector4;
                }
                for (int j = 0; j < this.smallCords.GetLength(0); j++)
                {
                    Vector2 a2 = Vector2.Lerp(this.smallCords[j, 0, 1], this.smallCords[j, 0, 0], timeStacker);
                    float d3 = 0.5f;
                    for (int k = 0; k < this.smallCords.GetLength(1); k++)
                    {
                        Vector2 vector5 = Vector2.Lerp(this.smallCords[j, k, 1], this.smallCords[j, k, 0], timeStacker);
                        Vector2 normalized = (a2 - vector5).normalized;
                        Vector2 a3 = Custom.PerpendicularVector(normalized);
                        float d4 = Vector2.Distance(a2, vector5) / 5f;
                        (sLeaser.sprites[this.SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4, a2 - normalized * d4 - a3 * d3 - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 1, a2 - normalized * d4 + a3 * d3 - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 2, vector5 + normalized * d4 - a3 * d3 - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(j)] as TriangleMesh).MoveVertice(k * 4 + 3, vector5 + normalized * d4 + a3 * d3 - camPos);
                        a2 = vector5;
                    }
                }
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[this.firstSprite].color = this.owner.armJointGraphics[0].metalColor;
                for (int i = 0; i < this.coord.GetLength(0); i++)
                {
                    sLeaser.sprites[this.SegmentSprite(i, 0)].color = Color.Lerp(this.owner.armJointGraphics[0].BaseColor(default(Vector2)), this.owner.armJointGraphics[0].metalColor, 0.5f);
                    sLeaser.sprites[this.SegmentSprite(i, 1)].color = Color.Lerp(this.owner.armJointGraphics[0].HighLightColor(default(Vector2)), this.owner.armJointGraphics[0].metalColor, 0.35f);
                }
                for (int j = 0; j < this.smallCords.GetLength(0); j++)
                {
                    if (this.smallCoordColors[j] == 0)
                    {
                        sLeaser.sprites[this.SmallCordSprite(j)].color = this.owner.armJointGraphics[0].metalColor;
                    }
                    else if (this.smallCoordColors[j] == 1)
                    {
                        sLeaser.sprites[this.SmallCordSprite(j)].color = Color.Lerp(new Color(0f, 0f, 1f), this.owner.armJointGraphics[0].metalColor, 0.5f);
                        //sLeaser.sprites[this.SmallCordSprite(j)].color = Color.Lerp(new Color(1f, 0f, 0f), this.owner.armJointGraphics[0].metalColor, 0.5f);
                    }
                    else if (this.smallCoordColors[j] == 2)
                    {
                        sLeaser.sprites[this.SmallCordSprite(j)].color = Color.Lerp(new Color(0f, 1f, 0f), this.owner.armJointGraphics[0].metalColor, 0.5f);
                        //sLeaser.sprites[this.SmallCordSprite(j)].color = Color.Lerp(new Color(0f, 0f, 1f), this.owner.armJointGraphics[0].metalColor, 0.5f);
                    }
                }
            }

            private MaxGraphics owner;
            public int firstSprite;
            public int totalSprites;
            public float[] smallCordsLengths;
            public Vector2[] smallCordsHeadDirs;
            public int[] smallCoordColors;
            public Vector2[,] coord;
            public Vector2[,,] smallCords;
        }

        public class DisconnectedUbilicalCord
        {
            public DisconnectedUbilicalCord(MaxGraphics owner, int firstSprite)
            {
                this.owner = owner;
                this.firstSprite = firstSprite;
                this.smallCords = new Vector2[10, 6, 3];
                this.smallCordsLengths = new float[this.smallCords.GetLength(0)];
                this.smallCordsHeadDirs = new Vector2[this.smallCords.GetLength(0)];
                this.smallCoordColors = new int[this.smallCords.GetLength(0)];
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    this.smallCordsLengths[i] = Mathf.Lerp(1f, 20f, UnityEngine.Random.value);
                    this.smallCoordColors[i] = ((UnityEngine.Random.value >= 0.5f) ? UnityEngine.Random.Range(1, 3) : 0);
                    this.smallCordsHeadDirs[i] = Custom.RNV() * UnityEngine.Random.value;
                }
                this.totalSprites = this.smallCords.GetLength(0);
            }

            public int SmallCordSprite(int c)
            {
                return this.firstSprite + c;
            }

            public void Reset(Vector2 pnt)
            {
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    for (int j = 0; j < this.smallCords.GetLength(1); j++)
                    {
                        this.smallCords[i, j, 0] = pnt + Custom.RNV() * UnityEngine.Random.value;
                        this.smallCords[i, j, 1] = this.smallCords[i, j, 0];
                        this.smallCords[i, j, 2] *= 0f;
                    }
                }
            }

            public void Update()
            {
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    for (int j = 0; j < this.smallCords.GetLength(1); j++)
                    {
                        float num = (float)j / (float)(this.smallCords.GetLength(1) - 1);
                        this.smallCords[i, j, 1] = this.smallCords[i, j, 0];
                        this.smallCords[i, j, 0] += this.smallCords[i, j, 2];
                        this.smallCords[i, j, 2] *= 0.94f;
                        this.smallCords[i, j, 2].y -= this.owner.owner.room.gravity * 0.9f;
                        SharedPhysics.TerrainCollisionData terrainCollisionData = new SharedPhysics.TerrainCollisionData(this.smallCords[i, j, 0], this.smallCords[i, j, 1], this.smallCords[i, j, 2], 1f, new IntVector2(0, 0), true);
                        terrainCollisionData = SharedPhysics.VerticalCollision(this.owner.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.HorizontalCollision(this.owner.owner.room, terrainCollisionData);
                        terrainCollisionData = SharedPhysics.SlopesVertically(this.owner.owner.room, terrainCollisionData);
                        this.smallCords[i, j, 0] = terrainCollisionData.pos;
                        this.smallCords[i, j, 2] = terrainCollisionData.vel;
                    }
                    float num2 = this.smallCordsLengths[i] / (float)this.smallCords.GetLength(1);
                    for (int k = 1; k < this.smallCords.GetLength(1); k++)
                    {
                        Vector2 a = Custom.DirVec(this.smallCords[i, k, 0], this.smallCords[i, k - 1, 0]);
                        float num3 = Vector2.Distance(this.smallCords[i, k, 0], this.smallCords[i, k - 1, 0]);
                        this.smallCords[i, k, 0] -= (num2 - num3) * a * 0.5f;
                        this.smallCords[i, k, 2] -= (num2 - num3) * a * 0.5f;
                        this.smallCords[i, k - 1, 0] += (num2 - num3) * a * 0.5f;
                        this.smallCords[i, k - 1, 2] += (num2 - num3) * a * 0.5f;
                    }
                    for (int l = 0; l < this.smallCords.GetLength(1) - 1; l++)
                    {
                        Vector2 a2 = Custom.DirVec(this.smallCords[i, l, 0], this.smallCords[i, l + 1, 0]);
                        float num4 = Vector2.Distance(this.smallCords[i, l, 0], this.smallCords[i, l + 1, 0]);
                        this.smallCords[i, l, 0] -= (num2 - num4) * a2 * 0.5f;
                        this.smallCords[i, l, 2] -= (num2 - num4) * a2 * 0.5f;
                        this.smallCords[i, l + 1, 0] += (num2 - num4) * a2 * 0.5f;
                        this.smallCords[i, l + 1, 2] += (num2 - num4) * a2 * 0.5f;
                    }
                    this.smallCords[i, this.smallCords.GetLength(1) - 1, 0] = this.owner.head.pos + Custom.DirVec(this.owner.owner.firstChunk.pos, this.owner.head.pos) * 3f;
                    this.smallCords[i, this.smallCords.GetLength(1) - 1, 2] *= 0f;
                    this.smallCords[i, this.smallCords.GetLength(1) - 2, 2] -= (this.owner.lookDir + this.smallCordsHeadDirs[i]) * 3f;
                    this.smallCords[i, this.smallCords.GetLength(1) - 3, 2] -= (this.owner.lookDir + this.smallCordsHeadDirs[i]) * 1.5f;
                }
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    sLeaser.sprites[this.SmallCordSprite(i)] = TriangleMesh.MakeLongMesh(this.smallCords.GetLength(1), false, false);
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    Vector2 a = Vector2.Lerp(this.smallCords[i, 0, 1], this.smallCords[i, 0, 0], timeStacker);
                    float d = 0.5f;
                    for (int j = 0; j < this.smallCords.GetLength(1); j++)
                    {
                        Vector2 vector = Vector2.Lerp(this.smallCords[i, j, 1], this.smallCords[i, j, 0], timeStacker);
                        Vector2 normalized = (a - vector).normalized;
                        Vector2 a2 = Custom.PerpendicularVector(normalized);
                        float d2 = Vector2.Distance(a, vector) / 5f;
                        (sLeaser.sprites[this.SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4, a - normalized * d2 - a2 * d - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 1, a - normalized * d2 + a2 * d - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 2, vector + normalized * d2 - a2 * d - camPos);
                        (sLeaser.sprites[this.SmallCordSprite(i)] as TriangleMesh).MoveVertice(j * 4 + 3, vector + normalized * d2 + a2 * d - camPos);
                        a = vector;
                    }
                }
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                Color b = Color.Lerp(this.owner.armJointGraphics[0].metalColor, new Color(0.7f, 0.7f, 0.7f), 0.15f);
                for (int i = 0; i < this.smallCords.GetLength(0); i++)
                {
                    if (this.smallCoordColors[i] == 0)
                    {
                        sLeaser.sprites[this.SmallCordSprite(i)].color = Color.Lerp(this.owner.armJointGraphics[0].metalColor, b, 0.5f);
                    }
                    else if (this.smallCoordColors[i] == 1)
                    {
                        sLeaser.sprites[this.SmallCordSprite(i)].color = Color.Lerp(new Color(0f, 0f, 1f), b, 0.5f);
                        //sLeaser.sprites[this.SmallCordSprite(i)].color = Color.Lerp(new Color(1f, 0f, 0f), b, 0.5f);
                    }
                    else if (this.smallCoordColors[i] == 2)
                    {
                        sLeaser.sprites[this.SmallCordSprite(i)].color = Color.Lerp(new Color(0f, 1f, 1f), b, 0.5f);
                        //sLeaser.sprites[this.SmallCordSprite(i)].color = Color.Lerp(new Color(0f, 0f, 1f), b, 0.5f);
                    }
                }
            }

            private MaxGraphics owner;
            public int firstSprite;
            public int totalSprites;
            public float[] smallCordsLengths;
            public Vector2[] smallCordsHeadDirs;
            public int[] smallCoordColors;
            public Vector2[,,] smallCords;
        }

        public class ArmBase
        {
            public ArmBase(MaxGraphics owner, int firstSprite)
            {
                this.owner = owner;
                this.firstSprite = firstSprite;
                this.totalSprites = 13;
            }

            public int SupportSprite(int side, int part)
            {
                return this.firstSprite + side * 3 + part;
            }

            public int CircleSprite(int side, int part)
            {
                return this.firstSprite + 9 + side * 2 + part;
            }

            public void Update()
            {
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[12];
                array[0] = new TriangleMesh.Triangle(0, 1, 2);
                array[1] = new TriangleMesh.Triangle(0, 2, 3);
                array[2] = new TriangleMesh.Triangle(0, 3, 4);
                array[3] = new TriangleMesh.Triangle(0, 4, 5);
                array[4] = new TriangleMesh.Triangle(4, 5, 7);
                array[5] = new TriangleMesh.Triangle(6, 5, 7);
                array[6] = new TriangleMesh.Triangle(11, 6, 7);
                array[7] = new TriangleMesh.Triangle(11, 7, 8);
                array[8] = new TriangleMesh.Triangle(11, 8, 9);
                array[9] = new TriangleMesh.Triangle(11, 9, 10);
                sLeaser.sprites[this.firstSprite + 6] = new TriangleMesh("Futile_White", array, false, false);
                sLeaser.sprites[this.firstSprite + 7] = new TriangleMesh("Futile_White", array, false, false);
                sLeaser.sprites[this.firstSprite + 8] = new TriangleMesh("Futile_White", array, false, false);
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[this.SupportSprite(i, j)] = new FSprite("pixel", true);
                        sLeaser.sprites[this.SupportSprite(i, j)].scaleX = 2f;
                        sLeaser.sprites[this.SupportSprite(i, j)].anchorY = 0f;
                        sLeaser.sprites[this.CircleSprite(i, j)] = new FSprite("Circle20", true);
                    }
                    sLeaser.sprites[this.CircleSprite(i, 0)].scale = 0.5f;
                    sLeaser.sprites[this.CircleSprite(i, 1)].scale = 0.45f;
                    sLeaser.sprites[this.SupportSprite(i, 2)] = new FSprite("deerEyeB", true);
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = Vector2.Lerp(this.owner.armJointGraphics[0].myJoint.lastPos, this.owner.armJointGraphics[0].myJoint.pos, timeStacker);
                Vector2 vector2 = this.owner.oracle.arm.BaseDir(timeStacker);
                vector -= vector2 * 10f;
                Vector2 vector3 = Custom.PerpendicularVector(vector2);
                this.UpdateMesh(sLeaser, camPos, this.firstSprite + 6, vector, vector2, vector3, 30f, 17f, 20f);
                this.UpdateMesh(sLeaser, camPos, this.firstSprite + 7, vector, vector2, vector3, 24f, 15f, 30f);
                this.UpdateMesh(sLeaser, camPos, this.firstSprite + 8, vector, vector2, vector3, 30f, 17f, 20f);
                Vector2 to = this.owner.armJointGraphics[0].myJoint.ElbowPos(timeStacker, Vector2.Lerp(this.owner.armJointGraphics[1].myJoint.lastPos, this.owner.armJointGraphics[1].myJoint.pos, timeStacker));
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vector4 = vector + vector2 * 11f + vector3 * 17f * ((i != 0) ? 1f : -1f);
                    Vector2 vector5 = Vector2.Lerp(Vector2.Lerp(this.owner.armJointGraphics[0].myJoint.lastPos, this.owner.armJointGraphics[0].myJoint.pos, timeStacker), to, 0.25f);
                    Vector2 vector6 = Custom.InverseKinematic(vector4, vector5, 25f, 45f, (i != 0) ? -1f : 1f);
                    sLeaser.sprites[this.SupportSprite(i, 0)].x = vector4.x - camPos.x;
                    sLeaser.sprites[this.SupportSprite(i, 0)].y = vector4.y - camPos.y;
                    sLeaser.sprites[this.SupportSprite(i, 0)].rotation = Custom.AimFromOneVectorToAnother(vector4, vector6);
                    sLeaser.sprites[this.SupportSprite(i, 0)].scaleY = Vector2.Distance(vector4, vector6) + 10f;
                    sLeaser.sprites[this.SupportSprite(i, 1)].x = vector6.x - camPos.x;
                    sLeaser.sprites[this.SupportSprite(i, 1)].y = vector6.y - camPos.y;
                    sLeaser.sprites[this.SupportSprite(i, 1)].rotation = Custom.AimFromOneVectorToAnother(vector6, vector5);
                    sLeaser.sprites[this.SupportSprite(i, 1)].scaleY = Vector2.Distance(vector6, vector5);
                    sLeaser.sprites[this.SupportSprite(i, 2)].x = vector6.x - camPos.x;
                    sLeaser.sprites[this.SupportSprite(i, 2)].y = vector6.y - camPos.y;
                    Vector2 vector7 = vector + 17f * vector3 * ((i != 0) ? 1f : -1f);
                    sLeaser.sprites[this.CircleSprite(i, 0)].x = vector7.x - camPos.x;
                    sLeaser.sprites[this.CircleSprite(i, 0)].y = vector7.y - camPos.y;
                    sLeaser.sprites[this.CircleSprite(i, 1)].x = vector7.x - camPos.x - 1f;
                    sLeaser.sprites[this.CircleSprite(i, 1)].y = vector7.y - camPos.y + 1f;
                }
            }

            private void UpdateMesh(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos, int sprite, Vector2 pos, Vector2 dir, Vector2 perp, float width, float height, float innerWidth)
            {
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(0, pos - innerWidth * 0.5f * perp - height * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(1, pos - width * perp - height * 0.75f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(2, pos - width * perp + height * 0.75f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(3, pos - width * 0.8f * perp + height * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(4, pos - width * 0.5f * perp + height * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(5, pos - width * 0.3f * perp - height * 0.1f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(6, pos + width * 0.3f * perp - height * 0.1f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(7, pos + width * 0.5f * perp + height * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(8, pos + width * 0.8f * perp + height * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(9, pos + width * perp + height * 0.75f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(10, pos + width * perp - height * 0.75f * dir - camPos);
                (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(11, pos + innerWidth * 0.5f * perp - height * dir - camPos);
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                Color color = Color.Lerp(this.owner.armJointGraphics[0].metalColor, this.owner.armJointGraphics[0].BaseColor(default(Vector2)), 0.8f);
                Color color2 = Color.Lerp(this.owner.armJointGraphics[0].metalColor, this.owner.armJointGraphics[0].HighLightColor(default(Vector2)), 0.8f);
                sLeaser.sprites[this.firstSprite + 6].color = color;
                sLeaser.sprites[this.firstSprite + 7].color = color2;
                sLeaser.sprites[this.firstSprite + 8].color = color;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sLeaser.sprites[this.SupportSprite(i, j)].color = this.owner.armJointGraphics[0].metalColor;
                    }
                    sLeaser.sprites[this.CircleSprite(i, 0)].color = color;
                    sLeaser.sprites[this.CircleSprite(i, 1)].color = color2;
                }
            }

            private MaxGraphics owner;
            public int firstSprite;
            public int totalSprites;
        }
    }
}