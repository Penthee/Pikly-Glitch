using Pikl;
using Pikl.States.Components;
using Pikl.Utils.Shaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    public enum FireType { Semi, Auto };


    [System.Serializable]
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Data/Interactable/New Weapon")]
    public class Weapon : Item {

        public int clipAmmo, clipSize, totalAmmo, ammoPerShot = 1;
        public float fireRate = 0.5f, reloadSpeed = 1;
        public FireType fireType;
        public Shot shot;
        public Shake shakeOnShot = ShakePresets.Shot;
        [HideInInspector]
        public float lastReloadTime, lastFireTime;
        [HideInInspector]
        public int reloadState;
        [HideInInspector]
        public bool reloading;

        int transfer = 0;
        public void Reload(bool instant = false) {
            transfer = Mathf.Clamp(clipSize - clipAmmo, 0, totalAmmo);

            if (transfer == 0)
                return;

            clipAmmo += transfer;
            totalAmmo -= transfer;

            lastReloadTime = Time.time - (instant ? reloadSpeed : Player.Player.I.shoot.reloadSpeedMultiplier == 1 ? 0 : reloadSpeed * Player.Player.I.shoot.reloadSpeedMultiplier);
            reloading = !instant;
        }

        internal void Init(string name, Sprite sprite, string description, ItemType type, int quantity, int maxStack,
                           int clipAmmo, int clipSize, int totalAmmo, int ammoPerShot, float fireRate, float reloadSpeed, 
                           FireType fireType, Shot shot) {
            Init(name, sprite, description, type, quantity, maxStack);
            this.clipAmmo = clipAmmo;
            this.clipSize = clipSize;
            this.totalAmmo = totalAmmo;
            this.ammoPerShot = ammoPerShot;
            this.fireRate = fireRate;
            this.reloadSpeed = reloadSpeed;
            this.fireType = fireType;
            this.shot = shot;
        }

        public static Weapon CreateInstance(Weapon weapon) {
            var data = CreateInstance<Weapon>();
            data.Init(weapon.name, weapon.sprite, weapon.description, weapon.type, weapon.quantity, weapon.maxStack,
                      weapon.clipAmmo, weapon.clipSize, weapon.totalAmmo, weapon.ammoPerShot, weapon.fireRate, weapon.reloadSpeed, 
                      weapon.fireType, weapon.shot);
            return data;
        }
    }

    [System.Serializable]
    public class Shot {
        public GameObject stock;
        public Damage damage;
        public Vector2 spawnOffset;
        public float force, accuracy;
        public int entitiesToPierce = -1;
        public bool isTrigger = false;
    }

}