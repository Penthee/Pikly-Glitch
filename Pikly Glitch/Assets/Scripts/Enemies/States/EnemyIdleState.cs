using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class EnemyIdleState : EnemyState {

        Vector3 targetPos;

        internal override void Enter(StateObject so) {
            base.Enter(so);
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

    }
}
