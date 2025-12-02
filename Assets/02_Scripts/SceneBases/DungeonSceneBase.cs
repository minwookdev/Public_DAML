using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RandomDungeonWithBluePrint;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.FrameWork {
    /// <summary>
    /// Scene Base Scripts
    /// </summary>
    [DisallowMultipleComponent]
    public class DungeonSceneBase : SceneSingleton<DungeonSceneBase> {
        [Space(5f), Title(title: "SCENE BASE", titleAlignment: TitleAlignments.Centered, horizontalLine: true, bold: true)]

        [Title("PRELOAD DATA")]
        public bool isPreloadData = true;

        [Title("OBJECT POOL"), ListDrawerSettings(NumberOfItemsPerPage = 5, ShowFoldout = true)]
        public PoolInfo[] DefaultPoolInformation = null;

        [Title("AUDIO CLIPS")]
        public StringAudioClipDictionary AudioClipDictionary = null;
        [PropertySpace(10f), SerializeField] private CameraController mainCamController = null;
        public Camera uiCamera = null;
        public Canvas damageTextCanvas = null;
        public RectTransform damageTextParent = null;

        [FoldoutGroup("Requires"), SerializeField] private RandomMapGenerator mapGen;
        [FoldoutGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentRoomMonsterKilledCount { get; private set; } = 0;
        [FoldoutGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloorMonsterKillCount { get; private set; } = 0;
        [FoldoutGroup("Requires"), ShowInInspector, ReadOnly] public int TotalMonsterKilledCount { get; private set; } = 0;
        [FoldoutGroup("Requires"), ShowInInspector, ReadOnly] public int CurrentFloor { get; private set; } = 0;
        public Room PlayerCurrentRoom { get; private set; } = null;
        public DungeonBluePrint DungeonBluePrint => mapGen.DungeonBluePrint;
        private LootTableDictionary lootTable = null;
        private readonly List<ItemLootRequest> monsterLootRequests = new();
        private const float ITEM_LOOT_DELAY_SECONDS = 0.1f;
        
        protected override void Initialize() {
            // create dynamic singleton managers
            FirebaseManager.Create();
            ResourceManager.Create();
            DataManager.Create();
            UserDataManager.Create();
            SceneManager.Create();
            ObjectPoolManager.Create();
            EffectManager.Create();
            InputManager.Create();
            RogueLiteManager.Create();
            SoundManager.Create();
            SkillEffectManager.Create();
        }

        public void Start() {
            // manager's ready
            SafeLoader.StartProcess(gameObject);
            SoundManager.Inst.RegistAudioClips(AudioClipDictionary);
            ObjectPoolManager.Inst.AddToPool(DefaultPoolInformation);
            DamageTextManager.Inst.Setup(damageTextCanvas, uiCamera);
            RogueLiteManager.Inst.SetMainCameraController(mainCamController);
            
            // set is dungeon entered
            RogueLiteManager.Inst.SetIsEnteredDungeon(true);
            
            // start random map generating
            DungeonUIPresenter.Inst.OpenFadePanel();
            DungeonUIPresenter.Inst.StartPanelTweenPlay();
            mapGen.GenerateNextMap(CurrentFloor);
            DungeonUIPresenter.Inst.CloseFadePanel();
            
            // preload all item icon sprites
            var allItemDict = DataManager.Inst.Items.Dict;
            foreach (var pair in allItemDict) {
                var item = pair.Value;
                ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(item.IconKey, false, null);
            }
            
            DungeonEvtManager.AddEventMonsterKilledByPlayer(OnMonsterKilledByPlayer);
            SetLootTable(mapGen.DungeonBluePrint.LootTable);
            
            // play dungeon bgm
            SoundManager.Inst.PlayBgm(SoundKey.BGM_Dungeon.ToKey(), 0.5f);
        }
        
        private void OnDisable() {
            SafeLoader.StopProcess();
        }
        
        #region Map Generating Methods
        
        public void CreateGate(Field field)
        {
            var gates = field.Gates;
            for (int i = 0; i < gates.Count; i++) {
                var gate = gates[i];
                var room = gate.Section.Room;
                var linkedSection = field.GetLinkedSectionByFrom(gate);
                var spawnedGateObject = ObjectPoolManager.Inst.Spawn<GateObject>(AddressablesKey.GateObject.ToKey(), Vector3.zero);
                spawnedGateObject.Initialize(gate, room, linkedSection);
                room.AddGateObject(spawnedGateObject);
                room.RoomData?.AddEventOnRoomLocked(spawnedGateObject);
            }
        }
        
        public void SetPlayer(Field field) {
            // Not Founded Entry Room
            if (!field.TryFindRoomFromType(RoomType.PlayerSpawnRoom, out Room result)) {
                CatLog.WLog("Not Exist PlayerSpawn Room !");
                return;
            }
            
            // Set RogueLite Player Object
            RogueLiteManager.Inst.SetPlayerOnEnteredDungeon(result.Rect.center);
            PlayerCurrentRoom = result;
            PlayerCurrentRoom.RoomData.EnteredPlayer();
        }
        
        public void DespawnGates() {
            ObjectPoolManager.Inst.DespawnAll("dungeon_door");
        }
        
        public void SetPlayersRoom(Room enteredRoom) {
            // 현재 플레이어의 방을 정의하는 변수가 덮혀씌워지는 것을 체크
            if (PlayerCurrentRoom != null) {
                CatLog.WLog("Overriding Players CurrentRoom !");
            }
            
            PlayerCurrentRoom = enteredRoom;
        }

        public void ClearPlayersRoom(Room leavesRoom) {
            // 현재 플레이어의 방과 나가려는 방이 일치하는지 확인
            if (!ReferenceEquals(PlayerCurrentRoom, leavesRoom)) {
                CatLog.ELog("Player Leaves Room Check Error !");
                return;
            }

            PlayerCurrentRoom = null;
        }
        
        public void ClearCurrentRoomKillCount()
        {
            CurrentFloorMonsterKillCount += CurrentRoomMonsterKilledCount;
            CurrentRoomMonsterKilledCount = 0;
        }

        public void AddCurrentRoomKillCount()
        {
            CurrentRoomMonsterKilledCount++;
        }
        
        public void RequestGenerateNextFloor() {
            CurrentFloor++;
            mapGen.GenerateNextMap(CurrentFloor);
        }

        public void RequestToTownScene() {
            DungeonEvtManager.InvokeEventMapDisposeBefore();
            mapGen.ClearMap();
            SoundManager.Inst.StopBgm();
            
            // load town scene
            SceneManager.Inst.LoadSceneWithLoadingScene(SceneName.TownScene);
        }
        
        public bool IsNextFloorLast() => mapGen.IsLastFloor(CurrentFloor + 1);
        
        #endregion
        
        #region Game Playing
        
        public void DisablePlayer() => RogueLiteManager.Inst.DisablePlayer();

        public void EnableInput() => InputManager.EnableInputSafe();

        public void DisableInput() => InputManager.DisableInputSafe();

        public void RestoreTimeScale() => RogueLiteManager.Inst.RestoreTimeScale();

        public void TimeScaleZero() => RogueLiteManager.Inst.TimeScaleZero();
        
        public void PlayButtonSE(bool isPositiveType) => SoundManager.Inst.PlayButtonSE(isPositiveType);

        #region ITEM & CURRENCY

        private void SetLootTable(LootTableDictionary lootTableDictionary) {
            lootTable = lootTableDictionary;
            // check is valid loot table value's
            foreach (var pair in lootTable) {
                var list = pair.Value.Loots;
                for (int i = 0; i < list.Count; i++) {
                    var lootData = list[i];
                    if (!lootData.isApplyAmountRandomRange) {
                        var value = lootData.Amount;
                        if (!Utility.IsValidRangeUShort(value)) {
                            CatLog.ELog("Invalid Amount Value !");
                        }
                    }
                    else {
                        var minValue = lootData.RandomAmountMin;
                        var maxValue = lootData.RandomAmountMax;
                        if (minValue > maxValue) {
                            CatLog.ELog("Invalid Random Amount Range !: MinValue > MaxValue");
                        }
                        if (!Utility.IsValidRangeUShort(minValue) || !Utility.IsValidRangeUShort(maxValue)) {
                            CatLog.ELog("Invalid Random Amount Range ! Min Or Max Value");
                        }
                    }
                }
            }
        }

        private void OnMonsterKilledByPlayer(MonsterStatus.KilledInfo killedInfo) {
            UpdateRequestsOnMonsterKilled(killedInfo.LootCategory);
            SpawnItemObject(monsterLootRequests, killedInfo.Position, killedInfo.DeathAnimDuration);
        }

        private void UpdateRequestsOnMonsterKilled(int categoryCode) {
            monsterLootRequests.Clear();
            if (categoryCode == 0 || !lootTable.TryGetValue((LootCategory)categoryCode, out var result)) {
                return;
            }

            var itemLootList = result.Loots;
            for (int i = 0; i < itemLootList.Count; i++) {
                var lootData = itemLootList[i];
                var isDropable = Extensions.EvaluateChance100I(lootData.Chance);
                if (!isDropable) {
                    continue;
                }
                
                // add item loot request
                var isCurrency = lootData.Code == ItemCode.Currency;
                if (!isCurrency) {
                    var amount = lootData.GetAmount();
                    var request = new ItemLootRequest {
                        Code = lootData.Code,
                        Amount = amount
                    };
                    monsterLootRequests.Add(request);
                    continue;
                }
                
                // add divide currency
                // TODO: change to divide count according loot category
                var divideCount = Random.Range(2, 3 + 1);
                var divideAmount = lootData.GetDivideAmount(divideCount);
                for (int j = 0; j < divideCount; j++) {
                    var request = new ItemLootRequest {
                        Code = lootData.Code,
                        Amount = divideAmount
                    };
                    monsterLootRequests.Add(request);
                }
            }
        }

        public void SpawnItemObject(List<ItemLootRequest> requests, Vector2 position, float initDelaySeconds = 0f) {
            for (int i = 0; i < requests.Count; i++) {
                var request = requests[i];
                var itemObject = ObjectPoolManager.Inst.Spawn<ItemObject>(AddressablesKey.Object_Item.ToKey(), position);
                itemObject.Init(in request, true, initDelaySeconds);
                initDelaySeconds += ITEM_LOOT_DELAY_SECONDS;
            }
        }

        public void DespawnAllItemObjects() {
            ObjectPoolManager.Inst.DespawnAll(AddressablesKey.Object_Item.ToKey());
        }
        
        #endregion
        
        #endregion
        
#if UNITY_EDITOR
        [PropertySpace(10f), HorizontalGroup("ButtonH1"), Button("Add AudioClips", ButtonSizes.Medium)]
        private void SetAudioClipDictionary(AudioClip[] addClips) {
            if (addClips == null || addClips.Length == 0) {
                return;
            }
            
            // AudioClipDictionary.Clear();
            for (int i = 0; i < addClips.Length; i++) {
                var clip = addClips[i];
                if (clip == null) {
                    continue;
                }
                var key = clip.name;
                if (AudioClipDictionary.TryAdd(key, clip)) {
                    continue;
                }
                CatLog.WLog($"Exist Dupe AudioClip: {key}");
            }
        }

        [PropertySpace(10f), HorizontalGroup("ButtonH1"), Button("Clear AudioClips", ButtonSizes.Medium)]
        private void ClearAudioClipDictionary() {
            AudioClipDictionary.Clear();
        }
#endif
    }
}