using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
using Pikl.States.Components;
using Pikl.Utils.Cameras;
//using Pikl.Extensions;

namespace Pikl.Player {
    public class Player : LivingStateObject {

        public static Player I;

        [HideInInspector]
        public PlayerHealth health;
        public PlayerMovement move;
        public PlayerInput input;
        public PlayerShoot shoot;
        public PlayerEvade evade;
        public Inventory inventory;
        public PlayerKnife knife;
        public SpriteRenderer weaponSprite;
        //public PlayerPowerup powerup = new PlayerPowerup();
        public float interactRadius, interactCooldown, lastInteractTime, useCooldown, lastUseTime;

        public int evadeID, shootID, secondaryID, stunID, interactID, aimID;

        //static internal AudioInfo laserSound/*, missileLaunch*/;

        internal override void Awake() {
            if (I == null)
                I = this;
            else
                Debug.LogError("More than one instance of Player.");

            base.Awake();
        }

        internal override void Start() {
            MessageMgr.I.AddListener("GameWin", OnGameWin);

            //laserSound = new AudioInfo("SFX/PlayerShoot");
            //missileLaunch = new AudioInfo("SFX/RocketLaunch");
            
            defaultState = new Idle();
            deadState = new Dead();
            pauseState = new Pause();

            health = GetComponent<PlayerHealth>();

            health.Init(this);
            move.Init(this);
            input.Init(this);
            shoot.Init(this);
            evade.Init(this);
            inventory.Init(this);
            knife.Init(this);

            fv2D = GetComponent<FaceInput2D>();
            //powerup.Init(this);

            ar = GetComponent<Animator>();

            StartCoroutine(WallCheckThing());

            base.Start();

            //Ref.I["PlayerObj"] = gameObject;
            //Ref.I["PlayerScript"] = this;

            //UIMgr.I.PauseFilterOff();
        }

        IEnumerator WallCheckThing() {
            while (!isDead) {

                yield return new WaitForSeconds(0.1f);
            }
        }

        internal override void Update() {
            if (MainCamera.I.mapMode)
                return;

            base.Update();


            input.Update();
            //shoot.Update();
            evade.Update();
            inventory.Update();
            //powerup.Update();
            knife.Update();

            //Debug.Log("Sprint Axis: " + input.SprintAxis);
        }

        internal override void FixedUpdate() {
            if (MainCamera.I.mapMode)
                return;

            base.FixedUpdate();
        }

        void OnDrawGizmos() {
            //foreach (Transform t in shoot.shootTransforms)
            //    Gizmos.DrawLine(transform.position, t.position);
        }

        void OnDestroy() {
            if (MessageMgr.I != null)
                MessageMgr.I.RemoveListener("GameWin", OnGameWin);
        }

        void OnGameWin() {
            //SwitchTo(new Pause());
        }
    }
}