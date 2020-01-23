using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Pikl.Utils.Shaker;
using Pikl.UI;
using Pikl.Data;
using Pikl.Player;
using Pikl.States;
using Pikl.States.Components;
using static Pikl.Utils.Shaker.Shaker;

namespace Pikl {
    public class Teleporter : MonoBehaviour {

        public LevelInfo level;
        public float initialDelay = 1.5f;

        List<Item> _items = new List<Item>();
        PlayerHealth _playerHealth;
        Camera main;
        bool started;
        
        void Start() {
            main = Camera.main;
        }

        void Update() {

        }

        IEnumerator DoTheFancy() {
            Shaker.I.ShakeOnce(ShakePresets.Explosion);
            yield return new WaitForSeconds(initialDelay);

            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText)?.StartScroll(level, _items, _playerHealth);
            
            Shaker.I.ActiveShakes.Clear();
        }

        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.name == "Player" && !started) {
                Player.Player player = collision.GetComponent<StateObject>() as Player.Player;

                player?.Pause();

                _items = player?.inventory.items;
                _playerHealth = player?.health;

                foreach (var c in collision.gameObject.GetComponents<Collider2D>()) {
                    c.enabled = false;
                }
                
                StartCoroutine("DoTheFancy");
                started = true;
            }
        }
    }
}