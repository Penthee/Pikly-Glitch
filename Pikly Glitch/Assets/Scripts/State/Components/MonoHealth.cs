using UnityEngine;
using System.Collections;
//using Pikl.Utils.RDS;
using Pikl.Components;
using System;
using Pikl.Enemy;
using Pikl.Utils.Shaker;
using Pikl.Audio;
using Pikl.Utils.RDS;
using Random = UnityEngine.Random;

namespace Pikl.States.Components {
    [System.Serializable]
    public class MonoHealth : MonoBehaviour, IHealth {

        public Damage.Status Status {
            get { return status; }
            set { status = value; }
        }

        public Damage.Status status = Damage.Status.Ok;

        public GameObject[] deathPlosions, spawnOnDamage;
        public bool spawnFromPool;
        Color originalSpriteColour;

        new SpriteRenderer renderer;
        new Collider2D collider;
        internal Rigidbody2D rb;

        public float hp { get; set; }
        [SerializeField]
        public bool invulnerable { get; set; }
        public bool playSoundOnDeath;// { get; set; }
        public AudioInfo deathSound;
        public bool playSoundOnDamage;// { get; set; }
        public AudioInfo damageSound;
        [ExposeProperty]
        public virtual float HP {
            get { return hp; }
            set { hp = value; }
        }

