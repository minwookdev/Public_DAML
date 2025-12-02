using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class DungeonShopPanel : MonoBehaviour {
        [FoldoutGroup("Player Area"), SerializeField] private InventoryPanelSystem inventoryPanelSystem = null;
        [FoldoutGroup("Player Area"), SerializeField] private TextMeshProUGUI tmpGold = null;
        [FoldoutGroup("Player Area"), SerializeField] private TextMeshProUGUI tmpStone = null;
        
        [FoldoutGroup("Shop Area"), SerializeField] private SellingItemSlot sellingSlotOrigin = null;
        [FoldoutGroup("Shop Area"), SerializeField] private List<SellingItemSlot> spawnedSellingSlots = new();
        [FoldoutGroup("Shop Area"), SerializeField] private RectTransform sellingSlotParent = null;
        [FoldoutGroup("Shop Area"), SerializeField] private Button btnClose = null; 
        
        private InteractionDungeonShop currentEnteredShop = null;
        private CharacterInventorySystem inventorySystem = null;
        private bool flag_Update_Inventory = true;
        private bool flag_Update_Currency = true;
        private bool isInit = false;

        private void Start() {
            DungeonEvtManager.AddEventOnPlayerCurrencyChanged(OnPlayerCurrencyChanged);
            DungeonEvtManager.AddEventOnPlayerBuyedItem(OnPlayerBuyedItem);
            DungeonEvtManager.AddEventOnPlayerSelledItem(OnPlayerSelledItem);
            DungeonEvtManager.AddEventOnPlayerInventoryChanged(OnPlayerInventoryChanged);
            DungeonEvtManager.AddEventMapDisposeBefore(ReleaseInteraction);
            btnClose.onClick.AddListener(ClosePanel);
        }

        private void OnDestroy() {
            DungeonEvtManager.RemoveEventOnPlayerCurrencyChanged(OnPlayerCurrencyChanged);
            DungeonEvtManager.RemoveEventOnPlayerBuyedItem(OnPlayerBuyedItem);
            DungeonEvtManager.RemoveEventOnPlayerSelledItem(OnPlayerSelledItem);
            DungeonEvtManager.RemoveEventOnPlayerInventoryChanged(OnPlayerInventoryChanged);
            DungeonEvtManager.RemoveEventMapDisposeBefore(ReleaseInteraction);
        }

        private void OnPlayerCurrencyChanged(int currencyA) {
            if (!gameObject.activeSelf) {
                flag_Update_Currency = true;
                return;
            }
            UpdateCurrencyPanel(currencyA);
        }

        private void OnPlayerInventoryChanged() {
            if (!gameObject.activeSelf) {
                flag_Update_Inventory = true;
            }
            UpdateInventoryPanel();
        }
        
        private void OnPlayerBuyedItem(int id, int price) {
            // remove from selling items list
            currentEnteredShop.RemoveSellingItem(id, price);
            UpdateShopPanel();
            UpdateInventoryPanel();
        }

        private void OnPlayerSelledItem() {
            UpdateInventoryPanel();
            UpdateCurrencyPanel(inventorySystem.Currency);
        }

        public void OnEnteredShop(InteractionDungeonShop shop) {
            if (!isInit) {
                Init();   
            }
            
            if (currentEnteredShop != shop) {
                currentEnteredShop = shop;
                UpdateShopPanel();
            }

            if (flag_Update_Inventory) {
                UpdateInventoryPanel();
            }

            if (flag_Update_Currency) {
                UpdateCurrencyPanel(inventorySystem.Currency);
            }
            
            gameObject.SetActive(true);
        }
        
        private void Init() {
            inventorySystem = RogueLiteManager.Inst.PlayerInvenSystem;
            inventoryPanelSystem.Init(inventorySystem, true);
            isInit = true;
        }

        private void UpdateInventoryPanel() {
            inventoryPanelSystem.UpdatePanel();
            flag_Update_Inventory = false;
        }
        
        private void UpdateCurrencyPanel(int currencyA) {
            var currencyB = UserDataManager.Inst.Currency;
            tmpGold.SetText(currencyA.ToStringN0());
            tmpStone.SetText(currencyB.ToStringN0());
            flag_Update_Currency = false;
        }
        
        private void UpdateShopPanel() {
            var itemLootList = currentEnteredShop.SellingItems;
            UIHelper.AddIfRequired(spawnedSellingSlots, sellingSlotOrigin, itemLootList.Count, sellingSlotParent);
            DisableAllSellingSlots();
            SetSellingSlots(itemLootList);
        }
                
        private void SetSellingSlots(List<ItemLootData> itemLootList) {
            for (int i = 0; i < itemLootList.Count; i++) {
                var lootData = itemLootList[i];
                spawnedSellingSlots[i].Set(lootData);
            }
        }
        
        private void DisableAllSellingSlots() {
            for (int i = 0; i < spawnedSellingSlots.Count; i++) {
                spawnedSellingSlots[i].Disable();
            }
        }

        private void ClosePanel() => gameObject.SetActive(false);

        private void ReleaseInteraction() => currentEnteredShop = null;
    }
}