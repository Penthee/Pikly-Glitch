using UnityEngine;
//using Pikl.Managers;
//using Pikl.Managers.Types;
//using Pikl.UI;
//using Pikl.Profile;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace Pikl {
    public sealed class Initialise : MonoBehaviour {
        public GameObject splashObject;
        public Text loadingText;
        public RectTransform loadingBar;
        public PostProcessVolume post;

        public float splashTimeDelay = 0f;
        public float splashLength = 5f;
        public float loadingAnimationSpeed = 1.2f;
        public float bloomIntensity = 12f;
        public float vignetteIntensity = 0.75f;
        public Ease loadingEase;

        float startTime;

        void Start() {
            Random.InitState((int)System.DateTime.Today.Ticks);

            Cursor.SetCursor(Resources.Load<Texture2D>("Sprites/UI/Cursors/pointer"), Vector2.zero, CursorMode.Auto);

            //if (FileMgr.I.config.LastPlayer == "")
            //	TableMgr.I.StackStat("ClientInstalls");

            //TableMgr.I.StackStat("ClientStarts");
            //TableMgr.I.PostStats();

            Invoke("TriggerSplash", splashTimeDelay);
        }

        void Update() {
            if (startTime + splashLength < Time.time || Input.GetKeyDown(KeyCode.Escape))
                LoadScene();

        }

        void TriggerSplash() {
            //splashObject.GetComponent<Animator>().SetTrigger("Play");
            //splashObject.GetComponent<AudioSource>().Play();
            startTime = Time.time;
            DOTween.To(() => loadingBar.sizeDelta, x => loadingBar.sizeDelta = x, new Vector2(Screen.width - 10, loadingBar.sizeDelta.y), splashLength).SetEase(loadingEase);


            Bloom bloom = null;
            Vignette vignette = null;
            post.profile.TryGetSettings(out bloom);
            post.profile.TryGetSettings(out vignette);

            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, bloomIntensity, splashLength).SetEase(loadingEase);
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, vignetteIntensity, splashLength).SetEase(loadingEase);

            AnimateText();
        }

        void AnimateText() {
            DOTween.To(() => loadingText.text, x => loadingText.text = x, "Loading...", loadingAnimationSpeed)
                   .SetEase(Ease.Linear);

            Invoke("ShittyLoop", loadingAnimationSpeed + (loadingAnimationSpeed / 4));
        }

        void ShittyLoop() {
            loadingText.text = "Loading";
            AnimateText();
        }

        void LoadScene() {
            SceneMgr.I.LoadScene("Menu");
        }
    }
}