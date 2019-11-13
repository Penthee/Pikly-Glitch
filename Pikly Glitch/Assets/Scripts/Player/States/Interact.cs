using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Utils.Shaker;
using Pikl.Interaction;
using Pikl.UI;
//using Pikl.Audio;

namespace Pikl.Player.States {
    public class Interact : PlayerState {

        InteractableObj io = null;

        public Interact(float lifetime) : base (lifetime, LifetimeAction.Drop) {
        }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            player.lastInteractTime = Time.time;

            //AudioMgr.I.PlaySound(Player.laserSound);

            io = player.input.FindClosestInteractable();

            if (io == null) {
                Debug.Log("no interactable found.");
                Exit();
            } else {
                //Debug.Log("interactable found... activating");
                io.Interact();
            }
        }

        internal override State Update() {

            return base.Update();
        }

        internal override void Exit() {
            player.interactID = 0;

            if (io && io as TerminalObj) {
                (io as TerminalObj).isOpen = false;
                (UIMgr.I.CurrentMenu as GameUI).CloseTerminal();
            }

            base.Exit();
        }
    }
}
