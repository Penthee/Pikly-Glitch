using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;

namespace Pikl.UI {
    public class MainMenu : Menu {
        public GameObject levelSelect;
        public GameObject title;
        public GameObject panel;
        public PostProcessVolume effects;
        public Texture2D pointerCursor;
        
        //[Range(1,5)]
        //public float titleShowDelay = 3f;
        bool _showLevelSelect;

        void Start() {
        }

        public override void Open() {
            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration)
                Cursor.visible = false;

            //Cursor.SetCursor(pointerCursor, new Vector2(pointerCursor.width * 0.05f, pointerCursor.height * 0.05f), CursorMode.Auto);
            
            //Invoke(nameof(EnableTitle), titleShowDelay);
            effects = Camera.main.GetComponent<PostProcessVolume>();
            effects.enabled = true;
            base.Open();
        }

        void EnableTitle() {
            //title.SetActive(true);
            //panel.SetActive(true);
        }

        public override void Close() {
            //title.SetActive(false);
            //panel.SetActive(false);
            base.Close();
        }

        bool isAlreadyDown = false;

        public override void OnUpdate() {
            base.OnUpdate();
        }

        public void ShowLevelSelect() {
            _showLevelSelect = true;
            levelSelect.SetActive(true);
        }

        public void HideLevelSelect() {
            _showLevelSelect = false;
            levelSelect.SetActive(false);
        }

        public void ToggleLevelSelect() {
            _showLevelSelect = !_showLevelSelect;
            levelSelect.SetActive(_showLevelSelect);
        }

        public void StartNewGame(int levelIndex) {
            if (levelIndex >= 0 && levelIndex < UIMgr.I.levelTexts.Length) {
                HideLevelSelect();
                UIMgr.I.OpenMenu(UIMgr.I.textRead);
                (UIMgr.I.textRead as LevelIntroText).StartScroll(UIMgr.I.levelTexts[levelIndex], null, null);
            }
            else {
                Debug.LogWarning($"Tried to load invalid level : {levelIndex.ToString()}", this);
            }

            effects.enabled = false;
            //SceneMgr.I.LoadScene("Text Read Scene");
        }

        public void ExitGame() {
            Application.Quit();
        }
    }
}