        [ExposeProperty]
        public bool Invulnerable {
            get { return invulnerable; }
            set { invulnerable = value; }
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
        public float DamageMultiplier {
            get { return damageMultiplier; }
            set { damageMultiplier = value; }
        }

        internal StateObject so;
        public int lives = 0, reanimateChance = 0;
        
        public float maxHp = 420,
                     invulTimer = 0.75f, healTimer = 1.42f,
                     lifetime, damageMultiplier = 1,
                     rendererDisableDelay,
                     colliderDisableDelay,
                     destroyDelay;

        [HideInInspector]
        public float healthDelta;

        public bool flashOnDamage,
            disableRenderer,
            disableCollider,
            destroy,
            recycle,
            takeDamage;

        public float lastDmgTime { get; internal set; }
        public float lastHealTime { get; internal set; }
        public float effectStartTime { get; internal set; }

        public bool isDead { get; set; }

        internal float startTime;

        void OnEnable() {
            //if (renderer == null)
            //    renderer = GetComponent<Renderer>();
            //if (collider == null)
            //collider = GetComponent<Collider2D>();

            //originalSpriteColour = renderer.material.color;
            //renderer = GetComponent<SkeletonAnimator>().Skeleton;
            foreach(GameObject e in deathPlosions)
                if (GameObjectMgr.I.CountPooled(e) + GameObjectMgr.I.CountSpawned(e) == 0)
                    GameObjectMgr.I.SpawnToPool(e, 16);

            foreach (GameObject e in spawnOnDamage)
                if (GameObjectMgr.I.CountPooled(e) + GameObjectMgr.I.CountSpawned(e) == 0)
                    GameObjectMgr.I.SpawnToPool(e, 16);

            isDead = false;
            startTime = Time.time;
            HP = maxHp;

            StartCoroutine(Lifetime());
            StartCoroutine(GetComponents());

        }

        internal virtual IEnumerator GetComponents() {
            so = GetComponent<StateObject>();
            if (so == null)
                so = transform.parent.GetComponent<StateObject>();

            while (collider == null && renderer == null && enabled) {
                collider = GetComponent<Collider2D>();
                renderer = GetComponent<SpriteRenderer>();
                rb = GetComponent<Rigidbody2D>();

                if (renderer != null)
                    originalSpriteColour = renderer.color;

                yield return new WaitForEndOfFrame();
            }
        }

        void Update() {
            healthDelta = hp / maxHp;
        }

        public virtual void TakeDamage(float dmg) {
            if (invulnerable || isDead || lastDmgTime + invulTimer > Time.time)
                return;

            lastDmgTime = Time.time;

            HP -= dmg * damageMultiplier;
            HP = Mathf.Clamp(HP, 0, maxHp);

            if (flashOnDamage)
                StartCoroutine(FlashRed());

            if (playSoundOnDamage)
                AudioMgr.I.PlaySound(new AudioInfo(damageSound.audioName));

            if (spawnOnDamage.Length > 0) {
                if (spawnOnDamage.Length > 0) {
                    if (spawnFromPool) {
                        foreach (GameObject obj in spawnOnDamage)
                            GameObjectMgr.I.Spawn(obj, transform.position, transform.rotation);
                    } else {
                        foreach (GameObject obj in spawnOnDamage)
                            Instantiate(obj, transform.position, transform.rotation);
                    }
                }
            }

            if (HP == 0)
                Die();
        }

        public virtual void TakeDamage(Damage dmg) {
            if (invulnerable || isDead || lastDmgTime + invulTimer > Time.time)
                return;

            if (status == Damage.Status.Ok) {
                if (RDSRandom.IsPercentHit(dmg.effectChance)) {
                    if (dmg.ticks > 0)
                        StartCoroutine(DamageTick(dmg));

                    if (dmg.effectDuration > 0)
                        StartCoroutine(StartEffect(dmg));
                }
            }

            if (dmg.baseDmg >= HP && (dmg.origin != null && dmg.origin.name.Contains("Player"))) {
                //if (GetComponent<EnemyStateObject>() != null)
                //    ScoreMgr.I.AddKill((int)GetComponent<EnemyStateObject>().sides + gameObject.name, transform.position);
                //else
                //    ScoreMgr.I.AddKill("2Circle(Clone)", transform.position);
            }
            if (dmg.knockbackForce != 0)
                ApplyKnockback(dmg.origin.position, dmg.knockbackForce);

            if (dmg.type == Damage.Type.Heal)
                Heal(dmg.baseDmg);
            else
                TakeDamage(dmg.baseDmg);
        }

        internal void ApplyKnockback(Vector3 origin, float force) {
            if (rb == null)
                return;

            Vector3 forceDirection = (transform.position - origin).normalized;
            rb.AddForce(forceDirection * force);
        }

        public IEnumerator Lifetime() {
            while (!isDead) {
                yield return new WaitForEndOfFrame();

                if (lifetime > 0 && startTime + lifetime < Time.time) {
                    Die();
                }
            }
        }

        public IEnumerator FlashRed() {
            renderer.color = new Color(1, 0, 0);

            yield return new WaitForSeconds(0.1f);

            renderer.color = originalSpriteColour;
        }

        public IEnumerator StartEffect(Damage dmg) {
            status = TypeToStatus(dmg.type);

            switch (status) {
                case Damage.Status.Burning:
                    renderer.color = Color.red;
                    break;
                case Damage.Status.Chilled:
                    renderer.color = Color.cyan;
                    //move.force *= 0.25f;
                    break;
                case Damage.Status.Stunned:
                    renderer.color = Color.gray;
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
            renderer.color = originalSpriteColour;

            //move.force = move.originalForce;
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
        }

        public virtual void Die() {
            if (lives >= 1 && Random.Range(reanimateChance, 101) == 100) {
                StartCoroutine(nameof(Reanimate));
            } else {
                isDead = true;
            }

            if (so)
                so.SwitchTo(so.deadState);
            

            //Shaker.I.ShakeCameraOnce(ShakePresets.Bump);

            if (deathPlosions.Length > 0) {
                if (spawnFromPool) {
                    foreach (GameObject plosion in deathPlosions)
                        GameObjectMgr.I.Spawn(plosion, transform.position, transform.rotation);
                } else {
                    foreach (GameObject plosion in deathPlosions)
                        Instantiate(plosion, transform.position, transform.rotation);
                }
            }


            if (playSoundOnDeath)
                AudioMgr.I.PlaySound(new AudioInfo(deathSound.audioName));

            if (disableRenderer)
                StartCoroutine(DisableRenderer());

            if (disableCollider)
                StartCoroutine(DisableCollider());

            if (lives <= 0 && (destroy || recycle))
                StartCoroutine(Destroy());

            lives--;
        }

        IEnumerator Reanimate() {
            yield return new WaitForSeconds(Random.Range(1.2f, 5f));

            hp = maxHp * (0.1f * Mathf.Clamp(lives, 1, 10));
            
            if (so)
                so.SwitchToDefault();
            
            if (disableCollider) collider.enabled = true;
            if (disableRenderer) renderer.enabled = true;
        }

        internal virtual IEnumerator Destroy() {
            yield return new WaitForSeconds(destroyDelay);
            if (recycle)
                GameObjectMgr.I.Recycle(gameObject);
            else
                Destroy(gameObject);
        }

        internal virtual IEnumerator DisableRenderer() {
            yield return new WaitForSeconds(rendererDisableDelay);
            renderer.enabled = false;
        }

        internal virtual IEnumerator DisableCollider() {
            yield return new WaitForSeconds(colliderDisableDelay);
            collider.enabled = false;
        }

        void OnCollisionEnter2D(Collision2D other) {
            if (takeDamage) {
                switch (other.gameObject.tag) {
                    case "PlayerDamage":
                    case "EnemyDamage":
                    case "Damage":
                        TakeDamage(other.gameObject.GetComponent<DamageObject>().damage);
                        break;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (takeDamage) {
                switch (other.gameObject.tag) {
                    case "PlayerDamage":
                    case "EnemyDamage":
                    case "Damage":
                        TakeDamage(other.GetComponent<DamageObject>().damage);
                        break;
                }
            }
        }
    }
}