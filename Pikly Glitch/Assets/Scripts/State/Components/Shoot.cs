using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;

namespace Pikl.States.Components {
    [System.Serializable]
    public class Shoot { //TODO: Generalise this better, needs to work well for player and enemies
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
        
        public bool Locked {
            get { return locked; }
            set { locked = value; }
        }

        [SerializeField] GameObject shootObj;
        [SerializeField] Vector3 size = new Vector3(1, 1, 1);

        [SerializeField] float force, cooldown;
        [SerializeField] bool locked;

        //public Damage damage;
        Vector3 origScale;
        internal float origCooldown;

        public virtual void Init(StateObject so) {
            this.so = so;
            origCooldown = cooldown;
        }
    }
}