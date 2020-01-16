using UnityEngine;
using Pikl.Data;
using Pikl.UI;
using UnityEditor;

namespace Pikl.Interaction {
    [System.Serializable]
    public class TerminalObj : InteractableObj {

        [Expandable]
        public Terminal terminal;
        public Door[] doorsToOpen;
        public Trap[] trapsToTrigger;

        [HideInInspector]
        public bool isOpen = false;

        void Start() {
        }

        public override void Interact() {
            base.Interact();

            Open();
        }

        public override void Update() {
            if (isOpen && Vector2.Distance(transform.position, Player.Player.I.t.position) > interactRadius) {
                Close();
            }

            base.Update();
        }

        public void Open() {
            (UIMgr.I.CurrentMenu as GameUI).DisplayTerminal(terminal);

            isOpen = true;

            foreach(Door d in doorsToOpen)
                if (d != null)
                    d.locked = false;

            if (trapsToTrigger.Length > 0) {
                foreach(Trap t in trapsToTrigger) {
                    t.Trigger();
                }
            }
        }

        public void Close() {
            Player.Player.I.asyncStates[Player.Player.I.interactID].Exit();
            (UIMgr.I.CurrentMenu as GameUI).CloseTerminal();
            isOpen = false;
        }
#if UNITY_EDITOR
        internal override void OnDrawGizmos() {
            
            foreach(Door d in doorsToOpen)
                if (d != null)
                    Gizmos.DrawLine(transform.position, d.transform.position);

            if (trapsToTrigger.Length > 0) {
                foreach (Trap t in trapsToTrigger) {
                    if (t)
                        Gizmos.DrawLine(transform.position, t.transform.position);
                }
            }

            base.OnDrawGizmos();
        }
#endif
    }
}