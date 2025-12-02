using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class DungeonStartPanel : MonoBehaviour {
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private TextMeshProUGUI tmpDifficulty = null;
        [SerializeField] private float startYPos = 0f;
        [SerializeField] private float startSlowYPos = 0f;
        [SerializeField] private float endSlowYPos = 0f;
        [SerializeField] private float endYPos = 0f;
        [SerializeField] private float startAreaDuration = 0f;
        [SerializeField] private float slowAreaDuration = 0f;
        [SerializeField] private float endAreaDuration = 0f;
        [SerializeField] private float startDelaySecodns = 0.5f;

        public void OpenPanel(string dungeonId) => PlayAsync(dungeonId).Forget();

        private async UniTaskVoid PlayAsync(string dungeonId) {
            // wait delay seconds and data manage init completed
            float currentWaitSeconds = 0f;
            while (true) {
                currentWaitSeconds += Time.deltaTime;
                if (currentWaitSeconds >= startDelaySecodns && DataManager.Inst.DungeonInfos != null) {
                    break;
                }
                
                var isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                if (isCanceled) {
                    return;
                }
            }
            
            Play(dungeonId);
        }
        
        private void Play(string dungeonId) {
            var dungeonInfo = DataManager.Inst.DungeonInfos;
            if (dungeonInfo == null) {
                gameObject.SetActive(false);
                return;
            }

            // found dungeon info
            DungeonInfo matchedInfo = null;
            for (int i = 0; i < dungeonInfo.Length; i++) {
                var info = dungeonInfo[i];
                if (info.dungeonId != dungeonId) {
                    continue;
                }
                matchedInfo = info;
                break;
            }
            // not found dungeon info
            matchedInfo ??= dungeonInfo[0];
            
            // update and play tween
            UpdatePanel(matchedInfo.dungeonName, matchedInfo.dungeonDesc, matchedInfo.difficultyValue);
            PlayTween();
        }

        private void UpdatePanel(string dungeonName, string desc, int difficulty) {
            tmpName.SetText(dungeonName);
            tmpDesc.SetText(desc);
            tmpDifficulty.SetText("D: " + difficulty.ToString());
        }

        private void PlayTween() {
            rectTr.anchoredPosition = new Vector2(rectTr.anchoredPosition.x, startYPos);
            gameObject.SetActive(true);
            
            // play sequence
            DOTween.Sequence()
                   .Append(rectTr.DOAnchorPosY(startSlowYPos, startAreaDuration))
                   .Append(rectTr.DOAnchorPosY(endSlowYPos, slowAreaDuration))
                   .Append(rectTr.DOAnchorPosY(endYPos, endAreaDuration))
                   .OnComplete(() => {
                       gameObject.SetActive(false);
                   });
        }
    }
}