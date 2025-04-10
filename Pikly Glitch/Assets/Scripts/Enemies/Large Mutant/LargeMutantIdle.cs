﻿using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class LargeMutantIdle : EnemyState {

        internal override void Enter(StateObject so) {
            base.Enter(so);

            so.ar.Play("Idle");
        }

        internal override State Update() {
            if ((so as EnemyStateObject).sight.CanSeePlayer(Player.Player.I.t.position)) {
                return new LargeMutantChase();
            }

            return base.Update();
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

    }
}
