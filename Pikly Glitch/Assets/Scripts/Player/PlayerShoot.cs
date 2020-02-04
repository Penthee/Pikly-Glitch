using UnityEngine;
using System.Linq;
using System.Collections;
using Pikl.States;
using Pikl.States.Components;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Pikl.Player {
    [System.Serializable]
    public class PlayerShoot : Shoot {
        public LayerMask aimLayers, aimBlockLayers;
        [HideInInspector]
        public Vector2 position;

        [ReadOnly] public float damageMultiplier = 1;
        [ReadOnly] public float reloadSpeedMultiplier = 1;
        [ReadOnly] public float fireRateMultiplier = 1;
    }
}