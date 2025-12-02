using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using RandomDungeonWithBluePrint;

namespace CoffeeCat.FrameWork {
	public class RogueLiteManager : DynamicSingleton<RogueLiteManager> {
		// Propreites: Show Inspector
		[ShowInInspector, ReadOnly] public string ReservedDungeonId { get; private set; } = null;
		[ShowInInspector, ReadOnly] public DungeonBluePrint ReservedDungeonBluePrint { get; private set; } = null;
		private bool isInvocableInteractEvent = false;
		
		// Properties
		public CharacterInventorySystem PlayerInvenSystem { get; private set; } = new();
		public Player_Dungeon SpawnedPlayer { get; private set; } = null;
		public Interaction EnteredInteractable { get; private set; } = null;
		public CameraController MainCamController { get; private set; } = null;
		public PlayerAddressablesKey playerKey { get; private set; } = PlayerAddressablesKey.Flower_Dungeon;
		public Vector3 SpawnedPlayerPosition => SpawnedPlayer.Tr.position;
		public Vector2Int LastUpdatedResolution = Vector2Int.zero;
		public bool IsEnteredDungeon { get; private set; } = false;
		private const int TargetFrameRate = 60;

		// Events
		private readonly UnityEvent onInteracted = new();
		private readonly UnityEvent<Vector2Int> onResolutionChanged = new();

		protected override void Initialize() {
			base.Initialize();
			
			// init inventory system
			PlayerInvenSystem.Init(DataManager.Inst.Items);
			
			// set target frame
			Application.targetFrameRate = TargetFrameRate;
			
			InputManager.AddEventJoyStickDragged(DisableInvocableInteractEvent);
			InputManager.AddEventJoyStickInputBegan(SetInvocableInteractEvent);
			InputManager.AddEventJoyStickInputEnded(InvokeIfPossibleInteractEvent);
			
#if UNITY_EDITOR || UNITY_STANDALONE
			InputManager.AddEventStandaloneInteractionInput(InteractIfValid);
#endif
		}
		
		protected override void InvokeOnDestroy() {
			InputManager.RemoveEventJoyStickDragged(DisableInvocableInteractEvent);
			InputManager.RemoveEventJoyStickInputBegan(SetInvocableInteractEvent);
			InputManager.RemoveEventJoyStickInputEnded(InvokeIfPossibleInteractEvent);
			
#if UNITY_EDITOR || UNITY_STANDALONE
			InputManager.RemoveEventStandaloneInteractionInput(InteractIfValid);
#endif
		}

		private void Update() {
			var currentResolution = new Vector2Int(Screen.width, Screen.height);
			if (currentResolution == LastUpdatedResolution) {
				return;
			}
			LastUpdatedResolution = currentResolution;
			onResolutionChanged.Invoke(LastUpdatedResolution);
		}
		
		#region Camera
		
		public void SetMainCameraController(CameraController mainCamController) => MainCamController = mainCamController;

		#endregion
		
		#region Player
		
		public void SetPlayerOnEnteredDungeon(Vector2 playerSpawnPosition) {
			SpawnPlayer();
			SetPlayerPosition(playerSpawnPosition);
			ActivePlayer();
			MainCamController.SetTarget(SpawnedPlayer.Tr);
		}

		public void SetDungeonPlayerKey(PlayerAddressablesKey key)
		{
			playerKey = key;
		}
		
		private void SpawnPlayer() {
			if (SpawnedPlayer) {
				return;
			}
			
			// spawn character by current selected index and upgrade count
			var selectedCharacterIndex = UserDataManager.Inst.SelectedCharacterIndex;
			var characterSelectData = DataManager.Inst.CharacterSelectDatas;
			var characterInfo = characterSelectData.GetCharacterInfo(selectedCharacterIndex);
			var dungeonCharacterKey = characterInfo.DungeonSpawnKey;
			SpawnedPlayer = ObjectPoolManager.Inst.Spawn<Player_Dungeon>(dungeonCharacterKey.ToKey(), Vector3.zero);
			
			// only change skeleton asset data if beginner upgrade is completed
			if (!UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner)) {
				return;
			}
			
			var skeletonAssetKey = characterSelectData.GetSkeletonDataAssetKeys(selectedCharacterIndex);
			skeletonAssetKey.IncrementSkeletonDataVersion();
			SpawnedPlayer.SetSkeletonData(skeletonAssetKey);
		}

		private void SetPlayerPosition(Vector2 position) {
			if (!SpawnedPlayer) {
				return;
			}
			SpawnedPlayer.Tr.position = position;
		}

