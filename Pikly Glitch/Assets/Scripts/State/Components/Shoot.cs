using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;

namespace Pikl.States.Components {
    [System.Serializable]
    public class Shoot {
        [HideInInspector]
        public StateObject so;

        [HideInInspector]
        public float lastTime;

        public List<Transform> shootTransforms = new List<Transform>();

        public GameObject ShootObj {
            get { return shootObj; }
            set { shootObj = value; }
        }

        public Vector3 Size {
            get { return size; }
            set { size = value; }
        }

        public float Force {
            get { return force; }
            set { force = value; }
        }

        public float Cooldown {
            get { return cooldown; }
            set { cooldown = value; }
        }

        public float BounceAmount {
            get { return bounceAmount; }
            set { bounceAmount = value; }
        }

        public int BounceLength {
            get { return bounceLength; }
            set { bounceLength = value; }
        }

        public bool Locked {
            get { return locked; }
            set { locked = value; }
        }

        [SerializeField]
        GameObject shootObj;
        [SerializeField]
        Vector3 size = new Vector3(1, 1, 1);
        [SerializeField]
        float force, cooldown, bounceAmount;
        [SerializeField]
        int bounceLength;
        [SerializeField]
        bool locked;

        //public Damage damage;
        Vector3 origScale;
        internal float origCooldown;

        public virtual void Init(StateObject so) {
            this.so = so;
            origCooldown = cooldown;
        }
        
        public IEnumerator BounceScale(Transform t, Shoot shoot) {
            origScale = t.localScale;

            if (bounceLength <= 0) {
                float bounceTime = 0;
                bool reverse = false;

                while (bounceTime < Cooldown) {
                    if (bounceTime > Cooldown / 2)
                        reverse = true;

                    Vector3 scale = t.localScale;
                    scale.x += (reverse ? shoot.BounceAmount : -shoot.BounceAmount);
                    scale.y += (reverse ? shoot.BounceAmount : -shoot.BounceAmount);

                    scale.x = Mathf.Clamp01(scale.x);
                    scale.y = Mathf.Clamp01(scale.y);

                    t.localScale = scale;

                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();

                    bounceTime += Time.fixedDeltaTime * 2;
                }

            } else {

                for (int j = 0; j < 2; j++) {
                    for (int i = 0; i < shoot.BounceLength; i++) {
                        Vector3 scale = t.localScale;
                        scale.x += (j == 0 ? -shoot.BounceAmount : shoot.BounceAmount);
                        scale.y += (j == 0 ? -shoot.BounceAmount : shoot.BounceAmount);

                        t.localScale = scale;

                        yield return new WaitForFixedUpdate();
                        yield return new WaitForFixedUpdate();
                    }
                }
            }

            t.localScale = origScale;
        }

    }
}