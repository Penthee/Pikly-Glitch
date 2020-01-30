#define LOGGING

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
        [BoxGroup("Randomiser Settings")] public bool randomiseItems;

        public ItemRandomiser itemRandomiser;
        public Room startRoom;
        public Transform player, cam;

        public Color working, fail, success;
        public SpriteRenderer sprite;

        [SerializeField] Room deadEnd;
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
            
            LoadRooms();
            LoadCorridors();
#if LOGGING 
            LoadLogs(); 
#endif
        }
        void Start() {
            //Randomise();
        }
        void LoadRooms() {
            roomPool.Clear();
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/" + _currentSceneName)) {
                if ( /*o.name.Contains("Teleporter") || */o.name.Contains("StartingRoom")) continue;

                Room r = Instantiate(o).GetComponent<Room>();
                r.gameObject.SetActive(false);
                roomPool.Add(r);
            }
            Debug.Log($"Loaded Rooms: {roomPool.Count}");
        }
        void LoadCorridors() {
            foreach (GameObject o in Resources.LoadAll<GameObject>("Prefabs/Levels/Connectors")) {
                if (o.name.Contains("Dead End")) continue;

                corridorPool.Add(o.GetComponent<Room>());
            }
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

            Randomise();
        }
        void Reset() {
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
                    foreach (Room corridor in corridorPool.Shuffle().Take(availableRooms.Count)) {
                        List<ConnectPoint> points = GetFreeConnectPoints(RoomType.Room);
                        if (points.Count <= 0) break;

                        await TryPlaceCorridor(corridor, points);
                    }
                }

                foreach (Room room in availableRooms.Shuffle()) {
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
                sprite.color = success;
                CleanupEmptyCorridors();
                SealAllExits();
                if (generateOnSuccess)
                    Invoke("DoTheThing", 1);
                state = RandomiserState.Success;
                Debug.Log($"Generation Success!");
                if (randomiseItems)
                    itemRandomiser.Randomise();
            } else if (Failsafe) {
                sprite.color = fail;
                if (generateOnFail)
                    Invoke("DoTheThing", 1);
                state = RandomiserState.Fail;
                Debug.Log($"Failsafe hit");
            } else if (GenerationFail) {
                sprite.color = fail;
                if (generateOnFail)
                    Invoke("DoTheThing", 1);
                state = RandomiserState.Fail;
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
        async Task<RoomStatus> TryPlaceCorridor(Room corridor, List<ConnectPoint> points) {
            Room c = Instantiate(corridor);
            RoomStatus status = await TryPlaceRoom(c, points);
            if (status == RoomStatus.Invalid)
                DisconnectAndDestroy(c);
            return status;
        }
        
        async Task<RoomStatus> TryPlaceRoom(Room r, List<ConnectPoint> points) {
            if (!r) {
                Debug.Log("Room given was null");
                return RoomStatus.Invalid;
            }

            if (points.Count == 0) {
                Debug.Log("Empty ConnectPoint list");
                return RoomStatus.Invalid;
            }
            
            bool valid = false;
            
            foreach (ConnectPoint cp in points) {
                if (cp == null) {
                    Debug.Log("CP given was null");
                    continue;
                }
                
                r.gameObject.SetActive(true);
                r.status = RoomStatus.Active;
                
                //Debug.Log($"Trying: {r.name} -> {cp.t.parent.name} : {cp.t.name}");

                for (int i = 0; i < r.connectPoints.Count; i++) {
                    if (r.FailedToConnect(cp) || cp.ConnectionFailCount() > connectFailTolerance) {
                        //Debug.Log($"Already failed, skipping: {r.name} -> {cp.t.parent.name} : {cp.t.name}");
                        continue;
                    }

                    Align1(r.transform, r.connectPoints[0].t, cp.t);
                    //Align2(r.transform, r.connectPoints[0].t, cp.t);
                    //Align3(r.transform, r.connectPoints[0].t, cp.t);
                    //Alignment(r.transform, r.connectPoints[0].t, cp.t.parent, cp.t);
                    
                    await Slowdown(true);
                    
                    bool overlap = r.IsOverlapping();
                    bool space = r.connectPoints.Any(e => !e.isConnected && e.HasSpaceInfront);
                    valid = !overlap && space;
                    //Debug.Log($"Overlap: {overlap}, Space: {space}, Attempts: {i}, totalCPoints: {r.connectPoints.Count}");

                    if (!valid) {
                        MarkRoomAsInvalid(r, i, cp);
                        continue;
                    }
                    MarkRoomAsValid(r, i, cp);
                    break;
                }

                /*Debug.Log(valid
                    ? $"Placement Success: {r.name} -> {cp.t.parent.name} : {cp.t.name}"
                    : $"Placement Fail: {r.name} -> {cp.t.parent.name} : {cp.t.name}");*/
                
                if (valid) break;
            }

#if LOGGING
            WriteLineToLog(4, $"{r.name.Split('(')[0]},{valid.ToString()}");
#endif
            if (valid) return RoomStatus.PlacedAndValid;
            
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
            Room r = Instantiate(deadEnd);
            Align1(r.transform, r.connectPoints[0].t, cp.t);
            //Align2(r.transform, r.connectPoints[0].t, cp.t);
            MarkRoomAsValid(r, 0, cp);
        }
        void Align1(Transform target, Transform targetC, Transform sourceC) {
            target.rotation = sourceC.rotation * targetC.localRotation;
            target.position = sourceC.position + (target.position - targetC.position);
        }
        void Align2(Transform target, Transform targetChild, Transform sourceChild) {
            target.rotation = sourceChild.rotation * Quaternion.Inverse(target.rotation * Quaternion.Inverse(targetChild.rotation));
            target.position = sourceChild.position + (target.position - targetChild.position);
        }
        void Align3(Transform target, Transform targetChild, Transform sourceChild) {
            Vector3 rotation = sourceChild.eulerAngles;
            rotation.z += 180;
            target.rotation = sourceChild.rotation * target.rotation;
            //target.Rotate(Vector3.forward * -270f, Space.Self);
            //target.eulerAngles = sourceChild.Rotate() rotation; 
            target.position = sourceChild.position + (target.position - targetChild.position);
        }
        void Alignment(Transform tr1P, Transform tr1C, Transform tr2P, Transform tr2C) {
            Vector3 v1 = tr1P.position - tr1C.position;
            Vector3 v2 = tr2C.position - tr2P.position;
            tr1P.rotation = Quaternion.FromToRotation(v1, v2) * tr1P.rotation;
            tr1P.position = tr2C.position + v2.normalized * v1.magnitude;
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
                e.status != RoomStatus.PlacedAndValid && !e.gameObject.activeSelf &&
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
                Destroy(r.gameObject);
            else
                r.gameObject.SetActive(false);
        }
        
        async Task Slowdown(bool fixedUpdate = false) {
#if UNITY_EDITOR || DEBUG
            if (generationSlowdown) {
                await new WaitForSeconds(generationSpeed);
                return;
            }
#endif
            if (fixedUpdate)
                await _waitForFixed;
            else
                await _waitForFrame;
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