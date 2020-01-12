using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
//using Pikl.Extensions;

namespace Pikl.Enemy {
    public class Mutant : EnemyStateObject {

        internal override void Awake() {
            base.Awake();
        }

        internal override void Start() {
            defaultState = new MutantIdle();
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

        public void SpawnDmgObj() {
            Instantiate(meleeDmgObj, t.position + (t.right * meleeAttackRange), Quaternion.identity);
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
            Gizmos.color = Color.white;
        }

        void OnDestroy() {
        }
    }
}