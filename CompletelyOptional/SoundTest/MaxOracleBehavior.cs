using System;
using System.Collections.Generic;
using CoralBrain;
using HUD;
using Music;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class MaxOracleBehavior : OracleBehavior
    {
        public MaxOracleBehavior(MaxOracle oracle) : base(oracle)
        {
            this.oracle = oracle;

            this.currentGetTo = oracle.firstChunk.pos;
            this.lastPos = oracle.firstChunk.pos;
            this.nextPos = oracle.firstChunk.pos;
            this.pathProgression = 1f;
            this.investigateAngle = UnityEngine.Random.value * 360f;
            this.allSubBehaviors = new List<SubBehavior>();
            this.currSubBehavior = new NoSubBehavior(this);
            this.allSubBehaviors.Add(this.currSubBehavior);
            this.working = 1f;
            this.getToWorking = 1f;
            this.movementBehavior = ((UnityEngine.Random.value >= 0.5f) ? MovementBehavior.Idle : MovementBehavior.Meditate);
            this.playerEnteredWithMark = false; //oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark;
            this.voice = null;
        }

        public void SeePlayer()
        {
            if (this.timeSinceSeenPlayer < 0)
            {
                this.timeSinceSeenPlayer = 0;
            }
            this.greenNeuron = null;
            for (int i = 0; i < this.oracle.room.updateList.Count; i++)
            {
                if (this.oracle.room.updateList[i] is NSHSwarmer)
                {
                    this.greenNeuron = (this.oracle.room.updateList[i] as NSHSwarmer);
                    break;
                }
            }
            Debug.Log(string.Concat(new object[]
            {
            "See player ",
            this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad,
            " gn?:",
            this.greenNeuron != null
            }));
            if ((this.greenNeuron != null || (this.player.objectInStomach != null && this.player.objectInStomach.type == AbstractPhysicalObject.AbstractObjectType.NSHSwarmer)))
            {
                this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.pebblesSeenGreenNeuron = true;
                this.NewAction(Action.GetNeuron_Init);
            }
            else if (this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
            {
                if (this.oracle.room.game.StoryCharacter == 2)
                {
                    this.NewAction(Action.MeetRed_Init);
                }
                else if (this.oracle.room.game.StoryCharacter == 1)
                {
                    this.NewAction(Action.MeetYellow_Init);
                }
                else
                {
                    this.NewAction(Action.MeetWhite_Shocked);
                }
                if (this.oracle.room.game.StoryCharacter != 2)
                {
                    this.SlugcatEnterRoomReaction();
                }
            }
            else
            {
                Debug.Log("Throw out player " + this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts);
                if (this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0)
                {
                    this.NewAction(Action.ThrowOut_KillOnSight);
                }
                else
                {
                    this.NewAction(Action.ThrowOut_SecondThrowOut);
                    this.SlugcatEnterRoomReaction();
                }
                this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts++;
            }
            this.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
        }

        public override void Update(bool eu)
        {
            if (this.timeSinceSeenPlayer >= 0)
            {
                this.timeSinceSeenPlayer++;
            }

            this.restartConversationAfterCurrentDialoge = false;

            base.Update(eu); //Noting Important :P
            for (int j = 0; j < this.oracle.room.game.cameras.Length; j++)
            {
                if (this.oracle.room.game.cameras[j].room == this.oracle.room)
                {
                    this.oracle.room.game.cameras[j].virtualMicrophone.volumeGroups[2] = 1f - this.oracle.room.gravity;
                }
                else
                {
                    this.oracle.room.game.cameras[j].virtualMicrophone.volumeGroups[2] = 1f;
                }
            }

            this.currSubBehavior.Update();

            if (!this.currSubBehavior.CurrentlyCommunicating)
            {
                this.pathProgression = Mathf.Min(1f, this.pathProgression + 1f / Mathf.Lerp(40f + this.pathProgression * 80f, Vector2.Distance(this.lastPos, this.nextPos) / 5f, 0.5f));
            }

            //Debug.Log(string.Concat((this.oracle as MaxOracle).arm.cornerPositions[0], (this.oracle as MaxOracle).arm.cornerPositions[1], (this.oracle as MaxOracle).arm.cornerPositions[2], (this.oracle as MaxOracle).arm.cornerPositions[3]));
            this.currentGetTo = Custom.Bezier(this.lastPos, this.ClampVectorInRoom(this.lastPos + this.lastPosHandle), this.nextPos, this.ClampVectorInRoom(this.nextPos + this.nextPosHandle), this.pathProgression);
            this.floatyMovement = false;
            this.investigateAngle += this.invstAngSpeed;
            this.inActionCounter++;

            if (this.pathProgression >= 1f && this.consistentBasePosCounter > 100 && !this.oracle.arm.baseMoving)
            {
                this.allStillCounter++;
            }
            else
            {
                this.allStillCounter = 0;
            }
            this.lastKillFac = this.killFac;

            this.action = Action.General_Idle;

            switch (this.action)
            {
                case Action.General_Idle:
                    if (this.movementBehavior != MovementBehavior.Idle && this.movementBehavior != MovementBehavior.Meditate)
                    {
                        this.movementBehavior = MovementBehavior.Idle;
                    }
                    this.throwOutCounter = 0;
                    if (false)
                    {
                        this.discoverCounter++;
                        if (this.oracle.room.GetTilePosition(this.player.mainBodyChunk.pos).y < 32 && (this.discoverCounter > 220 || Custom.DistLess(this.player.mainBodyChunk.pos, this.oracle.firstChunk.pos, 150f) || !Custom.DistLess(this.player.mainBodyChunk.pos, this.oracle.room.MiddleOfTile(this.oracle.room.ShortcutLeadingToNode(1).StartTile), 150f)))
                        {
                            this.SeePlayer();
                        }
                    }
                    break;

                case Action.General_GiveMark:
                    this.movementBehavior = MovementBehavior.KeepDistance;
                    if (this.inActionCounter > 30 && this.inActionCounter < 300)
                    {
                        this.player.Stun(20);
                        this.player.mainBodyChunk.vel += Vector2.ClampMagnitude(this.oracle.room.MiddleOfTile(24, 14) - this.player.mainBodyChunk.pos, 40f) / 40f * 2.8f * Mathf.InverseLerp(30f, 160f, (float)this.inActionCounter);
                    }
                    if (this.inActionCounter == 30)
                    {
                        this.oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0f, 1f, 1f);
                    }
                    if (this.inActionCounter == 300)
                    {
                        this.player.mainBodyChunk.vel += Custom.RNV() * 10f;
                        this.player.bodyChunks[1].vel += Custom.RNV() * 10f;
                        this.player.Stun(40);
                        (this.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                        if (this.player.slugcatStats.name == SlugcatStats.Name.Red)
                        {
                            this.oracle.room.game.GetStorySession.saveState.redExtraCycles = true;
                            if (this.oracle.room.game.cameras[0].hud != null)
                            {
                                if (this.oracle.room.game.cameras[0].hud.textPrompt != null)
                                {
                                    this.oracle.room.game.cameras[0].hud.textPrompt.cycleTick = 0;
                                }
                                if (this.oracle.room.game.cameras[0].hud.map != null && this.oracle.room.game.cameras[0].hud.map.cycleLabel != null)
                                {
                                    this.oracle.room.game.cameras[0].hud.map.cycleLabel.UpdateCycleText();
                                }
                            }
                            if (this.player.redsIllness != null)
                            {
                                this.player.redsIllness.GetBetter();
                            }
                            if (!this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.pebblesHasIncreasedRedsKarmaCap)
                            {
                                this.oracle.room.game.GetStorySession.saveState.IncreaseKarmaCapOneStep();
                                this.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.pebblesHasIncreasedRedsKarmaCap = true;
                            }
                            else
                            {
                                Debug.Log("PEBBLES HAS ALREADY GIVEN RED ONE KARMA CAP STEP");
                            }
                        }
                        else
                        {
                            (this.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 9;
                        }
                        (this.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = (this.oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap;
                        for (int l = 0; l < this.oracle.room.game.cameras.Length; l++)
                        {
                            if (this.oracle.room.game.cameras[l].hud.karmaMeter != null)
                            {
                                this.oracle.room.game.cameras[l].hud.karmaMeter.UpdateGraphic();
                            }
                        }
                        for (int m = 0; m < 20; m++)
                        {
                            this.oracle.room.AddObject(new Spark(this.player.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                    }
                    if (this.inActionCounter > 300 && this.player.graphicsModule != null)
                    {
                        (this.player.graphicsModule as PlayerGraphics).markAlpha = Mathf.Max((this.player.graphicsModule as PlayerGraphics).markAlpha, Mathf.InverseLerp(500f, 300f, (float)this.inActionCounter));
                    }
                    if (this.inActionCounter >= 500)
                    {
                        this.NewAction(this.afterGiveMarkAction);
                    }
                    break;
            }
            this.Move();
            if (this.working != this.getToWorking)
            {
                this.working = Custom.LerpAndTick(this.working, this.getToWorking, 0.05f, 0.0333333351f);
            }

            if (!this.currSubBehavior.Gravity)
            {
                this.oracle.room.gravity = Custom.LerpAndTick(this.oracle.room.gravity, 0f, 0.05f, 0.02f);
            }
            else
            {
                this.oracle.room.gravity = 1f - this.working;
            }
        }

        public void SlugcatEnterRoomReaction()
        {
            this.getToWorking = 0f;
            this.oracle.room.PlaySound(SoundID.SS_AI_Exit_Work_Mode, 0f, 1f, 1f);
            if (this.oracle.graphicsModule != null)
            {
                (this.oracle.graphicsModule as MaxGraphics).halo.ChangeAllRadi();
                (this.oracle.graphicsModule as MaxGraphics).halo.connectionsFireChance = 1f;
            }
            this.TurnOffSSMusic(true);
        }

        public void TurnOffSSMusic(bool abruptEnd)
        {
            Debug.Log("Fading out SS music " + abruptEnd);
            for (int i = 0; i < this.oracle.room.updateList.Count; i++)
            {
                if (this.oracle.room.updateList[i] is SSMusicTrigger)
                {
                    this.oracle.room.updateList[i].Destroy();
                    break;
                }
            }
            if (abruptEnd && this.oracle.room.game.manager.musicPlayer != null && this.oracle.room.game.manager.musicPlayer.song != null && this.oracle.room.game.manager.musicPlayer.song is SSSong)
            {
                this.oracle.room.game.manager.musicPlayer.song.FadeOut(2f);
            }
        }

        public void NewAction(Action nextAction)
        {
            Debug.Log(string.Concat(new string[]
            {
            "new action: ",
            nextAction.ToString(),
            " (from ",
            this.action.ToString(),
            ")"
            }));
            if (nextAction == this.action)
            {
                return;
            }
            SubBehavior.SubBehavID subBehavID;
            switch (nextAction)
            {
                case Action.MeetWhite_Shocked:
                case Action.MeetWhite_Curious:
                case Action.MeetWhite_Talking:
                case Action.MeetWhite_Texting:
                case Action.MeetWhite_Images:
                case Action.MeetWhite_SecondCurious:
                    subBehavID = SubBehavior.SubBehavID.MeetWhite;
                    break;

                case Action.MeetYellow_Init:
                    subBehavID = SubBehavior.SubBehavID.MeetYellow;
                    break;

                case Action.MeetRed_Init:
                    subBehavID = SubBehavior.SubBehavID.MeetRed;
                    break;

                case Action.GetNeuron_Init:
                case Action.GetNeuron_TakeNeuron:
                case Action.GetNeuron_GetOutOfStomach:
                case Action.GetNeuron_InspectNeuron:
                    subBehavID = SubBehavior.SubBehavID.GetNeuron;
                    break;

                case Action.ThrowOut_ThrowOut:
                case Action.ThrowOut_SecondThrowOut:
                case Action.ThrowOut_KillOnSight:
                case Action.ThrowOut_Polite_ThrowOut:
                    subBehavID = SubBehavior.SubBehavID.ThrowOut;
                    break;

                default:
                    subBehavID = SubBehavior.SubBehavID.General;
                    break;
            }
            this.currSubBehavior.NewAction(this.action, nextAction);
            if (subBehavID != SubBehavior.SubBehavID.General && subBehavID != this.currSubBehavior.ID)
            {
                SubBehavior subBehavior = null;
                for (int i = 0; i < this.allSubBehaviors.Count; i++)
                {
                    if (this.allSubBehaviors[i].ID == subBehavID)
                    {
                        subBehavior = this.allSubBehaviors[i];
                        break;
                    }
                }
                if (subBehavior == null)
                {
                    subBehavior = new NoSubBehavior(this);
                    this.allSubBehaviors.Add(subBehavior);
                }
                subBehavior.Activate(this.action, nextAction);
                this.currSubBehavior.Deactivate();
                Debug.Log("Switching subbehavior to: " + subBehavior.ID.ToString() + " from: " + this.currSubBehavior.ID.ToString());
                this.currSubBehavior = subBehavior;
            }
            this.inActionCounter = 0;
            this.action = nextAction;
        }

        private void Move()
        {
            this.movementBehavior = MovementBehavior.Meditate;
            switch (this.movementBehavior)
            {
                case MovementBehavior.Idle:
                    this.invstAngSpeed = 1f;
                    if (this.investigateMarble == null)
                    {
                        this.investigateMarble = this.oracle.marbles[UnityEngine.Random.Range(0, this.oracle.marbles.Count)];
                    }
                    if (this.investigateMarble != null && (this.investigateMarble.orbitObj == this.oracle || Custom.DistLess(new Vector2(250f, 150f), this.investigateMarble.firstChunk.pos, 100f)))
                    {
                        this.investigateMarble = null;
                    }
                    if (this.investigateMarble != null)
                    {
                        this.lookPoint = this.investigateMarble.firstChunk.pos;
                        if (Custom.DistLess(this.nextPos, this.investigateMarble.firstChunk.pos, 100f))
                        {
                            this.floatyMovement = true;
                            this.nextPos = this.investigateMarble.firstChunk.pos - Custom.DegToVec(this.investigateAngle) * 50f;
                        }
                        else
                        {
                            this.SetNewDestination(this.investigateMarble.firstChunk.pos - Custom.DegToVec(this.investigateAngle) * 50f);
                        }
                        if (this.pathProgression == 1f && UnityEngine.Random.value < 0.005f)
                        {
                            this.investigateMarble = null;
                        }
                    }
                    break;

                case MovementBehavior.Meditate:
                    if (this.nextPos != this.oracle.room.MiddleOfTile(24, 17))
                    {
                        this.SetNewDestination(this.oracle.room.MiddleOfTile(24, 17));
                    }
                    this.investigateAngle = 0f;
                    this.lookPoint = this.oracle.firstChunk.pos + new Vector2(0f, -40f);
                    //Close Eyes
                    //(this.oracle.graphicsModule as MaxGraphics).blink = 5;
                    break;

                case MovementBehavior.KeepDistance:
                    {
                        this.lookPoint = this.player.DangerPos;
                        Vector2 vector = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                        if (!this.oracle.room.GetTile(vector).Solid && this.oracle.room.aimap.getAItile(vector).terrainProximity > 2 && Vector2.Distance(vector, this.player.DangerPos) > Vector2.Distance(this.nextPos, this.player.DangerPos) + 100f)
                        {
                            this.SetNewDestination(vector);
                        }
                        break;
                    }
                case MovementBehavior.Investigate:
                    {
                        this.lookPoint = this.player.DangerPos;
                        if (this.investigateAngle < -90f || this.investigateAngle > 90f || (float)this.oracle.room.aimap.getAItile(this.nextPos).terrainProximity < 2f)
                        {
                            this.investigateAngle = Mathf.Lerp(-70f, 70f, UnityEngine.Random.value);
                            this.invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, UnityEngine.Random.value) * ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                        }
                        Vector2 vector = this.player.DangerPos + Custom.DegToVec(this.investigateAngle) * 150f;
                        if ((float)this.oracle.room.aimap.getAItile(vector).terrainProximity >= 2f)
                        {
                            if (this.pathProgression > 0.9f)
                            {
                                if (Custom.DistLess(this.oracle.firstChunk.pos, vector, 30f))
                                {
                                    this.floatyMovement = true;
                                }
                                else if (!Custom.DistLess(this.nextPos, vector, 30f))
                                {
                                    this.SetNewDestination(vector);
                                }
                            }
                            this.nextPos = vector;
                        }
                        break;
                    }
                case MovementBehavior.Talk:
                    {
                        this.lookPoint = this.player.DangerPos;
                        Vector2 vector = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                        if (this.CommunicatePosScore(vector) + 40f < this.CommunicatePosScore(this.nextPos) && !Custom.DistLess(vector, this.nextPos, 30f))
                        {
                            this.SetNewDestination(vector);
                        }
                        break;
                    }
                case MovementBehavior.ShowMedia:
                    if (this.currSubBehavior is SSOracleMeetWhite)
                    {
                        (this.currSubBehavior as SSOracleMeetWhite).ShowMediaMovementBehavior();
                    }
                    break;
            }
            if (this.currSubBehavior != null && this.currSubBehavior.LookPoint != null)
            {
                this.lookPoint = this.currSubBehavior.LookPoint.Value;
            }
            this.consistentBasePosCounter++;
            if (this.oracle.room.readyForAI)
            {
                Vector2 vector = new Vector2(UnityEngine.Random.value * this.oracle.room.PixelWidth, UnityEngine.Random.value * this.oracle.room.PixelHeight);
                if (!this.oracle.room.GetTile(vector).Solid && this.BasePosScore(vector) + 40f < this.BasePosScore(this.baseIdeal))
                {
                    this.baseIdeal = vector;
                    this.consistentBasePosCounter = 0;
                }
            }
            else
            {
                this.baseIdeal = this.nextPos;
            }
        }

        private float BasePosScore(Vector2 tryPos)
        {
            if (this.movementBehavior == MovementBehavior.Meditate)
            {
                return Vector2.Distance(tryPos, this.oracle.room.MiddleOfTile(24, 5));
            }
            if (this.movementBehavior == MovementBehavior.ShowMedia)
            {
                return -Vector2.Distance(this.player.DangerPos, tryPos);
            }
            float num = Mathf.Abs(Vector2.Distance(this.nextPos, tryPos) - 200f);
            return num + Custom.LerpMap(Vector2.Distance(this.player.DangerPos, tryPos), 40f, 300f, 800f, 0f);
        }

        private float CommunicatePosScore(Vector2 tryPos)
        {
            if (this.oracle.room.GetTile(tryPos).Solid)
            {
                return float.MaxValue;
            }
            float num = Mathf.Abs(Vector2.Distance(tryPos, this.player.DangerPos) - ((this.movementBehavior != MovementBehavior.Talk) ? 400f : 250f));
            num -= (float)Custom.IntClamp(this.oracle.room.aimap.getAItile(tryPos).terrainProximity, 0, 8) * 10f;
            if (this.movementBehavior == MovementBehavior.ShowMedia)
            {
                num += (float)(Custom.IntClamp(this.oracle.room.aimap.getAItile(tryPos).terrainProximity, 8, 16) - 8) * 10f;
            }
            return num;
        }

        public abstract class TalkBehavior : SubBehavior
        {
            public TalkBehavior(MaxOracleBehavior owner, SubBehavior.SubBehavID ID) : base(owner, ID)
            {
            }

            public override void NewAction(Action oldAction, Action newAction)
            {
                base.NewAction(oldAction, newAction);
                this.communicationIndex = 0;
            }

            public int communicationIndex;

            public int communicationPause;
        }

        public abstract class ConversationBehavior : TalkBehavior
        {
            public ConversationBehavior(MaxOracleBehavior owner, SubBehavior.SubBehavID ID, Conversation.ID convoID) : base(owner, ID)
            {
                this.convoID = convoID;
            }

            public Conversation.ID convoID;
        }

        public class SSOracleMeetWhite : ConversationBehavior
        {
            public SSOracleMeetWhite(MaxOracleBehavior owner) : base(owner, SubBehavior.SubBehavID.MeetWhite, Conversation.ID.Pebbles_White)
            {
                this.chatLabel = new OracleChatLabel(owner);
                base.oracle.room.AddObject(this.chatLabel);
                this.chatLabel.Hide();
            }

            public OracleChatLabel chatLabel;

            public override void Update()
            {
                switch (base.action)
                {
                    case Action.MeetWhite_Shocked:
                        this.owner.movementBehavior = MovementBehavior.KeepDistance;
                        if (this.owner.oracle.room.game.manager.rainWorld.progression.miscProgressionData.redHasVisitedPebbles)
                        {
                            if (base.inActionCounter > 40)
                            {
                                this.owner.NewAction(Action.General_GiveMark);
                                this.owner.afterGiveMarkAction = Action.General_MarkTalk;
                            }
                        }
                        else if (this.owner.oracle.room.game.IsStorySession && this.owner.oracle.room.game.GetStorySession.saveState.deathPersistentSaveData.theMark)
                        {
                            if (base.inActionCounter > 40)
                            {
                                this.owner.NewAction(Action.General_MarkTalk);
                            }
                        }
                        else if (base.inActionCounter > 120)
                        {
                            this.owner.NewAction(Action.MeetWhite_Curious);
                        }
                        break;

                    case Action.MeetWhite_Curious:
                        this.owner.movementBehavior = MovementBehavior.Investigate;
                        if (base.inActionCounter > 360)
                        {
                            this.owner.NewAction(Action.MeetWhite_Talking);
                        }
                        break;

                    case Action.MeetWhite_Talking:
                        this.owner.movementBehavior = MovementBehavior.Talk;
                        if (!this.CurrentlyCommunicating && this.communicationPause > 0)
                        {
                            this.communicationPause--;
                        }
                        if (!this.CurrentlyCommunicating && this.communicationPause < 1)
                        {
                            if (this.communicationIndex >= 4)
                            {
                                this.owner.NewAction(Action.MeetWhite_Texting);
                            }
                            else if (this.owner.allStillCounter > 20)
                            {
                                this.NextCommunication();
                            }
                        }
                        if (!this.CurrentlyCommunicating)
                        {
                            this.owner.nextPos += Custom.RNV();
                        }
                        break;

                    case Action.MeetWhite_Texting:
                        base.movementBehavior = MovementBehavior.ShowMedia;
                        if (base.oracle.graphicsModule != null)
                        {
                            (base.oracle.graphicsModule as OracleGraphics).halo.connectionsFireChance = 0f;
                        }
                        if (!this.CurrentlyCommunicating && this.communicationPause > 0)
                        {
                            this.communicationPause--;
                        }
                        if (!this.CurrentlyCommunicating && this.communicationPause < 1)
                        {
                            if (this.communicationIndex >= 6)
                            {
                                this.owner.NewAction(Action.MeetWhite_Images);
                            }
                            else if (this.owner.allStillCounter > 20)
                            {
                                this.NextCommunication();
                            }
                        }
                        this.chatLabel.setPos = new Vector2?(this.showMediaPos);
                        break;

                    case Action.MeetWhite_Images:
                        base.movementBehavior = MovementBehavior.ShowMedia;
                        if (this.communicationPause > 0)
                        {
                            this.communicationPause--;
                        }
                        if (base.inActionCounter > 150 && this.communicationPause < 1)
                        {
                            if (this.communicationIndex >= 3)
                            {
                                this.owner.NewAction(Action.MeetWhite_SecondCurious);
                            }
                            else
                            {
                                this.NextCommunication();
                            }
                        }
                        if (this.showImage != null)
                        {
                            this.showImage.setPos = new Vector2?(this.showMediaPos);
                        }
                        if (UnityEngine.Random.value < 0.0333333351f)
                        {
                            this.idealShowMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
                            this.showMediaPos += Custom.RNV() * UnityEngine.Random.value * 30f;
                        }
                        break;

                    case Action.MeetWhite_SecondCurious:
                        base.movementBehavior = MovementBehavior.Investigate;
                        if (base.inActionCounter == 80)
                        {
                            Debug.Log("extra talk");
                            //this.voice = base.oracle.room.PlaySound(SoundID.SS_AI_Talk_5, base.oracle.firstChunk);
                            //this.voice.requireActiveUpkeep = true;
                        }
                        if (base.inActionCounter > 240)
                        {
                            this.owner.NewAction(Action.General_GiveMark);
                            this.owner.afterGiveMarkAction = Action.General_MarkTalk;
                        }
                        break;
                }
            }

            public override void NewAction(Action oldAction, Action newAction)
            {
                base.NewAction(oldAction, newAction);
                if (oldAction == Action.MeetWhite_Texting)
                {
                    this.chatLabel.Hide();
                }
                if (oldAction == Action.MeetWhite_Images && this.showImage != null)
                {
                    this.showImage.Destroy();
                    this.showImage = null;
                }
                switch (newAction)
                {
                    case Action.MeetWhite_Curious:
                        this.owner.investigateAngle = Mathf.Lerp(-70f, 70f, UnityEngine.Random.value);
                        this.owner.invstAngSpeed = Mathf.Lerp(0.4f, 0.8f, UnityEngine.Random.value) * ((UnityEngine.Random.value >= 0.5f) ? 1f : -1f);
                        break;

                    case Action.MeetWhite_Texting:
                        this.communicationPause = 170;
                        break;
                }
            }

            public override void Deactivate()
            {
                this.chatLabel.Hide();
                if (this.showImage != null)
                {
                    this.showImage.Destroy();
                }
                base.Deactivate();
            }

            private void NextCommunication()
            {
                Debug.Log(string.Concat(new object[]
                {
                "New com att: ",
                base.action.ToString(),
                " ",
                this.communicationIndex
                }));
                switch (base.action)
                {
                    case Action.MeetWhite_Texting:
                        this.chatLabel.NewPhrase(this.communicationIndex);
                        break;

                    case Action.MeetWhite_Images:
                        if (this.showImage != null)
                        {
                            this.showImage.Destroy();
                        }
                        switch (this.communicationIndex)
                        {
                            case 0:
                                this.showImage = base.oracle.myScreen.AddImage("AIimg1");
                                this.communicationPause = 380;
                                break;

                            case 1:
                                this.showImage = base.oracle.myScreen.AddImage("AIimg2");
                                this.communicationPause = 290;
                                break;

                            case 2:
                                {
                                    ProjectionScreen myScreen = base.oracle.myScreen;
                                    List<string> list = new List<string>();
                                    list.Add("AIimg3a");
                                    list.Add("AIimg3b");
                                    this.showImage = myScreen.AddImage(list, 15);
                                    this.communicationPause = 330;
                                    break;
                                }
                        }
                        if (this.showImage != null)
                        {
                            base.oracle.room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
                            this.showImage.lastPos = this.showMediaPos;
                            this.showImage.pos = this.showMediaPos;
                            this.showImage.lastAlpha = 0f;
                            this.showImage.alpha = 0f;
                            this.showImage.setAlpha = new float?(1f);
                        }
                        break;
                }
                this.communicationIndex++;
            }

            public void ShowMediaMovementBehavior()
            {
                this.owner.lookPoint = base.player.DangerPos;
                Vector2 vector = new Vector2(UnityEngine.Random.value * base.oracle.room.PixelWidth, UnityEngine.Random.value * base.oracle.room.PixelHeight);
                if (this.owner.CommunicatePosScore(vector) + 40f < this.owner.CommunicatePosScore(this.owner.nextPos) && !Custom.DistLess(vector, this.owner.nextPos, 30f))
                {
                    this.owner.SetNewDestination(vector);
                }
                this.consistentShowMediaPosCounter += (int)Custom.LerpMap(Vector2.Distance(this.showMediaPos, this.idealShowMediaPos), 0f, 200f, 1f, 10f);
                vector = new Vector2(UnityEngine.Random.value * base.oracle.room.PixelWidth, UnityEngine.Random.value * base.oracle.room.PixelHeight);
                if (this.ShowMediaScore(vector) + 40f < this.ShowMediaScore(this.idealShowMediaPos))
                {
                    this.idealShowMediaPos = vector;
                    this.consistentShowMediaPosCounter = 0;
                }
                vector = this.idealShowMediaPos + Custom.RNV() * UnityEngine.Random.value * 40f;
                if (this.ShowMediaScore(vector) + 20f < this.ShowMediaScore(this.idealShowMediaPos))
                {
                    this.idealShowMediaPos = vector;
                    this.consistentShowMediaPosCounter = 0;
                }
                if (this.consistentShowMediaPosCounter > 300)
                {
                    this.showMediaPos = Vector2.Lerp(this.showMediaPos, this.idealShowMediaPos, 0.1f);
                    this.showMediaPos = Custom.MoveTowards(this.showMediaPos, this.idealShowMediaPos, 10f);
                }
            }

            private float ShowMediaScore(Vector2 tryPos)
            {
                if (base.oracle.room.GetTile(tryPos).Solid)
                {
                    return float.MaxValue;
                }
                float num = Mathf.Abs(Vector2.Distance(tryPos, base.player.DangerPos) - 250f);
                num -= Math.Min((float)base.oracle.room.aimap.getAItile(tryPos).terrainProximity, 9f) * 30f;
                num -= Vector2.Distance(tryPos, this.owner.nextPos) * 0.5f;
                for (int i = 0; i < base.oracle.arm.joints.Length; i++)
                {
                    num -= Mathf.Min(Vector2.Distance(tryPos, base.oracle.arm.joints[i].pos), 100f) * 10f;
                }
                if (base.oracle.graphicsModule != null)
                {
                    for (int j = 0; j < (base.oracle.graphicsModule as OracleGraphics).umbCord.coord.GetLength(0); j += 3)
                    {
                        num -= Mathf.Min(Vector2.Distance(tryPos, (base.oracle.graphicsModule as OracleGraphics).umbCord.coord[j, 0]), 100f);
                    }
                }
                return num;
            }

            public ProjectedImage showImage;
            public Vector2 idealShowMediaPos;
            public Vector2 showMediaPos;
            public int consistentShowMediaPosCounter;
        }

        private void SetNewDestination(Vector2 dst)
        {
            this.lastPos = this.currentGetTo;
            this.nextPos = dst;
            this.lastPosHandle = Custom.RNV() * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(this.lastPos, this.nextPos);
            this.nextPosHandle = -this.GetToDir * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(this.lastPos, this.nextPos);
            this.pathProgression = 0f;
        }

        private Vector2 ClampVectorInRoom(Vector2 v)
        {
            Vector2 result = v;
            result.x = Mathf.Clamp(result.x, (this.oracle as MaxOracle).arm.cornerPositions[0].x + 10f, (this.oracle as MaxOracle).arm.cornerPositions[1].x - 10f);
            result.y = Mathf.Clamp(result.y, (this.oracle as MaxOracle).arm.cornerPositions[2].y + 10f, (this.oracle as MaxOracle).arm.cornerPositions[1].y - 10f);
            return result;
        }

        public override Vector2 OracleGetToPos
        {
            get
            {
                Vector2 v = this.currentGetTo;
                if (this.floatyMovement && Custom.DistLess(this.oracle.firstChunk.pos, this.nextPos, 50f))
                {
                    v = this.nextPos;
                }
                return this.ClampVectorInRoom(v);
            }
        }

        public override Vector2 BaseGetToPos
        {
            get
            {
                return this.baseIdeal;
            }
        }

        public override Vector2 GetToDir
        {
            get
            {
                if (this.movementBehavior == MovementBehavior.Idle)
                {
                    return Custom.DegToVec(this.investigateAngle);
                }
                if (this.movementBehavior == MovementBehavior.Investigate)
                {
                    return -Custom.DegToVec(this.investigateAngle);
                }
                return new Vector2(0f, 1f);
            }
        }

        public override bool EyesClosed
        {
            get
            {
                return this.movementBehavior == MovementBehavior.Meditate;
            }
        }

        public bool HandTowardsPlayer()
        {
            return ((this.action == Action.General_GiveMark && (float)this.inActionCounter > 30f && this.inActionCounter < 300) || this.action == Action.ThrowOut_KillOnSight);
        }

        public string ReplaceParts(string s)
        {
            return s;
        }

        public RainWorld rainWorld
        {
            get
            {
                return this.oracle.room.game.rainWorld;
            }
        }

        public void SpecialEvent(string eventName)
        {
            Debug.Log("SPECEVENT : " + eventName);
            if (eventName == "karma")
            {
                this.afterGiveMarkAction = this.action;
                this.NewAction(Action.General_GiveMark);
            }
        }

        private Vector2 lastPos;
        private Vector2 nextPos;
        private Vector2 lastPosHandle;
        private Vector2 nextPosHandle;
        private Vector2 currentGetTo;
        private float pathProgression;
        private float investigateAngle;
        private float invstAngSpeed;
        public PebblesPearl investigateMarble;
        private Vector2 baseIdeal;
        public float working;
        public float getToWorking;
        public bool floatyMovement;
        public int discoverCounter;
        public float killFac;
        public float lastKillFac;
        public int throwOutCounter;
        public int playerOutOfRoomCounter;
        public SubBehavior currSubBehavior;
        public List<SubBehavior> allSubBehaviors;
        public NSHSwarmer greenNeuron;

        private bool pearlPickupReaction = true;
        private bool lastPearlPickedUp = true;
        private bool restartConversationAfterCurrentDialoge;
        private bool playerEnteredWithMark;
        public int timeSinceSeenPlayer = -1;
        public Action action;
        public Action afterGiveMarkAction;
        public MovementBehavior movementBehavior;

        public enum Action
        {
            General_Idle,
            General_MarkTalk,//
            General_GiveMark,
            MeetWhite_Shocked,
            MeetWhite_Curious,
            MeetWhite_Talking,
            MeetWhite_Texting,
            MeetWhite_Images,
            MeetWhite_SecondCurious,
            MeetYellow_Init,
            MeetRed_Init,
            GetNeuron_Init,
            GetNeuron_TakeNeuron,
            GetNeuron_GetOutOfStomach,
            GetNeuron_InspectNeuron,
            ThrowOut_ThrowOut,
            ThrowOut_SecondThrowOut,
            ThrowOut_KillOnSight,
            ThrowOut_Polite_ThrowOut
        }

        public enum MovementBehavior
        {
            Idle,
            Meditate,
            KeepDistance,//
            Investigate,
            Talk,
            ShowMedia,
            MusicList
        }

        public abstract class SubBehavior
        {
            public SubBehavior(MaxOracleBehavior owner, SubBehavior.SubBehavID ID)
            {
                this.owner = owner;
                this.ID = ID;
            }

            public Action action
            {
                get
                {
                    return this.owner.action;
                }
            }

            public MaxOracle oracle
            {
                get
                {
                    return this.oracle as MaxOracle; //this.owner.oracle
                }
            }

            public int inActionCounter
            {
                get
                {
                    return this.owner.inActionCounter;
                }
            }

            public MovementBehavior movementBehavior
            {
                get
                {
                    return this.owner.movementBehavior;
                }
                set
                {
                    this.owner.movementBehavior = value;
                }
            }

            public Player player
            {
                get
                {
                    return this.owner.player;
                }
            }

            public virtual Vector2? LookPoint
            {
                get
                {
                    return default(Vector2?);
                }
            }

            public virtual void Update()
            {
            }

            public virtual void NewAction(Action oldAction, Action newAction)
            {
            }

            public virtual bool CurrentlyCommunicating
            {
                get
                {
                    return false;
                }
            }

            public virtual void Activate(Action oldAction, Action newAction)
            {
                this.NewAction(oldAction, newAction);
            }

            public virtual void Deactivate()
            {
            }

            public virtual bool Gravity
            {
                get
                {
                    return true;
                }
            }

            public SubBehavior.SubBehavID ID;
            public MaxOracleBehavior owner;

            public enum SubBehavID
            {
                General,
                MeetWhite,
                MeetRed,
                MeetYellow,
                ThrowOut,
                GetNeuron
            }
        }

        public class NoSubBehavior : SubBehavior
        {
            public NoSubBehavior(MaxOracleBehavior owner) : base(owner, SubBehavior.SubBehavID.General)
            {
            }
        }
    }
}