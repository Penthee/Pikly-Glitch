//#define LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using Pikl.Extensions;
using Pikl.Profile;
using Pikl.States.Components;
using Pikl.Utils.RDS;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Pikl.Control {
    public enum RandomiserState {
        Idle,
        Randomising,
        Fail,
        Success
    };
    
    public class BetterLevelRandomiser : MonoBehaviour {

        [BoxGroup("Randomiser Settings")] [Range(1, 100)] public int iterationMax = 1;
        [BoxGroup("Randomiser Settings")] [Range(0, 20)] public int genFailTolerance, connectFailTolerance;
        [BoxGroup("Randomiser Settings")] public bool generateOnSuccess, generateOnFail, generationSlowdown;
        [BoxGroup("Randomiser Settings")] [ShowIf("generationSlowdown")][Range(0.02f, 5)] public float generationSpeed = 1;
        [BoxGroup("Randomiser Settings")] public bool randomiseOnStart, randomiseItems;

        public ItemRandomiser itemRandomiser;
        public Room startRoom;
        public Transform player, cam;

        public Color working, fail, success;
        public SpriteRenderer sprite;

        Room _deadEnd;
        [SerializeField] List<Room> roomPool = new List<Room>();
        [SerializeField] List<Room> corridorPool = new List<Room>();
        [SerializeField] List<Room> placedAndValidRooms = new List<Room>();
        

        [ReadOnly] public RandomiserState state;
        readonly WaitForEndOfFrame _waitForFrame = new WaitForEndOfFrame();
        readonly WaitForFixedUpdate _waitForFixed = new WaitForFixedUpdate();
        
        List<string> logs = new List<string>();
        
        string _currentSceneName;
        int _iterations = 0, _iterationFail = 0;
        bool Failsafe => ++_iterations >= iterationMax;
        bool GenerationFail => _iterationFail > InvalidRoomCount + genFailTolerance;
        int InvalidRoomCount => roomPool.Count(e => e.status != RoomStatus.PlacedAndValid);
        void Awake() {
            _currentSceneName = SceneManager.GetActiveScene().name;
            sprite = GetComponent<SpriteRenderer>();
            _deadEnd = Resources.Load<Room>("Prefabs/Levels/Connectors/Dead End");
            
            LoadRooms();
            LoadCorridors();
#if LOGGING 
            LoadLogs(); 
#endif
        }
        void Start() {
            if (randomiseOnStart)
                Randomise();
        }
        void LoadRooms() {
            roomPool.Clear();
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/" + _currentSceneName)) {
                if (o.name.Contains("StartingRoom")) continue;
                Room r = Instantiate(o).GetComponent<Room>();
                r.DisableRoom();
                roomPool.Add(r);
            }
            Debug.Log($"Loaded Rooms: {roomPool.Count}");
        }
        void LoadCorridors() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/Connectors")) {
                if (o.name.Contains("Dead End")) continue;
                corridorPool.Add(o.GetComponent<Room>());
                GameObjectMgr.I.CreatePool(o, 3);
            }
            
            GameObjectMgr.I.CreatePool(_deadEnd.gameObject, 8);
            
            Debug.Log($"Loaded Corridors: {corridorPool.Count}");
        }
#if LOGGING
        void LoadLogs() {
            string[] list = new[] {"SuccessTime", "FailTime", "Success", "Iteration", "PlacementSuccess" };
            string directory = Path.Combine(FileMgr.I.gameDataPath, "LOGGING");
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            foreach (string log in list) logs.Add(Path.Combine(directory, log));
            ClearLogs();
        }
