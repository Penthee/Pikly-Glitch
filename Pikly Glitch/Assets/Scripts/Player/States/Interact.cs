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

        public Interact(InteractableObj obj) : base (0, LifetimeAction.Drop) {
            io = obj;
        }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            if (io == null) {
                so.StopAsync(stateID);
                return;
            }
            
            player.lastInteractTime = Time.time;
            
            io.Interact();
        }
        
        internal override void Exit() {
            if (io && io as TerminalObj) {
                (io as TerminalObj).isOpen = false;
                (UIMgr.I.CurrentMenu as GameUI).CloseTerminal();
            }

            base.Exit();
        }
    }
}
