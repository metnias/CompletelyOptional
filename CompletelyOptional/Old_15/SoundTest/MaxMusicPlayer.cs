using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Music;
using System.IO;
using RWCustom;
using UnityEngine;
using VoidSea;

namespace CompletelyOptional
{
    public class MaxMusicPlayer : MusicPlayer
    {
        public MaxMusicPlayer(ProcessManager manager) : base(manager)
        {
            this.gameObj = new GameObject("MusicMax Player");
            this.mainSongMix = 0f;
            this.droneGoalMix = 0f;
            this.musicContext = MusicPlayer.MusicContext.Menu;
        }

        public new void UpdateMusicContext(MainLoopProcess currentProcess)
        {
            this.musicContext = MusicPlayer.MusicContext.Menu;
        }

        public override void Update()
        {
            base.Update();
            if (this.song != null)
            {
                this.song.Update();
                if (this.threatTracker != null)
                {
                    if (this.threatTracker.currentThreat > this.song.fadeOutAtThreat)
                    {
                        this.song.FadeOut(200f);
                    }
                    this.droneGoalMix = this.threatTracker.recommendedDroneVolume * Mathf.Lerp(1f - this.mainSongMix, 1f, this.song.droneVolume);
                }
                else
                {
                    this.droneGoalMix = 0f;
                }
                if (this.song.FadingOut)
                {
                    this.mainSongMix = Mathf.Max(0f, this.mainSongMix - 1f / (1f + this.song.fadeOutTime));
                    if (this.mainSongMix <= 0f)
                    {
                        this.song.StopAndDestroy();
                        this.song = null;
                    }
                }
                else if (this.nextSong == null || this.nextSong.FadingOut)
                {
                    this.mainSongMix = Mathf.Min(1f, this.mainSongMix + 1f / (1f + this.song.fadeInTime));
                }
            }
            else
            {
                this.mainSongMix = Mathf.Max(0f, this.mainSongMix - 0.025f);
                if (this.threatTracker != null)
                {
                    this.droneGoalMix = this.threatTracker.recommendedDroneVolume;
                }
                else
                {
                    this.droneGoalMix = 0f;
                }
            }
            if (this.nextSong != null)
            {
                if (this.song == null)
                {
                    this.song = this.nextSong;
                    this.song.playWhenReady = true;
                    this.nextSong = null;
                    if (this.song.fadeInTime == 0f)
                    {
                        this.mainSongMix = 1f;
                    }
                    else
                    {
                        this.mainSongMix = 0f;
                    }
                }
                else
                {
                    this.mainSongMix = Mathf.Max(0f, this.mainSongMix - 0.0125f);
                    if (this.mainSongMix == 0f)
                    {
                        this.song.StopAndDestroy();
                        this.song = null;
                    }
                }
            }
            if (this.proceduralMusic != null)
            {
                if (this.nextProcedural != null)
                {
                    this.droneGoalMix = 0f;
                }
                this.proceduralMusic.Update();
                if (this.nextProcedural != null && this.proceduralMusic.volume == 0f)
                {
                    this.proceduralMusic.StopAndDestroy();
                    this.proceduralMusic = null;
                }
            }
            else if (this.nextProcedural != null)
            {
                if (this.nextProcedural != string.Empty)
                {
                    this.proceduralMusic = new ProceduralMusic(this, this.nextProcedural);
                    this.nextProcedural = null;
                }
                else
                {
                    this.proceduralMusic = null;
                }
            }
            if (this.threatTracker != null)
            {
                this.threatTracker.Update();
            }
            if (this.multiplayerDJ != null)
            {
                this.multiplayerDJ.Update();
            }
        }

        public new void NewCycleEvent()
        {
            this.hasPlayedASongThisCycle = false;
        }

        public new void DeathEvent()
        {
            if (this.song != null && this.song.stopAtDeath)
            {
                this.song.FadeOut(60f);
            }
            if (this.nextSong != null && this.nextSong.stopAtDeath)
            {
                this.nextSong = null;
            }
            if (this.threatTracker != null)
            {
                this.threatTracker.threatDeclineCounter = Math.Max(this.threatTracker.threatDeclineCounter, 400);
            }
        }

        public new void GateEvent()
        {
            if (this.song != null && this.song.stopAtGate)
            {
                this.song.FadeOut(120f);
            }
            if (this.nextSong != null && this.nextSong.stopAtGate)
            {
                this.nextSong = null;
            }
        }

        public new void FadeOutAllNonGhostSongs(float fadeOutTime)
        {
            if (this.song != null && !(this.song is GhostSong))
            {
                this.song.FadeOut(fadeOutTime);
            }
            if (this.nextSong != null && !(this.nextSong is GhostSong))
            {
                this.nextSong = null;
            }
            this.nextProcedural = string.Empty;
        }

