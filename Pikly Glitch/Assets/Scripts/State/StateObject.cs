using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using Pikl.Collections;
//using Pikl.Components;
using System.Linq;

namespace Pikl.States {
    /// <summary>
    /// StateObject is the Pikl replacement for GameObjects.
    /// It is the base class for any entity within the world that isn't completely static (like walls)
    /// should derive from, it has support for managing itself through states, which are awesome.
    /// In addition to that cool thing, it has support for slow-update (Ticks), along with sequencing states.
    /// </summary>
    public class StateObject : MonoBehaviour {
        #region Sub Classes
        #endregion

        #region Fields
        /// <summary>Returns the current state.</summary>
        //[ExposeProperty] 
        public State CurrentState {
            get {
                return state.Peek();
            }
            set {
                Debug.LogWarning("You shouldn't be setting this!");
                SwitchTo(value);
            }
        }
        /// <summary>
        /// Defines how a state sequence will loop.
        /// </summary>
        public enum LoopType {
            /// <summary>
            /// No looping, the sequence will be played once.
            /// </summary>
            Once,
            /// <summary>
            /// Normal looping, will continue to loop until interrupted by something of higher precedence.
            /// </summary>
            Normal,
            /// <summary>
            /// When each iteration of the loop is complete, the order of the sequence is reversed for the next iteration.
            /// </summary>
            PingPong
        }
        /// <summary>The amount of states that can be stored.</summary>
        [HideInInspector]
        //[Range(0, 250)]
        public int stateHistoryLength = 25, stateBufferLength = 25, stateQueueLength = 25;
        /// <summary>
        /// The speed (in seconds) at which the local tick is called, 0 is disabled.
        /// TODO - There is currently no support for enabling/disabling the local ticks during game time,
        ///        it's either on or off at object creation - fix it!. 
        /// </summary>
        public float localTickSpeed = 0;
        /// <summary>The StateHisory that hold the current and previous states/sequences.</summary>
        //[SerializeField]
        public StateHistory state = new StateHistory();
        /// <summary>The states that are still to be used in the current sequence.</summary>
        public StateSequence buffer = new StateSequence();
        /// <summary>The sequences and states waiting to be put into the buffer.</summary>
        NestedStateSequence stateQueue = new NestedStateSequence();
        /// <summary>
        /// If the current sequence is set to loop, then a copy will be put in here.
        /// The nextSequence will take precedence over the buffer and queue.
        /// </summary>
        NestedStateSequence nextSequence = new NestedStateSequence();

        /// <summary>Compulsory state that every StateObject must have.</summary>
        public State pauseState, deadState, defaultState;
        /// <summary>Optional default sequence, this will take precedence over defaultState.</summary>
        public NestedStateSequence defaultSequence = new NestedStateSequence();
        /// <summary>States in this list will be run along with any other state running</summary>
        internal Dictionary<int, State> asyncStates = new Dictionary<int, State>();
        OpenStack<State> asyncStateHistroy = new OpenStack<State>();
        //TODO - Change isPaused/isDead into properties and remove the methods.
        //Maybe isDead should be in LivingEntityStateObject, as static objects can't really die
        /// <summary>Indicates whether the StateObject is paused, this should be set in the pauseState.</summary>
        public bool isPaused;
        /// <summary>Indicates whether the StateObject is dead, this should be set in the deadState.</summary>
        //[HideInInspector]
        public bool isDead;
        [HideInInspector]
        /// <summary>
        /// False by default, when there are no more states in the buffer or queue, this is used as a fall-back sequence.
        /// This will most commonly be used when coming out of a sequence automatically using Switch().
        /// </summary>
        public bool useDefaultSequence = false;
        internal Transform t = null;
        /// <summary>The gameObject's Rigidbody2D.</summary>
        internal Rigidbody2D rb = null;
        /// <summary>The gameObject's Animator.</summary>
        internal Animator ar = null;
        /// <summary>The gameObject's SpriteRenderer.</summary>
        internal new SpriteRenderer renderer = null;
        /// <summary>The gameObject's EntityRenderer - for spriter.</summary>
        //internal EntityRenderer e_renderer;
        /// <summary>The gameObject's Collider2D.</summary>
        internal Collider2D c2d = null;
        #endregion

