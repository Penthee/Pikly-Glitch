using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerKnife {
        public float activeTime, cooldown;
        public GameObject obj;
        [HideInInspector]
        public float lastSwipeTime;
        [HideInInspector]
        public bool swiping;

        Player _player;

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
            swiping = true;
            
            if (_player.input.MoveAxis.magnitude > 0)
                _player.ar.Play("Knife Run");
            else
                _player.ar.Play("Knife");
        }

        void Stop() {
            swiping = false;
        }
    }
}