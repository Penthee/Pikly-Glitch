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

        //static AudioInfo boostSound = new AudioInfo("SFX/PlayerBoost");

        public Evade() : base(0, LifetimeAction.Drop) { }

        public Evade(float lifetime) : base(lifetime, LifetimeAction.Drop) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            //player.shipAR.SetBool("roll", true);
            //player.shipAR.SetInteger("index", Random.Range(0, 2));

            evadeDir = player.input.MoveAxisRaw.normalized;

            if (evadeDir.magnitude == 0)
                evadeDir = player.rb.velocity.normalized;

            player.rb.AddForce(evadeDir * player.evade.Force);

            player.evade.lastTime = Time.time;
            player.evade.Stamina = Mathf.Clamp(player.evade.Stamina - player.evade.EvadeCost, 0, player.evade.MaxStamina);

            //var script = Camera.main.GetComponent<Utils.Effects.ImageEffects.VignetteAndChromaticAberration>();
            //DOTween.To(() => script.chromaticAberration, x => script.chromaticAberration = x, 20, player.evade.Cooldown * 0.5f)
                   //.OnComplete(() => DOTween.To(() => script.chromaticAberration, x => script.chromaticAberration = x, 0, player.evade.Cooldown * 0.5f));

            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("EnemyDamage"), true);
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

            //Shaker.I.ShakeCameraOnce(ShakePresets.Evade);
            //AudioMgr.I.PlaySound(boostSound);

            //player.evade.particles.transform.rotation = Quaternion.AngleAxis(Mathf.Repeat((player.input.MoveAxisRaw).ToAngle() + 120, 360), Vector3.forward);
            //player.evade.particles.Play();

            //WaveExploPostProcessing.Get().StartIt(Camera.main.WorldToViewportPoint(player.t.position), -0.15f, 1.1f, 0.75f, 0.2f, 0.025f, Ease.Linear, 0.5f, Ease.Linear);

        }


        internal override void Exit() {
            //player.shipAR.SetBool("roll", false);

            player.evadeID = 0;

            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("EnemyDamage"), false);
            Physics2D.IgnoreLayerCollision(player.gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
            //player.evade.hasReleasedSinceLastEvade = !player.input.EvadeInput;

            base.Exit();
        }
    }
}
