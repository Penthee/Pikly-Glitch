using UnityEngine;
using System.Collections;
using Pikl.Extensions;

namespace Pikl.Components {
    public class ForceOnStart : MonoBehaviour {
        public float minForce, maxForce;
        public float minTorque, maxTorque;
        public Vector2 direction;
        public bool enable = false, start = true, 
                    random = true, local = true;

        //Rigidbody2D rb;

        void Start() {
            if (start)
                AddForce();

            //MessageMgr.I.AddListener("Pause", Pause);
            //MessageMgr.I.AddListener("Unpause", UnPause);
            //rb = GetComponent<Rigidbody2D>();
        }

        void OnEnable() {
            if (enable)
                AddForce();

        }

        public void AddForce() {
            if (random) {
                GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1, 2), Random.Range(-1, 2)) * Random.Range(minForce, maxForce));
                GetComponent<Rigidbody2D>().AddTorque(Random.Range(minTorque, maxTorque));
            } else {
                if (local)
                    direction = direction.Rotate(transform.eulerAngles.z);

                GetComponent<Rigidbody2D>().AddForce(direction.normalized * Random.Range(minForce, maxForce));
            }
        }

        void Update() {
            UnityEngine.Debug.DrawRay(transform.position, direction);
        }

        //Vector2 velocityAtPause;
        //void Pause() {
        //    velocityAtPause = GetComponent<Rigidbody2D>().velocity;
        //    GetComponent<Rigidbody2D>().isKinematic = true;
        //    StartCoroutine(DoParticles(true));
        //}

        //void UnPause() {
        //    GetComponent<Rigidbody2D>().isKinematic = false;
        //    GetComponent<Rigidbody2D>().velocity = velocityAtPause;
        //    StartCoroutine(DoParticles(false));
        //}

        //IEnumerator DoParticles(bool pause) {
        //    yield return new WaitForEndOfFrame();
        //    yield return new WaitForEndOfFrame();
        //    var particles = GetComponentInChildren<ParticleSystem>();
        //    if (particles != null) {
        //        if (pause)
        //            particles.Pause();
        //        else
        //            particles.Play();
        //    }

        //}

        //void OnDestroy() {
        //    MessageMgr.I.RemoveListener("Pause", Pause);
        //    MessageMgr.I.RemoveListener("Unpause", UnPause);
        //}
    }
}