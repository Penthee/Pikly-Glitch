using System.Collections.Generic;
using Pikl.States;
using Pikl.Collections;

namespace Pikl.Collections {
    /// <summary>Provides a Queue which has a limit, and when hitting that limit it drops the items at the back of the queue.</summary>
    public class OpenQueue<T> {
        public int maxCount = 250;
        public List<T> items = new List<T>();

        /// <summary>The number of items in the stack.</summary>
        public int Count {
            get { return items.Count; }
        }

        /// <summary>Add an item to the queue, if the stack is full then the item at position Count - 1 (back) will be removed.</summary>
        public void Push(T item) {
            if (items.Count >= maxCount)
                items.RemoveAt(Count - 1);

            items.Add(item);
        }
        /// <summary>Returns the item at the front of the queue, but does not remove it.</summary>
        public T Peek() {
            if (items.Count > 0)
                return items[0];
            else
                return default(T);
        }
        /// <summary>Removes and returns the item at the front of the queue.</summary>
        public T Pop() {
            if (items.Count > 0) {
                T temp = items[0];
                items.RemoveAt(0);
                return temp;
            } else
                return default(T);
        }

        /// <summary>Allows removal of a specific item in the queue.</summary>
        /// <param name="index">The index in the stack, 0 = front, Count - 1 = back.</param>
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