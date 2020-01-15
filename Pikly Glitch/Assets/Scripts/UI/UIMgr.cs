using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pikl.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Audio;
//using Pikl.Utils.Coroutines;
using TeamUtility.IO;
//using Pikl.Audio;
using Pikl.Profile;
using Pikl.Data;
using Pikl.Player;
using Pikl.States.Components;
using TMPro;

namespace Pikl.UI {
    public class UIMgr : Singleton<UIMgr> {
        public Menu CurrentMenu {
            get;
            private set;
        }

        [HideInInspector]
        //public GameSetup gs;
        public Modal modal, modalConfirm;
        public Menu mainMenu, gameUI, textRead;
        public LevelInfo[] levelTexts;

        public OpenStack<Menu> menuHistory = new OpenStack<Menu>();
        //GameSetup gameSetup;
        //AudioInfo menuMusic, /*gameMusic,*/ bossMusic, bossDeadMusic, boopSound;
        //public AudioMixer audioMixer;
        [Expandable]
        public List<Item> tempInventory = new List<Item>();

        public float _hp;
        public float _armour;
        public float _maxArmour;
        public float _maxHp;
        public bool _invulnerable;
        public float _damageMultiplier;
        AudioMixerSnapshot normalSnapshot;
        //AudioMixerSnapshot pauseSnapshot25;
        AudioMixerSnapshot pauseSnapshot50;
        //AudioMixerSnapshot pauseSnapshot75;
        AudioMixerSnapshot pauseSnapshot100;

        protected UIMgr() { }

        public override void Start() {
            Application.runInBackground = true;

            //menuMusic = new AudioInfo("Music/MenuMusic");
            //gameMusic = new AudioInfo("Music/GameMusic");
            //bossMusic = new AudioInfo("Music/BossMusic");
            //bossDeadMusic = new AudioInfo("Music/BossDeadMusic");
            //boopSound = new AudioInfo("SFX/ButtonPress");
            //normalSnapshot = audioMixer.FindSnapshot("NormalSnapshot");
            //pauseSnapshot25 = audioMixer.FindSnapshot("25% Pause");
            //pauseSnapshot50 = audioMixer.FindSnapshot("50% Pause");
            //pauseSnapshot75 = audioMixer.FindSnapshot("75% Pause");
            //pauseSnapshot100 = audioMixer.FindSnapshot("100% Pause");

            CheckScene();
            //    CurrentMenu = FileMgr.I.config.LastPlayer == "" ? profileSelect : mainMenu;

            //MessageMgr.I.AddListener<string>("GameOver", OnGameOver);
            //MessageMgr.I.AddListener("GameWin", OnGameComplete);
            MessageMgr.I.AddListener<string>("EnterScene", OnEnterScene);

            //PlayMenuMusic();

            InputAdapter.Instance.InputDeviceChanged += InputDeviceChanged;
            
            ProfileMgr.I.WakeUp();
        }

        void CheckScene() {
            if (SceneMgr.I.LoadedSceneName != null && SceneMgr.I.LoadedSceneName.Contains("Menu"))
                OpenMenu(mainMenu);
            else
                OpenMenu(gameUI);
        }

        void OnEnterScene(string scene) {
            CheckScene();
        }

        void Update() {
            if (CurrentMenu != null)
                CurrentMenu.OnUpdate();
        }

        public void OpenMenu(Menu menu) {
            if (CurrentMenu != null) {
                CurrentMenu.Close();
                CurrentMenu.gameObject.SetActive(false);
                menuHistory.Push(CurrentMenu);
            }

            CurrentMenu = menu;
            CurrentMenu.gameObject.SetActive(true);
            CurrentMenu.Open();

        }

        public void ShowModal(string message) {
            modal.Close();
            modal.Message = message;
            modal.Open();
        }

        public void ShowModalConfirm(string message) {
            modalConfirm.Close();
            modalConfirm.Message = message;
            modalConfirm.Open();
        }

        public void ModalResponse(bool response) {
            //PlayBoop();
            modal.Close();
            modal.result = response;
        }

        public void ModalConfirmResponse() {
            //PlayBoop();
            modalConfirm.Close();
            modalConfirm.result = true;
        }

        public void OnBackButtonPress() {
            //PlayBoop();

            if (modal.IsOpen) {
                modal.Close();
                modal.result = false;
            } else if(modalConfirm.IsOpen) {
                modalConfirm.Close();
                modalConfirm.result = true;
            }
             else {
                for (int i = 0; i < menuHistory.Peek().menuDepth - 1; i++)
                    menuHistory.Pop();

                OpenMenu(menuHistory.Pop());
            }
        }

        //void OnGameOver(string cause) {
        //    (gameOver as GameOver).cause = cause;
        //    OpenMenu(gameOver);
        //}

        //void OnGameComplete() {
        //    OpenMenu(gameComplete);
        //}

        //public void PlayBoop() {
        //    if (!AudioMgr.I.IsAudioPlaying(boopSound)) {
        //        AudioMgr.I.PlaySound(boopSound);
        //        boopSound.source.ignoreListenerPause = true;
        //    }
        //}
        //public void PlayMenuMusic() {
        //    if (!AudioMgr.I.IsAudioPlaying(menuMusic)) {
        //        AudioMgr.I.StopAllMusic();
        //        menuMusic = AudioMgr.I.PlaySound(menuMusic);
        //    }
        //}

