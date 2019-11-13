using System.Collections.Generic;
using Pikl.States;

namespace Pikl.Collections {
    public class StateStack {
        public OpenStack<State> sequence = new OpenStack<State>();
        public StateObject.LoopType loopType = StateObject.LoopType.Once;

        public StateStack() { }

        public StateStack(StateObject.LoopType loopType, params State[] states) {
            foreach (State s in states)
                sequence.Push(s);

            this.loopType = loopType;
        }

        public StateStack(StateSequence sequenceToClone) {
            sequence.maxCount = sequenceToClone.MaxCount;

            foreach (State s in sequenceToClone.items)
                sequence.Push(s);

            loopType = sequenceToClone.LoopType;
        }

        public StateStack(OpenQueue<State> sequence, StateObject.LoopType loopType) {
            this.sequence.maxCount = sequence.maxCount;

            foreach (State s in sequence.items)
                this.sequence.Push(s);

            this.loopType = loopType;
        }

        public KeyValuePair<State, StateObject.LoopType> Next {
            get {
                return new KeyValuePair<State, StateObject.LoopType>(sequence.Pop(), loopType);
            }
        }

        public void Push(State s) {
            sequence.Push(s);
        }

        public State Peek() {
            return sequence.Peek();
        }

        public State Pop() {
            return sequence.Pop();
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