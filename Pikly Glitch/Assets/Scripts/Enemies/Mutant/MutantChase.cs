using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;
using System.Collections.Generic;

namespace Pikl.Enemy {
    public class MutantChase : EnemyState {

        bool startedNoLOSTimer = false;
        float noLOSTime;

        Vector3 targetPos;

        internal override void Enter(StateObject so) {
            base.Enter(so);


        }

        internal override State Update() {
            enemy.fv2D.Override(Player.Player.I.t.position - so.transform.position);
            enemy.movement.Move(so.transform.right);

            if (!enemy.sight.CanSeePlayer(Player.Player.I.t.position)) {
                if (Vector2.Distance(so.transform.position, Player.Player.I.t.position) > enemy.sight.range)
                    return new MutantIdle();

                if (!startedNoLOSTimer) {
                    startedNoLOSTimer = true;
                    noLOSTime = Time.time;
                } else if (noLOSTime + enemy.sight.noLOSChaseTime < Time.time) {
                    return new MutantIdle();
                }

            } else {
                startedNoLOSTimer = false;
            }

            if (Vector2.Distance(enemy.t.position, Player.Player.I.t.position) < enemy.meleeAttackRange)
                return new MutantSmack(enemy.meleeAttackTime);


            return base.Update();
        }

        void DoPathFind() {
          
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

    }
}
