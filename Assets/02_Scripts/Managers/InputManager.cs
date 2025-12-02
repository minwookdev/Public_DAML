using UnityEngine;
using UnityEngine.Events;
using CoffeeCat.Editor;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    public class InputManager : DynamicSingleton<InputManager> {
        // Fields
        private InputCanvas inputCanvas = null;
        private MobileJoyStick joyStick = null;
        private bool isInputEnabled = true;
        public const float EVASION_KEY_THRESHOLD = 0.35f;

        // mobile joysticj input events
        private readonly UnityEvent<float, Vector2> onJoyStickInputBegan = new();       // Direction Input Start
        private readonly UnityEvent<float, Vector2> onJoyStickInputUpdate = new();      // Direction Input Update
        private readonly UnityEvent<float, Vector2> onJoyStickInputFixedUpdate = new(); // Direction Input Fixed Update
        private readonly UnityEvent onJoyStickInputDragged = new();                     // JoyStick Dragged
        private readonly UnityEvent onJoyStickInputEnded = new();                       // Direction Input End
        private readonly UnityEvent<Vector2> onEvasionInputEvent = new();
        
#if UNITY_EDITOR || UNITY_STANDALONE
        // standalone input events
        private readonly UnityEvent<float, Vector2> onStandaloneDirectionInputBegan = new();
        private readonly UnityEvent<float, Vector2> onStandaloneDirectionInputUpdate = new();
        private readonly UnityEvent<float, Vector2> onStandaloneDirectionInputFixedUpdate = new();
        private readonly UnityEvent onStandaloneDirectionInputEnded = new();
        private readonly UnityEvent onStandaloneInteractionInput = new();

        // standalone direction input variables
        private bool isStandaloneDirectionInputBegan = false;
        private bool isPublishStandaloneInputEvent = true;
        private const string AXIS_ROW_KEY = "Horizontal";
        private const string AXIS_COL_KEY = "Vertical";
        private Vector2 standaloneNormalizedInputDirection = Vector2.zero;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        private void Update() {
            if (!isInputEnabled || !isPublishStandaloneInputEvent) {
                return;
            }
            UpdateStandAloneDirectionInputs();
            UpdateStandaloneOtherInputEvents();
        }

        private void FixedUpdate() {
            if (!isInputEnabled || !isPublishStandaloneInputEvent) {
                return;
            }
            FixedUpdateStandAloneDirectionInput();
        }

        private void UpdateStandAloneDirectionInputs() {
            var hor = Input.GetAxisRaw(AXIS_ROW_KEY);
            var ver = Input.GetAxisRaw(AXIS_COL_KEY);
            if (hor == 0 && ver == 0) {
                if (!isStandaloneDirectionInputBegan) {
                    return;
                }
                isStandaloneDirectionInputBegan = false;
                onStandaloneDirectionInputEnded.Invoke();
                return;
            }
            
            standaloneNormalizedInputDirection = new Vector2(hor, ver);
            standaloneNormalizedInputDirection.Normalize();
            if (!isStandaloneDirectionInputBegan) {
                isStandaloneDirectionInputBegan = true;
                onStandaloneDirectionInputBegan.Invoke(1f, standaloneNormalizedInputDirection);
                
                // If you want to distinguish between the frame where input starts and the frame where it is updated, uncomment it.
                // return;
            }
            onStandaloneDirectionInputUpdate.Invoke(1f, standaloneNormalizedInputDirection);
        }

        private void FixedUpdateStandAloneDirectionInput() {
            if (!isStandaloneDirectionInputBegan) {
                return;
            }
            onStandaloneDirectionInputFixedUpdate.Invoke(1f, standaloneNormalizedInputDirection);
        }

        private void UpdateStandaloneOtherInputEvents() {
            if (Input.GetKeyDown(KeyCode.E)) {
                onStandaloneInteractionInput.Invoke();
            }
            
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isStandaloneDirectionInputBegan) {
                    InvokeEventEvasionInput(standaloneNormalizedInputDirection);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.M)) {
                if (DungeonUIPresenter.IsExist) {
                    DungeonUIPresenter.Inst.OpenMinimapInStandalone();
                }
            }

            if (Input.GetKeyDown(KeyCode.K)) {
                if (DungeonUIPresenter.IsExist) {
                    DungeonUIPresenter.Inst.OpenPlayerSkillInStandalone();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Semicolon)) {
                DebugManager.Inst.OnForcedNextFloor();
            }

            if (Input.GetKeyDown(KeyCode.N)) {
                DebugManager.Inst.OnForcedEnableSkillSelectPanel();
            }

            if (Input.GetKeyDown(KeyCode.O)) {
                DebugManager.Inst.OnForcedDisableSkillSelectPanel();
            }

            if (Input.GetKeyDown(KeyCode.LeftBracket)) {
                DebugManager.Inst.EnablePlayerBattleMode();
                CatLog.Log("Enable Player Battle Mode Forced.");
            }

            if (Input.GetKeyDown(KeyCode.RightBracket)) {
                DebugManager.Inst.DisablePlayerBattleMode();
                CatLog.Log("Disable Player Battle Mode Forced.");
            }

            if (Input.GetKeyDown(KeyCode.Keypad0)) {
                var player = RogueLiteManager.Inst.SpawnedPlayer;
                player.SendMessage("OnDead", SendMessageOptions.DontRequireReceiver);
            }
        }
