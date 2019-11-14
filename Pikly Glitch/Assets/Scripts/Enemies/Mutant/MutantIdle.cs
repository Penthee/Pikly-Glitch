using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class MutantIdle : EnemyState {

        Vector3 targetPos;

        internal override void Enter(StateObject so) {
            so.ar.Play("Idle");
            base.Enter(so);
        }

        internal override State Update() {
            if ((so as EnemyStateObject).sight.CanSeePlayer(Player.Player.I.t.position)) {
                return new MutantChase();
            }

            return base.Update();
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

    }
}
