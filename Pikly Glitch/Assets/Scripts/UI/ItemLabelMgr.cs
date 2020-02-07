using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Pikl.Interaction;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pikl.UI {
    public class ItemLabelMgr : Singleton<ItemLabelMgr> {

        public GameObject label, trapMarker, enemyMarker;
        public Vector3 offset;
        public int labelPoolSize = 10, trapMarkerPoolSize = 6, enemyMarkerPoolSize = 25;
        
        Dictionary<Transform, GameObject> _activeLabels = new Dictionary<Transform, GameObject>();
        Dictionary<Transform, GameObject> _activeTrapMarkers = new Dictionary<Transform, GameObject>();
        Dictionary<Transform, GameObject> _activeEnemyMarkers = new Dictionary<Transform, GameObject>();
        
        Stack<GameObject> _labelPool = new Stack<GameObject>();
        Stack<GameObject> _trapMarkerPool = new Stack<GameObject>();
        Stack<GameObject> _enemyMarkerPool = new Stack<GameObject>();
        
        public override void Start() {
            CreatePools();
            
            DontDestroyOnLoad(this);
        }
        void CreatePools() {
            GameObject o;

            for(int i = 0; i < labelPoolSize; i ++) {
                o = Instantiate(label, transform);
                o.SetActive(false);

                _labelPool.Push(o);
            }
            
            for(int i = 0; i < trapMarkerPoolSize; i ++) {
                o = Instantiate(trapMarker, transform);
                o.SetActive(false);

                _trapMarkerPool.Push(o);
            }
            
            for(int i = 0; i < enemyMarkerPoolSize; i ++) {
                o = Instantiate(enemyMarker, transform);
                o.SetActive(false);

                _enemyMarkerPool.Push(o);
            }
        }
        GameObject GetLabel() {
            return _labelPool.Count > 0 ? _labelPool.Pop() : null;
        }
        GameObject GetTrapMarker() {
            return _trapMarkerPool.Count > 0 ? _trapMarkerPool.Pop() : null;
        }
        GameObject GetEnemyMarker() {
            return _enemyMarkerPool.Count > 0 ? _enemyMarkerPool.Pop() : null;
        }
        public void Update() {
            UpdateMovement();
        }
        void UpdateMovement() {
            foreach(KeyValuePair<Transform, GameObject> item in _activeLabels)
                item.Value.transform.position = item.Key.position + offset;

            foreach (KeyValuePair<Transform, GameObject> item in _activeTrapMarkers)
                item.Value.transform.position = item.Key.position;

            foreach (KeyValuePair<Transform, GameObject> item in _activeEnemyMarkers)
                item.Value.transform.position = item.Key.position;
        }
        public void ShowLabel(string labelText, Transform parent) {
            if (_activeLabels.ContainsKey(parent)) return;

            GameObject obj = GetLabel();
            
            if (obj == null) return;

            obj.transform.position = parent.position + offset;
            obj.GetComponent<Text>().text = labelText;
            obj.SetActive(true);

            _activeLabels.Add(parent, obj);
        }
        public void HideLabel(Transform t) {
            if (!_activeLabels.ContainsKey(t)) return;

            _activeLabels[t].SetActive(false);
            _labelPool.Push(_activeLabels[t]);
            _activeLabels.Remove(t);
        }
        public void ShowTrapMarker(Transform parent) {
            if (_activeTrapMarkers.ContainsKey(parent)) return;

            GameObject obj = GetTrapMarker();
            
            if (obj == null) return;

            obj.transform.position = parent.position;
            obj.SetActive(true);

            _activeTrapMarkers.Add(parent, obj);
        }
        public void HideTrapMarker(Transform t) {
            if (!_activeTrapMarkers.ContainsKey(t)) return;

            _activeTrapMarkers[t].SetActive(false);
            _trapMarkerPool.Push(_activeTrapMarkers[t]);
            _activeTrapMarkers.Remove(t);
        }
        public void ShowEnemyMarker(Transform parent) {
            if (_activeEnemyMarkers.ContainsKey(parent)) return;

            GameObject obj = GetEnemyMarker();
            
            if (obj == null) return;

            obj.transform.position = parent.position;
            obj.SetActive(true);

            _activeEnemyMarkers.Add(parent, obj);
        }
        public void HideEnemyMarker(Transform t) {
            if (!_activeEnemyMarkers.ContainsKey(t)) return;

            _activeEnemyMarkers[t].SetActive(false);
            _enemyMarkerPool.Push(_activeEnemyMarkers[t]);
            _activeEnemyMarkers.Remove(t);
        }
        public void HideAllMarkers() {
            foreach (GameObject marker in _activeEnemyMarkers.Values) {
                marker.SetActive(false);
                _enemyMarkerPool.Push(marker);
            }
            
            foreach (GameObject marker in _activeTrapMarkers.Values) {
                marker.SetActive(false);
                _trapMarkerPool.Push(marker);
            }

            _activeEnemyMarkers.Clear();
            _activeTrapMarkers.Clear();
        }
    }
}