using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using TeamUtility.IO;
using Pikl.Extensions;
using System.Collections.Generic;
using Pikl.Interaction;
using Pikl.Data;
//using Pikl.Profile;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerInput {
        [HideInInspector]
        public Player player;
        public float aimCameraDistance = 0.5f;
        public bool useLocked;
        static Dictionary<string, string> mappings = new Dictionary<string, string>();
        Vector3 lastMoveAxis, lastMoveAxisRaw, lastMouseDir, lastStickDir;
        internal bool aimAssist;

        public float lastCraftToggle, craftToggleCooldown = 1.5f;

        public void Init(Player player) {
            this.player = player;

            aimAssist = false;// ProfileMgr.I.profile.aimAssist.Value;

            mappings.Clear();
            mappings.Add("MoveHorizontal", LookupMgr.I.controllerConfigs.FindMapping("Move").axis);
            mappings.Add("MoveVertical", LookupMgr.I.controllerConfigs.FindMapping("Move").altAxis);
            mappings.Add("ShootHorizontal", LookupMgr.I.controllerConfigs.FindMapping("Shoot").axis);
            mappings.Add("ShootVertical", LookupMgr.I.controllerConfigs.FindMapping("Shoot").altAxis);
            mappings.Add("Primary", LookupMgr.I.controllerConfigs.FindMapping("Primary").axis);
            mappings.Add("Secondary", LookupMgr.I.controllerConfigs.FindMapping("Secondary").axis);
            mappings.Add("Evade", LookupMgr.I.controllerConfigs.FindMapping("Evade").axis);
            mappings.Add("Cycle", LookupMgr.I.controllerConfigs.FindMapping("Cycle Secondary").axis);
        }

        public static bool Pause {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return Input.GetKeyDown(KeyCode.Escape);
                else
                    return InputMgr.GetButtonDown("Pause");
            }
        }
        
        internal Vector3 MouseDir {
            get {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                //Debug.HBDebug.Log("MouseDir : " + (pos));
                return pos - player.transform.position;
            }
        }

        internal Vector3 MouseDirFromWeapon {
            get {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 0;
                //Debug.HBDebug.Log("MouseDirFromWeapon : " + (pos));
                return pos - player.weaponSprite.transform.position;
            }
        }

        int[] aimAssistDegrees = new int[] { -2, 2, -4, 4, -6, 6, -8, 8, -10, 10, -12, 12 };
        internal Vector3 StickDir {
            get {
                Vector3 actualStickDir = InputMgr.GetAxisVector(mappings["ShootHorizontal"], mappings["ShootVertical"]);

                if (aimAssist && !Physics2D.Raycast(player.t.position, actualStickDir, 42, 1 << LayerMask.NameToLayer("Enemy")))
                {
                    for (int i = 0; i < aimAssistDegrees.Length; i++)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(player.t.position, actualStickDir.Rotate(aimAssistDegrees[i]), 42, player.shoot.aimLayers);
                        UnityEngine.Debug.DrawLine(player.t.position, player.t.position + actualStickDir.Rotate(aimAssistDegrees[i]) * 50, Color.yellow);

                        if (hit)
                        {
                            var hit2 = Physics2D.Raycast(player.t.position, (hit.point - player.t.position.To2DXY()), hit.distance, player.shoot.aimBlockLayers);
                            if (hit2)
                            {
                                UnityEngine.Debug.DrawLine(player.t.position, player.t.position.To2DXY() + ((hit2.point - player.t.position.To2DXY()) * hit2.distance), Color.red);
                            }
                            else
                            {
                                //Debug.HBDebug.Log("Ray hit : " + hit.transform.name);
                                //if (hit && (1 << LayerMask.NameToLayer("Wall")) >> hit.transform.gameObject.layer == 0) {
                                //if ((LayerMask.NameToLayer("Enemy") == (LayerMask.NameToLayer("Enemy") | (1 << hit.transform.gameObject.layer)))) {
                                //Debug.HBDebug.Log("Aiming at enemy, not blocked by wall, lerping...");
                                return Vector3.Lerp(actualStickDir, (hit.transform.position - player.t.position), 1);
                                //}
                            }
                        }
                    }
                }

                return actualStickDir;
            }
        }

        public Vector3 MoveAxis {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return new Vector3(InputMgr.GetAxis("Horizontal"), InputMgr.GetAxis("Vertical"), 0);
                else
                    return InputMgr.GetAxisVector(mappings["MoveHorizontal"], mappings["MoveVertical"]);
            }
        }

        public Vector3 MoveAxisRaw {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return new Vector3(InputMgr.GetAxisRaw("Horizontal"), InputMgr.GetAxisRaw("Vertical"), 0);
                else
                    return InputMgr.GetAxisVector(mappings["MoveHorizontal"], mappings["MoveVertical"]);
            }
        }
        
        internal Vector3 LastMouseDir {
            get { return lastMouseDir; }
        }

        internal Vector3 MouseVelocity {
            get { return MouseDir - LastMouseDir; }
        }

        internal Vector3 LastStickDir {
            get { return lastStickDir; }
        }

        public Vector3 LastMoveAxis {
            get { return lastMoveAxis; }
        }

        public Vector3 LastMoveAxisRaw {
            get { return lastMoveAxisRaw; }
            internal set { lastMoveAxisRaw = value; }
        }

        public bool ShootAxis() {
                Weapon w = (player.inventory.SelectedItem as Weapon);
                if (!w) return false;

                return w &&
                       (w.fireType == FireType.Semi ? HasReleasedTrigger() : true) &&
                       !w.reloading &&
                       w.clipAmmo > 0 &&
                       AimAxis &&
                       ShootInput && 
                       (player.stunID == 0 && 
                       player.shootID == 0) &&
                       HasCooledDown(w.lastFireTime, w.fireRate);
        }

        bool triggerDown = false;
        bool HasReleasedTrigger() {
            if (!triggerDown && ShootInput) {
                triggerDown = true;
                return true;
            } else if (!ShootInput) {
                triggerDown = false;
                return false;
            }

            return false;
        }

        bool interactDown = false;
        bool HasReleasedInteract() {
            if (!interactDown && InteractInput) {
                interactDown = true;
                return true;
            } else if (!InteractInput) {
                interactDown = false;
                return false;
            }

            return false;
        }

        bool ShootInput {
            get {
                //if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return InputMgr.GetAxisRaw("Shoot") != 0;
                //else {
                //    if (mappings["Primary"].Contains("Bumper"))
                //        return InputMgr.GetButton(mappings["Primary"]);
                //    return InputMgr.GetAxis(mappings["Primary"]) != 0;
                //}
            }
        }

        public bool AimAxis {
            get {
                return
                    (player.inventory.SelectedType == ItemType.Weapon) &&
                    AimInput;
            }
        }

        public bool AimInput {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return InputMgr.GetAxisRaw("Aim") != 0;
                else {
                    if (mappings["Secondary"].Contains("Bumper"))
                        return InputMgr.GetButton(mappings["Secondary"]);
                    return InputMgr.GetAxis(mappings["Secondary"]) != 0;
                }
            }
        }

        public bool InteractInput {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return InputMgr.GetAxis("Interact") != 0;
                else
                    return InputMgr.GetButton(mappings["Interact"]);
            }
        }

        public bool InteractAxis {
            get {
                return HasReleasedInteract() && 
                       InteractInput &&
                       HasCooledDown(player.lastInteractTime, player.interactCooldown);// && 
                       //FindClosestInteractable() != null;
            }
        }

        public bool SprintAxis {
            get {
                return
                    (player.stunID == 0 && player.evadeID == 0) &&
                    !player.evade.hasReleasedSinceLastEvade &&
                    MoveAxisRaw.magnitude != 0 &&
                    player.evade.Stamina >= 1 &&
                    EvadeInput &&
                    HasCooledDown(player.evade.lastTime, player.evade.Cooldown);
            }
        }

        public bool EvadeAxis {
            get {
                return /*!PauseMgr.I.Paused &&*/
                       (player.stunID == 0 && player.evadeID == 0) &&
                       player.evade.hasReleasedSinceLastEvade &&
                       HasCooledDown(player.evade.lastTime, player.evade.Cooldown) &&
                       EvadeInput &&
                       MoveAxisRaw.magnitude != 0 &&
                       player.evade.Stamina >= player.evade.EvadeCost;
            }
        }

        public bool EvadeInput {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return InputMgr.GetAxis("Evade") != 0;
                else
                    return InputMgr.GetButton(mappings["Evade"]);
            }
        }

        bool ReloadInput {
            get {
                //if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                return InputMgr.GetAxisRaw("Reload") != 0;
                //else {
                //    if (mappings["Primary"].Contains("Bumper"))
                //        return InputMgr.GetButton(mappings["Primary"]);
                //    return InputMgr.GetAxis(mappings["Primary"]) != 0;
                //}
            }
        }

        public bool ReloadAxis {
            get {
                Weapon weapon = player.inventory.SelectedItem as Weapon;
                return
                    weapon != null &&
                    !weapon.reloading &&
                    weapon.type == ItemType.Weapon &&
                    weapon.clipAmmo < weapon.clipSize &&
                    ReloadInput;
            }
        }

        public bool DropAxis {
            get {
                return DropInput;
            }
        }

        bool DropInput {
            get {
                //if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                return InputMgr.GetAxisRaw("Drop") != 0;
                //else {
                //    if (mappings["Primary"].Contains("Bumper"))
                //        return InputMgr.GetButton(mappings["Primary"]);
                //    return InputMgr.GetAxis(mappings["Primary"]) != 0;
                //}
            }
        }

        public bool UseAxis {
            get {
                if (InputMgr.GetAxisRaw("Shoot") == 0) {
                    useLocked = false;
                }

                return
                    !(UI.UIMgr.I.CurrentMenu as UI.GameUI).craftingUI.activeSelf &&
                    (player.inventory.SelectedType == ItemType.Consumable ||
                    player.inventory.SelectedType == ItemType.Throwable) &&
                    HasCooledDown(player.lastUseTime, player.useCooldown) &&
                    !useLocked &&
                    InputMgr.GetAxisRaw("Shoot") != 0;
            }
        }

        public bool CraftingInput {
            get {
                if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                    return InputMgr.GetAxis("Crafting") != 0;
                else
                    return InputMgr.GetButton(mappings["Crafting"]);
            }
        }

        public bool CraftingAxis {
            get {
                return HasCooledDown(lastCraftToggle, craftToggleCooldown) && CraftingInput;
            }
        }

        float dist, shortest;
        public InteractableObj FindClosestInteractable() {
            InteractableObj i = null;
            dist = 420; shortest = 420;

            foreach (Collider2D o in Physics2D.OverlapCircleAll(player.transform.position, player.interactRadius, 1 << LayerMask.NameToLayer("Object"))) {
                //Debug.Log("interactable found");
                dist = Vector2.Distance(o.transform.position, player.transform.position);
                if (dist < shortest) {
                    shortest = dist;
                    i = o.GetComponent<InteractableObj>();
                }
            }

            return i;
        }

        public bool SwipeAxis {
            get {
                return !player.knife.swiping &&
                        (player.inventory.SelectedType != ItemType.Consumable) &&
                        HasCooledDown(player.knife.lastSwipeTime, player.knife.cooldown) &&
                        ShootInput &&
                        !AimAxis;
            }
        }

        public bool HasCooledDown(float lastTime, float cooldown) {
            return lastTime + cooldown < Time.time;
        }

        internal void Update() {
            UpdateLastAxis();

            UpdateStateInput();

            UpdateAsync();

            //if (InputMgr.GetAxisRaw("Shoot") == 0)
            //    useLocked = false;
        }

        void UpdateLastAxis() {
            Vector3 axis = MoveAxis;

            if (axis.magnitude != 0)
                lastMoveAxis = axis;

            axis = MoveAxisRaw;

            if (axis.magnitude != 0)
                lastMoveAxisRaw = axis;

            axis = MouseDir;

            if (axis != lastMouseDir)
                lastMouseDir = axis;

            //if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration) {
            //    axis = StickDir;

            //    if (axis != Vector3.zero && axis.magnitude >= lastStickDir.magnitude) {
            //        lastStickDir = axis;
            //    }
            //}
        }

        void UpdateStateInput() {
            State state = null;

            if (player.state.Peek() is PlayerState)
                state = (player.state.Peek() as PlayerState).HandleInput();

            if (state != null)
                player.SwitchTo(state);
        }

        void UpdateAsync() {
            State state = null;
            State[] asyncStates = new State[player.asyncStates.Count];
            player.asyncStates.Values.CopyTo(asyncStates, 0);

            foreach (State s in asyncStates) {
                if (s is PlayerState) {
                    state = (s as PlayerState).HandleInput();

                    if (state != null)
                        player.SwitchTo(state);
                }
            }
        }

    }
}