        #region Initialization
        /// <summary>
        /// Set the default/dead/pause states before you call base.Start().
        /// </summary>
        internal virtual void Start() {
#if UNITY_EDITOR
            VerifyCachedStates();
#endif
            CacheCommonComponents();

            buffer.MaxCount = stateBufferLength;
            nextSequence.MaxCount = stateBufferLength;
            stateQueue.MaxCount = stateQueueLength;
            state.MaxCount = stateHistoryLength;

            MessageMgr.I.AddListener("GlobalTick", OnGlobalTick);

            SwitchToDefault();

            if (localTickSpeed > 0)
                StartCoroutine(LocalTick());

        }
        
        internal virtual void Awake() {
            MessageMgr.I.AddListener("Pause", Pause);
            MessageMgr.I.AddListener("Unpause", UnPause);
        }

        /// <summary>
        /// Makes sure that the compulsory states are not null.
        /// Also warns you about the defaultSequence if it's not set.
        /// </summary>
        void VerifyCachedStates() {
            if (pauseState == null)
                Debug.LogError("Pause State not set in " + name + "!", this);

            if (deadState == null)
                Debug.LogError("Dead State not set in " + name + "!", this);

            if (defaultState == null)
                Debug.LogError("Default State not set in " + name + "!", this);

            if (defaultSequence == null)
                Debug.LogWarning("Default sequence not set in " + name + "!", this);
        }

        void CacheCommonComponents() {
            t = transform;

            rb = GetComponent<Rigidbody2D>() ?? null;

            ar = GetComponent<Animator>() ?? null;

            renderer = GetComponent<SpriteRenderer>() ?? null;

            c2d = GetComponent<Collider2D>() ?? null;

            //e_renderer = GetComponent<Spriter2UnityDX.EntityRenderer>() ?? null;

            //fv2D = GetComponent<FaceVector2D>() ?? null;

//#if UNITY_EDITOR
//            if (rb == null)
//                Debug.HBDebug.LogWarning("rigidbody2D is null", this);
//            if (ar == null)
//                Debug.HBDebug.LogWarning("animator is null.", this);
//            if (renderer == null)
//                Debug.HBDebug.LogWarning("sprite renderer is null.", this);
//            if (c2d == null)
//                Debug.HBDebug.LogWarning("collider2D is null.", this);
//#endif
        }
        #endregion

        #region Update
        State tempState;
        State[] tempAsyncStates;
        internal virtual void Update() {
            tempState = this.state.Peek().Update();
            if (tempState != null)
                SwitchTo(tempState);

            if (asyncStates.Count > 0) {
                tempAsyncStates = new State[this.asyncStates.Count];
                this.asyncStates.Values.CopyTo(tempAsyncStates, 0);
                foreach (State s in tempAsyncStates) {
                    tempState = s.Update();

                    if (tempState != null)
                        SwitchTo(tempState);
                }
            }
        }

        State tempStateFixed;
        State[] tempAsyncStatesFixed;
        internal virtual void FixedUpdate() {
            tempStateFixed = this.state.Peek().FixedUpdate();
            if (tempStateFixed != null)
                SwitchTo(tempStateFixed);

            if (asyncStates.Count > 0) {
                tempAsyncStatesFixed = new State[this.asyncStates.Count];
                this.asyncStates.Values.CopyTo(tempAsyncStatesFixed, 0);
                foreach (State s in tempAsyncStatesFixed) {
                    tempStateFixed = s.FixedUpdate();

                    if (tempStateFixed != null)
                        SwitchTo(tempStateFixed);
                }
            }
        }

        /// <summary>
        /// Runs every localTickSpeed.
        /// </summary>
        internal IEnumerator LocalTick() {
            yield return new WaitForSeconds(localTickSpeed);

            while (true) {
                State state = this.state.Peek().LocalTick();

                if (state != null)
                    SwitchTo(state);

                State[] asyncStates = new State[this.asyncStates.Count];
                this.asyncStates.Values.CopyTo(asyncStates, 0);

                foreach (State s in asyncStates)
                    s.LocalTick();

                yield return new WaitForSeconds(localTickSpeed);
            }
        }