		private void ActivePlayer() {
			if (!SpawnedPlayer)
				return;
			SpawnedPlayer.gameObject.SetActive(true);
		}

		public void DisablePlayer() {
			if (!SpawnedPlayer)
				return;
			SpawnedPlayer.gameObject.SetActive(false);
		}

		public void DespawnPlayer() {
			if (!SpawnedPlayer) {
				return;
			}
			ObjectPoolManager.Inst.Despawn(SpawnedPlayer.gameObject);
		}
		
		#endregion

		public void IncreasePlayerHealth(float incAmount) {
			if (!SpawnedPlayer) {
				return;
			}
			SpawnedPlayer.IncreasePlayerHealth(incAmount);
		}

		public void TimeScaleZero() {
			Time.timeScale = 0f;
		}

		public void RestoreTimeScale() {
			Time.timeScale = 1f;
		}

		public bool IsPlayerExistAndAlive() => SpawnedPlayer && !SpawnedPlayer.IsDead();

		public bool IsPlayerNotExistOrDeath() => !SpawnedPlayer || SpawnedPlayer.IsDead();
        
		public void OnEnterInteract(Interaction interactable) => EnteredInteractable = interactable;
		
		private void InteractIfValid() {
			if (!EnteredInteractable) {
				return;
			}
			EnteredInteractable.Interact();
		}

		public void OnExitInteract() {
			EnteredInteractable = null;
			isInvocableInteractEvent = false;
		}
		
		private void DisableInvocableInteractEvent() => isInvocableInteractEvent = false;

		private void SetInvocableInteractEvent(float factor, Vector2 direction) {
			if (!EnteredInteractable) {
				return;
			}

			isInvocableInteractEvent = true;
		}

		private void InvokeIfPossibleInteractEvent() {
			if (!isInvocableInteractEvent) {
				return;
			}
			isInvocableInteractEvent = false;
			InteractIfValid();
		}
		
		public void EnableSkillSelectIfPossible() {
			SpawnedPlayer.EnableSkillSelectPanelIfPossible();
		}

		public void UpdatePlayerSkill(int skillIndex)
		{
			SpawnedPlayer.UpdateSkillData(skillIndex);
		}
		
		public void AddCurrency(int value) => PlayerInvenSystem.AddCurrency(value);

		public void AddItem(int itemId, int amount) => PlayerInvenSystem.AddItem(itemId, amount);

		public void ForcedIncreasePlayerLevel() {
			if (!SpawnedPlayer) {
				return;
			}
			SpawnedPlayer.ForcedIncreaseLevel();
		}

		public void AddExp(float amount) => SpawnedPlayer.AddExp(amount);

		public void SetIsEnteredDungeon(bool value) {
			IsEnteredDungeon = value;
		}

		#region Town Scene Methods
		
		public void EnteringDungeon(string enteringDungeonId) {
			// set reserved dungeon id / blueprint
			ReservedDungeonId = enteringDungeonId;
			var bluePrint = DataManager.Inst.GetBluePrintQueue(ReservedDungeonId);
			if (!bluePrint) {
				CatLog.ELog("Dungeon BluePrint Queue is Null !");
				return;
			}
			ReservedDungeonBluePrint = bluePrint;
			
			// load dungeon scene
			SceneManager.Inst.LoadSceneSingle(SceneName.DungeonScene, true, false);
		}

		public void ClearReservedBluePrint() {
			ReservedDungeonId = "";
			ReservedDungeonBluePrint = null;
		}
		
		#endregion
		
		#region Events
		
		public static void AddEventToOnResolutionChanged(UnityAction<Vector2Int> unityAction) {
			if (!IsExistWithLog()) {
				return;
			}
            Inst.onResolutionChanged.AddListener(unityAction);
		}
		
		public static void RemoveEventToOnResolutionChanged(UnityAction<Vector2Int> unityAction) {
			if (!IsExistWithLog(false)) {
				return;
			}
			Inst.onResolutionChanged.RemoveListener(unityAction);
		}
		
		public static void AddEventToOnInteracted(UnityAction unityAction) {
			if (!IsExistWithLog()) {
				return;
			}
			Inst.onInteracted.AddListener(unityAction);
		}
		
		public static void RemoveEventToOnInteracted(UnityAction unityAction) {
			if (!IsExistWithLog(false)) {
				return;
			}
			Inst.onInteracted.RemoveListener(unityAction);
		}
		
		#endregion
	}
}
