namespace CoffeeCat.Editor {
#if UNITY_EDITOR || DEBUG_MODE
    using FrameWork;
    using Sirenix.OdinInspector;
    
    /// <summary>
    /// DebugManager GameObject attaches 'EditorOnly' Tag.
    /// </summary>
    public class DebugManager : SceneSingleton<DebugManager> {
        public void OnForcedNextFloor() {
            if (DungeonSceneBase.IsExist) {
                DungeonSceneBase.Inst.RequestGenerateNextFloor();
            }
        }

        public void OnForcedEnableSkillSelectPanel() {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            if (player) {
                player.EnableSkillSelectPanelForced();
            }
        }
        
        public void OnForcedDisableSkillSelectPanel() {
            if (!DungeonUIPresenter.IsExist) {
                return;
            }
            DungeonUIPresenter.Inst.CloseSkillSelectPanel();
        }

        public void EnablePlayerBattleMode() {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            if (player) {
                player.SetBattleMode(true);
                player.ActiveSkillEffect();
            }
        }

        public void DisablePlayerBattleMode() {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            if (player) {
                player.SetBattleMode(false);
            }
        }

        [PropertySpace(10f), Button("Debug AddItem", ButtonSizes.Medium)]
        public void ForcedAddItem(int itemId, int amount) {
            RogueLiteManager.Inst.AddItem(itemId, amount);
        }

        [PropertySpace(10f), Button("Add All Equipment Items", ButtonSizes.Medium)]
        public void AddAllEquipmentsItem() {
            if (!DataManager.IsExist || !RogueLiteManager.IsExist) {
                return;
            }
            
            var inventorySystem = RogueLiteManager.Inst.PlayerInvenSystem;
            var items = DataManager.Inst.Items.EquipmentDict;
            foreach (var pair in items) {
                inventorySystem.AddItem(pair.Value);
            }
        }
        
        [PropertySpace(10f), Button("Add CurrencyB [10,000]", ButtonSizes.Medium)]
        public void AddCurrencyB()
        {
            UserDataManager.Inst.IncreaseCurrency(10000);
            TownEvtManager.InvokeCurrencyChanged(UserDataManager.Inst.Currency);
        }
    }
#endif
}