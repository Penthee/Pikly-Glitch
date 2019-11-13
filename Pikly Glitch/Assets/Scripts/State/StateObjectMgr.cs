using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pikl.States;

namespace Pikl {
    /// <summary>
    /// Provides the functionality for Instantiating, Spawning, Despawning, Destroying and State Switching for StateObjects 
    /// either individually, by tag or all objects at once.
    /// 
    /// For GameObjects that do not use the StateObject, see <see cref="Pikl.Managers.GameObjectMgr"/>.
    /// </summary>
    public class StateObjectMgr : Singleton<StateObjectMgr> {
        protected StateObjectMgr() { }

        #region Fields / Initialization
        public static Dictionary<string, List<StateObject>> objects = new Dictionary<string, List<StateObject>>();
        public bool haltGlobalTick = false;
        public float globalTickSpeed;

        Transform poolObj;//, levelObj;

        Vector2 despawnVector = new Vector2(42000, 42000);

        public static int NextStateID {
            get {
                return ++stateID;
            }
        }

        static int stateID;

        #endregion

        public override void Awake() {
            MessageMgr.I.AddListener<string>("EnterScene", OnEnterScene);
            MessageMgr.I.AddListener<string>("ExitScene", OnExitScene);
            
            if (globalTickSpeed > 0)
                StartCoroutine(GlobalTick());

            OnEnterScene(SceneMgr.I.LoadedSceneName);

            base.Awake();
        }

        #region Events
        /// <summary>
        /// Called whenever a new scene is loaded
        /// </summary>
        void OnEnterScene(string scene) {
            poolObj = (GameObject.Find("StateObject Pool") ?? new GameObject("StateObject Pool")).transform;
            poolObj.transform.parent = transform;

            //levelObj = (GameObject.Find("Level") ?? new GameObject("Level")).transform;

            CacheObjectsInScene();
        }

        /// <summary>
        /// Called whenever a new scene is loaded, but before the old one is destroyed
        /// </summary>
        void OnExitScene(string scene) {
            objects = new Dictionary<string, List<StateObject>>();
        }

        /// <summary>
        /// Grabs all existing StateObjects and stores them in the pool
        /// </summary>
        void CacheObjectsInScene() {

            foreach (StateObject so in GameObject.FindObjectsOfType<StateObject>()) {
                if (!objects.ContainsKey(so.tag))
                    objects.Add(so.tag, new List<StateObject>());

                if (!objects[so.tag].Contains(so))
                    objects[so.tag].Add(so);
            }
        }

        /// <summary>
        /// The top level Coroutine for handling the state object global ticks.
        /// </summary>
        IEnumerator GlobalTick() {
            while (true) {
                if (haltGlobalTick) {
                    haltGlobalTick = false;
                    yield break;
                }

                MessageMgr.I.Broadcast("SoGlobalTick");

                yield return new WaitForSeconds(globalTickSpeed);
            }
        }

        #endregion

        //UI stuff - look at this later ↓
        public string[] ObjectTypes() {
            string[] keys = new string[objects.Keys.Count];
            objects.Keys.CopyTo(keys, 0);
            return keys;
        }

        public StateObject[] ObjectsForType(string obj) {
            StateObject[] objs = new StateObject[objects[obj].Count];
            objects[obj].CopyTo(objs, 0);
            return objs;
        }
        //UI stuff - look at this later ↑

        #region Spawning / Despawning
        /// <summary>
        /// Instantiates a GameObject and sets it's position to (42000, 42000) and then turns it off.
        /// </summary>
        /// <param name="obj">The GameObject to instantiate.</param>
        /// <param name="amount">How many instances to instantiate.</param>
        public void SpawnToPool(StateObject obj, System.Int16 amount = 1) {
            if (obj == null) {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Tried to Instantiate a null object!", this);
#endif
                return;
            }
            if (!objects.ContainsKey(obj.tag))
                objects.Add(obj.tag, new List<StateObject>());

            for (int i = 0; i < amount; i++) {
                GameObject _obj = Instantiate(obj.gameObject) as GameObject;

                _obj.transform.parent = poolObj.transform;
                _obj.GetComponent<StateObject>().TurnOff(despawnVector);

                objects[_obj.tag].Add(_obj.GetComponent<StateObject>());
            }


        }

