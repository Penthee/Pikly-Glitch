using UnityEngine;

namespace Pikl.Utils.Shaker {
    public enum ShakeState { FadingIn, FadingOut, Sustained, Inactive }
    [System.Serializable]
    public class Shake {

        /// <summary>Gets the current state of the shake.</summary>
        public ShakeState CurrentState {
            get {
                if (IsFadingIn)
                    return ShakeState.FadingIn;
                else if (IsFadingOut)
                    return ShakeState.FadingOut;
                else if (IsShaking)
                    return ShakeState.Sustained;
                else
                    return ShakeState.Inactive;
            }
        }

        bool IsShaking {
            get {
                return FadeMagnitude > 0 || sustain;
            }
        }

        bool IsFadingOut {
            get {
                return !sustain && FadeMagnitude > 0;
            }
        }

        bool IsFadingIn {
            get {
                return FadeMagnitude < 1 && sustain && fadeInTime > 0;
            }
        }
        
        public float ScaleAmt {
            get;
            private set;
        }

        public float FadeMagnitude {
            get;
            private set;
        }

        float ZoomScale {
            get {
                return Camera.main.orthographicSize / zoomAtStartOfShake;
            }
        }

        public Vector2 positionInfluence;
        /// <summary>
        /// The name of the shake instance, for reference only.
        /// </summary>
        public string name;
        public float magnitude, roughness, fadeOutTime, fadeInTime, rotationInfluence, scaleInfluence;
        public bool loop, sustain = true;
        
        Vector3 amt;
        float tick, scaleTick, startFollowSpeed, zoomAtStartOfShake;

        public Shake(Shake s) : this (s.magnitude, s.roughness, s.fadeInTime, s.fadeOutTime, 
                                      s.positionInfluence, s.rotationInfluence, s.scaleInfluence) {
            name = s.name;
        }

        public Shake() : this(1, 1, 0, 0) { }

        public Shake(float magnitude) : this(magnitude, 1, 0, 0) { }

        public Shake(float magnitude, float roughness) : this(magnitude, roughness, 0, 0.5f) { }

        public Shake(float magnitude, float roughness, float fadeInTime, float fadeOutTime) : this(magnitude, roughness, fadeInTime, fadeOutTime, Vector2.one * 0.25f, 0.25f, 0.1f) { }

        public Shake(float magnitude, float roughness, float fadeInTime, float fadeOutTime, 
                     Vector2 positionInfluence, float rotationInfluence, float scaleInfluence) {
            this.magnitude = magnitude;
            this.roughness = roughness;

            this.fadeInTime = fadeInTime;
            this.fadeOutTime = fadeOutTime;

            this.positionInfluence = positionInfluence;
            this.rotationInfluence = rotationInfluence;
            this.scaleInfluence = scaleInfluence;

        }

        public Vector3 UpdateShake() {
            amt.x = Mathf.PerlinNoise(tick, 0) - 0.5f;
            amt.y = Mathf.PerlinNoise(0, tick) - 0.5f;
            amt.z = Mathf.PerlinNoise(tick, tick) - 0.5f;
            amt = Vector3.ClampMagnitude(amt, 1);

            ScaleAmt = Mathf.PerlinNoise(scaleTick, scaleTick);
            ScaleAmt = Mathf.Clamp01(ScaleAmt);

            if (sustain) {
                if (sustain && FadeMagnitude < 1)
                    FadeMagnitude += Time.deltaTime / (fadeInTime > 0 ? fadeInTime : 1);
                else if (!loop)
                    sustain = false;
            }

            if (!sustain)
                FadeMagnitude -= Time.deltaTime / (fadeOutTime > 0 ? fadeOutTime : 1);

            FadeMagnitude = Mathf.Clamp01(FadeMagnitude);

            tick += Time.deltaTime * roughness * (sustain ? 1 : FadeMagnitude);
            scaleTick += Time.deltaTime * roughness * (sustain ? 1 : FadeMagnitude);


            amt.x *= magnitude * FadeMagnitude * ZoomScale;
            amt.y *= magnitude * FadeMagnitude * ZoomScale;
            amt.z *= magnitude * FadeMagnitude;

            ScaleAmt *= magnitude * FadeMagnitude;
            
            return amt;
        }

        public void FadeIn() {
            tick = Random.Range(-100f, 100f);
            scaleTick = Random.Range(-100f, 100f);

            FadeMagnitude = fadeInTime > 0 ? 0 : 1;

            //startFollowSpeed = MainCamera.I.followSpeed;
            zoomAtStartOfShake = Camera.main.orthographicSize;
            sustain = true;
        }

        public void FadeOut() {
            if (fadeOutTime == 0)
                FadeMagnitude = 0;

            sustain = false;
        }
    }
}