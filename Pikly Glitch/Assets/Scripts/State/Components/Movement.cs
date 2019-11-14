using UnityEngine;
using System.Collections;

namespace Pikl.States.Components {
    [System.Serializable]
    public class Movement {
        //public enum Direction { Right, DownRight, Down, DownLeft, Left, UpLeft, Up, UpRight };
        public enum Direction { Up, Right, Down, Left };
        public enum MoveType { Linear, Sine };


        [HideInInspector]
        public StateObject so;
        public float force, stepTime, actionDelay, sineMagnitude = 1;
        public MoveType moveType = MoveType.Linear;
        [HideInInspector]
        public float originalForce;
        [HideInInspector]
        public Direction currentDirection;
        [HideInInspector]
        public Vector3 currentDirectionVector {
            get;
            private set;
        }
        
        public void Init(StateObject so) {
            originalForce = force;
            this.so = so;
        }

        #region Move
        /// <summary>Moves in a given direction.</summary>
        internal void Move(Vector3 dir, ForceMode2D forceMode = ForceMode2D.Force) {
            Move(dir, force, forceMode);
        }

        /// <summary>Moves in a given direction.</summary>
        internal void Move(Vector3 dir, float force, ForceMode2D forceMode = ForceMode2D.Force) {
            if (so.rb == null || dir.magnitude == 0)
                return;

            dir = Vector3.ClampMagnitude(dir, 1);

            float f = moveType == MoveType.Linear ? force : Mathf.Abs(Mathf.Sin(Time.time * sineMagnitude) * force);

            so.rb.AddForce(dir * f, forceMode);

            currentDirectionVector = so.rb.velocity.normalized;

            currentDirection = dir.y > 0 ? Direction.Up : Direction.Down;
            currentDirection = dir.x > 0 ? Direction.Right : Direction.Left;
        }
        #endregion

        #region Move To Target
        /// <summary>Moves towards a given target until within a given range.</summary>
        internal void MoveTo(Vector3 target, ForceMode2D forceMode = ForceMode2D.Force) {
            MoveTo(target, force, forceMode);
        }
        /// <summary>Moves towards a given target until within a given range.</summary>
        internal void MoveTo(Vector3 target, float force, ForceMode2D forceMode = ForceMode2D.Force) {
            if (so.rb == null)
                return;

            so.StartCoroutine(MoveToward(target, force, forceMode));
        }

        IEnumerator MoveToward(Vector3 target, float force, ForceMode2D forceMode) {
            //TODO - continually move toward a target until within collider range.
            //currentDirection = dir.x > 0 ? Direction.Right : Direction.Left;
            yield return new WaitForFixedUpdate();
        }
        #endregion

        #region Move Step
        /// <summary>Moves in a given direction, for stepTime.</summary>
        internal void MoveStep(Vector3 dir, ForceMode2D forceMode = ForceMode2D.Force) {
            MoveStep(dir, force, forceMode);
        }
        /// <summary>Moves in a given direction, for stepTime.</summary>
        internal void MoveStep(Vector3 dir, float force, ForceMode2D forceMode) {
            so.StartCoroutine(DoMoveStep(dir, force, forceMode));
        }

        IEnumerator DoMoveStep(Vector3 dir, float force, ForceMode2D forceMode) {
            float startMoveTime = Time.time;

            while (startMoveTime + stepTime > Time.time) {
                Move(dir, force, forceMode);
                yield return new WaitForFixedUpdate();
            }
        }
        #endregion
    }
}