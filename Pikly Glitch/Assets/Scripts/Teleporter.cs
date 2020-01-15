using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pikl.Utils.Shaker;
using Pikl.UI;
using Pikl.Data;
using Pikl.Player;
using Pikl.States;
using static Pikl.Utils.Shaker.Shaker;

namespace Pikl {
    public class Teleporter : MonoBehaviour {

        public LevelInfo level;
        public float initialDelay = 1.5f;

        List<Item> _items = new List<Item>();
        
        Camera main;
        void Start() {
            main = Camera.main;
        }

        void Update() {

        }

        IEnumerator DoTheFancy() {
            Shaker.I.ShakeOnce(ShakePresets.Explosion);
            yield return new WaitForSeconds(initialDelay);

            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText).StartScroll(level, _items);
            
            Shaker.I.ActiveShakes.Clear();
        }

        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.name == "Player") {
                StateObject so = collision.GetComponent<StateObject>();

                if (so == null) return;
                
                so.Pause();
                
                _items = (so as Player.Player).inventory.items;

                foreach (var c in collision.gameObject.GetComponents<Collider2D>()) {
                    c.enabled = false;
                    StartCoroutine("DoTheFancy");
                }
            }
        }
    }
}