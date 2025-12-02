using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using RandomDungeonWithBluePrint;

namespace CoffeeCat {
    public enum RoomType {
        PlayerSpawnRoom,
        MonsterSpawnRoom,
        ShopRoom,
        BossRoom,
        RewardRoom,
        EmptyRoom,
        ExitRoom,
    }

    public class DungeonEvtManager : SceneSingleton<DungeonEvtManager> {
        [Title("Events Map", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Map"), SerializeField] private UnityEvent<Field> onMapGenerateCompleted = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent onMapDisposeBefore = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomEntering = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomLeft = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnRoomFirstEntering = new();
        [TabGroup("Events Map"), SerializeField] private UnityEvent<RoomDataStruct> OnClearedRoom = null;

        [Title("Events Player", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerKilled = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnOpeningSkillSelectPanel = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnIncreasePlayerExp = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnIncreasePlayerHP = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<float, float> OnDecreasePlayerHP = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnSkillSelectCompleted = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerLevelUp = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<PlayerSkill> OnPassiveSkillAcquiredForEnhance = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<EquipmentSet> OnPlayerEquipmentsChanged = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerInventoryChanged = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<int> OnPlayerCurrencyChanged = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent<int, int> OnPlayerBuyedItem = new();
        [TabGroup("Events Player"), SerializeField] private UnityEvent OnPlayerSelledItem = new();
        
        [Title("Events Monster", TitleAlignment = TitleAlignments.Centered)]
        [TabGroup("Events Monster"), SerializeField] private UnityEvent<MonsterStatus.KilledInfo> OnMonsterKilledByPlayer = new(); // exp, loot category code 

        public static bool IsExistInst(bool printLog) {
            if (IsExist) {
                return true;
            }
            if (printLog) {
                CatLog.WLog("DungeonEvtManager Is Not Exist.");
            }
            return false;
        }
        
        #region Stage Events

        // Add Event ===================================================================================================

        public static void AddEventRoomFirstEnteringEvent(UnityAction<RoomDataStruct> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomFirstEntering.AddListener(action);
        }

        public static void AddEventRoomEnteringEvent(UnityAction<RoomDataStruct> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomEntering.AddListener(action);
        }

        public static void AddEventRoomLeftEvent(UnityAction<RoomDataStruct> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomLeft.AddListener(action);
        }

        public static void AddEventClearedRoomEvent(UnityAction<RoomDataStruct> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnClearedRoom.AddListener(action);
        }

        public static void AddEventMapGenerateCompleted(UnityAction<Field> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.onMapGenerateCompleted.AddListener(action);
        }

        public static void AddEventMapDisposeBefore(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.onMapDisposeBefore.AddListener(action);
        }

        // Remove Envet ================================================================================================

        public static void RemoveEventMapGenerateCompleted(UnityAction<Field> action) {
            if (!IsExist) return;
            Inst.onMapGenerateCompleted.RemoveListener(action);
        }

        public static void RemoveEventMapDisposeBefore(UnityAction action) {
            if (!IsExist) return;
            Inst.onMapDisposeBefore.RemoveListener(action);
        }

        public static void RemoveEventRoomEntering(UnityAction<RoomDataStruct> action) {
            if (!IsExist) return;
            Inst.OnRoomEntering.RemoveListener(action);
        }

        public static void RemoveEventRoomLeft(UnityAction<RoomDataStruct> action) {
            if (!IsExist) return;
            Inst.OnRoomLeft.RemoveListener(action);
        }

        public static void RemoveEventRoomFirstEntering(UnityAction<RoomDataStruct> action) {
            if (!IsExist) return;
            Inst.OnRoomFirstEntering.RemoveListener(action);
        }

        public static void RemoveEventClearedRoom(UnityAction<RoomDataStruct> action) {
            if (!IsExist) return;
            Inst.OnClearedRoom.RemoveListener(action);
        }

        // Invoke Event ================================================================================================

        public static void InvokeEventRoomEnteringFirst(RoomDataStruct roomType) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomFirstEntering.Invoke(roomType);
        }

        public static void InvokeEventRoomEntering(RoomDataStruct roomType) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomEntering.Invoke(roomType);
        }

        public static void InvokeEventRoomLeft(RoomDataStruct roomType) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnRoomLeft.Invoke(roomType);
        }

        public static void InvokeEventClearedRoom(RoomDataStruct roomType) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnClearedRoom.Invoke(roomType);
        }

        public static void InvokeEventMapGenerateCompleted(Field field) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.onMapGenerateCompleted.Invoke(field);
        }

