using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pikl.Data;
using UnityEditor;

namespace Pikl.Interaction {
    [System.Serializable]
    public class InteractableObj : MonoBehaviour {
        
        public Transform interactableTransform;
        //TODO - Let InteractableObjs detect multiple things using layer
        //public LayerMask interactWith;
        public float radius;
        public bool autoInteract;

        bool hasInteracted = false;

        void Start() {
            SetTransform();
        }

        float distance;
        public virtual void Update() {
            if (autoInteract) {
                distance = Vector2.Distance(Player.Player.I.transform.position, interactableTransform.position);
                if (distance <= radius) {
                    if (!hasInteracted) {
                        Debug.Log(name + " Auto-Interacting...");
                        hasInteracted = true;
                        Interact();
                    }
                } else {
                    hasInteracted = false;
                }
            }
        }

        public virtual void Interact() {

        }

        void SetTransform() {
            if (interactableTransform == null)
                interactableTransform = transform;
        }
#if UNITY_EDITOR
        internal virtual void OnDrawGizmos() {
            SetTransform();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);

            Handles.Label(transform.position, name);
        }
#endif
    }
}