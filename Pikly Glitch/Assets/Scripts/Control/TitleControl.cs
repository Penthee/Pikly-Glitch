using UnityEngine;
//using Pikl.Managers;
//using Pikl.Managers.Types;
//using Pikl.UI;
//using Pikl.Profile;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Pikl.Utils.Shaker;

namespace Pikl {
    public sealed class TitleControl : MonoBehaviour {
        
        public PostProcessVolume post;
        public SpriteRenderer black;

        [Range(0, 10)]
        public int bloomIntensityStart = 0, bloomIntensityEnd = 5;
        [Range(0, 50)]
        public int distortStart = 0, distortEnd = 40;
        [Range(2, 10)]
        public float bloomStartTime = 2, distortStartTime = 2, bloomLength = 10, distortLength = 10;

        [Range(0.05f, 0.5f)]
        public float fadeBlackSpeed;

        public Ease bloomEase, distortEase, blackEase;
        public Shake shake = ShakePresets.HandheldCamera;

        Bloom bloom = null;
        LensDistortion distort = null;
        float startTime;

        void Start() {
            post.profile.TryGetSettings(out bloom);
            post.profile.TryGetSettings(out distort);

            Invoke("StartBloom", bloomStartTime);
            Invoke("StartDistort", distortStartTime);
            Invoke("FadeBlack", distortStartTime + distortLength);

            Shaker.I.StartCameraShake(ShakePresets.HandheldCamera);
            Shaker.I.StartCameraShake(shake);

        }
        
        void StartBloom() {
            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, bloomIntensityEnd, bloomLength).SetEase(bloomEase);
        }

        void StartDistort() {
            DOTween.To(() => distort.intensity.value, x => distort.intensity.value = x, distortEnd, distortLength).SetEase(distortEase);
        }

        void FadeBlack() {
            DOTween.To(() => black.color, x => black.color = x, Color.black, fadeBlackSpeed).SetEase(blackEase);
        }

        //void TriggerSplash() {
        //    //splashObject.GetComponent<Animator>().SetTrigger("Play");
        //    //splashObject.GetComponent<AudioSource>().Play();
        //    startTime = Time.time;
        //    DOTween.To(() => loadingBar.sizeDelta, x => loadingBar.sizeDelta = x, new Vector2(Screen.width - 10, loadingBar.sizeDelta.y), splashLength).SetEase(loadingEase);


        //    Bloom bloom = null;
        //    Vignette vignette = null;
        //    post.profile.TryGetSettings(out bloom);
        //    post.profile.TryGetSettings(out vignette);

        //    DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, bloomIntensity, splashLength).SetEase(loadingEase);
        //    DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, vignetteIntensity, splashLength).SetEase(loadingEase);

        //    AnimateText();
        //}

        //void AnimateText() {
        //    DOTween.To(() => loadingText.text, x => loadingText.text = x, "Loading...", loadingAnimationSpeed)
        //           .SetEase(Ease.Linear);

        //    Invoke("ShittyLoop", loadingAnimationSpeed + (loadingAnimationSpeed / 4));
        //}
    }
}