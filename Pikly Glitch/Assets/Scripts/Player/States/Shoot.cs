using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.Utils.Shaker;
using Pikl.Data;
using Pikl.Extensions;
using Pikl.Components;
using Pikl.States.Components;
//using Pikl.Audio;

namespace Pikl.Player.States {
    public class Shoot : PlayerState {
        int counter;
        Weapon weapon;

        public Shoot(float lifetime) : base (lifetime, LifetimeAction.Drop) { }

        internal override void Enter(StateObject so) {
            base.Enter(so);

            weapon = (player.inventory.SelectedItem as Weapon);

            if (!weapon)
                Exit();

            //player.shoot.lastTime = Time.time;

            DoTheShoot();

            Shaker.I.ShakeCameraOnce(weapon.shakeOnShot);

            //AudioMgr.I.PlaySound(Player.laserSound);
            weapon.lastFireTime = Time.time;
            Exit();
        }

        void DoTheShoot() {
            for (int i = 0; i < weapon.ammoPerShot; i++) {
                if (weapon.clipAmmo <= 0) {
                    weapon.Reload();
                    Exit();
                    return;
                } else
                    SpawnShot(player.weaponSprite.transform.position.To2DXY() + weapon.shot.spawnOffset, i);
            }
        }

        void SpawnShot(Vector3 pos, int i) {
            GameObject obj = GameObjectMgr.I.Spawn(weapon.shot.stock, pos, CalcAccuracy()) as GameObject;
            DamageObject damageObj = obj.GetComponent<DamageObject>();
            ForceOnStart force = obj.GetComponent<ForceOnStart>();

            damageObj.damage = new Damage(weapon.shot.damage);
            force.minForce = weapon.shot.force;
            force.maxForce = weapon.shot.force;

            weapon.clipAmmo--;
            counter++;
        }

        Quaternion CalcAccuracy() {
            float accuracy = Random.Range(-weapon.shot.accuracy, weapon.shot.accuracy) + 90;
            return Quaternion.LookRotation(Vector3.forward, player.input.MouseDirFromWeapon.normalized.Rotate(accuracy));
        }


        internal override void Exit() {
            player.shootID = 0;
            Debug.Log("Shots spawned this state: " + counter.ToString());
            base.Exit();
        }
    }
}
