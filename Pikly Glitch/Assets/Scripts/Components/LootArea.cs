using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Pikl.Data;
using Pikl.Interaction;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pikl.Components {
    public class LootArea : MonoBehaviour {

        [ReadOnly][SerializeField]GameObject _empty;
        [BoxGroup("Loot Area Settings")][Range(0, 6)] public int minimumItemCount;
        [BoxGroup("Loot Area Settings")][Range(1, 6)] public int maximumItemCount;
        [BoxGroup("Loot Area Settings")]public Item preferredItem;
        [BoxGroup("Loot Area Settings")]public BoxCollider2D area;

        [ReadOnly][SerializeField] List<Item> items = new List<Item>();

        Transform _t;
        Vector2 _extents;
        void Awake() {
            _empty = Resources.Load("Prefabs/Items/Empty Item") as GameObject;
            _t = transform;
            _extents = area.size * 0.5f;
        }
        public void GiveItem(Item item) {
            if (item != null)
                items.Add(item);

            PlaceItem(item);
        }
        void PlaceItem(Item item) {
            GameObject obj = Instantiate(_empty, GetPosition(), GetRotation(), _t);
            obj.GetComponent<ItemPickup>().item = item;
        }
        Vector3 GetPosition() {
            return _t.position + new Vector3(Random.Range(-_extents.x, _extents.x), Random.Range(-_extents.y, _extents.y), 0);
        }
        Quaternion GetRotation() {
            return Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        }

        public void Clear() {
            for (int i = 0; i < _t.childCount; i++) 
                Destroy(_t.GetChild(i).gameObject);

            items.Clear();
        }
    }
}
