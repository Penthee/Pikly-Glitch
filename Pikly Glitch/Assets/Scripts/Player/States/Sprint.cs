using UnityEngine;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Player {
    public class Sprint : PlayerState {

        internal override void Enter(StateObject so) {
            base.Enter(so);
        }

        internal override void Exit() {
            player.evade.hasReleasedSinceLastEvade = true;
            base.Exit();
        }

        internal override State Update() {
            return base.Update();
        }

        internal override State FixedUpdate() {
            base.FixedUpdate();

            player.move.MoveFast(player.input.MoveAxis);

            return null;
        }

        internal override State HandleInput() {
            if (player.evade.Stamina == 0 || player.input.MoveAxisRaw.magnitude == 0)
                return new Idle();

            if (player.input.ReloadAxis) {
                (player.inventory.SelectedItem as Weapon).Reload();
                //TODO - Take player out of sprint state when reloading
            }

            if (player.input.InteractAxis) {
                var obj = player.input.FindClosestInteractable();
                if (obj)
                    player.interactID = player.StartAsync(new States.Interact(obj));
            }

            if (player.input.DropAxis)
                player.inventory.Drop();

            return base.HandleInput();
        }
    }
}