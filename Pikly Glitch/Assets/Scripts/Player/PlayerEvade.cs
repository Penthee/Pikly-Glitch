using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
//using Pikl.UI;
using DG.Tweening;
//using Pikl.Audio;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerEvade {
        [HideInInspector]
        public Player player;

        //[HideInInspector]
        public float lastTime, lastOverheatTime, overheatCooldown;
        public bool coolingDown;

        //public ParticleSystem Particles {
        //    get { return particles; }
        //    set { particles = value; }
        //}

        public float TrailFadeTime {
            get { return trailFadeTime; }
            set { trailFadeTime = value; }
        }

        public float Force {
            get { return force; }
            set { force = value; }
        }

        public float Cooldown {
            get { return cooldown; }
            set { cooldown = value; }
        }

        public float MaxStamina {
            get { return maxStamina; }
            set { maxStamina = value; }
        }

        public float Stamina {
            get { return stamina; }
            set {
                stamina = value;
                if (stamina <= 0) {
                    coolingDown = true;
                    //DOTween.To(() => (UIMgr.I.CurrentMenu as GameUI).staminaGlow.color, x => (UIMgr.I.CurrentMenu as GameUI).staminaGlow.color = x, Color.white, 0.25f).SetEase(Ease.Linear);
                    lastOverheatTime = Time.time;
                    hasReleasedSinceLastEvade = true;
                    //AudioMgr.I.PlaySound("SFX/PlayerOverheat");
                    //player.overheatSteam.Play();
                }
            }
        }

        public float EvadeCost {
            get { return evadeCost; }
            set { evadeCost = value; }
        }

        public float StaminaRecoverRate {
            get { return staminaRecoverRate; }
            set { staminaRecoverRate = value; }
        }

        //[SerializeField]
        public ParticleSystem particles;
        
        [SerializeField]
        float trailFadeTime, force, cooldown,
              maxStamina, staminaRecoverRate,
              stamina, evadeCost;

        public bool hasReleasedSinceLastEvade = true;

        public void Init(Player player) {
            this.player = player;

            player.StartCoroutine(RecoverStamina());

            //particles = player.transform.FindChild("Evade Particles").GetComponent<ParticleSystem>();
        }

        public void Update() {
            if (coolingDown && lastOverheatTime + overheatCooldown < Time.time) {
                coolingDown = false;
                //DOTween.To(() => (UIMgr.I.CurrentMenu as GameUI).staminaGlow.color, x => (UIMgr.I.CurrentMenu as GameUI).staminaGlow.color = x, Color.clear, 0.25f).SetEase(Ease.Linear);
            }
        }

        int stamVal = 0;
        IEnumerator RecoverStamina() {
            while (true) { 
            //while (!player.health.isDead) {
                yield return new WaitForSeconds(StaminaRecoverRate);
                if (!coolingDown) {

                    stamVal = 1 * (player.input.EvadeAxis ? -1 : 1);

                    Stamina = Mathf.Clamp(Stamina + stamVal, 0, MaxStamina);
                    //if (Stamina % EvadeCost == 0 && stamVal == 1 && UIMgr.I.CurrentMenu is GameUI)
                    //    UIMgr.I.StartCoroutine((UIMgr.I.CurrentMenu as GameUI).FlashStaminaBar());
                }
            }
        }

    }
}