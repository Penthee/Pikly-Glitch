using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Pikl.Components;
using Pikl.Profile;
using DG.Tweening;

namespace Pikl.Audio {
    public class AudioMgr : Singleton<AudioMgr> {
        protected AudioMgr() { }
        
        public AudioMixer masterMixer;
        public AudioMixerGroup masterGroup, musicGroup, sfxGroup, speechGroup;
        public AudioSource[] globalSources;

        List<AudioInfo> playingAudio = new List<AudioInfo>();

        AudioMixerGroup this[string i] {
            get {
                switch(i)
                {
                    case "Music": return musicGroup;
                    case "SFX": return sfxGroup;
                    case "Speech": return speechGroup;
                }

                return null;
            }
        }

        public override void Awake() {
            EnsureReferences();

            MessageMgr.I.AddListener<string>("GameOver", OnGameOver);

            StartCoroutine(MonitorSources());

            SetLevels();

            base.Awake();
        }

        public void SetLevels() {
            if (ProfileMgr.I.profile != null) {
                SetMasterLevel(ProfileMgr.I.profile.masterVol.Value);
                SetMusicLevel(ProfileMgr.I.profile.musicVol.Value);
                SetSfxLevel(ProfileMgr.I.profile.soundVol.Value);
                SetSpeechLevel(0);
            } else {
                StartCoroutine(FadeInLevels());
                SetSpeechLevel(0);
            }
        }

        IEnumerator FadeInLevels() {
            for(int i = 0; i < 100; i ++) {
                SetMasterLevel(i * 0.01f);
                SetMusicLevel(i * 0.01f);
                SetSfxLevel(i * 0.01f);

                yield return new WaitForSeconds(0.01f);
            }
        }

        void EnsureReferences() {
            GameObject sources = Resources.Load("Audio/Global Audio Sources") as GameObject;
            sources = Instantiate(sources, Vector3.zero, Quaternion.identity) as GameObject;
            sources.transform.parent = transform;

            globalSources = sources.GetComponentsInChildren<AudioSource>();
            if (globalSources == null)
                Debug.LogWarning("There was no 'Global Audio Source' in 'Resources/Audio'!");

            if (masterMixer == null)
                masterMixer = Resources.Load("Audio/AudioMixer") as AudioMixer;
            if (masterMixer == null)
                Debug.LogWarning("There was no 'AudioMixer' in 'Resources/Audio'!");

            if (masterGroup == null)
                masterGroup = masterMixer.FindMatchingGroups("Master")[0];
            if (masterGroup == null)
                Debug.LogWarning("There was no 'Master' in 'Resources/Audio/AudioMixer'!");

            if (musicGroup == null)
                musicGroup = masterMixer.FindMatchingGroups("Music")[0];
            if (musicGroup == null)
                Debug.LogWarning("There was no 'Music' in 'Resources/Audio/AudioMixer'!");

            if (sfxGroup == null)
                sfxGroup = masterMixer.FindMatchingGroups("SFX")[0];
            if (sfxGroup == null)
                Debug.LogWarning("There was no 'SFX' in 'Resources/Audio/AudioMixer'!");

            if (speechGroup == null)
                speechGroup = masterMixer.FindMatchingGroups("Speech")[0];
            if (speechGroup == null)
                Debug.LogWarning("There was no 'AudioMixer' in 'Resources/Audio/AudioMixer'!");
        }

        void OnGameOver(string cause) {

        }

        public void SetMasterLevel(float masterLevel) {
            masterMixer.SetFloat("MasterVol", LinearToDecibel(masterLevel));
        }

        public void SetSfxLevel(float sfxLevel) {
            masterMixer.SetFloat("SfxVol", LinearToDecibel(sfxLevel));
        }

        public void SetMusicLevel(float musicLevel) {
            masterMixer.SetFloat("MusicVol", LinearToDecibel(musicLevel));
        }

        public void SetSpeechLevel(float speechLevel) {
            masterMixer.SetFloat("SpeechVol", LinearToDecibel(speechLevel));
        }

        private float LinearToDecibel(float linear) {
            float dB;

            if (linear != 0)
                dB = 20.0f * Mathf.Log10(linear);
            else
                dB = -144.0f;

            return dB;
        }

        private float DecibelToLinear(float dB) {
            float linear = Mathf.Pow(10.0f, dB / 20.0f);

            return linear;
        }

