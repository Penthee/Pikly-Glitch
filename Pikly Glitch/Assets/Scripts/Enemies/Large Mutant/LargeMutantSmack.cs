using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class LargeMutantSmack : EnemyState {

        public LargeMutantSmack() : base() { }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            so.ar.Play("Attack");
        }

    }
}