#endif

        private void Start() {
            LoadInputCanvas();
            
            // Add Scene Change Events
            SceneManager.Inst.ChangeBeforeEvent += OnSceneChangeBrfore;
            SceneManager.Inst.ChangeAfterEvent += OnSceneChangeAfter;
            Input.multiTouchEnabled = false;
            
            // AddEventEvasionInput((direction) => { CatLog.Log("Invoke Evasion Event !"); });
            // AddEventDirectionInputBegan((_, _) => { CatLog.Log("Began Input !"); });
            // AddEventDirectionInputUpdate((_, _) => { CatLog.Log("Update Input !"); });
            // AddEventDirectionInputFixedUpdate((_, _) => { CatLog.Log("FixedUpdate Input !"); });
            // AddEventDirectionInputEnded(() => { CatLog.Log("Ended Input !"); });
        }
        
        private void LoadInputCanvas() {
            ResourceManager.Inst.AddressablesAsyncLoad<GameObject>(AddressablesKey.InputCanvas.ToKey(), true, (loadedGameObject) => {
                if (!loadedGameObject) {
                    CatLog.ELog("Input Canvas Load Failed");
                    return;
                }
                
                var instanced = Instantiate(loadedGameObject, Vector3.zero, Quaternion.identity);
                if (!instanced || !instanced.TryGetComponent(out inputCanvas)) {
                    CatLog.ELog("InputCanvas Setup Failed.");
                    return;
                }
                // set dontdestory flag
                DontDestroyOnLoad(inputCanvas.gameObject);
                
                SetCameraOnInputCanvas();
                joyStick = inputCanvas.JoyStick;

                if (isInputEnabled) {
                    EnableJoyStick();
                }
                else {
                    DisableJoyStick();
                }
            });
        }
        
        private void SetCameraOnInputCanvas() {
            var uicam = GameObject.FindGameObjectWithTag(Defines.TAG_UICAM);
            if (!uicam || !uicam.TryGetComponent(out Camera uicamera)) {
                CatLog.ELog("UI Camera Not Found");
                return;
            }
                
            inputCanvas.SetCamera(uicamera, Camera.main);
        }

        private void OnSceneChangeBrfore(SceneName sceneName) {
            inputCanvas.ReleaseUICamera();
        }

        private void OnSceneChangeAfter(SceneName sceneName) {
            SetCameraOnInputCanvas();
        }
        
        public JoyStickType GetCurrentJoyStickType() {
            return joyStick.JoyStickType;
        }

        public void SetJoyStickType(JoyStickType type) {
            joyStick.SetJoyStickType(type);
        }

        public static void EnableInputSafe() {
            if (!IsExist) {
                return;
            }
            inst.EnableInput();
        }

        public static void DisableInputSafe() {
            if (!IsExist) {
                return;
            }
            inst.DisableInput();
        }

        private void EnableInput() {
            isInputEnabled = true;
            if (!joyStick) {
                return;
            }
            EnableJoyStick();
        }

        private void DisableInput() {
            isInputEnabled = false;
            if (!joyStick) {
                return;
            }
            DisableJoyStick();
        }
        
        private void DisableJoyStick() => joyStick.DisableJoyStick();

        private void EnableJoyStick() => joyStick.EnableJoyStick();
        
        public void DisablePublishInputEvent() {
            joyStick.DisablePublishInputEvent();
#if UNITY_EDITOR || UNITY_STANDALONE
            isPublishStandaloneInputEvent = false;
#endif
        }

        public void RestorePublishInputEvent() {
            joyStick.RestorePublishInputEvent();
#if UNITY_EDITOR || UNITY_STANDALONE
            isPublishStandaloneInputEvent = true;
#endif
        }

        public InputCanvas GetInputCanvas() {
            return inputCanvas;
        }

        #region Events
        
        #region Direction Input
        
        #region Add Event
        
        public static void AddEventJoyStickInputBegan(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputBegan.AddListener(unityAction);
        }
        
        public static void AddEventJoyStickInputUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputUpdate.AddListener(unityAction);
        }
        
        public static void AddEventJoyStickInputFixedUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputFixedUpdate.AddListener(unityAction);
        }
        
        public static void AddEventJoyStickDragged(UnityAction unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputDragged.AddListener(unityAction);
        }
        
        public static void AddEventJoyStickInputEnded(UnityAction unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputEnded.AddListener(unityAction);
        }
        
        #endregion
        
        #region Remove Event
        
        public static void RemoveEventJoyStickInputBegan(UnityAction<float, Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onJoyStickInputBegan.RemoveListener(unityAction);
        }
                
        public static void RemoveEventJoyStickInputUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExist)
                return;
            inst.onJoyStickInputUpdate.RemoveListener(unityAction);
        }
        
        public static void RemoveEventJoyStickInputFixedUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onJoyStickInputFixedUpdate.RemoveListener(unityAction);
        }
        
        public static void RemoveEventJoyStickDragged(UnityAction unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onJoyStickInputDragged.RemoveListener(unityAction);
        }
        
        public static void RemoveEventJoyStickInputEnded(UnityAction unityAction) {
            if (!IsExist)
                return;
            inst.onJoyStickInputEnded.RemoveListener(unityAction);
        }
        
        #endregion
        
        #region Invoke Event 
        
        public static void InvokeEventJoyStickInputBegan(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputBegan.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventJoyStickInputUpdate(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputUpdate.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventJoyStickInputFixedUpdate(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputFixedUpdate.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventJoyStickDragged() {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputDragged.Invoke();
        }
        
        public static void InvokeEventJoyStickInputEnded() {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onJoyStickInputEnded.Invoke();
        }
        
        #endregion
        
        #endregion
        
        #region Evasion Input

        public static void AddEventEvasionInput(UnityAction<Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            
            inst.onEvasionInputEvent.AddListener(unityAction);
        }

        public static void RemoveEventEvasionInput(UnityAction<Vector2> unityAction) {
            if (!IsExist) {
                return;
            }
            
            inst.onEvasionInputEvent.RemoveListener(unityAction);
        }

        public static void InvokeEventEvasionInput(Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            
            inst.onEvasionInputEvent.Invoke(direction);
        }

        #endregion
        
        #region Editor Or Standalone
        
#if UNITY_EDITOR || UNITY_STANDALONE

        public static void AddEventStandaloneInputBegan(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputBegan.AddListener(unityAction);
        }
        
        public static void AddEventStandaloneInputUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputUpdate.AddListener(unityAction);
        }
        
        public static void AddEventStandaloneInputFixedUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputFixedUpdate.AddListener(unityAction);
        }
        
        public static void AddEventStandaloneInputEnded(UnityAction unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputEnded.AddListener(unityAction);
        }
        
        public static void AddEventStandaloneInteractionInput(UnityAction unityAction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneInteractionInput.AddListener(unityAction);
        }
        
        public static void RemoveEventStandaloneInputBegan(UnityAction<float, Vector2> unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onStandaloneDirectionInputBegan.RemoveListener(unityAction);
        }
        
        public static void RemoveEventStandaloneInputUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onStandaloneDirectionInputUpdate.RemoveListener(unityAction);
        }
        
        public static void RemoveEventStandaloneInputFixedUpdate(UnityAction<float, Vector2> unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onStandaloneDirectionInputFixedUpdate.RemoveListener(unityAction);
        }
        
        public static void RemoveEventStandaloneInputEnded(UnityAction unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onStandaloneDirectionInputEnded.RemoveListener(unityAction);
        }
        
        public static void RemoveEventStandaloneInteractionInput(UnityAction unityAction) {
            if (!IsExist) {
                return;
            }
            inst.onStandaloneInteractionInput.RemoveListener(unityAction);
        }
        
        public static void InvokeEventStandaloneInputBegan(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputBegan.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventStandaloneInputUpdate(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputUpdate.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventStandaloneInputFixedUpdate(float magnitude, Vector2 direction) {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputFixedUpdate.Invoke(magnitude, direction);
        }
        
        public static void InvokeEventStandaloneInputEnded() {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneDirectionInputEnded.Invoke();
        }
        
        public static void InvokeEventStandaloneInteractionInput() {
            if (!IsExistWithLog()) {
                return;
            }
            inst.onStandaloneInteractionInput.Invoke();
        }
        
#endif
        
        #endregion

        #endregion
    }
}
