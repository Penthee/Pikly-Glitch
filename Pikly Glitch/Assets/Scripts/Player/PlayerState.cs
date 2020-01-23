using System;
using UnityEngine;
using Pikl.States;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Utils.Cameras;
using Pikl.Data;
using Pikl.Components;

namespace Pikl.Player {
    public class PlayerState : State {
        internal Player player;

        public PlayerState() : base() { }
        public PlayerState(float lifetime, LifetimeAction la = LifetimeAction.Next, State nextStateOverride = null) : base(lifetime, la, nextStateOverride) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);
            player = player ?? this.so.GetComponent<Player>();
        }

        internal override State Update() {
            //ClampPositionWithinCamera();
            if (player.inventory.SelectedType == ItemType.Weapon && (player.inventory.SelectedItem as Weapon).reloading) {
                if (Time.time > (player.inventory.SelectedItem as Weapon).lastReloadTime + (player.inventory.SelectedItem as Weapon).reloadSpeed) {
                    Debug.Log("finished reloading");
                    (player.inventory.SelectedItem as Weapon).reloading = false;
                }
            }

            return base.Update();
        }

        internal override State FixedUpdate() {
            return base.FixedUpdate();
        }

        static int cycle;
        internal virtual State HandleInput() {
            if (player.input.AimAxis) {
                var r = Physics2D.Raycast(player.t.position.To2DXY(), player.input.MouseDir.normalized, player.input.MouseDir.magnitude, 1 << LayerMask.NameToLayer("Terrain"));
                Debug.DrawRay(player.t.position, player.input.MouseDir, Color.blue);
                float offsetConstraint = 1000;
                if (r) {
                    //Debug.Log("Camera limiting ray hit!");
                    offsetConstraint = Vector2.Distance(player.t.position, r.point) * 0.1125f;
                }

                Vector2 targetOffset = Vector2.zero;
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration) {
                    targetOffset = player.input.MouseDir * player.input.aimCameraDistance;
                } else {
                    targetOffset = player.input.StickDir * player.input.aimCameraDistance * 2.5f;
                }
                //Debug.Log("constraint: " + offsetConstraint + ", magnitude: " + targetOffset.magnitude);
                targetOffset = Vector2.ClampMagnitude(targetOffset, offsetConstraint);

                MainCamera.I.offset = Vector2.Lerp(MainCamera.I.offset, targetOffset, 5f * Time.deltaTime);
            } else {
                MainCamera.I.offset = Vector2.Lerp(MainCamera.I.offset, Vector2.zero, 5f * Time.deltaTime);
            }

            //EDITOR CHEATS
//#if UNITY_EDITOR
//            if (InputMgr.GetKeyDown(KeyCode.B))
//                player.shoot.secondaryAmmo[PlayerShoot.SecondaryType.Bombs] += 420;
//            if (InputMgr.GetKeyDown(KeyCode.M))
//                player.shoot.secondaryAmmo[PlayerShoot.SecondaryType.Missiles] += 420;
//            if (InputMgr.GetKeyDown(KeyCode.N))
//                player.shoot.secondaryAmmo[PlayerShoot.SecondaryType.Shockwave] += 420;
//            if (InputMgr.GetKeyDown(KeyCode.H))
//                player.health.HP = player.health.MaxHp;
//            if (InputMgr.GetKeyDown(KeyCode.I)) {
//                if (player.health.invulnerable) {
//                    player.health.Invulnerable = false;
//                } else {
//                    player.powerup.Activate(Powerup.Type.Invulnerability, 999);
//                }
//            }
//#endif

            return null;
        }

        internal override State OnTriggerEnter2D(Collider2D other) {
            switch (other.tag) {
                case "EnemyDamage":
                case "Damage":
                    if (other.GetComponent<DamageObject>() != null)
                        player.health.TakeDamage(other.GetComponent<DamageObject>().damage);
                    break;
            }

            return base.OnTriggerEnter2D(other);
        }

        internal override State OnTriggerStay2D(Collider2D other) {
            //switch (other.tag) {
            //    case "EnemyDamage":
            //    case "Damage":
            //        if (other.GetComponent<DamageObject>() != null)
            //            player.health.TakeDamage(other.GetComponent<DamageObject>().damage);
            //        break;
            //}

            return base.OnTriggerStay2D(other);
        }

        void ClampPositionWithinCamera() {
            Vector3 pos = Camera.main.WorldToViewportPoint(so.transform.position);

            pos.x = Mathf.Clamp01(pos.x);
            pos.y = Mathf.Clamp01(pos.y);

            so.transform.position = Camera.main.ViewportToWorldPoint(pos);
        }
    }
}