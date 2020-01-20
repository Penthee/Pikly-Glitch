using UnityEngine;

namespace Pikl.States {
    //[System.Serializable]
    //public class State : System.ICloneable {
    public class State {
        #region Fields & Initialization
        public enum LifetimeAction {
            /// <summary>
            /// Standard lifetime action, goes to either:<para/>
            /// stateOverride
            /// Next in sequence
            /// defaultSequence or defaultState
            /// </summary>
            Next,
            /// <summary>
            /// Switches to the previous state.<para/>
            /// This does not include asynchronous states.
            /// </summary>
            Revert,
            /// <summary>
            /// Switches to the last dropped asynchronous state.
            /// </summary>
            RevertAsync,
            /// <summary>
            /// Switches to defaultSequence or defaultState
            /// </summary>
            Default,
            /// <summary>
            /// Stops running the state asynchronously.
            /// </summary>
            Drop,
            /// <summary>
            /// Custom Action, override PerformLifetimeAction if you wish to use this
            /// </summary>
            Custom

        };

        internal StateObject so;
        internal State nextStateOverride;
        internal float lifetime, startTime = -1000, origLifetime, lifetimeRemainingAtPause;
        internal bool isInSequence;
        internal bool started;
        internal bool IsActive {
            get {
                return started && (lifetime == 0 || startTime + lifetime < Time.time);
            }
        }
        internal LifetimeAction lifetimeAciton;
        internal readonly int stateID;
        public State() {
            stateID = StateObjectMgr.NextStateID;
        }

        public State(float lifetime, LifetimeAction la = LifetimeAction.Next, State nextStateOverride = null) : this() {
            this.lifetime = lifetime;
            this.lifetimeAciton = la;
            this.nextStateOverride = nextStateOverride;
            stateID = StateObjectMgr.NextStateID;
        }

        internal virtual void Enter(StateObject so) {
            started = true;
            startTime = Time.time;
            origLifetime = lifetime;
            lifetime = origLifetime < 0 ? Random.Range(0f, Mathf.Abs(origLifetime)) : lifetime;
            this.so = so;
        }

        internal virtual void Exit() { }

        internal virtual void PerformLifetimeAction() {
            switch (lifetimeAciton) {
                case LifetimeAction.Next:

                    if (nextStateOverride != null)
                        so.SwitchTo(nextStateOverride);
                    else if (isInSequence)
                        so.Next();
                    else
                        so.SwitchToDefault();

                    break;
                case LifetimeAction.Drop:
                    so.StopAsync(stateID);

                    if (nextStateOverride != null)
                        so.StartAsync(nextStateOverride);

                    break;
                case LifetimeAction.Default:
                    so.SwitchToDefault();
                    break;
                case LifetimeAction.Revert:
                    so.Revert(2);
                    break;
                case LifetimeAction.RevertAsync:
                    so.StopAsync(stateID);

                    so.RevertAsync();
                    break;
            }
        }
        #endregion

        #region Update
        internal virtual State Update() {
            //State lifetime.
            //When the time is up, either go to a specified override state,
            //go to the next state in the sequence or revert to default.
            if (lifetime > 0 && startTime + lifetime < Time.time) {
                PerformLifetimeAction();
            }

            //if (PauseMgr.I.Paused) {
            //    startTime = (Time.time + lifetime) - lifetimeRemainingAtPause;
            //}

            return null;
        }

        internal virtual State FixedUpdate() {
            return null;
        }

        internal virtual State LocalTick() {
            return null;
        }

        internal virtual State GlobalTick() {
            return null;
        }
        #endregion

        #region Collision
        internal virtual State OnCollisionEnter2D(Collision2D other) {
            return null;
        }

        internal virtual State OnCollisionStay2D(Collision2D other) {
            return null;
        }

        internal virtual State OnCollisionExit2D(Collision2D other) {
            return null;
        }

        internal virtual State OnTriggerEnter2D(Collider2D other) {
            return null;
        }

        internal virtual State OnTriggerStay2D(Collider2D other) {
            return null;
        }

        internal virtual State OnTriggerExit2D(Collider2D other) {
            return null;
        }
        #endregion
    }
}