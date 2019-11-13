using UnityEngine;
using System.Collections;
using Pikl.States.Components;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerMovement : Movement {
        public float walkForce, sprintForce;

        #region Move
        internal void MoveSlow(Vector3 dir, ForceMode2D forceMode = ForceMode2D.Force) {
            Move(dir, walkForce, forceMode);
        }

        internal void MoveFast(Vector3 dir, ForceMode2D forceMode = ForceMode2D.Force) {
            Move(dir, sprintForce, forceMode);
        }
        #endregion
    }
}