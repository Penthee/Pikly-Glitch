using UnityEngine;
using System.Collections;

namespace Pikl.Utils.Cameras {
    [RequireComponent(typeof(BoxCollider2D))]
    public class CameraZone : MonoBehaviour {

        public Bounds bounds;
        public bool fitZoomOnEnter = true, gizmo = true;

        new MainCamera camera;

        void Start() {
            StartCoroutine(FindCamera());
        }

        IEnumerator FindCamera() {
            do {
                GameObject c = GameObject.Find("Main Camera");

                if (c != null)
                    camera = c.GetComponent<MainCamera>();

                yield return new WaitForEndOfFrame();
            } while (camera == null);
        }

        //void OnTriggerEnter2D(Collider2D other) {
        //    if (other.gameObject.tag == "Player" && camera != null)
        //        camera.SwitchBounds(bounds, fitZoomOnEnter);
        //}

        void OnDrawGizmos() {
            if (gizmo) {
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
        
    }
}