using UnityEngine;
using System.Collections.Generic;
using Pikl.States.Components;
using Pikl.States;
using TeamUtility.IO;
using Pikl.Data;
using Pikl.Interaction;

namespace Pikl.Player {
    [System.Serializable]
    public class Inventory {
        
        public Item SelectedItem {
            get {
                return items[SelectedIndex];
            }
            private set { items[SelectedIndex] = value; }
        }

        public ItemType SelectedType {
            get { return items[selectedIndex].type; }
        }

        public int SelectedIndex {
            get {
                return selectedIndex;
            }
            private set {
                if (value < 0 || value >= size)
                    return;

                for (int i = 0; i < size; i++) {
                    if (items[i] != null)
                        items[i].selected = value == i;
                }

                selectedIndex = value;
            }
        }
        [Expandable]
        public List<Item> items;
        public GameObject emptyItemObj;
        public Item emptyItem;
        public Data.Material emptyCan;
        Player player;
        public readonly int size = 20;
        public readonly float dropDist = 0.85f;
        [SerializeField]
        int selectedIndex = 0;

        public void Init(Player player) {
            this.player = player;

            for(int i = 0; i < size; i++) {
                items.Add(Item.CreateInstance(emptyItem));
            }


            items[0].selected = true;
        }

        public void Update() {
            float moveInput = 0, reorderInput = 0;

            if (!(UI.UIMgr.I.gameUI as UI.GameUI).craftingUI.activeSelf) {
                moveInput = -InputMgr.GetAxisRaw("Mouse Scrollwheel");
                reorderInput = InputMgr.GetAxisRaw("Reorder");
            }

            Weapon w = (SelectedItem as Weapon);

            if (reorderInput > 0) {
                int newIndex = selectedIndex + (int)moveInput;
                if (newIndex >= 0 && newIndex < size) {
                    Item current = items[selectedIndex];
                    Item swap = items[newIndex];

                    items[selectedIndex] = swap;
                    items[newIndex] = current;
                }
            }

            if (!(UI.UIMgr.I.gameUI as UI.GameUI).craftingUI.activeSelf) {
                if (moveInput != 0 && (w == null || !w.reloading))
                    SelectedIndex += (int)moveInput;
            }

            DeleteZeroQuantities();
        }

        void DeleteZeroQuantities() {
            for (int i = 0; i < size; i++) {
                if (items[i].quantity <= 0)
                    Delete(i);
            }
        }

        public int ItemCount(Item item) {
            int count = 0;

            foreach(Item i in items) {
                if (i.name == item.name)
                    count += i.quantity;
            }

            return count;
        }

        public bool Add(Item item) {
            
            Item existing = items.Find(e => e.name == item.name && e.quantity < e.maxStack);
            if (existing != null) {
                //(existing as Weapon).totalAmmo += (item as Weapon).totalAmmo + (item as Weapon).clipAmmo;
                //if (item as Weapon == null) {
                //possibly change item.quantity to existing.maxStack?

                if (existing.quantity < existing.maxStack) {
                    Debug.Log("Item already exists, stacking...");

                    int transfer = Mathf.Clamp(item.quantity, 1, existing.maxStack - existing.quantity);

                    if (transfer > existing.maxStack - existing.quantity) {
                        Item newItem = Item.CreateInstance(item);
                        newItem.quantity -= transfer;
                        CreatePickup(newItem);
                    }

                    existing.quantity += transfer;

                    return true;
                } else {
                    Debug.Log("Item already exists, making new stack...");
                    return AddNew(item);
                }
                //}
            } else {
                return AddNew(item);
            }
        }

        bool AddNew(Item item) {
            Item empty = items.Find(e => e.name == "Empty");
            if (empty != null) {
                Item newItem = CreateInstance(item);

                if (empty.selected) newItem.selected = true;
                items[items.IndexOf(empty)] = newItem;

                Debug.Log("New item added, " + newItem.name);
                return true;
            } else {
                Debug.Log("Inventory already full, cannot pick up.");
                return false;
            }
        }

        Item CreateInstance(Item item) {
            switch(item.type) {
                default: return Item.CreateInstance(item);
                case ItemType.Weapon: return Weapon.CreateInstance(item as Weapon);
                case ItemType.Throwable: return Throwable.CreateInstance(item as Throwable);
                case ItemType.Consumable: return Consumable.CreateInstance(item as Consumable);
                case ItemType.Material: return Data.Material.CreateInstance(item as Data.Material);
                //case ItemType.Tool: return Tool.CreateInstance(item as Tool);
            }
        }

        public void Drop() {
            if (SelectedItem.name == "Empty")
                return;

            if (SelectedType == ItemType.Weapon) {
                List<Item> existing = items.FindAll(e => e.name == SelectedItem.name);
                if (existing.Count > 1) {
                    Item i = items.Find(e => e.name == SelectedItem.name && e.GetHashCode() != SelectedItem.GetHashCode());
                    (i as Weapon).totalAmmo += (SelectedItem as Weapon).totalAmmo + (SelectedItem as Weapon).clipAmmo;
                    (SelectedItem as Weapon).totalAmmo = 0;
                    (SelectedItem as Weapon).clipAmmo = 0;
                }
            }

            CreatePickup(SelectedItem);

            Item newEmpty = Item.CreateInstance(emptyItem);
            newEmpty.selected = true;
            SelectedItem = newEmpty;
        }

        public void DeleteForCraft(Item item, int quantityToDelete) {
            int quantityDeleted = 0;

            for(int i = 0; i < items.Count; i ++) {
                if (items[i].name == item.name) {
                    //if the current item stack has more than enough
                    //remove quantity from this stack
                    //tally removed
                    //else if the player does not have enough in this stack
                    //delete this stack, tally it's quantity
                    //move to the next stack
                    if (items[i].quantity >= quantityToDelete) {
                        //remove some from stack
                        items[i].quantity -= quantityToDelete - quantityDeleted;
                        quantityDeleted += quantityToDelete;
                    } else {
                        //use whole stack
                        quantityDeleted += items[i].quantity;
                        items[i].quantity = 0;
                    }

                    if (items[i].quantity <= 0) {
                        Delete(items[i]);
                    }
                }

                if (quantityDeleted >= quantityToDelete)
                    return;
            }
        }

        void Delete(Item item) {
            if (!items.Find(e => e.name == item.name))
                return;

            Item newEmpty = Item.CreateInstance(emptyItem);
            newEmpty.selected = item.selected;
            items[items.IndexOf(items.Find(e => e.name == item.name))] = newEmpty;
            //item = newEmpty;
        }

        void Delete(int index) {
            if (items[index].name == "Empty")
                return;

            Item newEmpty = Item.CreateInstance(emptyItem);
            newEmpty.selected = items[index].selected;
            items[index] = newEmpty;
        }

        void DeleteSelected() {
            if (SelectedItem.name == "Empty")
                return;

            Item newEmpty = Item.CreateInstance(emptyItem);
            newEmpty.selected = true;
            SelectedItem = newEmpty;
        }

        void CreatePickup(Item i) {
            GameObject o = Object.Instantiate(emptyItemObj, player.transform.position + (player.fv2D.lastDir.normalized * dropDist), Quaternion.identity);
            o.GetComponent<ItemPickup>().item = i;

            //switch(i.type) {
            //    case ItemType.None: break;
            //    case ItemType.Consumable:
            //    case ItemType.Material:
            //    case ItemType.Tool:
            //        break;
            //    case ItemType.Weapon:
            //        o.AddComponent<WeaponPickup>().weapon = new Weapon(i as Weapon);
            //        break;
            //}
        }
        
    }
}