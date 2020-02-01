using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Pikl.Control {
    public enum RoomType { Corridor, Room }
    public enum RoomStatus { Waiting, Active, Invalid, PlacedAndValid }
    [Serializable]
    public class Room : MonoBehaviour {
        public RoomType type;
        [ReadOnly] public RoomStatus status;
        public List<ConnectPoint> connectPoints = new List<ConnectPoint>();
        [ReadOnly] public PolygonCollider2D polygonBounds;

        [SerializeField] bool roomEnabled;
        
        void Awake() {
            polygonBounds = GetComponent<PolygonCollider2D>();
            
            foreach(ConnectPoint cp in connectPoints)
                cp.connectFails = new List<ConnectPoint>();
        }

        public void EnableRoom() {
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            roomEnabled = true;
        }

        public void DisableRoom() {
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (!child.name.Contains("ConnectPoint"))
                    child.gameObject.SetActive(false);
            }

            polygonBounds.enabled = false;
            roomEnabled = false;
        }

        public bool IsRoomEnabled() {
            return roomEnabled;
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

        public void ClearAllFails() {
            foreach (ConnectPoint point in connectPoints) {
                if (point == null || point.connectFails.Count == 0)
                    continue;
                
                point.ClearFails();
            }
        }
        
        public bool HasFailedToConnect(ConnectPoint cp) {
            foreach(ConnectPoint thisCp in connectPoints) {
                if (cp.connectFails.Contains(thisCp))
                    return true;
            }

            return false;
        }
        public void OnDrawGizmos() {
            foreach (ConnectPoint cp in connectPoints) {
                if (!cp.t)
                    continue;
                
                Vector3 position = cp.t.position;
                Gizmos.color = Color.white;
                Gizmos.DrawIcon(position, "DotPoint", true);
                Gizmos.DrawLine(position, position + cp.t.right);
                
                string label = new Regex(@"\d").Match(cp.t.name).Value;
                Handles.color = Color.white;
                Handles.Label(position,  label == string.Empty ? "0" : label, new GUIStyle { fontSize = 42 });

                if (!cp.isConnected || cp.connectedTo == null) continue;
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(position, cp.connectedTo.t.position);
            }
            Gizmos.color = Color.white;
        }
    }

    [Serializable]
    public class ConnectPoint {
        public List<ConnectPoint> connectFails = new List<ConnectPoint>();
        public Transform t;
        [ReadOnly] public bool isConnected;
        [ReadOnly][SerializeField] public ConnectPoint connectedTo;
        const float MINIMUM_SPACE_REQUIRED = 5f;

        public ConnectPoint(Transform t) {
            this.t = t;
        }
        public bool HasSpaceInfront {

            get {
                bool levelCast = !Physics2D.Raycast(t.position, -t.right, MINIMUM_SPACE_REQUIRED, 1 << LayerMask.NameToLayer("Level"));
                bool doorCast = !Physics2D.Raycast(t.position, -t.right, 1, 1 << LayerMask.NameToLayer("Ground"));
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
        public void RecordFail(ConnectPoint cp) {
            if (!cp.t) return;
            
            if (!connectFails.Contains(cp))  connectFails.Add(cp);
            if (!cp.connectFails.Contains(this))  cp.connectFails.Add(this);
        }
        public int ConnectionFailCount() {
            return connectFails.Count;
        }
        public void ClearFails() {
            connectFails.Clear();
        }
    }
}