using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Menu;
using UnityEngine;

namespace CompletelyOptional
{
    public class SoundTestOwner : UpdatableAndDeletable, IDrawable
    {
        public SoundTestOwner(Room room, SoundingGameSession gameSession, bool fadeSprite)
        {
            this.room = room;
            this.gameSession = gameSession;
            this.fadeSprite = fadeSprite;
            this.overlay = new SoundTest(room.game.manager, room.game, this);
            gameSession.overlay = this.overlay;
            this.overlay.fadingOut = true;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            this.overlay.Update();
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!this.fadeSprite)
            {
                sLeaser.sprites = new FSprite[0];
                return;
            }
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            sLeaser.sprites[0].color = Color.black;
            sLeaser.sprites[0].isVisible = false;
            this.AddToContainer(sLeaser, rCam, null);
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            this.overlay.GrafUpdate(timeStacker);
            if (!this.fadeSprite || this.ui == null)
            {
                return;
            }
            Vector2 vector = this.ui.DrawSize(timeStacker);
            float num = Mathf.Lerp(this.ui.lastVisFac, this.ui.visFac, timeStacker);
            if (num <= 0f)
            {
                sLeaser.sprites[0].isVisible = false;
            }
            else
            {
                sLeaser.sprites[0].isVisible = true;
                sLeaser.sprites[0].x = this.ui.DrawX(timeStacker) + vector.x / 2f;
                sLeaser.sprites[0].y = this.ui.DrawY(timeStacker) + vector.y / 2f;
                sLeaser.sprites[0].scaleX = (vector.x * 2f + 700f) / 16f;
                sLeaser.sprites[0].scaleY = (vector.y * 2f + 700f) / 16f;
                sLeaser.sprites[0].alpha = 0.6f * num;
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("HUD2");
            }
            foreach (FSprite fsprite in sLeaser.sprites)
            {
                fsprite.RemoveFromContainer();
                newContatiner.AddChild(fsprite);
            }
        }

        public SoundTest overlay;
        public SoundTestPlayerUI ui;
        public SoundingGameSession gameSession;
        public bool fadeSprite;
    }
}