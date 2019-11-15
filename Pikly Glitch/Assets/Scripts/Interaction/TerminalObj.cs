using UnityEngine;
using Pikl.Data;
using Pikl.UI;
using UnityEditor;

namespace Pikl.Interaction {
    [System.Serializable]
    public class TerminalObj : InteractableObj {

        [Expandable]
        public Terminal terminal;
        public Door doorToOpen;
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
            if (isOpen && Vector2.Distance(transform.position, Player.Player.I.t.position) > radius) {
                Close();
            }

            base.Update();
        }

        public void Open() {
            (UIMgr.I.CurrentMenu as GameUI).DisplayTerminal(terminal);

            isOpen = true;

            if (doorToOpen)
                doorToOpen.locked = false;

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

        internal override void OnDrawGizmos() {
            if (doorToOpen)
                Gizmos.DrawLine(transform.position, doorToOpen.transform.position);

            if (trapsToTrigger.Length > 0) {
                foreach (Trap t in trapsToTrigger) {
                    if (t)
                        Gizmos.DrawLine(transform.position, t.transform.position);
                }
            }

            base.OnDrawGizmos();
        }
    }
}