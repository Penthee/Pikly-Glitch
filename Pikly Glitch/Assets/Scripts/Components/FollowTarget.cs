using UnityEngine;

namespace Pikl.Components {
    public class FollowTarget : MonoBehaviour {

        public Transform target;
        public Vector2 pos;
        public bool copy, destroyNoTarget, recycle;
        //public bool rotation, scale;

        Vector3 lastPos;

        void Update() {
            if (target != null) {
                if (destroyNoTarget && !target.gameObject.activeInHierarchy)
                    DestroyThis();
                transform.position = Position;

                lastPos = target.transform.position;
            } else {
                if (destroyNoTarget)
                    DestroyThis();
                transform.position = pos;

            }
        }

        void DestroyThis() {
            if (recycle)
                GameObjectMgr.I.Recycle(gameObject);
            else
                Destroy(gameObject);
                
        }

        Vector3 Position {
            get {
                Vector3 pos = target.transform.position;
                if (!copy) {
                    pos = lastPos - pos;
                }

                return pos;
            }
        }
    }
}