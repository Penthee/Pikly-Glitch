using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Utils.Shaker;
using DG.Tweening;
using Pikl.Profile;

namespace Pikl.Player {
    public class Dead : PlayerState {

        //GameObject front;

        internal override void Enter(StateObject _so) {
            base.Enter(_so);

            //DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.001f, 1.5f).SetEase(Ease.InSine);
            //DOTween.To(() => so.renderer.color, x => so.renderer.color = x, Color.clear, 0.3f).SetEase(Ease.InSine);
            //UI.UIMgr.I.PauseFilterOn();

            var s4 = Camera.main.GetComponent<Utils.Effects.ImageEffects.ColorCorrectionCurves>();
            DOTween.To(() => s4.saturation, x => s4.saturation = x, 0.15f, 2.5f).SetEase(Ease.InSine);

            so.isDead = true;
            player.fv2D.locked = true;

            foreach(Collider2D c2d in so.GetComponents<Collider2D>()) {
                c2d.enabled = false;
            }

            //player.ship.SetActive(false);
            
            Shaker.I.ShakeCameraOnce(ShakePresets.Death);
        }
        
        internal override void Exit() {
            base.Exit();
            so.isDead = false;
            player.fv2D.locked = false;
            so.renderer.enabled = true;
        }

        //Don't remove these - important that base state is overriden.
        internal override State Update() {
            return base.Update();
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

        internal override State HandleInput() {
            return base.HandleInput();
        }
    }
}