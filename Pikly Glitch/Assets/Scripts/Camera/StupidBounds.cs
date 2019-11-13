using UnityEngine;
using System.Collections;

public class StupidBounds : MonoBehaviour {

    public Bounds bounds;
    public Vector3 origCenter;

    void Start() {
        origCenter = bounds.center;
    }
    void Update() {
        bounds.center = origCenter + transform.position;
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
