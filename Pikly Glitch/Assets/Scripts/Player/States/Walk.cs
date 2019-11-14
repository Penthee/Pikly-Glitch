using UnityEngine;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Player {
    public class Walk : PlayerState {

        internal override void Enter(StateObject so) {
            base.Enter(so);
            if (!player.input.AimAxis)
                player.ar.Play("Run");
        }

        internal override void Exit() {
            base.Exit();
        }

        internal override State Update() {
            return base.Update();
        }

        internal override State FixedUpdate() {
            base.FixedUpdate();

            if (player.input.AimAxis) {
                player.move.MoveSlow(player.input.MoveAxis);
                player.ar.Play("Aim");
            } else {
                player.move.Move(player.input.MoveAxis);
                if (!player.knife.swiping)
                    player.ar.Play("Run");
            }

            return null;
        }

        internal override State HandleInput() {
            if (player != null && player.input.MoveAxisRaw.magnitude == 0)
                return new Idle();

            if (player.aimID == 0 && player.input.AimAxis)
                player.aimID = player.StartAsync(new States.Aim(0));

            if (player.input.EvadeAxis)
                player.evadeID = player.StartAsync(new States.Evade(player.evade.Cooldown));

            if (player.input.ShootAxis())
                player.shootID = player.StartAsync(new States.Shoot((player.inventory.SelectedItem as Weapon).fireRate));

            if (player.input.ReloadAxis)
                (player.inventory.SelectedItem as Weapon).Reload();

            if (player.input.InteractAxis)
                player.interactID = player.StartAsync(new States.Interact(0));

            if (player.input.UseAxis) {
                switch (player.inventory.SelectedItem.type) {
                    case ItemType.Consumable:
                        (player.inventory.SelectedItem as Consumable).Use();
                        break;
                    case ItemType.Throwable:
                        (player.inventory.SelectedItem as Throwable).Use();
                        break;
                }

                return base.HandleInput();
            }

            if (player.input.DropAxis)
                player.inventory.Drop();

            if (player.input.SwipeAxis)
                player.knife.Swipe();

            if (player.input.CraftingAxis)
                (UI.UIMgr.I.gameUI as UI.GameUI).ToggleCraftingUI();

            return base.HandleInput();
        }
    }
}