        public static void InvokeEventMapDisposeBefore() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.onMapDisposeBefore.Invoke();
        }

        // =============================================================================================================

        #endregion

        #region Player Events

        // Add Event ===================================================================================================  

        public static void AddEventIncreasePlayerExp(UnityAction<float, float> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnIncreasePlayerExp.AddListener(action);
        }

        public static void AddEventIncreasePlayerHP(UnityAction<float, float> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnIncreasePlayerHP.AddListener(action);
        }

        public static void AddEventDecreasePlayerHP(UnityAction<float, float> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnDecreasePlayerHP.AddListener(action);
        }

        public static void AddEventPlayerLevelUp(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerLevelUp.AddListener(action);
        }

        public static void AddEventSkillSelectCompleted(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnSkillSelectCompleted.AddListener(action);
        }

        public static void AddEventOpeningSkillSelectPanel(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnOpeningSkillSelectPanel.AddListener(action);
        }

        public static void AddEventOnPlayerKilled(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerKilled.AddListener(action);
        }
        
        public static void AddEventPassiveSkillAcquiredForEnhance(UnityAction<PlayerSkill> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPassiveSkillAcquiredForEnhance.AddListener(action);
        }
        
        public static void AddEventOnPlayerEquipmentsChanged(UnityAction<EquipmentSet> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerEquipmentsChanged.AddListener(action);
        }
        
        public static void AddEventOnPlayerInventoryChanged(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerInventoryChanged.AddListener(action);
        }
        
        public static void AddEventOnPlayerCurrencyChanged(UnityAction<int> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerCurrencyChanged.AddListener(action);
        }
        
        public static void AddEventOnPlayerBuyedItem(UnityAction<int, int> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerBuyedItem.AddListener(action);
        }
        
        public static void AddEventOnPlayerSelledItem(UnityAction action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerSelledItem.AddListener(action);
        }

        // Remove Envet ================================================================================================

        public static void RemoveEventIncreasePlayerExp(UnityAction<float, float> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnIncreasePlayerExp.RemoveListener(action);
        }

        public static void RemoveEventIncreasePlayerHP(UnityAction<float, float> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnIncreasePlayerHP.RemoveListener(action);
        }

        public static void RemoveEventDecreasePlayerHP(UnityAction<float, float> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnDecreasePlayerHP.RemoveListener(action);
        }

        public static void RemoveEventPlayerLevelUp(UnityAction action) {
            if (!IsExist) {
                return;
            }

            Inst.OnPlayerLevelUp.RemoveListener(action);
        }

        public static void RemoveEventSkillSelectCompleted(UnityAction action) {
            if (!IsExist) {
                return;
            }

            Inst.OnSkillSelectCompleted.RemoveListener(action);
        }

        public static void RemoveEventOpeningSkillSelectPanel(UnityAction action) {
            if (!IsExist) {
                return;
            }

            Inst.OnOpeningSkillSelectPanel.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerKilled(UnityAction action) {
            if (!IsExist) {
                return;
            }

            Inst.OnPlayerKilled.RemoveListener(action);
        }
        
        public static void RemoveEventPassiveSkillAcquiredForEnhance(UnityAction<PlayerSkill> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnPassiveSkillAcquiredForEnhance.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerEquipmentsChanged(UnityAction<EquipmentSet> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnPlayerEquipmentsChanged.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerInventoryChanged(UnityAction action) {
            if (!IsExist) {
                return;
            }

            Inst.OnPlayerInventoryChanged.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerCurrencyChanged(UnityAction<int> action) {
            if (!IsExist) return;
            Inst.OnPlayerCurrencyChanged.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerBuyedItem(UnityAction<int, int> action) {
            if (!IsExist) return;
            Inst.OnPlayerBuyedItem.RemoveListener(action);
        }
        
        public static void RemoveEventOnPlayerSelledItem(UnityAction action) {
            if (!IsExist) return;
            Inst.OnPlayerSelledItem.RemoveListener(action);
        }

        // Invoke Event ================================================================================================

        public static void InvokeEventIncreasePlayerExp(float currentExp, float maxExp) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnIncreasePlayerExp?.Invoke(currentExp, maxExp);
        }

        public static void InvokeEventIncreasePlayerHP(float hp, float maxHp) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnIncreasePlayerHP.Invoke(hp, maxHp);
        }

        public static void InvokeEventDecreasePlayerHP(float hp, float maxHp) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnDecreasePlayerHP.Invoke(hp, maxHp);
        }

        public static void InvokeEventPlayerLevelUp() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerLevelUp.Invoke();
        }

        public static void InvokeEventSkillSelectCompleted() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnSkillSelectCompleted.Invoke();
        }

        public static void InvokeEventOpeningSkillSelectPanel() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnOpeningSkillSelectPanel.Invoke();
        }

        public static void InvokeEventPlayerKilled() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerKilled.Invoke();
        }
        
        public static void InvokeEventPassiveSkillAcquiredForEnhance(PlayerSkill skill) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPassiveSkillAcquiredForEnhance.Invoke(skill);
        }
        
        public static void InvokeEventOnPlayerEquipmentsChanged(EquipmentSet set) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerEquipmentsChanged.Invoke(set);
        }
        
        public static void InvokeEventOnPlayerInventoryChanged() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerInventoryChanged.Invoke();
        }
        
        public static void InvokeEventOnPlayerCurrencyChanged(int currencyA) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerCurrencyChanged.Invoke(currencyA);
        }
        
        public static void InvokeEventOnPlayerBuyedItem(int itemId, int price) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerBuyedItem.Invoke(itemId, price);
        }
        
        public static void InvokeEventOnPlayerSelledItem() {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnPlayerSelledItem.Invoke();
        }

        // =============================================================================================================

        #endregion

        #region Monster Events

        // Add Event ===================================================================================================  

        public static void AddEventMonsterKilledByPlayer(UnityAction<MonsterStatus.KilledInfo> action) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnMonsterKilledByPlayer.AddListener(action);
        }

        // Remove Envet ================================================================================================

        public static void RemoveEventMonsterKilledByPlayer(UnityAction<MonsterStatus.KilledInfo> action) {
            if (!IsExist) {
                return;
            }

            Inst.OnMonsterKilledByPlayer.RemoveListener(action);
        }

        // Invoke Event ================================================================================================

        public static void InvokeEventMonsterKilledByPlayer(MonsterStatus.KilledInfo killedInfo) {
            if (!IsExist) {
                CatLog.Log("DungeonEvtManager Is Not Exist.");
                return;
            }

            Inst.OnMonsterKilledByPlayer.Invoke(killedInfo);
        }
        
        // =============================================================================================================

        #endregion
    }
}