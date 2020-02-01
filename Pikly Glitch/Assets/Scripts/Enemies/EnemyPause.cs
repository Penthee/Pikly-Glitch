using System;
using UnityEngine;
using Pikl.States;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Utils.Cameras;
using Pikl.Data;

namespace Pikl.Enemy {
    public class EnemyPause : EnemyState {
        public EnemyPause() : base() { }
        public EnemyPause(float lifetime, LifetimeAction la = LifetimeAction.Next, State nextStateOverride = null) : base(lifetime, la, nextStateOverride) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            enemy = (so as EnemyStateObject);
        }

    }
}