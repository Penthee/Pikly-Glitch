using UnityEngine;

namespace Pikl {
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T _instance;
        private static object _lock = new object();

        public bool inSceneAtStart;

        public void WakeUp() { }

        public virtual void Awake() {
            if (inSceneAtStart) {
                if (!_instance) {
                    _instance = this as T;
                    DontDestroyOnLoad(gameObject);
                } else {
                    print("Destroying second instance of object " + typeof(T).ToString());
                    Destroy(gameObject);
                }
            }
        }

        public virtual void Start() {
            //GameObject control = GameObject.Find("Control");

            //if (control == null) {
            //    control = new GameObject("Control");
            //    control.AddComponent<Components.Singleton<GameObject>>();
            //}

            //transform.SetParent(control.transform);
        }

        /// <summary>The instance reference to the singleton object</summary>
        public static T I {
            get {
                if (applicationIsQuitting) {
#if UNITY_EDITOR
                    print("[Singleton] Instance '" + typeof(T) +
                                                 "' already destroyed on application quit." +
                                                 " Won't create again - returning null.");
#endif
                    return null;
                }

                lock (_lock) {
                    if (_instance == null) {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1) {
#if UNITY_EDITOR
                            Debug.LogError("[Singleton] Something went really wrong " +
                                                       " - there should never be more than 1 singleton!" +
                                                       " Reopening the scene might fix it.");
#endif
                            return _instance;
                        }

                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = typeof(T).ToString();

                            DontDestroyOnLoad(singleton);
                        } 
                    }

                    return _instance;
                }
            }
        }

        private static bool applicationIsQuitting = false;
        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        void OnApplicationQuit() {
        
            applicationIsQuitting = true;
        }
    }
}