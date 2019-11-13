using UnityEngine;
using System.Collections;

namespace Pikl.Components {
    public class FaceVector2D : MonoBehaviour {

        public Transform t {
            get;
            private set;
        }
        public Rigidbody2D rb {
            get;
            private set;
        }

        public Vector3 vector;
        public float rotateSpeed = 0.05f;
        public int leniance = 5;
        public bool rotate, lerp, gizmo, locked;
        
        public int outputRot {
            get;
            private set;
        }

        [HideInInspector]
        public Vector3 dir, lastDir;

        [HideInInspector]
        public float origRotateSpeed;

        internal virtual void Start() {
            if (rb == null)
                rb = transform.GetComponent<Rigidbody2D>();

            if (t == null)
                t = transform;

            origRotateSpeed = rotateSpeed;
        }

        bool overrideDir;
        Vector3 oDir;
        public void Override(Vector2 dir) {
            overrideDir = true;
            oDir = dir;
        }
        
        public void StopOverride() {
            overrideDir = false;
        }

        void FixedUpdate() {
            if (!locked) {
                dir = overrideDir ? oDir : GetDir();

                int angle = Mathf.RoundToInt(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                int mod = angle % 45;

                if (Mathf.Abs(mod) <= leniance) {
                    if (rotate) {
                        if (lerp)
                            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotateSpeed);
                        else
                            t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, Mathf.MoveTowardsAngle(t.eulerAngles.z, angle, rotateSpeed));
                    }

                    outputRot = angle < 0 ? ((180 + angle) / 45) + 4 : angle / 45;
                }
            }
            lastDir = dir;
        }

        internal virtual Vector3 GetDir() {
            return dir;
        }

        void OnDrawGizmos() {
            if (gizmo) {
                Vector3 to = new Vector3(transform.position.x + dir.x, transform.position.y + dir.y);
                Gizmos.DrawLine(transform.position, to);
            }
        }
    }
}