        public void FadeOutAllSongs(float fadeOutTime)
        {
            if (this.song != null)
            {
                this.song.FadeOut(fadeOutTime);
            }
            if (this.nextSong != null)
            {
                this.nextSong = null;
            }
            this.nextProcedural = string.Empty;
        }

        public void RequestGhostSong(string ghostSongName)
        {
            if (this.song != null && this.song is GhostSong)
            {
                return;
            }
            if (this.nextSong != null && this.nextSong is GhostSong)
            {
                return;
            }
            if (!this.manager.rainWorld.setup.playMusic)
            {
                return;
            }
            Song song = new GhostSong(this, ghostSongName);
            if (this.song == null)
            {
                this.song = song;
                this.song.playWhenReady = true;
            }
            else
            {
                this.nextSong = song;
                this.nextSong.playWhenReady = false;
            }
        }

        public void RequestSSSong()
        {
            if (this.song != null && this.song is SSSong)
            {
                return;
            }
            if (this.nextSong != null && this.nextSong is SSSong)
            {
                return;
            }
            if (!this.manager.rainWorld.setup.playMusic)
            {
                return;
            }
            Song song = new SSSong(this);
            if (this.song == null)
            {
                this.song = song;
                this.song.playWhenReady = true;
            }
            else
            {
                this.nextSong = song;
                this.nextSong.playWhenReady = false;
            }
        }

        public void RequestVoidSeaMusic(VoidSeaScene scene)
        {
            if (this.song != null && this.song is VoidSeaMusic)
            {
                return;
            }
            if (this.nextSong != null && this.nextSong is VoidSeaMusic)
            {
                return;
            }
            if (!this.manager.rainWorld.setup.playMusic)
            {
                return;
            }
            this.nextProcedural = string.Empty;
            Song song = new VoidSeaMusic(this, scene);
            if (this.song == null)
            {
                this.song = song;
                this.song.playWhenReady = true;
            }
            else
            {
                this.nextSong = song;
                this.nextSong.playWhenReady = false;
            }
        }

        public new void RequestArenaSong(string songName, float fadeInTime)
        {
            return;/*
            if (this.song != null)
            {
                return;
            }
            Debug.Log("Arena song: " + songName);
            this.song = new Song(this, songName, MusicPlayer.MusicContext.Arena);
            this.mainSongMix = 0f;
            this.song.fadeInTime = fadeInTime;
            this.song.playWhenReady = true;
            this.song.baseVolume = 0.3f * this.manager.rainWorld.options.arenaMusicVolume;
            this.nextSong = null;*/
        }

        public new void GameRequestsSong(MusicEvent musicEvent)
        {
            return;
            Debug.Log("Game request song " + musicEvent.songName);
            if (this.song != null)
            {
                Debug.Log("already playing " + this.song.name);
            }
            if (!this.manager.rainWorld.setup.playMusic)
            {
                return;
            }
            if (this.song != null && (this.song.priority >= musicEvent.prio || this.song.name == musicEvent.songName))
            {
                return;
            }
            if (this.threatTracker != null)
            {
                if (musicEvent.maxThreatLevel < this.threatTracker.currentThreat)
                {
                    return;
                }
                if (this.threatTracker.ghostMode > 0f)
                {
                    return;
                }
            }
            if (this.manager.currentMainLoop.ID == ProcessManager.ProcessID.Game && (this.manager.currentMainLoop as RainWorldGame).session is StoryGameSession)
            {
                SaveState saveState = ((this.manager.currentMainLoop as RainWorldGame).session as StoryGameSession).saveState;
                for (int i = 0; i < saveState.deathPersistentSaveData.songsPlayRecords.Count; i++)
                {
                    if (saveState.deathPersistentSaveData.songsPlayRecords[i].songName == musicEvent.songName)
                    {
                        int num = saveState.cycleNumber - saveState.deathPersistentSaveData.songsPlayRecords[i].cycleLastPlayed;
                        if (num < musicEvent.cyclesRest || musicEvent.cyclesRest < 0)
                        {
                            return;
                        }
                    }
                }
            }
            if (musicEvent.oneSongPerCycle && this.hasPlayedASongThisCycle)
            {
                return;
            }
            this.hasPlayedASongThisCycle = true;
            Debug.Log("Play song " + musicEvent.songName);
            Song song = new Song(this, musicEvent.songName, MusicPlayer.MusicContext.StoryMode);
            song.fadeOutAtThreat = musicEvent.maxThreatLevel;
            song.Loop = musicEvent.loop;
            song.priority = musicEvent.prio;
            song.baseVolume = musicEvent.volume;
            song.fadeInTime = musicEvent.fadeInTime;
            song.stopAtDeath = musicEvent.stopAtDeath;
            song.stopAtGate = musicEvent.stopAtGate;
            if (musicEvent.roomsRange > -1 && this.threatTracker != null)
            {
                song.roomTransitions = musicEvent.roomsRange + this.threatTracker.roomSwitches;
            }
            if (this.song == null)
            {
                this.song = song;
                this.song.playWhenReady = true;
            }
            else
            {
                if (this.nextSong != null && (this.nextSong.priority >= musicEvent.prio || this.nextSong.name == musicEvent.songName))
                {
                    return;
                }
                this.nextSong = song;
                this.nextSong.playWhenReady = false;
            }
            if (this.manager.currentMainLoop.ID == ProcessManager.ProcessID.Game && (this.manager.currentMainLoop as RainWorldGame).session is StoryGameSession)
            {
                SaveState saveState2 = ((this.manager.currentMainLoop as RainWorldGame).session as StoryGameSession).saveState;
                for (int j = 0; j < saveState2.deathPersistentSaveData.songsPlayRecords.Count; j++)
                {
                    if (saveState2.deathPersistentSaveData.songsPlayRecords[j].songName == musicEvent.songName)
                    {
                        saveState2.deathPersistentSaveData.songsPlayRecords[j].cycleLastPlayed = saveState2.cycleNumber;
                        return;
                    }
                }
                saveState2.deathPersistentSaveData.songsPlayRecords.Add(new DeathPersistentSaveData.SongPlayRecord(musicEvent.songName, saveState2.cycleNumber));
            }
        }

