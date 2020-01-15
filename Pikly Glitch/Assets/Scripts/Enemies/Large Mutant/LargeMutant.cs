using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
//using Pikl.Extensions;

namespace Pikl.Enemy {
    public class LargeMutant : Mutant {

        internal override void Start() {
            base.Start();
            
            defaultState = new LargeMutantIdle();
            deadState = new EnemyDeadState();
            SwitchToDefault();
        }

        public void SpawnRockObj() {
            Vector3 pos = t.position + (t.right * meleeAttackRange);
            GameObjectMgr.I.Spawn(rockDamageObj, pos, t.rotation);
        }
    }
}