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
        [SerializeField] List<Room> placedRooms = new List<Room>();
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
                if ( /*o.name.Contains("Teleporter") || */o.name.Contains("StartingRoom"))
                    continue;

                Room r = Instantiate(o).GetComponent<Room>();
                r.gameObject.SetActive(false);
                roomPool.Add(r);
            }

            Debug.Log($"Loaded Rooms: {roomPool.Count}");
        }

        void LoadConnectors() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/Connectors")) {
                //if (o.name.Contains("Dead End"))
                //    continue;

                corridorPool.Add(o.GetComponent<Room>());
            }

            Debug.Log($"Loaded Connectors: {corridorPool.Count}");
        }

        [Button]
        void ManualRandomise() {
            if (isRandomising)
                return;

            Randomise();
        }

        void Reset() {
            roomPool.RemoveAll(room => room != null);

            foreach (ConnectPoint cp in startingRoom.connectPoints) cp.isConnected = false;
            startingRoom.isPlaced = false;

            foreach (Room room in placedRooms.ToList()) {
                placedRooms.Remove(room);
                GameObject.Destroy(room.gameObject);
            }

            LoadRooms();

            placedRooms.Clear();
        }

        async void Randomise() {
            isRandomising = true;
            PlaceStartingRoom();
            PlacePlayer();

            _i = 0;

            do {
                if (placedRooms.Count > 0 || _i > 0) Reset();
                //await Branch(startingRoom);
                await Layer();

                Debug.Log(string.Format("Iteration: {0}, Rooms Placed: {1}/{2}",
                    _i++.ToString(), roomPool.Count(e => e.isPlaced).ToString(), roomPool.Count.ToString()));

                await _waitForFrame;
            } while (!Failsafe && roomPool.Any(e => !e.isPlaced));

            Debug.Log(Failsafe ? "Failsafe hit." : "Completed Randomisation.");
            isRandomising = false;
            await _waitForFrame;
        }

        //Each Layer's goal is to connect a single pair (corridor + room) to all currently open connect points
        //
        //Room currentRoom = startingRoom
        //
        //Do
        //    //All of these connectPoints must be used this branch
        //    List ConnectPoint openConnectPoints = GetValidConnectPoints
        //    TryPlaceCorridorAndRoom (openConnectPoints) {
        //        do {
        //            
        //            
        //            
        //            foreach cp in openConnectPoints {
        //                Room corridor = PlaceCorridor(SelectCorridorToPlace(), cp)
        //                foreach (_cp in corridor.connectPoints) {
        //                    Room room = PlaceRoom(SelectRoomToPlace(), cp)
        //                }
        //            }
        //
        //
        //
        //        }
        //
        //
        //
        //
        //    }
        //While Failsafe or valid ConnectPoints > 0
        //
        //
        //

        async Task Layer() {
            List<ConnectPoint> freeConnectPoints = GetFreeConnectPoints();
            
            int fails = 0;
            
            do {
                Room room, corridor;
                foreach (ConnectPoint cp in freeConnectPoints) {
                    room = null;
                    corridor = null;
                    
                    switch (cp.t.parent.GetComponent<Room>().type) {
                        case RoomType.Corridor:
                            room = GetRandomRoom();
                            if (room) {
                                room = await TryPlaceRoom(room, cp);
                                /*if (!room) {
                                    
                                }*/
                            } else {
                                await PlaceDeadEnd(cp);
                            }
                            break;
                        case RoomType.Room:
                            corridor = GetRandomCorridor();
                            corridor = await TryPlaceRoom(corridor, cp);
                            break;
                    }

                    if (room == null && corridor == null) {
                        fails++;
                    }
                }
                await _waitForFrame;
                
                freeConnectPoints = GetFreeConnectPoints();
            } while (fails < 100 && (freeConnectPoints?.Any(e => !e.isConnected) ?? false));

            if (fails >= 100) {
                Debug.Log("Failed Layer");
            }
            
            await _waitForFrame;
        }
        //Try connecting the room to the connect point, cycling through each connect point on the room before giving up
        async Task<Room> TryPlaceRoom(Room r, ConnectPoint cp) {
            if (!r) {
                Debug.Log("Room given was null");
                return null;
            }

            int attempts = 0;
            bool valid;
            do {
                Debug.Log($"Trying {r.name} to {cp.t.name}:{cp.t.parent.name}...");
                
                r.gameObject.SetActive(true);
                
                AlignConnectors(r.transform, r.connectPoints[attempts++].t, r.type, cp.t, cp.t.parent.GetComponent<Room>().type);
                //Important to wait for physics update before collision check
                await new WaitForFixedUpdate();
                valid = !r.IsOverlapping();

                if (!valid) continue;
                
                r.connectPoints[attempts - 1].isConnected = true;
                if (r.connectPoints.Any(e => !e.isConnected && !e.HasSpaceInfront)) {
                    r.connectPoints[attempts - 1].isConnected = false;
                    Debug.Log($"No space infront of {r.name}");
                    valid = false;
                } else {
                    cp.isConnected = true;
                    placedRooms.Add(r);
                    Debug.Log($"Success!");
                    return r;
                }

            } while (attempts < r.connectPoints.Count);

            if (r.type == RoomType.Room)
                r.gameObject.SetActive(false);
            else 
                Destroy(r.gameObject);
            
            return null;
        }
        void PlaceStartingRoom() {
            startingRoom.transform.position = Vector3.zero;
            startingRoom.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }
        void PlacePlayer() {
            Transform spawnPoint = startingRoom.transform.Find("SpawnPoint");
            if (spawnPoint != null) {
                player.position = spawnPoint.position;
            } else {
                Debug.LogError("NO SPAWN POINT FOUND FOR PLAYER IN STARTING ROOM FOR LEVEL " + _currentSceneName);
                player.position = startingRoom.transform.position;
            }

            cam.position = new Vector3(player.position.x, player.position.y, -10);
            //TODO - refactor player control stuff - do the colliders
            player.GetComponent<PlayerHealth>().Invulnerable = true;
        }
        async Task PlaceDeadEnd(ConnectPoint cp) {
            Room r = await TryPlaceRoom(Instantiate(deadEnd).GetComponent<Room>(), cp);
        }
        void AlignConnectors(Transform t1, Transform t1C, RoomType t1T, Transform t2C, RoomType t2T) {
            if (t1T == RoomType.Corridor && t2T == RoomType.Corridor)
                //t1.rotation = t2C.rotation;// * t1C.localRotation;
                t1.rotation = t2C.rotation * Quaternion.Inverse(t1C.localRotation);
            else
                //t1.rotation = t2C.rotation * Quaternion.Inverse(t1C.localRotation);
                t1.rotation = t2C.rotation * t1C.localRotation;
            
            t1.position = t2C.position + (t1.position - t1C.position);
        }
        Room GetRandomCorridor() {
            return Instantiate(corridorPool[Random.Range(0, corridorPool.Count)]);
        }
        Room GetRandomRoom() {
            List<Room> ununsedRooms = AvailableRooms();
            //foreach(var bah in ununsedRooms) Debug.Log($"bah: {bah.name}");
            return ununsedRooms.Count > 0 ? ununsedRooms[Random.Range(0, ununsedRooms.Count)] : null;
        }

        List<Room> AvailableRooms() {
            return roomPool.Where(e => !e.isPlaced && !e.gameObject.activeSelf && e.connectPoints.Any(c => !c.isConnected)).ToList();
        }
        List<ConnectPoint> GetFreeConnectPoints() {
            ConnectPoint startingPoint = startingRoom.connectPoints.FirstOrDefault(e => !e.isConnected);

            List<ConnectPoint> validPoints = new List<ConnectPoint>();
            foreach (Room room in placedRooms.Where(e => e.connectPoints.Any(x => !x.isConnected))) {
                foreach(ConnectPoint point in room.connectPoints.Where(e => !e.isConnected))
                    validPoints.Add(point);
            }

            if (startingPoint != null && startingPoint.t != null)
                validPoints.Add(startingPoint);
            
            return validPoints.Count == 0 ? null : validPoints;
        }
        ConnectPoint GetFreeConnectPoint() {
            //TODO : Pick the closest point to the SpawnPoint.
            List<ConnectPoint> freeConnectPoints = GetFreeConnectPoints();
            return freeConnectPoints.Count == 0 ? null : freeConnectPoints[Random.Range(0, freeConnectPoints.Count)];
        }
    }
}