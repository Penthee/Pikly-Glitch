using UnityEngine;

namespace Pikl.States {
    public class EntityStateObject : StateObject {
        #region Fields & Initialization
        
        [HideInInspector]
        public Color originalSpriteColour;
        [HideInInspector]
        public Color originalColour;
        #endregion

        internal override void Start() {
            base.Start();
            
            if (renderer == null) {
               // originalSpriteColour = e_renderer.Color;
               // originalColour = e_renderer.Material.color;
            } else {
                originalSpriteColour = renderer.color;
                originalColour = renderer.material.color;
            }
        }

        #region Collision
        internal virtual void OnCollisionEnter2D(Collision2D other) {
            State _state = state.Peek().OnCollisionEnter2D(other);
            if (_state != null)
                SwitchTo(_state);
        }

        internal virtual void OnTriggerEnter2D(Collider2D other) {
            State _state = state.Peek().OnTriggerEnter2D(other);
            if (_state != null)
                SwitchTo(_state);
        }
        #endregion


    }
}