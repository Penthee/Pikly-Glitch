using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerPowerup {
        Player _player;

        [BoxGroup("Powerup Settings")] public float berserkTime, resistanceTime, speedTime;
        [BoxGroup("Powerup Settings")] public float berserkFireRateMultiplier, berserkDamageMultiplier, 
            resistDamageMultiplier, speedMoveMultiplier, speedStaminaMultiplier, speedReloadMultiplier;
        
        [ReadOnly] public float berserkTimeRemaining, resistanceTimeRemaining, speedTimeRemaining;
        [ReadOnly] public bool berserkActive, resistanceActive, speedActive;
        
        public void Init(Player player) {
            _player = player;
        }
        public void Update() {
            if (berserkActive) CountdownBerserk();
            if (resistanceActive) CountdownResistance();
            if (speedActive) CountdownSpeed();
        }
        public void Berserk() { //Fire Candy
            if (berserkTimeRemaining <= 0) {
                _player.shoot.damageMultiplier = berserkDamageMultiplier;
                _player.shoot.fireRateMultiplier = berserkFireRateMultiplier;
            }

            berserkTimeRemaining += berserkTime;
            berserkActive = true;
        }
        public void Resistance() { //Relaxant
            if (resistanceTimeRemaining <= 0) 
                _player.health.damageMultiplier *= resistDamageMultiplier;
            
            resistanceTimeRemaining += resistanceTime;
            resistanceActive = true;
        }
        public void Speed() { //Stimulant
            if (speedTimeRemaining <= 0) {
                _player.move.force *= speedMoveMultiplier;
                _player.evade.StaminaRecoverRate *= speedStaminaMultiplier;
                _player.shoot.reloadSpeedMultiplier = speedReloadMultiplier;
            }

            speedTimeRemaining += speedTime;
            speedActive = true;
        }
        void CountdownBerserk() {
            berserkTimeRemaining -= Time.deltaTime;
            
            if (berserkTimeRemaining <= 0)
                DisableBerserk();
        }
        void CountdownResistance() {
            resistanceTimeRemaining -= Time.deltaTime;
            
            if (resistanceTimeRemaining <= 0)
                DisableResistance();
        }
        void CountdownSpeed() {
            speedTimeRemaining -= Time.deltaTime;
            
            if (speedTimeRemaining <= 0)
                DisableSpeed();
        }
        void DisableBerserk() {
            _player.shoot.damageMultiplier = 1;
            _player.shoot.fireRateMultiplier = 1;
            berserkActive = false;
        }
        void DisableResistance() {
            _player.health.damageMultiplier = 1;
            resistanceActive = false;
        }
        void DisableSpeed() {
            _player.move.force = _player.move.originalForce;
            _player.evade.StaminaRecoverRate = _player.evade.origStaminaRecoverRate;
            _player.shoot.reloadSpeedMultiplier = 1;
            speedActive = false;
        }
    }
}