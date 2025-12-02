using UnityEngine;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using CoffeeCat.UI;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class DamageTextManager : DynamicSingleton<DamageTextManager> {
        [Title("Information")]
        [SerializeField, ReadOnly] Camera mainCamera = null;
        [SerializeField, ReadOnly] Camera uiCamera = null;
        [SerializeField, ReadOnly] Canvas targetCanvas = null;
        [SerializeField, ReadOnly] RectTransform textsParentRectTr = null;
        [ShowInInspector, ReadOnly] public bool IsSetupCompleted { get; private set; } = false;

        [Title("Texts")]
        [SerializeField, ReadOnly] GameObject damageTextOriginGameObject = null;
        [SerializeField, ReadOnly] private int poolInitializingCount = 30;
        [SerializeField, ReadOnly] string spawnKey = string.Empty;
        private const string textFormat = "#,##0";
        private readonly Color monsterDamagedColor = new(1f, 0.7f, 0f, 1f);
        private readonly Color playerDamagedColor = new(1f, 0f, 0f, 1f);
        
        protected override void Initialize() {
            base.Initialize();
            SceneManager.Inst.ChangeBeforeEvent += Clear;
        }

        /// <summary>
        /// 씬 진입 후 반드시 한번 호출해야 DamageText기능 사용 가능
        /// </summary>
        /// <param name="textRenderCanvas"></param>
        /// <param name="damageTextParent"></param>
        public void Setup(Canvas textRenderCanvas, Camera uiCamera) {
            // Make Damage Texts Parent in Rendering Canvas
            this.mainCamera = Camera.main;
            this.uiCamera = uiCamera;
            this.targetCanvas = textRenderCanvas;
            var parentRectTr = new GameObject("DamageTextsParent").AddComponent<RectTransform>();
            parentRectTr.SetParent(this.targetCanvas.transform);
            parentRectTr.SetAnchorAndPivot(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            parentRectTr.ResetPosition();
            parentRectTr.SetAsFirstSibling();
            textsParentRectTr = parentRectTr;

            // Check DamageText Origin GameObject
            if (damageTextOriginGameObject == null) {
                // Load DamageText GameObject
                ResourceManager.Inst.AddressablesAsyncLoad<GameObject>("DamageText", false, (loadedObject) => {
                    if (!loadedObject) {
                        CatLog.ELog("DamageTextManager Init Failed. DamageText Load Failed.");
                        return;
                    }
                    damageTextOriginGameObject = loadedObject;
                    spawnKey = loadedObject.name;
                    InitializeObjectPool();
                });
            }
            else {
                InitializeObjectPool();
            }

            // ObjectPool Initializing
            void InitializeObjectPool() {
                PoolInfo poolInfo = PoolInfo.Create(damageTextOriginGameObject, poolInitializingCount);
                ObjectPoolManager.Inst.AddToPool(poolInfo);
                IsSetupCompleted = true;
            }
        }

        /// <summary>
        /// 씬 정리될 때 호출 (현재 씬 정보 해제)
        /// </summary>
        /// <param name="sceneName"></param>
        private void Clear(SceneName sceneName) {
            this.targetCanvas = null;
            this.textsParentRectTr = null;
            IsSetupCompleted = false;
        }

        #region FUNCTIONS

        public void OnFloatingText(float damageCount, Vector2 startPosition, bool isDamagedPlayer) {
            OnFloatingText(damageCount.ToString(textFormat), startPosition, isDamagedPlayer);
        }

        public void OnFloatingText(string damageCountStr, Vector2 startPosition, bool isDamagedPlayer) {
            var targetColor = isDamagedPlayer ? playerDamagedColor : monsterDamagedColor;
            var spawnedDamageText = ObjectPoolManager.Inst.Spawn<DamageText>(spawnKey, Vector2.zero, Quaternion.identity);
            spawnedDamageText.OnFloating(startPosition, damageCountStr, targetColor);
            /*Vector2 playPosition = UIHelper.WorldPositionToCanvasAnchoredPosition(mainCamera, startPosition, targetCanvas.GetComponent<RectTransform>());*/
        }

        public void OnReflectingText(float damageCount, Vector2 startPosition, Vector2 direction, bool isDamagedPlayer) {
            OnReflectingText(damageCount.ToString(textFormat), startPosition, direction, isDamagedPlayer);
        }

        public void OnReflectingText(string damageCountStr, Vector2 startPosition, Vector2 direction, bool isDamagedPlayer) {
            var targetColor = isDamagedPlayer ? playerDamagedColor : monsterDamagedColor;
            var spawnedDamageText = ObjectPoolManager.Inst.Spawn<DamageText>(spawnKey, Vector2.zero, Quaternion.identity);
            //Vector2 playPosition = UIHelper.WorldPositionToCanvasAnchoredPosition(mainCamera, startPosition, targetCanvas.GetComponent<RectTransform>());
            spawnedDamageText.OnReflecting(startPosition, direction, damageCountStr, targetColor);
        }

        public void OnTransmittanceText(float damageCount, Vector2 startPosition, Vector2 direction, bool isDamagedPlayer) {
            this.OnTransmittanceText(damageCount.ToString(textFormat), startPosition, direction, isDamagedPlayer);
        }

        public void OnTransmittanceText(string damageCountStr, Vector2 startPosition, Vector2 direction, bool isDamagedPlayer) {
            var targetColor = isDamagedPlayer ? playerDamagedColor : monsterDamagedColor;
            var spawnText = ObjectPoolManager.Inst.Spawn<DamageText>(spawnKey, Vector2.zero, Quaternion.identity);
            spawnText.OnTransmittance(startPosition, direction, damageCountStr, targetColor);
        }

        #endregion
    }
}
