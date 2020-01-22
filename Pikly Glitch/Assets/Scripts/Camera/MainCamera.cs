using UnityEngine;
using System.Collections;
using Pikl.Extensions;
using TeamUtility.IO;
using DG.Tweening;

namespace Pikl.Utils.Cameras {
    public class MainCamera : MonoBehaviour {
        protected MainCamera() { }

        public static MainCamera I;

        public Camera losCamera, mapCamera, mapCamera2, mapCamera3, mapCamera4;
        public Ease mapEase;
        public Transform target;
        public Vector3 offset;
        [HideInInspector]
        public Vector3 shakeOffset;

        public float followSpeed = 0.05f, mapCameraSpeed = 0.2f, targetSize = 4, zoomMagnitude = 1, zoomSmoothing = 0.25f, 
                     minSize = 1, boundsLockHardness = 0.2f, losSizeDiff = 0.25f, mapSize = 13, mapTransitionTime = 1;
        public Bounds bounds;
        public bool mapMode, mapButtonIsDown, scaleWidth = true, gizmo = true, UI;

        [HideInInspector]
        public float origSize, scaleOffset, mapLastToggle;

        new Camera camera;
        Player.Player player;
        Vector3 lastPos;
        Vector2 velocity;

        public float Width {
            get {
                return camera.orthographicSize * 2 * camera.aspect;
            }
        }

        void Awake() {
            I = this;
            camera = Camera.main;
            origSize = camera.orthographicSize;
            targetSize = origSize;
        }

        void Start() {
            StartCoroutine(FindPlayer());
            
            if (scaleWidth)
                ScaleWidth();
            
            //losCamera = GetComponentInChildren<Camera>();
            //CameraShaker.I.StartShake(ShakePresets.HandheldCamera);
        }
        
        void ScaleWidth() {
            float TARGET_WIDTH = 2560f;
            float TARGET_HEIGHT = 1440f;
            int PIXELS_TO_UNITS = 100; // 1:1 ratio of pixels to units

            float desiredRatio = TARGET_WIDTH / TARGET_HEIGHT;
            float currentRatio = (float)Screen.width / (float)Screen.height;

            if (currentRatio >= desiredRatio) {
                // Our resolution has plenty of width, so we just need to use the height to determine the camera size
                camera.orthographicSize = TARGET_HEIGHT / 4 / PIXELS_TO_UNITS;
            } else {
                // Our camera needs to zoom out further than just fitting in the height of the image.
                // Determine how much bigger it needs to be, then apply that to our original algorithm.
                float differenceInSize = desiredRatio / currentRatio;
                camera.orthographicSize = TARGET_HEIGHT / 4 / PIXELS_TO_UNITS * differenceInSize;
            }
        }


        IEnumerator FindPlayer() {
            while (target == null) {

                GameObject p = GameObject.Find("Player");

                if (p != null)
                    target = p.transform;

                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSeconds(1f);
            //target = null;
            //transform.SetParent((Ref.I["Mover"] as GameObject).transform);
        }

        delegate float Slider(float val, string prefix, float min, float max, int pad);

        void OnGUI() {
            if (UI) {
                Slider s = delegate (float val, string prefix, float min, float max, int pad) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(prefix, GUILayout.MaxWidth(pad));
                    val = GUILayout.HorizontalSlider(val, min, max);
                    GUILayout.Label(val.ToString("F2"), GUILayout.MaxWidth(50));
                    GUILayout.EndHorizontal();
                    return val;
                };

                GUI.Box(new Rect(Screen.width - 310 - 250, 10, 250, 215), "Camera Properties");
                GUILayout.BeginArea(new Rect(Screen.width - 310 - 240, 35, 230, 215));

                offset.x = s(offset.x, "Offset X", -10, 10, 50);
                offset.y = s(offset.y, "Offset Y", -10, 10, 50);

                followSpeed = s(followSpeed, "Follow Speed", 0f, 1, 100);
                zoomMagnitude = s(zoomMagnitude, "Zoom Magnitude", 0, 3, 100);
                zoomSmoothing = 1 - s(1 - zoomSmoothing, "Zoom Smoothing", 0f, 0.99f, 100);
                minSize = s(minSize, "Minimum Zoom", 0.5f, 2, 100);
                boundsLockHardness = s(boundsLockHardness, "Bounds Lock Hardness", 0f, 1, 100);

                GUILayout.EndArea();
            }
        }

