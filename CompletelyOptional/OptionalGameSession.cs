using System;
using System.Collections.Generic;
using System.IO;
using ArenaBehaviors;
using HUD;
using RWCustom;
using UnityEngine;

namespace CompletelyOptional
{
    public class OptionalGameSession : ArenaGameSession
    {
        public OptionalGameSession(RainWorldGame game) : base(game)
        {
            //this.PlayMode = this.arenaSitting.sandboxPlayMode;
            //if (!this.PlayMode)
            //{
            this.arenaSitting.currentLevel = 0;
            if (this.noRain == null)
            {
                base.AddBehavior(new NoRain(this));
            }
            //}
            //else
            //{
            //this.arenaSitting.gameTypeSetup.UpdateCustomWinCondition();
            //this.checkWinLose = (this.arenaSitting.players.Count > 0 && this.arenaSitting.gameTypeSetup.customWinCondition);
            //}
        }

        public override bool SpawnDefaultRoomItems
        {
            get
            {
                return false;
            }
        }

        public override void SpawnCreatures()
        {
            base.SpawnCreatures();
            if (this.PlayMode)
            {
                int num = -1;
                List<SandboxEditor.PlacedIconData> list = SandboxEditor.LoadConfiguration(ref num, this.arenaSitting.GetCurrentLevel);
                for (int i = 0; i < list.Count; i++)
                {
                    this.SpawnEntity(list[i]);
                }
            }
            else
            {
                bool flag = false;
                if (this.arenaSitting.players.Count < 1)
                {
                    flag = true;
                    this.arenaSitting.players.Add(new ArenaSitting.ArenaPlayer(0));
                }
                for (int j = 0; j < this.arenaSitting.players.Count; j++)
                {
                    AbstractCreature abstractCreature = new AbstractCreature(this.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, new WorldCoordinate(0, 0, -50, 0), this.game.GetNewID());
                    this.game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
                    abstractCreature.abstractAI.SetDestinationNoPathing(new WorldCoordinate(0, -1, -1, UnityEngine.Random.Range(0, this.game.world.GetAbstractRoom(0).nodes.Length)), true);
                    (abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator = 10 + this.arenaSitting.players[j].playerNumber;
                }
                if (flag)
                {
                    this.arenaSitting.players.Clear();
                }
            }
        }

        public override void Initiate()
        {
            base.Initiate();
            if (this.PlayMode)
            {
                for (int i = 0; i < 30; i++)
                {
                    this.game.Update();
                    this.counter = 0;
                }
                base.SpawnPlayers(base.room, this.playerSpawnDens);
                base.AddHUD();
            }
            else
            {
                base.AddHUD();
            }
            this.sandboxInitiated = true;
        }

        public override void Update()
        {
            base.Update();
        }

        public void CustomGameOver()
        {
            this.winLoseGameOver = true;
            this.outsidePlayersCountAsDead = false;
            this.checkWinLose = false;
        }

        public override bool ShouldSessionEnd()
        {
            return false;
        }

        private void SpawnEntity(SandboxEditor.PlacedIconData placedIconData)
        {
            IconSymbol.IconSymbolData data = placedIconData.data;
            WorldCoordinate pos = new WorldCoordinate(0, -1, -1, -1);
            pos.x = Mathf.RoundToInt(placedIconData.pos.x / 20f);
            pos.y = Mathf.RoundToInt(placedIconData.pos.y / 20f);
            EntityID entityID = (!base.GameTypeSetup.saveCreatures) ? this.game.GetNewID() : placedIconData.ID;
            if (data.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
            {
                AbstractCreature abstractCreature = null;
                if (base.GameTypeSetup.saveCreatures)
                {
                    for (int i = 0; i < this.arenaSitting.creatures.Count; i++)
                    {
                        if (this.arenaSitting.creatures[i].creatureTemplate.type == data.critType && this.arenaSitting.creatures[i].ID == entityID)
                        {
                            abstractCreature = this.arenaSitting.creatures[i];
                            this.arenaSitting.creatures.RemoveAt(i);
                            for (int j = 0; j < 2; j++)
                            {
                                abstractCreature.state.CycleTick();
                            }
                            string creatureString = SaveState.AbstractCreatureToString(abstractCreature);
                            abstractCreature = SaveState.AbstractCreatureFromString(this.game.world, creatureString, false);
                            abstractCreature.pos = pos;
                            break;
                        }
                    }
                }
                if (abstractCreature == null)
                {
                    abstractCreature = new AbstractCreature(this.game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID);
                }
                CreatureTemplate.Type critType = data.critType;
                switch (critType)
                {
                    case CreatureTemplate.Type.TentaclePlant:
                    case CreatureTemplate.Type.PoleMimic:
                        abstractCreature.pos.x = -1;
                        abstractCreature.pos.y = -1;
                        abstractCreature.pos.abstractNode = data.intData;
                        this.game.world.GetAbstractRoom(0).entitiesInDens.Add(abstractCreature);
                        break;

                    default:
                        switch (critType)
                        {
                            case CreatureTemplate.Type.Fly:
                            case CreatureTemplate.Type.Leech:
                            case CreatureTemplate.Type.SeaLeech:
                                for (int k = 0; k < 5; k++)
                                {
                                    this.game.world.GetAbstractRoom(0).AddEntity(new AbstractCreature(this.game.world, StaticWorld.GetCreatureTemplate(data.critType), null, pos, entityID));
                                }
                                break;

                            default:
                                if (critType == CreatureTemplate.Type.Slugcat)
                                {
                                    if (this.playerSpawnDens == null)
                                    {
                                        this.playerSpawnDens = new List<int>();
                                    }
                                    this.playerSpawnDens.Add(data.intData);
                                }
                                if (critType != CreatureTemplate.Type.Spider)
                                {
                                    this.game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
                                }
                                break;
                        }
                        break;

                    case CreatureTemplate.Type.Centipede:
                        {
                            float num = 0f;
                            if (data.intData == 2)
                            {
                                num = Mathf.Lerp(0.265f, 0.55f, Mathf.Pow(Custom.ClampedRandomVariation(0.5f, 0.5f, 0.7f), 1.2f));
                            }
                            else if (data.intData == 3)
                            {
                                num = Mathf.Lerp(0.7f, 1f, Mathf.Pow(UnityEngine.Random.value, 0.6f));
                            }
                            abstractCreature.spawnData = "{" + num.ToString() + "}";
                            this.game.world.GetAbstractRoom(0).AddEntity(abstractCreature);
                            break;
                        }
                }
            }
            else
            {
                AbstractPhysicalObject.AbstractObjectType itemType = data.itemType;
                if (itemType != AbstractPhysicalObject.AbstractObjectType.Spear)
                {
                    if (itemType != AbstractPhysicalObject.AbstractObjectType.WaterNut)
                    {
                        if (itemType != AbstractPhysicalObject.AbstractObjectType.SporePlant)
                        {
                            if (itemType != AbstractPhysicalObject.AbstractObjectType.BubbleGrass)
                            {
                                if (AbstractConsumable.IsTypeConsumable(data.itemType))
                                {
                                    this.game.world.GetAbstractRoom(0).AddEntity(new AbstractConsumable(this.game.world, data.itemType, null, pos, entityID, -1, -1, null));
                                }
                                else
                                {
                                    this.game.world.GetAbstractRoom(0).AddEntity(new AbstractPhysicalObject(this.game.world, data.itemType, null, pos, entityID));
                                }
                            }
                            else
                            {
                                this.game.world.GetAbstractRoom(0).AddEntity(new BubbleGrass.AbstractBubbleGrass(this.game.world, null, pos, entityID, 1f, -1, -1, null));
                            }
                        }
                        else
                        {
                            this.game.world.GetAbstractRoom(0).AddEntity(new SporePlant.AbstractSporePlant(this.game.world, null, pos, entityID, -1, -1, null, false, true));
                        }
                    }
                    else
                    {
                        this.game.world.GetAbstractRoom(0).AddEntity(new WaterNut.AbstractWaterNut(this.game.world, null, pos, entityID, -1, -1, null, false));
                    }
                }
                else
                {
                    this.game.world.GetAbstractRoom(0).AddEntity(new AbstractSpear(this.game.world, null, pos, entityID, data.intData == 1));
                }
            }
        }

        public SandboxEditor editor;
        public bool PlayMode;
        //public SandboxOverlay overlay;
        public bool overlaySpawned;
        public List<int> playerSpawnDens;
        private bool sandboxInitiated;
        private bool winLoseGameOver;
        private bool checkWinLose;
    }
}