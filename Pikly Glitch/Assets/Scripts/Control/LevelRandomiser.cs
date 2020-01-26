using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Pikl.Utils.RDS;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Pikl.States.Components;

namespace Pikl.Control {
    public class LevelRandomiser : MonoBehaviour {

        public int iterationMax = 1;
        public Room startingRoom;
        public Transform player, cam;
        
        [SerializeField] List<Room> roomPool = new List<Room>();
        [SerializeField] List<Room> corridorPool = new List<Room>();
        [SerializeField] List<GameObject> _tempListToDisable = new List<GameObject>();
        [SerializeField] Room deadEnd;

        [ReadOnly] public bool isRandomising;
        readonly WaitForEndOfFrame _waitForFrame = new WaitForEndOfFrame();
        string _currentSceneName;
        int _i = 0;
        bool Failsafe => _i >= iterationMax;
        void Awake() {
            _currentSceneName = SceneManager.GetActiveScene().name;
            
            LoadRooms();
            LoadConnectors();
        }
        void Start() {
            //StartCoroutine(Randomise());
        }
        void LoadRooms() {
            roomPool.Clear();
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/" + _currentSceneName)) {
                if (/*o.name.Contains("Teleporter") || */o.name.Contains("StartingRoom"))
                    continue;
                
                Room r = Instantiate(o).GetComponent<Room>();
                r.gameObject.SetActive(false);
                roomPool.Add(r);
            }

            Debug.Log($"Loaded Rooms: {roomPool.Count}");
        }
        void LoadConnectors() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/Connectors")) {
                if (o.name.Contains("Dead End"))
                    continue;
                
