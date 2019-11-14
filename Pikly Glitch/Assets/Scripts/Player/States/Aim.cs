using UnityEngine;
using System.Collections;
using Pikl.States;

namespace Pikl.Player.States {
    public class Aim : PlayerState {
        string previousAnim;

        public Aim(string previousAnim) : base(0, LifetimeAction.Drop) {
            this.previousAnim = previousAnim;
        }

        public Aim(float lifetime) : base(lifetime, LifetimeAction.Drop) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);
            player.ar.Play("Aim");
        }

        internal override State HandleInput() {

            if (!player.input.AimInput)
                Exit();

            return base.HandleInput();
        }

        internal override void Exit() {

            switch (previousAnim) {
                case "Idle":
                case "Run":
                    player.ar.Play(previousAnim);
                    break;
                default: player.ar.Play("Idle"); break;
            }

            player.aimID = 0;

            base.Exit();
        }
    }
}