        /// <summary>
        /// Runs every StateObjectManager.globalTickSpeed.
        /// </summary>
        internal void OnGlobalTick() {
            State state = this.state.Peek().GlobalTick();

            if (state != null)
                SwitchTo(state);

            State[] asyncStates = new State[this.asyncStates.Count];
            this.asyncStates.Values.CopyTo(asyncStates, 0);
            foreach (State s in asyncStates)
                s.GlobalTick();
        }
        #endregion

        #region Object Control
        /// <summary>Called when this object is pulled out of a pool.</summary>
        public virtual void TurnOn(Vector2 pos, Quaternion rot) {
            transform.position = pos;
            transform.rotation = rot;
            gameObject.SetActive(true);
        }

        /// <summary>Called when this object is placed in a pool.</summary>
        public virtual void TurnOff(Vector2 despawnVector) {
            transform.position = despawnVector;
            gameObject.SetActive(false);
        }

        /// <summary>Pauses the object, placing it in a pause state.</summary>
        public virtual void Pause() {
            CurrentState.lifetimeRemainingAtPause = (CurrentState.startTime + CurrentState.lifetime) - Time.time;
            SwitchTo(pauseState);
        }

        /// <summary>Un-pauses the object, switching to default.</summary>
        public virtual void UnPause() {
            SwitchToDefault();
        }
        #endregion

        #region State Control
        /// <summary>Instantly switches to a specified state.</summary>
        internal void SwitchTo(State s) {
            if (state.Peek() != null) {
                state.Peek().Exit();
                ForgetSpecialStates();
            }

            s.Enter(this);

            //if (s.isInSequence && buffer.Count == 0 && stateQueue.Count == 0 && nextSequence.Count == 0)
            //    state.PushStack(new StateStack());

            state.Push(s);

            //UnityEngine.Debug.Log(s.so.name + " switched to " + s.ToString());
        }


        /// <summary>Instantly switches to the default sequence or state, depending on useDefaultSequence.</summary>
        internal void SwitchToDefault() {
            if (useDefaultSequence)
                SwitchToNestedSequence(defaultSequence, true);
            else
                SwitchTo(defaultState);
        }


        ///<summary>Switches through the sequence given.</summary>
        internal void SwitchToSequence(StateSequence sequence) {
            if (sequence.Count == 0)
                throw new System.ArgumentException("The sequence was empty!", "sequence");

            //Make a new copy so that the references to the original aren't screwed with
            //Feed the sequence to the buffer
            buffer = new StateSequence(sequence);

            //Add the specified amount of looped sequences onto the queue
            if (sequence.LoopCount > 1)
                AddToQueue(sequence, (short)(sequence.LoopCount - 1));

            //Set up nextSequence if it's set to loop
            if (sequence.LoopType != LoopType.Once)
                SetupSequenceLoop(sequence);

            //Start the sequence
            SwitchTo(buffer.Pop());
        }

        ///<summary>Switches through the nested sequence given.</summary>
        internal void SwitchToNestedSequence(NestedStateSequence nestedSequence, bool defaultSequence = false) {
            if (nestedSequence.Count == 0)
                throw new System.ArgumentException("The nested sequence was empty!", "nestedSequence");

            //Make a new copy so that the references to the original aren't screwed with
            NestedStateSequence nsq = new NestedStateSequence(nestedSequence);

            //Switch to the first sequence
            SwitchToSequence(nsq.Pop());

            //Add the rest to the queue, ignoring the loop count because this isn't the full nsq to loop
            AddToQueue(nsq, 1);

            //Add repeated copies (loopCount) of the nestedSequence to the queue
            if (nestedSequence.LoopCount > 1)
                AddToQueue(nestedSequence);

            //Set up nextSequence if it's set to loop
            if (nestedSequence.LoopType != LoopType.Once)
                SetupSequenceLoop(nestedSequence);

            //End the default sequence with default state if it doesn't loop.
            if (defaultSequence && nestedSequence.LoopType == LoopType.Once)
                AddToQueue(new StateSequence(StateObject.LoopType.Once, defaultState));
        }

