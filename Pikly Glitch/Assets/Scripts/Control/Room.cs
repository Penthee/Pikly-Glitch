using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Control {
    public class Room : MonoBehaviour {

        public List<Transform> connectPoints = new List<Transform>();
        public Bounds bounds;

        public void OnDrawGizmos() {
            if (bounds.size != Vector3.zero)
                Gizmos.DrawWireCube(bounds.center, bounds.size);

            foreach (Transform cp in connectPoints) {
                Vector3 position = cp.position;
                Gizmos.DrawIcon(position, "DotPoint", true);
                Gizmos.DrawLine(position, (position + cp.right));
            }
        }
    }
}