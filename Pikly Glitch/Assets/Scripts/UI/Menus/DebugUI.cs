using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Pikl.Extensions;
using Luminosity.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;
using Pikl.Components;
using Pikl.Interaction;
using Pikl.Player;
using Pikl.States;
using Pikl.States.Components;

namespace Pikl.UI {
    public class DebugUI : MonoBehaviour {
        public Player.Player player;

        public GameObject debugPanel, container;
        public Text debugTitle, debugValues;
        public Dropdown itemList;
        public bool debug;

        List<GameObject> itemObjectList = new List<GameObject>();

        public void Start() {
#if UNITY_EDITOR || DEBUG
            debug = true;
#else
            return;
#endif
            FindPlayer();

            string[] folders = new[] {"Consumables", "Materials", "Explosives", "Tools", "Weapons"};
            for (int i = 0; i < folders.Length; i++) {
                var items = Resources.LoadAll("Prefabs/Items/" + folders[i], typeof(GameObject))
                    .Cast<GameObject>().ToArray();

                var options = new List<Dropdown.OptionData>();
                foreach (GameObject item in items) {
                    itemObjectList.Add(item);
                    options.Add(new Dropdown.OptionData(item.name));
                }

                itemList.AddOptions(options);
                itemList.value = 1;
                itemList.value = 0;
            }
        }


        public void Update() {
            if (player != null) {
#if UNITY_EDITOR
                if (debug) {
                    debugPanel.SetActive(true);
                    UpdateDebugValues();
                }
                else {
                    debugPanel.SetActive(false);
                }
#endif
            }
        }

        public void FindPlayer() {
            StartCoroutine(FindPlayerCoroutine());
        }

        IEnumerator FindPlayerCoroutine() {
            do {
                //player = Ref.I["PlayerScript"] as Player.Player;
                var bah = GameObject.Find("Player");
                if (bah)
                    player = bah.GetComponent<Player.Player>();

                yield return new WaitForEndOfFrame();
            } while (player == null);

            lastSelected = player;
        }

        public void DisableUseActions() {
            if (player != null)
                player.input.DisableUseActions();
        }

        public void EnableUseActions() {
            if (player != null)
                player.input.EnableUseActions();
        }

        StateObject lastSelected;

        StateObject GetSelected() {
#if UNITY_EDITOR
            GameObject selected = UnityEditor.Selection.activeObject as GameObject;
            if (selected) {
                StateObject so = selected.GetComponent<StateObject>();
                if (so == null)
                    return lastSelected;

                if (lastSelected == so) {
                    return so;
                }
                else {
                    lastSelected = so;
                    return so;
                }
            }
            else {
                return null;
            }
#else
            return null;
#endif
        }

        void UpdateDebugValues() {
            StateObject selected = GetSelected();

            if (selected == null) {
                debugTitle.text = "Debug - None selected";
                return;
            }

            try {

                debugTitle.text = "Debug - " + selected.name;

                debugValues.text = string.Empty;

                debugValues.text += string.Format("{0:f1}, {1:f1}", selected.rb.velocity.x, selected.rb.velocity.y);
                debugValues.text += System.Environment.NewLine;
                debugValues.text += selected.CurrentState.ToString().Split('.').Last();
                debugValues.text += System.Environment.NewLine;

                foreach (var item in selected.asyncStates) {
                    debugValues.text += "(A) " + item.Value.ToString().Split('.').Last() + " ";
                }
            } catch { }
        }

        bool GetInvuln(StateObject so) {
            if (so == null)
                return false;

            var health = so.GetComponent<MonoHealth>();

            if (health != null) {
                return health.Invulnerable;
            }

            var pHealth = so.GetComponent<PlayerHealth>();

            if (pHealth != null) {
                return pHealth.Invulnerable;
            }

            return false;
        }

        public void OnInvulnToggle() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            var health = selected.GetComponent<MonoHealth>();

            if (health != null) {
                health.Invulnerable = !health.Invulnerable;
                return;
            }

            var pHealth = selected.GetComponent<PlayerHealth>();

            if (pHealth != null) {
                pHealth.Invulnerable = !pHealth.Invulnerable;
            }
        }

        public void OnInfiniteHPPress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            var health = selected.GetComponent<MonoHealth>();

            if (health != null) {
                health.MaxHp = 999999;
                health.HP = health.MaxHp;
                return;
            }

            var pHealth = selected.GetComponent<PlayerHealth>();

            if (pHealth != null) {
                pHealth.MaxHp = 999999;
                pHealth.HP = pHealth.MaxHp;
            }
        }

        public void OnInfiniteStaminaPress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            PlayerEvade evade = selected.GetComponent<Player.Player>().evade;

            if (evade != null) {
                evade.StaminaRecoverRate = 0;
            }
        }

        public void OnDamagePress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            PlayerKnife knife = selected.GetComponent<Player.Player>().knife;

            if (knife != null) {
                knife.obj.GetComponent<DamageObject>().damage.baseDmg = 9999;
            }
        }

        public void OnSpeedPress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            PlayerMovement movement = selected.GetComponent<Player.Player>().move;

            if (movement != null) {
                movement.force *= 3;
                movement.walkForce *= 3;
            }
        }

        public void OnCollisionPress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            var player = selected.GetComponent<Player.Player>();
            if (player == null) return;
            foreach (Collider2D c in player.GetComponents<Collider2D>()) {
                c.enabled = !c.enabled;
            }
        }

        public void OnGiveItemPress() {
            StateObject selected = GetSelected();
            if (selected == null) return;

            var player = selected.GetComponent<Player.Player>();
            if (player == null) return;
            player.inventory.Add(itemObjectList[itemList.value].GetComponent<ItemPickup>().item);
        }

        public void ShowHidePress() {
            container.SetActive(!container.activeSelf);
        }
    }
}