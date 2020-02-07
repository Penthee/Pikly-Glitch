using UnityEngine;
using System.Collections;
using Pikl.Utils.RDS;
using Pikl.Utils.Shaker;
using Pikl.Audio;
using DG.Tweening;
using System;
using Pikl.Data;
using Pikl.Player;
using Pikl.UI;

namespace Pikl.States.Components {
    [System.Serializable]
    public class PlayerHealth : MonoBehaviour, IHealth {
        [HideInInspector]
        public StateObject so;
        [HideInInspector]
        public State deadState;

        public Damage.Status Status {
            get { return status; }
            set { status = value; }
        }

        public Damage.Status status = Damage.Status.Ok;

        public GameObject[] deathPlosions, spawnOnDamage;
        public bool spawnFromPool;

        [ExposeProperty]
        public float HP {
            get;
            set;
        }

        [ExposeProperty]
        public float Armour {
            get;
            set;
        }

        public float MaxHp {
            get {
                return maxHp;
            }
            set {
                maxHp = value;
            }
        }

        [ExposeProperty]
        public bool Invulnerable {
            get { return invulnerable; }
            set { invulnerable = value; }
        }

        [ExposeProperty]
        public float DamageMultiplier {
            get { return damageMultiplier; }
            set { damageMultiplier = value; }
        }

        public float maxHp = 420,
                     invulTimer = 0.75f, healTimer = 1.42f;

        public bool flashOnDamage;

        public bool invulnerable { get; set; }

        public float lastDmgTime { get; private set; }

        public float lastHealTime { get; private set; }
        public float effectStartTime { get; private set; }
        public float damageMultiplier = 1;

        public bool isDead { get; set; }
        public float maxArmour;

        //float origHP = 5;
        Color[] originalColours;

        public void Init(StateObject so) {
            this.so = so;
            status = Damage.Status.Ok;
            deadState = new Player.Dead();

            SetInfo();
            //originalColours = new Color[(so as Player.Player).ship.GetComponent<MeshRenderer>().materials.Length];
            //foreach (var mat in (so as Player.Player).ship.GetComponent<MeshRenderer>().materials)
            //    originalColours[i++] = mat.color;
        }

        void SetInfo() {
            if (UIMgr.I._hp > 0) {
                HP = UIMgr.I._hp;
                Armour = UIMgr.I._armour;
                maxArmour = UIMgr.I._maxArmour;
                maxHp = UIMgr.I._maxHp;
                invulnerable = UIMgr.I._invulnerable;
                damageMultiplier = UIMgr.I._damageMultiplier;
                UIMgr.I._hp = 0;
            } else {
                HP = maxHp;
            }
        }

        public void TakeDamage(float dmg) {
            if (invulnerable || isDead || lastDmgTime + invulTimer > Time.time)
                return;

            dmg = dmg * damageMultiplier;
            lastDmgTime = Time.time;
            
            float damageDealt = 0;
            do {
                if (Armour > 0) {
                        Armour -= dmg -damageDealt;
                        if (Armour < 0)
                            damageDealt = dmg + Armour;
                        else
                            damageDealt = dmg;

                        Armour = Mathf.Clamp(Armour, 0, maxArmour);
                } else {
                    HP -= dmg - damageDealt;
                    HP = Mathf.Clamp(HP, 0, maxHp + 50);
                    damageDealt = dmg;
                }
            } while (damageDealt < dmg);

            if (flashOnDamage && dmg > 0)
                so.StartCoroutine(FlashRed());

            if (spawnOnDamage.Length > 0) {
                if (spawnOnDamage.Length > 0) {
                    if (spawnFromPool) {
                        foreach (GameObject obj in spawnOnDamage)
                            GameObjectMgr.I.Spawn(obj, so.transform.position, so.transform.rotation);
                    } else {
                        foreach (GameObject obj in spawnOnDamage)
                            Instantiate(obj, so.transform.position, so.transform.rotation);
                    }
                }
            }

            //AudioMgr.I.PlaySound(new AudioInfo("SFX/PlayerHit"));

            //if (HP <= 25) {
            //float m = (3 - (HP - 1)) * 0.05f;
            ApplyVignette();

            Shaker.I.ShakeCameraOnce(ShakePresets.Shot);
            //}

            if (HP <= 0)
                Die();
        }

        public void TakeDamage(Damage dmg) {
            if (invulnerable || isDead || lastDmgTime + invulTimer > Time.time)
                return;

            if (status == Damage.Status.Ok) {
                if (RDSRandom.IsPercentHit(dmg.effectChance)) {
                    if (dmg.ticks > 0)
                        (so as LivingStateObject).StartCoroutine(DamageTick(dmg));

                    if (dmg.effectDuration > 0)
                        (so as LivingStateObject).StartCoroutine(StartEffect(dmg));
                }
            }

            if (dmg.knockbackForce != 0)
                ApplyKnockback(dmg.origin.position, dmg.knockbackForce);

            if (dmg.type == Damage.Type.Heal)
                Heal(dmg.baseDmg);
            else
                TakeDamage(dmg.baseDmg);
        }

        void ApplyKnockback(Vector3 origin, float force) {
            Vector3 forceDirection = (so.transform.position - origin).normalized;
            so.rb.AddForce(forceDirection * force);
        }

