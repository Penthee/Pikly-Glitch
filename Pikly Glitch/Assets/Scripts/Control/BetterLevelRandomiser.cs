#define LOGGING
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Pikl.Utils.RDS;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using Pikl.Extensions;
using Pikl.Profile;
using Pikl.States.Components;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace Pikl.Control {
    public class BetterLevelRandomiser : MonoBehaviour {

        [Range(1, 100)]
        public int iterationMax = 1;
        
        public bool generateOnSuccess, generateOnFail, generationSlowdown;
        [EnableIf("generationSlowdown")][Range(0.02f, 5)]
        public float generationSpeed = 1;
        public Room startingRoom;
        public Transform player, cam;

        public Color working, fail, success;
        public SpriteRenderer sprite;

        [SerializeField] List<Room> roomPool = new List<Room>();
        [SerializeField] List<Room> corridorPool = new List<Room>();
        [SerializeField] List<Room> placedAndValidRooms = new List<Room>();
        
        [SerializeField] Room deadEnd;

        [ReadOnly] public bool isRandomising;
        readonly WaitForEndOfFrame _waitForFrame = new WaitForEndOfFrame();
        readonly WaitForFixedUpdate _waitForFixed = new WaitForFixedUpdate();
        
        List<string> logs = new List<string>();
        
        string _currentSceneName;
        int _iterations = 0, _iterationFail = 0;
        bool Failsafe => _iterations >= iterationMax;
        bool GenerationFail => _iterationFail > InvalidRoomCount;
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
            string[] list = new[] {"Time", "Success", "Iteration"};
            string directory = Path.Combine(FileMgr.I.gameDataPath, "LOGGING");
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            foreach (string log in list) logs.Add(Path.Combine(directory, log));
            ClearLogs();
        }
#endif 
        [Button]
        void DoTheThing() {
            if (isRandomising)
                return;

            Randomise();
        }
        void Reset() {
            startingRoom.DisconnectAllPoints();
            startingRoom.status = RoomStatus.PlacedAndValid;
            
            foreach (Room room in roomPool) DisconnectAndDestroy(room);

            foreach (Room corridor in placedAndValidRooms.Where(e => e.type == RoomType.Corridor).ToList()) DisconnectAndDestroy(corridor);

            placedAndValidRooms.Clear();
            sprite.color = working;
        }

        async void Randomise() {
            Stopwatch watch = Stopwatch.StartNew();
            isRandomising = true;
            Reset();
            PlaceStartingRoom();
            PlacePlayer();

            _iterations = 0;
            _iterationFail = 0;
            
            do {
                foreach (Room corridor in corridorPool.Shuffle()) {
                    List<ConnectPoint> points = GetFreeConnectPoints(RoomType.Room);
                    if (points.Count <= 0) continue;
                    
                    Room c = Instantiate(corridor);
                    RoomStatus status = await TryPlaceRoom(c, points);
                    if (status != RoomStatus.Invalid) continue;
                    c.DisconnectAllPoints();
                    Destroy(c.gameObject);
                }

                foreach (Room room in AvailableRoomsToPlace().Shuffle()) {
                    List<ConnectPoint> points = GetFreeConnectPoints(RoomType.Corridor);
                    if (points.Count <= 0) continue;

                    RoomStatus status = await TryPlaceRoom(room, points);
                    if (status == RoomStatus.Invalid) _iterationFail++;
                }

                Debug.Log(string.Format("Iteration: [{0}] : Total:{5} Active:{1}/{2} Valid:{3}/{4}",
                    (++_iterations).ToString(), 
                    roomPool.Count(e => e.status == RoomStatus.Active).ToString(), roomPool.Count(e => e.status == RoomStatus.Waiting).ToString(),
                    roomPool.Count(e => e.status == RoomStatus.PlacedAndValid).ToString(), roomPool.Count(e => e.status == RoomStatus.Invalid).ToString(),
                    roomPool.Count.ToString()));

                //await Slowdown();
            } while (!Failsafe && !GenerationFail && InvalidRoomCount > 0);

#if LOGGING
            watch.Stop();
#endif

            if (InvalidRoomCount == 0) {
                sprite.color = success;
                IEnumerable<ConnectPoint> points = GetFreeConnectPoints(RoomType.Corridor).Concat(GetFreeConnectPoints(RoomType.Room));
                foreach (ConnectPoint cp in points) SealWithDeadEnd(cp);
                Debug.Log($"Generation Success!");
                if (generateOnSuccess)
                    Invoke("DoTheThing", 1);
            } else if (Failsafe) {
                sprite.color = fail;
                Debug.Log($"Failsafe hit");
                if (generateOnFail)
                    Invoke("DoTheThing", 1);
            } else if (GenerationFail) {
                sprite.color = fail;
                Debug.Log($"Generation failed!");
                if (generateOnFail)
                    Invoke("DoTheThing", 1);
            }
#if LOGGING
            WriteLineToLog(0, watch.ElapsedMilliseconds.ToString());
            WriteLineToLog(1, (InvalidRoomCount == 0).ToString());
            WriteLineToLog(2, string.Join(":", _iterations, _iterationFail));
#endif
            isRandomising = false;
            await _waitForFrame;
        }
        async Task<RoomStatus> TryPlaceRoom(Room r, List<ConnectPoint> cps) {
            if (!r) {
                Debug.Log("Room given was null");
                return RoomStatus.Invalid;
            }

            if (cps.Count == 0) {
                Debug.Log("Empty ConnectPoint list");
                return RoomStatus.Invalid;
            }
            
            bool valid = false;
            
            foreach (ConnectPoint cp in cps) {
                if (cp == null) {
                    Debug.Log("CP given was null");
                    continue;
                }
                
                r.gameObject.SetActive(true);
                r.status = RoomStatus.Active;
                
                Debug.Log($"Trying: {r.name} -> {cp.t.parent.name} : {cp.t.name}");

                for (int i = 0; i < r.connectPoints.Count; i++) {
                    AlignConnectors(r.transform, r.connectPoints[i].t, r.type, cp.t, cp.t.parent.GetComponent<Room>().type);
                    
                    await Slowdown(true);
                    
                    bool overlap = r.IsOverlapping();
                    bool space = r.connectPoints.Any(e => !e.isConnected && e.HasSpaceInfront);
                    valid = !overlap && space;
                    Debug.Log($"Overlap: {overlap}, Space: {space}, Attempts: {i}, totalCPoints: {r.connectPoints.Count}");

                    if (!valid) continue;
                    MarkRoomAsValid(r, i, cp);
                    break;
                }

                Debug.Log(valid
                    ? $"Placement Success: {r.name} -> {cp.t.parent.name} : {cp.t.name}"
                    : $"Placement Fail: {r.name} -> {cp.t.parent.name} : {cp.t.name}");
                
                if (valid) break;
            }
                
            if (valid) return RoomStatus.PlacedAndValid;
            
            DisconnectAndDestroy(r);
            return RoomStatus.Invalid;
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
        void SealWithDeadEnd(ConnectPoint cp) {
            Room r = Instantiate(deadEnd);
            AlignConnectors(r.transform, r.connectPoints[0].t, RoomType.Corridor, cp.t, cp.t.parent.GetComponent<Room>().type);
            MarkRoomAsValid(r, 0, cp);
        }
        void AlignConnectors(Transform t1, Transform t1C, RoomType t1T, Transform t2C, RoomType t2T) {
            if (t1T == RoomType.Corridor && t2T == RoomType.Corridor)
                //t1.rotation = t2C.rotation * t1C.localRotation;
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
                ConnectPoint startingPoint = startingRoom.connectPoints.FirstOrDefault(e => !e.isConnected);

                if (startingPoint != null && startingPoint.t != null)
                    validPoints.Add(startingPoint);
            }

            foreach (Room room in placedAndValidRooms.Where(e => e.type == type && e.connectPoints.Any(x => !x.isConnected))) {
                foreach(ConnectPoint point in room.connectPoints.Where(e => !e.isConnected))
                    validPoints.Add(point);
            }
            
            return validPoints;
        }
        void MarkRoomAsValid(Room r, int i, ConnectPoint cp) {
            r.status = RoomStatus.PlacedAndValid;
            r.connectPoints[i].Connect(cp);
            placedAndValidRooms.Add(r);
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