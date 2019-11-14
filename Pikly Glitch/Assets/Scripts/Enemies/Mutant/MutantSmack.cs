using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class MutantSmack : EnemyState {

        public MutantSmack(float lifetime, LifetimeAction la = LifetimeAction.Next, State nextStateOverride = null) : base(lifetime, la, nextStateOverride) { }

        Vector3 targetPos;

        internal override void Enter(StateObject so) {
            base.Enter(so);

            Vector3 pos = enemy.t.position + (enemy.t.right * enemy.meleeAttackRange);
            GameObjectMgr.I.Spawn(enemy.meleeDmgObj, pos);
            so.ar.Play("Attack");
        }

    }
}
