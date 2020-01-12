using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pikl.Utils.Shaker;
using Pikl.UI;
using Pikl.Data;
using Pikl.States;
using static Pikl.Utils.Shaker.Shaker;

namespace Pikl {
    public class Teleporter : MonoBehaviour {

        public LevelInfo level;
        public float initialDelay = 1.5f;

        Camera main;
        void Start() {
            main = Camera.main;
        }

        void Update() {

        }

        IEnumerator DoTheFancy() {
            CameraShakeInstance s = Shaker.I.StartCameraShake(ShakePresets.Vibration);
            yield return new WaitForSeconds(initialDelay);

            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText).StartScroll(level);
            Shaker.I.StopShake(s.s);
        }

        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.name == "Player") {
                StateObject so = collision.GetComponent<StateObject>();

                if (so != null)
                    so.Pause();

                foreach (var collider in collision.gameObject.GetComponents<Collider2D>()) {
                    collider.enabled = false;
                    StartCoroutine("DoTheFancy");
                }
            }
        }
    }
}