        //public void PlayBossMusic() {
        //    StartCoroutine(TransitionMusic());
        //}

        //IEnumerator TransitionMusic() {
        //    pauseSnapshot100.TransitionTo(1f);
        //    yield return new WaitForSeconds(1f);

        //    AudioMgr.I.StopAllMusic();
        //    AudioMgr.I.PlaySound(bossMusic);

        //    normalSnapshot.TransitionTo(2f);

        //    while (ChunkMgr.I.bossActive) {
        //        yield return new WaitForEndOfFrame();
        //    }

        //    pauseSnapshot100.TransitionTo(1f);

        //    MusicMgr.I.SelectNewSong(false);

        //    yield return new WaitForSeconds(1f);

        //    AudioMgr.I.StopAllMusic();
        //    AudioMgr.I.PlaySound(bossDeadMusic);


        //    normalSnapshot.TransitionTo(2f);
        //}

        //public IEnumerator StartNewLevelSong() {
        //    AudioMgr.I.StopAllMusic();
        //    MusicMgr.I.PlayNewSong();
        //    yield return new WaitForFixedUpdate();
        //}

        //Coroutine pauseAudio;

        //public void PauseFilterOn() {
        //    pauseAudio = StartCoroutine(PauseAudio(true, 1f));
        //    TransitionSnapshots(pauseSnapshot50, pauseSnapshot100, 0.5f);
        //}

        //public void PauseFilterOff() {
        //    pauseAudio = StartCoroutine(PauseAudio(false, 0f));
        //    TransitionSnapshots(pauseSnapshot100, normalSnapshot, 1f);
        //}

        //IEnumerator PauseAudio(bool pause, float delay) {
        //    if (pauseAudio != null)
        //        StopCoroutine(pauseAudio);

        //    yield return StartCoroutine(CoroutineHelper.I.WaitForRealTime(delay));
        //    AudioListener.pause = pause;

        //}

        //public void OnGUI()
        //{
        //    if (CurrentMenu.name == "Game UI")
        //    {
        //        if (GUI.Button(new Rect(0, 420, 100, 80), "FilterOn"))
        //        {
        //            PauseFilterOn();
        //        }

        //        if (GUI.Button(new Rect(0, 500, 100, 80), "FilterOff"))
        //        {
        //            PauseFilterOff();
        //        }
        //    }
        //}

        // NEW COPY PASTA CODE FOR TRANSITIONS WHEN TIMESCALE = 0 (PAUSE FADE STUFF)

        //Coroutine transitionCoroutine;
        //AudioMixerSnapshot endSnapshot;

        //public void TransitionSnapshots(AudioMixerSnapshot fromSnapshot, AudioMixerSnapshot toSnapshot, float transitionTime) {
        //    EndTransition();
        //    transitionCoroutine = StartCoroutine(TransitionSnapshotsCoroutine(fromSnapshot, toSnapshot, transitionTime));
        //}

        //IEnumerator TransitionSnapshotsCoroutine(AudioMixerSnapshot fromSnapshot, AudioMixerSnapshot toSnapshot, float transitionTime) {
        //    // transition values
        //    int steps = 50;
        //    float timeStep = (transitionTime / (float)steps);
        //    float transitionPercentage = 0.0f;
        //    float startTime = 0f;

        //    // set up snapshots
        //    endSnapshot = toSnapshot;
        //    AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[] { fromSnapshot, toSnapshot };
        //    float[] weights = new float[2];

        //    // stepped-transition
        //    for (int i = 0; i < steps; i++) {
        //        transitionPercentage = ((float)i) / steps;
        //        weights[0] = 1.0f - transitionPercentage;
        //        weights[1] = transitionPercentage;
        //        audioMixer.TransitionToSnapshots(snapshots, weights, 0f);

        //        // this is required because WaitForSeconds doesn't work when Time.timescale == 0
        //        startTime = Time.realtimeSinceStartup;
        //        while (Time.realtimeSinceStartup < (startTime + timeStep)) {
        //            yield return null;
        //        }
        //    }

        //    // finalize
        //    EndTransition();
        //}

        //void EndTransition() {
        //    if ((transitionCoroutine == null) || (endSnapshot == null)) {
        //        return;
        //    }
        //    StopCoroutine(transitionCoroutine);
        //    endSnapshot.TransitionTo(0f);
        //}

        void InputDeviceChanged(InputDevice inputDevice) {
            switch(inputDevice) {
                case InputDevice.Joystick:
                    CurrentMenu.SetFocus();
                    break;
                case InputDevice.KeyboardAndMouse: break;
            }
        }

        public void HoldTemporaryItems(List<Item> items) {
            if (items  == null || items.Count == 0) return;
            
            tempInventory = new List<Item>(items);
        }

        public void HoldPlayerInfo(PlayerHealth ph) {
            if (ph == null) return;
            
            _hp = ph.HP;
            _armour = ph.Armour;
            _maxArmour = ph.maxArmour;
            _maxHp = ph.maxHp;
            _invulnerable = ph.invulnerable;
            _damageMultiplier = ph.damageMultiplier;
        }
    }

}
