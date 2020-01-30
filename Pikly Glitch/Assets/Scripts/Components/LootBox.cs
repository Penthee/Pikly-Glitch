using System.Collections.Generic;
using Pikl.Data;
using Pikl.Interaction;
using UnityEngine;

namespace Pikl.Components {
    public class LootBox : InteractableObj {

        [Range(1, 5)] public int minimumItemCount;
        [Range(2, 5)] public int maximumItemCount;

        [SerializeField] List<Item> items = new List<Item>();
        
        void Start() {
            
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
            base.Interact();
        }

        public override void Update() {
            base.Update();
        }
    }
}
