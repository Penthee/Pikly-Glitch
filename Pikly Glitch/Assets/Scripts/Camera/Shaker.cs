using UnityEngine;
using System.Collections.Generic;
using Pikl.Utils.Cameras;

namespace Pikl.Utils.Shaker {
    public class Shaker : Singleton<Shaker> {
        protected Shaker() { }

        public List<ShakeInstance> ActiveShakes {
            get {
                return activeShakes;
            }
        }

        List<ShakeInstance> activeShakes = new List<ShakeInstance>();
        ShakeInstance[] activeShakesArray;

        void Update() {
            if (activeShakes.Count == 0)
                return;

            activeShakesArray = new ShakeInstance[activeShakes.Count];
            activeShakes.CopyTo(activeShakesArray);

            List<int> toRemove = new List<int>();
            int i = 0;
            foreach (ShakeInstance si in activeShakesArray) {
                si.PosAddShake = Vector3.zero;
                si.RotAddShake = Vector3.zero;
                si.ScaleAddShake = 0;

                //for (int i = 0; i < activeShakes.Count; i++) {
                //    //Because we remove things from the activeShakes
                //    if (i >= activeShakes.Count)
                //        break;
                    
                if (si.s.CurrentState == ShakeState.Inactive && !si.s.sustain) {
                    toRemove.Add(i);
                    //activeShakes.RemoveAt(i--);
                } else if (si.s.CurrentState != ShakeState.Inactive) {
                    Vector3 pos = si.s.UpdateShake();
                    if (!si.locked) {

                        si.PosAddShake += new Vector2(pos.x * si.s.positionInfluence.x, pos.y * si.s.positionInfluence.y);
                        si.RotAddShake += new Vector3(0, 0, pos.z * si.s.rotationInfluence);

                        si.ScaleAddShake += si.s.ScaleAmt * si.s.scaleInfluence;
                    }
                }

                i++;
                //}

                //MainCamera.I.shakeOffset = posAddShake;
                //transform.localEulerAngles = rotAddShake;
                //MainCamera.I.scaleOffset = scaleAddShake;
            }

            foreach (int j in toRemove)
                if (j > 0 && j < activeShakes.Count)
                    activeShakes.RemoveAt(j);

        }

        public ShakeInstance ShakeOnce(Shake shake) {
            return ShakeOnce(new ShakeInstance(shake));
        }

        public ShakeInstance StartShake(Shake shake) {
            return StartShake(new ShakeInstance(shake));
        }

        /// <summary>Shake the object once, fading in and out.</summary>
        public ShakeInstance ShakeOnce(ShakeInstance si) {
            si.s.loop = false;
            si.s.FadeIn();

            if (!activeShakes.Contains(si))
                activeShakes.Add(si);

            return si;
        }

        /// <summary>Start shaking the object repeatedly.</summary>
        public ShakeInstance StartShake(ShakeInstance si) {
            si.s.loop = true;
            si.s.FadeIn();

            if (!activeShakes.Contains(si))
                activeShakes.Add(si);

            return si;
        }

        public CameraShakeInstance ShakeCameraOnce(Shake s) {
            return ShakeCameraOnce(new CameraShakeInstance(s));
        }

        public CameraShakeInstance StartCameraShake(Shake s) {
            return StartCameraShake(new CameraShakeInstance(s));
        }

        /// <summary>Shake the camera once, fading in and out.</summary>
        public CameraShakeInstance ShakeCameraOnce(CameraShakeInstance csi) {
            csi.s.loop = false;
            csi.s.FadeIn();
            
            if (!activeShakes.Contains(csi))
                activeShakes.Add(csi);

            return csi;
        }

        /// <summary>Start shaking the camera repeatedly.</summary>
        public CameraShakeInstance StartCameraShake(CameraShakeInstance csi) {
            csi.s.loop = true;
            csi.s.FadeIn();

            if (!activeShakes.Contains(csi))
                activeShakes.Add(csi);

            return csi;
        }

        public void StopShake(Shake shake) {
            shake.FadeOut();
        }

        public void StopShake(ShakeInstance si) {
            si.s.FadeOut();
        }

        public class ShakeInstance {
            public Shake s { get; internal set; }
            public Transform t { get; internal set; }

            public virtual Vector2 PosAddShake {
                get { return posAddShake; }
                internal set {
                    if (t)
                        t.position = targetPos.GetValueOrDefault() + new Vector3(value.x, value.y);
                    posAddShake = value;
                }
            }
            public virtual Vector3 RotAddShake { 
                get { return rotAddShake; }
                internal set {
                    if (t)
                        t.eulerAngles = targetRot.GetValueOrDefault() + value;
                    rotAddShake = value;
                }
            }
            public virtual float ScaleAddShake {
                get { return scaleAddShake; }
                internal set {
                    if (t && value != 0)
                        t.localScale = targetScale.GetValueOrDefault() + (Vector3.one * value);
                    scaleAddShake = value;
                }
            }
            public bool locked;

            internal Vector2 posAddShake, rotAddShake;
            internal Vector3? targetPos, targetRot, targetScale;
            internal float scaleAddShake;

            public ShakeInstance(Shake s) : this(s, null, null, null, null) { }

            public ShakeInstance(Shake s, Transform t) : this(s, t, t.position, t.eulerAngles, t.localScale) { }

            public ShakeInstance(Shake s, Transform t, Vector3? targetPos, Vector3? targetRot, Vector3? targetScale) {
                this.s = s;
                this.t = t;

                if (targetPos == null) 
                    targetPos = Vector3.zero;

                if (targetRot == null)
                    targetRot = Vector3.zero;

                if (targetScale == null)
                    targetScale = Vector3.one;

                this.targetPos = targetPos;
                this.targetRot = targetRot;
                this.targetScale = targetScale;
            }
        }

        public class CameraShakeInstance : ShakeInstance {
            public static MainCamera camera;

            public override Vector2 PosAddShake {
                get {
                    return posAddShake;
                }
                internal set {
                    camera.shakeOffset = value;
                    posAddShake = value;
                }
            }

            public override Vector3 RotAddShake {
                get {
                    return rotAddShake;
                }
                internal set {
                    camera.transform.localEulerAngles = value;
                    rotAddShake = value;
                }
            }

            public override float ScaleAddShake {
                get {
                    return scaleAddShake;
                }
                internal set {
                    camera.scaleOffset = value;
                    scaleAddShake = value;
                }
            }

            public CameraShakeInstance(Shake s) : base(s) {
                if (camera == null)
                    camera = Camera.main.GetComponent<MainCamera>();
            }
        }
    }
}