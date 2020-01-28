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
using UnityEditor.Experimental.GraphView;
using UnityEngine.Windows;

namespace Pikl.Control {
    public class LevelRandomiser : MonoBehaviour {

        [Range(1, 100)]
        public int iterationMax = 1;
        [Range(0.02f, 5)]
        public float generationSpeed = 1;
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
            startingRoom.DisconnectAllPoints();
            startingRoom.status = RoomStatus.Waiting;

            foreach (Room room in placedRooms.ToList()) {
                room.DisconnectAllPoints();
                if (room.type == RoomType.Corridor)
                    GameObject.Destroy(room.gameObject);
                else 
                    room.gameObject.SetActive(false);
                
                placedRooms.Remove(room);
            }

            placedRooms.Clear();
        }

        async void Randomise() {
            isRandomising = true;
            PlaceStartingRoom();
            PlacePlayer();

            _i = 0;
            
            do {
                //if (placedRooms.Count > 0 || _i > 0) Reset();
                //await Branch(startingRoom);
                //await Layer();
                ConnectPoint cp = GetFreeConnectPoint();
                if (cp != null)
                    await NewBranch(cp);
                else {
                    Debug.Log("No Connect points found, cannot branch");
                    Reset();
                }

                Debug.Log(string.Format("Iteration: {0}, Rooms Placed: {1}/{2}/{3}",
                    _i++.ToString(), roomPool.Count(e => e.status == RoomStatus.PlacedAndValid).ToString(),
                    roomPool.Count(e => e.status == RoomStatus.PlacedAndValid).ToString(),
                    roomPool.Count.ToString()));

                await _waitForFrame;
            } while (!Failsafe && roomPool.Any(e => e.status != RoomStatus.PlacedAndValid));

            Debug.Log(Failsafe ? "Failsafe hit." : "Completed Randomisation.");
            isRandomising = false;
            await _waitForFrame;
        }
        
        /* NEW BRANCH APPROACH: THE SUPER COMEBACK REDEMPTION
         * Randomise {
         * Select connect point from existing, active & unconnected ones 
         * For every branch (connect point)
         *     IF CONNECT POINT TYPE IS ROOM {
         *         Start:
         *         Room corridor = place corridor
         *             list<room> roomsPlaced
         *             for each connect point on the corridor 
         *                 try placing a room using all of it's connect points
         *                 if success, continue
         *                 if fail, delete/deactivate corridor/rooms, goto Start
         *  
         *         if success
         *             mark rooms as placed, connect points
         *     }
         *     IF CONNECT POINT TYPE IS CORRIDOR {
         *         check amount of remaining rooms to place
         *         if 0, seal all connect points, generation complete
         *         if > 0, try place a remaining room onto each point in every combination
         *             if all combinations fail, generation is dud, restart
         *     }
         * } 
         */
        async Task<bool> NewBranch(ConnectPoint connectPoint) {
            BranchStart:

            int fails = 0;
            bool success = false;
            Room corridor = null;
            List<Room> rooms = new List<Room>(); 
                

            switch (connectPoint.t.parent.GetComponent<Room>().type) {
                case RoomType.Room:
                    corridor = await TryPlaceRoom(GetRandomCorridor(), connectPoint);
                    if (corridor.status != RoomStatus.PlacedAndValid) {
                        Debug.Log("Could not place corridor");
                        corridor.DisconnectAllPoints();
                        Destroy(corridor.gameObject);
                        goto BranchStart;
                    }
                    rooms = new List<Room>();
                    Room r = GetRandomRoom();
                    if (r) {
                        r = await TryPlaceRoom(r, corridor.connectPoints.Where(e => !e.isConnected).ToList());
                        rooms.Add(r);
                        
                        if (r.status == RoomStatus.PlacedAndValid) { //Success
                            success = true;
                            await new WaitForSeconds(generationSpeed);
                        } else {
                            //Fail
                            corridor.DisconnectAllPoints();
                            placedRooms.Remove(corridor);
                            Destroy(corridor.gameObject);
                            foreach (Room _r in rooms) {
                                _r.DisconnectAllPoints();
                                
                                _r.status = RoomStatus.Waiting;
                                _r.gameObject.SetActive(false);
                                placedRooms.Remove(_r);
                            }
                            Debug.Log("Branch Fail, restarting");
                            await new WaitForSeconds(generationSpeed);
                            goto BranchStart;
                        }
                    } else {
                        Debug.Log("Ran out of rooms to place");
                        _i = 999;
                    }
                    break;
                case RoomType.Corridor:
                    List<Room> availableRooms = AvailableRooms();
                    if (availableRooms.Count == 0) {
                        await PlaceDeadEnd(connectPoint);
                    } else {
                        Room room = GetRandomRoom();
                        if (room) {
                            room = await TryPlaceRoom(room, connectPoint);
                            if (!room) {
                                Debug.Log("Dud generation.");
                                _i = 999;
                                await new WaitForSeconds(generationSpeed);
                            }
                        }

                    }
                    break;
                default:
                    throw new Exception("WTF");
            }
            
            if (success) {
                Debug.Log("Branch Success");
                //corridor.isPlaced = true;
                placedRooms.Add(corridor);
                foreach (Room _r in rooms) {
                    placedRooms.Add(_r);
                    //_r.isPlaced = true;
                    foreach (ConnectPoint cp in _r.connectPoints)
                        cp.isConnected = true;
                    
                }
                await new WaitForSeconds(generationSpeed);
                return true;
            } else {
                return false;
            }
        }
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

