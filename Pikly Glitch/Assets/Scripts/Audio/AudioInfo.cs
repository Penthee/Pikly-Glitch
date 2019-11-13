using Pikl.Audio;
using UnityEngine;

namespace Pikl.Audio {
    [System.Serializable]
    public class AudioInfo {
        [HideInInspector]
        public AudioSource source = null;
        public string audioName;
        [HideInInspector]
        public float volume, minDistance, maxDistance;
        [HideInInspector]
        public bool loop, threeD, paused;
        public Transform obj;
        public Vector2 pos;

        public AudioInfo(string audioName) : this(audioName, Vector2.zero) { }

        public AudioInfo(string audioName, Vector2 pos, Transform obj = null) {
            //AudioMgr.I.WakeUp();
            this.audioName = audioName;

            this.pos = pos;
            this.obj = obj;
            paused = false;
            Set();
        }

        public void Set() {
            AudioTables.I.WakeUp();

            AudioInfoSettings settings = AudioTables.I[audioName.Split('/')[0]].GetSettings(audioName.Split('/')[1]);
            
            volume = settings.volume;
            loop = settings.loop;

            threeD = settings.threeD;
            minDistance = settings.minDistance;
            maxDistance = settings.maxDistance;

        }

        public void Pause() {
            if (source != null)
                source.Pause();
            paused = true;
        }

        public void UnPause() {
            if (source != null)
                source.UnPause();
            paused = false;
        }
    }
}