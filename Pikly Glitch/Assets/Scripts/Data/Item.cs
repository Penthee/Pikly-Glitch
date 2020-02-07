using Pikl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    public enum ItemType {
        None,
        Weapon,
        Throwable,
        Material,
        Consumable,
        Tool,
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Data/Interactable/New Item")]
    public class Item : Interactable {

        public string description;
        public int quantity = 1, maxStack;
        public ItemType type = ItemType.None;
        public bool selected; 

        public override void Interact() {
            base.Interact();

            PickUp();
        }

        public virtual void PickUp() {
            //Supposed to be empty
        }

        public virtual void Use() {
            Player.Player.I.lastUseTime = Time.time;
            Player.Player.I.input.useLocked = true;
            quantity--;
        }

        public virtual void Drop() {
            //Supposed to be empty
        }

        public virtual void Init(Item i) {
            Init(i.name, i.sprite, i.description, i.type, i.quantity, i.maxStack);
        }

        public virtual void Init(string name, Sprite sprite, string description, ItemType type, int quantity, int maxStack) {
            Init(name, sprite);
            this.description = description;
            this.type = type;
            this.quantity = quantity == 0 ? 1 : quantity;
            this.maxStack = maxStack;
        }

        public static Item CreateInstance(Item item) {
            var data = CreateInstance<Item>();
            data.Init(item.name, item.sprite, item.description, item.type, item.quantity, item.maxStack);
            return data;
        }
        
        public static Item CreateInstanceExt(Item item) {
            switch(item.type) {
                default: return Item.CreateInstance(item);
                case ItemType.Weapon: return Weapon.CreateInstance(item as Weapon);
                case ItemType.Throwable: return Throwable.CreateInstance(item as Throwable);
                case ItemType.Consumable: return Consumable.CreateInstance(item as Consumable);
                case ItemType.Material: return Data.Material.CreateInstance(item as Data.Material);
                case ItemType.Tool: return Tool.CreateInstance(item as Tool);
            }
        }
    }
}