using System.Collections.Generic;
using Pikl.States;
using Pikl.Collections;

namespace Pikl.Collections {
    /// <summary>Provides a Stack which has a limit, and when hitting that limit it drops the items at the bottom of the stack.</summary>
    public class OpenStack<T> {
        public int maxCount = 25;
        public List<T> items = new List<T>();

        /// <summary>The number of items in the stack.</summary>
        public int Count {
            get { return items.Count; }
        }

        /// <summary>Add an item to the stack, if the stack is full then the item at position 0 will be removed.</summary>
        public void Push(T item) {
            if (items.Count >= maxCount)
                items.RemoveAt(0);

            items.Add(item);
        }
        /// <summary>Returns the item at the top of the stack, but does not remove it.</summary>
        public T Peek() {
            if (items.Count > 0)
                return items[items.Count - 1];
            else
                return default(T);
        }
        /// <summary>Removes and returns the item at the top of the stack.</summary>
        public T Pop() {
            if (items.Count > 0) {
                T temp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return temp;
            } else
                return default(T);
        }

        /// <summary>Allows removal of a specific item in the stack.</summary>
        /// <param name="index">The index in the stack, 0 = bottom, Count - 1 = top.</param>
        public T Remove(int index) {
            if (items.Count > 0) {
                T temp = items[index];
                items.RemoveAt(index);
                return temp;
            } else
                return default(T);
        }
    }
}