using Pikl.States;
using Pikl.Components;
using Pikl.States.Components;
using UnityEngine;

namespace Pikl.Enemy {
    public class EnemyStateObject : LivingStateObject {
        public Sight sight;
        public Movement movement;
        public MonoHealth health;

        public GameObject meleeDmgObj, rockDamageObj;
        public float meleeAttackRange, meleeAttackTime, rockThrowChance;

/*        internal override void Awake() {
            base.Awake();
        }*/

        internal override void Start() {
            sight.Init(this);
            movement.Init(this);

            pauseState = new EnemyPauseState();

            base.Start();
        }

/*        internal override void Update() {
            base.Update();
        }*/

        public void SpawnDmgObj() {
            GameObjectMgr.I.Spawn(meleeDmgObj, t.position + (t.right * meleeAttackRange));
        }


/*        void OnDrawGizmos() {
        }*/

/*        void OnDestroy() {
        }*/
    }
}