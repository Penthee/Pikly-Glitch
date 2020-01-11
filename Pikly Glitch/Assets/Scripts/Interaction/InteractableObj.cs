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
        public float interactRadius, labelRadius = 10;
        public bool autoInteract;

        bool hasInteracted = false;

        void Start() {
            SetTransform();
        }

        float distance;
        public virtual void Update() {
            if (autoInteract) {
                distance = Vector2.Distance(Player.Player.I.transform.position, interactableTransform.position);
                if (distance <= interactRadius) {
                    if (!hasInteracted) {
                        Debug.Log(name + " Auto-Interacting...");
                        hasInteracted = true;
                        Interact();
                    }
                } else {
                    hasInteracted = false;
                }

                if (distance <= labelRadius) {
                    ItemLabelMgr.I.CreateNewLabel(name, interactableTransform);
                } else {
                    ItemLabelMgr.I.RemoveLabel(interactableTransform);
                }
            }
        }

        public virtual void Interact() {
        }

        private void OnDestroy() {
            if (ItemLabelMgr.I != null)
                ItemLabelMgr.I.RemoveLabel(transform);
        }

        void SetTransform() {
            if (interactableTransform == null)
                interactableTransform = transform;
        }
#if UNITY_EDITOR
        internal virtual void OnDrawGizmos() {
            SetTransform();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);

            Handles.Label(transform.position, name);
        }
#endif
    }
}