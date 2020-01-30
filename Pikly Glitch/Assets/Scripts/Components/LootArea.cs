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
        [Range(0, 6)] public int minimumItemCount;
        [Range(1, 6)] public int maximumItemCount;

        public Bounds area;

        [SerializeField] List<Item> items = new List<Item>();

        Transform _t;
        
        void Awake() {
            _empty = Resources.Load("Prefabs/Items/Empty Item") as GameObject;
            _t = transform;
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
            return new Vector3(Random.Range(-area.extents.x, area.extents.x), Random.Range(-area.extents.y, area.extents.y), 0);
        }

        Quaternion GetRotation() {
            return Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        }
        void Update() {
        
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(area.center, area.size);
        }
    }
}