        public new void MenuRequestsSong(string name, float priority, float fadeInTime)
        {
            return;
            Debug.Log("Song " + name);
            if (this.song != null && (this.song.priority >= priority || this.song.name == name))
            {
                return;
            }
            MenuOrSlideShowSong menuOrSlideShowSong = new MenuOrSlideShowSong(this, name, priority, fadeInTime);
            if (this.song == null)
            {
                this.song = menuOrSlideShowSong;
                this.song.playWhenReady = true;
            }
            else
            {
                if (this.nextSong != null && (this.nextSong.priority >= priority || this.nextSong.name == name))
                {
                    return;
                }
                this.nextSong = menuOrSlideShowSong;
                this.nextSong.playWhenReady = false;
            }
        }

        public new void GameRequestsSongStop(StopMusicEvent stopMusicEvent)
        {
            this.StopSongIfItShouldStop(stopMusicEvent, this.song);
            this.StopSongIfItShouldStop(stopMusicEvent, this.nextSong);
        }

        public new void RainRequestStopSong()
        {
            if (this.song != null)
            {
                this.song.FadeOut(40f);
            }
            if (this.nextSong != null)
            {
                this.nextSong = null;
            }
        }

        private void StopSongIfItShouldStop(StopMusicEvent stopMusicEvent, Song testSong)
        {
            if (testSong == null)
            {
                return;
            }
            if (stopMusicEvent.prio < testSong.priority)
            {
                return;
            }
            if (stopMusicEvent.type == StopMusicEvent.Type.SpecificSong && testSong.name != stopMusicEvent.songName)
            {
                return;
            }
            if (stopMusicEvent.type == StopMusicEvent.Type.AllButSpecific && testSong.name == stopMusicEvent.songName)
            {
                return;
            }
            testSong.FadeOut(stopMusicEvent.fadeOutTime);
        }

        public new void NewRegion(string newRegion)
        {
            return;
            Debug.Log("NEW MUSIC REGION: " + newRegion);
            this.nextProcedural = null;
            if (this.proceduralMusic == null)
            {
                this.proceduralMusic = new ProceduralMusic(this, newRegion);
            }
            else
            {
                this.nextProcedural = newRegion;
            }
        }

        private string musicFolderPath = string.Concat(
            Custom.RootFolderDirectory(),
            Path.DirectorySeparatorChar,
            "SoundEffects",
            Path.DirectorySeparatorChar,
            "Music",
            Path.DirectorySeparatorChar
        );

        //public GameObject gameObj;
        //public Song song;
        //public Song nextSong;
        //public float mainSongMix;
        //public float droneGoalMix;
        //public MusicPlayer.MusicContext musicContext;
        //public PlayerThreatTracker threatTracker;
        //public MultiplayerDJ multiplayerDJ;
        //public ProceduralMusic proceduralMusic;
        private string nextProcedural;
        //public bool hasPlayedASongThisCycle;
        //public enum MusicContext
    }
}