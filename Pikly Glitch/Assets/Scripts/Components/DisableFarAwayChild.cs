using UnityEngine;

namespace Pikl.Components {
    [ExecuteInEditMode]
    public class DisableFarAwayChild : MonoBehaviour {

        public float updateInterval = 1, range = 15;

        static Transform _cam;
        Transform t;
        GameObject child;
    
        void Awake() {
            if (!_cam)
                _cam = Camera.main.transform;

            t = transform;
            child = t.GetChild(0).gameObject;
        }
    
        void Start() {
            InvokeRepeating("UpdateDisable", updateInterval, updateInterval);
        }

        void UpdateDisable() {
            if (_cam)
                child.SetActive(Vector2.Distance(_cam.position, t.position) < range);
            else
                child.SetActive(true);
        }
    }
}