        /// <summary>Switch to the given state after the current sequence has finished it's iteration.</summary>
        internal void SwitchToAfter(State s) {
            SwitchToSequenceAfter(new StateSequence(1, s));
        }

        /// <summary>Switch to the given state sequence after the current sequence has finished it's iteration.</summary>
        internal void SwitchToSequenceAfter(StateSequence sequence) {
            if (sequence.Count == 0)
                throw new System.ArgumentException("The sequence was empty!", "sequence");

            //Add the specified amount of looped sequences onto the queue
            //This one starts at 0 to always add one - verify this is correct
            AddToQueue(sequence);
            //for (int i = 0; i < sequence.LoopCount; i++)
            //    stateQueue.Push(new StateSequence(sequence));
        }

        /// <summary>
        /// Allows additional states to be run at the same time as other states.
        /// </summary>
        internal int StartAsync(State s) {
            s.Enter(this);
            //Debug.HBDebug.Log(s.stateID + " added to async list");
            asyncStates.Add(s.stateID, s);
            return s.stateID;
        }

        /// <summary>
        /// Removes the state from the async list.
        /// </summary>
        internal bool StopAsync(int stateID) {
            if (asyncStates.ContainsKey(stateID)) {
                asyncStateHistroy.Push(asyncStates[stateID]);

                asyncStates[stateID].Exit();
                asyncStates.Remove(stateID);

                return true;
            } else {
                Debug.LogWarning(stateID + " not found!", this);
            }

            return false;
        }

        internal void StopAllAsync() {
            foreach(var a in asyncStates.ToList()) {
                asyncStates[a.Key].Exit();
                asyncStates.Remove(a.Key);
            }
        }

        /// <summary>
        /// Cancel the loop and switch to a specified state<para/>
        /// when it has finished it's iteration.<para/>
        /// If no state is given defaults will be used.
        /// </summary>.
        internal void StopLoopAfterIteration(State s = null) {
            stateQueue.items.Clear();
            nextSequence.items.Clear();

            if (s == null) {
                //if (useDefaultSequence)
                //TODO - Implement SwitchToNestedSequenceAfter
                //SwitchToNestedSequenceAfter(defaultSequence, true);
                //else
                SwitchToAfter(defaultState);
            } else {
                SwitchToAfter(s);
            }
        }


        /// <summary>
        /// Cancel the nested loop and switch to a specified state<para/>
        /// when it has finished it's iteration.
        /// If no state is given defaults will be used.<para/>
        /// </summary>.
        internal void StopNestedLoopAfterIteration(State s = null) {
            nextSequence.items.Clear();

            if (s == null) {
                //if (useDefaultSequence)
                //    SwitchToNestedSequenceAfter(defaultSequence, true);
                //else
                SwitchToAfter(defaultState);
            } else {
                SwitchToAfter(s);
            }
        }

        /// <summary>
        /// Immediately cancel all loops and switch to a specified state.<para/>
        /// If no state is given defaults will be used.
        /// </summary>
        internal void StopLoopsImmediately(bool useDefaultSequence, State s = null) {
            buffer = new StateSequence();
            nextSequence = new NestedStateSequence();
            stateQueue = new NestedStateSequence();

            if (s == null) {
                if (useDefaultSequence)
                    SwitchToNestedSequence(defaultSequence, true);
                else
                    SwitchTo(defaultState);
            } else {
                SwitchTo(s);
            }
        }

