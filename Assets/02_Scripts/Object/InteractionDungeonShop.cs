using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionDungeonShop : Interaction {
        public List<ItemLootData> SellingItems { get; private set; } = new();
        private GuaranteedLootList lootTable = null;
        private DungeonShopPanel shopPanel = null;

        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.DungeonShop;
            infoText.gameObject.SetActive(false);
        }

        public void Init(GuaranteedLootList table) {
            // set loot table
            lootTable = table;
            RefreshSellingItems();
            shopPanel = DungeonUIPresenter.Inst.ShopPanel;
        }

        private void RefreshSellingItems() {
            SellingItems.Clear();
            var raffledItems = lootTable.GetShopRoomLootList();
            for (int i = 0; i < raffledItems.Count; i++) {
                var raffledItem = raffledItems[i];
                var clone = raffledItem.Clone();
                SellingItems.Add(clone);
            }
            
            // fix amount
            for (int i = 0; i < SellingItems.Count; i++) {
                var item = SellingItems[i];
                item.Amount = item.GetAmount();
            }
        }

        public void RemoveSellingItem(int id, int price) {
            for (int i = SellingItems.Count - 1; i >= 0; i--) {
                var sellingItem = SellingItems[i];
                if ((int)sellingItem.Code != id || sellingItem.Price != price) {
                    continue;
                }
                SellingItems.RemoveAt(i);
            }
        }

        public override void Interact() {
            shopPanel.OnEnteredShop(this);
            CatLog.Log("OnInteract: SHOP");
        }

        protected override void OnPlayerEnter() => infoText.gameObject.SetActive(true);

        protected override void OnPlayerExit() => infoText.gameObject.SetActive(false);
    }
}