using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Pikl.Components;
using Pikl.Data;
using Pikl.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pikl.Control {
    public class ItemRandomiser : MonoBehaviour {

        public BetterLevelRandomiser levelRandomiser;
        
        [ReadOnly][SerializeField] DropTable _dropTable;
        
        [ReadOnly][SerializeField] LootBox[] _lootBoxes;
        [ReadOnly][SerializeField] LootArea[] _rngAreas;
        [ReadOnly][SerializeField] IEnumerable<ItemPickup> _itemPickups;

        void Awake() {
            LoadTable();
        }
        public void Reset() {
            ClearGivenLoot();
            LoadTable();
        }
        void LoadTable() {
            _dropTable = ScriptableObject.CreateInstance<DropTable>();
            _dropTable.Init();
        }
        void FindAllRNGItems() {
            _lootBoxes = FindObjectsOfType(typeof(LootBox)) as LootBox[];
            _rngAreas = FindObjectsOfType(typeof(LootArea)) as LootArea[];
            _itemPickups = (FindObjectsOfType(typeof(ItemPickup)) as ItemPickup[]).Where(e => e.randomise);
        }
        [Button]
        public void Randomise() {
            if (levelRandomiser.state != RandomiserState.Success) {
                    Debug.Log("Cannot randomise items, level randomiser has not succeeded");
                return;
            }
            
            FindAllRNGItems();

            Item itemToGive = null;
            
            foreach (LootArea area in _rngAreas) {
                int itemsToGive = Random.Range(area.minimumItemCount, area.maximumItemCount + 1);
                for (int i = 0; i < itemsToGive; i++) {
                    itemToGive = area.preferredItem ? _dropTable.GetPreferredItem(area.preferredItem) : _dropTable.GetItem();
                    if (!itemToGive) break;
                    
                    area.GiveItem(itemToGive);
                }
            }
            
            foreach (LootBox box in _lootBoxes) {
                int itemsToGive = Random.Range(box.minimumItemCount, box.maximumItemCount + 1);
                for (int i = 0; i < itemsToGive; i++) {
                    itemToGive = _dropTable.GetItem();
                    if (!itemToGive) break;
                    
                    box.GiveItem(itemToGive);
                }
            }
            
            foreach (ItemPickup pickup in _itemPickups) {
                itemToGive = _dropTable.GetItem();
                if (!itemToGive) break;
                
                pickup.item = itemToGive;
                pickup.SetValues();
            }
        }

        void ClearGivenLoot() {
            foreach (LootArea area in _rngAreas)
                area.Clear();
            foreach (LootBox box in _lootBoxes)
                box.Clear();
            //Individual items don't need clearing, fully overwritten on next randomisation.
        }
        
    }
}
