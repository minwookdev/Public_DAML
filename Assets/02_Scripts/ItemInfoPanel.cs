using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class ItemInfoPanel : MonoBehaviour {
        [SerializeField] private CommonIconSlot commonSlot = null;
        [SerializeField] private RectTransform slotParent = null;
        [SerializeField] private NameWithValueSlot originSlot = null;
        [SerializeField] private List<NameWithValueSlot> spawnedSlots = new();
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private Button btnEquip = null;
        [SerializeField] private Button btnRelease = null;
        [SerializeField] private Button btnBuy = null;
        [SerializeField] private Button btnSell = null;
        [SerializeField] private Button btnRemove = null;
        [SerializeField] private Button btnClose = null;
        [SerializeField] private Button btnCloseTop = null;
        [SerializeField] private Button btnCloseBack = null;
        private readonly Dictionary<string, string> itemAttributeDict = new();
        private ItemInfoType itemInfoType;
        private CharacterInventorySystem inventorySystem = null;
        private Item itemRef = null;

        private void Start() => SetButtonEvents();

        public void Set(Item item, ItemInfoType type) {
            SetInventorySystemIfNull();

            // update info only flagged
            if (item != itemRef || item.IsUpdated) {
                itemRef = item;
                SetItemIconSlot();
                SetValueSlots();
                SetDescription();
            }

            // update button only changed request type
            if (type != itemInfoType) {
                itemInfoType = type;
                ActivateButtons();
            }

            itemRef.IsUpdated = false;
            gameObject.SetActive(true);
        }

        private void SetInventorySystemIfNull() {
            if (inventorySystem != null) {
                return;
            }
            inventorySystem = RogueLiteManager.Inst.PlayerInvenSystem;
        }

        private void SetItemIconSlot() {
            commonSlot.Set(itemRef.IconKey);
        }

        private void SetValueSlots() {
            itemAttributeDict.Clear();
            itemRef.WriteAttributes(itemAttributeDict);
            
            // spawn require slots
            var requireSlotCount = itemAttributeDict.Count;
            var slotSpawnCount = requireSlotCount - spawnedSlots.Count;
            if (slotSpawnCount > 0) {
                for (int i = 0; i < slotSpawnCount; i++) {
                    var clone = Instantiate(originSlot, slotParent);
                    spawnedSlots.Add(clone);
                    clone.SetSiblingIndex(1);
                }
            }

            // disable slots
            for (int i = 0; i < spawnedSlots.Count; i++) {
                spawnedSlots[i].Disable();
            }
            
            // set slots
            int index = 0;
            foreach (var pair in itemAttributeDict) {
                spawnedSlots[index].Set(pair.Key, pair.Value);
                index++;
            }
        }

        private void SetDescription() {
            tmpDesc.SetText(itemRef.DescKey);
        }
        
        private void ActivateButtons() {
            switch (itemInfoType) {
                case ItemInfoType.ReadOnly:
                    btnEquip.gameObject.SetActive(false);
                    btnRelease.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnSell.gameObject.SetActive(false);
                    btnRemove.gameObject.SetActive(false);
                    break;
                case ItemInfoType.Equipable:
                    btnEquip.gameObject.SetActive(true);
                    btnRelease.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnSell.gameObject.SetActive(false);
                    btnRemove.gameObject.SetActive(true);
                    break;
                case ItemInfoType.Releaseable:
                    btnEquip.gameObject.SetActive(false);
                    btnRelease.gameObject.SetActive(true);
                    btnBuy.gameObject.SetActive(false);
                    btnSell.gameObject.SetActive(false);
                    btnRemove.gameObject.SetActive(false);
                    break;
                case ItemInfoType.Buyable:
                    btnEquip.gameObject.SetActive(false);
                    btnRelease.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(true);
                    btnSell.gameObject.SetActive(false);
                    btnRemove.gameObject.SetActive(false);
                    break;
                case ItemInfoType.Sellable:
                    btnEquip.gameObject.SetActive(false);
                    btnRelease.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnSell.gameObject.SetActive(true);
                    btnRemove.gameObject.SetActive(false);
                    break;
                default: 
                    throw new ArgumentOutOfRangeException();
            }

            btnClose.gameObject.SetActive(true);
        }

        private void Disable() => gameObject.SetActive(false);

        private void SetButtonEvents() {
            // btnEquip.onClick.RemoveAllListeners();
            // btnRelease.onClick.RemoveAllListeners();
            // btnBuy.onClick.RemoveAllListeners();
            // btnSell.onClick.RemoveAllListeners();
            // btnRemove.onClick.RemoveAllListeners();
            // btnClose.onClick.RemoveAllListeners();
            // btnCloseBack.onClick.RemoveAllListeners();
            // btnCloseTop.onClick.RemoveAllListeners();
            
            if (RogueLiteManager.Inst.IsEnteredDungeon) {
                btnEquip.onClick.AddListener(OnButtonEquipInDungeon);
                btnRelease.onClick.AddListener(OnButtonReleaseInDungeon);
                btnBuy.onClick.AddListener(OnButtonBuyInDungeon);
                btnSell.onClick.AddListener(OnButtonSellInDungeon);
                btnRemove.onClick.AddListener(OnButtonRemoveInDungeon);
            }
            else {
                btnEquip.onClick.AddListener(OnButtonEquipInTown);
                btnRelease.onClick.AddListener(OnButtonReleaseInTown);
                btnBuy.onClick.AddListener(OnButtonBuyInTown);
                btnSell.onClick.AddListener(OnButtonSellInTown);
                btnRemove.onClick.AddListener(OnButtonRemoveInTown);
            }

            btnClose.onClick.AddListener(Disable);
            btnCloseTop.onClick.AddListener(Disable);
            btnCloseBack.onClick.AddListener(Disable);
        }

        #region Dugneon Button Events

        private void OnButtonEquipInDungeon() {
            inventorySystem.EquipItem(itemRef);
            itemRef = null;
            gameObject.SetActive(false);
        }

        private void OnButtonReleaseInDungeon() {
            inventorySystem.ReleaseItem(itemRef);
            itemRef = null;
            gameObject.SetActive(false);
        }

        private void OnButtonBuyInDungeon() {
            DungeonUIPresenter.Inst.OpenConfirmPanel("정말 아이템을 구매합니까?", () => {
                var isSuccessed = inventorySystem.BuyItem(itemRef);
                if (!isSuccessed) {
                    DungeonUIPresenter.Inst.OpenConfirmPanel("아이템을 구매하지 못했습니다.");
                }
                itemRef = null;
                gameObject.SetActive(false);
                SoundManager.Inst.PlayCurrencyUsedSE();
            }, () => {
                CatLog.Log("Canceled Buy Item.");
            });
        }

        private void OnButtonSellInDungeon() {
            DungeonUIPresenter.Inst.OpenConfirmPanel("정말 아이템을 판매합니까?", () => {
                var isSuccessed = inventorySystem.SellItem(itemRef);
                if (!isSuccessed) {
                    DungeonUIPresenter.Inst.OpenConfirmPanel("아이템을 판매하지 못했습니다.");
                }
                itemRef = null;
                gameObject.SetActive(false);
                SoundManager.Inst.PlayCurrencyUsedSE();
            }, () => {
                CatLog.Log("Canceled Sell Item.");
            });
        }

        private void OnButtonRemoveInDungeon() {
            DungeonUIPresenter.Inst.OpenConfirmPanel("정말 이 아이템을 삭제합니까? \n 정말로요?", () => {
                var isSuccessed = inventorySystem.RemoveItem(itemRef);
                if (!isSuccessed) {
                    DungeonUIPresenter.Inst.OpenConfirmPanel("아이템을 판매하지 못했습니다.");
                }
                
                itemRef = null;
                gameObject.SetActive(false);
            }, () => {
                CatLog.Log("Canceled Remove Item.");
            });
        }
        
        #endregion
        
        #region Town Button Events

        private void OnButtonEquipInTown() {
            
        }

        private void OnButtonReleaseInTown() {
            
        }

        private void OnButtonBuyInTown() {
            
        }

        private void OnButtonSellInTown() {
            
        }

        private void OnButtonRemoveInTown() {
            
        }
        
        #endregion
    }
}