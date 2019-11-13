using System;
using UnityEngine;
using Pikl.States;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Utils.Cameras;
using Pikl.Data;

namespace Pikl.Enemy {
    public class EnemyState : State {
        public EnemyState() : base() { }
        public EnemyState(float lifetime, LifetimeAction la = LifetimeAction.Next, State nextStateOverride = null) : base(lifetime, la, nextStateOverride) { }

        internal EnemyStateObject enemy = null;

        internal override void Enter(StateObject so) {
            base.Enter(so);

            enemy = (so as EnemyStateObject);
        }

        internal override State Update() {
            return base.Update();
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

        internal override State OnTriggerEnter2D(Collider2D other) {
            //switch (other.tag) {
            //    case "EnemyDamage":
            //    case "Damage":
            //        if (other.GetComponent<DamageObject>() != null)
            //            player.health.TakeDamage(other.GetComponent<DamageObject>().damage);
            //        break;
            //}

            return base.OnTriggerEnter2D(other);
        }

        internal override State OnTriggerStay2D(Collider2D other) {

            return base.OnTriggerStay2D(other);
        }
    }
}