        public bool IsAudioPlaying(AudioInfo ai) {
            if (ai.source == null)
                return false;

            return playingAudio.Exists(e => e.source.GetInstanceID() == ai.source.GetInstanceID());
        }

        #region Play/Stop Sound
        public AudioInfo PlaySound(string audioName) {
            return PlaySound(new AudioInfo(audioName));
        }

        public AudioInfo PlaySound(string audioName, Transform obj) {
            return PlaySound(new AudioInfo(audioName, Vector2.zero, obj));
        }

        public AudioInfo PlaySound(string audioName, Vector2 pos) {
            return PlaySound(new AudioInfo(audioName, pos));
        }

        public AudioInfo PlaySound(AudioInfo audioInfo, Vector2 pos) {
            audioInfo.pos = pos;
            return PlaySound(audioInfo);
        }

        public AudioInfo PlaySound(AudioInfo audioInfo, Transform obj) {
            audioInfo.obj = obj;
            return PlaySound(audioInfo);
        }

        public AudioInfo PlaySound(AudioInfo audioInfo, float delay = 0) {

            string category = audioInfo.audioName.Split('/')[0];
            string audio = audioInfo.audioName.Split('/')[1];
            IEnumerable<Utils.RDS.IRDSObject> clips = AudioTables.I[category][audio].rdsResult;

            foreach (Utils.RDS.IRDSObject clip in clips) {
                PlayClip(audioInfo, (clip as Utils.RDS.RDSAudioItem).rdsClip);
            }

            return audioInfo;
        }

        public void PlaySoundDirect(AudioInfo audioInfo, AudioClip clip, double delay = 0) {
            PlayClip(audioInfo, clip, delay);
        }

        void PlayClip(AudioInfo audioInfo, AudioClip clip, double delay = 0) {
            if (audioInfo.source == null)
                audioInfo.source = GetGlobalSource();

            if (audioInfo != null && audioInfo.source != null) {
                audioInfo.source.clip = clip;
                audioInfo.source.loop = audioInfo.loop;
                audioInfo.source.outputAudioMixerGroup = this[audioInfo.audioName.Split('/')[0]];
                audioInfo.source.volume = audioInfo.volume;

                audioInfo.source.spatialBlend = audioInfo.threeD ? 1 : 0;
                audioInfo.source.minDistance = audioInfo.minDistance;
                audioInfo.source.maxDistance = audioInfo.maxDistance;

                if (audioInfo.obj != null)
                    audioInfo.source.GetComponent<FollowTarget>().target = audioInfo.obj;
                else
                    audioInfo.source.GetComponent<FollowTarget>().pos = audioInfo.pos;

                if (delay > 0)
                    audioInfo.source.PlayScheduled(UnityEngine.AudioSettings.dspTime + delay);
                else
                    audioInfo.source.Play();

                playingAudio.Add(audioInfo);
#if UNITY_EDITOR
            } else {
                Debug.Log("There was no source for the " + clip + " clip!");
#endif
            }
        }

        public void StopSound(AudioInfo audioInfo) {
            if (audioInfo.source != null) {
                playingAudio[playingAudio.IndexOf(audioInfo)].source.Stop();
                audioInfo.source.clip = null;
                //audioInfo.source = null;
                playingAudio.Remove(audioInfo);
            }
        }

        public void StopAllMusic() {
            foreach (AudioInfo ai in playingAudio.Where(e => e.source.clip != null && 
                                                             e.source.isPlaying && 
                                                             e.audioName.Split('/')[0] == "Music").ToList()) {
                StopSound(ai);
            }
        }
#endregion

        public int PlayCount(string name) {
            return playingAudio.Count(e => e.audioName == name);
        }

        public AudioSource GetGlobalSource() {
            foreach (AudioSource source in globalSources) {
                if (playingAudio.Count(e => e.source.GetInstanceID() == source.GetInstanceID()) == 0)
                    return source;
            }
#if UNITY_EDITOR
            Debug.Log("There were no global audio sources available!");
#endif
            return null;
        }

        IEnumerator MonitorSources() {
            while (true) {
                foreach (AudioInfo ai in playingAudio.Where(e => !e.paused && (e.source != null && !e.source.isPlaying)).ToList()) {
                    if (ai.source != null)
                        ai.source.clip = null;
                    ai.source = null;
                    playingAudio.Remove(ai);
                }

                //yield return new WaitForSeconds(0.05f);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}