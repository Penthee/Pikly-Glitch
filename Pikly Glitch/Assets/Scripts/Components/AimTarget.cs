using UnityEngine;
using System.Collections;

namespace HotBox.Components {
    public class AimTarget : MonoBehaviour {
        public Transform t;
        public float distance = 0.5f;

        void Update() {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            transform.position = (t.position + mousePos) * distance;
        }
    }
}