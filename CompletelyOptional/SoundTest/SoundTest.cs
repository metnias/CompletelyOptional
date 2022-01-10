using System;
using System.Linq;
using RWCustom;
using UnityEngine;
using Menu;
using Music;
using System.IO;
using System.Collections.Generic;

namespace CompletelyOptional
{
    public class SoundTest : Menu.Menu
    {
        public SoundTest(ProcessManager manager, RainWorldGame game, SoundTestOwner overlayOwner) : base(manager, ProcessManager.ProcessID.PauseMenu)
        {
            this.game = game;
            this.overlayOwner = overlayOwner;
            this.darkFade = 1f;
            this.lastDarkFade = 1f;
            this.pages.Add(new Page(this, null, "main", 0));
            this.fadeSprite = new FSprite("Futile_White", true);
            this.fadeSprite.color = new Color(0f, 0f, 0f);
            this.fadeSprite.x = game.rainWorld.screenSize.x / 2f;
            this.fadeSprite.y = game.rainWorld.screenSize.y / 2f;
            this.fadeSprite.shader = game.rainWorld.Shaders["EdgeFade"];
            Futile.stage.AddChild(this.fadeSprite);

            if (OptionMod.songNameDict != null) { return; }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            List<string> keys = new List<string>();

            //Get Songs
            string path = string.Concat(
                Custom.RootFolderDirectory(),
                Path.DirectorySeparatorChar,
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "Music",
                Path.DirectorySeparatorChar,
                "Songs",
                Path.DirectorySeparatorChar
            );
            DirectoryInfo dirSong = new DirectoryInfo(path);
            foreach (FileInfo file in dirSong.GetFiles())
            {
                string key = file.Name;
                if (key.Substring(0, 2) != "NA" && key.Substring(0, 2) != "RW") { continue; }
                if (key.Substring(key.Length - 4) == "meta") { continue; }
                key = key.Substring(0, key.Length - 4);

                if (key.Substring(0, 2) == "RW" && key.Substring(4, 1) == " ")
                {
                    key = key.Insert(3, "0");
                }

                keys.Add(key);
            }
            keys.Sort();
            for (int k = 0; k < keys.Count; k++)
            {
                string name = keys[k];
                if (name.Substring(0, 2) == "NA")
                {
                    name = name.Substring(8);
                }
                else
                {
                    name = name.Substring(3);
                    if (int.TryParse(name.Substring(0, 1), out int p))
                    {
                        name = name.Substring(5);
                        if (keys[k].Substring(3, 1) == "0")
                        {
                            keys[k] = keys[k].Remove(3, 1);
                        }
                    }
                    else
                    {
                        name = name.Replace('_', ' ');
                    }
                }
                dict.Add(name, keys[k]);
                //Debug.Log(keys[k] + "/" + name);
            }
            OptionMod.songNameDict = dict;
        }

        public void Initiate()
        {
            this.manager.soundLoader.ReleaseAllUnityAudio();
            if (this.manager.menuMic != null)
            {
                this.manager.sideProcesses.Remove(this.manager.menuMic);
                this.manager.menuMic = null;
            }
            this.playerUI = new SoundTestPlayerUI(this, this.pages[0], this.overlayOwner);
            this.pages[0].subObjects.Add(this.playerUI);
        }

        public SoundTestPlayerUI playerUI;