        //int i = 0;
        public IEnumerator FlashRed() {
            //i = 0;
            //TODO: take MonoHealth flash and insert here
            //foreach(var mat in (so as Player.Player).ship.GetComponent<MeshRenderer>().materials)
            //    mat.color = Color.red;

            //(so as Player.Player).leftCylinder.GetComponent<SpriteRenderer>().color = Color.red;
            //(so as Player.Player).rightCylinder.GetComponent<SpriteRenderer>().color = Color.red;

            yield return new WaitForSeconds(0.1f);

            //foreach (var mat in (so as Player.Player).ship.GetComponent<MeshRenderer>().materials)
            //    mat.DOBlendableColor(originalColours[i++], 0.75f);

            //(so as Player.Player).leftCylinder.GetComponent<SpriteRenderer>().DOBlendableColor(Color.white, 0.75f);
            //(so as Player.Player).rightCylinder.GetComponent<SpriteRenderer>().DOBlendableColor(Color.white, 0.75f);
            //so.renderer.DOBlendableColor((so as EntityStateObject).originalSpriteColour, 0.75f);
        }

        public IEnumerator StartEffect(Damage dmg) {
            status = TypeToStatus(dmg.type);

            switch (status) {
                case Damage.Status.Burning:
                    //   so.e_renderer.Color = new Color(1, 0.4f, 0);
                    break;
                case Damage.Status.Chilled:
                    //   so.e_renderer.Color = Color.cyan;
                    //(so as LivingStateObject).move.force *= 0.25f;
                    break;
                case Damage.Status.Stunned:
                    //(so as LivingStateObject).move.force = 0;
                    Shaker.I.ShakeCameraOnce(ShakePresets.Explosion);
                    (so as LivingStateObject).fv2D.locked = true;
                    (so as Player.Player).shoot.Locked = true;
                    //(so as Player.Player).stunID = so.StartAsync(new Stun());
                    break;
            }

            yield return new WaitForSeconds(dmg.effectDuration);

            StopEffect();
        }

        public Damage.Status TypeToStatus(Damage.Type type) {
            switch (type) {
                case Damage.Type.Burn: return Damage.Status.Burning;
                case Damage.Type.Chill: return Damage.Status.Chilled;
                case Damage.Type.Stun: return Damage.Status.Stunned;

                case Damage.Type.Normal:
                case Damage.Type.Heal:
                default:
                    return Damage.Status.Ok;
            }
        }

        public void StopEffect() {
            status = Damage.Status.Ok;
            //  so.e_renderer.Color = (so as EntityStateObject).originalColour;
            (so as LivingStateObject).fv2D.locked = false;
            (so as Player.Player).shoot.Locked = false;
            //(so as LivingStateObject).move.force = (so as LivingStateObject).move.originalForce;
            so.StopAsync((so as Player.Player).stunID);
        }

        public IEnumerator DamageTick(Damage dmg) {
            int ticks = dmg.ticks;

            yield return new WaitForSeconds(dmg.tickRepeatRate);

            while (true) {
                if (dmg.type == Damage.Type.Heal)
                    Heal(dmg.baseDmg);
                else
                    TakeDamage(dmg.baseDmg);

                if (HP == 0 || --ticks <= 0)
                    break;

                yield return new WaitForSeconds(dmg.tickRepeatRate);
            }
        }

        public void Heal(float amount) {
            if (isDead || lastHealTime + healTimer > Time.time)
                return;

            HP += amount;
            HP = Mathf.Clamp(HP, 0, maxHp);

            ApplyVignette();
        }

        public void Overheal(float amount) {
            HP += amount;
            HP = Mathf.Clamp(HP, 0, maxHp + 50);
            ApplyVignette();
        }

        internal void AddArmour(int amount) {
            Armour += amount;
            Armour = Mathf.Clamp(Armour, 0, maxArmour);
        }

        void ApplyVignette() {
            float m = Mathf.Clamp((25 - HP) * 0.02f, 0, 0.3f);

            var s = Camera.main.GetComponent<Utils.Effects.ImageEffects.VignetteAndChromaticAberration>();
            DOTween.To(() => s.intensity, x => s.intensity = x, m, 1);
        }

        public void Die() {
            if (so is Player.Player player) {
                if (player.inventory.Exists("Teleport")) {
                    SaveLife();
                    return;
                }
            }

            if (deathPlosions.Length > 0) {
                if (spawnFromPool) {
                    foreach (GameObject plosion in deathPlosions)
                        GameObjectMgr.I.Spawn(plosion, so.transform.position, so.transform.rotation);
                } else {
                    foreach (GameObject plosion in deathPlosions)
                        Instantiate(plosion, so.transform.position, so.transform.rotation);
                }
            }

            isDead = true;

            so.SwitchTo(deadState);

            MessageMgr.I.Broadcast("PlayerDeath");
        }

        void SaveLife() {
            Vector3 spawn = GameObject.Find("SpawnPoint").transform.position;
            (so as Player.Player).inventory.RemoveOneOrDelete("Teleport");
            so.t.position = spawn;
            HP = maxHp * 0.5f;
            SaveLifeEffect();
        }

        void SaveLifeEffect() {
            //TODO: Sparkles n shit
        }
    }
}