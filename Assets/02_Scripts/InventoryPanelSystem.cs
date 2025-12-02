using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoffeeCat.Utils;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    [Serializable]
    public class InventoryPanelSystem {
        private enum Mode {
            ReadOnly,
            Inventory,
            Shop,
        }

        [SerializeField] private Mode mode = Mode.ReadOnly;
        [SerializeField] private RectTransform slotParent = null;
        [SerializeField] private ItemSlot slotPrefab = null;
        [SerializeField] private Button btnSortAll = null;
        [SerializeField] private Button btnResource = null;
        [SerializeField] private Button btnConsumable = null;
        [SerializeField] private Button btnSortWaepon = null;
        [SerializeField] private Button btnSortArmor = null;
        [SerializeField] private Button btnSortGloves = null;
        [SerializeField] private Button btnSortShoes = null;
        [SerializeField] private Button btnAllEquipments = null;
        [SerializeField] private Button btnResourceConsumable = null;
        [SerializeField] private List<ItemSlot> spawnedSlots = new();
        private CharacterInventorySystem playerInventory = null;
        private ItemSortType currentSortType = ItemSortType.All;
        private bool isIgnoreRepeatedSortType = false;
        private Button selectedButton = null;

        public void Init(CharacterInventorySystem playerInventorySystem, bool ignoreRepeatedSortTypeOnButton) {
            playerInventory = playerInventorySystem;
            isIgnoreRepeatedSortType = ignoreRepeatedSortTypeOnButton;
            InitSortButtons();
        }
        
        public void UpdatePanel(ItemSortType requestSortType) {
            var sortedItems = playerInventory.GetItemsBySortType(requestSortType);
            DisableAllItemSlots();
            UIHelper.AddIfRequired(spawnedSlots, slotPrefab, sortedItems.Count, slotParent);
            SetItemSlotsByMode(sortedItems);
            currentSortType = requestSortType;
            SetInteractableBySortType();
        }
        
        public void UpdatePanel() => UpdatePanel(currentSortType);
        
        public void DisableAllItemSlots() {
            for (int i = 0; i < spawnedSlots.Count; i++) {
                spawnedSlots[i].Disable();
            }
        }
        
        private void SetItemSlotsByMode(List<Item> sortedItems) {
            switch(mode) {
                case Mode.ReadOnly:  SetSlotsOnReadOnlyMode(sortedItems); break;
                case Mode.Inventory: SetSlotsOnInvenMode(sortedItems);    break;
                case Mode.Shop:      SetSlotsOnShopMode(sortedItems);     break;
                default:             throw new NotImplementedException($"{mode} is not implemented.");
            }
        }

        private void SetSlotsOnInvenMode(List<Item> sortedItems) {
            for (int i = 0; i < sortedItems.Count; i++) {
                var item = sortedItems[i];
                switch (item.Type) {
                    case ItemType.Resource:   spawnedSlots[i].Set(item, ItemInfoType.ReadOnly);  break;
                    case ItemType.Consumable: spawnedSlots[i].Set(item, ItemInfoType.ReadOnly);  break;
                    case ItemType.Equipment:  spawnedSlots[i].Set(item, ItemInfoType.Equipable); break;
                    case ItemType.None:
                    default: throw new NotImplementedException($"{item.Type} is not implemented.");
                }
            }
        }
        
        private void SetSlotsOnShopMode(List<Item> sortedItems) {
            for (int i = 0; i < sortedItems.Count; i++) {
                var item = sortedItems[i];
                spawnedSlots[i].Set(item, ItemInfoType.Sellable);
            }
        }

        private void SetSlotsOnReadOnlyMode(List<Item> sortedItems) {
            for (int i = 0; i < sortedItems.Count; i++) {
                var item = sortedItems[i];
                spawnedSlots[i].Set(item, ItemInfoType.ReadOnly);
            }
        }
        
        private void InitSortButtons() {
            RemoveEventValidButtons();
            AddEventValidButtons();
        }

        private void RemoveEventValidButtons() {
            btnSortAll?.onClick.RemoveAllListeners();
            btnConsumable?.onClick.RemoveAllListeners();
            btnResource?.onClick.RemoveAllListeners();
            btnSortWaepon?.onClick.RemoveAllListeners();
            btnSortArmor?.onClick.RemoveAllListeners();
            btnSortGloves?.onClick.RemoveAllListeners();
            btnSortShoes?.onClick.RemoveAllListeners();
            btnAllEquipments?.onClick.RemoveAllListeners();
            btnResourceConsumable?.onClick.RemoveAllListeners();
        }

        private void UpdatePanelOnButton(ItemSortType requestSortType) {
            if (isIgnoreRepeatedSortType && requestSortType == currentSortType) {
                return;
            }

            SoundManager.Inst.PlayButtonSE(true);
            UpdatePanel(requestSortType);
        }

        private void AddEventValidButtons() {
            btnSortAll?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.All));
            btnConsumable?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Consumable));
            btnResource?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Resource));
            btnSortWaepon?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Weapon));
            btnSortArmor?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Armor));
            btnSortGloves?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Gloves));
            btnSortShoes?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.Shoes));
            btnAllEquipments?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.AllEquipments));
            btnResourceConsumable?.onClick.AddListener(() => UpdatePanelOnButton(ItemSortType.ResourceAndConsumable));
        }

        private void SetInteractableBySortType() {
            selectedButton.SetInteractableSafeNull(true);
            selectedButton = currentSortType switch {
                ItemSortType.All                   => btnSortAll,
                ItemSortType.AllEquipments         => btnAllEquipments,
                ItemSortType.Weapon                => btnSortWaepon,
                ItemSortType.Armor                 => btnSortArmor,
                ItemSortType.Gloves                => btnSortGloves,
                ItemSortType.Shoes                 => btnSortShoes,
                ItemSortType.Resource              => btnResource,
                ItemSortType.Consumable            => btnConsumable,
                ItemSortType.ResourceAndConsumable => btnResourceConsumable,
                ItemSortType.None                  => null,
                ItemSortType.Ring                  => null,
                ItemSortType.Artifact              => null,
                _                                  => throw new ArgumentOutOfRangeException()
            };
            selectedButton.SetInteractableSafeNull(false);
        }
    }
}