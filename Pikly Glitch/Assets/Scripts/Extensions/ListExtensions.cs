using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Extensions {
    public static class ListExtensions {
        public static IList<T> Shuffle<T>(this IList<T> ts) {
            int count = ts.Count;
            int last = count - 1;
            for (int i = 0; i < last; ++i) {
                int r = UnityEngine.Random.Range(i, count);
                T tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
            return ts;
        }
    }
}
