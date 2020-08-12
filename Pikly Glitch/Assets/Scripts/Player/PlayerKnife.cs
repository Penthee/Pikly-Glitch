using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerKnife {
        public float activeTime, moveSpeedMod, cooldown;
        public GameObject obj;
        [HideInInspector]
        public float lastSwipeTime;
        [HideInInspector]
        public bool swiping;

        public bool hasShockblade;

        Player _player;
        float origForce;

        public void Init(Player player) {
            _player = player;
        }
        
        public void Update() {
            if (swiping && lastSwipeTime + activeTime < Time.time)
                Stop();
        }

        public void Swipe() {
            if (swiping) return;
            
            lastSwipeTime = Time.time;
            origForce = _player.move.force;
            _player.move.force = _player.move.force * moveSpeedMod;
            swiping = true;
            
            if (hasShockblade)
                _player.ar.Play(_player.input.MoveAxis.magnitude > 0 ? "Shockblade Run" : "Shockblade");
            else 
                _player.ar.Play(_player.input.MoveAxis.magnitude > 0 ? "Knife Run" : "Knife");
        }

        void Stop() {
            swiping = false;
            _player.move.force = origForce;
        }
    }
}