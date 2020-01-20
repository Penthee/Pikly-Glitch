using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Data;

namespace Pikl.Player.States {
    public class Aim : PlayerState {
        string previousAnim;
        Weapon w;

        public Aim(string previousAnim) : base(0, LifetimeAction.Drop) {
            this.previousAnim = previousAnim;
        }

        public Aim(float lifetime) : base(lifetime, LifetimeAction.Drop) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);
            //player.ar.Play("Aim");
            w = (player.inventory.SelectedItem as Weapon);

            if (w) {
                player.weaponSprite.gameObject.SetActive(true);
                player.weaponSprite.sprite = w.sprite;
            }
        }

        internal override State HandleInput() {

            if (!player.input.AimInput)
                player.StopAsync(player.aimID);

            return base.HandleInput();
        }

        internal override void Exit() {

            //switch (previousAnim) {
            //    case "Idle":
            //    case "Run":
            //        player.ar.Play(previousAnim);
            //        break;
            //    default: player.ar.Play("Idle"); break;
            //}
            player.weaponSprite.gameObject.SetActive(false);

            //player.aimID = 0;

            base.Exit();
        }
    }
}
