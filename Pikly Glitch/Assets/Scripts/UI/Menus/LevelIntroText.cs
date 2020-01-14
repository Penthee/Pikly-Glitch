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

        public LevelInfo levelText;
        public LevelInfo[] deathTexts;
        public Text title, text;
        
        Tween tween;

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

        public void StartScroll(LevelInfo lt) {
            levelText = lt;

            title.text = levelText.name;
            tween = DOTween.To(() => text.text, x => text.text = x, levelText.text, levelText.scrollSpeed).SetEase(Ease.Linear);
        }

        public void OnSkipButtonPress() {
            if (tween != null) {
                if (tween.IsComplete() || !tween.IsActive()) {
                    Continue();
                } else {
                    tween.Complete();
                }
            } else {
                Debug.Log("Tween was null in text scrolly what");
                Continue();
            }
        }

        void Continue() {
            text.text = string.Empty;
            SceneMgr.I.LoadScene(levelText.sceneToOpen);
        }

        public void StartDeathScroll() {
            levelText = GetDeathText();

            if (levelText == null) {
                print("Death Text NULL!!!!!"); return;
            }
            
            title.text = levelText.name;
            tween = DOTween.To(() => text.text, x => text.text = x, levelText.text, levelText.scrollSpeed).SetEase(Ease.Linear);
        }

        LevelInfo GetDeathText() {
            if (deathTexts.Length == 0) return null;
            
            return deathTexts[Random.Range(0, deathTexts.Length)];
        }
    }
}