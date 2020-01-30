using System;
using Pikl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pikl.Utils.RDS;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pikl.Data {
    [Serializable] [CreateAssetMenu(fileName = "New Drop Table", menuName = "Data/New Drop Table")]
    public class DropTable : ScriptableObject {
        
        [SerializeField] public List<RDSItem> items;

        RDSTable table;

        public void Init() {
            foreach (RDSItem item in items)
                item.rdsProbability = item.probability;
            
            table = new RDSTable(items);
        }
        
        public Item GetItem() {
            RDSItem result = table.rdsResult.FirstOrDefault() as RDSItem;
            if (result != null)
                return result.item;
            return null;
        }
    }
}