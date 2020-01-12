using Pikl.States;

namespace Pikl.Player {
    internal class Pause : State {

        internal override void Enter(StateObject _so) {
            base.Enter(_so);
            so.ar.StopPlayback();
            so.isDead = true;
        }

        internal override void Exit() {
            base.Exit();
            so.ar.StartPlayback();
            so.isDead = false;
        }

    }
}