using UnityEngine;

namespace Pikl.Components {
    public class SimpleSingleton<Instance> : MonoBehaviour where Instance : SimpleSingleton<Instance> {
        public static Instance instance;
        public bool isPersistant = true;

        public virtual void Awake() {
            if (isPersistant) {
                if (!instance) {
                    instance = this as Instance;
                } else {
                    Destroy(gameObject);
                }

                DontDestroyOnLoad(gameObject);
            } else {
                instance = this as Instance;
            }
        }
    }
}