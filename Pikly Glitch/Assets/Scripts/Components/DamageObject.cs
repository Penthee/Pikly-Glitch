using UnityEngine;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;

namespace Pikl.Components {
    //[RequireComponent(typeof(Explode))]
    public class DamageObject : MonoBehaviour {
        public LayerMask layersToDamage;
        public bool selfAsOrigin = true, useExplode = true, useRaycast = false, disabled = false;
        //public float activateDelay;
        public Damage damage;
        public int entitiesToPierce = -1;
        public bool disableIfNoCollisionAtStart, disableAfterTrigger;

        int origPierce = -1;

        void Start() {
            origPierce = entitiesToPierce;
            if (selfAsOrigin)
                damage.origin = transform;

            
        }

        void OnEnable() {
            entitiesToPierce = origPierce;

            //wtf does this do idk
            //if (useRaycast) {
            //    RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, 50, layersToDamage);
            //    if (hit) {
            //        if (hit.transform.GetComponent(typeof(IHealth)) != null) {
            //            Damage(hit.transform.GetComponent(typeof(IHealth)) as IHealth);
            //        } else if (hit.transform.GetComponent<Shooter>() != null) {
            //            hit.transform.GetComponent<Shooter>().TakeDamage(damage);
            //        } else {
            //            Wall w = hit.transform.GetComponent<Wall>();
            //            if (w != null)
            //                w.Check(gameObject);
            //        }
            //    }
            //}
        }

        private void Update() {
            if (disableIfNoCollisionAtStart) {
                if (!GetComponent<Collider2D>().IsTouchingLayers(layersToDamage))
                    GetComponent<Collider2D>().enabled = false;
            }
        }

        void OnCollisionEnter2D(Collision2D other) {
            if ((layersToDamage.value & (1 << other.gameObject.layer)) > 0) {
                Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

                if (rb)
                    ApplyKnockback(rb, transform.position, damage.knockbackForce);

                if (other.gameObject.GetComponent(typeof(IHealth)) != null)
                    Damage(other.gameObject.GetComponent(typeof(IHealth)) as IHealth);
            }

            if (disableAfterTrigger)
                GetComponent<Collider2D>().enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other) {
            if ((layersToDamage.value & (1 << other.gameObject.layer)) > 0) {
                Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

                if (rb)
                    ApplyKnockback(rb, transform.position, damage.knockbackForce);

                if (other.gameObject.GetComponent(typeof(IHealth)) != null)
                    Damage(other.gameObject.GetComponent(typeof(IHealth)) as IHealth);
            }

            if (disableAfterTrigger)
                GetComponent<Collider2D>().enabled = false;
        }

        internal void ApplyKnockback(Rigidbody2D rb, Vector3 origin, float force) {
            if (rb == null)
                return;

            Vector3 forceDirection = (rb.transform.position - origin).normalized;
            rb.AddForce(forceDirection * force);
        }


        void Damage(IHealth health) {
            if (disabled)
                return;

            if (entitiesToPierce != 0) {
                entitiesToPierce--;
                if (health != null)
                    health.TakeDamage(damage);
            } else {
                if (useExplode)
                    //StartCoroutine(GetComponent<Explode>().DoTheExplode());
                    GetComponent<Explode>().DoTheExplode();
                else
                    Destroy(gameObject);
            }
        }

    }
}