#endif 
        [Button]
        void DoTheThing() {
            if (state == RandomiserState.Randomising)
                return;
            
            StateObjectMgr.I.WakeUp();
            StateObjectMgr.I.CacheObjectsInScene();
            StateObjectMgr.I.PauseEverything();
            
            Randomise();
        }
        void Reset() {
            itemRandomiser.Reset();
            startRoom.DisconnectAllPoints();
            startRoom.ClearAllFails();
            startRoom.status = RoomStatus.PlacedAndValid;

            foreach (Room room in roomPool) {
                DisconnectAndDestroy(room);
                room.ClearAllFails();
            }

            foreach (Room corridor in placedAndValidRooms.Where(e => e.type == RoomType.Corridor).ToList()) {
                DisconnectAndDestroy(corridor);
                corridor.ClearAllFails();
            }

            placedAndValidRooms.Clear();
            sprite.color = working;
        }

        async void Randomise() {
            Stopwatch watch = Stopwatch.StartNew();
            state = RandomiserState.Randomising;
            Reset();
            PlaceStartingRoom();
            PlacePlayer();

            _iterations = 0;
            _iterationFail = 0;
            
            do {
                List<Room> availableRooms = AvailableRoomsToPlace();

                if (availableRooms.Count > 0) {
                    foreach (Room corridor in corridorPool.Where(e => e.connectPoints.Count >= (_iterations == 0 ? 3 : 1)).Shuffle().Take(availableRooms.Count)) {
                    //foreach (Room corridor in corridorPool.Where(e => e.connectPoints.Count >= (availableRooms.Count > 6 ? 3 : 1)).Take(availableRooms.Count).Shuffle()) {
                    //foreach (Room corridor in corridorPool.Shuffle().Take(availableRooms.Count)) {
                        List<ConnectPoint> points = GetFreeConnectPoints(RoomType.Room);
                        if (points.Count <= 0) break;

                        await TryPlaceCorridor(corridor, points);
                    }
                }

                foreach (Room room in EnumerableExtensions.Shuffle(availableRooms)) {
                    List<ConnectPoint> points = GetFreeConnectPoints(RoomType.Corridor);
                    if (points.Count <= 0) break;

                    RoomStatus status = await TryPlaceRoom(room, points);
                    _iterationFail += status == RoomStatus.Invalid ? 1 : -1;
                }

                /*Debug.Log(string.Format("Iteration:{0} | Total:{5} Active:{1}/{2} Valid:{3}/{4}",
                    (++_iterations).ToString(), 
                    roomPool.Count(e => e.status == RoomStatus.Active).ToString(), roomPool.Count(e => e.status == RoomStatus.Waiting).ToString(),
                    roomPool.Count(e => e.status == RoomStatus.PlacedAndValid).ToString(), roomPool.Count(e => e.status == RoomStatus.Invalid).ToString(),
                    roomPool.Count.ToString()));*/

            } while (!Failsafe && !GenerationFail && InvalidRoomCount > 0);
            
            watch.Stop();
            
            if (InvalidRoomCount == 0) {
                Success();
            } else if (Failsafe) {
                Fail();
                Debug.Log($"Failsafe hit");
            } else if (GenerationFail) {
                Fail();
                Debug.Log($"Generation failed!");
            } else {
                state = RandomiserState.Fail;
                throw new Exception("What the fudge, unexpected state at end of generation");
            }
#if LOGGING
            WriteLineToLog(InvalidRoomCount != 0 ? 1 : 0, watch.ElapsedMilliseconds.ToString());
            WriteLineToLog(2, (InvalidRoomCount == 0).ToString());
            WriteLineToLog(3, string.Join(",", _iterations, _iterationFail));
#endif
            await _waitForFrame;
        }

        void Success() {
            sprite.color = success;
            state = RandomiserState.Success;
            
            foreach(Room r in placedAndValidRooms) r.EnableRoom();
            
            CleanupEmptyCorridors();
            SealAllExits();
//#if !UNITY_EDITOR && !DEBUG
            if (!generateOnSuccess) DestroyAllPools();  
//#endif
            if (randomiseItems) itemRandomiser.Randomise();
            if (generateOnSuccess) Invoke("DoTheThing", 1);
            Debug.Log($"Generation Success!");
            StateObjectMgr.I.UnPauseEverything();
        }

        void Fail() {
            sprite.color = fail;
            if (generateOnFail)
                Invoke("DoTheThing", 1);
            state = RandomiserState.Fail;
        }
        
        async Task TryPlaceCorridor(Room corridor, List<ConnectPoint> points) {
            Room c = GameObjectMgr.I.Spawn(corridor.gameObject).GetComponent<Room>();
            //Room c = Instantiate(corridor);
            RoomStatus status = await TryPlaceRoom(c, points);
            if (status == RoomStatus.Invalid)
                DisconnectAndDestroy(c);
        }
        async Task<RoomStatus> TryPlaceRoom(Room r, List<ConnectPoint> points) {
            //r.EnableRoom();
            r.polygonBounds.enabled = true;
            r.status = RoomStatus.Active;
            
            foreach (ConnectPoint cp in points) {
                if (cp == null) {
                    Debug.Log("CP given was null");
                    continue;
                }
                //Debug.Log($"Trying: {r.name} -> {cp.t.parent.name} : {cp.t.name}");

                for (int i = 0; i < r.connectPoints.Count; i++) {
                    if (r.HasFailedToConnect(cp) || cp.ConnectionFailCount() > connectFailTolerance) {
                        //Debug.Log($"Already failed, skipping: {r.name} -> {cp.t.parent.name} : {cp.t.name}");
                        continue;
                    }

                    Align(cp.t.parent, cp.t, r.transform, r.connectPoints[0].t);
                    
                    await Slowdown();
                    
                    bool overlap = r.IsOverlapping();
                    bool space = r.connectPoints.Any(e => !e.isConnected && e.HasSpaceInfront);
                    bool valid = !overlap && space;
                    //Debug.Log($"Overlap: {overlap}, Space: {space}, Attempts: {i}, totalCPoints: {r.connectPoints.Count}");
#if LOGGING
                    //WriteLineToLog(4, $"{r.name.Split('(')[0]},{valid.ToString()}");
#endif
                    if (!valid) {
                        MarkRoomAsInvalid(r, i, cp);
                        continue;
                    }
                    MarkRoomAsValid(r, i, cp);
                    return RoomStatus.PlacedAndValid;
                }
                /*Debug.Log(valid
                    ? $"Placement Success: {r.name} -> {cp.t.parent.name} : {cp.t.name}"
                    : $"Placement Fail: {r.name} -> {cp.t.parent.name} : {cp.t.name}");*/
            }
            DisconnectAndDestroy(r);
            return RoomStatus.Invalid;
        }
        void PlaceStartingRoom() {
            startRoom.transform.position = Vector3.zero;
            startRoom.transform.Rotate(Vector3.forward, Random.Range(0, 360));
        }
        void PlacePlayer() {
            Transform spawnPoint = startRoom.transform.Find("SpawnPoint");
            if (spawnPoint != null) {
                player.position = spawnPoint.position;
            } else {
                Debug.LogError("NO SPAWN POINT FOUND FOR PLAYER IN STARTING ROOM FOR LEVEL " + _currentSceneName);
                player.position = startRoom.transform.position;
            }

            cam.position = new Vector3(player.position.x, player.position.y, -10);
            //TODO - refactor player control stuff - do the colliders
            player.GetComponent<PlayerHealth>().Invulnerable = true;
        }
        void CleanupEmptyCorridors() {
            foreach (Room corridor in placedAndValidRooms.Where(e => e.type == RoomType.Corridor).ToList()) {
                int roomConnectCount = 0;
                foreach (ConnectPoint cp in corridor.connectPoints) {
                    if (!cp.isConnected)
                        continue;
                    roomConnectCount += cp.connectedTo.t.parent.GetComponent<Room>().type == RoomType.Room ? 1 : 0;
                }

                if (roomConnectCount > 1)
                    continue;
                
                foreach (ConnectPoint cp in corridor.connectPoints) {
                    Room room = cp.t.parent.GetComponent<Room>();
                    if (room.type == RoomType.Corridor)
                        DisconnectAndDestroy(cp.t.GetComponent<Room>());
                }
                DisconnectAndDestroy(corridor);
            }
        }
        void SealAllExits() {
            IEnumerable<ConnectPoint> points =
                GetFreeConnectPoints(RoomType.Corridor).Concat(GetFreeConnectPoints(RoomType.Room));
            foreach (ConnectPoint cp in points) SealWithDeadEnd(cp);
        }
        void SealWithDeadEnd(ConnectPoint cp) {
            Room r = GameObjectMgr.I.Spawn(_deadEnd.gameObject).GetComponent<Room>();
            Align(cp.t.parent, cp.t, r.transform, r.connectPoints[0].t);
            MarkRoomAsValid(r, 0, cp);
        }
        void Align(Transform t1, Transform t1C, Transform t2, Transform t2C) {
            //t1: connecting to this object, t2: object to be placed
            if (t1C.right == t2C.right) t2.Rotate(t2.forward, 5);

            t2.rotation = Quaternion.FromToRotation(t2C.right, -t1C.right) * t2.rotation;
            t2.position = t1C.position + (t2.position - t2C.position);
        }
        Room GetRandomCorridor() {
            return Instantiate(corridorPool[Random.Range(0, corridorPool.Count)]);
        }
        Room GetRandomRoom() {
            List<Room> ununsedRooms = AvailableRoomsToPlace();
            //foreach(var bah in ununsedRooms) Debug.Log($"bah: {bah.name}");
            return ununsedRooms.Count > 0 ? ununsedRooms[Random.Range(0, ununsedRooms.Count)] : null;
        }
        List<Room> AvailableRoomsToPlace() {
            return roomPool.Where(e =>
                e.status != RoomStatus.PlacedAndValid && e.status != RoomStatus.Active &&
                e.connectPoints.All(c => !c.isConnected)).ToList();
        }
        List<ConnectPoint> GetFreeConnectPoints(RoomType type) {
            List<ConnectPoint> validPoints = new List<ConnectPoint>();
            
            if (type == RoomType.Room) {
                ConnectPoint startingPoint = startRoom.connectPoints.FirstOrDefault(e => !e.isConnected);

                if (startingPoint != null && startingPoint.t != null)
                    validPoints.Add(startingPoint);
            }

            foreach (Room room in placedAndValidRooms.Where(e => e.type == type && e.connectPoints.Any(x => !x.isConnected))) {
                int count = AvailableRoomsToPlace().Count;
                if (count <= 1) {
                    foreach (ConnectPoint point in room.connectPoints.Where(e => !e.isConnected))
                        validPoints.Add(point);
                } else {
                    foreach (ConnectPoint point in room.connectPoints.Where(e => !e.isConnected).Take(count))
                        validPoints.Add(point);
                }
            }
            
            return validPoints;
        }
        void MarkRoomAsValid(Room r, int i, ConnectPoint cp) {
            r.EnableRoom();
            r.status = RoomStatus.PlacedAndValid;
            r.connectPoints[i].Connect(cp);
            placedAndValidRooms.Add(r);
        }
        void MarkRoomAsInvalid(Room r, int i, ConnectPoint cp) {
            r.status = RoomStatus.Invalid;
            r.connectPoints[i].RecordFail(cp);
            
            if (placedAndValidRooms.Contains(r))
                placedAndValidRooms.Remove(r);
        }
        void DisconnectAndDestroy(Room r) {
            if (r == null) return;
            
            r.status = RoomStatus.Waiting;
            r.DisconnectAllPoints();
            if (placedAndValidRooms.Contains(r)) placedAndValidRooms.Remove(r);
            
            if (r.type == RoomType.Corridor)
                //Destroy(r.gameObject);
                GameObjectMgr.I.Recycle(r.gameObject);
            else
                r.DisableRoom();
            //r.gameObject.SetActive(false);
        }
//#if !UNITY_EDITOR && !DEBUG
        void DestroyAllPools() {
            foreach (Room c in corridorPool) {
                GameObjectMgr.I.DestroyPooled(c.gameObject);
            }
            
            GameObjectMgr.I.DestroyPooled(_deadEnd.gameObject);
        }
//#endif 
        async Task Slowdown() {
#if UNITY_EDITOR || DEBUG
            if (generationSlowdown) {
                await new WaitForSeconds(generationSpeed);
                return;
            }
#endif
            await _waitForFixed;
        }
#if LOGGING 
        void ClearLogs() {
            foreach (string log in logs) {
                if (File.Exists(log))
                    File.Delete(log);
                File.Create(log);
            }
        }
        void WriteLineToLog(int logIndex, string value) {
            using (StreamWriter writer = new StreamWriter(new FileStream(logs[logIndex], FileMode.Append))) {
                writer.WriteLine(value);
                writer.Close();
            }
        }
#endif
    }
}