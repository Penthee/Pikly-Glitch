using UnityEngine;
using System.Collections;
using Pikl.States;
//using Pikl.Managers;
//using Pikl.Managers.Types;
using Pikl.Utils.Shaker;
//using Pikl.Audio;
using DG.Tweening;
using Pikl.Extensions;
//using Pikl.Utils.RDS;

namespace Pikl.Player.States {
    public class Evade : PlayerState {
        Vector2 evadeDir;
        float drag;

        //static AudioInfo boostSound = new AudioInfo("SFX/PlayerBoost");

        public Evade() : base(0, LifetimeAction.Drop) { }

        public Evade(float lifetime) : base(lifetime, LifetimeAction.Default) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            evadeDir = player.input.MoveAxisRaw.normalized;

            if (evadeDir.magnitude == 0)
                evadeDir = player.rb.velocity.normalized;

            drag = player.rb.drag;
            player.rb.drag = player.evade.drag;
            player.rb.velocity = Vector2.zero;

            player.rb.AddForce(evadeDir * player.evade.Force);

            player.evade.lastTime = Time.time;
            player.evade.Stamina = Mathf.Clamp(player.evade.Stamina - player.evade.EvadeCost, 0, player.evade.MaxStamina);

            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("EnemyDamage"), true);
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

            //AudioMgr.I.PlaySound(boostSound);

            player.fv2D.Override(evadeDir);
            player.ar.Play("Evade");

        }


        internal override void Exit() {

            player.evadeID = 0;

            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("EnemyDamage"), false);
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
            //player.evade.hasReleasedSinceLastEvade = !player.input.EvadeInput;

            player.fv2D.StopOverride();
            player.rb.drag = drag;
            base.Exit();
        }
    }
}
