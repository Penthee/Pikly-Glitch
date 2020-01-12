using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;
using Pikl.States;
using Pikl.States.Components;

namespace Pikl.UI {
    public class DebugUI : MonoBehaviour {
        public Player.Player player;

        public GameObject debugPanel;
        public Text debugTitle, debugValues;
        public Toggle invulnToggle;
        public bool debug;

        public void Start() {

            StartCoroutine(GetPlayer());

#if UNITY_EDITOR
            debug = true;
#endif 
        }


        public void Update() {
            if (player != null) {
#if UNITY_EDITOR
                if (debug) {
                    debugPanel.SetActive(true);
                    UpdateDebugValues();

                } else {
                    debugPanel.SetActive(false);
                }
#endif
            }
        }

        IEnumerator GetPlayer() // In case the player isn't immediately available.
        {
            do {
                //player = Ref.I["PlayerScript"] as Player.Player;
                var bah = GameObject.Find("Player");
                if (bah)
                    player = bah.GetComponent<Player.Player>();

                yield return new WaitForEndOfFrame();
            } while (player == null);

            lastSelected = player;
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
                } else {
                    invulnToggle.isOn = GetInvuln(so);
                    lastSelected = so;
                    return so;
                }

            } else {
                return null;
            }
#else
            return null;
#endif
        }

        void UpdateDebugValues() {
            StateObject selected = GetSelected();

            if (selected != null) {
                debugTitle.text = "Debug - " + selected.name;

                debugValues.text = string.Empty;

                debugValues.text += string.Format("{0:f1}, {1:f1}", selected.rb.velocity.x, selected.rb.velocity.y);
                debugValues.text += System.Environment.NewLine;
                debugValues.text += selected.CurrentState.ToString().Split('.').Last();
                debugValues.text += System.Environment.NewLine;

                foreach (var item in selected.asyncStates) {
                    debugValues.text += "(A) " + item.Value.ToString().Split('.').Last() + " ";
                }
            }
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

        public void OnInvulnToggle(Toggle value) {
            StateObject selected = GetSelected();
            if (selected == null) {
                Debug.Log("SO null in invulnToggle()");
                return;
            }

            var health = selected.GetComponent<MonoHealth>();

            if (health != null) {
                health.Invulnerable = value.isOn;
                return;
            }

            var pHealth = selected.GetComponent<PlayerHealth>();

            if (pHealth != null) {
                pHealth.Invulnerable = value.isOn;
            }
        }

        public void OnInfiniteHPPress() {
            StateObject selected = GetSelected();
            if (selected == null) {
                Debug.Log("SO null in OnInfiniteHPPress()");
                return;
            }

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

    }
}