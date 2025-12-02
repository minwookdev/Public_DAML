using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class LoadingSceneBase : SceneSingleton<LoadingSceneBase> {
        [Header("FIELDS")]
        [SerializeField] private Slider sliderLoading = null;
        [SerializeField] private TextMeshProUGUI tmpLoadingPercent = null;
        private const float NEXT_SCENE_ACTIVE_WAIT_SECONDS = 1f;
        private float currentWaitSeconds = 0f;

        private void Start() {
            SceneManager.Inst.StartLoadNextScene();
        }

        private void Update() {
            var progress = SceneManager.Inst.SceneLoadAsyncProgress;
            sliderLoading.value = progress;
            var progressValue = (int)(progress * 100f);
            tmpLoadingPercent.text = $"{progressValue} %";

            var isCompleted = SceneManager.Inst.IsNextSceneLoadCompleted;
            if (!isCompleted) {
                return;
            }

            currentWaitSeconds += Time.deltaTime;
            if (currentWaitSeconds >= NEXT_SCENE_ACTIVE_WAIT_SECONDS) {
                SceneManager.Inst.ActiveNextScene();
            }
        }
    }
}
