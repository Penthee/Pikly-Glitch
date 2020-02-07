using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Pikl.Data;
using Pikl.UI;
using UnityEditor;

namespace Pikl.Interaction {
    [System.Serializable]
    public class InteractableObj : MonoBehaviour {
        
        //TODO - Let InteractableObjs detect multiple things using layer
        //public LayerMask interactWith;
        public float interactRadius, labelRadius = 10;
        public bool autoInteract;

        [ReadOnly][SerializeField] bool hasInteracted = false;
        Transform _interactableTransform;

        internal virtual void Awake() {
            SetTransform();
        }
        internal virtual void Start() {
            InvokeRepeating("CheckForLabel", 1, 1);
        }

        float _distance;
        public virtual void Update() {
            if (!autoInteract) return;
            _distance = Vector2.Distance(Player.Player.I.transform.position, _interactableTransform.position);
            if (_distance <= interactRadius) {
                if (hasInteracted) return;
                Debug.Log(name + " Auto-Interacting...");
                hasInteracted = true;
                Interact();
            } else {
                hasInteracted = false;
            }
        }

        void CheckForLabel() {
            if (_distance <= labelRadius) {
                ItemLabelMgr.I.ShowLabel(name, _interactableTransform);
            } else {
                ItemLabelMgr.I.HideLabel(_interactableTransform);
            }
        }

        public virtual void Interact() {
        }

        private void OnDestroy() {
            if (ItemLabelMgr.I != null)
                ItemLabelMgr.I.HideLabel(transform);
        }

        void SetTransform() {
            if (_interactableTransform == null)
                _interactableTransform = transform;
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