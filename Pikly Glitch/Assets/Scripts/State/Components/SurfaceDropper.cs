using UnityEngine;
using System.Collections;

namespace Pikl.States.Components {
    [System.Serializable]
    public class SurfaceDropper : MonoBehaviour {
        public GameObject surface;
        public float distance = 1;

        Vector3 lastDropPos;

        public void Start() {
            lastDropPos = transform.position;
        }

        public void Update() {
            if (Vector2.Distance(lastDropPos, transform.position) > surface.GetComponent<SpriteRenderer>().bounds.size.x * distance) {
                lastDropPos = transform.position;
                GameObject.Instantiate(surface, transform.position, Quaternion.identity);
            }
        }
    }
}