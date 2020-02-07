using UnityEngine;
using Pikl.Data;

namespace Pikl.Interaction {
    [System.Serializable]
    public class ItemPickup : InteractableObj {

        [Expandable]
        public Item item;
        public bool randomise;

        internal override void Start() {
            base.Start();
            
            if (item == null) return;
            SetSprite();
            SetName();
            SetQuantity();
        }

        public override void Interact() {
            base.Interact();

            if (Player.Player.I.inventory.Add(item)) {
                PickUp();
            }
        }

        void PickUp() {
            Debug.Log("Picked up " + item.name);
            Destroy(gameObject);
        }

        public void SetValues() {
            SetName();
            SetQuantity();
            SetSprite();
        }

        void SetName() {
            name = item.name;
        }

        void SetQuantity() {
            if (item.quantity == 0)
                item.quantity = 1;
        }

        void SetSprite() {
            if (item.sprite != null && GetComponent<SpriteRenderer>() != null) {
                GetComponent<SpriteRenderer>().sprite = item.sprite;
            }
        }

        void OnDrawGizmosSelected() {
            SetValues();
        }
    }
}