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

        Player player;

        public void Init(Player player) {
            this.player = player;
        }

        public void Update() {
            if (swiping) {
                if (lastSwipeTime + activeTime < Time.time)
                    Stop();
            }
        }

        public void Swipe() {
            obj.SetActive(true);
            lastSwipeTime = Time.time;
            swiping = true;
        }

        void Stop() {
            obj.SetActive(false);
            swiping = false;
        }
    }
}