        ///<summary>
        /// Moves to the next state.
        /// The order of precedence/importance is:<para/>
        /// The buffer - The current sequence of states.<para/>
        /// The state queue - Any states given to the SwitchAfter* functions - this will interrupt the looping sequence at the end of it's iteration.<para/>
        /// nextSequence - If the current sequence is set to loop, then this will contain the next iteration of the loop.<para/>
        /// The default sequence - This will be played constantly if enabled and set to loop, or once followed by the default state.<para/>
        /// The default state - This will be used if the default sequence is disabled or and not set to loop.
        ///</summary>
        internal void Next() {
            if (buffer.Peek() != null) {
                SwitchTo(buffer.Pop());
                return;

            } else if (stateQueue.Count > 0) {
                if (stateQueue.Peek().LoopType == LoopType.Once) {
                    buffer = new StateSequence(stateQueue.Pop());
                    SwitchTo(buffer.Pop());
                } else {
                    SwitchToSequence(stateQueue.Pop());
                }
                return;

            } else if (nextSequence.Count > 0) {
                SwitchToNestedSequence(nextSequence);
                nextSequence.items.Clear();
                return;

            } else if (useDefaultSequence) {
                SwitchToNestedSequence(defaultSequence, true);
                return;

            } else {
                SwitchTo(defaultState);
                return;
            }
        }

        /// <summary>Instantly reverts to a previous state</summary>
        /// <param name = "steps" >
        /// The amount of states to travel through, must be greater than 0 and no larger than stateHistoryLength.
        /// </param>
        internal void Revert(int steps = 1) {
            //if (steps <= 0)
            //    throw new System.ArgumentOutOfRangeException("steps", "Must be greater than zero!");

            //if (steps > state.Count)
            //    throw new System.ArgumentOutOfRangeException("steps", "Cannot be larger than the state history!");

            steps = Mathf.Clamp(steps, 1, state.Count);

            for (int i = 0; i < steps; i++)
                ForgetAndExitSpecialStates();

            if (state.Peek() == null) {
                Debug.Log("There is no state in the history to revert back to!");
                return;
            }

            SwitchTo(state.Pop());
        }

        /// <summary>Instantly revert to the last dropped state</summary>
        internal void RevertAsync(int steps = 1) {
            if (steps <= 0)
                throw new System.ArgumentOutOfRangeException("steps", "Must be greater than zero!");

            if (steps > asyncStateHistroy.Count)
                throw new System.ArgumentOutOfRangeException("steps", "Cannot be larger than the state history!");

            for (int i = 0; i < steps - 1; i++)
                asyncStateHistroy.Pop();

            if (asyncStateHistroy.Peek() == null)
                throw new System.InvalidOperationException("There is no state in the history to revert back to!");

            SwitchTo(asyncStateHistroy.Pop());
        }

        void AddToQueue(StateSequence sq, short count = 0) {
            for (int i = 0; i < (count > 0 ? count : sq.LoopCount); i++)
                stateQueue.Push(new StateSequence(sq));
        }

        void AddToQueue(NestedStateSequence nsq, short count = 0) {
            for (int i = 0; i < (count > 0 ? count : nsq.LoopCount); i++) {
                foreach (StateSequence sq in nsq.items)
                    AddToQueue(sq);
            }
        }
        
        void SetupSequenceLoop(StateSequence sequence) {
            nextSequence = new NestedStateSequence(sequence.LoopType, sequence);

            if (nextSequence.LoopType == LoopType.PingPong)
                nextSequence.items.Reverse(0, nextSequence.Count);
        }

        void SetupSequenceLoop(NestedStateSequence sequence) {
            nextSequence = new NestedStateSequence(sequence);

            if (nextSequence.LoopType == LoopType.PingPong) {
                for (int i = 0; i < nextSequence.Count; i++) {
                    nextSequence.items[i].items.Reverse(0, nextSequence.items[i].Count);
                }
                nextSequence.items.Reverse(0, nextSequence.Count);
            }
            //UnityEngine.Debug.Log(nextSequence.Serialize());
        }

        /// <summary>Forgets pause and dead states, and remembers all others in the buffer.</summary>
        void ForgetAndExitSpecialStates() {
            //Forget the pause/dead states.
            if (state.Peek().GetType() == pauseState.GetType() ||
                state.Peek().GetType() == deadState.GetType()) {

                state.Peek().Exit();
                state.Pop();
            }
        }

        /// <summary>Makes sure that pause or dead are never remembered when switching.</summary>
        void ForgetSpecialStates() {
            if (pauseState != null && deadState != null) {
                if (state.Peek().GetType() == pauseState.GetType() ||
                    state.Peek().GetType() == deadState.GetType()) {
                    state.Pop();
                }
            }
        }
        #endregion
    }
}