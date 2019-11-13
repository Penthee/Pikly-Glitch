using UnityEngine;
using System.Collections;
using Pikl.States;

namespace Pikl.Enemy {
    public class EnemyDeadState : EnemyState {

        public EnemyDeadState() : base() { }

        public EnemyDeadState(float lifetime) : base(lifetime) {
            
        }

        internal override void Enter(StateObject _so) {
            base.Enter(_so);
            so.isDead = true;
            Object.Destroy(so.gameObject);
        }

        internal override void Exit() {
            base.Exit();
            so.isDead = false;
        }
    }
}
