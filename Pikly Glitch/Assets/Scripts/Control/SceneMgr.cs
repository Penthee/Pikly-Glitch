using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Pikl.Player;

namespace Pikl {
    public class SceneMgr : Singleton<SceneMgr> {

        protected SceneMgr() { }

        /// <summary>
        /// The ID of the currently loaded scene.
        /// </summary>
        public int LoadedScene {
            get { return SceneManager.GetActiveScene().buildIndex; }
        }
        /// <summary>
        /// The name of the currently loaded scene.
        /// </summary>
        public string LoadedSceneName {
            get { return SceneManager.GetActiveScene().name; }
        }

        /// <summary>
        /// Indicates whether the Scene Manager is currently loading a scene.
        /// </summary>
        public bool IsLoadingScene {
            get;
            private set;
        }
        
        public void LoadScene(string name) {
            if (IsLoadingScene)
                return;

            IsLoadingScene = true;

            MessageMgr.I.Broadcast("ExitScene", SceneManager.GetActiveScene().name);

            StartCoroutine(LoadSceneAsync(name));
        }
        
        public AsyncOperation sceneAsync = null;
        IEnumerator LoadSceneAsync(string name) {
            //yield return new WaitForSeconds(1f);

            sceneAsync = SceneManager.LoadSceneAsync(name);

            while(!sceneAsync.isDone)
                yield return sceneAsync;

            MessageMgr.I.Broadcast("EnterScene", name);
            IsLoadingScene = false;
        }
    }
}