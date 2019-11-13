using System.Collections.Generic;
using Pikl.States;

namespace Pikl.Collections {
    public class NestedStateSequence {
        StateObject.LoopType loopType = StateObject.LoopType.Once;
        OpenQueue<StateSequence> sequences;
        short loopCount = 1;

        public NestedStateSequence() {
            sequences = new OpenQueue<StateSequence>();
        }

        public NestedStateSequence(NestedStateSequence nestedSequenceToClone) {
            sequences = new OpenQueue<StateSequence>();

            sequences.maxCount = nestedSequenceToClone.sequences.maxCount;
            LoopCount = nestedSequenceToClone.loopCount;
            LoopType = nestedSequenceToClone.loopType;

            foreach (StateSequence sequence in nestedSequenceToClone.items)
                Push(new StateSequence(sequence));
        }

        public NestedStateSequence(short loopCount, params StateSequence[] sequences) {
            this.sequences = new OpenQueue<StateSequence>();
            LoopType = StateObject.LoopType.Once;
            LoopCount = loopCount;

            foreach (StateSequence sequence in sequences)
                Push(new StateSequence(sequence));
        }

        public NestedStateSequence(StateObject.LoopType loopType, params StateSequence[] sequences) {
            this.sequences = new OpenQueue<StateSequence>();
            LoopType = loopType;

            foreach (StateSequence sequence in sequences)
                Push(new StateSequence(sequence));
        }

        public StateSequence Next() {
            return sequences.Pop();
        }

        public void Push(StateSequence sq) {
            sequences.Push(sq);
        }


        public StateSequence Peek() {
            return sequences.Peek();
        }

        public StateSequence Pop() {
            return sequences.Pop();
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
                //if (loopType == StateObject.LoopType.Once) {
                loopCount = value;
                //} else {
                //#if UNITY_EDITOR
                //                    UnityEngine.Debug.LogWarning("loop count cannot be changed if the loop type is not Once!");
                //#endif
                //}
            }
        }

        public List<StateSequence> items {
            get {
                return sequences.items;
            }
        }

        public int Count {
            get {
                return sequences.Count;
            }
        }

        public int MaxCount {
            get {
                return sequences.maxCount;
            }
            set {
                sequences.maxCount = value;
            }
        }
    }
}