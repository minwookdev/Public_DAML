using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class MobileJoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        [Title("JoyStick")]
        [SerializeField] private Canvas canvas = null;
        [SerializeField] private CanvasGroup joyStickCanvasGroup = null;
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private RectTransform stickRectTr = null;
        [SerializeField] private float maxStickMoveSqrMagnitude = 5000f;
        [SerializeField, ReadOnly] private float currentStickSqrMagnitude = 0f;
        [SerializeField, ReadOnly] private float magnitude = 0f;
        [SerializeField, ReadOnly] private bool isTouched = false;
        [field: SerializeField] public JoyStickType JoyStickType { get; private set; } = JoyStickType.Dynamic;
        private bool isScreenVertical = false;
        private bool isStickDragged = false;
        private bool isPublishInputEvent = true;
        private Vector2 stickDirection = Vector2.zero;
        private Vector2 initStickAnchoredPos = Vector2.zero;
        private Vector2 inputBeganAnchoredPos = Vector2.zero;
        private Vector2 reservedJoyStickPosition = Vector2.zero;
        private float currentTouchedUpdateTime = 0f;
        
        [Title("JoyStick Dynamic Area")]
        [SerializeField] private RectTransform areaRectTr = null;
        [SerializeField] private EventTrigger eventTriggerStickArea = null;
        [SerializeField] private RawImage areaRawImage = null;

        [Title("JoyStick Positions")]
        [SerializeField] private RectTransform vStaticPosRectTr = null;
        [SerializeField] private RectTransform hStaticPosRectTr = null;
        
        [Title("Frame Images")]
        [SerializeField] private Image[] frameImages = null;

        private void OnEnable() {
            OnResolutionChanged(new Vector2Int(Screen.width, Screen.height));
            RogueLiteManager.AddEventToOnResolutionChanged(OnResolutionChanged);
        }

        private void Start() {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            InitJoyStick();
#elif UNITY_STANDALONE
            gameObject.SetActive(false);
#endif
        }

        private void Update() {
            if (!isTouched) {
                return;
            }
            currentTouchedUpdateTime += Time.deltaTime;
            if (isPublishInputEvent) {
                InputManager.InvokeEventJoyStickInputUpdate(magnitude, stickDirection);
            }
        }

        private void FixedUpdate() {
            if (!isTouched) {
                return;
            }
            if (isPublishInputEvent) {
                InputManager.InvokeEventJoyStickInputFixedUpdate(magnitude, stickDirection);
            }
        }

        private void OnDisable() => RogueLiteManager.RemoveEventToOnResolutionChanged(OnResolutionChanged);

        #region EventTrigger

        private void OnPointerDownEventTrigger(PointerEventData pointerEventData) => OnJoyStickPointerDown(pointerEventData, JoyStickType.Static);

        private void OnPointerDragEventTrigger(PointerEventData pointerEventData) => OnJoyStickPointerDrag(pointerEventData, JoyStickType.Static);

        private void OnPointerUpEventTrigger() => OnJoyStickPointerUp(JoyStickType.Static);

        #endregion
        
        #region IPointerEvents
        
        public void OnPointerDown(PointerEventData eventData) => OnJoyStickPointerDown(eventData, JoyStickType.Dynamic);

        public void OnDrag(PointerEventData eventData) => OnJoyStickPointerDrag(eventData, JoyStickType.Dynamic);

        public void OnPointerUp(PointerEventData eventData) => OnJoyStickPointerUp(JoyStickType.Dynamic);

        #endregion

        private void OnJoyStickPointerDown(PointerEventData eventData, JoyStickType allowedType) {
            if (isTouched || JoyStickType == allowedType) {
                return;
            }

            isTouched = true;
            switch (JoyStickType) {
                case JoyStickType.Static:
                    var staticTouchPos = GetLocalTouchPosStatic(eventData.position);
                    UpdateJoyStick(staticTouchPos);
                    break;
                case JoyStickType.Dynamic:
                    var dynamicTouchPos = GetLocalTouchPosDynamic(eventData.position);
                    rectTr.anchoredPosition = dynamicTouchPos;
                    UpdateInputBeganPosition();
                    UpdateJoyStick(dynamicTouchPos);
                    joyStickCanvasGroup.alpha = 1f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isPublishInputEvent) {
                InputManager.InvokeEventJoyStickInputBegan(magnitude, stickDirection);
            }
        }

        private void OnJoyStickPointerDrag(PointerEventData eventData, JoyStickType allowedType) {
            if (!isTouched || JoyStickType == allowedType) {
                return;
            }

            if (!isStickDragged) {
                isStickDragged = true;
                if (isPublishInputEvent) {
                    InputManager.InvokeEventJoyStickDragged();   
                }
            }

            Vector2 touchPosition = JoyStickType switch {
                JoyStickType.Static  => GetLocalTouchPosStatic(eventData.position),
                JoyStickType.Dynamic => GetLocalTouchPosDynamic(eventData.position),
                _                    => throw new ArgumentOutOfRangeException()
            };

            UpdateJoyStick(touchPosition);
        }

        private void OnJoyStickPointerUp(JoyStickType allowedType) {
            if (!isTouched || JoyStickType == allowedType) {
                return;
            }
            
            InvokeIfValidEvasionEvent();
            Clear();

            if (JoyStickType == JoyStickType.Static) {
                joyStickCanvasGroup.alpha = 0f;
            }
        }
        
        private void InitJoyStick() {
            // set alpha
            joyStickCanvasGroup.alpha = JoyStickType == JoyStickType.Static ? 1f : 0f;
            
            // add event to resolution changed
            UpdateRaycastTargetByJoyStickType();
            
            // set default positions
            InitStickAnchoredPosition();
            UpdateInputBeganPosition();
            AddEvevntsToTrigger();
        }

        public void SetJoyStickType(JoyStickType type) {
            if (JoyStickType == type) {
                return;
            }
            
            JoyStickType = type;
            joyStickCanvasGroup.alpha = JoyStickType == JoyStickType.Static ? 1f : 0f;
            
            UpdateJoyStickPosition();
            UpdateInputBeganPosition();
            UpdateRaycastTargetByJoyStickType();
        }
        
        private void OnResolutionChanged(Vector2Int changedResolution) {
            UpdateJoyStickPositionAsync(changedResolution).Forget();
        }

        private async UniTaskVoid UpdateJoyStickPositionAsync(Vector2Int changedResolution) {
            await UniTask.NextFrame(PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            var currentScreenAspectRatio = changedResolution.y / changedResolution.x;
            
            // set screen mode and reserved joystick position
            isScreenVertical = currentScreenAspectRatio > 1.0f;
            reservedJoyStickPosition = isScreenVertical ? vStaticPosRectTr.position : hStaticPosRectTr.position;
            UpdateJoyStickPosition();
        }

        private void UpdateRaycastTargetByJoyStickType() {
            switch (JoyStickType) {
                case JoyStickType.Static:
                    areaRawImage.raycastTarget = false;
                    eventTriggerStickArea.enabled = false;
                    joyStickCanvasGroup.blocksRaycasts = true;
                    break;
                case JoyStickType.Dynamic:
                    areaRawImage.raycastTarget = true;
                    eventTriggerStickArea.enabled = true;
                    joyStickCanvasGroup.blocksRaycasts = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void InitStickAnchoredPosition() {
            initStickAnchoredPos = stickRectTr.anchoredPosition;
        }
        
        private void UpdateJoyStickPosition() {
            if (JoyStickType == JoyStickType.Dynamic) {
                return;
            }
            rectTr.position = reservedJoyStickPosition;
        }

        private void UpdateInputBeganPosition() {
            if (JoyStickType == JoyStickType.Dynamic) {
                inputBeganAnchoredPos = rectTr.anchoredPosition;
            }
            else if (JoyStickType == JoyStickType.Static) {
                inputBeganAnchoredPos = Vector2.zero;
            }
        }

        private void AddEvevntsToTrigger() {
            // Add Event Trigger
            var pointerDownEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerDown};
            pointerDownEntry.callback.AddListener((data) => OnPointerDownEventTrigger((PointerEventData)data));
            
            var pointerDragEntry = new EventTrigger.Entry {eventID = EventTriggerType.Drag};
            pointerDragEntry.callback.AddListener((data) => OnPointerDragEventTrigger((PointerEventData)data));
            
            var pointerUpEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerUp};
            pointerUpEntry.callback.AddListener((_) => OnPointerUpEventTrigger());
            
            eventTriggerStickArea.triggers.Add(pointerDownEntry);
            eventTriggerStickArea.triggers.Add(pointerDragEntry);
            eventTriggerStickArea.triggers.Add(pointerUpEntry);
        }

        private void UpdateJoyStick(Vector2 localPosition) {
            UpdateStickPosition(localPosition);
            UpdateDirectionFromStickPosition();
            UpdateFrameDirection();
            UpdateMagnitudeValue();
        }

        private Vector2 GetLocalTouchPosStatic(Vector2 screenTouchPos) {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTr, screenTouchPos, canvas.worldCamera, out Vector2 localTouchPos)) {
                localTouchPos = Vector2.zero;
            }
            return localTouchPos;
        }

        private Vector2 GetLocalTouchPosDynamic(Vector2 screenTouchPos) {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(areaRectTr, screenTouchPos, canvas.worldCamera, out Vector2 localTouchPos)) {
                localTouchPos = Vector2.zero;
            }
            return localTouchPos;
        }
        
        private void UpdateStickPosition(Vector2 localTouchPos) {
            // if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTr, screenTouchPos, canvas.worldCamera, out Vector2 localTouchPos)) {
            //     localTouchPos = Vector2.zero;
            // }
            var distance = localTouchPos - inputBeganAnchoredPos;
            var clampedStickPos =  ClampJoystickPosition(distance);
            stickRectTr.anchoredPosition = clampedStickPos;
        }
        
        private Vector2 ClampJoystickPosition(Vector2 distance) {
            currentStickSqrMagnitude = distance.sqrMagnitude;
            if (currentStickSqrMagnitude > maxStickMoveSqrMagnitude) {
                currentStickSqrMagnitude = maxStickMoveSqrMagnitude;
                var direction = distance.normalized;
                distance = direction * Mathf.Sqrt(currentStickSqrMagnitude);
            }
            return distance;
        }

        private void UpdateDirectionFromStickPosition() {
            stickDirection = stickRectTr.anchoredPosition - initStickAnchoredPos;
            stickDirection.Normalize();
        }

        private void UpdateFrameDirection() {
            int index = GetQuadrantIndex(stickDirection);
            for (int i = 0; i < frameImages.Length; i++) {
                frameImages[i].gameObject.SetActive(i == index);
            }
        }

        private int GetQuadrantIndex(Vector2 direction) {
            if (direction.x < 0) {
                return direction.y > 0 ? 0 : 2;
            }

            return direction.y > 0 ? 1 : 3;
        }

        private void UpdateMagnitudeValue() {
            var clampMagnitude = currentStickSqrMagnitude / maxStickMoveSqrMagnitude;
            magnitude = Mathf.Clamp(clampMagnitude, 0f, 1f);
        }

        private void InvokeIfValidEvasionEvent() {
            if (isPublishInputEvent && isStickDragged && currentTouchedUpdateTime <= InputManager.EVASION_KEY_THRESHOLD) {
                InputManager.InvokeEventEvasionInput(stickDirection);
            }
        }

        private void Clear() {
            isTouched = false;
            isStickDragged = false;
            stickDirection = Vector2.zero;
            stickRectTr.anchoredPosition = Vector2.zero;
            currentStickSqrMagnitude = 0f;
            currentTouchedUpdateTime = 0f;
            magnitude = 0f;
            for (int i = 0; i < frameImages.Length; i++) {
                frameImages[i].gameObject.SetActive(false);
            }

            joyStickCanvasGroup.alpha = JoyStickType switch {
                JoyStickType.Static  => 1f,
                JoyStickType.Dynamic => 0f,
                _                    => throw new NotImplementedException()
            };

            if (isPublishInputEvent) {
                InputManager.InvokeEventJoyStickInputEnded();
            }
        }

        public void DisablePublishInputEvent() {
            // Check to prevent duplicate event issuance
            if (!isPublishInputEvent) {
                return;
            }
            isPublishInputEvent = false;
            if (isTouched) {
                InputManager.InvokeEventJoyStickInputEnded();
            }
        }
        
        public void RestorePublishInputEvent() {
            // Check to prevent duplicate event issuance
            if (isPublishInputEvent) {
                return;
            }
            isPublishInputEvent = true;
            if (isTouched) {
                InputManager.InvokeEventJoyStickInputBegan(magnitude, stickDirection);
            }
            if (isStickDragged) {
                InputManager.InvokeEventJoyStickDragged();
            }
        }

        public void DisableJoyStick() {
            Clear();
            areaRectTr.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        
        public void EnableJoyStick() {
            gameObject.SetActive(true);
            areaRectTr.gameObject.SetActive(true);
        }
    } 
}