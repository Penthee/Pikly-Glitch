using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pikl {
    public class GameObjectMgr : Singleton<GameObjectMgr> {
        protected GameObjectMgr() { }
        
        #region Fields & Initialization
        public enum StartupPoolMode { Awake, Start, CallManually };

        [System.Serializable]
        public class StartupPool {
            public int size;
            public GameObject prefab;
        }

        List<GameObject> tempList = new List<GameObject>();

        Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

        public StartupPoolMode startupPoolMode;
        public StartupPool[] startupPools;

        Transform poolObj, levelObj;

        bool startupPoolsCreated;

        public override void Awake() {
            base.Awake();

            MessageMgr.I.AddListener<string>("EnterScene", OnEnterScene);
            MessageMgr.I.AddListener<string>("ExitScene", OnExitScene);

            if (startupPoolMode == StartupPoolMode.Awake)
                StartCoroutine(CreateStartupPools());

            OnEnterScene(SceneMgr.I.LoadedSceneName);
        }

        public override void Start() {
            base.Start();

            if (startupPoolMode == StartupPoolMode.Start)
                StartCoroutine(CreateStartupPools());
        }

        /// <summary>
        /// Called whenever a scene is loaded.
        /// </summary>
        void OnEnterScene(string scene) {
            poolObj = (GameObject.Find("GameObject Pool") ?? new GameObject("GameObject Pool")).transform;
            poolObj.transform.parent = I.transform;

            levelObj = (GameObject.Find("Level") ?? new GameObject("Level")).transform;
        }

        /// <summary>
        /// Called whenever a new scene is loaded, but before the old one is destroyed
        /// </summary>
        void OnExitScene(string scene) {
            //pooledObjects.Clear();
            //pooledObjects = new Dictionary<GameObject, List<GameObject>>();

            //spawnedObjects.Clear();
            //spawnedObjects = new Dictionary<GameObject, GameObject>();
        }

        public IEnumerator CreateStartupPools() {
            if (!startupPoolsCreated) {
                I.startupPoolsCreated = true;
                var pools = I.startupPools;

                if (pools != null && pools.Length > 0)
                    for (int i = 0; i < pools.Length; ++i) {
                        CreatePool(pools[i].prefab, pools[i].size);
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                    }
            }
        }
        #endregion

        #region Pool Creation
        public void CreatePool(GameObject prefab, int initialPoolSize) {
            if (prefab != null) {
                if (!pooledObjects.ContainsKey(prefab)) {
                    var list = new List<GameObject>();
                    pooledObjects.Add(prefab, list);

                    if (initialPoolSize > 0) {
                        bool active = prefab.activeSelf;
                        prefab.SetActive(false);

                        while (list.Count < initialPoolSize) {
                            GameObject obj = Instantiate(prefab);
                            obj.transform.SetParent(poolObj);
                            list.Add(obj);
                        }

                        prefab.SetActive(active);
                    }
                } else {
                    SpawnToPool(prefab, initialPoolSize);
                }
            }
        }
        #endregion

        #region Spawning
        public GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, bool setActive = true) {
            Transform objTransform;
            GameObject obj;

            //If the pool contains a list for this prefab
            if (I.pooledObjects.ContainsKey(prefab)) {

                //If that list is empty, put an instance in it and call Spawn again.
                if (I.pooledObjects[prefab].Count == 0) {
                    SpawnToPool(prefab, 1);
                    return Spawn(prefab, parent, position, rotation);
                }

                //Grab an instance from the pool if there is one
                if (I.pooledObjects[prefab][0] != null) {
                    obj = I.pooledObjects[prefab][0];
                    I.pooledObjects[prefab].RemoveAt(0);
                } else {
                    Debug.LogWarning("There was a null object in the pool! Whats up with dat?! Removing ref and trying again...", this);

                    if (I.pooledObjects[prefab].Count == 0)
                        SpawnToPool(prefab, 1);
                    else
                        I.pooledObjects[prefab].RemoveAt(0);

                    return Spawn(prefab, parent, position, rotation);
                    //return null;
                }

            } else {
                //If there isn't a list within the pool, create one and call Spawn again.
                CreatePool(prefab, 1);
                return Spawn(prefab, parent, position, rotation);
            }

            //Spawn the object
            objTransform = obj.transform;
            objTransform.SetParent(parent ?? levelObj);
            objTransform.localPosition = position;
            objTransform.localRotation = rotation;
            obj.SetActive(setActive);

            if (obj.GetComponent<Renderer>() != null)
                obj.GetComponent<Renderer>().enabled = true;

            //if (obj.GetComponent<Collider2D>() != null)
            //    obj.GetComponent<Collider2D>().enabled = true;

            I.spawnedObjects.Add(obj, prefab);

            return obj;
        }

        public GameObject Spawn(GameObject prefab, Transform parent, Vector3 position) {
            return Spawn(prefab, parent, position, Quaternion.identity);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true) {
            return Spawn(prefab, null, position, rotation, setActive);
        }

        public GameObject Spawn(GameObject prefab, Transform parent) {
            return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position) {
            return Spawn(prefab, null, position, Quaternion.identity);
        }

        public GameObject Spawn(GameObject prefab) {
            return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
        }


        public void SpawnToPool(GameObject prefab) {
            bool active = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject obj = Instantiate(prefab);
            obj.transform.SetParent(poolObj);
            I.pooledObjects[prefab].Add(obj);

            prefab.SetActive(active);
        }

        /// <summary>
        /// Spawns amount copies of prefab into the pool, or creates a new pool if this object doesn't exist in one.
        /// </summary>
        public void SpawnToPool(GameObject prefab, int amount) {
            if (!I.pooledObjects.ContainsKey(prefab)) {
                CreatePool(prefab, amount);
            } else {
                for (int i = 0; i < amount; i++)
                    SpawnToPool(prefab);
            }
        }
        #endregion

        #region Recycling
        /// <summary>
        /// Recycles a specific instance of a prefab, this cannot be the prefab in the project.
        /// </summary>
        public void Recycle(GameObject obj) {
            GameObject prefab;
            if (I.spawnedObjects.TryGetValue(obj, out prefab)) {
                Recycle(obj, prefab);
            } else {
                Destroy(obj);
            }
        }
        /// <summary>
        /// Recycles a specific instance of a prefab, this cannot be the prefab in the project.
        /// Instead of disabling the object, it's moved to a specified position.
        /// </summary>
        public void Recycle(GameObject obj, Vector2 position) {
            GameObject prefab;
            if (I.spawnedObjects.TryGetValue(obj, out prefab)) {
                Recycle(obj, prefab, true);
                obj.transform.position = position;
            } else {
                Destroy(obj);
            }
        }

        /// <summary>
        /// Recycles amount of spawned instances of prefab. <para/>
        /// Only use this if you don't care which objects are recycled.
        /// </summary>
        public void Recycle(GameObject prefab, int amount) {
            foreach (var item in I.spawnedObjects)
                if (item.Value == prefab)
                    tempList.Add(item.Key);

            for (int i = 0; i < amount; ++i) {
                try {
                    Recycle(tempList[i]);
                } catch {
                    Debug.LogWarning("There were not enough objects to recycle " + amount + " of them.", this);
                    break;
                }
            }

            tempList.Clear();
        }

        /// <summary>
        /// Recycle a specific list of objects.
        /// </summary>
        /// <param name="objs"></param>
        public void Recycle(List<GameObject> objs) {
            foreach (GameObject obj in objs)
                Recycle(obj);
        }

        /// <summary>
        /// Recycles the obj instance of the prefab.
        /// </summary>
        void Recycle(GameObject obj, GameObject prefab, bool active = false) {
            I.pooledObjects[prefab].Add(obj);
            I.spawnedObjects.Remove(obj);
            obj.transform.SetParent(poolObj);
            obj.name = prefab.name;
            obj.SetActive(active);
            //print("Successfully recycled " + obj.name + " into pool (" + prefab.name + ")!");
        }

        /// <summary>
        /// Recycles all instances of a specified prefab.
        /// </summary>
        /// <param name="prefab"></param>
        public void RecycleAll(GameObject prefab) {
            foreach (var item in I.spawnedObjects)
                if (item.Value == prefab)
                    tempList.Add(item.Key);

            for (int i = 0; i < tempList.Count; ++i)
                Recycle(tempList[i]);

            tempList.Clear();
        }

        /// <summary>
        /// Recycles all instances of all prefabs.
        /// </summary>
        public void RecycleAll() {
            tempList.AddRange(I.spawnedObjects.Keys);
            for (int i = 0; i < tempList.Count; ++i)
                Recycle(tempList[i]);
            tempList.Clear();
        }
        #endregion

        #region Destroying
        /// <summary>
        /// Destroys all of objects of type prefab in the pool.
        /// </summary>
        public void DestroyPooled(GameObject prefab) {
            List<GameObject> pooled;

            if (I.pooledObjects.TryGetValue(prefab, out pooled)) {
                for (int i = 0; i < pooled.Count; ++i)
                    Destroy(pooled[i]);

                pooled.Clear();
            }
        }

        /// <summary>
        /// Destroys a specified amount of objects of type prefab in the pool.
        /// </summary>
        public void DestroyPooled(GameObject prefab, short amount) {
            List<GameObject> pooled;

            if (I.pooledObjects.TryGetValue(prefab, out pooled)) {
                for (int i = 0; i < amount; ++i)
                    GameObject.Destroy(pooled[i]);

                pooled.Clear();
            }
        }

        /// <summary>
        /// Destroys every object of every type in the pool and live objects.
        /// </summary>
        public void DestroyAll() {
            RecycleAll();

            I.spawnedObjects = new Dictionary<GameObject, GameObject>();

            DestroyAllPooled();
        }

        public void DestroyAll(GameObject prefab) {
            RecycleAll(prefab);
            DestroyPooled(prefab);
        }

        /// <summary>
        /// Destroys every object of every type in the pool.
        /// </summary>
        public void DestroyAllPooled() {
            foreach (var list in I.pooledObjects) {
                foreach (GameObject obj in list.Value) {
                    MonoBehaviour.Destroy(obj);
                }
            }

            I.pooledObjects = new Dictionary<GameObject, List<GameObject>>();
        }
        #endregion

        #region Utilities
        public bool IsSpawned(GameObject obj) {
            return I.spawnedObjects.ContainsKey(obj);
        }

        public int CountPooled(GameObject prefab) {
            List<GameObject> list;
            if (I.pooledObjects.TryGetValue(prefab, out list))
                return list.Count;
            return 0;
        }

        public int CountAllPooled() {
            int count = 0;
            foreach (var list in I.pooledObjects.Values)
                count += list.Count;
            return count;
        }

        public int CountSpawned(GameObject prefab) {
            int count = 0;

            foreach (var InstancePrefab in I.spawnedObjects.Values)
                if (prefab == InstancePrefab)
                    ++count;

            return count;
        }

        public int CountAllSpawned() {
            return spawnedObjects.Values.Count;
        }

        public List<GameObject> GetPooled(GameObject prefab, List<GameObject> list = null, bool appendList = false) {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            List<GameObject> pooled;
            if (I.pooledObjects.TryGetValue(prefab, out pooled))
                list.AddRange(pooled);
            return list;
        }

        public List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list = null, bool appendList = false) {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            foreach (var item in I.spawnedObjects)
                if (item.Value == prefab)
                    list.Add(item.Key);
            return list;
        }
        #endregion
    }
}