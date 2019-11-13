using System.Collections.Generic;
using Pikl.States;

namespace Pikl.Collections {
    public class StateSequence {
        StateObject.LoopType loopType = StateObject.LoopType.Once;
        OpenQueue<State> sequence;
        short loopCount = 1;

        public StateSequence() {
            sequence = new OpenQueue<State>();
        }

        public StateSequence(StateSequence sequenceToClone) {
            sequence = new OpenQueue<State>();
            sequence.maxCount = sequenceToClone.sequence.maxCount;

            foreach (State s in sequenceToClone.sequence.items)
                Push(s);

            LoopType = sequenceToClone.loopType;
            LoopCount = sequenceToClone.loopCount;
        }

        public StateSequence(params State[] states) : this (1, states) {}

        public StateSequence(short loopCount, params State[] states) {
            sequence = new OpenQueue<State>();
            foreach (State s in states)
                Push(s);

            LoopType = StateObject.LoopType.Once;
            LoopCount = loopCount;
        }

        public StateSequence(StateObject.LoopType loopType, params State[] states) {
            sequence = new OpenQueue<State>();
            foreach (State s in states)
                Push(s);

            LoopType = loopType;
            LoopCount = loopCount;
        }

        //public KeyValuePair<State, StateObject.LoopType> Next {
        //    get {
        //        return new KeyValuePair<State, StateObject.LoopType>(sequence.Pop(), loopType);
        //    }
        //}

        //public void Push<T>(T t) where T : State {
        //    //if (!(t is State))
        //    //    throw new System.InvalidOperationException("Type (" + t.GetType() + ") was not a state!");

        //    T s = (T)t.Clone();
        //    s.isInSequence = true;

        //    //if (s is Enemy.PatrolOnTargetState)
        //    //    UnityEngine.Debug.Log((t as Enemy.PatrolOnTargetState).moveTarget.name);

        //    sequence.Push(s);
        //}

        public void Push(State s) {
            s.isInSequence = true;
            sequence.Push(s);
        }


        public State Peek() {
            return sequence.Peek();
        }

        public State Pop() {
            return sequence.Pop();
        }

        public StateObject.LoopType LoopType {
            get {
                return loopType;
            }
            set {
                loopType = value;
                if (value != StateObject.LoopType.Once)
                    loopCount = 1;
            }
        }

        public short LoopCount {
            get {
                return loopCount;
            }
            set {
                if (loopType != StateObject.LoopType.Once) {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogWarning("loop count cannot be changed if the loop type is not Once!");
#endif
                } else {
                    loopCount = value;
                }
            }
        }

        public List<State> items {
            get {
                return sequence.items;
            }
        }

        public int Count {
            get {
                return sequence.Count;
            }
        }

        public int MaxCount {
            get {
                return sequence.maxCount;
            }
            set {
                sequence.maxCount = value;
            }
        }
    }
}