        void Update() {
            //if (shaker.scaleAddShake == 0)
                StackScrollInput();

            //if (target != null)
            //    bounds = target.GetComponent<StupidBounds>().bounds;
            if (InputMgr.GetAxisRaw("Map") != 0) {
                if (!mapButtonIsDown) {
                    if (mapLastToggle + mapTransitionTime < Time.time) {
                        mapMode = !mapMode;
                        mapButtonIsDown = true;
                        mapLastToggle = Time.time;

                        if (mapMode) {
                            mapCamera.gameObject.SetActive(true);

                            mapCamera.orthographicSize = camera.orthographicSize;
                            mapCamera2.orthographicSize = camera.orthographicSize;
                            mapCamera3.orthographicSize = camera.orthographicSize;
                            mapCamera4.orthographicSize = camera.orthographicSize;

                            mapCamera.transform.position = target.position;
                            mapCamera.transform.rotation = transform.rotation;

                            DOTween.To(() => mapCamera.orthographicSize, x => mapCamera.orthographicSize = x, mapSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera2.orthographicSize, x => mapCamera2.orthographicSize = x, mapSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera3.orthographicSize, x => mapCamera3.orthographicSize = x, mapSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera4.orthographicSize, x => mapCamera4.orthographicSize = x, mapSize, mapTransitionTime).SetEase(mapEase);

                            BoxCollider box = mapCamera.GetComponentInChildren<BoxCollider>();
                            DOTween.To(() => box.center, x => box.center = x, Vector3.zero, 1).SetEase(mapEase);
                        } else {
                            DOTween.To(() => mapCamera.orthographicSize, x => mapCamera.orthographicSize = x, camera.orthographicSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera2.orthographicSize, x => mapCamera2.orthographicSize = x, camera.orthographicSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera3.orthographicSize, x => mapCamera3.orthographicSize = x, camera.orthographicSize, mapTransitionTime).SetEase(mapEase);
                            DOTween.To(() => mapCamera4.orthographicSize, x => mapCamera4.orthographicSize = x, camera.orthographicSize, mapTransitionTime).SetEase(mapEase);

                            DOTween.To(() => mapCamera.transform.position, x => mapCamera.transform.position = x, target.position, mapTransitionTime + 0.25f).SetEase(mapEase)
                                   .OnComplete(() => mapCamera.gameObject.SetActive(false));

                            BoxCollider box = mapCamera.GetComponentInChildren<BoxCollider>();
                            DOTween.To(() => box.center, x => box.center = x, new Vector3(0, 0, -10), mapTransitionTime).SetEase(mapEase);
                        }
                    }
                }
            } else {
                mapButtonIsDown = false;
            }
        }

        Vector3 toFollow, tPos;
        void FixedUpdate() {
            //shakeOffset = shake.s.posAddShake;
            //transform.localEulerAngles = shake.s.scaleAddShake;
            //scaleOffset = shake.s.scaleAddShake;
            if (mapMode)
                CalcMapCamera();
            else
                CalcCamera();
        }

        void CalcCamera() {
            if (target != null) {
                toFollow = target.transform.position;

                tPos = Vector2.zero;
                tPos.x = Mathf.Lerp(transform.position.x, toFollow.x, followSpeed);
                tPos.y = Mathf.Lerp(transform.position.y, toFollow.y, followSpeed);
                tPos.z = transform.position.z;

                transform.position = tPos + offset + shakeOffset;
                GetCameraOutOfWalls();
                //var something = tPos + offset + shakeOffset;
                //GetComponent<Rigidbody2D>().AddForce((something - transform.position).normalized * followSpeed);
            }

            camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Mathf.Clamp(targetSize + scaleOffset, 0, bounds.extents.y), zoomSmoothing);
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, minSize, Mathf.Clamp(origSize + scaleOffset, 0, bounds.extents.y));

            //losCamera.orthographicSize = Mathf.Clamp(camera.orthographicSize - losSizeDiff, minSize - losSizeDiff, bounds.extents.y);

            ClampCameraInBounds();
        }

        void CalcMapCamera() {
            Vector2 input = new Vector2(InputMgr.GetAxis("Horizontal"), InputMgr.GetAxis("Vertical"));

            //camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, Mathf.Clamp(targetSize + scaleOffset, 0, bounds.extents.y), zoomSmoothing);

            Vector3 targetPos = mapCamera.transform.position.To2DXY() + input * mapCameraSpeed;
            targetPos.z = transform.position.z;

            mapCamera.transform.position = targetPos;
        }

        Vector3 insidePoint = Vector3.zero;
        void GetCameraOutOfWalls() {
            Ray r = camera.ViewportPointToRay(Vector3.one * 0.5f);
            RaycastHit2D rh = Physics2D.Raycast(r.origin, r.direction, 1000, 1 << LayerMask.NameToLayer("Terrain"));

            if (rh) {
                //Debug.Log("Camera is inside a wall!");

                insidePoint = rh.collider.bounds.ClosestPoint(rh.point);
                insidePoint.z = transform.position.z;
                insidePoint -= Player.Player.I.input.MouseDir * 0.01f;

                transform.position = insidePoint;
            }
            //Debug.DrawRay(camera.transform.position.To2DXY(), insidePoint, Color.yellow);
        }

        void StackScrollInput() {
            float input = 0;

            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.KeyboardConfiguration)
                input = -InputMgr.GetAxisRaw("Zoom") * zoomMagnitude;

            if (input != 0)
                targetSize += input;

            targetSize = Mathf.Clamp(targetSize, minSize, origSize);
        }

        void ClampCameraInBounds() {
            float vertExtent = camera.orthographicSize;
            float horExtent = vertExtent * Screen.width / Screen.height;

            float minX = (horExtent - bounds.extents.x) + bounds.center.x;
            float maxX = (bounds.extents.x - horExtent) + bounds.center.x;
            float minY = (vertExtent - bounds.extents.y) + bounds.center.y;
            float maxY = (bounds.extents.y - vertExtent) + bounds.center.y;

            Vector3 t = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), Mathf.Clamp(transform.position.y, minY, maxY), -1);
            transform.position = Vector3.Lerp(transform.position, t, boundsLockHardness);
        }

        void OnDrawGizmos() {
            if (gizmo) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}