                corridorPool.Add(o.GetComponent<Room>());
            }
            Debug.Log($"Loaded Connectors: {corridorPool.Count}");
        }
        [Button] void ManualRandomise() {
            if (isRandomising)
                return;

            Randomise();
        }

        void Reset() {
            roomPool.RemoveAll(room => room != null);

            foreach (ConnectPoint cp in startingRoom.connectPoints) cp.isConnected = false;
            startingRoom.isPlaced = false;
            
            foreach (GameObject room in _tempListToDisable.ToList()) {
                _tempListToDisable.Remove(room);
                GameObject.Destroy(room);
            }

            LoadRooms();

            _tempListToDisable.Clear();
        }
        async void Randomise() {
            isRandomising = true;
            PlaceStartingRoom();
            PlacePlayer();

            _i = 0;

            do {
                if (_tempListToDisable.Count > 0 || _i > 0) Reset();
                await Branch(startingRoom);
                
                Debug.Log(string.Format("Iteration: {0}, Rooms Placed: {1}/{2}", 
                    _i++.ToString(), roomPool.Count(e => e.isPlaced).ToString(), roomPool.Count.ToString()));
                
                await _waitForFrame;
            } while (!Failsafe && roomPool.Any(e => !e.isPlaced));

            Debug.Log(Failsafe ? "Failsafe hit." : "Completed Randomisation.");
            isRandomising = false;
            await _waitForFrame;
        }

        //Starting Room
        //Loop through all connect points
        //    add a random corridor to them
        //    if no corridor could be placed, use a dead end.
        //    check all the free connect points on the placed corridor
        //        try placing a room to each
        //        if room placement fail, use dead end
        
        async Task Branch(Room room) {
            foreach (ConnectPoint cp in room.connectPoints.Where(e => !e.isConnected)) {
                Debug.Log(string.Format("Branching {0} : {1}", room.gameObject.name, cp.t.name));
                Room corridor = await PlaceCorridor(cp, GetRandomCorridor());
                await _waitForFrame;
                
                if (!corridor) {
                    await PlaceDeadEnd(cp);
                    continue;
                }

                _tempListToDisable.Add(corridor.gameObject);
                foreach (ConnectPoint _cp in corridor.connectPoints.Where(e => !e.isConnected)) {
                    Room roomPlaced = await PlaceRoom(_cp);
                    
                    await _waitForFrame;
                    if (roomPlaced) {
                        _tempListToDisable.Add(roomPlaced.gameObject);
                        roomPool[roomPool.IndexOf(roomPlaced)].isPlaced = true;
                        await Branch(roomPlaced);
                    } else {
                        await PlaceDeadEnd(_cp);
                    }
                }
                await _waitForFrame;
            }
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
            //TODO - refactor player control stuff - do the colliders
            player.GetComponent<PlayerHealth>().Invulnerable = true;
        }

        async Task PlaceDeadEnd(ConnectPoint _cp) {
            Room _deadEnd = await PlaceCorridor(_cp, Instantiate(deadEnd));
            if (_deadEnd)
                _tempListToDisable.Add(_deadEnd.gameObject);
        }
        //TODO - Merge PlaceRoom and PlaceCorridor, add type enum in Room
        async Task<Room> PlaceCorridor(ConnectPoint toConnectTo, Room corridor) {
            if (!corridor)
                return null;
            
            int attempts = 0;
            do {
                Debug.Log($"Attempting Connector: {corridor.name}");
                
                if (attempts >= corridor.connectPoints.Count) {
                    foreach (ConnectPoint cp in corridor.connectPoints) cp.isConnected = false;
                    corridor.gameObject.SetActive(false);
                    corridor = GetRandomCorridor();
                    attempts = 0;
                }

                AlignConnectors(corridor.transform, corridor.connectPoints[attempts++].t, toConnectTo.t);
                await new WaitForFixedUpdate();

            } while (attempts < corridor.connectPoints.Count && corridor.IsOverlapping());

            corridor.connectPoints[attempts - 1].isConnected = true;
            toConnectTo.isConnected = true;
            Debug.Log($"Placed Connector: {corridor.name}");
            return corridor;
        }
        async Task<Room> PlaceRoom(ConnectPoint toConnectTo) {
            Room room = GetRandomRoom();
            if (!room)
                return null;
            
            int attempts = 0, fails = 0;
            room.gameObject.SetActive(true);
            do {
                Debug.Log($"Attempting Room: {room.name}");

                if (attempts >= room.connectPoints.Count) {
                    foreach (ConnectPoint cp in room.connectPoints) cp.isConnected = false;
                    room.gameObject.SetActive(false);
                    room = GetRandomRoom();
                    room.gameObject.SetActive(true);
                    attempts = 0;
                }
                
                AlignConnectors(room.transform, room.connectPoints[attempts++].t, toConnectTo.t);
                await new WaitForFixedUpdate();

                if (AvailableRooms().Count == 1 && attempts >= room.connectPoints.Count) {
                    toConnectTo = GetFreeConnectPoint();
                    if (fails++ > 10 || toConnectTo == null) {
                        room.gameObject.SetActive(false);
                        return null;
                    }
                }
            } while (room.IsOverlapping());

            room.connectPoints[attempts - 1].isConnected = true;
            toConnectTo.isConnected = true;
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
            List<Room> ununsedRooms = AvailableRooms();
            return ununsedRooms.Count > 0 ? ununsedRooms[Random.Range(0, ununsedRooms.Count)] : null;
        }

        List<Room> AvailableRooms() {
            return roomPool.Where(e => !e.isPlaced && e.connectPoints.Any(c => !c.t.gameObject.activeSelf && !c.isConnected)).ToList();
        }
        List<ConnectPoint> GetFreeConnectPoints() {
            ConnectPoint startingPoint = startingRoom.connectPoints.First(e => !e.isConnected);

            List<ConnectPoint> validPoints = new List<ConnectPoint>();
            foreach (GameObject obj in _tempListToDisable.Where(e => e.GetComponent<Room>().connectPoints.Any(x => !x.isConnected))) {
                foreach(ConnectPoint point in obj.GetComponent<Room>().connectPoints.Where(e => !e.isConnected))
                    validPoints.Add(point);
            }

            if (startingPoint.t != null)
                validPoints.Add(startingPoint);
            
            if (validPoints.Count == 0)
                return null;
            
            return validPoints;
        }
        ConnectPoint GetFreeConnectPoint() {
            List<ConnectPoint> freeConnectPoints = GetFreeConnectPoints();
            
            return freeConnectPoints[Random.Range(0, freeConnectPoints.Count)];
        }
    }
}