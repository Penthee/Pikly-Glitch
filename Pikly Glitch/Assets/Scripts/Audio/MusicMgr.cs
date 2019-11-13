using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Pikl.Chunks;

namespace Pikl.Audio {
    public class MusicMgr : Singleton<MusicMgr> {
        protected MusicMgr() { }

        public List<Music> musicSettings = new List<Music>();

        public Music current;
        string previousClipName;
        
        public AudioInfo SelectNewSong(bool instantWallTransition) {
            do {
                current = musicSettings[Random.Range(0, musicSettings.Count)];
                current.audioInfo = new AudioInfo("Music/GameMusic");
                current.audioInfo.source = AudioMgr.I.GetGlobalSource();
            } while (current.audioInfo.source.clip != null && previousClipName == current.audioInfo.source.clip.name);

            if (current.audioInfo.source.clip != null)
                previousClipName = current.audioInfo.source.clip.name;

            //BgBehaviour.colours = current.bgColours;
            //AudioMgr.I.StartCoroutine(BgBehaviour.SetNewBackgroundColour(current.bgColours[Random.Range(0, current.bgColours.Length)], current.wallColour));
            //ChunkMgr.I.wallColour = current.wallColour;
            //ChunkMgr.I.StartCoroutine(ChunkMgr.I.SetWallColours());

            return current.audioInfo;
        }

        public AudioInfo PlayNewSong() {
            AudioClip mainClip = Resources.Load<AudioClip>(current.main);

            AudioInfo ai = new AudioInfo("Music/GameMusic");
            ai.source = AudioMgr.I.GetGlobalSource();
            ai.loop = true;
            AudioMgr.I.PlaySoundDirect(ai, mainClip);

            return current.audioInfo;
        }
    }

    [System.Serializable]
    public class Music {
        public AudioInfo audioInfo;
        public string main;
        public Color[] bgColours = new Color[5];
        public Color wallColour;
    }
}