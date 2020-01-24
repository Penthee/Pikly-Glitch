using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pikl.Utils.RDS;
using UnityEditor.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.XR.WSA.Input;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Pikl.States.Components;

namespace Pikl.Control {
    public class LevelRandomiser : MonoBehaviour {

        public int iterationMax = 1;
        public Room startingRoom;
        public Transform player, cam;
        
        [SerializeField] Dictionary<Room, bool> roomPool = new Dictionary<Room, bool>();
        [SerializeField] List<Room> corridorPool = new List<Room>();
        List<GameObject> tempListToDestroy = new List<GameObject>();

        [ReadOnly] public bool isRandomising;
        readonly WaitForEndOfFrame _waitForFrame = new WaitForEndOfFrame();
        string _currentSceneName;
        int _i = 0;
        bool Failsafe => _i < iterationMax;
        
        void Awake() {
            _currentSceneName = SceneManager.GetActiveScene().name;
            
            LoadRooms();
            LoadConnectors();
        }
        void Start() {
            //StartCoroutine(Randomise());
        }

        void LoadRooms() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/" + _currentSceneName)) {
                roomPool.Add(o.GetComponent<Room>(), false);
            }
            Debug.Log($"Loaded Rooms: {roomPool.Count}");
            //Remove teleporter room from this list
        }
        void LoadConnectors() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/Connectors")) {
                corridorPool.Add(o.GetComponent<Room>());
            }
            Debug.Log($"Loaded Connectors: {corridorPool.Count}");
        }

        
        [Button]
        void ManualRandomise() {
            if (isRandomising)
                return;
            
            foreach (GameObject o in tempListToDestroy) {
                Destroy(o);
            }

            tempListToDestroy.Clear();
            
            StartCoroutine(Randomise());
        }
        IEnumerator Randomise() {
            isRandomising = true;
            PlaceStartingRoom();
            PlacePlayer();

            Room currentRoom = startingRoom;
            _i = 0;
            
            do {
                Room roomPlaced = null;
                foreach (Transform cp in currentRoom.connectPoints) {
                    Room connector = PlaceCorridor(cp);
                    tempListToDestroy.Add(connector.gameObject);
                    
                    yield return _waitForFrame;

                    foreach (Transform _cp in connector.connectPoints) {
                        roomPlaced = PlaceRoom(_cp);
                        tempListToDestroy.Add(roomPlaced.gameObject);
                        yield return _waitForFrame;
                    }
                }

                Debug.Log(string.Format("Iteration: {0}, Rooms Placed: {1}/{2}", 
                    _i++.ToString(), roomPool.Count(e => e.Value == true).ToString(), roomPool.Count.ToString()));

                if (roomPlaced != null) {
                    roomPool[roomPlaced] = true;
                    currentRoom = roomPlaced;
                }
                yield return _waitForFrame;
            } while (Failsafe && roomPool.Any(e => e.Value == false));

            Debug.Log(!Failsafe ? "Completed Randomisation, Failsafe hit." : "Completed Randomisation.");
            //TODO - refactor
            player.GetComponent<PlayerHealth>().Invulnerable = false;
            isRandomising = false;
        }
        void PlaceStartingRoom() {
            startingRoom.transform.position = Vector3.zero;
            startingRoom.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }
        void PlacePlayer() {
            Vector3? center = startingRoom.transform.Find("SpawnPoint")?.position;
            if (center == null) {
                Debug.LogError("NO SPAWN POINT FOUND FOR PLAYER IN STARTING ROOM FOR LEVEL " + _currentSceneName);
                player.position = startingRoom.transform.position;
            } else
                player.position = center.Value;

            cam.position = new Vector3(player.position.x, player.position.y, -10);

            player.GetComponent<PlayerHealth>().Invulnerable = true;
        }
        Room PlaceCorridor(Transform toConnectTo) {
            Room corridor = GetRandomCorridor();
            int attempts = 0;
            do {
                //Debug.Log($"Attempting Connector: {corridor.name}");
                
                if (attempts >= corridor.connectPoints.Count) {
                    Destroy(corridor);
                    corridor = GetRandomCorridor();
                    attempts = 0;
                }

                AlignConnectors(corridor.transform, corridor.connectPoints[attempts], toConnectTo);

            } while (!Validate(corridor));
            
            Debug.Log($"Placed Connector: {corridor.name}");
            
            return corridor;
        }
        Room PlaceRoom(Transform toConnectTo) {
            Room room = GetRandomRoom();
            int attempts = 0;
            do {
                //Debug.Log($"Attempting Room: {room.name}");

                if (attempts >= room.connectPoints.Count) {
                    Destroy(room);
                    room = GetRandomRoom();
                    attempts = 0;
                }
                
                AlignConnectors(room.transform, room.connectPoints[attempts], toConnectTo);
                
            } while (!Validate(room));
            
            Debug.Log($"Placed Room: {room.name}");
            
            return room;
        }

        void AlignConnectors(Transform t1, Transform t1C, Transform t2C) {
            t1.rotation = t2C.rotation * Quaternion.Inverse(t1C.localRotation);
            t1.position = t2C.position + (t1.position - t1C.position);
        }
        Room GetRandomCorridor() {
            return Instantiate(corridorPool[Random.Range(0, corridorPool.Count)]);
        }
        Room GetRandomRoom() {
            List<KeyValuePair<Room, bool>> ununsedRooms = roomPool.Where(e => e.Value == false).ToList();
            return ununsedRooms.Count > 0 ? Instantiate(ununsedRooms[Random.Range(0, ununsedRooms.Count)].Key) : null;
        }
        bool CheckBoundingBox(Bounds one, Bounds two) {
            return one.Intersects(two);
        }
        bool Validate(Room room) {
            //Check whether anything intersects with this room's bounding box
            return true;
        }
    }

}