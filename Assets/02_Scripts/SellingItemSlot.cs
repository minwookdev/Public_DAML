using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class SellingItemSlot : CommonIconSlot {
        [SerializeField] private TextMeshProUGUI tmpItemName = null;
        [SerializeField] private TextMeshProUGUI tmpItemType = null;
        [SerializeField] private TextMeshProUGUI tmpItemPrice = null;
        [SerializeField] private Button btn = null;
        [SerializeField] private Item sellingItem = null;

        private void Start() => btn.onClick.AddListener(OnButtonEvent);

        private void OnButtonEvent() => DungeonUIPresenter.Inst.OpenItemInfoPanel(sellingItem, ItemInfoType.Buyable);
        
        public void Set(ItemLootData itemLootData) {
            var allItemData = DataManager.Inst.Items;
            var entity = allItemData.GetItemData((int)itemLootData.Code);
            if (!entity) {
                CatLog.WLog("Failed to Get Item Data: " + itemLootData.Code);
                return;
            }

            // clone and copy selling price
            sellingItem = entity.Clone();
            sellingItem.Price = itemLootData.Price;
            
            LoadIconSpriteIfDiff(sellingItem.IconKey);
            tmpItemName.text = sellingItem.NameKey;
            tmpItemType.text = sellingItem.Type.ToStringEx();
            tmpItemPrice.text = itemLootData.Price.ToStringN0();
            
            gameObject.SetActive(true);
        }
    }
}