        async Task<Room> TryPlaceRoom(Room r, List<ConnectPoint> cps) {
            foreach(ConnectPoint cp in cps) 
                r = await TryPlaceRoom(r, cp);
            return r;
        }
        //Try connecting the room to the connect point, cycling through each connect point on the room before giving up
        async Task<Room> TryPlaceRoom(Room r, ConnectPoint cp) {
            if (!r || cp == null) {
                Debug.Log("Room or CP given was null");
                return null;
            }

            int attempts = 0;
            bool valid;
            
            do {
                Debug.Log($"Trying {r.name} to {cp.t.name}:{cp.t.parent.name}...");
                
                r.gameObject.SetActive(true);
                
                r.DisconnectAllPoints();
                
                AlignConnectors(r.transform, r.connectPoints[attempts++].t, r.type, cp.t, cp.t.parent.GetComponent<Room>().type);
                //Important to wait for physics update before collision check
                //await new WaitForFixedUpdate();
                await new WaitForSeconds(generationSpeed);
                
                bool overlap = r.IsOverlapping();
                bool space = r.connectPoints.Any(e => !e.isConnected && e.HasSpaceInfront);

                Debug.Log($"Overlap: {overlap}, Space: {space}, Attempts: {attempts}, totalCPoints: {r.connectPoints.Count}");
                
                valid = !overlap && space && attempts < r.connectPoints.Count;

            } while (!valid && attempts < r.connectPoints.Count);
            
            if (valid) {
                r.status = RoomStatus.PlacedAndValid;
                cp.t.parent.GetComponent<Room>().status = RoomStatus.PlacedAndValid;

                placedRooms.Add(r);
                placedRooms.Add(cp.t.parent.GetComponent<Room>());
            } else {
                r.status = RoomStatus.PlacedAndValid;
                cp.t.parent.GetComponent<Room>().status = RoomStatus.PlacedAndValid;
            }
            
            cp.Connect(r.connectPoints[attempts - 1]);

            return r;
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
                t1.rotation = t2C.rotation * t1C.localRotation;
                //t1.rotation = t2C.rotation * Quaternion.Inverse(t1C.localRotation);
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
            return roomPool.Where(e =>
                e.status != RoomStatus.PlacedAndValid && !e.gameObject.activeSelf &&
                e.connectPoints.Any(c => !c.isConnected)).ToList();
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
            
            return validPoints;
        }
        ConnectPoint GetFreeConnectPoint() {
            //TODO : Pick the closest point to the SpawnPoint.
            List<ConnectPoint> freeConnectPoints = GetFreeConnectPoints();
            return freeConnectPoints.Count == 0 ? null : freeConnectPoints[Random.Range(0, freeConnectPoints.Count)];
        }
    }
}