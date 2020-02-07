using System.Collections;
using UnityEngine;

namespace Pikl.Enemies {
    public class Trap : MonoBehaviour {
        public GameObject enemy;
        public Transform spawnPos;
        [Range(1, 10)]
        public float distance = 5;
        [Range(1, 40)]
        public int quantity = 10;
        [Range(0.1f, 0.75f)]
        public float spawnSpeed = 0.3f;
        [Range(0.5f, 2f)]
        public float triggerTimeMin = 0.5f, triggerTimeMax = 2f;

        float insideTime, triggerTime;
        bool setInsideTime;

        public bool triggered = false;

        GameObject player;

        void Start()
        {
            player = GameObject.Find("Player");
            triggerTime = Random.Range(triggerTimeMin, triggerTimeMax);
        }

        void Update() {
            if (!player)
                return;

            if (PlayerInside()) {
                Trigger();
            }
        }

        public void Trigger() {
            if (!triggered) {
                triggered = true;
                StartCoroutine(Spawn());
            }
        }

        bool PlayerInside() {
            if (triggered)
                return false;

            if (Vector2.Distance(transform.position, player.transform.position) < distance) {
                if (!setInsideTime) {
                    insideTime = Time.time;
                    setInsideTime = true;
                }

                if (Time.time < triggerTime + insideTime) {
                    return true;
                }
            } else {
                setInsideTime = false;
            }

            return false;
        }

        IEnumerator Spawn() {
            for(int i = 0; i < quantity; i++) {
                GameObject.Instantiate(enemy, spawnPos.position, Quaternion.identity);
                yield return new WaitForSeconds(spawnSpeed);
            }

            enabled = false;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, distance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPos.position, 0.125f);
            Gizmos.color = Color.white;
        }
    }
}
