using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
using Pikl.States.Components;
using Pikl.UI;
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
            //powerup.Init(this);

            fv2D = GetComponent<FaceInput2D>();
            ar = GetComponent<Animator>();

            base.Start();

            //UIMgr.I.PauseFilterOff();
        }

        internal override void Update() {
            if (MainCamera.I.mapMode)
                return;

            base.Update();

            input.Update();
            evade.Update();
            inventory.Update();
            knife.Update();
            //powerup.Update();
        }

        internal override void FixedUpdate() {
            if (MainCamera.I.mapMode)
                return;

            base.FixedUpdate();
        }


        public void EndGame() {
            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText).StartDeathScroll();
        }

        void OnDrawGizmos() {
            //foreach (Transform t in shoot.shootTransforms)
            //    Gizmos.DrawLine(transform.position, t.position);
        }
    }
}