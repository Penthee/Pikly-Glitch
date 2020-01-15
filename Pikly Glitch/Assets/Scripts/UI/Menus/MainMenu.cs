using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;

namespace Pikl.UI {
    public class MainMenu : Menu {
        public GameObject levelSelect;
        public GameObject title;
        public GameObject panel;
        //[Range(1,5)]
        //public float titleShowDelay = 3f;
        bool _showLevelSelect;

        void Start() {
        }

        public override void Open() {
            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration)
                Cursor.visible = false;

            //Cursor.SetCursor(crosshairCursor, new Vector2(crosshairCursor.width / 2, crosshairCursor.height / 2), CursorMode.Auto);
            
            //Invoke(nameof(EnableTitle), titleShowDelay);
            
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

        public void StartNewGame(int levelIndex) {
            if (levelIndex >= 0 && levelIndex < UIMgr.I.levelTexts.Length) {
                HideLevelSelect();
                UIMgr.I.OpenMenu(UIMgr.I.textRead);
                (UIMgr.I.textRead as LevelIntroText).StartScroll(UIMgr.I.levelTexts[levelIndex], null);
            }
            else {
                Debug.LogWarning($"Tried to load invalid level : {levelIndex.ToString()}", this);
            }

            //SceneMgr.I.LoadScene("Text Read Scene");
        }

        public void ExitGame() {
            Application.Quit();
        }
    }
}