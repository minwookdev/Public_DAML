using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class PlayerInfoPanel : MonoBehaviour {
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotWaepon = null;
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotAccessory = null;
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotArmor = null;
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotGloves = null;
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotShoes = null;
        [FoldoutGroup("Equipments"), SerializeField] private ItemSlot slotArtifact = null;
        [FoldoutGroup("Equipments"), SerializeField] private TextMeshProUGUI tmpPlayerLevel = null;
        
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpHp = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpDamage = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpDefense = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpMoveSpeed = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpPenetration = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpCriticalChance = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpCriticalMultiplier = null;
        [FoldoutGroup("Stats"), SerializeField] private TextMeshProUGUI tmpCriticalResistance = null;

        [FoldoutGroup("Inventory"), SerializeField] private InventoryPanelSystem inventoryPanelSystem = null;
        
        // others
        private bool isInit = false;
        private Player_Dungeon player = null;
        private CharacterInventorySystem inventorySystem = null;

        private bool flag_Update_Inventory = true;
        private bool flag_Update_Equipments = true;
        
        private void OnEnable() => UpdatePanel();

        private void Start() {
            DungeonEvtManager.AddEventOnPlayerEquipmentsChanged(OnEquipmentsChanged);
            DungeonEvtManager.AddEventOnPlayerInventoryChanged(OnInventoryChanged);
        }

        private void OnDestroy() {
            DungeonEvtManager.RemoveEventOnPlayerEquipmentsChanged(OnEquipmentsChanged);
            DungeonEvtManager.RemoveEventOnPlayerInventoryChanged(OnInventoryChanged);
        }

        private void UpdatePanel() {
            Init();
            if (flag_Update_Inventory) {
                UpdateInventory();
            }
            if (flag_Update_Equipments) {
                UpdateEquipments();
            }
            UpdateStats();
        }

        private void Init() {
            if (isInit) {
                return;
            }
            player = RogueLiteManager.Inst.SpawnedPlayer;
            inventorySystem = RogueLiteManager.Inst.PlayerInvenSystem;
            inventoryPanelSystem.Init(inventorySystem, true);
            isInit = true;
        }
        
        #region Equipments
        
        private void UpdateEquipments() {
            ClearAllEquipmentSlot();
            var equipmentSet = inventorySystem.EquipmentSet;
            var itemWaepon = equipmentSet.Weapon;
            var itemAccessory = equipmentSet.Accessory;
            var itemArmor = equipmentSet.Armor;
            var itemGloves = equipmentSet.Gloves;
            var itemShoes = equipmentSet.Shoes;
            var itemArtifact = equipmentSet.Artifact;

            // set equipment slot if not null
            if (itemWaepon)    slotWaepon.Set(itemWaepon, ItemInfoType.Releaseable);
            if (itemAccessory) slotAccessory.Set(itemAccessory, ItemInfoType.Releaseable);
            if (itemArmor)     slotArmor.Set(itemArmor, ItemInfoType.Releaseable);
            if (itemGloves)    slotGloves.Set(itemGloves, ItemInfoType.Releaseable);
            if (itemShoes)     slotShoes.Set(itemShoes, ItemInfoType.Releaseable);
            if (itemArtifact)  slotArtifact.Set(itemArtifact, ItemInfoType.Releaseable);
            
            tmpPlayerLevel.SetText("Lv. " + player.CurrentLevel.ToString());
            flag_Update_Equipments = false;
        }

        private void ClearAllEquipmentSlot() {
            slotWaepon.Clear();
            slotAccessory.Clear();
            slotArmor.Clear();
            slotGloves.Clear();
            slotShoes.Clear();
            slotArtifact.Clear();
        }
        
        #endregion
        
        #region Stats

        private void UpdateStats() {
            var stats = player.EnhancedStat;
            tmpHp.SetText($"{stats.CurrentHp.ToStringN0()} / {stats.MaxHp.ToStringN0()}");
            var damageDeviation = stats.AttackPower * stats.DamageDeviation;
            tmpDamage.SetText($"{(stats.AttackPower - damageDeviation).ToStringN0()} ~ {(stats.AttackPower + damageDeviation).ToStringN0()}");
            tmpDefense.SetText(stats.Defense.ToStringN0());
            tmpMoveSpeed.SetText(stats.MoveSpeed.ToStringN0());
            tmpPenetration.SetText(stats.Penetration.ToStringN0());
            tmpCriticalChance.SetText($"{(stats.CriticalChance * 100).ToStringN1()} %");
            tmpCriticalMultiplier.SetText((stats.CriticalMultiplier * 100f).ToStringN0() + " %");
            tmpCriticalResistance.SetText(stats.CriticalResistance.ToStringN0());
        }
        
        #endregion
        
        #region Inventory

        private void UpdateInventory() {
            inventoryPanelSystem.UpdatePanel();
            flag_Update_Inventory = false;
        }

        #endregion
        
        #region Events

        private void OnEquipmentsChanged(EquipmentSet equipments) {
            if (!gameObject.activeSelf) {
                flag_Update_Equipments = true;
                return;
            }
            UpdateEquipments();
            UpdateStats();
        }

        private void OnInventoryChanged() {
            if (!gameObject.activeSelf) {
                flag_Update_Inventory = true;
                return;
            }
            UpdateInventory();
        }
        
        #endregion
    }
}