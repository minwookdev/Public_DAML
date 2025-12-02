using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using UnityRandom = UnityEngine.Random;
using PathFindGrid = CoffeeCat.Pathfinding2D.Grid;

namespace RandomDungeonWithBluePrint
{
    public class RandomMapGenerator : MonoBehaviour
    {
        [Serializable]
        public class BluePrintWithWeight
        {
            public FieldBluePrint BluePrint = default;
            public int Weight = default;
        }
        
        [Title("Generate Options")]
        [ShowInInspector, ReadOnly] private int seed = default;
        [SerializeField] private FieldView fieldView = null;
        [SerializeField] public DungeonBluePrint dungeonBluePrint = null;
        [SerializeField] private FieldBluePrint TestBluePrint = null; // 확정 생성 BluePrint
        [SerializeField] private BluePrintWithWeight[] bluePrints = null;
        public Field field { get; private set; }
        public DungeonBluePrint DungeonBluePrint => dungeonBluePrint;

        [Title("Bake PathFind Grid (Not Using)")]
        [SerializeField, ReadOnly] private bool isBakePathFindGrid = false;
        [SerializeField, ReadOnly] private PathFindGrid pathFindGrid;
        
        #region DEBUG
#if UNITY_EDITOR || DEBUG_MODE
        [Title("Debugging Options", TitleAlignment = TitleAlignments.Centered)]
        public bool IsDisplayRoomType = false;
        public bool IsDisplaySectionRectDrawer = false;
        public bool IsDisplaySectionIndex = false;
        public bool IsDisplayRoomRectDrawer = false;
        private bool initializedDebugObservable = false;
        public Color RoomDrawerColor = Color.green;
        public Color SectionDrawerColor = Color.white;
#endif
        #endregion

        private void Awake()
        {
            // assign random seed
            seed = UnityRandom.Range(int.MinValue, int.MaxValue);
            
            // Init Random Seed
            // UnityRandom.InitState(seed);
        }

        public bool IsLastFloor(int floorCount) {
            return floorCount >= dungeonBluePrint.NormalMapBluePrints.Length - 1;
        }

        public void GenerateNextMap(int currentFloor) {
            // Reached The Maximum Floor: Load Boss Map Scene
            if (IsLastFloor(currentFloor)) {
                EnteringBossRoom();
                return;
            }

            // if exist reserved blueprint, use reserved blueprint
            var reservedBluePrint = RogueLiteManager.Inst.ReservedDungeonBluePrint;
            if (reservedBluePrint) {
                dungeonBluePrint = reservedBluePrint;
                RogueLiteManager.Inst.ClearReservedBluePrint();
            }

            var bluePrint = dungeonBluePrint.NormalMapBluePrints[currentFloor];
            ExecuteGenerate(bluePrint);
        }
        
        private void ExecuteGenerate(FieldBluePrint bluePrint) {
            if (TestBluePrint) {
                bluePrint = TestBluePrint;
                CatLog.WLog("Override Test BluePrint: [GENERATE TEST MODE MAP");
            }
            
            if (!bluePrint) {
                CatLog.ELog("BluePrint Is Null");
                return;
            }
            
            // Invoke Event Before Dispose Generated Map  
            if (field != null) {
                DungeonEvtManager.InvokeEventMapDisposeBefore();
            }
            
            // Clear And ReGenerate Dungeon Map
            Create(bluePrint);
            DungeonEvtManager.InvokeEventMapGenerateCompleted(field);
            
            // Bake PathFind Grid
            if (isBakePathFindGrid) {
                pathFindGrid.CreateGridDictionary(this);
            }
            
            InitDebugs();
        }
        
        private void Create(BluePrintWithWeight bluePrintWeight) {
            ClearMap();
            Create(bluePrintWeight.BluePrint);
        }

        private void Create(FieldBluePrint bluePrint) {
            ClearMap();
            field = FieldBuilder.Build(bluePrint);
            fieldView.DrawDungeon(field);
        }

        private void EnteringBossRoom() {
            CatLog.Log("Load Boss Map Scene");
            DungeonEvtManager.InvokeEventMapDisposeBefore();
            ClearMap();
            var key = dungeonBluePrint.BossMapSceneKey;
            SceneManager.Inst.LoadSceneAdditive(key);
        }

        private BluePrintWithWeight Raffle()
        {
            var candidate = bluePrints.ToList();
            var rand = UnityRandom.Range(0, candidate.Sum(c => c.Weight));
            var pick = 0;
            for (var i = 0; i < candidate.Count; i++)
            {
                if (rand < candidate[i].Weight)
                {
                    pick = i;
                    break;
                }

                rand -= candidate[i].Weight;
            }

            return candidate[pick];
        }

        public void ClearMap() {
            ClearDebug();
            field?.Dispose();
            field = null;
            fieldView.AllTilemapClear();
        }

        public void SetItemInConnections() => SetItemInConnectionsAsync().Forget();

