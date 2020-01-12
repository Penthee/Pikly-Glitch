using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Enemy {
    public class LargeMutantThrow : EnemyState {

        public LargeMutantThrow() : base() { }


        internal override void Enter(StateObject so) {
            base.Enter(so);

            so.ar.Play("Throw");
        }

    }
}
