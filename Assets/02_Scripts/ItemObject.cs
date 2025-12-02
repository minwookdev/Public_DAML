using Cysharp.Threading.Tasks;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Random = UnityEngine.Random;

namespace CoffeeCat {
    public class ItemObject : MonoBehaviour {
        [SerializeField] private Transform tr = null;
        [SerializeField] private Transform modelTr = null;
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        private bool isTriggeredOnPlayer = false;
        private bool isEndedAnimation = false;
        private bool isPlayTrackingOnTriggered = false;
        private ItemLootRequest currentRequest = default;
        private ItemDatas itemDatas = null;
        private Transform playerTr = null;
        
        // consts
        private readonly Vector2 endPositionRandomRange = new(1f, 0.5f);
        private const float DISTANCE_SQR_MAGNITUDE = 0.045f; // 0.15f, 0.15f
        private const float DROP_MIN_HEIGHT = 2.5f;
        private const float DROP_MAX_HEIGHT = 3f;
        private const float MIN_DROP_HEIGHT = 0.2f;
        private const float ANIMATION_DURATION = 0.3f;
        private const float ITEM_MOVE_SPEED = 8f;
        
        private void Awake() {
            if (endPositionRandomRange.x < 0f || endPositionRandomRange.y < 0f) {
                CatLog.ELog("ItemObject's endPositionRandomRange, dropHeight must be greater than 0");
            }

            // get item data container
            itemDatas = DataManager.Inst.Items;
        }

        private void Start() {
            if (playerTr) {
                return;
            }
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            if (player) {
                playerTr = player.Tr;
            }
        }

        private void Update() {
            if (!isTriggeredOnPlayer || !isEndedAnimation) {
                return;
            }

            if (!playerTr || !isPlayTrackingOnTriggered) {
                AddItemWithDespawn();
                return;
            }

            var targetPosition = playerTr.position;
            var direction = Math2DHelper.GetDirection(tr.position, targetPosition);
            if (Vector2.SqrMagnitude(direction) <= DISTANCE_SQR_MAGNITUDE) {
                AddItemWithDespawn();
                return;
            }
            var movePosition = tr.GetPositionV2() + direction.normalized * (Time.deltaTime * ITEM_MOVE_SPEED);
            tr.position = movePosition;
        }
        
        private void OnDisable() {
            modelTr.position = Vector3.zero;
            isEndedAnimation = false;
            isTriggeredOnPlayer = false;
        }
        
        public void Init(in ItemLootRequest request, bool playBezierCurve, float delaySeconds = 0f, bool playTracking = true) {
            currentRequest = request;
            isPlayTrackingOnTriggered = playTracking;
            
            var iconKey = itemDatas.GetIconKey((int)currentRequest.Code);
            if (iconKey != string.Empty) {
                SafeLoader.Load<Sprite>(iconKey, false, (loadedSprite) => {
                    spriteRenderer.sprite = loadedSprite;
                });
            }

            if (!playBezierCurve) {
                isEndedAnimation = true;
                return;
            }
            
            StartBezierAnimationAsync(delaySeconds).Forget();
        }
        
        private void OnTriggerStay2D(Collider2D other) {
            if (isTriggeredOnPlayer || !Utility.IsItemAquisitionLayer(other)) {
                return;
            }
            isTriggeredOnPlayer = true;
        }

        private void AddItemWithDespawn() {
            RogueLiteManager.Inst.AddItem((int)currentRequest.Code, currentRequest.Amount);
            ObjectPoolManager.Inst.Despawn(gameObject);
            switch (currentRequest.Code) {
                case ItemCode.Currency:
                    SoundManager.Inst.PlayCurrencyRandomSE();
                    break;
                case ItemCode.SmallHeal:
                case ItemCode.MediumHeal:
                case ItemCode.LargeHeal:
                    SoundManager.Inst.PlayConsumeItemSE();
                    break;
                case ItemCode.ExpForced:
                case ItemCode.ExpFull:
                case ItemCode.ExpSmall:
                case ItemCode.ExpMedium:
                case ItemCode.ExpLarge:
                    SoundManager.Inst.PlayExpRandomSE();
                    break;
                default: 
                    SoundManager.Inst.PlayDefaultItemSE();
                    break;
            }
        }
        
        private async UniTaskVoid StartBezierAnimationAsync(float delayMilliseconds) {
            if (delayMilliseconds > 0f) {
                var isCancelledDelay = await UniTask.WaitForSeconds(delayMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                if (isCancelledDelay) {
                    return;
                }   
            }
            
            // start bezier curve animation
            Vector2 startPoint = tr.position;
            
            // calculate end position
            float randomX = Random.Range(startPoint.x - endPositionRandomRange.x, startPoint.x + endPositionRandomRange.x);
            float randomY = Random.Range(startPoint.y - endPositionRandomRange.y, startPoint.y + endPositionRandomRange.y);
            Vector2 endPoint = new Vector2(randomX, randomY);
            
            // Calculate the height the object will rise based on the EndPos height
            float controlPointX = startPoint.x + (endPoint.x - startPoint.x) * 0.5f;
            float controlPointY = 0f;
            float randomDropHeight = Random.Range(DROP_MIN_HEIGHT, DROP_MAX_HEIGHT);
            if (endPoint.y < startPoint.y) {
                var distance = startPoint.y - endPoint.y;
                var height = randomDropHeight - distance;
                if (height < MIN_DROP_HEIGHT) {
                    height = MIN_DROP_HEIGHT;
                }

                controlPointY = startPoint.y + height;
            }
            else {
                var distance = endPoint.y - startPoint.y;
                var height = randomDropHeight + distance;
                controlPointY = startPoint.y + height;
            }
            Vector2 controlPoint = new Vector2(controlPointX, controlPointY);

            float t = 0f;
            while (t < 1f) {
                t += Time.deltaTime / ANIMATION_DURATION;
                var bezierPos = Math2DHelper.CalculateQuadBezierPoint2D(startPoint, controlPoint, endPoint, t);
                tr.position = bezierPos;
                
                var isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                if (isCancelled) {
                    return;
                }
            }
            tr.position = endPoint;
            isEndedAnimation = true;
            SoundManager.Inst.PlaySE(SoundKey.Dropped_Item.ToKey());
        }
    }   
}