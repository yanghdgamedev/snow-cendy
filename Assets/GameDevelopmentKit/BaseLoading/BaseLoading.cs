using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevelopmentKit.Scripts
{
    public abstract class BaseLoading : MonoBehaviour {
    
        public float maxTimeLoad = 5;
        public float minTimeLoad = 2f;
        public string sceneLoad;
        private AsyncOperation asyncOperation;

        
        private void Start()
        {
            StartCoroutine(Loading());
        }
        
        protected virtual IEnumerator Loading()
        {
            asyncOperation = SceneManager.LoadSceneAsync(sceneLoad);
            asyncOperation.allowSceneActivation = false;
            CheckOpen();
            yield return OnStartLoad();
            float elaspedTime = 0;
            float progessPercent = 0;
            int count = 0;
            while (elaspedTime < maxTimeLoad)
            {
                count++;
                elaspedTime += Time.deltaTime;
                progessPercent = elaspedTime / maxTimeLoad;
                OnChangePercent(progessPercent);
                if (count % 3 == 0 && elaspedTime >= minTimeLoad && ShouldLoadFaster()) {
                    break;
                }
    
                yield return null;
            }
        
            if (progessPercent < 1f) {
                float fakeDuration = 0.75f;
                float fakeProgress = 0f;
                float fakeElapedTime = 0;
                float elaspedProgress = 1f - progessPercent;
                while (fakeElapedTime < fakeDuration) {
                    fakeElapedTime += Time.deltaTime;
                    fakeProgress = fakeElapedTime / fakeDuration * elaspedProgress;
                    OnChangePercent(fakeProgress + progessPercent);
                    yield return null;
                }
            }

            asyncOperation.allowSceneActivation = true;
            OnLoadDone();
        }

        protected abstract void CheckOpen();

        protected abstract IEnumerator OnStartLoad();
        protected abstract void OnLoadDone();

        protected abstract void OnChangePercent(float percent);
        protected abstract bool ShouldLoadFaster();
    }
}
