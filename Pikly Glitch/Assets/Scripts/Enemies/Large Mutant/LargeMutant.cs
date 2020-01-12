using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
//using Pikl.Extensions;

namespace Pikl.Enemy {
    public class LargeMutant : EnemyStateObject {

        internal override void Awake() {
            base.Awake();
        }

        internal override void Start() {
            defaultState = new LargeMutantIdle();
            deadState = new EnemyDeadState();
            //pauseState = new Pause();

            //health = GetComponent<PlayerHealth>();
            //health.Init(this);

            fv2D = GetComponent<FaceVector2D>();

            base.Start();
        }

        internal override void Update() {
            base.Update();
        }

        public void SpawnRockObj() {
            Vector3 pos = t.position + (t.right * meleeAttackRange);
            GameObjectMgr.I.Spawn(rockDamageObj, pos, t.rotation);
        }


        void OnDrawGizmos() {
        }

        void OnDestroy() {
        }
    }
}