        /// <summary>Repositions GameObject to pos, rot and turns it on.</summary>
        /// <param name="amount">The amount to spawn, a warning will be logged if there isn't enough GameObjects in the pool.</param>
        public List<StateObject> SpawnToGame(string tag, Vector3 pos, Quaternion rot, System.Int16 amount = 1) {
            if (!objects.ContainsKey(tag)) {
                Debug.LogWarning("There were no instances of " + tag + " in the pool, spawn some to the pool first.", this);
                return null;
            }

            List<StateObject> toSpawn = objects[tag].Where(e => !e.gameObject.activeInHierarchy).Take(amount).ToList();

            if (toSpawn.Count < amount)
                Debug.LogWarning("There was an insufficient amount (" + toSpawn.Count + ") of " + tag + " in the pool to spawn " + amount + " of them.", this);

            foreach (StateObject so in toSpawn)
                so.TurnOn(pos, rot);

            return toSpawn;
        }

        /// <summary>Repositions a specific object's position to infinity, then turns it off.</summary>
        public void DespawnFromGame(StateObject obj) {
            obj.TurnOff(despawnVector);
        }

        /// <summary>Repositions all objects of tag and turns them off.</summary>
        public void DespawnAllFromGame(string tag) {
            foreach (StateObject so in objects[tag])
                so.TurnOff(despawnVector);
        }

        /// <summary>Destroys a specific GameObject and removes it from pools.</summary>
        public void Destroy(StateObject obj) {
            objects[obj.tag].Remove(obj);
            MonoBehaviour.Destroy(obj.gameObject);
        }

        /// <summary>Destroys all objects of type obj and removes them from pools</summary>
        public void DestroyAll(string tag) {
            for (int i = objects[tag].Count - 1; i >= 0; i--) {
                MonoBehaviour.Destroy(objects[tag][i].gameObject);
                objects[tag].RemoveAt(i);
            }
        }

        /// <summary>Destroys every object that belongs to GameObjectManager.</summary>
        /// <remarks>Initially created to be called via Listener when switching scenes.</remarks>
        public void DestroyEverything() {
            foreach (string tag in objects.Keys) {
                for (int j = objects[tag].Count - 1; j >= 0; j--) {
                    MonoBehaviour.Destroy(objects[tag][j].gameObject);
                    objects[tag].RemoveAt(j);
                }
            }

            objects = new Dictionary<string, List<StateObject>>();
        }
        #endregion

        #region Pause / Unpause
        /// <summary>
        /// Pause a specific StateObject.
        /// </summary>
        public void PauseObj(StateObject obj) {
            obj.Pause();
        }

        /// <summary>
        /// Un-Pause a specific StateObject.
        /// </summary>
        public void UnPauseObj(StateObject obj) {
            obj.UnPause();
        }

        /// <summary>
        /// Pause all objects that belong to a tag.
        /// </summary>
        public void PauseAllObj(string tag) {
            foreach (StateObject so in objects[tag])
                so.Pause();
        }

        /// <summary>
        /// Un-Pause all objects that belong to a tag.
        /// </summary>
        public void UnPauseAllObj(string tag) {
            foreach (StateObject so in objects[tag])
                so.UnPause();
        }

        /// <summary>
        /// Pauses all StateObjects
        /// </summary>
        public void PauseEverything() {
            foreach (List<StateObject> sol in objects.Values) {
                foreach (StateObject so in sol)
                    so.Pause();
            }
        }

        /// <summary>
        /// UnPauses all StateObjects
        /// </summary>
        public void UnPauseEverything() {
            foreach (List<StateObject> sol in objects.Values) {
                foreach (StateObject so in sol)
                    so.UnPause();
            }
        }
        #endregion

        #region State Switching
        public void SwitchAll(string tag, State state) {
            foreach (StateObject so in objects[tag]) {
                so.SwitchTo(state);
            }
        }
        #endregion

    }
}