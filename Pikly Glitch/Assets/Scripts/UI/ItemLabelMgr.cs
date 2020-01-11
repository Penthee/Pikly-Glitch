using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pikl {
    public class ItemLabelMgr : Singleton<ItemLabelMgr> {

        public GameObject label;
        public Vector3 offset;
        public int poolSize = 25;

        public Dictionary<Transform, GameObject> pool = new Dictionary<Transform, GameObject>();
        
        List<GameObject> labels = new List<GameObject>();
        
        public override void Start() {
            CreatePool();
        }

        void CreatePool() {
            GameObject o;

            for(int i = 0; i < poolSize; i ++) {
                o = Instantiate(label, transform) as GameObject;
                o.SetActive(false);

                labels.Add(o);
            }
        }

        GameObject GetLabel() {
            if (labels.Count > 0) {
                var o = labels[0];
                labels.RemoveAt(0);
                return o;
            } else {
                return null;
            }
        }

        public void CreateNewLabel(string name, Transform parent) {
            if (pool.ContainsKey(parent))
                return;

            var obj = GetLabel();
            
            if (obj == null)
                return;

            obj.transform.position = parent.position + offset;
            obj.GetComponent<Text>().text = name;
            obj.SetActive(true);

            pool.Add(parent, obj);
        }

        public void Update() {
            UpdateMovement();
        }

        void UpdateMovement() {
            foreach(var item in pool) {
                item.Value.transform.position = item.Key.position + offset;
            }
        }

        public void RemoveLabel(Transform t) {
            if (!pool.ContainsKey(t))
                return;

            pool[t].SetActive(false);
            labels.Add(pool[t]);
            pool.Remove(t);
        }
    }
}