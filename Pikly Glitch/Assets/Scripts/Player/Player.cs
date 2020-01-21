using System;
using UnityEngine;
using Pikl.States;
using System.Collections;
using Pikl.Collections;
using Pikl.Components;
using Pikl.States.Components;
using Pikl.UI;
using Pikl.Utils.Cameras;
using UnityEngine.InputSystem;

//using Pikl.Extensions;

namespace Pikl.Player {
    public class Player : MonoBehaviour, PlayerControls.IPlayerActions {

        public static Player I;

        [Space] PlayerControls playerControls;
        
        //public PlayerHealth health;
        //public PlayerMovement move;
        //public PlayerInput input;
        //public PlayerShoot shoot;
        //public PlayerEvade evade;
        //public PlayerKnife knife;
        //public PlayerPowerup powerup = new PlayerPowerup();
        //public float interactRadius, interactCooldown, lastInteractTime, useCooldown, lastUseTime;
        
        public Inventory inventory;
        public SpriteRenderer weaponSprite;

        internal void Awake() {
            if (I == null)
                I = this;
            else
                Debug.LogError("More than one instance of Player.");

            //var actionMap = inputAsset.FindActionMap("Anomaly");
            
            playerControls.Player.SetCallbacks(this); 
        }

        void OnEnable() {
            playerControls.Enable();
        }

        void OnDisable() {
            playerControls.Disable();
        }

        internal void Start() {
            inventory.Init(this);

            ar = GetComponent<Animator>();
        }

        internal void Update() {
            if (MainCamera.I.mapMode)
                return;

            inventory.Update();
        }
        
        internal void FixedUpdate() {
            if (MainCamera.I.mapMode)
                return;
        }

        public void EndGame() {
            UIMgr.I.OpenMenu(UIMgr.I.textRead);
            (UIMgr.I.textRead as LevelIntroText).StartDeathScroll();
        }
        
        void OnDrawGizmos() {
            //foreach (Transform t in shoot.shootTransforms)
            //    Gizmos.DrawLine(transform.position, t.position);
        }

        public void OnMove(InputAction.CallbackContext context) {
            var v = context.ReadValue<Vector2>();
            Debug.Log("Move! : " + v.ToString());
        }

        public void OnLook(InputAction.CallbackContext context) {
            var v = context.ReadValue<Vector2>();
            Debug.Log("Look! : " + v.ToString());
        }

        public void OnAim(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnFire(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnInteract(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnDropItem(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnSelectItem(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }
    }
}