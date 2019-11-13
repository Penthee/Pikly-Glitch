using Pikl.States;

namespace Pikl.Collections {
    public class StateHistory {
        OpenStack<State> history = new OpenStack<State>();

        public void Push(State state) {
            history.Push(state);
        }

        public State Peek() {
            return history.Peek();
        }
        
        public State Pop() {
            return history.Pop();
        }

        public int Count {
            get {
                return history.Count;
            }
        }

        public int MaxCount {
            get {
                return history.maxCount;
            }
            set {
                history.maxCount = value;
            }
        }
    }
}
