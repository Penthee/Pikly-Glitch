using Pikl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pikl.Data {
    [CreateAssetMenu(fileName = "New Drop Table", menuName = "Data/New Drop Table")]
    public class Droppable : ScriptableObject {

        public List<Item> items;

        public Item GetItem() {
            return null;
        }
    }
}