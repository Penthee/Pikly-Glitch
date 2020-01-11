using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Player {
    public class Idle : PlayerState {

        Vector3 targetPos;

        internal override void Enter(StateObject so) {
            base.Enter(so);
            if (!player.input.AimAxis)
                player.ar.Play("Idle");
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

        internal override State Update() {
            if (player.input.AimAxis)
                player.ar.Play("Aim");
            else if (!player.knife.swiping)
                player.ar.Play("Idle");

            return base.Update();
        }

        internal override State HandleInput() {
            if (player != null && player.input.MoveAxisRaw.magnitude != 0)
                return new Walk();

            if (player.aimID == 0 && player.input.AimAxis)
                player.aimID = player.StartAsync(new States.Aim(0));

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

            //if (player.input.SecondaryFire)
            //    player.secondaryID = player.StartAsync(new States.ShootSecondary(0.25f));

            //if (player.input.AutoShootAxis)
            //    player.shoot.autoShoot = !player.shoot.autoShoot;

            return base.HandleInput();
        }
    }
}
