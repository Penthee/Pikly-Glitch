using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using UnityEngine;

namespace Pikl.Control {
    public enum RoomType { Corridor, Room }
    [Serializable]
    public class Room : MonoBehaviour {
        
        public RoomType type;
        public bool isPlaced;
        public List<ConnectPoint> connectPoints = new List<ConnectPoint>();
        [HideInInspector]
        public PolygonCollider2D polygonBounds;

        void Awake() {
            polygonBounds = GetComponent<PolygonCollider2D>();
        }
        [Button("Validate Overlap")] public bool IsOverlapping() {
            Physics2D.Simulate(Time.fixedDeltaTime);
            Physics2D.SyncTransforms();
            bool overlap = polygonBounds.IsTouchingLayers(1 << LayerMask.NameToLayer("Level"));
            Debug.Log(string.Format("Validating {0} : {1}", gameObject.name, overlap.ToString()));
            return overlap;
        }

        public void OnDrawGizmos() {
            foreach (ConnectPoint cp in connectPoints) {
                if (!cp.t)
                    continue;
                
                Vector3 position = cp.t.position;
                Gizmos.DrawIcon(position, "DotPoint", true);
                Gizmos.DrawLine(position, position + cp.t.right);
            }
        }
    }

    [Serializable]
    public class ConnectPoint {
        public Transform t;
        [ReadOnly] public bool isConnected;

        static float minimumSpaceRequired = 10f;
        
        public ConnectPoint(Transform t) {
            this.t = t;
        }

        public bool HasSpaceInfront => !Physics2D.Raycast(t.position, t.right, minimumSpaceRequired,
            1 << LayerMask.NameToLayer("Level"));
    }
}