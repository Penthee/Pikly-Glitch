using System.Collections.Generic;
using NaughtyAttributes;
using Pikl.Data;
using Pikl.Interaction;
using UnityEngine;

namespace Pikl.Components {
    public class LootBox : InteractableObj {

        [BoxGroup("Loot Box Settings")][Range(1, 5)] public int minimumItemCount;
        [BoxGroup("Loot Box Settings")][Range(2, 5)] public int maximumItemCount;

        [ReadOnly][SerializeField] List<Item> items = new List<Item>();
        Transform _t;
        
        internal override void Awake() {
            _t = transform;
        }

        public void SetItems(List<Item> newItems) {
            if (newItems != null)
                items = newItems;
        }

        public void GiveItem(Item item) {
            if (item != null)
                items.Add(item);
        }
        
        public override void Interact() {
            //TODO: Create UI and allow player to select items from the box
            foreach (Item i in items) {
                Player.Player.I.inventory.Add(i);
            }

            Clear();
            
            base.Interact();
        }

        public override void Update() {
            base.Update();
        }

        public void Clear() {
            items.Clear();
        }
    }
}