        public override void Update()
        {
            base.Update();
            this.lastDarkFade = this.darkFade;
            if (this.fadingOut)
            {
                this.darkFade = Mathf.Max(0f, this.darkFade - 0.0333333351f);
                if (this.darkFade == 0) { this.fadingOut = false; }
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            if (this.lastDarkFade > 0f || this.darkFade > 0f)
            {
                float num = Mathf.Lerp(this.lastDarkFade, this.darkFade, timeStacker);
                this.fadeSprite.scaleX = (this.game.rainWorld.screenSize.x * Mathf.Lerp(1.5f, 1f, num) + 2f) / 16f;
                this.fadeSprite.scaleY = (this.game.rainWorld.screenSize.y * Mathf.Lerp(2.5f, 1.5f, num) + 2f) / 16f;
                this.fadeSprite.alpha = Mathf.InverseLerp(0f, 0.9f, num);
            }
            else if (this.fadeSprite != null)
            {
                this.fadeSprite.RemoveFromContainer();
                this.fadeSprite = null;
            }
        }

        public override void ShutDownProcess()
        {
            base.ShutDownProcess();
            if (this.fadeSprite != null)
            {
                this.fadeSprite.RemoveFromContainer();
            }

            this.manager.soundLoader.ReleaseAllUnityAudio();
            this.processActive = false;
            this.manager.currentMainLoop = null;
            this.manager.soundLoader.ReleaseAllUnityAudio();
            HeavyTexturesCache.ClearRegisteredFutileAtlases();
            GC.Collect();
            Resources.UnloadUnusedAssets();
            this.manager.menuMic = new MenuMicrophone(this.manager, this.manager.soundLoader);
            this.manager.sideProcesses.Add(this.manager.menuMic);
            //base.PlaySound(SoundID.HUD_Exit_Game);

            OptionsMenuPatch.mod = true;
            if (this.manager.musicPlayer != null)
            {
                UnityEngine.GameObject.Destroy(this.manager.musicPlayer.gameObj);
                this.manager.musicPlayer.song?.StopAndDestroy();
                this.manager.musicPlayer.ShutDownProcess();
                this.manager.sideProcesses.Remove(this.manager.musicPlayer);
            }
            this.manager.musicPlayer = new Music.MusicPlayer(this.manager);
            this.manager.sideProcesses.Add(this.manager.musicPlayer);

            OptionsMenuPatch.modmenu = new ConfigMenu(this.manager);
            this.manager.currentMainLoop = OptionsMenuPatch.modmenu;
            OptionsMenuPatch.modmenu.OpenMenu();
        }

        public RainWorldGame game;
        public SoundTestOwner overlayOwner;
        public MouseDragger mouseDragger;
        private FSprite fadeSprite;
        private float darkFade;
        private float lastDarkFade;
        public bool fadingOut;

        public class MouseDragger : MenuObject
        {
            public MouseDragger(Menu.Menu menu, MenuObject owner) : base(menu, owner)
            {
            }

            private SoundTest overlay
            {
                get
                {
                    return this.menu as SoundTest;
                }
            }

            private Room room
            {
                get
                {
                    return this.overlay.game.cameras[0].room;
                }
            }

            public override void Update()
            {
                base.Update();
                if (this.room.game.pauseMenu != null)
                {
                    this.dragChunk = null;
                    return;
                }
                Vector2 vector = this.menu.mousePosition + this.overlay.game.cameras[0].pos;
                if (this.dragChunk != null)
                {
                    if (!this.menu.mouseDown || this.dragChunk.owner.slatedForDeletetion || (this.dragChunk.owner is Creature && (this.dragChunk.owner as Creature).enteringShortCut != null))
                    {
                        this.dragChunk = null;
                    }
                    else
                    {
                        this.dragChunk.vel += vector + this.dragOffset - this.dragChunk.pos;
                        this.dragChunk.pos += vector + this.dragOffset - this.dragChunk.pos;
                    }
                }
                else if (this.menu.manager.menuesMouseMode && this.menu.pressButton)
                {
                    float b = float.MaxValue;
                    for (int i = 0; i < this.room.physicalObjects.Length; i++)
                    {
                        for (int j = 0; j < this.room.physicalObjects[i].Count; j++)
                        {
                            if (!this.room.physicalObjects[i][j].slatedForDeletetion && (!(this.room.physicalObjects[i][j] is Creature) || (this.room.physicalObjects[i][j] as Creature).enteringShortCut == null))
                            {
                                for (int k = 0; k < this.room.physicalObjects[i][j].bodyChunks.Length; k++)
                                {
                                    if (Custom.DistLess(vector, this.room.physicalObjects[i][j].bodyChunks[k].pos, Mathf.Min(this.room.physicalObjects[i][j].bodyChunks[k].rad + 10f, b)))
                                    {
                                        b = Vector2.Distance(vector, this.room.physicalObjects[i][j].bodyChunks[k].pos);
                                        this.dragChunk = this.room.physicalObjects[i][j].bodyChunks[k];
                                        this.dragOffset = this.dragChunk.pos - vector;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private BodyChunk dragChunk;
            private Vector2 dragOffset;
        }
    }
}
