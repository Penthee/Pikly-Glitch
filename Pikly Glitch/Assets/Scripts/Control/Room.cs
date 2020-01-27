using System;
using System.Collections.Generic;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using NaughtyAttributes;
using UnityEngine;

namespace Pikl.Control {
    public enum RoomType { Corridor, Room }
    public enum RoomStatus { Inactive, Placed, PlacedAndValid }
    [Serializable]
    public class Room : MonoBehaviour {
        
        public RoomType type;
        [ReadOnly] public RoomStatus status;
        public List<ConnectPoint> connectPoints = new List<ConnectPoint>();
        [HideInInspector]
        public PolygonCollider2D polygonBounds;

        void Awake() {
            polygonBounds = GetComponent<PolygonCollider2D>();
        }

        [Button("Validate Overlap")] void IsOverlapButton() {
            Debug.Log(string.Format("Overlap: {0} : {1}", gameObject.name, IsOverlapping().ToString()));
        }
        
        public bool IsOverlapping() {
            Physics2D.Simulate(Time.fixedDeltaTime);
            Physics2D.SyncTransforms();
            return polygonBounds.IsTouchingLayers(1 << LayerMask.NameToLayer("Level"));
        }

        public void DisconnectAllPoints() {
            foreach(ConnectPoint cp in connectPoints) cp.Disconnect();
        }

        public void OnDrawGizmos() {
            foreach (ConnectPoint cp in connectPoints) {
                if (!cp.t)
                    continue;
                
                Vector3 position = cp.t.position;
                Gizmos.color = Color.white;
                Gizmos.DrawIcon(position, "DotPoint", true);
                Gizmos.DrawLine(position, position + cp.t.right);

                if (cp.isConnected && cp.connectedTo != null) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(position, cp.connectedTo.t.position);
                }
            }
            Gizmos.color = Color.white;
        }
    }

    [Serializable]
    public class ConnectPoint {
        public Transform t;
        [ReadOnly] public bool isConnected;
        [ReadOnly] public ConnectPoint connectedTo;
        static float minimumSpaceRequired = 10f;
        
        public ConnectPoint(Transform t) {
            this.t = t;
        }

        public bool HasSpaceInfront {

            get {
                bool levelCast = !Physics2D.Raycast(t.position, t.right, minimumSpaceRequired, 1 << LayerMask.NameToLayer("Level"));
                bool doorCast = !Physics2D.Raycast(t.position, t.right, 1, 1 << LayerMask.NameToLayer("Ground"));
                return !levelCast || !doorCast;
            }
        }

        public void Connect(ConnectPoint cp) {
            if (cp.t == null)
                return;
            
            isConnected = true;
            connectedTo = cp;

            cp.isConnected = true;
            cp.connectedTo = this;
        }

        public void Disconnect() {
            if (connectedTo != null) {
                connectedTo.connectedTo = null;
                connectedTo.isConnected = false;
            }

            isConnected = false;
            connectedTo = null;
        }
    }
}