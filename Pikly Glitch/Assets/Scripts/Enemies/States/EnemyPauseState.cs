using UnityEngine;
using System.Collections;
using Pikl.States;

namespace Pikl.Enemy {
    public class EnemyPauseState : EnemyState {

        public EnemyPauseState() : base() { }

        internal override void Enter(StateObject _so) {
            base.Enter(_so);
            so.ar.StopPlayback();
            so.isDead = true;
        }

        internal override void Exit() {
            base.Exit();
            so.ar.StartPlayback();
            so.isDead = false;
        }
    }
}
