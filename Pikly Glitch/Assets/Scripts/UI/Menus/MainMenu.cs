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


        void Start() {
        }

        public override void Open() {
            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration)
                Cursor.visible = false;

            //Cursor.SetCursor(crosshairCursor, new Vector2(crosshairCursor.width / 2, crosshairCursor.height / 2), CursorMode.Auto);

            Debug.Log("MAIN MENU UI Open");

            base.Open();
        }

        public override void Close() {
            //Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
            base.Close();
        }

        bool isAlreadyDown = false;
        public override void OnUpdate() {

            base.OnUpdate();
        }

        public void OnNewGameClick() {
            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText).StartScroll(UIMgr.I.levelTexts[0]);
            //SceneMgr.I.LoadScene("Text Read Scene");
        }
    }
}