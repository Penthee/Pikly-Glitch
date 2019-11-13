using UnityEngine;
using System.Collections;
using Pikl.Utils.Shaker;
//using Pikl.Audio;

namespace Pikl.Components {
    public class Explode : MonoBehaviour {
        public LayerMask collidesWith;
        public GameObject[] spawnOnExplode;
        public Vector3 spawnOffset, spawnRotation;
        public float minimumLifetime, explodeDelay, destroyDelay;
        public bool autoExplode, explodeOnCollision, onlyExplodeWhenMoving, 
                    explodeWhenOffscreen,
                    recycle, destroy = true, destroyParent, destroyRoot, destroyChildren = true, 
                    playSound, disableRenderer;
        public float destroyChildrenDelay, autoExplodeTimer;
        public string soundName, spawnOnExplodeParent;
        public Shake explodeShake = ShakePresets.Explosion;

        Rigidbody2D rb;
        float spawnTime;
        [HideInInspector]
        public bool exploding;

        bool OnlyExplodeWhenMoving {
            get {
                if (onlyExplodeWhenMoving && rb != null)
                    return rb.velocity.magnitude != 0;

                return true;
            }
        }
        static bool pooled;

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
            //if (!pooled) {
            //    foreach (GameObject obj in spawnOnExplode) {
            //        GameObjectMgr.I.SpawnToPool(obj, 170);
            //    }
            //    pooled = true;
            //}
        }

        void OnEnable() {
            exploding = false;
            spawnTime = Time.time;
        }

        //IEnumerator Init() {
        //    yield return new WaitForEndOfFrame();
        //}

        void Update() {
            if (!exploding &&
                ((autoExplode && spawnTime + autoExplodeTimer < Time.time) ||
                (explodeWhenOffscreen /*&& Helper.IsPointOffscreen(transform.position)*/)))
                //StartCoroutine(DoTheExplode());
                DoTheExplode();
        }

        public void DoTheExplode() { 
        //public IEnumerator DoTheExplode() {
            exploding = true;

            if (explodeShake.magnitude > 0)
                Shaker.I.ShakeCameraOnce(explodeShake);

            if (disableRenderer) {
                GetComponent<Renderer>().enabled = false;
                GetComponent<Rigidbody2D>().simulated = false;
            }

            if (GetComponent<Collider2D>() != null)
                GetComponent<Collider2D>().enabled = false;

            if (spawnOnExplode.Length > 0) {
                foreach (GameObject obj in spawnOnExplode) {
                    if (obj != null) {
                        GameObject _obj = GameObjectMgr.I.Spawn(obj, transform.position + spawnOffset, Quaternion.Euler(spawnRotation.x, spawnRotation.y, spawnRotation.z));
                        if (!string.IsNullOrEmpty(spawnOnExplodeParent))
                            _obj.transform.parent = GameObject.Find(spawnOnExplodeParent).transform;
                    }
                }
            }

            //AudioInfo ai = null;
            //if (playSound)
              //  AudioMgr.I.PlaySound(new AudioInfo(soundName));

            //if (explodeDelay > 0)
            //    yield return new WaitForSeconds(explodeDelay);

            if (destroyChildren) {
                DestroyChildren();
            } else {
                transform.DetachChildren();
            }

            //if (playSound) {
            //    while (ai.source.isPlaying) {
            //        yield return new WaitForEndOfFrame();
            //    }
            //}

            if (destroyRoot) {
                Destroy(transform.root.gameObject, destroyDelay);
            } else if (destroyParent) {
                Destroy(transform.parent.gameObject, destroyDelay);
            } else if (recycle) {
                //Add Delay to Recycle
                //yield return new WaitForSeconds(destroyDelay);
                GameObjectMgr.I.Recycle(gameObject);
            } else if (destroy) {
                Destroy(gameObject, destroyDelay);
            }

        }

        void DestroyChildren() {
            if (destroyChildrenDelay > 0) {
                for (int i = 0; i < transform.childCount; i++) {
                    GameObject child = transform.GetChild(i).gameObject;
                    if (child.GetComponent<ParticleSystem>() != null)
                        child.GetComponent<ParticleSystem>().loop = false;

                    //child.GetComponent<Destroy>().enabled = true;
                }

                transform.DetachChildren();
            }
        }

        void OnCollisionEnter2D(Collision2D other) {
            if (OnlyExplodeWhenMoving && explodeOnCollision && (collidesWith.value & (1 << other.gameObject.layer)) > 0 && spawnTime + minimumLifetime < Time.time) {
                if (!exploding)
                    //StartCoroutine(DoTheExplode());
                    DoTheExplode();
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (OnlyExplodeWhenMoving && explodeOnCollision && (collidesWith.value & (1 << other.gameObject.layer)) > 0 && spawnTime + minimumLifetime < Time.time) {
                if (!exploding)
                    //StartCoroutine(DoTheExplode());
                    DoTheExplode();
            }
        }

        //????????????????????????????????????
        //void OnBecameVisible() {
        //    enabled = true;
        //}

        //void OnBecameInvisible() {
        //    enabled = false;
        //}
    }
}