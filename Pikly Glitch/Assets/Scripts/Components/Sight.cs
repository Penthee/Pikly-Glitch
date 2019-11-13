using UnityEngine;
using Pikl.States;
using Pikl.Extensions;

namespace Pikl.Components {
    [System.Serializable]
    public class Sight {
        public float range;
        public float noLOSChaseTime;
        public LayerMask playerLayerMask;
        public LayerMask blockingLayers;

        internal StateObject so;

        public void Init(StateObject so) {
            this.so = so;
        }

        /// <summary>
        /// Determines whether this instance can see the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public bool CanSeePlayer(Vector2 position) {
            Vector2 direction = (position - so.transform.position.To2DXY()).normalized;

            RaycastHit2D blockingHit = Physics2D.Raycast(so.transform.position, direction, range, blockingLayers.value);

            RaycastHit2D hit = Physics2D.Raycast(so.transform.position, direction, range, playerLayerMask.value);

            return hit && (blockingHit ? blockingHit.distance > hit.distance : true) ? hit.transform.name == "Player" : false;
        }
        
    }
}