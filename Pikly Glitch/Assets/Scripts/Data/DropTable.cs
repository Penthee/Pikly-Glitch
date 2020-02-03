using System;
using Pikl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pikl.Utils.RDS;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Pikl.Data {
    [Serializable] [CreateAssetMenu(fileName = "New Drop Table", menuName = "Data/New Drop Table")]
    public class DropTable : ScriptableObject {
        
        [SerializeField] List<RDSItem> items;

        RDSTable _table;

        public void Init() {

            List<RDSItem> rdsItems = (Resources.Load(string.Concat("Data/DropTables/", SceneManager.GetActiveScene().name)) as DropTable)?.items;
            
            if (rdsItems == null) {
                Debug.Log($"Empty Drop Table for {SceneManager.GetActiveScene().name}");
                return;
            }
            
            items = new List<RDSItem>();
            foreach (RDSItem item in rdsItems)
                items.Add(new RDSItem(item.item, item.probability, item.supply));

            _table = new RDSTable(items);
        }
        
        public Item GetItem() {
            return (_table.rdsResult.FirstOrDefault() as RDSItem)?.item;
        }

        public Item GetPreferredItem(Item preferred) {
            List<RDSItem> list = _table.mcontents.Cast<RDSItem>().ToList();
            if (list.All(e => e.item.name != preferred.name))
                return null;

            RDSItem item = list.FirstOrDefault(e => e.item.name == preferred.name);
            if (item == null) return null;
            
            if (--item.supply <= 0)
                _table.RemoveEntry(item);
            
            return item.item;
        }
    }
}