        private async UniTaskVoid SetItemInConnectionsAsync() {
            float timeElaped = 0f;
            while (true) {
                timeElaped += Time.deltaTime;
                if (timeElaped >= 3f) {
                    throw new Exception($"Wait TimeOut: {nameof(SetItemInConnectionsAsync)}");
                }

                if (ObjectPoolManager.IsExist && ObjectPoolManager.Inst.IsExistInPool(AddressablesKey.Object_Item.ToKey())) {
                    break;
                }

                var isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                if (isCanceled) {
                    return;
                }
            }
            
            ItemLootRequest RequestCurrency = new ItemLootRequest {
                Code = ItemCode.Currency,
                Amount = 5
            };
            ItemLootRequest RequestExp = new ItemLootRequest {
                Code = ItemCode.ExpSmall,
                Amount = 1
            };
            
            var connectionDetails = field.ConnectionDetails;
            for (int i = 0; i < connectionDetails.Count; i++) {
                var connectionDetail = connectionDetails[i];
                var ways = connectionDetail.Ways;
                if (ways.Count <= 2) {
                    continue;
                }
                
                int randomValue = UnityRandom.Range(0, 2);
                for (int j = 1; j < ways.Count - 1; j++) {
                    var way = connectionDetail.Ways[j];
                    var spawnPos = new Vector2(way.x + Constants.TileRadius, way.y + Constants.TileRadius);
                    var itemObject = ObjectPoolManager.Inst.Spawn<ItemObject>(AddressablesKey.Object_Item.ToKey(), spawnPos, Quaternion.identity);
                    itemObject.Init(randomValue == 0 ? RequestCurrency : RequestExp, false, playTracking: false);
                }
            }
        }

        #region Debug_Drawer
        
        private void InitDebugs() {
#if UNITY_EDITOR || DEBUG_MODE
            if (IsDisplayRoomType) {
                DisplayRoomType();
            }

            if (IsDisplaySectionIndex) {
                DisplaySectionIndex();
            }
            
            if (initializedDebugObservable)
                return;
            
            // Init Observable Room Type
            this.ObserveEveryValueChanged(_ => IsDisplayRoomType)
                .Skip(0)
                .TakeUntilDestroy(this)
                .Subscribe(isEnable => {
                    if (isEnable) {
                        DisplayRoomType();
                    }
                    else {
                        ClearRoomTypeText();
                    }
                })
                .AddTo(this);
            
            // Init Observable Section Index
            this.ObserveEveryValueChanged(_ => IsDisplaySectionIndex)
                .Skip(0)
                .TakeUntilDestroy(this)
                .Subscribe(isEnable => {
                    if (isEnable) {
                        DisplaySectionIndex();
                    }
                    else {
                        ClearSectionIndexText();
                    }
                })
                .AddTo(this);
            
            initializedDebugObservable = true;
#endif
        }

        private void ClearDebug() {
#if UNITY_EDITOR || DEBUG_MODE
            ClearRoomTypeText();
            ClearSectionIndexText();
#endif
        }
        
        /// <summary>
        /// RoomType을 표시
        /// </summary>
        private void DisplayRoomType() {
#if UNITY_EDITOR || DEBUG_MODE
            ClearRoomTypeText();
            
            foreach (var room in field.Rooms) {
                Vector2 spawnPoint = new Vector2(room.Rect.xMin, room.Rect.yMin);
                var text = ObjectPoolManager.Inst.Spawn<TextMeshPro>("editor_text_room_type", spawnPoint, Quaternion.identity);
                text.SetText(room.RoomType.ToStringExtended());
            }
#endif
        }

        private void ClearRoomTypeText() {
#if UNITY_EDITOR || DEBUG_MODE
            if (!ObjectPoolManager.Inst.IsExistInPool("editor_text_room_type"))
                return;
            ObjectPoolManager.Inst.DespawnAll("editor_text_room_type");
#endif
        }

        private void DisplaySectionIndex() {
#if UNITY_EDITOR || DEBUG_MODE
            ClearSectionIndexText();
            
            var sections = field?.Sections;
            if (sections == null)
                return;
            
            for (int i = 0; i < sections.Count; i++) {
                var rect = sections[i].Rect;
                var point = new Vector2(rect.xMin, rect.yMax);
                var text = ObjectPoolManager.Inst.Spawn<TextMeshPro>("editor_text_section_index", point, Quaternion.identity);
                text.SetText("< " + sections[i].Index.ToString() + " >");
            }
#endif
        }
        
        private void ClearSectionIndexText() {
#if UNITY_EDITOR || DEBUG_MODE
            if (!ObjectPoolManager.Inst.IsExistInPool("editor_text_section_index"))
                return;
            ObjectPoolManager.Inst.DespawnAll("editor_text_section_index");
#endif
        }
        
        private void OnDrawGizmos() {
#if UNITY_EDITOR || DEBUG_MODE
            if (field == null) {
                return;
            }
            
            // Draw Room Rect
            if (IsDisplaySectionRectDrawer) {
                var sections = field?.Sections;
                if (sections == null)
                    return;
                
                Gizmos.color = SectionDrawerColor;
                for (int i = 0; i < sections.Count; i++) {
                    if (sections[i] == null)
                        continue;
                    var rect = sections[i].Rect;
                    var center = new Vector3(rect.center.x, rect.center.y, 0f);
                    var size = new Vector3(rect.width, rect.height, 0f);
                    Gizmos.DrawWireCube(center, size);
                }
            }
            
            // Draw Section Rect
            if (IsDisplayRoomRectDrawer) {
                var rooms = field?.Rooms;
                if (rooms == null)
                    return;
                
                Gizmos.color = RoomDrawerColor;
                for (var i = 0; i < rooms.Count; i++) {
                    var room = rooms[i];
                    if (room == null)
                        continue;
                    RectInt floorRectInt = room.FloorRectInt;
                    Vector3 centerVec = new Vector3(floorRectInt.center.x, floorRectInt.center.y, 0f);
                    Vector3 sizeVec = new Vector3(floorRectInt.width, floorRectInt.height, 0f);
                    Gizmos.DrawWireCube(centerVec, sizeVec);
                }
            }

            // Restore Gizmos Color 
            Gizmos.color = Color.white;
#endif
        }
        
        #endregion
    }
}