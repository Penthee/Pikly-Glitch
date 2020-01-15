using Pikl.States;

namespace Pikl.Player {
    internal class Pause : PlayerState {

        internal override void Enter(StateObject _so) {
            base.Enter(_so);
            so.ar.Play("Idle");
            //so.isDead = true;
            player.fv2D.locked = true;
        }

        internal override void Exit() {
            base.Exit();
            //so.isDead = false;
            player.fv2D.locked = false;
        }

    }
}