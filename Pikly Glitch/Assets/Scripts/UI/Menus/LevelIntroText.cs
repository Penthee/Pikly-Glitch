using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;
using DG.Tweening;
using Pikl.Player;
using Pikl.States.Components;

/*
 * open menu, giving scene to open after text
 * grab text from data object
 * start scroll sequence
 * if space or A pressed
    * check if sequence has ended
        * open scene
    *skip to end of sequence
 */

namespace Pikl.UI {
    public class LevelIntroText : Menu {

        public GameObject panel;
        public LevelInfo levelText;
        public LevelInfo[] deathTexts;
        public Text title, text;
        
        Tween tween;
        float startTime;
        bool hasSkipped;
        
        void Start() {
        }

        public override void Open() {
            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration)
                Cursor.visible = false;

            //Cursor.SetCursor(crosshairCursor, new Vector2(crosshairCursor.width / 2, crosshairCursor.height / 2), CursorMode.Auto);

            base.Open();
        }


        public override void Close() {
            //Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
            base.Close();
        }

        public override void OnUpdate() {
            base.OnUpdate();
        }

        public void StartScroll(LevelInfo lt, List<Item> items, PlayerHealth ph) {
            levelText = lt;
            
            title.text = levelText.name;
            text.text = string.Empty;
            
            panel.SetActive(true);
            tween = DOTween.To(() => text.text, x => text.text = x, levelText.text, levelText.scrollSpeed).SetEase(Ease.Linear);
            startTime = Time.time;
            
            UIMgr.I.HoldTemporaryItems(items);
            UIMgr.I.HoldPlayerInfo(ph);
        }

        public void OnSkipButtonPress() {
            if (hasSkipped || startTime + levelText.scrollSpeed < Time.time) {
                Continue();
            } else {
                tween.Complete();
                hasSkipped = true;
            }
        }

        void Continue() {
            hasSkipped = false;
            text.text = string.Empty;
            panel.SetActive(false);
            SceneMgr.I.LoadScene(levelText.sceneToOpen);
        }

        public void StartDeathScroll() {
            levelText = GetDeathText();

            if (levelText == null) {
                print("Death Text NULL!!!!!"); return;
            }
            
            panel.SetActive(true);
            title.text = levelText.name;
            tween = DOTween.To(() => text.text, x => text.text = x, levelText.text, levelText.scrollSpeed).SetEase(Ease.Linear);
        }

        LevelInfo GetDeathText() {
            return deathTexts.Length == 0 ? null : deathTexts[Random.Range(0, deathTexts.Length)];
        }
    }
}